import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { EnhancedEmployeeService } from '../../../services/enhanced-employee.service';
import { BranchService } from '../../../services/branch.service';
import { CreateEmployeeDto, Employee } from '../../../models/employee.models';
import { Branch } from '../../../models/admin.models';
import { NotificationService } from '../../../core/services/notification.service';
import { LoadingService } from '../../../core/services/loading.service';
import { FormValidationService } from '../../../shared/services/form-validation.service';
import { FormValidationDirective } from '../../../shared/directives/form-validation.directive';

@Component({
    selector: 'app-employee-create',
    imports: [CommonModule, FormsModule, ReactiveFormsModule, FormValidationDirective],
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
            <li class="breadcrumb-item active" aria-current="page">Add Employee</li>
          </ol>
        </nav>
        <h1>Add New Employee</h1>
        <p class="text-muted">Create a new employee profile</p>
      </div>
      <button class="btn btn-outline-secondary" (click)="navigateToEmployeeList()">
        <i class="fas fa-arrow-left me-2"></i>Back to List
      </button>
    </div>

    <div class="row">
      <div class="col-lg-8 mx-auto">
        <div class="card">
          <div class="card-header">
            <h5 class="card-title mb-0">
              <i class="fas fa-user-plus me-2"></i>Employee Information
            </h5>
          </div>
          <div class="card-body">
            <form [formGroup]="employeeForm" (ngSubmit)="onSubmit()" [appFormValidation]="employeeForm">
              <!-- Profile Photo Section -->
              <div class="row mb-4">
                <div class="col-12 text-center">
                  <div class="profile-photo-section">
                    <div class="profile-photo-preview">
                      <img [src]="profilePhotoPreview" 
                           alt="Profile Preview" 
                           class="profile-photo-img">
                      <div class="profile-photo-overlay" (click)="fileInput.click()">
                        <i class="fas fa-camera"></i>
                        <span>Upload Photo</span>
                      </div>
                    </div>
                    <input #fileInput 
                           type="file" 
                           accept="image/*" 
                           (change)="onFileSelected($event)"
                           class="d-none">
                    <p class="text-muted mt-2 mb-0">
                      <small>Click to upload profile photo (optional)</small>
                    </p>
                  </div>
                </div>
              </div>

              <!-- Personal Information -->
              <div class="section-header">
                <h6><i class="fas fa-user me-2"></i>Personal Information</h6>
              </div>
              
              <div class="row g-3 mb-4">
                <div class="col-md-6">
                  <label class="form-label">First Name <span class="text-danger">*</span></label>
                  <input type="text" 
                         class="form-control" 
                         formControlName="firstName"
                         [class.is-invalid]="isFieldInvalid('firstName')"
                         placeholder="Enter first name">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid('firstName')">
                    First name is required
                  </div>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Last Name <span class="text-danger">*</span></label>
                  <input type="text" 
                         class="form-control" 
                         formControlName="lastName"
                         [class.is-invalid]="isFieldInvalid('lastName')"
                         placeholder="Enter last name">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid('lastName')">
                    Last name is required
                  </div>
                </div>
              </div>

              <div class="row g-3 mb-4">
                <div class="col-md-6">
                  <label class="form-label">Email <span class="text-danger">*</span></label>
                  <input type="email" 
                         class="form-control" 
                         formControlName="email"
                         [class.is-invalid]="isFieldInvalid('email')"
                         placeholder="Enter email address">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid('email')">
                    <span *ngIf="employeeForm.get('email')?.errors?.['required']">Email is required</span>
                    <span *ngIf="employeeForm.get('email')?.errors?.['email']">Please enter a valid email</span>
                  </div>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Phone <span class="text-danger">*</span></label>
                  <input type="tel" 
                         class="form-control" 
                         formControlName="phone"
                         [class.is-invalid]="isFieldInvalid('phone')"
                         placeholder="Enter phone number">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid('phone')">
                    Phone number is required
                  </div>
                </div>
              </div>

              <div class="row g-3 mb-4">
                <div class="col-md-6">
                  <label class="form-label">Date of Birth <span class="text-danger">*</span></label>
                  <input type="date" 
                         class="form-control" 
                         formControlName="dateOfBirth"
                         [class.is-invalid]="isFieldInvalid('dateOfBirth')">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid('dateOfBirth')">
                    Date of birth is required
                  </div>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Joining Date <span class="text-danger">*</span></label>
                  <input type="date" 
                         class="form-control" 
                         formControlName="joiningDate"
                         [class.is-invalid]="isFieldInvalid('joiningDate')">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid('joiningDate')">
                    Joining date is required
                  </div>
                </div>
              </div>

              <!-- Job Information -->
              <div class="section-header">
                <h6><i class="fas fa-briefcase me-2"></i>Job Information</h6>
              </div>

              <div class="row g-3 mb-4">
                <div class="col-md-6">
                  <label class="form-label">Branch <span class="text-danger">*</span></label>
                  <select class="form-select" 
                          formControlName="branchId"
                          [class.is-invalid]="isFieldInvalid('branchId')">
                    <option value="">Select Branch</option>
                    <option *ngFor="let branch of branches" [value]="branch.id">
                      {{ branch.name }} ({{ branch.country }})
                    </option>
                  </select>
                  <div class="invalid-feedback" *ngIf="isFieldInvalid('branchId')">
                    Please select a branch
                  </div>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Department <span class="text-danger">*</span></label>
                  <select class="form-select" 
                          formControlName="department"
                          [class.is-invalid]="isFieldInvalid('department')">
                    <option value="">Select Department</option>
                    <option *ngFor="let dept of departments" [value]="dept">
                      {{ dept }}
                    </option>
                  </select>
                  <div class="invalid-feedback" *ngIf="isFieldInvalid('department')">
                    Please select a department
                  </div>
                </div>
              </div>

              <div class="row g-3 mb-4">
                <div class="col-md-6">
                  <label class="form-label">Designation <span class="text-danger">*</span></label>
                  <select class="form-select" 
                          formControlName="designation"
                          [class.is-invalid]="isFieldInvalid('designation')">
                    <option value="">Select Designation</option>
                    <option *ngFor="let desig of designations" [value]="desig">
                      {{ desig }}
                    </option>
                  </select>
                  <div class="invalid-feedback" *ngIf="isFieldInvalid('designation')">
                    Please select a designation
                  </div>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Basic Salary <span class="text-danger">*</span></label>
                  <div class="input-group">
                    <span class="input-group-text">$</span>
                    <input type="number" 
                           class="form-control" 
                           formControlName="basicSalary"
                           [class.is-invalid]="isFieldInvalid('basicSalary')"
                           placeholder="Enter basic salary"
                           min="0"
                           step="0.01">
                  </div>
                  <div class="invalid-feedback" *ngIf="isFieldInvalid('basicSalary')">
                    <span *ngIf="employeeForm.get('basicSalary')?.errors?.['required']">Basic salary is required</span>
                    <span *ngIf="employeeForm.get('basicSalary')?.errors?.['min']">Salary must be greater than 0</span>
                  </div>
                </div>
              </div>

              <div class="row g-3 mb-4">
                <div class="col-md-6">
                  <label class="form-label">Reporting Manager</label>
                  <select class="form-select" formControlName="reportingManagerId">
                    <option value="">Select Reporting Manager (Optional)</option>
                    <option *ngFor="let manager of managers" [value]="manager.id">
                      {{ manager.firstName }} {{ manager.lastName }} - {{ manager.designation }}
                    </option>
                  </select>
                </div>
              </div>

              <!-- Form Actions -->
              <div class="d-flex justify-content-end gap-3 mt-4">
                <button type="button" 
                        class="btn btn-outline-secondary" 
                        (click)="navigateToEmployeeList()"
                        [disabled]="isSubmitting">
                  <i class="fas fa-times me-2"></i>Cancel
                </button>
                <button type="submit" 
                        class="btn btn-primary btn-rounded"
                        [disabled]="!employeeForm.valid || isSubmitting">
                  <span *ngIf="isSubmitting" class="spinner-border spinner-border-sm me-2" role="status"></span>
                  <i *ngIf="!isSubmitting" class="fas fa-save me-2"></i>
                  {{ isSubmitting ? 'Creating...' : 'Create Employee' }}
                </button>
              </div>
            </form>
          </div>
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

    .breadcrumb {
      background: none;
      padding: 0;
      margin-bottom: 0.5rem;
    }

    .breadcrumb-item a {
      color: #6c757d;
    }

    .breadcrumb-item a:hover {
      color: #495057;
    }

    .cursor-pointer {
      cursor: pointer;
    }

    .section-header {
      border-bottom: 2px solid #e9ecef;
      margin-bottom: 1.5rem;
      padding-bottom: 0.5rem;
    }

    .section-header h6 {
      color: #495057;
      font-weight: 600;
      margin-bottom: 0;
    }

    .profile-photo-section {
      margin-bottom: 2rem;
    }

    .profile-photo-preview {
      position: relative;
      display: inline-block;
      width: 120px;
      height: 120px;
      border-radius: 50%;
      overflow: hidden;
      border: 3px solid #e9ecef;
      cursor: pointer;
      transition: all 0.3s ease;
    }

    .profile-photo-preview:hover {
      border-color: #007bff;
      transform: scale(1.05);
    }

    .profile-photo-img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .profile-photo-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.7);
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      color: white;
      opacity: 0;
      transition: opacity 0.3s ease;
    }

    .profile-photo-preview:hover .profile-photo-overlay {
      opacity: 1;
    }

    .profile-photo-overlay i {
      font-size: 1.5rem;
      margin-bottom: 0.25rem;
    }

    .profile-photo-overlay span {
      font-size: 0.875rem;
      font-weight: 500;
    }

    .form-label {
      font-weight: 500;
      color: #374151;
      margin-bottom: 0.5rem;
    }

    .form-control, .form-select {
      border: 2px solid #e5e7eb;
      border-radius: 8px;
      padding: 0.75rem 1rem;
      font-size: 14px;
      transition: all 0.15s ease-in-out;
    }

    .form-control:focus, .form-select:focus {
      border-color: #3b82f6;
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
      outline: none;
    }

    .input-group-text {
      background-color: #f8f9fa;
      border: 2px solid #e5e7eb;
      border-right: none;
      color: #6c757d;
      font-weight: 500;
    }

    .input-group .form-control {
      border-left: none;
    }

    .input-group .form-control:focus {
      border-left: none;
    }

    .btn-rounded {
      border-radius: 50px;
    }

    .btn-primary {
      background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
      border: none;
      color: white;
      box-shadow: 0 2px 4px rgba(59, 130, 246, 0.3);
    }

    .btn-primary:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(59, 130, 246, 0.4);
    }

    .btn-primary:disabled {
      opacity: 0.6;
      transform: none;
      box-shadow: none;
    }

    .btn-outline-secondary {
      border: 2px solid #6c757d;
      color: #6c757d;
    }

    .btn-outline-secondary:hover:not(:disabled) {
      background: #6c757d;
      color: white;
      transform: translateY(-1px);
    }

    .text-danger {
      color: #dc3545 !important;
    }

    .invalid-feedback {
      display: block;
      width: 100%;
      margin-top: 0.25rem;
      font-size: 0.875rem;
      color: #dc3545;
    }

    .is-invalid {
      border-color: #dc3545;
    }

    .is-invalid:focus {
      border-color: #dc3545;
      box-shadow: 0 0 0 3px rgba(220, 53, 69, 0.1);
    }

    .spinner-border-sm {
      width: 1rem;
      height: 1rem;
    }

    /* Mobile responsiveness */
    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
      }
      
      .page-header .btn {
        width: 100%;
      }
      
      .profile-photo-preview {
        width: 100px;
        height: 100px;
      }
      
      .d-flex.justify-content-end {
        flex-direction: column;
        gap: 0.5rem;
      }
      
      .d-flex.justify-content-end .btn {
        width: 100%;
      }
    }

    /* Touch-friendly improvements */
    .profile-photo-preview {
      -webkit-tap-highlight-color: rgba(0, 0, 0, 0.05);
      touch-action: manipulation;
    }

    .profile-photo-preview:active {
      transform: scale(0.98);
    }

    @media (max-width: 576px) {
      .profile-photo-overlay {
        opacity: 1;
        background: rgba(0, 0, 0, 0.5);
      }
    }
  `]
})
export class EmployeeCreateComponent implements OnInit {
    employeeForm: FormGroup;
    branches: Branch[] = [];
    departments: string[] = [];
    designations: string[] = [];
    managers: Employee[] = [];
    isSubmitting = false;
    profilePhotoPreview = '/assets/images/avatars/default-avatar.png';
    selectedFile: File | null = null;

    constructor(
        private fb: FormBuilder,
        private employeeService: EnhancedEmployeeService,
        private branchService: BranchService,
        private router: Router,
        private notificationService: NotificationService,
        private loadingService: LoadingService,
        private formValidationService: FormValidationService
    ) {
        this.employeeForm = this.createForm();
    }

    ngOnInit(): void {
        this.loadFormData();
    }

    private createForm(): FormGroup {
        return this.fb.group({
            branchId: ['', Validators.required],
            firstName: ['', [Validators.required, Validators.minLength(2)]],
            lastName: ['', [Validators.required, Validators.minLength(2)]],
            email: ['', [Validators.required, Validators.email]],
            phone: ['', Validators.required],
            dateOfBirth: ['', Validators.required],
            joiningDate: ['', Validators.required],
            designation: ['', Validators.required],
            department: ['', Validators.required],
            basicSalary: ['', [Validators.required, Validators.min(0)]],
            reportingManagerId: ['']
        });
    }

    private loadFormData(): void {
        // Load branches
        this.branchService.getAllBranches().subscribe({
            next: (response) => {
                if (response.success && response.data) {
                    this.branches = response.data;
                }
            },
            error: () => {
                // Use mock data for development
                this.branches = [
                    {
                        id: 1,
                        organizationId: 1,
                        name: 'Main Office',
                        country: 'United States',
                        currency: 'USD',
                        timeZone: 'America/New_York',
                        address: '123 Business St, New York, NY 10001',
                        localHolidays: [],
                        complianceSettings: {},
                        createdAt: new Date(),
                        updatedAt: new Date()
                    }
                ];
            }
        });

        // Load departments and designations
        this.employeeService.getDepartments().subscribe({
            next: (departments) => this.departments = departments,
            error: () => {
                // Use mock data for development
                this.departments = ['Development', 'Human Resources', 'Marketing', 'Sales', 'Finance', 'Operations'];
            }
        });

        this.employeeService.getDesignations().subscribe({
            next: (designations) => this.designations = designations,
            error: () => {
                // Use mock data for development
                this.designations = [
                    'Senior Developer', 'Junior Developer', 'Development Manager',
                    'HR Manager', 'HR Executive', 'Marketing Manager', 'Marketing Executive',
                    'Sales Manager', 'Sales Executive', 'Finance Manager', 'Accountant'
                ];
            }
        });

        // Load managers
        this.employeeService.getManagers().subscribe({
            next: (managers) => this.managers = managers,
            error: () => {
                // Use mock data for development
                this.managers = [
                    {
                        id: 2,
                        employeeId: 'EMP002',
                        branchId: 1,
                        firstName: 'Jane',
                        lastName: 'Smith',
                        email: 'jane.smith@company.com',
                        phone: '+1-555-0102',
                        dateOfBirth: '1985-08-22',
                        joiningDate: '2018-03-10',
                        designation: 'Development Manager',
                        department: 'Development',
                        basicSalary: 95000,
                        status: 'Active' as any,
                        createdAt: '2018-03-10T00:00:00Z'
                    }
                ];
            }
        });
    }

    onFileSelected(event: any): void {
        const file = event.target.files[0];
        if (file) {
            // Validate file type
            if (!file.type.startsWith('image/')) {
                this.notificationService.showError('Please select a valid image file');
                return;
            }

            // Validate file size (max 5MB)
            if (file.size > 5 * 1024 * 1024) {
                this.notificationService.showError('File size must be less than 5MB');
                return;
            }

            this.selectedFile = file;

            // Create preview
            const reader = new FileReader();
            reader.onload = (e: any) => {
                this.profilePhotoPreview = e.target.result;
            };
            reader.readAsDataURL(file);
        }
    }

    isFieldInvalid(fieldName: string): boolean {
        return this.formValidationService.isFieldInvalid(this.employeeForm, fieldName);
    }

    getValidationMessage(fieldName: string): string | null {
        const control = this.employeeForm.get(fieldName);
        return control ? this.formValidationService.getValidationMessage(control) : null;
    }

    onSubmit(): void {
        if (this.employeeForm.valid) {
            this.isSubmitting = true;

            const formValue = this.employeeForm.value;
            const createDto: CreateEmployeeDto = {
                branchId: parseInt(formValue.branchId),
                firstName: formValue.firstName.trim(),
                lastName: formValue.lastName.trim(),
                email: formValue.email.trim().toLowerCase(),
                phone: formValue.phone.trim(),
                dateOfBirth: formValue.dateOfBirth,
                joiningDate: formValue.joiningDate,
                designation: formValue.designation,
                department: formValue.department,
                basicSalary: parseFloat(formValue.basicSalary),
                reportingManagerId: formValue.reportingManagerId ? parseInt(formValue.reportingManagerId) : undefined,
                profilePhoto: this.selectedFile || undefined
            };

            this.employeeService.create(createDto).subscribe({
                next: (response) => {
                    const employee = response.data!;
                    this.router.navigate(['/employees']);
                },
                error: (error) => {
                    console.error('Error creating employee:', error);
                    this.isSubmitting = false;
                }
            });
        } else {
            // Use the validation service to mark fields and get error message
            const firstError = this.formValidationService.validateFormAndGetFirstError(this.employeeForm);
            this.notificationService.showError(firstError || 'Please fill in all required fields correctly');
        }
    }

    navigateToEmployeeList(): void {
        this.router.navigate(['/employees']);
    }
}