import { TestBed } from '@angular/core/testing';
import { ApplicationRef } from '@angular/core';
import { SwUpdate } from '@angular/service-worker';
import { of, Subject } from 'rxjs';
import { PwaService } from './pwa.service';

describe('PwaService', () => {
  let service: PwaService;
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
      providers: [
        PwaService,
        { provide: SwUpdate, useValue: swUpdateSpy },
        { provide: ApplicationRef, useValue: appRefSpy }
      ]
    });

    service = TestBed.inject(PwaService);
    mockSwUpdate = TestBed.inject(SwUpdate) as jasmine.SpyObj<SwUpdate>;
    mockAppRef = TestBed.inject(ApplicationRef) as jasmine.SpyObj<ApplicationRef>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with online status', () => {
    service.isOnline$.subscribe(isOnline => {
      expect(isOnline).toBe(navigator.onLine);
    });
  });

  it('should detect standalone mode correctly', () => {
    // Mock window.matchMedia
    Object.defineProperty(window, 'matchMedia', {
      writable: true,
      value: jasmine.createSpy('matchMedia').and.returnValue({
        matches: true
      })
    });

    const isStandalone = service.isStandalone();
    expect(isStandalone).toBe(true);
  });

  it('should request notification permission', async () => {
    // Mock Notification API
    Object.defineProperty(window, 'Notification', {
      writable: true,
      value: {
        permission: 'default',
        requestPermission: jasmine.createSpy('requestPermission').and.returnValue(Promise.resolve('granted'))
      }
    });

    const permission = await service.requestNotificationPermission();
    expect(permission).toBe('granted');
    expect(window.Notification.requestPermission).toHaveBeenCalled();
  });

  it('should show notification when permission is granted', async () => {
    // Mock Notification API
    Object.defineProperty(window, 'Notification', {
      writable: true,
      value: {
        permission: 'granted',
        requestPermission: jasmine.createSpy('requestPermission').and.returnValue(Promise.resolve('granted'))
      }
    });

    // Mock service worker registration
    Object.defineProperty(navigator, 'serviceWorker', {
      writable: true,
      value: {
        ready: Promise.resolve({
          showNotification: jasmine.createSpy('showNotification').and.returnValue(Promise.resolve())
        })
      }
    });

    await service.showNotification('Test Title', { body: 'Test Body' });
    
    const registration = await navigator.serviceWorker.ready;
    expect(registration.showNotification).toHaveBeenCalledWith('Test Title', jasmine.any(Object));
  });

  it('should handle service worker updates', () => {
    const versionUpdatesSubject = mockSwUpdate.versionUpdates as Subject<any>;
    
    service.updateAvailable$.subscribe(available => {
      if (available) {
        expect(available).toBe(true);
      }
    });

    // Simulate version ready event
    versionUpdatesSubject.next({ type: 'VERSION_READY' });
  });

  it('should apply updates correctly', async () => {
    mockSwUpdate.activateUpdate.and.returnValue(Promise.resolve(true));
    
    // Mock window.location.reload
    Object.defineProperty(window, 'location', {
      writable: true,
      value: {
        reload: jasmine.createSpy('reload')
      }
    });

    await service.applyUpdate();
    
    expect(mockSwUpdate.activateUpdate).toHaveBeenCalled();
    expect(window.location.reload).toHaveBeenCalled();
  });

  it('should store offline data correctly', () => {
    const testData = { action: 'test', data: { id: 1 } };
    
    service.storeOfflineData('test-action', testData);
    
    const storedData = JSON.parse(localStorage.getItem('stride-hr-offline-data') || '[]');
    expect(storedData.length).toBe(1);
    expect(storedData[0].action).toBe('test-action');
    expect(storedData[0].data).toEqual(testData);
  });

  it('should handle network status changes', () => {
    let isOnlineValue: boolean;
    
    service.isOnline$.subscribe(online => {
      isOnlineValue = online;
    });

    // Simulate going offline
    Object.defineProperty(navigator, 'onLine', {
      writable: true,
      value: false
    });
    
    window.dispatchEvent(new Event('offline'));
    expect(isOnlineValue!).toBe(false);

    // Simulate going online
    Object.defineProperty(navigator, 'onLine', {
      writable: true,
      value: true
    });
    
    window.dispatchEvent(new Event('online'));
    expect(isOnlineValue!).toBe(true);
  });

  afterEach(() => {
    localStorage.clear();
  });
});