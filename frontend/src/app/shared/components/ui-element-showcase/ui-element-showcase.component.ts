import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { UIIntegrationService, UIIntegrationReport } from '../../services/ui-integration.service';
import { ButtonHandlerService } from '../../services/button-handler.service';
import { DropdownDataService, DropdownOption } from '../../services/dropdown-data.service';
import { SearchService, SearchResponse } from '../../services/search.service';
import { FormValidationService } from '../../services/form-validation.service';
import { CRUDOperationsService } from '../../services/crud-operations.service';
import { NavigationService } from '../../../core/services/navigation.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-ui-element-showcase',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="ui-showcase-container">
      <div class="page-header">
        <h1>UI Element Showcase</h1>
        <p class="text-muted">Demonstration of all interactive UI elements with proper functionality</p>
        
        <div class="d-flex gap-2 mb-3">
          <button class="btn btn-primary" (click)="runHealthCheck()">
            <i class="fas fa-heartbeat me-2"></i>Run Health Check
          </button>
          <button class="btn btn-success" (click)="autoFixIssues()" [disabled]="isAutoFixing">
            <i class="fas fa-wrench me-2"></i>
            {{ isAutoFixing ? 'Auto-Fixing...' : 'Auto-Fix Issues' }}
          </button>
          <button class="btn btn-info" (click)="exportReport()">
            <i class="fas fa-download me-2"></i>Export Report
          </button>
        </div>

        <!-- Health Status -->
        <div class="health-status" *ngIf="healthReport">
          <div class="alert" [class]="'alert-' + getHealthAlertClass(healthReport.overallHealth)">
            <h5 class="alert-heading">
              <i [class]="getHealthIcon(healthReport.overallHealth)" class="me-2"></i>
              Overall Health: {{ healthReport.overallHealth | titlecase }}
            </h5>
            <p class="mb-0">
              {{ healthReport.workingElements }} of {{ healthReport.totalElements }} elements are working properly
              ({{ getHealthPercentage(healthReport) }}%)
            </p>
          </div>
        </div>
      </div>

      <!-- Navigation Elements -->
      <div class="showcase-section">
        <h3><i class="fas fa-compass me-2"></i>Navigation Elements</h3>
        <div class="card">
          <div class="card-body">
            <h5>Navigation Menu Items</h5>
            <div class="row g-2">
              <div class="col-md-3" *ngFor="let navItem of navigationItems">
                <button 
                  class="btn btn-outline-primary w-100" 
                  (click)="testNavigation(navItem.route)"
                  [title]="navItem.label">
                  <i [class]="navItem.icon + ' me-2'"></i>
                  {{ navItem.label }}
                </button>
              </div>
            </div>
            
            <h5 class="mt-4">Breadcrumb Navigation</h5>
            <nav aria-label="breadcrumb">
              <ol class="breadcrumb">
                <li class="breadcrumb-item">
                  <a href="#" (click)="testNavigation('/dashboard')">Dashboard</a>
                </li>
                <li class="breadcrumb-item">
                  <a href="#" (click)="testNavigation('/employees')">Employees</a>
                </li>
                <li class="breadcrumb-item active" aria-current="page">UI Showcase</li>
              </ol>
            </nav>
          </div>
        </div>
      </div>

      <!-- Button Elements -->
      <div class="showcase-section">
        <h3><i class="fas fa-mouse-pointer me-2"></i>Button Elements</h3>
        <div class="card">
          <div class="card-body">
            <h5>Action Buttons</h5>
            <div class="row g-2 mb-3">
              <div class="col-md-2">
                <button class="btn btn-success w-100" (click)="testButton('btn-check-in')">
                  <i class="fas fa-sign-in-alt me-2"></i>Check In
                </button>
              </div>
              <div class="col-md-2">
                <button class="btn btn-danger w-100" (click)="testButton('btn-check-out')">
                  <i class="fas fa-sign-out-alt me-2"></i>Check Out
                </button>
              </div>
              <div class="col-md-2">
                <button class="btn btn-warning w-100" (click)="testButton('btn-start-break')">
                  <i class="fas fa-coffee me-2"></i>Start Break
                </button>
              </div>
              <div class="col-md-2">
                <button class="btn btn-info w-100" (click)="testButton('btn-end-break')">
                  <i class="fas fa-stop me-2"></i>End Break
                </button>
              </div>
              <div class="col-md-2">
                <button class="btn btn-primary w-100" (click)="testButton('btn-add-employee')">
                  <i class="fas fa-user-plus me-2"></i>Add Employee
                </button>
              </div>
              <div class="col-md-2">
                <button class="btn btn-secondary w-100" (click)="testButton('btn-generate-report')">
                  <i class="fas fa-chart-bar me-2"></i>Generate Report
                </button>
              </div>
            </div>

            <h5>CRUD Operation Buttons</h5>
            <div class="row g-2">
              <div class="col-md-2">
                <button class="btn btn-outline-primary w-100" (click)="testCRUD('create')">
                  <i class="fas fa-plus me-2"></i>Create
                </button>
              </div>
              <div class="col-md-2">
                <button class="btn btn-outline-info w-100" (click)="testCRUD('read')">
                  <i class="fas fa-eye me-2"></i>View
                </button>
              </div>
              <div class="col-md-2">
                <button class="btn btn-outline-warning w-100" (click)="testCRUD('update')">
                  <i class="fas fa-edit me-2"></i>Edit
                </button>
              </div>
              <div class="col-md-2">
                <button class="btn btn-outline-danger w-100" (click)="testCRUD('delete')">
                  <i class="fas fa-trash me-2"></i>Delete
                </button>
              </div>
              <div class="col-md-2">
                <button class="btn btn-outline-success w-100" (click)="testCRUD('list')">
                  <i class="fas fa-list me-2"></i>List
                </button>
              </div>
              <div class="col-md-2">
                <button class="btn btn-outline-secondary w-100" (click)="testCRUD('search')">
                  <i class="fas fa-search me-2"></i>Search
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Dropdown Elements -->
      <div class="showcase-section">
        <h3><i class="fas fa-chevron-down me-2"></i>Dropdown Elements</h3>
        <div class="card">
          <div class="card-body">
            <div class="row g-3">
              <div class="col-md-3">
                <label class="form-label">Departments</label>
                <select class="form-select" (change)="onDropdownChange('departments', $event)">
                  <option value="">Select Department</option>
                  <option *ngFor="let option of departmentOptions" [value]="option.value">
                    {{ option.label }}
                  </option>
                </select>
              </div>
              <div class="col-md-3">
                <label class="form-label">Designations</label>
                <select class="form-select" (change)="onDropdownChange('designations', $event)">
                  <option value="">Select Designation</option>
                  <option *ngFor="let option of designationOptions" [value]="option.value">
                    {{ option.label }}
                  </option>
                </select>
              </div>
              <div class="col-md-3">
                <label class="form-label">Branches</label>
                <select class="form-select" (change)="onDropdownChange('branches', $event)">
                  <option value="">Select Branch</option>
                  <option *ngFor="let option of branchOptions" [value]="option.value">
                    {{ option.label }}
                  </option>
                </select>
              </div>
              <div class="col-md-3">
                <label class="form-label">Leave Types</label>
                <select class="form-select" (change)="onDropdownChange('leave-types', $event)">
                  <option value="">Select Leave Type</option>
                  <option *ngFor="let option of leaveTypeOptions" [value]="option.value">
                    {{ option.label }}
                  </option>
                </select>
              </div>
            </div>
            
            <div class="mt-3">
              <button class="btn btn-outline-primary" (click)="refreshAllDropdowns()">
                <i class="fas fa-sync me-2"></i>Refresh All Dropdowns
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Search Elements -->
      <div class="showcase-section">
        <h3><i class="fas fa-search me-2"></i>Search Elements</h3>
        <div class="card">
          <div class="card-body">
            <div class="row g-3">
              <div class="col-md-6">
                <label class="form-label">Employee Search</label>
                <input 
                  type="text" 
                  class="form-control" 
                  placeholder="Search employees..."
                  [(ngModel)]="employeeSearchTerm"
                  (input)="onSearch('employees', employeeSearchTerm)">
                <div class="search-results mt-2" *ngIf="employeeSearchResults.results.length > 0">
                  <div class="list-group">
                    <div 
                      class="list-group-item list-group-item-action" 
                      *ngFor="let result of employeeSearchResults.results"
                      (click)="selectSearchResult('employees', result)">
                      <div class="d-flex w-100 justify-content-between">
                        <h6 class="mb-1" [innerHTML]="result.highlighted || result.label"></h6>
                        <small>{{ result.data?.designation }}</small>
                      </div>
                      <p class="mb-1" *ngIf="result.subtitle">{{ result.subtitle }}</p>
                    </div>
                  </div>
                </div>
              </div>
              <div class="col-md-6">
                <label class="form-label">Project Search</label>
                <input 
                  type="text" 
                  class="form-control" 
                  placeholder="Search projects..."
                  [(ngModel)]="projectSearchTerm"
                  (input)="onSearch('projects', projectSearchTerm)">
                <div class="search-results mt-2" *ngIf="projectSearchResults.results.length > 0">
                  <div class="list-group">
                    <div 
                      class="list-group-item list-group-item-action" 
                      *ngFor="let result of projectSearchResults.results"
                      (click)="selectSearchResult('projects', result)">
                      <div class="d-flex w-100 justify-content-between">
                        <h6 class="mb-1" [innerHTML]="result.highlighted || result.label"></h6>
                        <small>{{ result.data?.status }}</small>
                      </div>
                      <p class="mb-1" *ngIf="result.subtitle">{{ result.subtitle }}</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Form Elements -->
      <div class="showcase-section">
        <h3><i class="fas fa-wpforms me-2"></i>Form Elements</h3>
        <div class="card">
          <div class="card-body">
            <form [formGroup]="demoForm" (ngSubmit)="onFormSubmit()">
              <div class="row g-3">
                <div class="col-md-6">
                  <label class="form-label">First Name *</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    formControlName="firstName"
                    [class.is-invalid]="formValidation.isFieldInvalid(demoForm, 'firstName')"
                    placeholder="Enter first name">
                  <div class="invalid-feedback" *ngIf="formValidation.isFieldInvalid(demoForm, 'firstName')">
                    {{ formValidation.getValidationMessage(demoForm.get('firstName')!) }}
                  </div>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Last Name *</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    formControlName="lastName"
                    [class.is-invalid]="formValidation.isFieldInvalid(demoForm, 'lastName')"
                    placeholder="Enter last name">
                  <div class="invalid-feedback" *ngIf="formValidation.isFieldInvalid(demoForm, 'lastName')">
                    {{ formValidation.getValidationMessage(demoForm.get('lastName')!) }}
                  </div>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Email *</label>
                  <input 
                    type="email" 
                    class="form-control" 
                    formControlName="email"
                    [class.is-invalid]="formValidation.isFieldInvalid(demoForm, 'email')"
                    placeholder="Enter email address">
                  <div class="invalid-feedback" *ngIf="formValidation.isFieldInvalid(demoForm, 'email')">
                    {{ formValidation.getValidationMessage(demoForm.get('email')!) }}
                  </div>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Phone</label>
                  <input 
                    type="tel" 
                    class="form-control" 
                    formControlName="phone"
                    [class.is-invalid]="formValidation.isFieldInvalid(demoForm, 'phone')"
                    placeholder="Enter phone number">
                  <div class="invalid-feedback" *ngIf="formValidation.isFieldInvalid(demoForm, 'phone')">
                    {{ formValidation.getValidationMessage(demoForm.get('phone')!) }}
                  </div>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Date of Birth</label>
                  <input 
                    type="date" 
                    class="form-control" 
                    formControlName="dateOfBirth"
                    [class.is-invalid]="formValidation.isFieldInvalid(demoForm, 'dateOfBirth')">
                  <div class="invalid-feedback" *ngIf="formValidation.isFieldInvalid(demoForm, 'dateOfBirth')">
                    {{ formValidation.getValidationMessage(demoForm.get('dateOfBirth')!) }}
                  </div>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Department</label>
                  <select 
                    class="form-select" 
                    formControlName="department"
                    [class.is-invalid]="formValidation.isFieldInvalid(demoForm, 'department')">
                    <option value="">Select Department</option>
                    <option *ngFor="let option of departmentOptions" [value]="option.value">
                      {{ option.label }}
                    </option>
                  </select>
                  <div class="invalid-feedback" *ngIf="formValidation.isFieldInvalid(demoForm, 'department')">
                    {{ formValidation.getValidationMessage(demoForm.get('department')!) }}
                  </div>
                </div>
              </div>
              
              <div class="mt-4">
                <button type="submit" class="btn btn-primary me-2" [disabled]="!demoForm.valid">
                  <i class="fas fa-save me-2"></i>Save Form
                </button>
                <button type="button" class="btn btn-outline-secondary me-2" (click)="resetForm()">
                  <i class="fas fa-undo me-2"></i>Reset
                </button>
                <button type="button" class="btn btn-outline-info" (click)="validateForm()">
                  <i class="fas fa-check me-2"></i>Validate
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>

      <!-- Health Report -->
      <div class="showcase-section" *ngIf="healthReport">
        <h3><i class="fas fa-chart-pie me-2"></i>Health Report</h3>
        <div class="row g-3">
          <div class="col-md-2" *ngFor="let category of getHealthCategories()">
            <div class="card text-center">
              <div class="card-body">
                <h5 class="card-title">{{ category.name }}</h5>
                <div class="progress mb-2">
                  <div 
                    class="progress-bar" 
                    [class]="'bg-' + getHealthColor(category.healthScore)"
                    [style.width.%]="category.healthScore">
                  </div>
                </div>
                <p class="card-text">
                  <small class="text-muted">
                    {{ category.workingElements }}/{{ category.totalElements }}
                  </small>
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- Recommendations -->
        <div class="mt-4" *ngIf="healthReport.recommendations.length > 0">
          <h5>Recommendations</h5>
          <div class="alert alert-info">
            <ul class="mb-0">
              <li *ngFor="let recommendation of healthReport.recommendations">
                {{ recommendation }}
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .ui-showcase-container {
      padding: 2rem;
    }

    .page-header {
      margin-bottom: 2rem;
    }

    .page-header h1 {
      font-size: 2.5rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .showcase-section {
      margin-bottom: 3rem;
    }

    .showcase-section h3 {
      color: var(--primary);
      margin-bottom: 1rem;
      padding-bottom: 0.5rem;
      border-bottom: 2px solid var(--primary);
    }

    .health-status {
      margin-bottom: 2rem;
    }

    .search-results {
      max-height: 200px;
      overflow-y: auto;
      border: 1px solid #dee2e6;
      border-radius: 0.375rem;
    }

    .list-group-item {
      cursor: pointer;
    }

    .list-group-item:hover {
      background-color: #f8f9fa;
    }

    .progress {
      height: 8px;
    }

    .card {
      border: none;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      border-radius: 12px;
    }

    .card-header {
      background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
      border-bottom: 1px solid #dee2e6;
      border-radius: 12px 12px 0 0 !important;
    }

    .btn {
      border-radius: 8px;
      font-weight: 500;
      transition: all 0.2s ease;
    }

    .btn:hover {
      transform: translateY(-1px);
    }

    .btn:active {
      transform: translateY(0);
    }

    .form-control,
    .form-select {
      border-radius: 8px;
      border: 2px solid #e9ecef;
      transition: all 0.15s ease;
    }

    .form-control:focus,
    .form-select:focus {
      border-color: var(--primary);
      box-shadow: 0 0 0 0.2rem rgba(var(--primary-rgb), 0.25);
    }

    .invalid-feedback {
      display: block;
    }

    /* Mobile responsiveness */
    @media (max-width: 768px) {
      .ui-showcase-container {
        padding: 1rem;
      }

      .page-header h1 {
        font-size: 2rem;
      }

      .showcase-section h3 {
        font-size: 1.5rem;
      }

      .row.g-2 > * {
        margin-bottom: 0.5rem;
      }

      .btn {
        width: 100%;
        margin-bottom: 0.5rem;
      }
    }
  `]
})
export class UIElementShowcaseComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Health report
  healthReport: UIIntegrationReport | null = null;
  isAutoFixing = false;

  // Navigation items
  navigationItems = [
    { label: 'Dashboard', route: '/dashboard', icon: 'fas fa-tachometer-alt' },
    { label: 'Employees', route: '/employees', icon: 'fas fa-users' },
    { label: 'Attendance', route: '/attendance', icon: 'fas fa-clock' },
    { label: 'Projects', route: '/projects', icon: 'fas fa-project-diagram' },
    { label: 'Reports', route: '/reports', icon: 'fas fa-chart-bar' },
    { label: 'Settings', route: '/settings', icon: 'fas fa-cog' }
  ];

  // Dropdown options
  departmentOptions: DropdownOption[] = [];
  designationOptions: DropdownOption[] = [];
  branchOptions: DropdownOption[] = [];
  leaveTypeOptions: DropdownOption[] = [];

  // Search
  employeeSearchTerm = '';
  projectSearchTerm = '';
  employeeSearchResults: SearchResponse = { results: [], totalCount: 0, hasMore: false, searchTerm: '' };
  projectSearchResults: SearchResponse = { results: [], totalCount: 0, hasMore: false, searchTerm: '' };

  // Demo form
  demoForm: FormGroup;

  constructor(
    private uiIntegration: UIIntegrationService,
    private buttonHandler: ButtonHandlerService,
    private dropdownData: DropdownDataService,
    private searchService: SearchService,
    public formValidation: FormValidationService,
    private crudOperations: CRUDOperationsService,
    private navigationService: NavigationService,
    private notificationService: NotificationService,
    private fb: FormBuilder
  ) {
    this.demoForm = this.createDemoForm();
  }

  ngOnInit(): void {
    this.initializeShowcase();
    this.loadDropdownData();
    this.setupSearchSubscriptions();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createDemoForm(): FormGroup {
    return this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [FormValidationService.phoneValidator]],
      dateOfBirth: [''],
      department: ['']
    });
  }

  private initializeShowcase(): void {
    // Initialize UI services
    this.uiIntegration.initializeUIServices().subscribe();
    
    // Register demo form
    this.formValidation.registerForm('demo-form', this.demoForm);
    
    // Run initial health check
    this.runHealthCheck();
  }

  private loadDropdownData(): void {
    // Load department options
    this.dropdownData.getDropdownData('departments')
      .pipe(takeUntil(this.destroy$))
      .subscribe(options => {
        this.departmentOptions = options;
      });

    // Load designation options
    this.dropdownData.getDropdownData('designations')
      .pipe(takeUntil(this.destroy$))
      .subscribe(options => {
        this.designationOptions = options;
      });

    // Load branch options
    this.dropdownData.getDropdownData('branches')
      .pipe(takeUntil(this.destroy$))
      .subscribe(options => {
        this.branchOptions = options;
      });

    // Load leave type options
    this.dropdownData.getDropdownData('leave-types')
      .pipe(takeUntil(this.destroy$))
      .subscribe(options => {
        this.leaveTypeOptions = options;
      });
  }

  private setupSearchSubscriptions(): void {
    // Employee search results
    this.searchService.getSearchResults('employees')
      .pipe(takeUntil(this.destroy$))
      .subscribe(results => {
        this.employeeSearchResults = results;
      });

    // Project search results
    this.searchService.getSearchResults('projects')
      .pipe(takeUntil(this.destroy$))
      .subscribe(results => {
        this.projectSearchResults = results;
      });
  }

  // Health check methods
  runHealthCheck(): void {
    this.notificationService.showInfo('Running UI health check...');
    
    this.uiIntegration.generateIntegrationReport()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (report) => {
          this.healthReport = report;
          this.notificationService.showSuccess('Health check completed');
        },
        error: (error) => {
          console.error('Health check failed:', error);
          this.notificationService.showError('Health check failed');
        }
      });
  }

  autoFixIssues(): void {
    this.isAutoFixing = true;
    
    this.uiIntegration.autoFixUIIssues()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (success) => {
          this.isAutoFixing = false;
          if (success) {
            // Re-run health check after auto-fix
            setTimeout(() => this.runHealthCheck(), 1000);
          }
        },
        error: (error) => {
          console.error('Auto-fix failed:', error);
          this.isAutoFixing = false;
        }
      });
  }

  exportReport(): void {
    if (!this.healthReport) {
      this.notificationService.showWarning('No health report available to export');
      return;
    }

    this.uiIntegration.exportReport('json')
      .pipe(takeUntil(this.destroy$))
      .subscribe(blob => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `ui-health-report-${new Date().toISOString().split('T')[0]}.json`;
        link.click();
        window.URL.revokeObjectURL(url);
        
        this.notificationService.showSuccess('Report exported successfully');
      });
  }

  // Navigation methods
  testNavigation(route: string): void {
    this.navigationService.navigateTo(route)
      .then(success => {
        if (success) {
          this.notificationService.showSuccess(`Navigation to ${route} successful`);
        } else {
          this.notificationService.showError(`Navigation to ${route} failed`);
        }
      });
  }

  // Button methods
  testButton(buttonId: string): void {
    this.buttonHandler.handleButtonClick(buttonId)
      .then(() => {
        this.notificationService.showSuccess(`Button ${buttonId} clicked successfully`);
      })
      .catch(error => {
        console.error(`Button ${buttonId} failed:`, error);
        this.notificationService.showError(`Button ${buttonId} failed`);
      });
  }

  // CRUD methods
  testCRUD(operation: string): void {
    const operationId = `employee-${operation}`;
    const testData = { firstName: 'Test', lastName: 'User', email: 'test@example.com' };
    const testParams = { id: 1 };

    this.crudOperations.executeCRUDOperation(operationId, testData, testParams)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.notificationService.showSuccess(`CRUD ${operation} operation successful`);
        },
        error: (error) => {
          console.error(`CRUD ${operation} failed:`, error);
          this.notificationService.showError(`CRUD ${operation} operation failed`);
        }
      });
  }

  // Dropdown methods
  onDropdownChange(dropdownId: string, event: any): void {
    const value = event.target.value;
    this.notificationService.showInfo(`${dropdownId} changed to: ${value}`);
  }

  refreshAllDropdowns(): void {
    this.dropdownData.refreshAllDropdowns()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notificationService.showSuccess('All dropdowns refreshed');
        },
        error: (error) => {
          console.error('Dropdown refresh failed:', error);
          this.notificationService.showError('Failed to refresh dropdowns');
        }
      });
  }

  // Search methods
  onSearch(searchId: string, searchTerm: string): void {
    this.searchService.search(searchId, searchTerm);
  }

  selectSearchResult(searchId: string, result: any): void {
    this.notificationService.showInfo(`Selected ${searchId}: ${result.label}`);
    
    // Clear search results
    if (searchId === 'employees') {
      this.employeeSearchTerm = result.label;
      this.employeeSearchResults = { results: [], totalCount: 0, hasMore: false, searchTerm: '' };
    } else if (searchId === 'projects') {
      this.projectSearchTerm = result.label;
      this.projectSearchResults = { results: [], totalCount: 0, hasMore: false, searchTerm: '' };
    }
  }

  // Form methods
  onFormSubmit(): void {
    if (this.demoForm.valid) {
      const formData = this.demoForm.value;
      this.notificationService.showSuccess('Form submitted successfully');
      console.log('Form data:', formData);
    } else {
      const firstError = this.formValidation.validateFormAndGetFirstError(this.demoForm);
      this.notificationService.showError(firstError || 'Please fix form errors');
    }
  }

  resetForm(): void {
    this.demoForm.reset();
    this.formValidation.clearValidationErrors(this.demoForm);
    this.notificationService.showInfo('Form reset');
  }

  validateForm(): void {
    this.formValidation.markAllFieldsAsTouched(this.demoForm);
    const errors = this.formValidation.getAllValidationErrors(this.demoForm);
    
    if (Object.keys(errors).length === 0) {
      this.notificationService.showSuccess('Form validation passed');
    } else {
      this.notificationService.showError(`Form has ${Object.keys(errors).length} validation errors`);
    }
  }

  // Health report helper methods
  getHealthCategories(): any[] {
    if (!this.healthReport) return [];
    return Object.values(this.healthReport.categories);
  }

  getHealthAlertClass(health: string): string {
    switch (health) {
      case 'excellent': return 'success';
      case 'good': return 'info';
      case 'fair': return 'warning';
      case 'poor': return 'danger';
      default: return 'secondary';
    }
  }

  getHealthIcon(health: string): string {
    switch (health) {
      case 'excellent': return 'fas fa-check-circle text-success';
      case 'good': return 'fas fa-thumbs-up text-info';
      case 'fair': return 'fas fa-exclamation-triangle text-warning';
      case 'poor': return 'fas fa-times-circle text-danger';
      default: return 'fas fa-question-circle text-secondary';
    }
  }

  getHealthColor(score: number): string {
    if (score >= 90) return 'success';
    if (score >= 70) return 'info';
    if (score >= 50) return 'warning';
    return 'danger';
  }

  getHealthPercentage(report: UIIntegrationReport): number {
    return report.totalElements > 0 ? Math.round((report.workingElements / report.totalElements) * 100) : 100;
  }
}