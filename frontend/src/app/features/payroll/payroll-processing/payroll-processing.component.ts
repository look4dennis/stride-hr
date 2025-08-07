import { Component, OnInit, OnDestroy, TemplateRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { ModalService } from '../../../services/modal.service';

import { PayrollService } from '../../../services/payroll.service';
import { EmployeeService } from '../../../services/employee.service';
import {
  PayrollBatch,
  PayrollPeriod,
  PayrollBatchStatus,
  CreatePayrollBatchDto,
  ProcessPayrollDto
} from '../../../models/payroll.models';
import { Employee, PagedResult } from '../../../models/employee.models';

@Component({
  selector: 'app-payroll-processing',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,

    NgbDropdownModule
  ],
  template: `
    <div class="payroll-processing-container">
      <!-- Header -->
      <div class="page-header d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="h3 mb-1">Payroll Processing</h1>
          <p class="text-muted mb-0">Process and manage employee payroll</p>
        </div>
        <div class="d-flex gap-2">
          <button class="btn btn-outline-primary" (click)="refreshData()">
            <i class="fas fa-sync-alt me-2"></i>Refresh
          </button>
          <button class="btn btn-primary" (click)="openCreateBatchModal()">
            <i class="fas fa-plus me-2"></i>New Payroll Batch
          </button>
        </div>
      </div>

      <!-- Filters -->
      <div class="card mb-4">
        <div class="card-body">
          <div class="row g-3">
            <div class="col-md-3">
              <label class="form-label">Branch</label>
              <select class="form-select" [(ngModel)]="selectedBranchId" (change)="onFilterChange()">
                <option value="">All Branches</option>
                <option *ngFor="let branch of branches" [value]="branch.id">
                  {{ branch.name }}
                </option>
              </select>
            </div>
            <div class="col-md-3">
              <label class="form-label">Status</label>
              <select class="form-select" [(ngModel)]="selectedStatus" (change)="onFilterChange()">
                <option value="">All Status</option>
                <option *ngFor="let status of payrollStatuses" [value]="status">
                  {{ status }}
                </option>
              </select>
            </div>
            <div class="col-md-3">
              <label class="form-label">Period</label>
              <select class="form-select" [(ngModel)]="selectedPeriod" (change)="onFilterChange()">
                <option value="">All Periods</option>
                <option *ngFor="let period of availablePeriods" [value]="period.key">
                  {{ period.label }}
                </option>
              </select>
            </div>
            <div class="col-md-3 d-flex align-items-end">
              <button class="btn btn-outline-secondary w-100" (click)="clearFilters()">
                <i class="fas fa-times me-2"></i>Clear Filters
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Payroll Batches -->
      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5 class="card-title mb-0">Payroll Batches</h5>
              <div class="d-flex gap-2">
                <div class="btn-group" ngbDropdown>
                  <button class="btn btn-outline-secondary btn-sm dropdown-toggle" ngbDropdownToggle>
                    <i class="fas fa-download me-2"></i>Export
                  </button>
                  <div class="dropdown-menu" ngbDropdownMenu>
                    <button class="dropdown-item" (click)="exportData('excel')">
                      <i class="fas fa-file-excel me-2"></i>Excel
                    </button>
                    <button class="dropdown-item" (click)="exportData('pdf')">
                      <i class="fas fa-file-pdf me-2"></i>PDF
                    </button>
                  </div>
                </div>
              </div>
            </div>
            <div class="card-body">
              <div *ngIf="loading" class="text-center py-4">
                <div class="spinner-border text-primary" role="status">
                  <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mt-2 text-muted">Loading payroll batches...</p>
              </div>

              <div *ngIf="!loading && payrollBatches.length === 0" class="text-center py-5">
                <i class="fas fa-money-bill-wave text-muted mb-3" style="font-size: 3rem;"></i>
                <h4>No Payroll Batches Found</h4>
                <p class="text-muted">Create your first payroll batch to get started.</p>
                <button class="btn btn-primary" (click)="openCreateBatchModal()">
                  <i class="fas fa-plus me-2"></i>Create Payroll Batch
                </button>
              </div>

              <div *ngIf="!loading && payrollBatches.length > 0" class="table-responsive">
                <table class="table table-hover">
                  <thead>
                    <tr>
                      <th>Batch Name</th>
                      <th>Period</th>
                      <th>Branch</th>
                      <th>Employees</th>
                      <th>Total Amount</th>
                      <th>Status</th>
                      <th>Created</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let batch of payrollBatches; trackBy: trackByBatchId">
                      <td>
                        <div class="fw-medium">{{ batch.name }}</div>
                      </td>
                      <td>
                        <div>{{ formatPeriod(batch.period) }}</div>
                        <small class="text-muted">{{ batch.period.workingDays }} working days</small>
                      </td>
                      <td>{{ batch.branchName }}</td>
                      <td>
                        <div>{{ batch.processedEmployees }} / {{ batch.totalEmployees }}</div>
                        <div class="progress mt-1" style="height: 4px;">
                          <div class="progress-bar" 
                               [style.width.%]="getProcessingProgress(batch)"
                               [class]="getProgressBarClass(batch)">
                          </div>
                        </div>
                      </td>
                      <td>
                        <div class="fw-medium">{{ formatCurrency(batch.totalAmount, batch.currency) }}</div>
                      </td>
                      <td>
                        <span class="badge" [class]="getStatusBadgeClass(batch.status)">
                          {{ batch.status }}
                        </span>
                      </td>
                      <td>
                        <div>{{ batch.createdAt | date:'MMM dd, yyyy' }}</div>
                        <small class="text-muted">by {{ batch.createdBy }}</small>
                      </td>
                      <td>
                        <div class="btn-group" ngbDropdown>
                          <button class="btn btn-outline-secondary btn-sm dropdown-toggle" 
                                  ngbDropdownToggle>
                            Actions
                          </button>
                          <div class="dropdown-menu" ngbDropdownMenu>
                            <button class="dropdown-item" (click)="viewBatchDetails(batch)">
                              <i class="fas fa-eye me-2"></i>View Details
                            </button>
                            <button class="dropdown-item" 
                                    *ngIf="canProcessBatch(batch)"
                                    (click)="processBatch(batch)">
                              <i class="fas fa-calculator me-2"></i>Process Payroll
                            </button>
                            <button class="dropdown-item" 
                                    *ngIf="canApproveBatch(batch)"
                                    (click)="approveBatch(batch)">
                              <i class="fas fa-check me-2"></i>Approve
                            </button>
                            <button class="dropdown-item" 
                                    *ngIf="canReleaseBatch(batch)"
                                    (click)="releaseBatch(batch)">
                              <i class="fas fa-paper-plane me-2"></i>Release
                            </button>
                            <div class="dropdown-divider"></div>
                            <button class="dropdown-item" (click)="generatePayslips(batch)">
                              <i class="fas fa-file-pdf me-2"></i>Generate Payslips
                            </button>
                            <button class="dropdown-item" (click)="emailPayslips(batch)">
                              <i class="fas fa-envelope me-2"></i>Email Payslips
                            </button>
                            <div class="dropdown-divider" *ngIf="canDeleteBatch(batch)"></div>
                            <button class="dropdown-item text-danger" 
                                    *ngIf="canDeleteBatch(batch)"
                                    (click)="deleteBatch(batch)">
                              <i class="fas fa-trash me-2"></i>Delete
                            </button>
                          </div>
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create Batch Modal -->
    <ng-template #createBatchModal let-modal>
      <div class="modal-header">
        <h4 class="modal-title">Create New Payroll Batch</h4>
        <button type="button" class="btn-close" (click)="modal.dismiss()"></button>
      </div>
      <form [formGroup]="createBatchForm" (ngSubmit)="createBatch(modal)">
        <div class="modal-body">
          <div class="mb-3">
            <label class="form-label">Batch Name *</label>
            <input type="text" class="form-control" formControlName="name" 
                   placeholder="e.g., January 2024 Payroll">
            <div class="invalid-feedback" 
                 *ngIf="createBatchForm.get('name')?.invalid && createBatchForm.get('name')?.touched">
              Batch name is required
            </div>
          </div>

          <div class="row">
            <div class="col-md-6">
              <label class="form-label">Month *</label>
              <select class="form-select" formControlName="month">
                <option value="">Select Month</option>
                <option *ngFor="let month of months; let i = index" [value]="i + 1">
                  {{ month }}
                </option>
              </select>
            </div>
            <div class="col-md-6">
              <label class="form-label">Year *</label>
              <select class="form-select" formControlName="year">
                <option value="">Select Year</option>
                <option *ngFor="let year of years" [value]="year">
                  {{ year }}
                </option>
              </select>
            </div>
          </div>

          <div class="mb-3 mt-3">
            <label class="form-label">Branch *</label>
            <select class="form-select" formControlName="branchId">
              <option value="">Select Branch</option>
              <option *ngFor="let branch of branches" [value]="branch.id">
                {{ branch.name }}
              </option>
            </select>
          </div>

          <div class="mb-3">
            <label class="form-label">Employees</label>
            <div class="form-check">
              <input class="form-check-input" type="radio" 
                     formControlName="employeeSelection" value="all" id="allEmployees">
              <label class="form-check-label" for="allEmployees">
                All Active Employees
              </label>
            </div>
            <div class="form-check">
              <input class="form-check-input" type="radio" 
                     formControlName="employeeSelection" value="selected" id="selectedEmployees">
              <label class="form-check-label" for="selectedEmployees">
                Selected Employees
              </label>
            </div>
          </div>

          <div *ngIf="createBatchForm.get('employeeSelection')?.value === 'selected'" class="mb-3">
            <label class="form-label">Select Employees</label>
            <div class="employee-selection-container" style="max-height: 200px; overflow-y: auto;">
              <div class="form-check" *ngFor="let employee of employees">
                <input class="form-check-input" type="checkbox" 
                       [value]="employee.id" 
                       (change)="onEmployeeSelectionChange($event, employee.id)"
                       [id]="'emp-' + employee.id">
                <label class="form-check-label" [for]="'emp-' + employee.id">
                  {{ employee.firstName }} {{ employee.lastName }} ({{ employee.employeeId }})
                </label>
              </div>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" (click)="modal.dismiss()">
            Cancel
          </button>
          <button type="submit" class="btn btn-primary" 
                  [disabled]="createBatchForm.invalid || creating">
            <span *ngIf="creating" class="spinner-border spinner-border-sm me-2"></span>
            Create Batch
          </button>
        </div>
      </form>
    </ng-template>
  `,
  styles: [`
    .payroll-processing-container {
      padding: 1.5rem;
    }

    .page-header h1 {
      color: var(--text-primary);
      font-weight: 600;
    }

    .card {
      border: 1px solid var(--gray-200);
      border-radius: 12px;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-tertiary) 100%);
      border-bottom: 1px solid var(--gray-200);
      border-radius: 12px 12px 0 0;
    }

    .table th {
      font-weight: 600;
      color: var(--text-primary);
      background-color: var(--bg-secondary);
      border-bottom: 2px solid var(--gray-200);
    }

    .table td {
      vertical-align: middle;
    }

    .badge {
      font-size: 0.75rem;
      padding: 0.375rem 0.75rem;
    }

    .badge.bg-draft { background-color: #6c757d !important; }
    .badge.bg-processing { background-color: #fd7e14 !important; }
    .badge.bg-calculated { background-color: #0dcaf0 !important; }
    .badge.bg-pending-approval { background-color: #ffc107 !important; color: #000; }
    .badge.bg-approved { background-color: #198754 !important; }
    .badge.bg-released { background-color: #20c997 !important; }
    .badge.bg-failed { background-color: #dc3545 !important; }

    .progress {
      background-color: var(--gray-200);
    }

    .progress-bar.bg-success { background-color: #198754 !important; }
    .progress-bar.bg-warning { background-color: #ffc107 !important; }
    .progress-bar.bg-info { background-color: #0dcaf0 !important; }

    .btn {
      border-radius: 8px;
      font-weight: 500;
    }

    .btn-primary {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      border: none;
    }

    .employee-selection-container {
      border: 1px solid var(--gray-200);
      border-radius: 8px;
      padding: 1rem;
      background-color: var(--bg-secondary);
    }

    .form-check {
      margin-bottom: 0.5rem;
    }

    .spinner-border-sm {
      width: 1rem;
      height: 1rem;
    }

    @media (max-width: 768px) {
      .payroll-processing-container {
        padding: 1rem;
      }
      
      .page-header {
        flex-direction: column;
        gap: 1rem;
      }
      
      .table-responsive {
        font-size: 0.875rem;
      }
    }
  `]
})
export class PayrollProcessingComponent implements OnInit, OnDestroy {
  @ViewChild('createBatchModal') createBatchModal!: TemplateRef<any>;
  
