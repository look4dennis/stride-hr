import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { EmployeeService } from '../../../services/employee.service';
import { Employee, EmployeeSearchCriteria, PagedResult, EmployeeStatus } from '../../../models/employee.models';
import { NotificationService } from '../../../core/services/notification.service';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
    selector: 'app-employee-list',
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    template: `
    <div class="page-header d-flex justify-content-between align-items-center">
      <div>
        <h1>Employee Management</h1>
        <p class="text-muted">Manage your organization's employees</p>
      </div>
      <button class="btn btn-primary btn-rounded" (click)="openAddEmployeeModal()">
        <i class="fas fa-plus me-2"></i>Add Employee
      </button>
    </div>

    <!-- Search and Filter Section -->
    <div class="card mb-4">
      <div class="card-body">
        <form [formGroup]="searchForm" (ngSubmit)="onSearch()">
          <div class="row g-3">
            <div class="col-md-4">
              <label class="form-label">Search</label>
              <div class="input-group">
                <span class="input-group-text">
                  <i class="fas fa-search"></i>
                </span>
                <input 
                  type="text" 
                  class="form-control" 
                  formControlName="searchTerm"
                  placeholder="Search by name, email, or employee ID">
              </div>
            </div>
            <div class="col-md-2">
              <label class="form-label">Department</label>
              <select class="form-select" formControlName="department">
                <option value="">All Departments</option>
                <option *ngFor="let dept of departments" [value]="dept">{{ dept }}</option>
              </select>
            </div>
            <div class="col-md-2">
              <label class="form-label">Designation</label>
              <select class="form-select" formControlName="designation">
                <option value="">All Designations</option>
                <option *ngFor="let desig of designations" [value]="desig">{{ desig }}</option>
              </select>
            </div>
            <div class="col-md-2">
              <label class="form-label">Status</label>
              <select class="form-select" formControlName="status">
                <option value="">All Status</option>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
                <option value="OnLeave">On Leave</option>
                <option value="Terminated">Terminated</option>
                <option value="Resigned">Resigned</option>
              </select>
            </div>
            <div class="col-md-2 d-flex align-items-end">
              <button type="submit" class="btn btn-outline-primary me-2">
                <i class="fas fa-search me-1"></i>Search
              </button>
              <button type="button" class="btn btn-outline-secondary" (click)="clearFilters()">
                <i class="fas fa-times me-1"></i>Clear
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>

    <!-- Employee List -->
    <div class="card">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h5 class="card-title mb-0">
          Employees ({{ pagedResult?.totalCount || 0 }})
        </h5>
        <div class="d-flex align-items-center">
          <div class="me-3">
            <label class="form-label me-2 mb-0">View:</label>
            <div class="btn-group" role="group">
              <input type="radio" class="btn-check" name="viewMode" id="gridView" 
                     [checked]="viewMode === 'grid'" (change)="setViewMode('grid')">
              <label class="btn btn-outline-primary btn-sm" for="gridView">
                <i class="fas fa-th"></i>
              </label>
              <input type="radio" class="btn-check" name="viewMode" id="listView" 
                     [checked]="viewMode === 'list'" (change)="setViewMode('list')">
              <label class="btn btn-outline-primary btn-sm" for="listView">
                <i class="fas fa-list"></i>
              </label>
            </div>
          </div>
          <div>
            <select class="form-select form-select-sm" (change)="onPageSizeChange($event)">
              <option value="10">10 per page</option>
              <option value="25">25 per page</option>
              <option value="50">50 per page</option>
            </select>
          </div>
        </div>
      </div>
      
      <div class="card-body" *ngIf="loading">
        <div class="text-center py-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
          <p class="mt-2 text-muted">Loading employees...</p>
        </div>
      </div>

      <!-- Grid View -->
      <div class="card-body" *ngIf="!loading && viewMode === 'grid'">
        <div class="row g-4" *ngIf="employees.length > 0; else noEmployees">
          <div class="col-xl-3 col-lg-4 col-md-6" *ngFor="let employee of employees">
            <div class="employee-card">
              <div class="employee-avatar">
                <img [src]="getProfilePhoto(employee)" 
                     [alt]="employee.firstName + ' ' + employee.lastName"
                     class="avatar-img">
                <div class="status-badge" [class]="'status-' + employee.status.toLowerCase()">
                  {{ employee.status }}
                </div>
              </div>
              <div class="employee-info">
                <h6 class="employee-name">{{ employee.firstName }} {{ employee.lastName }}</h6>
                <p class="employee-id">{{ employee.employeeId }}</p>
                <p class="employee-designation">{{ employee.designation }}</p>
                <p class="employee-department">{{ employee.department }}</p>
                <p class="employee-contact">
                  <i class="fas fa-envelope me-1"></i>{{ employee.email }}
                </p>
              </div>
              <div class="employee-actions">
                <button class="btn btn-sm btn-outline-primary" (click)="viewEmployee(employee.id)">
                  <i class="fas fa-eye me-1"></i>View
                </button>
                <button class="btn btn-sm btn-outline-secondary" (click)="editEmployee(employee.id)">
                  <i class="fas fa-edit me-1"></i>Edit
                </button>
                <div class="dropdown d-inline">
                  <button class="btn btn-sm btn-outline-secondary dropdown-toggle" 
                          type="button" data-bs-toggle="dropdown">
                    <i class="fas fa-ellipsis-v"></i>
                  </button>
                  <ul class="dropdown-menu">
                    <li><a class="dropdown-item" (click)="viewOnboarding(employee.id)">
                      <i class="fas fa-user-plus me-2"></i>Onboarding
                    </a></li>
                    <li><a class="dropdown-item" (click)="initiateExit(employee.id)">
                      <i class="fas fa-sign-out-alt me-2"></i>Exit Process
                    </a></li>
                    <li><hr class="dropdown-divider"></li>
                    <li><a class="dropdown-item text-danger" (click)="deactivateEmployee(employee.id)">
                      <i class="fas fa-user-times me-2"></i>Deactivate
                    </a></li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- List View -->
      <div class="table-responsive" *ngIf="!loading && viewMode === 'list'">
        <table class="table table-hover" *ngIf="employees.length > 0; else noEmployees">
          <thead>
            <tr>
              <th (click)="sort('employeeId')" class="sortable">
                Employee ID
                <i class="fas fa-sort ms-1"></i>
              </th>
              <th (click)="sort('firstName')" class="sortable">
                Name
                <i class="fas fa-sort ms-1"></i>
              </th>
              <th>Email</th>
              <th>Department</th>
              <th>Designation</th>
              <th (click)="sort('joiningDate')" class="sortable">
                Joining Date
                <i class="fas fa-sort ms-1"></i>
              </th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let employee of employees">
              <td>
                <strong>{{ employee.employeeId }}</strong>
              </td>
              <td>
                <div class="d-flex align-items-center">
                  <img [src]="getProfilePhoto(employee)" 
                       [alt]="employee.firstName + ' ' + employee.lastName"
                       class="avatar-sm me-2">
                  <div>
                    <div class="fw-semibold">{{ employee.firstName }} {{ employee.lastName }}</div>
                    <small class="text-muted">{{ employee.phone }}</small>
                  </div>
                </div>
              </td>
              <td>{{ employee.email }}</td>
              <td>{{ employee.department }}</td>
              <td>{{ employee.designation }}</td>
              <td>{{ formatDate(employee.joiningDate) }}</td>
              <td>
                <span class="badge" [class]="'bg-' + getStatusColor(employee.status)">
                  {{ employee.status }}
                </span>
              </td>
              <td>
                <div class="btn-group" role="group">
                  <button class="btn btn-sm btn-outline-primary" (click)="viewEmployee(employee.id)">
                    <i class="fas fa-eye"></i>
                  </button>
                  <button class="btn btn-sm btn-outline-secondary" (click)="editEmployee(employee.id)">
                    <i class="fas fa-edit"></i>
                  </button>
                  <div class="dropdown">
                    <button class="btn btn-sm btn-outline-secondary dropdown-toggle" 
                            type="button" data-bs-toggle="dropdown">
                      <i class="fas fa-ellipsis-v"></i>
                    </button>
                    <ul class="dropdown-menu">
                      <li><a class="dropdown-item" (click)="viewOnboarding(employee.id)">
                        <i class="fas fa-user-plus me-2"></i>Onboarding
                      </a></li>
                      <li><a class="dropdown-item" (click)="initiateExit(employee.id)">
                        <i class="fas fa-sign-out-alt me-2"></i>Exit Process
                      </a></li>
                      <li><hr class="dropdown-divider"></li>
                      <li><a class="dropdown-item text-danger" (click)="deactivateEmployee(employee.id)">
                        <i class="fas fa-user-times me-2"></i>Deactivate
                      </a></li>
                    </ul>
                  </div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- No Employees Template -->
      <ng-template #noEmployees>
        <div class="card-body text-center py-5">
          <i class="fas fa-users text-muted mb-3" style="font-size: 3rem;"></i>
          <h5>No employees found</h5>
          <p class="text-muted">Try adjusting your search criteria or add a new employee.</p>
          <button class="btn btn-primary btn-rounded" (click)="openAddEmployeeModal()">
            <i class="fas fa-plus me-2"></i>Add First Employee
          </button>
        </div>
      </ng-template>

      <!-- Pagination -->
      <div class="card-footer" *ngIf="pagedResult && pagedResult.totalPages > 1">
        <nav aria-label="Employee pagination">
          <ul class="pagination justify-content-center mb-0">
            <li class="page-item" [class.disabled]="pagedResult.page === 1">
              <a class="page-link" (click)="goToPage(pagedResult.page - 1)">Previous</a>
            </li>
            <li class="page-item" 
                *ngFor="let page of getPageNumbers()" 
                [class.active]="page === pagedResult.page">
              <a class="page-link" (click)="goToPage(page)">{{ page }}</a>
            </li>
            <li class="page-item" [class.disabled]="pagedResult.page === pagedResult.totalPages">
              <a class="page-link" (click)="goToPage(pagedResult.page + 1)">Next</a>
            </li>
          </ul>
        </nav>
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

    .employee-card {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
      border: 1px solid #e9ecef;
      transition: all 0.3s ease;
      height: 100%;
      display: flex;
      flex-direction: column;
    }

    .employee-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
    }

    .employee-avatar {
      text-align: center;
      margin-bottom: 1rem;
      position: relative;
    }

    .avatar-img {
      width: 80px;
      height: 80px;
      border-radius: 50%;
      object-fit: cover;
      border: 3px solid #f8f9fa;
    }

    .avatar-sm {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      object-fit: cover;
    }

    .status-badge {
      position: absolute;
      bottom: 0;
      right: calc(50% - 40px);
      padding: 2px 8px;
      border-radius: 12px;
      font-size: 0.75rem;
      font-weight: 500;
      color: white;
    }

    .status-active { background-color: #28a745; }
    .status-inactive { background-color: #6c757d; }
    .status-onleave { background-color: #ffc107; color: #000; }
    .status-terminated { background-color: #dc3545; }
    .status-resigned { background-color: #fd7e14; }

    .employee-info {
      flex-grow: 1;
      text-align: center;
      margin-bottom: 1rem;
    }

    .employee-name {
      font-weight: 600;
      color: #2c3e50;
      margin-bottom: 0.25rem;
    }

    .employee-id {
      font-size: 0.875rem;
      color: #6c757d;
      margin-bottom: 0.5rem;
    }

    .employee-designation {
      font-weight: 500;
      color: #495057;
      margin-bottom: 0.25rem;
    }

    .employee-department {
      font-size: 0.875rem;
      color: #6c757d;
      margin-bottom: 0.5rem;
    }

    .employee-contact {
      font-size: 0.875rem;
      color: #6c757d;
      margin-bottom: 0;
    }

    .employee-actions {
      display: flex;
      gap: 0.5rem;
      justify-content: center;
    }

    .sortable {
      cursor: pointer;
      user-select: none;
    }

    .sortable:hover {
      background-color: #f8f9fa;
    }

    .btn-rounded {
      border-radius: 50px;
    }

    .input-group-text {
      background-color: #f8f9fa;
      border-color: #dee2e6;
    }

    .page-link {
      cursor: pointer;
    }
  `]
})
export class EmployeeListComponent implements OnInit {
  employees: Employee[] = [];
  pagedResult: PagedResult<Employee> | null = null;
  searchForm: FormGroup;
  departments: string[] = [];
  designations: string[] = [];
  viewMode: 'grid' | 'list' = 'grid';
  loading = false;
  currentSort = { field: '', direction: 'asc' as 'asc' | 'desc' };

