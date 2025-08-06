import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-route-error',
  imports: [CommonModule],
  template: `
    <div class="route-error-container">
      <div class="error-content">
        <div class="error-icon">
          <i class="fas fa-exclamation-triangle"></i>
        </div>
        <h2>Oops! Something went wrong</h2>
        <p class="error-message">{{ errorMessage }}</p>
        <div class="error-details" *ngIf="showDetails">
          <p><strong>Route:</strong> {{ failedRoute }}</p>
          <p><strong>Error:</strong> {{ technicalError }}</p>
        </div>
        <div class="action-buttons">
          <button class="btn btn-primary" (click)="retry()">
            <i class="fas fa-redo me-2"></i>Try Again
          </button>
          <button class="btn btn-outline-secondary" (click)="goHome()">
            <i class="fas fa-home me-2"></i>Go to Dashboard
          </button>
          <button class="btn btn-outline-info" (click)="toggleDetails()">
            <i class="fas fa-info-circle me-2"></i>
            {{ showDetails ? 'Hide' : 'Show' }} Details
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .route-error-container {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 60vh;
      padding: 2rem;
      background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
    }

    .error-content {
      text-align: center;
      max-width: 600px;
      background: white;
      padding: 3rem;
      border-radius: 16px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
    }

    .error-icon {
      font-size: 4rem;
      color: #ffc107;
      margin-bottom: 1.5rem;
    }

    .error-content h2 {
      color: #495057;
      font-weight: 700;
      margin-bottom: 1rem;
      font-size: 2rem;
    }

    .error-message {
      color: #6c757d;
      font-size: 1.1rem;
      margin-bottom: 2rem;
      line-height: 1.6;
    }

    .error-details {
      background: #f8f9fa;
      border: 1px solid #e9ecef;
      border-radius: 8px;
      padding: 1.5rem;
      margin-bottom: 2rem;
      text-align: left;
    }

    .error-details p {
      margin-bottom: 0.5rem;
      font-family: 'Courier New', monospace;
      font-size: 0.9rem;
      color: #495057;
    }

    .error-details strong {
      color: #212529;
    }

    .action-buttons {
      display: flex;
      gap: 1rem;
      justify-content: center;
      flex-wrap: wrap;
    }

    .btn {
      border-radius: 8px;
      font-weight: 500;
      padding: 0.75rem 1.5rem;
      transition: all 0.15s ease-in-out;
      border: 2px solid transparent;
    }

    .btn-primary {
      background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
      color: white;
    }

    .btn-primary:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0, 123, 255, 0.4);
    }

    .btn-outline-secondary {
      border-color: #6c757d;
      color: #6c757d;
    }

    .btn-outline-secondary:hover {
      background: #6c757d;
      color: white;
      transform: translateY(-2px);
    }

    .btn-outline-info {
      border-color: #17a2b8;
      color: #17a2b8;
    }

    .btn-outline-info:hover {
      background: #17a2b8;
      color: white;
      transform: translateY(-2px);
    }

    @media (max-width: 768px) {
      .route-error-container {
        padding: 1rem;
      }

      .error-content {
        padding: 2rem;
      }

      .error-content h2 {
        font-size: 1.5rem;
      }

      .action-buttons {
        flex-direction: column;
      }

      .btn {
        width: 100%;
      }
    }
  `]
})
export class RouteErrorComponent {
  @Input() errorMessage = 'The page you requested could not be loaded. This might be due to a temporary issue or the page may not exist.';
  @Input() failedRoute = '';
  @Input() technicalError = '';
  
  showDetails = false;

  constructor(private router: Router) {}

  retry(): void {
    if (this.failedRoute) {
      this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
        this.router.navigate([this.failedRoute]);
      });
    } else {
      window.location.reload();
    }
  }

  goHome(): void {
    this.router.navigate(['/dashboard']);
  }

  toggleDetails(): void {
    this.showDetails = !this.showDetails;
  }
}