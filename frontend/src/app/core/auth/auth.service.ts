import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
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
          this.setAuthData(response);
        })
      );
  }

  logout(): void {
    // Call logout endpoint if needed
    this.http.post(`${this.API_URL}/auth/logout`, {}).subscribe();
    
    this.clearAuthData();
    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = localStorage.getItem('refreshToken');
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/refresh`, { refreshToken })
      .pipe(
        tap(response => {
          this.setAuthData(response);
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

  private loadStoredAuth(): void {
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');
    const expiry = localStorage.getItem('tokenExpiry');

    if (token && userStr && expiry) {
      const expiryDate = new Date(expiry);
      if (expiryDate > new Date()) {
        this.tokenSubject.next(token);
        this.currentUserSubject.next(JSON.parse(userStr));
      } else {
        this.clearAuthData();
      }
    }
  }
}