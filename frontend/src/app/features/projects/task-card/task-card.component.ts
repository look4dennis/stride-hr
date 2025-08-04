import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbTooltipModule, NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { Task, Project, TaskPriority, TaskStatus } from '../../../models/project.models';

@Component({
  selector: 'app-task-card',
  standalone: true,
  imports: [CommonModule, NgbTooltipModule, NgbDropdownModule],
  template: `
    <div class="task-card" 
         [class.high-priority]="task.priority === 'High' || task.priority === 'Critical'"
         [class.overdue]="isOverdue()"
         (click)="onTaskClick()">
      
      <!-- Priority indicator -->
      <div class="priority-indicator" [class]="getPriorityClass()"></div>
      
      <!-- Task header -->
      <div class="task-header d-flex justify-content-between align-items-start mb-2">
        <h6 class="task-title mb-0" [ngbTooltip]="task.title">
          {{ task.title | slice:0:40 }}{{ task.title.length > 40 ? '...' : '' }}
        </h6>
        
        <div class="task-actions dropdown" (click)="$event.stopPropagation()">
          <button class="btn btn-sm btn-link text-muted p-0" 
                  type="button" 
                  data-bs-toggle="dropdown"
                  [ngbTooltip]="'Task actions'">
            <i class="fas fa-ellipsis-v"></i>
          </button>
          <ul class="dropdown-menu dropdown-menu-end">
            <li>
              <a class="dropdown-item" href="#" (click)="onEditClick()">
                <i class="fas fa-edit me-2"></i> Edit
              </a>
            </li>
            <li>
              <a class="dropdown-item" href="#" (click)="onAssignClick()">
                <i class="fas fa-user me-2"></i> Assign
              </a>
            </li>
            <li><hr class="dropdown-divider"></li>
            <li>
              <a class="dropdown-item text-danger" href="#" (click)="onDeleteClick()">
                <i class="fas fa-trash me-2"></i> Delete
              </a>
            </li>
          </ul>
        </div>
      </div>

      <!-- Task description -->
      <p class="task-description text-muted mb-2" *ngIf="task.description">
        {{ task.description | slice:0:80 }}{{ task.description.length > 80 ? '...' : '' }}
      </p>

      <!-- Task metadata -->
      <div class="task-metadata mb-3">
        <!-- Priority badge -->
        <span class="badge me-2" [class]="getPriorityBadgeClass()">
          {{ task.priority }}
        </span>

        <!-- Hours info -->
        <small class="text-muted me-2" *ngIf="task.estimatedHours">
          <i class="fas fa-clock me-1"></i>
          {{ task.actualHours || 0 }}h / {{ task.estimatedHours }}h
        </small>

        <!-- Comments count -->
        <small class="text-muted me-2" *ngIf="task.comments && task.comments.length > 0">
          <i class="fas fa-comment me-1"></i>
          {{ task.comments.length }}
        </small>

        <!-- Attachments count -->
        <small class="text-muted" *ngIf="task.attachments && task.attachments.length > 0">
          <i class="fas fa-paperclip me-1"></i>
          {{ task.attachments.length }}
        </small>
      </div>

      <!-- Task footer -->
      <div class="task-footer d-flex justify-content-between align-items-center">
        <!-- Assignee -->
        <div class="assignee-info d-flex align-items-center">
          <img *ngIf="task.assignedToPhoto" 
               [src]="task.assignedToPhoto" 
               [alt]="task.assignedToName"
               class="assignee-avatar me-2"
               [ngbTooltip]="task.assignedToName">
          <div *ngIf="!task.assignedToPhoto && task.assignedToName" 
               class="assignee-avatar-placeholder me-2"
               [ngbTooltip]="task.assignedToName">
            {{ getInitials(task.assignedToName) }}
          </div>
          <span *ngIf="!task.assignedToName" class="text-muted small">
            <i class="fas fa-user-plus"></i> Unassigned
          </span>
        </div>

        <!-- Due date -->
        <div class="due-date" *ngIf="task.dueDate">
          <small [class]="getDueDateClass()" [ngbTooltip]="getDueDateTooltip()">
            <i class="fas fa-calendar-alt me-1"></i>
            {{ formatDueDate() }}
          </small>
        </div>
      </div>

      <!-- Progress bar -->
      <div class="progress mt-2" style="height: 4px;" *ngIf="showProgress()">
        <div class="progress-bar" 
             [style.width.%]="getProgress()"
             [class]="getProgressBarClass()">
        </div>
      </div>

      <!-- Blocked indicator -->
      <div *ngIf="task.status === 'Blocked'" class="blocked-indicator mt-2">
        <small class="text-danger">
          <i class="fas fa-exclamation-triangle me-1"></i>
          Blocked
        </small>
      </div>
    </div>
  `,
  styles: [`
    .task-card {
      background: white;
      border: 1px solid #e9ecef;
      border-radius: 8px;
      padding: 12px;
      margin-bottom: 8px;
      cursor: pointer;
      transition: all 0.2s ease;
      position: relative;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .task-card:hover {
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      transform: translateY(-1px);
    }

    .task-card.high-priority {
      border-left: 4px solid #dc3545;
    }

    .task-card.overdue {
      border-left: 4px solid #fd7e14;
      background-color: #fff8f0;
    }

    .priority-indicator {
      position: absolute;
      top: 0;
      right: 0;
      width: 0;
      height: 0;
      border-style: solid;
    }

    .priority-indicator.critical {
      border-width: 0 20px 20px 0;
      border-color: transparent #dc3545 transparent transparent;
    }

    .priority-indicator.high {
      border-width: 0 15px 15px 0;
      border-color: transparent #fd7e14 transparent transparent;
    }

    .task-title {
      font-weight: 600;
      color: #495057;
      line-height: 1.3;
    }

    .task-description {
      font-size: 0.875rem;
      line-height: 1.4;
    }

    .task-metadata {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 0.25rem;
    }

    .assignee-avatar {
      width: 24px;
      height: 24px;
      border-radius: 50%;
      object-fit: cover;
    }

    .assignee-avatar-placeholder {
      width: 24px;
      height: 24px;
      border-radius: 50%;
      background-color: #6c757d;
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.75rem;
      font-weight: 600;
    }

    .task-footer {
      margin-top: auto;
    }

    .due-date small {
      font-size: 0.75rem;
    }

    .due-date .text-danger {
      font-weight: 600;
    }

    .due-date .text-warning {
      font-weight: 500;
    }

    .blocked-indicator {
      background-color: #f8d7da;
      border: 1px solid #f5c6cb;
      border-radius: 4px;
      padding: 4px 8px;
      text-align: center;
    }

    .task-actions .btn {
      opacity: 0;
      transition: opacity 0.2s ease;
    }

    .task-card:hover .task-actions .btn {
      opacity: 1;
    }

    .dropdown-menu {
      font-size: 0.875rem;
    }

    .badge {
      font-size: 0.7rem;
      font-weight: 500;
    }

    .progress {
      border-radius: 2px;
    }

    .progress-bar {
      border-radius: 2px;
    }

    @media (max-width: 576px) {
      .task-card {
        padding: 10px;
      }
      
      .task-title {
        font-size: 0.9rem;
      }
      
      .task-description {
        font-size: 0.8rem;
      }
    }
  `]
})
export class TaskCardComponent {
  @Input() task!: Task;
  @Input() project?: Project;
  @Output() taskClick = new EventEmitter<Task>();
  @Output() taskUpdate = new EventEmitter<Task>();
  @Output() taskDelete = new EventEmitter<Task>();

