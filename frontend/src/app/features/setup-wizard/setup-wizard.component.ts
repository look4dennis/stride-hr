import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { 
  SetupWizardService, 
  SetupWizardStep, 
  OrganizationSetupData, 
  AdminUserSetupData, 
  BranchSetupData, 
  SystemPreferencesData 
} from '../../core/services/setup-wizard.service';
import { NotificationService } from '../../core/services/notification.service';
import { LoadingService } from '../../core/services/loading.service';

@Component({
  selector: 'app-setup-wizard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="setup-wizard-container">
      <div class="setup-wizard-card">
        <!-- Header -->
        <div class="setup-header">
          <h1 class="setup-title">Welcome to StrideHR</h1>
          <p class="setup-subtitle">Let's set up your organization in a few simple steps</p>
          
          <!-- Progress Bar -->
          <div class="progress-container">
            <div class="progress">
              <div 
                class="progress-bar" 
                [style.width.%]="getProgressPercentage()">
              </div>
            </div>
            <div class="progress-text">
              Step {{ currentStepIndex + 1 }} of {{ steps.length }}
            </div>
          </div>
        </div>

        <!-- Step Content -->
        <div class="setup-content">
          <!-- Organization Setup -->
          <div *ngIf="currentStep?.id === 'organization'" class="step-content">
            <div class="step-header">
              <h2>Organization Information</h2>
              <p>Tell us about your organization</p>
            </div>
            
            <form [formGroup]="organizationForm" class="setup-form">
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label">Organization Name *</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    formControlName="name"
                    placeholder="Enter organization name">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid(organizationForm, 'name')">
                    Organization name is required
                  </div>
                </div>
                
                <div class="col-md-6 mb-3">
                  <label class="form-label">Email Address *</label>
                  <input 
                    type="email" 
                    class="form-control" 
                    formControlName="email"
                    placeholder="Enter email address">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid(organizationForm, 'email')">
                    Valid email is required
                  </div>
                </div>
              </div>
              
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label">Phone Number *</label>
                  <input 
                    type="tel" 
                    class="form-control" 
                    formControlName="phone"
                    placeholder="Enter phone number">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid(organizationForm, 'phone')">
                    Phone number is required
                  </div>
                </div>
                
                <div class="col-md-6 mb-3">
                  <label class="form-label">Website</label>
                  <input 
                    type="url" 
                    class="form-control" 
                    formControlName="website"
                    placeholder="Enter website URL">
                </div>
              </div>
              
              <div class="mb-3">
                <label class="form-label">Address *</label>
                <textarea 
                  class="form-control" 
                  formControlName="address"
                  rows="3"
                  placeholder="Enter organization address"></textarea>
                <div class="invalid-feedback" *ngIf="isFieldInvalid(organizationForm, 'address')">
                  Address is required
                </div>
              </div>
              
              <div class="row">
                <div class="col-md-4 mb-3">
                  <label class="form-label">Normal Working Hours *</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    formControlName="normalWorkingHours"
                    placeholder="e.g., 9:00 AM - 5:00 PM">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid(organizationForm, 'normalWorkingHours')">
                    Working hours is required
                  </div>
                </div>
                
                <div class="col-md-4 mb-3">
                  <label class="form-label">Overtime Rate</label>
                  <input 
                    type="number" 
                    class="form-control" 
                    formControlName="overtimeRate"
                    placeholder="1.5"
                    step="0.1"
                    min="0">
                </div>
                
                <div class="col-md-4 mb-3">
                  <label class="form-label">Productive Hours Threshold</label>
                  <input 
                    type="number" 
                    class="form-control" 
                    formControlName="productiveHoursThreshold"
                    placeholder="8"
                    min="0">
                </div>
              </div>
            </form>
          </div>

          <!-- Admin User Setup -->
          <div *ngIf="currentStep?.id === 'admin-user'" class="step-content">
            <div class="step-header">
              <h2>Administrator Account</h2>
              <p>Create the first administrator account for your organization</p>
            </div>
            
            <form [formGroup]="adminUserForm" class="setup-form">
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label">First Name *</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    formControlName="firstName"
                    placeholder="Enter first name">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid(adminUserForm, 'firstName')">
                    First name is required
                  </div>
                </div>
                
                <div class="col-md-6 mb-3">
                  <label class="form-label">Last Name *</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    formControlName="lastName"
                    placeholder="Enter last name">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid(adminUserForm, 'lastName')">
                    Last name is required
                  </div>
                </div>
              </div>
              
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label">Email Address *</label>
                  <input 
                    type="email" 
                    class="form-control" 
                    formControlName="email"
                    placeholder="Enter email address">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid(adminUserForm, 'email')">
                    Valid email is required
                  </div>
                </div>
                
                <div class="col-md-6 mb-3">
                  <label class="form-label">Phone Number *</label>
                  <input 
                    type="tel" 
                    class="form-control" 
                    formControlName="phone"
                    placeholder="Enter phone number">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid(adminUserForm, 'phone')">
                    Phone number is required
                  </div>
                </div>
              </div>
              
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label">Password *</label>
                  <input 
                    type="password" 
                    class="form-control" 
                    formControlName="password"
                    placeholder="Enter password">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid(adminUserForm, 'password')">
                    Password must be at least 8 characters
                  </div>
                </div>
                
                <div class="col-md-6 mb-3">
                  <label class="form-label">Confirm Password *</label>
                  <input 
                    type="password" 
                    class="form-control" 
                    formControlName="confirmPassword"
                    placeholder="Confirm password">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid(adminUserForm, 'confirmPassword')">
                    Passwords must match
                  </div>
                </div>
              </div>
            </form>
          </div>

          <!-- Branch Setup -->
          <div *ngIf="currentStep?.id === 'branch'" class="step-content">
            <div class="step-header">
              <h2>Branch Configuration</h2>
              <p>Set up your first branch or head office</p>
            </div>
            
            <form [formGroup]="branchForm" class="setup-form">
              <div class="mb-3">
                <div class="form-check">
                  <input 
                    class="form-check-input" 
                    type="checkbox" 
                    formControlName="isHeadOffice"
                    id="isHeadOffice">
                  <label class="form-check-label" for="isHeadOffice">
                    This is the head office
                  </label>
                </div>
              </div>
              
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label">Branch Name *</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    formControlName="name"
                    placeholder="Enter branch name">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid(branchForm, 'name')">
                    Branch name is required
                  </div>
                </div>
                
                <div class="col-md-6 mb-3">
                  <label class="form-label">Email Address *</label>
                  <input 
                    type="email" 
                    class="form-control" 
                    formControlName="email"
                    placeholder="Enter email address">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid(branchForm, 'email')">
                    Valid email is required
                  </div>
                </div>
              </div>
              
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label">Phone Number *</label>
                  <input 
                    type="tel" 
                    class="form-control" 
                    formControlName="phone"
                    placeholder="Enter phone number">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid(branchForm, 'phone')">
                    Phone number is required
                  </div>
                </div>
              </div>
              
              <div class="mb-3">
                <label class="form-label">Address *</label>
                <textarea 
                  class="form-control" 
                  formControlName="address"
                  rows="3"
                  placeholder="Enter branch address"></textarea>
                <div class="invalid-feedback" *ngIf="isFieldInvalid(branchForm, 'address')">
                  Address is required
                </div>
              </div>
            </form>
          </div>

          <!-- Role Configuration -->
          <div *ngIf="currentStep?.id === 'roles'" class="step-content">
            <div class="step-header">
              <h2>Role Configuration</h2>
              <p>Select the roles you want to create for your organization</p>
            </div>
            
            <form [formGroup]="rolesForm" class="setup-form">
              <div class="roles-selection">
                <div class="role-card" 
                     *ngFor="let role of availableRoles; let i = index"
                     [class.selected]="isRoleSelected(role.id)"
                     (click)="toggleRole(role.id)">
                  <div class="role-checkbox">
                    <input 
                      type="checkbox" 
                      [id]="'role-' + role.id"
                      [checked]="isRoleSelected(role.id)"
                      (change)="toggleRole(role.id)"
                      class="form-check-input">
                  </div>
                  <div class="role-icon" [ngClass]="role.colorClass">
                    <i [class]="role.icon"></i>
                  </div>
                  <div class="role-info">
                    <h5>{{ role.name }}</h5>
                    <p>{{ role.description }}</p>
                  </div>
                </div>
              </div>
              
              <div class="role-selection-note">
                <small class="text-muted">
                  <i class="fas fa-info-circle me-1"></i>
                  You can modify roles and permissions later in the system settings.
                </small>
              </div>
            </form>
          </div>

          <!-- System Preferences -->
          <div *ngIf="currentStep?.id === 'preferences'" class="step-content">
            <div class="step-header">
              <h2>System Preferences</h2>
              <p>Configure system-wide preferences and settings</p>
            </div>
            
            <form [formGroup]="preferencesForm" class="setup-form">
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label">Timezone</label>
                  <select class="form-control" formControlName="timezone">
                    <option value="UTC">UTC</option>
                    <option value="America/New_York">Eastern Time</option>
                    <option value="America/Chicago">Central Time</option>
                    <option value="America/Denver">Mountain Time</option>
                    <option value="America/Los_Angeles">Pacific Time</option>
                    <option value="Europe/London">London</option>
                    <option value="Asia/Dubai">Dubai</option>
                    <option value="Asia/Kolkata">India</option>
                  </select>
                </div>
                
                <div class="col-md-6 mb-3">
                  <label class="form-label">Date Format</label>
                  <select class="form-control" formControlName="dateFormat">
                    <option value="MM/DD/YYYY">MM/DD/YYYY</option>
                    <option value="DD/MM/YYYY">DD/MM/YYYY</option>
                    <option value="YYYY-MM-DD">YYYY-MM-DD</option>
                  </select>
                </div>
              </div>
              
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label">Currency</label>
                  <select class="form-control" formControlName="currency">
                    <option value="USD">USD - US Dollar</option>
                    <option value="EUR">EUR - Euro</option>
                    <option value="GBP">GBP - British Pound</option>
                    <option value="AED">AED - UAE Dirham</option>
                    <option value="INR">INR - Indian Rupee</option>
                  </select>
                </div>
                
                <div class="col-md-6 mb-3">
                  <label class="form-label">Language</label>
                  <select class="form-control" formControlName="language">
                    <option value="en">English</option>
                    <option value="ar">Arabic</option>
                    <option value="es">Spanish</option>
                    <option value="fr">French</option>
                  </select>
                </div>
              </div>
              
              <div class="preferences-toggles">
                <div class="form-check mb-3">
                  <input 
                    class="form-check-input" 
                    type="checkbox" 
                    formControlName="enableNotifications"
                    id="enableNotifications">
                  <label class="form-check-label" for="enableNotifications">
                    Enable email notifications
                  </label>
                </div>
                
                <div class="form-check mb-3">
                  <input 
                    class="form-check-input" 
                    type="checkbox" 
                    formControlName="enableRealTimeUpdates"
                    id="enableRealTimeUpdates">
                  <label class="form-check-label" for="enableRealTimeUpdates">
                    Enable real-time updates
                  </label>
                </div>
              </div>
            </form>
          </div>
        </div>

        <!-- Navigation -->
        <div class="setup-navigation">
          <button 
            type="button" 
            class="btn btn-outline-secondary"
            [disabled]="currentStepIndex === 0 || isSubmitting"
            (click)="previousStep()">
            <i class="fas fa-arrow-left me-2"></i>
            Previous
          </button>
          
          <button 
            type="button" 
            class="btn btn-primary"
            [disabled]="!canProceed() || isSubmitting"
            (click)="nextStep()">
            <span *ngIf="isSubmitting" class="spinner-border spinner-border-sm me-2"></span>
            {{ isLastStep() ? 'Complete Setup' : 'Next' }}
            <i *ngIf="!isLastStep() && !isSubmitting" class="fas fa-arrow-right ms-2"></i>
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .setup-wizard-container {
      min-height: 100vh;
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem;
    }

    .setup-wizard-card {
      background: white;
      border-radius: 20px;
      width: 100%;
      max-width: 800px;
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
      overflow: hidden;
    }

    .setup-header {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      color: white;
      padding: 2rem;
      text-align: center;
    }

    .setup-title {
      font-size: 2.5rem;
      font-weight: 700;
      margin-bottom: 0.5rem;
      color: white;
    }

    .setup-subtitle {
      font-size: 1.1rem;
      opacity: 0.9;
      margin-bottom: 2rem;
    }

    .progress-container {
      max-width: 400px;
      margin: 0 auto;
    }

    .progress {
      height: 8px;
      background: rgba(255, 255, 255, 0.2);
      border-radius: 4px;
      overflow: hidden;
      margin-bottom: 0.5rem;
    }

    .progress-bar {
      height: 100%;
      background: white;
      border-radius: 4px;
      transition: width 0.3s ease;
    }

    .progress-text {
      font-size: 0.9rem;
      opacity: 0.8;
    }

    .setup-content {
      padding: 2rem;
      min-height: 400px;
    }

    .step-header {
      text-align: center;
      margin-bottom: 2rem;
    }

    .step-header h2 {
      color: var(--text-primary);
      font-weight: 600;
      margin-bottom: 0.5rem;
    }

    .step-header p {
      color: var(--text-secondary);
      font-size: 1rem;
    }

    .setup-form {
      max-width: 600px;
      margin: 0 auto;
    }

    .form-label {
      font-weight: 500;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .form-control {
      border: 2px solid var(--gray-200);
      border-radius: 8px;
      padding: 0.75rem 1rem;
      transition: all 0.15s ease-in-out;
    }

    .form-control:focus {
      border-color: var(--primary);
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
    }

    .form-control.is-invalid {
      border-color: var(--danger);
    }

    .invalid-feedback {
      display: block;
      color: var(--danger);
      font-size: 0.875rem;
      margin-top: 0.25rem;
    }

    .roles-selection {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 1rem;
      max-width: 700px;
      margin: 0 auto;
    }

    .role-card {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 1rem;
      border: 2px solid var(--gray-200);
      border-radius: 12px;
      transition: all 0.15s ease-in-out;
      cursor: pointer;
      position: relative;
    }

    .role-card:hover {
      border-color: var(--primary);
      transform: translateY(-2px);
    }

    .role-card.selected {
      border-color: var(--primary);
      background-color: rgba(59, 130, 246, 0.05);
    }

    .role-checkbox {
      display: flex;
      align-items: center;
    }

    .role-checkbox input[type="checkbox"] {
      width: 18px;
      height: 18px;
      margin: 0;
    }

    .role-selection-note {
      text-align: center;
      margin-top: 2rem;
      max-width: 500px;
      margin-left: auto;
      margin-right: auto;
    }

    .role-icon {
      width: 50px;
      height: 50px;
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 1.25rem;
    }

    .role-info h5 {
      margin-bottom: 0.25rem;
      color: var(--text-primary);
      font-weight: 600;
    }

    .role-info p {
      margin-bottom: 0;
      color: var(--text-secondary);
      font-size: 0.875rem;
    }

    .preferences-toggles {
      max-width: 400px;
      margin: 2rem auto 0;
    }

    .form-check-label {
      color: var(--text-primary);
      font-weight: 500;
    }

    .setup-navigation {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.5rem 2rem;
      background: var(--bg-secondary);
      border-top: 1px solid var(--gray-200);
    }

    .btn {
      border-radius: 8px;
      font-weight: 500;
      padding: 0.75rem 1.5rem;
      transition: all 0.15s ease-in-out;
    }

    .btn-primary {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      border: none;
    }

    .btn-primary:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(59, 130, 246, 0.4);
    }

    .btn-outline-secondary {
      border: 2px solid var(--gray-300);
      color: var(--text-secondary);
    }

    .btn-outline-secondary:hover:not(:disabled) {
      background: var(--gray-100);
      border-color: var(--gray-400);
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    @media (max-width: 768px) {
      .setup-wizard-container {
        padding: 1rem;
      }

      .setup-wizard-card {
        max-width: 100%;
      }

      .setup-title {
        font-size: 2rem;
      }

      .setup-content {
        padding: 1.5rem;
      }

      .setup-navigation {
        padding: 1rem 1.5rem;
      }

      .roles-selection {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class SetupWizardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  steps: SetupWizardStep[] = [];
  currentStepIndex = 0;
  currentStep: SetupWizardStep | null = null;
  isSubmitting = false;

  // Forms
  organizationForm!: FormGroup;
  adminUserForm!: FormGroup;
  branchForm!: FormGroup;
  rolesForm!: FormGroup;
  preferencesForm!: FormGroup;

  // Role selection data
  availableRoles = [
    {
      id: 'superadmin',
      name: 'Super Admin',
      description: 'Full system access and control',
      icon: 'fas fa-crown',
      colorClass: 'bg-danger',
      required: true
    },
    {
      id: 'admin',
      name: 'Admin',
      description: 'Organization-wide administrative access',
      icon: 'fas fa-user-shield',
      colorClass: 'bg-primary',
      required: false
    },
    {
      id: 'hr',
      name: 'HR',
      description: 'Human resources management',
      icon: 'fas fa-users',
      colorClass: 'bg-success',
      required: false
    },
    {
      id: 'manager',
      name: 'Manager',
      description: 'Team and project management',
      icon: 'fas fa-user-tie',
      colorClass: 'bg-warning',
      required: false
    },
    {
      id: 'employee',
      name: 'Employee',
      description: 'Standard employee access',
      icon: 'fas fa-user',
      colorClass: 'bg-info',
      required: false
    }
  ];

  constructor(
    private fb: FormBuilder,
    private setupWizardService: SetupWizardService,
    private router: Router,
    private notificationService: NotificationService,
    private loadingService: LoadingService
  ) {
    this.initializeForms();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngOnInit(): void {
    // Check if setup is required
    this.shouldShowSetupWizard();
    
    this.steps = this.setupWizardService.getSetupSteps();
    this.currentStep = this.steps[0];
    
    // Load default roles from backend
    this.loadDefaultRoles();
    
    // Pre-populate forms with any existing data
    this.loadExistingSetupData();
  }

  private loadDefaultRoles(): void {
    this.setupWizardService.getDefaultRoles().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.availableRoles = response.data.map(role => ({
            id: role.name.toLowerCase(),
            name: role.name,
            description: role.description,
            icon: this.getRoleIcon(role.name),
            colorClass: this.getRoleColorClass(role.name),
            required: role.isRequired || role.name === 'SuperAdmin'
          }));
          
          // Update roles form with new roles
          this.updateRolesForm();
        }
      },
      error: (error) => {
        console.error('Failed to load default roles:', error);
        // Keep the existing hardcoded roles as fallback
      }
    });
  }

  private getRoleIcon(roleName: string): string {
    const iconMap: { [key: string]: string } = {
      'SuperAdmin': 'fas fa-crown',
      'Admin': 'fas fa-user-shield',
      'HR': 'fas fa-users',
      'Manager': 'fas fa-user-tie',
      'Employee': 'fas fa-user'
    };
    return iconMap[roleName] || 'fas fa-user';
  }

  private getRoleColorClass(roleName: string): string {
    const colorMap: { [key: string]: string } = {
      'SuperAdmin': 'bg-danger',
      'Admin': 'bg-primary',
      'HR': 'bg-success',
      'Manager': 'bg-warning',
      'Employee': 'bg-info'
    };
    return colorMap[roleName] || 'bg-secondary';
  }

  private updateRolesForm(): void {
    const rolesFormGroup: any = {};
    this.availableRoles.forEach(role => {
      rolesFormGroup[role.id] = [role.required]; // Select required roles by default
    });
    this.rolesForm = this.fb.group(rolesFormGroup);
  }

  private initializeForms(): void {
    this.organizationForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, this.phoneValidator]],
      address: ['', [Validators.required, Validators.minLength(10)]],
      website: ['', this.websiteValidator],
      taxId: [''],
      registrationNumber: [''],
      normalWorkingHours: ['9:00 AM - 5:00 PM', Validators.required],
      overtimeRate: [1.5, [Validators.min(0), Validators.max(10)]],
      productiveHoursThreshold: [8, [Validators.min(1), Validators.max(24)]]
    });

    this.adminUserForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, this.phoneValidator]],
      password: ['', [Validators.required, Validators.minLength(8), this.strongPasswordValidator]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });

    this.branchForm = this.fb.group({
      name: ['Head Office', [Validators.required, Validators.minLength(2)]],
      address: ['', [Validators.required, Validators.minLength(10)]],
      phone: ['', [Validators.required, this.phoneValidator]],
      email: ['', [Validators.required, Validators.email]],
      isHeadOffice: [true]
    });

    // Initialize roles form with required roles selected by default
    const rolesFormGroup: any = {};
    this.availableRoles.forEach(role => {
      rolesFormGroup[role.id] = [role.required]; // Select required roles by default
    });
    this.rolesForm = this.fb.group(rolesFormGroup);

    this.preferencesForm = this.fb.group({
      timezone: ['UTC'],
      dateFormat: ['MM/DD/YYYY'],
      currency: ['USD'],
      language: ['en'],
      enableNotifications: [true],
      enableRealTimeUpdates: [true]
    });
  }

  private passwordMatchValidator(group: FormGroup) {
    const password = group.get('password');
    const confirmPassword = group.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    return null;
  }

  // Custom validators
  private strongPasswordValidator(control: any) {
    const value = control.value;
    if (!value) return null;

    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumeric = /[0-9]/.test(value);
    const hasSpecialChar = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(value);
    const isValidLength = value.length >= 8;

    const passwordValid = hasUpperCase && hasLowerCase && hasNumeric && hasSpecialChar && isValidLength;

    if (!passwordValid) {
      return {
        strongPassword: {
          hasUpperCase,
          hasLowerCase,
          hasNumeric,
          hasSpecialChar,
          isValidLength
        }
      };
    }

    return null;
  }

  private phoneValidator(control: any) {
    const value = control.value;
    if (!value) return null;

    // Basic phone validation - can be enhanced based on requirements
    const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
    return phoneRegex.test(value) ? null : { invalidPhone: true };
  }

  private websiteValidator(control: any) {
    const value = control.value;
    if (!value) return null;

    const urlRegex = /^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/;
    return urlRegex.test(value) ? null : { invalidWebsite: true };
  }

  getProgressPercentage(): number {
    return ((this.currentStepIndex + 1) / this.steps.length) * 100;
  }

  canProceed(): boolean {
    switch (this.currentStep?.id) {
      case 'organization':
        return this.organizationForm.valid;
      case 'admin-user':
        return this.adminUserForm.valid;
      case 'branch':
        return this.branchForm.valid;
      case 'roles':
        return this.hasRequiredRolesSelected();
      case 'preferences':
        return this.preferencesForm.valid;
      default:
        return false;
    }
  }

  // Role selection methods
  isRoleSelected(roleId: string): boolean {
    return this.rolesForm.get(roleId)?.value || false;
  }

  toggleRole(roleId: string): void {
    const role = this.availableRoles.find(r => r.id === roleId);
    if (role && role.required) {
      // Don't allow deselecting required roles
      return;
    }
    
    const control = this.rolesForm.get(roleId);
    if (control) {
      control.setValue(!control.value);
    }
  }

  hasRequiredRolesSelected(): boolean {
    return this.availableRoles
      .filter(role => role.required)
      .every(role => this.isRoleSelected(role.id));
  }

  isLastStep(): boolean {
    return this.currentStepIndex === this.steps.length - 1;
  }

  previousStep(): void {
    if (this.currentStepIndex > 0) {
      this.currentStepIndex--;
      this.currentStep = this.steps[this.currentStepIndex];
    }
  }

  async nextStep(): Promise<void> {
    if (!this.canProceed()) {
      this.markCurrentFormAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.loadingService.setLoading(true, 'setup-wizard');

    try {
      // Save current step data
      await this.saveCurrentStepData();

      if (this.isLastStep()) {
        // Complete setup
        await this.completeSetup();
      } else {
        // Move to next step
        this.currentStepIndex++;
        this.currentStep = this.steps[this.currentStepIndex];
      }
    } catch (error: any) {
      this.notificationService.showError(
        error.message || 'An error occurred during setup',
        'Setup Error'
      );
    } finally {
      this.isSubmitting = false;
      this.loadingService.setLoading(false, 'setup-wizard');
    }
  }

  private async saveCurrentStepData(): Promise<void> {
    return new Promise((resolve, reject) => {
      let saveObservable: any;

      switch (this.currentStep?.id) {
        case 'organization':
          saveObservable = this.setupWizardService.saveOrganizationData(this.organizationForm.value);
          break;
        case 'admin-user':
          saveObservable = this.setupWizardService.saveAdminUserData(this.adminUserForm.value);
          break;
        case 'branch':
          saveObservable = this.setupWizardService.saveBranchData(this.branchForm.value);
          break;
        case 'roles':
          const selectedRoles = this.availableRoles
            .filter(role => this.isRoleSelected(role.id))
            .map(role => role.id);
          const roleData = {
            selectedRoles,
            customRoles: [] // Can be extended later for custom roles
          };
          saveObservable = this.setupWizardService.saveRoleConfiguration(roleData);
          break;
        case 'preferences':
          saveObservable = this.setupWizardService.saveSystemPreferences(this.preferencesForm.value);
          break;
        default:
          resolve();
          return;
      }

      saveObservable.subscribe({
        next: (response: any) => {
          if (response.success) {
            resolve();
          } else {
            reject(new Error(response.message || 'Failed to save step data'));
          }
        },
        error: (error: any) => {
          reject(error);
        }
      });
    });
  }

  private async completeSetup(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.setupWizardService.completeSetup().subscribe({
        next: (response) => {
          if (response.success) {
            this.notificationService.showSuccess(
              'Your organization has been set up successfully! Redirecting to dashboard...',
              'Setup Complete'
            );

            // Add a small delay before redirect to show the success message
            setTimeout(() => {
              this.router.navigate(['/dashboard']);
            }, 2000);
            
            resolve();
          } else {
            reject(new Error(response.message || 'Failed to complete setup'));
          }
        },
        error: (error) => {
          reject(error);
        }
      });
    });
  }

  private markCurrentFormAsTouched(): void {
    let form: FormGroup | null = null;

    switch (this.currentStep?.id) {
      case 'organization':
        form = this.organizationForm;
        break;
      case 'admin-user':
        form = this.adminUserForm;
        break;
      case 'branch':
        form = this.branchForm;
        break;
      case 'roles':
        form = this.rolesForm;
        break;
      case 'preferences':
        form = this.preferencesForm;
        break;
    }

    if (form) {
      Object.keys(form.controls).forEach(key => {
        const control = form!.get(key);
        control?.markAsTouched();
      });
    }
  }

  isFieldInvalid(form: FormGroup, fieldName: string): boolean {
    const field = form.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldErrorMessage(form: FormGroup, fieldName: string): string {
    const field = form.get(fieldName);
    if (!field || !field.errors || !field.touched) return '';

    const errors = field.errors;

    if (errors['required']) return `${this.getFieldDisplayName(fieldName)} is required`;
    if (errors['email']) return 'Please enter a valid email address';
    if (errors['minlength']) return `${this.getFieldDisplayName(fieldName)} must be at least ${errors['minlength'].requiredLength} characters`;
    if (errors['min']) return `${this.getFieldDisplayName(fieldName)} must be at least ${errors['min'].min}`;
    if (errors['max']) return `${this.getFieldDisplayName(fieldName)} cannot exceed ${errors['max'].max}`;
    if (errors['invalidPhone']) return 'Please enter a valid phone number';
    if (errors['invalidWebsite']) return 'Please enter a valid website URL';
    if (errors['passwordMismatch']) return 'Passwords do not match';
    if (errors['strongPassword']) {
      const requirements = [];
      if (!errors['strongPassword'].hasUpperCase) requirements.push('uppercase letter');
      if (!errors['strongPassword'].hasLowerCase) requirements.push('lowercase letter');
      if (!errors['strongPassword'].hasNumeric) requirements.push('number');
      if (!errors['strongPassword'].hasSpecialChar) requirements.push('special character');
      if (!errors['strongPassword'].isValidLength) requirements.push('at least 8 characters');
      return `Password must contain: ${requirements.join(', ')}`;
    }

    return 'Invalid input';
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      'name': 'Name',
      'firstName': 'First Name',
      'lastName': 'Last Name',
      'email': 'Email',
      'phone': 'Phone',
      'address': 'Address',
      'website': 'Website',
      'password': 'Password',
      'confirmPassword': 'Confirm Password',
      'normalWorkingHours': 'Working Hours',
      'overtimeRate': 'Overtime Rate',
      'productiveHoursThreshold': 'Productive Hours Threshold'
    };
    return displayNames[fieldName] || fieldName;
  }

  // Method to check if setup wizard should be shown
  shouldShowSetupWizard(): void {
    this.setupWizardService.isSetupRequired()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (isRequired) => {
          if (!isRequired) {
            this.router.navigate(['/dashboard']);
          }
        },
        error: (error) => {
          console.error('Error checking setup requirement:', error);
        }
      });
  }

  // Load existing setup data if available
  private loadExistingSetupData(): void {
    const currentData = this.setupWizardService.getCurrentSetupData();
    
    if (currentData.organization) {
      this.organizationForm.patchValue(currentData.organization);
    }
    
    if (currentData.adminUser) {
      // Don't pre-populate password fields for security
      const { password, confirmPassword, ...adminData } = currentData.adminUser;
      this.adminUserForm.patchValue(adminData);
    }
    
    if (currentData.branch) {
      this.branchForm.patchValue(currentData.branch);
    }
    
    if (currentData.preferences) {
      this.preferencesForm.patchValue(currentData.preferences);
    }
    
    if (currentData.roles) {
      // Update role selections
      currentData.roles.selectedRoles.forEach(roleId => {
        const control = this.rolesForm.get(roleId);
        if (control) {
          control.setValue(true);
        }
      });
    }
  }
}