import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PayrollService } from './payroll.service';
import {
  PayrollBatch,
  PayrollRecord,
  PayrollFormula,
  PayslipTemplate,
  CreatePayrollBatchDto,
  ProcessPayrollDto,
  PayrollBatchStatus,
  PayrollStatus,
  FormulaType
} from '../models/payroll.models';
import { environment } from '../../environments/environment';

describe('PayrollService', () => {
  let service: PayrollService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/payroll`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [PayrollService]
    });
    service = TestBed.inject(PayrollService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Payroll Batch Management', () => {
    it('should get payroll batches', () => {
      const mockBatches: PayrollBatch[] = [
        {
          id: 1,
          name: 'January 2024 Payroll',
          period: {
            month: 1,
            year: 2024,
            startDate: new Date('2024-01-01'),
            endDate: new Date('2024-01-31'),
            workingDays: 22,
            actualWorkingDays: 22
          },
          branchId: 1,
          branchName: 'Main Branch',
          totalEmployees: 50,
          processedEmployees: 50,
          totalAmount: 500000,
          currency: 'USD',
          status: PayrollBatchStatus.Released,
          createdAt: new Date(),
          createdBy: 'HR Manager'
        }
      ];

      service.getPayrollBatches().subscribe(batches => {
        expect(batches).toEqual(mockBatches);
        expect(batches.length).toBe(1);
        expect(batches[0].name).toBe('January 2024 Payroll');
      });

      const req = httpMock.expectOne(`${apiUrl}/batches`);
      expect(req.request.method).toBe('GET');
      req.flush(mockBatches);
    });

    it('should get payroll batches with filters', () => {
      const branchId = 1;
      const status = PayrollBatchStatus.Approved;

      service.getPayrollBatches(branchId, status).subscribe();

      const req = httpMock.expectOne(`${apiUrl}/batches?branchId=1&status=Approved`);
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should create payroll batch', () => {
      const createDto: CreatePayrollBatchDto = {
        name: 'February 2024 Payroll',
        period: {
          month: 2,
          year: 2024,
          startDate: new Date('2024-02-01'),
          endDate: new Date('2024-02-29'),
          workingDays: 21,
          actualWorkingDays: 0
        },
        branchId: 1
      };

      const mockBatch: PayrollBatch = {
        id: 2,
        ...createDto,
        branchName: 'Main Branch',
        totalEmployees: 0,
        processedEmployees: 0,
        totalAmount: 0,
        currency: 'USD',
        status: PayrollBatchStatus.Draft,
        createdAt: new Date(),
        createdBy: 'HR Manager'
      };

      service.createPayrollBatch(createDto).subscribe(batch => {
        expect(batch).toEqual(mockBatch);
        expect(batch.name).toBe('February 2024 Payroll');
      });

      const req = httpMock.expectOne(`${apiUrl}/batches`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(mockBatch);
    });

    it('should process payroll', () => {
      const processDto: ProcessPayrollDto = {
        batchId: 1,
        customValues: {
          1: { bonus: 1000 },
          2: { overtime: 500 }
        }
      };

      const mockBatch: PayrollBatch = {
        id: 1,
        name: 'January 2024 Payroll',
        period: {
          month: 1,
          year: 2024,
          startDate: new Date('2024-01-01'),
          endDate: new Date('2024-01-31'),
          workingDays: 22,
          actualWorkingDays: 22
        },
        branchId: 1,
        branchName: 'Main Branch',
        totalEmployees: 50,
        processedEmployees: 50,
        totalAmount: 500000,
        currency: 'USD',
        status: PayrollBatchStatus.Calculated,
        createdAt: new Date(),
        processedAt: new Date(),
        createdBy: 'HR Manager'
      };

      service.processPayroll(processDto).subscribe(batch => {
        expect(batch.status).toBe(PayrollBatchStatus.Calculated);
        expect(batch.processedAt).toBeDefined();
      });

      const req = httpMock.expectOne(`${apiUrl}/batches/1/process`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(processDto);
      req.flush(mockBatch);
    });

    it('should approve payroll', () => {
      const approveDto = {
        batchId: 1,
        comments: 'Approved after review'
      };

      service.approvePayroll(approveDto).subscribe();

      const req = httpMock.expectOne(`${apiUrl}/batches/1/approve`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(approveDto);
      req.flush({});
    });
  });

  describe('Payroll Records', () => {
    it('should get payroll records for batch', () => {
      const batchId = 1;
      const mockRecords: PayrollRecord[] = [
        {
          id: 1,
          employeeId: 1,
          employeeName: 'John Doe',
          employeeCode: 'EMP001',
          department: 'IT',
          designation: 'Developer',
          period: {
            month: 1,
            year: 2024,
            startDate: new Date('2024-01-01'),
            endDate: new Date('2024-01-31'),
            workingDays: 22,
            actualWorkingDays: 22
          },
          basicSalary: 5000,
          allowances: [],
          deductions: [],
          grossSalary: 5000,
          totalDeductions: 500,
          netSalary: 4500,
          currency: 'USD',
          status: PayrollStatus.Calculated,
          calculatedAt: new Date()
        }
      ];

      service.getPayrollRecords(batchId).subscribe(records => {
        expect(records).toEqual(mockRecords);
        expect(records.length).toBe(1);
      });

      const req = httpMock.expectOne(`${apiUrl}/batches/1/records`);
      expect(req.request.method).toBe('GET');
      req.flush(mockRecords);
    });
  });

  describe('Payroll Formulas', () => {
    it('should get payroll formulas', () => {
      const mockFormulas: PayrollFormula[] = [
        {
          id: 1,
          name: 'Basic HRA',
          description: 'House Rent Allowance calculation',
          formula: 'basicSalary * 0.4',
          type: FormulaType.Allowance,
          isActive: true,
          createdAt: new Date(),
          updatedAt: new Date()
        }
      ];

      service.getPayrollFormulas().subscribe(formulas => {
        expect(formulas).toEqual(mockFormulas);
        expect(formulas[0].name).toBe('Basic HRA');
      });

      const req = httpMock.expectOne(`${apiUrl}/formulas`);
      expect(req.request.method).toBe('GET');
      req.flush(mockFormulas);
    });

    it('should test formula', () => {
      const formula = 'basicSalary * 0.4';
      const testData = { basicSalary: 5000 };
      const expectedResult = { result: 2000, isValid: true };

      service.testFormula(formula, testData).subscribe(result => {
        expect(result).toEqual(expectedResult);
        expect(result.result).toBe(2000);
        expect(result.isValid).toBe(true);
      });

      const req = httpMock.expectOne(`${apiUrl}/formulas/test`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ formula, testData });
      req.flush(expectedResult);
    });
  });

  describe('Currency Management', () => {
    it('should convert currency', () => {
      const amount = 1000;
      const fromCurrency = 'USD';
      const toCurrency = 'EUR';
      const expectedResult = { convertedAmount: 850, rate: 0.85 };

      service.convertCurrency(amount, fromCurrency, toCurrency).subscribe(result => {
        expect(result).toEqual(expectedResult);
        expect(result.convertedAmount).toBe(850);
        expect(result.rate).toBe(0.85);
      });

      const req = httpMock.expectOne(`${apiUrl}/currency/convert`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ amount, fromCurrency, toCurrency });
      req.flush(expectedResult);
    });
  });

  describe('Utility Methods', () => {
    it('should get current payroll period', () => {
      const period = service.getCurrentPayrollPeriod();
      
      expect(period.month).toBeDefined();
      expect(period.year).toBeDefined();
      expect(period.startDate).toBeDefined();
      expect(period.endDate).toBeDefined();
      expect(period.workingDays).toBeGreaterThan(0);
    });

    it('should format currency', () => {
      const formatted = service.formatCurrency(1234.56, 'USD');
      expect(formatted).toContain('$');
      expect(formatted).toContain('1,234.56');
    });
  });

  describe('Real-time Updates', () => {
    it('should notify payroll updates', () => {
      const testData = { type: 'payroll_processed', batchId: 1 };
      
      service.payrollUpdates$.subscribe(data => {
        expect(data).toEqual(testData);
      });

      service.notifyPayrollUpdate(testData);
    });
  });
});