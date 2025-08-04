import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { Calendar, CalendarOptions } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import { LeaveService } from '../../../services/leave.service';
import { EmployeeService } from '../../../services/employee.service';
import {
  LeaveCalendarEntry,
  LeaveConflict,
  CalendarEvent,
  LeaveType,
  LeaveStatus
} from '../../../models/leave.models';
import { Employee } from '../../../models/employee.models';

@Component({
  selector: 'app-leave-calendar',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgbModule],
  template: `
    <div class="card">
      <div class="card-header">
        <div class="d-flex justify-content-between align-items-center">
          <h5 class="card-title mb-0">
            <i class="fas fa-calendar-alt me-2"></i>
            Leave Calendar
          </h5>
          <div class="d-flex gap-2">
            <button 
              class="btn btn-outline-primary btn-sm"
              (click)="toggleView()">
              <i class="fas fa-eye me-1"></i>
              {{ currentView === 'dayGridMonth' ? 'Week View' : 'Month View' }}
            </button>
            <button 
              class="btn btn-primary btn-sm"
              (click)="refreshCalendar()">
              <i class="fas fa-sync-alt me-1"></i>
              Refresh
            </button>
          </div>
        </div>
      </div>
      
      <div class="card-body">
        <!-- Filters -->
        <form [formGroup]="filterForm" class="mb-4">
          <div class="row g-3">
            <div class="col-md-3">
              <label class="form-label">Employee</label>
              <select class="form-select form-select-sm" formControlName="employeeId">
                <option value="">All Employees</option>
                <option *ngFor="let employee of employees" [value]="employee.id">
                  {{ employee.firstName }} {{ employee.lastName }}
                </option>
              </select>
            </div>
            <div class="col-md-3">
              <label class="form-label">Department</label>
              <select class="form-select form-select-sm" formControlName="department">
                <option value="">All Departments</option>
                <option *ngFor="let dept of departments" [value]="dept">
                  {{ dept }}
                </option>
              </select>
            </div>
            <div class="col-md-3">
              <label class="form-label">Leave Type</label>
              <select class="form-select form-select-sm" formControlName="leaveType">
                <option value="">All Types</option>
                <option *ngFor="let type of leaveTypes" [value]="type.value">
                  {{ type.label }}
                </option>
              </select>
            </div>
            <div class="col-md-3">
              <label class="form-label">Status</label>
              <select class="form-select form-select-sm" formControlName="status">
                <option value="">All Status</option>
                <option *ngFor="let status of leaveStatuses" [value]="status.value">
                  {{ status.label }}
                </option>
              </select>
            </div>
          </div>
        </form>

        <!-- Legend -->
        <div class="mb-3">
          <small class="text-muted me-3">Legend:</small>
          <span *ngFor="let type of leaveTypes" class="badge me-2 mb-1" 
                [style.background-color]="getLeaveTypeColor(type.value)">
            {{ type.label }}
          </span>
        </div>

        <!-- Calendar -->
        <div #calendarContainer class="calendar-container"></div>

        <!-- Conflicts Alert -->
        <div *ngIf="conflicts.length > 0" class="alert alert-warning mt-3">
          <h6 class="alert-heading">
            <i class="fas fa-exclamation-triangle me-2"></i>
            Leave Conflicts Detected
          </h6>
          <div class="row">
            <div class="col-md-6" *ngFor="let conflict of conflicts">
              <div class="d-flex align-items-center mb-2">
                <div class="flex-shrink-0">
                  <div class="avatar-sm bg-warning rounded-circle d-flex align-items-center justify-content-center">
                    <i class="fas fa-user text-white"></i>
                  </div>
                </div>
                <div class="flex-grow-1 ms-3">
                  <h6 class="mb-0">{{ conflict.employeeName }}</h6>
                  <small class="text-muted">
                    {{ conflict.department }} - {{ conflict.conflictDate | date:'mediumDate' }}
                  </small>
                  <div class="text-warning">{{ conflict.conflictReason }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Statistics -->
        <div class="row mt-4">
          <div class="col-md-3">
            <div class="card bg-primary text-white">
              <div class="card-body text-center">
                <h4 class="mb-1">{{ statistics.totalLeaves }}</h4>
                <small>Total Leaves</small>
              </div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="card bg-success text-white">
              <div class="card-body text-center">
                <h4 class="mb-1">{{ statistics.approvedLeaves }}</h4>
                <small>Approved</small>
              </div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="card bg-warning text-white">
              <div class="card-body text-center">
                <h4 class="mb-1">{{ statistics.pendingLeaves }}</h4>
                <small>Pending</small>
              </div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="card bg-danger text-white">
              <div class="card-body text-center">
                <h4 class="mb-1">{{ statistics.conflicts }}</h4>
                <small>Conflicts</small>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Event Details Modal -->
    <div class="modal fade" id="eventModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content" *ngIf="selectedEvent">
          <div class="modal-header">
            <h5 class="modal-title">Leave Details</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div class="row">
              <div class="col-sm-4"><strong>Employee:</strong></div>
              <div class="col-sm-8">{{ selectedEvent.extendedProps.employeeName }}</div>
            </div>
            <div class="row mt-2">
              <div class="col-sm-4"><strong>Leave Type:</strong></div>
              <div class="col-sm-8">
                <span class="badge" [style.background-color]="getLeaveTypeColor(selectedEvent.extendedProps.leaveType)">
                  {{ getLeaveTypeText(selectedEvent.extendedProps.leaveType) }}
                </span>
              </div>
            </div>
            <div class="row mt-2">
              <div class="col-sm-4"><strong>Status:</strong></div>
              <div class="col-sm-8">
                <span class="badge" [style.background-color]="getLeaveStatusColor(selectedEvent.extendedProps.status)">
                  {{ getLeaveStatusText(selectedEvent.extendedProps.status) }}
                </span>
              </div>
            </div>
            <div class="row mt-2">
              <div class="col-sm-4"><strong>Date:</strong></div>
              <div class="col-sm-8">{{ selectedEvent.start | date:'fullDate' }}</div>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
            <button 
              type="button" 
              class="btn btn-primary"
              (click)="viewLeaveRequest(selectedEvent.extendedProps.requestId)">
              View Request
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .calendar-container {
      min-height: 600px;
    }

    .card {
      border-radius: 12px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      background: linear-gradient(135deg, var(--bs-primary) 0%, #0056b3 100%);
      color: white;
      border-radius: 12px 12px 0 0;
    }

    .form-select-sm {
      border-radius: 6px;
    }

    .badge {
      font-size: 0.75em;
      padding: 0.35em 0.65em;
      border-radius: 6px;
    }

    .avatar-sm {
      width: 2rem;
      height: 2rem;
    }

    .alert {
      border-radius: 8px;
      border: none;
    }

    .alert-warning {
      background-color: #fff3cd;
      color: #856404;
    }

    .btn {
      border-radius: 6px;
    }

    /* FullCalendar customizations */
    :host ::ng-deep .fc {
      font-family: inherit;
    }

    :host ::ng-deep .fc-toolbar-title {
      font-size: 1.25rem;
      font-weight: 600;
    }

    :host ::ng-deep .fc-button {
      border-radius: 6px;
      font-weight: 500;
    }

    :host ::ng-deep .fc-event {
      border-radius: 4px;
      border: none;
      font-size: 0.8rem;
      padding: 2px 4px;
    }

    :host ::ng-deep .fc-daygrid-event {
      margin: 1px 0;
    }

    :host ::ng-deep .fc-day-today {
      background-color: rgba(13, 110, 253, 0.1) !important;
    }
  `]
})
export class LeaveCalendarComponent implements OnInit {
  @ViewChild('calendarContainer', { static: true }) calendarContainer!: ElementRef;

