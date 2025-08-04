import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  PayrollRecord,
  PayrollBatch,
  PayrollFormula,
  PayslipTemplate,
  PayrollReport,
  CurrencyExchangeRate,
  CreatePayrollBatchDto,
  ProcessPayrollDto,
  ApprovePayrollDto,
  PayslipGenerationDto,
  PayrollCalculationDto,
  PayrollPeriod,
  PayrollStatus,
  PayrollBatchStatus
} from '../models/payroll.models';

@Injectable({
  providedIn: 'root'
})
export class PayrollService {
  private readonly apiUrl = `${environment.apiUrl}/payroll`;
  
  // Real-time updates
  private payrollUpdatesSubject = new BehaviorSubject<any>(null);
  public payrollUpdates$ = this.payrollUpdatesSubject.asObservable();

  constructor(private http: HttpClient) {}

  // Payroll Batch Management
  getPayrollBatches(branchId?: number, status?: PayrollBatchStatus): Observable<PayrollBatch[]> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId.toString());
    if (status) params = params.set('status', status);
    
    return this.http.get<PayrollBatch[]>(`${this.apiUrl}/batches`, { params });
  }

  getPayrollBatch(id: number): Observable<PayrollBatch> {
    return this.http.get<PayrollBatch>(`${this.apiUrl}/batches/${id}`);
  }

  createPayrollBatch(dto: CreatePayrollBatchDto): Observable<PayrollBatch> {
    return this.http.post<PayrollBatch>(`${this.apiUrl}/batches`, dto);
  }

  processPayroll(dto: ProcessPayrollDto): Observable<PayrollBatch> {
    return this.http.post<PayrollBatch>(`${this.apiUrl}/batches/${dto.batchId}/process`, dto);
  }

  approvePayroll(dto: ApprovePayrollDto): Observable<PayrollBatch> {
    return this.http.post<PayrollBatch>(`${this.apiUrl}/batches/${dto.batchId}/approve`, dto);
  }

  releasePayroll(batchId: number): Observable<PayrollBatch> {
    return this.http.post<PayrollBatch>(`${this.apiUrl}/batches/${batchId}/release`, {});
  }

  deletePayrollBatch(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/batches/${id}`);
  }

  // Payroll Records
  getPayrollRecords(batchId: number): Observable<PayrollRecord[]> {
    return this.http.get<PayrollRecord[]>(`${this.apiUrl}/batches/${batchId}/records`);
  }

  getPayrollRecord(id: number): Observable<PayrollRecord> {
    return this.http.get<PayrollRecord>(`${this.apiUrl}/records/${id}`);
  }

  calculatePayroll(dto: PayrollCalculationDto): Observable<PayrollRecord> {
    return this.http.post<PayrollRecord>(`${this.apiUrl}/calculate`, dto);
  }

  updatePayrollRecord(id: number, record: Partial<PayrollRecord>): Observable<PayrollRecord> {
    return this.http.put<PayrollRecord>(`${this.apiUrl}/records/${id}`, record);
  }

  // Payroll Formulas
  getPayrollFormulas(): Observable<PayrollFormula[]> {
    return this.http.get<PayrollFormula[]>(`${this.apiUrl}/formulas`);
  }

  getPayrollFormula(id: number): Observable<PayrollFormula> {
    return this.http.get<PayrollFormula>(`${this.apiUrl}/formulas/${id}`);
  }

  createPayrollFormula(formula: Omit<PayrollFormula, 'id' | 'createdAt' | 'updatedAt'>): Observable<PayrollFormula> {
    return this.http.post<PayrollFormula>(`${this.apiUrl}/formulas`, formula);
  }

  updatePayrollFormula(id: number, formula: Partial<PayrollFormula>): Observable<PayrollFormula> {
    return this.http.put<PayrollFormula>(`${this.apiUrl}/formulas/${id}`, formula);
  }

  deletePayrollFormula(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/formulas/${id}`);
  }

  testFormula(formula: string, testData: any): Observable<{ result: number; isValid: boolean; error?: string }> {
    return this.http.post<{ result: number; isValid: boolean; error?: string }>(`${this.apiUrl}/formulas/test`, {
      formula,
      testData
    });
  }

  // Payslip Templates
  getPayslipTemplates(branchId?: number): Observable<PayslipTemplate[]> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId.toString());
    
    return this.http.get<PayslipTemplate[]>(`${this.apiUrl}/templates`, { params });
  }

  getPayslipTemplate(id: number): Observable<PayslipTemplate> {
    return this.http.get<PayslipTemplate>(`${this.apiUrl}/templates/${id}`);
  }

  createPayslipTemplate(template: Omit<PayslipTemplate, 'id' | 'createdAt' | 'updatedAt'>): Observable<PayslipTemplate> {
    return this.http.post<PayslipTemplate>(`${this.apiUrl}/templates`, template);
  }

  updatePayslipTemplate(id: number, template: Partial<PayslipTemplate>): Observable<PayslipTemplate> {
    return this.http.put<PayslipTemplate>(`${this.apiUrl}/templates/${id}`, template);
  }

  deletePayslipTemplate(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/templates/${id}`);
  }

  // Payslip Generation
  generatePayslips(dto: PayslipGenerationDto): Observable<{ payslipUrls: string[]; batchId: string }> {
    return this.http.post<{ payslipUrls: string[]; batchId: string }>(`${this.apiUrl}/payslips/generate`, dto);
  }

  downloadPayslip(payrollRecordId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/payslips/${payrollRecordId}/download`, {
      responseType: 'blob'
    });
  }

  emailPayslips(payrollRecordIds: number[]): Observable<{ sent: number; failed: number }> {
    return this.http.post<{ sent: number; failed: number }>(`${this.apiUrl}/payslips/email`, {
      payrollRecordIds
    });
  }

  // Currency Management
  getCurrencyRates(): Observable<CurrencyExchangeRate[]> {
    return this.http.get<CurrencyExchangeRate[]>(`${this.apiUrl}/currency-rates`);
  }

  updateCurrencyRate(rate: Partial<CurrencyExchangeRate>): Observable<CurrencyExchangeRate> {
    return this.http.put<CurrencyExchangeRate>(`${this.apiUrl}/currency-rates/${rate.id}`, rate);
  }

  convertCurrency(amount: number, fromCurrency: string, toCurrency: string): Observable<{ convertedAmount: number; rate: number }> {
    return this.http.post<{ convertedAmount: number; rate: number }>(`${this.apiUrl}/currency/convert`, {
      amount,
      fromCurrency,
      toCurrency
    });
  }

  // Reports
  getPayrollReports(period?: PayrollPeriod, branchId?: number): Observable<PayrollReport[]> {
    let params = new HttpParams();
    if (period) {
      params = params.set('month', period.month.toString());
      params = params.set('year', period.year.toString());
    }
    if (branchId) params = params.set('branchId', branchId.toString());
    
    return this.http.get<PayrollReport[]>(`${this.apiUrl}/reports`, { params });
  }

  generatePayrollReport(type: string, period: PayrollPeriod, branchId?: number): Observable<PayrollReport> {
    return this.http.post<PayrollReport>(`${this.apiUrl}/reports/generate`, {
      type,
      period,
      branchId
    });
  }

  exportPayrollReport(reportId: number, format: 'pdf' | 'excel' | 'csv'): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/reports/${reportId}/export/${format}`, {
      responseType: 'blob'
    });
  }

  // Analytics
  getPayrollAnalytics(period: PayrollPeriod, branchId?: number): Observable<any> {
    let params = new HttpParams()
      .set('month', period.month.toString())
      .set('year', period.year.toString());
    
    if (branchId) params = params.set('branchId', branchId.toString());
    
    return this.http.get(`${this.apiUrl}/analytics`, { params });
  }

  getPayrollTrends(months: number, branchId?: number): Observable<any> {
    let params = new HttpParams().set('months', months.toString());
    if (branchId) params = params.set('branchId', branchId.toString());
    
    return this.http.get(`${this.apiUrl}/analytics/trends`, { params });
  }

  // Utility Methods
  getCurrentPayrollPeriod(): PayrollPeriod {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth() + 1;
    
    const startDate = new Date(year, month - 1, 1);
    const endDate = new Date(year, month, 0);
    
    return {
      month,
      year,
      startDate,
      endDate,
      workingDays: this.calculateWorkingDays(startDate, endDate),
      actualWorkingDays: 0 // Will be calculated based on attendance
    };
  }

  private calculateWorkingDays(startDate: Date, endDate: Date): number {
    let workingDays = 0;
    const currentDate = new Date(startDate);
    
    while (currentDate <= endDate) {
      const dayOfWeek = currentDate.getDay();
      if (dayOfWeek !== 0 && dayOfWeek !== 6) { // Not Sunday (0) or Saturday (6)
        workingDays++;
      }
      currentDate.setDate(currentDate.getDate() + 1);
    }
    
    return workingDays;
  }

  formatCurrency(amount: number, currency: string): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency
    }).format(amount);
  }

  // Real-time updates
  notifyPayrollUpdate(data: any): void {
    this.payrollUpdatesSubject.next(data);
  }
}