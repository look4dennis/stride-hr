import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EnhancedEmployeeService } from '../../../services/enhanced-employee.service';
import { Employee, EmployeeExitProcess, ExitType, ClearanceStep } from '../../../models/employee.models';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-employee-exit',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
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
            <li class="breadcrumb-item">
              <a (click)="navigateToEmployeeProfile()" class="text-decoration-none cursor-pointer">
                {{ employee?.firstName }} {{ employee?.lastName }}
              </a>
            </li>
            <li class="breadcrumb-item active" aria-current="page">Exit Process</li>
          </ol>
        </nav>
        <h1>Employee Exit Process</h1>
        <p class="text-muted" *ngIf="employee">{{ employee.employeeId }} - {{ employee.designation }}</p>
      </div>
      <button class="btn btn-outline-secondary" (click)="navigateToEmployeeProfile()">
        <i class="fas fa-arrow-left me-2"></i>Back to Profile
      </button>
    </div>

    <div class="row">
      <div class="col-lg-8 mx-auto">
        <!-- Exit Form (if not initiated) -->
        <div class="card" *ngIf="!exitProcess">
          <div class="card-header">
            <h5 class="card-title mb-0">
              <i class="fas fa-sign-out-alt me-2"></i>Initiate Exit Process
            </h5>
          </div>
          <div class="card-body">
            <form [formGroup]="exitForm" (ngSubmit)="initiateExit()">
              <div class="row g-3">
                <div class="col-md-6">
                  <label class="form-label">Exit Date <span class="text-danger">*</span></label>
                  <input type="date" 
                         class="form-control" 
                         formControlName="exitDate"
                         [class.is-invalid]="isFieldInvalid('exitDate')">
                  <div class="invalid-feedback" *ngIf="isFieldInvalid('exitDate')">
                    Exit date is required
                  </div>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Exit Type <span class="text-danger">*</span></label>
                  <select class="form-select" 
                          formControlName="exitType"
                          [class.is-invalid]="isFieldInvalid('exitType')">
                    <option value="">Select Exit Type</option>
                    <option value="Resignation">Resignation</option>
                    <option value="Termination">Termination</option>
                    <option value="Retirement">Retirement</option>
                    <option value="EndOfContract">End of Contract</option>
                  </select>
                  <div class="invalid-feedback" *ngIf="isFieldInvalid('exitType')">
                    Please select an exit type
                  </div>
                </div>
              </div>

              <div class="mb-3 mt-3">
                <label class="form-label">Reason <span class="text-danger">*</span></label>
                <textarea class="form-control" 
                          formControlName="reason"
                          rows="3"
                          [class.is-invalid]="isFieldInvalid('reason')"
                          placeholder="Please provide the reason for exit"></textarea>
                <div class="invalid-feedback" *ngIf="isFieldInvalid('reason')">
                  Reason is required
                </div>
              </div>

              <div class="mb-3">
                <label class="form-label">Handover Notes</label>
                <textarea class="form-control" 
                          formControlName="handoverNotes"
                          rows="4"
                          placeholder="Provide details about work handover, ongoing projects, etc."></textarea>
              </div>

              <div class="d-flex justify-content-end gap-3">
                <button type="button" 
                        class="btn btn-outline-secondary" 
                        (click)="navigateToEmployeeProfile()">
                  Cancel
                </button>
                <button type="submit" 
                        class="btn btn-warning"
                        [disabled]="!exitForm.valid || loading">
                  <span *ngIf="loading" class="spinner-border spinner-border-sm me-2" role="status"></span>
                  <i *ngIf="!loading" class="fas fa-sign-out-alt me-2"></i>
                  {{ loading ? 'Initiating...' : 'Initiate Exit Process' }}
                </button>
              </div>
            </form>
          </div>
        </div>

        <!-- Exit Process Status (if initiated) -->
        <div class="card" *ngIf="exitProcess">
          <div class="card-header">
            <div class="d-flex justify-content-between align-items-center">
              <h5 class="card-title mb-0">
                <i class="fas fa-sign-out-alt me-2"></i>Exit Process Status
              </h5>
              <span class="badge" 
                    [class]="'bg-' + getStatusColor(exitProcess.status)">
                {{ exitProcess.status }}
              </span>
            </div>
          </div>
          
          <div class="card-body">
            <!-- Exit Details -->
            <div class="row mb-4">
              <div class="col-md-6">
                <strong>Exit Date:</strong>
                <p>{{ formatDate(exitProcess.exitDate) }}</p>
              </div>
              <div class="col-md-6">
                <strong>Exit Type:</strong>
                <p>{{ exitProcess.exitType }}</p>
              </div>
              <div class="col-12">
                <strong>Reason:</strong>
                <p>{{ exitProcess.reason }}</p>
              </div>
              <div class="col-12" *ngIf="exitProcess.handoverNotes">
                <strong>Handover Notes:</strong>
                <p>{{ exitProcess.handoverNotes }}</p>
              </div>
            </div>

            <!-- Clearance Steps -->
            <div class="clearance-section">
              <h6 class="mb-3">
                <i class="fas fa-tasks me-2"></i>Clearance Steps
              </h6>
              
              <div class="clearance-steps">
                <div class="clearance-item" 
                     *ngFor="let step of exitProcess.clearanceSteps"
                     [class.completed]="step.completed">
                  
                  <div class="clearance-indicator">
                    <i class="fas fa-check" *ngIf="step.completed"></i>
                    <i class="fas fa-clock" *ngIf="!step.completed"></i>
                  </div>
                  
                  <div class="clearance-content">
                    <div class="clearance-header">
                      <h6 class="clearance-title">{{ step.department }}</h6>
                      <span class="badge" 
                            [class]="step.completed ? 'bg-success' : 'bg-warning'">
                        {{ step.completed ? 'Completed' : 'Pending' }}
                      </span>
                    </div>
                    
                    <p class="clearance-description">{{ step.description }}</p>
                    
                    <div class="clearance-details" *ngIf="step.completed">
                      <small class="text-muted">
                        Completed on {{ formatDate(step.completedAt!) }}
                        <span *ngIf="step.notes"> - {{ step.notes }}</span>
                      </small>
                    </div>
                    
                    <div class="clearance-actions" *ngIf="!step.completed">
                      <button class="btn btn-success btn-sm" 
                              (click)="completeClearanceStep(step)"
                              [disabled]="loading">
                        <i class="fas fa-check me-1"></i>Mark as Complete
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Final Settlement (if applicable) -->
            <div class="final-settlement" *ngIf="exitProcess.finalSettlement">
              <h6 class="mb-3">
                <i class="fas fa-calculator me-2"></i>Final Settlement
              </h6>
              
              <div class="settlement-details">
                <div class="row">
                  <div class="col-md-6">
                    <div class="settlement-item">
                      <span>Basic Salary:</span>
                      <span>{{ exitProcess.finalSettlement.currency }} {{ exitProcess.finalSettlement.basicSalary }}</span>
                    </div>
                    <div class="settlement-item">
                      <span>Leave Encashment:</span>
                      <span>{{ exitProcess.finalSettlement.currency }} {{ exitProcess.finalSettlement.leaveEncashment }}</span>
                    </div>
                    <div class="settlement-item">
                      <span>Bonus:</span>
                      <span>{{ exitProcess.finalSettlement.currency }} {{ exitProcess.finalSettlement.bonus }}</span>
                    </div>
                  </div>
                  <div class="col-md-6">
                    <div class="settlement-item">
                      <span>Deductions:</span>
                      <span class="text-danger">-{{ exitProcess.finalSettlement.currency }} {{ exitProcess.finalSettlement.deductions }}</span>
                    </div>
                    <div class="settlement-item total">
                      <span><strong>Total Amount:</strong></span>
                      <span><strong>{{ exitProcess.finalSettlement.currency }} {{ exitProcess.finalSettlement.totalAmount }}</strong></span>
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
    <div class="text-center py-5" *ngIf="loading && !exitProcess">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="mt-2 text-muted">Loading exit process information...</p>
    </div>
  `,
  styles: [`
    .breadcrumb {
      background: none;
      padding: 0;
      margin-bottom: 0.5rem;
    }

    .cursor-pointer {
      cursor: pointer;
    }

    .clearance-steps {
      position: relative;
    }

    .clearance-item {
      display: flex;
      margin-bottom: 2rem;
      position: relative;
    }

    .clearance-item:not(:last-child)::after {
      content: '';
      position: absolute;
      left: 20px;
      top: 50px;
      bottom: -32px;
      width: 2px;
      background: #e9ecef;
    }

    .clearance-item.completed::after {
      background: #28a745;
    }

    .clearance-indicator {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-right: 1rem;
      flex-shrink: 0;
      position: relative;
      z-index: 1;
    }

    .clearance-item:not(.completed) .clearance-indicator {
      background: #ffc107;
      color: #000;
    }

    .clearance-item.completed .clearance-indicator {
      background: #28a745;
      color: white;
    }

    .clearance-content {
      flex: 1;
      padding-top: 0.25rem;
    }

    .clearance-header {
      display: flex;
      justify-content: between;
      align-items: center;
      margin-bottom: 0.5rem;
    }

    .clearance-title {
      margin: 0;
      color: #495057;
      font-weight: 600;
    }

    .clearance-item.completed .clearance-title {
      color: #28a745;
    }

    .clearance-description {
      color: #6c757d;
      margin-bottom: 1rem;
      font-size: 0.9rem;
    }

    .clearance-actions {
      margin-top: 1rem;
    }

    .settlement-details {
      background: #f8f9fa;
      border-radius: 8px;
      padding: 1.5rem;
    }

    .settlement-item {
      display: flex;
      justify-content: space-between;
      margin-bottom: 0.75rem;
      padding-bottom: 0.5rem;
      border-bottom: 1px solid #e9ecef;
    }

    .settlement-item.total {
      border-bottom: 2px solid #007bff;
      margin-top: 1rem;
      padding-top: 1rem;
    }

    .settlement-item:last-child {
      margin-bottom: 0;
    }

    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
      }
      
      .page-header .btn {
        width: 100%;
      }
      
      .clearance-item {
        flex-direction: column;
        text-align: center;
      }
      
      .clearance-indicator {
        margin: 0 auto 1rem auto;
      }
      
      .clearance-item::after {
        display: none;
      }
    }
  `]
})
export class EmployeeExitComponent implements OnInit {
  employee: Employee | null = null;
  exitProcess: EmployeeExitProcess | null = null;
  exitForm: FormGroup;
  loading = false;
  employeeId: number = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private employeeService: EnhancedEmployeeService,
    private notificationService: NotificationService
  ) {
    this.exitForm = this.createExitForm();
  }

  ngOnInit(): void {
    this.employeeId = parseInt(this.route.snapshot.params['id']);
    if (this.employeeId) {
      this.loadEmployee();
      this.loadExitProcess();
    }
  }

  private createExitForm(): FormGroup {
    return this.fb.group({
      exitDate: ['', Validators.required],
      exitType: ['', Validators.required],
      reason: ['', Validators.required],
      handoverNotes: ['']
    });
  }

  loadEmployee(): void {
    this.employeeService.getEmployeeById(this.employeeId).subscribe({
      next: (employee) => {
        this.employee = employee;
      },
      error: (error) => {
        this.notificationService.showError('Failed to load employee information');
      }
    });
  }

  loadExitProcess(): void {
    this.loading = true;
    
    this.employeeService.getEmployeeExitProcess(this.employeeId).subscribe({
      next: (exitProcess) => {
        this.exitProcess = exitProcess;
        this.loading = false;
      },
      error: (error) => {
        // Exit process not found - this is expected for new exits
        this.loading = false;
      }
    });
  }

  initiateExit(): void {
    if (!this.exitForm.valid) {
      this.markFormGroupTouched();
      return;
    }

    this.loading = true;
    const formValue = this.exitForm.value;
    
    const exitData: Partial<EmployeeExitProcess> = {
      exitDate: formValue.exitDate,
      exitType: formValue.exitType as ExitType,
      reason: formValue.reason,
      handoverNotes: formValue.handoverNotes || undefined
    };

    this.employeeService.initiateExitProcess(this.employeeId, exitData).subscribe({
      next: (exitProcess) => {
        this.exitProcess = exitProcess;
        this.notificationService.showSuccess('Exit process initiated successfully');
        this.loading = false;
      },
      error: (error) => {
        this.notificationService.showError('Failed to initiate exit process');
        this.loading = false;
      }
    });
  }

  completeClearanceStep(step: ClearanceStep): void {
    // This would typically update the specific clearance step
    step.completed = true;
    step.completedAt = new Date().toISOString();
    this.notificationService.showSuccess(`${step.department} clearance completed`);
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.exitForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  private markFormGroupTouched(): void {
    Object.keys(this.exitForm.controls).forEach(key => {
      const control = this.exitForm.get(key);
      if (control) {
        control.markAsTouched();
      }
    });
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Initiated': return 'warning';
      case 'InProgress': return 'info';
      case 'Completed': return 'success';
      case 'Cancelled': return 'secondary';
      default: return 'secondary';
    }
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  navigateToEmployeeList(): void {
    this.router.navigate(['/employees']);
  }

  navigateToEmployeeProfile(): void {
    this.router.navigate(['/employees', this.employeeId]);
  }
}