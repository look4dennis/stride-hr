import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of, combineLatest } from 'rxjs';
import { map, catchError, tap, shareReplay } from 'rxjs/operators';
import { NotificationService } from '../../core/services/notification.service';

export interface DropdownOption {
  value: any;
  label: string;
  disabled?: boolean;
  group?: string;
  data?: any;
}

export interface DropdownConfig {
  id: string;
  endpoint?: string;
  staticData?: DropdownOption[];
  valueField?: string;
  labelField?: string;
  groupField?: string;
  sortBy?: string;
  filterBy?: string;
  dependencies?: string[];
  cacheDuration?: number;
  refreshOnFocus?: boolean;
}

export interface DropdownValidationResult {
  dropdownId: string;
  isWorking: boolean;
  hasData: boolean;
  dataCount: number;
  error?: string;
  suggestions?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class DropdownDataService {
  private readonly API_URL = 'https://localhost:5001/api';
  private dropdownConfigs = new Map<string, DropdownConfig>();
  private dropdownData = new Map<string, BehaviorSubject<DropdownOption[]>>();
  private dropdownCache = new Map<string, { data: DropdownOption[]; timestamp: number }>();

  constructor(
    private http: HttpClient,
    private notificationService: NotificationService
  ) {
    this.initializeCommonDropdowns();
  }

  /**
   * Initialize common dropdown configurations
   */
  private initializeCommonDropdowns(): void {
    // Employee-related dropdowns
    this.registerDropdown('departments', {
      id: 'departments',
      endpoint: '/employees/departments',
      staticData: [
        { value: 'Development', label: 'Development' },
        { value: 'Human Resources', label: 'Human Resources' },
        { value: 'Marketing', label: 'Marketing' },
        { value: 'Sales', label: 'Sales' },
        { value: 'Finance', label: 'Finance' },
        { value: 'Operations', label: 'Operations' }
      ],
      cacheDuration: 300000 // 5 minutes
    });

    this.registerDropdown('designations', {
      id: 'designations',
      endpoint: '/employees/designations',
      staticData: [
        { value: 'Senior Developer', label: 'Senior Developer' },
        { value: 'Junior Developer', label: 'Junior Developer' },
        { value: 'Development Manager', label: 'Development Manager' },
        { value: 'HR Manager', label: 'HR Manager' },
        { value: 'HR Executive', label: 'HR Executive' },
        { value: 'Marketing Manager', label: 'Marketing Manager' },
        { value: 'Marketing Executive', label: 'Marketing Executive' },
        { value: 'Sales Manager', label: 'Sales Manager' },
        { value: 'Sales Executive', label: 'Sales Executive' },
        { value: 'Finance Manager', label: 'Finance Manager' },
        { value: 'Accountant', label: 'Accountant' }
      ],
      cacheDuration: 300000
    });

    this.registerDropdown('branches', {
      id: 'branches',
      endpoint: '/branches',
      valueField: 'id',
      labelField: 'name',
      staticData: [
        { value: 1, label: 'Main Office (United States)', data: { country: 'United States', currency: 'USD' } }
      ],
      cacheDuration: 600000 // 10 minutes
    });

    this.registerDropdown('managers', {
      id: 'managers',
      endpoint: '/employees/managers',
      valueField: 'id',
      labelField: 'fullName',
      dependencies: ['branches'],
      cacheDuration: 180000 // 3 minutes
    });

    this.registerDropdown('employees', {
      id: 'employees',
      endpoint: '/employees',
      valueField: 'id',
      labelField: 'fullName',
      dependencies: ['branches', 'departments'],
      cacheDuration: 180000
    });

    // Role-related dropdowns
    this.registerDropdown('roles', {
      id: 'roles',
      endpoint: '/roles',
      valueField: 'id',
      labelField: 'name',
      staticData: [
        { value: 1, label: 'Employee' },
        { value: 2, label: 'Manager' },
        { value: 3, label: 'HR' },
        { value: 4, label: 'Admin' },
        { value: 5, label: 'SuperAdmin' }
      ],
      cacheDuration: 600000
    });

    // Leave-related dropdowns
    this.registerDropdown('leave-types', {
      id: 'leave-types',
      endpoint: '/leave/types',
      staticData: [
        { value: 'Annual', label: 'Annual Leave' },
        { value: 'Sick', label: 'Sick Leave' },
        { value: 'Personal', label: 'Personal Leave' },
        { value: 'Maternity', label: 'Maternity Leave' },
        { value: 'Paternity', label: 'Paternity Leave' },
        { value: 'Emergency', label: 'Emergency Leave' }
      ],
      cacheDuration: 600000
    });

    this.registerDropdown('leave-status', {
      id: 'leave-status',
      staticData: [
        { value: 'Pending', label: 'Pending' },
        { value: 'Approved', label: 'Approved' },
        { value: 'Rejected', label: 'Rejected' },
        { value: 'Cancelled', label: 'Cancelled' }
      ]
    });

    // Project-related dropdowns
    this.registerDropdown('project-status', {
      id: 'project-status',
      staticData: [
        { value: 'Planning', label: 'Planning' },
        { value: 'Active', label: 'Active' },
        { value: 'OnHold', label: 'On Hold' },
        { value: 'Completed', label: 'Completed' },
        { value: 'Cancelled', label: 'Cancelled' }
      ]
    });

    this.registerDropdown('project-priority', {
      id: 'project-priority',
      staticData: [
        { value: 'Low', label: 'Low Priority' },
        { value: 'Medium', label: 'Medium Priority' },
        { value: 'High', label: 'High Priority' },
        { value: 'Critical', label: 'Critical Priority' }
      ]
    });

    // Attendance-related dropdowns
    this.registerDropdown('attendance-status', {
      id: 'attendance-status',
      staticData: [
        { value: 'Present', label: 'Present' },
        { value: 'Absent', label: 'Absent' },
        { value: 'Late', label: 'Late' },
        { value: 'OnBreak', label: 'On Break' },
        { value: 'HalfDay', label: 'Half Day' },
        { value: 'OnLeave', label: 'On Leave' }
      ]
    });

    this.registerDropdown('break-types', {
      id: 'break-types',
      staticData: [
        { value: 'Tea', label: 'Tea Break' },
        { value: 'Lunch', label: 'Lunch Break' },
        { value: 'Personal', label: 'Personal Break' },
        { value: 'Meeting', label: 'Meeting Break' }
      ]
    });

    // Report-related dropdowns
    this.registerDropdown('report-types', {
      id: 'report-types',
      staticData: [
        { value: 'Attendance', label: 'Attendance Report' },
        { value: 'Payroll', label: 'Payroll Report' },
        { value: 'Performance', label: 'Performance Report' },
        { value: 'Leave', label: 'Leave Report' },
        { value: 'Project', label: 'Project Report' }
      ]
    });

    this.registerDropdown('report-formats', {
      id: 'report-formats',
      staticData: [
        { value: 'pdf', label: 'PDF' },
        { value: 'excel', label: 'Excel' },
        { value: 'csv', label: 'CSV' },
        { value: 'json', label: 'JSON' }
      ]
    });

    // Time-related dropdowns
    this.registerDropdown('months', {
      id: 'months',
      staticData: [
        { value: 1, label: 'January' },
        { value: 2, label: 'February' },
        { value: 3, label: 'March' },
        { value: 4, label: 'April' },
        { value: 5, label: 'May' },
        { value: 6, label: 'June' },
        { value: 7, label: 'July' },
        { value: 8, label: 'August' },
        { value: 9, label: 'September' },
        { value: 10, label: 'October' },
        { value: 11, label: 'November' },
        { value: 12, label: 'December' }
      ]
    });

    this.registerDropdown('years', {
      id: 'years',
      staticData: this.generateYearOptions()
    });
  }

  /**
   * Generate year options for dropdowns
   */
  private generateYearOptions(): DropdownOption[] {
    const currentYear = new Date().getFullYear();
    const years: DropdownOption[] = [];
    
    for (let year = currentYear - 5; year <= currentYear + 5; year++) {
      years.push({ value: year, label: year.toString() });
    }
    
    return years;
  }

  /**
   * Register a dropdown configuration
   */
  registerDropdown(dropdownId: string, config: DropdownConfig): void {
    this.dropdownConfigs.set(dropdownId, config);
    this.dropdownData.set(dropdownId, new BehaviorSubject<DropdownOption[]>([]));
    
    // Load initial data
    this.loadDropdownData(dropdownId);
  }

  /**
   * Get dropdown data as observable
   */
  getDropdownData(dropdownId: string): Observable<DropdownOption[]> {
    const subject = this.dropdownData.get(dropdownId);
    if (!subject) {
      console.warn(`Dropdown not registered: ${dropdownId}`);
      return of([]);
    }
    
    return subject.asObservable();
  }

  /**
   * Load dropdown data from API or static data
   */
  loadDropdownData(dropdownId: string, forceRefresh = false): Observable<DropdownOption[]> {
    const config = this.dropdownConfigs.get(dropdownId);
    if (!config) {
      console.error(`Dropdown config not found: ${dropdownId}`);
      return of([]);
    }

    // Check cache first
    if (!forceRefresh && this.isCacheValid(dropdownId)) {
      const cachedData = this.dropdownCache.get(dropdownId)!.data;
      this.updateDropdownData(dropdownId, cachedData);
      return of(cachedData);
    }

    // Load from API or use static data
    if (config.endpoint) {
      return this.loadFromAPI(dropdownId, config);
    } else if (config.staticData) {
      const data = this.processStaticData(config.staticData, config);
      this.updateDropdownData(dropdownId, data);
      this.cacheData(dropdownId, data);
      return of(data);
    } else {
      console.warn(`No data source configured for dropdown: ${dropdownId}`);
      return of([]);
    }
  }

  /**
   * Load dropdown data from API
   */
  private loadFromAPI(dropdownId: string, config: DropdownConfig): Observable<DropdownOption[]> {
    const url = `${this.API_URL}${config.endpoint}`;
    
    return this.http.get<any>(url).pipe(
      map(response => {
        // Handle different response formats
        let data = response;
        if (response.data) {
          data = response.data;
        } else if (response.items) {
          data = response.items;
        }
        
        return this.processAPIData(data, config);
      }),
      tap(data => {
        this.updateDropdownData(dropdownId, data);
        this.cacheData(dropdownId, data);
      }),
      catchError(error => {
        console.warn(`API call failed for dropdown ${dropdownId}, using static data:`, error);
        
        // Fallback to static data if available
        if (config.staticData) {
          const fallbackData = this.processStaticData(config.staticData, config);
          this.updateDropdownData(dropdownId, fallbackData);
          return of(fallbackData);
        }
        
        this.notificationService.showWarning(`Failed to load ${dropdownId} data`);
        return of([]);
      }),
      shareReplay(1)
    );
  }

  /**
   * Process API response data
   */
  private processAPIData(data: any[], config: DropdownConfig): DropdownOption[] {
    if (!Array.isArray(data)) {
      console.warn('API response is not an array:', data);
      return [];
    }

    return data.map(item => {
      const option: DropdownOption = {
        value: config.valueField ? item[config.valueField] : item,
        label: config.labelField ? item[config.labelField] : item.toString(),
        disabled: item.disabled || false,
        data: item
      };

      if (config.groupField && item[config.groupField]) {
        option.group = item[config.groupField];
      }

      return option;
    });
  }

  /**
   * Process static data
   */
  private processStaticData(data: DropdownOption[], config: DropdownConfig): DropdownOption[] {
    let processedData = [...data];

    // Apply sorting if specified
    if (config.sortBy) {
      processedData.sort((a, b) => {
        const aValue = (a as any)[config.sortBy!];
        const bValue = (b as any)[config.sortBy!];
        return aValue < bValue ? -1 : aValue > bValue ? 1 : 0;
      });
    }

    return processedData;
  }

  /**
   * Update dropdown data subject
   */
  private updateDropdownData(dropdownId: string, data: DropdownOption[]): void {
    const subject = this.dropdownData.get(dropdownId);
    if (subject) {
      subject.next(data);
    }
  }

  /**
   * Cache dropdown data
   */
  private cacheData(dropdownId: string, data: DropdownOption[]): void {
    const config = this.dropdownConfigs.get(dropdownId);
    if (config?.cacheDuration) {
      this.dropdownCache.set(dropdownId, {
        data,
        timestamp: Date.now()
      });
    }
  }

  /**
   * Check if cached data is still valid
   */
  private isCacheValid(dropdownId: string): boolean {
    const config = this.dropdownConfigs.get(dropdownId);
    const cached = this.dropdownCache.get(dropdownId);
    
    if (!config?.cacheDuration || !cached) {
      return false;
    }
    
    return Date.now() - cached.timestamp < config.cacheDuration;
  }

  /**
   * Refresh dropdown data
   */
  refreshDropdown(dropdownId: string): Observable<DropdownOption[]> {
    return this.loadDropdownData(dropdownId, true);
  }

  /**
   * Refresh all dropdowns
   */
  refreshAllDropdowns(): Observable<void> {
    const refreshObservables = Array.from(this.dropdownConfigs.keys()).map(
      dropdownId => this.refreshDropdown(dropdownId)
    );
    
    return combineLatest(refreshObservables).pipe(
      map(() => void 0),
      tap(() => this.notificationService.showSuccess('All dropdowns refreshed'))
    );
  }

  /**
   * Filter dropdown options
   */
  filterDropdownOptions(dropdownId: string, searchTerm: string): Observable<DropdownOption[]> {
    return this.getDropdownData(dropdownId).pipe(
      map(options => options.filter(option => 
        option.label.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (option.value && option.value.toString().toLowerCase().includes(searchTerm.toLowerCase()))
      ))
    );
  }

  /**
   * Get dropdown option by value
   */
  getDropdownOption(dropdownId: string, value: any): Observable<DropdownOption | undefined> {
    return this.getDropdownData(dropdownId).pipe(
      map(options => options.find(option => option.value === value))
    );
  }

  /**
   * Validate all dropdowns
   */
  validateAllDropdowns(): Observable<DropdownValidationResult[]> {
    const results: DropdownValidationResult[] = [];
    const validationPromises: Promise<void>[] = [];

    this.dropdownConfigs.forEach((config, dropdownId) => {
      const promise = new Promise<void>((resolve) => {
        this.loadDropdownData(dropdownId).subscribe({
          next: (data) => {
            results.push({
              dropdownId,
              isWorking: true,
              hasData: data.length > 0,
              dataCount: data.length,
              suggestions: data.length === 0 ? [
                'Check API endpoint configuration',
                'Verify static data is properly configured',
                'Ensure database connectivity'
              ] : undefined
            });
            resolve();
          },
          error: (error) => {
            results.push({
              dropdownId,
              isWorking: false,
              hasData: false,
              dataCount: 0,
              error: error.message || 'Failed to load dropdown data',
              suggestions: [
                'Check API endpoint accessibility',
                'Verify network connectivity',
                'Check server logs for errors',
                'Ensure proper authentication'
              ]
            });
            resolve();
          }
        });
      });
      
      validationPromises.push(promise);
    });

    return new Observable(observer => {
      Promise.all(validationPromises).then(() => {
        observer.next(results);
        observer.complete();
      });
    });
  }

  /**
   * Get dropdown configuration
   */
  getDropdownConfig(dropdownId: string): DropdownConfig | undefined {
    return this.dropdownConfigs.get(dropdownId);
  }

  /**
   * Update dropdown configuration
   */
  updateDropdownConfig(dropdownId: string, updates: Partial<DropdownConfig>): void {
    const existing = this.dropdownConfigs.get(dropdownId);
    if (existing) {
      this.dropdownConfigs.set(dropdownId, { ...existing, ...updates });
    }
  }

  /**
   * Clear dropdown cache
   */
  clearCache(dropdownId?: string): void {
    if (dropdownId) {
      this.dropdownCache.delete(dropdownId);
    } else {
      this.dropdownCache.clear();
    }
  }
}