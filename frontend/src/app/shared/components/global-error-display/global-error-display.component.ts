import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ErrorHandlingService, ErrorLog } from '../../../core/services/error-handling.service';
import { ConnectionService } from '../../../core/services/connection.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-global-error-display',
  standalone: true,
  imports: [CommonModule],
  template: `
    <!-- Critical Error Banner -->
    <div 
      *ngIf="criticalError" 
      class="alert alert-danger alert-dismissible critical-error-banner" 
      role="alert">
      <div class="d-flex align-items-center">
        <i class="fas fa-exclamation-triangle me-2"></i>
        <div class="flex-grow-1">
          <strong>Critical Error:</strong> {{ criticalError.error.message || 'A critical system error occurred' }}
        </div>
        <button 
          type="button" 
          class="btn-close" 
          (click)="dismissCriticalError()"
          aria-label="Close">
        </button>
      </div>
      
      <div class="mt-2" *ngIf="criticalError">
        <small class="text-muted">
          Error ID: {{ criticalError.id }} | 
          Time: {{ criticalError.timestamp | date:'short' }}
        </small>
        <div class="mt-1">
          <button 
            class="btn btn-sm btn-outline-light me-2" 
            (click)="reloadPage()">
            <i class="fas fa-sync-alt"></i> Reload Page
          </button>
          <button 
            class="btn btn-sm btn-outline-light" 
            (click)="showErrorDetails = !showErrorDetails">
            <i class="fas fa-info-circle"></i> 
            {{ showErrorDetails ? 'Hide' : 'Show' }} Details
          </button>
        </div>
        
        <div class="error-details mt-2" *ngIf="showErrorDetails">
          <pre class="small">{{ formatErrorDetails(criticalError) }}</pre>
        </div>
      </div>
    </div>

    <!-- Connection Status Banner -->
    <div 
      *ngIf="!isOnline" 
      class="alert alert-warning connection-banner" 
      role="alert">
      <div class="d-flex align-items-center">
        <i class="fas fa-wifi-slash me-2"></i>
        <div class="flex-grow-1">
          <strong>You are offline.</strong> 
          Some features may not be available. 
          <span *ngIf="pendingActionsCount > 0">
            {{ pendingActionsCount }} actions are queued for when you reconnect.
          </span>
        </div>
        <button 
          class="btn btn-sm btn-outline-warning" 
          (click)="testConnection()"
          [disabled]="isTestingConnection">
          <i class="fas fa-redo" *ngIf="!isTestingConnection"></i>
          <i class="fas fa-spinner fa-spin" *ngIf="isTestingConnection"></i>
          Test Connection
        </button>
      </div>
    </div>

    <!-- Error Summary (for development/admin) -->
    <div 
      *ngIf="showErrorSummary && recentErrors.length > 0" 
      class="alert alert-info error-summary" 
      role="alert">
      <div class="d-flex align-items-center justify-content-between">
        <div>
          <i class="fas fa-bug me-2"></i>
          <strong>{{ recentErrors.length }}</strong> recent errors detected
        </div>
        <div>
          <button 
            class="btn btn-sm btn-outline-info me-2" 
            (click)="toggleErrorSummary()">
            {{ showDetailedSummary ? 'Hide' : 'Show' }} Details
          </button>
          <button 
            class="btn btn-sm btn-outline-info" 
            (click)="clearErrorLogs()">
            Clear
          </button>
        </div>
      </div>
      
      <div class="mt-2" *ngIf="showDetailedSummary">
        <div class="error-list">
          <div 
            *ngFor="let error of recentErrors.slice(0, 5)" 
            class="error-item small">
            <div class="d-flex justify-content-between align-items-start">
              <div>
                <span class="badge" [class]="getErrorBadgeClass(error.severity)">
                  {{ error.severity }}
                </span>
                <span class="ms-2">{{ error.error.message || 'Unknown error' }}</span>
              </div>
              <small class="text-muted">{{ error.timestamp | date:'short' }}</small>
            </div>
            <div class="text-muted small mt-1" *ngIf="error.context.component">
              Component: {{ error.context.component }}
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .critical-error-banner {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      z-index: 9999;
      margin: 0;
      border-radius: 0;
      border: none;
      border-bottom: 3px solid #dc3545;
    }

    .connection-banner {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      z-index: 9998;
      margin: 0;
      border-radius: 0;
      border: none;
      border-bottom: 2px solid #ffc107;
    }

    .error-summary {
      position: fixed;
      bottom: 20px;
      right: 20px;
      max-width: 400px;
      z-index: 9997;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .error-details {
      background-color: rgba(255, 255, 255, 0.1);
      border-radius: 0.25rem;
      padding: 0.5rem;
      max-height: 200px;
      overflow-y: auto;
    }

    .error-details pre {
      margin: 0;
      white-space: pre-wrap;
      word-break: break-word;
      color: rgba(255, 255, 255, 0.9);
    }

    .error-list {
      max-height: 300px;
      overflow-y: auto;
    }

    .error-item {
      padding: 0.5rem;
      border-bottom: 1px solid rgba(0, 0, 0, 0.1);
    }

    .error-item:last-child {
      border-bottom: none;
    }

    .badge.bg-critical {
      background-color: #dc3545 !important;
    }

    .badge.bg-high {
      background-color: #fd7e14 !important;
    }

    .badge.bg-medium {
      background-color: #ffc107 !important;
      color: #000;
    }

    .badge.bg-low {
      background-color: #6c757d !important;
    }

    /* Responsive adjustments */
    @media (max-width: 768px) {
      .error-summary {
        position: fixed;
        bottom: 10px;
        left: 10px;
        right: 10px;
        max-width: none;
      }

      .critical-error-banner .btn {
        font-size: 0.75rem;
        padding: 0.25rem 0.5rem;
      }

      .connection-banner .btn {
        font-size: 0.75rem;
        padding: 0.25rem 0.5rem;
      }
    }

    /* Animation for banners */
    .critical-error-banner,
    .connection-banner {
      animation: slideDown 0.3s ease-out;
    }

    @keyframes slideDown {
      from {
        transform: translateY(-100%);
        opacity: 0;
      }
      to {
        transform: translateY(0);
        opacity: 1;
      }
    }

    .error-summary {
      animation: slideUp 0.3s ease-out;
    }

    @keyframes slideUp {
      from {
        transform: translateY(100%);
        opacity: 0;
      }
      to {
        transform: translateY(0);
        opacity: 1;
      }
    }
  `]
})
export class GlobalErrorDisplayComponent implements OnInit, OnDestroy {
  private readonly errorHandlingService = inject(ErrorHandlingService);
  private readonly connectionService = inject(ConnectionService);
  private readonly destroy$ = new Subject<void>();

