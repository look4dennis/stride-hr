import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { HttpClient, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService, LoginRequest, AuthResponse, User } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let routerSpy: jasmine.SpyObj<Router>;

  const mockUser: User = {
    id: 1,
    employeeId: 'EMP001',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@example.com',
    branchId: 1,
    roles: ['Employee']
  };

  const mockAuthResponse: AuthResponse = {
    token: 'mock-jwt-token',
    refreshToken: 'mock-refresh-token',
    user: mockUser,
    expiresAt: new Date(Date.now() + 3600000).toISOString() // 1 hour from now
  };

  beforeEach(() => {
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
    imports: [],
    providers: [
        AuthService,
        { provide: Router, useValue: routerSpyObj },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
    ]
});

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    // Clear localStorage before each test
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('login', () => {
    it('should login user and store auth data', () => {
      const loginRequest: LoginRequest = {
        email: 'john.doe@example.com',
        password: 'password123'
      };

      service.login(loginRequest).subscribe(response => {
        expect(response).toEqual(mockAuthResponse);
        expect(service.currentUser).toEqual(mockUser);
        expect(service.token).toBe(mockAuthResponse.token);
        expect(service.isAuthenticated).toBe(true);
      });

      const req = httpMock.expectOne(`${service['API_URL']}/auth/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(loginRequest);
      req.flush(mockAuthResponse);

      // Check localStorage
      expect(localStorage.getItem('token')).toBe(mockAuthResponse.token);
      expect(localStorage.getItem('refreshToken')).toBe(mockAuthResponse.refreshToken);
      expect(localStorage.getItem('user')).toBe(JSON.stringify(mockUser));
    });
  });

  describe('logout', () => {
    beforeEach(() => {
      // Set up authenticated state
      localStorage.setItem('token', mockAuthResponse.token);
      localStorage.setItem('user', JSON.stringify(mockUser));
      service['tokenSubject'].next(mockAuthResponse.token);
      service['currentUserSubject'].next(mockUser);
    });

    it('should logout user and clear auth data', () => {
      service.logout();

      const req = httpMock.expectOne(`${service['API_URL']}/auth/logout`);
      expect(req.request.method).toBe('POST');
      req.flush({});

      expect(service.currentUser).toBeNull();
      expect(service.token).toBeNull();
      expect(service.isAuthenticated).toBe(false);
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);

      // Check localStorage is cleared
      expect(localStorage.getItem('token')).toBeNull();
      expect(localStorage.getItem('user')).toBeNull();
    });
  });

  describe('hasRole', () => {
    beforeEach(() => {
      service['currentUserSubject'].next(mockUser);
    });

    it('should return true if user has the role', () => {
      expect(service.hasRole('Employee')).toBe(true);
    });

    it('should return false if user does not have the role', () => {
      expect(service.hasRole('Admin')).toBe(false);
    });

    it('should return false if no user is logged in', () => {
      service['currentUserSubject'].next(null);
      expect(service.hasRole('Employee')).toBe(false);
    });
  });

  describe('hasAnyRole', () => {
    beforeEach(() => {
      service['currentUserSubject'].next(mockUser);
    });

    it('should return true if user has any of the roles', () => {
      expect(service.hasAnyRole(['Admin', 'Employee'])).toBe(true);
    });

    it('should return false if user has none of the roles', () => {
      expect(service.hasAnyRole(['Admin', 'Manager'])).toBe(false);
    });
  });

  describe('refreshToken', () => {
    it('should refresh token and update auth data', () => {
      localStorage.setItem('refreshToken', 'old-refresh-token');

      const newAuthResponse: AuthResponse = {
        ...mockAuthResponse,
        token: 'new-jwt-token',
        refreshToken: 'new-refresh-token'
      };

      service.refreshToken().subscribe(response => {
        expect(response).toEqual(newAuthResponse);
        expect(service.token).toBe(newAuthResponse.token);
      });

      const req = httpMock.expectOne(`${service['API_URL']}/auth/refresh`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ refreshToken: 'old-refresh-token' });
      req.flush(newAuthResponse);

      expect(localStorage.getItem('token')).toBe(newAuthResponse.token);
      expect(localStorage.getItem('refreshToken')).toBe(newAuthResponse.refreshToken);
    });
  });

  describe('loadStoredAuth', () => {
    it('should load valid stored auth data on service initialization', () => {
      const futureDate = new Date(Date.now() + 3600000).toISOString();
      localStorage.setItem('token', mockAuthResponse.token);
      localStorage.setItem('user', JSON.stringify(mockUser));
      localStorage.setItem('tokenExpiry', futureDate);

      // Create new service instance to trigger loadStoredAuth
      const httpClient = TestBed.inject(HttpClient);
      const newService = new AuthService(httpClient, routerSpy);

      expect(newService.currentUser).toEqual(mockUser);
      expect(newService.token).toBe(mockAuthResponse.token);
      expect(newService.isAuthenticated).toBe(true);
    });

    it('should clear expired auth data on service initialization', () => {
      const pastDate = new Date(Date.now() - 3600000).toISOString();
      localStorage.setItem('token', mockAuthResponse.token);
      localStorage.setItem('user', JSON.stringify(mockUser));
      localStorage.setItem('tokenExpiry', pastDate);

      // Create new service instance to trigger loadStoredAuth
      const httpClient = TestBed.inject(HttpClient);
      const newService = new AuthService(httpClient, routerSpy);

      expect(newService.currentUser).toBeNull();
      expect(newService.token).toBeNull();
      expect(newService.isAuthenticated).toBe(false);
      expect(localStorage.getItem('token')).toBeNull();
    });
  });
});