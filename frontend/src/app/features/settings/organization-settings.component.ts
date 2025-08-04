import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { OrganizationService } from '../../services/organization.service';
import { Organization, CreateOrganizationDto, UpdateOrganizationDto } from '../../models/admin.models';

@Component({
  selector: 'app-organization-settings',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="organization-settings-container">
      <!-- Header -->
      <div class="page-header">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <h1>Organization Settings</h1>
            <p class="text-muted">Configure organization details and global settings</p>
          </div>
          <button class="btn btn-outline-secondary" routerLink="/settings">
            <i class="fas fa-arrow-left me-2"></i>Back to Settings
          </button>
        </div>
      </div>

      <div class="row">
        <!-- Organization Form -->
        <div class="col-lg-8">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">Organization Information</h5>
            </div>
            <div class="card-body">
              <form [formGroup]="organizationForm" (ngSubmit)="saveOrganization()">
                <div class="row g-3">
                  <!-- Organization Name -->
                  <div class="col-md-6">
                    <label class="form-label">Organization Name *</label>
                    <input 
                      type="text" 
                      class="form-control"
                      formControlName="name"
                      [class.is-invalid]="organizationForm.get('name')?.invalid && organizationForm.get('name')?.touched">
                    <div class="invalid-feedback" *ngIf="organizationForm.get('name')?.invalid && organizationForm.get('name')?.touched">
                      Organization name is required
                    </div>
                  </div>

                  <!-- Email -->
                  <div class="col-md-6">
                    <label class="form-label">Email Address *</label>
                    <input 
                      type="email" 
                      class="form-control"
                      formControlName="email"
                      [class.is-invalid]="organizationForm.get('email')?.invalid && organizationForm.get('email')?.touched">
                    <div class="invalid-feedback" *ngIf="organizationForm.get('email')?.invalid && organizationForm.get('email')?.touched">
                      Valid email address is required
                    </div>
                  </div>

                  <!-- Phone -->
                  <div class="col-md-6">
                    <label class="form-label">Phone Number *</label>
                    <input 
                      type="tel" 
                      class="form-control"
                      formControlName="phone"
                      [class.is-invalid]="organizationForm.get('phone')?.invalid && organizationForm.get('phone')?.touched">
                    <div class="invalid-feedback" *ngIf="organizationForm.get('phone')?.invalid && organizationForm.get('phone')?.touched">
                      Phone number is required
                    </div>
                  </div>

                  <!-- Normal Working Hours -->
                  <div class="col-md-6">
                    <label class="form-label">Normal Working Hours *</label>
                    <input 
                      type="text" 
                      class="form-control"
                      formControlName="normalWorkingHours"
                      placeholder="e.g., 08:00"
                      [class.is-invalid]="organizationForm.get('normalWorkingHours')?.invalid && organizationForm.get('normalWorkingHours')?.touched">
                    <div class="invalid-feedback" *ngIf="organizationForm.get('normalWorkingHours')?.invalid && organizationForm.get('normalWorkingHours')?.touched">
                      Normal working hours is required
                    </div>
                  </div>

                  <!-- Address -->
                  <div class="col-12">
                    <label class="form-label">Address *</label>
                    <textarea 
                      class="form-control" 
                      rows="3"
                      formControlName="address"
                      [class.is-invalid]="organizationForm.get('address')?.invalid && organizationForm.get('address')?.touched"></textarea>
                    <div class="invalid-feedback" *ngIf="organizationForm.get('address')?.invalid && organizationForm.get('address')?.touched">
                      Address is required
                    </div>
                  </div>

                  <!-- Overtime Rate -->
                  <div class="col-md-6">
                    <label class="form-label">Overtime Rate Multiplier *</label>
                    <input 
                      type="number" 
                      class="form-control"
                      formControlName="overtimeRate"
                      step="0.1"
                      min="1"
                      [class.is-invalid]="organizationForm.get('overtimeRate')?.invalid && organizationForm.get('overtimeRate')?.touched">
                    <div class="invalid-feedback" *ngIf="organizationForm.get('overtimeRate')?.invalid && organizationForm.get('overtimeRate')?.touched">
                      Overtime rate must be at least 1.0
                    </div>
                  </div>

                  <!-- Productive Hours Threshold -->
                  <div class="col-md-6">
                    <label class="form-label">Productive Hours Threshold *</label>
                    <input 
                      type="number" 
                      class="form-control"
                      formControlName="productiveHoursThreshold"
                      min="1"
                      max="24"
                      [class.is-invalid]="organizationForm.get('productiveHoursThreshold')?.invalid && organizationForm.get('productiveHoursThreshold')?.touched">
                    <div class="invalid-feedback" *ngIf="organizationForm.get('productiveHoursThreshold')?.invalid && organizationForm.get('productiveHoursThreshold')?.touched">
                      Productive hours threshold must be between 1 and 24
                    </div>
                  </div>

                  <!-- Branch Isolation -->
                  <div class="col-12">
                    <div class="form-check">
                      <input 
                        class="form-check-input" 
                        type="checkbox" 
                        formControlName="branchIsolationEnabled"
                        id="branchIsolation">
                      <label class="form-check-label" for="branchIsolation">
                        Enable Branch Data Isolation
                      </label>
                      <div class="form-text">
                        When enabled, users can only access data from their assigned branch
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Form Actions -->
                <div class="d-flex justify-content-end gap-2 mt-4">
                  <button type="button" class="btn btn-outline-secondary" (click)="resetForm()">
                    Reset
                  </button>
                  <button type="submit" class="btn btn-primary" [disabled]="organizationForm.invalid || isLoading">
                    <span *ngIf="isLoading" class="spinner-border spinner-border-sm me-2"></span>
                    {{ currentOrganization ? 'Update Organization' : 'Create Organization' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>

        <!-- Logo Upload -->
        <div class="col-lg-4">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">Organization Logo</h5>
            </div>
            <div class="card-body text-center">
              <!-- Current Logo -->
              <div class="logo-preview mb-3" *ngIf="currentLogo">
                <img [src]="currentLogo" alt="Organization Logo" class="img-fluid rounded">
              </div>
              
              <!-- No Logo Placeholder -->
              <div class="logo-placeholder mb-3" *ngIf="!currentLogo">
                <i class="fas fa-building text-muted"></i>
                <p class="text-muted mt-2">No logo uploaded</p>
              </div>

              <!-- Upload Controls -->
              <div class="upload-controls">
                <input 
                  type="file" 
                  #fileInput 
                  class="d-none" 
                  accept="image/*"
                  (change)="onFileSelected($event)">
                
                <button 
                  type="button" 
                  class="btn btn-outline-primary btn-sm me-2"
                  (click)="fileInput.click()"
                  [disabled]="isUploadingLogo">
                  <span *ngIf="isUploadingLogo" class="spinner-border spinner-border-sm me-1"></span>
                  <i class="fas fa-upload me-1" *ngIf="!isUploadingLogo"></i>
                  {{ currentLogo ? 'Change Logo' : 'Upload Logo' }}
                </button>

                <button 
                  type="button" 
                  class="btn btn-outline-danger btn-sm"
                  (click)="deleteLogo()"
                  *ngIf="currentLogo"
                  [disabled]="isUploadingLogo">
                  <i class="fas fa-trash me-1"></i>
                  Delete
                </button>
              </div>

              <div class="form-text mt-2">
                Supported formats: JPG, PNG, GIF<br>
                Maximum size: 10MB
              </div>
            </div>
          </div>

          <!-- Quick Stats -->
          <div class="card mt-4">
            <div class="card-header">
              <h5 class="card-title mb-0">Organization Stats</h5>
            </div>
            <div class="card-body">
              <div class="stat-item mb-3">
                <div class="d-flex justify-content-between">
                  <span>Total Branches</span>
                  <strong>{{ organizationStats.totalBranches }}</strong>
                </div>
              </div>
              <div class="stat-item mb-3">
                <div class="d-flex justify-content-between">
                  <span>Total Employees</span>
                  <strong>{{ organizationStats.totalEmployees }}</strong>
                </div>
              </div>
              <div class="stat-item">
                <div class="d-flex justify-content-between">
                  <span>Active Users</span>
                  <strong>{{ organizationStats.activeUsers }}</strong>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .organization-settings-container {
      padding: 2rem;
    }

    .page-header {
      margin-bottom: 2rem;
    }

    .page-header h1 {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .card {
      border: 1px solid var(--gray-200);
      border-radius: 12px;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-tertiary) 100%);
      border-bottom: 1px solid var(--gray-200);
      padding: 1.25rem 1.5rem;
      border-radius: 12px 12px 0 0;
    }

    .card-title {
      font-weight: 600;
      color: var(--text-primary);
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

    .btn {
      border-radius: 8px;
      font-weight: 500;
      padding: 0.625rem 1.25rem;
      transition: all 0.15s ease-in-out;
    }

    .btn-primary {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      border: none;
      color: white;
    }

    .btn-primary:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(59, 130, 246, 0.4);
    }

    .logo-preview img {
      max-width: 200px;
      max-height: 150px;
      object-fit: contain;
    }

    .logo-placeholder {
      padding: 2rem;
      border: 2px dashed var(--gray-300);
      border-radius: 8px;
    }

    .logo-placeholder i {
      font-size: 3rem;
    }

    .stat-item {
      padding: 0.5rem 0;
      border-bottom: 1px solid var(--gray-100);
    }

    .stat-item:last-child {
      border-bottom: none;
    }
  `]
})
export class OrganizationSettingsComponent implements OnInit {
  organizationForm: FormGroup;
  currentOrganization: Organization | null = null;
  currentLogo: string | null = null;
  isLoading = false;
  isUploadingLogo = false;

  organizationStats = {
    totalBranches: 3,
    totalEmployees: 125,
    activeUsers: 98
  };

  constructor(
    private fb: FormBuilder,
    private organizationService: OrganizationService
  ) {
    this.organizationForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadOrganization();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required]],
      address: ['', [Validators.required]],
      normalWorkingHours: ['08:00', [Validators.required]],
      overtimeRate: [1.5, [Validators.required, Validators.min(1)]],
      productiveHoursThreshold: [8, [Validators.required, Validators.min(1), Validators.max(24)]],
      branchIsolationEnabled: [false]
    });
  }

  private loadOrganization(): void {
    this.isLoading = true;
    
    // For now, we'll assume organization ID 1 exists
    this.organizationService.getOrganization(1).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.currentOrganization = response.data;
          this.populateForm(response.data);
          this.loadLogo();
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading organization:', error);
        this.isLoading = false;
      }
    });
  }

  private populateForm(organization: Organization): void {
    this.organizationForm.patchValue({
      name: organization.name,
      email: organization.email,
      phone: organization.phone,
      address: organization.address,
      normalWorkingHours: organization.normalWorkingHours,
      overtimeRate: organization.overtimeRate,
      productiveHoursThreshold: organization.productiveHoursThreshold,
      branchIsolationEnabled: organization.branchIsolationEnabled
    });
  }

  private loadLogo(): void {
    if (this.currentOrganization) {
      this.organizationService.getLogo(this.currentOrganization.id).subscribe({
        next: (blob) => {
          this.currentLogo = URL.createObjectURL(blob);
        },
        error: (error) => {
          console.log('No logo found or error loading logo:', error);
          this.currentLogo = null;
        }
      });
    }
  }

  saveOrganization(): void {
    if (this.organizationForm.invalid) {
      this.organizationForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const formData = this.organizationForm.value;

    if (this.currentOrganization) {
      // Update existing organization
      this.organizationService.updateOrganization(this.currentOrganization.id, formData).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('Organization updated successfully');
            this.currentOrganization = response.data!;
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error updating organization:', error);
          this.isLoading = false;
        }
      });
    } else {
      // Create new organization
      this.organizationService.createOrganization(formData).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('Organization created successfully');
            this.currentOrganization = response.data!;
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error creating organization:', error);
          this.isLoading = false;
        }
      });
    }
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file && this.currentOrganization) {
      this.uploadLogo(file);
    }
  }

  private uploadLogo(file: File): void {
    if (!this.currentOrganization) return;

    this.isUploadingLogo = true;
    this.organizationService.uploadLogo(this.currentOrganization.id, file).subscribe({
      next: (response) => {
        if (response.success) {
          console.log('Logo uploaded successfully');
          this.loadLogo(); // Reload the logo
        }
        this.isUploadingLogo = false;
      },
      error: (error) => {
        console.error('Error uploading logo:', error);
        this.isUploadingLogo = false;
      }
    });
  }

  deleteLogo(): void {
    if (!this.currentOrganization) return;

    this.isUploadingLogo = true;
    this.organizationService.deleteLogo(this.currentOrganization.id).subscribe({
      next: (response) => {
        if (response.success) {
          console.log('Logo deleted successfully');
          this.currentLogo = null;
        }
        this.isUploadingLogo = false;
      },
      error: (error) => {
        console.error('Error deleting logo:', error);
        this.isUploadingLogo = false;
      }
    });
  }

  resetForm(): void {
    if (this.currentOrganization) {
      this.populateForm(this.currentOrganization);
    } else {
      this.organizationForm.reset();
      this.organizationForm.patchValue({
        normalWorkingHours: '08:00',
        overtimeRate: 1.5,
        productiveHoursThreshold: 8,
        branchIsolationEnabled: false
      });
    }
  }
}