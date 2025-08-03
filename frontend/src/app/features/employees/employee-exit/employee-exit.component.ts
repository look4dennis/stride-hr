import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { EmployeeService } from '../../../services/employee.service';
import { 
  Employee, 
  EmployeeExitProcess, 
  ExitType, 
  ExitStatus,
  AssetHandover,
  ClearanceStep,
  FinalSettlement
} from '../../../models/employee.models';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-employee-exit',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="page-header">
      <nav aria-label="breadcrumb">
        <ol class="breadcrumb">
          <li class="breadcrumb-item">
            <a routerLink="/employees" class="text-decoration-none">Employees</a>
          </li>
          <li class="breadcrumb-item">
            <a [routerLink]="['/employees', employee?.id]" class="text-decoration-none">
              {{ employee?.firstName }} {{ employee?.lastName }}
            </a>
          </li>
          <li class="breadcrumb-item active">Exit Process</li>
        </ol>
      </nav>
      <div class="d-flex justify-content-between align-items-center">
        <div>
          <h1>Employee Exit Process</h1>
          <p class="text-muted">Manage the employee exit workflow</p>
        </div>
        <button class="btn btn-outline-primary" (click)="goBack()">
          <i class="fas fa-arrow-left me-2"></i>Back to Profile
        </button>
      </div>
    </div>

    <div class="row" *ngIf="employee">
      <!-- Employee Info Card -->
      <div class="col-lg-4">
        <div class="card">
          <div class="card-body text-center">
            <img [src]="getProfilePhoto()" 
                 [alt]="employee.firstName + ' ' + employee.lastName"
                 class="profile-photo mb-3">
            <h5>{{ employee.firstName }} {{ employee.lastName }}</h5>
            <p class="text-muted">{{ employee.designation }}</p>
            <p class="text-muted">{{ employee.department }}</p>
            <span class="badge" [class]="'bg-' + getStatusColor(exitProcess.status)" *ngIf="exitProcess">
              {{ getStatusText(exitProcess.status) }}
            </span>
          </div>
        </div>

        <!-- Exit Summary -->
        <div class="card mt-4" *ngIf="exitProcess">
          <div class="card-header">
            <h6 class="card-title mb-0">Exit Summary</h6>
          </div>
          <div class="card-body">
            <div class="exit-summary">
              <div class="summary-item">
                <span class="summary-label">Exit Date:</span>
                <span class="summary-value">{{ formatDate(exitProcess.exitDate) }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Exit Type:</span>
                <span class="summary-value">{{ exitProcess.exitType }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Reason:</span>
                <span class="summary-value">{{ exitProcess.reason }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Assets to Return:</span>
                <span class="summary-value">{{ exitProcess.assetsToReturn.length }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Clearance Steps:</span>
                <span class="summary-value">
                  {{ getCompletedClearanceSteps() }} / {{ exitProcess.clearanceSteps.length }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Exit Process Details -->
      <div class="col-lg-8">
        <!-- Initiate Exit Form (if no exit process exists) -->
        <div class="card" *ngIf="!exitProcess && !loading">
          <div class="card-header">
            <h5 class="card-title mb-0">Initiate Exit Process</h5>
          </div>
          <div class="card-body">
            <form [formGroup]="exitForm" (ngSubmit)="initiateExit()">
              <div class="row g-3">
                <div class="col-md-6">
                  <label class="form-label">Exit Date</label>
                  <input type="date" class="form-control" formControlName="exitDate" required>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Exit Type</label>
                  <select class="form-select" formControlName="exitType" required>
                    <option value="">Select Exit Type</option>
                    <option value="Resignation">Resignation</option>
                    <option value="Termination">Termination</option>
                    <option value="Retirement">Retirement</option>
                    <option value="EndOfContract">End of Contract</option>
                  </select>
                </div>
                <div class="col-12">
                  <label class="form-label">Reason for Exit</label>
                  <textarea class="form-control" formControlName="reason" rows="3" 
                            placeholder="Please provide the reason for exit..."></textarea>
                </div>
                <div class="col-12">
                  <label class="form-label">Handover Notes (Optional)</label>
                  <textarea class="form-control" formControlName="handoverNotes" rows="3" 
                            placeholder="Any handover instructions or notes..."></textarea>
                </div>
                <div class="col-12">
                  <button type="submit" class="btn btn-primary" [disabled]="!exitForm.valid">
                    <i class="fas fa-play me-2"></i>Initiate Exit Process
                  </button>
                </div>
              </div>
            </form>
          </div>
        </div>

        <!-- Exit Process Tabs (if exit process exists) -->
        <div class="card" *ngIf="exitProcess">
          <div class="card-header">
            <ul class="nav nav-tabs card-header-tabs" role="tablist">
              <li class="nav-item">
                <a class="nav-link active" data-bs-toggle="tab" href="#clearance" role="tab">
                  Clearance Steps
                </a>
              </li>
              <li class="nav-item">
                <a class="nav-link" data-bs-toggle="tab" href="#assets" role="tab">
                  Asset Handover
                </a>
              </li>
              <li class="nav-item">
                <a class="nav-link" data-bs-toggle="tab" href="#settlement" role="tab">
                  Final Settlement
                </a>
              </li>
            </ul>
          </div>
          
          <div class="card-body">
            <div class="tab-content">
              <!-- Clearance Steps Tab -->
              <div class="tab-pane fade show active" id="clearance" role="tabpanel">
                <div class="clearance-steps">
                  <div class="step-item" 
                       *ngFor="let step of exitProcess.clearanceSteps"
                       [class.completed]="step.completed">
                    
                    <div class="step-header">
                      <div class="step-indicator">
                        <i class="fas fa-check" *ngIf="step.completed"></i>
                        <i class="fas fa-clock" *ngIf="!step.completed"></i>
                      </div>
                      
                      <div class="step-content">
                        <div class="step-title">{{ step.department }}</div>
                        <div class="step-description">{{ step.description }}</div>
                        <div class="step-meta" *ngIf="step.completed">
                          <small class="text-muted">
                            Completed on {{ formatDate(step.completedAt!) }}
                            <span *ngIf="step.notes"> - {{ step.notes }}</span>
                          </small>
                        </div>
                      </div>
                      
                      <div class="step-actions">
                        <button class="btn btn-sm" 
                                [class]="step.completed ? 'btn-outline-warning' : 'btn-outline-success'"
                                (click)="toggleClearanceStep(step)">
                          <i class="fas" [class]="step.completed ? 'fa-undo' : 'fa-check'"></i>
                          {{ step.completed ? 'Mark Incomplete' : 'Mark Complete' }}
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Asset Handover Tab -->
              <div class="tab-pane fade" id="assets" role="tabpanel">
                <div class="table-responsive">
                  <table class="table table-hover">
                    <thead>
                      <tr>
                        <th>Asset Name</th>
                        <th>Type</th>
                        <th>Condition</th>
                        <th>Status</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr *ngFor="let asset of exitProcess.assetsToReturn">
                        <td>
                          <strong>{{ asset.assetName }}</strong>
                          <br>
                          <small class="text-muted">ID: {{ asset.assetId }}</small>
                        </td>
                        <td>{{ asset.assetType }}</td>
                        <td>{{ asset.condition }}</td>
                        <td>
                          <span class="badge" [class]="asset.returnedAt ? 'bg-success' : 'bg-warning'">
                            {{ asset.returnedAt ? 'Returned' : 'Pending' }}
                          </span>
                        </td>
                        <td>
                          <button class="btn btn-sm" 
                                  [class]="asset.returnedAt ? 'btn-outline-warning' : 'btn-outline-success'"
                                  (click)="toggleAssetReturn(asset)">
                            <i class="fas" [class]="asset.returnedAt ? 'fa-undo' : 'fa-check'"></i>
                            {{ asset.returnedAt ? 'Mark Not Returned' : 'Mark Returned' }}
                          </button>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>

              <!-- Final Settlement Tab -->
              <div class="tab-pane fade" id="settlement" role="tabpanel">
                <div class="settlement-details" *ngIf="exitProcess.finalSettlement">
                  <div class="row g-3">
                    <div class="col-md-6">
                      <div class="settlement-item">
                        <label class="settlement-label">Basic Salary</label>
                        <div class="settlement-value">
                          {{ exitProcess.finalSettlement.currency }} {{ exitProcess.finalSettlement.basicSalary | number:'1.2-2' }}
                        </div>
                      </div>
                    </div>
                    <div class="col-md-6">
                      <div class="settlement-item">
                        <label class="settlement-label">Pending Leaves</label>
                        <div class="settlement-value">
                          {{ exitProcess.finalSettlement.pendingLeaves }} days
                        </div>
                      </div>
                    </div>
                    <div class="col-md-6">
                      <div class="settlement-item">
                        <label class="settlement-label">Leave Encashment</label>
                        <div class="settlement-value text-success">
                          +{{ exitProcess.finalSettlement.currency }} {{ exitProcess.finalSettlement.leaveEncashment | number:'1.2-2' }}
                        </div>
                      </div>
                    </div>
                    <div class="col-md-6">
                      <div class="settlement-item">
                        <label class="settlement-label">Bonus</label>
                        <div class="settlement-value text-success">
                          +{{ exitProcess.finalSettlement.currency }} {{ exitProcess.finalSettlement.bonus | number:'1.2-2' }}
                        </div>
                      </div>
                    </div>
                    <div class="col-md-6">
                      <div class="settlement-item">
                        <label class="settlement-label">Deductions</label>
                        <div class="settlement-value text-danger">
                          -{{ exitProcess.finalSettlement.currency }} {{ exitProcess.finalSettlement.deductions | number:'1.2-2' }}
                        </div>
                      </div>
                    </div>
                    <div class="col-md-6">
                      <div class="settlement-item total-amount">
                        <label class="settlement-label">Total Amount</label>
                        <div class="settlement-value">
                          {{ exitProcess.finalSettlement.currency }} {{ exitProcess.finalSettlement.totalAmount | number:'1.2-2' }}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="alert alert-info" *ngIf="!exitProcess.finalSettlement">
                  <i class="fas fa-info-circle me-2"></i>
                  Final settlement will be calculated once all clearance steps are completed.
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Actions Card -->
        <div class="card mt-4" *ngIf="exitProcess && exitProcess.status !== 'Completed'">
          <div class="card-header">
            <h6 class="card-title mb-0">Actions</h6>
          </div>
          <div class="card-body">
            <div class="d-flex gap-2">
              <button class="btn btn-success" 
                      (click)="completeExitProcess()"
                      [disabled]="!canCompleteExit()">
                <i class="fas fa-check-double me-2"></i>Complete Exit Process
              </button>
              <button class="btn btn-outline-primary" (click)="generateExitReport()">
                <i class="fas fa-file-pdf me-2"></i>Generate Report
              </button>
              <button class="btn btn-outline-danger" (click)="cancelExitProcess()">
                <i class="fas fa-times me-2"></i>Cancel Exit Process
              </button>
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
      <p class="mt-2 text-muted">Loading exit process information...</p>
    </div>
  `,
  styles: [`
    .profile-photo {
      width: 100px;
      height: 100px;
      border-radius: 50%;
      object-fit: cover;
      border: 3px solid #f8f9fa;
    }

    .exit-summary {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .summary-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.5rem;
      background: #f8f9fa;
      border-radius: 6px;
    }

    .summary-label {
      font-size: 0.875rem;
      color: #6c757d;
      font-weight: 500;
    }

    .summary-value {
      font-weight: 600;
      color: #2c3e50;
    }

    .clearance-steps {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .step-item {
      border: 2px solid #e9ecef;
      border-radius: 12px;
      transition: all 0.3s ease;
    }

    .step-item:hover {
      border-color: #007bff;
      box-shadow: 0 2px 8px rgba(0, 123, 255, 0.1);
    }

    .step-item.completed {
      border-color: #28a745;
      background-color: #f8fff9;
    }

    .step-header {
      display: flex;
      align-items: center;
      padding: 1.25rem;
    }

    .step-indicator {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-right: 1rem;
      flex-shrink: 0;
    }

    .step-item:not(.completed) .step-indicator {
      background-color: #ffc107;
      color: #000;
    }

    .step-item.completed .step-indicator {
      background-color: #28a745;
      color: white;
    }

    .step-content {
      flex-grow: 1;
      margin-right: 1rem;
    }

    .step-title {
      font-weight: 600;
      color: #2c3e50;
      margin-bottom: 0.25rem;
    }

    .step-description {
      color: #6c757d;
      font-size: 0.875rem;
      line-height: 1.4;
      margin-bottom: 0.25rem;
    }

    .step-meta {
      font-size: 0.75rem;
    }

    .step-actions {
      flex-shrink: 0;
    }

    .settlement-item {
      padding: 1rem;
      border: 1px solid #e9ecef;
      border-radius: 8px;
      background: #f8f9fa;
    }

    .settlement-item.total-amount {
      background: #e3f2fd;
      border-color: #2196f3;
    }

    .settlement-label {
      display: block;
      font-size: 0.875rem;
      color: #6c757d;
      margin-bottom: 0.25rem;
      font-weight: 500;
    }

    .settlement-value {
      font-size: 1.125rem;
      font-weight: 600;
      color: #2c3e50;
    }

    .total-amount .settlement-value {
      font-size: 1.25rem;
      color: #1976d2;
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
  `]
})
export class EmployeeExitComponent implements OnInit {
  employee: Employee | null = null;
  exitProcess: EmployeeExitProcess | null = null;
  exitForm: FormGroup;
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private employeeService: EmployeeService,
    private fb: FormBuilder,
    private notificationService: NotificationService
  ) {
    this.exitForm = this.fb.group({
      exitDate: ['', Validators.required],
      exitType: ['', Validators.required],
      reason: ['', Validators.required],
      handoverNotes: ['']
    });
  }

  ngOnInit(): void {
    const employeeId = this.route.snapshot.params['id'];
    if (employeeId) {
      this.loadEmployee(parseInt(employeeId));
      this.loadExitProcess(parseInt(employeeId));
    }
  }

  loadEmployee(id: number): void {
    // Mock employee data
    this.employee = {
      id: id,
      employeeId: 'EMP001',
      branchId: 1,
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@company.com',
      phone: '+1-555-0101',
      profilePhoto: '/assets/images/avatars/john-doe.jpg',
      dateOfBirth: '1990-05-15',
      joiningDate: '2020-01-15',
      designation: 'Senior Developer',
      department: 'Development',
      basicSalary: 75000,
      status: 'Active' as any,
      createdAt: '2020-01-15T00:00:00Z'
    };
  }

  loadExitProcess(employeeId: number): void {
    this.loading = true;

    // Mock exit process data (comment out to show initiate form)
    setTimeout(() => {
      this.exitProcess = {
        employeeId: employeeId,
        exitDate: '2024-02-15',
        reason: 'Better career opportunity',
        exitType: ExitType.Resignation,
        handoverNotes: 'All project documentation has been updated and shared with the team.',
        assetsToReturn: [
          {
            assetId: 1,
            assetName: 'MacBook Pro 16"',
            assetType: 'Laptop',
            condition: 'Good',
            notes: 'Minor scratches on the lid'
          },
          {
            assetId: 2,
            assetName: 'iPhone 13',
            assetType: 'Mobile Phone',
            condition: 'Excellent',
            returnedAt: '2024-02-10T10:00:00Z'
          },
          {
            assetId: 3,
            assetName: 'Access Card',
            assetType: 'Security',
            condition: 'Good'
          }
        ],
        clearanceSteps: [
          {
            id: '1',
            department: 'IT Department',
            description: 'Return all IT equipment and revoke system access',
            completed: true,
            completedBy: 5,
            completedAt: '2024-02-10T14:00:00Z',
            notes: 'All equipment returned and access revoked'
          },
          {
            id: '2',
            department: 'HR Department',
            description: 'Complete exit interview and documentation',
            completed: false
          },
          {
            id: '3',
            department: 'Finance Department',
            description: 'Process final settlement and clear dues',
            completed: false
          },
          {
            id: '4',
            department: 'Security',
            description: 'Return access cards and security clearance',
            completed: false
          }
        ],
        finalSettlement: {
          basicSalary: 75000,
          pendingLeaves: 5,
          leaveEncashment: 12500,
          bonus: 5000,
          deductions: 2500,
          totalAmount: 90000,
          currency: 'USD'
        },
        status: ExitStatus.InProgress
      };
      this.loading = false;
    }, 500);

    // Uncomment for production
    // this.employeeService.getEmployeeExitProcess(employeeId).subscribe({
    //   next: (exitProcess) => {
    //     this.exitProcess = exitProcess;
    //     this.loading = false;
    //   },
    //   error: (error) => {
    //     // No exit process found, show initiate form
    //     this.loading = false;
    //   }
    // });
  }

  initiateExit(): void {
    if (!this.exitForm.valid || !this.employee) return;

    const formValue = this.exitForm.value;
    const exitData: Partial<EmployeeExitProcess> = {
      exitDate: formValue.exitDate,
      exitType: formValue.exitType,
      reason: formValue.reason,
      handoverNotes: formValue.handoverNotes
    };

    // Mock initiation for development
    setTimeout(() => {
      this.loadExitProcess(this.employee!.id);
      this.notificationService.showSuccess('Exit process initiated successfully');
    }, 500);

    // Uncomment for production
    // this.employeeService.initiateExitProcess(this.employee.id, exitData).subscribe({
    //   next: (exitProcess) => {
    //     this.exitProcess = exitProcess;
    //     this.notificationService.showSuccess('Exit process initiated successfully');
    //   },
    //   error: (error) => {
    //     this.notificationService.showError('Failed to initiate exit process');
    //   }
    // });
  }

  toggleClearanceStep(step: ClearanceStep): void {
    step.completed = !step.completed;
    
    if (step.completed) {
      step.completedAt = new Date().toISOString();
      step.completedBy = 1; // Current user ID
    } else {
      step.completedAt = undefined;
      step.completedBy = undefined;
      step.notes = undefined;
    }

    const message = step.completed ? 
      `Clearance step "${step.department}" marked as completed` : 
      `Clearance step "${step.department}" marked as incomplete`;
    this.notificationService.showSuccess(message);
  }

  toggleAssetReturn(asset: AssetHandover): void {
    if (asset.returnedAt) {
      asset.returnedAt = undefined;
      asset.notes = undefined;
    } else {
      asset.returnedAt = new Date().toISOString();
    }

    const message = asset.returnedAt ? 
      `Asset "${asset.assetName}" marked as returned` : 
      `Asset "${asset.assetName}" marked as not returned`;
    this.notificationService.showSuccess(message);
  }

  canCompleteExit(): boolean {
    if (!this.exitProcess) return false;
    
    const allClearanceCompleted = this.exitProcess.clearanceSteps.every(step => step.completed);
    const allAssetsReturned = this.exitProcess.assetsToReturn.every(asset => asset.returnedAt);
    
    return allClearanceCompleted && allAssetsReturned;
  }

  completeExitProcess(): void {
    if (!this.canCompleteExit() || !this.exitProcess) return;

    this.exitProcess.status = ExitStatus.Completed;
    this.notificationService.showSuccess('Exit process completed successfully!');
  }

  generateExitReport(): void {
    this.notificationService.showInfo('Generating exit process report...');
  }

  cancelExitProcess(): void {
    if (confirm('Are you sure you want to cancel the exit process?')) {
      this.exitProcess = null;
      this.notificationService.showSuccess('Exit process cancelled');
    }
  }

  getCompletedClearanceSteps(): number {
    return this.exitProcess?.clearanceSteps.filter(step => step.completed).length || 0;
  }

  getStatusColor(status: ExitStatus): string {
    switch (status) {
      case ExitStatus.Initiated: return 'primary';
      case ExitStatus.InProgress: return 'warning';
      case ExitStatus.Completed: return 'success';
      case ExitStatus.Cancelled: return 'danger';
      default: return 'secondary';
    }
  }

  getStatusText(status: ExitStatus): string {
    switch (status) {
      case ExitStatus.Initiated: return 'Initiated';
      case ExitStatus.InProgress: return 'In Progress';
      case ExitStatus.Completed: return 'Completed';
      case ExitStatus.Cancelled: return 'Cancelled';
      default: return 'Unknown';
    }
  }

  getProfilePhoto(): string {
    return this.employee?.profilePhoto || '/assets/images/avatars/default-avatar.png';
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  goBack(): void {
    this.router.navigate(['/employees', this.employee?.id]);
  }
}