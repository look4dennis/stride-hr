import { HttpInterceptorFn, HttpErrorResponse, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError, BehaviorSubject, filter, take, switchMap, Observable, finalize } from 'rxjs';
import { AuthService } from '../auth/auth.service';

let isRefreshing = false;
const refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  // Skip auth header for auth endpoints to avoid circular calls
  const isAuthEndpoint = req.url.includes('/auth/login') || 
                        req.url.includes('/auth/refresh') || 
                        req.url.includes('/auth/validate');

  // Add auth header if user is authenticated and not an auth endpoint
  let authReq = req;
  if (!isAuthEndpoint && authService.isAuthenticated && authService.token) {
    authReq = addTokenHeader(req, authService.token);
  }

  return next(authReq).pipe(
    catchError(error => {
      if (error instanceof HttpErrorResponse && error.status === 401 && !isAuthEndpoint) {
        return handle401Error(authReq, next, authService);
      }
      return throwError(() => error);
    })
  );
};

function addTokenHeader(request: HttpRequest<any>, token: string): HttpRequest<any> {
  return request.clone({
    headers: request.headers.set('Authorization', `Bearer ${token}`)
  });
}

function handle401Error(request: HttpRequest<any>, next: any, authService: AuthService): Observable<any> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    return authService.refreshToken().pipe(
      switchMap((response: any) => {
        if (response.success && response.data?.token) {
          refreshTokenSubject.next(response.data.token);
          return next(addTokenHeader(request, response.data.token));
        } else {
          throw new Error('Invalid refresh response');
        }
      }),
      catchError((error) => {
        console.error('Token refresh failed in interceptor:', error);
        authService.logout();
        return throwError(() => error);
      }),
      finalize(() => {
        isRefreshing = false;
      })
    );
  }

  // Wait for the refresh to complete
  return refreshTokenSubject.pipe(
    filter(token => token !== null),
    take(1),
    switchMap((token) => next(addTokenHeader(request, token)))
  );
}