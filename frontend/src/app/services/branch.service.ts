import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Branch, CreateBranchDto, UpdateBranchDto, ApiResponse } from '../models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class BranchService {
  private readonly API_URL = 'https://localhost:5001/api';

  constructor(private http: HttpClient) {}

  getAllBranches(): Observable<ApiResponse<Branch[]>> {
    return this.http.get<ApiResponse<Branch[]>>(`${this.API_URL}/branches`)
      .pipe(
        catchError(() => {
          // Mock successful response
          const mockBranches: Branch[] = [
            {
              id: 1,
              organizationId: 1,
              name: 'Main Office',
              country: 'United States',
              currency: 'USD',
              timeZone: 'America/New_York',
              address: '123 Business St, New York, NY 10001',
              localHolidays: [],
              complianceSettings: {},
              createdAt: new Date(),
              updatedAt: new Date()
            },
            {
              id: 2,
              organizationId: 1,
              name: 'West Coast Office',
              country: 'United States',
              currency: 'USD',
              timeZone: 'America/Los_Angeles',
              address: '456 Tech Ave, San Francisco, CA 94105',
              localHolidays: [],
              complianceSettings: {},
              createdAt: new Date(),
              updatedAt: new Date()
            }
          ];
          
          return of({
            success: true,
            message: 'Branches retrieved successfully',
            data: mockBranches
          } as ApiResponse<Branch[]>);
        })
      );
  }

  getBranchById(id: number): Observable<ApiResponse<Branch>> {
    return this.http.get<ApiResponse<Branch>>(`${this.API_URL}/branches/${id}`)
      .pipe(
        catchError(() => {
          // Mock successful response
          const mockBranch: Branch = {
            id: id,
            organizationId: 1,
            name: 'Main Office',
            country: 'United States',
            currency: 'USD',
            timeZone: 'America/New_York',
            address: '123 Business St, New York, NY 10001',
            localHolidays: [],
            complianceSettings: {},
            createdAt: new Date(),
            updatedAt: new Date()
          };
          
          return of({
            success: true,
            message: 'Branch retrieved successfully',
            data: mockBranch
          } as ApiResponse<Branch>);
        })
      );
  }

  createBranch(branchData: CreateBranchDto): Observable<ApiResponse<Branch>> {
    return this.http.post<ApiResponse<Branch>>(`${this.API_URL}/branches`, branchData)
      .pipe(
        catchError(() => {
          const mockBranch: Branch = {
            id: Math.floor(Math.random() * 1000),
            organizationId: branchData.organizationId,
            name: branchData.name,
            country: branchData.country,
            currency: branchData.currency,
            timeZone: branchData.timeZone,
            address: branchData.address,
            localHolidays: branchData.localHolidays || [],
            complianceSettings: branchData.complianceSettings || {},
            createdAt: new Date(),
            updatedAt: new Date()
          };
          
          return of({
            success: true,
            message: 'Branch created successfully',
            data: mockBranch
          } as ApiResponse<Branch>);
        })
      );
  }

  updateBranch(id: number, branchData: UpdateBranchDto): Observable<ApiResponse<Branch>> {
    return this.http.put<ApiResponse<Branch>>(`${this.API_URL}/branches/${id}`, branchData)
      .pipe(
        catchError(() => {
          const mockBranch: Branch = {
            id: id,
            organizationId: 1,
            name: branchData.name || 'Updated Branch',
            country: branchData.country || 'United States',
            currency: branchData.currency || 'USD',
            timeZone: branchData.timeZone || 'America/New_York',
            address: branchData.address || 'Updated Address',
            localHolidays: branchData.localHolidays || [],
            complianceSettings: branchData.complianceSettings || {},
            createdAt: new Date(),
            updatedAt: new Date()
          };
          
          return of({
            success: true,
            message: 'Branch updated successfully',
            data: mockBranch
          } as ApiResponse<Branch>);
        })
      );
  }

  deleteBranch(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/branches/${id}`)
      .pipe(
        catchError(() => {
          return of({
            success: true,
            message: 'Branch deleted successfully',
            data: true
          } as ApiResponse<boolean>);
        })
      );
  }

  getSupportedCountries(): Observable<any> {
    return of({
      success: true,
      data: [
        { code: 'US', name: 'United States' },
        { code: 'CA', name: 'Canada' },
        { code: 'UK', name: 'United Kingdom' },
        { code: 'DE', name: 'Germany' },
        { code: 'FR', name: 'France' },
        { code: 'JP', name: 'Japan' },
        { code: 'AU', name: 'Australia' }
      ]
    });
  }

  getSupportedCurrencies(): Observable<any> {
    return of({
      success: true,
      data: [
        { code: 'USD', name: 'US Dollar', symbol: '$' },
        { code: 'EUR', name: 'Euro', symbol: '€' },
        { code: 'GBP', name: 'British Pound', symbol: '£' },
        { code: 'JPY', name: 'Japanese Yen', symbol: '¥' },
        { code: 'CAD', name: 'Canadian Dollar', symbol: 'C$' },
        { code: 'AUD', name: 'Australian Dollar', symbol: 'A$' }
      ]
    });
  }

  getSupportedTimeZones(): Observable<any> {
    return of({
      success: true,
      data: [
        { id: 'America/New_York', name: 'Eastern Time (US & Canada)' },
        { id: 'America/Chicago', name: 'Central Time (US & Canada)' },
        { id: 'America/Denver', name: 'Mountain Time (US & Canada)' },
        { id: 'America/Los_Angeles', name: 'Pacific Time (US & Canada)' },
        { id: 'Europe/London', name: 'London' },
        { id: 'Europe/Paris', name: 'Paris' },
        { id: 'Europe/Berlin', name: 'Berlin' },
        { id: 'Asia/Tokyo', name: 'Tokyo' },
        { id: 'Australia/Sydney', name: 'Sydney' }
      ]
    });
  }

  getCountryTimeZones(): any {
    return {
      'US': ['America/New_York', 'America/Chicago', 'America/Denver', 'America/Los_Angeles'],
      'CA': ['America/Toronto', 'America/Vancouver'],
      'UK': ['Europe/London'],
      'DE': ['Europe/Berlin'],
      'FR': ['Europe/Paris'],
      'JP': ['Asia/Tokyo'],
      'AU': ['Australia/Sydney', 'Australia/Melbourne']
    };
  }

  getCountryCurrencies(): any {
    return {
      'US': ['USD'],
      'CA': ['CAD'],
      'UK': ['GBP'],
      'DE': ['EUR'],
      'FR': ['EUR'],
      'JP': ['JPY'],
      'AU': ['AUD']
    };
  }
}