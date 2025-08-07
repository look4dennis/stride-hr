export interface Employee {
  id: number;
  employeeId: string;
  branchId: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  profilePhoto?: string;
  dateOfBirth: string;
  joiningDate: string;
  designation: string;
  department: string;
  basicSalary: number;
  status: EmployeeStatus;
  reportingManagerId?: number;
  reportingManager?: Employee;
  subordinates?: Employee[];
  roles?: string[];
  createdAt: string;
  updatedAt?: string;
  branch?: Branch;
}

export interface Branch {
  id: number;
  organizationId: number;
  name: string;
  country: string;
  currency: string;
  timeZone: string;
  address: string;
  localHolidays: string[];
  complianceSettings: Record<string, any>;
}

export interface CreateEmployeeDto {
  branchId: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  joiningDate: string;
  designation: string;
  department: string;
  basicSalary: number;
  reportingManagerId?: number;
  profilePhoto?: File;
}

export interface UpdateEmployeeDto {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  dateOfBirth?: string;
  designation?: string;
  department?: string;
  basicSalary?: number;
  reportingManagerId?: number;
  status?: EmployeeStatus;
  profilePhoto?: File;
}

export interface EmployeeSearchCriteria {
  searchTerm?: string;
  department?: string;
  designation?: string;
  branchId?: number;
  status?: EmployeeStatus;
  reportingManagerId?: number;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface EmployeeOnboardingStep {
  id: string;
  title: string;
  description: string;
  completed: boolean;
  required: boolean;
  order: number;
}

export interface EmployeeOnboarding {
  employeeId: number;
  steps: EmployeeOnboardingStep[];
  overallProgress: number;
  startedAt: string;
  completedAt?: string;
  status: OnboardingStatus;
}

export interface EmployeeExitProcess {
  employeeId: number;
  exitDate: string;
  reason: string;
  exitType: ExitType;
  handoverNotes?: string;
  assetsToReturn: AssetHandover[];
  clearanceSteps: ClearanceStep[];
  finalSettlement?: FinalSettlement;
  status: ExitStatus;
}

export interface AssetHandover {
  assetId: number;
  assetName: string;
  assetType: string;
  condition: string;
  returnedAt?: string;
  notes?: string;
}

export interface ClearanceStep {
  id: string;
  department: string;
  description: string;
  completed: boolean;
  completedBy?: number;
  completedAt?: string;
  notes?: string;
}

export interface FinalSettlement {
  basicSalary: number;
  pendingLeaves: number;
  leaveEncashment: number;
  bonus: number;
  deductions: number;
  totalAmount: number;
  currency: string;
}

export interface OrganizationalChart {
  employee: Employee;
  children: OrganizationalChart[];
  level: number;
}

export enum EmployeeStatus {
  Active = 'Active',
  Inactive = 'Inactive',
  OnLeave = 'OnLeave',
  Terminated = 'Terminated',
  Resigned = 'Resigned'
}

export enum OnboardingStatus {
  NotStarted = 'NotStarted',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Delayed = 'Delayed'
}

export enum ExitType {
  Resignation = 'Resignation',
  Termination = 'Termination',
  Retirement = 'Retirement',
  EndOfContract = 'EndOfContract'
}

export enum ExitStatus {
  Initiated = 'Initiated',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export interface EmployeeRole {
  id: number;
  employeeId: number;
  employeeName: string;
  roleId: number;
  roleName: string;
  roleDescription: string;
  assignedDate: string;
  revokedDate?: string;
  assignedBy: number;
  assignedByName: string;
  revokedBy?: number;
  revokedByName?: string;
  isActive: boolean;
  notes?: string;
}

export interface EmployeeRoleModel {
  id: number;
  name: string;
  description: string;
  permissions: string[];
}

export interface AssignRoleDto {
  employeeId: number;
  roleId: number;
  notes?: string;
}

export interface RevokeRoleDto {
  employeeId: number;
  roleId: number;
  notes?: string;
}