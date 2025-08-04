import { TestBed } from '@angular/core/testing';
import { ApplicationRef } from '@angular/core';
import { SwUpdate } from '@angular/service-worker';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of, Subject } from 'rxjs';

import { PwaService } from './services/pwa.service';
import { PushNotificationService } from './services/push-notification.service';
import { OfflineStorageService } from './services/offline-storage.service';

describe('PWA Functionality Tests', () => {
  let pwaService: PwaService;
  let pushNotificationService: PushNotificationService;
  let offlineStorageService: OfflineStorageService;

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
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should create all PWA services', () => {
    expect(pwaService).toBeTruthy();
    expect(pushNotificationService).toBeTruthy();
    expect(offlineStorageService).toBeTruthy();
  });

  it('should handle offline data storage', () => {
    const actionId = offlineStorageService.storeAttendanceCheckIn('Office');
    expect(actionId).toBeTruthy();

    const actions = offlineStorageService.getPendingActions();
    expect(actions.length).toBe(1);
    expect(actions[0].type).toBe('attendance');
    expect(actions[0].action).toBe('check-in');
  });

  it('should cache data correctly', () => {
    const testData = { id: 1, name: 'Test Employee' };
    offlineStorageService.cacheEmployeeProfile(testData);

    const cachedData = offlineStorageService.getCachedEmployeeProfile();
    expect(cachedData).toEqual(testData);
  });

  it('should detect online/offline status', () => {
    let isOnline: boolean;
    pwaService.isOnline$.subscribe(online => {
      isOnline = online;
    });

    expect(isOnline!).toBe(navigator.onLine);
  });

  it('should support push notifications when available', () => {
    // Mock service worker support
    Object.defineProperty(navigator, 'serviceWorker', {
      writable: true,
      value: {}
    });
    Object.defineProperty(window, 'PushManager', {
      writable: true,
      value: {}
    });
    Object.defineProperty(window, 'Notification', {
      writable: true,
      value: {}
    });

    const isSupported = pushNotificationService.isSupported();
    expect(isSupported).toBe(true);
  });

  it('should handle PWA installation prompt', () => {
    let canInstall: boolean;
    pwaService.canInstall$.subscribe(can => {
      canInstall = can;
    });

    // Initially should not be able to install
    expect(canInstall!).toBe(false);
  });
});