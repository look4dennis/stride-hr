import { Component, OnInit, OnDestroy, TemplateRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { NgbModal, NgbTooltip, NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';

import { PayrollService } from '../../../services/payroll.service';
import { EmployeeService } from '../../../services/employee.service';
import {
  PayrollBatch,
  PayrollRecord,
  PayrollBatchStatus,
  PayrollApprovalWorkflow,
  ApprovalStatus,
  ApprovePayrollDto
} from '../../../models/payroll.models';

@Component({
  selector: 'app-payroll-approval',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgbTooltip,
    NgbDropdownModule
  ],
  template: `
    <div class="payroll-approval-container">
      <!-- Header -->
      <div class="page-header d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="h3 mb-1">Payroll Approval</h1>
          <p class="text-muted mb-0">Review and approve payroll batches</p>
        </div>
        <div class="d-flex gap-2">
          <button class="btn btn-outline-primary" (click)="refreshData()">
            <i class="fas fa-sync-alt me-2"></i>Refresh
          </button>
        </div>
      </div>

      <!-- Approval Stats -->
      <div class="row mb-4">
        <div class="col-md-3">
          <div class="card dashboard-widget">
            <div class="card-body text-center">
              <i class="fas fa-clock text-warning mb-2" style="font-size: 2rem;"></i>
              <h3 class="widget-value">{{ pendingApprovals }}</h3>
              <p class="text-muted mb-0">Pending Approvals</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card dashboard-widget">
            <div class="card-body text-center">
              <i class="fas fa-check-circle text-success mb-2" style="font-size: 2rem;"></i>
              <h3 class="widget-value">{{ approvedToday }}</h3>
              <p class="text-muted mb-0">Approved Today</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card dashboard-widget">
            <div class="card-body text-center">
              <i class="fas fa-money-bill-wave text-info mb-2" style="font-size: 2rem;"></i>
              <h3 class="widget-value">{{ formatCurrency(totalApprovalAmount, 'USD') }}</h3>
              <p class="text-muted mb-0">Total Amount</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card dashboard-widget">
            <div class="card-body text-center">
              <i class="fas fa-users text-primary mb-2" style="font-size: 2rem;"></i>
              <h3 class="widget-value">{{ totalEmployeesForApproval }}</h3>
              <p class="text-muted mb-0">Employees</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Filters -->
      <div class="card mb-4">
        <div class="card-body">
          <div class="row g-3">
            <div class="col-md-3">
              <label class="form-label">Status</label>
              <select class="form-select" [(ngModel)]="selectedStatus" (change)="onFilterChange()">
                <option value="">All Status</option>
                <option value="PendingApproval">Pending Approval</option>
                <option value="Approved">Approved</option>
                <option value="Rejected">Rejected</option>
              </select>
            </div>
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
              <label class="form-label">Priority</label>
              <select class="form-select" [(ngModel)]="selectedPriority" (change)="onFilterChange()">
                <option value="">All Priorities</option>
                <option value="high">High Priority</option>
                <option value="normal">Normal Priority</option>
                <option value="low">Low Priority</option>
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

      <!-- Approval Queue -->
      <div class="card">
        <div class="card-header d-flex justify-content-between align-items-center">
          <h5 class="card-title mb-0">Approval Queue</h5>
          <div class="d-flex gap-2">
            <button class="btn btn-success btn-sm" 
                    [disabled]="selectedBatches.length === 0"
                    (click)="bulkApprove()">
              <i class="fas fa-check me-2"></i>Bulk Approve ({{ selectedBatches.length }})
            </button>
          </div>
        </div>
        <div class="card-body">
          <div *ngIf="loading" class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-2 text-muted">Loading approval queue...</p>
          </div>

          <div *ngIf="!loading && approvalBatches.length === 0" class="text-center py-5">
            <i class="fas fa-clipboard-check text-muted mb-3" style="font-size: 3rem;"></i>
            <h4>No Payroll Batches for Approval</h4>
            <p class="text-muted">All payroll batches are up to date.</p>
          </div>

          <div *ngIf="!loading && approvalBatches.length > 0" class="table-responsive">
            <table class="table table-hover">
              <thead>
                <tr>
                  <th>
                    <input type="checkbox" class="form-check-input" 
                           (change)="toggleSelectAll($event)"
                           [checked]="allSelected">
                  </th>
                  <th>Batch Name</th>
                  <th>Period</th>
                  <th>Branch</th>
                  <th>Employees</th>
                  <th>Total Amount</th>
                  <th>Priority</th>
                  <th>Submitted</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let batch of approvalBatches; trackBy: trackByBatchId">
                  <td>
                    <input type="checkbox" class="form-check-input" 
                           [checked]="selectedBatches.includes(batch.id)"
                           (change)="toggleBatchSelection(batch.id, $event)">
                  </td>
                  <td>
                    <div class="fw-medium">{{ batch.name }}</div>
                    <small class="text-muted">{{ batch.id }}</small>
                  </td>
                  <td>
                    <div>{{ formatPeriod(batch.period) }}</div>
                    <small class="text-muted">{{ batch.period.workingDays }} working days</small>
                  </td>
                  <td>{{ batch.branchName }}</td>
                  <td>
                    <div>{{ batch.processedEmployees }} employees</div>
                    <div class="progress mt-1" style="height: 4px;">
                      <div class="progress-bar bg-success" 
                           [style.width.%]="100">
                      </div>
                    </div>
                  </td>
                  <td>
                    <div class="fw-medium">{{ formatCurrency(batch.totalAmount, batch.currency) }}</div>
                  </td>
                  <td>
                    <span class="badge" [class]="getPriorityBadgeClass(batch)">
                      {{ getPriority(batch) }}
                    </span>
                  </td>
                  <td>
                    <div>{{ batch.processedAt | date:'MMM dd, yyyy' }}</div>
                    <small class="text-muted">{{ batch.processedAt | date:'HH:mm' }}</small>
                  </td>
                  <td>
                    <span class="badge" [class]="getStatusBadgeClass(batch.status)">
                      {{ batch.status }}
                    </span>
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
                        <button class="dropdown-item" (click)="reviewPayroll(batch)">
                          <i class="fas fa-search me-2"></i>Review Payroll
                        </button>
                        <div class="dropdown-divider"></div>
                        <button class="dropdown-item text-success" 
                                *ngIf="canApprove(batch)"
                                (click)="approveBatch(batch)">
                          <i class="fas fa-check me-2"></i>Approve
                        </button>
                        <button class="dropdown-item text-danger" 
                                *ngIf="canReject(batch)"
                                (click)="rejectBatch(batch)">
                          <i class="fas fa-times me-2"></i>Reject
                        </button>
                        <div class="dropdown-divider"></div>
                        <button class="dropdown-item" (click)="viewApprovalHistory(batch)">
                          <i class="fas fa-history me-2"></i>Approval History
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

    <!-- Approval Modal -->
    <ng-template #approvalModal let-modal>
      <div class="modal-header">
        <h4 class="modal-title">Approve Payroll Batch</h4>
        <button type="button" class="btn-close" (click)="modal.dismiss()"></button>
      </div>
      <form [formGroup]="approvalForm" (ngSubmit)="submitApproval(modal)">
        <div class="modal-body">
          <div class="alert alert-info">
            <i class="fas fa-info-circle me-2"></i>
            You are about to approve payroll batch: <strong>{{ selectedBatch?.name }}</strong>
          </div>

          <div class="row mb-3">
            <div class="col-md-6">
              <strong>Total Amount:</strong><br>
              <span class="h5 text-success">{{ formatCurrency(selectedBatch?.totalAmount || 0, selectedBatch?.currency || 'USD') }}</span>
            </div>
            <div class="col-md-6">
              <strong>Employees:</strong><br>
              <span class="h5 text-primary">{{ selectedBatch?.processedEmployees }}</span>
            </div>
          </div>

          <div class="mb-3">
            <label class="form-label">Approval Comments</label>
            <textarea class="form-control" formControlName="comments" rows="4"
                      placeholder="Add any comments or notes for this approval..."></textarea>
          </div>

          <div class="form-check">
            <input class="form-check-input" type="checkbox" formControlName="confirmApproval" id="confirmApproval">
            <label class="form-check-label" for="confirmApproval">
              I confirm that I have reviewed the payroll details and approve this batch for release
            </label>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" (click)="modal.dismiss()">
            Cancel
          </button>
          <button type="submit" class="btn btn-success" 
                  [disabled]="approvalForm.invalid || approving">
            <span *ngIf="approving" class="spinner-border spinner-border-sm me-2"></span>
            <i class="fas fa-check me-2" *ngIf="!approving"></i>
            Approve Batch
          </button>
        </div>
      </form>
    </ng-template>

    <!-- Rejection Modal -->
    <ng-template #rejectionModal let-modal>
      <div class="modal-header">
        <h4 class="modal-title">Reject Payroll Batch</h4>
        <button type="button" class="btn-close" (click)="modal.dismiss()"></button>
      </div>
      <form [formGroup]="rejectionForm" (ngSubmit)="submitRejection(modal)">
        <div class="modal-body">
          <div class="alert alert-warning">
            <i class="fas fa-exclamation-triangle me-2"></i>
            You are about to reject payroll batch: <strong>{{ selectedBatch?.name }}</strong>
          </div>

          <div class="mb-3">
            <label class="form-label">Rejection Reason *</label>
            <select class="form-select" formControlName="reason">
              <option value="">Select a reason</option>
              <option value="calculation_error">Calculation Error</option>
              <option value="missing_data">Missing Data</option>
              <option value="policy_violation">Policy Violation</option>
              <option value="documentation_incomplete">Documentation Incomplete</option>
              <option value="other">Other</option>
            </select>
          </div>

          <div class="mb-3">
            <label class="form-label">Detailed Comments *</label>
            <textarea class="form-control" formControlName="comments" rows="4"
                      placeholder="Please provide detailed comments explaining the rejection..."></textarea>
          </div>

          <div class="form-check">
            <input class="form-check-input" type="checkbox" formControlName="notifyHR" id="notifyHR" checked>
            <label class="form-check-label" for="notifyHR">
              Notify HR team about this rejection
            </label>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" (click)="modal.dismiss()">
            Cancel
          </button>
          <button type="submit" class="btn btn-danger" 
                  [disabled]="rejectionForm.invalid || rejecting">
            <span *ngIf="rejecting" class="spinner-border spinner-border-sm me-2"></span>
            <i class="fas fa-times me-2" *ngIf="!rejecting"></i>
            Reject Batch
          </button>
        </div>
      </form>
    </ng-template>
  `,
  styles: [`
    .payroll-approval-container {
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

    .dashboard-widget {
      transition: all 0.2s ease-in-out;
    }

    .dashboard-widget:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
    }

    .widget-value {
      font-size: 2rem;
      font-weight: 700;
      color: var(--primary);
      margin-bottom: 0.5rem;
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

    .badge.bg-pending { background-color: #ffc107 !important; color: #000; }
    .badge.bg-approved { background-color: #198754 !important; }
    .badge.bg-rejected { background-color: #dc3545 !important; }
    .badge.bg-high { background-color: #dc3545 !important; }
    .badge.bg-normal { background-color: #0dcaf0 !important; }
    .badge.bg-low { background-color: #6c757d !important; }

    .progress {
      background-color: var(--gray-200);
    }

    .btn {
      border-radius: 8px;
      font-weight: 500;
    }

    .btn-primary {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      border: none;
    }

    .spinner-border-sm {
      width: 1rem;
      height: 1rem;
    }

    .alert {
      border-radius: 8px;
    }

    @media (max-width: 768px) {
      .payroll-approval-container {
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
export class PayrollApprovalComponent implements OnInit, OnDestroy {
  @ViewChild('approvalModal') approvalModal!: TemplateRef<any>;
  @ViewChild('rejectionModal') rejectionModal!: TemplateRef<any>;
  
  private destroy$ = new Subject<void>();

  // Data
  approvalBatches: PayrollBatch[] = [];
  branches: any[] = [];
  selectedBatch: PayrollBatch | null = null;
  selectedBatches: number[] = [];
  
  // Stats
  pendingApprovals = 0;
  approvedToday = 0;
  totalApprovalAmount = 0;
  totalEmployeesForApproval = 0;
  
  // Filters
  selectedStatus: string | null = null;
  selectedBranchId: number | null = null;
  selectedPriority: string | null = null;
  
  // UI State
  loading = false;
  approving = false;
  rejecting = false;
  allSelected = false;
  
  // Forms
  approvalForm: FormGroup;
  rejectionForm: FormGroup;

  constructor(
    private payrollService: PayrollService,
    private employeeService: EmployeeService,
    private modalService: NgbModal,
    private fb: FormBuilder
  ) {
    this.approvalForm = this.fb.group({
      comments: [''],
      confirmApproval: [false, [Validators.requiredTrue]]
    });

    this.rejectionForm = this.fb.group({
      reason: ['', [Validators.required]],
      comments: ['', [Validators.required]],
      notifyHR: [true]
    });
  }

  ngOnInit(): void {
    this.loadApprovalData();
    this.setupRealtimeUpdates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadApprovalData(): void {
    this.loading = true;
    
    forkJoin({
      batches: this.payrollService.getPayrollBatches(undefined, PayrollBatchStatus.PendingApproval),
      employees: this.employeeService.getEmployees()
    }).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (data) => {
        this.approvalBatches = data.batches;
        this.extractBranches(data.employees.items);
        this.updateStats();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading approval data:', error);
        this.loading = false;
      }
    });
  }

  private extractBranches(employees: any[]): void {
    const branchMap = new Map();
    employees.forEach(emp => {
      if (!branchMap.has(emp.branchId)) {
        branchMap.set(emp.branchId, {
          id: emp.branchId,
          name: emp.branch?.name || `Branch ${emp.branchId}`
        });
      }
    });
    this.branches = Array.from(branchMap.values());
  }

  private updateStats(): void {
    this.pendingApprovals = this.approvalBatches.filter(b => 
      b.status === PayrollBatchStatus.PendingApproval
    ).length;
    
    this.approvedToday = this.approvalBatches.filter(b => 
      b.approvedAt && new Date(b.approvedAt).toDateString() === new Date().toDateString()
    ).length;
    
    this.totalApprovalAmount = this.approvalBatches.reduce((sum, batch) => sum + batch.totalAmount, 0);
    this.totalEmployeesForApproval = this.approvalBatches.reduce((sum, batch) => sum + batch.processedEmployees, 0);
  }

  private setupRealtimeUpdates(): void {
    this.payrollService.payrollUpdates$
      .pipe(takeUntil(this.destroy$))
      .subscribe(update => {
        if (update && (update.type === 'payroll_approved' || update.type === 'payroll_rejected')) {
          this.refreshData();
        }
      });
  }

  refreshData(): void {
    this.loadApprovalData();
  }

  onFilterChange(): void {
    this.loading = true;
    
    this.payrollService.getPayrollBatches(
      this.selectedBranchId || undefined,
      PayrollBatchStatus.PendingApproval
    ).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (batches) => {
        this.approvalBatches = batches.filter(batch => {
          if (this.selectedStatus && batch.status !== this.selectedStatus) return false;
          if (this.selectedPriority && this.getPriority(batch).toLowerCase() !== this.selectedPriority) return false;
          return true;
        });
        this.updateStats();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error filtering batches:', error);
        this.loading = false;
      }
    });
  }

  clearFilters(): void {
    this.selectedStatus = null;
    this.selectedBranchId = null;
    this.selectedPriority = null;
    this.onFilterChange();
  }

  toggleSelectAll(event: any): void {
    this.allSelected = event.target.checked;
    if (this.allSelected) {
      this.selectedBatches = this.approvalBatches.map(batch => batch.id);
    } else {
      this.selectedBatches = [];
    }
  }

  toggleBatchSelection(batchId: number, event: any): void {
    if (event.target.checked) {
      this.selectedBatches.push(batchId);
    } else {
      this.selectedBatches = this.selectedBatches.filter(id => id !== batchId);
    }
    this.allSelected = this.selectedBatches.length === this.approvalBatches.length;
  }

  bulkApprove(): void {
    if (this.selectedBatches.length === 0) return;
    
    const confirmMessage = `Are you sure you want to approve ${this.selectedBatches.length} payroll batches?`;
    if (confirm(confirmMessage)) {
      // Implementation for bulk approval
      console.log('Bulk approving batches:', this.selectedBatches);
    }
  }

  viewBatchDetails(batch: PayrollBatch): void {
    // Implementation for viewing batch details
    console.log('View batch details:', batch.id);
  }

  reviewPayroll(batch: PayrollBatch): void {
    // Implementation for reviewing payroll details
    console.log('Review payroll:', batch.id);
  }

  approveBatch(batch: PayrollBatch): void {
    this.selectedBatch = batch;
    this.modalService.open(this.approvalModal, { 
      size: 'lg',
      backdrop: 'static'
    });
  }

  rejectBatch(batch: PayrollBatch): void {
    this.selectedBatch = batch;
    this.modalService.open(this.rejectionModal, { 
      size: 'lg',
      backdrop: 'static'
    });
  }

  submitApproval(modal: any): void {
    if (this.approvalForm.valid && this.selectedBatch) {
      this.approving = true;
      
      const dto: ApprovePayrollDto = {
        batchId: this.selectedBatch.id,
        comments: this.approvalForm.value.comments
      };

      this.payrollService.approvePayroll(dto)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (updatedBatch) => {
            const index = this.approvalBatches.findIndex(b => b.id === this.selectedBatch!.id);
            if (index !== -1) {
              this.approvalBatches[index] = updatedBatch;
            }
            this.approving = false;
            modal.close();
            this.approvalForm.reset();
            this.selectedBatch = null;
            this.updateStats();
          },
          error: (error) => {
            console.error('Error approving payroll:', error);
            this.approving = false;
          }
        });
    }
  }

  submitRejection(modal: any): void {
    if (this.rejectionForm.valid && this.selectedBatch) {
      this.rejecting = true;
      
      // Implementation for rejecting payroll
      console.log('Rejecting batch:', this.selectedBatch.id, this.rejectionForm.value);
      
      // Simulate API call
      setTimeout(() => {
        this.rejecting = false;
        modal.close();
        this.rejectionForm.reset();
        this.selectedBatch = null;
      }, 1000);
    }
  }

  viewApprovalHistory(batch: PayrollBatch): void {
    // Implementation for viewing approval history
    console.log('View approval history:', batch.id);
  }

  // Utility methods
  trackByBatchId(index: number, batch: PayrollBatch): number {
    return batch.id;
  }

  formatPeriod(period: any): string {
    const months = [
      'January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December'
    ];
    return `${months[period.month - 1]} ${period.year}`;
  }

  formatCurrency(amount: number, currency: string): string {
    return this.payrollService.formatCurrency(amount, currency);
  }

  getPriority(batch: PayrollBatch): string {
    const amount = batch.totalAmount;
    if (amount > 100000) return 'High';
    if (amount > 50000) return 'Normal';
    return 'Low';
  }

  getPriorityBadgeClass(batch: PayrollBatch): string {
    const priority = this.getPriority(batch).toLowerCase();
    return `badge bg-${priority}`;
  }

  getStatusBadgeClass(status: PayrollBatchStatus): string {
    const statusClasses: Record<PayrollBatchStatus, string> = {
      [PayrollBatchStatus.Draft]: 'bg-secondary',
      [PayrollBatchStatus.Processing]: 'bg-warning',
      [PayrollBatchStatus.Calculated]: 'bg-info',
      [PayrollBatchStatus.PendingApproval]: 'bg-pending',
      [PayrollBatchStatus.Approved]: 'bg-approved',
      [PayrollBatchStatus.Released]: 'bg-approved',
      [PayrollBatchStatus.Failed]: 'bg-danger'
    };
    return `badge ${statusClasses[status] || 'bg-secondary'}`;
  }

  canApprove(batch: PayrollBatch): boolean {
    return batch.status === PayrollBatchStatus.PendingApproval;
  }

  canReject(batch: PayrollBatch): boolean {
    return batch.status === PayrollBatchStatus.PendingApproval;
  }
}