  filterForm: FormGroup;
  calendar!: Calendar;
  currentView = 'dayGridMonth';

  employees: Employee[] = [];
  departments: string[] = [];
  leaveEntries: LeaveCalendarEntry[] = [];
  conflicts: LeaveConflict[] = [];
  selectedEvent?: CalendarEvent;

  leaveTypes = [
    { value: LeaveType.Annual, label: 'Annual Leave' },
    { value: LeaveType.Sick, label: 'Sick Leave' },
    { value: LeaveType.Personal, label: 'Personal Leave' },
    { value: LeaveType.Maternity, label: 'Maternity Leave' },
    { value: LeaveType.Paternity, label: 'Paternity Leave' },
    { value: LeaveType.Emergency, label: 'Emergency Leave' },
    { value: LeaveType.Bereavement, label: 'Bereavement Leave' },
    { value: LeaveType.Study, label: 'Study Leave' },
    { value: LeaveType.Unpaid, label: 'Unpaid Leave' },
    { value: LeaveType.Compensatory, label: 'Compensatory Leave' }
  ];

  leaveStatuses = [
    { value: LeaveStatus.Pending, label: 'Pending' },
    { value: LeaveStatus.Approved, label: 'Approved' },
    { value: LeaveStatus.Rejected, label: 'Rejected' },
    { value: LeaveStatus.Cancelled, label: 'Cancelled' },
    { value: LeaveStatus.PartiallyApproved, label: 'Partially Approved' }
  ];

  statistics = {
    totalLeaves: 0,
    approvedLeaves: 0,
    pendingLeaves: 0,
    conflicts: 0
  };

  constructor(
    private fb: FormBuilder,
    private leaveService: LeaveService,
    private employeeService: EmployeeService
  ) {
    this.filterForm = this.createFilterForm();
  }

  ngOnInit(): void {
    this.initializeCalendar();
    this.loadEmployees();
    this.setupFilterSubscriptions();
    this.loadCalendarData();
  }

  private createFilterForm(): FormGroup {
    return this.fb.group({
      employeeId: [''],
      department: [''],
      leaveType: [''],
      status: ['']
    });
  }

