import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NgbModal, NgbDropdownModule, NgbTooltipModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { Subject, takeUntil } from 'rxjs';

import { ProjectService } from '../../../services/project.service';
import { 
  Project, 
  ProjectSearchCriteria, 
  ProjectStatus, 
  ProjectPriority,
  CreateProjectDto
} from '../../../models/project.models';
import { KanbanBoardComponent } from '../kanban-board/kanban-board.component';
import { ProjectProgressComponent } from '../project-progress/project-progress.component';

@Component({
    selector: 'app-project-list',
    imports: [
        CommonModule,
        FormsModule,
        RouterModule,
        NgbDropdownModule,
        NgbTooltipModule,
        NgbPaginationModule,
        KanbanBoardComponent,
        ProjectProgressComponent
    ],
    template: `
    <div class="page-header d-flex justify-content-between align-items-center mb-4">
      <div>
        <h1>Project Management</h1>
        <p class="text-muted mb-0">Manage projects and track progress with Kanban boards</p>
      </div>
      <div class="header-actions">
        <button class="btn btn-primary" (click)="openProjectModal()">
          <i class="fas fa-plus me-2"></i>
          New Project
        </button>
      </div>
    </div>

    <!-- Project Selection and Filters -->
    <div class="project-controls mb-4" *ngIf="!selectedProject">
      <div class="row">
        <div class="col-md-4">
          <div class="input-group">
            <span class="input-group-text">
              <i class="fas fa-search"></i>
            </span>
            <input type="text" 
                   class="form-control" 
                   placeholder="Search projects..."
                   [(ngModel)]="searchCriteria.searchTerm"
                   (input)="onSearch()">
          </div>
        </div>
        <div class="col-md-2">
          <select class="form-select" 
                  [(ngModel)]="searchCriteria.status" 
                  (change)="onSearch()">
            <option value="">All Status</option>
            <option value="Planning">Planning</option>
            <option value="InProgress">In Progress</option>
            <option value="OnHold">On Hold</option>
            <option value="Completed">Completed</option>
          </select>
        </div>
        <div class="col-md-2">
          <select class="form-select" 
                  [(ngModel)]="searchCriteria.priority" 
                  (change)="onSearch()">
            <option value="">All Priority</option>
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
            <option value="Critical">Critical</option>
          </select>
        </div>
        <div class="col-md-2">
          <div class="dropdown">
            <button class="btn btn-outline-secondary dropdown-toggle w-100" 
                    type="button" 
                    data-bs-toggle="dropdown">
              <i class="fas fa-th me-1"></i> View
            </button>
            <ul class="dropdown-menu">
              <li>
                <a class="dropdown-item" 
                   [class.active]="viewMode === 'grid'"
                   href="#" 
                   (click)="setViewMode('grid')">
                  <i class="fas fa-th me-2"></i> Grid View
                </a>
              </li>
              <li>
                <a class="dropdown-item" 
                   [class.active]="viewMode === 'list'"
                   href="#" 
                   (click)="setViewMode('list')">
                  <i class="fas fa-list me-2"></i> List View
                </a>
              </li>
            </ul>
          </div>
        </div>
        <div class="col-md-2">
          <button class="btn btn-outline-secondary w-100" (click)="clearFilters()">
            <i class="fas fa-times me-1"></i> Clear
          </button>
        </div>
      </div>
    </div>

    <!-- Back to Projects Button (when viewing a specific project) -->
    <div class="mb-3" *ngIf="selectedProject">
      <button class="btn btn-outline-secondary" (click)="backToProjects()">
        <i class="fas fa-arrow-left me-2"></i>
        Back to Projects
      </button>
    </div>

    <!-- Loading State -->
    <div *ngIf="loading" class="text-center py-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading projects...</span>
      </div>
      <p class="mt-2 text-muted">Loading projects...</p>
    </div>

    <!-- Project List View -->
    <div *ngIf="!loading && !selectedProject">
      <!-- Grid View -->
      <div *ngIf="viewMode === 'grid'" class="projects-grid">
        <div class="row">
          <div class="col-lg-4 col-md-6 mb-4" *ngFor="let project of projects; trackBy: trackByProjectId">
            <div class="project-card" (click)="selectProject(project)">
              <div class="project-header">
                <div class="d-flex justify-content-between align-items-start">
                  <h5 class="project-title mb-2">{{ project.name }}</h5>
                  <div class="dropdown" (click)="$event.stopPropagation()">
                    <button class="btn btn-sm btn-link text-muted" 
                            type="button" 
                            data-bs-toggle="dropdown">
                      <i class="fas fa-ellipsis-v"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end">
                      <li>
                        <a class="dropdown-item" href="#" (click)="selectProject(project)">
                          <i class="fas fa-eye me-2"></i> View Board
                        </a>
                      </li>
                      <li>
                        <a class="dropdown-item" href="#" (click)="editProject(project)">
                          <i class="fas fa-edit me-2"></i> Edit
                        </a>
                      </li>
                      <li><hr class="dropdown-divider"></li>
                      <li>
                        <a class="dropdown-item text-danger" href="#" (click)="deleteProject(project)">
                          <i class="fas fa-trash me-2"></i> Delete
                        </a>
                      </li>
                    </ul>
                  </div>
                </div>
                <p class="project-description text-muted mb-3">
                  {{ project.description | slice:0:100 }}{{ project.description.length > 100 ? '...' : '' }}
                </p>
              </div>

              <div class="project-meta mb-3">
                <div class="d-flex justify-content-between align-items-center mb-2">
                  <span class="badge" [class]="getStatusBadgeClass(project.status)">
                    {{ project.status }}
                  </span>
                  <span class="badge" [class]="getPriorityBadgeClass(project.priority)">
                    {{ project.priority }}
                  </span>
                </div>
                
                <div class="progress mb-2" style="height: 6px;">
                  <div class="progress-bar bg-success" 
                       [style.width.%]="project.progress.completionPercentage || 0">
                  </div>
                </div>
                <small class="text-muted">
                  {{ project.progress.completionPercentage || 0 }}% Complete
                </small>
              </div>

              <div class="project-footer">
                <div class="d-flex justify-content-between align-items-center">
                  <div class="team-avatars">
                    <img *ngFor="let member of project.teamMembers?.slice(0, 3)" 
                         [src]="member.employeePhoto || '/assets/images/default-avatar.png'" 
                         [alt]="member.employeeName"
                         class="team-avatar"
                         [ngbTooltip]="member.employeeName">
                    <span *ngIf="project.teamMembers && project.teamMembers.length > 3" 
                          class="team-count"
                          [ngbTooltip]="'And ' + (project.teamMembers.length - 3) + ' more'">
                      +{{ project.teamMembers.length - 3 }}
                    </span>
                  </div>
                  <small class="text-muted">
                    <i class="fas fa-calendar-alt me-1"></i>
                    {{ project.endDate | date:'MMM dd, yyyy' }}
                  </small>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- List View -->
      <div *ngIf="viewMode === 'list'" class="projects-list">
        <div class="table-responsive">
          <table class="table table-hover">
            <thead>
              <tr>
                <th>Project</th>
                <th>Status</th>
                <th>Priority</th>
                <th>Progress</th>
                <th>Team</th>
                <th>Due Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let project of projects; trackBy: trackByProjectId" 
                  class="project-row" 
                  (click)="selectProject(project)">
                <td>
                  <div class="project-info">
                    <h6 class="mb-1">{{ project.name }}</h6>
                    <small class="text-muted">{{ project.description | slice:0:60 }}...</small>
                  </div>
                </td>
                <td>
                  <span class="badge" [class]="getStatusBadgeClass(project.status)">
                    {{ project.status }}
                  </span>
                </td>
                <td>
                  <span class="badge" [class]="getPriorityBadgeClass(project.priority)">
                    {{ project.priority }}
                  </span>
                </td>
                <td>
                  <div class="progress" style="width: 100px; height: 8px;">
                    <div class="progress-bar bg-success" 
                         [style.width.%]="project.progress.completionPercentage || 0">
                    </div>
                  </div>
                  <small class="text-muted">{{ project.progress.completionPercentage || 0 }}%</small>
                </td>
                <td>
                  <div class="team-avatars">
                    <img *ngFor="let member of project.teamMembers?.slice(0, 2)" 
                         [src]="member.employeePhoto || '/assets/images/default-avatar.png'" 
                         [alt]="member.employeeName"
                         class="team-avatar-sm"
                         [ngbTooltip]="member.employeeName">
                    <span *ngIf="project.teamMembers && project.teamMembers.length > 2" 
                          class="team-count-sm">
                      +{{ project.teamMembers.length - 2 }}
                    </span>
                  </div>
                </td>
                <td>
                  <small [class]="getDueDateClass(project.endDate)">
                    {{ project.endDate | date:'MMM dd, yyyy' }}
                  </small>
                </td>
                <td>
                  <div class="btn-group btn-group-sm" (click)="$event.stopPropagation()">
                    <button class="btn btn-outline-primary" 
                            (click)="selectProject(project)"
                            [ngbTooltip]="'View Kanban Board'">
                      <i class="fas fa-columns"></i>
                    </button>
                    <button class="btn btn-outline-secondary" 
                            (click)="editProject(project)"
                            [ngbTooltip]="'Edit Project'">
                      <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn btn-outline-danger" 
                            (click)="deleteProject(project)"
                            [ngbTooltip]="'Delete Project'">
                      <i class="fas fa-trash"></i>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Pagination -->
      <div class="d-flex justify-content-between align-items-center mt-4" *ngIf="totalProjects > 0">
        <div class="results-info">
          <small class="text-muted">
            Showing {{ (currentPage - 1) * pageSize + 1 }} to 
            {{ getMaxDisplayed() }} of 
            {{ totalProjects }} projects
          </small>
        </div>
        <ngb-pagination 
          [(page)]="currentPage"
          [pageSize]="pageSize"
          [collectionSize]="totalProjects"
          [maxSize]="5"
          [rotate]="true"
          [boundaryLinks]="true"
          (pageChange)="onPageChange($event)">
        </ngb-pagination>
      </div>

      <!-- Empty State -->
      <div *ngIf="projects.length === 0 && !loading" class="text-center py-5">
        <i class="fas fa-project-diagram text-muted mb-3" style="font-size: 4rem;"></i>
        <h4>No projects found</h4>
        <p class="text-muted">Create your first project to get started with task management.</p>
        <button class="btn btn-primary" (click)="openProjectModal()">
          <i class="fas fa-plus me-2"></i>
          Create Project
        </button>
      </div>
    </div>

    <!-- Kanban Board View -->
    <div *ngIf="selectedProject" class="project-board">
      <div class="row">
        <!-- Project Progress Sidebar -->
        <div class="col-lg-3 mb-4">
          <app-project-progress [project]="selectedProject"></app-project-progress>
        </div>
        
        <!-- Kanban Board -->
        <div class="col-lg-9">
          <app-kanban-board 
            [project]="selectedProject"
            (taskCreated)="onTaskCreated($event)"
            (taskUpdated)="onTaskUpdated($event)"
            (taskDeleted)="onTaskDeleted($event)">
          </app-kanban-board>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .page-header h1 {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .project-card {
      background: white;
      border: 1px solid #e9ecef;
      border-radius: 12px;
      padding: 1.5rem;
      cursor: pointer;
      transition: all 0.2s ease;
      height: 100%;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .project-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
      border-color: #0d6efd;
    }

    .project-title {
      font-weight: 600;
      color: #495057;
      margin-bottom: 0.5rem;
    }

    .project-description {
      line-height: 1.4;
      font-size: 0.9rem;
    }

    .team-avatars {
      display: flex;
      align-items: center;
    }

    .team-avatar {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      border: 2px solid white;
      margin-right: -8px;
      object-fit: cover;
    }

    .team-avatar-sm {
      width: 24px;
      height: 24px;
      border-radius: 50%;
      border: 2px solid white;
      margin-right: -6px;
      object-fit: cover;
    }

    .team-count,
    .team-count-sm {
      background: #6c757d;
      color: white;
      border-radius: 50%;
      width: 28px;
      height: 28px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.7rem;
      font-weight: 600;
      margin-left: 8px;
    }

    .team-count-sm {
      width: 24px;
      height: 24px;
      font-size: 0.65rem;
    }

    .project-row {
      cursor: pointer;
    }

    .project-row:hover {
      background-color: #f8f9fa;
    }

    .badge {
      font-size: 0.75rem;
      font-weight: 500;
    }

    .progress {
      border-radius: 4px;
    }

    .project-controls {
      background: white;
      border-radius: 8px;
      padding: 1.5rem;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .dropdown-item.active {
      background-color: #0d6efd;
      color: white;
    }

    .results-info {
      font-size: 0.875rem;
    }

    @media (max-width: 768px) {
      .project-controls .row > div {
        margin-bottom: 1rem;
      }
      
      .header-actions {
        margin-top: 1rem;
      }
      
      .project-board .col-lg-3 {
        order: 2;
      }
      
      .project-board .col-lg-9 {
        order: 1;
      }
    }
  `]
})
export class ProjectListComponent implements OnInit, OnDestroy {
  projects: Project[] = [];
  selectedProject: Project | undefined = undefined;
  totalProjects = 0;
  loading = false;
  viewMode: 'grid' | 'list' = 'grid';

