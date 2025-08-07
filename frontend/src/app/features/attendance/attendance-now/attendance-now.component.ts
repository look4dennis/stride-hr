import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil, interval, startWith, switchMap } from 'rxjs';
import { EnhancedAttendanceService } from '../../../services/enhanced-attendance.service';
import { 
  TodayAttendanceOverview,
  EmployeeAttendanceStatus,
  AttendanceStatusType,
  AttendanceStatusColors,
  BreakTypeLabels
} from '../../../models/attendance.models';

@Component({
    selector: 'app-attendance-now',
    imports: [CommonModule, FormsModule],
    template: `
    <div class="page-header d-flex justify-content-between align-items-center">
      <div>
        <h1>Attendance Now</h1>
        <p class="text-muted">Real-time organization-wide attendance overview</p>
      </div>
      <div class="d-flex gap-2">
        <button 
          class="btn btn-outline-secondary"
          (click)="refreshData()"
          [disabled]="isLoading">
          <i class="fas fa-sync-alt me-2" [class.fa-spin]="isLoading"></i>
          Refresh
        </button>
        <button 
          class="btn btn-outline-primary"
          (click)="goBack()">
          <i class="fas fa-arrow-left me-2"></i>
          Back to Tracker
        </button>
      </div>
    </div>

    <!-- Summary Cards -->
    <div class="row mb-4" *ngIf="overview">
      <div class="col-lg-2 col-md-4 col-sm-6 mb-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body text-center">
            <div class="stat-icon bg-primary mb-2">
              <i class="fas fa-users"></i>
            </div>
            <h3 class="stat-number">{{ overview.summary.totalEmployees }}</h3>
            <p class="stat-label mb-0">Total Employees</p>
          </div>
        </div>
      </div>
      
      <div class="col-lg-2 col-md-4 col-sm-6 mb-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body text-center">
            <div class="stat-icon bg-success mb-2">
              <i class="fas fa-check-circle"></i>
            </div>
            <h3 class="stat-number">{{ overview.summary.presentCount }}</h3>
            <p class="stat-label mb-0">Present</p>
          </div>
        </div>
      </div>
      
      <div class="col-lg-2 col-md-4 col-sm-6 mb-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body text-center">
            <div class="stat-icon bg-danger mb-2">
              <i class="fas fa-times-circle"></i>
            </div>
            <h3 class="stat-number">{{ overview.summary.absentCount }}</h3>
            <p class="stat-label mb-0">Absent</p>
          </div>
        </div>
      </div>
      
      <div class="col-lg-2 col-md-4 col-sm-6 mb-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body text-center">
            <div class="stat-icon bg-warning mb-2">
              <i class="fas fa-clock"></i>
            </div>
            <h3 class="stat-number">{{ overview.summary.lateCount }}</h3>
            <p class="stat-label mb-0">Late</p>
          </div>
        </div>
      </div>
      
      <div class="col-lg-2 col-md-4 col-sm-6 mb-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body text-center">
            <div class="stat-icon bg-info mb-2">
              <i class="fas fa-coffee"></i>
            </div>
            <h3 class="stat-number">{{ overview.summary.onBreakCount }}</h3>
            <p class="stat-label mb-0">On Break</p>
          </div>
        </div>
      </div>
      
      <div class="col-lg-2 col-md-4 col-sm-6 mb-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body text-center">
            <div class="stat-icon bg-secondary mb-2">
              <i class="fas fa-calendar-times"></i>
            </div>
            <h3 class="stat-number">{{ overview.summary.onLeaveCount }}</h3>
            <p class="stat-label mb-0">On Leave</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="row mb-4">
      <div class="col-12">
        <div class="card border-0 shadow-sm">
          <div class="card-body">
            <div class="row align-items-center">
              <div class="col-md-4">
                <label class="form-label">Filter by Status</label>
                <select class="form-select" [(ngModel)]="selectedStatus" (change)="applyFilters()">
                  <option value="">All Statuses</option>
                  <option value="Present">Present</option>
                  <option value="Absent">Absent</option>
                  <option value="Late">Late</option>
                  <option value="OnBreak">On Break</option>
                  <option value="OnLeave">On Leave</option>
                </select>
              </div>
              <div class="col-md-4">
                <label class="form-label">Filter by Department</label>
                <select class="form-select" [(ngModel)]="selectedDepartment" (change)="applyFilters()">
                  <option value="">All Departments</option>
                  <option *ngFor="let dept of departments" [value]="dept">{{ dept }}</option>
                </select>
              </div>
              <div class="col-md-4">
                <label class="form-label">Search Employee</label>
                <input 
                  type="text" 
                  class="form-control" 
                  placeholder="Search by name or ID..."
                  [(ngModel)]="searchTerm"
                  (input)="applyFilters()">
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Employee List -->
    <div class="row">
      <div class="col-12">
        <div class="card border-0 shadow-sm">
          <div class="card-header bg-white border-bottom">
            <h5 class="mb-0">
              <i class="fas fa-list me-2"></i>
              Employee Attendance Status
              <span class="badge bg-primary ms-2">{{ filteredEmployees.length }}</span>
            </h5>
          </div>
          <div class="card-body p-0">
            <div class="table-responsive">
              <table class="table table-hover mb-0">
                <thead class="table-light">
                  <tr>
                    <th>Employee</th>
                    <th>Status</th>
                    <th>Check-in Time</th>
                    <th>Working Hours</th>
                    <th>Break Time</th>
                    <th>Current Activity</th>
                    <th>Location</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let employeeStatus of filteredEmployees; trackBy: trackByEmployeeId">
                    <td>
                      <div class="d-flex align-items-center">
                        <img 
                          [src]="employeeStatus.employee.profilePhoto || '/assets/images/default-avatar.png'"
                          [alt]="employeeStatus.employee.firstName + ' ' + employeeStatus.employee.lastName"
                          class="rounded-circle me-3"
                          width="40"
                          height="40"
                          style="object-fit: cover;">
                        <div>
                          <div class="fw-semibold">
                            {{ employeeStatus.employee.firstName }} {{ employeeStatus.employee.lastName }}
                          </div>
                          <small class="text-muted">
                            {{ employeeStatus.employee.employeeId }} • {{ employeeStatus.employee.designation }}
                          </small>
                        </div>
                      </div>
                    </td>
                    <td>
                      <span class="badge" [class]="'bg-' + getStatusColor(employeeStatus.status)">
                        <i class="fas" [class]="getStatusIcon(employeeStatus.status)" class="me-1"></i>
                        {{ getStatusLabel(employeeStatus.status) }}
                      </span>
                      <div *ngIf="employeeStatus.isLate" class="text-warning small mt-1">
                        <i class="fas fa-exclamation-triangle me-1"></i>
                        Late Arrival
                      </div>
                    </td>
                    <td>
                      <span *ngIf="employeeStatus.checkInTime">
                        {{ formatTime(employeeStatus.checkInTime) }}
                      </span>
                      <span *ngIf="!employeeStatus.checkInTime" class="text-muted">--:--</span>
                    </td>
                    <td>
                      <span class="fw-semibold">{{ formatDuration(employeeStatus.totalWorkingHours) }}</span>
                    </td>
                    <td>
                      <span>{{ formatDuration(employeeStatus.totalBreakTime) }}</span>
                    </td>
                    <td>
                      <div *ngIf="employeeStatus.currentBreak" class="text-info">
                        <i class="fas fa-coffee me-1"></i>
                        {{ getBreakLabel(employeeStatus.currentBreak.type) }}
                        <small class="d-block text-muted">
                          Since {{ formatTime(employeeStatus.currentBreak.startTime) }}
                        </small>
                      </div>
                      <div *ngIf="!employeeStatus.currentBreak && employeeStatus.status === 'Present'" class="text-success">
                        <i class="fas fa-laptop me-1"></i>
                        Working
                      </div>
                      <div *ngIf="employeeStatus.status === 'Absent'" class="text-muted">
                        <i class="fas fa-minus me-1"></i>
                        Not Available
                      </div>
                    </td>
                    <td>
                      <div *ngIf="employeeStatus.location" class="text-muted small">
                        <i class="fas fa-map-marker-alt me-1"></i>
                        <span>{{ formatLocation(employeeStatus.location) }}</span>
                      </div>
                      <div *ngIf="!employeeStatus.location" class="text-muted">
                        <i class="fas fa-question-circle me-1"></i>
                        Unknown
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
            
            <div *ngIf="filteredEmployees.length === 0" class="text-center py-5">
              <i class="fas fa-search text-muted mb-3" style="font-size: 3rem;"></i>
              <h5 class="text-muted">No employees found</h5>
              <p class="text-muted">Try adjusting your filters or search criteria.</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Last Updated Info -->
    <div class="row mt-3" *ngIf="overview">
      <div class="col-12">
        <div class="text-center text-muted small">
          <i class="fas fa-clock me-1"></i>
          Last updated: {{ formatTime(getCurrentTime()) }}
          <span class="mx-2">•</span>
          Auto-refresh every 30 seconds
        </div>
      </div>
    </div>
  `,
    styles: [`
    .page-header {
      margin-bottom: 2rem;
    }
    
    .page-header h1 {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .stat-icon {
      width: 50px;
      height: 50px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto;
      color: white;
      font-size: 1.25rem;
    }

    .stat-number {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0.5rem 0;
    }

    .stat-label {
      font-size: 0.875rem;
      color: var(--text-secondary);
      font-weight: 500;
    }

    .table th {
      font-weight: 600;
      color: var(--text-primary);
      border-bottom: 2px solid var(--bs-border-color);
      padding: 1rem 0.75rem;
    }

    .table td {
      padding: 1rem 0.75rem;
      vertical-align: middle;
      border-bottom: 1px solid var(--bs-border-color-translucent);
    }

    .table-hover tbody tr:hover {
      background-color: var(--bs-light);
    }

    .badge {
      font-size: 0.75rem;
      font-weight: 500;
    }

    .fw-semibold {
      font-weight: 600;
    }

    @media (max-width: 768px) {
      .table-responsive {
        font-size: 0.875rem;
      }
      
      .stat-number {
        font-size: 1.5rem;
      }
      
      .stat-icon {
        width: 40px;
        height: 40px;
        font-size: 1rem;
      }
    }
  `]
})
export class AttendanceNowComponent implements OnInit, OnDestroy {
  overview: TodayAttendanceOverview | null = null;
  filteredEmployees: EmployeeAttendanceStatus[] = [];
  departments: string[] = [];
  
