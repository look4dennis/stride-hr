export interface PerformanceReview {
  id: number;
  employeeId: number;
  reviewerId: number;
  reviewPeriod: string;
  startDate: Date;
  endDate: Date;
  status: PerformanceReviewStatus;
  overallRating: number;
  goals: PerformanceGoal[];
  feedback: string;
  employeeSelfAssessment: string;
  managerComments: string;
  createdAt: Date;
  updatedAt: Date;
  employee?: {
    id: number;
    firstName: string;
    lastName: string;
    employeeId: string;
    designation: string;
    profilePhoto?: string;
  };
  reviewer?: {
    id: number;
    firstName: string;
    lastName: string;
    employeeId: string;
  };
}

export interface PerformanceGoal {
  id: number;
  performanceReviewId: number;
  title: string;
  description: string;
  targetValue: string;
  actualValue?: string;
  weight: number;
  status: GoalStatus;
  rating: number;
  comments: string;
}

export interface PIP {
  id: number;
  employeeId: number;
  managerId: number;
  title: string;
  description: string;
  startDate: Date;
  endDate: Date;
  status: PIPStatus;
  improvementAreas: ImprovementArea[];
  milestones: PIPMilestone[];
  supportResources: string;
  finalOutcome?: PIPOutcome;
  createdAt: Date;
  updatedAt: Date;
  employee?: {
    id: number;
    firstName: string;
    lastName: string;
    employeeId: string;
    designation: string;
    profilePhoto?: string;
  };
  manager?: {
    id: number;
    firstName: string;
    lastName: string;
    employeeId: string;
  };
}

export interface ImprovementArea {
  id: number;
  pipId: number;
  area: string;
  currentState: string;
  expectedState: string;
  actionPlan: string;
  progress: number;
  status: ImprovementStatus;
}

export interface PIPMilestone {
  id: number;
  pipId: number;
  title: string;
  description: string;
  dueDate: Date;
  completedDate?: Date;
  status: MilestoneStatus;
  feedback: string;
}

export interface TrainingModule {
  id: number;
  title: string;
  description: string;
  category: string;
  duration: number; // in minutes
  difficulty: TrainingDifficulty;
  content: string;
  materials: TrainingMaterial[];
  assessments: Assessment[];
  prerequisites: number[];
  isActive: boolean;
  createdBy: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface TrainingMaterial {
  id: number;
  trainingModuleId: number;
  title: string;
  type: MaterialType;
  url: string;
  description: string;
  order: number;
}

export interface Assessment {
  id: number;
  trainingModuleId: number;
  title: string;
  description: string;
  passingScore: number;
  timeLimit: number; // in minutes
  questions: AssessmentQuestion[];
  isActive: boolean;
}

export interface AssessmentQuestion {
  id: number;
  assessmentId: number;
  question: string;
  type: QuestionType;
  options: string[];
  correctAnswer: string;
  points: number;
  explanation: string;
}

export interface EmployeeTraining {
  id: number;
  employeeId: number;
  trainingModuleId: number;
  enrolledDate: Date;
  startedDate?: Date;
  completedDate?: Date;
  status: TrainingStatus;
  progress: number;
  score?: number;
  attempts: number;
  certificateIssued: boolean;
  certificateUrl?: string;
  employee?: {
    id: number;
    firstName: string;
    lastName: string;
    employeeId: string;
    profilePhoto?: string;
  };
  trainingModule?: TrainingModule;
}

export interface Certification {
  id: number;
  employeeId: number;
  trainingModuleId: number;
  certificateNumber: string;
  issuedDate: Date;
  expiryDate?: Date;
  score: number;
  certificateUrl: string;
  isValid: boolean;
  employee?: {
    id: number;
    firstName: string;
    lastName: string;
    employeeId: string;
    profilePhoto?: string;
  };
  trainingModule?: {
    id: number;
    title: string;
    category: string;
  };
}

// Enums
export enum PerformanceReviewStatus {
  Draft = 'Draft',
  InProgress = 'InProgress',
  EmployeeReview = 'EmployeeReview',
  ManagerReview = 'ManagerReview',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export enum GoalStatus {
  NotStarted = 'NotStarted',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Exceeded = 'Exceeded',
  NotMet = 'NotMet'
}

export enum PIPStatus {
  Active = 'Active',
  OnTrack = 'OnTrack',
  AtRisk = 'AtRisk',
  Completed = 'Completed',
  Extended = 'Extended',
  Terminated = 'Terminated'
}

export enum PIPOutcome {
  Successful = 'Successful',
  Extended = 'Extended',
  Terminated = 'Terminated'
}

export enum ImprovementStatus {
  NotStarted = 'NotStarted',
  InProgress = 'InProgress',
  Completed = 'Completed',
  NeedsAttention = 'NeedsAttention'
}

export enum MilestoneStatus {
  Pending = 'Pending',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Overdue = 'Overdue'
}

export enum TrainingDifficulty {
  Beginner = 'Beginner',
  Intermediate = 'Intermediate',
  Advanced = 'Advanced',
  Expert = 'Expert'
}

export enum MaterialType {
  Document = 'Document',
  Video = 'Video',
  Audio = 'Audio',
  Link = 'Link',
  Presentation = 'Presentation'
}

export enum QuestionType {
  MultipleChoice = 'MultipleChoice',
  TrueFalse = 'TrueFalse',
  ShortAnswer = 'ShortAnswer',
  Essay = 'Essay'
}

export enum TrainingStatus {
  NotStarted = 'NotStarted',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Failed = 'Failed',
  Expired = 'Expired'
}

// DTOs for API calls
export interface CreatePerformanceReviewDto {
  employeeId: number;
  reviewPeriod: string;
  startDate: Date;
  endDate: Date;
  goals: CreatePerformanceGoalDto[];
}

export interface CreatePerformanceGoalDto {
  title: string;
  description: string;
  targetValue: string;
  weight: number;
}

export interface CreatePIPDto {
  employeeId: number;
  title: string;
  description: string;
  startDate: Date;
  endDate: Date;
  improvementAreas: CreateImprovementAreaDto[];
  milestones: CreatePIPMilestoneDto[];
  supportResources: string;
}

export interface CreateImprovementAreaDto {
  area: string;
  currentState: string;
  expectedState: string;
  actionPlan: string;
}

export interface CreatePIPMilestoneDto {
  title: string;
  description: string;
  dueDate: Date;
}

export interface CreateTrainingModuleDto {
  title: string;
  description: string;
  category: string;
  duration: number;
  difficulty: TrainingDifficulty;
  content: string;
  prerequisites: number[];
}

export interface EnrollEmployeeDto {
  employeeId: number;
  trainingModuleId: number;
}