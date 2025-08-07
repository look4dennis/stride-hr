import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { takeUntil, catchError, finalize } from 'rxjs/operators';
import { LoadingService } from '../../core/services/loading.service';
import { NotificationService } from '../../core/services/notification.service';
import { ErrorHandlingService } from '../../core/services/error-handling.service';
import { ConnectionService } from '../../core/services/connection.service';

@Component({
  template: ''
})
export abstract class BaseComponent implements OnInit, OnDestroy {
  protected readonly loadingService = inject(LoadingService);
  protected readonly notificationService = inject(NotificationService);
  protected readonly errorHandlingService = inject(ErrorHandlingService);
  protected readonly connectionService = inject(ConnectionService);
  
  protected destroy$ = new Subject<void>();
  protected isLoading = false;
  protected error: string | null = null;
  protected componentId: string;
  protected isOnline = true;

  constructor() {
    this.componentId = this.constructor.name;
  }

  ngOnInit(): void {
    this.initializeComponent();
    this.subscribeToLoading();
    this.subscribeToConnectionStatus();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.clearComponentLoading();
  }

  // Abstract method for component initialization
  protected abstract initializeComponent(): void;

  // Loading state management
  protected showLoading(message?: string): void {
    this.isLoading = true;
    this.loadingService.setComponentLoading(this.componentId, true);
    if (message) {
      this.loadingService.setLoading(true, this.componentId, message);
    }
  }

  protected hideLoading(): void {
    this.isLoading = false;
    this.loadingService.setComponentLoading(this.componentId, false);
    this.loadingService.clearLoading(this.componentId);
  }

  protected isComponentLoading(): Observable<boolean> {
    return this.loadingService.isComponentLoading(this.componentId);
  }

  // Enhanced error handling
  protected handleError(error: any, customMessage?: string): void {
    this.hideLoading();
    
    const errorContext = {
      component: this.componentId,
      timestamp: new Date(),
      url: window.location.href
    };

    // Use the comprehensive error handling service
    this.errorHandlingService.handleHttpError(error, errorContext);
    
    // Set local error state for component-specific handling
    this.error = customMessage || this.extractErrorMessage(error);
  }

  protected clearError(): void {
    this.error = null;
  }

  // Success handling
  protected showSuccess(message: string): void {
    this.notificationService.showSuccess(message);
  }

  protected showInfo(message: string): void {
    this.notificationService.showInfo(message);
  }

  protected showWarning(message: string): void {
    this.notificationService.showWarning(message);
  }

  // Utility methods
  protected executeWithLoading<T>(
    operation: () => Observable<T>,
    loadingMessage?: string,
    successMessage?: string
  ): Observable<T> {
    this.showLoading(loadingMessage);
    this.clearError();

    return new Observable<T>(observer => {
      operation().pipe(
        takeUntil(this.destroy$)
      ).subscribe({
        next: (result) => {
          this.hideLoading();
          if (successMessage) {
            this.showSuccess(successMessage);
          }
          observer.next(result);
          observer.complete();
        },
        error: (error) => {
          this.handleError(error);
          observer.error(error);
        }
      });
    });
  }

  protected retryOperation<T>(operation: () => Observable<T>, maxRetries: number = 3): Observable<T> {
    return this.errorHandlingService.retryOperation(operation, {
      maxRetries,
      delayMs: 1000,
      exponentialBackoff: true
    });
  }

  // Enhanced operation handling with comprehensive error support
  protected executeOperation<T>(
    operation: () => Observable<T>,
    options: {
      loadingMessage?: string;
      successMessage?: string;
      errorMessage?: string;
      enableRetry?: boolean;
      maxRetries?: number;
    } = {}
  ): Observable<T> {
    this.clearError();
    
    if (options.loadingMessage) {
      this.showLoading(options.loadingMessage);
    }

    const executeWithRetry = options.enableRetry 
      ? this.errorHandlingService.retryOperation(operation, {
          maxRetries: options.maxRetries || 3
        })
      : operation();

    return executeWithRetry.pipe(
      takeUntil(this.destroy$),
      finalize(() => this.hideLoading()),
      catchError(error => {
        this.handleError(error, options.errorMessage);
        throw error;
      })
    );
  }

  // Offline-aware operation execution
  protected executeOfflineAwareOperation<T>(
    operation: () => Observable<T>,
    fallbackData?: T,
    options: {
      queueWhenOffline?: boolean;
      showOfflineMessage?: boolean;
    } = {}
  ): Observable<T> {
    if (!this.isOnline) {
      if (options.showOfflineMessage !== false) {
        this.showWarning('You are currently offline. Some features may not be available.');
      }

      if (fallbackData !== undefined) {
        return new Observable(observer => {
          observer.next(fallbackData);
          observer.complete();
        });
      }
    }

    return this.executeOperation(operation, {
      enableRetry: true,
      maxRetries: 2
    });
  }

  // Form validation error handling
  protected handleValidationErrors(errors: any): { [key: string]: string } {
    return this.errorHandlingService.handleValidationErrors(errors, this.componentId);
  }

  // Database connection error handling
  protected handleDatabaseError(error: any): void {
    const errorContext = {
      component: this.componentId,
      operation: 'database_operation',
      timestamp: new Date()
    };

    this.errorHandlingService.handleDatabaseConnectionFailure(error, errorContext);
  }

  // Connection status utilities
  protected shouldShowOfflineIndicator(): boolean {
    return !this.isOnline;
  }

  protected getPendingOfflineActionsCount(): number {
    return this.connectionService.getPendingActionsCount();
  }

  // Private methods
  private subscribeToLoading(): void {
    this.isComponentLoading().pipe(
      takeUntil(this.destroy$)
    ).subscribe(loading => {
      this.isLoading = loading;
    });
  }

  private subscribeToConnectionStatus(): void {
    this.connectionService.connectionStatus$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(status => {
      this.isOnline = status.isOnline && status.isConnectedToServer;
    });
  }

  private clearComponentLoading(): void {
    this.loadingService.clearComponentLoading(this.componentId);
  }

  private extractErrorMessage(error: any): string {
    if (typeof error === 'string') {
      return error;
    }
    
    if (error?.message) {
      return error.message;
    }
    
    if (error?.error?.message) {
      return error.error.message;
    }
    
    if (error?.status) {
      return `HTTP Error ${error.status}: ${error.statusText || 'Unknown error'}`;
    }
    
    return 'An unexpected error occurred';
  }
}