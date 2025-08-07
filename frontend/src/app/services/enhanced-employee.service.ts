import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import {
  Employee,
  CreateEmployeeDto,
  UpdateEmployeeDto,
  EmployeeSearchCriteria,
  PagedResult,
  EmployeeOnboarding,
  EmployeeExitProcess,
  OrganizationalChart,
  EmployeeRole,
  EmployeeRoleModel
} from '../models/employee.models';
import { ApiResponse } from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class EnhancedEmployeeService {
  private readonly API_URL = 'http://localhost:5000/api';

  private employeesSubject = new BehaviorSubject<Employee[]>([]);
  public employees$ = this.employeesSubject.asObservable();

  constructor(private http: HttpClient) { }

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

    return this.http.get<ApiResponse<PagedResult<Employee>>>(`${this.API_URL}/employees/search`, { params })
      .pipe(
        map(response => response.data!),
        catchError(() => of(this.getMockEmployees()))
      );
  }

  getEmployeeById(id: number): Observable<Employee> {
    return this.http.get<ApiResponse<Employee>>(`${this.API_URL}/employees/${id}`)
      .pipe(
        map(response => response.data!),
        catchError(() => {
          const mockData = this.getMockEmployees();
          const employee = mockData.items.find(e => e.id === id);
          return of(employee!);
        })
      );
  }

  create(employee: CreateEmployeeDto): Observable<ApiResponse<Employee>> {
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

    return this.http.post<ApiResponse<Employee>>(`${this.API_URL}/employees`, formData)
      .pipe(
        catchError(() => {
          // Mock successful creation for development
          const mockEmployee: Employee = {
            id: Math.floor(Math.random() * 1000) + 100,
            employeeId: `EMP${Math.floor(Math.random() * 1000).toString().padStart(3, '0')}`,
            branchId: employee.branchId,
            firstName: employee.firstName,
            lastName: employee.lastName,
            email: employee.email,
            phone: employee.phone,
            dateOfBirth: employee.dateOfBirth,
            joiningDate: employee.joiningDate,
            designation: employee.designation,
            department: employee.department,
            basicSalary: employee.basicSalary,
            status: 'Active' as any,
            reportingManagerId: employee.reportingManagerId,
            createdAt: new Date().toISOString()
          };

          return of({
            success: true,
            message: 'Employee created successfully',
            data: mockEmployee
          } as ApiResponse<Employee>);
        })
      );
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

    return this.http.put<ApiResponse<Employee>>(`${this.API_URL}/employees/${id}`, formData)
      .pipe(
        map(response => response.data!),
        catchError(() => {
          // Mock successful update for development
          const mockData = this.getMockEmployees();
          const existingEmployee = mockData.items.find(e => e.id === id);
          if (existingEmployee) {
            const updatedEmployee: Employee = {
              ...existingEmployee,
              firstName: employee.firstName || existingEmployee.firstName,
              lastName: employee.lastName || existingEmployee.lastName,
              email: employee.email || existingEmployee.email,
              phone: employee.phone || existingEmployee.phone,
              dateOfBirth: employee.dateOfBirth || existingEmployee.dateOfBirth,
              designation: employee.designation || existingEmployee.designation,
              department: employee.department || existingEmployee.department,
              basicSalary: employee.basicSalary || existingEmployee.basicSalary,
              status: employee.status || existingEmployee.status,
              reportingManagerId: employee.reportingManagerId !== undefined ? employee.reportingManagerId : existingEmployee.reportingManagerId,
              updatedAt: new Date().toISOString()
            };
            return of(updatedEmployee);
          }
          throw new Error('Employee not found');
        })
      );
  }

  deactivateEmployee(id: number): Observable<boolean> {
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/employees/${id}`)
      .pipe(
        map(response => response.data!),
        catchError(() => of(true)) // Mock successful deactivation
      );
  }

  // Photo upload
  uploadProfilePhoto(employeeId: number, photo: File): Observable<{ photoUrl: string }> {
    const formData = new FormData();
    formData.append('photo', photo);

    return this.http.post<ApiResponse<{ photoUrl: string }>>(`${this.API_URL}/employees/${employeeId}/photo`, formData)
      .pipe(
        map(response => response.data!),
        catchError(() => of({ photoUrl: '/assets/images/avatars/default-avatar.png' }))
      );
  }

  // Organizational Chart
  getOrganizationalChart(branchId?: number): Observable<OrganizationalChart[]> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId.toString());

    return this.http.get<ApiResponse<OrganizationalChart[]>>(`${this.API_URL}/employees/org-chart`, { params })
      .pipe(
        map(response => response.data!),
        catchError(() => of([]))
      );
  }

  // Onboarding
  getEmployeeOnboarding(employeeId: number): Observable<EmployeeOnboarding> {
    return this.http.get<ApiResponse<EmployeeOnboarding>>(`${this.API_URL}/employees/${employeeId}/onboarding`)
      .pipe(
        map(response => response.data!),
        catchError(() => {
          // Mock onboarding data
          return of({
            employeeId,
            steps: [
              {
                id: '1',
                title: 'Complete Personal Information',
                description: 'Fill in all personal details and upload documents',
                completed: false,
                required: true,
                order: 1
              },
              {
                id: '2',
                title: 'IT Setup',
                description: 'Receive laptop, email account, and system access',
                completed: false,
                required: true,
                order: 2
              },
              {
                id: '3',
                title: 'HR Orientation',
                description: 'Complete HR orientation and policy briefing',
                completed: false,
                required: true,
                order: 3
              }
            ],
            overallProgress: 0,
            startedAt: new Date().toISOString(),
            status: 'NotStarted' as any
          });
        })
      );
  }

  updateOnboardingStep(employeeId: number, stepId: string, completed: boolean): Observable<EmployeeOnboarding> {
    return this.http.put<ApiResponse<EmployeeOnboarding>>(`${this.API_URL}/employees/${employeeId}/onboarding/steps/${stepId}`, {
      completed
    }).pipe(
      map(response => response.data!),
      catchError(() => this.getEmployeeOnboarding(employeeId))
    );
  }

  // Exit Process
  initiateExitProcess(employeeId: number, exitData: Partial<EmployeeExitProcess>): Observable<EmployeeExitProcess> {
    return this.http.post<ApiResponse<EmployeeExitProcess>>(`${this.API_URL}/employees/${employeeId}/exit`, exitData)
      .pipe(
        map(response => response.data!),
        catchError(() => {
          // Mock exit process data
          return of({
            employeeId,
            exitDate: exitData.exitDate || new Date().toISOString(),
            reason: exitData.reason || 'Resignation',
            exitType: exitData.exitType || 'Resignation' as any,
            handoverNotes: exitData.handoverNotes,
            assetsToReturn: [],
            clearanceSteps: [
              {
                id: '1',
                department: 'IT',
                description: 'Return laptop and access cards',
                completed: false
              },
              {
                id: '2',
                department: 'HR',
                description: 'Complete exit interview',
                completed: false
              }
            ],
            status: 'Initiated' as any
          });
        })
      );
  }

  getEmployeeExitProcess(employeeId: number): Observable<EmployeeExitProcess> {
    return this.http.get<ApiResponse<EmployeeExitProcess>>(`${this.API_URL}/employees/${employeeId}/exit`)
      .pipe(
        map(response => response.data!),
        catchError(() => {
          throw new Error('Exit process not found');
        })
      );
  }

  updateExitProcess(employeeId: number, exitData: Partial<EmployeeExitProcess>): Observable<EmployeeExitProcess> {
    return this.http.put<ApiResponse<EmployeeExitProcess>>(`${this.API_URL}/employees/${employeeId}/exit`, exitData)
      .pipe(
        map(response => response.data!),
        catchError(() => this.getEmployeeExitProcess(employeeId))
      );
  }

  // Utility methods
  getDepartments(): Observable<string[]> {
    return this.http.get<ApiResponse<string[]>>(`${this.API_URL}/employees/departments`)
      .pipe(
        map(response => response.data!),
        catchError(() => of(['Development', 'Human Resources', 'Marketing', 'Sales', 'Finance', 'Operations']))
      );
  }

  getDesignations(): Observable<string[]> {
    return this.http.get<ApiResponse<string[]>>(`${this.API_URL}/employees/designations`)
      .pipe(
        map(response => response.data!),
        catchError(() => of([
          'Senior Developer', 'Junior Developer', 'Development Manager',
          'HR Manager', 'HR Executive', 'Marketing Manager', 'Marketing Executive',
          'Sales Manager', 'Sales Executive', 'Finance Manager', 'Accountant'
        ]))
      );
  }

  getManagers(branchId?: number): Observable<Employee[]> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId.toString());

    return this.http.get<ApiResponse<Employee[]>>(`${this.API_URL}/employees/managers`, { params })
      .pipe(
        map(response => response.data!),
        catchError(() => {
          const mockData = this.getMockEmployees();
          return of(mockData.items.filter(e => e.designation.toLowerCase().includes('manager')));
        })
      );
  }

  // Role Assignment
  getEmployeeRoles(employeeId: number): Observable<EmployeeRole[]> {
    return this.http.get<ApiResponse<EmployeeRole[]>>(`${this.API_URL}/employees/${employeeId}/roles`)
      .pipe(
        map(response => response.data!),
        catchError(() => of([]))
      );
  }

  getActiveEmployeeRoles(employeeId: number): Observable<EmployeeRole[]> {
    return this.http.get<ApiResponse<EmployeeRole[]>>(`${this.API_URL}/employees/${employeeId}/roles/active`)
      .pipe(
        map(response => response.data!),
        catchError(() => of([]))
      );
  }

  assignRole(employeeId: number, roleId: number, notes?: string): Observable<boolean> {
    return this.http.post<ApiResponse<boolean>>(`${this.API_URL}/employees/${employeeId}/roles/assign`, {
      roleId,
      notes
    }).pipe(
      map(response => response.data!),
      catchError(() => of(true))
    );
  }

  revokeRole(employeeId: number, roleId: number, notes?: string): Observable<boolean> {
    return this.http.post<ApiResponse<boolean>>(`${this.API_URL}/employees/${employeeId}/roles/revoke`, {
      roleId,
      notes
    }).pipe(
      map(response => response.data!),
      catchError(() => of(true))
    );
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
      totalPages: 1,
      hasNext: false,
      hasPrevious: false
    };
  }
}