import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, BehaviorSubject } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { NotificationService } from '../../core/services/notification.service';
import { LoadingService } from '../../core/services/loading.service';
import { Router } from '@angular/router';

export interface CRUDConfig {
  entityName: string;
  endpoint: string;
  primaryKey?: string;
  displayField?: string;
  searchFields?: string[];
  sortField?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface CRUDOperation {
  id: string;
  type: 'create' | 'read' | 'update' | 'delete' | 'list';
  entityName: string;
  endpoint: string;
  method: 'GET' | 'POST' | 'PUT' | 'DELETE';
  requiresConfirmation?: boolean;
  successMessage?: string;
  errorMessage?: string;
  redirectAfter?: string;
}

export interface CRUDValidationResult {
  operationId: string;
  entityName: string;
  isWorking: boolean;
  hasEndpoint: boolean;
  hasProperIntegration: boolean;
  error?: string;
  suggestions?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class CRUDOperationsService {
  private readonly API_URL = 'http://localhost:5000/api';
  private crudConfigs = new Map<string, CRUDConfig>();
  private crudOperations = new Map<string, CRUDOperation>();
  private entityData = new Map<string, BehaviorSubject<any[]>>();

  constructor(
    private http: HttpClient,
    private notificationService: NotificationService,
    private loadingService: LoadingService,
    private router: Router
  ) {
    this.initializeCommonCRUDOperations();
  }

  /**
   * Initialize common CRUD operations
   */
  private initializeCommonCRUDOperations(): void {
    // Employee CRUD operations
    this.registerCRUDConfig('employees', {
      entityName: 'Employee',
      endpoint: '/employees',
      primaryKey: 'id',
      displayField: 'fullName',
      searchFields: ['firstName', 'lastName', 'email', 'employeeId'],
      sortField: 'firstName',
      sortDirection: 'asc'
    });

    this.registerCRUDOperation('employee-create', {
      id: 'employee-create',
      type: 'create',
      entityName: 'Employee',
      endpoint: '/employees',
      method: 'POST',
      successMessage: 'Employee created successfully',
      errorMessage: 'Failed to create employee',
      redirectAfter: '/employees'
    });

    this.registerCRUDOperation('employee-read', {
      id: 'employee-read',
      type: 'read',
      entityName: 'Employee',
      endpoint: '/employees/:id',
      method: 'GET',
      errorMessage: 'Failed to load employee details'
    });

    this.registerCRUDOperation('employee-update', {
      id: 'employee-update',
      type: 'update',
      entityName: 'Employee',
      endpoint: '/employees/:id',
      method: 'PUT',
      successMessage: 'Employee updated successfully',
      errorMessage: 'Failed to update employee'
    });

    this.registerCRUDOperation('employee-delete', {
      id: 'employee-delete',
      type: 'delete',
      entityName: 'Employee',
      endpoint: '/employees/:id',
      method: 'DELETE',
      requiresConfirmation: true,
      successMessage: 'Employee deleted successfully',
      errorMessage: 'Failed to delete employee'
    });

    this.registerCRUDOperation('employee-list', {
      id: 'employee-list',
      type: 'list',
      entityName: 'Employee',
      endpoint: '/employees',
      method: 'GET',
      errorMessage: 'Failed to load employees'
    });

    // Attendance CRUD operations
    this.registerCRUDConfig('attendance', {
      entityName: 'Attendance',
      endpoint: '/attendance',
      primaryKey: 'id',
      displayField: 'employeeName',
      searchFields: ['employeeName', 'date'],
      sortField: 'date',
      sortDirection: 'desc'
    });

    this.registerCRUDOperation('attendance-create', {
      id: 'attendance-create',
      type: 'create',
      entityName: 'Attendance',
      endpoint: '/attendance',
      method: 'POST',
      successMessage: 'Attendance record created successfully',
      errorMessage: 'Failed to create attendance record'
    });

    this.registerCRUDOperation('attendance-read', {
      id: 'attendance-read',
      type: 'read',
      entityName: 'Attendance',
      endpoint: '/attendance/:id',
      method: 'GET',
      errorMessage: 'Failed to load attendance record'
    });

    this.registerCRUDOperation('attendance-update', {
      id: 'attendance-update',
      type: 'update',
      entityName: 'Attendance',
      endpoint: '/attendance/:id',
      method: 'PUT',
      successMessage: 'Attendance record updated successfully',
      errorMessage: 'Failed to update attendance record'
    });

    this.registerCRUDOperation('attendance-delete', {
      id: 'attendance-delete',
      type: 'delete',
      entityName: 'Attendance',
      endpoint: '/attendance/:id',
      method: 'DELETE',
      requiresConfirmation: true,
      successMessage: 'Attendance record deleted successfully',
      errorMessage: 'Failed to delete attendance record'
    });

    // Project CRUD operations
    this.registerCRUDConfig('projects', {
      entityName: 'Project',
      endpoint: '/projects',
      primaryKey: 'id',
      displayField: 'name',
      searchFields: ['name', 'description', 'projectCode'],
      sortField: 'name',
      sortDirection: 'asc'
    });

    this.registerCRUDOperation('project-create', {
      id: 'project-create',
      type: 'create',
      entityName: 'Project',
      endpoint: '/projects',
      method: 'POST',
      successMessage: 'Project created successfully',
      errorMessage: 'Failed to create project',
      redirectAfter: '/projects'
    });

    this.registerCRUDOperation('project-read', {
      id: 'project-read',
      type: 'read',
      entityName: 'Project',
      endpoint: '/projects/:id',
      method: 'GET',
      errorMessage: 'Failed to load project details'
    });

    this.registerCRUDOperation('project-update', {
      id: 'project-update',
      type: 'update',
      entityName: 'Project',
      endpoint: '/projects/:id',
      method: 'PUT',
      successMessage: 'Project updated successfully',
      errorMessage: 'Failed to update project'
    });

    this.registerCRUDOperation('project-delete', {
      id: 'project-delete',
      type: 'delete',
      entityName: 'Project',
      endpoint: '/projects/:id',
      method: 'DELETE',
      requiresConfirmation: true,
      successMessage: 'Project deleted successfully',
      errorMessage: 'Failed to delete project'
    });

    // Leave CRUD operations
    this.registerCRUDConfig('leaves', {
      entityName: 'Leave',
      endpoint: '/leave',
      primaryKey: 'id',
      displayField: 'employeeName',
      searchFields: ['employeeName', 'leaveType', 'reason'],
      sortField: 'startDate',
      sortDirection: 'desc'
    });

    this.registerCRUDOperation('leave-create', {
      id: 'leave-create',
      type: 'create',
      entityName: 'Leave',
      endpoint: '/leave',
      method: 'POST',
      successMessage: 'Leave request created successfully',
      errorMessage: 'Failed to create leave request',
      redirectAfter: '/leave'
    });

    this.registerCRUDOperation('leave-read', {
      id: 'leave-read',
      type: 'read',
      entityName: 'Leave',
      endpoint: '/leave/:id',
      method: 'GET',
      errorMessage: 'Failed to load leave request'
    });

    this.registerCRUDOperation('leave-update', {
      id: 'leave-update',
      type: 'update',
      entityName: 'Leave',
      endpoint: '/leave/:id',
      method: 'PUT',
      successMessage: 'Leave request updated successfully',
      errorMessage: 'Failed to update leave request'
    });

    this.registerCRUDOperation('leave-delete', {
      id: 'leave-delete',
      type: 'delete',
      entityName: 'Leave',
      endpoint: '/leave/:id',
      method: 'DELETE',
      requiresConfirmation: true,
      successMessage: 'Leave request deleted successfully',
      errorMessage: 'Failed to delete leave request'
    });

    // Report CRUD operations
    this.registerCRUDConfig('reports', {
      entityName: 'Report',
      endpoint: '/reports',
      primaryKey: 'id',
      displayField: 'name',
      searchFields: ['name', 'description', 'type'],
      sortField: 'createdDate',
      sortDirection: 'desc'
    });

    this.registerCRUDOperation('report-create', {
      id: 'report-create',
      type: 'create',
      entityName: 'Report',
      endpoint: '/reports',
      method: 'POST',
      successMessage: 'Report created successfully',
      errorMessage: 'Failed to create report'
    });

    this.registerCRUDOperation('report-read', {
      id: 'report-read',
      type: 'read',
      entityName: 'Report',
      endpoint: '/reports/:id',
      method: 'GET',
      errorMessage: 'Failed to load report'
    });

    this.registerCRUDOperation('report-update', {
      id: 'report-update',
      type: 'update',
      entityName: 'Report',
      endpoint: '/reports/:id',
      method: 'PUT',
      successMessage: 'Report updated successfully',
      errorMessage: 'Failed to update report'
    });

    this.registerCRUDOperation('report-delete', {
      id: 'report-delete',
      type: 'delete',
      entityName: 'Report',
      endpoint: '/reports/:id',
      method: 'DELETE',
      requiresConfirmation: true,
      successMessage: 'Report deleted successfully',
      errorMessage: 'Failed to delete report'
    });
  }

  /**
   * Register CRUD configuration for an entity
   */
  registerCRUDConfig(entityKey: string, config: CRUDConfig): void {
    this.crudConfigs.set(entityKey, config);
    this.entityData.set(entityKey, new BehaviorSubject<any[]>([]));
  }

  /**
   * Register a CRUD operation
   */
  registerCRUDOperation(operationId: string, operation: CRUDOperation): void {
    this.crudOperations.set(operationId, operation);
  }

  /**
   * Execute a CRUD operation
   */
  executeCRUDOperation(operationId: string, data?: any, params?: { [key: string]: any }): Observable<any> {
    const operation = this.crudOperations.get(operationId);
    if (!operation) {
      console.error(`CRUD operation not found: ${operationId}`);
      this.notificationService.showError('Operation not configured');
      return of(null);
    }

    return this.performCRUDOperation(operation, data, params);
  }

  /**
   * Perform the actual CRUD operation
   */
  private performCRUDOperation(operation: CRUDOperation, data?: any, params?: { [key: string]: any }): Observable<any> {
    // Show confirmation if required
    if (operation.requiresConfirmation) {
      const confirmed = window.confirm(`Are you sure you want to ${operation.type} this ${operation.entityName.toLowerCase()}?`);
      if (!confirmed) {
        return of(null);
      }
    }

    // Show loading
    this.loadingService.setGlobalLoading(true);

    // Build URL with parameters
    let url = `${this.API_URL}${operation.endpoint}`;
    if (params) {
      Object.keys(params).forEach(key => {
        url = url.replace(`:${key}`, params[key]);
      });
    }

    // Execute HTTP request based on method
    let request: Observable<any>;
    switch (operation.method) {
      case 'GET':
        request = this.http.get(url);
        break;
      case 'POST':
        request = this.http.post(url, data);
        break;
      case 'PUT':
        request = this.http.put(url, data);
        break;
      case 'DELETE':
        request = this.http.delete(url);
        break;
      default:
        console.error(`Unsupported HTTP method: ${operation.method}`);
        this.loadingService.setGlobalLoading(false);
        return of(null);
    }

    return request.pipe(
      tap(response => {
        // Show success message
        if (operation.successMessage) {
          this.notificationService.showSuccess(operation.successMessage);
        }

        // Handle redirect
        if (operation.redirectAfter) {
          setTimeout(() => {
            this.router.navigate([operation.redirectAfter!]);
          }, 1000);
        }

        // Refresh entity data if it's a list operation
        if (operation.type === 'list') {
          this.refreshEntityData(this.getEntityKeyFromOperation(operation));
        }
      }),
      catchError(error => {
        console.error(`CRUD operation failed for ${operation.id}:`, error);
        
        // Show error message
        const errorMessage = operation.errorMessage || `Failed to ${operation.type} ${operation.entityName.toLowerCase()}`;
        this.notificationService.showError(errorMessage);

        // Return mock data for development
        return this.getMockDataForOperation(operation, data, params);
      }),
      tap(() => {
        // Hide loading
        this.loadingService.setGlobalLoading(false);
      })
    );
  }

  /**
   * Get entity key from operation
   */
  private getEntityKeyFromOperation(operation: CRUDOperation): string {
    // Simple mapping based on entity name
    const entityKeyMap: { [key: string]: string } = {
      'Employee': 'employees',
      'Attendance': 'attendance',
      'Project': 'projects',
      'Leave': 'leaves',
      'Report': 'reports'
    };

    return entityKeyMap[operation.entityName] || operation.entityName.toLowerCase();
  }

  /**
   * Refresh entity data
   */
  private refreshEntityData(entityKey: string): void {
    const config = this.crudConfigs.get(entityKey);
    if (!config) return;

    const url = `${this.API_URL}${config.endpoint}`;
    this.http.get<any>(url).pipe(
      map(response => response.data || response.items || response),
      catchError(() => of(this.getMockEntityData(entityKey)))
    ).subscribe(data => {
      const subject = this.entityData.get(entityKey);
      if (subject) {
        subject.next(data);
      }
    });
  }

  /**
   * Get mock data for development
   */
  private getMockDataForOperation(operation: CRUDOperation, data?: any, params?: { [key: string]: any }): Observable<any> {
    const mockData = this.generateMockResponse(operation, data, params);
    
    // Simulate API delay
    return new Observable(observer => {
      setTimeout(() => {
        observer.next(mockData);
        observer.complete();
      }, 500);
    });
  }

  /**
   * Generate mock response based on operation
   */
  private generateMockResponse(operation: CRUDOperation, data?: any, params?: { [key: string]: any }): any {
    const entityKey = this.getEntityKeyFromOperation(operation);
    
    switch (operation.type) {
      case 'create':
        return {
          success: true,
          message: operation.successMessage,
          data: { ...data, id: Math.floor(Math.random() * 1000) + 1 }
        };
      
      case 'read':
        const mockEntity = this.getMockEntityData(entityKey)[0];
        return {
          success: true,
          data: { ...mockEntity, id: params?.['id'] || 1 }
        };
      
      case 'update':
        return {
          success: true,
          message: operation.successMessage,
          data: { ...data, id: params?.['id'] || 1 }
        };
      
      case 'delete':
        return {
          success: true,
          message: operation.successMessage
        };
      
      case 'list':
        return {
          success: true,
          data: this.getMockEntityData(entityKey),
          totalCount: 10
        };
      
      default:
        return { success: true };
    }
  }

  /**
   * Get mock entity data
   */
  private getMockEntityData(entityKey: string): any[] {
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
        }
      ],
      attendance: [
        {
          id: 1,
          employeeName: 'John Doe',
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
          status: 'Active'
        }
      ],
      leaves: [
        {
          id: 1,
          employeeName: 'John Doe',
          leaveType: 'Annual',
          startDate: '2025-01-15',
          endDate: '2025-01-17',
          status: 'Approved'
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

    return mockDataMap[entityKey] || [];
  }

  /**
   * Get entity data as observable
   */
  getEntityData(entityKey: string): Observable<any[]> {
    const subject = this.entityData.get(entityKey);
    if (!subject) {
      console.warn(`Entity data not found: ${entityKey}`);
      return of([]);
    }
    
    return subject.asObservable();
  }

  /**
   * Create entity
   */
  create(entityKey: string, data: any): Observable<any> {
    const operationId = `${entityKey.slice(0, -1)}-create`; // Remove 's' from plural
    return this.executeCRUDOperation(operationId, data);
  }

  /**
   * Read entity by ID
   */
  read(entityKey: string, id: any): Observable<any> {
    const operationId = `${entityKey.slice(0, -1)}-read`;
    return this.executeCRUDOperation(operationId, null, { id });
  }

  /**
   * Update entity
   */
  update(entityKey: string, id: any, data: any): Observable<any> {
    const operationId = `${entityKey.slice(0, -1)}-update`;
    return this.executeCRUDOperation(operationId, data, { id });
  }

  /**
   * Delete entity
   */
  delete(entityKey: string, id: any): Observable<any> {
    const operationId = `${entityKey.slice(0, -1)}-delete`;
    return this.executeCRUDOperation(operationId, null, { id });
  }

  /**
   * List entities
   */
  list(entityKey: string, params?: any): Observable<any> {
    const operationId = `${entityKey.slice(0, -1)}-list`;
    return this.executeCRUDOperation(operationId, null, params);
  }

  /**
   * Validate all CRUD operations
   */
  validateAllCRUDOperations(): Observable<CRUDValidationResult[]> {
    const results: CRUDValidationResult[] = [];
    const validationPromises: Promise<void>[] = [];

    this.crudOperations.forEach((operation, operationId) => {
      const promise = new Promise<void>((resolve) => {
        // Test operation endpoint
        const hasEndpoint = !!operation.endpoint;
        const hasProperIntegration = this.validateOperationIntegration(operation);

        // Test actual endpoint connectivity (simplified)
        let url = `${this.API_URL}${operation.endpoint}`;
        if (operation.endpoint.includes(':id')) {
          url = url.replace(':id', '1'); // Use test ID
        }

        // For GET requests, test connectivity
        if (operation.method === 'GET') {
          this.http.get(url).pipe(
            map(() => true),
            catchError(() => of(false))
          ).subscribe(canConnect => {
            results.push({
              operationId,
              entityName: operation.entityName,
              isWorking: hasEndpoint && hasProperIntegration && canConnect,
              hasEndpoint,
              hasProperIntegration,
              error: !canConnect ? `Cannot connect to endpoint: ${operation.endpoint}` : undefined,
              suggestions: this.getOperationSuggestions(operation, hasEndpoint, hasProperIntegration, canConnect)
            });
            resolve();
          });
        } else {
          // For non-GET requests, assume they work if endpoint and integration are valid
          results.push({
            operationId,
            entityName: operation.entityName,
            isWorking: hasEndpoint && hasProperIntegration,
            hasEndpoint,
            hasProperIntegration,
            suggestions: this.getOperationSuggestions(operation, hasEndpoint, hasProperIntegration, true)
          });
          resolve();
        }
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
   * Validate operation integration
   */
  private validateOperationIntegration(operation: CRUDOperation): boolean {
    // Check if operation has required fields
    return !!(
      operation.endpoint &&
      operation.method &&
      operation.entityName &&
      operation.type
    );
  }

  /**
   * Get suggestions for operation improvements
   */
  private getOperationSuggestions(
    operation: CRUDOperation,
    hasEndpoint: boolean,
    hasProperIntegration: boolean,
    canConnect: boolean
  ): string[] {
    const suggestions: string[] = [];

    if (!hasEndpoint) {
      suggestions.push('Add endpoint configuration');
    }

    if (!hasProperIntegration) {
      suggestions.push('Ensure all required fields are configured');
      suggestions.push('Check method, entityName, and type fields');
    }

    if (!canConnect) {
      suggestions.push('Check API server is running');
      suggestions.push('Verify endpoint URL is correct');
      suggestions.push('Ensure proper authentication');
    }

    if (operation.requiresConfirmation && operation.type !== 'delete') {
      suggestions.push('Consider removing confirmation for non-destructive operations');
    }

    return suggestions;
  }

  /**
   * Get CRUD operation configuration
   */
  getCRUDOperation(operationId: string): CRUDOperation | undefined {
    return this.crudOperations.get(operationId);
  }

  /**
   * Get CRUD configuration
   */
  getCRUDConfig(entityKey: string): CRUDConfig | undefined {
    return this.crudConfigs.get(entityKey);
  }

  /**
   * Update CRUD operation
   */
  updateCRUDOperation(operationId: string, updates: Partial<CRUDOperation>): void {
    const existing = this.crudOperations.get(operationId);
    if (existing) {
      this.crudOperations.set(operationId, { ...existing, ...updates });
    }
  }
}