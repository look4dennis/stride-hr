import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  LeaveRequest,
  CreateLeaveRequest,
  LeaveBalance,
  LeavePolicy,
  LeaveCalendarEntry,
  LeaveConflict,
  LeaveApproval,
  LeaveRequestFilter,
  LeaveCalendarFilter,
  CalendarEvent,
  LeaveStatus,
  LeaveType
} from '../models/leave.models';

@Injectable({
  providedIn: 'root'
})
export class LeaveService {
  private readonly apiUrl = `${environment.apiUrl}/api/leave`;
  
  // State management
  private leaveRequestsSubject = new BehaviorSubject<LeaveRequest[]>([]);
  private leaveBalancesSubject = new BehaviorSubject<LeaveBalance[]>([]);
  private leavePoliciesSubject = new BehaviorSubject<LeavePolicy[]>([]);
  
  public leaveRequests$ = this.leaveRequestsSubject.asObservable();
  public leaveBalances$ = this.leaveBalancesSubject.asObservable();
  public leavePolicies$ = this.leavePoliciesSubject.asObservable();

  constructor(private http: HttpClient) {}

  // Leave Request Management
  createLeaveRequest(request: CreateLeaveRequest): Observable<LeaveRequest> {
    return this.http.post<any>(`${this.apiUrl}/requests`, request).pipe(
      map(response => this.mapLeaveRequestDates(response.data)),
      tap(() => this.refreshLeaveRequests())
    );
  }

  getLeaveRequest(id: number): Observable<LeaveRequest> {
    return this.http.get<any>(`${this.apiUrl}/requests/${id}`).pipe(
      map(response => this.mapLeaveRequestDates(response.data))
    );
  }

  getMyLeaveRequests(): Observable<LeaveRequest[]> {
    return this.http.get<any>(`${this.apiUrl}/requests`).pipe(
      map(response => response.data.map((req: any) => this.mapLeaveRequestDates(req))),
      tap(requests => this.leaveRequestsSubject.next(requests))
    );
  }

  getEmployeeLeaveRequests(employeeId: number): Observable<LeaveRequest[]> {
    return this.http.get<any>(`${this.apiUrl}/requests/employee/${employeeId}`).pipe(
      map(response => response.data.map((req: any) => this.mapLeaveRequestDates(req)))
    );
  }

  getPendingRequests(): Observable<LeaveRequest[]> {
    return this.http.get<any>(`${this.apiUrl}/requests/pending`).pipe(
      map(response => response.data.map((req: any) => this.mapLeaveRequestDates(req)))
    );
  }

  getRequestsForApproval(): Observable<LeaveRequest[]> {
    return this.http.get<any>(`${this.apiUrl}/requests/for-approval`).pipe(
      map(response => response.data.map((req: any) => this.mapLeaveRequestDates(req)))
    );
  }

  updateLeaveRequest(id: number, request: CreateLeaveRequest): Observable<LeaveRequest> {
    return this.http.put<any>(`${this.apiUrl}/requests/${id}`, request).pipe(
      map(response => this.mapLeaveRequestDates(response.data)),
      tap(() => this.refreshLeaveRequests())
    );
  }

  cancelLeaveRequest(id: number): Observable<boolean> {
    return this.http.delete<any>(`${this.apiUrl}/requests/${id}`).pipe(
      map(response => response.success),
      tap(() => this.refreshLeaveRequests())
    );
  }

  // Leave Approval Workflow
  approveLeaveRequest(id: number, approval: LeaveApproval): Observable<LeaveRequest> {
    return this.http.post<any>(`${this.apiUrl}/requests/${id}/approve`, approval).pipe(
      map(response => this.mapLeaveRequestDates(response.data)),
      tap(() => this.refreshLeaveRequests())
    );
  }

