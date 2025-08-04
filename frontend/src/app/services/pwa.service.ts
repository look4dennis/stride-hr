import { Injectable, ApplicationRef } from '@angular/core';
import { SwUpdate, VersionReadyEvent } from '@angular/service-worker';
import { BehaviorSubject, Observable, concat, interval, of } from 'rxjs';
import { first, filter, switchMap, tap } from 'rxjs/operators';

export interface PwaInstallPrompt {
  prompt(): Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

@Injectable({
  providedIn: 'root'
})
export class PwaService {
  private deferredPrompt: PwaInstallPrompt | null = null;
  private isOnlineSubject = new BehaviorSubject<boolean>(navigator.onLine);
  private installPromptSubject = new BehaviorSubject<boolean>(false);
  private updateAvailableSubject = new BehaviorSubject<boolean>(false);

  public readonly isOnline$ = this.isOnlineSubject.asObservable();
  public readonly canInstall$ = this.installPromptSubject.asObservable();
  public readonly updateAvailable$ = this.updateAvailableSubject.asObservable();

  constructor(
    private swUpdate: SwUpdate,
    private appRef: ApplicationRef
  ) {
    this.initializeNetworkStatus();
    this.initializeServiceWorkerUpdates();
    this.initializeInstallPrompt();
  }

  /**
   * Initialize network status monitoring
   */
  private initializeNetworkStatus(): void {
    window.addEventListener('online', () => {
      this.isOnlineSubject.next(true);
      this.syncOfflineData();
    });

    window.addEventListener('offline', () => {
      this.isOnlineSubject.next(false);
    });
  }

  /**
   * Initialize service worker update checking
   */
  private initializeServiceWorkerUpdates(): void {
    if (!this.swUpdate.isEnabled) {
      return;
    }

    // Check for updates when app becomes stable
    const appIsStable$ = this.appRef.isStable.pipe(
      first(isStable => isStable === true)
    );
    const everySixHours$ = interval(6 * 60 * 60 * 1000);
    const everySixHoursOnceAppIsStable$ = concat(appIsStable$, everySixHours$);

    everySixHoursOnceAppIsStable$.subscribe(() => {
      this.swUpdate.checkForUpdate();
    });

    // Handle version updates
    this.swUpdate.versionUpdates
      .pipe(
        filter((evt): evt is VersionReadyEvent => evt.type === 'VERSION_READY')
      )
      .subscribe(() => {
        this.updateAvailableSubject.next(true);
      });
  }

  /**
   * Initialize PWA install prompt handling
   */
  private initializeInstallPrompt(): void {
    window.addEventListener('beforeinstallprompt', (e: any) => {
      e.preventDefault();
      this.deferredPrompt = e;
      this.installPromptSubject.next(true);
    });

    window.addEventListener('appinstalled', () => {
      this.deferredPrompt = null;
      this.installPromptSubject.next(false);
      this.showInstallSuccessMessage();
    });
  }

  /**
   * Prompt user to install PWA
   */
  async promptInstall(): Promise<boolean> {
    if (!this.deferredPrompt) {
      return false;
    }

    try {
      await this.deferredPrompt.prompt();
      const choiceResult = await this.deferredPrompt.userChoice;
      
      if (choiceResult.outcome === 'accepted') {
        this.deferredPrompt = null;
        this.installPromptSubject.next(false);
        return true;
      }
      
      return false;
    } catch (error) {
      console.error('Error prompting for install:', error);
      return false;
    }
  }

  /**
   * Apply available service worker update
   */
  async applyUpdate(): Promise<void> {
    if (!this.swUpdate.isEnabled) {
      return;
    }

    try {
      await this.swUpdate.activateUpdate();
      this.updateAvailableSubject.next(false);
      window.location.reload();
    } catch (error) {
      console.error('Error applying update:', error);
    }
  }

  /**
   * Check if PWA is running in standalone mode
   */
  isStandalone(): boolean {
    return window.matchMedia('(display-mode: standalone)').matches ||
           (window.navigator as any).standalone === true;
  }

