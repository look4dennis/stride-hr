import { TestBed, ComponentFixture } from '@angular/core/testing';
import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ServiceWorkerModule, SwUpdate } from '@angular/service-worker';
import { TestConfig } from './test-config';
import { of, Subject } from 'rxjs';

/**
 * PWA Installation and Offline Functionality Tests
 * Tests service worker registration, app installation, and offline capabilities
 */

@Component({
  selector: 'app-pwa-installation',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="pwa-test-container">
      <!-- Installation prompt -->
      <div class="install-section" *ngIf="canInstall">
        <h3>Install StrideHR</h3>
        <p>Install StrideHR for a better experience with offline access and push notifications.</p>
        <button class="btn btn-primary install-btn" (click)="installApp()">
          Install App
        </button>
        <button class="btn btn-secondary" (click)="dismissInstall()">
          Not Now
        </button>
      </div>

      <!-- Service Worker Status -->
      <div class="sw-status">
        <h4>Service Worker Status</h4>
        <div class="status-item">
          <span class="label">Registered:</span>
          <span class="value" [class.success]="swRegistered" [class.error]="!swRegistered">
            {{ swRegistered ? 'Yes' : 'No' }}
          </span>
        </div>
        <div class="status-item">
          <span class="label">Update Available:</span>
          <span class="value" [class.warning]="updateAvailable">
            {{ updateAvailable ? 'Yes' : 'No' }}
          </span>
        </div>
        <div class="status-item">
          <span class="label">Offline Ready:</span>
          <span class="value" [class.success]="offlineReady" [class.error]="!offlineReady">
            {{ offlineReady ? 'Yes' : 'No' }}
          </span>
        </div>
      </div>

      <!-- Connection Status -->
      <div class="connection-status">
        <h4>Connection Status</h4>
        <div class="status-indicator" [class.online]="isOnline" [class.offline]="!isOnline">
          <span class="status-dot"></span>
          {{ isOnline ? 'Online' : 'Offline' }}
        </div>
        <button class="btn btn-sm btn-outline-secondary" (click)="toggleOfflineMode()">
          {{ isOnline ? 'Simulate Offline' : 'Go Online' }}
        </button>
      </div>

      <!-- Offline Features -->
      <div class="offline-features">
        <h4>Offline Features</h4>
        <div class="feature-list">
          <div class="feature-item" *ngFor="let feature of offlineFeatures">
            <span class="feature-icon" [class.available]="feature.available">
              {{ feature.available ? '✓' : '✗' }}
            </span>
            <span class="feature-name">{{ feature.name }}</span>
            <span class="feature-status">{{ feature.status }}</span>
          </div>
        </div>
      </div>

      <!-- Cache Status -->
      <div class="cache-status">
        <h4>Cache Status</h4>
        <div class="cache-info">
          <div class="cache-item">
            <span class="label">App Shell:</span>
            <span class="value">{{ cacheStatus.appShell }}</span>
          </div>
          <div class="cache-item">
            <span class="label">API Data:</span>
            <span class="value">{{ cacheStatus.apiData }}</span>
          </div>
          <div class="cache-item">
            <span class="label">Assets:</span>
            <span class="value">{{ cacheStatus.assets }}</span>
          </div>
        </div>
        <button class="btn btn-sm btn-outline-primary" (click)="refreshCache()">
          Refresh Cache
        </button>
        <button class="btn btn-sm btn-outline-danger" (click)="clearCache()">
          Clear Cache
        </button>
      </div>

      <!-- Push Notifications -->
      <div class="push-notifications">
        <h4>Push Notifications</h4>
        <div class="notification-status">
          <span class="label">Permission:</span>
          <span class="value" [ngClass]="getNotificationPermissionClass()">
            {{ notificationPermission }}
          </span>
        </div>
        <button 
          class="btn btn-sm btn-primary" 
          (click)="requestNotificationPermission()"
          [disabled]="notificationPermission === 'granted'">
          Enable Notifications
        </button>
        <button 
          class="btn btn-sm btn-secondary" 
          (click)="sendTestNotification()"
          [disabled]="notificationPermission !== 'granted'">
          Test Notification
        </button>
      </div>

      <!-- Offline Data Sync -->
      <div class="offline-sync">
        <h4>Offline Data Sync</h4>
        <div class="sync-status">
          <span class="label">Pending Actions:</span>
          <span class="value">{{ pendingSyncActions }}</span>
        </div>
        <div class="sync-actions">
          <button class="btn btn-sm btn-primary" (click)="addOfflineAction()">
            Add Test Action
          </button>
          <button class="btn btn-sm btn-success" (click)="syncOfflineData()">
            Sync Now
          </button>
          <button class="btn btn-sm btn-warning" (click)="clearOfflineData()">
            Clear Offline Data
          </button>
        </div>
      </div>

      <!-- App Update -->
      <div class="app-update" *ngIf="updateAvailable">
        <div class="update-banner">
          <span class="update-icon">🔄</span>
          <span class="update-message">A new version of StrideHR is available!</span>
          <button class="btn btn-sm btn-primary" (click)="updateApp()">
            Update Now
          </button>
          <button class="btn btn-sm btn-secondary" (click)="dismissUpdate()">
            Later
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .pwa-test-container {
      padding: 1rem;
      max-width: 800px;
      margin: 0 auto;
    }

    .install-section,
    .sw-status,
    .connection-status,
    .offline-features,
    .cache-status,
    .push-notifications,
    .offline-sync,
    .app-update {
      background: #f8f9fa;
      border: 1px solid #dee2e6;
      border-radius: 0.5rem;
      padding: 1.5rem;
      margin-bottom: 1.5rem;
    }

    .install-section {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      text-align: center;
    }

    .install-section h3 {
      margin-bottom: 1rem;
    }

    .install-section p {
      margin-bottom: 1.5rem;
      opacity: 0.9;
    }

    .install-btn {
      background: rgba(255, 255, 255, 0.2);
      border: 2px solid rgba(255, 255, 255, 0.3);
      color: white;
      margin-right: 1rem;
    }

    .install-btn:hover {
      background: rgba(255, 255, 255, 0.3);
      border-color: rgba(255, 255, 255, 0.5);
    }

    h4 {
      margin-bottom: 1rem;
      color: #495057;
      font-size: 1.1rem;
    }

    .status-item,
    .cache-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.5rem;
      padding: 0.5rem;
      background: white;
      border-radius: 0.25rem;
    }

    .label {
      font-weight: 500;
      color: #6c757d;
    }

    .value {
      font-weight: 600;
    }

    .value.success {
      color: #28a745;
    }

    .value.error {
      color: #dc3545;
    }

    .value.warning {
      color: #ffc107;
    }

    .connection-status {
      text-align: center;
    }

    .status-indicator {
      display: inline-flex;
      align-items: center;
      padding: 0.5rem 1rem;
      border-radius: 2rem;
      font-weight: 600;
      margin-bottom: 1rem;
    }

    .status-indicator.online {
      background: #d4edda;
      color: #155724;
    }

    .status-indicator.offline {
      background: #f8d7da;
      color: #721c24;
    }

    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      margin-right: 0.5rem;
    }

    .status-indicator.online .status-dot {
      background: #28a745;
    }

    .status-indicator.offline .status-dot {
      background: #dc3545;
    }

    .feature-list {
      display: grid;
      gap: 0.5rem;
    }

    .feature-item {
      display: flex;
      align-items: center;
      padding: 0.75rem;
      background: white;
      border-radius: 0.25rem;
    }

    .feature-icon {
      width: 24px;
      text-align: center;
      margin-right: 0.75rem;
      font-weight: bold;
    }

    .feature-icon.available {
      color: #28a745;
    }

    .feature-icon:not(.available) {
      color: #dc3545;
    }

    .feature-name {
      flex: 1;
      font-weight: 500;
    }

    .feature-status {
      font-size: 0.875rem;
      color: #6c757d;
    }

    .cache-info {
      margin-bottom: 1rem;
    }

    .notification-status {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.75rem;
      background: white;
      border-radius: 0.25rem;
      margin-bottom: 1rem;
    }

    .granted {
      color: #28a745;
    }

    .denied {
      color: #dc3545;
    }

    .default {
      color: #ffc107;
    }

    .sync-status {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.75rem;
      background: white;
      border-radius: 0.25rem;
      margin-bottom: 1rem;
    }

    .sync-actions {
      display: flex;
      gap: 0.5rem;
      flex-wrap: wrap;
    }

    .update-banner {
      display: flex;
      align-items: center;
      padding: 1rem;
      background: #fff3cd;
      border: 1px solid #ffeaa7;
      border-radius: 0.375rem;
      color: #856404;
    }

    .update-icon {
      font-size: 1.5rem;
      margin-right: 1rem;
    }

    .update-message {
      flex: 1;
      font-weight: 500;
    }

    .update-banner button {
      margin-left: 0.5rem;
    }

    .btn {
      padding: 0.375rem 0.75rem;
      border-radius: 0.375rem;
      border: 1px solid transparent;
      font-weight: 500;
      text-decoration: none;
      cursor: pointer;
      transition: all 0.15s ease-in-out;
    }

    .btn-primary {
      background: #007bff;
      border-color: #007bff;
      color: white;
    }

    .btn-secondary {
      background: #6c757d;
      border-color: #6c757d;
      color: white;
    }

    .btn-success {
      background: #28a745;
      border-color: #28a745;
      color: white;
    }

    .btn-warning {
      background: #ffc107;
      border-color: #ffc107;
      color: #212529;
    }

    .btn-danger {
      background: #dc3545;
      border-color: #dc3545;
      color: white;
    }

    .btn-outline-primary {
      background: transparent;
      border-color: #007bff;
      color: #007bff;
    }

    .btn-outline-secondary {
      background: transparent;
      border-color: #6c757d;
      color: #6c757d;
    }

    .btn-outline-danger {
      background: transparent;
      border-color: #dc3545;
      color: #dc3545;
    }

    .btn:disabled {
      opacity: 0.65;
      cursor: not-allowed;
    }

    .btn-sm {
      padding: 0.25rem 0.5rem;
      font-size: 0.875rem;
    }

    @media (max-width: 768px) {
      .pwa-test-container {
        padding: 0.5rem;
      }

      .sync-actions {
        flex-direction: column;
      }

      .update-banner {
        flex-direction: column;
        text-align: center;
      }

      .update-banner button {
        margin: 0.25rem;
      }
    }
  `]
})
class PWAInstallationComponent {
  canInstall = false;
  swRegistered = false;
  updateAvailable = false;
  offlineReady = false;
  isOnline = navigator.onLine;
  notificationPermission: NotificationPermission = 'default';
  pendingSyncActions = 0;

  offlineFeatures = [
    { name: 'View cached employee data', available: true, status: 'Ready' },
    { name: 'Submit attendance offline', available: true, status: 'Ready' },
    { name: 'Access dashboard widgets', available: true, status: 'Ready' },
    { name: 'View recent reports', available: true, status: 'Ready' },
    { name: 'Submit leave requests', available: true, status: 'Ready' },
    { name: 'Background sync', available: false, status: 'Not supported' }
  ];

  cacheStatus = {
    appShell: 'Cached',
    apiData: 'Partial',
    assets: 'Cached'
  };

  private deferredPrompt: any;

  constructor() {
    this.initializePWA();
    this.setupEventListeners();
    this.checkNotificationPermission();
    this.loadPendingSyncActions();
  }

  private initializePWA() {
    // Check service worker registration
    if ('serviceWorker' in navigator) {
      navigator.serviceWorker.ready.then(registration => {
        this.swRegistered = true;
        this.offlineReady = true;
        console.log('Service Worker registered:', registration);
      }).catch(error => {
        console.error('Service Worker registration failed:', error);
      });
    }

    // Check for app install prompt
    window.addEventListener('beforeinstallprompt', (e) => {
      e.preventDefault();
      this.deferredPrompt = e;
      this.canInstall = true;
    });

    // Check if already installed
    window.addEventListener('appinstalled', () => {
      this.canInstall = false;
      console.log('PWA was installed');
    });
  }

  private setupEventListeners() {
    // Online/offline events
    window.addEventListener('online', () => {
      this.isOnline = true;
      this.syncOfflineData();
    });

    window.addEventListener('offline', () => {
      this.isOnline = false;
    });

    // Service worker update events
    if ('serviceWorker' in navigator) {
      navigator.serviceWorker.addEventListener('message', event => {
        if (event.data && event.data.type === 'UPDATE_AVAILABLE') {
          this.updateAvailable = true;
        }
      });
    }
  }

  private checkNotificationPermission() {
    if ('Notification' in window) {
      this.notificationPermission = Notification.permission;
    }
  }

  private loadPendingSyncActions() {
    const stored = localStorage.getItem('stride-hr-offline-actions');
    if (stored) {
      const actions = JSON.parse(stored);
      this.pendingSyncActions = actions.filter((action: any) => !action.synced).length;
    }
  }

  async installApp() {
    if (this.deferredPrompt) {
      this.deferredPrompt.prompt();
      const { outcome } = await this.deferredPrompt.userChoice;
      
      if (outcome === 'accepted') {
        console.log('User accepted the install prompt');
        this.canInstall = false;
      } else {
        console.log('User dismissed the install prompt');
      }
      
      this.deferredPrompt = null;
    }
  }

  dismissInstall() {
    this.canInstall = false;
    this.deferredPrompt = null;
  }

  toggleOfflineMode() {
    // This is a simulation for testing
    this.isOnline = !this.isOnline;
    
    if (this.isOnline) {
      window.dispatchEvent(new Event('online'));
    } else {
      window.dispatchEvent(new Event('offline'));
    }
  }

  async refreshCache() {
    if ('serviceWorker' in navigator) {
      const registration = await navigator.serviceWorker.ready;
      if (registration.active) {
        registration.active.postMessage({ type: 'REFRESH_CACHE' });
        console.log('Cache refresh requested');
      }
    }
  }

  async clearCache() {
    if ('caches' in window) {
      const cacheNames = await caches.keys();
      await Promise.all(
        cacheNames.map(cacheName => caches.delete(cacheName))
      );
      console.log('All caches cleared');
      
      // Update cache status
      this.cacheStatus = {
        appShell: 'Cleared',
        apiData: 'Cleared',
        assets: 'Cleared'
      };
    }
  }

  async requestNotificationPermission() {
    if ('Notification' in window) {
      const permission = await Notification.requestPermission();
      this.notificationPermission = permission;
      
      if (permission === 'granted') {
        console.log('Notification permission granted');
      } else {
        console.log('Notification permission denied');
      }
    }
  }

  sendTestNotification() {
    if ('Notification' in window && Notification.permission === 'granted') {
      new Notification('StrideHR Test Notification', {
        body: 'This is a test notification from StrideHR PWA',
        icon: '/icons/icon-192x192.png',
        badge: '/icons/icon-72x72.png',
        tag: 'test-notification'
      });
    }
  }

  addOfflineAction() {
    const action = {
      id: Date.now().toString(),
      type: 'attendance',
      action: 'check-in',
      data: {
        timestamp: new Date().toISOString(),
        location: 'Test Location'
      },
      synced: false
    };

    const stored = localStorage.getItem('stride-hr-offline-actions');
    const actions = stored ? JSON.parse(stored) : [];
    actions.push(action);
    
    localStorage.setItem('stride-hr-offline-actions', JSON.stringify(actions));
    this.pendingSyncActions = actions.filter((a: any) => !a.synced).length;
    
    console.log('Offline action added:', action);
  }

  async syncOfflineData() {
    const stored = localStorage.getItem('stride-hr-offline-actions');
    if (!stored) return;

    const actions = JSON.parse(stored);
    const pendingActions = actions.filter((action: any) => !action.synced);

    if (pendingActions.length === 0) {
      console.log('No pending actions to sync');
      return;
    }

    console.log(`Syncing ${pendingActions.length} offline actions...`);

    // Simulate sync process
    for (const action of pendingActions) {
      try {
        // In a real app, this would make API calls
        await this.simulateApiCall(action);
        action.synced = true;
        console.log('Synced action:', action.id);
      } catch (error) {
        console.error('Failed to sync action:', action.id, error);
      }
    }

    localStorage.setItem('stride-hr-offline-actions', JSON.stringify(actions));
    this.pendingSyncActions = actions.filter((a: any) => !a.synced).length;

    // Show success notification
    if ('Notification' in window && Notification.permission === 'granted') {
      new Notification('Data Synchronized', {
        body: `${pendingActions.length} offline actions have been synchronized.`,
        icon: '/icons/icon-192x192.png',
        tag: 'sync-complete'
      });
    }
  }

  private async simulateApiCall(action: any): Promise<void> {
    // Simulate network delay
    await new Promise(resolve => setTimeout(resolve, 500));
    
    // Simulate occasional failures
    if (Math.random() < 0.1) {
      throw new Error('Network error');
    }
    
    console.log('API call simulated for action:', action.type);
  }

  clearOfflineData() {
    localStorage.removeItem('stride-hr-offline-actions');
    this.pendingSyncActions = 0;
    console.log('Offline data cleared');
  }

  updateApp() {
    if ('serviceWorker' in navigator) {
      navigator.serviceWorker.ready.then(registration => {
        if (registration.waiting) {
          registration.waiting.postMessage({ type: 'SKIP_WAITING' });
          window.location.reload();
        }
      });
    }
  }

  dismissUpdate() {
    this.updateAvailable = false;
  }

  getNotificationPermissionClass(): string {
    return this.notificationPermission;
  }
}

describe('PWA Installation and Offline Functionality Tests', () => {
  let component: PWAInstallationComponent;
  let fixture: ComponentFixture<PWAInstallationComponent>;
  let swUpdate: jasmine.SpyObj<SwUpdate>;

  beforeEach(async () => {
    TestConfig.setupAllMocks();
    
    const swUpdateSpy = jasmine.createSpyObj('SwUpdate', ['checkForUpdate', 'activateUpdate'], {
      isEnabled: true,
      versionUpdates: new Subject()
    });

    await TestBed.configureTestingModule({
      imports: [
        PWAInstallationComponent,
        ServiceWorkerModule.register('ngsw-worker.js', { enabled: false })
      ],
      providers: [
        { provide: SwUpdate, useValue: swUpdateSpy }
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(PWAInstallationComponent);
    component = fixture.componentInstance;
    swUpdate = TestBed.inject(SwUpdate) as jasmine.SpyObj<SwUpdate>;
    fixture.detectChanges();
  });

  afterEach(() => {
    fixture.destroy();
    localStorage.clear();
  });

  describe('PWA Installation', () => {
    it('should handle app installation prompt', () => {
      // Simulate beforeinstallprompt event
      const mockPrompt = {
        prompt: jasmine.createSpy('prompt'),
        userChoice: Promise.resolve({ outcome: 'accepted' })
      };

      component['deferredPrompt'] = mockPrompt;
      component.canInstall = true;
      fixture.detectChanges();

      const installButton = fixture.nativeElement.querySelector('.install-btn');
      expect(installButton).toBeTruthy();

      // Test installation
      component.installApp();
      expect(mockPrompt.prompt).toHaveBeenCalled();
    });

    it('should dismiss installation prompt', () => {
      component.canInstall = true;
      component['deferredPrompt'] = {};
      
      component.dismissInstall();
      
      expect(component.canInstall).toBe(false);
      expect(component['deferredPrompt']).toBeNull();
    });

    it('should hide install section when not installable', () => {
      component.canInstall = false;
      fixture.detectChanges();

      const installSection = fixture.nativeElement.querySelector('.install-section');
      expect(installSection).toBeFalsy();
    });
  });

  describe('Service Worker Status', () => {
    it('should display service worker registration status', () => {
      const statusItems = fixture.nativeElement.querySelectorAll('.status-item');
      expect(statusItems.length).toBeGreaterThan(0);

      // Check for registration status
      const registeredStatus = Array.from(statusItems).find((item: any) => 
        item.textContent.includes('Registered')
      );
      expect(registeredStatus).toBeTruthy();
    });

    it('should handle service worker registration', async () => {
      // Mock service worker registration
      Object.defineProperty(navigator, 'serviceWorker', {
        writable: true,
        value: {
          ready: Promise.resolve({
            active: true,
            waiting: null
          }),
          addEventListener: jasmine.createSpy('addEventListener')
        }
      });

      component['initializePWA']();
      
      // Wait for async operations
      await new Promise(resolve => setTimeout(resolve, 100));
      
      expect(component.swRegistered).toBe(true);
      expect(component.offlineReady).toBe(true);
    });
  });

  describe('Connection Status', () => {
    it('should display current connection status', () => {
      const connectionStatus = fixture.nativeElement.querySelector('.connection-status');
      const statusIndicator = fixture.nativeElement.querySelector('.status-indicator');
      
      expect(connectionStatus).toBeTruthy();
      expect(statusIndicator).toBeTruthy();
      
      // Should reflect navigator.onLine
      expect(component.isOnline).toBe(navigator.onLine);
    });

    it('should toggle offline mode for testing', () => {
      const initialStatus = component.isOnline;
      
      component.toggleOfflineMode();
      
      expect(component.isOnline).toBe(!initialStatus);
    });

    it('should handle online/offline events', () => {
      spyOn(component, 'syncOfflineData');
      
      // Simulate going online
      component.isOnline = false;
      window.dispatchEvent(new Event('online'));
      
      expect(component.isOnline).toBe(true);
      expect(component.syncOfflineData).toHaveBeenCalled();
      
      // Simulate going offline
      window.dispatchEvent(new Event('offline'));
      expect(component.isOnline).toBe(false);
    });
  });

  describe('Offline Features', () => {
    it('should display available offline features', () => {
      const featureItems = fixture.nativeElement.querySelectorAll('.feature-item');
      expect(featureItems.length).toBe(component.offlineFeatures.length);

      // Check feature availability indicators
      const availableFeatures = fixture.nativeElement.querySelectorAll('.feature-icon.available');
      const expectedAvailable = component.offlineFeatures.filter(f => f.available).length;
      expect(availableFeatures.length).toBe(expectedAvailable);
    });

    it('should show correct feature status', () => {
      const firstFeature = component.offlineFeatures[0];
      expect(firstFeature.name).toBeTruthy();
      expect(firstFeature.available).toBeDefined();
      expect(firstFeature.status).toBeTruthy();
    });
  });

  describe('Cache Management', () => {
    it('should display cache status', () => {
      const cacheItems = fixture.nativeElement.querySelectorAll('.cache-item');
      expect(cacheItems.length).toBeGreaterThan(0);

      // Check cache status values
      expect(component.cacheStatus.appShell).toBeTruthy();
      expect(component.cacheStatus.apiData).toBeTruthy();
      expect(component.cacheStatus.assets).toBeTruthy();
    });

    it('should handle cache refresh', async () => {
      // Mock service worker
      Object.defineProperty(navigator, 'serviceWorker', {
        writable: true,
        value: {
          ready: Promise.resolve({
            active: {
              postMessage: jasmine.createSpy('postMessage')
            }
          })
        }
      });

      await component.refreshCache();
      
      // Should send refresh message to service worker
      const registration = await navigator.serviceWorker.ready;
      expect(registration.active?.postMessage).toHaveBeenCalledWith({ type: 'REFRESH_CACHE' });
    });

    it('should handle cache clearing', async () => {
      // Mock caches API
      Object.defineProperty(window, 'caches', {
        writable: true,
        value: {
          keys: () => Promise.resolve(['cache1', 'cache2']),
          delete: jasmine.createSpy('delete').and.returnValue(Promise.resolve(true))
        }
      });

      await component.clearCache();
      
      expect(caches.delete).toHaveBeenCalledWith('cache1');
      expect(caches.delete).toHaveBeenCalledWith('cache2');
      expect(component.cacheStatus.appShell).toBe('Cleared');
    });
  });

  describe('Push Notifications', () => {
    it('should check notification permission', () => {
      // Mock Notification API
      Object.defineProperty(window, 'Notification', {
        writable: true,
        value: {
          permission: 'default',
          requestPermission: jasmine.createSpy('requestPermission').and.returnValue(Promise.resolve('granted'))
        }
      });

      component['checkNotificationPermission']();
      expect(component.notificationPermission).toBe('default');
    });

    it('should request notification permission', async () => {
      Object.defineProperty(window, 'Notification', {
        writable: true,
        value: {
          permission: 'default',
          requestPermission: jasmine.createSpy('requestPermission').and.returnValue(Promise.resolve('granted'))
        }
      });

      await component.requestNotificationPermission();
      
      expect(Notification.requestPermission).toHaveBeenCalled();
      expect(component.notificationPermission).toBe('granted');
    });

    it('should send test notification when permitted', () => {
      const NotificationSpy = jasmine.createSpy('Notification').and.returnValue({});
      (NotificationSpy as any).permission = 'granted';
      Object.defineProperty(window, 'Notification', {
        writable: true,
        value: NotificationSpy
      });

      component.notificationPermission = 'granted';
      component.sendTestNotification();
      
      expect(Notification).toHaveBeenCalledWith('StrideHR Test Notification', jasmine.any(Object));
    });

    it('should get correct permission class', () => {
      component.notificationPermission = 'granted';
      expect(component.getNotificationPermissionClass()).toBe('granted');

      component.notificationPermission = 'denied';
      expect(component.getNotificationPermissionClass()).toBe('denied');

      component.notificationPermission = 'default';
      expect(component.getNotificationPermissionClass()).toBe('default');
    });
  });

  describe('Offline Data Sync', () => {
    it('should add offline actions', () => {
      const initialCount = component.pendingSyncActions;
      
      component.addOfflineAction();
      
      expect(component.pendingSyncActions).toBe(initialCount + 1);
      
      // Check localStorage
      const stored = localStorage.getItem('stride-hr-offline-actions');
      expect(stored).toBeTruthy();
      
      const actions = JSON.parse(stored!);
      expect(actions.length).toBeGreaterThan(0);
      expect(actions[actions.length - 1].synced).toBe(false);
    });

    it('should sync offline data', async () => {
      // Add test data
      const testAction = {
        id: '123',
        type: 'attendance',
        action: 'check-in',
        data: { timestamp: new Date().toISOString() },
        synced: false
      };
      
      localStorage.setItem('stride-hr-offline-actions', JSON.stringify([testAction]));
      component.pendingSyncActions = 1;

      await component.syncOfflineData();
      
      // Check that action was marked as synced
      const stored = localStorage.getItem('stride-hr-offline-actions');
      const actions = JSON.parse(stored!);
      expect(actions[0].synced).toBe(true);
      expect(component.pendingSyncActions).toBe(0);
    });

    it('should clear offline data', () => {
      // Add test data
      localStorage.setItem('stride-hr-offline-actions', JSON.stringify([{ id: '123' }]));
      component.pendingSyncActions = 1;
      
      component.clearOfflineData();
      
      expect(localStorage.getItem('stride-hr-offline-actions')).toBeNull();
      expect(component.pendingSyncActions).toBe(0);
    });

    it('should load pending sync actions on init', () => {
      const testActions = [
        { id: '1', synced: false },
        { id: '2', synced: true },
        { id: '3', synced: false }
      ];
      
      localStorage.setItem('stride-hr-offline-actions', JSON.stringify(testActions));
      
      component['loadPendingSyncActions']();
      
      expect(component.pendingSyncActions).toBe(2); // Only unsynced actions
    });
  });

  describe('App Updates', () => {
    it('should handle app updates', () => {
      component.updateAvailable = true;
      fixture.detectChanges();

      const updateBanner = fixture.nativeElement.querySelector('.update-banner');
      expect(updateBanner).toBeTruthy();

      const updateButton = fixture.nativeElement.querySelector('.update-banner .btn-primary');
      expect(updateButton).toBeTruthy();
    });

    it('should dismiss update notification', () => {
      component.updateAvailable = true;
      
      component.dismissUpdate();
      
      expect(component.updateAvailable).toBe(false);
    });

    it('should trigger app update', () => {
      // Mock service worker with waiting worker
      Object.defineProperty(navigator, 'serviceWorker', {
        writable: true,
        value: {
          ready: Promise.resolve({
            waiting: {
              postMessage: jasmine.createSpy('postMessage')
            }
          })
        }
      });

      spyOn(window.location, 'reload');

      component.updateApp();
      
      // Should trigger reload after sending skip waiting message
      setTimeout(() => {
        expect(window.location.reload).toHaveBeenCalled();
      }, 100);
    });
  });

  describe('Mobile PWA Features', () => {
    it('should adapt to mobile viewport', () => {
      // Simulate mobile viewport
      Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: 375 });
      window.dispatchEvent(new Event('resize'));
      
      fixture.detectChanges();
      
      // Component should handle mobile layout
      expect(component).toBeTruthy();
    });

    it('should support touch interactions', () => {
      const installButton = fixture.nativeElement.querySelector('.install-btn');
      
      if (installButton) {
        // Simulate touch event
        const touchEvent = new TouchEvent('touchstart', {
          touches: [new Touch({
            identifier: 1,
            target: installButton,
            clientX: 100,
            clientY: 100,
            radiusX: 2.5,
            radiusY: 2.5,
            rotationAngle: 10,
            force: 0.5
          })]
        });
        
        expect(() => {
          installButton.dispatchEvent(touchEvent);
        }).not.toThrow();
      }
    });
  });
});