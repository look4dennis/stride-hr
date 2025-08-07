import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { NotificationService } from '../services/notification.service';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate, CanActivateChild {
  constructor(
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.checkRoleAccess(route, state);
  }

  canActivateChild(
    childRoute: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.checkRoleAccess(childRoute, state);
  }

  private checkRoleAccess(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    // First check if user is authenticated
    if (!this.authService.isAuthenticated) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }

    // Check for required roles if specified in route data
    const requiredRoles = route.data?.['roles'] as string[];
    const requiredPermissions = route.data?.['permissions'] as string[];

    // If no roles or permissions are required, allow access
    if ((!requiredRoles || requiredRoles.length === 0) && 
        (!requiredPermissions || requiredPermissions.length === 0)) {
      return true;
    }

    // Check roles
    if (requiredRoles && requiredRoles.length > 0) {
      if (!this.authService.hasAnyRole(requiredRoles)) {
        this.handleUnauthorizedAccess(route.routeConfig?.path || '', requiredRoles);
        return false;
      }
    }

    // Check permissions
    if (requiredPermissions && requiredPermissions.length > 0) {
      if (!this.authService.hasAnyPermission(requiredPermissions)) {
        this.handleUnauthorizedAccess(route.routeConfig?.path || '', [], requiredPermissions);
        return false;
      }
    }

    return true;
  }

  private handleUnauthorizedAccess(
    attemptedRoute: string, 
    requiredRoles: string[] = [], 
    requiredPermissions: string[] = []
  ): void {
    const currentUser = this.authService.getCurrentUser();
    const userRoles = currentUser?.roles?.join(', ') || 'No roles';
    
    let message = `Access denied to ${attemptedRoute}. `;
    if (requiredRoles.length > 0) {
      message += `Required roles: ${requiredRoles.join(', ')}. `;
    }
    if (requiredPermissions.length > 0) {
      message += `Required permissions: ${requiredPermissions.join(', ')}. `;
    }
    message += `Your roles: ${userRoles}`;

    // Only log access denied in development mode for debugging
    if (!environment.production) {
      console.warn('Role Guard - Access Denied:', {
        attemptedRoute,
        requiredRoles,
        requiredPermissions,
        userRoles: currentUser?.roles,
        userId: currentUser?.id
      });
    }

    this.notificationService.showError('You do not have permission to access this page.');
    this.router.navigate(['/unauthorized'], { 
      queryParams: { 
        route: attemptedRoute,
        reason: 'insufficient_permissions'
      }
    });
  }
}