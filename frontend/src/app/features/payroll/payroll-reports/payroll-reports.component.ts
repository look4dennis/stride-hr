import { Component, OnInit, OnDestroy, TemplateRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { NgbModal, NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';

import { PayrollService } from '../../../services/payroll.service';
import { EmployeeService } from '../../../services/employee.service';
import {
  PayrollReport,
  PayrollPeriod,
  PayrollReportType
} from '../../../models/payroll.models';
import { Employee } from '../../../models/employee.models';

@Component({
  selector: 'app-payroll-reports',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,

    NgbDropdownModule
  ],
  template: `
    <div class="payroll-reports-container">
      <!-- Header -->
      <div class="page-header d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="h3 mb-1">Payroll Reports</h1>
          <p class="text-muted mb-0">Generate and view payroll reports and analytics</p>
        </div>
        <div class="d-flex gap-2">
          <button class="btn btn-outline-primary" (click)="refreshData()">
            <i class="fas fa-sync-alt me-2"></i>Refresh
          </button>
          <button class="btn btn-primary" (click)="generateNewReport()">
            <i class="fas fa-plus me-2"></i>Generate Report
          </button>
        </div>
      </div>

      <!-- Filters -->
      <div class="card mb-4">
        <div class="card-body">
          <form [formGroup]="filterForm" (ngSubmit)="applyFilters()">
            <div class="row g-3">
              <div class="col-md-3">
                <label class="form-label">Report Type</label>
                <select class="form-select" formControlName="reportType">
                  <option value="">All Types</option>
                  <option *ngFor="let type of reportTypes" [value]="type">
                    {{ getReportTypeLabel(type) }}
                  </option>
                </select>
              </div>
              <div class="col-md-2">
                <label class="form-label">Month</label>
                <select class="form-select" formControlName="month">
                  <option value="">All Months</option>
                  <option *ngFor="let month of months; let i = index" [value]="i + 1">
                    {{ month }}
                  </option>
                </select>
              </div>
              <div class="col-md-2">
                <label class="form-label">Year</label>
                <select class="form-select" formControlName="year">
                  <option value="">All Years</option>
                  <option *ngFor="let year of years" [value]="year">
                    {{ year }}
                  </option>
                </select>
              </div>
              <div class="col-md-3">
                <label class="form-label">Branch</label>
                <select class="form-select" formControlName="branchId">
                  <option value="">All Branches</option>
                  <option *ngFor="let branch of branches" [value]="branch.id">
                    {{ branch.name }}
                  </option>
                </select>
              </div>
              <div class="col-md-2 d-flex align-items-end">
                <button type="submit" class="btn btn-outline-secondary w-100">
                  <i class="fas fa-filter me-2"></i>Apply
                </button>
              </div>
            </div>
          </form>
        </div>
      </div>

      <!-- Quick Stats -->
      <div class="row mb-4">
        <div class="col-md-3">
          <div class="card dashboard-widget">
            <div class="card-body text-center">
              <i class="fas fa-file-alt text-primary mb-2" style="font-size: 2rem;"></i>
              <h3 class="widget-value">{{ totalReports }}</h3>
              <p class="text-muted mb-0">Total Reports</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card dashboard-widget">
            <div class="card-body text-center">
              <i class="fas fa-calendar-month text-success mb-2" style="font-size: 2rem;"></i>
              <h3 class="widget-value">{{ currentMonthReports }}</h3>
              <p class="text-muted mb-0">This Month</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card dashboard-widget">
            <div class="card-body text-center">
              <i class="fas fa-money-bill-wave text-warning mb-2" style="font-size: 2rem;"></i>
              <h3 class="widget-value">{{ formatCurrency(totalPayrollAmount) }}</h3>
              <p class="text-muted mb-0">Total Payroll</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card dashboard-widget">
            <div class="card-body text-center">
              <i class="fas fa-users text-info mb-2" style="font-size: 2rem;"></i>
              <h3 class="widget-value">{{ totalEmployees }}</h3>
              <p class="text-muted mb-0">Employees</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Reports List -->
      <div class="card">
        <div class="card-header d-flex justify-content-between align-items-center">
          <h5 class="card-title mb-0">Generated Reports</h5>
          <div class="d-flex gap-2">
            <div class="btn-group" ngbDropdown>
              <button class="btn btn-outline-secondary btn-sm dropdown-toggle" ngbDropdownToggle>
                <i class="fas fa-download me-2"></i>Bulk Export
              </button>
              <div class="dropdown-menu" ngbDropdownMenu>
                <button class="dropdown-item" (click)="bulkExport('pdf')">
                  <i class="fas fa-file-pdf me-2"></i>PDF
                </button>
                <button class="dropdown-item" (click)="bulkExport('excel')">
                  <i class="fas fa-file-excel me-2"></i>Excel
                </button>
                <button class="dropdown-item" (click)="bulkExport('csv')">
                  <i class="fas fa-file-csv me-2"></i>CSV
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
            <p class="mt-2 text-muted">Loading reports...</p>
          </div>

          <div *ngIf="!loading && reports.length === 0" class="text-center py-5">
            <i class="fas fa-chart-bar text-muted mb-3" style="font-size: 3rem;"></i>
            <h4>No Reports Found</h4>
            <p class="text-muted">Generate your first payroll report to get started.</p>
            <button class="btn btn-primary" (click)="generateNewReport()">
              <i class="fas fa-plus me-2"></i>Generate Report
            </button>
          </div>

          <div *ngIf="!loading && reports.length > 0" class="table-responsive">
            <table class="table table-hover">
              <thead>
                <tr>
                  <th>Report Name</th>
                  <th>Type</th>
                  <th>Period</th>
                  <th>Branch</th>
                  <th>Generated</th>
                  <th>Generated By</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let report of reports; trackBy: trackByReportId">
                  <td>
                    <div class="fw-medium">{{ report.name }}</div>
                  </td>
                  <td>
                    <span class="badge bg-primary">{{ getReportTypeLabel(report.type) }}</span>
                  </td>
                  <td>
                    <div>{{ formatPeriod(report.period) }}</div>
                  </td>
                  <td>{{ getBranchName(report.branchId) }}</td>
                  <td>
                    <div>{{ report.generatedAt | date:'MMM dd, yyyy' }}</div>
                    <small class="text-muted">{{ report.generatedAt | date:'HH:mm' }}</small>
                  </td>
                  <td>{{ report.generatedBy }}</td>
                  <td>
                    <div class="btn-group" ngbDropdown>
                      <button class="btn btn-outline-secondary btn-sm dropdown-toggle" 
                              ngbDropdownToggle>
                        Actions
                      </button>
                      <div class="dropdown-menu" ngbDropdownMenu>
                        <button class="dropdown-item" (click)="viewReport(report)">
                          <i class="fas fa-eye me-2"></i>View
                        </button>
                        <div class="dropdown-divider"></div>
                        <button class="dropdown-item" (click)="exportReport(report, 'pdf')">
                          <i class="fas fa-file-pdf me-2"></i>Export PDF
                        </button>
                        <button class="dropdown-item" (click)="exportReport(report, 'excel')">
                          <i class="fas fa-file-excel me-2"></i>Export Excel
                        </button>
                        <button class="dropdown-item" (click)="exportReport(report, 'csv')">
                          <i class="fas fa-file-csv me-2"></i>Export CSV
                        </button>
                        <div class="dropdown-divider"></div>
                        <button class="dropdown-item" (click)="shareReport(report)">
                          <i class="fas fa-share me-2"></i>Share
                        </button>
                        <button class="dropdown-item text-danger" (click)="deleteReport(report)">
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

    <!-- Generate Report Modal -->
    <ng-template #generateReportModal let-modal>
      <div class="modal-header">
        <h4 class="modal-title">Generate New Report</h4>
        <button type="button" class="btn-close" (click)="modal.dismiss()"></button>
      </div>
      <form [formGroup]="generateReportForm" (ngSubmit)="generateReport(modal)">
        <div class="modal-body">
          <div class="mb-3">
            <label class="form-label">Report Name *</label>
            <input type="text" class="form-control" formControlName="name" 
                   placeholder="e.g., January 2024 Payroll Summary">
          </div>

          <div class="mb-3">
            <label class="form-label">Report Type *</label>
            <select class="form-select" formControlName="type">
              <option value="">Select Report Type</option>
              <option *ngFor="let type of reportTypes" [value]="type">
                {{ getReportTypeLabel(type) }}
              </option>
            </select>
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
            <label class="form-label">Branch</label>
            <select class="form-select" formControlName="branchId">
              <option value="">All Branches</option>
              <option *ngFor="let branch of branches" [value]="branch.id">
                {{ branch.name }}
              </option>
            </select>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" (click)="modal.dismiss()">
            Cancel
          </button>
          <button type="submit" class="btn btn-primary" 
                  [disabled]="generateReportForm.invalid || generating">
            <span *ngIf="generating" class="spinner-border spinner-border-sm me-2"></span>
            Generate Report
          </button>
        </div>
      </form>
    </ng-template>
  `,
  styles: [`
    .payroll-reports-container {
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

    @media (max-width: 768px) {
      .payroll-reports-container {
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
export class PayrollReportsComponent implements OnInit, OnDestroy {
  @ViewChild('generateReportModal') generateReportModal!: TemplateRef<any>;
  
  private destroy$ = new Subject<void>();

  // Data
  reports: PayrollReport[] = [];
  branches: any[] = [];
  
  // Stats
  totalReports = 0;
  currentMonthReports = 0;
  totalPayrollAmount = 0;
  totalEmployees = 0;
  
  // UI State
  loading = false;
  generating = false;
  
  // Forms
  filterForm: FormGroup;
  generateReportForm: FormGroup;
  
  // Constants
  reportTypes = Object.values(PayrollReportType);
  months = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];
  years = Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - 2 + i);

  constructor(
    private payrollService: PayrollService,
    private employeeService: EmployeeService,
    private modalService: NgbModal,
    private fb: FormBuilder
  ) {
    this.filterForm = this.fb.group({
      reportType: [''],
      month: [''],
      year: [''],
      branchId: ['']
    });

    this.generateReportForm = this.fb.group({
      name: ['', [Validators.required]],
      type: ['', [Validators.required]],
      month: ['', [Validators.required]],
      year: ['', [Validators.required]],
      branchId: ['']
    });
  }

  ngOnInit(): void {
    this.loadInitialData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadInitialData(): void {
    this.loading = true;
    
    forkJoin({
      reports: this.payrollService.getPayrollReports(),
      employees: this.employeeService.getEmployees(),
      analytics: this.payrollService.getPayrollAnalytics(this.payrollService.getCurrentPayrollPeriod())
    }).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (data) => {
        this.reports = data.reports;
        this.extractBranches(data.employees.items);
        this.updateStats(data.analytics);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading data:', error);
        this.loading = false;
      }
    });
  }

  private extractBranches(employees: Employee[]): void {
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

  private updateStats(analytics: any): void {
    this.totalReports = this.reports.length;
    this.currentMonthReports = this.reports.filter(r => 
      r.period.month === new Date().getMonth() + 1 && 
      r.period.year === new Date().getFullYear()
    ).length;
    this.totalPayrollAmount = analytics?.totalPayroll || 0;
    this.totalEmployees = analytics?.totalEmployees || 0;
  }

  refreshData(): void {
    this.loadInitialData();
  }

  applyFilters(): void {
    const filters = this.filterForm.value;
    this.loading = true;

    const period: PayrollPeriod | undefined = filters.month && filters.year ? {
      month: filters.month,
      year: filters.year,
      startDate: new Date(filters.year, filters.month - 1, 1),
      endDate: new Date(filters.year, filters.month, 0),
      workingDays: 0,
      actualWorkingDays: 0
    } : undefined;

    this.payrollService.getPayrollReports(period, filters.branchId || undefined)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (reports) => {
          this.reports = reports.filter(r => 
            !filters.reportType || r.type === filters.reportType
          );
          this.loading = false;
        },
        error: (error) => {
          console.error('Error filtering reports:', error);
          this.loading = false;
        }
      });
  }

  generateNewReport(): void {
    this.modalService.open(this.generateReportModal, { 
      size: 'lg',
      backdrop: 'static'
    });
  }

  generateReport(modal: any): void {
    if (this.generateReportForm.valid) {
      this.generating = true;
      
      const formValue = this.generateReportForm.value;
      const period: PayrollPeriod = {
        month: formValue.month,
        year: formValue.year,
        startDate: new Date(formValue.year, formValue.month - 1, 1),
        endDate: new Date(formValue.year, formValue.month, 0),
        workingDays: 0,
        actualWorkingDays: 0
      };

      this.payrollService.generatePayrollReport(
        formValue.type,
        period,
        formValue.branchId || undefined
      ).pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (report) => {
          this.reports.unshift(report);
          this.generating = false;
          modal.close();
          this.generateReportForm.reset();
        },
        error: (error) => {
          console.error('Error generating report:', error);
          this.generating = false;
        }
      });
    }
  }

  viewReport(report: PayrollReport): void {
    // Implementation for viewing report details
    console.log('View report:', report.id);
  }

  exportReport(report: PayrollReport, format: 'pdf' | 'excel' | 'csv'): void {
    this.payrollService.exportPayrollReport(report.id, format)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `${report.name}.${format}`;
          link.click();
          window.URL.revokeObjectURL(url);
        },
        error: (error) => {
          console.error('Error exporting report:', error);
        }
      });
  }

  shareReport(report: PayrollReport): void {
    // Implementation for sharing report
    console.log('Share report:', report.id);
  }

  deleteReport(report: PayrollReport): void {
    if (confirm('Are you sure you want to delete this report?')) {
      // Implementation for deleting report
      console.log('Delete report:', report.id);
    }
  }

  bulkExport(format: 'pdf' | 'excel' | 'csv'): void {
    // Implementation for bulk export
    console.log('Bulk export in format:', format);
  }

  // Utility methods
  trackByReportId(index: number, report: PayrollReport): number {
    return report.id;
  }

  formatPeriod(period: PayrollPeriod): string {
    return `${this.months[period.month - 1]} ${period.year}`;
  }

  formatCurrency(amount: number): string {
    return this.payrollService.formatCurrency(amount, 'USD');
  }

  getReportTypeLabel(type: PayrollReportType): string {
    const labels = {
      [PayrollReportType.PayrollSummary]: 'Payroll Summary',
      [PayrollReportType.TaxReport]: 'Tax Report',
      [PayrollReportType.StatutoryReport]: 'Statutory Report',
      [PayrollReportType.DepartmentWise]: 'Department Wise',
      [PayrollReportType.BranchWise]: 'Branch Wise'
    };
    return labels[type] || type;
  }

  getBranchName(branchId?: number): string {
    if (!branchId) return 'All Branches';
    const branch = this.branches.find(b => b.id === branchId);
    return branch?.name || `Branch ${branchId}`;
  }
}