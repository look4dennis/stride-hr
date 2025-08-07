import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ModalService } from '../../../services/modal.service';
import { PerformanceService } from '../../../services/performance.service';
import { EmployeeService } from '../../../services/employee.service';
import { PerformanceReview, PerformanceReviewStatus, GoalStatus, CreatePerformanceReviewDto } from '../../../models/performance.models';
import { Employee } from '../../../models/employee.models';

@Component({
  selector: 'app-performance-review',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NgbModule],
  template: `
    <div class="container-fluid">
      <div class="page-header d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="h3 mb-0">Performance Reviews</h1>
          <p class="text-muted">Manage employee performance evaluations</p>
        </div>
        <button class="btn btn-primary" (click)="openCreateModal()">
          <i class="fas fa-plus me-2"></i>Create Review
        </button>
      </div>

      <!-- Filters -->
      <div class="card mb-4">
        <div class="card-body">
          <div class="row g-3">
            <div class="col-md-3">
              <label class="form-label">Employee</label>
              <select class="form-select" [(ngModel)]="selectedEmployeeId" (change)="loadReviews()">
                <option value="">All Employees</option>
                <option *ngFor="let employee of employees" [value]="employee.id">
                  {{employee.firstName}} {{employee.lastName}} ({{employee.employeeId}})
                </option>
              </select>
            </div>
            <div class="col-md-3">
              <label class="form-label">Status</label>
              <select class="form-select" [(ngModel)]="selectedStatus" (change)="loadReviews()">
                <option value="">All Status</option>
                <option value="Draft">Draft</option>
                <option value="InProgress">In Progress</option>
                <option value="EmployeeReview">Employee Review</option>
                <option value="ManagerReview">Manager Review</option>
                <option value="Completed">Completed</option>
              </select>
            </div>
            <div class="col-md-3">
              <label class="form-label">Review Period</label>
              <input type="text" class="form-control" [(ngModel)]="searchPeriod" 
                     placeholder="e.g., Q1 2024" (input)="loadReviews()">
            </div>
            <div class="col-md-3 d-flex align-items-end">
              <button class="btn btn-outline-secondary" (click)="clearFilters()">
                <i class="fas fa-times me-2"></i>Clear Filters
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Reviews List -->
      <div class="card">
        <div class="card-body">
          <div class="table-responsive" *ngIf="reviews.length > 0; else noReviews">
            <table class="table table-hover">
              <thead>
                <tr>
                  <th>Employee</th>
                  <th>Review Period</th>
                  <th>Status</th>
                  <th>Overall Rating</th>
                  <th>Goals Progress</th>
                  <th>Created Date</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let review of reviews">
                  <td>
                    <div class="d-flex align-items-center">
                      <img [src]="review.employee?.profilePhoto || '/assets/images/default-avatar.png'" 
                           class="rounded-circle me-2" width="32" height="32" alt="Profile">
                      <div>
                        <div class="fw-medium">{{review.employee?.firstName}} {{review.employee?.lastName}}</div>
                        <small class="text-muted">{{review.employee?.employeeId}} - {{review.employee?.designation}}</small>
                      </div>
                    </div>
                  </td>
                  <td>{{review.reviewPeriod}}</td>
                  <td>
                    <span class="badge" [ngClass]="getStatusBadgeClass(review.status)">
                      {{review.status}}
                    </span>
                  </td>
                  <td>
                    <div class="d-flex align-items-center">
                      <div class="rating-stars me-2">
                        <i *ngFor="let star of [1,2,3,4,5]" 
                           class="fas fa-star" 
                           [class.text-warning]="star <= review.overallRating"
                           [class.text-muted]="star > review.overallRating"></i>
                      </div>
                      <span>{{review.overallRating}}/5</span>
                    </div>
                  </td>
                  <td>
                    <div class="progress" style="height: 8px;">
                      <div class="progress-bar" 
                           [style.width.%]="getGoalsProgress(review.goals)"
                           [ngClass]="getProgressBarClass(getGoalsProgress(review.goals))"></div>
                    </div>
                    <small class="text-muted">{{getCompletedGoals(review.goals)}}/{{review.goals.length || 0}} goals</small>
                  </td>
                  <td>{{review.createdAt | date:'short'}}</td>
                  <td>
                    <div class="btn-group btn-group-sm">
                      <button class="btn btn-outline-primary" (click)="viewReview(review)">
                        <i class="fas fa-eye"></i>
                      </button>
                      <button class="btn btn-outline-secondary" (click)="editReview(review)" 
                              *ngIf="review.status === 'Draft' || review.status === 'InProgress'">
                        <i class="fas fa-edit"></i>
                      </button>
                      <button class="btn btn-outline-success" (click)="submitReview(review)" 
                              *ngIf="review.status === 'Draft'">
                        <i class="fas fa-paper-plane"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <ng-template #noReviews>
            <div class="text-center py-5">
              <i class="fas fa-chart-line text-muted mb-3" style="font-size: 3rem;"></i>
              <h4>No Performance Reviews Found</h4>
              <p class="text-muted">Create your first performance review to get started.</p>
              <button class="btn btn-primary" (click)="openCreateModal()">
                <i class="fas fa-plus me-2"></i>Create Review
              </button>
            </div>
          </ng-template>
        </div>
      </div>
    </div>

    <!-- Create/Edit Review Modal -->
    <ng-template #reviewModal let-modal>
      <div class="modal-header">
        <h4 class="modal-title">{{isEditMode ? 'Edit' : 'Create'}} Performance Review</h4>
        <button type="button" class="btn-close" (click)="modal.dismiss()"></button>
      </div>
      <form [formGroup]="reviewForm" (ngSubmit)="saveReview(modal)">
        <div class="modal-body">
          <div class="row g-3">
            <div class="col-md-6">
              <label class="form-label">Employee *</label>
              <select class="form-select" formControlName="employeeId" [disabled]="isEditMode">
                <option value="">Select Employee</option>
                <option *ngFor="let employee of employees" [value]="employee.id">
                  {{employee.firstName}} {{employee.lastName}} ({{employee.employeeId}})
                </option>
              </select>
              <div class="invalid-feedback" *ngIf="reviewForm.get('employeeId')?.invalid && reviewForm.get('employeeId')?.touched">
                Please select an employee
              </div>
            </div>
            <div class="col-md-6">
              <label class="form-label">Review Period *</label>
              <input type="text" class="form-control" formControlName="reviewPeriod" 
                     placeholder="e.g., Q1 2024">
              <div class="invalid-feedback" *ngIf="reviewForm.get('reviewPeriod')?.invalid && reviewForm.get('reviewPeriod')?.touched">
                Review period is required
              </div>
            </div>
            <div class="col-md-6">
              <label class="form-label">Start Date *</label>
              <input type="date" class="form-control" formControlName="startDate">
              <div class="invalid-feedback" *ngIf="reviewForm.get('startDate')?.invalid && reviewForm.get('startDate')?.touched">
                Start date is required
              </div>
            </div>
            <div class="col-md-6">
              <label class="form-label">End Date *</label>
              <input type="date" class="form-control" formControlName="endDate">
              <div class="invalid-feedback" *ngIf="reviewForm.get('endDate')?.invalid && reviewForm.get('endDate')?.touched">
                End date is required
              </div>
            </div>
          </div>

          <!-- Goals Section -->
          <div class="mt-4">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <h5>Performance Goals</h5>
              <button type="button" class="btn btn-sm btn-outline-primary" (click)="addGoal()">
                <i class="fas fa-plus me-1"></i>Add Goal
              </button>
            </div>
            
            <div formArrayName="goals">
              <div *ngFor="let goal of goalsArray.controls; let i = index" 
                   [formGroupName]="i" class="card mb-3">
                <div class="card-body">
                  <div class="d-flex justify-content-between align-items-start mb-2">
                    <h6 class="card-title mb-0">Goal {{i + 1}}</h6>
                    <button type="button" class="btn btn-sm btn-outline-danger" 
                            (click)="removeGoal(i)" *ngIf="goalsArray.length > 1">
                      <i class="fas fa-trash"></i>
                    </button>
                  </div>
                  <div class="row g-2">
                    <div class="col-md-6">
                      <label class="form-label">Title *</label>
                      <input type="text" class="form-control" formControlName="title" 
                             placeholder="Goal title">
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Weight (%) *</label>
                      <input type="number" class="form-control" formControlName="weight" 
                             min="1" max="100" placeholder="25">
                    </div>
                    <div class="col-12">
                      <label class="form-label">Description *</label>
                      <textarea class="form-control" formControlName="description" 
                                rows="2" placeholder="Describe the goal..."></textarea>
                    </div>
                    <div class="col-12">
                      <label class="form-label">Target Value *</label>
                      <input type="text" class="form-control" formControlName="targetValue" 
                             placeholder="e.g., Increase sales by 20%">
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" (click)="modal.dismiss()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="reviewForm.invalid || loading">
            <span *ngIf="loading" class="spinner-border spinner-border-sm me-2"></span>
            {{isEditMode ? 'Update' : 'Create'}} Review
          </button>
        </div>
      </form>
    </ng-template>
  `,
  styles: [`
    .rating-stars .fa-star {
      font-size: 0.875rem;
    }
    
    .progress {
      background-color: #e9ecef;
    }
    
    .badge {
      font-size: 0.75rem;
    }
    
    .card-title {
      color: var(--text-primary);
      font-weight: 600;
    }
    
    .btn-group-sm .btn {
      padding: 0.25rem 0.5rem;
    }
    
    .modal-body {
      max-height: 70vh;
      overflow-y: auto;
    }
  `]
})
export class PerformanceReviewComponent implements OnInit {
  @ViewChild('reviewModal') reviewModal!: TemplateRef<any>;
  
