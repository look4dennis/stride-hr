import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { LeaveService } from '../../../services/leave.service';
import { LeaveRequestFormComponent } from '../leave-request-form/leave-request-form.component';
import { LeaveCalendarComponent } from '../leave-calendar/leave-calendar.component';
import { LeaveBalanceComponent } from '../leave-balance/leave-balance.component';
import { 
  LeaveRequest, 
  CreateLeaveRequest, 
  LeaveStatus, 
  LeaveType,
  LeaveRequestFilter 
} from '../../../models/leave.models';

@Component({
  selector: 'app-leave-list',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    NgbModule,
    LeaveRequestFormComponent,
    LeaveCalendarComponent,
    LeaveBalanceComponent
  ],
  template: `
    <div class="page-header">
      <div class="d-flex justify-content-between align-items-center">
        <div>
          <h1>Leave Management</h1>
          <p class="text-muted">Request and manage employee leave</p>
        </div>
        <div class="d-flex gap-2">
          <button 
            class="btn btn-outline-primary"
            [class.active]="activeTab === 'requests'"
            (click)="setActiveTab('requests')">
            <i class="fas fa-list me-2"></i>My Requests
          </button>
          <button 
            class="btn btn-outline-primary"
            [class.active]="activeTab === 'calendar'"
            (click)="setActiveTab('calendar')">
            <i class="fas fa-calendar-alt me-2"></i>Calendar
          </button>
          <button 
            class="btn btn-outline-primary"
            [class.active]="activeTab === 'balance'"
            (click)="setActiveTab('balance')">
            <i class="fas fa-chart-pie me-2"></i>Balance
          </button>
          <button 
            class="btn btn-primary"
            (click)="showNewRequestForm()">
            <i class="fas fa-plus me-2"></i>New Request
          </button>
        </div>
      </div>
    </div>

    <!-- New Request Form Modal -->
    <div class="modal fade" id="newRequestModal" tabindex="-1" *ngIf="showRequestForm">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <app-leave-request-form
            [leaveRequest]="editingRequest"
            [isEditMode]="!!editingRequest"
            (formSubmit)="onRequestSubmit($event)"
            (formCancel)="hideRequestForm()">
          </app-leave-request-form>
        </div>
      </div>
    </div>

    <!-- Main Content -->
    <div class="row" *ngIf="activeTab === 'requests'">
      <!-- Filters -->
      <div class="col-12 mb-4">
        <div class="card">
          <div class="card-body">
            <form [formGroup]="filterForm" class="row g-3">
              <div class="col-md-3">
                <label class="form-label">Status</label>
                <select class="form-select" formControlName="status">
                  <option value="">All Status</option>
                  <option *ngFor="let status of leaveStatuses" [value]="status.value">
                    {{ status.label }}
                  </option>
                </select>
              </div>
              <div class="col-md-3">
                <label class="form-label">Leave Type</label>
                <select class="form-select" formControlName="leaveType">
                  <option value="">All Types</option>
                  <option *ngFor="let type of leaveTypes" [value]="type.value">
                    {{ type.label }}
                  </option>
                </select>
              </div>
              <div class="col-md-3">
                <label class="form-label">From Date</label>
                <input type="date" class="form-control" formControlName="startDate">
              </div>
              <div class="col-md-3">
                <label class="form-label">To Date</label>
                <input type="date" class="form-control" formControlName="endDate">
              </div>
            </form>
          </div>
        </div>
      </div>

      <!-- Leave Requests List -->
      <div class="col-12">
        <div class="card">
          <div class="card-header">
            <h5 class="card-title mb-0">
              <i class="fas fa-list me-2"></i>
              My Leave Requests
            </h5>
          </div>
          <div class="card-body">
            <div class="table-responsive" *ngIf="filteredRequests.length > 0; else noRequests">
              <table class="table table-hover">
                <thead>
                  <tr>
                    <th>Leave Type</th>
                    <th>Dates</th>
                    <th>Days</th>
                    <th>Status</th>
                    <th>Applied On</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let request of filteredRequests">
                    <td>
                      <span class="badge" [style.background-color]="getLeaveTypeColor(request.leaveType)">
                        {{ request.leaveTypeName }}
                      </span>
                    </td>
                    <td>
                      <div>
                        <strong>{{ request.startDate | date:'mediumDate' }}</strong>
                        <span *ngIf="request.startDate !== request.endDate">
                          to <strong>{{ request.endDate | date:'mediumDate' }}</strong>
                        </span>
                      </div>
                    </td>
                    <td>
                      <span class="fw-bold">{{ request.requestedDays }}</span>
                      <span *ngIf="request.approvedDays !== request.requestedDays" class="text-muted">
                        ({{ request.approvedDays }} approved)
                      </span>
                    </td>
                    <td>
                      <span class="badge" [style.background-color]="getLeaveStatusColor(request.status)">
                        {{ getLeaveStatusText(request.status) }}
                      </span>
                    </td>
                    <td>{{ request.createdAt | date:'mediumDate' }}</td>
                    <td>
                      <div class="btn-group btn-group-sm">
                        <button 
                          class="btn btn-outline-primary"
                          (click)="viewRequest(request)">
                          <i class="fas fa-eye"></i>
                        </button>
                        <button 
                          class="btn btn-outline-warning"
                          *ngIf="canEdit(request)"
                          (click)="editRequest(request)">
                          <i class="fas fa-edit"></i>
                        </button>
                        <button 
                          class="btn btn-outline-danger"
                          *ngIf="canCancel(request)"
                          (click)="cancelRequest(request)">
                          <i class="fas fa-times"></i>
                        </button>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <ng-template #noRequests>
              <div class="text-center py-5">
                <i class="fas fa-calendar-times text-muted mb-3" style="font-size: 3rem;"></i>
                <h5 class="text-muted">No Leave Requests Found</h5>
                <p class="text-muted">You haven't submitted any leave requests yet.</p>
                <button class="btn btn-primary" (click)="showNewRequestForm()">
                  <i class="fas fa-plus me-2"></i>Submit Your First Request
                </button>
              </div>
            </ng-template>
          </div>
        </div>
      </div>
    </div>

    <!-- Calendar Tab -->
    <div *ngIf="activeTab === 'calendar'">
      <app-leave-calendar></app-leave-calendar>
    </div>

    <!-- Balance Tab -->
    <div *ngIf="activeTab === 'balance'">
      <app-leave-balance></app-leave-balance>
    </div>
  `,
  styles: [`
    .page-header {
      margin-bottom: 2rem;
    }
    
    .page-header h1 {
      font-size: 2rem;
      font-weight: 700;
      color: var(--bs-gray-800);
      margin-bottom: 0.5rem;
    }

    .btn.active {
      background-color: var(--bs-primary);
      color: white;
      border-color: var(--bs-primary);
    }

    .card {
      border-radius: 12px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      background: linear-gradient(135deg, var(--bs-light) 0%, #f8f9fa 100%);
      border-bottom: 1px solid var(--bs-gray-200);
    }

    .table th {
      font-weight: 600;
      color: var(--bs-gray-700);
      border-bottom: 2px solid var(--bs-gray-200);
    }

    .table td {
      vertical-align: middle;
    }

    .badge {
      font-size: 0.75em;
      padding: 0.35em 0.65em;
      border-radius: 6px;
    }

    .btn {
      border-radius: 6px;
      font-weight: 500;
    }

    .btn-group-sm > .btn {
      padding: 0.25rem 0.5rem;
    }

    .form-control, .form-select {
      border-radius: 6px;
      border: 1px solid var(--bs-gray-300);
    }

    .form-control:focus, .form-select:focus {
      border-color: var(--bs-primary);
      box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.25);
    }

    .modal-content {
      border-radius: 12px;
      border: none;
    }

    @media (max-width: 768px) {
      .page-header .d-flex {
        flex-direction: column;
        gap: 1rem;
      }
      
      .btn-group {
        flex-wrap: wrap;
      }
    }
  `]
})
export class LeaveListComponent implements OnInit {
  activeTab = 'requests';
  showRequestForm = false;
  editingRequest?: LeaveRequest;
  
