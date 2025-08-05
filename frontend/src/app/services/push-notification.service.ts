import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

export interface NotificationPayload {
  title: string;
  body: string;
  icon?: string;
  badge?: string;
  tag?: string;
  data?: any;
  actions?: Array<{
    action: string;
    title: string;
    icon?: string;
  }>;
  requireInteraction?: boolean;
  silent?: boolean;
  vibrate?: number[];
}

@Injectable({
  providedIn: 'root'
})
export class PushNotificationService {
  private readonly API_URL = environment.apiUrl;
  private subscriptionSubject = new BehaviorSubject<PushSubscription | null>(null);
  public readonly subscription$ = this.subscriptionSubject.asObservable();

  constructor(private http: HttpClient) {
    this.initializeSubscription();
  }

  /**
   * Initialize push notification subscription
   */
  private async initializeSubscription(): Promise<void> {
    if (!this.isSupported()) {
      console.warn('Push notifications are not supported');
      return;
    }

    try {
      const registration = await navigator.serviceWorker.ready;
      const subscription = await registration.pushManager.getSubscription();
      this.subscriptionSubject.next(subscription);
    } catch (error) {
      console.error('Error initializing push subscription:', error);
    }
  }

  /**
   * Check if push notifications are supported
   */
  isSupported(): boolean {
    return 'serviceWorker' in navigator && 
           'PushManager' in window && 
           'Notification' in window;
  }

  /**
   * Request notification permission
   */
  async requestPermission(): Promise<NotificationPermission> {
    if (!this.isSupported()) {
      return 'denied';
    }

    if (Notification.permission === 'granted') {
      return 'granted';
    }

    if (Notification.permission === 'denied') {
      return 'denied';
    }

    const permission = await Notification.requestPermission();
    return permission;
  }

  /**
   * Subscribe to push notifications
   */
  async subscribe(): Promise<PushSubscription | null> {
    if (!this.isSupported()) {
      console.warn('Push notifications not supported');
      return null;
    }

    const permission = await this.requestPermission();
    if (permission !== 'granted') {
      console.warn('Push notification permission denied');
      return null;
    }

    try {
      const registration = await navigator.serviceWorker.ready;
      
      // Check if already subscribed
      let subscription = await registration.pushManager.getSubscription();
      
      if (!subscription) {
        // Create new subscription
        subscription = await registration.pushManager.subscribe({
          userVisibleOnly: true,
          applicationServerKey: this.urlBase64ToUint8Array(environment.vapidPublicKey)
        });
      }

      // Send subscription to server
      await this.sendSubscriptionToServer(subscription);
      this.subscriptionSubject.next(subscription);
      
      return subscription;
    } catch (error) {
      console.error('Error subscribing to push notifications:', error);
      return null;
    }
  }

  /**
   * Unsubscribe from push notifications
   */
  async unsubscribe(): Promise<boolean> {
    try {
      const registration = await navigator.serviceWorker.ready;
      const subscription = await registration.pushManager.getSubscription();
      
      if (subscription) {
        const unsubscribed = await subscription.unsubscribe();
        if (unsubscribed) {
          await this.removeSubscriptionFromServer(subscription);
          this.subscriptionSubject.next(null);
        }
        return unsubscribed;
      }
      
      return true;
    } catch (error) {
      console.error('Error unsubscribing from push notifications:', error);
      return false;
    }
  }

  /**
   * Show local notification
   */
  async showLocalNotification(payload: NotificationPayload): Promise<void> {
    const permission = await this.requestPermission();
    if (permission !== 'granted') {
      console.warn('Cannot show notification: permission denied');
      return;
    }

    const options: NotificationOptions = {
      body: payload.body,
      icon: payload.icon || '/icons/icon-192x192.png',
      badge: payload.badge || '/icons/icon-72x72.png',
      tag: payload.tag,
      data: payload.data,
      requireInteraction: payload.requireInteraction || false,
      silent: payload.silent || false
    };

    try {
      const registration = await navigator.serviceWorker.ready;
      await registration.showNotification(payload.title, options);
    } catch (error) {
      console.error('Error showing notification:', error);
      // Fallback to browser notification
      new Notification(payload.title, options);
    }
  }

