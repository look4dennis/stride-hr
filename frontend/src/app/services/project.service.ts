import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
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

  constructor(private http: HttpClient) {}

  // Project CRUD operations
  getProjects(criteria: ProjectSearchCriteria): Observable<{ projects: Project[], totalCount: number }> {
    let params = new HttpParams()
      .set('page', criteria.page.toString())
      .set('pageSize', criteria.pageSize.toString());

    if (criteria.searchTerm) params = params.set('searchTerm', criteria.searchTerm);
    if (criteria.status) params = params.set('status', criteria.status);
    if (criteria.priority) params = params.set('priority', criteria.priority);
    if (criteria.teamMemberId) params = params.set('teamMemberId', criteria.teamMemberId.toString());
    if (criteria.startDate) params = params.set('startDate', criteria.startDate.toISOString());
    if (criteria.endDate) params = params.set('endDate', criteria.endDate.toISOString());

    return this.http.get<{ projects: Project[], totalCount: number }>(`${this.apiUrl}`, { params });
  }

  getProject(id: number): Observable<Project> {
    return this.http.get<Project>(`${this.apiUrl}/${id}`);
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
}