  private destroy$ = new Subject<void>();

  // Data
  payrollBatches: PayrollBatch[] = [];
  branches: any[] = [];
  employees: Employee[] = [];
  
  // Filters
  selectedBranchId: number | null = null;
  selectedStatus: PayrollBatchStatus | null = null;
  selectedPeriod: string | null = null;
  
  // UI State
  loading = false;
  creating = false;
  
  // Form
  createBatchForm: FormGroup;
  selectedEmployeeIds: number[] = [];
  
  // Constants
  payrollStatuses = Object.values(PayrollBatchStatus);
  months = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];
  years = Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - 2 + i);
  availablePeriods = [
    { key: 'current', label: 'Current Month' },
    { key: 'last', label: 'Last Month' },
    { key: 'last3', label: 'Last 3 Months' },
    { key: 'last6', label: 'Last 6 Months' }
  ];

  constructor(
    private payrollService: PayrollService,
    private employeeService: EmployeeService,
    private modalService: ModalService,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.createBatchForm = this.fb.group({
      name: ['', Validators.required],
      month: ['', Validators.required],
      year: ['', Validators.required],
      branchId: ['', Validators.required],
      employeeSelection: ['all', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadInitialData();
    this.setupRealtimeUpdates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadInitialData(): void {
    this.loading = true;
    
    forkJoin({
      batches: this.payrollService.getPayrollBatches(),
      employees: this.employeeService.getEmployees()
    }).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (data) => {
        this.payrollBatches = data.batches;
        this.employees = data.employees.items; // Extract items from PagedResult
        this.extractBranches();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading data:', error);
        this.loading = false;
      }
    });
  }

  private extractBranches(): void {
    const branchMap = new Map();
    this.employees.forEach(emp => {
      if (!branchMap.has(emp.branchId)) {
        branchMap.set(emp.branchId, {
          id: emp.branchId,
          name: emp.branch?.name || `Branch ${emp.branchId}`
        });
      }
    });
    this.branches = Array.from(branchMap.values());
  }

  private setupRealtimeUpdates(): void {
    this.payrollService.payrollUpdates$
      .pipe(takeUntil(this.destroy$))
      .subscribe(update => {
        if (update) {
          this.handleRealtimeUpdate(update);
        }
      });
  }

  private handleRealtimeUpdate(update: any): void {
    switch (update.type) {
      case 'payroll_processed':
      case 'payroll_approved':
      case 'payroll_released':
        this.refreshData();
        break;
    }
  }

  refreshData(): void {
    this.loadInitialData();
  }

  onFilterChange(): void {
    this.loading = true;
    
    this.payrollService.getPayrollBatches(
      this.selectedBranchId || undefined,
      this.selectedStatus || undefined
    ).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (batches) => {
        this.payrollBatches = batches;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error filtering batches:', error);
        this.loading = false;
      }
    });
  }

  clearFilters(): void {
    this.selectedBranchId = null;
    this.selectedStatus = null;
    this.selectedPeriod = null;
    this.onFilterChange();
  }

  openCreateBatchModal(): void {
    this.modalService.openTemplate(this.createBatchModal, { 
      size: 'lg',
      backdrop: 'static'
    });
  }

  onEmployeeSelectionChange(event: any, employeeId: number): void {
    if (event.target.checked) {
      this.selectedEmployeeIds.push(employeeId);
    } else {
      this.selectedEmployeeIds = this.selectedEmployeeIds.filter(id => id !== employeeId);
    }
  }

  createBatch(modal: any): void {
    if (this.createBatchForm.valid) {
      this.creating = true;
      
      const formValue = this.createBatchForm.value;
      const period: PayrollPeriod = {
        month: formValue.month,
        year: formValue.year,
        startDate: new Date(formValue.year, formValue.month - 1, 1),
        endDate: new Date(formValue.year, formValue.month, 0),
        workingDays: 0, // Will be calculated by backend
        actualWorkingDays: 0
      };

      const dto: CreatePayrollBatchDto = {
        name: formValue.name,
        period,
        branchId: formValue.branchId,
        employeeIds: formValue.employeeSelection === 'selected' ? this.selectedEmployeeIds : undefined
      };

      this.payrollService.createPayrollBatch(dto)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (batch) => {
            this.payrollBatches.unshift(batch);
            this.creating = false;
            modal.close();
            this.createBatchForm.reset();
            this.selectedEmployeeIds = [];
          },
          error: (error) => {
            console.error('Error creating batch:', error);
            this.creating = false;
          }
        });
    }
  }

  viewBatchDetails(batch: PayrollBatch): void {
    this.router.navigate(['/payroll/batch', batch.id]);
  }

  processBatch(batch: PayrollBatch): void {
    const dto: ProcessPayrollDto = {
      batchId: batch.id
    };

    this.payrollService.processPayroll(dto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updatedBatch) => {
          const index = this.payrollBatches.findIndex(b => b.id === batch.id);
          if (index !== -1) {
            this.payrollBatches[index] = updatedBatch;
          }
        },
        error: (error) => {
          console.error('Error processing payroll:', error);
        }
      });
  }

  approveBatch(batch: PayrollBatch): void {
    const dto = {
      batchId: batch.id,
      comments: 'Approved via UI'
    };

    this.payrollService.approvePayroll(dto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updatedBatch) => {
          const index = this.payrollBatches.findIndex(b => b.id === batch.id);
          if (index !== -1) {
            this.payrollBatches[index] = updatedBatch;
          }
        },
        error: (error) => {
          console.error('Error approving payroll:', error);
        }
      });
  }

  releaseBatch(batch: PayrollBatch): void {
    this.payrollService.releasePayroll(batch.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updatedBatch) => {
          const index = this.payrollBatches.findIndex(b => b.id === batch.id);
          if (index !== -1) {
            this.payrollBatches[index] = updatedBatch;
          }
        },
        error: (error) => {
          console.error('Error releasing payroll:', error);
        }
      });
  }

  generatePayslips(batch: PayrollBatch): void {
    // Implementation for payslip generation
    console.log('Generate payslips for batch:', batch.id);
  }

  emailPayslips(batch: PayrollBatch): void {
    // Implementation for emailing payslips
    console.log('Email payslips for batch:', batch.id);
  }

  deleteBatch(batch: PayrollBatch): void {
    if (confirm('Are you sure you want to delete this payroll batch?')) {
      this.payrollService.deletePayrollBatch(batch.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.payrollBatches = this.payrollBatches.filter(b => b.id !== batch.id);
          },
          error: (error) => {
            console.error('Error deleting batch:', error);
          }
        });
    }
  }

  exportData(format: 'excel' | 'pdf'): void {
    // Implementation for data export
    console.log('Export data in format:', format);
  }

  // Utility methods
  trackByBatchId(index: number, batch: PayrollBatch): number {
    return batch.id;
  }

  formatPeriod(period: PayrollPeriod): string {
    return `${this.months[period.month - 1]} ${period.year}`;
  }

  formatCurrency(amount: number, currency: string): string {
    return this.payrollService.formatCurrency(amount, currency);
  }

  getProcessingProgress(batch: PayrollBatch): number {
    return batch.totalEmployees > 0 ? (batch.processedEmployees / batch.totalEmployees) * 100 : 0;
  }

  getProgressBarClass(batch: PayrollBatch): string {
    const progress = this.getProcessingProgress(batch);
    if (progress === 100) return 'bg-success';
    if (progress > 50) return 'bg-info';
    return 'bg-warning';
  }

  getStatusBadgeClass(status: PayrollBatchStatus): string {
    const statusClasses = {
      [PayrollBatchStatus.Draft]: 'bg-draft',
      [PayrollBatchStatus.Processing]: 'bg-processing',
      [PayrollBatchStatus.Calculated]: 'bg-calculated',
      [PayrollBatchStatus.PendingApproval]: 'bg-pending-approval',
      [PayrollBatchStatus.Approved]: 'bg-approved',
      [PayrollBatchStatus.Released]: 'bg-released',
      [PayrollBatchStatus.Failed]: 'bg-failed'
    };
    return `badge ${statusClasses[status] || 'bg-secondary'}`;
  }

  canProcessBatch(batch: PayrollBatch): boolean {
    return batch.status === PayrollBatchStatus.Draft || batch.status === PayrollBatchStatus.Failed;
  }

  canApproveBatch(batch: PayrollBatch): boolean {
    return batch.status === PayrollBatchStatus.PendingApproval;
  }

  canReleaseBatch(batch: PayrollBatch): boolean {
    return batch.status === PayrollBatchStatus.Approved;
  }

  canDeleteBatch(batch: PayrollBatch): boolean {
    return batch.status === PayrollBatchStatus.Draft || batch.status === PayrollBatchStatus.Failed;
  }
}