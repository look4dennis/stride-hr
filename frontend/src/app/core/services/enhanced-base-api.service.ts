import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, of, BehaviorSubject } from 'rxjs';
import { map, catchError, tap, finalize, shareReplay, retry } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { CacheService } from './cache.service';
import { LoadingService } from './loading.service';
import { NotificationService } from './notification.service';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: any[];
  pagination?: PaginationInfo;
  timestamp: string;
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
  [key: string]: any;
}

export interface CacheOptions {
  enabled: boolean;
  ttl?: number; // Time to live in milliseconds
  key?: string; // Custom cache key
}

export interface RequestOptions {
  cache?: CacheOptions;
  loading?: boolean | string; // true for global loading, string for component-specific
  showErrors?: boolean;
  retry?: number;
  timeout?: number;
}

@Injectable({
  providedIn: 'root'
})
export abstract class EnhancedBaseApiService<T> {
  protected abstract readonly endpoint: string;
  protected readonly apiUrl = environment.apiUrl;
  
  // Request tracking
  private activeRequests = new Map<string, Observable<any>>();
  private requestCountSubject = new BehaviorSubject<number>(0);
  public requestCount$ = this.requestCountSubject.asObservable();

  constructor(
    protected http: HttpClient,
    protected cacheService: CacheService,
    protected loadingService: LoadingService,
    protected notificationService: NotificationService
  ) {}

  /**
   * Get all items with optional query parameters
   */
  getAll(params?: QueryParams, options?: RequestOptions): Observable<ApiResponse<T[]>> {
    const cacheKey = this.generateCacheKey('getAll', params);
    const requestKey = `getAll_${cacheKey}`;

    // Check for existing request
    if (this.activeRequests.has(requestKey)) {
      return this.activeRequests.get(requestKey)!;
    }

    // Check cache first if enabled
    if (options?.cache?.enabled) {
      const cached = this.cacheService.getCachedApiResponse<ApiResponse<T[]>>(this.endpoint, params);
      if (cached) {
        return of(cached);
      }
    }

    const httpParams = this.buildHttpParams(params);
    const request$ = this.http.get<ApiResponse<T[]>>(`${this.apiUrl}/${this.endpoint}`, { params: httpParams })
      .pipe(
        retry(options?.retry || 1),
        map(response => this.processResponse(response)),
        tap(response => {
          // Cache successful responses
          if (options?.cache?.enabled && response.success) {
            this.cacheService.cacheApiResponse(
              this.endpoint, 
              params, 
              response, 
              options.cache.ttl
            );
          }
        }),
        catchError(error => this.handleError(error, options)),
        finalize(() => {
          this.activeRequests.delete(requestKey);
          this.updateRequestCount();
          if (options?.loading) {
            this.hideLoading(options.loading);
          }
        }),
        shareReplay(1)
      );

    // Show loading if requested
    if (options?.loading) {
      this.showLoading(options.loading);
    }

    // Track active request
    this.activeRequests.set(requestKey, request$);
    this.updateRequestCount();

    return request$;
  }

  /**
   * Get item by ID
   */
  getById(id: number | string, options?: RequestOptions): Observable<ApiResponse<T>> {
    const cacheKey = this.generateCacheKey('getById', { id });
    const requestKey = `getById_${id}`;

    // Check for existing request
    if (this.activeRequests.has(requestKey)) {
      return this.activeRequests.get(requestKey)!;
    }

    // Check cache first if enabled
    if (options?.cache?.enabled) {
      const cached = this.cacheService.getCachedApiResponse<ApiResponse<T>>(this.endpoint, { id });
      if (cached) {
        return of(cached);
      }
    }

    const request$ = this.http.get<ApiResponse<T>>(`${this.apiUrl}/${this.endpoint}/${id}`)
      .pipe(
        retry(options?.retry || 1),
        map(response => this.processResponse(response)),
        tap(response => {
          // Cache successful responses
          if (options?.cache?.enabled && response.success) {
            this.cacheService.cacheApiResponse(
              this.endpoint, 
              { id }, 
              response, 
              options.cache.ttl
            );
          }
        }),
        catchError(error => this.handleError(error, options)),
        finalize(() => {
          this.activeRequests.delete(requestKey);
          this.updateRequestCount();
          if (options?.loading) {
            this.hideLoading(options.loading);
          }
        }),
        shareReplay(1)
      );

    // Show loading if requested
    if (options?.loading) {
      this.showLoading(options.loading);
    }

    // Track active request
    this.activeRequests.set(requestKey, request$);
    this.updateRequestCount();

    return request$;
  }

