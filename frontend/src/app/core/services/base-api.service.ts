import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, timer, BehaviorSubject } from 'rxjs';
import { catchError, retry, retryWhen, delayWhen, take, concat, finalize } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { LoadingService } from './loading.service';
import { NotificationService } from './notification.service';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: ValidationError[];
  pagination?: PaginationInfo;
  timestamp: string;
}

export interface ValidationError {
  field: string;
  message: string;
  code?: string;
}

export interface PaginationInfo {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface QueryParams {
  [key: string]: string | number | boolean | undefined;
}

export interface RetryConfig {
  maxRetries: number;
  delayMs: number;
  exponentialBackoff: boolean;
}

@Injectable({
  providedIn: 'root'
})
export abstract class BaseApiService<T, CreateDto = Partial<T>, UpdateDto = Partial<T>> {
  protected readonly http = inject(HttpClient);
  protected readonly loadingService = inject(LoadingService);
  protected readonly notificationService = inject(NotificationService);
  
  protected readonly baseUrl = environment.apiUrl;
  protected abstract readonly endpoint: string;
  
  // Default retry configuration
  protected readonly defaultRetryConfig: RetryConfig = {
    maxRetries: 3,
    delayMs: 1000,
    exponentialBackoff: true
  };

  // Loading state management
  private loadingStates = new Map<string, BehaviorSubject<boolean>>();

  constructor() {}

  // CRUD Operations
  getAll(params?: QueryParams): Observable<ApiResponse<T[]>> {
    const operationKey = `${this.endpoint}-getAll`;
    return this.executeWithRetry(
      () => {
        const httpParams = this.buildHttpParams(params);
        return this.http.get<ApiResponse<T[]>>(`${this.baseUrl}/${this.endpoint}`, { params: httpParams });
      },
      operationKey
    );
  }

  getById(id: number | string): Observable<ApiResponse<T>> {
    const operationKey = `${this.endpoint}-getById-${id}`;
    return this.executeWithRetry(
      () => this.http.get<ApiResponse<T>>(`${this.baseUrl}/${this.endpoint}/${id}`),
      operationKey
    );
  }

  create(entity: CreateDto): Observable<ApiResponse<T>> {
    const operationKey = `${this.endpoint}-create`;
    return this.executeWithRetry(
      () => {
        const formData = this.prepareFormData(entity);
        return this.http.post<ApiResponse<T>>(`${this.baseUrl}/${this.endpoint}`, formData);
      },
      operationKey,
      { maxRetries: 1, delayMs: 1000, exponentialBackoff: false } // Less retries for create operations
    );
  }

  update(id: number | string, entity: UpdateDto): Observable<ApiResponse<T>> {
    const operationKey = `${this.endpoint}-update-${id}`;
    return this.executeWithRetry(
      () => {
        const formData = this.prepareFormData(entity);
        return this.http.put<ApiResponse<T>>(`${this.baseUrl}/${this.endpoint}/${id}`, formData);
      },
      operationKey,
      { maxRetries: 1, delayMs: 1000, exponentialBackoff: false }
    );
  }

  delete(id: number | string): Observable<ApiResponse<boolean>> {
    const operationKey = `${this.endpoint}-delete-${id}`;
    return this.executeWithRetry(
      () => this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${this.endpoint}/${id}`),
      operationKey,
      { maxRetries: 1, delayMs: 1000, exponentialBackoff: false }
    );
  }

  // Batch operations
  createBatch(entities: CreateDto[]): Observable<ApiResponse<T[]>> {
    const operationKey = `${this.endpoint}-createBatch`;
    return this.executeWithRetry(
      () => this.http.post<ApiResponse<T[]>>(`${this.baseUrl}/${this.endpoint}/batch`, entities),
      operationKey,
      { maxRetries: 1, delayMs: 1000, exponentialBackoff: false }
    );
  }

  updateBatch(updates: { id: number | string; data: UpdateDto }[]): Observable<ApiResponse<T[]>> {
    const operationKey = `${this.endpoint}-updateBatch`;
    return this.executeWithRetry(
      () => this.http.put<ApiResponse<T[]>>(`${this.baseUrl}/${this.endpoint}/batch`, updates),
      operationKey,
      { maxRetries: 1, delayMs: 1000, exponentialBackoff: false }
    );
  }

  deleteBatch(ids: (number | string)[]): Observable<ApiResponse<boolean>> {
    const operationKey = `${this.endpoint}-deleteBatch`;
    return this.executeWithRetry(
      () => this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${this.endpoint}/batch`, { body: { ids } }),
      operationKey,
      { maxRetries: 1, delayMs: 1000, exponentialBackoff: false }
    );
  }

  // Search and filtering
  search(query: string, params?: QueryParams): Observable<ApiResponse<T[]>> {
    const operationKey = `${this.endpoint}-search`;
    const searchParams = { ...params, q: query };
    return this.executeWithRetry(
      () => {
        const httpParams = this.buildHttpParams(searchParams);
        return this.http.get<ApiResponse<T[]>>(`${this.baseUrl}/${this.endpoint}/search`, { params: httpParams });
      },
      operationKey
    );
  }

