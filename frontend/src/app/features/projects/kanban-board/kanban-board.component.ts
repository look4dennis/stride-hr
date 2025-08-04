import { Component, OnInit, OnDestroy, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { NgbModal, NgbDropdownModule, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { Subject, takeUntil } from 'rxjs';

import { ProjectService } from '../../../services/project.service';
import { 
  Project, 
  Task, 
  KanbanColumn, 
  TaskStatus, 
  TaskPriority,
  CreateTaskDto,
  UpdateTaskDto
} from '../../../models/project.models';
import { TaskCardComponent } from '../task-card/task-card.component';
import { TaskModalComponent } from '../task-modal/task-modal.component';

@Component({
    selector: 'app-kanban-board',
    imports: [
        CommonModule,
        FormsModule,
        DragDropModule,
        NgbDropdownModule,
        NgbTooltipModule,
        TaskCardComponent
    ],
    template: `
    <div class="kanban-container">
      <!-- Header with controls -->
      <div class="kanban-header d-flex justify-content-between align-items-center mb-4">
        <div class="d-flex align-items-center">
          <h3 class="mb-0 me-3">{{ project?.name || 'Project Board' }}</h3>
          <div class="view-toggle btn-group" role="group">
            <input type="radio" class="btn-check" name="viewMode" id="kanban-view" 
                   [checked]="viewMode === 'kanban'" (change)="setViewMode('kanban')">
            <label class="btn btn-outline-primary" for="kanban-view">
              <i class="fas fa-columns me-1"></i> Kanban
            </label>
            
            <input type="radio" class="btn-check" name="viewMode" id="list-view" 
                   [checked]="viewMode === 'list'" (change)="setViewMode('list')">
            <label class="btn btn-outline-primary" for="list-view">
              <i class="fas fa-list me-1"></i> List
            </label>
          </div>
        </div>
        
        <div class="kanban-actions">
          <button class="btn btn-success me-2" (click)="openTaskModal()">
            <i class="fas fa-plus me-1"></i> Add Task
          </button>
          <div class="dropdown">
            <button class="btn btn-outline-secondary dropdown-toggle" 
                    type="button" data-bs-toggle="dropdown">
              <i class="fas fa-filter me-1"></i> Filter
            </button>
            <ul class="dropdown-menu">
              <li><h6 class="dropdown-header">Priority</h6></li>
              <li><a class="dropdown-item" href="#" (click)="filterByPriority('All')">All</a></li>
              <li><a class="dropdown-item" href="#" (click)="filterByPriority('Critical')">Critical</a></li>
              <li><a class="dropdown-item" href="#" (click)="filterByPriority('High')">High</a></li>
              <li><a class="dropdown-item" href="#" (click)="filterByPriority('Medium')">Medium</a></li>
              <li><a class="dropdown-item" href="#" (click)="filterByPriority('Low')">Low</a></li>
              <li><hr class="dropdown-divider"></li>
              <li><h6 class="dropdown-header">Assigned To</h6></li>
              <li><a class="dropdown-item" href="#" (click)="filterByAssignee('All')">All</a></li>
              <li><a class="dropdown-item" href="#" (click)="filterByAssignee('Me')">My Tasks</a></li>
            </ul>
          </div>
        </div>
      </div>

      <!-- Kanban Board View -->
      <div *ngIf="viewMode === 'kanban'" class="kanban-board">
        <div class="kanban-columns" 
             cdkDropListGroup>
          <div class="kanban-column" 
               *ngFor="let column of filteredColumns; trackBy: trackByColumnId">
            <div class="column-header" [style.border-left-color]="column.color">
              <div class="d-flex justify-content-between align-items-center">
                <h5 class="column-title mb-0">
                  {{ column.title }}
                  <span class="task-count badge bg-secondary ms-2">
                    {{ column.tasks.length }}
                  </span>
                  <span *ngIf="column.limit" class="limit-indicator text-muted ms-1">
                    / {{ column.limit }}
                  </span>
                </h5>
                <div class="column-actions">
                  <button class="btn btn-sm btn-outline-primary" 
                          (click)="openTaskModal(column.status)"
                          [ngbTooltip]="'Add task to ' + column.title">
                    <i class="fas fa-plus"></i>
                  </button>
                </div>
              </div>
              
              <!-- Column limit warning -->
              <div *ngIf="column.limit && column.tasks.length >= column.limit" 
                   class="alert alert-warning alert-sm mt-2 mb-0">
                <i class="fas fa-exclamation-triangle me-1"></i>
                Column limit reached
              </div>
            </div>

            <div class="column-content"
                 cdkDropList
                 [cdkDropListData]="column.tasks"
                 [cdkDropListConnectedTo]="getConnectedLists()"
                 (cdkDropListDropped)="onTaskDrop($event, column)">
              
              <app-task-card 
                *ngFor="let task of column.tasks; trackBy: trackByTaskId"
                [task]="task"
                [project]="project"
                (taskClick)="openTaskModal(undefined, task)"
                (taskUpdate)="onTaskUpdate($event)"
                cdkDrag
                [cdkDragData]="task">
              </app-task-card>

              <!-- Empty state -->
              <div *ngIf="column.tasks.length === 0" class="empty-column">
                <i class="fas fa-inbox text-muted mb-2"></i>
                <p class="text-muted mb-0">No tasks</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- List View -->
      <div *ngIf="viewMode === 'list'" class="list-view">
        <div class="list-filters mb-3">
          <div class="row">
            <div class="col-md-4">
              <input type="text" class="form-control" placeholder="Search tasks..." 
                     [(ngModel)]="searchTerm" (input)="applyFilters()">
            </div>
            <div class="col-md-2">
              <select class="form-select" [(ngModel)]="selectedStatus" (change)="applyFilters()">
                <option value="">All Status</option>
                <option value="Todo">To Do</option>
                <option value="InProgress">In Progress</option>
                <option value="InReview">In Review</option>
                <option value="Done">Done</option>
                <option value="Blocked">Blocked</option>
              </select>
            </div>
            <div class="col-md-2">
              <select class="form-select" [(ngModel)]="selectedPriority" (change)="applyFilters()">
                <option value="">All Priority</option>
                <option value="Critical">Critical</option>
                <option value="High">High</option>
                <option value="Medium">Medium</option>
                <option value="Low">Low</option>
              </select>
            </div>
            <div class="col-md-2">
              <select class="form-select" [(ngModel)]="selectedAssignee" (change)="applyFilters()">
                <option value="">All Assignees</option>
                <option value="me">My Tasks</option>
                <!-- Add team members dynamically -->
              </select>
            </div>
            <div class="col-md-2">
              <button class="btn btn-outline-secondary w-100" (click)="clearFilters()">
                <i class="fas fa-times me-1"></i> Clear
              </button>
            </div>
          </div>
        </div>

        <div class="task-list">
          <div class="table-responsive">
            <table class="table table-hover">
              <thead>
                <tr>
                  <th>Task</th>
                  <th>Status</th>
                  <th>Priority</th>
                  <th>Assignee</th>
                  <th>Due Date</th>
                  <th>Progress</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let task of filteredTasks; trackBy: trackByTaskId" 
                    class="task-row" (click)="openTaskModal(undefined, task)">
                  <td>
                    <div class="task-info">
                      <h6 class="mb-1">{{ task.title }}</h6>
                      <small class="text-muted">{{ task.description | slice:0:50 }}...</small>
                    </div>
                  </td>
                  <td>
                    <span class="badge" [class]="getStatusBadgeClass(task.status)">
                      {{ getStatusLabel(task.status) }}
                    </span>
                  </td>
                  <td>
                    <span class="badge" [class]="getPriorityBadgeClass(task.priority)">
                      {{ task.priority }}
                    </span>
                  </td>
                  <td>
                    <div class="assignee-info" *ngIf="task.assignedToName">
                      <img *ngIf="task.assignedToPhoto" 
                           [src]="task.assignedToPhoto" 
                           [alt]="task.assignedToName"
                           class="assignee-avatar me-2">
                      <span>{{ task.assignedToName }}</span>
                    </div>
                    <span *ngIf="!task.assignedToName" class="text-muted">Unassigned</span>
                  </td>
                  <td>
                    <span *ngIf="task.dueDate" [class]="getDueDateClass(task.dueDate)">
                      {{ task.dueDate | date:'MMM dd, yyyy' }}
                    </span>
                    <span *ngIf="!task.dueDate" class="text-muted">No due date</span>
                  </td>
                  <td>
                    <div class="progress" style="height: 8px;">
                      <div class="progress-bar" 
                           [style.width.%]="getTaskProgress(task)"
                           [class]="getProgressBarClass(task.status)">
                      </div>
                    </div>
                    <small class="text-muted">{{ getTaskProgress(task) }}%</small>
                  </td>
                  <td>
                    <div class="btn-group btn-group-sm">
                      <button class="btn btn-outline-primary" 
                              (click)="$event.stopPropagation(); openTaskModal(undefined, task)"
                              [ngbTooltip]="'Edit task'">
                        <i class="fas fa-edit"></i>
                      </button>
                      <button class="btn btn-outline-danger" 
                              (click)="$event.stopPropagation(); deleteTask(task)"
                              [ngbTooltip]="'Delete task'">
                        <i class="fas fa-trash"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Empty state for list view -->
          <div *ngIf="filteredTasks.length === 0" class="text-center py-5">
            <i class="fas fa-tasks text-muted mb-3" style="font-size: 3rem;"></i>
            <h4>No tasks found</h4>
            <p class="text-muted">Try adjusting your filters or create a new task.</p>
            <button class="btn btn-primary" (click)="openTaskModal()">
              <i class="fas fa-plus me-1"></i> Create Task
            </button>
          </div>
        </div>
      </div>

      <!-- Loading state -->
      <div *ngIf="loading" class="text-center py-5">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <p class="mt-2 text-muted">Loading project data...</p>
      </div>
    </div>
  `,
    styles: [`
    .kanban-container {
      height: calc(100vh - 200px);
      overflow: hidden;
    }

    .kanban-header {
      background: white;
      padding: 1rem;
      border-bottom: 1px solid #dee2e6;
      position: sticky;
      top: 0;
      z-index: 10;
    }

    .kanban-board {
      height: 100%;
      overflow: hidden;
    }

    .kanban-columns {
      display: flex;
      gap: 1rem;
      padding: 1rem;
      height: 100%;
      overflow-x: auto;
      overflow-y: hidden;
    }

    .kanban-column {
      flex: 0 0 300px;
      background: #f8f9fa;
      border-radius: 8px;
      display: flex;
      flex-direction: column;
      height: 100%;
    }

    .column-header {
      padding: 1rem;
      border-left: 4px solid;
      background: white;
      border-radius: 8px 8px 0 0;
      border-bottom: 1px solid #dee2e6;
    }

    .column-title {
      font-weight: 600;
      color: #495057;
    }

    .task-count {
      font-size: 0.75rem;
    }

    .column-content {
      flex: 1;
      padding: 1rem;
      overflow-y: auto;
      min-height: 200px;
    }

    .empty-column {
      text-align: center;
      padding: 2rem 1rem;
      color: #6c757d;
    }

    .empty-column i {
      font-size: 2rem;
      display: block;
    }

    .cdk-drag-preview {
      box-sizing: border-box;
      border-radius: 8px;
      box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
      transform: rotate(2deg);
    }

    .cdk-drag-placeholder {
      opacity: 0.4;
      border: 2px dashed #ccc;
      background: transparent;
    }

    .cdk-drag-animating {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }

    .cdk-drop-list-dragging .cdk-drag:not(.cdk-drag-placeholder) {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }

    .list-view {
      padding: 1rem;
    }

    .task-row {
      cursor: pointer;
    }

    .task-row:hover {
      background-color: #f8f9fa;
    }

    .assignee-avatar {
      width: 24px;
      height: 24px;
      border-radius: 50%;
      object-fit: cover;
    }

    .alert-sm {
      padding: 0.25rem 0.5rem;
      font-size: 0.75rem;
    }

    .limit-indicator {
      font-size: 0.75rem;
    }

    .view-toggle .btn-check:checked + .btn {
      background-color: #0d6efd;
      border-color: #0d6efd;
      color: white;
    }

    @media (max-width: 768px) {
      .kanban-columns {
        flex-direction: column;
        height: auto;
      }
      
      .kanban-column {
        flex: none;
        height: 400px;
      }
    }
  `]
})
export class KanbanBoardComponent implements OnInit, OnDestroy {
  @Input() project: Project | null = null;
  @Input() projectId: number | null = null;
  @Output() taskCreated = new EventEmitter<Task>();
  @Output() taskUpdated = new EventEmitter<Task>();
  @Output() taskDeleted = new EventEmitter<number>();

  viewMode: 'kanban' | 'list' = 'kanban';
  columns: KanbanColumn[] = [];
  filteredColumns: KanbanColumn[] = [];
  allTasks: Task[] = [];
  filteredTasks: Task[] = [];
  loading = false;

  // Filters
  searchTerm = '';
  selectedStatus = '';
  selectedPriority = '';
  selectedAssignee = '';
  priorityFilter = 'All';
  assigneeFilter = 'All';

  private destroy$ = new Subject<void>();

  constructor(
    private projectService: ProjectService,
    private modalService: NgbModal
  ) {}

  ngOnInit(): void {
    this.initializeKanbanBoard();
    this.subscribeToUpdates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeKanbanBoard(): void {
    this.columns = this.projectService.getDefaultKanbanColumns();
    this.loadTasks();
  }

  private loadTasks(): void {
    if (!this.projectId && !this.project?.id) return;

    this.loading = true;
    const id = this.projectId || this.project!.id;

    this.projectService.getTasks({
      projectId: id,
      page: 1,
      pageSize: 1000 // Load all tasks for Kanban
    }).pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (response) => {
        this.allTasks = response.tasks;
        this.distributeTasks();
        this.applyFilters();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading tasks:', error);
        this.loading = false;
      }
    });
  }

  private distributeTasks(): void {
    // Reset all columns
    this.columns.forEach(column => column.tasks = []);

    // Distribute tasks to appropriate columns
    this.allTasks.forEach(task => {
      const column = this.columns.find(col => col.status === task.status);
      if (column) {
        column.tasks.push(task);
      }
    });

    this.filteredColumns = [...this.columns];
  }

  private subscribeToUpdates(): void {
    this.projectService.kanbanUpdate$
      .pipe(takeUntil(this.destroy$))
      .subscribe(update => {
        if (update) {
          this.loadTasks(); // Reload tasks on updates
        }
      });
  }

  setViewMode(mode: 'kanban' | 'list'): void {
    this.viewMode = mode;
    if (mode === 'list') {
      this.filteredTasks = [...this.allTasks];
      this.applyFilters();
    }
  }

  onTaskDrop(event: CdkDragDrop<Task[]>, targetColumn: KanbanColumn): void {
    const task = event.item.data as Task;
    
    if (event.previousContainer === event.container) {
      // Reordering within the same column
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      // Moving between columns
      const previousColumn = this.columns.find(col => 
        col.tasks === event.previousContainer.data
      );
      
      // Check column limits
      if (targetColumn.limit && targetColumn.tasks.length >= targetColumn.limit) {
        // Show warning and prevent drop
        alert(`Cannot move task. ${targetColumn.title} has reached its limit of ${targetColumn.limit} tasks.`);
        return;
      }

      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );

      // Update task status
      const updatedTask: UpdateTaskDto = {
        status: targetColumn.status
      };

      this.projectService.updateTask(task.id, updatedTask)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (updated) => {
            task.status = updated.status;
            this.taskUpdated.emit(updated);
            this.projectService.notifyKanbanUpdate({ type: 'task_moved', task: updated });
          },
          error: (error) => {
            console.error('Error updating task status:', error);
            // Revert the move on error
            this.distributeTasks();
          }
        });
    }
  }

  openTaskModal(defaultStatus?: TaskStatus, task?: Task): void {
    const modalRef = this.modalService.open(TaskModalComponent, {
      size: 'lg',
      backdrop: 'static'
    });

    modalRef.componentInstance.project = this.project;
    modalRef.componentInstance.task = task;
    modalRef.componentInstance.defaultStatus = defaultStatus;

    modalRef.result.then((result) => {
      if (result) {
        if (task) {
          this.taskUpdated.emit(result);
        } else {
          this.taskCreated.emit(result);
        }
        this.loadTasks();
      }
    }).catch(() => {
      // Modal dismissed
    });
  }

  onTaskUpdate(task: Task): void {
    this.taskUpdated.emit(task);
    this.loadTasks();
  }

  deleteTask(task: Task): void {
    if (confirm(`Are you sure you want to delete "${task.title}"?`)) {
      this.projectService.deleteTask(task.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.taskDeleted.emit(task.id);
            this.loadTasks();
          },
          error: (error) => {
            console.error('Error deleting task:', error);
          }
        });
    }
  }

  // Filter methods
  filterByPriority(priority: string): void {
    this.priorityFilter = priority;
    this.applyFilters();
  }

  filterByAssignee(assignee: string): void {
    this.assigneeFilter = assignee;
    this.applyFilters();
  }

  applyFilters(): void {
    if (this.viewMode === 'kanban') {
      this.filteredColumns = this.columns.map(column => ({
        ...column,
        tasks: this.filterTasks(column.tasks)
      }));
    } else {
      this.filteredTasks = this.filterTasks(this.allTasks);
    }
  }

  private filterTasks(tasks: Task[]): Task[] {
    return tasks.filter(task => {
      // Search term filter
      if (this.searchTerm && !task.title.toLowerCase().includes(this.searchTerm.toLowerCase()) &&
          !task.description.toLowerCase().includes(this.searchTerm.toLowerCase())) {
        return false;
      }

      // Status filter
      if (this.selectedStatus && task.status !== this.selectedStatus) {
        return false;
      }

      // Priority filter
      if (this.priorityFilter !== 'All' && task.priority !== this.priorityFilter) {
        return false;
      }
      if (this.selectedPriority && task.priority !== this.selectedPriority) {
        return false;
      }

      // Assignee filter
      if (this.assigneeFilter === 'Me' && task.assignedTo !== this.getCurrentUserId()) {
        return false;
      }
      if (this.selectedAssignee === 'me' && task.assignedTo !== this.getCurrentUserId()) {
        return false;
      }

      return true;
    });
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = '';
    this.selectedPriority = '';
    this.selectedAssignee = '';
    this.priorityFilter = 'All';
    this.assigneeFilter = 'All';
    this.applyFilters();
  }

  // Utility methods
  getConnectedLists(): string[] {
    return this.columns.map(column => column.id);
  }

  trackByColumnId(index: number, column: KanbanColumn): string {
    return column.id;
  }

  trackByTaskId(index: number, task: Task): number {
    return task.id;
  }

  getStatusBadgeClass(status: TaskStatus): string {
    const classes = {
      [TaskStatus.Todo]: 'bg-secondary',
      [TaskStatus.InProgress]: 'bg-primary',
      [TaskStatus.InReview]: 'bg-warning',
      [TaskStatus.Done]: 'bg-success',
      [TaskStatus.Blocked]: 'bg-danger'
    };
    return classes[status] || 'bg-secondary';
  }

  getPriorityBadgeClass(priority: TaskPriority): string {
    const classes = {
      [TaskPriority.Low]: 'bg-light text-dark',
      [TaskPriority.Medium]: 'bg-info',
      [TaskPriority.High]: 'bg-warning',
      [TaskPriority.Critical]: 'bg-danger'
    };
    return classes[priority] || 'bg-light text-dark';
  }

  getStatusLabel(status: TaskStatus): string {
    const labels = {
      [TaskStatus.Todo]: 'To Do',
      [TaskStatus.InProgress]: 'In Progress',
      [TaskStatus.InReview]: 'In Review',
      [TaskStatus.Done]: 'Done',
      [TaskStatus.Blocked]: 'Blocked'
    };
    return labels[status] || status;
  }

  getDueDateClass(dueDate: Date): string {
    const today = new Date();
    const due = new Date(dueDate);
    const diffTime = due.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays < 0) return 'text-danger'; // Overdue
    if (diffDays <= 1) return 'text-warning'; // Due soon
    return 'text-muted'; // Normal
  }

  getTaskProgress(task: Task): number {
    const statusProgress = {
      [TaskStatus.Todo]: 0,
      [TaskStatus.InProgress]: 50,
      [TaskStatus.InReview]: 80,
      [TaskStatus.Done]: 100,
      [TaskStatus.Blocked]: 25
    };
    return statusProgress[task.status] || 0;
  }

  getProgressBarClass(status: TaskStatus): string {
    const classes = {
      [TaskStatus.Todo]: 'bg-secondary',
      [TaskStatus.InProgress]: 'bg-primary',
      [TaskStatus.InReview]: 'bg-warning',
      [TaskStatus.Done]: 'bg-success',
      [TaskStatus.Blocked]: 'bg-danger'
    };
    return classes[status] || 'bg-secondary';
  }

  private getCurrentUserId(): number {
    // This should be implemented to get the current user's ID
    // For now, returning a placeholder
    return 1;
  }
}