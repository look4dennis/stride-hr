import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Branch,
  CreateBranchDto,
  UpdateBranchDto,
  BranchCompliance,
  LocalHoliday,
  SupportedCountry,
  SupportedCurrency,
  CurrencyConversion,
  ApiResponse
} from '../models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class BranchService {
  private readonly apiUrl = `${environment.apiUrl}/branch`;

  constructor(private http: HttpClient) {}

  // Branch CRUD Operations
  getAllBranches(): Observable<ApiResponse<Branch[]>> {
    return this.http.get<ApiResponse<Branch[]>>(this.apiUrl);
  }

  getBranch(id: number): Observable<ApiResponse<Branch>> {
    return this.http.get<ApiResponse<Branch>>(`${this.apiUrl}/${id}`);
  }

  getBranchesByOrganization(organizationId: number): Observable<ApiResponse<Branch[]>> {
    return this.http.get<ApiResponse<Branch[]>>(`${this.apiUrl}/organization/${organizationId}`);
  }

  createBranch(dto: CreateBranchDto): Observable<ApiResponse<Branch>> {
    return this.http.post<ApiResponse<Branch>>(this.apiUrl, dto);
  }

  updateBranch(id: number, dto: UpdateBranchDto): Observable<ApiResponse<Branch>> {
    return this.http.put<ApiResponse<Branch>>(`${this.apiUrl}/${id}`, dto);
  }

  deleteBranch(id: number): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  // Compliance Management
  getComplianceSettings(branchId: number): Observable<ApiResponse<BranchCompliance>> {
    return this.http.get<ApiResponse<BranchCompliance>>(`${this.apiUrl}/${branchId}/compliance`);
  }

  updateComplianceSettings(branchId: number, compliance: BranchCompliance): Observable<ApiResponse<void>> {
    return this.http.put<ApiResponse<void>>(`${this.apiUrl}/${branchId}/compliance`, compliance);
  }

  // Holiday Management
  getLocalHolidays(branchId: number): Observable<ApiResponse<LocalHoliday[]>> {
    return this.http.get<ApiResponse<LocalHoliday[]>>(`${this.apiUrl}/${branchId}/holidays`);
  }

  updateLocalHolidays(branchId: number, holidays: LocalHoliday[]): Observable<ApiResponse<void>> {
    return this.http.put<ApiResponse<void>>(`${this.apiUrl}/${branchId}/holidays`, holidays);
  }

  // Reference Data
  getSupportedCountries(): Observable<ApiResponse<string[]>> {
    return this.http.get<ApiResponse<string[]>>(`${this.apiUrl}/supported-countries`);
  }

  getSupportedCurrencies(): Observable<ApiResponse<string[]>> {
    return this.http.get<ApiResponse<string[]>>(`${this.apiUrl}/supported-currencies`);
  }

  getSupportedTimeZones(): Observable<ApiResponse<string[]>> {
    return this.http.get<ApiResponse<string[]>>(`${this.apiUrl}/supported-timezones`);
  }

  // Currency Conversion
  convertCurrency(amount: number, fromCurrency: string, toCurrency: string): Observable<ApiResponse<number>> {
    const request = {
      amount,
      fromCurrency,
      toCurrency
    };
    return this.http.post<ApiResponse<number>>(`${this.apiUrl}/convert-currency`, request);
  }

  // Utility Methods
  validateBranchData(dto: CreateBranchDto | UpdateBranchDto): string[] {
    const errors: string[] = [];

    if (!dto.name?.trim()) {
      errors.push('Branch name is required');
    }

    if (!dto.country?.trim()) {
      errors.push('Country is required');
    }

    if (!dto.currency?.trim()) {
      errors.push('Currency is required');
    }

    if (!dto.timeZone?.trim()) {
      errors.push('Time zone is required');
    }

    if (!dto.address?.trim()) {
      errors.push('Address is required');
    }

    return errors;
  }

  getCountryTimeZones(): Record<string, string[]> {
    return {
      'United States': ['America/New_York', 'America/Chicago', 'America/Denver', 'America/Los_Angeles'],
      'United Kingdom': ['Europe/London'],
      'India': ['Asia/Kolkata'],
      'Australia': ['Australia/Sydney', 'Australia/Melbourne', 'Australia/Perth'],
      'Canada': ['America/Toronto', 'America/Vancouver', 'America/Montreal'],
      'Germany': ['Europe/Berlin'],
      'France': ['Europe/Paris'],
      'Japan': ['Asia/Tokyo'],
      'Singapore': ['Asia/Singapore'],
      'UAE': ['Asia/Dubai']
    };
  }

  getCountryCurrencies(): Record<string, string> {
    return {
      'United States': 'USD',
      'United Kingdom': 'GBP',
      'India': 'INR',
      'Australia': 'AUD',
      'Canada': 'CAD',
      'Germany': 'EUR',
      'France': 'EUR',
      'Japan': 'JPY',
      'Singapore': 'SGD',
      'UAE': 'AED'
    };
  }
}