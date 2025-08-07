import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ErrorBoundaryComponent, ErrorBoundaryConfig } from './error-boundary.component';

/**
 * Example component showing how to use the ErrorBoundaryComponent
 * This can be used as a reference for implementing error boundaries in feature components
 */
@Component({
  selector: 'app-error-boundary-example',
  standalone: true,
  imports: [CommonModule, ErrorBoundaryComponent],
  template: `
    <!-- Basic error boundary usage -->
    <app-error-boundary>
      <div class="content-that-might-fail">
        <h3>This content is protected by an error boundary</h3>
        <p>If an error occurs in this section, the error boundary will catch it.</p>
        
        <button class="btn btn-danger" (click)="triggerError()">
          Trigger Test Error
        </button>
      </div>
    </app-error-boundary>

    <!-- Error boundary with custom configuration -->
    <app-error-boundary 
      [config]="customErrorConfig"
      [retryCallback]="retryOperation">
      <div class="another-protected-section">
        <h3>Custom Error Boundary</h3>
        <p>This section has custom error handling configuration.</p>
        
        <button class="btn btn-warning" (click)="triggerNetworkError()">
          Trigger Network Error
        </button>
      </div>
    </app-error-boundary>

    <!-- Error boundary for specific error -->
    <app-error-boundary 
      [error]="specificError"
      [config]="{ showRetryButton: true, customMessage: 'Failed to load user data' }"
      [retryCallback]="loadUserData">
      <div class="user-data-section" *ngIf="!specificError">
        <h3>User Data</h3>
        <p>User data would be displayed here...</p>
      </div>
    </app-error-boundary>
  `,
  styles: [`
    .content-that-might-fail,
    .another-protected-section,
    .user-data-section {
      padding: 1rem;
      margin: 1rem 0;
      border: 1px solid #dee2e6;
      border-radius: 0.375rem;
      background-color: #f8f9fa;
    }

    .btn {
      margin-right: 0.5rem;
    }
  `]
})
export class ErrorBoundaryExampleComponent {
  specificError: any = null;

  customErrorConfig: ErrorBoundaryConfig = {
    showRetryButton: true,
    showReloadButton: false,
    showNavigationButton: true,
    fallbackRoute: '/dashboard',
    customMessage: 'Something went wrong with this feature. Please try again.',
    logError: true
  };

  /**
   * Trigger a test error to demonstrate error boundary
   */
  triggerError(): void {
    throw new Error('This is a test error to demonstrate error boundary functionality');
  }

  /**
   * Trigger a network error simulation
   */
  triggerNetworkError(): void {
    const networkError = {
      status: 500,
      message: 'Internal Server Error',
      statusText: 'Internal Server Error'
    };
    
    // Simulate an error that would be caught by error boundary
    setTimeout(() => {
      throw networkError;
    }, 100);
  }

  /**
   * Retry operation callback
   */
  retryOperation = (): void => {
    console.log('Retrying operation...');
    
    // Simulate a retry operation
    setTimeout(() => {
      console.log('Retry completed successfully');
      // In a real scenario, you would re-execute the failed operation
    }, 1000);
  };

  /**
   * Load user data with error handling
   */
  loadUserData = (): void => {
    console.log('Loading user data...');
    
    // Simulate loading user data
    setTimeout(() => {
      // Simulate success - clear the error
      this.specificError = null;
      console.log('User data loaded successfully');
    }, 1000);
  };

  /**
   * Simulate a specific error for demonstration
   */
  simulateSpecificError(): void {
    this.specificError = {
      status: 404,
      message: 'User data not found',
      statusText: 'Not Found'
    };
  }
}