  filterForm: FormGroup;
  leaveRequests: LeaveRequest[] = [];
  filteredRequests: LeaveRequest[] = [];
  
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

  constructor(
    private fb: FormBuilder,
    private leaveService: LeaveService,
    private router: Router
  ) {
    this.filterForm = this.createFilterForm();
  }

  ngOnInit(): void {
    this.loadLeaveRequests();
    this.setupFilterSubscriptions();
  }

  private createFilterForm(): FormGroup {
    return this.fb.group({
      status: [''],
      leaveType: [''],
      startDate: [''],
      endDate: ['']
    });
  }

  private setupFilterSubscriptions(): void {
    this.filterForm.valueChanges.subscribe(() => {
      this.applyFilters();
    });
  }

  private loadLeaveRequests(): void {
    this.leaveService.getMyLeaveRequests().subscribe({
      next: (requests) => {
        this.leaveRequests = requests;
        this.applyFilters();
      },
      error: (error) => {
        console.error('Error loading leave requests:', error);
      }
    });
  }

  private applyFilters(): void {
    const filters = this.filterForm.value;
    
    this.filteredRequests = this.leaveRequests.filter(request => {
      if (filters.status && request.status !== parseInt(filters.status)) {
        return false;
      }
      
      if (filters.leaveType && request.leaveType !== parseInt(filters.leaveType)) {
        return false;
      }
      
      if (filters.startDate && request.startDate < new Date(filters.startDate)) {
        return false;
      }
      
      if (filters.endDate && request.endDate > new Date(filters.endDate)) {
        return false;
      }
      
      return true;
    });
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }

