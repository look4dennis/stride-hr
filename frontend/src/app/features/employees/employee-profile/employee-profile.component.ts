import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EnhancedEmployeeService } from '../../../services/enhanced-employee.service';
import { Employee, UpdateEmployeeDto, EmployeeStatus } from '../../../models/employee.models';
import { NotificationService } from '../../../core/services/notification.service';
import { EmployeeRolesComponent } from '../employee-roles/employee-roles.component';

@Component({
  selector: 'app-employee-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, EmployeeRolesComponent],
  template: `
    <div class="page-header d-flex justify-content-between align-items-center">
      <div>
        <nav aria-label="breadcrumb">
          <ol class="breadcrumb">
            <li class="breadcrumb-item">
              <a (click)="navigateToEmployeeList()" class="text-decoration-none cursor-pointer">
                <i class="fas fa-users me-1"></i>Employees
              </a>
            </li>
            <li class="breadcrumb-item active" aria-current="page">
              {{ employee?.firstName }} {{ employee?.lastName }}
            </li>
          </ol>
        </nav>
        <h1>Employee Profile</h1>
        <p class="text-muted" *ngIf="employee">{{ employee.employeeId }} - {{ employee.designation }}</p>
      </div>
      <div class="d-flex gap-2">
        <button class="btn btn-outline-primary" (click)="toggleEditMode()" [disabled]="loading">
          <i class="fas" [class.fa-edit]="!isEditMode" [class.fa-times]="isEditMode" class="me-2"></i>
          {{ isEditMode ? 'Cancel' : 'Edit' }}
        </button>
        <button class="btn btn-outline-secondary" (click)="navigateToEmployeeList()">
          <i class="fas fa-arrow-left me-2"></i>Back to List
        </button>
      </div>
    </div>

    <div class="row" *ngIf="employee">
      <!-- Profile Information -->
      <div class="col-lg-4">
        <div class="card mb-4">
          <div class="card-body text-center">
            <div class="profile-photo-section mb-3">
              <img [src]="getProfilePhoto()" 
                   [alt]="employee.firstName + ' ' + employee.lastName"
                   class="profile-photo">
              <div class="profile-status-badge" [class]="'status-' + employee.status.toLowerCase()">
                {{ employee.status }}
              </div>
            </div>
            <h4 class="mb-1">{{ employee.firstName }} {{ employee.lastName }}</h4>
            <p class="text-muted mb-2">{{ employee.designation }}</p>
            <p class="text-muted mb-3">{{ employee.department }}</p>
            
            <div class="contact-info">
              <div class="contact-item">
                <i class="fas fa-envelope text-muted me-2"></i>
                <span>{{ employee.email }}</span>
              </div>
              <div class="contact-item">
                <i class="fas fa-phone text-muted me-2"></i>
                <span>{{ employee.phone }}</span>
              </div>
              <div class="contact-item">
                <i class="fas fa-id-badge text-muted me-2"></i>
                <span>{{ employee.employeeId }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="card mt-4">
          <div class="card-header">
            <h6 class="card-title mb-0">Quick Actions</h6>
          </div>
          <div class="card-body">
            <div class="d-grid gap-2">
              <button class="btn btn-outline-primary btn-sm" (click)="viewOnboarding()">
                <i class="fas fa-user-plus me-2"></i>View Onboarding
              </button>
              <button class="btn btn-outline-info btn-sm" (click)="viewAttendance()">
                <i class="fas fa-clock me-2"></i>View Attendance
              </button>
              <button class="btn btn-outline-success btn-sm" (click)="viewPayroll()">
                <i class="fas fa-money-bill me-2"></i>View Payroll
              </button>
              <button class="btn btn-outline-warning btn-sm" (click)="initiateExit()">
                <i class="fas fa-sign-out-alt me-2"></i>Exit Process
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Profile Details -->
      <div class="col-lg-8">
        <div class="card">
          <div class="card-header">
            <ul class="nav nav-tabs card-header-tabs" role="tablist">
              <li class="nav-item">
                <a class="nav-link active" data-bs-toggle="tab" href="#personal-info" role="tab">
                  Personal Information
                </a>
              </li>
              <li class="nav-item">
                <a class="nav-link" data-bs-toggle="tab" href="#employment-info" role="tab">
                  Employment Details
                </a>
              </li>
              <li class="nav-item">
                <a class="nav-link" data-bs-toggle="tab" href="#roles" role="tab">
                  Roles & Permissions
                </a>
              </li>
              <li class="nav-item">
                <a class="nav-link" data-bs-toggle="tab" href="#documents" role="tab">
                  Documents
                </a>
              </li>
            </ul>
          </div>
          
          <div class="card-body">
            <div class="tab-content">
              <!-- Personal Information Tab -->
              <div class="tab-pane fade show active" id="personal-info" role="tabpanel">
                <form [formGroup]="profileForm" *ngIf="profileForm">
                  <div class="row g-3">
                    <div class="col-md-6">
                      <label class="form-label">First Name</label>
                      <input type="text" class="form-control" formControlName="firstName" 
                             [readonly]="!editMode">
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Last Name</label>
                      <input type="text" class="form-control" formControlName="lastName" 
                             [readonly]="!editMode">
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Email</label>
                      <input type="email" class="form-control" formControlName="email" 
                             [readonly]="!editMode">
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Phone</label>
                      <input type="tel" class="form-control" formControlName="phone" 
                             [readonly]="!editMode">
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Date of Birth</label>
                      <input type="date" class="form-control" formControlName="dateOfBirth" 
                             [readonly]="!editMode">
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Status</label>
                      <select class="form-select" formControlName="status" [disabled]="!editMode">
                        <option value="Active">Active</option>
                        <option value="Inactive">Inactive</option>
                        <option value="OnLeave">On Leave</option>
                        <option value="Terminated">Terminated</option>
                        <option value="Resigned">Resigned</option>
                      </select>
                    </div>
                  </div>
                </form>
              </div>

              <!-- Employment Details Tab -->
              <div class="tab-pane fade" id="employment-info" role="tabpanel">
                <form [formGroup]="profileForm" *ngIf="profileForm">
                  <div class="row g-3">
                    <div class="col-md-6">
                      <label class="form-label">Employee ID</label>
                      <input type="text" class="form-control" [value]="employee.employeeId" readonly>
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Joining Date</label>
                      <input type="date" class="form-control" [value]="formatDateForInput(employee.joiningDate)" readonly>
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Designation</label>
                      <input type="text" class="form-control" formControlName="designation" 
                             [readonly]="!editMode">
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Department</label>
                      <input type="text" class="form-control" formControlName="department" 
                             [readonly]="!editMode">
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Basic Salary</label>
                      <input type="number" class="form-control" formControlName="basicSalary" 
                             [readonly]="!editMode">
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Reporting Manager</label>
                      <select class="form-select" formControlName="reportingManagerId" [disabled]="!editMode">
                        <option value="">No Manager</option>
                        <option *ngFor="let manager of managers" [value]="manager.id">
                          {{ manager.firstName }} {{ manager.lastName }}
                        </option>
                      </select>
                    </div>
                  </div>
                </form>
              </div>

              <!-- Roles Tab -->
              <div class="tab-pane fade" id="roles" role="tabpanel">
                <app-employee-roles [employee]="employee" *ngIf="employee"></app-employee-roles>
              </div>

              <!-- Documents Tab -->
              <div class="tab-pane fade" id="documents" role="tabpanel">
                <div class="row g-3">
                  <div class="col-12">
                    <div class="alert alert-info">
                      <i class="fas fa-info-circle me-2"></i>
                      Document management will be implemented in the document management module.
                    </div>
                  </div>
                </div>
              </div>
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
      <p class="mt-2 text-muted">Loading employee profile...</p>
    </div>
  `,
    styles: [`
    .profile-photo-container {
      position: relative;
      display: inline-block;
    }

    .profile-photo {
      width: 150px;
      height: 150px;
      border-radius: 50%;
      object-fit: cover;
      border: 4px solid #f8f9fa;
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    }

    .photo-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.7);
      border-radius: 50%;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      color: white;
      cursor: pointer;
      opacity: 0;
      transition: opacity 0.3s ease;
    }

    .photo-overlay:hover {
      opacity: 1;
    }

    .photo-overlay i {
      font-size: 1.5rem;
      margin-bottom: 0.5rem;
    }

    .photo-overlay span {
      font-size: 0.875rem;
      font-weight: 500;
    }

    .contact-info {
      text-align: left;
    }

    .contact-item {
      display: flex;
      align-items: center;
      margin-bottom: 0.75rem;
      padding: 0.5rem;
      background: #f8f9fa;
      border-radius: 8px;
    }

    .contact-item:last-child {
      margin-bottom: 0;
    }

    .status-badge {
      margin-bottom: 1rem;
    }

    .nav-tabs .nav-link {
      color: #6c757d;
      border: none;
      border-bottom: 2px solid transparent;
    }

    .nav-tabs .nav-link.active {
      color: #495057;
      background-color: transparent;
      border-color: #007bff;
    }

    .form-control[readonly] {
      background-color: #f8f9fa;
      border-color: #e9ecef;
    }

    .breadcrumb {
      background: none;
      padding: 0;
      margin-bottom: 0.5rem;
    }

    .breadcrumb-item + .breadcrumb-item::before {
      content: ">";
      color: #6c757d;
    }
  `]
})
export class EmployeeProfileComponent implements OnInit {
  employee: Employee | null = null;
  profileForm: FormGroup | null = null;
  managers: Employee[] = [];
  editMode = false;
  loading = false;
  selectedPhoto: File | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private employeeService: EnhancedEmployeeService,
    private fb: FormBuilder,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    const employeeId = this.route.snapshot.params['id'];
    if (employeeId) {
      this.loadEmployee(parseInt(employeeId));
      this.loadManagers();
    }
  }

  loadEmployee(id: number): void {
    this.loading = true;
    
    this.employeeService.getEmployeeById(id).subscribe({
      next: (employee) => {
        this.employee = employee;
        this.initializeForm();
        this.loading = false;
      },
      error: (error) => {
        this.notificationService.showError('Failed to load employee profile');
        this.loading = false;
      }
    });
  }

  loadManagers(): void {
    this.employeeService.getManagers().subscribe({
      next: (managers) => this.managers = managers,
      error: (error) => console.error('Failed to load managers', error)
    });
  }

  initializeForm(): void {
    if (!this.employee) return;

    this.profileForm = this.fb.group({
      firstName: [this.employee.firstName, [Validators.required]],
      lastName: [this.employee.lastName, [Validators.required]],
      email: [this.employee.email, [Validators.required, Validators.email]],
      phone: [this.employee.phone, [Validators.required]],
      dateOfBirth: [this.formatDateForInput(this.employee.dateOfBirth), [Validators.required]],
      designation: [this.employee.designation, [Validators.required]],
      department: [this.employee.department, [Validators.required]],
      basicSalary: [this.employee.basicSalary, [Validators.required, Validators.min(0)]],
      status: [this.employee.status, [Validators.required]],
      reportingManagerId: [this.employee.reportingManagerId]
    });
  }

  toggleEditMode(): void {
    this.editMode = !this.editMode;
  }

  get isEditMode(): boolean {
    return this.editMode;
  }

  navigateToEmployeeList(): void {
    this.router.navigate(['/employees']);
  }

  cancelEdit(): void {
    this.editMode = false;
    this.selectedPhoto = null;
    this.initializeForm(); // Reset form to original values
  }

  saveChanges(): void {
    if (!this.profileForm?.valid || !this.employee) return;

    const formValue = this.profileForm.value;
    const updateDto: UpdateEmployeeDto = {
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      email: formValue.email,
      phone: formValue.phone,
      dateOfBirth: formValue.dateOfBirth,
      designation: formValue.designation,
      department: formValue.department,
      basicSalary: formValue.basicSalary,
      status: formValue.status,
      reportingManagerId: formValue.reportingManagerId || undefined,
      profilePhoto: this.selectedPhoto || undefined
    };

    this.employeeService.updateEmployee(this.employee.id, updateDto).subscribe({
      next: (updatedEmployee) => {
        this.employee = updatedEmployee;
        this.editMode = false;
        this.selectedPhoto = null;
        this.notificationService.showSuccess('Employee profile updated successfully');
      },
      error: (error) => {
        this.notificationService.showError('Failed to update employee profile');
      }
    });
  }

  triggerPhotoUpload(): void {
    const photoInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    photoInput?.click();
  }

  onPhotoSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedPhoto = file;
      
      // Preview the selected photo
      const reader = new FileReader();
      reader.onload = (e) => {
        if (this.employee) {
          this.employee.profilePhoto = e.target?.result as string;
        }
      };
      reader.readAsDataURL(file);
    }
  }

  getProfilePhoto(): string {
    return this.employee?.profilePhoto || '/assets/images/avatars/default-avatar.png';
  }

  getStatusColor(status: EmployeeStatus): string {
    switch (status) {
      case EmployeeStatus.Active: return 'success';
      case EmployeeStatus.Inactive: return 'secondary';
      case EmployeeStatus.OnLeave: return 'warning';
      case EmployeeStatus.Terminated: return 'danger';
      case EmployeeStatus.Resigned: return 'info';
      default: return 'secondary';
    }
  }

  formatDateForInput(dateString: string): string {
    return new Date(dateString).toISOString().split('T')[0];
  }

  // Navigation methods
  viewOnboarding(): void {
    this.router.navigate(['/employees', this.employee?.id, 'onboarding']);
  }

  viewAttendance(): void {
    this.router.navigate(['/attendance'], { queryParams: { employeeId: this.employee?.id } });
  }

  viewPayroll(): void {
    this.router.navigate(['/payroll'], { queryParams: { employeeId: this.employee?.id } });
  }

  initiateExit(): void {
    this.router.navigate(['/employees', this.employee?.id, 'exit']);
  }
}