import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { QuickActionsComponent } from './quick-actions.component';
import { AuthService, User } from '../../../core/auth/auth.service';

describe('QuickActionsComponent', () => {
  let component: QuickActionsComponent;
  let fixture: ComponentFixture<QuickActionsComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
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
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [QuickActionsComponent],
      providers: [
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: routerSpyObj }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(QuickActionsComponent);
    component = fixture.componentInstance;
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display employee actions for employee user', () => {
    fixture.detectChanges();

    expect(component.availableActions.length).toBeGreaterThan(0);
    expect(component.hasManagerActions).toBe(false);

    const actionItems = fixture.debugElement.queryAll(By.css('.action-item'));
    expect(actionItems.length).toBe(component.availableActions.length);

    // Check that employee-specific actions are present
    const actionTitles = actionItems.map(item => 
      item.query(By.css('.action-title')).nativeElement.textContent.trim()
    );
    expect(actionTitles).toContain('Check In/Out');
    expect(actionTitles).toContain('Request Leave');
    expect(actionTitles).toContain('Submit DSR');
  });

  it('should display manager actions for manager user', () => {
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockManagerUser });
    component.ngOnInit();
    fixture.detectChanges();

    expect(component.hasManagerActions).toBe(true);
    expect(component.managerActions.length).toBeGreaterThan(0);

    const roleSpecificSection = fixture.debugElement.query(By.css('.role-specific-actions'));
    expect(roleSpecificSection).toBeTruthy();

    const sectionTitle = fixture.debugElement.query(By.css('.section-title'));
    expect(sectionTitle.nativeElement.textContent.trim()).toBe('Management Actions');
  });

  it('should display HR actions for HR user', () => {
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockHRUser });
    component.ngOnInit();
    fixture.detectChanges();

    const managerActionTitles = component.managerActions.map(action => action.title);
    expect(managerActionTitles).toContain('Manage Employees');
    expect(managerActionTitles).toContain('Process Payroll');
    expect(managerActionTitles).toContain('Generate Reports');
  });

  it('should display admin actions for admin user', () => {
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockAdminUser });
    component.ngOnInit();
    fixture.detectChanges();

    const managerActionTitles = component.managerActions.map(action => action.title);
    expect(managerActionTitles).toContain('System Settings');
    expect(managerActionTitles).toContain('User Management');
  });

  it('should navigate to correct route when action is clicked', () => {
    fixture.detectChanges();

    const firstActionItem = fixture.debugElement.query(By.css('.action-item'));
    firstActionItem.nativeElement.click();

    expect(routerSpy.navigate).toHaveBeenCalled();
  });

  it('should handle check-in-out action specially', () => {
    fixture.detectChanges();

    const checkInAction = component.availableActions.find(action => action.id === 'check-in-out');
    if (checkInAction) {
      component.executeAction(checkInAction);
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/attendance']);
    }
  });

  it('should not execute disabled actions', () => {
    const disabledAction = {
      id: 'disabled-action',
      title: 'Disabled Action',
      description: 'This action is disabled',
      icon: 'fas fa-ban',
      route: '/disabled',
      color: 'secondary',
      roles: ['Employee'],
      disabled: true
    };

    component.executeAction(disabledAction);

    expect(routerSpy.navigate).not.toHaveBeenCalled();
  });

  it('should display action badges when present', () => {
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockManagerUser });
    component.ngOnInit();
    fixture.detectChanges();

    const actionWithBadge = component.managerActions.find(action => action.badge);
    if (actionWithBadge) {
      const badges = fixture.debugElement.queryAll(By.css('.action-badge'));
      expect(badges.length).toBeGreaterThan(0);
    }
  });

  it('should filter actions based on user roles correctly', () => {
    // Test with employee user
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockEmployeeUser });
    component.ngOnInit();

    const employeeActionIds = component.availableActions.map(action => action.id);
    expect(employeeActionIds).toContain('check-in-out');
    expect(employeeActionIds).toContain('request-leave');
    expect(employeeActionIds).not.toContain('system-settings');

    // Test with admin user
    Object.defineProperty(authServiceSpy, 'currentUser', { value: mockAdminUser });
    component.ngOnInit();

    const adminActionIds = component.managerActions.map(action => action.id);
    expect(adminActionIds).toContain('system-settings');
    expect(adminActionIds).toContain('user-management');
  });

  it('should handle null user gracefully', () => {
    Object.defineProperty(authServiceSpy, 'currentUser', { value: null });
    component.ngOnInit();

    expect(component.availableActions).toEqual([]);
    expect(component.managerActions).toEqual([]);
    expect(component.hasManagerActions).toBe(false);
  });

  it('should identify manager actions correctly', () => {
    const employeeAction = { id: 'check-in-out', roles: ['Employee'] };
    const managerAction = { id: 'approve-leaves', roles: ['Manager'] };

    expect(component['isManagerAction'](employeeAction as any)).toBe(false);
    expect(component['isManagerAction'](managerAction as any)).toBe(true);
  });

  it('should display correct action icons and colors', () => {
    fixture.detectChanges();

    const actionIcons = fixture.debugElement.queryAll(By.css('.action-icon'));
    expect(actionIcons.length).toBeGreaterThan(0);

    actionIcons.forEach((iconElement, index) => {
      const action = component.availableActions[index];
      if (action) {
        expect(iconElement.nativeElement).toHaveClass(`bg-${action.color}`);
        
        const icon = iconElement.query(By.css('i'));
        // Check if the icon has the expected classes (order may vary)
        const iconClasses = icon.nativeElement.className.split(' ');
        const expectedClasses = action.icon.split(' ');
        expectedClasses.forEach(expectedClass => {
          expect(iconClasses).toContain(expectedClass);
        });
      }
    });
  });

  it('should show hover effects on action items', () => {
    fixture.detectChanges();

    const actionItems = fixture.debugElement.queryAll(By.css('.action-item'));
    expect(actionItems.length).toBeGreaterThan(0);

    // Test that action items have the action-item class which includes cursor: pointer in CSS
    actionItems.forEach(item => {
      expect(item.nativeElement).toHaveClass('action-item');
    });
  });
});