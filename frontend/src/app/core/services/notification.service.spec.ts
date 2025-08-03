import { TestBed } from '@angular/core/testing';
import { NotificationService, Notification } from './notification.service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NotificationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('showSuccess', () => {
    it('should add success notification', () => {
      const message = 'Operation successful';
      const title = 'Success';

      service.showSuccess(message, title);

      service.notifications$.subscribe(notifications => {
        expect(notifications.length).toBe(1);
        expect(notifications[0].type).toBe('success');
        expect(notifications[0].message).toBe(message);
        expect(notifications[0].title).toBe(title);
        expect(notifications[0].duration).toBe(5000);
      });
    });

    it('should use default title when not provided', () => {
      const message = 'Operation successful';

      service.showSuccess(message);

      service.notifications$.subscribe(notifications => {
        expect(notifications[0].title).toBe('Success');
      });
    });
  });

  describe('showError', () => {
    it('should add error notification', () => {
      const message = 'Operation failed';
      const title = 'Error';

      service.showError(message, title);

      service.notifications$.subscribe(notifications => {
        expect(notifications.length).toBe(1);
        expect(notifications[0].type).toBe('error');
        expect(notifications[0].message).toBe(message);
        expect(notifications[0].title).toBe(title);
        expect(notifications[0].duration).toBe(8000);
      });
    });
  });

  describe('showWarning', () => {
    it('should add warning notification', () => {
      const message = 'Warning message';
      const title = 'Warning';

      service.showWarning(message, title);

      service.notifications$.subscribe(notifications => {
        expect(notifications.length).toBe(1);
        expect(notifications[0].type).toBe('warning');
        expect(notifications[0].message).toBe(message);
        expect(notifications[0].title).toBe(title);
        expect(notifications[0].duration).toBe(6000);
      });
    });
  });

  describe('showInfo', () => {
    it('should add info notification', () => {
      const message = 'Info message';
      const title = 'Info';

      service.showInfo(message, title);

      service.notifications$.subscribe(notifications => {
        expect(notifications.length).toBe(1);
        expect(notifications[0].type).toBe('info');
        expect(notifications[0].message).toBe(message);
        expect(notifications[0].title).toBe(title);
        expect(notifications[0].duration).toBe(5000);
      });
    });
  });

  describe('removeNotification', () => {
    it('should remove notification by id', () => {
      service.showSuccess('Test message');
      
      let notificationId: string;
      service.notifications$.subscribe(notifications => {
        if (notifications.length > 0) {
          notificationId = notifications[0].id;
        }
      });

      service.removeNotification(notificationId!);

      service.notifications$.subscribe(notifications => {
        expect(notifications.length).toBe(0);
      });
    });
  });

  describe('clearAll', () => {
    it('should clear all notifications', () => {
      service.showSuccess('Message 1');
      service.showError('Message 2');
      service.showWarning('Message 3');

      service.clearAll();

      service.notifications$.subscribe(notifications => {
        expect(notifications.length).toBe(0);
      });
    });
  });

  describe('auto-removal', () => {
    it('should schedule auto-removal when duration is provided', () => {
      spyOn(window, 'setTimeout').and.callThrough();
      
      service.showSuccess('Test message', 'Test', 1000);
      
      expect(window.setTimeout).toHaveBeenCalled();
    });
  });

  describe('notification ordering', () => {
    it('should add new notifications to the beginning of the array', () => {
      service.showSuccess('First message');
      service.showError('Second message');

      service.notifications$.subscribe(notifications => {
        expect(notifications.length).toBe(2);
        expect(notifications[0].message).toBe('Second message');
        expect(notifications[1].message).toBe('First message');
      });
    });
  });
});