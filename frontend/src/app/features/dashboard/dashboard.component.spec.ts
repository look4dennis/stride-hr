import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DashboardComponent } from './dashboard.component';
import { AuthService, User } from '../../core/auth/auth.service';
import { WeatherService } from '../../core/services/weather.service';
import { BirthdayService } from '../../core/services/birthday.service';
import { NotificationService } from '../../core/services/notification.service';
import { Router } from '@angular/router';
import { of } from 'rxjs';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let weatherServiceSpy: jasmine.SpyObj<WeatherService>;
  let birthdayServiceSpy: jasmine.SpyObj<BirthdayService>;
  let notificationServiceSpy: jasmine.SpyObj<NotificationService>;
  let routerSpy: jasmine.SpyObj<Router>;

  const mockEmployeeUser: User = {
    id: 1,
    employeeId: 'EMP001',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@example.com',
    branchId: 1,
    roles: ['Employee']
  };

  const mockManagerUser: User = {
    id: 2,
    employeeId: 'MGR001',
    firstName: 'Jane',
    lastName: 'Smith',
    email: 'jane.smith@example.com',
    branchId: 1,
    roles: ['Manager', 'Employee']
  };

  const mockHRUser: User = {
    id: 3,
    employeeId: 'HR001',
    firstName: 'Alice',
    lastName: 'Johnson',
    email: 'alice.johnson@example.com',
    branchId: 1,
    roles: ['HR', 'Manager', 'Employee']
  };

  const mockAdminUser: User = {
    id: 4,
    employeeId: 'ADM001',
    firstName: 'Bob',
    lastName: 'Wilson',
    email: 'bob.wilson@example.com',
    branchId: 1,
    roles: ['Admin', 'HR', 'Manager', 'Employee']
  };

  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', [], {
      currentUser: mockEmployeeUser
    });
    const weatherSpy = jasmine.createSpyObj('WeatherService', ['refreshWeather'], {
      weather$: of(null)
    });
    const birthdaySpy = jasmine.createSpyObj('BirthdayService', ['sendBirthdayWish'], {
      todayBirthdays$: of([])
    });
    const notificationSpy = jasmine.createSpyObj('NotificationService', ['showSuccess', 'showError']);
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        { provide: AuthService, useValue: authSpy },
        { provide: WeatherService, useValue: weatherSpy },
        { provide: BirthdayService, useValue: birthdaySpy },
        { provide: NotificationService, useValue: notificationSpy },
        { provide: Router, useValue: routerSpyObj }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    weatherServiceSpy = TestBed.inject(WeatherService) as jasmine.SpyObj<WeatherService>;
    birthdayServiceSpy = TestBed.inject(BirthdayService) as jasmine.SpyObj<BirthdayService>;
    notificationServiceSpy = TestBed.inject(NotificationService) as jasmine.SpyObj<NotificationService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display welcome message with user name', () => {
    fixture.detectChanges();

    const welcomeTitle = fixture.debugElement.query(By.css('.welcome-title'));
    expect(welcomeTitle.nativeElement.textContent.trim()).toBe('Welcome back, John!');
  });

  it('should display role-based dashboard widgets for employee', () => {
    fixture.detectChanges();

    const widgets = fixture.debugElement.queryAll(By.css('.dashboard-widget'));
    expect(widgets.length).toBe(4);

    // Check employee-specific widget labels
    const widgetLabels = widgets.map(widget => 
      widget.query(By.css('.widget-label')).nativeElement.textContent.trim()
    );

    expect(widgetLabels).toContain('Hours Today');
    expect(widgetLabels).toContain('Active Tasks');
    expect(widgetLabels).toContain('Leave Balance');
    expect(widgetLabels).toContain('Productivity');
  });

  it('should initialize currentUser from AuthService', () => {
    component.ngOnInit();
    expect(component.currentUser).toEqual(mockEmployeeUser);
  });

  it('should display manager dashboard for manager user', () => {
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockManagerUser });
    component.ngOnInit();
    fixture.detectChanges();

    expect(component.getPrimaryRole()).toBe('Manager');

    const widgets = fixture.debugElement.queryAll(By.css('.dashboard-widget'));
    const widgetLabels = widgets.map(widget => 
      widget.query(By.css('.widget-label')).nativeElement.textContent.trim()
    );

    expect(widgetLabels).toContain('Team Members');
    expect(widgetLabels).toContain('Present Today');
    expect(widgetLabels).toContain('Active Projects');
    expect(widgetLabels).toContain('Pending Approvals');
  });

  it('should display HR dashboard for HR user', () => {
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockHRUser });
    component.ngOnInit();
    fixture.detectChanges();

    expect(component.getPrimaryRole()).toBe('HR');

    const widgets = fixture.debugElement.queryAll(By.css('.dashboard-widget'));
    const widgetLabels = widgets.map(widget => 
      widget.query(By.css('.widget-label')).nativeElement.textContent.trim()
    );

    expect(widgetLabels).toContain('Total Employees');
    expect(widgetLabels).toContain('Present Today');
    expect(widgetLabels).toContain('Pending Leaves');
    expect(widgetLabels).toContain('Payroll Status');
  });

  it('should display admin dashboard for admin user', () => {
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockAdminUser });
    component.ngOnInit();
    fixture.detectChanges();

    expect(component.getPrimaryRole()).toBe('Admin');

    const widgets = fixture.debugElement.queryAll(By.css('.dashboard-widget'));
    const widgetLabels = widgets.map(widget => 
      widget.query(By.css('.widget-label')).nativeElement.textContent.trim()
    );

    expect(widgetLabels).toContain('Total Branches');
    expect(widgetLabels).toContain('Total Employees');
    expect(widgetLabels).toContain('System Health');
    expect(widgetLabels).toContain('Active Users');
  });

  it('should return correct role-based welcome message', () => {
    // Test Employee message
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockEmployeeUser });
    component.ngOnInit();
    expect(component.getRoleBasedWelcomeMessage()).toContain('Stay productive and manage your work-life balance');

    // Test Manager message
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockManagerUser });
    component.ngOnInit();
    expect(component.getRoleBasedWelcomeMessage()).toContain('Lead your team to success');

    // Test HR message
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockHRUser });
    component.ngOnInit();
    expect(component.getRoleBasedWelcomeMessage()).toContain('Manage employee lifecycle');

    // Test Admin message
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockAdminUser });
    component.ngOnInit();
    expect(component.getRoleBasedWelcomeMessage()).toContain('Monitor and manage your organization');
  });

  it('should display correct user role with additional roles count', () => {
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockManagerUser });
    component.ngOnInit();
    
    const roleDisplay = component.getUserRoleDisplay();
    expect(roleDisplay).toBe('Manager (+1 more)');
  });

  it('should determine primary role correctly based on hierarchy', () => {
    // Admin should be primary even with other roles
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockAdminUser });
    component.ngOnInit();
    expect(component.getPrimaryRole()).toBe('Admin');

    // HR should be primary over Manager and Employee
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockHRUser });
    component.ngOnInit();
    expect(component.getPrimaryRole()).toBe('HR');

    // Manager should be primary over Employee
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockManagerUser });
    component.ngOnInit();
    expect(component.getPrimaryRole()).toBe('Manager');

    // Employee should be default
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockEmployeeUser });
    component.ngOnInit();
    expect(component.getPrimaryRole()).toBe('Employee');
  });

  it('should include weather-time widget', () => {
    fixture.detectChanges();

    const weatherWidget = fixture.debugElement.query(By.css('app-weather-time-widget'));
    expect(weatherWidget).toBeTruthy();
  });

  it('should include birthday widget', () => {
    fixture.detectChanges();

    const birthdayWidget = fixture.debugElement.query(By.css('app-birthday-widget'));
    expect(birthdayWidget).toBeTruthy();
  });

  it('should include quick actions component', () => {
    fixture.detectChanges();

    const quickActions = fixture.debugElement.query(By.css('app-quick-actions'));
    expect(quickActions).toBeTruthy();
  });

  it('should display recent activities', () => {
    fixture.detectChanges();

    const activitiesCard = fixture.debugElement.query(By.css('.card'));
    expect(activitiesCard).toBeTruthy();

    const cardTitle = activitiesCard.query(By.css('.card-title'));
    expect(cardTitle.nativeElement.textContent.trim()).toBe('Recent Activities');

    const activityItems = fixture.debugElement.queryAll(By.css('.activity-item'));
    expect(activityItems.length).toBe(component.recentActivities.length);
  });

  it('should handle null user gracefully', () => {
    Object.defineProperty(authServiceSpy, 'currentUser', { value: null });
    component.ngOnInit();
    fixture.detectChanges();

    expect(component.getPrimaryRole()).toBe('Employee');
    expect(component.getUserRoleDisplay()).toBe('Employee');
  });

  it('should handle user with no roles', () => {
    const userWithNoRoles = { ...mockEmployeeUser, roles: [] };
    Object.defineProperty(authServiceSpy, 'currentUser', { value: userWithNoRoles });
    component.ngOnInit();

    expect(component.getPrimaryRole()).toBe('Employee');
  });
});