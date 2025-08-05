import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

export interface User {
  id: number;
  employeeId: string;
  firstName: string;
  lastName: string;
  email: string;
  branchId: number;
  roles: string[];
  profilePhoto?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
  expiresAt: string;
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
    // Temporarily use the test endpoint that works
    return this.http.post<any>(`${this.API_URL}/TestAuth/simple-login`, credentials)
      .pipe(
        tap(response => {
          // Extract the auth data from the API response
          const authResponse: AuthResponse = {
            token: response.data.token,
            refreshToken: response.data.refreshToken,
            user: response.data.user,
            expiresAt: response.data.expiresAt
          };
          this.setAuthData(authResponse);
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

  private setAuthData(authResponse: AuthResponse): void {
    localStorage.setItem('token', authResponse.token);
    localStorage.setItem('refreshToken', authResponse.refreshToken);
    localStorage.setItem('user', JSON.stringify(authResponse.user));
    localStorage.setItem('tokenExpiry', authResponse.expiresAt);

    this.tokenSubject.next(authResponse.token);
    this.currentUserSubject.next(authResponse.user);
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