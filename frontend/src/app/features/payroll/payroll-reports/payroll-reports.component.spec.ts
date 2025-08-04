import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';

import { PayrollReportsComponent } from './payroll-reports.component';
import { PayrollService } from '../../../services/payroll.service';
import { EmployeeService } from '../../../services/employee.service';
import { PayrollReport, PayrollReportType, PayrollPeriod } from '../../../models/payroll.models';
import { Employee, PagedResult } from '../../../models/employee.models';

describe('PayrollReportsComponent', () => {
  let component: PayrollReportsComponent;
  let fixture: ComponentFixture<PayrollReportsComponent>;
  let mockPayrollService: jasmine.SpyObj<PayrollService>;
  let mockEmployeeService: jasmine.SpyObj<EmployeeService>;
  let mockModalService: jasmine.SpyObj<NgbModal>;

  const mockPayrollReport: PayrollReport = {
    id: 1,
    name: 'January 2024 Payroll Summary',
    type: PayrollReportType.PayrollSummary,
    period: {
      month: 1,
      year: 2024,
      startDate: new Date('2024-01-01'),
      endDate: new Date('2024-01-31'),
      workingDays: 22,
      actualWorkingDays: 22
    } as PayrollPeriod,
    branchId: 1,
    data: {},
    generatedAt: new Date('2024-01-31'),
    generatedBy: 'HR Manager'
  };

  const mockEmployees: PagedResult<Employee> = {
    items: [
      {
        id: 1,
        employeeId: 'EMP001',
        branchId: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@company.com',
        phone: '+1-555-0101',
        dateOfBirth: '1990-05-15',
        joiningDate: '2020-01-15',
        designation: 'Developer',
        department: 'IT',
        basicSalary: 5000,
        status: 'Active' as any,
        createdAt: '2020-01-15T00:00:00Z',
        branch: { id: 1, name: 'Main Branch' } as any
      }
    ],
    totalCount: 1,
    page: 1,
    pageSize: 10,
    totalPages: 1
  };

  const mockAnalytics = {
    totalPayroll: 100000,
    totalEmployees: 50
  };

  beforeEach(async () => {
    const payrollServiceSpy = jasmine.createSpyObj('PayrollService', [
      'getPayrollReports',
      'generatePayrollReport',
      'exportPayrollReport',
      'getPayrollAnalytics',
      'getCurrentPayrollPeriod',
      'formatCurrency'
    ]);

    const employeeServiceSpy = jasmine.createSpyObj('EmployeeService', [
      'getEmployees'
    ]);

    const modalServiceSpy = jasmine.createSpyObj('NgbModal', ['open']);

    await TestBed.configureTestingModule({
      imports: [PayrollReportsComponent, ReactiveFormsModule],
      providers: [
        FormBuilder,
        { provide: PayrollService, useValue: payrollServiceSpy },
        { provide: EmployeeService, useValue: employeeServiceSpy },
        { provide: NgbModal, useValue: modalServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PayrollReportsComponent);
    component = fixture.componentInstance;
    mockPayrollService = TestBed.inject(PayrollService) as jasmine.SpyObj<PayrollService>;
    mockEmployeeService = TestBed.inject(EmployeeService) as jasmine.SpyObj<EmployeeService>;
    mockModalService = TestBed.inject(NgbModal) as jasmine.SpyObj<NgbModal>;

    // Setup default mock returns
    mockPayrollService.getPayrollReports.and.returnValue(of([mockPayrollReport]));
    mockEmployeeService.getEmployees.and.returnValue(of(mockEmployees));
    mockPayrollService.getPayrollAnalytics.and.returnValue(of(mockAnalytics));
    mockPayrollService.getCurrentPayrollPeriod.and.returnValue({
      month: 1,
      year: 2024,
      startDate: new Date('2024-01-01'),
      endDate: new Date('2024-01-31'),
      workingDays: 22,
      actualWorkingDays: 22
    });
    mockPayrollService.formatCurrency.and.returnValue('$100,000.00');
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load initial data on init', () => {
    component.ngOnInit();

    expect(mockPayrollService.getPayrollReports).toHaveBeenCalled();
    expect(mockEmployeeService.getEmployees).toHaveBeenCalled();
    expect(mockPayrollService.getPayrollAnalytics).toHaveBeenCalled();
    expect(component.reports).toEqual([mockPayrollReport]);
  });

  it('should handle loading error gracefully', () => {
    mockPayrollService.getPayrollReports.and.returnValue(throwError('API Error'));
    spyOn(console, 'error');

    component.ngOnInit();

    expect(console.error).toHaveBeenCalledWith('Error loading data:', 'API Error');
    expect(component.loading).toBeFalse();
  });

  it('should extract branches from employees', () => {
    component.ngOnInit();

    expect(component.branches).toEqual([
      { id: 1, name: 'Main Branch' }
    ]);
  });

  it('should update stats correctly', () => {
    component.ngOnInit();

    expect(component.totalReports).toBe(1);
    expect(component.totalPayrollAmount).toBe(100000);
    expect(component.totalEmployees).toBe(50);
  });

  it('should calculate current month reports correctly', () => {
    const currentDate = new Date();
    const currentMonthReport = {
      ...mockPayrollReport,
      period: {
        ...mockPayrollReport.period,
        month: currentDate.getMonth() + 1,
        year: currentDate.getFullYear()
      }
    };
    
    mockPayrollService.getPayrollReports.and.returnValue(of([currentMonthReport]));

    component.ngOnInit();

    expect(component.currentMonthReports).toBe(1);
  });

  it('should open generate report modal', () => {
    const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
    mockModalService.open.and.returnValue(mockModalRef as any);

    component.generateNewReport();

    expect(mockModalService.open).toHaveBeenCalled();
  });

  it('should generate report successfully', () => {
    component.generateReportForm.patchValue({
      name: 'Test Report',
      type: PayrollReportType.PayrollSummary,
      month: 1,
      year: 2024,
      branchId: 1
    });

    const mockModal = { close: jasmine.createSpy() };
    mockPayrollService.generatePayrollReport.and.returnValue(of(mockPayrollReport));

    component.generateReport(mockModal);

    expect(mockPayrollService.generatePayrollReport).toHaveBeenCalled();
    expect(mockModal.close).toHaveBeenCalled();
    expect(component.reports).toContain(mockPayrollReport);
  });

  it('should not generate report with invalid form', () => {
    const mockModal = { close: jasmine.createSpy() };
    component.generateReportForm.patchValue({ name: '' }); // Invalid form

    component.generateReport(mockModal);

    expect(mockPayrollService.generatePayrollReport).not.toHaveBeenCalled();
    expect(mockModal.close).not.toHaveBeenCalled();
  });

  it('should apply filters correctly', () => {
    component.filterForm.patchValue({
      reportType: PayrollReportType.PayrollSummary,
      month: 1,
      year: 2024,
      branchId: 1
    });

    mockPayrollService.getPayrollReports.and.returnValue(of([mockPayrollReport]));

    component.applyFilters();

    expect(mockPayrollService.getPayrollReports).toHaveBeenCalledWith(
      jasmine.any(Object), // period object
      1 // branchId
    );
  });

  it('should export report successfully', () => {
    const mockBlob = new Blob(['test'], { type: 'application/pdf' });
    mockPayrollService.exportPayrollReport.and.returnValue(of(mockBlob));
    
    spyOn(window.URL, 'createObjectURL').and.returnValue('blob:url');
    spyOn(window.URL, 'revokeObjectURL');
    
    const mockLink = {
      href: '',
      download: '',
      click: jasmine.createSpy('click')
    };
    spyOn(document, 'createElement').and.returnValue(mockLink as any);

    component.exportReport(mockPayrollReport, 'pdf');

    expect(mockPayrollService.exportPayrollReport).toHaveBeenCalledWith(mockPayrollReport.id, 'pdf');
    expect(mockLink.click).toHaveBeenCalled();
    expect(window.URL.revokeObjectURL).toHaveBeenCalled();
  });

  it('should handle export error gracefully', () => {
    mockPayrollService.exportPayrollReport.and.returnValue(throwError('Export Error'));
    spyOn(console, 'error');

    component.exportReport(mockPayrollReport, 'pdf');

    expect(console.error).toHaveBeenCalledWith('Error exporting report:', 'Export Error');
  });

  it('should refresh data', () => {
    spyOn(component as any, 'loadInitialData');

    component.refreshData();

    expect((component as any).loadInitialData).toHaveBeenCalled();
  });

  it('should format period correctly', () => {
    const period = { month: 1, year: 2024 } as PayrollPeriod;

    const formatted = component.formatPeriod(period);

    expect(formatted).toBe('January 2024');
  });

  it('should format currency using service', () => {
    const result = component.formatCurrency(50000);

    expect(mockPayrollService.formatCurrency).toHaveBeenCalledWith(50000, 'USD');
    expect(result).toBe('$100,000.00');
  });

  it('should get correct report type label', () => {
    expect(component.getReportTypeLabel(PayrollReportType.PayrollSummary)).toBe('Payroll Summary');
    expect(component.getReportTypeLabel(PayrollReportType.TaxReport)).toBe('Tax Report');
    expect(component.getReportTypeLabel(PayrollReportType.StatutoryReport)).toBe('Statutory Report');
    expect(component.getReportTypeLabel(PayrollReportType.DepartmentWise)).toBe('Department Wise');
    expect(component.getReportTypeLabel(PayrollReportType.BranchWise)).toBe('Branch Wise');
  });

  it('should get correct branch name', () => {
    component.branches = [{ id: 1, name: 'Main Branch' }];

    expect(component.getBranchName(1)).toBe('Main Branch');
    expect(component.getBranchName(999)).toBe('Branch 999');
    expect(component.getBranchName(undefined)).toBe('All Branches');
  });

  it('should track reports by ID', () => {
    const result = component.trackByReportId(0, mockPayrollReport);

    expect(result).toBe(mockPayrollReport.id);
  });

  it('should validate filter form', () => {
    expect(component.filterForm.valid).toBeTrue(); // All fields are optional

    component.filterForm.patchValue({
      reportType: PayrollReportType.PayrollSummary,
      month: 1,
      year: 2024,
      branchId: 1
    });

    expect(component.filterForm.valid).toBeTrue();
  });

  it('should validate generate report form', () => {
    expect(component.generateReportForm.valid).toBeFalse();

    component.generateReportForm.patchValue({
      name: 'Test Report',
      type: PayrollReportType.PayrollSummary,
      month: 1,
      year: 2024,
      branchId: 1
    });

    expect(component.generateReportForm.valid).toBeTrue();
  });

  it('should handle view report action', () => {
    spyOn(console, 'log');

    component.viewReport(mockPayrollReport);

    expect(console.log).toHaveBeenCalledWith('View report:', mockPayrollReport.id);
  });

  it('should handle share report action', () => {
    spyOn(console, 'log');

    component.shareReport(mockPayrollReport);

    expect(console.log).toHaveBeenCalledWith('Share report:', mockPayrollReport.id);
  });

  it('should handle delete report with confirmation', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    spyOn(console, 'log');

    component.deleteReport(mockPayrollReport);

    expect(window.confirm).toHaveBeenCalled();
    expect(console.log).toHaveBeenCalledWith('Delete report:', mockPayrollReport.id);
  });

  it('should not delete report without confirmation', () => {
    spyOn(window, 'confirm').and.returnValue(false);
    spyOn(console, 'log');

    component.deleteReport(mockPayrollReport);

    expect(console.log).not.toHaveBeenCalled();
  });

  it('should handle bulk export action', () => {
    spyOn(console, 'log');

    component.bulkExport('pdf');

    expect(console.log).toHaveBeenCalledWith('Bulk export in format:', 'pdf');
  });

  it('should filter reports by type when applying filters', () => {
    const taxReport = { ...mockPayrollReport, type: PayrollReportType.TaxReport };
    mockPayrollService.getPayrollReports.and.returnValue(of([mockPayrollReport, taxReport]));
    
    component.filterForm.patchValue({
      reportType: PayrollReportType.PayrollSummary
    });

    component.applyFilters();

    expect(component.reports).toEqual([mockPayrollReport]);
    expect(component.reports).not.toContain(taxReport);
  });

  it('should create period object correctly when generating report', () => {
    component.generateReportForm.patchValue({
      name: 'Test Report',
      type: PayrollReportType.PayrollSummary,
      month: 3,
      year: 2024,
      branchId: 1
    });

    const mockModal = { close: jasmine.createSpy() };
    mockPayrollService.generatePayrollReport.and.returnValue(of(mockPayrollReport));

    component.generateReport(mockModal);

    const expectedPeriod = {
      month: 3,
      year: 2024,
      startDate: new Date(2024, 2, 1), // March 1st (month is 0-indexed)
      endDate: new Date(2024, 3, 0),   // Last day of March
      workingDays: 0,
      actualWorkingDays: 0
    };

    expect(mockPayrollService.generatePayrollReport).toHaveBeenCalledWith(
      PayrollReportType.PayrollSummary,
      expectedPeriod,
      1
    );
  });
});