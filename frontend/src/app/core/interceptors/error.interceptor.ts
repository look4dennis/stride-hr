import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ErrorHandlingService } from '../services/error-handling.service';
import { ConnectionService } from '../services/connection.service';
import { LoadingService } from '../services/loading.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const errorHandlingService = inject(ErrorHandlingService);
  const connectionService = inject(ConnectionService);
  const loadingService = inject(LoadingService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Create error context
      const errorContext = {
        component: 'http-interceptor',
        operation: `${req.method} ${req.url}`,
        timestamp: new Date(),
        url: req.url,
        userAgent: navigator.userAgent
      };

      // Don't show notifications for expected development errors or specific URLs
      const skipNotificationUrls = [
        '/employees/birthdays/', 
        '/weather', 
        '/manifest.webmanifest',
        '/health'
      ];
      
      const shouldSkipNotification = skipNotificationUrls.some(url => req.url.includes(url)) ||
        req.headers.has('X-Skip-Error-Notification');

      // Handle offline scenarios
      if (error.status === 0 && !navigator.onLine) {
        // Queue the request for offline processing if it's a data modification
        if (['POST', 'PUT', 'DELETE'].includes(req.method)) {
          const body = req.body ? JSON.stringify(req.body) : undefined;
          connectionService.queueOfflineAction(req.method, req.url, body);
        }
      }

      // Use the comprehensive error handling service
      if (!shouldSkipNotification) {
        errorHandlingService.handleHttpError(error, errorContext);
      }

      // Clear any loading states for this request
      const operationKey = `${req.method}-${req.url}`;
      loadingService.clearLoading(operationKey);

      // Return enhanced error object
      return throwError(() => ({
        status: error.status,
        message: error.message,
        originalError: error,
        shouldRetry: errorHandlingService['isRetryableError'](error),
        timestamp: new Date().toISOString(),
        context: errorContext
      }));
    })
  );
};