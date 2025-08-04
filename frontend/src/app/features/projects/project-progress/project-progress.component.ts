import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbProgressbarModule, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { Subject, takeUntil } from 'rxjs';

import { ProjectService } from '../../../services/project.service';
import { Project, ProjectProgress, TaskStatus } from '../../../models/project.models';

@Component({
    selector: 'app-project-progress',
    imports: [CommonModule, NgbProgressbarModule, NgbTooltipModule],
    template: `
    <div class="project-progress-container">
      <!-- Progress Header -->
      <div class="progress-header d-flex justify-content-between align-items-center mb-3">
        <h5 class="mb-0">
          <i class="fas fa-chart-line me-2 text-primary"></i>
          Project Progress
        </h5>
        <div class="progress-actions">
          <button class="btn btn-sm btn-outline-primary" 
                  (click)="refreshProgress()"
                  [disabled]="loading">
            <i class="fas fa-sync-alt me-1" [class.fa-spin]="loading"></i>
            Refresh
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div *ngIf="loading" class="text-center py-4">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading progress...</span>
        </div>
      </div>

      <!-- Progress Content -->
      <div *ngIf="!loading && progress" class="progress-content">
        <!-- Overall Progress -->
        <div class="overall-progress mb-4">
          <div class="d-flex justify-content-between align-items-center mb-2">
            <span class="progress-label">Overall Completion</span>
            <span class="progress-percentage" [class]="getProgressClass()">
              {{ progress.completionPercentage }}%
            </span>
          </div>
          <ngb-progressbar 
            [value]="progress.completionPercentage" 
            [type]="getProgressType()"
            [height]="'12px'"
            [animated]="true">
          </ngb-progressbar>
          <small class="text-muted mt-1 d-block">
            {{ progress.completedTasks }} of {{ progress.totalTasks }} tasks completed
          </small>
        </div>

        <!-- Task Status Breakdown -->
        <div class="task-breakdown mb-4">
          <h6 class="mb-3">Task Status Breakdown</h6>
          <div class="row">
            <div class="col-6 col-md-3 mb-3">
              <div class="status-card todo">
                <div class="status-icon">
                  <i class="fas fa-clipboard-list"></i>
                </div>
                <div class="status-info">
                  <div class="status-count">{{ progress.todoTasks }}</div>
                  <div class="status-label">To Do</div>
                </div>
              </div>
            </div>
            
            <div class="col-6 col-md-3 mb-3">
              <div class="status-card in-progress">
                <div class="status-icon">
                  <i class="fas fa-play-circle"></i>
                </div>
                <div class="status-info">
                  <div class="status-count">{{ progress.inProgressTasks }}</div>
                  <div class="status-label">In Progress</div>
                </div>
              </div>
            </div>
            
            <div class="col-6 col-md-3 mb-3">
              <div class="status-card completed">
                <div class="status-icon">
                  <i class="fas fa-check-circle"></i>
                </div>
                <div class="status-info">
                  <div class="status-count">{{ progress.completedTasks }}</div>
                  <div class="status-label">Completed</div>
                </div>
              </div>
            </div>
            
            <div class="col-6 col-md-3 mb-3">
              <div class="status-card total">
                <div class="status-icon">
                  <i class="fas fa-tasks"></i>
                </div>
                <div class="status-info">
                  <div class="status-count">{{ progress.totalTasks }}</div>
                  <div class="status-label">Total</div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Hours Tracking -->
        <div class="hours-tracking mb-4" *ngIf="project">
          <h6 class="mb-3">Hours Tracking</h6>
          <div class="row">
            <div class="col-md-6 mb-3">
              <div class="hours-card">
                <div class="d-flex justify-content-between align-items-center mb-2">
                  <span class="hours-label">
                    <i class="fas fa-clock me-2 text-info"></i>
                    Hours Progress
                  </span>
                  <span class="hours-percentage">
                    {{ getHoursPercentage() }}%
                  </span>
                </div>
                <ngb-progressbar 
                  [value]="getHoursPercentage()" 
                  [type]="getHoursProgressType()"
                  [height]="'8px'">
                </ngb-progressbar>
                <div class="hours-details mt-2">
                  <small class="text-muted">
                    {{ project.actualHours || 0 }}h worked of {{ project.estimatedHours }}h estimated
                  </small>
                </div>
              </div>
            </div>
            
            <div class="col-md-6 mb-3">
              <div class="hours-card">
                <div class="d-flex justify-content-between align-items-center mb-2">
                  <span class="hours-label">
                    <i class="fas fa-hourglass-half me-2 text-warning"></i>
                    Remaining Hours
                  </span>
                  <span class="hours-value">
                    {{ progress.remainingHours }}h
                  </span>
                </div>
                <div class="hours-details">
                  <small [class]="getRemainingHoursClass()">
                    <i class="fas" [class]="getRemainingHoursIcon()"></i>
                    {{ getRemainingHoursText() }}
                  </small>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Project Health Indicators -->
        <div class="project-health">
          <h6 class="mb-3">Project Health</h6>
          <div class="row">
            <div class="col-md-4 mb-3">
              <div class="health-indicator" [class]="getScheduleHealthClass()">
                <div class="health-icon">
                  <i class="fas" [class]="getScheduleHealthIcon()"></i>
                </div>
                <div class="health-info">
                  <div class="health-status">{{ getScheduleHealthText() }}</div>
                  <small class="health-label">Schedule</small>
                </div>
              </div>
            </div>
            
            <div class="col-md-4 mb-3">
              <div class="health-indicator" [class]="getBudgetHealthClass()">
                <div class="health-icon">
                  <i class="fas" [class]="getBudgetHealthIcon()"></i>
                </div>
                <div class="health-info">
                  <div class="health-status">{{ getBudgetHealthText() }}</div>
                  <small class="health-label">Budget</small>
                </div>
              </div>
            </div>
            
            <div class="col-md-4 mb-3">
              <div class="health-indicator" [class]="getQualityHealthClass()">
                <div class="health-icon">
                  <i class="fas fa-star"></i>
                </div>
                <div class="health-info">
                  <div class="health-status">Good</div>
                  <small class="health-label">Quality</small>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Quick Stats -->
        <div class="quick-stats mt-4">
          <div class="row text-center">
            <div class="col-4">
              <div class="stat-item">
                <div class="stat-value text-primary">{{ getDaysRemaining() }}</div>
                <div class="stat-label">Days Left</div>
              </div>
            </div>
            <div class="col-4">
              <div class="stat-item">
                <div class="stat-value text-success">{{ getVelocity() }}</div>
                <div class="stat-label">Tasks/Week</div>
              </div>
            </div>
            <div class="col-4">
              <div class="stat-item">
                <div class="stat-value text-info">{{ getTeamSize() }}</div>
                <div class="stat-label">Team Size</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Error State -->
      <div *ngIf="!loading && !progress" class="text-center py-4">
        <i class="fas fa-exclamation-triangle text-warning mb-2" style="font-size: 2rem;"></i>
        <h6>Unable to load progress data</h6>
        <p class="text-muted">Please try refreshing or contact support if the issue persists.</p>
        <button class="btn btn-primary" (click)="refreshProgress()">
          <i class="fas fa-sync-alt me-1"></i>
          Try Again
        </button>
      </div>
    </div>
  `,
    styles: [`
    .project-progress-container {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .progress-header h5 {
      font-weight: 600;
      color: #495057;
    }

    .overall-progress .progress-label {
      font-weight: 500;
      color: #495057;
    }

    .progress-percentage {
      font-weight: 700;
      font-size: 1.1rem;
    }

    .progress-percentage.on-track {
      color: #198754;
    }

    .progress-percentage.at-risk {
      color: #fd7e14;
    }

    .progress-percentage.behind {
      color: #dc3545;
    }

    .status-card {
      background: white;
      border: 2px solid #e9ecef;
      border-radius: 12px;
      padding: 1rem;
      text-align: center;
      transition: all 0.2s ease;
      height: 100%;
    }

    .status-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }

    .status-card.todo {
      border-color: #6c757d;
    }

    .status-card.in-progress {
      border-color: #0d6efd;
    }

    .status-card.completed {
      border-color: #198754;
    }

    .status-card.total {
      border-color: #6f42c1;
    }

    .status-icon {
      font-size: 1.5rem;
      margin-bottom: 0.5rem;
    }

    .status-card.todo .status-icon {
      color: #6c757d;
    }

    .status-card.in-progress .status-icon {
      color: #0d6efd;
    }

    .status-card.completed .status-icon {
      color: #198754;
    }

    .status-card.total .status-icon {
      color: #6f42c1;
    }

    .status-count {
      font-size: 1.5rem;
      font-weight: 700;
      color: #495057;
    }

    .status-label {
      font-size: 0.875rem;
      color: #6c757d;
      font-weight: 500;
    }

    .hours-card {
      background: #f8f9fa;
      border-radius: 8px;
      padding: 1rem;
    }

    .hours-label {
      font-weight: 500;
      color: #495057;
    }

    .hours-percentage,
    .hours-value {
      font-weight: 600;
      color: #495057;
    }

    .health-indicator {
      background: white;
      border: 2px solid #e9ecef;
      border-radius: 8px;
      padding: 1rem;
      display: flex;
      align-items: center;
      transition: all 0.2s ease;
    }

    .health-indicator:hover {
      transform: translateY(-1px);
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .health-indicator.healthy {
      border-color: #198754;
      background-color: #f8fff9;
    }

    .health-indicator.warning {
      border-color: #fd7e14;
      background-color: #fff8f0;
    }

    .health-indicator.danger {
      border-color: #dc3545;
      background-color: #fff5f5;
    }

    .health-icon {
      font-size: 1.25rem;
      margin-right: 0.75rem;
      width: 32px;
      text-align: center;
    }

    .health-indicator.healthy .health-icon {
      color: #198754;
    }

    .health-indicator.warning .health-icon {
      color: #fd7e14;
    }

    .health-indicator.danger .health-icon {
      color: #dc3545;
    }

    .health-status {
      font-weight: 600;
      color: #495057;
    }

    .health-label {
      color: #6c757d;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .stat-item {
      padding: 0.5rem;
    }

    .stat-value {
      font-size: 1.5rem;
      font-weight: 700;
    }

    .stat-label {
      font-size: 0.75rem;
      color: #6c757d;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-top: 0.25rem;
    }

    .quick-stats {
      border-top: 1px solid #e9ecef;
      padding-top: 1rem;
    }

    @media (max-width: 768px) {
      .project-progress-container {
        padding: 1rem;
      }
      
      .status-card {
        margin-bottom: 1rem;
      }
      
      .health-indicator {
        margin-bottom: 0.5rem;
      }
    }
  `]
})
export class ProjectProgressComponent implements OnInit, OnDestroy {
  @Input() project: Project | null = null;
  @Input() projectId: number | null = null;

