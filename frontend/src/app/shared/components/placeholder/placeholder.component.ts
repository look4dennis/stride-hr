import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-placeholder',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="placeholder-container">
      <div class="placeholder-content">
        <div class="placeholder-icon">
          <i class="fas fa-cog fa-spin"></i>
        </div>
        <h3>{{ title || 'Feature Coming Soon' }}</h3>
        <p>{{ message || 'This feature is currently under development and will be available soon.' }}</p>
        <div class="placeholder-actions" *ngIf="showActions">
          <button class="btn btn-primary" (click)="goBack()">Go Back</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .placeholder-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 400px;
      padding: 40px 20px;
    }

    .placeholder-content {
      text-align: center;
      max-width: 500px;
    }

    .placeholder-icon {
      font-size: 4rem;
      color: #6c757d;
      margin-bottom: 20px;
    }

    .placeholder-content h3 {
      color: #495057;
      margin-bottom: 15px;
      font-weight: 300;
    }

    .placeholder-content p {
      color: #6c757d;
      margin-bottom: 30px;
      line-height: 1.6;
    }

    .placeholder-actions {
      margin-top: 20px;
    }

    .btn {
      padding: 10px 20px;
      border: none;
      border-radius: 5px;
      cursor: pointer;
      font-size: 14px;
      transition: all 0.3s ease;
    }

    .btn-primary {
      background-color: #007bff;
      color: white;
    }

    .btn-primary:hover {
      background-color: #0056b3;
    }

    @keyframes pulse {
      0% { opacity: 0.6; }
      50% { opacity: 1; }
      100% { opacity: 0.6; }
    }

    .placeholder-icon i {
      animation: pulse 2s infinite;
    }
  `]
})
export class PlaceholderComponent {
  @Input() title?: string;
  @Input() message?: string;
  @Input() showActions = true;

  goBack(): void {
    window.history.back();
  }
}