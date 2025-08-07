import { Component, OnInit, OnDestroy, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { EnhancedAttendanceService } from '../../../services/enhanced-attendance.service';
import { 
  BreakType, 
  BreakRecord,
  BreakTypeLabels,
  AttendanceStatus,
  AttendanceRecord 
} from '../../../models/attendance.models';

@Component({
  selector: 'app-break-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="break-management">
      <div class="break-header">
        <h6 class="break-title">
          <i class="fas fa-coffee me-2"></i>
          Break Management
        </h6>
        <div class="break-status" *ngIf="currentBreak">
          <span class="badge bg-info">
            <i class="fas fa-clock me-1"></i>
            On {{ getBreakLabel(currentBreak.type) }}
          </span>
        </div>
      </div>

      <!-- Current Break Info -->
      <div class="current-break-info" *ngIf="currentBreak">
        <div class="break-card bg-info">
          <div class="break-details">
            <div class="break-type">
              <i class="fas" [class]="getBreakIcon(currentBreak.type)"></i>
              {{ getBreakLabel(currentBreak.type) }}
            </div>
            <div class="break-duration">
              Started: {{ formatTime(currentBreak.startTime) }}
              <div class="duration-counter" *ngIf="breakDuration">
                Duration: {{ breakDuration }}
              </div>
            </div>
          </div>
          <button 
            class="btn btn-light btn-sm"
            (click)="endCurrentBreak()"
            [disabled]="isLoading">
            <i class="fas fa-stop me-1"></i>
            <span *ngIf="!isLoading">End Break</span>
            <span *ngIf="isLoading">
              <span class="spinner-border spinner-border-sm me-1"></span>
              Ending...
            </span>
          </button>
        </div>
      </div>

      <!-- Start Break Options -->
      <div class="start-break-options" *ngIf="!currentBreak && canTakeBreak">
        <div class="break-types">
          <div class="row g-2">
            <div class="col-6" *ngFor="let breakType of availableBreakTypes">
              <button 
                class="btn btn-outline-primary btn-sm w-100"
                (click)="startBreak(breakType)"
                [disabled]="isLoading">
                <i class="fas" [class]="getBreakIcon(breakType)" class="me-1"></i>
                <div class="break-label">{{ getBreakLabel(breakType) }}</div>
                <small class="break-description">{{ getBreakDescription(breakType) }}</small>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Break History (Today) -->
      <div class="break-history" *ngIf="showHistory && todayBreaks.length > 0">
        <div class="history-header">
          <h6 class="history-title">Today's Breaks</h6>
        </div>
        <div class="history-list">
          <div class="history-item" *ngFor="let breakRecord of todayBreaks">
            <div class="history-icon">
              <i class="fas" [class]="getBreakIcon(breakRecord.type)"></i>
            </div>
            <div class="history-details">
              <div class="history-type">{{ getBreakLabel(breakRecord.type) }}</div>
              <div class="history-time">
                {{ formatTime(breakRecord.startTime) }} - 
                {{ breakRecord.endTime ? formatTime(breakRecord.endTime) : 'Ongoing' }}
              </div>
              <div class="history-duration" *ngIf="breakRecord.duration">
                Duration: {{ formatDuration(breakRecord.duration) }}
              </div>
            </div>
          </div>
        </div>
        
        <div class="break-summary">
          <div class="summary-item">
            <span class="summary-label">Total Breaks:</span>
            <span class="summary-value">{{ todayBreaks.length }}</span>
          </div>
          <div class="summary-item">
            <span class="summary-label">Total Break Time:</span>
            <span class="summary-value">{{ getTotalBreakTime() }}</span>
          </div>
        </div>
      </div>

      <!-- No Break State -->
      <div class="no-break-state" *ngIf="!currentBreak && !canTakeBreak">
        <div class="text-center text-muted">
          <i class="fas fa-clock mb-2" style="font-size: 2rem; opacity: 0.5;"></i>
          <p class="mb-0">{{ getNoBreakMessage() }}</p>
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
    .break-management {
      background: white;
      border-radius: 8px;
      border: 1px solid var(--bs-border-color);
      overflow: hidden;
    }

    .break-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem;
      background: var(--bs-light);
      border-bottom: 1px solid var(--bs-border-color);
    }

    .break-title {
      margin: 0;
      font-weight: 600;
      color: var(--bs-dark);
    }

    .current-break-info {
      padding: 1rem;
    }

    .break-card {
      border-radius: 8px;
      padding: 1rem;
      color: white;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .break-details {
      flex: 1;
    }

    .break-type {
      font-weight: 600;
      font-size: 1.1rem;
      margin-bottom: 0.5rem;
    }

    .break-type i {
      margin-right: 0.5rem;
    }

    .break-duration {
      font-size: 0.9rem;
      opacity: 0.9;
    }

    .duration-counter {
      margin-top: 0.25rem;
      font-weight: 500;
    }

    .start-break-options {
      padding: 1rem;
    }

    .break-types .btn {
      height: auto;
      padding: 0.75rem 0.5rem;
      text-align: center;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.25rem;
    }

    .break-label {
      font-weight: 600;
      font-size: 0.875rem;
    }

    .break-description {
      font-size: 0.75rem;
      opacity: 0.8;
    }

    .break-history {
      border-top: 1px solid var(--bs-border-color);
    }

    .history-header {
      padding: 0.75rem 1rem;
      background: var(--bs-light);
      border-bottom: 1px solid var(--bs-border-color);
    }

    .history-title {
      margin: 0;
      font-size: 0.875rem;
      font-weight: 600;
      color: var(--bs-dark);
    }

    .history-list {
      max-height: 200px;
      overflow-y: auto;
    }

    .history-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.75rem 1rem;
      border-bottom: 1px solid var(--bs-border-color-translucent);
    }

    .history-item:last-child {
      border-bottom: none;
    }

    .history-icon {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background: var(--bs-primary);
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.875rem;
      flex-shrink: 0;
    }

    .history-details {
      flex: 1;
    }

    .history-type {
      font-weight: 600;
      font-size: 0.875rem;
      color: var(--bs-dark);
    }

    .history-time {
      font-size: 0.75rem;
      color: var(--bs-secondary);
    }

    .history-duration {
      font-size: 0.75rem;
      color: var(--bs-info);
      font-weight: 500;
    }

    .break-summary {
      padding: 0.75rem 1rem;
      background: var(--bs-light);
      border-top: 1px solid var(--bs-border-color);
    }

    .summary-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 0.875rem;
      margin-bottom: 0.25rem;
    }

    .summary-item:last-child {
      margin-bottom: 0;
    }

    .summary-label {
      color: var(--bs-secondary);
    }

    .summary-value {
      font-weight: 600;
      color: var(--bs-dark);
    }

    .no-break-state {
      padding: 2rem 1rem;
    }

    .alert-sm {
      margin: 0.5rem 1rem;
      padding: 0.5rem 0.75rem;
      font-size: 0.875rem;
    }

    /* Mobile responsive */
    @media (max-width: 768px) {
      .break-card {
        flex-direction: column;
        gap: 1rem;
        text-align: center;
      }

      .break-types .col-6 {
        margin-bottom: 0.5rem;
      }

      .break-types .btn {
        padding: 0.5rem 0.25rem;
      }

      .break-label {
        font-size: 0.8rem;
      }

      .break-description {
        font-size: 0.7rem;
      }
    }
  `]
})
export class BreakManagementComponent implements OnInit, OnDestroy {
  @Input() attendanceStatus: AttendanceStatus | null = null;
  @Input() showHistory = true;
  @Input() canTakeBreak = true;
  @Output() breakStarted = new EventEmitter<AttendanceRecord>();
  @Output() breakEnded = new EventEmitter<AttendanceRecord>();

  currentBreak: BreakRecord | null = null;
  todayBreaks: BreakRecord[] = [];
  breakDuration: string | null = null;
  
  isLoading = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;

  availableBreakTypes = Object.values(BreakType);
  private destroy$ = new Subject<void>();
  private durationInterval: any;

  constructor(private attendanceService: EnhancedAttendanceService) {}

  ngOnInit(): void {
    this.updateBreakInfo();
    this.startDurationCounter();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.durationInterval) {
      clearInterval(this.durationInterval);
    }
  }

  ngOnChanges(): void {
    this.updateBreakInfo();
  }

  private updateBreakInfo(): void {
    if (this.attendanceStatus) {
      this.currentBreak = this.attendanceStatus.currentBreak || null;
      // In a real implementation, you would load today's breaks from the service
      this.todayBreaks = []; // TODO: Load from service
    }
  }

  private startDurationCounter(): void {
    this.durationInterval = setInterval(() => {
      if (this.currentBreak && this.currentBreak.startTime) {
        const startTime = new Date(this.currentBreak.startTime);
        const now = new Date();
        const diff = now.getTime() - startTime.getTime();
        
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
        
        this.breakDuration = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
      } else {
        this.breakDuration = null;
      }
    }, 1000);
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
          this.breakStarted.emit(record);
          this.clearMessageAfterDelay();
        },
        error: (error) => {
          this.isLoading = false;
          this.handleError('Failed to start break', error);
        }
      });
  }

  endCurrentBreak(): void {
    this.isLoading = true;
    this.clearMessages();

    this.attendanceService.endBreak()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (record) => {
          this.isLoading = false;
          this.successMessage = 'Break ended successfully!';
          this.breakEnded.emit(record);
          this.clearMessageAfterDelay();
        },
        error: (error) => {
          this.isLoading = false;
          this.handleError('Failed to end break', error);
        }
      });
  }

  // Utility Methods
  getBreakLabel(type?: BreakType): string {
    if (!type) return '';
    return BreakTypeLabels[type] || type;
  }

  getBreakIcon(type?: BreakType): string {
    const icons = {
      [BreakType.Tea]: 'fa-coffee',
      [BreakType.Lunch]: 'fa-utensils',
      [BreakType.Personal]: 'fa-user',
      [BreakType.Meeting]: 'fa-users'
    };
    return icons[type as BreakType] || 'fa-coffee';
  }

  getBreakDescription(type: BreakType): string {
    const descriptions = {
      [BreakType.Tea]: 'Short break',
      [BreakType.Lunch]: 'Meal break',
      [BreakType.Personal]: 'Personal time',
      [BreakType.Meeting]: 'Meeting break'
    };
    return descriptions[type] || '';
  }

  getNoBreakMessage(): string {
    if (!this.canTakeBreak) {
      return 'Please check in first to take breaks';
    }
    return 'No active breaks. Click a break type to start.';
  }

  getTotalBreakTime(): string {
    if (this.todayBreaks.length === 0) return '00:00';
    
    let totalMinutes = 0;
    this.todayBreaks.forEach(breakRecord => {
      if (breakRecord.duration) {
        const [hours, minutes] = breakRecord.duration.split(':').map(Number);
        totalMinutes += (hours * 60) + minutes;
      }
    });

    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
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