  reviews: PerformanceReview[] = [];
  employees: Employee[] = [];
  selectedEmployeeId: string = '';
  selectedStatus: string = '';
  searchPeriod: string = '';
  loading = false;
  
  reviewForm: FormGroup;
  isEditMode = false;
  currentReview: PerformanceReview | null = null;

  constructor(
    private performanceService: PerformanceService,
    private employeeService: EmployeeService,
    private modalService: ModalService,
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.reviewForm = this.createReviewForm();
  }

  ngOnInit() {
    this.loadEmployees();
    this.loadReviews();
  }

  createReviewForm(): FormGroup {
    return this.fb.group({
      employeeId: ['', Validators.required],
      reviewPeriod: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      goals: this.fb.array([this.createGoalForm()])
    });
  }

  createGoalForm(): FormGroup {
    return this.fb.group({
      title: ['', Validators.required],
      description: ['', Validators.required],
      targetValue: ['', Validators.required],
      weight: [25, [Validators.required, Validators.min(1), Validators.max(100)]]
    });
  }

  get goalsArray(): FormArray {
    return this.reviewForm.get('goals') as FormArray;
  }

  loadEmployees() {
    this.employeeService.getEmployees().subscribe({
      next: (result) => {
        this.employees = result.items;
      },
      error: (error) => {
        console.error('Error loading employees:', error);
      }
    });
  }

