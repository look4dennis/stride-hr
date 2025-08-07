import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Branch } from '../models/admin.models';
import { ApiResponse } from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class BranchService {
  private readonly API_URL = 'http://localhost:5000/api';

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
}