  showNewRequestForm(): void {
    this.editingRequest = undefined;
    this.showRequestForm = true;
    
    // Show modal
    setTimeout(() => {
      const modal = document.getElementById('newRequestModal');
      if (modal) {
        const bootstrapModal = new (window as any).bootstrap.Modal(modal);
        bootstrapModal.show();
      }
    }, 100);
  }

  hideRequestForm(): void {
    this.showRequestForm = false;
    this.editingRequest = undefined;
    
    // Hide modal
    const modal = document.getElementById('newRequestModal');
    if (modal) {
      const bootstrapModal = (window as any).bootstrap.Modal.getInstance(modal);
      if (bootstrapModal) {
        bootstrapModal.hide();
      }
    }
  }

  onRequestSubmit(request: CreateLeaveRequest): void {
    if (this.editingRequest) {
      // Update existing request
      this.leaveService.updateLeaveRequest(this.editingRequest.id, request).subscribe({
        next: () => {
          this.hideRequestForm();
          this.loadLeaveRequests();
        },
        error: (error) => {
          console.error('Error updating leave request:', error);
        }
      });
    } else {
      // Create new request
      this.leaveService.createLeaveRequest(request).subscribe({
        next: () => {
          this.hideRequestForm();
          this.loadLeaveRequests();
        },
        error: (error) => {
          console.error('Error creating leave request:', error);
        }
      });
    }
  }

  viewRequest(request: LeaveRequest): void {
    // Navigate to request details or show modal
    console.log('View request:', request);
  }

  editRequest(request: LeaveRequest): void {
    this.editingRequest = request;
    this.showRequestForm = true;
    
    // Show modal
    setTimeout(() => {
      const modal = document.getElementById('newRequestModal');
      if (modal) {
        const bootstrapModal = new (window as any).bootstrap.Modal(modal);
        bootstrapModal.show();
      }
    }, 100);
  }

  cancelRequest(request: LeaveRequest): void {
    if (confirm('Are you sure you want to cancel this leave request?')) {
      this.leaveService.cancelLeaveRequest(request.id).subscribe({
        next: () => {
          this.loadLeaveRequests();
        },
        error: (error) => {
          console.error('Error cancelling leave request:', error);
        }
      });
    }
  }

  canEdit(request: LeaveRequest): boolean {
    return request.status === LeaveStatus.Pending;
  }

  canCancel(request: LeaveRequest): boolean {
    return request.status === LeaveStatus.Pending || request.status === LeaveStatus.Approved;
  }

  getLeaveTypeColor(leaveType: LeaveType): string {
    return this.leaveService.getLeaveTypeColor(leaveType);
  }

  getLeaveStatusColor(status: LeaveStatus): string {
    return this.leaveService.getLeaveStatusColor(status);
  }

  getLeaveStatusText(status: LeaveStatus): string {
    return this.leaveService.getLeaveStatusText(status);
  }
}