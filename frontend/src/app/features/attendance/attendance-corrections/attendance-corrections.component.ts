import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AttendanceService } from '../../../services/attendance.service';
import { AttendanceRecord, AttendanceCorrectionRequest, AddMissingAttendanceRequest } from '../../../models/attendance.models';

@Component({
  selector: 'app-attendance-corrections',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="container-fluid">
      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <div class="d-flex justify-content-between align-items-center">
                <h4 class="card-title mb-0">
                  <i class="fas fa-edit me-2"></i>
                  Attendance Corrections
                </h4>
                <div class="d-flex gap-2">
                  <button class="btn btn-success" (click)="showAddMissingModal()">
                    <i class="fas fa-plus me-1"></i>
                    Add Missing Attendance
                  </button>
                  <button class="btn btn-primary" (click)="loadPendingCorrections()">
                    <i class="fas fa-refresh me-1"></i>
                    Refresh
                  </button>
                </div>
              </div>
            </div>
            
            <div class="card-body">
              <!-- Filters -->
              <div class="row mb-4">
                <div class="col-md-3">
                  <label class="form-label">Branch</label>
                  <select class="form-select" [(ngModel)]="selectedBranchId" (change)="loadPendingCorrections()">
                    <option value="">All Branches</option>
                    <option *ngFor="let branch of branches" [value]="branch.id">
                      {{branch.name}}
                    </option>
                  </select>
                </div>
                <div class="col-md-3">
                  <label class="form-label">Start Date</label>
                  <input type="date" class="form-control" [(ngModel)]="startDate" (change)="loadPendingCorrections()">
                </div>
                <div class="col-md-3">
                  <label class="form-label">End Date</label>
                  <input type="date" class="form-control" [(ngModel)]="endDate" (change)="loadPendingCorrections()">
                </div>
                <div class="col-md-3">
                  <label class="form-label">Filter</label>
                  <select class="form-select" [(ngModel)]="filterType" (change)="applyFilter()">
                    <option value="all">All Records</option>
                    <option value="missing_checkin">Missing Check-in</option>
                    <option value="missing_checkout">Missing Check-out</option>
                    <option value="late_arrivals">Late Arrivals</option>
                    <option value="early_departures">Early Departures</option>
                  </select>
                </div>
              </div>

              <!-- Pending Corrections Table -->
              <div class="table-responsive">
                <table class="table table-hover">
                  <thead class="table-dark">
                    <tr>
                      <th>Employee</th>
                      <th>Date</th>
                      <th>Check In</th>
                      <th>Check Out</th>
                      <th>Status</th>
                      <th>Issues</th>
                      <th>Working Hours</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let record of filteredRecords">
                      <td>
                        <div class="d-flex align-items-center">
                          <div class="avatar-sm me-2">
                            <div class="avatar-title bg-primary rounded-circle">
                              {{record.employee?.firstName?.charAt(0)}}{{record.employee?.lastName?.charAt(0)}}
                            </div>
                          </div>
                          <div>
                            <div class="fw-medium">{{record.employee?.firstName}} {{record.employee?.lastName}}</div>
                            <small class="text-muted">{{record.employee?.employeeId}}</small>
                          </div>
                        </div>
                      </td>
                      <td>{{formatDate(record.date)}}</td>
                      <td>
                        <span *ngIf="record.checkInTime" class="text-success">
                          {{formatTime(record.checkInTime)}}
                        </span>
                        <span *ngIf="!record.checkInTime" class="text-danger">
                          <i class="fas fa-times"></i> Missing
                        </span>
                      </td>
                      <td>
                        <span *ngIf="record.checkOutTime" class="text-success">
                          {{formatTime(record.checkOutTime)}}
                        </span>
                        <span *ngIf="!record.checkOutTime" class="text-warning">
                          <i class="fas fa-clock"></i> Pending
                        </span>
                      </td>
                      <td>
                        <span class="badge" 
                              [class.bg-success]="record.status === 'Present' && !record.isLate"
                              [class.bg-warning]="record.isLate"
                              [class.bg-danger]="record.status === 'Absent'">
                          {{record.status}}
                        </span>
                      </td>
                      <td>
                        <div class="d-flex flex-wrap gap-1">
                          <span class="badge bg-danger" *ngIf="!record.checkInTime">
                            <i class="fas fa-sign-in-alt"></i> No Check-in
                          </span>
                          <span class="badge bg-warning" *ngIf="!record.checkOutTime && record.checkInTime">
                            <i class="fas fa-sign-out-alt"></i> No Check-out
                          </span>
                          <span class="badge bg-warning" *ngIf="record.isLate">
                            <i class="fas fa-clock"></i> Late ({{formatTimeSpan(record.lateBy)}})
                          </span>
                          <span class="badge bg-info" *ngIf="record.isEarlyOut">
                            <i class="fas fa-door-open"></i> Early Out ({{formatTimeSpan(record.earlyOutBy)}})
                          </span>
                        </div>
                      </td>
                      <td>
                        <span *ngIf="record.totalWorkingHours">
                          {{formatTimeSpan(record.totalWorkingHours)}}
                        </span>
                        <span *ngIf="!record.totalWorkingHours" class="text-muted">-</span>
                      </td>
                      <td>
                        <div class="btn-group btn-group-sm">
                          <button class="btn btn-outline-primary" 
                                  (click)="correctAttendance(record)"
                                  title="Correct Attendance">
                            <i class="fas fa-edit"></i>
                          </button>
                          <button class="btn btn-outline-info" 
                                  (click)="viewDetails(record)"
                                  title="View Details">
                            <i class="fas fa-eye"></i>
                          </button>
                          <button class="btn btn-outline-danger" 
                                  (click)="deleteRecord(record)"
                                  title="Delete Record">
                            <i class="fas fa-trash"></i>
                          </button>
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <!-- No Data Message -->
              <div class="text-center py-5" *ngIf="filteredRecords.length === 0 && !loading">
                <i class="fas fa-check-circle fa-3x text-success mb-3"></i>
                <h5 class="text-muted">No Corrections Needed</h5>
                <p class="text-muted">All attendance records are complete and accurate</p>
              </div>

              <!-- Loading State -->
              <div class="text-center py-5" *ngIf="loading">
                <div class="spinner-border text-primary" role="status">
                  <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mt-2 text-muted">Loading attendance records...</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Correction Modal -->
    <div class="modal fade" id="correctionModal" tabindex="-1" *ngIf="selectedRecord">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="fas fa-edit me-2"></i>
              Correct Attendance - {{selectedRecord.employee?.firstName}} {{selectedRecord.employee?.lastName}}
            </h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <form [formGroup]="correctionForm" (ngSubmit)="submitCorrection()">
            <div class="modal-body">
              <div class="row">
                <div class="col-md-6">
                  <div class="card">
                    <div class="card-header bg-light">
                      <h6 class="mb-0">Current Record</h6>
                    </div>
                    <div class="card-body">
                      <table class="table table-sm">
                        <tr>
                          <td><strong>Date:</strong></td>
                          <td>{{formatDate(selectedRecord.date)}}</td>
                        </tr>
                        <tr>
                          <td><strong>Check In:</strong></td>
                          <td>
                            <span *ngIf="selectedRecord.checkInTime">{{formatDateTime(selectedRecord.checkInTime)}}</span>
                            <span *ngIf="!selectedRecord.checkInTime" class="text-danger">Not recorded</span>
                          </td>
                        </tr>
                        <tr>
                          <td><strong>Check Out:</strong></td>
                          <td>
                            <span *ngIf="selectedRecord.checkOutTime">{{formatDateTime(selectedRecord.checkOutTime)}}</span>
                            <span *ngIf="!selectedRecord.checkOutTime" class="text-warning">Not recorded</span>
                          </td>
                        </tr>
                        <tr>
                          <td><strong>Status:</strong></td>
                          <td>
                            <span class="badge bg-secondary">{{selectedRecord.status}}</span>
                          </td>
                        </tr>
                      </table>
                    </div>
                  </div>
                </div>
                <div class="col-md-6">
                  <div class="card">
                    <div class="card-header bg-primary text-white">
                      <h6 class="mb-0">Corrections</h6>
                    </div>
                    <div class="card-body">
                      <div class="mb-3">
                        <label class="form-label">Check In Time</label>
                        <input type="datetime-local" class="form-control" formControlName="checkInTime">
                      </div>
                      <div class="mb-3">
                        <label class="form-label">Check Out Time</label>
                        <input type="datetime-local" class="form-control" formControlName="checkOutTime">
                      </div>
                      <div class="mb-3">
                        <label class="form-label">Reason for Correction <span class="text-danger">*</span></label>
                        <textarea class="form-control" formControlName="reason" rows="3" 
                                  placeholder="Please provide a reason for this correction..."></textarea>
                        <div class="form-text">This will be logged for audit purposes</div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
              <button type="submit" class="btn btn-primary" [disabled]="correctionForm.invalid || submitting">
                <span *ngIf="!submitting">
                  <i class="fas fa-save me-1"></i>
                  Save Correction
                </span>
                <span *ngIf="submitting">
                  <span class="spinner-border spinner-border-sm me-1"></span>
                  Saving...
                </span>
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Add Missing Attendance Modal -->
    <div class="modal fade" id="addMissingModal" tabindex="-1">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="fas fa-plus me-2"></i>
              Add Missing Attendance
            </h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <form [formGroup]="addMissingForm" (ngSubmit)="submitAddMissing()">
            <div class="modal-body">
              <div class="row">
                <div class="col-md-6">
                  <div class="mb-3">
                    <label class="form-label">Employee <span class="text-danger">*</span></label>
                    <select class="form-select" formControlName="employeeId">
                      <option value="">Select Employee</option>
                      <option *ngFor="let emp of employees" [value]="emp.id">
                        {{emp.firstName}} {{emp.lastName}} ({{emp.employeeId}})
                      </option>
                    </select>
                  </div>
                  <div class="mb-3">
                    <label class="form-label">Date <span class="text-danger">*</span></label>
                    <input type="date" class="form-control" formControlName="date">
                  </div>
                </div>
                <div class="col-md-6">
                  <div class="mb-3">
                    <label class="form-label">Check In Time <span class="text-danger">*</span></label>
                    <input type="time" class="form-control" formControlName="checkInTime">
                  </div>
                  <div class="mb-3">
                    <label class="form-label">Check Out Time</label>
                    <input type="time" class="form-control" formControlName="checkOutTime">
                    <div class="form-text">Leave empty if employee hasn't checked out yet</div>
                  </div>
                </div>
              </div>
              <div class="mb-3">
                <label class="form-label">Reason <span class="text-danger">*</span></label>
                <textarea class="form-control" formControlName="reason" rows="3" 
                          placeholder="Please provide a reason for adding this attendance record..."></textarea>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
              <button type="submit" class="btn btn-success" [disabled]="addMissingForm.invalid || submitting">
                <span *ngIf="!submitting">
                  <i class="fas fa-plus me-1"></i>
                  Add Attendance
                </span>
                <span *ngIf="submitting">
                  <span class="spinner-border spinner-border-sm me-1"></span>
                  Adding...
                </span>
              </button>
            </div>
          </form>
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
      font-size: 12px;
      font-weight: 600;
    }
    
    .badge {
      font-size: 0.75em;
    }
    
    .table td {
      vertical-align: middle;
    }
  `]
})
export class AttendanceCorrectionsComponent implements OnInit {
  pendingRecords: AttendanceRecord[] = [];
  filteredRecords: AttendanceRecord[] = [];
  selectedRecord: AttendanceRecord | null = null;
  
  correctionForm: FormGroup;
  addMissingForm: FormGroup;
  
  loading = false;
  submitting = false;
  
  selectedBranchId = '';
  startDate = '';
  endDate = '';
  filterType = 'all';
  
  branches: any[] = [];
  employees: any[] = [];

  constructor(
    private fb: FormBuilder,
    private attendanceService: AttendanceService
  ) {
    this.correctionForm = this.fb.group({
      checkInTime: [''],
      checkOutTime: [''],
      reason: ['', Validators.required]
    });

    this.addMissingForm = this.fb.group({
      employeeId: ['', Validators.required],
      date: ['', Validators.required],
      checkInTime: ['', Validators.required],
      checkOutTime: [''],
      reason: ['', Validators.required]
    });

    // Set default dates (last 30 days)
    const today = new Date();
    const thirtyDaysAgo = new Date(today);
    thirtyDaysAgo.setDate(today.getDate() - 30);
    
    this.startDate = thirtyDaysAgo.toISOString().split('T')[0];
    this.endDate = today.toISOString().split('T')[0];
  }

  ngOnInit() {
    this.loadBranches();
    this.loadEmployees();
    this.loadPendingCorrections();
  }

  loadPendingCorrections() {
    if (!this.selectedBranchId) return;

    this.loading = true;
    const startDate = this.startDate ? new Date(this.startDate) : undefined;
    const endDate = this.endDate ? new Date(this.endDate) : undefined;
    
    this.attendanceService.getPendingCorrections(
      parseInt(this.selectedBranchId),
      startDate,
      endDate
    ).subscribe({
      next: (records) => {
        this.pendingRecords = records;
        this.applyFilter();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading pending corrections:', error);
        this.loading = false;
      }
    });
  }

  applyFilter() {
    switch (this.filterType) {
      case 'missing_checkin':
        this.filteredRecords = this.pendingRecords.filter(r => !r.checkInTime);
        break;
      case 'missing_checkout':
        this.filteredRecords = this.pendingRecords.filter(r => r.checkInTime && !r.checkOutTime);
        break;
      case 'late_arrivals':
        this.filteredRecords = this.pendingRecords.filter(r => r.isLate);
        break;
      case 'early_departures':
        this.filteredRecords = this.pendingRecords.filter(r => r.isEarlyOut);
        break;
      default:
        this.filteredRecords = [...this.pendingRecords];
    }
  }

  correctAttendance(record: AttendanceRecord) {
    this.selectedRecord = record;
    
    // Pre-populate form with current values
    this.correctionForm.patchValue({
      checkInTime: record.checkInTime ? this.formatDateTimeForInput(record.checkInTime) : '',
      checkOutTime: record.checkOutTime ? this.formatDateTimeForInput(record.checkOutTime) : '',
      reason: ''
    });

    // Show modal (you'll need to implement modal show/hide logic)
    console.log('Show correction modal for:', record);
  }

  submitCorrection() {
    if (this.correctionForm.invalid || !this.selectedRecord) return;

    this.submitting = true;
    const formValue = this.correctionForm.value;
    const request: AttendanceCorrectionRequest = {
      checkInTime: formValue.checkInTime ? new Date(formValue.checkInTime) : undefined,
      checkOutTime: formValue.checkOutTime ? new Date(formValue.checkOutTime) : undefined,
      reason: formValue.reason
    };

    this.attendanceService.correctAttendance(this.selectedRecord.id, request)
      .subscribe({
        next: () => {
          // Refresh the list
          this.loadPendingCorrections();
          
          // Hide modal and reset form
          this.selectedRecord = null;
          this.correctionForm.reset();
          
          // Show success message
          console.log('Attendance corrected successfully');
          this.submitting = false;
        },
        error: (error) => {
          console.error('Error correcting attendance:', error);
          this.submitting = false;
        }
      });
  }

  showAddMissingModal() {
    this.addMissingForm.reset();
    // Show modal
    console.log('Show add missing modal');
  }

  submitAddMissing() {
    if (this.addMissingForm.invalid) return;

    this.submitting = true;
    const formValue = this.addMissingForm.value;
    const request: AddMissingAttendanceRequest = {
      employeeId: parseInt(formValue.employeeId),
      date: new Date(formValue.date),
      checkInTime: this.combineDateAndTime(formValue.date, formValue.checkInTime),
      checkOutTime: formValue.checkOutTime ? this.combineDateAndTime(formValue.date, formValue.checkOutTime) : undefined,
      reason: formValue.reason
    };

    this.attendanceService.addMissingAttendance(request)
      .subscribe({
        next: () => {
          // Refresh the list
          this.loadPendingCorrections();
          
          // Hide modal and reset form
          this.addMissingForm.reset();
          
          // Show success message
          console.log('Missing attendance added successfully');
          this.submitting = false;
        },
        error: (error) => {
          console.error('Error adding missing attendance:', error);
          this.submitting = false;
        }
      });
  }

  viewDetails(record: AttendanceRecord) {
    // Navigate to detailed view or show details modal
    console.log('View details for:', record);
  }

  deleteRecord(record: AttendanceRecord) {
    if (!confirm('Are you sure you want to delete this attendance record?')) {
      return;
    }

    const reason = prompt('Please provide a reason for deletion:');
    if (!reason) return;

    this.attendanceService.deleteAttendanceRecord(record.id, reason)
      .subscribe({
        next: () => {
          this.loadPendingCorrections();
          console.log('Attendance record deleted successfully');
        },
        error: (error) => {
          console.error('Error deleting attendance record:', error);
        }
      });
  }

  formatDate(date: string | Date): string {
    return new Date(date).toLocaleDateString();
  }

  formatTime(time: string | Date): string {
    return new Date(time).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatDateTime(dateTime: string | Date): string {
    return new Date(dateTime).toLocaleString();
  }

  formatTimeSpan(timeSpan: string): string {
    if (!timeSpan) return '00:00';
    
    const parts = timeSpan.split(':');
    if (parts.length >= 2) {
      return `${parts[0]}:${parts[1]}`;
    }
    return timeSpan;
  }

  formatDateTimeForInput(dateTime: string | Date): string {
    const date = new Date(dateTime);
    return date.toISOString().slice(0, 16);
  }

  combineDateAndTime(date: string, time: string): Date {
    return new Date(`${date}T${time}`);
  }

  private loadBranches() {
    // Load branches from service
    this.branches = []; // Placeholder - implement when branch service is available
  }

  private loadEmployees() {
    // Load employees from service
    this.employees = []; // Placeholder - implement when employee service is available
  }
}