  selectedStatus = '';
  selectedDepartment = '';
  searchTerm = '';
  isLoading = false;
  
  private destroy$ = new Subject<void>();

  constructor(
    private attendanceService: EnhancedAttendanceService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAttendanceOverview();
    this.setupAutoRefresh();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadAttendanceOverview(): void {
    this.isLoading = true;
    
    this.attendanceService.getTodayAttendanceOverview()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (overview) => {
          this.overview = overview;
          this.processOverviewData();
          this.isLoading = false;
        },
        error: (error) => {
          console.log('API call failed, using mock data for development:', error);
          this.overview = this.attendanceService.getMockTodayOverview();
          this.processOverviewData();
          this.isLoading = false;
        }
      });
  }

  private processOverviewData(): void {
    if (!this.overview) return;
    
    this.filteredEmployees = [...this.overview.employeeStatuses];
    this.departments = [...new Set(this.overview.employeeStatuses.map(emp => emp.employee.department))];
    this.applyFilters();
  }

  private setupAutoRefresh(): void {
    interval(30000) // 30 seconds
      .pipe(
        takeUntil(this.destroy$),
        switchMap(() => this.attendanceService.getTodayAttendanceOverview())
      )
      .subscribe({
        next: (overview) => {
          this.overview = overview;
          this.processOverviewData();
        },
        error: (error) => {
          console.log('Auto-refresh failed (expected during development):', error);
          // Continue using existing data on refresh failure
        }
      });
  }

  refreshData(): void {
    this.loadAttendanceOverview();
    this.attendanceService.refreshTodayOverview();
  }

  applyFilters(): void {
    if (!this.overview) return;

    let filtered = [...this.overview.employeeStatuses];

    // Filter by status
    if (this.selectedStatus) {
      filtered = filtered.filter(emp => emp.status === this.selectedStatus);
    }

    // Filter by department
    if (this.selectedDepartment) {
      filtered = filtered.filter(emp => emp.employee.department === this.selectedDepartment);
    }

    // Filter by search term
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(emp => 
        emp.employee.firstName.toLowerCase().includes(term) ||
        emp.employee.lastName.toLowerCase().includes(term) ||
        emp.employee.employeeId.toLowerCase().includes(term) ||
        emp.employee.designation.toLowerCase().includes(term)
      );
    }

    this.filteredEmployees = filtered;
  }

  goBack(): void {
    this.router.navigate(['/attendance']);
  }

  // Utility Methods
  trackByEmployeeId(index: number, item: EmployeeAttendanceStatus): number {
    return item.employee.id;
  }

  getStatusLabel(status: AttendanceStatusType): string {
    return status.replace(/([A-Z])/g, ' $1').trim();
  }

  getStatusColor(status: AttendanceStatusType): string {
    return AttendanceStatusColors[status] || 'secondary';
  }

  getStatusIcon(status: AttendanceStatusType): string {
    const icons = {
      [AttendanceStatusType.Present]: 'fa-check-circle',
      [AttendanceStatusType.Absent]: 'fa-times-circle',
      [AttendanceStatusType.Late]: 'fa-clock',
      [AttendanceStatusType.OnBreak]: 'fa-coffee',
      [AttendanceStatusType.HalfDay]: 'fa-adjust',
      [AttendanceStatusType.OnLeave]: 'fa-calendar-times'
    };
    return icons[status] || 'fa-question-circle';
  }

  getBreakLabel(type?: string): string {
    if (!type) return '';
    return BreakTypeLabels[type as keyof typeof BreakTypeLabels] || type;
  }

  formatTime(timeString: string): string {
    if (!timeString) return '--:--';
    
    try {
      const date = new Date(timeString);
      return date.toLocaleTimeString('en-US', { 
        hour: '2-digit', 
        minute: '2-digit',
        hour12: true 
      });
    } catch {
      return '--:--';
    }
  }

  formatDuration(duration: string): string {
    if (!duration || duration === '00:00:00') return '--:--';
    return duration.substring(0, 5);
  }

  formatLocation(location: string): string {
    if (!location) return 'Unknown';
    
    // If it's coordinates, show a simplified version
    if (location.includes(',')) {
      return 'GPS Location';
    }
    
    return location;
  }

  getCurrentTime(): string {
    return new Date().toISOString();
  }
}