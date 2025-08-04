import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { AttendanceService } from '../../../services/attendance.service';
import { 
  AttendanceStatus, 
  BreakType, 
  AttendanceStatusType,
  BreakTypeLabels,
  AttendanceStatusColors 
} from '../../../models/attendance.models';

@Component({
    selector: 'app-attendance-tracker',
    imports: [CommonModule, FormsModule],
    template: `
    <div class="page-header d-flex justify-content-between align-items-center">
      <div>
        <h1>Attendance Tracking</h1>
        <p class="text-muted">Track your daily attendance and working hours</p>
      </div>
      <button 
        class="btn btn-outline-primary"
        (click)="navigateToAttendanceNow()">
        <i class="fas fa-users me-2"></i>
        Attendance Now
      </button>
    </div>

    <!-- Current Status Card -->
    <div class="row mb-4">
      <div class="col-12">
        <div class="card border-0 shadow-sm">
          <div class="card-body">
            <div class="row align-items-center">
              <div class="col-md-6">
                <div class="d-flex align-items-center mb-3">
                  <div class="status-indicator me-3" [class]="'status-' + (attendanceStatus?.currentStatus || 'absent')">
                    <i class="fas" [class]="getStatusIcon(attendanceStatus?.currentStatus)"></i>
                  </div>
                  <div>
                    <h5 class="mb-1">Current Status</h5>
                    <span class="badge" [class]="'bg-' + getStatusColor(attendanceStatus?.currentStatus)">
                      {{ getStatusLabel(attendanceStatus?.currentStatus) }}
                    </span>
                  </div>
                </div>
                
                <div class="row text-center" *ngIf="attendanceStatus?.isCheckedIn">
                  <div class="col-4">
                    <div class="stat-item">
                      <div class="stat-value">{{ formatTime(attendanceStatus?.totalWorkingHours) }}</div>
                      <div class="stat-label">Working Hours</div>
                    </div>
                  </div>
                  <div class="col-4">
                    <div class="stat-item">
                      <div class="stat-value">{{ formatTime(attendanceStatus?.totalBreakTime) }}</div>
                      <div class="stat-label">Break Time</div>
                    </div>
                  </div>
                  <div class="col-4">
                    <div class="stat-item">
                      <div class="stat-value">{{ formatTime(attendanceStatus?.checkInTime) }}</div>
                      <div class="stat-label">Check-in Time</div>
                    </div>
                  </div>
                </div>
              </div>
              
              <div class="col-md-6">
                <div class="attendance-actions text-center">
                  <!-- Check-in/Check-out Buttons -->
                  <div class="mb-3" *ngIf="!attendanceStatus?.isCheckedIn">
                    <button 
                      class="btn btn-success btn-lg rounded-pill px-4 me-2"
                      (click)="checkIn()"
                      [disabled]="isLoading">
                      <i class="fas fa-sign-in-alt me-2"></i>
                      <span *ngIf="!isLoading">Check In</span>
                      <span *ngIf="isLoading">
                        <span class="spinner-border spinner-border-sm me-2"></span>
                        Checking In...
                      </span>
                    </button>
                  </div>

                  <div class="mb-3" *ngIf="attendanceStatus?.isCheckedIn">
                    <!-- Break Management -->
                    <div class="mb-3" *ngIf="!attendanceStatus?.currentBreak">
                      <div class="dropdown d-inline-block me-2">
                        <button 
                          class="btn btn-warning rounded-pill dropdown-toggle"
                          type="button"
                          data-bs-toggle="dropdown"
                          [disabled]="isLoading">
                          <i class="fas fa-coffee me-2"></i>
                          Take Break
                        </button>
                        <ul class="dropdown-menu">
                          <li *ngFor="let breakType of breakTypes">
                            <a class="dropdown-item" 
                               href="#" 
                               (click)="startBreak(breakType); $event.preventDefault()">
                              <i class="fas" [class]="getBreakIcon(breakType)" class="me-2"></i>
                              {{ getBreakLabel(breakType) }}
                            </a>
                          </li>
                        </ul>
                      </div>
                    </div>

                    <!-- End Break Button -->
                    <div class="mb-3" *ngIf="attendanceStatus?.currentBreak">
                      <button 
                        class="btn btn-info rounded-pill me-2"
                        (click)="endBreak()"
                        [disabled]="isLoading">
                        <i class="fas fa-play me-2"></i>
                        <span *ngIf="!isLoading">End Break</span>
                        <span *ngIf="isLoading">
                          <span class="spinner-border spinner-border-sm me-2"></span>
                          Ending Break...
                        </span>
                      </button>
                      <small class="text-muted d-block">
                        On {{ getBreakLabel(attendanceStatus?.currentBreak?.type) }} since 
                        {{ formatTime(attendanceStatus?.currentBreak?.startTime) }}
                      </small>
                    </div>

                    <!-- Check-out Button -->
                    <button 
                      class="btn btn-danger rounded-pill px-4"
                      (click)="checkOut()"
                      [disabled]="isLoading || attendanceStatus?.currentBreak">
                      <i class="fas fa-sign-out-alt me-2"></i>
                      <span *ngIf="!isLoading">Check Out</span>
                      <span *ngIf="isLoading">
                        <span class="spinner-border spinner-border-sm me-2"></span>
                        Checking Out...
                      </span>
                    </button>
                    <div *ngIf="attendanceStatus?.currentBreak" class="text-muted small mt-2">
                      Please end your current break before checking out
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Location Info -->
    <div class="row mb-4" *ngIf="attendanceStatus?.location">
      <div class="col-12">
        <div class="card border-0 shadow-sm">
          <div class="card-body">
            <h6 class="card-title">
              <i class="fas fa-map-marker-alt me-2"></i>
              Location Information
            </h6>
            <p class="text-muted mb-0">
              <small>Last recorded location: {{ attendanceStatus?.location }}</small>
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Success/Error Messages -->
    <div class="alert alert-success alert-dismissible fade show" 
         *ngIf="successMessage" 
         role="alert">
      <i class="fas fa-check-circle me-2"></i>
      {{ successMessage }}
      <button type="button" class="btn-close" (click)="successMessage = null"></button>
    </div>

    <div class="alert alert-danger alert-dismissible fade show" 
         *ngIf="errorMessage" 
         role="alert">
      <i class="fas fa-exclamation-circle me-2"></i>
      {{ errorMessage }}
      <button type="button" class="btn-close" (click)="errorMessage = null"></button>
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

    .status-indicator {
      width: 60px;
      height: 60px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.5rem;
      color: white;
    }

    .status-present { background: linear-gradient(135deg, #10b981, #059669); }
    .status-absent { background: linear-gradient(135deg, #ef4444, #dc2626); }
    .status-late { background: linear-gradient(135deg, #f59e0b, #d97706); }
    .status-onbreak { background: linear-gradient(135deg, #06b6d4, #0891b2); }
    .status-halfday { background: linear-gradient(135deg, #6b7280, #4b5563); }
    .status-onleave { background: linear-gradient(135deg, #3b82f6, #2563eb); }

    .stat-item {
      padding: 0.5rem;
    }

    .stat-value {
      font-size: 1.25rem;
      font-weight: 600;
      color: var(--text-primary);
    }

    .stat-label {
      font-size: 0.875rem;
      color: var(--text-secondary);
    }

    .attendance-actions {
      min-height: 120px;
      display: flex;
      flex-direction: column;
      justify-content: center;
    }

    .btn-lg {
      padding: 0.75rem 2rem;
      font-size: 1.1rem;
    }

    .dropdown-menu {
      border: none;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    }

    .dropdown-item {
      padding: 0.75rem 1rem;
      transition: all 0.15s ease-in-out;
    }

    .dropdown-item:hover {
      background-color: var(--bs-light);
      transform: translateX(4px);
    }

    /* Mobile-responsive attendance tracker */
    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
      }
      
      .page-header .btn {
        width: 100%;
      }
      
      .card-body .row {
        flex-direction: column;
      }
      
      .status-indicator {
        width: 50px;
        height: 50px;
        font-size: 1.25rem;
      }
      
      .stat-item {
        padding: 0.25rem;
      }
      
      .stat-value {
        font-size: 1rem;
      }
      
      .stat-label {
        font-size: 0.8rem;
      }
      
      .attendance-actions {
        margin-top: 1.5rem;
        min-height: auto;
      }
      
      .btn-lg {
        width: 100%;
        margin-bottom: 0.75rem;
        padding: 0.875rem 1.5rem;
        font-size: 1rem;
      }
      
      .dropdown {
        width: 100%;
      }
      
      .dropdown .btn {
        width: 100%;
      }
      
      .dropdown-menu {
        width: 100%;
      }
    }

    /* Extra small screens */
    @media (max-width: 576px) {
      .card-body {
        padding: 1rem;
      }
      
      .status-indicator {
        width: 45px;
        height: 45px;
        font-size: 1.1rem;
      }
      
      .row.text-center .col-4 {
        margin-bottom: 1rem;
      }
      
      .btn-lg {
        padding: 0.75rem 1.25rem;
        font-size: 0.95rem;
      }
      
      .dropdown-item {
        padding: 0.875rem 1rem;
        font-size: 0.95rem;
      }
    }

    /* Touch-friendly improvements */
    .btn {
      -webkit-tap-highlight-color: rgba(0, 0, 0, 0.1);
      touch-action: manipulation;
    }

    .btn:active {
      transform: scale(0.98);
    }

    .dropdown-item {
      -webkit-tap-highlight-color: rgba(0, 0, 0, 0.05);
      touch-action: manipulation;
    }

    /* Improved alert styling for mobile */
    @media (max-width: 768px) {
      .alert {
        margin-bottom: 1rem;
        padding: 0.875rem 1rem;
        font-size: 0.9rem;
      }
      
      .alert .btn-close {
        padding: 0.5rem;
      }
    }
  `]
})
export class AttendanceTrackerComponent implements OnInit, OnDestroy {
  attendanceStatus: AttendanceStatus | null = null;
  isLoading = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;
  
