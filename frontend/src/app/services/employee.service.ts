import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { 
  Employee, 
  CreateEmployeeDto, 
  UpdateEmployeeDto, 
  EmployeeSearchCriteria, 
  PagedResult,
  EmployeeOnboarding,
  EmployeeExitProcess,
  OrganizationalChart,
  EmployeeRole
} from '../models/employee.models';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private readonly API_URL = 'http://localhost:5000/api';
  
  private employeesSubject = new BehaviorSubject<Employee[]>([]);
  public employees$ = this.employeesSubject.asObservable();

  constructor(private http: HttpClient) {}

  // Employee CRUD Operations
  getEmployees(criteria?: EmployeeSearchCriteria): Observable<PagedResult<Employee>> {
    let params = new HttpParams();
    
    if (criteria) {
      if (criteria.searchTerm) params = params.set('searchTerm', criteria.searchTerm);
      if (criteria.department) params = params.set('department', criteria.department);
      if (criteria.designation) params = params.set('designation', criteria.designation);
      if (criteria.branchId) params = params.set('branchId', criteria.branchId.toString());
      if (criteria.status) params = params.set('status', criteria.status);
      if (criteria.reportingManagerId) params = params.set('reportingManagerId', criteria.reportingManagerId.toString());
      if (criteria.page) params = params.set('page', criteria.page.toString());
      if (criteria.pageSize) params = params.set('pageSize', criteria.pageSize.toString());
      if (criteria.sortBy) params = params.set('sortBy', criteria.sortBy);
      if (criteria.sortDirection) params = params.set('sortDirection', criteria.sortDirection);
    }

    return this.http.get<PagedResult<Employee>>(`${this.API_URL}/employees`, { params });
  }

  getEmployeeById(id: number): Observable<Employee> {
    return this.http.get<Employee>(`${this.API_URL}/employees/${id}`);
  }

  createEmployee(employee: CreateEmployeeDto): Observable<Employee> {
    const formData = new FormData();
    
    // Add all employee data to FormData
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

    return this.http.post<Employee>(`${this.API_URL}/employees`, formData);
  }

  updateEmployee(id: number, employee: UpdateEmployeeDto): Observable<Employee> {
    const formData = new FormData();
    
    // Add all employee data to FormData
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

    return this.http.put<Employee>(`${this.API_URL}/employees/${id}`, formData);
  }

  deactivateEmployee(id: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.API_URL}/employees/${id}`);
  }

  // Photo upload
  uploadProfilePhoto(employeeId: number, photo: File): Observable<{ photoUrl: string }> {
    const formData = new FormData();
    formData.append('photo', photo);
    
    return this.http.post<{ photoUrl: string }>(`${this.API_URL}/employees/${employeeId}/photo`, formData);
  }

  // Organizational Chart
  getOrganizationalChart(branchId?: number): Observable<OrganizationalChart[]> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId.toString());
    
    return this.http.get<OrganizationalChart[]>(`${this.API_URL}/employees/org-chart`, { params });
  }

  // Onboarding
  getEmployeeOnboarding(employeeId: number): Observable<EmployeeOnboarding> {
    return this.http.get<EmployeeOnboarding>(`${this.API_URL}/employees/${employeeId}/onboarding`);
  }

  updateOnboardingStep(employeeId: number, stepId: string, completed: boolean): Observable<EmployeeOnboarding> {
    return this.http.put<EmployeeOnboarding>(`${this.API_URL}/employees/${employeeId}/onboarding/steps/${stepId}`, {
      completed
    });
  }

  // Exit Process
  initiateExitProcess(employeeId: number, exitData: Partial<EmployeeExitProcess>): Observable<EmployeeExitProcess> {
    return this.http.post<EmployeeExitProcess>(`${this.API_URL}/employees/${employeeId}/exit`, exitData);
  }

  getEmployeeExitProcess(employeeId: number): Observable<EmployeeExitProcess> {
    return this.http.get<EmployeeExitProcess>(`${this.API_URL}/employees/${employeeId}/exit`);
  }

  updateExitProcess(employeeId: number, exitData: Partial<EmployeeExitProcess>): Observable<EmployeeExitProcess> {
    return this.http.put<EmployeeExitProcess>(`${this.API_URL}/employees/${employeeId}/exit`, exitData);
  }

  // Utility methods
  getDepartments(): Observable<string[]> {
    return this.http.get<string[]>(`${this.API_URL}/employees/departments`);
  }

  getDesignations(): Observable<string[]> {
    return this.http.get<string[]>(`${this.API_URL}/employees/designations`);
  }

  getManagers(branchId?: number): Observable<Employee[]> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId.toString());
    
    return this.http.get<Employee[]>(`${this.API_URL}/employees/managers`, { params });
  }

  // Role Assignment
  getEmployeeRoles(employeeId: number): Observable<EmployeeRole[]> {
    return this.http.get<EmployeeRole[]>(`${this.API_URL}/employees/${employeeId}/roles`);
  }

  getActiveEmployeeRoles(employeeId: number): Observable<EmployeeRole[]> {
    return this.http.get<EmployeeRole[]>(`${this.API_URL}/employees/${employeeId}/roles/active`);
  }

  assignRole(employeeId: number, roleId: number, notes?: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.API_URL}/employees/${employeeId}/roles/assign`, {
      roleId,
      notes
    });
  }

  revokeRole(employeeId: number, roleId: number, notes?: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.API_URL}/employees/${employeeId}/roles/revoke`, {
      roleId,
      notes
    });
  }

  // Mock data for development
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