import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthGuard, RoleGuard } from './auth.guard';
import { AuthService } from './auth.service';

describe('AuthGuard', () => {
  let guard: AuthGuard;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(() => {
    const authSpy = jasmine.createSpyObj('AuthService', ['hasAnyRole'], {
      isAuthenticated: true
    });
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        AuthGuard,
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: routerSpyObj }
      ]
    });

    guard = TestBed.inject(AuthGuard);
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });

  describe('canActivate', () => {
    let route: ActivatedRouteSnapshot;
    let state: RouterStateSnapshot;

    beforeEach(() => {
      route = new ActivatedRouteSnapshot();
      state = { url: '/dashboard' } as RouterStateSnapshot;
    });

    it('should allow access when user is authenticated and no roles required', () => {
      Object.defineProperty(authServiceSpy, 'isAuthenticated', { value: true });
      route.data = {};

      const result = guard.canActivate(route, state);

      expect(result).toBe(true);
      expect(routerSpy.navigate).not.toHaveBeenCalled();
    });

    it('should allow access when user is authenticated and has required role', () => {
      Object.defineProperty(authServiceSpy, 'isAuthenticated', { value: true });
      route.data = { roles: ['Admin', 'Manager'] };
      authServiceSpy.hasAnyRole.and.returnValue(true);

      const result = guard.canActivate(route, state);

      expect(result).toBe(true);
      expect(authServiceSpy.hasAnyRole).toHaveBeenCalledWith(['Admin', 'Manager']);
      expect(routerSpy.navigate).not.toHaveBeenCalled();
    });

    it('should deny access when user is authenticated but lacks required role', () => {
      Object.defineProperty(authServiceSpy, 'isAuthenticated', { value: true });
      route.data = { roles: ['Admin'] };
      authServiceSpy.hasAnyRole.and.returnValue(false);

      const result = guard.canActivate(route, state);

      expect(result).toBe(false);
      expect(authServiceSpy.hasAnyRole).toHaveBeenCalledWith(['Admin']);
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });

    it('should deny access and redirect to login when user is not authenticated', () => {
      Object.defineProperty(authServiceSpy, 'isAuthenticated', { value: false });

      const result = guard.canActivate(route, state);

      expect(result).toBe(false);
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/dashboard' } });
    });
  });

  describe('canActivateChild', () => {
    it('should use the same logic as canActivate', () => {
      const route = new ActivatedRouteSnapshot();
      const state = { url: '/dashboard' } as RouterStateSnapshot;
      Object.defineProperty(authServiceSpy, 'isAuthenticated', { value: true });

      const result = guard.canActivateChild(route, state);

      expect(result).toBe(true);
    });
  });
});

describe('RoleGuard', () => {
  let guard: RoleGuard;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(() => {
    const authSpy = jasmine.createSpyObj('AuthService', ['hasAnyRole']);
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        RoleGuard,
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: routerSpyObj }
      ]
    });

    guard = TestBed.inject(RoleGuard);
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });

  describe('canActivate', () => {
    let route: ActivatedRouteSnapshot;
    let state: RouterStateSnapshot;

    beforeEach(() => {
      route = new ActivatedRouteSnapshot();
      state = { url: '/admin' } as RouterStateSnapshot;
    });

    it('should allow access when no roles are required', () => {
      route.data = {};

      const result = guard.canActivate(route, state);

      expect(result).toBe(true);
      expect(routerSpy.navigate).not.toHaveBeenCalled();
    });

    it('should allow access when user has required role', () => {
      route.data = { roles: ['Admin'] };
      authServiceSpy.hasAnyRole.and.returnValue(true);

      const result = guard.canActivate(route, state);

      expect(result).toBe(true);
      expect(authServiceSpy.hasAnyRole).toHaveBeenCalledWith(['Admin']);
      expect(routerSpy.navigate).not.toHaveBeenCalled();
    });

    it('should deny access when user lacks required role', () => {
      route.data = { roles: ['Admin'] };
      authServiceSpy.hasAnyRole.and.returnValue(false);

      const result = guard.canActivate(route, state);

      expect(result).toBe(false);
      expect(authServiceSpy.hasAnyRole).toHaveBeenCalledWith(['Admin']);
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });
  });
});