import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal, NgbDatepickerModule, NgbCalendar, NgbDate } from '@ng-bootstrap/ng-bootstrap';

import { ProjectService } from '../../../services/project.service';
import { EmployeeService } from '../../../services/employee.service';
import { 
  Task, 
  Project, 
  TaskStatus, 
  TaskPriority, 
  CreateTaskDto, 
  UpdateTaskDto 
} from '../../../models/project.models';
import { Employee } from '../../../models/employee.models';

@Component({
    selector: 'app-task-modal',
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        NgbDatepickerModule
    ],
    template: `
    <div class="modal-header">
      <h4 class="modal-title">
        <i class="fas fa-tasks me-2"></i>
        {{ task ? 'Edit Task' : 'Create New Task' }}
      </h4>
      <button type="button" class="btn-close" (click)="activeModal.dismiss()"></button>
    </div>

    <form [formGroup]="taskForm" (ngSubmit)="onSubmit()">
      <div class="modal-body">
        <!-- Task Title -->
        <div class="mb-3">
          <label for="title" class="form-label">
            Task Title <span class="text-danger">*</span>
          </label>
          <input type="text" 
                 id="title"
                 class="form-control" 
                 formControlName="title"
                 placeholder="Enter task title"
                 [class.is-invalid]="isFieldInvalid('title')">
          <div class="invalid-feedback" *ngIf="isFieldInvalid('title')">
            Task title is required
          </div>
        </div>

        <!-- Task Description -->
        <div class="mb-3">
          <label for="description" class="form-label">Description</label>
          <textarea id="description"
                    class="form-control" 
                    formControlName="description"
                    rows="3"
                    placeholder="Enter task description"></textarea>
        </div>

        <!-- Row 1: Priority and Status -->
        <div class="row mb-3">
          <div class="col-md-6">
            <label for="priority" class="form-label">
              Priority <span class="text-danger">*</span>
            </label>
            <select id="priority" 
                    class="form-select" 
                    formControlName="priority"
                    [class.is-invalid]="isFieldInvalid('priority')">
              <option value="">Select Priority</option>
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
              <option value="Critical">Critical</option>
            </select>
            <div class="invalid-feedback" *ngIf="isFieldInvalid('priority')">
              Priority is required
            </div>
          </div>
          
          <div class="col-md-6" *ngIf="task">
            <label for="status" class="form-label">Status</label>
            <select id="status" class="form-select" formControlName="status">
              <option value="Todo">To Do</option>
              <option value="InProgress">In Progress</option>
              <option value="InReview">In Review</option>
              <option value="Done">Done</option>
              <option value="Blocked">Blocked</option>
            </select>
          </div>
        </div>

        <!-- Row 2: Assignee and Estimated Hours -->
        <div class="row mb-3">
          <div class="col-md-6">
            <label for="assignedTo" class="form-label">Assign To</label>
            <select id="assignedTo" class="form-select" formControlName="assignedTo">
              <option value="">Select Assignee</option>
              <option *ngFor="let member of teamMembers" [value]="member.id">
                {{ member.firstName }} {{ member.lastName }}
              </option>
            </select>
          </div>
          
          <div class="col-md-6">
            <label for="estimatedHours" class="form-label">
              Estimated Hours <span class="text-danger">*</span>
            </label>
            <input type="number" 
                   id="estimatedHours"
                   class="form-control" 
                   formControlName="estimatedHours"
                   min="0.5"
                   step="0.5"
                   placeholder="0.0"
                   [class.is-invalid]="isFieldInvalid('estimatedHours')">
            <div class="invalid-feedback" *ngIf="isFieldInvalid('estimatedHours')">
              Estimated hours is required and must be greater than 0
            </div>
          </div>
        </div>

        <!-- Due Date -->
        <div class="mb-3">
          <label class="form-label">Due Date</label>
          <div class="input-group">
            <input class="form-control" 
                   placeholder="Select due date"
                   name="dueDate"
                   [(ngModel)]="dueDateModel"
                   [ngModelOptions]="{standalone: true}"
                   ngbDatepicker 
                   #dueDatePicker="ngbDatepicker"
                   readonly>
            <button class="btn btn-outline-secondary" 
                    type="button" 
                    (click)="dueDatePicker.toggle()">
              <i class="fas fa-calendar-alt"></i>
            </button>
            <button class="btn btn-outline-secondary" 
                    type="button" 
                    (click)="clearDueDate()"
                    *ngIf="dueDateModel">
              <i class="fas fa-times"></i>
            </button>
          </div>
          <ngb-datepicker #dueDatePickerPopup 
                          [(ngModel)]="dueDateModel"
                          [ngModelOptions]="{standalone: true}"
                          [minDate]="minDate"
                          [startDate]="startDate">
          </ngb-datepicker>
        </div>

        <!-- Task Progress (for existing tasks) -->
        <div class="mb-3" *ngIf="task">
          <label class="form-label">Progress</label>
          <div class="progress mb-2" style="height: 20px;">
            <div class="progress-bar" 
                 [style.width.%]="getTaskProgress()"
                 [class]="getProgressBarClass()">
              {{ getTaskProgress() }}%
            </div>
          </div>
          <small class="text-muted">
            <i class="fas fa-clock me-1"></i>
            {{ task.actualHours || 0 }}h worked of {{ task.estimatedHours }}h estimated
          </small>
        </div>

        <!-- Comments Section (for existing tasks) -->
        <div class="mb-3" *ngIf="task && task.comments && task.comments.length > 0">
          <label class="form-label">Recent Comments</label>
          <div class="comments-list">
            <div class="comment-item" 
                 *ngFor="let comment of task.comments.slice(-3)">
              <div class="d-flex align-items-start">
                <img *ngIf="comment.employeePhoto" 
                     [src]="comment.employeePhoto" 
                     [alt]="comment.employeeName"
                     class="comment-avatar me-2">
                <div *ngIf="!comment.employeePhoto" 
                     class="comment-avatar-placeholder me-2">
                  {{ getInitials(comment.employeeName) }}
                </div>
                <div class="flex-grow-1">
                  <div class="d-flex justify-content-between align-items-center mb-1">
                    <strong class="comment-author">{{ comment.employeeName }}</strong>
                    <small class="text-muted">{{ formatDate(comment.createdAt) }}</small>
                  </div>
                  <p class="comment-text mb-0">{{ comment.comment }}</p>
                </div>
              </div>
            </div>
          </div>
          <small class="text-muted" *ngIf="task.comments.length > 3">
            And {{ task.comments.length - 3 }} more comments...
          </small>
        </div>

        <!-- Add Comment -->
        <div class="mb-3" *ngIf="task">
          <label for="newComment" class="form-label">Add Comment</label>
          <div class="input-group">
            <textarea id="newComment"
                      class="form-control" 
                      [(ngModel)]="newComment"
                      [ngModelOptions]="{standalone: true}"
                      rows="2"
                      placeholder="Add a comment..."></textarea>
            <button class="btn btn-outline-primary" 
                    type="button" 
                    (click)="addComment()"
                    [disabled]="!newComment?.trim()">
              <i class="fas fa-paper-plane"></i>
            </button>
          </div>
        </div>

        <!-- Validation Summary -->
        <div class="alert alert-danger" *ngIf="taskForm.invalid && submitted">
          <h6><i class="fas fa-exclamation-triangle me-2"></i>Please fix the following errors:</h6>
          <ul class="mb-0">
            <li *ngIf="taskForm.get('title')?.invalid">Task title is required</li>
            <li *ngIf="taskForm.get('priority')?.invalid">Priority is required</li>
            <li *ngIf="taskForm.get('estimatedHours')?.invalid">
              Estimated hours is required and must be greater than 0
            </li>
          </ul>
        </div>
      </div>

      <div class="modal-footer">
        <button type="button" 
                class="btn btn-secondary" 
                (click)="activeModal.dismiss()">
          Cancel
        </button>
        <button type="submit" 
                class="btn btn-primary"
                [disabled]="loading">
          <span *ngIf="loading" class="spinner-border spinner-border-sm me-2"></span>
          <i *ngIf="!loading" class="fas fa-save me-2"></i>
          {{ task ? 'Update Task' : 'Create Task' }}
        </button>
      </div>
    </form>
  `,
    styles: [`
    .modal-header {
      background: linear-gradient(135deg, #0d6efd 0%, #0b5ed7 100%);
      color: white;
      border-bottom: none;
    }

    .modal-header .btn-close {
      filter: invert(1);
    }

    .modal-title {
      font-weight: 600;
    }

    .form-label {
      font-weight: 500;
      color: #495057;
    }

    .text-danger {
      font-weight: 500;
    }

    .comments-list {
      max-height: 200px;
      overflow-y: auto;
      border: 1px solid #dee2e6;
      border-radius: 6px;
      padding: 0.75rem;
      background-color: #f8f9fa;
    }

    .comment-item {
      margin-bottom: 1rem;
    }

    .comment-item:last-child {
      margin-bottom: 0;
    }

    .comment-avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      object-fit: cover;
    }

    .comment-avatar-placeholder {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background-color: #6c757d;
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.75rem;
      font-weight: 600;
    }

    .comment-author {
      font-size: 0.875rem;
      color: #495057;
    }

    .comment-text {
      font-size: 0.875rem;
      line-height: 1.4;
      color: #6c757d;
    }

    .progress {
      border-radius: 10px;
    }

    .progress-bar {
      border-radius: 10px;
      font-size: 0.75rem;
      font-weight: 600;
    }

    .alert {
      border-radius: 8px;
    }

    .alert ul {
      padding-left: 1.25rem;
    }

    .input-group .btn {
      border-left: none;
    }

    .form-control:focus + .btn,
    .form-control:focus ~ .btn {
      border-color: #86b7fe;
    }

    @media (max-width: 576px) {
      .modal-body {
        padding: 1rem;
      }
      
      .row > .col-md-6 {
        margin-bottom: 1rem;
      }
    }
  `]
})
export class TaskModalComponent implements OnInit {
  @Input() task?: Task;
  @Input() project?: Project;
  @Input() defaultStatus?: TaskStatus;

