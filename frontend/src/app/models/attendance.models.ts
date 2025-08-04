export interface AttendanceRecord {
  id: number;
  employeeId: number;
  date: string;
  checkInTime?: string;
  checkOutTime?: string;
  totalWorkingHours?: string;
  breakDuration?: string;
  overtimeHours?: string;
  status: AttendanceStatusType;
  location?: string;
  notes?: string;
  correctionReason?: string;
  correctedBy?: number;
  correctedAt?: Date;
  isLate: boolean;
  lateBy?: string;
  isEarlyOut: boolean;
  earlyOutBy?: string;
  employee?: {
    id: number;
    employeeId: string;
    firstName: string;
    lastName: string;
    profilePhoto?: string;
    designation: string;
    department: string;
  };
  breakRecords?: BreakRecord[];
}

export interface BreakRecord {
  id: number;
  attendanceRecordId: number;
  type: BreakType;
  startTime: string;
  endTime?: string;
  duration?: string;
}

export interface CheckInDto {
  location?: string;
  timestamp?: string;
}

export interface CheckOutDto {
  timestamp?: string;
}

export interface StartBreakDto {
  timestamp?: string;
}

export interface EndBreakDto {
  timestamp?: string;
}

export interface AttendanceStatus {
  employeeId: number;
  isCheckedIn: boolean;
  currentStatus: AttendanceStatusType;
  checkInTime?: string;
  currentBreak?: BreakRecord;
  totalWorkingHours: string;
  totalBreakTime: string;
  location?: string;
}

export interface AttendanceReportCriteria {
  branchId?: number;
  departmentId?: string;
  employeeId?: number;
  startDate: string;
  endDate: string;
  status?: AttendanceStatusType;
}

export interface AttendanceReport {
  criteria: AttendanceReportCriteria;
  records: AttendanceRecord[];
  summary: AttendanceSummary;
}

export interface AttendanceSummary {
  totalEmployees: number;
  presentCount: number;
  absentCount: number;
  lateCount: number;
  onBreakCount: number;
  onLeaveCount: number;
  averageWorkingHours: string;
  totalOvertimeHours: string;
}

export interface TodayAttendanceOverview {
  branchId: number;
  date: string;
  summary: AttendanceSummary;
  employeeStatuses: EmployeeAttendanceStatus[];
}

export interface EmployeeAttendanceStatus {
  employee: {
    id: number;
    employeeId: string;
    firstName: string;
    lastName: string;
    profilePhoto?: string;
    designation: string;
    department: string;
  };
  status: AttendanceStatusType;
  checkInTime?: string;
  checkOutTime?: string;
  currentBreak?: BreakRecord;
  totalWorkingHours: string;
  totalBreakTime: string;
  location?: string;
  isLate: boolean;
}

export interface LocationInfo {
  latitude: number;
  longitude: number;
  address?: string;
  accuracy?: number;
}

export enum AttendanceStatusType {
  Present = 'Present',
  Absent = 'Absent',
  Late = 'Late',
  OnBreak = 'OnBreak',
  HalfDay = 'HalfDay',
  OnLeave = 'OnLeave'
}

export enum BreakType {
  Tea = 'Tea',
  Lunch = 'Lunch',
  Personal = 'Personal',
  Meeting = 'Meeting'
}

export const BreakTypeLabels = {
  [BreakType.Tea]: 'Tea Break',
  [BreakType.Lunch]: 'Lunch Break',
  [BreakType.Personal]: 'Personal Break',
  [BreakType.Meeting]: 'Meeting Break'
};

export const AttendanceStatusLabels = {
  [AttendanceStatusType.Present]: 'Present',
  [AttendanceStatusType.Absent]: 'Absent',
  [AttendanceStatusType.Late]: 'Late',
  [AttendanceStatusType.OnBreak]: 'On Break',
  [AttendanceStatusType.HalfDay]: 'Half Day',
  [AttendanceStatusType.OnLeave]: 'On Leave'
};

export const AttendanceStatusColors = {
  [AttendanceStatusType.Present]: 'success',
  [AttendanceStatusType.Absent]: 'danger',
  [AttendanceStatusType.Late]: 'warning',
  [AttendanceStatusType.OnBreak]: 'info',
  [AttendanceStatusType.HalfDay]: 'secondary',
  [AttendanceStatusType.OnLeave]: 'primary'
};

