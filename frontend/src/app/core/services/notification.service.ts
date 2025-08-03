import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  duration?: number;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notifications = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notifications.asObservable();

  showSuccess(message: string, title: string = 'Success', duration: number = 5000): void {
    this.addNotification('success', title, message, duration);
  }

  showError(message: string, title: string = 'Error', duration: number = 8000): void {
    this.addNotification('error', title, message, duration);
  }

  showWarning(message: string, title: string = 'Warning', duration: number = 6000): void {
    this.addNotification('warning', title, message, duration);
  }

  showInfo(message: string, title: string = 'Info', duration: number = 5000): void {
    this.addNotification('info', title, message, duration);
  }

  removeNotification(id: string): void {
    const current = this.notifications.value;
    const updated = current.filter(n => n.id !== id);
    this.notifications.next(updated);
  }

  clearAll(): void {
    this.notifications.next([]);
  }

  private addNotification(type: Notification['type'], title: string, message: string, duration?: number): void {
    const notification: Notification = {
      id: this.generateId(),
      type,
      title,
      message,
      duration,
      timestamp: new Date()
    };

    const current = this.notifications.value;
    this.notifications.next([notification, ...current]);

    // Auto-remove notification after duration
    if (duration && duration > 0) {
      setTimeout(() => {
        this.removeNotification(notification.id);
      }, duration);
    }
  }

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }
}