import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { LoadingService } from '../../core/services/loading.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  template: ''
})
export abstract class BaseComponent implements OnInit, OnDestroy {
  protected readonly loadingService = inject(LoadingService);
  protected readonly notificationService = inject(NotificationService);
  
  protected destroy$ = new Subject<void>();
  protected isLoading = false;
  protected error: string | null = null;
  protected componentId: string;

  constructor() {
    this.componentId = this.constructor.name;
  }

  ngOnInit(): void {
    this.initializeComponent();
    this.subscribeToLoading();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.clearComponentLoading();
  }

  // Abstract method for component initialization
  protected abstract initializeComponent(): void;

  // Loading state management
  protected showLoading(message?: string): void {
    this.isLoading = true;
    this.loadingService.setComponentLoading(this.componentId, true);
    if (message) {
      this.loadingService.setLoading(true, this.componentId, message);
    }
  }

  protected hideLoading(): void {
    this.isLoading = false;
    this.loadingService.setComponentLoading(this.componentId, false);
    this.loadingService.clearLoading(this.componentId);
  }

  protected isComponentLoading(): Observable<boolean> {
    return this.loadingService.isComponentLoading(this.componentId);
  }

  // Error handling
  protected handleError(error: any, customMessage?: string): void {
    const errorMessage = customMessage || this.extractErrorMessage(error);
    this.error = errorMessage;
    this.hideLoading();
    
    console.error(`Error in ${this.componentId}:`, error);
    this.notificationService.showError(errorMessage);
  }

  protected clearError(): void {
    this.error = null;
  }

  // Success handling
  protected showSuccess(message: string): void {
    this.notificationService.showSuccess(message);
  }

  protected showInfo(message: string): void {
    this.notificationService.showInfo(message);
  }

  protected showWarning(message: string): void {
    this.notificationService.showWarning(message);
  }

  // Utility methods
  protected executeWithLoading<T>(
    operation: () => Observable<T>,
    loadingMessage?: string,
    successMessage?: string
  ): Observable<T> {
    this.showLoading(loadingMessage);
    this.clearError();

    return new Observable<T>(observer => {
      operation().pipe(
        takeUntil(this.destroy$)
      ).subscribe({
        next: (result) => {
          this.hideLoading();
          if (successMessage) {
            this.showSuccess(successMessage);
          }
          observer.next(result);
          observer.complete();
        },
        error: (error) => {
          this.handleError(error);
          observer.error(error);
        }
      });
    });
  }

  protected retryOperation<T>(operation: () => Observable<T>, maxRetries: number = 3): Observable<T> {
    return new Observable<T>(observer => {
      let attempts = 0;
      
      const attemptOperation = () => {
        attempts++;
        operation().pipe(
          takeUntil(this.destroy$)
        ).subscribe({
          next: (result) => {
            observer.next(result);
            observer.complete();
          },
          error: (error) => {
            if (attempts < maxRetries) {
              console.log(`Retrying operation (attempt ${attempts + 1}/${maxRetries})`);
              setTimeout(() => attemptOperation(), 1000 * attempts);
            } else {
              observer.error(error);
            }
          }
        });
      };
      
      attemptOperation();
    });
  }

  // Private methods
  private subscribeToLoading(): void {
    this.isComponentLoading().pipe(
      takeUntil(this.destroy$)
    ).subscribe(loading => {
      this.isLoading = loading;
    });
  }

  private clearComponentLoading(): void {
    this.loadingService.clearComponentLoading(this.componentId);
  }

  private extractErrorMessage(error: any): string {
    if (typeof error === 'string') {
      return error;
    }
    
    if (error?.message) {
      return error.message;
    }
    
    if (error?.error?.message) {
      return error.error.message;
    }
    
    if (error?.status) {
      return `HTTP Error ${error.status}: ${error.statusText || 'Unknown error'}`;
    }
    
    return 'An unexpected error occurred';
  }
}