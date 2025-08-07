import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ProgressiveLoadingService, LoadingState, LoadingTask } from '../../../core/services/progressive-loading.service';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-progressive-dashboard',
  imports: [CommonModule],
  template: `
    <div class="progressive-dashboard">
      <!-- Overall Progress Bar -->
      <div class="progress-header" *ngIf="showProgress && overallProgress < 100">
        <div class="progress-info">
          <h6 class="mb-1">Loading Dashboard</h6>
          <small class="text-muted">{{ getLoadingMessage() }}</small>
        </div>
        <div class="progress mb-3">
          <div class="progress-bar progress-bar-striped progress-bar-animated" 
               role="progressbar" 
               [style.width.%]="overallProgress"
               [attr.aria-valuenow]="overallProgress" 
               aria-valuemin="0" 
               aria-valuemax="100">
            {{ overallProgress.toFixed(0) }}%
          </div>
        </div>
      </div>

      <!-- Loading Tasks Status -->
      <div class="loading-tasks" *ngIf="showTaskDetails && loadingStates.size > 0">
        <div class="row g-3">
          <div class="col-md-6 col-lg-4" *ngFor="let state of getLoadingStatesArray()">
            <div class="task-card" [ngClass]="getTaskCardClass(state)">
              <div class="task-header">
                <div class="task-icon">
                  <i [class]="getTaskIcon(state)" 
                     [ngClass]="getTaskIconClass(state)"></i>
                </div>
                <div class="task-info">
                  <h6 class="task-name">{{ getTaskName(state.taskId) }}</h6>
                  <small class="task-status">{{ getStatusText(state.status) }}</small>
                </div>
              </div>
              
              <!-- Task Progress -->
              <div class="task-progress" *ngIf="state.status === 'loading'">
                <div class="progress progress-sm">
                  <div class="progress-bar" 
                       [style.width.%]="state.progress"
                       [attr.aria-valuenow]="state.progress">
                  </div>
                </div>
              </div>

              <!-- Task Duration -->
              <div class="task-duration" *ngIf="state.endTime && state.startTime">
                <small class="text-muted">
                  <i class="fas fa-clock me-1"></i>
                  {{ getDuration(state.startTime, state.endTime) }}ms
                </small>
              </div>

              <!-- Error Message -->
              <div class="task-error" *ngIf="state.status === 'failed' && state.error">
                <small class="text-danger">
                  <i class="fas fa-exclamation-triangle me-1"></i>
                  {{ getErrorMessage(state.error) }}
                </small>
              </div>

              <!-- Retry Button -->
              <div class="task-actions" *ngIf="state.status === 'failed'">
                <button class="btn btn-sm btn-outline-primary" 
                        (click)="retryTask(state.taskId)">
                  <i class="fas fa-redo me-1"></i>
                  Retry
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Dashboard Content Slots -->
      <div class="dashboard-content" [class.loading-overlay]="overallProgress < 100 && showProgress">
        <!-- Skeleton Loading for Dashboard Widgets -->
        <div class="skeleton-dashboard" *ngIf="overallProgress < 100 && showSkeletons">
          <div class="row g-4">
            <!-- Welcome Card Skeleton -->
            <div class="col-lg-8">
              <div class="skeleton-card skeleton-welcome">
                <div class="skeleton-content">
                  <div class="skeleton-line skeleton-title"></div>
                  <div class="skeleton-line skeleton-subtitle"></div>
                  <div class="skeleton-line skeleton-info"></div>
                </div>
                <div class="skeleton-avatar"></div>
              </div>
            </div>
            
            <!-- Weather Widget Skeleton -->
            <div class="col-lg-4">
              <div class="skeleton-card skeleton-weather">
                <div class="skeleton-line skeleton-weather-title"></div>
                <div class="skeleton-line skeleton-weather-temp"></div>
                <div class="skeleton-line skeleton-weather-desc"></div>
              </div>
            </div>

            <!-- Stats Widgets Skeleton -->
            <div class="col-md-3" *ngFor="let i of [1,2,3,4]">
              <div class="skeleton-card skeleton-stat">
                <div class="skeleton-icon"></div>
                <div class="skeleton-content">
                  <div class="skeleton-line skeleton-stat-value"></div>
                  <div class="skeleton-line skeleton-stat-label"></div>
                </div>
              </div>
            </div>

            <!-- Chart Skeleton -->
            <div class="col-lg-8">
              <div class="skeleton-card skeleton-chart">
                <div class="skeleton-line skeleton-chart-title"></div>
                <div class="skeleton-chart-area"></div>
              </div>
            </div>

            <!-- Quick Actions Skeleton -->
            <div class="col-lg-4">
              <div class="skeleton-card skeleton-actions">
                <div class="skeleton-line skeleton-actions-title"></div>
                <div class="skeleton-action" *ngFor="let i of [1,2,3,4]">
                  <div class="skeleton-action-icon"></div>
                  <div class="skeleton-action-text"></div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Actual Dashboard Content -->
        <div class="actual-dashboard" 
             [style.opacity]="overallProgress >= 100 ? 1 : 0"
             [style.transition]="'opacity 0.5s ease-in-out'">
          <ng-content></ng-content>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .progressive-dashboard {
      position: relative;
      min-height: 400px;
    }

    .progress-header {
      background: white;
      padding: 1.5rem;
      border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
      margin-bottom: 2rem;
    }

    .progress-info h6 {
      color: var(--text-primary);
      font-weight: 600;
    }

    .progress {
      height: 8px;
      border-radius: 4px;
      background-color: #f8f9fa;
    }

    .progress-bar {
      border-radius: 4px;
      background: linear-gradient(90deg, var(--primary) 0%, var(--primary-dark) 100%);
    }

    .loading-tasks {
      margin-bottom: 2rem;
    }

    .task-card {
      background: white;
      border-radius: 12px;
      padding: 1rem;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
      border-left: 4px solid transparent;
      transition: all 0.3s ease;
    }

    .task-card.pending {
      border-left-color: #6c757d;
    }

    .task-card.loading {
      border-left-color: var(--primary);
    }

    .task-card.completed {
      border-left-color: #28a745;
    }

    .task-card.failed {
      border-left-color: #dc3545;
    }

    .task-card.timeout {
      border-left-color: #ffc107;
    }

    .task-header {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 0.5rem;
    }

    .task-icon {
      width: 32px;
      height: 32px;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.875rem;
    }

    .task-icon.pending {
      background-color: #f8f9fa;
      color: #6c757d;
    }

    .task-icon.loading {
      background-color: rgba(59, 130, 246, 0.1);
      color: var(--primary);
    }

    .task-icon.completed {
      background-color: rgba(40, 167, 69, 0.1);
      color: #28a745;
    }

    .task-icon.failed {
      background-color: rgba(220, 53, 69, 0.1);
      color: #dc3545;
    }

    .task-icon.timeout {
      background-color: rgba(255, 193, 7, 0.1);
      color: #ffc107;
    }

    .task-info {
      flex: 1;
    }

    .task-name {
      font-size: 0.875rem;
      font-weight: 600;
      margin-bottom: 0.25rem;
      color: var(--text-primary);
    }

    .task-status {
      color: var(--text-secondary);
      text-transform: capitalize;
    }

    .progress-sm {
      height: 4px;
    }

    .task-duration,
    .task-error {
      margin-top: 0.5rem;
    }

    .task-actions {
      margin-top: 0.75rem;
    }

    .loading-overlay {
      position: relative;
    }

    .loading-overlay::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(255, 255, 255, 0.8);
      z-index: 1;
      pointer-events: none;
    }

    /* Skeleton Loading Styles */
    .skeleton-dashboard {
      position: relative;
      z-index: 2;
    }

    .skeleton-card {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
      height: 100%;
    }

    .skeleton-welcome {
      display: flex;
      align-items: center;
      justify-content: space-between;
      min-height: 120px;
    }

    .skeleton-weather {
      min-height: 120px;
    }

    .skeleton-stat {
      display: flex;
      align-items: center;
      gap: 1rem;
      min-height: 100px;
    }

    .skeleton-chart {
      min-height: 300px;
    }

    .skeleton-actions {
      min-height: 300px;
    }

    .skeleton-line {
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      border-radius: 4px;
      margin-bottom: 0.75rem;
    }

    .skeleton-title {
      height: 24px;
      width: 60%;
    }

    .skeleton-subtitle {
      height: 16px;
      width: 80%;
    }

    .skeleton-info {
      height: 14px;
      width: 40%;
    }

    .skeleton-avatar {
      width: 80px;
      height: 80px;
      border-radius: 50%;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
    }

    .skeleton-icon {
      width: 48px;
      height: 48px;
      border-radius: 8px;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
    }

    .skeleton-stat-value {
      height: 32px;
      width: 80px;
    }

    .skeleton-stat-label {
      height: 16px;
      width: 120px;
    }

    .skeleton-chart-title {
      height: 20px;
      width: 200px;
      margin-bottom: 1rem;
    }

    .skeleton-chart-area {
      height: 200px;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      border-radius: 8px;
    }

    .skeleton-actions-title {
      height: 20px;
      width: 150px;
      margin-bottom: 1rem;
    }

    .skeleton-action {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .skeleton-action-icon {
      width: 32px;
      height: 32px;
      border-radius: 6px;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
    }

    .skeleton-action-text {
      height: 16px;
      width: 120px;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      border-radius: 4px;
    }

    @keyframes skeleton-loading {
      0% {
        background-position: -200% 0;
      }
      100% {
        background-position: 200% 0;
      }
    }

    .actual-dashboard {
      position: relative;
      z-index: 3;
    }

    /* Mobile Responsive */
    @media (max-width: 768px) {
      .progress-header {
        padding: 1rem;
        margin-bottom: 1rem;
      }

      .task-card {
        padding: 0.75rem;
      }

      .task-header {
        gap: 0.5rem;
      }

      .task-icon {
        width: 28px;
        height: 28px;
        font-size: 0.75rem;
      }

      .skeleton-card {
        padding: 1rem;
      }

      .skeleton-welcome {
        flex-direction: column;
        text-align: center;
        gap: 1rem;
      }

      .skeleton-stat {
        flex-direction: column;
        text-align: center;
        gap: 0.5rem;
      }
    }
  `]
})
export class ProgressiveDashboardComponent implements OnInit, OnDestroy {
  @Input() showProgress = true;
  @Input() showTaskDetails = false;
  @Input() showSkeletons = true;
  @Input() userRole = 'Employee';

