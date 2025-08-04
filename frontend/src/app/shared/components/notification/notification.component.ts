import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { NotificationService, Notification } from '../../../core/services/notification.service';

@Component({
    selector: 'app-notification',
    imports: [CommonModule],
    template: `
    <div class="notification-container">
      <div 
        *ngFor="let notification of notifications" 
        class="alert alert-dismissible fade show"
        [ngClass]="getAlertClass(notification.type)"
        role="alert">
        <div class="d-flex align-items-start">
          <i class="me-2 mt-1" [ngClass]="getIconClass(notification.type)"></i>
          <div class="flex-grow-1">
            <strong>{{ notification.title }}</strong>
            <div>{{ notification.message }}</div>
            <small class="text-muted">{{ formatTime(notification.timestamp) }}</small>
          </div>
          <button 
            type="button" 
            class="btn-close" 
            (click)="removeNotification(notification.id)"
            aria-label="Close">
          </button>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .notification-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 1050;
      max-width: 400px;
      width: 100%;
    }

    .alert {
      margin-bottom: 10px;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
      border: none;
      border-radius: 8px;
    }

    .alert-success {
      background-color: #d1fae5;
      color: #065f46;
    }

    .alert-danger {
      background-color: #fee2e2;
      color: #991b1b;
    }

    .alert-warning {
      background-color: #fef3c7;
      color: #92400e;
    }

    .alert-info {
      background-color: #dbeafe;
      color: #1e40af;
    }

    @media (max-width: 768px) {
      .notification-container {
        left: 20px;
        right: 20px;
        max-width: none;
      }
    }
  `]
})
export class NotificationComponent implements OnInit, OnDestroy {
  notifications: Notification[] = [];
  private subscription: Subscription = new Subscription();

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.subscription.add(
      this.notificationService.notifications$.subscribe(
        notifications => this.notifications = notifications
      )
    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  removeNotification(id: string): void {
    this.notificationService.removeNotification(id);
  }

  getAlertClass(type: string): string {
    const classes = {
      success: 'alert-success',
      error: 'alert-danger',
      warning: 'alert-warning',
      info: 'alert-info'
    };
    return classes[type as keyof typeof classes] || 'alert-info';
  }

  getIconClass(type: string): string {
    const icons = {
      success: 'fas fa-check-circle text-success',
      error: 'fas fa-exclamation-circle text-danger',
      warning: 'fas fa-exclamation-triangle text-warning',
      info: 'fas fa-info-circle text-info'
    };
    return icons[type as keyof typeof icons] || 'fas fa-info-circle';
  }

  formatTime(timestamp: Date): string {
    return new Date(timestamp).toLocaleTimeString();
  }
}