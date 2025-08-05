import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { filter } from 'rxjs/operators';

import { ProjectService } from './project.service';
import { 
  Project, 
  Task, 
  CreateProjectDto, 
  CreateTaskDto, 
  UpdateTaskDto,
  ProjectSearchCriteria,
  TaskSearchCriteria,
  ProjectStatus,
  ProjectPriority,
  TaskStatus,
  TaskPriority,
  KanbanColumn
} from '../models/project.models';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('ProjectService', () => {
  let service: ProjectService;
  let httpMock: HttpTestingController;

  const mockProject: Project = {
    id: 1,
    name: 'Test Project',
    description: 'Test Description',
    startDate: new Date('2024-01-01'),
    endDate: new Date('2024-12-31'),
    estimatedHours: 100,
    actualHours: 50,
    budget: 10000,
    status: ProjectStatus.InProgress,
    priority: ProjectPriority.High,
    createdBy: 1,
    createdAt: new Date(),
    teamMembers: [],
    tasks: [],
    progress: {
      projectId: 1,
      totalTasks: 10,
      completedTasks: 3,
      inProgressTasks: 4,
      todoTasks: 3,
      completionPercentage: 30,
      isOnTrack: true,
      remainingHours: 50,
      budgetUtilization: 50
    }
  };

  const mockTask: Task = {
    id: 1,
    projectId: 1,
    title: 'Test Task',
    description: 'Test Description',
    estimatedHours: 8,
    actualHours: 4,
    status: TaskStatus.InProgress,
    priority: TaskPriority.High,
    assignedTo: 1,
    assignedToName: 'John Doe',
    createdAt: new Date(),
    updatedAt: new Date(),
    comments: [],
    attachments: []
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [ProjectService, provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(ProjectService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Project CRUD Operations', () => {
    it('should get projects with search criteria', () => {
      const criteria: ProjectSearchCriteria = {
        searchTerm: 'test',
        status: ProjectStatus.InProgress,
        priority: ProjectPriority.High,
        page: 1,
        pageSize: 10
      };
      const mockResponse = { projects: [mockProject], totalCount: 1 };

      service.getProjects(criteria).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(request => 
        request.url === '/api/projects' && 
        request.params.get('searchTerm') === 'test' &&
        request.params.get('status') === 'InProgress' &&
        request.params.get('priority') === 'High' &&
        request.params.get('page') === '1' &&
        request.params.get('pageSize') === '10'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should get a single project', () => {
      service.getProject(1).subscribe(project => {
        expect(project).toEqual(mockProject);
      });

      const req = httpMock.expectOne('/api/projects/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockProject);
    });

    it('should create a project', () => {
      const createDto: CreateProjectDto = {
        name: 'New Project',
        description: 'New Description',
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-12-31'),
        estimatedHours: 100,
        budget: 10000,
        priority: ProjectPriority.High,
        teamMemberIds: [1, 2, 3]
      };

      service.createProject(createDto).subscribe(project => {
        expect(project).toEqual(mockProject);
      });

      const req = httpMock.expectOne('/api/projects');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(mockProject);
    });

    it('should update a project', () => {
      const updateData = { name: 'Updated Project' };

      service.updateProject(1, updateData).subscribe(project => {
        expect(project).toEqual(mockProject);
      });

      const req = httpMock.expectOne('/api/projects/1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateData);
      req.flush(mockProject);
    });

    it('should delete a project', () => {
      service.deleteProject(1).subscribe();

      const req = httpMock.expectOne('/api/projects/1');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should get project progress', () => {
      const mockProgress = mockProject.progress!;

      service.getProjectProgress(1).subscribe(progress => {
        expect(progress).toEqual(mockProgress);
      });

      const req = httpMock.expectOne('/api/projects/1/progress');
      expect(req.request.method).toBe('GET');
      req.flush(mockProgress);
    });

    it('should get project hours report', () => {
      const mockReport = [{
        projectId: 1,
        projectName: 'Test Project',
        estimatedHours: 100,
        actualHours: 50,
        remainingHours: 50,
        completionPercentage: 50,
        isOverBudget: false,
        teamMembers: []
      }];

      service.getProjectHoursReport(1).subscribe(report => {
        expect(report).toEqual(mockReport);
      });

      const req = httpMock.expectOne('/api/projects/hours-report/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockReport);
    });
  });

  describe('Task CRUD Operations', () => {
    it('should get tasks with search criteria', () => {
      const criteria: TaskSearchCriteria = {
        projectId: 1,
        searchTerm: 'test',
        status: TaskStatus.InProgress,
        priority: TaskPriority.High,
        assignedTo: 1,
        page: 1,
        pageSize: 10
      };
      const mockResponse = { tasks: [mockTask], totalCount: 1 };

      service.getTasks(criteria).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(request => 
        request.url === '/api/tasks' && 
        request.params.get('projectId') === '1' &&
        request.params.get('searchTerm') === 'test' &&
        request.params.get('status') === 'InProgress' &&
        request.params.get('priority') === 'High' &&
        request.params.get('assignedTo') === '1' &&
        request.params.get('page') === '1' &&
        request.params.get('pageSize') === '10'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should get a single task', () => {
      service.getTask(1).subscribe(task => {
        expect(task).toEqual(mockTask);
      });

      const req = httpMock.expectOne('/api/tasks/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockTask);
    });

    it('should create a task', () => {
      const createDto: CreateTaskDto = {
        projectId: 1,
        title: 'New Task',
        description: 'New Description',
        estimatedHours: 8,
        priority: TaskPriority.High,
        assignedTo: 1,
        dueDate: new Date('2024-12-31')
      };

      service.createTask(createDto).subscribe(task => {
        expect(task).toEqual(mockTask);
      });

      const req = httpMock.expectOne('/api/tasks');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(mockTask);
    });

    it('should update a task', () => {
      const updateDto: UpdateTaskDto = {
        title: 'Updated Task',
        status: TaskStatus.Done
      };

      service.updateTask(1, updateDto).subscribe(task => {
        expect(task).toEqual(mockTask);
      });

      const req = httpMock.expectOne('/api/tasks/1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateDto);
      req.flush(mockTask);
    });

    it('should delete a task', () => {
      service.deleteTask(1).subscribe();

      const req = httpMock.expectOne('/api/tasks/1');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should update task status', () => {
      service.updateTaskStatus(1, TaskStatus.Done, 2).subscribe(task => {
        expect(task).toEqual(mockTask);
      });

      const req = httpMock.expectOne('/api/tasks/1/status');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual({
        status: TaskStatus.Done,
        position: 2
      });
      req.flush(mockTask);
    });

    it('should assign task to employee', () => {
      service.assignTask(1, 2).subscribe(task => {
        expect(task).toEqual(mockTask);
      });

      const req = httpMock.expectOne('/api/tasks/1/assign');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual({ employeeId: 2 });
      req.flush(mockTask);
    });

    it('should add task comment', () => {
      service.addTaskComment(1, 'Test comment').subscribe();

      const req = httpMock.expectOne('/api/tasks/1/comments');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ comment: 'Test comment' });
      req.flush(null);
    });

    it('should upload task attachment', () => {
      const file = new File(['test'], 'test.txt', { type: 'text/plain' });

      service.uploadTaskAttachment(1, file).subscribe();

      const req = httpMock.expectOne('/api/tasks/1/attachments');
      expect(req.request.method).toBe('POST');
      expect(req.request.body instanceof FormData).toBe(true);
      req.flush(null);
    });
  });

  describe('Kanban Operations', () => {
    it('should get kanban board', () => {
      const mockColumns: KanbanColumn[] = [
        {
          id: 'todo',
          title: 'To Do',
          status: TaskStatus.Todo,
          tasks: [],
          color: '#6c757d'
        },
        {
          id: 'inprogress',
          title: 'In Progress',
          status: TaskStatus.InProgress,
          tasks: [],
          color: '#0d6efd',
          limit: 3
        }
      ];

      service.getKanbanBoard(1).subscribe(columns => {
        expect(columns).toEqual(mockColumns);
      });

      const req = httpMock.expectOne('/api/projects/1/kanban');
      expect(req.request.method).toBe('GET');
      req.flush(mockColumns);
    });

    it('should return default kanban columns', () => {
      const columns = service.getDefaultKanbanColumns();
      
      expect(columns).toBeDefined();
      expect(columns.length).toBe(5);
      expect(columns[0].status).toBe(TaskStatus.Todo);
      expect(columns[1].status).toBe(TaskStatus.InProgress);
      expect(columns[2].status).toBe(TaskStatus.InReview);
      expect(columns[3].status).toBe(TaskStatus.Done);
      expect(columns[4].status).toBe(TaskStatus.Blocked);
    });

    it('should notify kanban updates', () => {
      const updateData = { type: 'task_moved', taskId: 1 };
      
      service.kanbanUpdate$.pipe(
        filter(update => update !== null)
      ).subscribe(update => {
        expect(update).toEqual(updateData);
      });

      service.notifyKanbanUpdate(updateData);
    });
  });

  describe('Error Handling', () => {
    it('should handle HTTP errors when getting projects', () => {
      const criteria: ProjectSearchCriteria = {
        page: 1,
        pageSize: 10
      };

      service.getProjects(criteria).subscribe({
        next: () => fail('should have failed with 404 error'),
        error: (error) => {
          expect(error.status).toBe(404);
        }
      });

      const req = httpMock.expectOne('/api/projects?page=1&pageSize=10');
      req.flush('Not Found', { status: 404, statusText: 'Not Found' });
    });

    it('should handle HTTP errors when creating tasks', () => {
      const createDto: CreateTaskDto = {
        projectId: 1,
        title: 'New Task',
        description: 'New Description',
        estimatedHours: 8,
        priority: TaskPriority.High,
        assignedTo: 1
      };

      service.createTask(createDto).subscribe({
        next: () => fail('should have failed with 400 error'),
        error: (error) => {
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne('/api/tasks');
      req.flush('Bad Request', { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('Parameter Handling', () => {
    it('should handle optional parameters in project search', () => {
      const criteria: ProjectSearchCriteria = {
        page: 1,
        pageSize: 10
      };

      service.getProjects(criteria).subscribe();

      const req = httpMock.expectOne('/api/projects?page=1&pageSize=10');
      expect(req.request.method).toBe('GET');
      req.flush({ projects: [], totalCount: 0 });
    });

    it('should handle date parameters correctly', () => {
      const startDate = new Date('2024-01-01');
      const endDate = new Date('2024-12-31');
      const criteria: ProjectSearchCriteria = {
        startDate,
        endDate,
        page: 1,
        pageSize: 10
      };

      service.getProjects(criteria).subscribe();

      const req = httpMock.expectOne(request => 
        request.url === '/api/projects' &&
        request.params.get('startDate') === startDate.toISOString() &&
        request.params.get('endDate') === endDate.toISOString()
      );
      expect(req.request.method).toBe('GET');
      req.flush({ projects: [], totalCount: 0 });
    });

    it('should handle optional task search parameters', () => {
      const criteria: TaskSearchCriteria = {
        page: 1,
        pageSize: 10
      };

      service.getTasks(criteria).subscribe();

      const req = httpMock.expectOne('/api/tasks?page=1&pageSize=10');
      expect(req.request.method).toBe('GET');
      req.flush({ tasks: [], totalCount: 0 });
    });
  });
});