  progress: ProjectProgress | null = null;
  loading = false;

  private destroy$ = new Subject<void>();

  constructor(private projectService: ProjectService) {}

  ngOnInit(): void {
    this.loadProgress();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProgress(): void {
    const id = this.projectId || this.project?.id;
    if (!id) return;

    this.loading = true;
    this.projectService.getProjectProgress(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (progress) => {
          this.progress = progress;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading project progress:', error);
          this.loading = false;
        }
      });
  }

  refreshProgress(): void {
    this.loadProgress();
  }

  getProgressClass(): string {
    if (!this.progress) return '';
    
    if (this.progress.completionPercentage >= 80) return 'on-track';
    if (this.progress.completionPercentage >= 50) return 'at-risk';
    return 'behind';
  }

  getProgressType(): string {
    if (!this.progress) return 'secondary';
    
    if (this.progress.completionPercentage >= 80) return 'success';
    if (this.progress.completionPercentage >= 50) return 'warning';
    return 'danger';
  }

  getHoursPercentage(): number {
    if (!this.project || !this.project.estimatedHours) return 0;
    return Math.min((this.project.actualHours || 0) / this.project.estimatedHours * 100, 100);
  }

  getHoursProgressType(): string {
    const percentage = this.getHoursPercentage();
    if (percentage <= 100) return 'info';
    return 'warning';
  }