  rejectLeaveRequest(id: number, rejection: LeaveApproval): Observable<LeaveRequest> {
    return this.http.post<any>(`${this.apiUrl}/requests/${id}/reject`, rejection).pipe(
      map(response => this.mapLeaveRequestDates(response.data)),
      tap(() => this.refreshLeaveRequests())
    );
  }

  escalateLeaveRequest(id: number, escalateToId: number, comments?: string): Observable<LeaveRequest> {
    return this.http.post<any>(`${this.apiUrl}/requests/${id}/escalate`, {
      escalateToId,
      comments
    }).pipe(
      map(response => this.mapLeaveRequestDates(response.data)),
      tap(() => this.refreshLeaveRequests())
    );
  }

  // Leave Balance Management
  getMyLeaveBalances(): Observable<LeaveBalance[]> {
    return this.http.get<any>(`${this.apiUrl}/balances`).pipe(
      map(response => response.data),
      tap(balances => this.leaveBalancesSubject.next(balances))
    );
  }

  getEmployeeLeaveBalances(employeeId: number): Observable<LeaveBalance[]> {
    return this.http.get<any>(`${this.apiUrl}/balances/employee/${employeeId}`).pipe(
      map(response => response.data)
    );
  }

  getLeaveBalance(employeeId: number, policyId: number, year: number): Observable<LeaveBalance> {
    return this.http.get<any>(`${this.apiUrl}/balances/${employeeId}/${policyId}/${year}`).pipe(
      map(response => response.data)
    );
  }

  // Leave Policy Management
  getLeavePolicies(): Observable<LeavePolicy[]> {
    return this.http.get<any>(`${this.apiUrl}/policies`).pipe(
      map(response => response.data),
      tap(policies => this.leavePoliciesSubject.next(policies))
    );
  }

  getLeavePolicy(id: number): Observable<LeavePolicy> {
    return this.http.get<any>(`${this.apiUrl}/policies/${id}`).pipe(
      map(response => response.data)
    );
  }

