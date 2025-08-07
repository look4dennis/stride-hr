import { Injectable, ErrorHandler, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, of, throwError, timer, BehaviorSubject } from 'rxjs';
import { retry, retryWhen, delayWhen, take, concatMap } from 'rxjs/operators';
import { NotificationService } from './notification.service';
import { LoadingService } from './loading.service';
import { Router } from '@angular/router';

export interface ErrorContext {
  component?: string;
  operation?: string;
  userId?: string;
  timestamp: Date;
  url?: string;
  userAgent?: string;
}

export interface RetryConfig {
  maxRetries: number;
  delayMs: number;
  exponentialBackoff: boolean;
  retryCondition?: (error: any) => boolean;
}

export interface ErrorLog {
  id: string;
  error: any;
  context: ErrorContext;
  severity: 'low' | 'medium' | 'high' | 'critical';
  handled: boolean;
  userNotified: boolean;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlingService implements ErrorHandler {
  private readonly notificationService = inject(NotificationService);
  private readonly loadingService = inject(LoadingService);
  private readonly router = inject(Router);

  private errorLogsSubject = new BehaviorSubject<ErrorLog[]>([]);
  public errorLogs$ = this.errorLogsSubject.asObservable();

  private readonly maxErrorLogs = 100;
  private readonly defaultRetryConfig: RetryConfig = {
    maxRetries: 3,
    delayMs: 1000,
    exponentialBackoff: true,
    retryCondition: (error) => this.isRetryableError(error)
  };

  constructor() {}

  /**
   * Global error handler implementation
   */
  handleError(error: any): void {
    const context: ErrorContext = {
      timestamp: new Date(),
      url: window.location.href,
      userAgent: navigator.userAgent
    };

    this.logError(error, context, 'high', false, false);

    // Handle different types of errors
    if (error instanceof HttpErrorResponse) {
      this.handleHttpError(error, context);
    } else if (error instanceof Error) {
      this.handleJavaScriptError(error, context);
    } else {
      this.handleUnknownError(error, context);
    }
  }

  /**
   * Handle HTTP errors with specific logic
   */
  handleHttpError(error: HttpErrorResponse, context?: ErrorContext): void {
    const errorContext = context || {
      timestamp: new Date(),
      url: window.location.href
    };

    let errorMessage = 'An unexpected error occurred';
    let severity: 'low' | 'medium' | 'high' | 'critical' = 'medium';
    let shouldShowNotification = true;
    let shouldRetry = false;

    if (error.error instanceof ErrorEvent) {
      // Client-side/network error
      errorMessage = `Network Error: ${error.error.message}`;
      severity = 'high';
      shouldRetry = true;
    } else {
      // Server-side error
      switch (error.status) {
        case 0:
          errorMessage = 'Unable to connect to server. Please check your internet connection.';
          severity = 'critical';
          shouldRetry = true;
          break;
        case 400:
          errorMessage = this.extractValidationErrors(error) || 'Invalid request data';
          severity = 'low';
          break;
        case 401:
          errorMessage = 'Authentication required. Please log in again.';
          severity = 'medium';
          shouldShowNotification = false; // Let auth service handle this
          this.handleAuthenticationError();
          break;
        case 403:
          errorMessage = 'You do not have permission to perform this action';
          severity = 'medium';
          break;
        case 404:
          errorMessage = 'The requested resource was not found';
          severity = 'low';
          break;
        case 409:
          errorMessage = error.error?.message || 'Conflict: Resource already exists or is in use';
          severity = 'medium';
          break;
        case 422:
          errorMessage = this.extractValidationErrors(error) || 'Validation failed. Please check your input data.';
          severity = 'low';
          break;
        case 429:
          errorMessage = 'Too many requests. Please try again later.';
          severity = 'medium';
          shouldRetry = true;
          break;
        case 500:
          errorMessage = 'Internal server error. Please try again later.';
          severity = 'high';
          shouldRetry = true;
          break;
        case 502:
        case 503:
        case 504:
          errorMessage = 'Service temporarily unavailable. Please try again later.';
          severity = 'high';
          shouldRetry = true;
          break;
        default:
          errorMessage = error.error?.message || `Server Error: ${error.status}`;
          severity = 'medium';
      }
    }

    this.logError(error, errorContext, severity, true, shouldShowNotification);

    if (shouldShowNotification) {
      this.showUserFriendlyError(errorMessage, shouldRetry, () => {
        // Retry callback would be implemented by the calling component
        console.log('Retry requested for error:', error);
      });
    }
  }

  /**
   * Handle JavaScript/runtime errors
   */
  private handleJavaScriptError(error: Error, context: ErrorContext): void {
    const errorMessage = `Application Error: ${error.message}`;
    
    this.logError(error, context, 'high', true, true);
    
    // Show user-friendly message for runtime errors
    this.notificationService.showError(
      'An unexpected error occurred. The page will be refreshed to restore functionality.',
      'Application Error'
    );

    // Optionally reload the page after a delay for critical errors
    setTimeout(() => {
      if (confirm('Would you like to refresh the page to restore functionality?')) {
        window.location.reload();
      }
    }, 3000);
  }

  /**
   * Handle unknown error types
   */
  private handleUnknownError(error: any, context: ErrorContext): void {
    const errorMessage = 'An unknown error occurred';
    
    this.logError(error, context, 'medium', true, true);
    
    this.notificationService.showError(errorMessage, 'Unknown Error');
  }

  /**
   * Handle authentication errors
   */
  private handleAuthenticationError(): void {
    // Clear any loading states
    this.loadingService.clearAll();
    
    // Redirect to login after a short delay
    setTimeout(() => {
      this.router.navigate(['/login'], { 
        queryParams: { returnUrl: this.router.url } 
      });
    }, 1000);
  }

  /**
   * Extract validation errors from HTTP error response
   */
  private extractValidationErrors(error: HttpErrorResponse): string | null {
    if (error.error?.errors) {
      if (Array.isArray(error.error.errors)) {
        return error.error.errors.map((err: any) => err.message || err).join(', ');
      } else if (typeof error.error.errors === 'object') {
        const validationErrors = Object.values(error.error.errors).flat();
        return validationErrors.join(', ');
      }
    }
    return error.error?.message || null;
  }

  /**
   * Show user-friendly error with retry option
   */
  private showUserFriendlyError(message: string, canRetry: boolean, retryCallback?: () => void): void {
    if (canRetry && retryCallback) {
      // For now, just show the error. In a full implementation, this could show a modal with retry button
      this.notificationService.showError(`${message} Click to retry.`, 'Error');
    } else {
      this.notificationService.showError(message);
    }
  }

  /**
   * Retry mechanism for failed operations
   */
  retryOperation<T>(
    operation: () => Observable<T>, 
    config: Partial<RetryConfig> = {}
  ): Observable<T> {
    const retryConfig = { ...this.defaultRetryConfig, ...config };
    
    return operation().pipe(
      retryWhen(errors =>
        errors.pipe(
          take(retryConfig.maxRetries),
          concatMap((error, index) => {
            if (!retryConfig.retryCondition || !retryConfig.retryCondition(error)) {
              return throwError(() => error);
            }
            
            const delay = retryConfig.exponentialBackoff 
              ? retryConfig.delayMs * Math.pow(2, index)
              : retryConfig.delayMs;
            
            console.log(`Retrying operation (attempt ${index + 1}/${retryConfig.maxRetries}) after ${delay}ms`);
            
            return timer(delay);
          })
        )
      )
    );
  }

  /**
   * Check if an error is retryable
   */
  private isRetryableError(error: any): boolean {
    if (error instanceof HttpErrorResponse) {
      // Retry on network errors and server errors, but not client errors
      return error.status === 0 || error.status >= 500 || error.status === 429;
    }
    return false;
  }

  /**
   * Handle database connection failures
   */
  handleDatabaseConnectionFailure(error: any, context?: ErrorContext): void {
    const errorContext = context || {
      timestamp: new Date(),
      operation: 'database_connection'
    };

    this.logError(error, errorContext, 'critical', true, true);

    this.notificationService.showError(
      'Database connection failed. Please check your connection and try again. If the problem persists, contact support.',
      'Database Error',
      10000
    );

    // Could implement automatic reconnection logic here
    this.attemptDatabaseReconnection();
  }

  /**
   * Attempt database reconnection
   */
  private attemptDatabaseReconnection(): void {
    // This would typically call a health check endpoint
    console.log('Attempting database reconnection...');
    
    // Implement reconnection logic here
    // For now, just log the attempt
  }

  /**
   * Handle validation errors for forms
   */
  handleValidationErrors(errors: any, formContext?: string): { [key: string]: string } {
    const validationErrors: { [key: string]: string } = {};

    if (Array.isArray(errors)) {
      errors.forEach((error: any) => {
        if (error.field && error.errorMessage) {
          validationErrors[error.field] = error.errorMessage;
        }
      });
    } else if (typeof errors === 'object') {
      Object.keys(errors).forEach(key => {
        const errorMessages = Array.isArray(errors[key]) ? errors[key] : [errors[key]];
        validationErrors[key] = errorMessages.join(', ');
      });
    }

    // Log validation errors
    this.logError(errors, {
      timestamp: new Date(),
      component: formContext,
      operation: 'form_validation'
    }, 'low', true, false);

    return validationErrors;
  }

  /**
   * Log error for debugging and monitoring
   */
  private logError(
    error: any, 
    context: ErrorContext, 
    severity: 'low' | 'medium' | 'high' | 'critical',
    handled: boolean,
    userNotified: boolean
  ): void {
    const errorLog: ErrorLog = {
      id: this.generateId(),
      error: this.serializeError(error),
      context,
      severity,
      handled,
      userNotified,
      timestamp: new Date()
    };

    // Add to error logs
    const currentLogs = this.errorLogsSubject.value;
    const updatedLogs = [errorLog, ...currentLogs].slice(0, this.maxErrorLogs);
    this.errorLogsSubject.next(updatedLogs);

    // Console logging with appropriate level
    const logLevel = severity === 'critical' ? 'error' : severity === 'high' ? 'error' : 'warn';
    console[logLevel]('Error logged:', {
      id: errorLog.id,
      severity,
      context,
      error: errorLog.error,
      timestamp: errorLog.timestamp
    });

    // In a production environment, you might want to send critical errors to a logging service
    if (severity === 'critical') {
      this.sendToLoggingService(errorLog);
    }
  }

  /**
   * Serialize error for logging
   */
  private serializeError(error: any): any {
    if (error instanceof Error) {
      return {
        name: error.name,
        message: error.message,
        stack: error.stack
      };
    } else if (error instanceof HttpErrorResponse) {
      return {
        status: error.status,
        statusText: error.statusText,
        message: error.message,
        url: error.url,
        error: error.error
      };
    }
    return error;
  }

  /**
   * Send critical errors to logging service
   */
  private sendToLoggingService(errorLog: ErrorLog): void {
    // In a real implementation, this would send to a logging service like Sentry, LogRocket, etc.
    console.error('Critical error logged:', errorLog);
  }

  /**
   * Get error logs for debugging
   */
  getErrorLogs(): ErrorLog[] {
    return this.errorLogsSubject.value;
  }

  /**
   * Clear error logs
   */
  clearErrorLogs(): void {
    this.errorLogsSubject.next([]);
  }

  /**
   * Generate unique ID for error logs
   */
  private generateId(): string {
    return Math.random().toString(36).substr(2, 9) + Date.now().toString(36);
  }
}