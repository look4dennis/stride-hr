import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Role,
  Permission,
  CreateRoleDto,
  UpdateRoleDto,
  EmployeeRole,
  AssignRoleDto,
  ApiResponse
} from '../models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private readonly apiUrl = `${environment.apiUrl}/role`;

  constructor(private http: HttpClient) {}

  // Role CRUD Operations
  getAllRoles(): Observable<{ success: boolean; data: { roles: Role[] } }> {
    return this.http.get<{ success: boolean; data: { roles: Role[] } }>(this.apiUrl);
  }

  getRole(id: number): Observable<{ success: boolean; data: { role: Role } }> {
    return this.http.get<{ success: boolean; data: { role: Role } }>(`${this.apiUrl}/${id}`);
  }

  createRole(dto: CreateRoleDto): Observable<{ success: boolean; message: string; data: { role: Role } }> {
    return this.http.post<{ success: boolean; message: string; data: { role: Role } }>(this.apiUrl, dto);
  }

  updateRole(id: number, dto: UpdateRoleDto): Observable<{ success: boolean; message: string }> {
    return this.http.put<{ success: boolean; message: string }>(`${this.apiUrl}/${id}`, dto);
  }

  deleteRole(id: number): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(`${this.apiUrl}/${id}`);
  }

  // Role Assignment
  assignRole(roleId: number, dto: AssignRoleDto): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.apiUrl}/${roleId}/assign`, dto);
  }

  removeRole(roleId: number, employeeId: number): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(`${this.apiUrl}/${roleId}/remove/${employeeId}`);
  }

  getEmployeeRoles(employeeId: number): Observable<{ success: boolean; data: { roles: EmployeeRole[] } }> {
    return this.http.get<{ success: boolean; data: { roles: EmployeeRole[] } }>(`${this.apiUrl}/employee/${employeeId}`);
  }

  // Permission Management
  getAllPermissions(): Observable<Permission[]> {
    // This would typically be a separate endpoint, but for now we'll use a mock
    return new Observable(observer => {
      observer.next(this.getMockPermissions());
      observer.complete();
    });
  }

  checkPermission(permission: string): Observable<{ success: boolean; data: { hasPermission: boolean; permission: string } }> {
    return this.http.get<{ success: boolean; data: { hasPermission: boolean; permission: string } }>(`${this.apiUrl}/check-permission/${permission}`);
  }

  // Utility Methods
  validateRoleData(dto: CreateRoleDto | UpdateRoleDto): string[] {
    const errors: string[] = [];

    if (!dto.name?.trim()) {
      errors.push('Role name is required');
    }

    if (!dto.description?.trim()) {
      errors.push('Role description is required');
    }

    if (dto.hierarchyLevel < 1 || dto.hierarchyLevel > 10) {
      errors.push('Hierarchy level must be between 1 and 10');
    }

    if (!dto.permissionIds || dto.permissionIds.length === 0) {
      errors.push('At least one permission must be selected');
    }

    return errors;
  }

  getHierarchyLevels(): { value: number; label: string }[] {
    return [
      { value: 1, label: 'Level 1 - Executive' },
      { value: 2, label: 'Level 2 - Senior Management' },
      { value: 3, label: 'Level 3 - Middle Management' },
      { value: 4, label: 'Level 4 - Team Lead' },
      { value: 5, label: 'Level 5 - Senior Staff' },
      { value: 6, label: 'Level 6 - Staff' },
      { value: 7, label: 'Level 7 - Junior Staff' },
      { value: 8, label: 'Level 8 - Intern' },
      { value: 9, label: 'Level 9 - Contractor' },
      { value: 10, label: 'Level 10 - Temporary' }
    ];
  }

  private getMockPermissions(): Permission[] {
    return [
      // Employee Management
      { id: 1, name: 'Employee.View', module: 'Employee', action: 'View', resource: 'Employee', description: 'View employee information' },
      { id: 2, name: 'Employee.Create', module: 'Employee', action: 'Create', resource: 'Employee', description: 'Create new employees' },
      { id: 3, name: 'Employee.Update', module: 'Employee', action: 'Update', resource: 'Employee', description: 'Update employee information' },
      { id: 4, name: 'Employee.Delete', module: 'Employee', action: 'Delete', resource: 'Employee', description: 'Delete employees' },
      
      // Attendance Management
      { id: 5, name: 'Attendance.View', module: 'Attendance', action: 'View', resource: 'Attendance', description: 'View attendance records' },
      { id: 6, name: 'Attendance.Manage', module: 'Attendance', action: 'Manage', resource: 'Attendance', description: 'Manage attendance records' },
      { id: 7, name: 'Attendance.Correct', module: 'Attendance', action: 'Correct', resource: 'Attendance', description: 'Correct attendance records' },
      
      // Payroll Management
      { id: 8, name: 'Payroll.View', module: 'Payroll', action: 'View', resource: 'Payroll', description: 'View payroll information' },
      { id: 9, name: 'Payroll.Process', module: 'Payroll', action: 'Process', resource: 'Payroll', description: 'Process payroll' },
      { id: 10, name: 'Payroll.Approve', module: 'Payroll', action: 'Approve', resource: 'Payroll', description: 'Approve payroll' },
      
      // Leave Management
      { id: 11, name: 'Leave.View', module: 'Leave', action: 'View', resource: 'Leave', description: 'View leave requests' },
      { id: 12, name: 'Leave.Approve', module: 'Leave', action: 'Approve', resource: 'Leave', description: 'Approve leave requests' },
      { id: 13, name: 'Leave.Manage', module: 'Leave', action: 'Manage', resource: 'Leave', description: 'Manage leave policies' },
      
      // Project Management
      { id: 14, name: 'Project.View', module: 'Project', action: 'View', resource: 'Project', description: 'View projects' },
      { id: 15, name: 'Project.Create', module: 'Project', action: 'Create', resource: 'Project', description: 'Create projects' },
      { id: 16, name: 'Project.Manage', module: 'Project', action: 'Manage', resource: 'Project', description: 'Manage projects' },
      
      // Role Management
      { id: 17, name: 'Role.View', module: 'Role', action: 'View', resource: 'Role', description: 'View roles' },
      { id: 18, name: 'Role.Create', module: 'Role', action: 'Create', resource: 'Role', description: 'Create roles' },
      { id: 19, name: 'Role.Update', module: 'Role', action: 'Update', resource: 'Role', description: 'Update roles' },
      { id: 20, name: 'Role.Delete', module: 'Role', action: 'Delete', resource: 'Role', description: 'Delete roles' },
      { id: 21, name: 'Role.Assign', module: 'Role', action: 'Assign', resource: 'Role', description: 'Assign roles to employees' },
      
      // System Administration
      { id: 22, name: 'System.Configure', module: 'System', action: 'Configure', resource: 'System', description: 'Configure system settings' },
      { id: 23, name: 'System.ViewLogs', module: 'System', action: 'ViewLogs', resource: 'System', description: 'View system logs' },
      { id: 24, name: 'System.ManageUsers', module: 'System', action: 'ManageUsers', resource: 'System', description: 'Manage system users' },
      
      // Reports
      { id: 25, name: 'Reports.View', module: 'Reports', action: 'View', resource: 'Reports', description: 'View reports' },
      { id: 26, name: 'Reports.Create', module: 'Reports', action: 'Create', resource: 'Reports', description: 'Create custom reports' },
      { id: 27, name: 'Reports.Export', module: 'Reports', action: 'Export', resource: 'Reports', description: 'Export reports' }
    ];
  }
}