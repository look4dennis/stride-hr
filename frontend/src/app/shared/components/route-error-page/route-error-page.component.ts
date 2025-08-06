import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { RouteLoadingService } from '../../../core/services/route-loading.service';

@Component({
  selector: 'app-route-error-page',
  imports: [CommonModule],
  template: `
    <div class="route-error-page">
      <div class="container">
        <div class="error-content">
          <div class="error-icon">
            <i class="fas fa-exclamation-triangle"></i>
          </div>
          <h1>Page Loading Failed</h1>
          <p class="error-description">
            We encountered an issue while loading the requested page. This could be due to:
          </p>
          <ul class="error-reasons">
            <li>Network connectivity issues</li>
            <li>Temporary server problems</li>
            <li>Browser cache issues</li>
            <li>The page may have been moved or removed</li>
          </ul>
          
          <div class="error-details" *ngIf="routeInfo">
            <h5>Technical Details:</h5>
            <div class="detail-item">
              <strong>Route:</strong> {{ routeInfo.route }}
            </div>
            <div class="detail-item" *ngIf="routeInfo.error">
              <strong>Error:</strong> {{ routeInfo.error }}
            </div>
            <div class="detail-item" *ngIf="routeInfo.timestamp">
              <strong>Time:</strong> {{ formatTimestamp(routeInfo.timestamp) }}
            </div>
          </div>

          <div class="action-buttons">
            <button class="btn btn-primary" (click)="retryRoute()">
              <i class="fas fa-redo me-2"></i>
              Try Again
            </button>
            <button class="btn btn-outline-secondary" (click)="goToDashboard()">
              <i class="fas fa-home me-2"></i>
              Go to Dashboard
            </button>
            <button class="btn btn-outline-info" (click)="refreshPage()">
              <i class="fas fa-refresh me-2"></i>
              Refresh Page
            </button>
          </div>

          <div class="help-section">
            <h6>Still having trouble?</h6>
            <p>
              If the problem persists, please contact your system administrator or try:
            </p>
            <ul>
              <li>Clearing your browser cache and cookies</li>
              <li>Trying a different browser</li>
              <li>Checking your internet connection</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .route-error-page {
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem;
    }

    .container {
      max-width: 800px;
      width: 100%;
    }

    .error-content {
      background: white;
      border-radius: 16px;
      padding: 3rem;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
      text-align: center;
    }

    .error-icon {
      font-size: 5rem;
      color: #ffc107;
      margin-bottom: 2rem;
      animation: pulse 2s infinite;
    }

    @keyframes pulse {
      0% { transform: scale(1); }
      50% { transform: scale(1.1); }
      100% { transform: scale(1); }
    }

    h1 {
      color: #495057;
      font-weight: 700;
      font-size: 2.5rem;
      margin-bottom: 1.5rem;
    }

    .error-description {
      color: #6c757d;
      font-size: 1.1rem;
      margin-bottom: 1.5rem;
      line-height: 1.6;
    }

    .error-reasons {
      text-align: left;
      max-width: 400px;
      margin: 0 auto 2rem;
      color: #6c757d;
    }

    .error-reasons li {
      margin-bottom: 0.5rem;
      padding-left: 0.5rem;
    }

    .error-details {
      background: #f8f9fa;
      border: 1px solid #e9ecef;
      border-radius: 8px;
      padding: 1.5rem;
      margin: 2rem 0;
      text-align: left;
    }

    .error-details h5 {
      color: #495057;
      margin-bottom: 1rem;
      font-weight: 600;
    }

    .detail-item {
      margin-bottom: 0.75rem;
      font-family: 'Courier New', monospace;
      font-size: 0.9rem;
      word-break: break-all;
    }

    .detail-item strong {
      color: #212529;
      font-family: inherit;
    }

    .action-buttons {
      display: flex;
      gap: 1rem;
      justify-content: center;
      flex-wrap: wrap;
      margin: 2rem 0;
    }

    .btn {
      border-radius: 8px;
      font-weight: 500;
      padding: 0.75rem 1.5rem;
      transition: all 0.15s ease-in-out;
      border: 2px solid transparent;
      text-decoration: none;
      display: inline-flex;
      align-items: center;
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
      background: transparent;
    }

    .btn-outline-secondary:hover {
      background: #6c757d;
      color: white;
      transform: translateY(-2px);
    }

    .btn-outline-info {
      border-color: #17a2b8;
      color: #17a2b8;
      background: transparent;
    }

    .btn-outline-info:hover {
      background: #17a2b8;
      color: white;
      transform: translateY(-2px);
    }

    .help-section {
      margin-top: 3rem;
      padding-top: 2rem;
      border-top: 1px solid #e9ecef;
      text-align: left;
    }

    .help-section h6 {
      color: #495057;
      font-weight: 600;
      margin-bottom: 1rem;
    }

    .help-section p {
      color: #6c757d;
      margin-bottom: 1rem;
    }

    .help-section ul {
      color: #6c757d;
      margin-left: 1rem;
    }

    .help-section li {
      margin-bottom: 0.5rem;
    }

    @media (max-width: 768px) {
      .route-error-page {
        padding: 1rem;
      }

      .error-content {
        padding: 2rem;
      }

      h1 {
        font-size: 2rem;
      }

      .error-icon {
        font-size: 4rem;
      }

      .action-buttons {
        flex-direction: column;
      }

      .btn {
        width: 100%;
        justify-content: center;
      }

      .help-section {
        text-align: center;
      }
    }
  `]
})
export class RouteErrorPageComponent implements OnInit {
  routeInfo: {
    route?: string;
    error?: string;
    timestamp?: string;
  } = {};

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private routeLoadingService: RouteLoadingService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.routeInfo = {
        route: params['route'],
        error: params['error'],
        timestamp: params['timestamp']
      };
    });
  }

  retryRoute(): void {
    if (this.routeInfo.route) {
      // Clear the error for this route
      this.routeLoadingService.clearRouteError(this.routeInfo.route);
      
      // Navigate to the failed route
      this.router.navigate([this.routeInfo.route]);
    } else {
      this.refreshPage();
    }
  }

  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  refreshPage(): void {
    window.location.reload();
  }

  formatTimestamp(timestamp: string): string {
    try {
      return new Date(timestamp).toLocaleString();
    } catch {
      return timestamp;
    }
  }
}