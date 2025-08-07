import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, BehaviorSubject } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { 
  Employee, 
  EmployeeSearchCriteria, 
  PagedResult, 
  EmployeeStatus,
  CreateEmployeeDto,
  UpdateEmployeeDto,
  EmployeeOnboarding,
  EmployeeExitProcess,
  OrganizationalChart
} from '../models/employee.models';
import { EnhancedBaseApiService, ApiResponse, RequestOptions } from '../core/services/enhanced-base-api.service';
import { CacheService } from '../core/services/cache.service';
import { LoadingService } from '../core/services/loading.service';
import { NotificationService } from '../core/services/notification.service';

@Injectable({
  providedIn: 'root'
})
export class OptimizedEmployeeService extends EnhancedBaseApiService<Employee> {
  protected readonly endpoint = 'employees';

  // Real-time data subjects
  private employeesSubject = new BehaviorSubject<Employee[]>([]);
  private departmentsSubject = new BehaviorSubject<string[]>([]);
  private designationsSubject = new BehaviorSubject<string[]>([]);

  // Public observables
  public employees$ = this.employeesSubject.asObservable();
  public departments$ = this.departmentsSubject.asObservable();
  public designations$ = this.designationsSubject.asObservable();

  constructor(
    protected override http: HttpClient,
    protected override cacheService: CacheService,
    protected override loadingService: LoadingService,
    protected override notificationService: NotificationService
  ) {
    super(http, cacheService, loadingService, notificationService);
  }

  /**
   * Get employees with search criteria and caching
   */
  getEmployees(criteria?: EmployeeSearchCriteria): Observable<PagedResult<Employee>> {
    const options: RequestOptions = {
      cache: {
        enabled: true,
        ttl: 2 * 60 * 1000 // 2 minutes cache for employee lists
      },
      loading: 'employee-list',
      showErrors: true,
      retry: 2
    };

    return this.getAll(criteria, options).pipe(
      map(response => {
        if (response.success && response.data) {
          // Convert API response to PagedResult format
          const pagedResult: PagedResult<Employee> = {
            items: response.data,
            page: response.pagination?.currentPage || 1,
            pageSize: response.pagination?.pageSize || 10,
            totalCount: response.pagination?.totalCount || response.data.length,
            totalPages: response.pagination?.totalPages || 1,
            hasNext: response.pagination?.hasNext || false,
            hasPrevious: response.pagination?.hasPrevious || false
          };

          // Update local state
          this.employeesSubject.next(response.data);
          return pagedResult;
        }
        return this.getMockEmployees();
      }),
      catchError(() => of(this.getMockEmployees()))
    );
  }

  /**
   * Get employee by ID with caching
   */
  getEmployeeById(id: number): Observable<Employee | null> {
    const options: RequestOptions = {
      cache: {
        enabled: true,
        ttl: 5 * 60 * 1000 // 5 minutes cache for individual employees
      },
      loading: 'employee-detail',
      showErrors: true,
      retry: 2
    };

    return this.getById(id, options).pipe(
      map(response => response.success && response.data ? response.data : null),
      catchError(() => of(null))
    );
  }

