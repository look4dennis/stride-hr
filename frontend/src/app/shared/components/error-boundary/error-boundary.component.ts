import { Component, Input, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ErrorHandlingService } from '../../../core/services/error-handling.service';
import { LoadingService } from '../../../core/services/loading.service';

export interface ErrorBoundaryConfig {
  showRetryButton?: boolean;
  showReloadButton?: boolean;
  showNavigationButton?: boolean;
  fallbackRoute?: string;
  customMessage?: string;
  logError?: boolean;
}

@Component({
  selector: 'app-error-boundary',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="error-boundary-container" *ngIf="hasError">
      <div class="error-boundary-content">
        <div class="error-icon">
          <i class="fas fa-exclamation-triangle"></i>
        </div>
        
        <h3 class="error-title">{{ errorTitle }}</h3>
        <p class="error-message">{{ errorMessage }}</p>
        
        <div class="error-details" *ngIf="showDetails && errorDetails">
          <button 
            class="btn btn-link btn-sm" 
            (click)="toggleDetails()"
            type="button">
            {{ showErrorDetails ? 'Hide' : 'Show' }} Details
          </button>
          
          <div class="error-details-content" *ngIf="showErrorDetails">
            <pre>{{ errorDetails }}</pre>
          </div>
        </div>
        
        <div class="error-actions">
          <button 
            *ngIf="config.showRetryButton !== false"
            class="btn btn-primary me-2" 
            (click)="retry()"
            [disabled]="isRetrying">
            <i class="fas fa-redo" *ngIf="!isRetrying"></i>
            <i class="fas fa-spinner fa-spin" *ngIf="isRetrying"></i>
            {{ isRetrying ? 'Retrying...' : 'Try Again' }}
          </button>
          
          <button 
            *ngIf="config.showReloadButton"
            class="btn btn-secondary me-2" 
            (click)="reloadPage()">
            <i class="fas fa-sync-alt"></i>
            Reload Page
          </button>
          
          <button 
            *ngIf="config.showNavigationButton !== false"
            class="btn btn-outline-primary" 
            (click)="navigateToFallback()">
            <i class="fas fa-home"></i>
            {{ config.fallbackRoute ? 'Go Back' : 'Go to Dashboard' }}
          </button>
        </div>
        
        <div class="error-support" *ngIf="showSupportInfo">
          <small class="text-muted">
            If this problem persists, please contact support with error ID: 
            <code>{{ errorId }}</code>
          </small>
        </div>
      </div>
    </div>
    
    <!-- Fallback content when no error -->
    <ng-content *ngIf="!hasError"></ng-content>
  `,
  styles: [`
    .error-boundary-container {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 400px;
      padding: 2rem;
      background-color: #f8f9fa;
      border: 1px solid #dee2e6;
      border-radius: 0.375rem;
      margin: 1rem 0;
    }
    
    .error-boundary-content {
      text-align: center;
      max-width: 500px;
    }
    
    .error-icon {
      font-size: 3rem;
      color: #dc3545;
      margin-bottom: 1rem;
    }
    
    .error-title {
      color: #dc3545;
      margin-bottom: 1rem;
      font-weight: 600;
    }
    
    .error-message {
      color: #6c757d;
      margin-bottom: 1.5rem;
      line-height: 1.5;
    }
    
    .error-details {
      margin-bottom: 1.5rem;
      text-align: left;
    }
    
    .error-details-content {
      background-color: #f1f3f4;
      border: 1px solid #dee2e6;
      border-radius: 0.25rem;
      padding: 1rem;
      margin-top: 0.5rem;
      max-height: 200px;
      overflow-y: auto;
    }
    
    .error-details-content pre {
      margin: 0;
      font-size: 0.875rem;
      white-space: pre-wrap;
      word-break: break-word;
    }
    
    .error-actions {
      margin-bottom: 1rem;
    }
    
    .error-support {
      margin-top: 1rem;
      padding-top: 1rem;
      border-top: 1px solid #dee2e6;
    }
    
    .error-support code {
      background-color: #e9ecef;
      padding: 0.125rem 0.25rem;
      border-radius: 0.25rem;
      font-size: 0.875rem;
    }
    
    @media (max-width: 576px) {
      .error-boundary-container {
        padding: 1rem;
        min-height: 300px;
      }
      
      .error-actions .btn {
        display: block;
        width: 100%;
        margin-bottom: 0.5rem;
        margin-right: 0 !important;
      }
    }
  `]
})
export class ErrorBoundaryComponent implements OnInit, OnDestroy {
  @Input() config: ErrorBoundaryConfig = {};
  @Input() error: any = null;
  @Input() showDetails: boolean = false;
  @Input() showSupportInfo: boolean = true;
  @Input() retryCallback?: () => void;

  private readonly errorHandlingService = inject(ErrorHandlingService);
  private readonly loadingService = inject(LoadingService);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();

  hasError = false;
  errorTitle = 'Something went wrong';
  errorMessage = 'An unexpected error occurred. Please try again.';
  errorDetails = '';
  errorId = '';
  showErrorDetails = false;
  isRetrying = false;

  ngOnInit(): void {
    if (this.error) {
      this.handleError(this.error);
    }

    // Listen for global errors if no specific error is provided
    if (!this.error) {
      this.errorHandlingService.errorLogs$.pipe(
        takeUntil(this.destroy$)
      ).subscribe(logs => {
        const recentCriticalError = logs.find(log => 
          log.severity === 'critical' && 
          !log.handled &&
          (Date.now() - log.timestamp.getTime()) < 5000 // Within last 5 seconds
        );
        
        if (recentCriticalError && !this.hasError) {
          this.handleError(recentCriticalError.error);
        }
      });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Handle error and display error boundary
   */
  handleError(error: any): void {
    this.hasError = true;
    this.errorId = this.generateErrorId();
    
    // Customize error message based on error type
    if (error?.status) {
      // HTTP error
      switch (error.status) {
        case 0:
          this.errorTitle = 'Connection Error';
          this.errorMessage = 'Unable to connect to the server. Please check your internet connection and try again.';
          break;
        case 404:
          this.errorTitle = 'Page Not Found';
          this.errorMessage = 'The requested page could not be found. It may have been moved or deleted.';
          break;
        case 500:
          this.errorTitle = 'Server Error';
          this.errorMessage = 'The server encountered an error. Please try again later.';
          break;
        default:
          this.errorTitle = 'Request Failed';
          this.errorMessage = error.message || 'The request could not be completed. Please try again.';
      }
    } else if (error?.message) {
      // JavaScript error
      this.errorTitle = 'Application Error';
      this.errorMessage = this.config.customMessage || 'An unexpected error occurred in the application.';
      this.errorDetails = error.message + (error.stack ? '\n\n' + error.stack : '');
    } else {
      // Unknown error
      this.errorTitle = 'Unknown Error';
      this.errorMessage = this.config.customMessage || 'An unknown error occurred. Please try again.';
      this.errorDetails = JSON.stringify(error, null, 2);
    }

    // Log error if enabled
    if (this.config.logError !== false) {
      console.error('Error boundary caught error:', error);
    }

    // Clear any loading states
    this.loadingService.clearAll();
  }

  /**
   * Toggle error details visibility
   */
  toggleDetails(): void {
    this.showErrorDetails = !this.showErrorDetails;
  }

  /**
   * Retry the failed operation
   */
  retry(): void {
    if (this.isRetrying) {
      return;
    }

    this.isRetrying = true;

    if (this.retryCallback) {
      try {
        this.retryCallback();
        
        // Reset error state after successful retry
        setTimeout(() => {
          this.hasError = false;
          this.isRetrying = false;
        }, 1000);
      } catch (error) {
        console.error('Retry callback failed:', error);
        this.isRetrying = false;
      }
    } else {
      // Default retry behavior - reload the current route
      setTimeout(() => {
        window.location.reload();
      }, 1000);
    }
  }

  /**
   * Reload the current page
   */
  reloadPage(): void {
    window.location.reload();
  }

  /**
   * Navigate to fallback route
   */
  navigateToFallback(): void {
    const fallbackRoute = this.config.fallbackRoute || '/dashboard';
    this.router.navigate([fallbackRoute]);
  }

  /**
   * Generate unique error ID for support
   */
  private generateErrorId(): string {
    const timestamp = Date.now().toString(36);
    const random = Math.random().toString(36).substr(2, 5);
    return `ERR-${timestamp}-${random}`.toUpperCase();
  }
}