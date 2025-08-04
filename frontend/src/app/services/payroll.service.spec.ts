import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { PayrollService } from './payroll.service';
import { PayrollRecord, PayrollBatch, PayrollCalculationDto } from '../models/payroll.models';

describe('PayrollService', () => {
  let service: PayrollService;
  let httpMock: HttpTestingController;

  const mockPayrollRecord: PayrollRecord = {
    id: 1,
    employeeId: 1,
    employeeName: 'John Doe',
    employeeCode: 'EMP001',
    department: 'IT',
    designation: 'Software Engineer',
    period: {
      month: 1,
      year: 2025,
      startDate: new Date('2025-01-01'),
      endDate: new Date('2025-01-31'),
      workingDays: 22,
      actualWorkingDays: 20
    },
    basicSalary: 50000,
    allowances: [],
    deductions: [],
    grossSalary: 55000,
    totalDeductions: 5000,
    netSalary: 50000,
    currency: 'USD',
    status: 'Calculated' as any,
    calculatedAt: new Date()
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [],
      providers: [
        PayrollService,
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(PayrollService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('calculatePayroll', () => {
    it('should calculate payroll for employee', () => {
      const request: PayrollCalculationDto = {
        employeeId: 1,
        period: {
          month: 1,
          year: 2025,
          startDate: new Date('2025-01-01'),
          endDate: new Date('2025-01-31'),
          workingDays: 22,
          actualWorkingDays: 20
        }
      };

      service.calculatePayroll(request).subscribe(result => {
        expect(result).toEqual(mockPayrollRecord);
        expect(result.employeeId).toBe(1);
        expect(result.grossSalary).toBe(55000);
        expect(result.netSalary).toBe(50000);
      });

      const req = httpMock.expectOne(`${service['apiUrl']}/calculate`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush(mockPayrollRecord);
    });
  });

  describe('getPayrollRecords', () => {
    it('should get payroll records for batch', () => {
      const batchId = 1;
      const mockRecords = [mockPayrollRecord];

      service.getPayrollRecords(batchId).subscribe(records => {
        expect(records).toEqual(mockRecords);
        expect(records.length).toBe(1);
        expect(records[0].employeeId).toBe(1);
      });

      const req = httpMock.expectOne(`${service['apiUrl']}/batches/${batchId}/records`);
      expect(req.request.method).toBe('GET');
      req.flush(mockRecords);
    });
  });

  describe('error handling', () => {
    it('should handle HTTP errors gracefully', () => {
      const request: PayrollCalculationDto = {
        employeeId: 999, // Non-existent employee
        period: {
          month: 1,
          year: 2025,
          startDate: new Date('2025-01-01'),
          endDate: new Date('2025-01-31'),
          workingDays: 22,
          actualWorkingDays: 20
        }
      };

      service.calculatePayroll(request).subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(404);
        }
      });

      const req = httpMock.expectOne(`${service['apiUrl']}/calculate`);
      req.flush('Employee not found', { status: 404, statusText: 'Not Found' });
    });
  });
});