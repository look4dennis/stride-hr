import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { NotificationService } from '../services/notification.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate, CanActivateChild {
  constructor(
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.checkAuth(route, state);
  }

  canActivateChild(
    childRoute: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.checkAuth(childRoute, state);
  }

  private checkAuth(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    // Check if user is authenticated
    if (!this.authService.isAuthenticated) {
      console.log('AuthGuard: User not authenticated, redirecting to login');
      
      // Store the attempted URL for redirecting after login
      this.router.navigate(['/login'], { 
        queryParams: { returnUrl: state.url }
      });
      return false;
    }

    // Check if token is still valid
    if (!this.authService.isTokenValid()) {
      console.log('AuthGuard: Token expired, attempting refresh');
      
      // Try to refresh token before redirecting to login
      this.authService.refreshToken().subscribe({
        next: (response) => {
          if (response.success) {
            console.log('AuthGuard: Token refreshed successfully');
            // Token refreshed, allow navigation
            return true;
          } else {
            this.handleExpiredSession(state.url);
            return false;
          }
        },
        error: () => {
          this.handleExpiredSession(state.url);
          return false;
        }
      });
      
      // For now, block navigation while refresh is in progress
      // In a real implementation, you might want to show a loading state
      return false;
    }

    // Check if user needs to change password
    const currentUser = this.authService.currentUser;
    if (currentUser?.forcePasswordChange && !state.url.includes('/change-password')) {
      console.log('AuthGuard: User must change password');
      this.router.navigate(['/change-password'], {
        queryParams: { returnUrl: state.url }
      });
      return false;
    }

    // Authentication successful
    return true;
  }

  private handleExpiredSession(attemptedUrl: string): void {
    console.log('AuthGuard: Session expired, redirecting to login');
    this.notificationService.showError('Your session has expired. Please log in again.');
    this.authService.logout();
    this.router.navigate(['/login'], { 
      queryParams: { returnUrl: attemptedUrl }
    });
  }
}