  /**
   * Create new item
   */
  create(item: Partial<T>, options?: RequestOptions): Observable<ApiResponse<T>> {
    const requestKey = `create_${Date.now()}`;

    const request$ = this.http.post<ApiResponse<T>>(`${this.apiUrl}/${this.endpoint}`, item)
      .pipe(
        retry(options?.retry || 0), // Usually don't retry create operations
        map(response => this.processResponse(response)),
        tap(response => {
          // Invalidate related cache entries
          if (response.success) {
            this.cacheService.invalidateApiCache(this.endpoint);
          }
        }),
        catchError(error => this.handleError(error, options)),
        finalize(() => {
          this.activeRequests.delete(requestKey);
          this.updateRequestCount();
          if (options?.loading) {
            this.hideLoading(options.loading);
          }
        })
      );

    // Show loading if requested
    if (options?.loading) {
      this.showLoading(options.loading);
    }

    // Track active request
    this.activeRequests.set(requestKey, request$);
    this.updateRequestCount();

    return request$;
  }

  /**
   * Update existing item
   */
  update(id: number | string, item: Partial<T>, options?: RequestOptions): Observable<ApiResponse<T>> {
    const requestKey = `update_${id}`;

    const request$ = this.http.put<ApiResponse<T>>(`${this.apiUrl}/${this.endpoint}/${id}`, item)
      .pipe(
        retry(options?.retry || 0), // Usually don't retry update operations
        map(response => this.processResponse(response)),
        tap(response => {
          // Invalidate related cache entries
          if (response.success) {
            this.cacheService.invalidateApiCache(this.endpoint);
          }
        }),
        catchError(error => this.handleError(error, options)),
        finalize(() => {
          this.activeRequests.delete(requestKey);
          this.updateRequestCount();
          if (options?.loading) {
            this.hideLoading(options.loading);
          }
        })
      );

    // Show loading if requested
    if (options?.loading) {
      this.showLoading(options.loading);
    }

    // Track active request
    this.activeRequests.set(requestKey, request$);
    this.updateRequestCount();

    return request$;
  }