  onTaskClick(): void {
    this.taskClick.emit(this.task);
  }

  onEditClick(): void {
    this.taskClick.emit(this.task);
  }

  onAssignClick(): void {
    // Implement assign functionality
    console.log('Assign task:', this.task);
  }

  onDeleteClick(): void {
    if (confirm(`Are you sure you want to delete "${this.task.title}"?`)) {
      this.taskDelete.emit(this.task);
    }
  }

  getPriorityClass(): string {
    switch (this.task.priority) {
      case TaskPriority.Critical:
        return 'critical';
      case TaskPriority.High:
        return 'high';
      default:
        return '';
    }
  }

  getPriorityBadgeClass(): string {
    const classes = {
      [TaskPriority.Low]: 'bg-light text-dark',
      [TaskPriority.Medium]: 'bg-info',
      [TaskPriority.High]: 'bg-warning',
      [TaskPriority.Critical]: 'bg-danger'
    };
    return classes[this.task.priority] || 'bg-light text-dark';
  }

  isOverdue(): boolean {
    if (!this.task.dueDate) return false;
    const today = new Date();
    const dueDate = new Date(this.task.dueDate);
    return dueDate < today && this.task.status !== TaskStatus.Done;
  }

  getDueDateClass(): string {
    if (!this.task.dueDate) return 'text-muted';
    
    const today = new Date();
    const dueDate = new Date(this.task.dueDate);
    const diffTime = dueDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays < 0) return 'text-danger'; // Overdue
    if (diffDays <= 1) return 'text-warning'; // Due soon
    if (diffDays <= 3) return 'text-info'; // Due this week
    return 'text-muted'; // Normal
  }

  getDueDateTooltip(): string {
    if (!this.task.dueDate) return '';
    
    const today = new Date();
    const dueDate = new Date(this.task.dueDate);
    const diffTime = dueDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays < 0) return `Overdue by ${Math.abs(diffDays)} day(s)`;
    if (diffDays === 0) return 'Due today';
    if (diffDays === 1) return 'Due tomorrow';
    return `Due in ${diffDays} day(s)`;
  }

  formatDueDate(): string {
    if (!this.task.dueDate) return '';
    
    const dueDate = new Date(this.task.dueDate);
    const today = new Date();
    const diffTime = dueDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays === 0) return 'Today';
    if (diffDays === 1) return 'Tomorrow';
    if (diffDays === -1) return 'Yesterday';
    if (diffDays < -1) return `${Math.abs(diffDays)}d ago`;
    if (diffDays <= 7) return `${diffDays}d`;
    
    return dueDate.toLocaleDateString('en-US', { 
      month: 'short', 
      day: 'numeric' 
    });
  }

  getInitials(name: string): string {
    if (!name) return '?';
    return name
      .split(' ')
      .map(word => word.charAt(0).toUpperCase())
      .slice(0, 2)
      .join('');
  }

  showProgress(): boolean {
    return this.task.estimatedHours > 0 || this.task.status !== TaskStatus.Todo;
  }

  getProgress(): number {
    if (this.task.estimatedHours > 0 && this.task.actualHours > 0) {
      return Math.min((this.task.actualHours / this.task.estimatedHours) * 100, 100);
    }
    
    // Progress based on status
    const statusProgress = {
      [TaskStatus.Todo]: 0,
      [TaskStatus.InProgress]: 50,
      [TaskStatus.InReview]: 80,
      [TaskStatus.Done]: 100,
      [TaskStatus.Blocked]: 25
    };
    
    return statusProgress[this.task.status] || 0;
  }

  getProgressBarClass(): string {
    const progress = this.getProgress();
    
    if (this.task.status === TaskStatus.Done) return 'bg-success';
    if (this.task.status === TaskStatus.Blocked) return 'bg-danger';
    if (progress >= 80) return 'bg-success';
    if (progress >= 50) return 'bg-warning';
    if (progress >= 25) return 'bg-info';
    return 'bg-secondary';
  }
}