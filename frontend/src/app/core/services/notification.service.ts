import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title?: string;
  message: string;
  duration?: number;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  constructor() {}

  showSuccess(message: string, title?: string, duration: number = 5000): void {
    this.addNotification('success', message, title, duration);
  }

  showError(message: string, title?: string, duration: number = 8000): void {
    this.addNotification('error', message, title, duration);
  }

  showWarning(message: string, title?: string, duration: number = 6000): void {
    this.addNotification('warning', message, title, duration);
  }

  showInfo(message: string, title?: string, duration: number = 5000): void {
    this.addNotification('info', message, title, duration);
  }

  private addNotification(type: Notification['type'], message: string, title?: string, duration?: number): void {
    const notification: Notification = {
      id: this.generateId(),
      type,
      title,
      message,
      duration,
      timestamp: new Date()
    };

    const currentNotifications = this.notificationsSubject.value;
    this.notificationsSubject.next([...currentNotifications, notification]);

    // Auto-remove notification after duration
    if (duration && duration > 0) {
      setTimeout(() => {
        this.removeNotification(notification.id);
      }, duration);
    }

    // Also log to console for development
    console.log(`[${type.toUpperCase()}] ${title ? title + ': ' : ''}${message}`);
  }

  removeNotification(id: string): void {
    const currentNotifications = this.notificationsSubject.value;
    const updatedNotifications = currentNotifications.filter(n => n.id !== id);
    this.notificationsSubject.next(updatedNotifications);
  }

  clearAll(): void {
    this.notificationsSubject.next([]);
  }

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }
}