  loadReviews() {
    this.loading = true;
    const employeeId = this.selectedEmployeeId ? parseInt(this.selectedEmployeeId) : undefined;
    
    this.performanceService.getPerformanceReviews(employeeId, this.selectedStatus).subscribe({
      next: (reviews) => {
        this.reviews = reviews.filter(review => 
          !this.searchPeriod || review.reviewPeriod.toLowerCase().includes(this.searchPeriod.toLowerCase())
        );
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading reviews:', error);
        this.loading = false;
      }
    });
  }

  clearFilters() {
    this.selectedEmployeeId = '';
    this.selectedStatus = '';
    this.searchPeriod = '';
    this.loadReviews();
  }

  openCreateModal() {
    this.isEditMode = false;
    this.currentReview = null;
    this.reviewForm = this.createReviewForm();
    this.modalService.openTemplate(this.reviewModal, { size: 'lg', backdrop: 'static' });
  }

  editReview(review: PerformanceReview) {
    this.isEditMode = true;
    this.currentReview = review;
    this.populateForm(review);
    this.modalService.openTemplate(this.reviewModal, { size: 'lg', backdrop: 'static' });
  }

  populateForm(review: PerformanceReview) {
    // Clear existing goals
    while (this.goalsArray.length !== 0) {
      this.goalsArray.removeAt(0);
    }

    // Add goals from review
    review.goals?.forEach(goal => {
      this.goalsArray.push(this.fb.group({
        title: [goal.title, Validators.required],
        description: [goal.description, Validators.required],
        targetValue: [goal.targetValue, Validators.required],
        weight: [goal.weight, [Validators.required, Validators.min(1), Validators.max(100)]]
      }));
    });

    this.reviewForm.patchValue({
      employeeId: review.employeeId,
      reviewPeriod: review.reviewPeriod,
      startDate: new Date(review.startDate).toISOString().split('T')[0],
      endDate: new Date(review.endDate).toISOString().split('T')[0]
    });
  }

