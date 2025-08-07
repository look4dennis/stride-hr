import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of, Subject } from 'rxjs';
import { map, catchError, debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { NotificationService } from '../../core/services/notification.service';

export interface SearchConfig {
  id: string;
  endpoint: string;
  searchFields: string[];
  displayFields: string[];
  valueField?: string;
  labelField?: string;
  placeholder?: string;
  minSearchLength?: number;
  debounceTime?: number;
  maxResults?: number;
  filters?: { [key: string]: any };
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface SearchResult {
  value: any;
  label: string;
  subtitle?: string;
  data: any;
  highlighted?: string;
}

export interface SearchResponse {
  results: SearchResult[];
  totalCount: number;
  hasMore: boolean;
  searchTerm: string;
}

export interface SearchValidationResult {
  searchId: string;
  isWorking: boolean;
  hasEndpoint: boolean;
  canConnect: boolean;
  error?: string;
  suggestions?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private readonly API_URL = 'http://localhost:5000/api';
  private searchConfigs = new Map<string, SearchConfig>();
  private searchSubjects = new Map<string, Subject<string>>();
  private searchResults = new Map<string, BehaviorSubject<SearchResponse>>();

  constructor(
    private http: HttpClient,
    private notificationService: NotificationService
  ) {
    this.initializeCommonSearches();
  }

  /**
   * Initialize common search configurations
   */
  private initializeCommonSearches(): void {
    // Employee search
    this.registerSearch('employees', {
      id: 'employees',
      endpoint: '/employees/search',
      searchFields: ['firstName', 'lastName', 'email', 'employeeId'],
      displayFields: ['firstName', 'lastName', 'email', 'designation', 'department'],
      valueField: 'id',
      labelField: 'fullName',
      placeholder: 'Search employees by name, email, or ID...',
      minSearchLength: 2,
      debounceTime: 300,
      maxResults: 50
    });

    // Attendance search
    this.registerSearch('attendance', {
      id: 'attendance',
      endpoint: '/attendance/search',
      searchFields: ['employeeName', 'employeeId'],
      displayFields: ['employeeName', 'date', 'checkInTime', 'checkOutTime', 'status'],
      valueField: 'id',
      labelField: 'employeeName',
      placeholder: 'Search attendance records...',
      minSearchLength: 2,
      debounceTime: 300,
      maxResults: 100
    });

    // Project search
    this.registerSearch('projects', {
      id: 'projects',
      endpoint: '/projects/search',
      searchFields: ['name', 'description', 'projectCode'],
      displayFields: ['name', 'description', 'status', 'startDate', 'endDate'],
      valueField: 'id',
      labelField: 'name',
      placeholder: 'Search projects by name or code...',
      minSearchLength: 2,
      debounceTime: 300,
      maxResults: 30
    });

    // Leave search
    this.registerSearch('leaves', {
      id: 'leaves',
      endpoint: '/leave/search',
      searchFields: ['employeeName', 'leaveType', 'reason'],
      displayFields: ['employeeName', 'leaveType', 'startDate', 'endDate', 'status'],
      valueField: 'id',
      labelField: 'employeeName',
      placeholder: 'Search leave requests...',
      minSearchLength: 2,
      debounceTime: 300,
      maxResults: 50
    });

    // Report search
    this.registerSearch('reports', {
      id: 'reports',
      endpoint: '/reports/search',
      searchFields: ['name', 'description', 'type'],
      displayFields: ['name', 'description', 'type', 'createdDate'],
      valueField: 'id',
      labelField: 'name',
      placeholder: 'Search reports...',
      minSearchLength: 2,
      debounceTime: 300,
      maxResults: 25
    });

    // Payroll search
    this.registerSearch('payroll', {
      id: 'payroll',
      endpoint: '/payroll/search',
      searchFields: ['employeeName', 'employeeId', 'payPeriod'],
      displayFields: ['employeeName', 'payPeriod', 'grossSalary', 'netSalary', 'status'],
      valueField: 'id',
      labelField: 'employeeName',
      placeholder: 'Search payroll records...',
      minSearchLength: 2,
      debounceTime: 300,
      maxResults: 50
    });

    // Performance search
    this.registerSearch('performance', {
      id: 'performance',
      endpoint: '/performance/search',
      searchFields: ['employeeName', 'reviewPeriod', 'reviewType'],
      displayFields: ['employeeName', 'reviewPeriod', 'overallRating', 'status'],
      valueField: 'id',
      labelField: 'employeeName',
      placeholder: 'Search performance reviews...',
      minSearchLength: 2,
      debounceTime: 300,
      maxResults: 30
    });

    // Training search
    this.registerSearch('training', {
      id: 'training',
      endpoint: '/training/search',
      searchFields: ['title', 'description', 'instructor'],
      displayFields: ['title', 'description', 'instructor', 'startDate', 'duration'],
      valueField: 'id',
      labelField: 'title',
      placeholder: 'Search training programs...',
      minSearchLength: 2,
      debounceTime: 300,
      maxResults: 25
    });

    // Branch search
    this.registerSearch('branches', {
      id: 'branches',
      endpoint: '/branches/search',
      searchFields: ['name', 'country', 'city'],
      displayFields: ['name', 'country', 'city', 'address'],
      valueField: 'id',
      labelField: 'name',
      placeholder: 'Search branches...',
      minSearchLength: 2,
      debounceTime: 300,
      maxResults: 20
    });

    // Role search
    this.registerSearch('roles', {
      id: 'roles',
      endpoint: '/roles/search',
      searchFields: ['name', 'description'],
      displayFields: ['name', 'description', 'permissions'],
      valueField: 'id',
      labelField: 'name',
      placeholder: 'Search roles...',
      minSearchLength: 2,
      debounceTime: 300,
      maxResults: 20
    });
  }

  /**
   * Register a search configuration
   */
  registerSearch(searchId: string, config: SearchConfig): void {
    this.searchConfigs.set(searchId, config);
    
    // Create search subject with debouncing
    const searchSubject = new Subject<string>();
    this.searchSubjects.set(searchId, searchSubject);
    
    // Create results subject
    const resultsSubject = new BehaviorSubject<SearchResponse>({
      results: [],
      totalCount: 0,
      hasMore: false,
      searchTerm: ''
    });
    this.searchResults.set(searchId, resultsSubject);

    // Set up search pipeline
    searchSubject.pipe(
      debounceTime(config.debounceTime || 300),
      distinctUntilChanged(),
      switchMap(searchTerm => this.performSearch(searchId, searchTerm))
    ).subscribe(response => {
      resultsSubject.next(response);
    });
  }

  /**
   * Perform search operation
   */
  search(searchId: string, searchTerm: string): void {
    const config = this.searchConfigs.get(searchId);
    if (!config) {
      console.error(`Search configuration not found: ${searchId}`);
      return;
    }

    if (searchTerm.length < (config.minSearchLength || 1)) {
      // Clear results if search term is too short
      const resultsSubject = this.searchResults.get(searchId);
      if (resultsSubject) {
        resultsSubject.next({
          results: [],
          totalCount: 0,
          hasMore: false,
          searchTerm
        });
      }
      return;
    }

    const searchSubject = this.searchSubjects.get(searchId);
    if (searchSubject) {
      searchSubject.next(searchTerm);
    }
  }

  /**
   * Get search results as observable
   */
  getSearchResults(searchId: string): Observable<SearchResponse> {
    const resultsSubject = this.searchResults.get(searchId);
    if (!resultsSubject) {
      console.error(`Search results not found: ${searchId}`);
      return of({
        results: [],
        totalCount: 0,
        hasMore: false,
        searchTerm: ''
      });
    }
    
    return resultsSubject.asObservable();
  }

  /**
   * Perform the actual search API call
   */
  private performSearch(searchId: string, searchTerm: string): Observable<SearchResponse> {
    const config = this.searchConfigs.get(searchId);
    if (!config) {
      return of({
        results: [],
        totalCount: 0,
        hasMore: false,
        searchTerm
      });
    }

    // Build search parameters
    let params = new HttpParams()
      .set('searchTerm', searchTerm)
      .set('maxResults', (config.maxResults || 50).toString());

    // Add search fields
    if (config.searchFields.length > 0) {
      params = params.set('searchFields', config.searchFields.join(','));
    }

    // Add filters
    if (config.filters) {
      Object.keys(config.filters).forEach(key => {
        params = params.set(key, config.filters![key]);
      });
    }

    // Add sorting
    if (config.sortBy) {
      params = params.set('sortBy', config.sortBy);
      params = params.set('sortDirection', config.sortDirection || 'asc');
    }

    const url = `${this.API_URL}${config.endpoint}`;

    return this.http.get<any>(url, { params }).pipe(
      map(response => this.processSearchResponse(response, config, searchTerm)),
      catchError(error => {
        console.warn(`Search API call failed for ${searchId}:`, error);
        return this.getMockSearchResults(searchId, searchTerm, config);
      })
    );
  }

  /**
   * Process search API response
   */
  private processSearchResponse(response: any, config: SearchConfig, searchTerm: string): SearchResponse {
    let data = response;
    let totalCount = 0;

    // Handle different response formats
    if (response.data) {
      data = response.data;
      totalCount = response.totalCount || data.length;
    } else if (response.items) {
      data = response.items;
      totalCount = response.totalCount || data.length;
    } else if (Array.isArray(response)) {
      data = response;
      totalCount = data.length;
    } else {
      console.warn('Unexpected search response format:', response);
      data = [];
    }

    const results = this.processSearchData(data, config, searchTerm);
    
    return {
      results,
      totalCount,
      hasMore: results.length < totalCount,
      searchTerm
    };
  }

  /**
   * Process search data into SearchResult format
   */
  private processSearchData(data: any[], config: SearchConfig, searchTerm: string): SearchResult[] {
    if (!Array.isArray(data)) {
      return [];
    }

    return data.map(item => {
      const result: SearchResult = {
        value: config.valueField ? item[config.valueField] : item.id,
        label: config.labelField ? item[config.labelField] : this.generateLabel(item, config),
        data: item
      };

      // Generate subtitle from display fields
      if (config.displayFields.length > 1) {
        const subtitleParts = config.displayFields
          .slice(1) // Skip first field (used for label)
          .map(field => item[field])
          .filter(value => value !== null && value !== undefined)
          .map(value => value.toString());
        
        if (subtitleParts.length > 0) {
          result.subtitle = subtitleParts.join(' • ');
        }
      }

      // Highlight search term in label
      if (searchTerm && result.label) {
        result.highlighted = this.highlightSearchTerm(result.label, searchTerm);
      }

      return result;
    });
  }

  /**
   * Generate label from item data
   */
  private generateLabel(item: any, config: SearchConfig): string {
    if (config.displayFields.length > 0) {
      const labelField = config.displayFields[0];
      return item[labelField] || item.name || item.title || item.id?.toString() || 'Unknown';
    }
    
    return item.name || item.title || item.label || item.id?.toString() || 'Unknown';
  }

  /**
   * Highlight search term in text
   */
  private highlightSearchTerm(text: string, searchTerm: string): string {
    if (!searchTerm || !text) return text;
    
    const regex = new RegExp(`(${searchTerm})`, 'gi');
    return text.replace(regex, '<mark>$1</mark>');
  }

  /**
   * Get mock search results for development
   */
  private getMockSearchResults(searchId: string, searchTerm: string, config: SearchConfig): Observable<SearchResponse> {
    const mockData = this.generateMockData(searchId, searchTerm);
    const results = this.processSearchData(mockData, config, searchTerm);
    
    return of({
      results,
      totalCount: results.length,
      hasMore: false,
      searchTerm
    });
  }

  /**
   * Generate mock data for development
   */
  private generateMockData(searchId: string, searchTerm: string): any[] {
    const mockDataMap: { [key: string]: any[] } = {
      employees: [
        {
          id: 1,
          employeeId: 'EMP001',
          firstName: 'John',
          lastName: 'Doe',
          fullName: 'John Doe',
          email: 'john.doe@company.com',
          designation: 'Senior Developer',
          department: 'Development'
        },
        {
          id: 2,
          employeeId: 'EMP002',
          firstName: 'Jane',
          lastName: 'Smith',
          fullName: 'Jane Smith',
          email: 'jane.smith@company.com',
          designation: 'Development Manager',
          department: 'Development'
        }
      ],
      attendance: [
        {
          id: 1,
          employeeName: 'John Doe',
          employeeId: 'EMP001',
          date: '2025-01-08',
          checkInTime: '09:00:00',
          checkOutTime: '17:30:00',
          status: 'Present'
        }
      ],
      projects: [
        {
          id: 1,
          name: 'StrideHR Platform',
          description: 'HR Management System',
          projectCode: 'SHR-001',
          status: 'Active',
          startDate: '2024-01-01',
          endDate: '2025-12-31'
        }
      ],
      leaves: [
        {
          id: 1,
          employeeName: 'John Doe',
          leaveType: 'Annual',
          startDate: '2025-01-15',
          endDate: '2025-01-17',
          status: 'Approved',
          reason: 'Family vacation'
        }
      ],
      reports: [
        {
          id: 1,
          name: 'Monthly Attendance Report',
          description: 'Attendance summary for the month',
          type: 'Attendance',
          createdDate: '2025-01-01'
        }
      ]
    };

    const data = mockDataMap[searchId] || [];
    
    // Filter mock data based on search term
    if (searchTerm) {
      return data.filter(item => 
        Object.values(item).some(value => 
          value && value.toString().toLowerCase().includes(searchTerm.toLowerCase())
        )
      );
    }
    
    return data;
  }

  /**
   * Clear search results
   */
  clearSearchResults(searchId: string): void {
    const resultsSubject = this.searchResults.get(searchId);
    if (resultsSubject) {
      resultsSubject.next({
        results: [],
        totalCount: 0,
        hasMore: false,
        searchTerm: ''
      });
    }
  }

  /**
   * Update search configuration
   */
  updateSearchConfig(searchId: string, updates: Partial<SearchConfig>): void {
    const existing = this.searchConfigs.get(searchId);
    if (existing) {
      this.searchConfigs.set(searchId, { ...existing, ...updates });
    }
  }

  /**
   * Get search configuration
   */
  getSearchConfig(searchId: string): SearchConfig | undefined {
    return this.searchConfigs.get(searchId);
  }

  /**
   * Validate all search configurations
   */
  validateAllSearches(): Observable<SearchValidationResult[]> {
    const results: SearchValidationResult[] = [];
    const validationPromises: Promise<void>[] = [];

    this.searchConfigs.forEach((config, searchId) => {
      const promise = new Promise<void>((resolve) => {
        // Test search endpoint connectivity
        const testUrl = `${this.API_URL}${config.endpoint}`;
        const testParams = new HttpParams().set('searchTerm', 'test').set('maxResults', '1');

        this.http.get(testUrl, { params: testParams }).pipe(
          map(() => true),
          catchError(() => of(false))
        ).subscribe(canConnect => {
          results.push({
            searchId,
            isWorking: canConnect,
            hasEndpoint: !!config.endpoint,
            canConnect,
            error: !canConnect ? `Cannot connect to search endpoint: ${config.endpoint}` : undefined,
            suggestions: !canConnect ? [
              'Check API endpoint URL',
              'Verify server is running',
              'Check network connectivity',
              'Ensure proper authentication'
            ] : undefined
          });
          resolve();
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
   * Perform advanced search with multiple criteria
   */
  advancedSearch(searchId: string, criteria: { [key: string]: any }): Observable<SearchResponse> {
    const config = this.searchConfigs.get(searchId);
    if (!config) {
      return of({
        results: [],
        totalCount: 0,
        hasMore: false,
        searchTerm: ''
      });
    }

    let params = new HttpParams();
    
    // Add all criteria as parameters
    Object.keys(criteria).forEach(key => {
      if (criteria[key] !== null && criteria[key] !== undefined && criteria[key] !== '') {
        params = params.set(key, criteria[key].toString());
      }
    });

    const url = `${this.API_URL}${config.endpoint}/advanced`;

    return this.http.get<any>(url, { params }).pipe(
      map(response => this.processSearchResponse(response, config, JSON.stringify(criteria))),
      catchError(error => {
        console.warn(`Advanced search failed for ${searchId}:`, error);
        return of({
          results: [],
          totalCount: 0,
          hasMore: false,
          searchTerm: JSON.stringify(criteria)
        });
      })
    );
  }

  /**
   * Get search suggestions based on partial input
   */
  getSearchSuggestions(searchId: string, partialTerm: string): Observable<string[]> {
    const config = this.searchConfigs.get(searchId);
    if (!config || partialTerm.length < 2) {
      return of([]);
    }

    const url = `${this.API_URL}${config.endpoint}/suggestions`;
    const params = new HttpParams()
      .set('term', partialTerm)
      .set('maxSuggestions', '10');

    return this.http.get<any>(url, { params }).pipe(
      map(response => response.suggestions || response.data || []),
      catchError(() => of([]))
    );
  }
}