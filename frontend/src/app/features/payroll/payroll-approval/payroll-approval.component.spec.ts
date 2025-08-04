import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';

import { PayrollApprovalComponent } from './payroll-approval.component';
import { PayrollService } from '../../../services/payroll.service';
import { EmployeeService } from '../../../services/employee.service';
import { PayrollBatch, PayrollBatchStatus, ApprovePayrollDto } from '../../../models/payroll.models';
import { Employee, PagedResult } from '../../../models/employee.models';

describe('PayrollApprovalComponent', () => {
  let component: PayrollApprovalComponent;
  let fixture: ComponentFixture<PayrollApprovalComponent>;
  let mockPayrollService: jasmine.SpyObj<PayrollService>;
  let mockEmployeeService: jasmine.SpyObj<EmployeeService>;
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
    },
    branchId: 1,
    branchName: 'Main Branch',
    totalEmployees: 10,
    processedEmployees: 10,
    totalAmount: 50000,
    currency: 'USD',
    status: PayrollBatchStatus.PendingApproval,
    createdAt: new Date('2024-01-01'),
    processedAt: new Date('2024-01-31'),
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
      'approvePayroll',
      'formatCurrency'
    ]);
    
    // Add the payrollUpdates$ property separately
    payrollServiceSpy.payrollUpdates$ = of(null);

    const employeeServiceSpy = jasmine.createSpyObj('EmployeeService', [
      'getEmployees'
    ]);

    const modalServiceSpy = jasmine.createSpyObj('NgbModal', ['open']);

    await TestBed.configureTestingModule({
      imports: [PayrollApprovalComponent, ReactiveFormsModule],
      providers: [
        FormBuilder,
        { provide: PayrollService, useValue: payrollServiceSpy },
        { provide: EmployeeService, useValue: employeeServiceSpy },
        { provide: NgbModal, useValue: modalServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PayrollApprovalComponent);
    component = fixture.componentInstance;
    mockPayrollService = TestBed.inject(PayrollService) as jasmine.SpyObj<PayrollService>;
    mockEmployeeService = TestBed.inject(EmployeeService) as jasmine.SpyObj<EmployeeService>;
    mockModalService = TestBed.inject(NgbModal) as jasmine.SpyObj<NgbModal>;

    // Setup default mock returns
    mockPayrollService.getPayrollBatches.and.returnValue(of([mockPayrollBatch]));
    mockEmployeeService.getEmployees.and.returnValue(of(mockEmployees));
    mockPayrollService.formatCurrency.and.returnValue('$50,000.00');
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load approval data on init', () => {
    component.ngOnInit();

    expect(mockPayrollService.getPayrollBatches).toHaveBeenCalledWith(undefined, PayrollBatchStatus.PendingApproval);
    expect(mockEmployeeService.getEmployees).toHaveBeenCalled();
    expect(component.approvalBatches).toEqual([mockPayrollBatch]);
  });

  it('should handle loading error gracefully', () => {
    mockPayrollService.getPayrollBatches.and.returnValue(throwError('API Error'));
    spyOn(console, 'error');

    component.ngOnInit();

    expect(console.error).toHaveBeenCalledWith('Error loading approval data:', 'API Error');
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

    expect(component.pendingApprovals).toBe(1);
    expect(component.totalApprovalAmount).toBe(50000);
    expect(component.totalEmployeesForApproval).toBe(10);
  });

  it('should calculate approved today correctly', () => {
    const todayBatch = {
      ...mockPayrollBatch,
      approvedAt: new Date()
    };
    mockPayrollService.getPayrollBatches.and.returnValue(of([todayBatch]));

    component.ngOnInit();

    expect(component.approvedToday).toBe(1);
  });

  it('should open approval modal', () => {
    const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
    mockModalService.open.and.returnValue(mockModalRef as any);

    component.approveBatch(mockPayrollBatch);

    expect(component.selectedBatch).toBe(mockPayrollBatch);
    expect(mockModalService.open).toHaveBeenCalled();
  });

  it('should open rejection modal', () => {
    const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
    mockModalService.open.and.returnValue(mockModalRef as any);

    component.rejectBatch(mockPayrollBatch);

    expect(component.selectedBatch).toBe(mockPayrollBatch);
    expect(mockModalService.open).toHaveBeenCalled();
  });

  it('should submit approval successfully', () => {
    component.selectedBatch = mockPayrollBatch;
    component.approvalForm.patchValue({
      comments: 'Approved',
      confirmApproval: true
    });

    const mockModal = { close: jasmine.createSpy() };
    const approvedBatch = { ...mockPayrollBatch, status: PayrollBatchStatus.Approved };
    mockPayrollService.approvePayroll.and.returnValue(of(approvedBatch));
    component.approvalBatches = [mockPayrollBatch];

    component.submitApproval(mockModal);

    expect(mockPayrollService.approvePayroll).toHaveBeenCalledWith({
      batchId: mockPayrollBatch.id,
      comments: 'Approved'
    } as ApprovePayrollDto);
    expect(mockModal.close).toHaveBeenCalled();
    expect(component.approvalBatches[0]).toEqual(approvedBatch);
  });

  it('should not submit approval with invalid form', () => {
    component.selectedBatch = mockPayrollBatch;
    component.approvalForm.patchValue({
      confirmApproval: false // Invalid - required to be true
    });

    const mockModal = { close: jasmine.createSpy() };

    component.submitApproval(mockModal);

    expect(mockPayrollService.approvePayroll).not.toHaveBeenCalled();
    expect(mockModal.close).not.toHaveBeenCalled();
  });

  it('should handle approval error', () => {
    component.selectedBatch = mockPayrollBatch;
    component.approvalForm.patchValue({
      comments: 'Approved',
      confirmApproval: true
    });

    const mockModal = { close: jasmine.createSpy() };
    mockPayrollService.approvePayroll.and.returnValue(throwError('API Error'));
    spyOn(console, 'error');

    component.submitApproval(mockModal);

    expect(console.error).toHaveBeenCalledWith('Error approving payroll:', 'API Error');
    expect(component.approving).toBeFalse();
  });

  it('should submit rejection successfully', () => {
    component.selectedBatch = mockPayrollBatch;
    component.rejectionForm.patchValue({
      reason: 'calculation_error',
      comments: 'Calculation errors found',
      notifyHR: true
    });

    const mockModal = { close: jasmine.createSpy() };
    jasmine.clock().install();

    component.submitRejection(mockModal);

    expect(component.rejecting).toBeTrue();

    jasmine.clock().tick(1001);

    expect(component.rejecting).toBeFalse();
    expect(mockModal.close).toHaveBeenCalled();
    expect(component.selectedBatch).toBeNull();

    jasmine.clock().uninstall();
  });

  it('should not submit rejection with invalid form', () => {
    component.selectedBatch = mockPayrollBatch;
    component.rejectionForm.patchValue({
      reason: '', // Invalid - required
      comments: ''  // Invalid - required
    });

    const mockModal = { close: jasmine.createSpy() };

    component.submitRejection(mockModal);

    expect(component.rejecting).toBeFalse();
    expect(mockModal.close).not.toHaveBeenCalled();
  });

  it('should toggle select all batches', () => {
    component.approvalBatches = [mockPayrollBatch, { ...mockPayrollBatch, id: 2 }];
    const event = { target: { checked: true } };

    component.toggleSelectAll(event);

    expect(component.allSelected).toBeTrue();
    expect(component.selectedBatches).toEqual([1, 2]);

    const uncheckEvent = { target: { checked: false } };
    component.toggleSelectAll(uncheckEvent);

    expect(component.allSelected).toBeFalse();
    expect(component.selectedBatches).toEqual([]);
  });

  it('should toggle individual batch selection', () => {
    component.approvalBatches = [mockPayrollBatch];
    const event = { target: { checked: true } };

    component.toggleBatchSelection(1, event);

    expect(component.selectedBatches).toContain(1);
    expect(component.allSelected).toBeTrue();

    const uncheckEvent = { target: { checked: false } };
    component.toggleBatchSelection(1, uncheckEvent);

    expect(component.selectedBatches).not.toContain(1);
    expect(component.allSelected).toBeFalse();
  });

  it('should handle bulk approve with confirmation', () => {
    component.selectedBatches = [1, 2];
    spyOn(window, 'confirm').and.returnValue(true);
    spyOn(console, 'log');

    component.bulkApprove();

    expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to approve 2 payroll batches?');
    expect(console.log).toHaveBeenCalledWith('Bulk approving batches:', [1, 2]);
  });

  it('should not bulk approve without selection', () => {
    component.selectedBatches = [];
    spyOn(console, 'log');

    component.bulkApprove();

    expect(console.log).not.toHaveBeenCalled();
  });

  it('should filter batches correctly', () => {
    component.selectedBranchId = 1;
    component.selectedStatus = 'PendingApproval';
    mockPayrollService.getPayrollBatches.and.returnValue(of([mockPayrollBatch]));

    component.onFilterChange();

    expect(mockPayrollService.getPayrollBatches).toHaveBeenCalledWith(1, PayrollBatchStatus.PendingApproval);
  });

  it('should clear all filters', () => {
    component.selectedStatus = 'PendingApproval';
    component.selectedBranchId = 1;
    component.selectedPriority = 'high';

    component.clearFilters();

    expect(component.selectedStatus).toBeNull();
    expect(component.selectedBranchId).toBeNull();
    expect(component.selectedPriority).toBeNull();
  });

  it('should get correct priority based on amount', () => {
    const highBatch = { ...mockPayrollBatch, totalAmount: 150000 };
    const normalBatch = { ...mockPayrollBatch, totalAmount: 75000 };
    const lowBatch = { ...mockPayrollBatch, totalAmount: 25000 };

    expect(component.getPriority(highBatch)).toBe('High');
    expect(component.getPriority(normalBatch)).toBe('Normal');
    expect(component.getPriority(lowBatch)).toBe('Low');
  });

  it('should get correct priority badge class', () => {
    const highBatch = { ...mockPayrollBatch, totalAmount: 150000 };

    expect(component.getPriorityBadgeClass(highBatch)).toBe('badge bg-high');
  });

  it('should get correct status badge class', () => {
    expect(component.getStatusBadgeClass(PayrollBatchStatus.PendingApproval)).toBe('badge bg-pending');
    expect(component.getStatusBadgeClass(PayrollBatchStatus.Approved)).toBe('badge bg-approved');
  });

  it('should determine approval permissions correctly', () => {
    const pendingBatch = { ...mockPayrollBatch, status: PayrollBatchStatus.PendingApproval };
    const approvedBatch = { ...mockPayrollBatch, status: PayrollBatchStatus.Approved };

    expect(component.canApprove(pendingBatch)).toBeTrue();
    expect(component.canReject(pendingBatch)).toBeTrue();
    expect(component.canApprove(approvedBatch)).toBeFalse();
    expect(component.canReject(approvedBatch)).toBeFalse();
  });

  it('should format period correctly', () => {
    const period = { month: 1, year: 2024 };

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

  it('should refresh data', () => {
    spyOn(component as any, 'loadApprovalData');

    component.refreshData();

    expect((component as any).loadApprovalData).toHaveBeenCalled();
  });

  it('should handle realtime updates', () => {
    spyOn(component, 'refreshData');
    const mockUpdate = { type: 'payroll_approved' };

    // Setup the observable to emit the update
    mockPayrollService.payrollUpdates$ = of(mockUpdate);
    
    component.ngOnInit();

    expect(component.refreshData).toHaveBeenCalled();
  });

  it('should validate approval form', () => {
    expect(component.approvalForm.valid).toBeFalse();

    component.approvalForm.patchValue({
      comments: 'Test comment',
      confirmApproval: true
    });

    expect(component.approvalForm.valid).toBeTrue();
  });

  it('should validate rejection form', () => {
    expect(component.rejectionForm.valid).toBeFalse();

    component.rejectionForm.patchValue({
      reason: 'calculation_error',
      comments: 'Test rejection reason',
      notifyHR: true
    });

    expect(component.rejectionForm.valid).toBeTrue();
  });

  it('should handle view batch details action', () => {
    spyOn(console, 'log');

    component.viewBatchDetails(mockPayrollBatch);

    expect(console.log).toHaveBeenCalledWith('View batch details:', mockPayrollBatch.id);
  });

  it('should handle review payroll action', () => {
    spyOn(console, 'log');

    component.reviewPayroll(mockPayrollBatch);

    expect(console.log).toHaveBeenCalledWith('Review payroll:', mockPayrollBatch.id);
  });

  it('should handle view approval history action', () => {
    spyOn(console, 'log');

    component.viewApprovalHistory(mockPayrollBatch);

    expect(console.log).toHaveBeenCalledWith('View approval history:', mockPayrollBatch.id);
  });

  it('should filter by priority when applying filters', () => {
    const highBatch = { ...mockPayrollBatch, totalAmount: 150000 };
    const lowBatch = { ...mockPayrollBatch, id: 2, totalAmount: 25000 };
    
    mockPayrollService.getPayrollBatches.and.returnValue(of([highBatch, lowBatch]));
    component.selectedPriority = 'high';

    component.onFilterChange();

    expect(component.approvalBatches).toEqual([highBatch]);
    expect(component.approvalBatches).not.toContain(lowBatch);
  });
});