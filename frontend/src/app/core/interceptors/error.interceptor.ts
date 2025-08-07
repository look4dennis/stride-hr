import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';
import { LoadingService } from '../services/loading.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);
  const loadingService = inject(LoadingService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred';
      let shouldShowNotification = true;
      let shouldRetry = false;

      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Network Error: ${error.error.message}`;
      } else {
        // Server-side error
        switch (error.status) {
          case 0:
            errorMessage = 'Unable to connect to server. Please check your internet connection.';
            shouldRetry = true;
            break;
          case 400:
            errorMessage = error.error?.message || 'Invalid request data';
            if (error.error?.errors && Array.isArray(error.error.errors)) {
              // Handle validation errors
              const validationErrors = error.error.errors.map((err: any) => err.message).join(', ');
              errorMessage = `Validation Error: ${validationErrors}`;
            }
            break;
          case 401:
            errorMessage = 'Authentication required. Please log in again.';
            shouldShowNotification = false; // Let auth service handle this
            break;
          case 403:
            errorMessage = 'You do not have permission to perform this action';
            break;
          case 404:
            errorMessage = 'The requested resource was not found';
            break;
          case 409:
            errorMessage = error.error?.message || 'Conflict: Resource already exists or is in use';
            break;
          case 422:
            errorMessage = 'Validation failed. Please check your input data.';
            if (error.error?.errors) {
              const validationErrors = Object.values(error.error.errors).flat().join(', ');
              errorMessage = `Validation Error: ${validationErrors}`;
            }
            break;
          case 429:
            errorMessage = 'Too many requests. Please try again later.';
            shouldRetry = true;
            break;
          case 500:
            errorMessage = 'Internal server error. Please try again later.';
            shouldRetry = true;
            break;
          case 502:
          case 503:
          case 504:
            errorMessage = 'Service temporarily unavailable. Please try again later.';
            shouldRetry = true;
            break;
          default:
            errorMessage = error.error?.message || `Server Error: ${error.status}`;
        }
      }

      // Don't show notifications for expected development errors or specific URLs
      const skipNotificationStatuses = [401]; // Only skip auth errors at interceptor level
      const skipNotificationUrls = [
        '/employees/birthdays/', 
        '/weather', 
        '/manifest.webmanifest'
      ];
      
      const shouldSkipNotification = skipNotificationStatuses.includes(error.status) || 
        skipNotificationUrls.some(url => req.url.includes(url));

      // Show user-friendly notification (unless handled by BaseApiService)
      if (shouldShowNotification && !shouldSkipNotification && !req.headers.has('X-Skip-Error-Notification')) {
        notificationService.showError(errorMessage);
      }

      // Enhanced logging with request context
      const logLevel = shouldSkipNotification ? 'log' : 'error';
      console[logLevel]('HTTP Error:', {
        url: req.url,
        method: req.method,
        status: error.status,
        statusText: error.statusText,
        message: errorMessage,
        timestamp: new Date().toISOString(),
        error: error.error
      });

      // Clear any loading states for this request
      const operationKey = `${req.method}-${req.url}`;
      loadingService.clearLoading(operationKey);

      return throwError(() => ({
        status: error.status,
        message: errorMessage,
        originalError: error,
        shouldRetry,
        timestamp: new Date().toISOString()
      }));
    })
  );
};