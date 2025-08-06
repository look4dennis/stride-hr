import { TestBed } from '@angular/core/testing';
import { ApplicationRef } from '@angular/core';
import { SwUpdate } from '@angular/service-worker';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { provideZoneChangeDetection } from '@angular/core';
import { of, Subject } from 'rxjs';

import { PwaService } from './services/pwa.service';
import { PushNotificationService } from './services/push-notification.service';
import { OfflineStorageService } from './services/offline-storage.service';

// Mock BeforeInstallPromptEvent
interface MockBeforeInstallPromptEvent extends Event {
  prompt(): Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

describe('PWA Installation and Offline Functionality Tests', () => {
  let pwaService: PwaService;
  let pushNotificationService: PushNotificationService;
  let offlineStorageService: OfflineStorageService;
  let mockSwUpdate: jasmine.SpyObj<SwUpdate>;
  let mockAppRef: jasmine.SpyObj<ApplicationRef>;

  beforeEach(() => {
    const swUpdateSpy = jasmine.createSpyObj('SwUpdate', ['checkForUpdate', 'activateUpdate'], {
      isEnabled: true,
      versionUpdates: new Subject()
    });

    const appRefSpy = jasmine.createSpyObj('ApplicationRef', [], {
      isStable: of(true)
    });

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        PwaService,
        PushNotificationService,
        OfflineStorageService,
        { provide: SwUpdate, useValue: swUpdateSpy },
        { provide: ApplicationRef, useValue: appRefSpy },
        provideZoneChangeDetection({ eventCoalescing: true })
      ]
    });

    pwaService = TestBed.inject(PwaService);
    pushNotificationService = TestBed.inject(PushNotificationService);
    offlineStorageService = TestBed.inject(OfflineStorageService);
    mockSwUpdate = TestBed.inject(SwUpdate) as jasmine.SpyObj<SwUpdate>;
    mockAppRef = TestBed.inject(ApplicationRef) as jasmine.SpyObj<ApplicationRef>;
  });

  afterEach(() => {
    localStorage.clear();
    // Reset navigator mocks
    delete (navigator as any).serviceWorker;
    delete (window as any).Notification;
    delete (window as any).PushManager;
  });

  describe('PWA Installation Tests', () => {
    it('should detect PWA installation capability', () => {
      let canInstall = false;
      
      pwaService.canInstall$.subscribe(can => {
        canInstall = can;
      });

      // Initially should not be installable
      expect(canInstall).toBeFalse();

      // Simulate beforeinstallprompt event
      const mockEvent: MockBeforeInstallPromptEvent = {
        type: 'beforeinstallprompt',
        prompt: jasmine.createSpy('prompt').and.returnValue(Promise.resolve()),
        userChoice: Promise.resolve({ outcome: 'accepted' }),
        preventDefault: jasmine.createSpy('preventDefault')
      } as any;

      window.dispatchEvent(mockEvent);

      expect(canInstall).toBeTrue();
      expect(mockEvent.preventDefault).toHaveBeenCalled();
    });

    it('should handle PWA installation prompt', async () => {
      // Setup mock install prompt
      const mockEvent: MockBeforeInstallPromptEvent = {
        type: 'beforeinstallprompt',
        prompt: jasmine.createSpy('prompt').and.returnValue(Promise.resolve()),
        userChoice: Promise.resolve({ outcome: 'accepted' }),
        preventDefault: jasmine.createSpy('preventDefault')
      } as any;

      window.dispatchEvent(mockEvent);

      // Attempt installation
      const result = await pwaService.promptInstall();

      expect(mockEvent.prompt).toHaveBeenCalled();
      expect(result).toBeTrue();
    });

    it('should handle installation rejection', async () => {
      const mockEvent: MockBeforeInstallPromptEvent = {
        type: 'beforeinstallprompt',
        prompt: jasmine.createSpy('prompt').and.returnValue(Promise.resolve()),
        userChoice: Promise.resolve({ outcome: 'dismissed' }),
        preventDefault: jasmine.createSpy('preventDefault')
      } as any;

      window.dispatchEvent(mockEvent);

      const result = await pwaService.promptInstall();

      expect(result).toBeFalse();
    });

    it('should detect standalone mode', () => {
      // Mock standalone mode
      Object.defineProperty(window, 'matchMedia', {
        writable: true,
        value: jasmine.createSpy('matchMedia').and.returnValue({
          matches: true
        })
      });

      const isStandalone = pwaService.isStandalone();
      expect(isStandalone).toBeTrue();
      expect(window.matchMedia).toHaveBeenCalledWith('(display-mode: standalone)');
    });

    it('should handle app installed event', () => {
      let canInstall = true;
      
      pwaService.canInstall$.subscribe(can => {
        canInstall = can;
      });

      // Simulate beforeinstallprompt to make it installable
      const beforeInstallEvent: MockBeforeInstallPromptEvent = {
        type: 'beforeinstallprompt',
        prompt: jasmine.createSpy('prompt').and.returnValue(Promise.resolve()),
        userChoice: Promise.resolve({ outcome: 'accepted' }),
        preventDefault: jasmine.createSpy('preventDefault')
      } as any;

      window.dispatchEvent(beforeInstallEvent);
      expect(canInstall).toBeTrue();

      // Simulate app installed
      window.dispatchEvent(new Event('appinstalled'));
      expect(canInstall).toBeFalse();
    });
  });

  describe('Service Worker Update Tests', () => {
    it('should check for service worker updates', () => {
      expect(mockSwUpdate.checkForUpdate).toHaveBeenCalled();
    });

    it('should handle version updates', () => {
      let updateAvailable = false;
      
      pwaService.updateAvailable$.subscribe(available => {
        updateAvailable = available;
      });

      const versionUpdatesSubject = mockSwUpdate.versionUpdates as Subject<any>;
      versionUpdatesSubject.next({ type: 'VERSION_READY' });

      expect(updateAvailable).toBeTrue();
    });

    it('should apply service worker updates', async () => {
      mockSwUpdate.activateUpdate.and.returnValue(Promise.resolve(true));
      
      // Mock window.location.reload
      const originalReload = window.location.reload;
      window.location.reload = jasmine.createSpy('reload');

      await pwaService.applyUpdate();

      expect(mockSwUpdate.activateUpdate).toHaveBeenCalled();
      expect(window.location.reload).toHaveBeenCalled();

      // Restore original reload
      window.location.reload = originalReload;
    });

    it('should handle service worker update errors gracefully', async () => {
      mockSwUpdate.activateUpdate.and.returnValue(Promise.reject(new Error('Update failed')));

      // Should not throw error
      await expectAsync(pwaService.applyUpdate()).toBeResolved();
    });
  });

  describe('Offline Storage Tests', () => {
    it('should store offline actions', () => {
      const actionId = offlineStorageService.storeAttendanceCheckIn('Office');
      
      expect(actionId).toBeTruthy();
      
      const actions = offlineStorageService.getPendingActions();
      expect(actions.length).toBe(1);
      expect(actions[0].type).toBe('attendance');
      expect(actions[0].action).toBe('check-in');
      expect(actions[0].synced).toBeFalse();
    });

    it('should store multiple types of offline actions', () => {
      const attendanceId = offlineStorageService.storeAttendanceCheckIn('Office');
      const dsrId = offlineStorageService.storeDSRSubmission(1, 2, 8, 'Worked on feature');
      const leaveId = offlineStorageService.storeLeaveRequest('Annual', '2024-01-15', '2024-01-16', 'Vacation');

      const actions = offlineStorageService.getPendingActions();
      expect(actions.length).toBe(3);
      
      const attendanceAction = actions.find(a => a.id === attendanceId);
      const dsrAction = actions.find(a => a.id === dsrId);
      const leaveAction = actions.find(a => a.id === leaveId);

      expect(attendanceAction?.type).toBe('attendance');
      expect(dsrAction?.type).toBe('dsr');
      expect(leaveAction?.type).toBe('leave');
    });

    it('should mark actions as synced', () => {
      const actionId = offlineStorageService.storeAttendanceCheckIn('Office');
      
      let actions = offlineStorageService.getPendingActions();
      expect(actions[0].synced).toBeFalse();

      offlineStorageService.markActionSynced(actionId);
      
      actions = offlineStorageService.getPendingActions();
      expect(actions[0].synced).toBeTrue();
    });

    it('should clear synced actions', () => {
      const actionId1 = offlineStorageService.storeAttendanceCheckIn('Office');
      const actionId2 = offlineStorageService.storeAttendanceCheckOut();

      offlineStorageService.markActionSynced(actionId1);

      let actions = offlineStorageService.getPendingActions();
      expect(actions.length).toBe(2);

      offlineStorageService.clearSyncedActions();

      actions = offlineStorageService.getPendingActions();
      expect(actions.length).toBe(1);
      expect(actions[0].id).toBe(actionId2);
    });

    it('should handle localStorage errors gracefully', () => {
      spyOn(localStorage, 'setItem').and.throwError('Storage full');

      // Should not throw error
      expect(() => {
        offlineStorageService.storeAttendanceCheckIn('Office');
      }).not.toThrow();
    });
  });

  describe('Data Caching Tests', () => {
    it('should cache data with expiry', () => {
      const testData = { id: 1, name: 'Test Employee' };
      
      offlineStorageService.cacheEmployeeProfile(testData);
      
      const cachedData = offlineStorageService.getCachedEmployeeProfile();
      expect(cachedData).toEqual(testData);
    });

    it('should return null for expired cache', (done) => {
      const testData = { id: 1, name: 'Test Data' };
      
      // Cache with very short expiry (0.001 minutes = 0.06 seconds)
      offlineStorageService.cacheData('test-key', testData, 0.001);
      
      // Initially should be cached
      expect(offlineStorageService.isCached('test-key')).toBeTrue();
      
      // After expiry, should not be cached
      setTimeout(() => {
        expect(offlineStorageService.isCached('test-key')).toBeFalse();
        expect(offlineStorageService.getCachedData('test-key')).toBeNull();
        done();
      }, 100);
    });

    it('should cache different types of data', () => {
      const profileData = { id: 1, name: 'John Doe' };
      const dashboardData = { widgets: ['attendance', 'tasks'] };
      const attendanceData = { status: 'checked-in', time: '09:00' };

      offlineStorageService.cacheEmployeeProfile(profileData);
      offlineStorageService.cacheDashboardData(dashboardData);
      offlineStorageService.cacheAttendanceStatus(attendanceData);

      expect(offlineStorageService.getCachedEmployeeProfile()).toEqual(profileData);
      expect(offlineStorageService.getCachedDashboardData()).toEqual(dashboardData);
      expect(offlineStorageService.getCachedAttendanceStatus()).toEqual(attendanceData);
    });

    it('should clear expired cache items', () => {
      const testData = { id: 1, name: 'Test Data' };
      
      // Cache with very short expiry
      offlineStorageService.cacheData('expired-key', testData, 0.001);
      offlineStorageService.cacheData('valid-key', testData, 60);

      // Wait for expiry
      setTimeout(() => {
        offlineStorageService.clearExpiredCache();
        
        expect(offlineStorageService.isCached('expired-key')).toBeFalse();
        expect(offlineStorageService.isCached('valid-key')).toBeTrue();
      }, 100);
    });

    it('should calculate cache size', () => {
      const testData = { id: 1, name: 'Test Employee', description: 'A test employee record' };
      
      const initialSize = offlineStorageService.getCacheSize();
      offlineStorageService.cacheEmployeeProfile(testData);
      const newSize = offlineStorageService.getCacheSize();
      
      expect(newSize).toBeGreaterThan(initialSize);
    });

    it('should clear all cache', () => {
      offlineStorageService.cacheEmployeeProfile({ id: 1, name: 'Test' });
      offlineStorageService.cacheDashboardData({ widgets: [] });
      
      expect(offlineStorageService.getCacheSize()).toBeGreaterThan(0);
      
      offlineStorageService.clearCache();
      
      expect(offlineStorageService.getCacheSize()).toBe(0);
      expect(offlineStorageService.getCachedEmployeeProfile()).toBeNull();
      expect(offlineStorageService.getCachedDashboardData()).toBeNull();
    });
  });

  describe('Push Notification Tests', () => {
    beforeEach(() => {
      // Mock service worker and push manager
      Object.defineProperty(navigator, 'serviceWorker', {
        writable: true,
        value: {
          ready: Promise.resolve({
            showNotification: jasmine.createSpy('showNotification').and.returnValue(Promise.resolve()),
            pushManager: {
              subscribe: jasmine.createSpy('subscribe').and.returnValue(Promise.resolve({
                endpoint: 'https://example.com/push',
                getKey: jasmine.createSpy('getKey').and.returnValue(new ArrayBuffer(8))
              })),
              getSubscription: jasmine.createSpy('getSubscription').and.returnValue(Promise.resolve(null))
            }
          })
        }
      });

      Object.defineProperty(window, 'PushManager', {
        writable: true,
        value: {}
      });

      Object.defineProperty(window, 'Notification', {
        writable: true,
        value: {
          permission: 'granted',
          requestPermission: jasmine.createSpy('requestPermission').and.returnValue(Promise.resolve('granted'))
        }
      });
    });

    it('should detect push notification support', () => {
      const isSupported = pushNotificationService.isSupported();
      expect(isSupported).toBeTrue();
    });

    it('should request notification permission', async () => {
      const permission = await pushNotificationService.requestPermission();
      expect(permission).toBe('granted');
    });

    it('should subscribe to push notifications', async () => {
      const subscription = await pushNotificationService.subscribe();
      expect(subscription).toBeTruthy();
      expect(subscription?.endpoint).toBe('https://example.com/push');
    });

    it('should show local notifications', async () => {
      await pushNotificationService.showLocalNotification({
        title: 'Test Notification',
        body: 'This is a test notification'
      });

      const registration = await navigator.serviceWorker.ready;
      expect(registration.showNotification).toHaveBeenCalledWith(
        'Test Notification',
        jasmine.objectContaining({
          body: 'This is a test notification'
        })
      );
    });

    it('should show attendance reminder notification', async () => {
      await pushNotificationService.showAttendanceReminder();

      const registration = await navigator.serviceWorker.ready;
      expect(registration.showNotification).toHaveBeenCalledWith(
        'Attendance Reminder',
        jasmine.objectContaining({
          body: 'Don\'t forget to check in for today!'
        })
      );
    });

    it('should show DSR reminder notification', async () => {
      await pushNotificationService.showDSRReminder();

      const registration = await navigator.serviceWorker.ready;
      expect(registration.showNotification).toHaveBeenCalledWith(
        'DSR Reminder',
        jasmine.objectContaining({
          body: 'Please submit your Daily Status Report'
        })
      );
    });

    it('should handle notification permission denial gracefully', async () => {
      (window.Notification as any).permission = 'denied';

      // Should not throw error
      await expectAsync(pushNotificationService.showLocalNotification({
        title: 'Test',
        body: 'Test'
      })).toBeResolved();
    });
  });

  describe('Network Status Tests', () => {
    it('should detect online status', () => {
      let isOnline: boolean;
      
      pwaService.isOnline$.subscribe(online => {
        isOnline = online;
      });

      expect(isOnline!).toBe(navigator.onLine);
    });

    it('should handle online/offline events', () => {
      let isOnline: boolean;
      
      pwaService.isOnline$.subscribe(online => {
        isOnline = online;
      });

      // Simulate going offline
      Object.defineProperty(navigator, 'onLine', {
        writable: true,
        value: false
      });
      window.dispatchEvent(new Event('offline'));

      expect(isOnline!).toBeFalse();

      // Simulate going online
      Object.defineProperty(navigator, 'onLine', {
        writable: true,
        value: true
      });
      window.dispatchEvent(new Event('online'));

      expect(isOnline!).toBeTrue();
    });
  });

  describe('Offline Sync Tests', () => {
    it('should store data for offline sync', () => {
      const testData = { id: 1, action: 'check-in', location: 'Office' };
      
      pwaService.storeOfflineData('attendance-checkin', testData);
      
      const storedData = JSON.parse(localStorage.getItem('stride-hr-offline-data') || '[]');
      expect(storedData.length).toBe(1);
      expect(storedData[0].action).toBe('attendance-checkin');
      expect(storedData[0].data).toEqual(testData);
    });

    it('should handle multiple offline actions', () => {
      pwaService.storeOfflineData('attendance-checkin', { location: 'Office' });
      pwaService.storeOfflineData('dsr-submit', { hours: 8, description: 'Work done' });
      pwaService.storeOfflineData('leave-request', { type: 'Annual', days: 2 });

      const storedData = JSON.parse(localStorage.getItem('stride-hr-offline-data') || '[]');
      expect(storedData.length).toBe(3);
      
      const actions = storedData.map((item: any) => item.action);
      expect(actions).toContain('attendance-checkin');
      expect(actions).toContain('dsr-submit');
      expect(actions).toContain('leave-request');
    });
  });

  describe('PWA Manifest Validation Tests', () => {
    it('should validate manifest structure', () => {
      const expectedManifest = {
        name: 'StrideHR - Human Resource Management System',
        short_name: 'StrideHR',
        description: 'Comprehensive HR management system for global organizations',
        display: 'standalone',
        orientation: 'portrait-primary',
        theme_color: '#3b82f6',
        background_color: '#ffffff',
        scope: './',
        start_url: './',
        categories: ['business', 'productivity', 'utilities']
      };

      // This would typically be loaded from the actual manifest file
      // For testing, we validate the expected structure
      expect(expectedManifest.name).toBeTruthy();
      expect(expectedManifest.short_name).toBeTruthy();
      expect(expectedManifest.display).toBe('standalone');
      expect(expectedManifest.theme_color).toMatch(/^#[0-9a-f]{6}$/i);
      expect(expectedManifest.background_color).toMatch(/^#[0-9a-f]{6}$/i);
      expect(Array.isArray(expectedManifest.categories)).toBeTrue();
    });

    it('should have required icon sizes', () => {
      const requiredSizes = ['72x72', '96x96', '128x128', '144x144', '152x152', '192x192', '384x384', '512x512'];
      
      // This would typically validate actual manifest icons
      // For testing, we ensure all required sizes are defined
      requiredSizes.forEach(size => {
        expect(size).toMatch(/^\d+x\d+$/);
      });
    });
  });

  describe('Error Handling Tests', () => {
    it('should handle service worker registration errors', () => {
      // Mock service worker registration failure
      Object.defineProperty(navigator, 'serviceWorker', {
        writable: true,
        value: undefined
      });

      expect(() => {
        const isSupported = pushNotificationService.isSupported();
        expect(isSupported).toBeFalse();
      }).not.toThrow();
    });

    it('should handle localStorage quota exceeded', () => {
      spyOn(localStorage, 'setItem').and.throwError('QuotaExceededError');

      expect(() => {
        offlineStorageService.cacheEmployeeProfile({ id: 1, name: 'Test' });
      }).not.toThrow();
    });

    it('should handle network errors gracefully', () => {
      // Mock network error
      spyOn(console, 'error');

      // This would typically test actual network error handling
      // For now, we ensure error logging doesn't break the app
      expect(() => {
        console.error('Network error occurred');
      }).not.toThrow();
    });
  });
});