import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { EnhancedEmployeeService } from '../../../services/enhanced-employee.service';
import { Employee, EmployeeOnboarding, EmployeeOnboardingStep } from '../../../models/employee.models';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-employee-onboarding',
  standalone: true,
  imports: [CommonModule],
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
            <li class="breadcrumb-item active" aria-current="page">Onboarding</li>
          </ol>
        </nav>
        <h1>Employee Onboarding</h1>
        <p class="text-muted" *ngIf="employee">{{ employee.employeeId }} - {{ employee.designation }}</p>
      </div>
      <button class="btn btn-outline-secondary" (click)="navigateToEmployeeProfile()">
        <i class="fas fa-arrow-left me-2"></i>Back to Profile
      </button>
    </div>

    <div class="row" *ngIf="onboarding">
      <div class="col-lg-8 mx-auto">
        <div class="card">
          <div class="card-header">
            <div class="d-flex justify-content-between align-items-center">
              <h5 class="card-title mb-0">
                <i class="fas fa-user-plus me-2"></i>Onboarding Progress
              </h5>
              <div class="progress-info">
                <span class="badge bg-primary">{{ onboarding.overallProgress }}% Complete</span>
              </div>
            </div>
            <div class="progress mt-3">
              <div class="progress-bar" 
                   role="progressbar" 
                   [style.width.%]="onboarding.overallProgress"
                   [attr.aria-valuenow]="onboarding.overallProgress"
                   aria-valuemin="0" 
                   aria-valuemax="100">
              </div>
            </div>
          </div>
          
          <div class="card-body">
            <div class="onboarding-steps">
              <div class="step-item" 
                   *ngFor="let step of onboarding.steps; let i = index"
                   [class.completed]="step.completed"
                   [class.current]="!step.completed && isCurrentStep(step)">
                
                <div class="step-indicator">
                  <div class="step-number" *ngIf="!step.completed">{{ i + 1 }}</div>
                  <i class="fas fa-check" *ngIf="step.completed"></i>
                </div>
                
                <div class="step-content">
                  <div class="step-header">
                    <h6 class="step-title">{{ step.title }}</h6>
                    <span class="badge" 
                          [class]="step.completed ? 'bg-success' : (step.required ? 'bg-warning' : 'bg-secondary')">
                      {{ step.completed ? 'Completed' : (step.required ? 'Required' : 'Optional') }}
                    </span>
                  </div>
                  
                  <p class="step-description">{{ step.description }}</p>
                  
                  <div class="step-actions" *ngIf="!step.completed">
                    <button class="btn btn-primary btn-sm" 
                            (click)="completeStep(step)"
                            [disabled]="loading">
                      <i class="fas fa-check me-1"></i>Mark as Complete
                    </button>
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
      <p class="mt-2 text-muted">Loading onboarding information...</p>
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

    .onboarding-steps {
      position: relative;
    }

    .step-item {
      display: flex;
      margin-bottom: 2rem;
      position: relative;
    }

    .step-item:not(:last-child)::after {
      content: '';
      position: absolute;
      left: 20px;
      top: 50px;
      bottom: -32px;
      width: 2px;
      background: #e9ecef;
    }

    .step-item.completed::after {
      background: #28a745;
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
      position: relative;
      z-index: 1;
    }

    .step-item:not(.completed) .step-indicator {
      background: #f8f9fa;
      border: 2px solid #dee2e6;
      color: #6c757d;
    }

    .step-item.current .step-indicator {
      background: #007bff;
      border-color: #007bff;
      color: white;
    }

    .step-item.completed .step-indicator {
      background: #28a745;
      border-color: #28a745;
      color: white;
    }

    .step-number {
      font-weight: 600;
      font-size: 0.875rem;
    }

    .step-content {
      flex: 1;
      padding-top: 0.25rem;
    }

    .step-header {
      display: flex;
      justify-content: between;
      align-items: center;
      margin-bottom: 0.5rem;
    }

    .step-title {
      margin: 0;
      color: #495057;
      font-weight: 600;
    }

    .step-item.completed .step-title {
      color: #28a745;
    }

    .step-description {
      color: #6c757d;
      margin-bottom: 1rem;
      font-size: 0.9rem;
    }

    .step-actions {
      margin-top: 1rem;
    }

    .progress {
      height: 8px;
      border-radius: 4px;
    }

    .progress-bar {
      border-radius: 4px;
      transition: width 0.3s ease;
    }

    .progress-info {
      font-size: 0.875rem;
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
      
      .step-item {
        flex-direction: column;
        text-align: center;
      }
      
      .step-indicator {
        margin: 0 auto 1rem auto;
      }
      
      .step-item::after {
        display: none;
      }
    }
  `]
})
export class EmployeeOnboardingComponent implements OnInit {
  employee: Employee | null = null;
  onboarding: EmployeeOnboarding | null = null;
  loading = false;
  employeeId: number = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private employeeService: EnhancedEmployeeService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.employeeId = parseInt(this.route.snapshot.params['id']);
    if (this.employeeId) {
      this.loadEmployee();
      this.loadOnboarding();
    }
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

  loadOnboarding(): void {
    this.loading = true;
    
    this.employeeService.getEmployeeOnboarding(this.employeeId).subscribe({
      next: (onboarding) => {
        this.onboarding = onboarding;
        this.calculateProgress();
        this.loading = false;
      },
      error: (error) => {
        this.notificationService.showError('Failed to load onboarding information');
        this.loading = false;
      }
    });
  }

  completeStep(step: EmployeeOnboardingStep): void {
    this.loading = true;
    
    this.employeeService.updateOnboardingStep(this.employeeId, step.id, true).subscribe({
      next: (updatedOnboarding) => {
        this.onboarding = updatedOnboarding;
        this.calculateProgress();
        this.notificationService.showSuccess(`${step.title} marked as complete`);
        this.loading = false;
      },
      error: (error) => {
        this.notificationService.showError('Failed to update onboarding step');
        this.loading = false;
      }
    });
  }

  isCurrentStep(step: EmployeeOnboardingStep): boolean {
    if (!this.onboarding) return false;
    
    // Find the first incomplete step
    const incompleteSteps = this.onboarding.steps.filter(s => !s.completed);
    return incompleteSteps.length > 0 && incompleteSteps[0].id === step.id;
  }

  private calculateProgress(): void {
    if (!this.onboarding) return;
    
    const totalSteps = this.onboarding.steps.length;
    const completedSteps = this.onboarding.steps.filter(s => s.completed).length;
    this.onboarding.overallProgress = totalSteps > 0 ? Math.round((completedSteps / totalSteps) * 100) : 0;
  }

  navigateToEmployeeList(): void {
    this.router.navigate(['/employees']);
  }

  navigateToEmployeeProfile(): void {
    this.router.navigate(['/employees', this.employeeId]);
  }
}