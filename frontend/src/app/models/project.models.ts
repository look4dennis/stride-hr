export interface Project {
  id: number;
  name: string;
  description: string;
  startDate: Date;
  endDate: Date;
  estimatedHours: number;
  actualHours: number;
  budget: number;
  status: ProjectStatus;
  priority: ProjectPriority;
  createdBy: number;
  createdAt: Date;
  teamMembers: ProjectMember[];
  tasks: Task[];
  progress: ProjectProgress;
}

export interface Task {
  id: number;
  projectId: number;
  title: string;
  description: string;
  estimatedHours: number;
  actualHours: number;
  status: TaskStatus;
  priority: TaskPriority;
  assignedTo: number;
  assignedToName?: string;
  assignedToPhoto?: string;
  dueDate?: Date;
  createdAt: Date;
  updatedAt: Date;
  comments: TaskComment[];
  attachments: TaskAttachment[];
}

export interface TaskComment {
  id: number;
  taskId: number;
  employeeId: number;
  employeeName: string;
  employeePhoto?: string;
  comment: string;
  createdAt: Date;
}

export interface TaskAttachment {
  id: number;
  taskId: number;
  fileName: string;
  filePath: string;
  fileSize: number;
  uploadedBy: number;
  uploadedAt: Date;
}

export interface ProjectMember {
  id: number;
  projectId: number;
  employeeId: number;
  employeeName: string;
  employeePhoto?: string;
  role: string;
  joinedAt: Date;
}

export interface ProjectProgress {
  projectId: number;
  totalTasks: number;
  completedTasks: number;
  inProgressTasks: number;
  todoTasks: number;
  completionPercentage: number;
  isOnTrack: boolean;
  remainingHours: number;
  budgetUtilization: number;
}

export interface KanbanColumn {
  id: string;
  title: string;
  status: TaskStatus;
  tasks: Task[];
  color: string;
  limit?: number;
}

export interface CreateProjectDto {
  name: string;
  description: string;
  startDate: Date;
  endDate: Date;
  estimatedHours: number;
  budget: number;
  priority: ProjectPriority;
  teamMemberIds: number[];
}

export interface CreateTaskDto {
  projectId: number;
  title: string;
  description: string;
  estimatedHours: number;
  priority: TaskPriority;
  assignedTo: number;
  dueDate?: Date;
}

export interface UpdateTaskDto {
  title?: string;
  description?: string;
  estimatedHours?: number;
  status?: TaskStatus;
  priority?: TaskPriority;
  assignedTo?: number;
  dueDate?: Date;
}

export interface ProjectSearchCriteria {
  searchTerm?: string;
  status?: ProjectStatus;
  priority?: ProjectPriority;
  teamMemberId?: number;
  startDate?: Date;
  endDate?: Date;
  page: number;
  pageSize: number;
}

export interface TaskSearchCriteria {
  projectId?: number;
  searchTerm?: string;
  status?: TaskStatus;
  priority?: TaskPriority;
  assignedTo?: number;
  dueDate?: Date;
  page: number;
  pageSize: number;
}

export enum ProjectStatus {
  Planning = 'Planning',
  InProgress = 'InProgress',
  OnHold = 'OnHold',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export enum TaskStatus {
  Todo = 'Todo',
  InProgress = 'InProgress',
  InReview = 'InReview',
  Done = 'Done',
  Blocked = 'Blocked'
}

export enum ProjectPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical'
}

export enum TaskPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical'
}

export interface ProjectHoursReport {
  projectId: number;
  projectName: string;
  estimatedHours: number;
  actualHours: number;
  remainingHours: number;
  completionPercentage: number;
  isOverBudget: boolean;
  teamMembers: ProjectMemberHours[];
}

export interface ProjectMemberHours {
  employeeId: number;
  employeeName: string;
  hoursWorked: number;
  tasksCompleted: number;
  productivity: number;
}