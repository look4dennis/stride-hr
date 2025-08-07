import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { BaseApiService, ApiResponse } from '../core/services/base-api.service';
import { 
  Employee, 
  CreateEmployeeDto, 
  UpdateEmployeeDto, 
  EmployeeSearchCriteria, 
  PagedResult,
  EmployeeOnboarding,
  EmployeeExitProcess,
  OrganizationalChart
} from '../models/employee.models';

@Injectable({
  providedIn: 'root'
})
export class EnhancedEmployeeService extends BaseApiService<Employee, CreateEmployeeDto, UpdateEmployeeDto> {
  protected readonly endpoint = 'employees';

  // Override getAll to handle search criteria
  getEmployees(criteria?: EmployeeSearchCriteria): Observable<PagedResult<Employee>> {
    const params = this.buildSearchParams(criteria);
    return this.getAll(params).pipe(
      map(response => this.extractPagedResult(response))
    );
  }

  // Employee-specific operations
  uploadProfilePhoto(employeeId: number, photo: File): Observable<{ photoUrl: string }> {
    const operationKey = `${this.endpoint}-uploadPhoto-${employeeId}`;
    return this.executeWithRetry(
      () => {
        const formData = new FormData();
        formData.append('photo', photo);
        return this.http.post<ApiResponse<{ photoUrl: string }>>(`${this.baseUrl}/${this.endpoint}/${employeeId}/photo`, formData);
      },
      operationKey
    ).pipe(
      map(response => response.data!),
      tap(() => this.showSuccess('Profile photo uploaded successfully'))
    );
  }

  // Organizational Chart
  getOrganizationalChart(branchId?: number): Observable<OrganizationalChart[]> {
    const operationKey = `${this.endpoint}-orgChart`;
    const params = branchId ? { branchId } : undefined;
    
    return this.executeWithRetry(
      () => {
        const httpParams = this.buildHttpParams(params);
        return this.http.get<ApiResponse<OrganizationalChart[]>>(`${this.baseUrl}/${this.endpoint}/org-chart`, { params: httpParams });
      },
      operationKey
    ).pipe(
      map(response => response.data || [])
    );
  }

  // Onboarding Management
  getEmployeeOnboarding(employeeId: number): Observable<EmployeeOnboarding> {
    const operationKey = `${this.endpoint}-onboarding-${employeeId}`;
    return this.executeWithRetry(
      () => this.http.get<ApiResponse<EmployeeOnboarding>>(`${this.baseUrl}/${this.endpoint}/${employeeId}/onboarding`),
      operationKey
    ).pipe(
      map(response => response.data!)
    );
  }

  updateOnboardingStep(employeeId: number, stepId: string, completed: boolean): Observable<EmployeeOnboarding> {
    const operationKey = `${this.endpoint}-onboardingStep-${employeeId}-${stepId}`;
    return this.executeWithRetry(
      () => this.http.put<ApiResponse<EmployeeOnboarding>>(`${this.baseUrl}/${this.endpoint}/${employeeId}/onboarding/steps/${stepId}`, { completed }),
      operationKey
    ).pipe(
      map(response => response.data!),
      tap(() => this.showSuccess(`Onboarding step ${completed ? 'completed' : 'updated'} successfully`))
    );
  }

  // Exit Process Management
  initiateExitProcess(employeeId: number, exitData: Partial<EmployeeExitProcess>): Observable<EmployeeExitProcess> {
    const operationKey = `${this.endpoint}-initiateExit-${employeeId}`;
    return this.executeWithRetry(
      () => this.http.post<ApiResponse<EmployeeExitProcess>>(`${this.baseUrl}/${this.endpoint}/${employeeId}/exit`, exitData),
      operationKey
    ).pipe(
      map(response => response.data!),
      tap(() => this.showSuccess('Exit process initiated successfully'))
    );
  }

  getEmployeeExitProcess(employeeId: number): Observable<EmployeeExitProcess> {
    const operationKey = `${this.endpoint}-exitProcess-${employeeId}`;
    return this.executeWithRetry(
      () => this.http.get<ApiResponse<EmployeeExitProcess>>(`${this.baseUrl}/${this.endpoint}/${employeeId}/exit`),
      operationKey
    ).pipe(
      map(response => response.data!)
    );
  }

  updateExitProcess(employeeId: number, exitData: Partial<EmployeeExitProcess>): Observable<EmployeeExitProcess> {
    const operationKey = `${this.endpoint}-updateExit-${employeeId}`;
    return this.executeWithRetry(
      () => this.http.put<ApiResponse<EmployeeExitProcess>>(`${this.baseUrl}/${this.endpoint}/${employeeId}/exit`, exitData),
      operationKey
    ).pipe(
      map(response => response.data!),
      tap(() => this.showSuccess('Exit process updated successfully'))
    );
  }

  // Utility methods
  getDepartments(): Observable<string[]> {
    const operationKey = `${this.endpoint}-departments`;
    return this.executeWithRetry(
      () => this.http.get<ApiResponse<string[]>>(`${this.baseUrl}/${this.endpoint}/departments`),
      operationKey
    ).pipe(
      map(response => response.data || [])
    );
  }

