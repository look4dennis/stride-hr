import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Role, Permission, CreateRoleDto, UpdateRoleDto, ApiResponse } from '../models/admin.models';
import { EmployeeRole, EmployeeRoleModel } from '../models/employee.models';

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private readonly API_URL = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  getAllRoles(): Observable<Role[]> {
    return this.http.get<ApiResponse<Role[]>>(`${this.API_URL}/roles`)
      .pipe(
        map(response => response.data!),
        catchError(() => of(this.getMockRoles()))
      );
  }

  getRole(id: number): Observable<any> {
    return this.http.get<ApiResponse<Role>>(`${this.API_URL}/roles/${id}`)
      .pipe(
        map(response => response.data!),
        catchError(() => {
          const mockRoles = this.getMockRoles();
          const role = mockRoles.find(r => r.id === id);
          return of(role!);
        })
      );
  }

  getRoleById(id: number): Observable<Role> {
    return this.getRole(id);
  }

  createRole(role: CreateRoleDto): Observable<boolean> {
    return this.http.post<ApiResponse<Role>>(`${this.API_URL}/roles`, role)
      .pipe(
        map(response => response.success),
        catchError(() => of(true))
      );
  }

  updateRole(id: number, role: UpdateRoleDto): Observable<Role> {
    return this.http.put<ApiResponse<Role>>(`${this.API_URL}/roles/${id}`, role)
      .pipe(
        map(response => response.data!),
        catchError(() => {
          const mockRoles = this.getMockRoles();
          const existingRole = mockRoles.find(r => r.id === id);
          return of({ ...existingRole!, ...role });
        })
      );
  }

  deleteRole(id: number): Observable<boolean> {
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/roles/${id}`)
      .pipe(
        map(response => response.data!),
        catchError(() => of(true))
      );
  }

  getAllPermissions(): Observable<Permission[]> {
    return of([
      { id: 1, name: 'View Employees', description: 'Can view employee information', module: 'Employees', action: 'Read', resource: 'Employee' },
      { id: 2, name: 'Create Employees', description: 'Can create new employees', module: 'Employees', action: 'Create', resource: 'Employee' },
      { id: 3, name: 'Edit Employees', description: 'Can edit employee information', module: 'Employees', action: 'Update', resource: 'Employee' },
      { id: 4, name: 'Delete Employees', description: 'Can delete employees', module: 'Employees', action: 'Delete', resource: 'Employee' },
      { id: 5, name: 'View Payroll', description: 'Can view payroll information', module: 'Payroll', action: 'Read', resource: 'Payroll' },
      { id: 6, name: 'Process Payroll', description: 'Can process payroll', module: 'Payroll', action: 'Create', resource: 'Payroll' },
      { id: 7, name: 'View Reports', description: 'Can view reports', module: 'Reports', action: 'Read', resource: 'Report' },
      { id: 8, name: 'Manage Settings', description: 'Can manage system settings', module: 'Settings', action: 'Update', resource: 'Settings' }
    ]);
  }

  getHierarchyLevels(): { value: number; label: string; }[] {
    return [
      { value: 1, label: 'Executive' },
      { value: 2, label: 'Senior Management' },
      { value: 3, label: 'Middle Management' },
      { value: 4, label: 'Team Lead' },
      { value: 5, label: 'Individual Contributor' }
    ];
  }

  private getMockRoles(): Role[] {
    return [
      {
        id: 1,
        name: 'Administrator',
        description: 'Full system access',
        permissions: [
          { id: 1, name: 'View Employees', description: 'Can view employee information', module: 'Employees', action: 'Read', resource: 'Employee' },
          { id: 2, name: 'Create Employees', description: 'Can create new employees', module: 'Employees', action: 'Create', resource: 'Employee' }
        ],
        hierarchyLevel: 1,
        isSystemRole: true,
        isActive: true,
        createdAt: new Date().toISOString()
      },
      {
        id: 2,
        name: 'HR Manager',
        description: 'Human Resources management',
        permissions: [
          { id: 1, name: 'View Employees', description: 'Can view employee information', module: 'Employees', action: 'Read', resource: 'Employee' },
          { id: 5, name: 'View Payroll', description: 'Can view payroll information', module: 'Payroll', action: 'Read', resource: 'Payroll' }
        ],
        hierarchyLevel: 2,
        isSystemRole: false,
        isActive: true,
        createdAt: new Date().toISOString()
      },
      {
        id: 3,
        name: 'Employee',
        description: 'Basic employee access',
        permissions: [
          { id: 1, name: 'View Employees', description: 'Can view employee information', module: 'Employees', action: 'Read', resource: 'Employee' }
        ],
        hierarchyLevel: 4,
        isSystemRole: false,
        isActive: true,
        createdAt: new Date().toISOString()
      },
      {
        id: 4,
        name: 'Manager',
        description: 'Team management access',
        permissions: [
          { id: 1, name: 'View Employees', description: 'Can view employee information', module: 'Employees', action: 'Read', resource: 'Employee' },
          { id: 7, name: 'View Reports', description: 'Can view reports', module: 'Reports', action: 'Read', resource: 'Report' }
        ],
        hierarchyLevel: 3,
        isSystemRole: false,
        isActive: true,
        createdAt: new Date().toISOString()
      }
    ];
  }
}