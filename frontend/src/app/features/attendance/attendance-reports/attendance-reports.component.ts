import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AttendanceService } from '../../../services/attendance.service';
import { AttendanceReportRequest, AttendanceReportResponse } from '../../../models/attendance.models';

@Component({
  selector: 'app-attendance-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="container-fluid">
      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h4 class="card-title mb-0">
                <i class="fas fa-chart-bar me-2"></i>
                Attendance Reports & Analytics
              </h4>
            </div>
            <div class="card-body">
              <!-- Report Generation Form -->
              <form [formGroup]="reportForm" (ngSubmit)="generateReport()" class="mb-4">
                <div class="row">
                  <div class="col-md-3">
                    <label class="form-label">Start Date</label>
                    <input type="date" class="form-control" formControlName="startDate" required>
                  </div>
                  <div class="col-md-3">
                    <label class="form-label">End Date</label>
                    <input type="date" class="form-control" formControlName="endDate" required>
                  </div>
                  <div class="col-md-2">
                    <label class="form-label">Report Type</label>
                    <select class="form-select" formControlName="reportType">
                      <option value="summary">Summary</option>
                      <option value="detailed">Detailed</option>
                      <option value="analytics">Analytics</option>
                    </select>
                  </div>
                  <div class="col-md-2">
                    <label class="form-label">Employee</label>
                    <select class="form-select" formControlName="employeeId">
                      <option value="">All Employees</option>
                      <option *ngFor="let emp of employees" [value]="emp.id">
                        {{emp.firstName}} {{emp.lastName}}
                      </option>
                    </select>
                  </div>
                  <div class="col-md-2">
                    <label class="form-label">&nbsp;</label>
                    <div class="d-grid">
                      <button type="submit" class="btn btn-primary" [disabled]="reportForm.invalid || loading">
                        <i class="fas fa-chart-line me-1"></i>
                        <span *ngIf="!loading">Generate</span>
                        <span *ngIf="loading">
                          <span class="spinner-border spinner-border-sm me-1"></span>
                          Generating...
                        </span>
                      </button>
                    </div>
                  </div>
                </div>
                
                <!-- Advanced Filters -->
                <div class="row mt-3" *ngIf="showAdvancedFilters">
                  <div class="col-md-3">
                    <label class="form-label">Department</label>
                    <select class="form-select" formControlName="departmentId">
                      <option value="">All Departments</option>
                      <option *ngFor="let dept of departments" [value]="dept.id">
                        {{dept.name}}
                      </option>
                    </select>
                  </div>
                  <div class="col-md-3">
                    <label class="form-label">Branch</label>
                    <select class="form-select" formControlName="branchId">
                      <option value="">All Branches</option>
                      <option *ngFor="let branch of branches" [value]="branch.id">
                        {{branch.name}}
                      </option>
                    </select>
                  </div>
                  <div class="col-md-6">
                    <label class="form-label">Include Options</label>
                    <div class="form-check-group">
                      <div class="form-check form-check-inline">
                        <input class="form-check-input" type="checkbox" formControlName="includeBreakDetails" id="includeBreaks">
                        <label class="form-check-label" for="includeBreaks">Break Details</label>
                      </div>
                      <div class="form-check form-check-inline">
                        <input class="form-check-input" type="checkbox" formControlName="includeOvertimeDetails" id="includeOvertime">
                        <label class="form-check-label" for="includeOvertime">Overtime Details</label>
                      </div>
                      <div class="form-check form-check-inline">
                        <input class="form-check-input" type="checkbox" formControlName="includeLateArrivals" id="includeLate">
                        <label class="form-check-label" for="includeLate">Late Arrivals</label>
                      </div>
                    </div>
                  </div>
                </div>
                
                <div class="row mt-2">
                  <div class="col-12">
                    <button type="button" class="btn btn-link p-0" (click)="toggleAdvancedFilters()">
                      <i class="fas" [class.fa-chevron-down]="!showAdvancedFilters" [class.fa-chevron-up]="showAdvancedFilters"></i>
                      {{showAdvancedFilters ? 'Hide' : 'Show'}} Advanced Filters
                    </button>
                  </div>
                </div>
              </form>

              <!-- Export Options -->
              <div class="row mb-4" *ngIf="reportData">
                <div class="col-12">
                  <div class="btn-group" role="group">
                    <button type="button" class="btn btn-outline-success" (click)="exportReport('excel')">
                      <i class="fas fa-file-excel me-1"></i>
                      Export to Excel
                    </button>
                    <button type="button" class="btn btn-outline-danger" (click)="exportReport('pdf')">
                      <i class="fas fa-file-pdf me-1"></i>
                      Export to PDF
                    </button>
                    <button type="button" class="btn btn-outline-info" (click)="exportReport('json')">
                      <i class="fas fa-file-code me-1"></i>
                      Export to JSON
                    </button>
                  </div>
                </div>
              </div>

              <!-- Report Summary -->
              <div class="row mb-4" *ngIf="reportData">
                <div class="col-12">
                  <div class="card bg-light">
                    <div class="card-body">
                      <h5 class="card-title">Report Summary</h5>
                      <div class="row">
                        <div class="col-md-2">
                          <div class="text-center">
                            <h4 class="text-primary mb-0">{{reportData.summary.totalEmployees}}</h4>
                            <small class="text-muted">Total Employees</small>
                          </div>
                        </div>
                        <div class="col-md-2">
                          <div class="text-center">
                            <h4 class="text-success mb-0">{{reportData.summary.averageAttendancePercentage}}%</h4>
                            <small class="text-muted">Avg Attendance</small>
                          </div>
                        </div>
                        <div class="col-md-2">
                          <div class="text-center">
                            <h4 class="text-info mb-0">{{formatTimeSpan(reportData.summary.averageWorkingHoursPerDay)}}</h4>
                            <small class="text-muted">Avg Hours/Day</small>
                          </div>
                        </div>
                        <div class="col-md-2">
                          <div class="text-center">
                            <h4 class="text-warning mb-0">{{reportData.summary.totalLateDays}}</h4>
                            <small class="text-muted">Total Late Days</small>
                          </div>
                        </div>
                        <div class="col-md-2">
                          <div class="text-center">
                            <h4 class="text-danger mb-0">{{reportData.summary.totalAbsentDays}}</h4>
                            <small class="text-muted">Total Absent Days</small>
                          </div>
                        </div>
                        <div class="col-md-2">
                          <div class="text-center">
                            <h4 class="text-secondary mb-0">{{formatTimeSpan(reportData.summary.totalOvertimeHours)}}</h4>
                            <small class="text-muted">Total Overtime</small>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Report Data Table -->
              <div class="table-responsive" *ngIf="reportData">
                <table class="table table-hover">
                  <thead class="table-dark">
                    <tr>
                      <th>Employee</th>
                      <th>Department</th>
                      <th>Working Days</th>
                      <th>Present Days</th>
                      <th>Absent Days</th>
                      <th>Late Days</th>
                      <th>Attendance %</th>
                      <th>Total Hours</th>
                      <th>Overtime</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let item of reportData.items">
                      <td>
                        <div class="d-flex align-items-center">
                          <div class="avatar-sm me-2">
                            <div class="avatar-title bg-primary rounded-circle">
                              {{item.employeeName.charAt(0)}}
                            </div>
                          </div>
                          <div>
                            <div class="fw-medium">{{item.employeeName}}</div>
                            <small class="text-muted">{{item.employeeCode}}</small>
                          </div>
                        </div>
                      </td>
                      <td>{{item.department}}</td>
                      <td>{{item.totalWorkingDays}}</td>
                      <td>
                        <span class="badge bg-success">{{item.presentDays}}</span>
                      </td>
                      <td>
                        <span class="badge bg-danger" *ngIf="item.absentDays > 0">{{item.absentDays}}</span>
                        <span *ngIf="item.absentDays === 0">0</span>
                      </td>
                      <td>
                        <span class="badge bg-warning" *ngIf="item.lateDays > 0">{{item.lateDays}}</span>
                        <span *ngIf="item.lateDays === 0">0</span>
                      </td>
                      <td>
                        <div class="progress" style="height: 20px;">
                          <div class="progress-bar" 
                               [class.bg-success]="item.attendancePercentage >= 90"
                               [class.bg-warning]="item.attendancePercentage >= 75 && item.attendancePercentage < 90"
                               [class.bg-danger]="item.attendancePercentage < 75"
                               [style.width.%]="item.attendancePercentage">
                            {{item.attendancePercentage}}%
                          </div>
                        </div>
                      </td>
                      <td>{{formatTimeSpan(item.totalWorkingHours)}}</td>
                      <td>{{formatTimeSpan(item.totalOvertimeHours)}}</td>
                      <td>
                        <div class="btn-group btn-group-sm">
                          <button class="btn btn-outline-primary" (click)="viewEmployeeDetails(item.employeeId)">
                            <i class="fas fa-eye"></i>
                          </button>
                          <button class="btn btn-outline-info" (click)="viewCalendar(item.employeeId)">
                            <i class="fas fa-calendar"></i>
                          </button>
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <!-- No Data Message -->
              <div class="text-center py-5" *ngIf="!reportData && !loading">
                <i class="fas fa-chart-bar fa-3x text-muted mb-3"></i>
                <h5 class="text-muted">No Report Generated</h5>
                <p class="text-muted">Select date range and click "Generate" to create attendance report</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .avatar-sm {
      width: 32px;
      height: 32px;
    }
    
    .avatar-title {
      width: 100%;
      height: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 14px;
      font-weight: 600;
    }
    
    .form-check-group {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
    }
    
    .progress {
      position: relative;
    }
    
    .progress-bar {
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-weight: 500;
      font-size: 12px;
    }
  `]
})
export class AttendanceReportsComponent implements OnInit {
  reportForm: FormGroup;
  reportData: AttendanceReportResponse | null = null;
  loading = false;
  showAdvancedFilters = false;
  
  employees: any[] = [];
  departments: any[] = [];
  branches: any[] = [];

  constructor(
    private fb: FormBuilder,
    private attendanceService: AttendanceService
  ) {
    this.reportForm = this.fb.group({
      startDate: [this.getDefaultStartDate(), Validators.required],
      endDate: [this.getDefaultEndDate(), Validators.required],
      reportType: ['summary'],
      employeeId: [''],
      departmentId: [''],
      branchId: [''],
      includeBreakDetails: [false],
      includeOvertimeDetails: [false],
      includeLateArrivals: [true],
      includeEarlyDepartures: [true]
    });
  }

  ngOnInit() {
    this.loadEmployees();
    this.loadDepartments();
    this.loadBranches();
  }

  private getDefaultStartDate(): string {
    const date = new Date();
    date.setDate(1); // First day of current month
    return date.toISOString().split('T')[0];
  }

  private getDefaultEndDate(): string {
    const date = new Date();
    return date.toISOString().split('T')[0];
  }

  toggleAdvancedFilters() {
    this.showAdvancedFilters = !this.showAdvancedFilters;
  }

  generateReport() {
    if (this.reportForm.invalid) return;

    this.loading = true;
    const request: AttendanceReportRequest = {
      ...this.reportForm.value,
      employeeId: this.reportForm.value.employeeId || null,
      departmentId: this.reportForm.value.departmentId || null,
      branchId: this.reportForm.value.branchId || null
    };

    this.attendanceService.generateAttendanceReport(request)
      .subscribe({
        next: (data) => {
          this.reportData = data;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error generating report:', error);
          this.loading = false;
          // Handle error (show toast notification)
        }
      });
  }

  exportReport(format: string) {
    if (!this.reportData) return;

    const request: AttendanceReportRequest = {
      ...this.reportForm.value,
      format,
      employeeId: this.reportForm.value.employeeId || null,
      departmentId: this.reportForm.value.departmentId || null,
      branchId: this.reportForm.value.branchId || null
    };

    this.attendanceService.exportAttendanceReport(request, format)
      .subscribe({
        next: (blob) => {
          // Create download link
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `attendance-report.${format}`;
          link.click();
          window.URL.revokeObjectURL(url);
        },
        error: (error) => {
          console.error('Error exporting report:', error);
          // Handle error
        }
      });
  }

  viewEmployeeDetails(employeeId: number) {
    // Navigate to employee attendance details
    console.log('View details for employee:', employeeId);
  }

  viewCalendar(employeeId: number) {
    // Navigate to employee attendance calendar
    console.log('View calendar for employee:', employeeId);
  }

  formatTimeSpan(timeSpan: string): string {
    if (!timeSpan) return '00:00';
    
    // Parse TimeSpan format (e.g., "08:30:00")
    const parts = timeSpan.split(':');
    if (parts.length >= 2) {
      return `${parts[0]}:${parts[1]}`;
    }
    return timeSpan;
  }

  private loadEmployees() {
    // Load employees from service
    this.employees = []; // Placeholder - implement when employee service is available
  }

  private loadDepartments() {
    // Load departments from service
    this.departments = []; // Placeholder - implement when department service is available
  }

  private loadBranches() {
    // Load branches from service
    this.branches = []; // Placeholder - implement when branch service is available
  }
}