  loadingStates = new Map<string, LoadingState>();
  overallProgress = 0;
  
  private destroy$ = new Subject<void>();
  private taskNames = new Map<string, string>();

  constructor(
    private progressiveLoadingService: ProgressiveLoadingService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.initializeProgressiveLoading();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.progressiveLoadingService.clear();
  }

  private initializeProgressiveLoading(): void {
    const currentUser = this.authService.currentUser;
    const userRole = currentUser?.roles?.[0] || this.userRole;

    // Create dashboard loading tasks
    const tasks = this.progressiveLoadingService.createDashboardTasks(userRole);
    
    // Store task names for display
    tasks.forEach(task => {
      this.taskNames.set(task.id, task.name);
    });

    // Add tasks to the service
    this.progressiveLoadingService.addTasks(tasks);

    // Subscribe to loading states
    this.progressiveLoadingService.loadingStates$
      .pipe(takeUntil(this.destroy$))
      .subscribe(states => {
        this.loadingStates = states;
      });

    // Subscribe to overall progress
    this.progressiveLoadingService.overallProgress$
      .pipe(takeUntil(this.destroy$))
      .subscribe(progress => {
        this.overallProgress = progress;
      });

    // Start loading
    this.progressiveLoadingService.startLoading()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (states) => {
          console.log('Dashboard loading completed:', states);
        },
        error: (error) => {
          console.error('Dashboard loading failed:', error);
        }
      });
  }

  getLoadingStatesArray(): LoadingState[] {
    return Array.from(this.loadingStates.values());
  }

  getTaskName(taskId: string): string {
    return this.taskNames.get(taskId) || taskId;
  }

  getTaskCardClass(state: LoadingState): string {
    return state.status;
  }

  getTaskIcon(state: LoadingState): string {
    switch (state.status) {
      case 'pending':
        return 'fas fa-clock';
      case 'loading':
        return 'fas fa-spinner fa-spin';
      case 'completed':
        return 'fas fa-check';
      case 'failed':
        return 'fas fa-times';
      case 'timeout':
        return 'fas fa-exclamation-triangle';
      default:
        return 'fas fa-question';
    }
  }

  getTaskIconClass(state: LoadingState): string {
    return state.status;
  }

  getStatusText(status: string): string {
    switch (status) {
      case 'pending':
        return 'Waiting';
      case 'loading':
        return 'Loading...';
      case 'completed':
        return 'Completed';
      case 'failed':
        return 'Failed';
      case 'timeout':
        return 'Timeout';
      default:
        return status;
    }
  }

  getDuration(startTime: number, endTime: number): number {
    return endTime - startTime;
  }

  getErrorMessage(error: any): string {
    if (typeof error === 'string') {
      return error;
    }
    return error?.message || 'Unknown error';
  }

  getLoadingMessage(): string {
    const completedTasks = Array.from(this.loadingStates.values())
      .filter(state => state.status === 'completed').length;
    const totalTasks = this.loadingStates.size;
    
    if (totalTasks === 0) {
      return 'Initializing...';
    }
    
    return `${completedTasks} of ${totalTasks} components loaded`;
  }

  retryTask(taskId: string): void {
    this.progressiveLoadingService.retryTask(taskId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (state) => {
          console.log('Task retried:', state);
        },
        error: (error) => {
          console.error('Task retry failed:', error);
        }
      });
  }
}