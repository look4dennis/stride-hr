import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Role, EmployeeRole } from '../models/employee.models';
import { ApiResponse } from '../models/api.models';

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

  getRoleById(id: number): Observable<Role> {
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

  createRole(role: Partial<Role>): Observable<Role> {
    return this.http.post<ApiResponse<Role>>(`${this.API_URL}/roles`, role)
      .pipe(
        map(response => response.data!),
        catchError(() => {
          // Mock successful creation
          const mockRole: Role = {
            id: Math.floor(Math.random() * 1000) + 100,
            name: role.name!,
            description: role.description!,
            permissions: role.permissions || []
          };
          return of(mockRole);
        })
      );
  }

  updateRole(id: number, role: Partial<Role>): Observable<Role> {
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

  private getMockRoles(): Role[] {
    return [
      {
        id: 1,
        name: 'Administrator',
        description: 'Full system access',
        permissions: ['*']
      },
      {
        id: 2,
        name: 'HR Manager',
        description: 'Human Resources management',
        permissions: ['employees.read', 'employees.write', 'payroll.read']
      },
      {
        id: 3,
        name: 'Employee',
        description: 'Basic employee access',
        permissions: ['profile.read', 'attendance.write']
      },
      {
        id: 4,
        name: 'Manager',
        description: 'Team management access',
        permissions: ['employees.read', 'reports.read', 'attendance.read']
      }
    ];
  }
}