  addGoal() {
    this.goalsArray.push(this.createGoalForm());
  }

  removeGoal(index: number) {
    this.goalsArray.removeAt(index);
  }

  saveReview(modal: any) {
    if (this.reviewForm.valid) {
      this.loading = true;
      const formValue = this.reviewForm.value;
      
      const reviewData: CreatePerformanceReviewDto = {
        employeeId: parseInt(formValue.employeeId),
        reviewPeriod: formValue.reviewPeriod,
        startDate: new Date(formValue.startDate),
        endDate: new Date(formValue.endDate),
        goals: formValue.goals
      };

      const request = this.isEditMode && this.currentReview
        ? this.performanceService.updatePerformanceReview(this.currentReview.id, reviewData as any)
        : this.performanceService.createPerformanceReview(reviewData);

      request.subscribe({
        next: (review) => {
          this.loadReviews();
          modal.close();
          this.loading = false;
        },
        error: (error) => {
          console.error('Error saving review:', error);
          this.loading = false;
        }
      });
    }
  }

  viewReview(review: PerformanceReview) {
    this.router.navigate(['/performance/reviews', review.id]);
  }

  submitReview(review: PerformanceReview) {
    if (confirm('Are you sure you want to submit this review? This action cannot be undone.')) {
      this.performanceService.submitPerformanceReview(review.id).subscribe({
        next: () => {
          this.loadReviews();
        },
        error: (error) => {
          console.error('Error submitting review:', error);
        }
      });
    }
  }

  getStatusBadgeClass(status: PerformanceReviewStatus): string {
    const classes = {
      'Draft': 'bg-secondary',
      'InProgress': 'bg-primary',
      'EmployeeReview': 'bg-info',
      'ManagerReview': 'bg-warning',
      'Completed': 'bg-success',
      'Cancelled': 'bg-danger'
    };
    return classes[status] || 'bg-secondary';
  }

  getGoalsProgress(goals: any[]): number {
    if (!goals || goals.length === 0) return 0;
    const completedGoals = goals.filter(goal => 
      goal.status === GoalStatus.Completed || goal.status === GoalStatus.Exceeded
    ).length;
    return Math.round((completedGoals / goals.length) * 100);
  }

  getCompletedGoals(goals: any[]): number {
    if (!goals) return 0;
    return goals.filter(goal => 
      goal.status === GoalStatus.Completed || goal.status === GoalStatus.Exceeded
    ).length;
  }

  getProgressBarClass(progress: number): string {
    if (progress >= 80) return 'bg-success';
    if (progress >= 60) return 'bg-warning';
    return 'bg-danger';
  }
}