  breakTypes = Object.values(BreakType);
  private destroy$ = new Subject<void>();

  constructor(
    private attendanceService: AttendanceService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAttendanceStatus();
    this.subscribeToStatusUpdates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadAttendanceStatus(): void {
    // For development, use mock data
    this.attendanceStatus = this.attendanceService.getMockAttendanceStatus();
    
    // Uncomment for production
    // this.attendanceService.getCurrentEmployeeStatus()
    //   .pipe(takeUntil(this.destroy$))
    //   .subscribe({
    //     next: (status) => this.attendanceStatus = status,
    //     error: (error) => this.handleError('Failed to load attendance status', error)
    //   });
  }

  private subscribeToStatusUpdates(): void {
    this.attendanceService.attendanceStatus$
      .pipe(takeUntil(this.destroy$))
      .subscribe(status => {
        if (status) {
          this.attendanceStatus = status;
        }
      });
  }

  checkIn(): void {
    this.isLoading = true;
    this.clearMessages();

    this.attendanceService.checkIn()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (record) => {
          this.isLoading = false;
          this.successMessage = `Check-in successful! Welcome back at ${this.formatTime(record.checkInTime)}`;
          this.loadAttendanceStatus();
        },
        error: (error) => {
          this.isLoading = false;
          this.handleError('Check-in failed', error);
        }
      });
  }

  checkOut(): void {
    this.isLoading = true;
    this.clearMessages();

    this.attendanceService.checkOut()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (record) => {
          this.isLoading = false;
          this.successMessage = `Check-out successful! Total working hours: ${this.formatTime(record.totalWorkingHours)}`;
          this.loadAttendanceStatus();
        },
        error: (error) => {
          this.isLoading = false;
          this.handleError('Check-out failed', error);
        }
      });
  }

  startBreak(type: BreakType): void {
    this.isLoading = true;
    this.clearMessages();

    this.attendanceService.startBreak(type)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (record) => {
          this.isLoading = false;
          this.successMessage = `${this.getBreakLabel(type)} started successfully!`;
          this.loadAttendanceStatus();
        },
        error: (error) => {
          this.isLoading = false;
          this.handleError('Failed to start break', error);
        }
      });
  }

  endBreak(): void {
    this.isLoading = true;
    this.clearMessages();

    this.attendanceService.endBreak()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (record) => {
          this.isLoading = false;
          this.successMessage = 'Break ended successfully!';
          this.loadAttendanceStatus();
        },
        error: (error) => {
          this.isLoading = false;
          this.handleError('Failed to end break', error);
        }
      });
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
        // If it's a duration format (HH:MM:SS)
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

  private clearMessages(): void {
    this.successMessage = null;
    this.errorMessage = null;
  }

  private handleError(message: string, error: any): void {
    console.error(message, error);
    this.errorMessage = error.error?.message || message;
  }
}