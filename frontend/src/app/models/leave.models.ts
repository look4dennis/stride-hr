export enum LeaveStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
  Cancelled = 4,
  PartiallyApproved = 5
}

export enum LeaveType {
  Annual = 1,
  Sick = 2,
  Personal = 3,
  Maternity = 4,
  Paternity = 5,
  Emergency = 6,
  Bereavement = 7,
  Study = 8,
  Unpaid = 9,
  Compensatory = 10
}

export enum ApprovalLevel {
  Manager = 1,
  HR = 2,
  Admin = 3
}

export enum ApprovalAction {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
  Escalated = 4
}

export interface LeaveRequest {
  id: number;
  employeeId: number;
  employeeName: string;
  leavePolicyId: number;
  leaveType: LeaveType;
  leaveTypeName: string;
  startDate: Date;
  endDate: Date;
  requestedDays: number;
  approvedDays: number;
  reason: string;
  comments?: string;
  status: LeaveStatus;
  approvedAt?: Date;
  approvedBy?: number;
  approvedByName?: string;
  rejectionReason?: string;
  isEmergency: boolean;
  attachmentPath?: string;
  createdAt: Date;
  approvalHistory: LeaveApprovalHistory[];
}

export interface CreateLeaveRequest {
  leavePolicyId: number;
  startDate: Date;
  endDate: Date;
  reason: string;
  comments?: string;
  isEmergency: boolean;
  attachmentPath?: string;
}

export interface LeaveBalance {
  id: number;
  employeeId: number;
  leavePolicyId: number;
  leaveType: LeaveType;
  leaveTypeName: string;
  year: number;
  allocatedDays: number;
  usedDays: number;
  carriedForwardDays: number;
  encashedDays: number;
  remainingDays: number;
}

export interface LeavePolicy {
  id: number;
  branchId: number;
  leaveType: LeaveType;
  name: string;
  description: string;
  annualAllocation: number;
  maxConsecutiveDays: number;
  minAdvanceNoticeDays: number;
  requiresApproval: boolean;
  isCarryForwardAllowed: boolean;
  maxCarryForwardDays: number;
  isEncashmentAllowed: boolean;
  encashmentRate: number;
  isActive: boolean;
}

export interface LeaveCalendarEntry {
  id: number;
  employeeId: number;
  employeeName: string;
  leaveRequestId: number;
  date: Date;
  isFullDay: boolean;
  startTime?: string;
  endTime?: string;
  leaveType: LeaveType;
  leaveTypeName: string;
  status: LeaveStatus;
}

export interface LeaveConflict {
  employeeId: number;
  employeeName: string;
  department: string;
  conflictDate: Date;
  conflictReason: string;
  conflictingRequestId: number;
}

export interface LeaveApprovalHistory {
  id: number;
  leaveRequestId: number;
  approverId: number;
  approverName: string;
  level: ApprovalLevel;
  action: ApprovalAction;
  comments?: string;
  escalatedToId?: number;
  escalatedToName?: string;
  actionDate: Date;
}

export interface LeaveApproval {
  approvedDays?: number;
  comments?: string;
}

export interface LeaveRequestFilter {
  status?: LeaveStatus;
  leaveType?: LeaveType;
  startDate?: Date;
  endDate?: Date;
  employeeId?: number;
  department?: string;
}

export interface LeaveCalendarFilter {
  startDate: Date;
  endDate: Date;
  employeeId?: number;
  department?: string;
  leaveType?: LeaveType;
}

// Calendar event interface for FullCalendar integration
export interface CalendarEvent {
  id: string;
  title: string;
  start: Date;
  end: Date;
  allDay: boolean;
  backgroundColor: string;
  borderColor: string;
  textColor: string;
  extendedProps: {
    employeeId: number;
    employeeName: string;
    leaveType: LeaveType;
    status: LeaveStatus;
    requestId: number;
  };
}