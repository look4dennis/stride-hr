import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DashboardComponent } from './dashboard.component';
import { AuthService, User } from '../../core/auth/auth.service';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  const mockUser: User = {
    id: 1,
    employeeId: 'EMP001',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@example.com',
    branchId: 1,
    roles: ['Employee']
  };

  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', [], {
      currentUser: mockUser
    });

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        { provide: AuthService, useValue: authSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display welcome message with user name', () => {
    fixture.detectChanges();

    const welcomeTitle = fixture.debugElement.query(By.css('.welcome-card .card-title'));
    expect(welcomeTitle.nativeElement.textContent.trim()).toBe('Welcome back, John!');
  });

  it('should display dashboard widgets', () => {
    fixture.detectChanges();

    const widgets = fixture.debugElement.queryAll(By.css('.dashboard-widget'));
    expect(widgets.length).toBe(4);

    // Check widget content
    const widgetValues = widgets.map(widget => 
      widget.query(By.css('.widget-value')).nativeElement.textContent.trim()
    );
    const widgetLabels = widgets.map(widget => 
      widget.query(By.css('.widget-label')).nativeElement.textContent.trim()
    );

    expect(widgetValues).toEqual(['150', '142', '23', '8']);
    expect(widgetLabels).toEqual(['Total Employees', 'Present Today', 'Active Projects', 'Pending Leaves']);
  });

  it('should display recent activities', () => {
    fixture.detectChanges();

    const cardTitles = fixture.debugElement.queryAll(By.css('.card-title'));
    const activitiesCard = cardTitles.find(title => title.nativeElement.textContent.includes('Recent Activities'));
    expect(activitiesCard).toBeTruthy();

    const activityItems = fixture.debugElement.queryAll(By.css('.activity-item'));
    expect(activityItems.length).toBe(3);
  });

  it('should display quick actions', () => {
    fixture.detectChanges();

    const cardTitles = fixture.debugElement.queryAll(By.css('.card-title'));
    const quickActionsCard = cardTitles.find(title => title.nativeElement.textContent.includes('Quick Actions'));
    expect(quickActionsCard).toBeTruthy();

    const actionButtons = fixture.debugElement.queryAll(By.css('.btn'));
    expect(actionButtons.length).toBeGreaterThan(0);

    const buttonTexts = actionButtons.map(btn => btn.nativeElement.textContent.trim());
    expect(buttonTexts).toContain('Check In/Out');
    expect(buttonTexts).toContain('Request Leave');
    expect(buttonTexts).toContain('Submit DSR');
    expect(buttonTexts).toContain('View Reports');
  });

  it('should initialize currentUser from AuthService', () => {
    component.ngOnInit();
    expect(component.currentUser).toEqual(mockUser);
  });

  it('should handle null user gracefully', () => {
    Object.defineProperty(authServiceSpy, 'currentUser', { value: null });
    component.ngOnInit();
    fixture.detectChanges();

    const welcomeTitle = fixture.debugElement.query(By.css('.welcome-card .card-title'));
    expect(welcomeTitle.nativeElement.textContent).toContain('Welcome back, !');
  });

  it('should have proper widget icons', () => {
    fixture.detectChanges();

    const widgetIcons = fixture.debugElement.queryAll(By.css('.widget-icon i'));
    expect(widgetIcons.length).toBe(4);

    const iconClasses = widgetIcons.map(icon => icon.nativeElement.className);
    expect(iconClasses).toContain('fas fa-users');
    expect(iconClasses).toContain('fas fa-clock');
    expect(iconClasses).toContain('fas fa-project-diagram');
    expect(iconClasses).toContain('fas fa-calendar-alt');
  });

  it('should have proper widget background colors', () => {
    fixture.detectChanges();

    const widgetIcons = fixture.debugElement.queryAll(By.css('.widget-icon'));
    expect(widgetIcons[0].nativeElement).toHaveClass('bg-primary');
    expect(widgetIcons[1].nativeElement).toHaveClass('bg-success');
    expect(widgetIcons[2].nativeElement).toHaveClass('bg-warning');
    expect(widgetIcons[3].nativeElement).toHaveClass('bg-info');
  });
});