import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PushNotificationService } from './push-notification.service';
import { environment } from '../../environments/environment';

describe('PushNotificationService', () => {
  let service: PushNotificationService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [PushNotificationService]
    });
    
    service = TestBed.inject(PushNotificationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should check if push notifications are supported', () => {
    // Mock service worker and push manager support
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

    const isSupported = service.isSupported();
    expect(isSupported).toBe(true);
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

    const permission = await service.requestPermission();
    expect(permission).toBe('granted');
    expect(window.Notification.requestPermission).toHaveBeenCalled();
  });

  it('should return granted if permission already granted', async () => {
    Object.defineProperty(window, 'Notification', {
      writable: true,
      value: {
        permission: 'granted'
      }
    });

    const permission = await service.requestPermission();
    expect(permission).toBe('granted');
  });

  it('should return denied if permission denied', async () => {
    Object.defineProperty(window, 'Notification', {
      writable: true,
      value: {
        permission: 'denied'
      }
    });

    const permission = await service.requestPermission();
    expect(permission).toBe('denied');
  });

  it('should show local notification', async () => {
    // Mock Notification API
    Object.defineProperty(window, 'Notification', {
      writable: true,
      value: {
        permission: 'granted',
        requestPermission: jasmine.createSpy('requestPermission').and.returnValue(Promise.resolve('granted'))
      }
    });

    // Mock service worker
    Object.defineProperty(navigator, 'serviceWorker', {
      writable: true,
      value: {
        ready: Promise.resolve({
          showNotification: jasmine.createSpy('showNotification').and.returnValue(Promise.resolve())
        })
      }
    });

    const payload = {
      title: 'Test Notification',
      body: 'This is a test notification'
    };

    await service.showLocalNotification(payload);

    const registration = await navigator.serviceWorker.ready;
    expect(registration.showNotification).toHaveBeenCalledWith(
      'Test Notification',
      jasmine.objectContaining({
        body: 'This is a test notification'
      })
    );
  });

  it('should subscribe to push notifications', async () => {
    const mockSubscription = {
      endpoint: 'https://example.com/push',
      expirationTime: null,
      options: {},
      getKey: jasmine.createSpy('getKey').and.returnValue(new ArrayBuffer(8)),
      toJSON: jasmine.createSpy('toJSON').and.returnValue({}),
      unsubscribe: jasmine.createSpy('unsubscribe').and.returnValue(Promise.resolve(true))
    } as unknown as PushSubscription;

    // Mock service worker and push manager
    Object.defineProperty(navigator, 'serviceWorker', {
      writable: true,
      value: {
        ready: Promise.resolve({
          pushManager: {
            getSubscription: jasmine.createSpy('getSubscription').and.returnValue(Promise.resolve(null)),
            subscribe: jasmine.createSpy('subscribe').and.returnValue(Promise.resolve(mockSubscription))
          }
        })
      }
    });

    Object.defineProperty(window, 'Notification', {
      writable: true,
      value: {
        permission: 'granted'
      }
    });

    const subscription = await service.subscribe();

    expect(subscription).toBe(mockSubscription);

    // Verify API call to server
    const req = httpMock.expectOne(`${environment.apiUrl}/notifications/subscribe`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should unsubscribe from push notifications', async () => {
    const mockUnsubscribeSubscription = {
      endpoint: 'https://example.com/push',
      expirationTime: null,
      options: {},
      getKey: jasmine.createSpy('getKey').and.returnValue(new ArrayBuffer(8)),
      toJSON: jasmine.createSpy('toJSON').and.returnValue({}),
      unsubscribe: jasmine.createSpy('unsubscribe').and.returnValue(Promise.resolve(true))
    } as unknown as PushSubscription;

    Object.defineProperty(navigator, 'serviceWorker', {
      writable: true,
      value: {
        ready: Promise.resolve({
          pushManager: {
            getSubscription: jasmine.createSpy('getSubscription').and.returnValue(Promise.resolve(mockUnsubscribeSubscription))
          }
        })
      }
    });

    const result = await service.unsubscribe();

    expect(result).toBe(true);
    expect(mockUnsubscribeSubscription.unsubscribe).toHaveBeenCalled();

    // Verify API call to server
    const req = httpMock.expectOne(`${environment.apiUrl}/notifications/unsubscribe`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should get notification preferences', () => {
    service.getNotificationPreferences().subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/notifications/preferences`);
    expect(req.request.method).toBe('GET');
    req.flush({ emailNotifications: true, pushNotifications: true });
  });

  it('should update notification preferences', () => {
    const preferences = { emailNotifications: false, pushNotifications: true };

    service.updateNotificationPreferences(preferences).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/notifications/preferences`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(preferences);
    req.flush(preferences);
  });

  it('should show attendance reminder notification', async () => {
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

    await service.showAttendanceReminder();

    const registration = await navigator.serviceWorker.ready;
    expect(registration.showNotification).toHaveBeenCalledWith(
      'Attendance Reminder',
      jasmine.objectContaining({
        body: 'Don\'t forget to check in for today!',
        tag: 'attendance-reminder'
      })
    );
  });

  it('should show DSR reminder notification', async () => {
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

    await service.showDSRReminder();

    const registration = await navigator.serviceWorker.ready;
    expect(registration.showNotification).toHaveBeenCalledWith(
      'DSR Reminder',
      jasmine.objectContaining({
        body: 'Please submit your Daily Status Report',
        tag: 'dsr-reminder'
      })
    );
  });

  it('should show leave approval notification', async () => {
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

    await service.showLeaveApprovalNotification('John Doe', 'Annual');

    const registration = await navigator.serviceWorker.ready;
    expect(registration.showNotification).toHaveBeenCalledWith(
      'Leave Request Pending',
      jasmine.objectContaining({
        body: 'John Doe has requested Annual leave',
        tag: 'leave-approval'
      })
    );
  });

  it('should show birthday notification', async () => {
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

    await service.showBirthdayNotification('Jane Smith');

    const registration = await navigator.serviceWorker.ready;
    expect(registration.showNotification).toHaveBeenCalledWith(
      '🎉 Birthday Today!',
      jasmine.objectContaining({
        body: 'It\'s Jane Smith\'s birthday today. Don\'t forget to wish them!',
        tag: 'birthday-notification'
      })
    );
  });
});