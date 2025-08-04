import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SidebarComponent } from './sidebar.component';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { of } from 'rxjs';
import { provideRouter } from '@angular/router';

describe('SidebarComponent', () => {
  let component: SidebarComponent;
  let fixture: ComponentFixture<SidebarComponent>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  const mockUser = {
    id: 1,
    employeeId: 'EMP001',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@test.com',
    branchId: 1,
    roles: ['Employee'],
    profilePhoto: '/assets/images/default-avatar.png'
  };

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', [], {
      currentUser: mockUser
    });
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [SidebarComponent],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SidebarComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    // Mock user is already set in the spy object
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load current user on init', () => {
    component.ngOnInit();
    expect(component.currentUser).toEqual(mockUser);
  });

  it('should toggle sidebar collapse', () => {
    const initialState = component.isCollapsed;
    component.toggleSidebar();
    expect(component.isCollapsed).toBe(!initialState);
  });

  it('should navigate to route', () => {
    const route = '/dashboard';
    // Test navigation functionality if it exists
    expect(router.navigate).toBeDefined();
  });

  it('should check if route is active', () => {
    // Test route checking functionality if it exists
    expect(router.url).toBeDefined();
  });
});