  taskForm: FormGroup;
  teamMembers: Employee[] = [];
  loading = false;
  submitted = false;
  newComment = '';

  // Date picker properties
  dueDateModel: NgbDate | null = null;
  minDate: NgbDate;
  startDate: NgbDate;

  constructor(
    public activeModal: NgbActiveModal,
    private fb: FormBuilder,
    private projectService: ProjectService,
    private employeeService: EmployeeService,
    private calendar: NgbCalendar
  ) {
    this.minDate = calendar.getToday();
    this.startDate = calendar.getToday();
    
    this.taskForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(1000)]],
      priority: ['', Validators.required],
      status: [this.defaultStatus || TaskStatus.Todo],
      assignedTo: [''],
      estimatedHours: ['', [Validators.required, Validators.min(0.5)]]
    });
  }

  ngOnInit(): void {
    this.loadTeamMembers();
    
    if (this.task) {
      this.populateForm();
    } else if (this.defaultStatus) {
      this.taskForm.patchValue({ status: this.defaultStatus });
    }
  }

  private loadTeamMembers(): void {
    if (!this.project) return;

    // Load team members for the project
    this.employeeService.getEmployees({
      page: 1,
      pageSize: 100,
      searchTerm: '',
      department: '',
      status: 'Active' as any
    }).subscribe({
      next: (response) => {
        this.teamMembers = response.items || [];
      },
      error: (error) => {
        console.error('Error loading team members:', error);
      }
    });
  }

  private populateForm(): void {
    if (!this.task) return;

    this.taskForm.patchValue({
      title: this.task.title,
      description: this.task.description,
      priority: this.task.priority,
      status: this.task.status,
      assignedTo: this.task.assignedTo,
      estimatedHours: this.task.estimatedHours
    });

    // Set due date
    if (this.task.dueDate) {
      const dueDate = new Date(this.task.dueDate);
      this.dueDateModel = {
        year: dueDate.getFullYear(),
        month: dueDate.getMonth() + 1,
        day: dueDate.getDate()
      } as any;
    }
  }

  onSubmit(): void {
    this.submitted = true;
    
    if (this.taskForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.loading = true;
    const formValue = this.taskForm.value;

    // Convert due date
    let dueDate: Date | undefined;
    if (this.dueDateModel) {
      dueDate = new Date(
        this.dueDateModel.year,
        this.dueDateModel.month - 1,
        this.dueDateModel.day
      );
    }

    if (this.task) {
      // Update existing task
      const updateDto: UpdateTaskDto = {
        title: formValue.title,
        description: formValue.description,
        priority: formValue.priority,
        status: formValue.status,
        assignedTo: formValue.assignedTo || undefined,
        estimatedHours: formValue.estimatedHours,
        dueDate: dueDate
      };

      this.projectService.updateTask(this.task.id, updateDto).subscribe({
        next: (updatedTask) => {
          this.loading = false;
          this.activeModal.close(updatedTask);
        },
        error: (error) => {
          console.error('Error updating task:', error);
          this.loading = false;
        }
      });
    } else {
      // Create new task
      const createDto: CreateTaskDto = {
        projectId: this.project!.id,
        title: formValue.title,
        description: formValue.description,
        priority: formValue.priority,
        assignedTo: formValue.assignedTo || 0,
        estimatedHours: formValue.estimatedHours,
        dueDate: dueDate
      };

      this.projectService.createTask(createDto).subscribe({
        next: (newTask) => {
          this.loading = false;
          this.activeModal.close(newTask);
        },
        error: (error) => {
          console.error('Error creating task:', error);
          this.loading = false;
        }
      });
    }
  }

  addComment(): void {
    if (!this.newComment?.trim() || !this.task) return;

    this.projectService.addTaskComment(this.task.id, this.newComment.trim()).subscribe({
      next: () => {
        this.newComment = '';
        // Reload task to get updated comments
        this.projectService.getTask(this.task!.id).subscribe({
          next: (updatedTask) => {
            this.task = updatedTask;
          }
        });
      },
      error: (error) => {
        console.error('Error adding comment:', error);
      }
    });
  }

  clearDueDate(): void {
    this.dueDateModel = null;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.taskForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched || this.submitted));
  }

  private markFormGroupTouched(): void {
    Object.keys(this.taskForm.controls).forEach(key => {
      const control = this.taskForm.get(key);
      control?.markAsTouched();
    });
  }

  getTaskProgress(): number {
    if (!this.task) return 0;
    
    if (this.task.estimatedHours > 0 && this.task.actualHours > 0) {
      return Math.min((this.task.actualHours / this.task.estimatedHours) * 100, 100);
    }
    
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
    if (!this.task) return 'bg-secondary';
    
    const progress = this.getTaskProgress();
    
    if (this.task.status === TaskStatus.Done) return 'bg-success';
    if (this.task.status === TaskStatus.Blocked) return 'bg-danger';
    if (progress >= 80) return 'bg-success';
    if (progress >= 50) return 'bg-warning';
    if (progress >= 25) return 'bg-info';
    return 'bg-secondary';
  }

  getInitials(name: string): string {
    if (!name) return '?';
    return name
      .split(' ')
      .map(word => word.charAt(0).toUpperCase())
      .slice(0, 2)
      .join('');
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}