  /**
   * Send subscription to server
   */
  private async sendSubscriptionToServer(subscription: PushSubscription): Promise<void> {
    const subscriptionData = {
      endpoint: subscription.endpoint,
      keys: {
        p256dh: this.arrayBufferToBase64(subscription.getKey('p256dh')!),
        auth: this.arrayBufferToBase64(subscription.getKey('auth')!)
      }
    };

    try {
      await firstValueFrom(this.http.post(`${this.API_URL}/notifications/subscribe`, subscriptionData));
      console.log('Push subscription sent to server');
    } catch (error) {
      console.error('Error sending subscription to server:', error);
    }
  }

  /**
   * Remove subscription from server
   */
  private async removeSubscriptionFromServer(subscription: PushSubscription): Promise<void> {
    try {
      await firstValueFrom(this.http.post(`${this.API_URL}/notifications/unsubscribe`, {
        endpoint: subscription.endpoint
      }));
      console.log('Push subscription removed from server');
    } catch (error) {
      console.error('Error removing subscription from server:', error);
    }
  }

  /**
   * Get notification preferences
   */
  getNotificationPreferences(): Observable<any> {
    return this.http.get(`${this.API_URL}/notifications/preferences`);
  }

  /**
   * Update notification preferences
   */
  updateNotificationPreferences(preferences: any): Observable<any> {
    return this.http.put(`${this.API_URL}/notifications/preferences`, preferences);
  }

  /**
   * Test push notification
   */
  async testNotification(): Promise<void> {
    await this.showLocalNotification({
      title: 'StrideHR Test Notification',
      body: 'Push notifications are working correctly!',
      tag: 'test-notification',
      requireInteraction: true,
      actions: [
        {
          action: 'view',
          title: 'View Dashboard',
          icon: '/icons/icon-72x72.png'
        },
        {
          action: 'dismiss',
          title: 'Dismiss',
          icon: '/icons/icon-72x72.png'
        }
      ]
    });
  }

  /**
   * Show attendance reminder notification
   */
  async showAttendanceReminder(): Promise<void> {
    await this.showLocalNotification({
      title: 'Attendance Reminder',
      body: 'Don\'t forget to check in for today!',
      tag: 'attendance-reminder',
      data: { type: 'attendance', action: 'check-in' },
      actions: [
        {
          action: 'checkin',
          title: 'Check In Now',
          icon: '/icons/icon-72x72.png'
        }
      ]
    });
  }

  /**
   * Show DSR reminder notification
   */
  async showDSRReminder(): Promise<void> {
    await this.showLocalNotification({
      title: 'DSR Reminder',
      body: 'Please submit your Daily Status Report',
      tag: 'dsr-reminder',
      data: { type: 'dsr', action: 'submit' },
      actions: [
        {
          action: 'submit-dsr',
          title: 'Submit DSR',
          icon: '/icons/icon-72x72.png'
        }
      ]
    });
  }

  /**
   * Show leave approval notification
   */
  async showLeaveApprovalNotification(employeeName: string, leaveType: string): Promise<void> {
    await this.showLocalNotification({
      title: 'Leave Request Pending',
      body: `${employeeName} has requested ${leaveType} leave`,
      tag: 'leave-approval',
      data: { type: 'leave', action: 'approve' },
      requireInteraction: true,
      actions: [
        {
          action: 'approve',
          title: 'Approve',
          icon: '/icons/icon-72x72.png'
        },
        {
          action: 'view',
          title: 'View Details',
          icon: '/icons/icon-72x72.png'
        }
      ]
    });
  }

  /**
   * Show birthday notification
   */
  async showBirthdayNotification(employeeName: string): Promise<void> {
    await this.showLocalNotification({
      title: '🎉 Birthday Today!',
      body: `It's ${employeeName}'s birthday today. Don't forget to wish them!`,
      tag: 'birthday-notification',
      data: { type: 'birthday', employeeName },
      actions: [
        {
          action: 'send-wishes',
          title: 'Send Wishes',
          icon: '/icons/icon-72x72.png'
        }
      ]
    });
  }

  /**
   * Utility methods
   */
  private urlBase64ToUint8Array(base64String: string): Uint8Array {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
      .replace(/-/g, '+')
      .replace(/_/g, '/');

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
      outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
  }

  private arrayBufferToBase64(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.byteLength; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
  }
}