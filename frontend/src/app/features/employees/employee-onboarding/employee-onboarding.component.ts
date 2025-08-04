import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { EmployeeService } from '../../../services/employee.service';
import { Employee, EmployeeOnboarding, EmployeeOnboardingStep, OnboardingStatus } from '../../../models/employee.models';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
    selector: 'app-employee-onboarding',
    imports: [CommonModule, RouterModule],
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
          <li class="breadcrumb-item active">Onboarding</li>
        </ol>
      </nav>
      <div class="d-flex justify-content-between align-items-center">
        <div>
          <h1>Employee Onboarding</h1>
          <p class="text-muted">Track and manage the onboarding process</p>
        </div>
        <button class="btn btn-outline-primary" (click)="goBack()">
          <i class="fas fa-arrow-left me-2"></i>Back to Profile
        </button>
      </div>
    </div>

    <div class="row" *ngIf="employee && onboarding">
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
            <span class="badge" [class]="'bg-' + getStatusColor(onboarding.status)">
              {{ getStatusText(onboarding.status) }}
            </span>
          </div>
        </div>

        <!-- Progress Summary -->
        <div class="card mt-4">
          <div class="card-header">
            <h6 class="card-title mb-0">Progress Summary</h6>
          </div>
          <div class="card-body">
            <div class="progress-summary">
              <div class="progress mb-3">
                <div class="progress-bar" 
                     [style.width.%]="onboarding.overallProgress"
                     [class]="'bg-' + getProgressColor(onboarding.overallProgress)">
                  {{ onboarding.overallProgress }}%
                </div>
              </div>
              
              <div class="progress-stats">
                <div class="stat-item">
                  <span class="stat-label">Completed:</span>
                  <span class="stat-value text-success">{{ getCompletedSteps() }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">Remaining:</span>
                  <span class="stat-value text-warning">{{ getRemainingSteps() }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">Total Steps:</span>
                  <span class="stat-value">{{ onboarding.steps.length }}</span>
                </div>
              </div>

              <div class="timeline-info mt-3">
                <div class="timeline-item">
                  <i class="fas fa-play-circle text-primary me-2"></i>
                  <span class="text-muted">Started: {{ formatDate(onboarding.startedAt) }}</span>
                </div>
                <div class="timeline-item" *ngIf="onboarding.completedAt">
                  <i class="fas fa-check-circle text-success me-2"></i>
                  <span class="text-muted">Completed: {{ formatDate(onboarding.completedAt) }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Onboarding Steps -->
      <div class="col-lg-8">
        <div class="card">
          <div class="card-header d-flex justify-content-between align-items-center">
            <h5 class="card-title mb-0">Onboarding Checklist</h5>
            <div class="btn-group" role="group">
              <button class="btn btn-sm btn-outline-primary" 
                      [class.active]="viewMode === 'all'"
                      (click)="setViewMode('all')">
                All Steps
              </button>
              <button class="btn btn-sm btn-outline-primary" 
                      [class.active]="viewMode === 'pending'"
                      (click)="setViewMode('pending')">
                Pending
              </button>
              <button class="btn btn-sm btn-outline-primary" 
                      [class.active]="viewMode === 'completed'"
                      (click)="setViewMode('completed')">
                Completed
              </button>
            </div>
          </div>
          
          <div class="card-body">
            <div class="onboarding-steps">
              <div class="step-item" 
                   *ngFor="let step of getFilteredSteps(); trackBy: trackByStepId"
                   [class.completed]="step.completed"
                   [class.required]="step.required">
                
                <div class="step-header" (click)="toggleStep(step)">
                  <div class="step-indicator">
                    <div class="step-number" *ngIf="!step.completed">{{ step.order }}</div>
                    <i class="fas fa-check" *ngIf="step.completed"></i>
                  </div>
                  
                  <div class="step-content">
                    <div class="step-title">
                      {{ step.title }}
                      <span class="required-badge" *ngIf="step.required">Required</span>
                    </div>
                    <div class="step-description">{{ step.description }}</div>
                  </div>
                  
                  <div class="step-actions">
                    <button class="btn btn-sm" 
                            [class]="step.completed ? 'btn-outline-warning' : 'btn-outline-success'"
                            (click)="toggleStepCompletion(step, $event)">
                      <i class="fas" [class]="step.completed ? 'fa-undo' : 'fa-check'"></i>
                      {{ step.completed ? 'Mark Incomplete' : 'Mark Complete' }}
                    </button>
                  </div>
                </div>
              </div>
            </div>

            <!-- No Steps Message -->
            <div class="text-center py-5" *ngIf="getFilteredSteps().length === 0">
              <i class="fas fa-tasks text-muted mb-3" style="font-size: 3rem;"></i>
              <h5>No {{ viewMode === 'all' ? '' : viewMode }} steps found</h5>
              <p class="text-muted">
                <span *ngIf="viewMode === 'pending'">All onboarding steps have been completed!</span>
                <span *ngIf="viewMode === 'completed'">No steps have been completed yet.</span>
                <span *ngIf="viewMode === 'all'">No onboarding steps are configured.</span>
              </p>
            </div>
          </div>
        </div>

        <!-- Actions Card -->
        <div class="card mt-4" *ngIf="onboarding.status !== 'Completed'">
          <div class="card-header">
            <h6 class="card-title mb-0">Actions</h6>
          </div>
          <div class="card-body">
            <div class="d-flex gap-2">
              <button class="btn btn-success" 
                      (click)="completeOnboarding()"
                      [disabled]="!canCompleteOnboarding()">
                <i class="fas fa-check-double me-2"></i>Complete Onboarding
              </button>
              <button class="btn btn-outline-primary" (click)="sendReminder()">
                <i class="fas fa-bell me-2"></i>Send Reminder
              </button>
              <button class="btn btn-outline-info" (click)="exportProgress()">
                <i class="fas fa-download me-2"></i>Export Progress
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
      <p class="mt-2 text-muted">Loading onboarding information...</p>
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

    .progress-stats {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .stat-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .stat-label {
      font-size: 0.875rem;
      color: #6c757d;
    }

    .stat-value {
      font-weight: 600;
    }

    .timeline-item {
      display: flex;
      align-items: center;
      margin-bottom: 0.5rem;
      font-size: 0.875rem;
    }

    .onboarding-steps {
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

    .step-item.required {
      border-left: 4px solid #ffc107;
    }

    .step-header {
      display: flex;
      align-items: center;
      padding: 1.25rem;
      cursor: pointer;
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
      background-color: #007bff;
      color: white;
    }

    .step-item.completed .step-indicator {
      background-color: #28a745;
      color: white;
    }

    .step-number {
      font-weight: 600;
      font-size: 0.875rem;
    }

    .step-content {
      flex-grow: 1;
      margin-right: 1rem;
    }

    .step-title {
      font-weight: 600;
      color: #2c3e50;
      margin-bottom: 0.25rem;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .step-description {
      color: #6c757d;
      font-size: 0.875rem;
      line-height: 1.4;
    }

    .required-badge {
      background-color: #ffc107;
      color: #000;
      font-size: 0.75rem;
      padding: 2px 6px;
      border-radius: 4px;
      font-weight: 500;
    }

    .step-actions {
      flex-shrink: 0;
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

    .btn-group .btn.active {
      background-color: #007bff;
      border-color: #007bff;
      color: white;
    }
  `]
})
export class EmployeeOnboardingComponent implements OnInit {
  employee: Employee | null = null;
  onboarding: EmployeeOnboarding | null = null;
  loading = false;
  viewMode: 'all' | 'pending' | 'completed' = 'all';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private employeeService: EmployeeService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    const employeeId = this.route.snapshot.params['id'];
    if (employeeId) {
      this.loadEmployee(parseInt(employeeId));
      this.loadOnboarding(parseInt(employeeId));
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

  loadOnboarding(employeeId: number): void {
    this.loading = true;

    // Mock onboarding data
    setTimeout(() => {
      this.onboarding = {
        employeeId: employeeId,
        steps: [
          {
            id: '1',
            title: 'Complete Personal Information',
            description: 'Fill out all required personal details and emergency contacts',
            completed: true,
            required: true,
            order: 1
          },
          {
            id: '2',
            title: 'Submit Required Documents',
            description: 'Upload ID proof, address proof, and educational certificates',
            completed: true,
            required: true,
            order: 2
          },
          {
            id: '3',
            title: 'IT Setup and Account Creation',
            description: 'Set up email account, system access, and receive equipment',
            completed: false,
            required: true,
            order: 3
          },
          {
            id: '4',
            title: 'HR Orientation Session',
            description: 'Attend company orientation and HR policy briefing',
            completed: false,
            required: true,
            order: 4
          },
          {
            id: '5',
            title: 'Department Introduction',
            description: 'Meet team members and understand role responsibilities',
            completed: false,
            required: true,
            order: 5
          },
          {
            id: '6',
            title: 'Training Program Enrollment',
            description: 'Enroll in mandatory training programs and certifications',
            completed: false,
            required: false,
            order: 6
          },
          {
            id: '7',
            title: 'Buddy Assignment',
            description: 'Get assigned a workplace buddy for guidance and support',
            completed: false,
            required: false,
            order: 7
          },
          {
            id: '8',
            title: 'First Week Check-in',
            description: 'Complete first week feedback and address any concerns',
            completed: false,
            required: true,
            order: 8
          }
        ],
        overallProgress: 25,
        startedAt: '2024-01-15T09:00:00Z',
        status: OnboardingStatus.InProgress
      };
      this.loading = false;
    }, 500);

    // Uncomment for production
    // this.employeeService.getEmployeeOnboarding(employeeId).subscribe({
    //   next: (onboarding) => {
    //     this.onboarding = onboarding;
    //     this.loading = false;
    //   },
    //   error: (error) => {
    //     this.notificationService.showError('Failed to load onboarding information');
    //     this.loading = false;
    //   }
    // });
  }

  setViewMode(mode: 'all' | 'pending' | 'completed'): void {
    this.viewMode = mode;
  }

  getFilteredSteps(): EmployeeOnboardingStep[] {
    if (!this.onboarding) return [];

    switch (this.viewMode) {
      case 'pending':
        return this.onboarding.steps.filter(step => !step.completed);
      case 'completed':
        return this.onboarding.steps.filter(step => step.completed);
      default:
        return this.onboarding.steps.sort((a, b) => a.order - b.order);
    }
  }

  toggleStep(step: EmployeeOnboardingStep): void {
    // Could expand/collapse step details in the future
  }

  toggleStepCompletion(step: EmployeeOnboardingStep, event: Event): void {
    event.stopPropagation();
    
    if (!this.employee || !this.onboarding) return;

    const newStatus = !step.completed;
    
    // Mock update for development
    step.completed = newStatus;
    this.updateOverallProgress();
    
    const message = newStatus ? 
      `Step "${step.title}" marked as completed` : 
      `Step "${step.title}" marked as incomplete`;
    this.notificationService.showSuccess(message);

    // Uncomment for production
    // this.employeeService.updateOnboardingStep(this.employee.id, step.id, newStatus).subscribe({
    //   next: (updatedOnboarding) => {
    //     this.onboarding = updatedOnboarding;
    //     const message = newStatus ? 
    //       `Step "${step.title}" marked as completed` : 
    //       `Step "${step.title}" marked as incomplete`;
    //     this.notificationService.showSuccess(message);
    //   },
    //   error: (error) => {
    //     this.notificationService.showError('Failed to update onboarding step');
    //   }
    // });
  }

  updateOverallProgress(): void {
    if (!this.onboarding) return;
    
    const completedSteps = this.onboarding.steps.filter(step => step.completed).length;
    const totalSteps = this.onboarding.steps.length;
    this.onboarding.overallProgress = Math.round((completedSteps / totalSteps) * 100);
    
    // Update status based on progress
    if (this.onboarding.overallProgress === 100) {
      this.onboarding.status = OnboardingStatus.Completed;
      this.onboarding.completedAt = new Date().toISOString();
    } else if (this.onboarding.overallProgress > 0) {
      this.onboarding.status = OnboardingStatus.InProgress;
    }
  }

  canCompleteOnboarding(): boolean {
    if (!this.onboarding) return false;
    
    const requiredSteps = this.onboarding.steps.filter(step => step.required);
    return requiredSteps.every(step => step.completed);
  }

  completeOnboarding(): void {
    if (!this.canCompleteOnboarding() || !this.onboarding) return;

    // Mark all steps as completed and update status
    this.onboarding.steps.forEach(step => step.completed = true);
    this.onboarding.overallProgress = 100;
    this.onboarding.status = OnboardingStatus.Completed;
    this.onboarding.completedAt = new Date().toISOString();
    
    this.notificationService.showSuccess('Onboarding process completed successfully!');
  }

  sendReminder(): void {
    this.notificationService.showSuccess('Reminder sent to employee and relevant stakeholders');
  }

  exportProgress(): void {
    this.notificationService.showInfo('Exporting onboarding progress report...');
  }

  getCompletedSteps(): number {
    return this.onboarding?.steps.filter(step => step.completed).length || 0;
  }

  getRemainingSteps(): number {
    return this.onboarding?.steps.filter(step => !step.completed).length || 0;
  }

  getStatusColor(status: OnboardingStatus): string {
    switch (status) {
      case OnboardingStatus.NotStarted: return 'secondary';
      case OnboardingStatus.InProgress: return 'primary';
      case OnboardingStatus.Completed: return 'success';
      case OnboardingStatus.Delayed: return 'danger';
      default: return 'secondary';
    }
  }

  getStatusText(status: OnboardingStatus): string {
    switch (status) {
      case OnboardingStatus.NotStarted: return 'Not Started';
      case OnboardingStatus.InProgress: return 'In Progress';
      case OnboardingStatus.Completed: return 'Completed';
      case OnboardingStatus.Delayed: return 'Delayed';
      default: return 'Unknown';
    }
  }

  getProgressColor(progress: number): string {
    if (progress >= 80) return 'success';
    if (progress >= 50) return 'primary';
    if (progress >= 25) return 'warning';
    return 'danger';
  }

  getProfilePhoto(): string {
    return this.employee?.profilePhoto || '/assets/images/avatars/default-avatar.png';
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  trackByStepId(index: number, step: EmployeeOnboardingStep): string {
    return step.id;
  }

  goBack(): void {
    this.router.navigate(['/employees', this.employee?.id]);
  }
}