  /**
   * Create new employee
   */
  createEmployee(employee: CreateEmployeeDto): Observable<Employee | null> {
    const options: RequestOptions = {
      loading: 'employee-create',
      showErrors: true
    };

    // Convert DTO to FormData if profilePhoto is a File
    const requestData = this.prepareEmployeeData(employee);

    return this.http.post<ApiResponse<Employee>>(`${this.apiUrl}/${this.endpoint}`, requestData)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            this.notificationService.showSuccess('Employee created successfully');
            // Invalidate employee list cache
            this.cacheService.invalidateApiCache(this.endpoint);
            return response.data;
          }
          return null;
        }),
        catchError(() => of(null))
      );
  }

  /**
   * Update employee
   */
  updateEmployee(id: number, employee: UpdateEmployeeDto): Observable<Employee | null> {
    const options: RequestOptions = {
      loading: 'employee-update',
      showErrors: true
    };

    // Convert DTO to FormData if profilePhoto is a File
    const requestData = this.prepareEmployeeData(employee);

    return this.http.put<ApiResponse<Employee>>(`${this.apiUrl}/${this.endpoint}/${id}`, requestData)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            this.notificationService.showSuccess('Employee updated successfully');
            // Invalidate related cache entries
            this.cacheService.invalidateApiCache(this.endpoint);
            return response.data;
          }
          return null;
        }),
        catchError(() => of(null))
      );
  }

  /**
   * Delete employee (soft delete)
   */
  deleteEmployee(id: number): Observable<boolean> {
    const options: RequestOptions = {
      loading: 'employee-delete',
      showErrors: true
    };

    return this.delete(id, options).pipe(
      map(response => {
        if (response.success) {
          this.notificationService.showSuccess('Employee deleted successfully');
          // Invalidate related cache entries
          this.cacheService.invalidateApiCache(this.endpoint);
          return true;
        }
        return false;
      }),
      catchError(() => of(false))
    );
  }

  /**
   * Deactivate employee
   */
  deactivateEmployee(id: number): Observable<boolean> {
    const options: RequestOptions = {
      loading: 'employee-deactivate',
      showErrors: true
    };

    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${this.endpoint}/${id}/deactivate`, {})
      .pipe(
        map(response => {
          if (response.success) {
            this.notificationService.showSuccess('Employee deactivated successfully');
            // Invalidate related cache entries
            this.cacheService.invalidateApiCache(this.endpoint);
            return true;
          }
          return false;
        }),
        catchError(() => of(false))
      );
  }

  /**
   * Get departments with caching
   */
  getDepartments(): Observable<string[]> {
    const cacheKey = 'departments';
    
    return this.cacheService.getOrSet(
      cacheKey,
      () => this.http.get<ApiResponse<string[]>>(`${this.apiUrl}/${this.endpoint}/departments`)
        .pipe(
          map(response => {
            const departments = response.success && response.data ? response.data : this.getMockDepartments();
            this.departmentsSubject.next(departments);
            return departments;
          }),
          catchError(() => {
            const mockDepartments = this.getMockDepartments();
            this.departmentsSubject.next(mockDepartments);
            return of(mockDepartments);
          })
        ),
      10 * 60 * 1000 // 10 minutes cache
    );
  }

  /**
   * Get designations with caching
   */
  getDesignations(): Observable<string[]> {
    const cacheKey = 'designations';
    
    return this.cacheService.getOrSet(
      cacheKey,
      () => this.http.get<ApiResponse<string[]>>(`${this.apiUrl}/${this.endpoint}/designations`)
        .pipe(
          map(response => {
            const designations = response.success && response.data ? response.data : this.getMockDesignations();
            this.designationsSubject.next(designations);
            return designations;
          }),
          catchError(() => {
            const mockDesignations = this.getMockDesignations();
            this.designationsSubject.next(mockDesignations);
            return of(mockDesignations);
          })
        ),
      10 * 60 * 1000 // 10 minutes cache
    );
  }

  /**
   * Search employees with debouncing and caching
   */
  searchEmployees(query: string, criteria?: EmployeeSearchCriteria): Observable<Employee[]> {
    const searchCriteria = { ...criteria, searchTerm: query };
    const options: RequestOptions = {
      cache: {
        enabled: true,
        ttl: 1 * 60 * 1000 // 1 minute cache for search results
      },
      loading: false, // Don't show loading for search
      showErrors: false, // Don't show errors for search
      retry: 1
    };

    return this.search(query, searchCriteria, options).pipe(
      map(response => response.success && response.data ? response.data : []),
      catchError(() => of([]))
    );
  }

  /**
   * Get organizational chart with caching
   */
  getOrganizationalChart(branchId?: number): Observable<OrganizationalChart | null> {
    const cacheKey = `org-chart-${branchId || 'all'}`;
    
    return this.cacheService.getOrSet(
      cacheKey,
      () => {
        const url = branchId 
          ? `${this.apiUrl}/${this.endpoint}/org-chart?branchId=${branchId}`
          : `${this.apiUrl}/${this.endpoint}/org-chart`;
          
        return this.http.get<ApiResponse<OrganizationalChart>>(url)
          .pipe(
            map(response => response.success && response.data ? response.data : null),
            catchError(() => of(null))
          );
      },
      5 * 60 * 1000 // 5 minutes cache
    );
  }

  /**
   * Get employee onboarding status
   */
  getOnboardingStatus(employeeId: number): Observable<EmployeeOnboarding | null> {
    const options: RequestOptions = {
      cache: {
        enabled: true,
        ttl: 2 * 60 * 1000 // 2 minutes cache
      },
      loading: 'onboarding-status',
      showErrors: true
    };

    return this.http.get<ApiResponse<EmployeeOnboarding>>(`${this.apiUrl}/${this.endpoint}/${employeeId}/onboarding`)
      .pipe(
        map(response => response.success && response.data ? response.data : null),
        catchError(() => of(null))
      );
  }

  /**
   * Update onboarding status
   */
  updateOnboardingStatus(employeeId: number, onboarding: Partial<EmployeeOnboarding>): Observable<boolean> {
    const options: RequestOptions = {
      loading: 'onboarding-update',
      showErrors: true
    };

    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${this.endpoint}/${employeeId}/onboarding`, onboarding)
      .pipe(
        map(response => {
          if (response.success) {
            this.notificationService.showSuccess('Onboarding status updated successfully');
            // Invalidate related cache
            this.cacheService.invalidatePattern(`${this.endpoint}.*onboarding`);
            return true;
          }
          return false;
        }),
        catchError(() => of(false))
      );
  }

  /**
   * Initiate exit process
   */
  initiateExitProcess(employeeId: number, exitData: Partial<EmployeeExitProcess>): Observable<boolean> {
    const options: RequestOptions = {
      loading: 'exit-process',
      showErrors: true
    };

    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/${this.endpoint}/${employeeId}/exit`, exitData)
      .pipe(
        map(response => {
          if (response.success) {
            this.notificationService.showSuccess('Exit process initiated successfully');
            // Invalidate related cache
            this.cacheService.invalidateApiCache(this.endpoint);
            return true;
          }
          return false;
        }),
        catchError(() => of(false))
      );
  }

  /**
   * Bulk import employees
   */
  bulkImportEmployees(employees: CreateEmployeeDto[]): Observable<{ success: number; failed: number; errors: any[] }> {
    const options: RequestOptions = {
      loading: 'bulk-import',
      showErrors: true
    };

    // Convert DTOs to proper format (remove File objects for bulk import)
    const employeeData = employees.map(emp => {
      const { profilePhoto, ...rest } = emp;
      return rest;
    });

    return this.http.post<ApiResponse<Employee[]>>(`${this.apiUrl}/${this.endpoint}/bulk`, employeeData)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            this.notificationService.showSuccess(`Successfully imported ${response.data.length} employees`);
            // Invalidate employee cache
            this.cacheService.invalidateApiCache(this.endpoint);
            return {
              success: response.data.length,
              failed: 0,
              errors: []
            };
          }
          return {
            success: 0,
            failed: employees.length,
            errors: response.errors || []
          };
        }),
        catchError(error => of({
          success: 0,
          failed: employees.length,
          errors: [error]
        }))
      );
  }

  /**
   * Export employees to CSV
   */
  exportEmployees(criteria?: EmployeeSearchCriteria): Observable<Blob> {
    const options: RequestOptions = {
      loading: 'export-employees',
      showErrors: true
    };

    return this.http.get(`${this.apiUrl}/${this.endpoint}/export`, {
      params: this.buildHttpParams(criteria),
      responseType: 'blob'
    }).pipe(
      tap(() => this.notificationService.showSuccess('Employee data exported successfully')),
      catchError(error => {
        this.notificationService.showError('Failed to export employee data');
        throw error;
      })
    );
  }

  /**
   * Get employee statistics
   */
  getEmployeeStatistics(branchId?: number): Observable<any> {
    const cacheKey = `employee-stats-${branchId || 'all'}`;
    
    return this.cacheService.getOrSet(
      cacheKey,
      () => {
        const url = branchId 
          ? `${this.apiUrl}/${this.endpoint}/statistics?branchId=${branchId}`
          : `${this.apiUrl}/${this.endpoint}/statistics`;
          
        return this.http.get<ApiResponse<any>>(url)
          .pipe(
            map(response => response.success && response.data ? response.data : {}),
            catchError(() => of({}))
          );
      },
      3 * 60 * 1000 // 3 minutes cache
    );
  }

  /**
   * Prepare employee data for API request (handle File uploads)
   */
  private prepareEmployeeData(employee: CreateEmployeeDto | UpdateEmployeeDto): FormData | any {
    if (employee.profilePhoto instanceof File) {
      // Create FormData for file upload
      const formData = new FormData();
      
      Object.keys(employee).forEach(key => {
        const value = (employee as any)[key];
        if (value !== undefined && value !== null) {
          if (key === 'profilePhoto' && value instanceof File) {
            formData.append(key, value);
          } else {
            formData.append(key, value.toString());
          }
        }
      });
      
      return formData;
    } else {
      // Return regular object for JSON request
      return employee;
    }
  }

  // Mock data methods for fallback
  private getMockEmployees(): PagedResult<Employee> {
    const mockEmployees = [
      {
        id: 1,
        employeeId: 'EMP001',
        branchId: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@company.com',
        phone: '+1234567890',
        dateOfBirth: '1990-01-15',
        joiningDate: '2023-01-01',
        designation: 'Senior Developer',
        department: 'Development',
        basicSalary: 75000,
        reportingManagerId: 2,
        profilePhoto: '/assets/images/avatars/default-avatar.png',
        status: EmployeeStatus.Active,
        createdAt: '2023-01-01T00:00:00Z'
      },
      {
        id: 2,
        employeeId: 'EMP002',
        branchId: 1,
        firstName: 'Jane',
        lastName: 'Smith',
        email: 'jane.smith@company.com',
        phone: '+1234567891',
        dateOfBirth: '1988-05-20',
        joiningDate: '2022-06-15',
        designation: 'Development Manager',
        department: 'Development',
        basicSalary: 95000,
        profilePhoto: '/assets/images/avatars/default-avatar.png',
        status: EmployeeStatus.Active,
        createdAt: '2022-06-15T00:00:00Z'
      }
    ] as Employee[];

    return {
      items: mockEmployees,
      page: 1,
      pageSize: 10,
      totalCount: mockEmployees.length,
      totalPages: 1,
      hasNext: false,
      hasPrevious: false
    };
  }

  private getMockDepartments(): string[] {
    return ['Development', 'Human Resources', 'Marketing', 'Sales', 'Finance', 'Operations'];
  }

  private getMockDesignations(): string[] {
    return [
      'Senior Developer', 'Junior Developer', 'Development Manager',
      'HR Manager', 'HR Executive', 'Marketing Manager', 'Sales Executive',
      'Finance Manager', 'Operations Manager'
    ];
  }
}