// New models for attendance management and reporting
export interface AttendanceReportRequest {
  startDate: Date;
  endDate: Date;
  employeeId?: number;
  branchId?: number;
  departmentId?: number;
  reportType?: string;
  format?: string;
  includeBreakDetails?: boolean;
  includeOvertimeDetails?: boolean;
  includeLateArrivals?: boolean;
  includeEarlyDepartures?: boolean;
}

export interface AttendanceReportResponse {
  reportType: string;
  startDate: Date;
  endDate: Date;
  generatedAt: Date;
  totalEmployees: number;
  items: AttendanceReportItem[];
  summary: AttendanceReportSummary;
}

export interface AttendanceReportItem {
  employeeId: number;
  employeeName: string;
  employeeCode: string;
  department: string;
  totalWorkingDays: number;
  presentDays: number;
  absentDays: number;
  lateDays: number;
  earlyDepartures: number;
  totalWorkingHours: string;
  totalOvertimeHours: string;
  totalBreakTime: string;
  attendancePercentage: number;
  details?: AttendanceDetailItem[];
}

export interface AttendanceDetailItem {
  date: Date;
  checkInTime?: Date;
  checkOutTime?: Date;
  workingHours?: string;
  breakDuration?: string;
  overtimeHours?: string;
  status: string;
  isLate: boolean;
  lateBy?: string;
  isEarlyOut: boolean;
  earlyOutBy?: string;
  notes?: string;
}

export interface AttendanceReportSummary {
  totalEmployees: number;
  totalWorkingDays: number;
  averageAttendancePercentage: number;
  totalPresentDays: number;
  totalAbsentDays: number;
  totalLateDays: number;
  totalEarlyDepartures: number;
  totalWorkingHours: string;
  totalOvertimeHours: string;
  averageWorkingHoursPerDay: string;
  averageOvertimePerDay: string;
}

export interface AttendanceCalendarResponse {
  year: number;
  month: number;
  days: AttendanceCalendarDay[];
  summary: AttendanceCalendarSummary;
}

export interface AttendanceCalendarDay {
  date: Date;
  status: AttendanceStatusType;
  checkInTime?: Date;
  checkOutTime?: Date;
  workingHours?: string;
  breakDuration?: string;
  overtimeHours?: string;
  isLate: boolean;
  lateBy?: string;
  isEarlyOut: boolean;
  earlyOutBy?: string;
  isWeekend: boolean;
  isHoliday: boolean;
  holidayName?: string;
  notes?: string;
  breaks: AttendanceCalendarBreak[];
}

export interface AttendanceCalendarBreak {
  type: BreakType;
  startTime: Date;
  endTime?: Date;
  duration?: string;
}

export interface AttendanceCalendarSummary {
  totalWorkingDays: number;
  presentDays: number;
  absentDays: number;
  lateDays: number;
  earlyDepartures: number;
  weekends: number;
  holidays: number;
  totalWorkingHours: string;
  totalOvertimeHours: string;
  attendancePercentage: number;
}

export interface AttendanceCorrectionRequest {
  checkInTime?: Date;
  checkOutTime?: Date;
  reason: string;
}

export interface AddMissingAttendanceRequest {
  employeeId: number;
  date: Date;
  checkInTime: Date;
  checkOutTime?: Date;
  reason: string;
}

export interface AttendanceAlertResponse {
  id: number;
  alertType: AttendanceAlertType;
  alertMessage: string;
  employeeId?: number;
  employeeName?: string;
  branchId?: number;
  branchName?: string;
  createdAt: Date;
  isRead: boolean;
  severity: string;
  metadata: { [key: string]: any };
}

export enum AttendanceAlertType {
  LateArrival = 'LateArrival',
  EarlyDeparture = 'EarlyDeparture',
  MissedCheckIn = 'MissedCheckIn',
  MissedCheckOut = 'MissedCheckOut',
  ExcessiveBreakTime = 'ExcessiveBreakTime',
  ConsecutiveAbsences = 'ConsecutiveAbsences',
  LowAttendancePercentage = 'LowAttendancePercentage',
  OvertimeThreshold = 'OvertimeThreshold',
  UnusualWorkingHours = 'UnusualWorkingHours'
}