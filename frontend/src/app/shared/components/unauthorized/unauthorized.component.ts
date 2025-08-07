import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NavigationService } from '../../../core/services/navigation.service';

@Component({
    selector: 'app-unauthorized',
    imports: [CommonModule, RouterModule],
    template: `
    <div class="error-container">
      <div class="error-content">
        <div class="error-icon">
          <i class="fas fa-lock text-warning"></i>
        </div>
        <h1 class="error-title">403</h1>
        <h2 class="error-subtitle">Access Forbidden</h2>
        <p class="error-message">
          You don't have permission to access this resource.
          Please contact your administrator if you believe this is an error.
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
      color: var(--warning);
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
export class UnauthorizedComponent {
  constructor(private navigationService: NavigationService) {}

  goBack(): void {
    this.navigationService.goBack();
  }
}