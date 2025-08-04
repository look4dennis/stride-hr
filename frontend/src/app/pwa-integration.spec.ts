import { TestBed } from '@angular/core/testing';
import { ApplicationRef } from '@angular/core';
import { SwUpdate } from '@angular/service-worker';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of, Subject } from 'rxjs';

import { PwaService } from './services/pwa.service';
import { PushNotificationService } from './services/push-notification.service';
import { OfflineStorageService } from './services/offline-storage.service';

describe('PWA Integration Tests', () => {
  let pwaService: PwaService;
  let pushNotificationService: PushNotificationService;
  let offlineStorageService: OfflineStorageService;
  let mockSwUpdate: jasmine.SpyObj<SwUpdate>;

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
        { provide: ApplicationRef, useValue: appRefSpy }
      ]
    });

    pwaService = TestBed.inject(PwaService);
    pushNotificationService = TestBed.inject(PushNotificationService);
    offlineStorageService = TestBed.inject(OfflineStorageService);
    mockSwUpdate = TestBed.inject(SwUpdate) as jasmine.SpyObj<SwUpdate>;
  });

  afterEach(() => {
    localStorage.clear();
  });

  describe('Offline Functionality Integration', () => {
    it('should store offline actions and sync when online', async () => {
      // Simulate going offline
      Object.defineProperty(navigator, 'onLine', {
        writable: true,
        value: false
      });
      window.dispatchEvent(new Event('offline'));

      // Store some offline actions
      const actionId1 = offlineStorageService.storeAttendanceCheckIn('Office');
      const actionId2 = offlineStorageService.storeDSRSubmission(1, 2, 8, 'Worked on feature');

      // Verify actions are stored
      const pendingActions = offlineStorageService.getPendingActions();
      expect(pendingActions.length).toBe(2);
      expect(pendingActions.every(a => !a.synced)).toBe(true);

      // Simulate going online
      Object.defineProperty(navigator, 'onLine', {
        writable: true,
        value: true
      });
      window.dispatchEvent(new Event('online'));

      // Verify online status is detected
      pwaService.isOnline$.subscribe(isOnline => {
        expect(isOnline).toBe(true);
      });
    });

    it('should cache data for offline access', () => {
      const testData = { id: 1, name: 'Test Employee' };
      
      // Cache employee profile
      offlineStorageService.cacheEmployeeProfile(testData);
      
      // Verify data is cached
      const cachedData = offlineStorageService.getCachedEmployeeProfile();
      expect(cachedData).toEqual(testData);
      
      // Verify cache status
      expect(offlineStorageService.isCached('employee-profile')).toBe(true);
    });

    it('should handle cache expiry correctly', (done) => {
      const testData = { id: 1, name: 'Test Data' };
      
      // Cache with very short expiry
      offlineStorageService.cacheData('test-key', testData, 0.001); // 0.001 minutes = 0.06 seconds
      
      // Initially should be cached
      expect(offlineStorageService.isCached('test-key')).toBe(true);
      
      // After expiry, should not be cached
      setTimeout(() => {
        expect(offlineStorageService.isCached('test-key')).toBe(false);
        done();
      }, 100);
    });
  });

  describe('Push Notification Integration', () => {
    it('should integrate with PWA service for notifications', async () => {
      // Mock notification support
      Object.defineProperty(window, 'Notification', {
        writable: true,
        value: {
          permission: 'granted'
        }
      });

      Object.defineProperty(navigator, 'serviceWorker', {
        writable: true,
        value: {
          ready: Promise.resolve({
            showNotification: jasmine.createSpy('showNotification').and.returnValue(Promise.resolve())
          })
        }
      });

      // Test PWA service notification
      await pwaService.showNotification('Test Title', { body: 'Test Body' });

      // Test push notification service
      await pushNotificationService.showLocalNotification({
        title: 'Push Test',
        body: 'Push notification test'
      });

      const registration = await navigator.serviceWorker.ready;
      expect(registration.showNotification).toHaveBeenCalledTimes(2);
    });

    it('should handle notification permission flow', async () => {
      Object.defineProperty(window, 'Notification', {
        writable: true,
        value: {
          permission: 'default',
          requestPermission: jasmine.createSpy('requestPermission').and.returnValue(Promise.resolve('granted'))
        }
      });

      // Request permission through PWA service
      const pwaPermission = await pwaService.requestNotificationPermission();
      expect(pwaPermission).toBe('granted');

      // Request permission through push notification service
      const pushPermission = await pushNotificationService.requestPermission();
      expect(pushPermission).toBe('granted');

      expect(window.Notification.requestPermission).toHaveBeenCalledTimes(2);
    });
  });

  describe('Service Worker Integration', () => {
    it('should handle service worker updates', () => {
      const versionUpdatesSubject = mockSwUpdate.versionUpdates as Subject<any>;
      let updateAvailable = false;

      pwaService.updateAvailable$.subscribe(available => {
        updateAvailable = available;
      });

      // Simulate version ready event
      versionUpdatesSubject.next({ type: 'VERSION_READY' });

      expect(updateAvailable).toBe(true);
    });

    it('should check for updates periodically', () => {
      // The service should check for updates when the app becomes stable
      expect(mockSwUpdate.checkForUpdate).toHaveBeenCalled();
    });
  });

  describe('PWA Installation Integration', () => {
    it('should handle install prompt flow', () => {
      let canInstall = false;

      pwaService.canInstall$.subscribe(can => {
        canInstall = can;
      });

      // Simulate beforeinstallprompt event
      const mockEvent = {
        preventDefault: jasmine.createSpy('preventDefault'),
        prompt: jasmine.createSpy('prompt').and.returnValue(Promise.resolve()),
        userChoice: Promise.resolve({ outcome: 'accepted' })
      };

      window.dispatchEvent(new CustomEvent('beforeinstallprompt', { detail: mockEvent }));

      expect(canInstall).toBe(true);
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
      expect(isStandalone).toBe(true);
    });
  });

  describe('Cross-Service Data Flow', () => {
    it('should coordinate between offline storage and PWA service', () => {
      // Store offline action
      const actionId = offlineStorageService.storeAttendanceCheckIn('Office');
      
      // Verify action is stored
      const actions = offlineStorageService.getPendingActions();
      expect(actions.length).toBe(1);
      
      // Store in PWA service as well (for sync)
      pwaService.storeOfflineData('attendance-checkin', actions[0].data);
      
      // Verify both services have the data
      const pwaOfflineData = JSON.parse(localStorage.getItem('stride-hr-offline-data') || '[]');
      expect(pwaOfflineData.length).toBe(1);
    });

    it('should handle notification preferences across services', async () => {
      // Mock notification support
      Object.defineProperty(window, 'Notification', {
        writable: true,
        value: {
          permission: 'granted'
        }
      });

      Object.defineProperty(navigator, 'serviceWorker', {
        writable: true,
        value: {
          ready: Promise.resolve({
            showNotification: jasmine.createSpy('showNotification').and.returnValue(Promise.resolve())
          })
        }
      });

      // Show notifications through both services
      await pwaService.showNotification('PWA Notification', { body: 'From PWA Service' });
      await pushNotificationService.showAttendanceReminder();

      const registration = await navigator.serviceWorker.ready;
      expect(registration.showNotification).toHaveBeenCalledTimes(2);
    });
  });

  describe('Error Handling Integration', () => {
    it('should handle service worker errors gracefully', async () => {
      mockSwUpdate.activateUpdate.and.returnValue(Promise.reject(new Error('Update failed')));

      // Should not throw error
      await expectAsync(pwaService.applyUpdate()).toBeResolved();
    });

    it('should handle notification errors gracefully', async () => {
      Object.defineProperty(window, 'Notification', {
        writable: true,
        value: {
          permission: 'denied'
        }
      });

      // Should not throw error when permission is denied
      await expectAsync(pwaService.showNotification('Test', { body: 'Test' })).toBeResolved();
      await expectAsync(pushNotificationService.showLocalNotification({
        title: 'Test',
        body: 'Test'
      })).toBeResolved();
    });

    it('should handle offline storage errors gracefully', () => {
      // Mock localStorage to throw error
      spyOn(localStorage, 'setItem').and.throwError('Storage full');

      // Should not throw error
      expect(() => {
        offlineStorageService.storeAction('attendance', 'check-in', {});
      }).not.toThrow();
    });
  });
});