  constructor(
    private employeeService: EmployeeService,
    private fb: FormBuilder,
    private router: Router,
    private notificationService: NotificationService,
    private loadingService: LoadingService
  ) {
    this.searchForm = this.fb.group({
      searchTerm: [''],
      department: [''],
      designation: [''],
      status: ['']
    });
  }

  ngOnInit(): void {
    this.loadEmployees();
    this.loadFilterOptions();
  }

  loadEmployees(criteria?: EmployeeSearchCriteria): void {
    this.loading = true;
    
    // For development, use mock data
    setTimeout(() => {
      const mockResult = this.employeeService.getMockEmployees();
      this.pagedResult = mockResult;
      this.employees = mockResult.items;
      this.loading = false;
    }, 500);

    // Uncomment for production
    // this.employeeService.getEmployees(criteria).subscribe({
    //   next: (result) => {
    //     this.pagedResult = result;
    //     this.employees = result.items;
    //     this.loading = false;
    //   },
    //   error: (error) => {
    //     this.notificationService.showError('Failed to load employees');
    //     this.loading = false;
    //   }
    // });
  }

  loadFilterOptions(): void {
    // Mock data for development
    this.departments = ['Development', 'Human Resources', 'Marketing', 'Sales', 'Finance'];
    this.designations = ['Senior Developer', 'Junior Developer', 'Development Manager', 'HR Manager', 'Marketing Manager'];

    // Uncomment for production
    // this.employeeService.getDepartments().subscribe(depts => this.departments = depts);
    // this.employeeService.getDesignations().subscribe(desigs => this.designations = desigs);
  }

