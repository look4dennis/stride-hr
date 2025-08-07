import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, takeUntil, interval, startWith, switchMap, of } from 'rxjs';
import { EnhancedAttendanceService } from '../../../services/enhanced-attendance.service';
import {
  AttendanceStatus,
  TodayAttendanceOverview,
  AttendanceStatusType,
  BreakType,
  BreakTypeLabels,
  AttendanceStatusColors
} from '../../../models/attendance.models';

@Component({
  selector: 'app-attendance-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="attendance-widget">
      <!-- Personal Attendance Status (for all users) -->
      <div class="widget-section" *ngIf="showPersonalStatus">
        <div class="section-header">
          <h6 class="section-title">
            <i class="fas fa-user-clock me-2"></i>
            My Attendance
          </h6>
          <button 
            class="btn btn-sm btn-outline-primary"
            (click)="navigateToTracker()"
            title="Open Attendance Tracker">
            <i class="fas fa-external-link-alt"></i>
          </button>
        </div>
        
        <div class="personal-status" *ngIf="personalStatus">
          <div class="status-card" [class]="'status-' + (personalStatus.currentStatus || 'absent').toLowerCase()">
            <div class="status-info">
              <div class="status-badge">
                <span class="badge" [class]="'bg-' + getStatusColor(personalStatus.currentStatus)">
                  <i class="fas" [class]="getStatusIcon(personalStatus.currentStatus)" class="me-1"></i>
                  {{ getStatusLabel(personalStatus.currentStatus) }}
                </span>
              </div>
              
              <div class="status-details" *ngIf="personalStatus.isCheckedIn">
                <div class="detail-item">
                  <span class="detail-label">Working:</span>
                  <span class="detail-value">{{ formatDuration(personalStatus.totalWorkingHours) }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">Break:</span>
                  <span class="detail-value">{{ formatDuration(personalStatus.totalBreakTime) }}</span>
                </div>
                <div class="detail-item" *ngIf="personalStatus.checkInTime">
                  <span class="detail-label">Check-in:</span>
                  <span class="detail-value">{{ formatTime(personalStatus.checkInTime) }}</span>
                </div>
              </div>

              <!-- Current Break Info -->
              <div class="break-info" *ngIf="personalStatus.currentBreak">
                <div class="alert alert-info alert-sm">
                  <i class="fas fa-coffee me-2"></i>
                  On {{ getBreakLabel(personalStatus.currentBreak.type) }} since 
                  {{ formatTime(personalStatus.currentBreak.startTime) }}
                </div>
              </div>
            </div>

            <!-- Quick Actions -->
            <div class="quick-actions" *ngIf="showQuickActions">
              <div class="action-buttons">
                <!-- Check-in Button -->
                <button 
                  *ngIf="!personalStatus.isCheckedIn"
                  class="btn btn-success btn-sm"
                  (click)="quickCheckIn()"
                  [disabled]="isLoading">
                  <i class="fas fa-sign-in-alt me-1"></i>
                  <span *ngIf="!isLoading">Check In</span>
                  <span *ngIf="isLoading">
                    <span class="spinner-border spinner-border-sm me-1"></span>
                    Checking In...
                  </span>
                </button>

                <!-- Break/End Break Button -->
                <div *ngIf="personalStatus.isCheckedIn && !personalStatus.currentBreak" class="dropdown d-inline-block">
                  <button 
                    class="btn btn-warning btn-sm dropdown-toggle"
                    type="button"
                    data-bs-toggle="dropdown"
                    [disabled]="isLoading">
                    <i class="fas fa-coffee me-1"></i>
                    Break
                  </button>
                  <ul class="dropdown-menu">
                    <li *ngFor="let breakType of breakTypes">
                      <a class="dropdown-item" 
                         href="#" 
                         (click)="quickStartBreak(breakType); $event.preventDefault()">
                        <i class="fas" [class]="getBreakIcon(breakType)" class="me-2"></i>
                        {{ getBreakLabel(breakType) }}
                      </a>
                    </li>
                  </ul>
                </div>

                <button 
                  *ngIf="personalStatus.isCheckedIn && personalStatus.currentBreak"
                  class="btn btn-info btn-sm"
                  (click)="quickEndBreak()"
                  [disabled]="isLoading">
                  <i class="fas fa-play me-1"></i>
                  <span *ngIf="!isLoading">End Break</span>
                  <span *ngIf="isLoading">
                    <span class="spinner-border spinner-border-sm me-1"></span>
                    Ending...
                  </span>
                </button>

                <!-- Check-out Button -->
                <button 
                  *ngIf="personalStatus.isCheckedIn && !personalStatus.currentBreak"
                  class="btn btn-danger btn-sm"
                  (click)="quickCheckOut()"
                  [disabled]="isLoading">
                  <i class="fas fa-sign-out-alt me-1"></i>
                  <span *ngIf="!isLoading">Check Out</span>
                  <span *ngIf="isLoading">
                    <span class="spinner-border spinner-border-sm me-1"></span>
                    Checking Out...
                  </span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Team/Branch Overview (for managers/HR/admin) -->
      <div class="widget-section" *ngIf="showTeamOverview && overview">
        <div class="section-header">
          <h6 class="section-title">
            <i class="fas fa-users me-2"></i>
            Team Attendance
          </h6>
          <button 
            class="btn btn-sm btn-outline-primary"
            (click)="navigateToAttendanceNow()"
            title="View All Attendance">
            <i class="fas fa-external-link-alt"></i>
          </button>
        </div>

        <div class="overview-stats">
          <div class="row g-2">
            <div class="col-6">
              <div class="stat-card bg-success">
                <div class="stat-icon">
                  <i class="fas fa-user-check"></i>
                </div>
                <div class="stat-content">
                  <div class="stat-number">{{ overview.summary.presentCount }}</div>
                  <div class="stat-label">Present</div>
                </div>
              </div>
            </div>
            <div class="col-6">
              <div class="stat-card bg-danger">
                <div class="stat-icon">
                  <i class="fas fa-user-times"></i>
                </div>
                <div class="stat-content">
                  <div class="stat-number">{{ overview.summary.absentCount }}</div>
                  <div class="stat-label">Absent</div>
                </div>
              </div>
            </div>
            <div class="col-6">
              <div class="stat-card bg-warning">
                <div class="stat-icon">
                  <i class="fas fa-clock"></i>
                </div>
                <div class="stat-content">
                  <div class="stat-number">{{ overview.summary.lateCount }}</div>
                  <div class="stat-label">Late</div>
                </div>
              </div>
            </div>
            <div class="col-6">
              <div class="stat-card bg-info">
                <div class="stat-icon">
                  <i class="fas fa-coffee"></i>
                </div>
                <div class="stat-content">
                  <div class="stat-number">{{ overview.summary.onBreakCount }}</div>
                  <div class="stat-label">On Break</div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="overview-summary">
          <div class="summary-item">
            <span class="summary-label">Total Employees:</span>
            <span class="summary-value">{{ overview.summary.totalEmployees }}</span>
          </div>
          <div class="summary-item">
            <span class="summary-label">Avg. Working Hours:</span>
            <span class="summary-value">{{ formatDuration(overview.summary.averageWorkingHours) }}</span>
          </div>
        </div>
      </div>

      <!-- Success/Error Messages -->
      <div class="alert alert-success alert-sm" 
           *ngIf="successMessage" 
           role="alert">
        <i class="fas fa-check-circle me-2"></i>
        {{ successMessage }}
      </div>

      <div class="alert alert-danger alert-sm" 
           *ngIf="errorMessage" 
           role="alert">
        <i class="fas fa-exclamation-circle me-2"></i>
        {{ errorMessage }}
      </div>
    </div>
  `,
  styles: [`
    .attendance-widget {
      background: white;
      border-radius: 12px;
      border: 1px solid var(--bs-border-color);
      overflow: hidden;
    }

    .widget-section {
      padding: 1rem;
    }

    .widget-section + .widget-section {
      border-top: 1px solid var(--bs-border-color-translucent);
    }

    .section-header {
      display: flex;
      justify-content: between;
      align-items: center;
      margin-bottom: 1rem;
    }

    .section-title {
      margin: 0;
      font-weight: 600;
      color: var(--bs-dark);
      flex: 1;
    }

    .personal-status {
      margin-bottom: 1rem;
    }

    .status-card {
      border-radius: 8px;
      padding: 1rem;
      background: var(--bs-light);
      border-left: 4px solid var(--bs-secondary);
    }

    .status-card.status-present {
      border-left-color: var(--bs-success);
      background: rgba(25, 135, 84, 0.05);
    }

    .status-card.status-absent {
      border-left-color: var(--bs-danger);
      background: rgba(220, 53, 69, 0.05);
    }

    .status-card.status-late {
      border-left-color: var(--bs-warning);
      background: rgba(255, 193, 7, 0.05);
    }

    .status-card.status-onbreak {
      border-left-color: var(--bs-info);
      background: rgba(13, 202, 240, 0.05);
    }

    .status-badge {
      margin-bottom: 0.75rem;
    }

    .status-details {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
      margin-bottom: 0.75rem;
    }

    .detail-item {
      display: flex;
      flex-direction: column;
      min-width: 80px;
    }

    .detail-label {
      font-size: 0.75rem;
      color: var(--bs-secondary);
      font-weight: 500;
    }

    .detail-value {
      font-size: 0.875rem;
      font-weight: 600;
      color: var(--bs-dark);
    }

    .break-info {
      margin-top: 0.75rem;
    }

    .alert-sm {
      padding: 0.5rem 0.75rem;
      margin-bottom: 0.5rem;
      font-size: 0.875rem;
    }

    .quick-actions {
      margin-top: 1rem;
      padding-top: 1rem;
      border-top: 1px solid var(--bs-border-color-translucent);
    }

    .action-buttons {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .action-buttons .btn {
      flex: 1;
      min-width: 100px;
    }

    .overview-stats {
      margin-bottom: 1rem;
    }

    .stat-card {
      border-radius: 8px;
      padding: 0.75rem;
      color: white;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .stat-icon {
      font-size: 1.25rem;
      opacity: 0.9;
    }

    .stat-content {
      flex: 1;
      text-align: right;
    }

    .stat-number {
      font-size: 1.25rem;
      font-weight: 700;
      line-height: 1;
    }

    .stat-label {
      font-size: 0.75rem;
      opacity: 0.9;
      font-weight: 500;
    }

    .overview-summary {
      padding-top: 0.75rem;
      border-top: 1px solid var(--bs-border-color-translucent);
    }

    .summary-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.25rem 0;
      font-size: 0.875rem;
    }

    .summary-label {
      color: var(--bs-secondary);
    }

    .summary-value {
      font-weight: 600;
      color: var(--bs-dark);
    }

    .dropdown-menu {
      border: none;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
      font-size: 0.875rem;
    }

    .dropdown-item {
      padding: 0.5rem 0.75rem;
    }

    .dropdown-item:hover {
      background-color: var(--bs-light);
    }

    /* Mobile responsive */
    @media (max-width: 768px) {
      .widget-section {
        padding: 0.75rem;
      }

      .status-details {
        gap: 0.75rem;
      }

      .detail-item {
        min-width: 70px;
      }

      .action-buttons .btn {
        min-width: 80px;
        font-size: 0.8rem;
        padding: 0.375rem 0.5rem;
      }

      .stat-card {
        padding: 0.5rem;
      }

      .stat-number {
        font-size: 1rem;
      }

      .stat-label {
        font-size: 0.7rem;
      }
    }
  `]
})
export class AttendanceWidgetComponent implements OnInit, OnDestroy {
  @Input() showPersonalStatus = true;
  @Input() showTeamOverview = false;
  @Input() showQuickActions = true;
  @Input() branchId?: number;

  personalStatus: AttendanceStatus | null = null;
  overview: TodayAttendanceOverview | null = null;

  isLoading = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;

  breakTypes = Object.values(BreakType);
  private destroy$ = new Subject<void>();

  constructor(
    private attendanceService: EnhancedAttendanceService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadData();
    this.setupRealTimeUpdates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadData(): void {
    if (this.showPersonalStatus) {
      this.loadPersonalStatus();
    }

    if (this.showTeamOverview) {
      this.loadTeamOverview();
    }
  }

  private loadPersonalStatus(): void {
    this.attendanceService.getCurrentEmployeeStatus()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (status) => this.personalStatus = status,
        error: (error) => {
          console.log('Failed to load personal status:', error);
          this.personalStatus = this.attendanceService.getMockAttendanceStatus();
        }
      });
  }

  private loadTeamOverview(): void {
    this.attendanceService.getTodayAttendanceOverview(this.branchId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (overview) => this.overview = overview,
        error: (error) => {
          console.log('Failed to load team overview:', error);
          this.overview = this.attendanceService.getMockTodayOverview();
        }
      });
  }

  private setupRealTimeUpdates(): void {
    // Update every 30 seconds
    interval(30000)
      .pipe(
        takeUntil(this.destroy$),
        startWith(0),
        switchMap(() => {
          if (this.showPersonalStatus) {
            return this.attendanceService.getCurrentEmployeeStatus();
          }
          return of(null);
        })
      )
      .subscribe({
        next: (status) => {
          if (status && this.showPersonalStatus) {
            this.personalStatus = status;
          }
        },
        error: (error) => console.log('Real-time update failed:', error)
      });

    // Separate interval for team overview
    if (this.showTeamOverview) {
      interval(30000)
        .pipe(
          takeUntil(this.destroy$),
          startWith(0),
          switchMap(() => this.attendanceService.getTodayAttendanceOverview(this.branchId))
        )
        .subscribe({
          next: (overview) => {
            if (overview) {
              this.overview = overview;
            }
          },
          error: (error) => console.log('Team overview update failed:', error)
        });
    }
  }

  // Quick Actions
  quickCheckIn(): void {
    this.isLoading = true;
    this.clearMessages();

    this.attendanceService.checkIn()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (record) => {
          this.isLoading = false;
          this.successMessage = 'Checked in successfully!';
          this.loadPersonalStatus();
          this.clearMessageAfterDelay();
        },
        error: (error) => {
          this.isLoading = false;
          this.handleError('Check-in failed', error);
        }
      });
  }

  quickCheckOut(): void {
    this.isLoading = true;
    this.clearMessages();

    this.attendanceService.checkOut()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (record) => {
          this.isLoading = false;
          this.successMessage = 'Checked out successfully!';
          this.loadPersonalStatus();
          this.clearMessageAfterDelay();
        },
        error: (error) => {
          this.isLoading = false;
          this.handleError('Check-out failed', error);
        }
      });
  }

  quickStartBreak(type: BreakType): void {
    this.isLoading = true;
    this.clearMessages();

    this.attendanceService.startBreak(type)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (record) => {
          this.isLoading = false;
          this.successMessage = `${this.getBreakLabel(type)} started!`;
          this.loadPersonalStatus();
          this.clearMessageAfterDelay();
        },
        error: (error) => {
          this.isLoading = false;
          this.handleError('Failed to start break', error);
        }
      });
  }

  quickEndBreak(): void {
    this.isLoading = true;
    this.clearMessages();

    this.attendanceService.endBreak()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (record) => {
          this.isLoading = false;
          this.successMessage = 'Break ended successfully!';
          this.loadPersonalStatus();
          this.clearMessageAfterDelay();
        },
        error: (error) => {
          this.isLoading = false;
          this.handleError('Failed to end break', error);
        }
      });
  }

  // Navigation
  navigateToTracker(): void {
    this.router.navigate(['/attendance']);
  }

  navigateToAttendanceNow(): void {
    this.router.navigate(['/attendance/now']);
  }

  // Utility Methods
  getStatusLabel(status?: AttendanceStatusType): string {
    if (!status) return 'Not Available';
    return status.replace(/([A-Z])/g, ' $1').trim();
  }

  getStatusColor(status?: AttendanceStatusType): string {
    if (!status) return 'secondary';
    return AttendanceStatusColors[status] || 'secondary';
  }

  getStatusIcon(status?: AttendanceStatusType): string {
    const icons = {
      [AttendanceStatusType.Present]: 'fa-check-circle',
      [AttendanceStatusType.Absent]: 'fa-times-circle',
      [AttendanceStatusType.Late]: 'fa-clock',
      [AttendanceStatusType.OnBreak]: 'fa-coffee',
      [AttendanceStatusType.HalfDay]: 'fa-adjust',
      [AttendanceStatusType.OnLeave]: 'fa-calendar-times'
    };
    return icons[status as AttendanceStatusType] || 'fa-question-circle';
  }

  getBreakLabel(type?: BreakType): string {
    if (!type) return '';
    return BreakTypeLabels[type] || type;
  }

  getBreakIcon(type: BreakType): string {
    const icons = {
      [BreakType.Tea]: 'fa-coffee',
      [BreakType.Lunch]: 'fa-utensils',
      [BreakType.Personal]: 'fa-user',
      [BreakType.Meeting]: 'fa-users'
    };
    return icons[type] || 'fa-coffee';
  }

  formatTime(timeString?: string): string {
    if (!timeString) return '--:--';

    try {
      const date = new Date(timeString);
      if (isNaN(date.getTime())) {
        return timeString.substring(0, 5);
      }
      return date.toLocaleTimeString('en-US', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
      });
    } catch {
      return timeString.substring(0, 5);
    }
  }

  formatDuration(duration?: string): string {
    if (!duration || duration === '00:00:00') return '--:--';
    return duration.substring(0, 5);
  }

  private clearMessages(): void {
    this.successMessage = null;
    this.errorMessage = null;
  }

  private clearMessageAfterDelay(): void {
    setTimeout(() => {
      this.successMessage = null;
      this.errorMessage = null;
    }, 3000);
  }

  private handleError(message: string, error: any): void {
    console.error(message, error);
    this.errorMessage = error.error?.message || message;
    this.clearMessageAfterDelay();
  }
}