  // Loading state management
  isLoading(operationKey?: string): Observable<boolean> {
    if (operationKey) {
      if (!this.loadingStates.has(operationKey)) {
        this.loadingStates.set(operationKey, new BehaviorSubject<boolean>(false));
      }
      return this.loadingStates.get(operationKey)!.asObservable();
    }
    return this.loadingService.loading$;
  }

  // Protected helper methods
  protected executeWithRetry<R>(
    operation: () => Observable<R>,
    operationKey: string,
    retryConfig: RetryConfig = this.defaultRetryConfig
  ): Observable<R> {
    this.setLoadingState(operationKey, true);
    
    return operation().pipe(
      retryWhen(errors => 
        errors.pipe(
          take(retryConfig.maxRetries),
          delayWhen((error, index) => {
            const delay = retryConfig.exponentialBackoff 
              ? retryConfig.delayMs * Math.pow(2, index)
              : retryConfig.delayMs;
            
            console.log(`Retrying ${operationKey} in ${delay}ms (attempt ${index + 1}/${retryConfig.maxRetries})`);
            return timer(delay);
          })
        )
      ),
      catchError((error: HttpErrorResponse) => this.handleError(error, operationKey)),
      finalize(() => this.setLoadingState(operationKey, false))
    );
  }

  protected buildHttpParams(params?: QueryParams): HttpParams {
    let httpParams = new HttpParams();
    
    if (params) {
      Object.keys(params).forEach(key => {
        const value = params[key];
        if (value !== undefined && value !== null && value !== '') {
          httpParams = httpParams.set(key, value.toString());
        }
      });
    }
    
    return httpParams;
  }

  protected prepareFormData(data: any): FormData | any {
    // Check if data contains File objects
    const hasFiles = this.hasFileObjects(data);
    
    if (hasFiles) {
      const formData = new FormData();
      this.appendToFormData(formData, data);
      return formData;
    }
    
    return data;
  }

  private hasFileObjects(obj: any): boolean {
    if (obj instanceof File) return true;
    if (obj && typeof obj === 'object') {
      return Object.values(obj).some(value => this.hasFileObjects(value));
    }
    return false;
  }

  private appendToFormData(formData: FormData, data: any, parentKey?: string): void {
    Object.keys(data).forEach(key => {
      const value = data[key];
      const formKey = parentKey ? `${parentKey}.${key}` : key;
      
      if (value instanceof File) {
        formData.append(formKey, value);
      } else if (value instanceof Array) {
        value.forEach((item, index) => {
          if (item instanceof File) {
            formData.append(`${formKey}[${index}]`, item);
          } else if (typeof item === 'object') {
            this.appendToFormData(formData, item, `${formKey}[${index}]`);
          } else {
            formData.append(`${formKey}[${index}]`, item?.toString() || '');
          }
        });
      } else if (value && typeof value === 'object' && !(value instanceof Date)) {
        this.appendToFormData(formData, value, formKey);
      } else if (value !== undefined && value !== null) {
        formData.append(formKey, value.toString());
      }
    });
  }

  private setLoadingState(operationKey: string, loading: boolean): void {
    // Set global loading state
    this.loadingService.setLoading(loading, operationKey);
    
    // Set operation-specific loading state
    if (!this.loadingStates.has(operationKey)) {
      this.loadingStates.set(operationKey, new BehaviorSubject<boolean>(false));
    }
    this.loadingStates.get(operationKey)!.next(loading);
  }

  private handleError(error: HttpErrorResponse, operationKey: string): Observable<never> {
    let errorMessage = 'An unexpected error occurred';
    let shouldShowNotification = true;

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Client Error: ${error.error.message}`;
    } else {
      // Server-side error
      switch (error.status) {
        case 0:
          errorMessage = 'Unable to connect to server. Please check your internet connection.';
          break;
        case 400:
          errorMessage = error.error?.message || 'Invalid request data';
          break;
        case 401:
          errorMessage = 'Authentication required. Please log in again.';
          shouldShowNotification = false; // Let auth interceptor handle this
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
          break;
        case 429:
          errorMessage = 'Too many requests. Please try again later.';
          break;
        case 500:
          errorMessage = 'Internal server error. Please try again later.';
          break;
        case 502:
        case 503:
        case 504:
          errorMessage = 'Service temporarily unavailable. Please try again later.';
          break;
        default:
          errorMessage = error.error?.message || `Server Error: ${error.status}`;
      }
    }

    // Log error for debugging
    console.error(`API Error in ${operationKey}:`, {
      status: error.status,
      message: errorMessage,
      url: error.url,
      error: error.error
    });

    // Show user-friendly notification
    if (shouldShowNotification) {
      this.notificationService.showError(errorMessage);
    }

    return throwError(() => new Error(errorMessage));
  }

  // Utility methods for subclasses
  protected showSuccess(message: string): void {
    this.notificationService.showSuccess(message);
  }

  protected showError(message: string): void {
    this.notificationService.showError(message);
  }

  protected showInfo(message: string): void {
    this.notificationService.showInfo(message);
  }

  protected showWarning(message: string): void {
    this.notificationService.showWarning(message);
  }
}