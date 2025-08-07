import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Organization,
  CreateOrganizationDto,
  UpdateOrganizationDto,
  OrganizationConfiguration,
  ApiResponse,
  FileUploadResponse
} from '../models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class OrganizationService {
  private readonly apiUrl = `${environment.apiUrl}/organization`;

  constructor(private http: HttpClient) {}

  // Organization CRUD Operations
  getAllOrganizations(): Observable<ApiResponse<Organization[]>> {
    return this.http.get<ApiResponse<Organization[]>>(this.apiUrl);
  }

  getOrganization(id: number): Observable<ApiResponse<Organization>> {
    return this.http.get<ApiResponse<Organization>>(`${this.apiUrl}/${id}`);
  }

  createOrganization(dto: CreateOrganizationDto): Observable<ApiResponse<Organization>> {
    return this.http.post<ApiResponse<Organization>>(this.apiUrl, dto);
  }

  updateOrganization(id: number, dto: UpdateOrganizationDto): Observable<ApiResponse<Organization>> {
    return this.http.put<ApiResponse<Organization>>(`${this.apiUrl}/${id}`, dto);
  }

  deleteOrganization(id: number): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  // Logo Management
  uploadLogo(organizationId: number, file: File): Observable<ApiResponse<string>> {
    const formData = new FormData();
    formData.append('file', file);
    
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/${organizationId}/logo`, formData);
  }

  getLogo(organizationId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${organizationId}/logo`, { responseType: 'blob' });
  }

  deleteLogo(organizationId: number): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${organizationId}/logo`);
  }

  // Configuration Management
  getConfiguration(organizationId: number): Observable<ApiResponse<OrganizationConfiguration>> {
    return this.http.get<ApiResponse<OrganizationConfiguration>>(`${this.apiUrl}/${organizationId}/configuration`);
  }

  updateConfiguration(organizationId: number, configuration: Record<string, any>): Observable<ApiResponse<void>> {
    return this.http.put<ApiResponse<void>>(`${this.apiUrl}/${organizationId}/configuration`, configuration);
  }

  // Utility Methods
  validateOrganizationData(dto: CreateOrganizationDto | UpdateOrganizationDto): string[] {
    const errors: string[] = [];

    if (!dto.name?.trim()) {
      errors.push('Organization name is required');
    }

    if (!dto.email?.trim()) {
      errors.push('Email is required');
    } else if (!this.isValidEmail(dto.email)) {
      errors.push('Invalid email format');
    }

    if (!dto.phone?.trim()) {
      errors.push('Phone number is required');
    }

    if (!dto.address?.trim()) {
      errors.push('Address is required');
    }

    if (dto.overtimeRate !== undefined && dto.overtimeRate < 0) {
      errors.push('Overtime rate cannot be negative');
    }

    if (dto.productiveHoursThreshold !== undefined && dto.productiveHoursThreshold < 0) {
      errors.push('Productive hours threshold cannot be negative');
    }

    return errors;
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }
}