  criticalError: ErrorLog | null = null;
  recentErrors: ErrorLog[] = [];
  isOnline = true;
  pendingActionsCount = 0;
  showErrorDetails = false;
  showErrorSummary = false; // Set to true in development
  showDetailedSummary = false;
  isTestingConnection = false;

  ngOnInit(): void {
    // Subscribe to error logs
    this.errorHandlingService.errorLogs$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(logs => {
      this.recentErrors = logs.filter(log => 
        (Date.now() - log.timestamp.getTime()) < 300000 // Last 5 minutes
      );

      // Show critical errors that haven't been handled
      const unhandledCritical = logs.find(log => 
        log.severity === 'critical' && 
        !log.handled &&
        (Date.now() - log.timestamp.getTime()) < 30000 // Last 30 seconds
      );

      if (unhandledCritical && !this.criticalError) {
        this.criticalError = unhandledCritical;
      }
    });

    // Subscribe to connection status
    this.connectionService.connectionStatus$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(status => {
      this.isOnline = status.isOnline && status.isConnectedToServer;
    });

    // Subscribe to offline actions
    this.connectionService.offlineActions$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(actions => {
      this.pendingActionsCount = actions.length;
    });

    // Enable error summary in development
    this.showErrorSummary = !environment.production;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Dismiss critical error banner
   */
  dismissCriticalError(): void {
    this.criticalError = null;
    this.showErrorDetails = false;
  }

  /**
   * Reload the page
   */
  reloadPage(): void {
    window.location.reload();
  }

  /**
   * Test connection manually
   */
  testConnection(): void {
    if (this.isTestingConnection) {
      return;
    }

    this.isTestingConnection = true;

    this.connectionService.testConnection().subscribe({
      next: (isConnected) => {
        this.isTestingConnection = false;
        console.log('Connection test result:', isConnected);
      },
      error: (error) => {
        this.isTestingConnection = false;
        console.error('Connection test failed:', error);
      }
    });
  }

  /**
   * Toggle error summary details
   */
  toggleErrorSummary(): void {
    this.showDetailedSummary = !this.showDetailedSummary;
  }

  /**
   * Clear error logs
   */
  clearErrorLogs(): void {
    this.errorHandlingService.clearErrorLogs();
    this.recentErrors = [];
  }

  /**
   * Format error details for display
   */
  formatErrorDetails(errorLog: ErrorLog): string {
    const details = {
      id: errorLog.id,
      timestamp: errorLog.timestamp,
      severity: errorLog.severity,
      context: errorLog.context,
      error: errorLog.error
    };

    return JSON.stringify(details, null, 2);
  }

  /**
   * Get CSS class for error severity badge
   */
  getErrorBadgeClass(severity: string): string {
    switch (severity) {
      case 'critical':
        return 'badge bg-critical';
      case 'high':
        return 'badge bg-high';
      case 'medium':
        return 'badge bg-medium';
      case 'low':
        return 'badge bg-low';
      default:
        return 'badge bg-secondary';
    }
  }
}