  getRemainingHoursClass(): string {
    if (!this.progress) return 'text-muted';
    
    if (this.progress.remainingHours < 0) return 'text-danger';
    if (this.progress.remainingHours < 10) return 'text-warning';
    return 'text-success';
  }

  getRemainingHoursIcon(): string {
    if (!this.progress) return 'fa-clock';
    
    if (this.progress.remainingHours < 0) return 'fa-exclamation-triangle';
    if (this.progress.remainingHours < 10) return 'fa-clock';
    return 'fa-check-circle';
  }

  getRemainingHoursText(): string {
    if (!this.progress) return 'Calculating...';
    
    if (this.progress.remainingHours < 0) return 'Over budget';
    if (this.progress.remainingHours < 10) return 'Running low';
    return 'On track';
  }

  getScheduleHealthClass(): string {
    if (!this.progress) return '';
    
    if (this.progress.isOnTrack) return 'healthy';
    if (this.progress.completionPercentage >= 50) return 'warning';
    return 'danger';
  }

  getScheduleHealthIcon(): string {
    if (!this.progress) return 'fa-clock';
    
    if (this.progress.isOnTrack) return 'fa-check-circle';
    if (this.progress.completionPercentage >= 50) return 'fa-exclamation-triangle';
    return 'fa-times-circle';
  }

  getScheduleHealthText(): string {
    if (!this.progress) return 'Unknown';
    
    if (this.progress.isOnTrack) return 'On Track';
    if (this.progress.completionPercentage >= 50) return 'At Risk';
    return 'Behind';
  }

  getBudgetHealthClass(): string {
    const hoursPercentage = this.getHoursPercentage();
    
    if (hoursPercentage <= 80) return 'healthy';
    if (hoursPercentage <= 100) return 'warning';
    return 'danger';
  }

  getBudgetHealthIcon(): string {
    const hoursPercentage = this.getHoursPercentage();
    
    if (hoursPercentage <= 80) return 'fa-dollar-sign';
    if (hoursPercentage <= 100) return 'fa-exclamation-triangle';
    return 'fa-times-circle';
  }

  getBudgetHealthText(): string {
    const hoursPercentage = this.getHoursPercentage();
    
    if (hoursPercentage <= 80) return 'Under Budget';
    if (hoursPercentage <= 100) return 'Near Budget';
    return 'Over Budget';
  }

  getQualityHealthClass(): string {
    // This would be based on actual quality metrics
    return 'healthy';
  }

  getDaysRemaining(): number {
    if (!this.project) return 0;
    
    const today = new Date();
    const endDate = new Date(this.project.endDate);
    const diffTime = endDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    return Math.max(0, diffDays);
  }

  getVelocity(): number {
    if (!this.progress || !this.project) return 0;
    
    const startDate = new Date(this.project.startDate);
    const today = new Date();
    const weeksElapsed = Math.max(1, (today.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24 * 7));
    
    return Math.round(this.progress.completedTasks / weeksElapsed);
  }

  getTeamSize(): number {
    return this.project?.teamMembers?.length || 0;
  }
}