  /**
   * Request notification permission
   */
  async requestNotificationPermission(): Promise<NotificationPermission> {
    if (!('Notification' in window)) {
      console.warn('This browser does not support notifications');
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
   * Show local notification
   */
  async showNotification(title: string, options?: NotificationOptions): Promise<void> {
    const permission = await this.requestNotificationPermission();
    
    if (permission !== 'granted') {
      console.warn('Notification permission not granted');
      return;
    }

    const defaultOptions: NotificationOptions = {
      icon: '/icons/icon-192x192.png',
      badge: '/icons/icon-72x72.png',
      ...options
    };

    if ('serviceWorker' in navigator && 'showNotification' in ServiceWorkerRegistration.prototype) {
      const registration = await navigator.serviceWorker.ready;
      await registration.showNotification(title, defaultOptions);
    } else {
      new Notification(title, defaultOptions);
    }
  }

  /**
   * Subscribe to push notifications
   */
  async subscribeToPushNotifications(): Promise<PushSubscription | null> {
    if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
      console.warn('Push notifications not supported');
      return null;
    }

    try {
      const registration = await navigator.serviceWorker.ready;
      const subscription = await registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: this.urlBase64ToUint8Array(this.getVapidPublicKey())
      });

      // Send subscription to server
      await this.sendSubscriptionToServer(subscription);
      return subscription;
    } catch (error) {
      console.error('Error subscribing to push notifications:', error);
      return null;
    }
  }

  /**
   * Sync offline data when connection is restored
   */
  private async syncOfflineData(): Promise<void> {
    try {
      // Get offline data from IndexedDB or localStorage
      const offlineData = this.getOfflineData();
      
      if (offlineData.length > 0) {
        console.log('Syncing offline data...', offlineData);
        
        // Process each offline action
        for (const data of offlineData) {
          await this.syncSingleItem(data);
        }
        
        // Clear offline data after successful sync
        this.clearOfflineData();
        
        // Show sync success notification
        await this.showNotification('Data Synchronized', {
          body: `${offlineData.length} offline actions have been synchronized.`,
          tag: 'sync-success'
        });
      }
    } catch (error) {
      console.error('Error syncing offline data:', error);
    }
  }

  /**
   * Store data for offline sync
   */
  storeOfflineData(action: string, data: any): void {
    const offlineData = this.getOfflineData();
    const newItem = {
      id: Date.now().toString(),
      action,
      data,
      timestamp: new Date().toISOString()
    };
    
    offlineData.push(newItem);
    localStorage.setItem('stride-hr-offline-data', JSON.stringify(offlineData));
  }

  /**
   * Get stored offline data
   */
  private getOfflineData(): any[] {
    const data = localStorage.getItem('stride-hr-offline-data');
    return data ? JSON.parse(data) : [];
  }

  /**
   * Clear offline data
   */
  private clearOfflineData(): void {
    localStorage.removeItem('stride-hr-offline-data');
  }

  /**
   * Sync a single offline item
   */
  private async syncSingleItem(item: any): Promise<void> {
    // This would be implemented based on the specific action type
    switch (item.action) {
      case 'attendance-checkin':
        // Sync attendance check-in
        break;
      case 'attendance-checkout':
        // Sync attendance check-out
        break;
      case 'dsr-submit':
        // Sync DSR submission
        break;
      case 'leave-request':
        // Sync leave request
        break;
      default:
        console.warn('Unknown offline action:', item.action);
    }
  }

  /**
   * Send push subscription to server
   */
  private async sendSubscriptionToServer(subscription: PushSubscription): Promise<void> {
    // This would send the subscription to your backend API
    console.log('Sending subscription to server:', subscription);
    // Implementation would depend on your API structure
  }

  /**
   * Get VAPID public key (this should come from your environment config)
   */
  private getVapidPublicKey(): string {
    // This should be your actual VAPID public key
    return 'BEl62iUYgUivxIkv69yViEuiBIa40HI80NqIUHI80NqIUHI80NqIUHI80NqIUHI80NqIUHI80NqIUHI80NqIUHI80NqI';
  }

  /**
   * Convert VAPID key to Uint8Array
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

  /**
   * Show install success message
   */
  private showInstallSuccessMessage(): void {
    console.log('StrideHR has been installed successfully!');
    // You could show a toast notification here
  }
}