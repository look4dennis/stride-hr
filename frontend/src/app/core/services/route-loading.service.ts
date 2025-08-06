import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { NotificationService } from './notification.service';

export interface RouteLoadingError {
  route: string;
  error: any;
  timestamp: Date;
  retryCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class RouteLoadingService {
  private loadingErrors = new BehaviorSubject<RouteLoadingError[]>([]);
  private maxRetries = 3;
  private retryDelay = 1000; // 1 second

  constructor(
    private router: Router,
    private notificationService: NotificationService
  ) {}

  /**
   * Handle lazy loading errors with retry mechanism
   */
  handleLazyLoadError(route: string, error: any): Promise<any> {
    console.error(`Failed to load route: ${route}`, error);
    
    const existingError = this.loadingErrors.value.find(e => e.route === route);
    
    if (existingError) {
      existingError.retryCount++;
      existingError.timestamp = new Date();
    } else {
      const newError: RouteLoadingError = {
        route,
        error,
        timestamp: new Date(),
        retryCount: 1
      };
      this.loadingErrors.next([...this.loadingErrors.value, newError]);
    }

    // If we haven't exceeded max retries, try again
    const currentError = existingError || this.loadingErrors.value.find(e => e.route === route)!;
    
    if (currentError.retryCount <= this.maxRetries) {
      this.notificationService.showError(
        `Loading failed. Retrying... (${currentError.retryCount}/${this.maxRetries})`
      );
      
      return new Promise((resolve, reject) => {
        setTimeout(() => {
          // Attempt to reload the chunk
          this.retryLazyLoad(route).then(resolve).catch(reject);
        }, this.retryDelay * currentError.retryCount);
      });
    } else {
      // Max retries exceeded, show error page
      this.notificationService.showError(
        'Failed to load page after multiple attempts. Please try refreshing the browser.'
      );
      
      return this.router.navigate(['/route-error'], {
        queryParams: {
          route,
          error: error.message || 'Unknown error',
          timestamp: new Date().toISOString()
        }
      });
    }
  }

  /**
   * Retry loading a lazy-loaded component
   */
  private async retryLazyLoad(route: string): Promise<any> {
    try {
      // Clear the module cache to force reload
      if ('webpackChunkName' in window) {
        delete (window as any).webpackChunkName;
      }
      
      // Attempt to re-import the module based on route
      const modulePromise = this.getModuleImportForRoute(route);
      if (modulePromise) {
        return await modulePromise;
      } else {
        throw new Error(`No module import found for route: ${route}`);
      }
    } catch (error) {
      console.error(`Retry failed for route: ${route}`, error);
      throw error;
    }
  }

  /**
   * Get the appropriate module import for a given route
   */
  private getModuleImportForRoute(route: string): (() => Promise<any>) | null {
    // Map routes to their import functions
    const routeImports: Record<string, () => Promise<any>> = {
      'employees': () => import('../../features/employees/employee-list/employee-list.component').then(m => m.EmployeeListComponent),
      'employees/add': () => import('../../features/employees/employee-create/employee-create.component').then(m => m.EmployeeCreateComponent),
      'employees/org-chart': () => import('../../features/employees/org-chart/org-chart.component').then(m => m.OrgChartComponent),
      'attendance': () => import('../../features/attendance/attendance-tracker/attendance-tracker.component').then(m => m.AttendanceTrackerComponent),
      'attendance/now': () => import('../../features/attendance/attendance-now/attendance-now.component').then(m => m.AttendanceNowComponent),
      'projects': () => import('../../features/projects/project-list/project-list.component').then(m => m.ProjectListComponent),
      'payroll': () => import('../../features/payroll/payroll-list/payroll-list.component').then(m => m.PayrollListComponent),
      'leave': () => import('../../features/leave/leave-list/leave-list.component').then(m => m.LeaveListComponent),
      'performance': () => import('../../features/performance/performance-list/performance-list.component').then(m => m.PerformanceListComponent),
      'reports': () => import('../../features/reports/report-list/report-list.component').then(m => m.ReportListComponent),
      'settings': () => import('../../features/settings/settings.component').then(m => m.SettingsComponent),
      'profile': () => import('../../features/profile/profile.component').then(m => m.ProfileComponent)
    };

    return routeImports[route] || null;
  }

  /**
   * Clear loading errors for a specific route
   */
  clearRouteError(route: string): void {
    const errors = this.loadingErrors.value.filter(e => e.route !== route);
    this.loadingErrors.next(errors);
  }

  /**
   * Clear all loading errors
   */
  clearAllErrors(): void {
    this.loadingErrors.next([]);
  }

  /**
   * Get current loading errors
   */
  getLoadingErrors(): Observable<RouteLoadingError[]> {
    return this.loadingErrors.asObservable();
  }

  /**
   * Create a safe lazy load function that handles errors
   */
  createSafeLazyLoad(route: string, importFn: () => Promise<any>): () => Promise<any> {
    return () => {
      return importFn().catch(error => {
        return this.handleLazyLoadError(route, error);
      });
    };
  }
}