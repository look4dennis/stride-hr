import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { BaseApiService, ApiResponse } from '../core/services/base-api.service';

export interface Organization {
  id: number;
  name: string;
  address: string;
  email: string;
  phone: string;
  logo?: string;
  website?: string;
  taxId?: string;
  registrationNumber?: string;
  normalWorkingHours: string;
  overtimeRate: number;
  productiveHoursThreshold: number;
  branchIsolationEnabled: boolean;
  configurationSettings: OrganizationConfig;
  createdAt: string;
  updatedAt?: string;
}

export interface OrganizationConfig {
  allowOvertime: boolean;
  requireLocationForAttendance: boolean;
  autoBreakDeduction: boolean;
  maxDailyWorkingHours: number;
  minDailyWorkingHours: number;
  weeklyOffDays: string[];
  publicHolidays: string[];
  leaveTypes: LeaveType[];
}

export interface LeaveType {
  id: number;
  name: string;
  maxDays: number;
  carryForward: boolean;
  requireApproval: boolean;
}

export interface CreateOrganizationDto {
  name: string;
  address: string;
  email: string;
  phone: string;
  logo?: File;
  website?: string;
  taxId?: string;
  registrationNumber?: string;
  normalWorkingHours: string;
  overtimeRate: number;
  productiveHoursThreshold: number;
  branchIsolationEnabled: boolean;
  configurationSettings: Partial<OrganizationConfig>;
}

export interface UpdateOrganizationDto extends Partial<CreateOrganizationDto> {}

@Injectable({
  providedIn: 'root'
})
export class EnhancedOrganizationService extends BaseApiService<Organization, CreateOrganizationDto, UpdateOrganizationDto> {
  protected readonly endpoint = 'organizations';

  // Get current organization (assuming single-tenant for now)
  getCurrentOrganization(): Observable<Organization> {
    const operationKey = `${this.endpoint}-current`;
    return this.executeWithRetry(
      () => this.http.get<ApiResponse<Organization>>(`${this.baseUrl}/${this.endpoint}/current`),
      operationKey
    ).pipe(
      map(response => response.data!)
    );
  }

  // Update organization logo
  updateLogo(organizationId: number, logo: File): Observable<{ logoUrl: string }> {
    const operationKey = `${this.endpoint}-updateLogo-${organizationId}`;
    return this.executeWithRetry(
      () => {
        const formData = new FormData();
        formData.append('logo', logo);
        return this.http.post<ApiResponse<{ logoUrl: string }>>(`${this.baseUrl}/${this.endpoint}/${organizationId}/logo`, formData);
      },
      operationKey
    ).pipe(
      map(response => response.data!),
      tap(() => this.showSuccess('Organization logo updated successfully'))
    );
  }

  // Update organization configuration
  updateConfiguration(organizationId: number, config: Partial<OrganizationConfig>): Observable<Organization> {
    const operationKey = `${this.endpoint}-updateConfig-${organizationId}`;
    return this.executeWithRetry(
      () => this.http.put<ApiResponse<Organization>>(`${this.baseUrl}/${this.endpoint}/${organizationId}/configuration`, config),
      operationKey
    ).pipe(
      map(response => response.data!),
      tap(() => this.showSuccess('Organization configuration updated successfully'))
    );
  }

  // Get organization statistics
  getOrganizationStats(organizationId: number): Observable<OrganizationStats> {
    const operationKey = `${this.endpoint}-stats-${organizationId}`;
    return this.executeWithRetry(
      () => this.http.get<ApiResponse<OrganizationStats>>(`${this.baseUrl}/${this.endpoint}/${organizationId}/stats`),
      operationKey
    ).pipe(
      map(response => response.data!)
    );
  }

  // Validate organization settings
  validateSettings(settings: Partial<OrganizationConfig>): Observable<ValidationResult> {
    const operationKey = `${this.endpoint}-validateSettings`;
    return this.executeWithRetry(
      () => this.http.post<ApiResponse<ValidationResult>>(`${this.baseUrl}/${this.endpoint}/validate-settings`, settings),
      operationKey
    ).pipe(
      map(response => response.data!)
    );
  }

  // Override create to show success message
  override create(organization: CreateOrganizationDto): Observable<ApiResponse<Organization>> {
    return super.create(organization).pipe(
      tap(() => this.showSuccess('Organization created successfully'))
    );
  }

  // Override update to show success message
  override update(id: number | string, organization: UpdateOrganizationDto): Observable<ApiResponse<Organization>> {
    return super.update(id, organization).pipe(
      tap(() => this.showSuccess('Organization updated successfully'))
    );
  }

  // Mock data for development
  getMockOrganization(): Organization {
    return {
      id: 1,
      name: 'StrideHR Demo Company',
      address: '123 Business Street, Tech City, TC 12345',
      email: 'info@stridehr-demo.com',
      phone: '+1-555-0100',
      logo: '/assets/images/logo.png',
      website: 'https://stridehr-demo.com',
      taxId: 'TAX123456789',
      registrationNumber: 'REG987654321',
      normalWorkingHours: '8',
      overtimeRate: 1.5,
      productiveHoursThreshold: 6,
      branchIsolationEnabled: false,
      configurationSettings: {
        allowOvertime: true,
        requireLocationForAttendance: true,
        autoBreakDeduction: false,
        maxDailyWorkingHours: 12,
        minDailyWorkingHours: 4,
        weeklyOffDays: ['Saturday', 'Sunday'],
        publicHolidays: ['2025-01-01', '2025-07-04', '2025-12-25'],
        leaveTypes: [
          {
            id: 1,
            name: 'Annual Leave',
            maxDays: 25,
            carryForward: true,
            requireApproval: true
          },
          {
            id: 2,
            name: 'Sick Leave',
            maxDays: 10,
            carryForward: false,
            requireApproval: false
          },
          {
            id: 3,
            name: 'Personal Leave',
            maxDays: 5,
            carryForward: false,
            requireApproval: true
          }
        ]
      },
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2025-01-08T00:00:00Z'
    };
  }

  getMockOrganizationStats(): OrganizationStats {
    return {
      totalEmployees: 150,
      activeEmployees: 142,
      totalBranches: 3,
      totalDepartments: 8,
      averageAttendanceRate: 94.5,
      totalPayrollAmount: 1250000,
      pendingLeaveRequests: 12,
      upcomingBirthdays: 5,
      recentHires: 8,
      pendingExits: 2
    };
  }
}

export interface OrganizationStats {
  totalEmployees: number;
  activeEmployees: number;
  totalBranches: number;
  totalDepartments: number;
  averageAttendanceRate: number;
  totalPayrollAmount: number;
  pendingLeaveRequests: number;
  upcomingBirthdays: number;
  recentHires: number;
  pendingExits: number;
}

export interface ValidationResult {
  isValid: boolean;
  errors: string[];
  warnings: string[];
}