  onSearch(): void {
    const formValue = this.searchForm.value;
    const criteria: EmployeeSearchCriteria = {
      searchTerm: formValue.searchTerm || undefined,
      department: formValue.department || undefined,
      designation: formValue.designation || undefined,
      status: formValue.status || undefined,
      page: 1,
      pageSize: 10,
      sortBy: this.currentSort.field || undefined,
      sortDirection: this.currentSort.direction
    };
    
    this.loadEmployees(criteria);
  }

  clearFilters(): void {
    this.searchForm.reset();
    this.currentSort = { field: '', direction: 'asc' };
    this.loadEmployees();
  }

  setViewMode(mode: 'grid' | 'list'): void {
    this.viewMode = mode;
  }

  sort(field: string): void {
    if (this.currentSort.field === field) {
      this.currentSort.direction = this.currentSort.direction === 'asc' ? 'desc' : 'asc';
    } else {
      this.currentSort.field = field;
      this.currentSort.direction = 'asc';
    }
    this.onSearch();
  }

  onPageSizeChange(event: any): void {
    const pageSize = parseInt(event.target.value);
    const criteria: EmployeeSearchCriteria = {
      ...this.searchForm.value,
      page: 1,
      pageSize: pageSize
    };
    this.loadEmployees(criteria);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= (this.pagedResult?.totalPages || 1)) {
      const criteria: EmployeeSearchCriteria = {
        ...this.searchForm.value,
        page: page,
        pageSize: this.pagedResult?.pageSize || 10
      };
      this.loadEmployees(criteria);
    }
  }

  getPageNumbers(): number[] {
    if (!this.pagedResult) return [];
    
    const totalPages = this.pagedResult.totalPages;
    const currentPage = this.pagedResult.page;
    const pages: number[] = [];
    
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);
    
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  getProfilePhoto(employee: Employee): string {
    return employee.profilePhoto || '/assets/images/avatars/default-avatar.png';
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

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  // Action methods
  openAddEmployeeModal(): void {
    this.router.navigate(['/employees/add']);
  }

  viewEmployee(id: number): void {
    this.router.navigate(['/employees', id]);
  }

  editEmployee(id: number): void {
    this.router.navigate(['/employees', id, 'edit']);
  }

  viewOnboarding(id: number): void {
    this.router.navigate(['/employees', id, 'onboarding']);
  }

  initiateExit(id: number): void {
    this.router.navigate(['/employees', id, 'exit']);
  }

  deactivateEmployee(id: number): void {
    if (confirm('Are you sure you want to deactivate this employee?')) {
      this.employeeService.deactivateEmployee(id).subscribe({
        next: () => {
          this.notificationService.showSuccess('Employee deactivated successfully');
          this.loadEmployees();
        },
        error: () => {
          this.notificationService.showError('Failed to deactivate employee');
        }
      });
    }
  }
}