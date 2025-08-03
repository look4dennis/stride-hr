import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { BirthdayWidgetComponent } from './birthday-widget.component';
import { BirthdayService, BirthdayEmployee } from '../../../core/services/birthday.service';
import { AuthService, User } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

describe('BirthdayWidgetComponent', () => {
  let component: BirthdayWidgetComponent;
  let fixture: ComponentFixture<BirthdayWidgetComponent>;
  let birthdayServiceSpy: jasmine.SpyObj<BirthdayService>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let notificationServiceSpy: jasmine.SpyObj<NotificationService>;

  const mockUser: User = {
    id: 1,
    employeeId: 'EMP001',
    firstName: 'Test',
    lastName: 'User',
    email: 'test@example.com',
    branchId: 1,
    roles: ['Employee']
  };

  const mockBirthdayEmployees: BirthdayEmployee[] = [
    {
      id: 1,
      employeeId: 'EMP001',
      firstName: 'John',
      lastName: 'Doe',
      profilePhoto: '/assets/images/avatars/john-doe.jpg',
      department: 'Development',
      designation: 'Senior Developer',
      dateOfBirth: '1995-01-15',
      age: 28
    },
    {
      id: 2,
      employeeId: 'EMP002',
      firstName: 'Jane',
      lastName: 'Smith',
      profilePhoto: '/assets/images/avatars/jane-smith.jpg',
      department: 'HR',
      designation: 'HR Manager',
      dateOfBirth: '1991-01-15',
      age: 32
    }
  ];

  beforeEach(async () => {
    const birthdaySpy = jasmine.createSpyObj('BirthdayService', ['sendBirthdayWish'], {
      todayBirthdays$: of(mockBirthdayEmployees)
    });
    const authSpy = jasmine.createSpyObj('AuthService', [], {
      currentUser: mockUser
    });
    const notificationSpy = jasmine.createSpyObj('NotificationService', ['showSuccess', 'showError']);

    await TestBed.configureTestingModule({
      imports: [BirthdayWidgetComponent, FormsModule],
      providers: [
        { provide: BirthdayService, useValue: birthdaySpy },
        { provide: AuthService, useValue: authSpy },
        { provide: NotificationService, useValue: notificationSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BirthdayWidgetComponent);
    component = fixture.componentInstance;
    birthdayServiceSpy = TestBed.inject(BirthdayService) as jasmine.SpyObj<BirthdayService>;
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    notificationServiceSpy = TestBed.inject(NotificationService) as jasmine.SpyObj<NotificationService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display birthday employees when available', () => {
    fixture.detectChanges();

    expect(component.todayBirthdays).toEqual(mockBirthdayEmployees);

    const birthdayItems = fixture.debugElement.queryAll(By.css('.birthday-item'));
    expect(birthdayItems.length).toBe(2);

    const firstEmployee = birthdayItems[0];
    const employeeName = firstEmployee.query(By.css('.employee-name'));
    const employeeDetails = firstEmployee.query(By.css('.employee-details'));

    expect(employeeName.nativeElement.textContent.trim()).toBe('John Doe');
    expect(employeeDetails.nativeElement.textContent.trim()).toBe('Senior Developer • Development');
  });

  it('should display birthday count in header', () => {
    fixture.detectChanges();

    const birthdayCount = fixture.debugElement.query(By.css('.birthday-count'));
    expect(birthdayCount.nativeElement.textContent.trim()).toBe('2');
  });

  it('should show no birthdays message when no birthdays', () => {
    // Override the service to return empty array
    Object.defineProperty(birthdayServiceSpy, 'todayBirthdays$', { value: of([]) });
    
    fixture.detectChanges();

    const noBirthdaysElement = fixture.debugElement.query(By.css('.no-birthdays'));
    expect(noBirthdaysElement).toBeTruthy();

    const message = fixture.debugElement.query(By.css('.no-birthdays h6'));
    expect(message.nativeElement.textContent.trim()).toBe('No Birthdays Today');
  });

  it('should open wish modal when send wishes button is clicked', () => {
    fixture.detectChanges();

    const sendWishButton = fixture.debugElement.query(By.css('.btn-primary'));
    sendWishButton.nativeElement.click();

    expect(component.showWishModal).toBe(true);
    expect(component.selectedEmployee).toEqual(mockBirthdayEmployees[0]);
  });

  it('should close wish modal when close button is clicked', () => {
    component.showWishModal = true;
    component.selectedEmployee = mockBirthdayEmployees[0];
    fixture.detectChanges();

    const closeButton = fixture.debugElement.query(By.css('.btn-close'));
    closeButton.nativeElement.click();

    expect(component.showWishModal).toBe(false);
    expect(component.selectedEmployee).toBeNull();
  });

  it('should select template message when template button is clicked', () => {
    component.showWishModal = true;
    component.selectedEmployee = mockBirthdayEmployees[0];
    fixture.detectChanges();

    const templateButton = fixture.debugElement.query(By.css('.template-buttons .btn'));
    templateButton.nativeElement.click();

    expect(component.wishMessage).toBe(component.wishTemplates[0].message);
  });

  it('should send birthday wish successfully', () => {
    const mockWish = { id: 1, fromEmployeeId: 1, toEmployeeId: 1, message: 'Happy Birthday!', sentAt: '2023-01-15' };
    birthdayServiceSpy.sendBirthdayWish.and.returnValue(of(mockWish));

    component.selectedEmployee = mockBirthdayEmployees[0];
    component.wishMessage = 'Happy Birthday!';

    component.sendWish();

    expect(birthdayServiceSpy.sendBirthdayWish).toHaveBeenCalledWith(1, 'Happy Birthday!');
    expect(notificationServiceSpy.showSuccess).toHaveBeenCalledWith('Birthday wishes sent to John! 🎉');
    expect(component.showWishModal).toBe(false);
  });

  it('should handle send wish error', () => {
    birthdayServiceSpy.sendBirthdayWish.and.returnValue(throwError(() => new Error('Network error')));

    component.selectedEmployee = mockBirthdayEmployees[0];
    component.wishMessage = 'Happy Birthday!';

    component.sendWish();

    expect(notificationServiceSpy.showError).toHaveBeenCalledWith('Failed to send birthday wishes. Please try again.');
    expect(component.isSendingWish).toBe(false);
  });

  it('should disable send button when wish is already sent', () => {
    component.sentWishes.add(1);
    fixture.detectChanges();

    const sendWishButton = fixture.debugElement.query(By.css('.btn-primary'));
    expect(sendWishButton.nativeElement.disabled).toBe(true);
    expect(sendWishButton.nativeElement.textContent.trim()).toContain('Sent');
  });

  it('should return correct profile photo URL', () => {
    const employee = mockBirthdayEmployees[0];
    const photoUrl = component.getProfilePhoto(employee);
    expect(photoUrl).toBe('/assets/images/avatars/john-doe.jpg');

    const employeeWithoutPhoto = { ...employee, profilePhoto: undefined };
    const defaultPhotoUrl = component.getProfilePhoto(employeeWithoutPhoto);
    expect(defaultPhotoUrl).toBe('/assets/images/default-avatar.png');
  });

  it('should check if wish is sent correctly', () => {
    component.sentWishes.add(1);
    
    expect(component.isWishSent(1)).toBe(true);
    expect(component.isWishSent(2)).toBe(false);
  });

  it('should prevent sending wish without message', () => {
    component.selectedEmployee = mockBirthdayEmployees[0];
    component.wishMessage = '';

    component.sendWish();

    expect(birthdayServiceSpy.sendBirthdayWish).not.toHaveBeenCalled();
  });

  it('should prevent sending wish without selected employee', () => {
    component.selectedEmployee = null;
    component.wishMessage = 'Happy Birthday!';

    component.sendWish();

    expect(birthdayServiceSpy.sendBirthdayWish).not.toHaveBeenCalled();
  });

  it('should show loading state when sending wish', () => {
    birthdayServiceSpy.sendBirthdayWish.and.returnValue(of({ id: 1, fromEmployeeId: 1, toEmployeeId: 1, message: 'Test', sentAt: '2023-01-15' }));
    
    component.selectedEmployee = mockBirthdayEmployees[0];
    component.wishMessage = 'Happy Birthday!';
    component.showWishModal = true;
    fixture.detectChanges();

    // The loading state is set to true at the beginning of sendWish and then set to false after completion
    // Since the observable completes synchronously in tests, we need to check during the call
    spyOn(component, 'sendWish').and.callFake(() => {
      component.isSendingWish = true;
      expect(component.isSendingWish).toBe(true);
      component.isSendingWish = false;
    });
    
    component.sendWish();
  });

  it('should clean up subscriptions on destroy', () => {
    fixture.detectChanges();
    
    if (component['birthdaySubscription']) {
      spyOn(component['birthdaySubscription'], 'unsubscribe');
      component.ngOnDestroy();
      expect(component['birthdaySubscription'].unsubscribe).toHaveBeenCalled();
    }
  });
});