import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';

import { PayrollProcessingComponent } from './payroll-processing.component';
import { PayrollService } from '../../../services/payroll.service';
import { EmployeeService } from '../../../services/employee.service';
import { PayrollBatch, PayrollBatchStatus, PayrollPeriod } from '../../../models/payroll.models';
import { Employee, PagedResult } from '../../../models/employee.models';

describe('PayrollProcessingComponent', () => {
  let component: PayrollProcessingComponent;
  let fixture: ComponentFixture<PayrollProcessingComponent>;
  let mockPayrollService: jasmine.SpyObj<PayrollService>;
  let mockEmployeeService: jasmine.SpyObj<EmployeeService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockModalService: jasmine.SpyObj<NgbModal>;

  const mockPayrollBatch: PayrollBatch = {
    id: 1,
    name: 'January 2024 Payroll',
    period: {
      month: 1,
      year: 2024,
      startDate: new Date('2024-01-01'),
      endDate: new Date('2024-01-31'),
      workingDays: 22,
      actualWorkingDays: 22
    } as PayrollPeriod,
    branchId: 1,
    branchName: 'Main Branch',
    totalEmployees: 10,
    processedEmployees: 10,
    totalAmount: 50000,
    currency: 'USD',
    status: PayrollBatchStatus.Draft,
    createdAt: new Date('2024-01-01'),
    createdBy: 'HR Manager'
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

  beforeEach(async () => {
    const payrollServiceSpy = jasmine.createSpyObj('PayrollService', [
      'getPayrollBatches',
      'createPayrollBatch',
      'processPayroll',
      'approvePayroll',
      'releasePayroll',
      'deletePayrollBatch',
      'formatCurrency'
    ], {
      payrollUpdates$: of(null)
    });

    const employeeServiceSpy = jasmine.createSpyObj('EmployeeService', [
      'getEmployees'
    ]);

    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    const modalServiceSpy = jasmine.createSpyObj('NgbModal', ['open']);

    await TestBed.configureTestingModule({
      imports: [PayrollProcessingComponent, ReactiveFormsModule],
      providers: [
        FormBuilder,
        { provide: PayrollService, useValue: payrollServiceSpy },
        { provide: EmployeeService, useValue: employeeServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: NgbModal, useValue: modalServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PayrollProcessingComponent);
    component = fixture.componentInstance;
    mockPayrollService = TestBed.inject(PayrollService) as jasmine.SpyObj<PayrollService>;
    mockEmployeeService = TestBed.inject(EmployeeService) as jasmine.SpyObj<EmployeeService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    mockModalService = TestBed.inject(NgbModal) as jasmine.SpyObj<NgbModal>;

    // Setup default mock returns
    mockPayrollService.getPayrollBatches.and.returnValue(of([mockPayrollBatch]));
    mockEmployeeService.getEmployees.and.returnValue(of(mockEmployees));
    mockPayrollService.formatCurrency.and.returnValue('$50,000.00');
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load initial data on init', () => {
    component.ngOnInit();

    expect(mockPayrollService.getPayrollBatches).toHaveBeenCalled();
    expect(mockEmployeeService.getEmployees).toHaveBeenCalled();
    expect(component.payrollBatches).toEqual([mockPayrollBatch]);
    expect(component.employees).toEqual(mockEmployees.items);
  });

  it('should handle loading error gracefully', () => {
    mockPayrollService.getPayrollBatches.and.returnValue(throwError('API Error'));
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

  it('should open create batch modal', () => {
    const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
    mockModalService.open.and.returnValue(mockModalRef as any);

    component.openCreateBatchModal();

    expect(mockModalService.open).toHaveBeenCalled();
  });

  it('should create payroll batch successfully', () => {
    component.createBatchForm.patchValue({
      name: 'Test Batch',
      month: 1,
      year: 2024,
      branchId: 1,
      employeeSelection: 'all'
    });

    const mockModal = { close: jasmine.createSpy() };
    mockPayrollService.createPayrollBatch.and.returnValue(of(mockPayrollBatch));

    component.createBatch(mockModal);

    expect(mockPayrollService.createPayrollBatch).toHaveBeenCalled();
    expect(mockModal.close).toHaveBeenCalled();
    expect(component.payrollBatches).toContain(mockPayrollBatch);
  });

  it('should process payroll batch', () => {
    const updatedBatch = { ...mockPayrollBatch, status: PayrollBatchStatus.Processing };
    mockPayrollService.processPayroll.and.returnValue(of(updatedBatch));
    component.payrollBatches = [mockPayrollBatch];

    component.processBatch(mockPayrollBatch);

    expect(mockPayrollService.processPayroll).toHaveBeenCalledWith({ batchId: mockPayrollBatch.id });
    expect(component.payrollBatches[0]).toEqual(updatedBatch);
  });

  it('should approve payroll batch', () => {
    const updatedBatch = { ...mockPayrollBatch, status: PayrollBatchStatus.Approved };
    mockPayrollService.approvePayroll.and.returnValue(of(updatedBatch));
    component.payrollBatches = [mockPayrollBatch];

    component.approveBatch(mockPayrollBatch);

    expect(mockPayrollService.approvePayroll).toHaveBeenCalled();
    expect(component.payrollBatches[0]).toEqual(updatedBatch);
  });

  it('should release payroll batch', () => {
    const updatedBatch = { ...mockPayrollBatch, status: PayrollBatchStatus.Released };
    mockPayrollService.releasePayroll.and.returnValue(of(updatedBatch));
    component.payrollBatches = [mockPayrollBatch];

    component.releaseBatch(mockPayrollBatch);

    expect(mockPayrollService.releasePayroll).toHaveBeenCalledWith(mockPayrollBatch.id);
    expect(component.payrollBatches[0]).toEqual(updatedBatch);
  });

  it('should delete payroll batch with confirmation', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    mockPayrollService.deletePayrollBatch.and.returnValue(of(void 0));
    component.payrollBatches = [mockPayrollBatch];

    component.deleteBatch(mockPayrollBatch);

    expect(window.confirm).toHaveBeenCalled();
    expect(mockPayrollService.deletePayrollBatch).toHaveBeenCalledWith(mockPayrollBatch.id);
    expect(component.payrollBatches).not.toContain(mockPayrollBatch);
  });

  it('should not delete payroll batch without confirmation', () => {
    spyOn(window, 'confirm').and.returnValue(false);
    component.payrollBatches = [mockPayrollBatch];

    component.deleteBatch(mockPayrollBatch);

    expect(mockPayrollService.deletePayrollBatch).not.toHaveBeenCalled();
    expect(component.payrollBatches).toContain(mockPayrollBatch);
  });

  it('should navigate to batch details', () => {
    component.viewBatchDetails(mockPayrollBatch);

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/payroll/batch', mockPayrollBatch.id]);
  });

  it('should filter batches by status and branch', () => {
    component.selectedBranchId = 1;
    component.selectedStatus = PayrollBatchStatus.Draft;
    mockPayrollService.getPayrollBatches.and.returnValue(of([mockPayrollBatch]));

    component.onFilterChange();

    expect(mockPayrollService.getPayrollBatches).toHaveBeenCalledWith(1, PayrollBatchStatus.Draft);
  });

  it('should clear all filters', () => {
    component.selectedBranchId = 1;
    component.selectedStatus = PayrollBatchStatus.Draft;
    component.selectedPeriod = 'current';

    component.clearFilters();

    expect(component.selectedBranchId).toBeNull();
    expect(component.selectedStatus).toBeNull();
    expect(component.selectedPeriod).toBeNull();
  });

  it('should calculate processing progress correctly', () => {
    const batch = { ...mockPayrollBatch, totalEmployees: 10, processedEmployees: 7 };

    const progress = component.getProcessingProgress(batch);

    expect(progress).toBe(70);
  });

  it('should return correct progress bar class', () => {
    const completeBatch = { ...mockPayrollBatch, totalEmployees: 10, processedEmployees: 10 };
    const partialBatch = { ...mockPayrollBatch, totalEmployees: 10, processedEmployees: 6 };
    const lowBatch = { ...mockPayrollBatch, totalEmployees: 10, processedEmployees: 3 };

    expect(component.getProgressBarClass(completeBatch)).toBe('bg-success');
    expect(component.getProgressBarClass(partialBatch)).toBe('bg-info');
    expect(component.getProgressBarClass(lowBatch)).toBe('bg-warning');
  });

  it('should return correct status badge class', () => {
    expect(component.getStatusBadgeClass(PayrollBatchStatus.Draft)).toContain('bg-draft');
    expect(component.getStatusBadgeClass(PayrollBatchStatus.Processing)).toContain('bg-processing');
    expect(component.getStatusBadgeClass(PayrollBatchStatus.Approved)).toContain('bg-approved');
  });

  it('should determine batch action permissions correctly', () => {
    const draftBatch = { ...mockPayrollBatch, status: PayrollBatchStatus.Draft };
    const pendingBatch = { ...mockPayrollBatch, status: PayrollBatchStatus.PendingApproval };
    const approvedBatch = { ...mockPayrollBatch, status: PayrollBatchStatus.Approved };

    expect(component.canProcessBatch(draftBatch)).toBeTrue();
    expect(component.canApproveBatch(pendingBatch)).toBeTrue();
    expect(component.canReleaseBatch(approvedBatch)).toBeTrue();
    expect(component.canDeleteBatch(draftBatch)).toBeTrue();
  });

  it('should format period correctly', () => {
    const period = { month: 1, year: 2024 } as PayrollPeriod;

    const formatted = component.formatPeriod(period);

    expect(formatted).toBe('January 2024');
  });

  it('should format currency using service', () => {
    const result = component.formatCurrency(50000, 'USD');

    expect(mockPayrollService.formatCurrency).toHaveBeenCalledWith(50000, 'USD');
    expect(result).toBe('$50,000.00');
  });

  it('should track batches by ID', () => {
    const result = component.trackByBatchId(0, mockPayrollBatch);

    expect(result).toBe(mockPayrollBatch.id);
  });

  it('should handle employee selection change', () => {
    const event = { target: { checked: true } };
    component.selectedEmployeeIds = [];

    component.onEmployeeSelectionChange(event, 1);

    expect(component.selectedEmployeeIds).toContain(1);

    const uncheckEvent = { target: { checked: false } };
    component.onEmployeeSelectionChange(uncheckEvent, 1);

    expect(component.selectedEmployeeIds).not.toContain(1);
  });

  it('should refresh data', () => {
    spyOn(component as any, 'loadInitialData');

    component.refreshData();

    expect((component as any).loadInitialData).toHaveBeenCalled();
  });

  it('should handle realtime updates', () => {
    spyOn(component, 'refreshData');
    const mockUpdate = { type: 'payroll_processed' };

    (component as any).handleRealtimeUpdate(mockUpdate);

    expect(component.refreshData).toHaveBeenCalled();
  });

  it('should validate create batch form', () => {
    expect(component.createBatchForm.valid).toBeFalse();

    component.createBatchForm.patchValue({
      name: 'Test Batch',
      month: 1,
      year: 2024,
      branchId: 1,
      employeeSelection: 'all'
    });

    expect(component.createBatchForm.valid).toBeTrue();
  });

  it('should not create batch with invalid form', () => {
    const mockModal = { close: jasmine.createSpy() };
    component.createBatchForm.patchValue({ name: '' }); // Invalid form

    component.createBatch(mockModal);

    expect(mockPayrollService.createPayrollBatch).not.toHaveBeenCalled();
    expect(mockModal.close).not.toHaveBeenCalled();
  });
});