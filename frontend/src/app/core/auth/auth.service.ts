import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

export interface User {
  id: number;
  employeeId: number;
  username: string;
  email: string;
  fullName: string;
  profilePhoto?: string;
  branchId: number;
  organizationId: number;
  branchName?: string;
  roles: string[];
  permissions: string[];
  isFirstLogin: boolean;
  forcePasswordChange: boolean;
  isTwoFactorEnabled: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  data: {
    token: string;
    refreshToken: string;
    user: User;
    expiresAt: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private tokenSubject = new BehaviorSubject<string | null>(null);

  public currentUser$ = this.currentUserSubject.asObservable();
  public token$ = this.tokenSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.loadStoredAuth();
  }

  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }

  get token(): string | null {
    return this.tokenSubject.value;
  }

  get isAuthenticated(): boolean {
    return !!this.token && !!this.currentUser;
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/login`, credentials)
      .pipe(
        tap(response => {
          if (response.success) {
            this.setAuthData(response);
          }
        }),
        catchError(error => {
          console.error('Login error:', error);
          
          // Handle specific error cases
          if (error.status === 401) {
            return throwError(() => ({
              success: false,
              message: error.error?.message || 'Invalid email or password',
              errors: error.error?.errors || ['Authentication failed']
            }));
          } else if (error.status === 429) {
            return throwError(() => ({
              success: false,
              message: 'Too many login attempts. Please try again later.',
              errors: ['Account temporarily locked']
            }));
          } else if (error.status === 0) {
            return throwError(() => ({
              success: false,
              message: 'Unable to connect to server. Please check your connection.',
              errors: ['Network error']
            }));
          }
          
          return throwError(() => ({
            success: false,
            message: 'An unexpected error occurred. Please try again.',
            errors: ['Server error']
          }));
        })
      );
  }

  logout(): void {
    // Call logout endpoint to revoke tokens on server
    this.http.post(`${this.API_URL}/auth/logout`, {}).subscribe({
      next: () => {
        console.log('Server logout successful');
      },
      error: (error) => {
        console.warn('Server logout failed:', error);
        // Continue with client-side logout even if server call fails
      },
      complete: () => {
        this.clearAuthData();
        this.router.navigate(['/login']);
      }
    });
  }

  logoutAll(): void {
    // Call logout-all endpoint to revoke all sessions
    this.http.post(`${this.API_URL}/auth/logout-all`, {}).subscribe({
      next: () => {
        console.log('All sessions logged out successfully');
      },
      error: (error) => {
        console.warn('Logout all sessions failed:', error);
      },
      complete: () => {
        this.clearAuthData();
        this.router.navigate(['/login']);
      }
    });
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = localStorage.getItem('refreshToken');
    const currentToken = localStorage.getItem('token');
    
    if (!refreshToken) {
      return throwError(() => ({
        success: false,
        message: 'No refresh token available',
        errors: ['Authentication required']
      }));
    }

    return this.http.post<AuthResponse>(`${this.API_URL}/auth/refresh`, { 
      refreshToken,
      token: currentToken 
    })
      .pipe(
        tap(response => {
          if (response.success) {
            this.setAuthData(response);
          }
        }),
        catchError(error => {
          console.error('Token refresh error:', error);
          
          // Clear auth data on refresh failure
          this.clearAuthData();
          
          if (error.status === 401 || error.status === 403) {
            // Redirect to login on invalid refresh token
            this.router.navigate(['/login']);
            return throwError(() => ({
              success: false,
              message: 'Session expired. Please log in again.',
              errors: ['Invalid refresh token']
            }));
          }
          
          return throwError(() => ({
            success: false,
            message: 'Failed to refresh session. Please log in again.',
            errors: ['Token refresh failed']
          }));
        })
      );
  }

  hasRole(role: string): boolean {
    return this.currentUser?.roles.includes(role) ?? false;
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some(role => this.hasRole(role));
  }

  hasPermission(permission: string): boolean {
    return this.currentUser?.permissions.includes(permission) ?? false;
  }

  hasAnyPermission(permissions: string[]): boolean {
    return permissions.some(permission => this.hasPermission(permission));
  }

  getCurrentUser(): User | null {
    return this.currentUser;
  }

  getToken(): string | null {
    return this.token;
  }

  isTokenValid(): boolean {
    const expiry = localStorage.getItem('tokenExpiry');
    if (!expiry) return false;
    
    const expiryDate = new Date(expiry);
    return expiryDate > new Date();
  }

  getTokenExpiry(): Date | null {
    const expiry = localStorage.getItem('tokenExpiry');
    return expiry ? new Date(expiry) : null;
  }

  private setAuthData(authResponse: AuthResponse): void {
    localStorage.setItem('token', authResponse.data.token);
    localStorage.setItem('refreshToken', authResponse.data.refreshToken);
    localStorage.setItem('user', JSON.stringify(authResponse.data.user));
    localStorage.setItem('tokenExpiry', authResponse.data.expiresAt);

    this.tokenSubject.next(authResponse.data.token);
    this.currentUserSubject.next(authResponse.data.user);
  }

  private clearAuthData(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('tokenExpiry');

    this.tokenSubject.next(null);
    this.currentUserSubject.next(null);
  }

  getActiveSessions(): Observable<any> {
    return this.http.get(`${this.API_URL}/auth/sessions`);
  }

  revokeSession(sessionId: string): Observable<any> {
    return this.http.delete(`${this.API_URL}/auth/sessions/${sessionId}`);
  }

  changePassword(currentPassword: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.API_URL}/auth/change-password`, {
      currentPassword,
      newPassword
    });
  }

  validateToken(token?: string): Observable<any> {
    const tokenToValidate = token || this.token;
    if (!tokenToValidate) {
      return throwError(() => ({ success: false, message: 'No token to validate' }));
    }

    return this.http.post(`${this.API_URL}/auth/validate`, { token: tokenToValidate });
  }

  getCurrentUserFromServer(): Observable<any> {
    return this.http.get(`${this.API_URL}/auth/me`);
  }

  private loadStoredAuth(): void {
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');
    const expiry = localStorage.getItem('tokenExpiry');

    if (token && userStr && expiry) {
      const expiryDate = new Date(expiry);
      if (expiryDate > new Date()) {
        this.tokenSubject.next(token);
        this.currentUserSubject.next(JSON.parse(userStr));
        
        // Validate token with server on app startup
        this.validateToken(token).subscribe({
          next: (response) => {
            if (!response.success) {
              console.warn('Stored token is invalid, clearing auth data');
              this.clearAuthData();
            }
          },
          error: () => {
            console.warn('Token validation failed, clearing auth data');
            this.clearAuthData();
          }
        });
      } else {
        this.clearAuthData();
      }
    }
  }
}