import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NavigationService } from '../../../core/services/navigation.service';

@Component({
    selector: 'app-not-found',
    imports: [CommonModule, RouterModule],
    template: `
    <div class="error-container">
      <div class="error-content">
        <div class="error-icon">
          <i class="fas fa-search text-primary"></i>
        </div>
        <h1 class="error-title">404</h1>
        <h2 class="error-subtitle">Page Not Found</h2>
        <p class="error-message">
          The page you're looking for doesn't exist or has been moved.
          Please check the URL or navigate back to the dashboard.
        </p>
        <div class="error-actions">
          <a routerLink="/dashboard" class="btn btn-primary">
            <i class="fas fa-home me-2"></i>
            Go to Dashboard
          </a>
          <button class="btn btn-outline-secondary ms-2" (click)="goBack()">
            <i class="fas fa-arrow-left me-2"></i>
            Go Back
          </button>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .error-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--bg-secondary);
      padding: 2rem;
    }

    .error-content {
      text-align: center;
      max-width: 500px;
    }

    .error-icon {
      font-size: 4rem;
      margin-bottom: 1rem;
    }

    .error-title {
      font-size: 6rem;
      font-weight: 700;
      color: var(--primary);
      margin-bottom: 0.5rem;
      line-height: 1;
    }

    .error-subtitle {
      font-size: 2rem;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 1rem;
    }

    .error-message {
      font-size: 1.1rem;
      color: var(--text-secondary);
      margin-bottom: 2rem;
      line-height: 1.6;
    }

    .error-actions {
      display: flex;
      justify-content: center;
      gap: 1rem;
      flex-wrap: wrap;
    }

    .btn {
      border-radius: 8px;
      font-weight: 500;
      padding: 0.75rem 1.5rem;
    }
  `]
})
export class NotFoundComponent {
  constructor(private navigationService: NavigationService) {}

  goBack(): void {
    this.navigationService.goBack();
  }
}