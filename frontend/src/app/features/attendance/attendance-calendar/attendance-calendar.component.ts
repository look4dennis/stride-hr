import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AttendanceService } from '../../../services/attendance.service';
import { AttendanceCalendarResponse, AttendanceCalendarDay } from '../../../models/attendance.models';

@Component({
  selector: 'app-attendance-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container-fluid">
      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <div class="d-flex justify-content-between align-items-center">
                <h4 class="card-title mb-0">
                  <i class="fas fa-calendar-alt me-2"></i>
                  Attendance Calendar
                </h4>
                <div class="d-flex align-items-center gap-3">
                  <!-- Month/Year Navigation -->
                  <div class="d-flex align-items-center gap-2">
                    <button class="btn btn-outline-primary btn-sm" (click)="previousMonth()">
                      <i class="fas fa-chevron-left"></i>
                    </button>
                    <select class="form-select form-select-sm" [(ngModel)]="selectedMonth" (change)="loadCalendar()" style="width: auto;">
                      <option *ngFor="let month of months; let i = index" [value]="i + 1">
                        {{month}}
                      </option>
                    </select>
                    <select class="form-select form-select-sm" [(ngModel)]="selectedYear" (change)="loadCalendar()" style="width: auto;">
                      <option *ngFor="let year of years" [value]="year">
                        {{year}}
                      </option>
                    </select>
                    <button class="btn btn-outline-primary btn-sm" (click)="nextMonth()">
                      <i class="fas fa-chevron-right"></i>
                    </button>
                  </div>
                  
                  <!-- Employee Selection (for HR/Manager) -->
                  <div *ngIf="showEmployeeSelector">
                    <select class="form-select form-select-sm" [(ngModel)]="selectedEmployeeId" (change)="loadCalendar()" style="width: 200px;">
                      <option value="">Select Employee</option>
                      <option *ngFor="let emp of employees" [value]="emp.id">
                        {{emp.firstName}} {{emp.lastName}}
                      </option>
                    </select>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="card-body">
              <!-- Calendar Summary -->
              <div class="row mb-4" *ngIf="calendarData">
                <div class="col-12">
                  <div class="row g-3">
                    <div class="col-md-2">
                      <div class="card bg-primary text-white">
                        <div class="card-body text-center py-2">
                          <h5 class="mb-0">{{calendarData.summary.totalWorkingDays}}</h5>
                          <small>Working Days</small>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-2">
                      <div class="card bg-success text-white">
                        <div class="card-body text-center py-2">
                          <h5 class="mb-0">{{calendarData.summary.presentDays}}</h5>
                          <small>Present Days</small>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-2">
                      <div class="card bg-danger text-white">
                        <div class="card-body text-center py-2">
                          <h5 class="mb-0">{{calendarData.summary.absentDays}}</h5>
                          <small>Absent Days</small>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-2">
                      <div class="card bg-warning text-white">
                        <div class="card-body text-center py-2">
                          <h5 class="mb-0">{{calendarData.summary.lateDays}}</h5>
                          <small>Late Days</small>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-2">
                      <div class="card bg-info text-white">
                        <div class="card-body text-center py-2">
                          <h5 class="mb-0">{{calendarData.summary.attendancePercentage}}%</h5>
                          <small>Attendance</small>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-2">
                      <div class="card bg-secondary text-white">
                        <div class="card-body text-center py-2">
                          <h5 class="mb-0">{{formatTimeSpan(calendarData.summary.totalWorkingHours)}}</h5>
                          <small>Total Hours</small>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Calendar Grid -->
              <div class="calendar-container" *ngIf="calendarData">
                <div class="calendar-grid">
                  <!-- Day Headers -->
                  <div class="calendar-header">
                    <div class="day-header" *ngFor="let day of dayHeaders">
                      {{day}}
                    </div>
                  </div>
                  
                  <!-- Calendar Days -->
                  <div class="calendar-body">
                    <div 
                      class="calendar-day" 
                      *ngFor="let day of calendarDays"
                      [class.weekend]="day.isWeekend"
                      [class.holiday]="day.isHoliday"
                      [class.present]="day.status === 'Present'"
                      [class.absent]="day.status === 'Absent'"
                      [class.late]="day.isLate"
                      [class.on-break]="day.status === 'OnBreak'"
                      [class.other-month]="!day.isCurrentMonth"
                      (click)="selectDay(day)"
                      [class.selected]="selectedDay?.date === day.date">
                      
                      <div class="day-number">{{day.dayNumber}}</div>
                      
                      <div class="day-content" *ngIf="day.isCurrentMonth">
                        <!-- Status Indicator -->
                        <div class="status-indicator" [title]="getStatusTitle(day)">
                          <i class="fas fa-circle" 
                             [class.text-success]="day.status === 'Present' && !day.isLate"
                             [class.text-warning]="day.isLate"
                             [class.text-danger]="day.status === 'Absent'"
                             [class.text-info]="day.status === 'OnBreak'"
                             [class.text-muted]="day.isWeekend || day.isHoliday">
                          </i>
                        </div>
                        
                        <!-- Time Info -->
                        <div class="time-info" *ngIf="day.checkInTime">
                          <small class="text-muted">
                            {{formatTime(day.checkInTime)}}
                            <span *ngIf="day.checkOutTime"> - {{formatTime(day.checkOutTime)}}</span>
                          </small>
                        </div>
                        
                        <!-- Late Indicator -->
                        <div class="late-indicator" *ngIf="day.isLate">
                          <small class="text-warning">
                            <i class="fas fa-clock"></i> Late
                          </small>
                        </div>
                        
                        <!-- Holiday Name -->
                        <div class="holiday-name" *ngIf="day.isHoliday">
                          <small class="text-primary">{{day.holidayName}}</small>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Day Details Modal/Panel -->
              <div class="row mt-4" *ngIf="selectedDay">
                <div class="col-12">
                  <div class="card border-primary">
                    <div class="card-header bg-primary text-white">
                      <h6 class="mb-0">
                        <i class="fas fa-info-circle me-2"></i>
                        Details for {{formatDate(selectedDay.date)}}
                      </h6>
                    </div>
                    <div class="card-body">
                      <div class="row">
                        <div class="col-md-6">
                          <table class="table table-sm">
                            <tr>
                              <td><strong>Status:</strong></td>
                              <td>
                                <span class="badge" 
                                      [class.bg-success]="selectedDay.status === 'Present' && !selectedDay.isLate"
                                      [class.bg-warning]="selectedDay.isLate"
                                      [class.bg-danger]="selectedDay.status === 'Absent'"
                                      [class.bg-info]="selectedDay.status === 'OnBreak'">
                                  {{getStatusDisplay(selectedDay.status)}}
                                  <span *ngIf="selectedDay.isLate"> (Late)</span>
                                </span>
                              </td>
                            </tr>
                            <tr *ngIf="selectedDay.checkInTime">
                              <td><strong>Check In:</strong></td>
                              <td>{{formatDateTime(selectedDay.checkInTime)}}</td>
                            </tr>
                            <tr *ngIf="selectedDay.checkOutTime">
                              <td><strong>Check Out:</strong></td>
                              <td>{{formatDateTime(selectedDay.checkOutTime)}}</td>
                            </tr>
                            <tr *ngIf="selectedDay.workingHours">
                              <td><strong>Working Hours:</strong></td>
                              <td>{{formatTimeSpan(selectedDay.workingHours)}}</td>
                            </tr>
                          </table>
                        </div>
                        <div class="col-md-6">
                          <table class="table table-sm">
                            <tr *ngIf="selectedDay.breakDuration">
                              <td><strong>Break Duration:</strong></td>
                              <td>{{formatTimeSpan(selectedDay.breakDuration)}}</td>
                            </tr>
                            <tr *ngIf="selectedDay.overtimeHours">
                              <td><strong>Overtime:</strong></td>
                              <td class="text-info">{{formatTimeSpan(selectedDay.overtimeHours)}}</td>
                            </tr>
                            <tr *ngIf="selectedDay.isLate && selectedDay.lateBy">
                              <td><strong>Late By:</strong></td>
                              <td class="text-warning">{{formatTimeSpan(selectedDay.lateBy)}}</td>
                            </tr>
                            <tr *ngIf="selectedDay.isEarlyOut && selectedDay.earlyOutBy">
                              <td><strong>Early Out By:</strong></td>
                              <td class="text-warning">{{formatTimeSpan(selectedDay.earlyOutBy)}}</td>
                            </tr>
                          </table>
                        </div>
                      </div>
                      
                      <!-- Break Details -->
                      <div *ngIf="selectedDay.breaks && selectedDay.breaks.length > 0">
                        <h6 class="mt-3">Break Details</h6>
                        <div class="table-responsive">
                          <table class="table table-sm">
                            <thead>
                              <tr>
                                <th>Type</th>
                                <th>Start Time</th>
                                <th>End Time</th>
                                <th>Duration</th>
                              </tr>
                            </thead>
                            <tbody>
                              <tr *ngFor="let break of selectedDay.breaks">
                                <td>
                                  <span class="badge bg-secondary">{{break.type}}</span>
                                </td>
                                <td>{{formatDateTime(break.startTime)}}</td>
                                <td>{{break.endTime ? formatDateTime(break.endTime) : 'Ongoing'}}</td>
                                <td>{{break.duration ? formatTimeSpan(break.duration) : '-'}}</td>
                              </tr>
                            </tbody>
                          </table>
                        </div>
                      </div>
                      
                      <!-- Notes -->
                      <div *ngIf="selectedDay.notes">
                        <h6 class="mt-3">Notes</h6>
                        <p class="text-muted">{{selectedDay.notes}}</p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Loading State -->
              <div class="text-center py-5" *ngIf="loading">
                <div class="spinner-border text-primary" role="status">
                  <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mt-2 text-muted">Loading calendar...</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .calendar-container {
      max-width: 100%;
      overflow-x: auto;
    }
    
    .calendar-grid {
      min-width: 700px;
    }
    
    .calendar-header {
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      gap: 1px;
      background-color: #dee2e6;
      border: 1px solid #dee2e6;
    }
    
    .day-header {
      background-color: #495057;
      color: white;
      padding: 10px;
      text-align: center;
      font-weight: 600;
      font-size: 14px;
    }
    
    .calendar-body {
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      gap: 1px;
      background-color: #dee2e6;
      border: 1px solid #dee2e6;
      border-top: none;
    }
    
    .calendar-day {
      background-color: white;
      min-height: 100px;
      padding: 8px;
      cursor: pointer;
      transition: all 0.2s ease;
      position: relative;
    }
    
    .calendar-day:hover {
      background-color: #f8f9fa;
      transform: translateY(-1px);
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    
    .calendar-day.selected {
      background-color: #e3f2fd;
      border: 2px solid #2196f3;
    }
    
    .calendar-day.weekend {
      background-color: #f8f9fa;
    }
    
    .calendar-day.holiday {
      background-color: #fff3e0;
    }
    
    .calendar-day.present {
      border-left: 4px solid #28a745;
    }
    
    .calendar-day.absent {
      border-left: 4px solid #dc3545;
    }
    
    .calendar-day.late {
      border-left: 4px solid #ffc107;
    }
    
    .calendar-day.other-month {
      opacity: 0.3;
      pointer-events: none;
    }
    
    .day-number {
      font-weight: 600;
      font-size: 16px;
      margin-bottom: 4px;
    }
    
    .day-content {
      font-size: 12px;
    }
    
    .status-indicator {
      margin-bottom: 2px;
    }
    
    .time-info {
      margin-bottom: 2px;
    }
    
    .late-indicator {
      margin-bottom: 2px;
    }
    
    .holiday-name {
      font-weight: 500;
    }
    
    @media (max-width: 768px) {
      .calendar-day {
        min-height: 80px;
        padding: 4px;
      }
      
      .day-number {
        font-size: 14px;
      }
      
      .day-content {
        font-size: 10px;
      }
    }
  `]
})
export class AttendanceCalendarComponent implements OnInit {
  @Input() employeeId?: number;
  @Input() showEmployeeSelector = false;

  calendarData: AttendanceCalendarResponse | null = null;
  selectedDay: any = null;
  loading = false;
  
  selectedMonth = new Date().getMonth() + 1;
  selectedYear = new Date().getFullYear();
  selectedEmployeeId?: number;
  
  employees: any[] = [];
  
  months = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];
  
  dayHeaders = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  
  years: number[] = [];
  calendarDays: any[] = [];

  constructor(private attendanceService: AttendanceService) {
    // Generate years (current year ± 5)
    const currentYear = new Date().getFullYear();
    for (let i = currentYear - 5; i <= currentYear + 1; i++) {
      this.years.push(i);
    }
  }

  ngOnInit() {
    if (this.employeeId) {
      this.selectedEmployeeId = this.employeeId;
    }
    
    if (this.showEmployeeSelector) {
      this.loadEmployees();
    }
    
    this.loadCalendar();
  }

  loadCalendar() {
    if (!this.selectedEmployeeId && this.showEmployeeSelector) {
      return;
    }

    this.loading = true;
    const empId = this.selectedEmployeeId || this.employeeId;
    if (empId) {
      this.attendanceService.getAttendanceCalendar(empId, this.selectedYear, this.selectedMonth)
        .subscribe({
          next: (data) => {
            this.calendarData = data;
            this.generateCalendarDays();
            this.loading = false;
          },
          error: (error) => {
            console.error('Error loading calendar:', error);
            this.loading = false;
          }
        });
    } else {
      this.loading = false;
    }
  }

  generateCalendarDays() {
    if (!this.calendarData) return;

    const firstDay = new Date(this.selectedYear, this.selectedMonth - 1, 1);
    const lastDay = new Date(this.selectedYear, this.selectedMonth, 0);
    const startDate = new Date(firstDay);
    startDate.setDate(startDate.getDate() - firstDay.getDay());
    
    const endDate = new Date(lastDay);
    endDate.setDate(endDate.getDate() + (6 - lastDay.getDay()));

    this.calendarDays = [];
    
    for (let date = new Date(startDate); date <= endDate; date.setDate(date.getDate() + 1)) {
      const isCurrentMonth = date.getMonth() === this.selectedMonth - 1;
      const dayData = this.calendarData.days.find(d => 
        new Date(d.date).toDateString() === date.toDateString()
      );
      
      this.calendarDays.push({
        date: new Date(date),
        dayNumber: date.getDate(),
        isCurrentMonth,
        isWeekend: date.getDay() === 0 || date.getDay() === 6,
        ...dayData
      });
    }
  }

  previousMonth() {
    if (this.selectedMonth === 1) {
      this.selectedMonth = 12;
      this.selectedYear--;
    } else {
      this.selectedMonth--;
    }
    this.loadCalendar();
  }

  nextMonth() {
    if (this.selectedMonth === 12) {
      this.selectedMonth = 1;
      this.selectedYear++;
    } else {
      this.selectedMonth++;
    }
    this.loadCalendar();
  }

  selectDay(day: any) {
    if (!day.isCurrentMonth) return;
    this.selectedDay = day;
  }

  getStatusTitle(day: any): string {
    if (day.isWeekend) return 'Weekend';
    if (day.isHoliday) return `Holiday: ${day.holidayName}`;
    if (day.status === 'Present' && day.isLate) return 'Present (Late)';
    return day.status || 'Absent';
  }

  getStatusDisplay(status: string): string {
    switch (status) {
      case 'Present': return 'Present';
      case 'Absent': return 'Absent';
      case 'OnBreak': return 'On Break';
      case 'Late': return 'Late';
      default: return status;
    }
  }

  formatDate(date: string | Date): string {
    return new Date(date).toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  formatTime(time: string | Date): string {
    return new Date(time).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatDateTime(dateTime: string | Date): string {
    return new Date(dateTime).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatTimeSpan(timeSpan: string): string {
    if (!timeSpan) return '00:00';
    
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
}