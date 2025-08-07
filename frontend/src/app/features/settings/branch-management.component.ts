import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { BranchService } from '../../services/branch.service';
import { Branch, CreateBranchDto, UpdateBranchDto, LocalHoliday } from '../../models/admin.models';

@Component({
  selector: 'app-branch-management',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="branch-management-container">
      <!-- Header -->
      <div class="page-header">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <h1>Branch Management</h1>
            <p class="text-muted">Manage branches, locations, and regional settings</p>
          </div>
          <div class="d-flex gap-2">
            <button class="btn btn-primary" (click)="showCreateModal()">
              <i class="fas fa-plus me-2"></i>Add Branch
            </button>
            <button class="btn btn-outline-secondary" routerLink="/settings">
              <i class="fas fa-arrow-left me-2"></i>Back to Settings
            </button>
          </div>
        </div>
      </div>

      <!-- Branches List -->
      <div class="card">
        <div class="card-header">
          <div class="d-flex justify-content-between align-items-center">
            <h5 class="card-title mb-0">All Branches</h5>
            <div class="search-box">
              <input 
                type="text" 
                class="form-control form-control-sm" 
                placeholder="Search branches..."
                [(ngModel)]="searchTerm"
                (input)="filterBranches()">
            </div>
          </div>
        </div>
        <div class="card-body">
          <div class="table-responsive">
            <table class="table table-hover">
              <thead>
                <tr>
                  <th>Branch Name</th>
                  <th>Country</th>
                  <th>Currency</th>
                  <th>Time Zone</th>
                  <th>Employees</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let branch of filteredBranches">
                  <td>
                    <div class="d-flex align-items-center">
                      <div class="branch-icon me-2">
                        <i class="fas fa-map-marker-alt text-primary"></i>
                      </div>
                      <div>
                        <div class="fw-semibold">{{ branch.name }}</div>
                        <small class="text-muted">{{ branch.address }}</small>
                      </div>
                    </div>
                  </td>
                  <td>
                    <span class="badge bg-light text-dark">{{ branch.country }}</span>
                  </td>
                  <td>{{ branch.currency }}</td>
                  <td>{{ branch.timeZone }}</td>
                  <td>
                    <span class="badge bg-info">{{ getBranchEmployeeCount(branch.id) }}</span>
                  </td>
                  <td>
                    <span class="badge bg-success">Active</span>
                  </td>
                  <td>
                    <div class="btn-group btn-group-sm">
                      <button class="btn btn-outline-primary" (click)="editBranch(branch)">
                        <i class="fas fa-edit"></i>
                      </button>
                      <button class="btn btn-outline-info" (click)="viewBranchDetails(branch)">
                        <i class="fas fa-eye"></i>
                      </button>
                      <button class="btn btn-outline-danger" (click)="deleteBranch(branch)">
                        <i class="fas fa-trash"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Create/Edit Branch Modal -->
      <div class="modal fade" [class.show]="showModal" [style.display]="showModal ? 'block' : 'none'" tabindex="-1">
        <div class="modal-dialog modal-lg">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">{{ isEditMode ? 'Edit Branch' : 'Create New Branch' }}</h5>
              <button type="button" class="btn-close" (click)="closeModal()"></button>
            </div>
            <div class="modal-body">
              <form [formGroup]="branchForm" (ngSubmit)="saveBranch()">
                <div class="row g-3">
                  <!-- Branch Name -->
                  <div class="col-md-6">
                    <label class="form-label">Branch Name *</label>
                    <input 
                      type="text" 
                      class="form-control"
                      formControlName="name"
                      [class.is-invalid]="branchForm.get('name')?.invalid && branchForm.get('name')?.touched">
                    <div class="invalid-feedback" *ngIf="branchForm.get('name')?.invalid && branchForm.get('name')?.touched">
                      Branch name is required
                    </div>
                  </div>

                  <!-- Country -->
                  <div class="col-md-6">
                    <label class="form-label">Country *</label>
                    <select 
                      class="form-select"
                      formControlName="country"
                      (change)="onCountryChange()"
                      [class.is-invalid]="branchForm.get('country')?.invalid && branchForm.get('country')?.touched">
                      <option value="">Select Country</option>
                      <option *ngFor="let country of supportedCountries" [value]="country">{{ country }}</option>
                    </select>
                    <div class="invalid-feedback" *ngIf="branchForm.get('country')?.invalid && branchForm.get('country')?.touched">
                      Country is required
                    </div>
                  </div>

                  <!-- Currency -->
                  <div class="col-md-6">
                    <label class="form-label">Currency *</label>
                    <select 
                      class="form-select"
                      formControlName="currency"
                      [class.is-invalid]="branchForm.get('currency')?.invalid && branchForm.get('currency')?.touched">
                      <option value="">Select Currency</option>
                      <option *ngFor="let currency of supportedCurrencies" [value]="currency">{{ currency }}</option>
                    </select>
                    <div class="invalid-feedback" *ngIf="branchForm.get('currency')?.invalid && branchForm.get('currency')?.touched">
                      Currency is required
                    </div>
                  </div>

                  <!-- Time Zone -->
                  <div class="col-md-6">
                    <label class="form-label">Time Zone *</label>
                    <select 
                      class="form-select"
                      formControlName="timeZone"
                      [class.is-invalid]="branchForm.get('timeZone')?.invalid && branchForm.get('timeZone')?.touched">
                      <option value="">Select Time Zone</option>
                      <option *ngFor="let tz of availableTimeZones" [value]="tz">{{ tz }}</option>
                    </select>
                    <div class="invalid-feedback" *ngIf="branchForm.get('timeZone')?.invalid && branchForm.get('timeZone')?.touched">
                      Time zone is required
                    </div>
                  </div>

                  <!-- Address -->
                  <div class="col-12">
                    <label class="form-label">Address *</label>
                    <textarea 
                      class="form-control" 
                      rows="3"
                      formControlName="address"
                      [class.is-invalid]="branchForm.get('address')?.invalid && branchForm.get('address')?.touched"></textarea>
                    <div class="invalid-feedback" *ngIf="branchForm.get('address')?.invalid && branchForm.get('address')?.touched">
                      Address is required
                    </div>
                  </div>

                  <!-- Local Holidays Section -->
                  <div class="col-12">
                    <div class="card">
                      <div class="card-header">
                        <div class="d-flex justify-content-between align-items-center">
                          <h6 class="mb-0">Local Holidays</h6>
                          <button type="button" class="btn btn-sm btn-outline-primary" (click)="addHoliday()">
                            <i class="fas fa-plus me-1"></i>Add Holiday
                          </button>
                        </div>
                      </div>
                      <div class="card-body">
                        <div *ngIf="localHolidays.length === 0" class="text-center text-muted py-3">
                          No holidays configured
                        </div>
                        <div *ngFor="let holiday of localHolidays; let i = index" class="holiday-item mb-2">
                          <div class="row g-2 align-items-center">
                            <div class="col-md-4">
                              <input 
                                type="text" 
                                class="form-control form-control-sm" 
                                placeholder="Holiday Name"
                                [(ngModel)]="holiday.name"
                                name="holidayName{{i}}">
                            </div>
                            <div class="col-md-3">
                              <input 
                                type="date" 
                                class="form-control form-control-sm"
                                [value]="formatDateForInput(holiday.date)"
                                (change)="updateHolidayDate(i, $event)"
                                name="holidayDate{{i}}">
                            </div>
                            <div class="col-md-3">
                              <div class="form-check">
                                <input 
                                  class="form-check-input" 
                                  type="checkbox" 
                                  [(ngModel)]="holiday.isRecurring"
                                  name="holidayRecurring{{i}}">
                                <label class="form-check-label">Recurring</label>
                              </div>
                            </div>
                            <div class="col-md-2">
                              <button type="button" class="btn btn-sm btn-outline-danger" (click)="removeHoliday(i)">
                                <i class="fas fa-trash"></i>
                              </button>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </form>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-outline-secondary" (click)="closeModal()">Cancel</button>
              <button type="button" class="btn btn-primary" (click)="saveBranch()" [disabled]="branchForm.invalid || isLoading">
                <span *ngIf="isLoading" class="spinner-border spinner-border-sm me-2"></span>
                {{ isEditMode ? 'Update Branch' : 'Create Branch' }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Modal Backdrop -->
      <div class="modal-backdrop fade" [class.show]="showModal" *ngIf="showModal"></div>
    </div>
  `,
  styles: [`
    .branch-management-container {
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

    .search-box {
      width: 250px;
    }

    .table th {
      font-weight: 600;
      color: var(--text-primary);
      background-color: var(--bg-secondary);
      border-bottom: 2px solid var(--gray-200);
    }

    .table-hover tbody tr:hover {
      background-color: var(--bg-tertiary);
    }

    .branch-icon {
      width: 32px;
      height: 32px;
      display: flex;
      align-items: center;
      justify-content: center;
      background-color: var(--primary-light);
      border-radius: 8px;
    }

    .modal.show {
      background-color: rgba(0, 0, 0, 0.5);
    }

    .modal-content {
      border-radius: 12px;
      border: none;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
    }

    .modal-header {
      border-bottom: 1px solid var(--gray-200);
      padding: 1.5rem;
    }

    .modal-title {
      font-weight: 600;
      color: var(--text-primary);
    }

    .form-label {
      font-weight: 500;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .form-control, .form-select {
      border: 2px solid var(--gray-200);
      border-radius: 8px;
      padding: 0.75rem 1rem;
      transition: all 0.15s ease-in-out;
    }

    .form-control:focus, .form-select:focus {
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

    .holiday-item {
      padding: 0.75rem;
      background-color: var(--bg-tertiary);
      border-radius: 8px;
    }

    .badge {
      font-size: 0.75rem;
      padding: 0.375rem 0.75rem;
    }
  `]
})
export class BranchManagementComponent implements OnInit {
  branches: Branch[] = [];
  filteredBranches: Branch[] = [];
  branchForm: FormGroup;
  showModal = false;
  isEditMode = false;
  isLoading = false;
  searchTerm = '';
  currentBranch: Branch | null = null;

  supportedCountries: string[] = [];
  supportedCurrencies: string[] = [];
  supportedTimeZones: string[] = [];
  availableTimeZones: string[] = [];
  localHolidays: LocalHoliday[] = [];

  constructor(
    private fb: FormBuilder,
    private branchService: BranchService
  ) {
    this.branchForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadBranches();
    this.loadReferenceData();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      organizationId: [1], // Assuming organization ID 1
      name: ['', [Validators.required]],
      country: ['', [Validators.required]],
      currency: ['', [Validators.required]],
      timeZone: ['', [Validators.required]],
      address: ['', [Validators.required]]
    });
  }

  private loadBranches(): void {
    this.branchService.getAllBranches().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.branches = response.data;
          this.filteredBranches = [...this.branches];
        }
      },
      error: (error) => {
        console.error('Error loading branches:', error);
      }
    });
  }

  private loadReferenceData(): void {
    // Load supported countries
    this.branchService.getSupportedCountries().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.supportedCountries = response.data;
        }
      }
    });

    // Load supported currencies
    this.branchService.getSupportedCurrencies().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.supportedCurrencies = response.data;
        }
      }
    });

    // Load supported time zones
    this.branchService.getSupportedTimeZones().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.supportedTimeZones = response.data;
          this.availableTimeZones = [...this.supportedTimeZones];
        }
      }
    });
  }

  filterBranches(): void {
    if (!this.searchTerm.trim()) {
      this.filteredBranches = [...this.branches];
    } else {
      const term = this.searchTerm.toLowerCase();
      this.filteredBranches = this.branches.filter(branch =>
        branch.name.toLowerCase().includes(term) ||
        branch.country.toLowerCase().includes(term) ||
        branch.address.toLowerCase().includes(term)
      );
    }
  }

  showCreateModal(): void {
    this.isEditMode = false;
    this.currentBranch = null;
    this.branchForm.reset();
    this.branchForm.patchValue({ organizationId: 1 });
    this.localHolidays = [];
    this.showModal = true;
  }

  editBranch(branch: Branch): void {
    this.isEditMode = true;
    this.currentBranch = branch;
    this.branchForm.patchValue({
      organizationId: branch.organizationId,
      name: branch.name,
      country: branch.country,
      currency: branch.currency,
      timeZone: branch.timeZone,
      address: branch.address
    });
    this.localHolidays = branch.localHolidays.map((holiday, index) => ({
      id: index,
      name: `Holiday ${index + 1}`,
      date: holiday,
      isRecurring: false,
      branchId: branch.id
    }));
    this.onCountryChange(); // Update available time zones
    this.showModal = true;
  }

  viewBranchDetails(branch: Branch): void {
    // Implementation for viewing branch details
    console.log('View branch details:', branch);
  }

  deleteBranch(branch: Branch): void {
    if (confirm(`Are you sure you want to delete the branch "${branch.name}"?`)) {
      this.branchService.deleteBranch(branch.id).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('Branch deleted successfully');
            this.loadBranches();
          }
        },
        error: (error) => {
          console.error('Error deleting branch:', error);
        }
      });
    }
  }

  onCountryChange(): void {
    const selectedCountry = this.branchForm.get('country')?.value;
    if (selectedCountry) {
      // Update available time zones based on country
      const countryTimeZones = this.branchService.getCountryTimeZones();
      this.availableTimeZones = countryTimeZones[selectedCountry] || this.supportedTimeZones;
      
      // Auto-select currency based on country
      const countryCurrencies = this.branchService.getCountryCurrencies();
      const currency = countryCurrencies[selectedCountry];
      if (currency) {
        this.branchForm.patchValue({ currency });
      }
    }
  }

  addHoliday(): void {
    this.localHolidays.push({
      id: this.localHolidays.length,
      name: '',
      date: new Date().toISOString().split('T')[0],
      isRecurring: false,
      branchId: 0
    });
  }

  removeHoliday(index: number): void {
    this.localHolidays.splice(index, 1);
  }

  updateHolidayDate(index: number, event: any): void {
    this.localHolidays[index].date = event.target.value;
  }

  formatDateForInput(date: string | Date): string {
    return new Date(date).toISOString().split('T')[0];
  }

  saveBranch(): void {
    if (this.branchForm.invalid) {
      this.branchForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const formData = {
      ...this.branchForm.value,
      localHolidays: this.localHolidays,
      complianceSettings: {}
    };

    if (this.isEditMode && this.currentBranch) {
      // Update existing branch
      this.branchService.updateBranch(this.currentBranch.id, formData).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('Branch updated successfully');
            this.loadBranches();
            this.closeModal();
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error updating branch:', error);
          this.isLoading = false;
        }
      });
    } else {
      // Create new branch
      this.branchService.createBranch(formData).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('Branch created successfully');
            this.loadBranches();
            this.closeModal();
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error creating branch:', error);
          this.isLoading = false;
        }
      });
    }
  }

  closeModal(): void {
    this.showModal = false;
    this.isEditMode = false;
    this.currentBranch = null;
    this.branchForm.reset();
    this.localHolidays = [];
  }

  getBranchEmployeeCount(branchId: number): number {
    // Mock data - in real implementation, this would come from the API
    const mockCounts: Record<number, number> = {
      1: 45,
      2: 32,
      3: 28
    };
    return mockCounts[branchId] || 0;
  }
}