  // Pagination
  currentPage = 1;
  pageSize = 12;

  // Search and filters
  searchCriteria: ProjectSearchCriteria = {
    searchTerm: '',
    status: undefined,
    priority: undefined,
    page: 1,
    pageSize: 12
  };

  private destroy$ = new Subject<void>();

  constructor(
    private projectService: ProjectService,
    private modalService: NgbModal
  ) {}

  ngOnInit(): void {
    this.loadProjects();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProjects(): void {
    this.loading = true;
    this.searchCriteria.page = this.currentPage;
    this.searchCriteria.pageSize = this.pageSize;

    this.projectService.getProjects(this.searchCriteria)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.projects = response.projects;
          this.totalProjects = response.totalCount;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading projects:', error);
          this.loading = false;
        }
      });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadProjects();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadProjects();
  }

  setViewMode(mode: 'grid' | 'list'): void {
    this.viewMode = mode;
  }

  clearFilters(): void {
    this.searchCriteria = {
      searchTerm: '',
      status: undefined,
      priority: undefined,
      page: 1,
      pageSize: this.pageSize
    };
    this.currentPage = 1;
    this.loadProjects();
  }

  selectProject(project: Project): void {
    this.selectedProject = project;
  }

  backToProjects(): void {
    this.selectedProject = undefined;
  }

  openProjectModal(): void {
    // TODO: Implement project creation modal
    console.log('Open project modal');
  }

  editProject(project: Project): void {
    // TODO: Implement project editing
    console.log('Edit project:', project);
  }

  deleteProject(project: Project): void {
    if (confirm(`Are you sure you want to delete "${project.name}"?`)) {
      this.projectService.deleteProject(project.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.loadProjects();
          },
          error: (error) => {
            console.error('Error deleting project:', error);
          }
        });
    }
  }

  onTaskCreated(task: any): void {
    // Refresh project data or handle task creation
    console.log('Task created:', task);
  }

  onTaskUpdated(task: any): void {
    // Handle task update
    console.log('Task updated:', task);
  }

  onTaskDeleted(taskId: number): void {
    // Handle task deletion
    console.log('Task deleted:', taskId);
  }

  trackByProjectId(index: number, project: Project): number {
    return project.id;
  }

  getStatusBadgeClass(status: ProjectStatus): string {
    const classes = {
      [ProjectStatus.Planning]: 'bg-secondary',
      [ProjectStatus.InProgress]: 'bg-primary',
      [ProjectStatus.OnHold]: 'bg-warning',
      [ProjectStatus.Completed]: 'bg-success',
      [ProjectStatus.Cancelled]: 'bg-danger'
    };
    return classes[status] || 'bg-secondary';
  }

  getPriorityBadgeClass(priority: ProjectPriority): string {
    const classes = {
      [ProjectPriority.Low]: 'bg-light text-dark',
      [ProjectPriority.Medium]: 'bg-info',
      [ProjectPriority.High]: 'bg-warning',
      [ProjectPriority.Critical]: 'bg-danger'
    };
    return classes[priority] || 'bg-light text-dark';
  }

  getDueDateClass(endDate: Date): string {
    const today = new Date();
    const due = new Date(endDate);
    const diffTime = due.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays < 0) return 'text-danger'; // Overdue
    if (diffDays <= 7) return 'text-warning'; // Due soon
    return 'text-muted'; // Normal
  }

  getMaxDisplayed(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalProjects);
  }
}