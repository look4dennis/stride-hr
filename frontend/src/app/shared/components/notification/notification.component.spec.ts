import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { BehaviorSubject } from 'rxjs';
import { NotificationComponent } from './notification.component';
import { NotificationService, Notification } from '../../../core/services/notification.service';

describe('NotificationComponent', () => {
  let component: NotificationComponent;
  let fixture: ComponentFixture<NotificationComponent>;
  let notificationService: jasmine.SpyObj<NotificationService>;
  let notificationsSubject: BehaviorSubject<Notification[]>;

  const mockNotifications: Notification[] = [
    {
      id: '1',
      type: 'success',
      title: 'Success',
      message: 'Operation completed successfully',
      duration: 5000,
      timestamp: new Date()
    },
    {
      id: '2',
      type: 'error',
      title: 'Error',
      message: 'Operation failed',
      duration: 8000,
      timestamp: new Date()
    }
  ];

  beforeEach(async () => {
    notificationsSubject = new BehaviorSubject<Notification[]>([]);
    const notificationServiceSpy = jasmine.createSpyObj('NotificationService', ['removeNotification'], {
      notifications$: notificationsSubject.asObservable()
    });

    await TestBed.configureTestingModule({
      imports: [NotificationComponent],
      providers: [
        { provide: NotificationService, useValue: notificationServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NotificationComponent);
    component = fixture.componentInstance;
    notificationService = TestBed.inject(NotificationService) as jasmine.SpyObj<NotificationService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display notifications', () => {
    notificationsSubject.next(mockNotifications);
    fixture.detectChanges();

    const alertElements = fixture.debugElement.queryAll(By.css('.alert'));
    expect(alertElements.length).toBe(2);

    const firstAlert = alertElements[0];
    expect(firstAlert.nativeElement.textContent).toContain('Success');
    expect(firstAlert.nativeElement.textContent).toContain('Operation completed successfully');

    const secondAlert = alertElements[1];
    expect(secondAlert.nativeElement.textContent).toContain('Error');
    expect(secondAlert.nativeElement.textContent).toContain('Operation failed');
  });

  it('should apply correct CSS classes based on notification type', () => {
    notificationsSubject.next(mockNotifications);
    fixture.detectChanges();

    const alertElements = fixture.debugElement.queryAll(By.css('.alert'));
    
    expect(alertElements[0].nativeElement).toHaveClass('alert-success');
    expect(alertElements[1].nativeElement).toHaveClass('alert-danger');
  });

  it('should display correct icons based on notification type', () => {
    notificationsSubject.next(mockNotifications);
    fixture.detectChanges();

    const iconElements = fixture.debugElement.queryAll(By.css('i'));
    
    expect(iconElements[0].nativeElement).toHaveClass('fas');
    expect(iconElements[0].nativeElement).toHaveClass('fa-check-circle');
    expect(iconElements[0].nativeElement).toHaveClass('text-success');

    expect(iconElements[1].nativeElement).toHaveClass('fas');
    expect(iconElements[1].nativeElement).toHaveClass('fa-exclamation-circle');
    expect(iconElements[1].nativeElement).toHaveClass('text-danger');
  });

  it('should call removeNotification when close button is clicked', () => {
    notificationsSubject.next([mockNotifications[0]]);
    fixture.detectChanges();

    const closeButton = fixture.debugElement.query(By.css('.btn-close'));
    closeButton.nativeElement.click();

    expect(notificationService.removeNotification).toHaveBeenCalledWith('1');
  });

  it('should format timestamp correctly', () => {
    const testDate = new Date('2023-01-01T12:30:45');
    const formattedTime = component.formatTime(testDate);
    
    expect(formattedTime).toBe(testDate.toLocaleTimeString());
  });

  it('should return correct alert class for each notification type', () => {
    expect(component.getAlertClass('success')).toBe('alert-success');
    expect(component.getAlertClass('error')).toBe('alert-danger');
    expect(component.getAlertClass('warning')).toBe('alert-warning');
    expect(component.getAlertClass('info')).toBe('alert-info');
    expect(component.getAlertClass('unknown' as any)).toBe('alert-info');
  });

  it('should return correct icon class for each notification type', () => {
    expect(component.getIconClass('success')).toBe('fas fa-check-circle text-success');
    expect(component.getIconClass('error')).toBe('fas fa-exclamation-circle text-danger');
    expect(component.getIconClass('warning')).toBe('fas fa-exclamation-triangle text-warning');
    expect(component.getIconClass('info')).toBe('fas fa-info-circle text-info');
    expect(component.getIconClass('unknown' as any)).toBe('fas fa-info-circle');
  });

  it('should handle empty notifications array', () => {
    notificationsSubject.next([]);
    fixture.detectChanges();

    const alertElements = fixture.debugElement.queryAll(By.css('.alert'));
    expect(alertElements.length).toBe(0);
  });

  it('should unsubscribe on destroy', () => {
    spyOn(component['subscription'], 'unsubscribe');
    
    component.ngOnDestroy();
    
    expect(component['subscription'].unsubscribe).toHaveBeenCalled();
  });
});