  getDesignations(): Observable<string[]> {
    const operationKey = `${this.endpoint}-designations`;
    return this.executeWithRetry(
      () => this.http.get<ApiResponse<string[]>>(`${this.baseUrl}/${this.endpoint}/designations`),
      operationKey
    ).pipe(
      map(response => response.data || [])
    );
  }

  getManagers(branchId?: number): Observable<Employee[]> {
    const operationKey = `${this.endpoint}-managers`;
    const params = branchId ? { branchId } : undefined;
    
    return this.executeWithRetry(
      () => {
        const httpParams = this.buildHttpParams(params);
        return this.http.get<ApiResponse<Employee[]>>(`${this.baseUrl}/${this.endpoint}/managers`, { params: httpParams });
      },
      operationKey
    ).pipe(
      map(response => response.data || [])
    );
  }

  // Override create to show success message
  override create(employee: CreateEmployeeDto): Observable<ApiResponse<Employee>> {
    return super.create(employee).pipe(
      tap(() => this.showSuccess('Employee created successfully'))
    );
  }

  // Override update to show success message
  override update(id: number | string, employee: UpdateEmployeeDto): Observable<ApiResponse<Employee>> {
    return super.update(id, employee).pipe(
      tap(() => this.showSuccess('Employee updated successfully'))
    );
  }

  // Override delete to show success message
  override delete(id: number | string): Observable<ApiResponse<boolean>> {
    return super.delete(id).pipe(
      tap(() => this.showSuccess('Employee deactivated successfully'))
    );
  }

  // Soft delete (deactivate)
  deactivateEmployee(id: number): Observable<boolean> {
    return this.delete(id).pipe(
      map(response => response.data || false)
    );
  }

  // Private helper methods
  private buildSearchParams(criteria?: EmployeeSearchCriteria): any {
    if (!criteria) return undefined;

    const params: any = {};
    
    if (criteria.searchTerm) params.searchTerm = criteria.searchTerm;
    if (criteria.department) params.department = criteria.department;
    if (criteria.designation) params.designation = criteria.designation;
    if (criteria.branchId) params.branchId = criteria.branchId;
    if (criteria.status) params.status = criteria.status;
    if (criteria.reportingManagerId) params.reportingManagerId = criteria.reportingManagerId;
    if (criteria.page) params.page = criteria.page;
    if (criteria.pageSize) params.pageSize = criteria.pageSize;
    if (criteria.sortBy) params.sortBy = criteria.sortBy;
    if (criteria.sortDirection) params.sortDirection = criteria.sortDirection;

    return params;
  }

  private extractPagedResult(response: ApiResponse<Employee[]>): PagedResult<Employee> {
    return {
      items: response.data || [],
      totalCount: response.pagination?.totalCount || 0,
      page: response.pagination?.currentPage || 1,
      pageSize: response.pagination?.pageSize || 20,
      totalPages: response.pagination?.totalPages || 1
    };
  }

  // Mock data fallback for development (when API is not available)
  getMockEmployees(): PagedResult<Employee> {
    const mockEmployees: Employee[] = [
      {
        id: 1,
        employeeId: 'EMP001',
        branchId: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@company.com',
        phone: '+1-555-0101',
        profilePhoto: '/assets/images/avatars/john-doe.jpg',
        dateOfBirth: '1990-05-15',
        joiningDate: '2020-01-15',
        designation: 'Senior Developer',
        department: 'Development',
        basicSalary: 75000,
        status: 'Active' as any,
        reportingManagerId: 2,
        createdAt: '2020-01-15T00:00:00Z'
      },
      {
        id: 2,
        employeeId: 'EMP002',
        branchId: 1,
        firstName: 'Jane',
        lastName: 'Smith',
        email: 'jane.smith@company.com',
        phone: '+1-555-0102',
        profilePhoto: '/assets/images/avatars/jane-smith.jpg',
        dateOfBirth: '1985-08-22',
        joiningDate: '2018-03-10',
        designation: 'Development Manager',
        department: 'Development',
        basicSalary: 95000,
        status: 'Active' as any,
        createdAt: '2018-03-10T00:00:00Z'
      },
      {
        id: 3,
        employeeId: 'EMP003',
        branchId: 1,
        firstName: 'Mike',
        lastName: 'Johnson',
        email: 'mike.johnson@company.com',
        phone: '+1-555-0103',
        profilePhoto: '/assets/images/avatars/mike-johnson.jpg',
        dateOfBirth: '1992-12-03',
        joiningDate: '2021-06-01',
        designation: 'Junior Developer',
        department: 'Development',
        basicSalary: 55000,
        status: 'Active' as any,
        reportingManagerId: 2,
        createdAt: '2021-06-01T00:00:00Z'
      },
      {
        id: 4,
        employeeId: 'EMP004',
        branchId: 1,
        firstName: 'Sarah',
        lastName: 'Wilson',
        email: 'sarah.wilson@company.com',
        phone: '+1-555-0104',
        profilePhoto: '/assets/images/avatars/sarah-wilson.jpg',
        dateOfBirth: '1988-04-18',
        joiningDate: '2019-09-15',
        designation: 'HR Manager',
        department: 'Human Resources',
        basicSalary: 85000,
        status: 'Active' as any,
        createdAt: '2019-09-15T00:00:00Z'
      }
    ];

    return {
      items: mockEmployees,
      totalCount: mockEmployees.length,
      page: 1,
      pageSize: 10,
      totalPages: 1
    };
  }
}