  /**
   * Delete item
   */
  delete(id: number | string, options?: RequestOptions): Observable<ApiResponse<boolean>> {
    const requestKey = `delete_${id}`;

    const request$ = this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${this.endpoint}/${id}`)
      .pipe(
        retry(options?.retry || 0), // Usually don't retry delete operations
        map(response => this.processResponse(response)),
        tap(response => {
          // Invalidate related cache entries
          if (response.success) {
            this.cacheService.invalidateApiCache(this.endpoint);
          }
        }),
        catchError(error => this.handleError(error, options)),
        finalize(() => {
          this.activeRequests.delete(requestKey);
          this.updateRequestCount();
          if (options?.loading) {
            this.hideLoading(options.loading);
          }
        })
      );

    // Show loading if requested
    if (options?.loading) {
      this.showLoading(options.loading);
    }

    // Track active request
    this.activeRequests.set(requestKey, request$);
    this.updateRequestCount();

    return request$;
  }

  /**
   * Search items with query
   */
  search(query: string, params?: QueryParams, options?: RequestOptions): Observable<ApiResponse<T[]>> {
    const searchParams = { ...params, q: query };
    const cacheKey = this.generateCacheKey('search', searchParams);
    const requestKey = `search_${cacheKey}`;

    // Check for existing request
    if (this.activeRequests.has(requestKey)) {
      return this.activeRequests.get(requestKey)!;
    }

    // Check cache first if enabled
    if (options?.cache?.enabled) {
      const cached = this.cacheService.getCachedApiResponse<ApiResponse<T[]>>(this.endpoint, searchParams);
      if (cached) {
        return of(cached);
      }
    }

    const httpParams = this.buildHttpParams(searchParams);
    const request$ = this.http.get<ApiResponse<T[]>>(`${this.apiUrl}/${this.endpoint}/search`, { params: httpParams })
      .pipe(
        retry(options?.retry || 1),
        map(response => this.processResponse(response)),
        tap(response => {
          // Cache successful responses
          if (options?.cache?.enabled && response.success) {
            this.cacheService.cacheApiResponse(
              this.endpoint, 
              searchParams, 
              response, 
              options.cache.ttl
            );
          }
        }),
        catchError(error => this.handleError(error, options)),
        finalize(() => {
          this.activeRequests.delete(requestKey);
          this.updateRequestCount();
          if (options?.loading) {
            this.hideLoading(options.loading);
          }
        }),
        shareReplay(1)
      );

    // Show loading if requested
    if (options?.loading) {
      this.showLoading(options.loading);
    }

    // Track active request
    this.activeRequests.set(requestKey, request$);
    this.updateRequestCount();

    return request$;
  }

  /**
   * Get paginated results
   */
  getPaginated(page: number = 1, pageSize: number = 10, params?: QueryParams, options?: RequestOptions): Observable<ApiResponse<T[]>> {
    const paginatedParams = { ...params, page, pageSize };
    return this.getAll(paginatedParams, options);
  }

  /**
   * Bulk operations
   */
  bulkCreate(items: Partial<T>[], options?: RequestOptions): Observable<ApiResponse<T[]>> {
    const requestKey = `bulkCreate_${Date.now()}`;

    const request$ = this.http.post<ApiResponse<T[]>>(`${this.apiUrl}/${this.endpoint}/bulk`, items)
      .pipe(
        map(response => this.processResponse(response)),
        tap(response => {
          // Invalidate related cache entries
          if (response.success) {
            this.cacheService.invalidateApiCache(this.endpoint);
          }
        }),
        catchError(error => this.handleError(error, options)),
        finalize(() => {
          this.activeRequests.delete(requestKey);
          this.updateRequestCount();
          if (options?.loading) {
            this.hideLoading(options.loading);
          }
        })
      );

    // Show loading if requested
    if (options?.loading) {
      this.showLoading(options.loading);
    }

    // Track active request
    this.activeRequests.set(requestKey, request$);
    this.updateRequestCount();

    return request$;
  }

  /**
   * Cancel all active requests
   */
  cancelAllRequests(): void {
    this.activeRequests.clear();
    this.updateRequestCount();
  }

  /**
   * Get active request count
   */
  getActiveRequestCount(): number {
    return this.activeRequests.size;
  }

  /**
   * Check if service has active requests
   */
  hasActiveRequests(): boolean {
    return this.activeRequests.size > 0;
  }

  /**
   * Process API response
   */
  protected processResponse<R>(response: ApiResponse<R>): ApiResponse<R> {
    // Add any global response processing here
    return response;
  }

  /**
   * Handle HTTP errors
   */
  protected handleError(error: HttpErrorResponse, options?: RequestOptions): Observable<never> {
    let errorMessage = 'An unexpected error occurred';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      switch (error.status) {
        case 400:
          errorMessage = 'Bad request. Please check your input.';
          break;
        case 401:
          errorMessage = 'Unauthorized. Please log in again.';
          break;
        case 403:
          errorMessage = 'Forbidden. You do not have permission to perform this action.';
          break;
        case 404:
          errorMessage = 'Resource not found.';
          break;
        case 500:
          errorMessage = 'Internal server error. Please try again later.';
          break;
        default:
          errorMessage = `Error ${error.status}: ${error.message}`;
      }
    }

    // Show error notification if requested
    if (options?.showErrors !== false) {
      this.notificationService.showError(errorMessage);
    }

    console.error('API Error:', error);
    return throwError(() => error);
  }

  /**
   * Build HTTP parameters
   */
  protected buildHttpParams(params?: QueryParams): HttpParams {
    let httpParams = new HttpParams();

    if (params) {
      Object.keys(params).forEach(key => {
        const value = params[key];
        if (value !== null && value !== undefined) {
          if (Array.isArray(value)) {
            value.forEach(item => {
              httpParams = httpParams.append(key, item.toString());
            });
          } else {
            httpParams = httpParams.set(key, value.toString());
          }
        }
      });
    }

    return httpParams;
  }

  /**
   * Generate cache key
   */
  protected generateCacheKey(method: string, params?: any): string {
    const paramString = params ? JSON.stringify(params) : '';
    return `${this.endpoint}_${method}_${this.hashString(paramString)}`;
  }

  /**
   * Simple hash function
   */
  protected hashString(str: string): string {
    let hash = 0;
    if (str.length === 0) return hash.toString();
    
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash;
    }
    
    return Math.abs(hash).toString();
  }

  /**
   * Show loading state
   */
  protected showLoading(loading: boolean | string): void {
    if (loading === true) {
      this.loadingService.setGlobalLoading(true);
    } else if (typeof loading === 'string') {
      this.loadingService.setComponentLoading(loading, true);
    }
  }

  /**
   * Hide loading state
   */
  protected hideLoading(loading: boolean | string): void {
    if (loading === true) {
      this.loadingService.setGlobalLoading(false);
    } else if (typeof loading === 'string') {
      this.loadingService.setComponentLoading(loading, false);
    }
  }

  /**
   * Update request count
   */
  protected updateRequestCount(): void {
    this.requestCountSubject.next(this.activeRequests.size);
  }
}