import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';

import { TaskCardComponent } from './task-card.component';
import { Task, TaskStatus, TaskPriority } from '../../../models/project.models';

describe('TaskCardComponent', () => {
  let component: TaskCardComponent;
  let fixture: ComponentFixture<TaskCardComponent>;

  const mockTask: Task = {
    id: 1,
    projectId: 1,
    title: 'Test Task',
    description: 'This is a test task description that is quite long to test truncation functionality',
    estimatedHours: 8,
    actualHours: 4,
    status: TaskStatus.InProgress,
    priority: TaskPriority.High,
    assignedTo: 1,
    assignedToName: 'John Doe',
    assignedToPhoto: 'https://example.com/photo.jpg',
    dueDate: new Date('2024-12-31'),
    createdAt: new Date('2024-01-01'),
    updatedAt: new Date('2024-01-15'),
    comments: [
      {
        id: 1,
        taskId: 1,
        employeeId: 1,
        employeeName: 'John Doe',
        comment: 'Test comment',
        createdAt: new Date()
      }
    ],
    attachments: [
      {
        id: 1,
        taskId: 1,
        fileName: 'test.pdf',
        filePath: '/files/test.pdf',
        fileSize: 1024,
        uploadedBy: 1,
        uploadedAt: new Date()
      }
    ]
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskCardComponent, NgbTooltipModule]
    }).compileComponents();

    fixture = TestBed.createComponent(TaskCardComponent);
    component = fixture.componentInstance;
    component.task = mockTask;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display task title', () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector('.task-title');
    expect(titleElement.textContent.trim()).toBe('Test Task');
  });

  it('should truncate long task titles', () => {
    const longTitleTask = { 
      ...mockTask, 
      title: 'This is a very long task title that should be truncated when displayed in the card' 
    };
    component.task = longTitleTask;
    fixture.detectChanges();

    const titleElement = fixture.nativeElement.querySelector('.task-title');
    expect(titleElement.textContent).toContain('...');
    expect(titleElement.textContent.length).toBeLessThan(longTitleTask.title.length);
  });

  it('should display task description', () => {
    fixture.detectChanges();
    const descriptionElement = fixture.nativeElement.querySelector('.task-description');
    expect(descriptionElement.textContent).toContain('This is a test task description');
  });

  it('should truncate long descriptions', () => {
    fixture.detectChanges();
    const descriptionElement = fixture.nativeElement.querySelector('.task-description');
    expect(descriptionElement.textContent).toContain('...');
  });

  it('should display priority badge with correct class', () => {
    fixture.detectChanges();
    const priorityBadge = fixture.nativeElement.querySelector('.badge');
    expect(priorityBadge.textContent.trim()).toBe('High');
    expect(priorityBadge.classList).toContain('bg-warning');
  });

  it('should display hours information', () => {
    fixture.detectChanges();
    const hoursInfo = fixture.nativeElement.querySelector('.text-muted');
    expect(hoursInfo.textContent).toContain('4h / 8h');
  });

  it('should display comments count', () => {
    fixture.detectChanges();
    const commentsCount = fixture.nativeElement.textContent;
    expect(commentsCount).toContain('1'); // Comment count
  });

  it('should display attachments count', () => {
    fixture.detectChanges();
    const attachmentsCount = fixture.nativeElement.textContent;
    expect(attachmentsCount).toContain('1'); // Attachment count
  });

  it('should display assignee photo when available', () => {
    fixture.detectChanges();
    const assigneePhoto = fixture.nativeElement.querySelector('.assignee-avatar');
    expect(assigneePhoto).toBeTruthy();
    expect(assigneePhoto.src).toBe('https://example.com/photo.jpg');
    expect(assigneePhoto.alt).toBe('John Doe');
  });

  it('should display assignee initials when photo not available', () => {
    const taskWithoutPhoto = { ...mockTask, assignedToPhoto: undefined };
    component.task = taskWithoutPhoto;
    fixture.detectChanges();

    const assigneePlaceholder = fixture.nativeElement.querySelector('.assignee-avatar-placeholder');
    expect(assigneePlaceholder).toBeTruthy();
    expect(assigneePlaceholder.textContent.trim()).toBe('JD');
  });

  it('should display unassigned state when no assignee', () => {
    const unassignedTask = { 
      ...mockTask, 
      assignedTo: 0, 
      assignedToName: undefined, 
      assignedToPhoto: undefined 
    };
    component.task = unassignedTask;
    fixture.detectChanges();

    const unassignedText = fixture.nativeElement.querySelector('.text-muted');
    expect(unassignedText.textContent).toContain('Unassigned');
  });

  it('should display due date', () => {
    fixture.detectChanges();
    const dueDateElement = fixture.nativeElement.querySelector('.due-date');
    expect(dueDateElement).toBeTruthy();
  });

  it('should show progress bar when task has estimated hours', () => {
    fixture.detectChanges();
    const progressBar = fixture.nativeElement.querySelector('.progress');
    expect(progressBar).toBeTruthy();
  });

  it('should show blocked indicator for blocked tasks', () => {
    const blockedTask = { ...mockTask, status: TaskStatus.Blocked };
    component.task = blockedTask;
    fixture.detectChanges();

    const blockedIndicator = fixture.nativeElement.querySelector('.blocked-indicator');
    expect(blockedIndicator).toBeTruthy();
    expect(blockedIndicator.textContent).toContain('Blocked');
  });

  it('should emit taskClick event when card is clicked', () => {
    spyOn(component.taskClick, 'emit');
    
    component.onTaskClick();
    
    expect(component.taskClick.emit).toHaveBeenCalledWith(mockTask);
  });

  it('should emit taskDelete event when delete is confirmed', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    spyOn(component.taskDelete, 'emit');
    
    component.onDeleteClick();
    
    expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to delete "Test Task"?');
    expect(component.taskDelete.emit).toHaveBeenCalledWith(mockTask);
  });

  it('should not emit taskDelete event when delete is cancelled', () => {
    spyOn(window, 'confirm').and.returnValue(false);
    spyOn(component.taskDelete, 'emit');
    
    component.onDeleteClick();
    
    expect(component.taskDelete.emit).not.toHaveBeenCalled();
  });

  it('should return correct priority class', () => {
    expect(component.getPriorityClass()).toBe('high');
    
    const criticalTask = { ...mockTask, priority: TaskPriority.Critical };
    component.task = criticalTask;
    expect(component.getPriorityClass()).toBe('critical');
    
    const mediumTask = { ...mockTask, priority: TaskPriority.Medium };
    component.task = mediumTask;
    expect(component.getPriorityClass()).toBe('');
  });

  it('should return correct priority badge class', () => {
    expect(component.getPriorityBadgeClass()).toBe('bg-warning');
    
    const criticalTask = { ...mockTask, priority: TaskPriority.Critical };
    component.task = criticalTask;
    expect(component.getPriorityBadgeClass()).toBe('bg-danger');
    
    const lowTask = { ...mockTask, priority: TaskPriority.Low };
    component.task = lowTask;
    expect(component.getPriorityBadgeClass()).toBe('bg-light text-dark');
  });

  it('should detect overdue tasks correctly', () => {
    const overdueTask = { 
      ...mockTask, 
      dueDate: new Date('2020-01-01'), 
      status: TaskStatus.InProgress 
    };
    component.task = overdueTask;
    expect(component.isOverdue()).toBe(true);
    
    const completedOverdueTask = { 
      ...mockTask, 
      dueDate: new Date('2020-01-01'), 
      status: TaskStatus.Done 
    };
    component.task = completedOverdueTask;
    expect(component.isOverdue()).toBe(false);
  });

  it('should return correct due date class', () => {
    const today = new Date();
    
    // Overdue task
    const overdueTask = { 
      ...mockTask, 
      dueDate: new Date(today.getTime() - 24 * 60 * 60 * 1000) 
    };
    component.task = overdueTask;
    expect(component.getDueDateClass()).toBe('text-danger');
    
    // Due tomorrow
    const dueSoonTask = { 
      ...mockTask, 
      dueDate: new Date(today.getTime() + 24 * 60 * 60 * 1000) 
    };
    component.task = dueSoonTask;
    expect(component.getDueDateClass()).toBe('text-warning');
    
    // Due next week
    const normalTask = { 
      ...mockTask, 
      dueDate: new Date(today.getTime() + 7 * 24 * 60 * 60 * 1000) 
    };
    component.task = normalTask;
    expect(component.getDueDateClass()).toBe('text-muted');
  });

  it('should format due date correctly', () => {
    const today = new Date();
    
    // Today
    const todayTask = { ...mockTask, dueDate: today };
    component.task = todayTask;
    expect(component.formatDueDate()).toBe('Today');
    
    // Tomorrow
    const tomorrowTask = { 
      ...mockTask, 
      dueDate: new Date(today.getTime() + 24 * 60 * 60 * 1000) 
    };
    component.task = tomorrowTask;
    expect(component.formatDueDate()).toBe('Tomorrow');
    
    // Yesterday
    const yesterdayTask = { 
      ...mockTask, 
      dueDate: new Date(today.getTime() - 24 * 60 * 60 * 1000) 
    };
    component.task = yesterdayTask;
    expect(component.formatDueDate()).toBe('Yesterday');
  });

  it('should generate correct initials', () => {
    expect(component.getInitials('John Doe')).toBe('JD');
    expect(component.getInitials('Jane')).toBe('J');
    expect(component.getInitials('Mary Jane Watson')).toBe('MJ');
    expect(component.getInitials('')).toBe('?');
  });

  it('should show progress when task has estimated hours', () => {
    expect(component.showProgress()).toBe(true);
    
    const noHoursTask = { ...mockTask, estimatedHours: 0, status: TaskStatus.Todo };
    component.task = noHoursTask;
    expect(component.showProgress()).toBe(false);
  });

  it('should calculate progress correctly', () => {
    // Based on actual vs estimated hours
    expect(component.getProgress()).toBe(50); // 4h actual / 8h estimated
    
    // Based on status when no hours
    const todoTask = { ...mockTask, estimatedHours: 0, actualHours: 0, status: TaskStatus.Todo };
    component.task = todoTask;
    expect(component.getProgress()).toBe(0);
    
    const doneTask = { ...mockTask, estimatedHours: 0, actualHours: 0, status: TaskStatus.Done };
    component.task = doneTask;
    expect(component.getProgress()).toBe(100);
  });

  it('should return correct progress bar class', () => {
    expect(component.getProgressBarClass()).toBe('bg-warning'); // 50% progress
    
    const doneTask = { ...mockTask, status: TaskStatus.Done };
    component.task = doneTask;
    expect(component.getProgressBarClass()).toBe('bg-success');
    
    const blockedTask = { ...mockTask, status: TaskStatus.Blocked };
    component.task = blockedTask;
    expect(component.getProgressBarClass()).toBe('bg-danger');
  });

  it('should handle task actions dropdown', () => {
    fixture.detectChanges();
    const actionsButton = fixture.nativeElement.querySelector('.task-actions .btn');
    expect(actionsButton).toBeTruthy();
  });

  it('should apply high priority styling', () => {
    fixture.detectChanges();
    const taskCard = fixture.nativeElement.querySelector('.task-card');
    expect(taskCard.classList).toContain('high-priority');
  });

  it('should apply overdue styling for overdue tasks', () => {
    const overdueTask = { 
      ...mockTask, 
      dueDate: new Date('2020-01-01'), 
      status: TaskStatus.InProgress 
    };
    component.task = overdueTask;
    fixture.detectChanges();
    
    const taskCard = fixture.nativeElement.querySelector('.task-card');
    expect(taskCard.classList).toContain('overdue');
  });
});