  private initializeCalendar(): void {
    const calendarOptions: CalendarOptions = {
      plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
      initialView: 'dayGridMonth',
      headerToolbar: {
        left: 'prev,next today',
        center: 'title',
        right: 'dayGridMonth,timeGridWeek'
      },
      height: 'auto',
      events: [],
      eventClick: this.handleEventClick.bind(this),
      dateClick: this.handleDateClick.bind(this),
      eventDidMount: this.handleEventDidMount.bind(this)
    };

    this.calendar = new Calendar(this.calendarContainer.nativeElement, calendarOptions);
    this.calendar.render();
  }

  private setupFilterSubscriptions(): void {
    this.filterForm.valueChanges.subscribe(() => {
      this.loadCalendarData();
    });
  }

  private loadEmployees(): void {
    this.employeeService.getEmployees().subscribe({
      next: (pagedResult) => {
        this.employees = pagedResult.items;
        this.departments = [...new Set(pagedResult.items.map((emp: Employee) => emp.department).filter((dept: string) => dept))];
      },
      error: (error) => {
        console.error('Error loading employees:', error);
      }
    });
  }

  private loadCalendarData(): void {
    const currentDate = this.calendar.getDate();
    const startOfMonth = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1);
    const endOfMonth = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 0);

    // Load leave calendar
    this.leaveService.getLeaveCalendar(startOfMonth, endOfMonth).subscribe({
      next: (entries) => {
        this.leaveEntries = this.applyFilters(entries);
        this.updateCalendarEvents();
        this.updateStatistics();
      },
      error: (error) => {
        console.error('Error loading leave calendar:', error);
      }
    });

    // Load conflicts
    this.leaveService.getTeamLeaveConflicts(startOfMonth, endOfMonth).subscribe({
      next: (conflicts) => {
        this.conflicts = conflicts;
        this.statistics.conflicts = conflicts.length;
      },
      error: (error) => {
        console.error('Error loading conflicts:', error);
      }
    });
  }

  private applyFilters(entries: LeaveCalendarEntry[]): LeaveCalendarEntry[] {
    const filters = this.filterForm.value;

    return entries.filter(entry => {
      if (filters.employeeId && entry.employeeId !== parseInt(filters.employeeId)) {
        return false;
      }

      if (filters.leaveType && entry.leaveType !== parseInt(filters.leaveType)) {
        return false;
      }

      if (filters.status && entry.status !== parseInt(filters.status)) {
        return false;
      }

      // Department filter would require employee data
      if (filters.department) {
        const employee = this.employees.find(e => e.id === entry.employeeId);
        if (!employee || employee.department !== filters.department) {
          return false;
        }
      }

      return true;
    });
  }

  private updateCalendarEvents(): void {
    const events = this.leaveService.convertToCalendarEvents(this.leaveEntries);
    this.calendar.removeAllEvents();
    this.calendar.addEventSource(events);
  }

  private updateStatistics(): void {
    this.statistics.totalLeaves = this.leaveEntries.length;
    this.statistics.approvedLeaves = this.leaveEntries.filter(e => e.status === LeaveStatus.Approved).length;
    this.statistics.pendingLeaves = this.leaveEntries.filter(e => e.status === LeaveStatus.Pending).length;
  }

  private handleEventClick(info: any): void {
    this.selectedEvent = {
      id: info.event.id,
      title: info.event.title,
      start: info.event.start,
      end: info.event.end,
      allDay: info.event.allDay,
      backgroundColor: info.event.backgroundColor,
      borderColor: info.event.borderColor,
      textColor: info.event.textColor,
      extendedProps: info.event.extendedProps
    };

    // Show modal (you might want to use a proper modal service)
    const modal = document.getElementById('eventModal');
    if (modal) {
      const bootstrapModal = new (window as any).bootstrap.Modal(modal);
      bootstrapModal.show();
    }
  }

  private handleDateClick(info: any): void {
    // Handle date click - could open new leave request form
    console.log('Date clicked:', info.dateStr);
  }

  private handleEventDidMount(info: any): void {
    // Add tooltip or additional styling
    info.el.setAttribute('title', `${info.event.extendedProps.employeeName} - ${info.event.title}`);
  }

  toggleView(): void {
    this.currentView = this.currentView === 'dayGridMonth' ? 'timeGridWeek' : 'dayGridMonth';
    this.calendar.changeView(this.currentView);
  }

  refreshCalendar(): void {
    this.loadCalendarData();
  }

  viewLeaveRequest(requestId: number): void {
    // Navigate to leave request details
    console.log('View leave request:', requestId);
  }

  getLeaveTypeColor(leaveType: LeaveType): string {
    return this.leaveService.getLeaveTypeColor(leaveType);
  }

  getLeaveStatusColor(status: LeaveStatus): string {
    return this.leaveService.getLeaveStatusColor(status);
  }

  getLeaveTypeText(leaveType: LeaveType): string {
    return this.leaveService.getLeaveTypeText(leaveType);
  }

  getLeaveStatusText(status: LeaveStatus): string {
    return this.leaveService.getLeaveStatusText(status);
  }
}