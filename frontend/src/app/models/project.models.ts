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

// Project Monitoring Models
export interface ProjectAnalytics {
    projectId: number;
    projectName: string;
    metrics: ProjectMetrics;
    trends: ProjectTrends;
    performance: ProjectPerformance;
    risks: ProjectRisk[];
    generatedAt: Date;
}

export interface ProjectMetrics {
    totalHoursWorked: number;
    estimatedHours: number;
    hoursVariance: number;
    budgetUtilized: number;
    budgetVariance: number;
    completionPercentage: number;
    totalTasks: number;
    completedTasks: number;
    overdueTasks: number;
    teamMembersCount: number;
    averageTaskCompletionTime: number;
}

export interface ProjectTrends {
    dailyProgress: DailyProgress[];
    weeklyHours: WeeklyHours[];
    teamProductivity: TeamMemberProductivity[];
    taskStatusTrends: TaskStatusTrend[];
}

export interface DailyProgress {
    date: Date;
    hoursWorked: number;
    tasksCompleted: number;
    completionPercentage: number;
}

export interface WeeklyHours {
    weekStartDate: Date;
    plannedHours: number;
    actualHours: number;
    variance: number;
}

export interface TeamMemberProductivity {
    employeeId: number;
    employeeName: string;
    hoursWorked: number;
    tasksCompleted: number;
    productivityScore: number;
    efficiencyRating: number;
}

export interface TaskStatusTrend {
    date: Date;
    todoTasks: number;
    inProgressTasks: number;
    completedTasks: number;
    overdueTasks: number;
}

export interface ProjectPerformance {
    overallEfficiency: number;
    qualityScore: number;
    timelineAdherence: number;
    budgetAdherence: number;
    teamSatisfaction: number;
    performanceGrade: string;
    strengthAreas: string[];
    improvementAreas: string[];
}

export interface ProjectAlert {
    id: number;
    projectId: number;
    alertType: string;
    message: string;
    severity: string;
    createdAt: Date;
    isResolved: boolean;
    resolvedBy?: number;
    resolvedAt?: Date;
}

export interface ProjectRisk {
    id: number;
    projectId: number;
    riskType: string;
    description: string;
    severity: string;
    probability: number;
    impact: number;
    mitigationPlan: string;
    status: string;
    assignedTo?: number;
    assignedToName?: string;
    identifiedAt: Date;
    resolvedAt?: Date;
}

export interface ProjectDashboard {
    teamLeaderId: number;
    teamLeaderName: string;
    projectAnalytics: ProjectAnalytics[];
    teamOverview: TeamOverview;
    criticalAlerts: ProjectAlert[];
    highRisks: ProjectRisk[];
}

export interface TeamOverview {
    totalProjects: number;
    activeProjects: number;
    completedProjects: number;
    delayedProjects: number;
    totalBudget: number;
    budgetUtilized: number;
    totalTeamMembers: number;
    overallProductivity: number;
    averageProjectHealth: number;
}

// Project Collaboration Models
export interface ProjectCollaboration {
    projectId: number;
    projectName: string;
    comments: ProjectComment[];
    activities: ProjectActivity[];
    teamMembers: ProjectTeamMember[];
    communicationStats: ProjectCommunicationStats;
}

export interface ProjectComment {
    id: number;
    projectId: number;
    taskId?: number;
    employeeId: number;
    employeeName: string;
    employeePhoto: string;
    comment: string;
    createdAt: Date;
    updatedAt?: Date;
    replies: ProjectCommentReply[];
}

export interface ProjectCommentReply {
    id: number;
    commentId: number;
    employeeId: number;
    employeeName: string;
    employeePhoto: string;
    reply: string;
    createdAt: Date;
}

export interface ProjectActivity {
    id: number;
    projectId: number;
    employeeId: number;
    employeeName: string;
    activityType: string;
    description: string;
    details: string;
    createdAt: Date;
}

export interface ProjectTeamMember {
    employeeId: number;
    employeeName: string;
    employeePhoto: string;
    role: string;
    joinedAt: Date;
}

export interface ProjectCommunicationStats {
    totalComments: number;
    totalActivities: number;
    activeTeamMembers: number;
    lastActivity: Date;
    memberActivities: TeamMemberActivity[];
}

export interface TeamMemberActivity {
    employeeId: number;
    employeeName: string;
    commentsCount: number;
    activitiesCount: number;
    lastActivity: Date;
}

export interface CreateProjectComment {
    projectId: number;
    taskId?: number;
    comment: string;
}

export interface CreateCommentReply {
    commentId: number;
    reply: string;
}