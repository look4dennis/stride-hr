import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of, throwError } from 'rxjs';
import { DashboardComponent } from './dashboard.component';
import { AuthService } from '../../core/auth/auth.service';
import { WeatherService } from '../../core/services/weather.service';
import { BirthdayService } from '../../core/services/birthday.service';
import { NotificationService } from '../../core/services/notification.service';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockWeatherService: jasmine.SpyObj<WeatherService>;
  let mockBirthdayService: jasmine.SpyObj<BirthdayService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;

  const mockUser = {
    id: 1,
    employeeId: 'EMP001',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@example.com',
    branchId: 1,
    roles: ['Employee']
  };

  const mockWeatherData = {
    location: 'New York, US',
    temperature: 22,
    description: 'Clear sky',
    icon: '01d',
    humidity: 65,
    windSpeed: 3.5,
    feelsLike: 24
  };

  const mockBirthdayData = [
    {
      id: 1,
      employeeId: 'EMP002',
      firstName: 'Jane',
      lastName: 'Smith',
      profilePhoto: 'jane.jpg',
      department: 'HR',
      designation: 'HR Manager',
      dateOfBirth: '1990-01-15',
      age: 34
    }
  ];

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['getCurrentUser'], {
      currentUser: mockUser
    });
    const weatherServiceSpy = jasmine.createSpyObj('WeatherService', ['getCurrentWeather', 'refreshWeather'], {
      weather$: of(mockWeatherData)
    });
    const birthdayServiceSpy = jasmine.createSpyObj('BirthdayService', ['getTodayBirthdays', 'sendBirthdayWish', 'getCurrentBirthdays', 'refreshBirthdays'], {
      todayBirthdays$: of(mockBirthdayData)
    });
    const notificationServiceSpy = jasmine.createSpyObj('NotificationService', ['showSuccess', 'showError', 'showWarning', 'showInfo']);

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: WeatherService, useValue: weatherServiceSpy },
        { provide: BirthdayService, useValue: birthdayServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    
    mockAuthService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    mockWeatherService = TestBed.inject(WeatherService) as jasmine.SpyObj<WeatherService>;
    mockBirthdayService = TestBed.inject(BirthdayService) as jasmine.SpyObj<BirthdayService>;
    mockNotificationService = TestBed.inject(NotificationService) as jasmine.SpyObj<NotificationService>;

    // Setup default mock returns
    mockWeatherService.getCurrentWeather.and.returnValue(mockWeatherData);
    mockBirthdayService.getCurrentBirthdays.and.returnValue(mockBirthdayData);
    mockBirthdayService.getTodayBirthdays.and.returnValue(of(mockBirthdayData));
    mockBirthdayService.sendBirthdayWish.and.returnValue(of({ id: 1, fromEmployeeId: 1, toEmployeeId: 1, message: 'Happy Birthday!', sentAt: new Date().toISOString() }));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with current user', () => {
    component.ngOnInit();
    expect(component.currentUser).toEqual(mockUser);
  });

  it('should get primary role correctly', () => {
    component.currentUser = { ...mockUser, roles: ['Employee'] };
    expect(component.getPrimaryRole()).toBe('Employee');

    component.currentUser = { ...mockUser, roles: ['Manager', 'Employee'] };
    expect(component.getPrimaryRole()).toBe('Manager');

    component.currentUser = { ...mockUser, roles: ['Admin', 'HR', 'Manager'] };
    expect(component.getPrimaryRole()).toBe('Admin');
  });

  it('should get user role display correctly', () => {
    component.currentUser = { ...mockUser, roles: ['Employee'] };
    expect(component.getUserRoleDisplay()).toBe('Employee');

    component.currentUser = { ...mockUser, roles: ['Manager', 'Employee'] };
    expect(component.getUserRoleDisplay()).toBe('Manager (+1 more)');
  });

  it('should get role-based welcome message', () => {
    component.currentUser = { ...mockUser, roles: ['Employee'] };
    const message = component.getRoleBasedWelcomeMessage();
    expect(message).toContain('Stay productive');

    component.currentUser = { ...mockUser, roles: ['Manager'] };
    const managerMessage = component.getRoleBasedWelcomeMessage();
    expect(managerMessage).toContain('Lead your team');
  });

  it('should display welcome section with user name', () => {
    component.currentUser = mockUser;
    fixture.detectChanges();

    const welcomeTitle = fixture.debugElement.query(By.css('.welcome-title'));
    expect(welcomeTitle.nativeElement.textContent).toContain('Welcome back, John!');
  });

  it('should display role-based dashboard content for Employee', () => {
    component.currentUser = { ...mockUser, roles: ['Employee'] };
    fixture.detectChanges();

    const employeeSection = fixture.debugElement.query(By.css('ng-container[ngSwitchCase="Employee"]'));
    expect(employeeSection).toBeTruthy();
  });

  it('should display role-based dashboard content for Manager', () => {
    component.currentUser = { ...mockUser, roles: ['Manager'] };
    fixture.detectChanges();

    const managerSection = fixture.debugElement.query(By.css('ng-container[ngSwitchCase="Manager"]'));
    expect(managerSection).toBeTruthy();
  });

  it('should display role-based dashboard content for HR', () => {
    component.currentUser = { ...mockUser, roles: ['HR'] };
    fixture.detectChanges();

    const hrSection = fixture.debugElement.query(By.css('ng-container[ngSwitchCase="HR"]'));
    expect(hrSection).toBeTruthy();
  });

  it('should display role-based dashboard content for Admin', () => {
    component.currentUser = { ...mockUser, roles: ['Admin'] };
    fixture.detectChanges();

    const adminSection = fixture.debugElement.query(By.css('ng-container[ngSwitchCase="Admin"]'));
    expect(adminSection).toBeTruthy();
  });

  it('should display default dashboard content for unknown roles', () => {
    component.currentUser = { ...mockUser, roles: ['UnknownRole'] };
    fixture.detectChanges();

    const defaultSection = fixture.debugElement.query(By.css('ng-container[ngSwitchDefault]'));
    expect(defaultSection).toBeTruthy();
  });

  it('should display weather widget', () => {
    fixture.detectChanges();

    const weatherWidget = fixture.debugElement.query(By.css('app-weather-time-widget'));
    expect(weatherWidget).toBeTruthy();
  });

  it('should display birthday widget', () => {
    fixture.detectChanges();

    const birthdayWidget = fixture.debugElement.query(By.css('app-birthday-widget'));
    expect(birthdayWidget).toBeTruthy();
  });

  it('should display quick actions component', () => {
    fixture.detectChanges();

    const quickActions = fixture.debugElement.query(By.css('app-quick-actions'));
    expect(quickActions).toBeTruthy();
  });

  it('should display recent activities section', () => {
    fixture.detectChanges();

    const activitiesSection = fixture.debugElement.query(By.css('.card-title'));
    expect(activitiesSection.nativeElement.textContent).toContain('Recent Activities');
  });

  it('should display activity items', () => {
    fixture.detectChanges();

    const activityItems = fixture.debugElement.queryAll(By.css('.activity-item'));
    expect(activityItems.length).toBe(component.recentActivities.length);
  });

  it('should handle null user gracefully', () => {
    component.currentUser = null;
    fixture.detectChanges();

    expect(() => component.getPrimaryRole()).not.toThrow();
    expect(component.getPrimaryRole()).toBe('Employee');
  });

  it('should handle empty roles array', () => {
    component.currentUser = { ...mockUser, roles: [] };
    expect(component.getPrimaryRole()).toBe('Employee');
  });

  it('should display user branch information when available', () => {
    component.currentUser = { ...mockUser, branchId: 5 };
    fixture.detectChanges();

    const branchInfo = fixture.debugElement.query(By.css('.user-branch'));
    expect(branchInfo.nativeElement.textContent).toContain('Branch 5');
  });

  it('should not display branch information when not available', () => {
    component.currentUser = { ...mockUser, branchId: undefined as any };
    fixture.detectChanges();

    const branchInfo = fixture.debugElement.query(By.css('.user-branch'));
    expect(branchInfo.nativeElement.textContent.trim()).toBe('');
  });

  it('should display user avatar when profile photo is available', () => {
    component.currentUser = { ...mockUser, profilePhoto: 'avatar.jpg' };
    fixture.detectChanges();

    const avatar = fixture.debugElement.query(By.css('.avatar-img'));
    expect(avatar).toBeTruthy();
    expect(avatar.nativeElement.src).toContain('avatar.jpg');
  });

  it('should not display avatar when profile photo is not available', () => {
    component.currentUser = { ...mockUser, profilePhoto: undefined };
    fixture.detectChanges();

    const avatar = fixture.debugElement.query(By.css('.avatar-img'));
    expect(avatar).toBeFalsy();
  });
});