  // Leave Calendar and Conflict Detection
  getLeaveCalendar(startDate: Date, endDate: Date): Observable<LeaveCalendarEntry[]> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<any>(`${this.apiUrl}/calendar`, { params }).pipe(
      map(response => response.data.map((entry: any) => this.mapCalendarEntryDates(entry)))
    );
  }

  getEmployeeLeaveCalendar(employeeId: number, startDate: Date, endDate: Date): Observable<LeaveCalendarEntry[]> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<any>(`${this.apiUrl}/calendar/employee/${employeeId}`, { params }).pipe(
      map(response => response.data.map((entry: any) => this.mapCalendarEntryDates(entry)))
    );
  }

  getTeamLeaveCalendar(startDate: Date, endDate: Date): Observable<LeaveCalendarEntry[]> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<any>(`${this.apiUrl}/calendar/team`, { params }).pipe(
      map(response => response.data.map((entry: any) => this.mapCalendarEntryDates(entry)))
    );
  }

  detectLeaveConflicts(startDate: Date, endDate: Date): Observable<LeaveConflict[]> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<any>(`${this.apiUrl}/conflicts`, { params }).pipe(
      map(response => response.data.map((conflict: any) => ({
        ...conflict,
        conflictDate: new Date(conflict.conflictDate)
      })))
    );
  }

  getTeamLeaveConflicts(startDate: Date, endDate: Date): Observable<LeaveConflict[]> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<any>(`${this.apiUrl}/conflicts/team`, { params }).pipe(
      map(response => response.data.map((conflict: any) => ({
        ...conflict,
        conflictDate: new Date(conflict.conflictDate)
      })))
    );
  }

  // Utility Methods
  calculateLeaveDays(startDate: Date, endDate: Date): Observable<number> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<any>(`${this.apiUrl}/calculate-days`, { params }).pipe(
      map(response => response.data.days)
    );
  }

  // Helper Methods
  convertToCalendarEvents(calendarEntries: LeaveCalendarEntry[]): CalendarEvent[] {
    return calendarEntries.map(entry => ({
      id: `leave-${entry.id}`,
      title: `${entry.employeeName} - ${entry.leaveTypeName}`,
      start: entry.date,
      end: entry.date,
      allDay: entry.isFullDay,
      backgroundColor: this.getLeaveTypeColor(entry.leaveType),
      borderColor: this.getLeaveTypeColor(entry.leaveType),
      textColor: '#ffffff',
      extendedProps: {
        employeeId: entry.employeeId,
        employeeName: entry.employeeName,
        leaveType: entry.leaveType,
        status: entry.status,
        requestId: entry.leaveRequestId
      }
    }));
  }

  getLeaveTypeColor(leaveType: LeaveType): string {
    const colors: { [key in LeaveType]: string } = {
      [LeaveType.Annual]: '#28a745',
      [LeaveType.Sick]: '#dc3545',
      [LeaveType.Personal]: '#17a2b8',
      [LeaveType.Maternity]: '#e83e8c',
      [LeaveType.Paternity]: '#6f42c1',
      [LeaveType.Emergency]: '#fd7e14',
      [LeaveType.Bereavement]: '#6c757d',
      [LeaveType.Study]: '#20c997',
      [LeaveType.Unpaid]: '#ffc107',
      [LeaveType.Compensatory]: '#007bff'
    };
    return colors[leaveType] || '#6c757d';
  }

  getLeaveStatusColor(status: LeaveStatus): string {
    const colors: { [key in LeaveStatus]: string } = {
      [LeaveStatus.Pending]: '#ffc107',
      [LeaveStatus.Approved]: '#28a745',
      [LeaveStatus.Rejected]: '#dc3545',
      [LeaveStatus.Cancelled]: '#6c757d',
      [LeaveStatus.PartiallyApproved]: '#17a2b8'
    };
    return colors[status] || '#6c757d';
  }

  getLeaveStatusText(status: LeaveStatus): string {
    const statusText: { [key in LeaveStatus]: string } = {
      [LeaveStatus.Pending]: 'Pending',
      [LeaveStatus.Approved]: 'Approved',
      [LeaveStatus.Rejected]: 'Rejected',
      [LeaveStatus.Cancelled]: 'Cancelled',
      [LeaveStatus.PartiallyApproved]: 'Partially Approved'
    };
    return statusText[status] || 'Unknown';
  }

  getLeaveTypeText(leaveType: LeaveType): string {
    const typeText: { [key in LeaveType]: string } = {
      [LeaveType.Annual]: 'Annual Leave',
      [LeaveType.Sick]: 'Sick Leave',
      [LeaveType.Personal]: 'Personal Leave',
      [LeaveType.Maternity]: 'Maternity Leave',
      [LeaveType.Paternity]: 'Paternity Leave',
      [LeaveType.Emergency]: 'Emergency Leave',
      [LeaveType.Bereavement]: 'Bereavement Leave',
      [LeaveType.Study]: 'Study Leave',
      [LeaveType.Unpaid]: 'Unpaid Leave',
      [LeaveType.Compensatory]: 'Compensatory Leave'
    };
    return typeText[leaveType] || 'Unknown';
  }

  // Private helper methods
  private refreshLeaveRequests(): void {
    this.getMyLeaveRequests().subscribe();
  }

  private mapLeaveRequestDates(request: any): LeaveRequest {
    return {
      ...request,
      startDate: new Date(request.startDate),
      endDate: new Date(request.endDate),
      createdAt: new Date(request.createdAt),
      approvedAt: request.approvedAt ? new Date(request.approvedAt) : undefined,
      approvalHistory: request.approvalHistory?.map((history: any) => ({
        ...history,
        actionDate: new Date(history.actionDate)
      })) || []
    };
  }

  private mapCalendarEntryDates(entry: any): LeaveCalendarEntry {
    return {
      ...entry,
      date: new Date(entry.date)
    };
  }
}