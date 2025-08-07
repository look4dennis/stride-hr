import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import {
  Project,
  Task,
  CreateProjectDto,
  CreateTaskDto,
  UpdateTaskDto,
  ProjectSearchCriteria,
  TaskSearchCriteria,
  ProjectProgress,
  ProjectHoursReport,
  KanbanColumn,
  TaskStatus
} from '../models/project.models';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private readonly apiUrl = '/api/projects';
  private readonly taskApiUrl = '/api/tasks';

  // Real-time updates for Kanban board
  private kanbanUpdateSubject = new BehaviorSubject<any>(null);
  public kanbanUpdate$ = this.kanbanUpdateSubject.asObservable();

  constructor(private http: HttpClient) { }

  // Project CRUD operations
  getProjects(criteria: ProjectSearchCriteria): Observable<{ projects: Project[], totalCount: number }> {
    // Always use mock data for development to avoid API errors
    console.log('ProjectService: Using mock data for development');
    return new Observable(observer => {
      setTimeout(() => {
        observer.next(this.getMockProjectsResponse(criteria));
        observer.complete();
      }, 300); // Simulate network delay
    });
  }

  getProject(id: number): Observable<Project> {
    // Return mock project data for development
    console.log('ProjectService: Using mock project data for development');
    const mockProject = this.getMockProjectsResponse({ page: 1, pageSize: 10 }).projects[0];
    return of(mockProject);
  }

  createProject(project: CreateProjectDto): Observable<Project> {
    return this.http.post<Project>(`${this.apiUrl}`, project);
  }

  updateProject(id: number, project: Partial<CreateProjectDto>): Observable<Project> {
    return this.http.put<Project>(`${this.apiUrl}/${id}`, project);
  }

  deleteProject(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getProjectProgress(id: number): Observable<ProjectProgress> {
    return this.http.get<ProjectProgress>(`${this.apiUrl}/${id}/progress`);
  }

  getProjectHoursReport(teamLeaderId: number): Observable<ProjectHoursReport[]> {
    return this.http.get<ProjectHoursReport[]>(`${this.apiUrl}/hours-report/${teamLeaderId}`);
  }

  // Task CRUD operations
  getTasks(criteria: TaskSearchCriteria): Observable<{ tasks: Task[], totalCount: number }> {
    let params = new HttpParams()
      .set('page', criteria.page.toString())
      .set('pageSize', criteria.pageSize.toString());

    if (criteria.projectId) params = params.set('projectId', criteria.projectId.toString());
    if (criteria.searchTerm) params = params.set('searchTerm', criteria.searchTerm);
    if (criteria.status) params = params.set('status', criteria.status);
    if (criteria.priority) params = params.set('priority', criteria.priority);
    if (criteria.assignedTo) params = params.set('assignedTo', criteria.assignedTo.toString());
    if (criteria.dueDate) params = params.set('dueDate', criteria.dueDate.toISOString());

    return this.http.get<{ tasks: Task[], totalCount: number }>(`${this.taskApiUrl}`, { params });
  }

  getTask(id: number): Observable<Task> {
    return this.http.get<Task>(`${this.taskApiUrl}/${id}`);
  }

  createTask(task: CreateTaskDto): Observable<Task> {
    return this.http.post<Task>(`${this.taskApiUrl}`, task);
  }

  updateTask(id: number, task: UpdateTaskDto): Observable<Task> {
    return this.http.put<Task>(`${this.taskApiUrl}/${id}`, task);
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.taskApiUrl}/${id}`);
  }

  // Kanban specific operations
  getKanbanBoard(projectId: number): Observable<KanbanColumn[]> {
    return this.http.get<KanbanColumn[]>(`${this.apiUrl}/${projectId}/kanban`);
  }

  updateTaskStatus(taskId: number, newStatus: TaskStatus, newPosition: number): Observable<Task> {
    return this.http.put<Task>(`${this.taskApiUrl}/${taskId}/status`, {
      status: newStatus,
      position: newPosition
    });
  }

  // Real-time updates
  notifyKanbanUpdate(update: any): void {
    this.kanbanUpdateSubject.next(update);
  }

  // Utility methods for Kanban board
  getDefaultKanbanColumns(): KanbanColumn[] {
    return [
      {
        id: 'todo',
        title: 'To Do',
        status: TaskStatus.Todo,
        tasks: [],
        color: '#6c757d',
        limit: undefined
      },
      {
        id: 'inprogress',
        title: 'In Progress',
        status: TaskStatus.InProgress,
        tasks: [],
        color: '#0d6efd',
        limit: 3
      },
      {
        id: 'inreview',
        title: 'In Review',
        status: TaskStatus.InReview,
        tasks: [],
        color: '#fd7e14',
        limit: 2
      },
      {
        id: 'done',
        title: 'Done',
        status: TaskStatus.Done,
        tasks: [],
        color: '#198754',
        limit: undefined
      },
      {
        id: 'blocked',
        title: 'Blocked',
        status: TaskStatus.Blocked,
        tasks: [],
        color: '#dc3545',
        limit: undefined
      }
    ];
  }

  // Task assignment and team collaboration
  assignTask(taskId: number, employeeId: number): Observable<Task> {
    return this.http.put<Task>(`${this.taskApiUrl}/${taskId}/assign`, { employeeId });
  }

  addTaskComment(taskId: number, comment: string): Observable<void> {
    return this.http.post<void>(`${this.taskApiUrl}/${taskId}/comments`, { comment });
  }

  uploadTaskAttachment(taskId: number, file: File): Observable<void> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<void>(`${this.taskApiUrl}/${taskId}/attachments`, formData);
  }

  // Project Monitoring Methods
  getTeamLeaderDashboard(): Observable<any> {
    return this.http.get<any>('/api/project-monitoring/team-leader-dashboard');
  }

  getTeamHoursTracking(startDate?: Date, endDate?: Date): Observable<any[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate.toISOString());
    if (endDate) params = params.set('endDate', endDate.toISOString());

    return this.http.get<any[]>('/api/project-monitoring/team-hours-tracking', { params });
  }

  getProjectAnalytics(projectId: number): Observable<any> {
    return this.http.get<any>(`/api/project-monitoring/projects/${projectId}/analytics`);
  }

  getProjectPerformance(projectId: number): Observable<any> {
    return this.http.get<any>(`/api/project-monitoring/projects/${projectId}/performance`);
  }

  getProjectAlerts(projectId: number): Observable<any[]> {
    return this.http.get<any[]>(`/api/project-monitoring/projects/${projectId}/alerts`);
  }

  createProjectAlert(projectId: number, alertType: string, message: string, severity: string): Observable<any> {
    return this.http.post<any>(`/api/project-monitoring/projects/${projectId}/alerts`, {
      alertType,
      message,
      severity
    });
  }

  resolveProjectAlert(alertId: number): Observable<void> {
    return this.http.put<void>(`/api/project-monitoring/alerts/${alertId}/resolve`, {});
  }

  getProjectRisks(projectId: number): Observable<any[]> {
    return this.http.get<any[]>(`/api/project-monitoring/projects/${projectId}/risks`);
  }

  createProjectRisk(projectId: number, riskType: string, description: string, severity: string, probability: number, impact: number): Observable<any> {
    return this.http.post<any>(`/api/project-monitoring/projects/${projectId}/risks`, {
      riskType,
      description,
      severity,
      probability,
      impact
    });
  }

  // Project Collaboration Methods
  getProjectCollaboration(projectId: number): Observable<any> {
    return this.http.get<any>(`/api/project-collaboration/projects/${projectId}/collaboration`);
  }

  getProjectComments(projectId: number): Observable<any[]> {
    return this.http.get<any[]>(`/api/project-collaboration/projects/${projectId}/comments`);
  }

  addProjectComment(comment: any): Observable<any> {
    return this.http.post<any>('/api/project-collaboration/comments', comment);
  }

  addCommentReply(reply: any): Observable<any> {
    return this.http.post<any>('/api/project-collaboration/comments/replies', reply);
  }

  deleteComment(commentId: number): Observable<void> {
    return this.http.delete<void>(`/api/project-collaboration/comments/${commentId}`);
  }

  deleteCommentReply(replyId: number): Observable<void> {
    return this.http.delete<void>(`/api/project-collaboration/comments/replies/${replyId}`);
  }

  getProjectActivities(projectId: number, limit: number = 50): Observable<any[]> {
    return this.http.get<any[]>(`/api/project-collaboration/projects/${projectId}/activities?limit=${limit}`);
  }

  logProjectActivity(projectId: number, activityType: string, description: string, details: string = ''): Observable<any> {
    return this.http.post<any>(`/api/project-collaboration/projects/${projectId}/activities`, {
      activityType,
      description,
      details
    });
  }

  getProjectCommunicationStats(projectId: number): Observable<any> {
    return this.http.get<any>(`/api/project-collaboration/projects/${projectId}/communication-stats`);
  }

  notifyTeamMembers(projectId: number, message: string, notificationType: string): Observable<void> {
    return this.http.post<void>(`/api/project-collaboration/projects/${projectId}/notify-team`, {
      message,
      notificationType
    });
  }

  // Development helper methods
  private isProductionEnvironment(): boolean {
    return false; // Set to true when backend API is ready
  }

  private getMockProjectsResponse(criteria: ProjectSearchCriteria): { projects: Project[], totalCount: number } {
    const mockProjects: Project[] = [
      {
        id: 1,
        name: 'StrideHR Mobile App',
        description: 'Developing a mobile application for StrideHR to enable employees to access HR services on the go.',
        status: 'InProgress' as any,
        priority: 'High' as any,
        startDate: new Date('2024-01-15'),
        endDate: new Date('2024-06-30'),
        estimatedHours: 500,
        actualHours: 320,
        budget: 50000,
        createdBy: 1,
        tasks: [],
        teamMembers: [
          {
            id: 1,
            projectId: 1,
            employeeId: 1,
            employeeName: 'John Doe',
            employeePhoto: '/assets/images/avatars/john-doe.jpg',
            role: 'Developer',
            joinedAt: new Date('2024-01-15')
          },
          {
            id: 2,
            projectId: 1,
            employeeId: 2,
            employeeName: 'Jane Smith',
            employeePhoto: '/assets/images/avatars/jane-smith.jpg',
            role: 'Designer',
            joinedAt: new Date('2024-01-16')
          }
        ],
        progress: {
          projectId: 1,
          totalTasks: 25,
          completedTasks: 15,
          inProgressTasks: 8,
          todoTasks: 2,
          completionPercentage: 60,
          isOnTrack: true,
          remainingHours: 180,
          budgetUtilization: 64
        },
        createdAt: new Date('2024-01-10')
      },
      {
        id: 2,
        name: 'Employee Portal Redesign',
        description: 'Redesigning the employee self-service portal with modern UI/UX principles.',
        status: 'Planning' as any,
        priority: 'Medium' as any,
        startDate: new Date('2024-04-01'),
        endDate: new Date('2024-08-15'),
        estimatedHours: 400,
        actualHours: 45,
        budget: 30000,
        createdBy: 2,
        tasks: [],
        teamMembers: [
          {
            id: 3,
            projectId: 2,
            employeeId: 3,
            employeeName: 'Mike Johnson',
            employeePhoto: '/assets/images/avatars/mike-johnson.jpg',
            role: 'UI/UX Designer',
            joinedAt: new Date('2024-03-20')
          }
        ],
        progress: {
          projectId: 2,
          totalTasks: 18,
          completedTasks: 2,
          inProgressTasks: 3,
          todoTasks: 13,
          completionPercentage: 11,
          isOnTrack: true,
          remainingHours: 355,
          budgetUtilization: 11
        },
        createdAt: new Date('2024-03-20')
      },
      {
        id: 3,
        name: 'Payroll System Integration',
        description: 'Integrating third-party payroll system with existing HR database.',
        status: 'Completed' as any,
        priority: 'Critical' as any,
        startDate: new Date('2023-10-01'),
        endDate: new Date('2024-01-31'),
        estimatedHours: 650,
        actualHours: 680,
        budget: 75000,
        createdBy: 1,
        tasks: [],
        teamMembers: [
          {
            id: 4,
            projectId: 3,
            employeeId: 4,
            employeeName: 'Sarah Wilson',
            employeePhoto: '/assets/images/avatars/sarah-wilson.jpg',
            role: 'Backend Developer',
            joinedAt: new Date('2023-10-01')
          },
          {
            id: 5,
            projectId: 3,
            employeeId: 5,
            employeeName: 'David Brown',
            employeePhoto: '/assets/images/avatars/david-brown.jpg',
            role: 'System Analyst',
            joinedAt: new Date('2023-10-05')
          }
        ],
        progress: {
          projectId: 3,
          totalTasks: 32,
          completedTasks: 32,
          inProgressTasks: 0,
          todoTasks: 0,
          completionPercentage: 100,
          isOnTrack: true,
          remainingHours: 0,
          budgetUtilization: 105
        },
        createdAt: new Date('2023-09-15')
      }
    ];

    // Apply filters
    let filteredProjects = mockProjects;

    if (criteria.searchTerm) {
      const searchTerm = criteria.searchTerm.toLowerCase();
      filteredProjects = filteredProjects.filter(p =>
        p.name.toLowerCase().includes(searchTerm) ||
        p.description.toLowerCase().includes(searchTerm)
      );
    }

    if (criteria.status) {
      filteredProjects = filteredProjects.filter(p => p.status === criteria.status);
    }

    if (criteria.priority) {
      filteredProjects = filteredProjects.filter(p => p.priority === criteria.priority);
    }

    // Apply pagination
    const startIndex = (criteria.page - 1) * criteria.pageSize;
    const endIndex = startIndex + criteria.pageSize;
    const paginatedProjects = filteredProjects.slice(startIndex, endIndex);

    return {
      projects: paginatedProjects,
      totalCount: filteredProjects.length
    };
  }
}