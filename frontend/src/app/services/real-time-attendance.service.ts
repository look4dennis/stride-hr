import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { 
  AttendanceStatus, 
  TodayAttendanceOverview,
  EmployeeAttendanceStatus,
  AttendanceRecord 
} from '../models/attendance.models';

export interface AttendanceUpdate {
  type: 'checkin' | 'checkout' | 'break_start' | 'break_end' | 'status_change';
  employeeId: number;
  branchId: number;
  data: any;
  timestamp: string;
}

@Injectable({
  providedIn: 'root'
})
export class RealTimeAttendanceService {
  private hubConnection: signalR.HubConnection | null = null;
  private connectionState = new BehaviorSubject<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected);
  
  // Real-time data streams
  private attendanceUpdates = new BehaviorSubject<AttendanceUpdate | null>(null);
  private personalStatusUpdates = new BehaviorSubject<AttendanceStatus | null>(null);
  private teamOverviewUpdates = new BehaviorSubject<TodayAttendanceOverview | null>(null);

  public connectionState$ = this.connectionState.asObservable();
  public attendanceUpdates$ = this.attendanceUpdates.asObservable();
  public personalStatusUpdates$ = this.personalStatusUpdates.asObservable();
  public teamOverviewUpdates$ = this.teamOverviewUpdates.asObservable();

  private readonly hubUrl = 'http://localhost:5000/hubs/attendance';
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;

  constructor() {
    // Only initialize in browser environment
    if (typeof window !== 'undefined' && !window.location.href.includes('localhost:9876')) {
      this.initializeConnection();
    }
  }

  private async initializeConnection(): Promise<void> {
    try {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(this.hubUrl, {
          withCredentials: true,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.previousRetryCount < 3) {
              return 1000 * Math.pow(2, retryContext.previousRetryCount); // Exponential backoff
            }
            return 30000; // 30 seconds for subsequent attempts
          }
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      this.setupEventHandlers();
      await this.startConnection();
    } catch (error) {
      console.log('SignalR initialization failed (expected during development):', error);
    }
  }

  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    // Connection state events
    this.hubConnection.onclose((error) => {
      console.log('SignalR connection closed:', error);
      this.connectionState.next(signalR.HubConnectionState.Disconnected);
    });

    this.hubConnection.onreconnecting((error) => {
      console.log('SignalR reconnecting:', error);
      this.connectionState.next(signalR.HubConnectionState.Reconnecting);
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId);
      this.connectionState.next(signalR.HubConnectionState.Connected);
      this.reconnectAttempts = 0;
    });

    // Attendance event handlers
    this.hubConnection.on('AttendanceUpdate', (update: AttendanceUpdate) => {
      console.log('Received attendance update:', update);
      this.attendanceUpdates.next(update);
    });

    this.hubConnection.on('PersonalStatusUpdate', (status: AttendanceStatus) => {
      console.log('Received personal status update:', status);
      this.personalStatusUpdates.next(status);
    });

    this.hubConnection.on('TeamOverviewUpdate', (overview: TodayAttendanceOverview) => {
      console.log('Received team overview update:', overview);
      this.teamOverviewUpdates.next(overview);
    });

    this.hubConnection.on('EmployeeCheckedIn', (data: { employeeId: number, record: AttendanceRecord }) => {
      this.handleEmployeeUpdate('checkin', data);
    });

    this.hubConnection.on('EmployeeCheckedOut', (data: { employeeId: number, record: AttendanceRecord }) => {
      this.handleEmployeeUpdate('checkout', data);
    });

    this.hubConnection.on('EmployeeStartedBreak', (data: { employeeId: number, record: AttendanceRecord }) => {
      this.handleEmployeeUpdate('break_start', data);
    });

    this.hubConnection.on('EmployeeEndedBreak', (data: { employeeId: number, record: AttendanceRecord }) => {
      this.handleEmployeeUpdate('break_end', data);
    });

    this.hubConnection.on('BranchAttendanceUpdate', (branchId: number, overview: TodayAttendanceOverview) => {
      console.log(`Branch ${branchId} attendance updated:`, overview);
      this.teamOverviewUpdates.next(overview);
    });
  }

  private async startConnection(): Promise<void> {
    if (!this.hubConnection) return;

    try {
      await this.hubConnection.start();
      console.log('SignalR connection established');
      this.connectionState.next(signalR.HubConnectionState.Connected);
      this.reconnectAttempts = 0;

      // Join user-specific groups
      await this.joinUserGroups();
    } catch (error) {
      console.log('SignalR connection failed (expected during development):', error);
      this.connectionState.next(signalR.HubConnectionState.Disconnected);
      
      // Retry connection
      if (this.reconnectAttempts < this.maxReconnectAttempts) {
        this.reconnectAttempts++;
        setTimeout(() => this.startConnection(), 5000 * this.reconnectAttempts);
      }
    }
  }

  private async joinUserGroups(): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      return;
    }

    try {
      // Join personal updates group
      await this.hubConnection.invoke('JoinPersonalGroup');
      
      // Join branch group if user has appropriate role
      // This would be determined by user's role and branch
      const userBranchId = this.getCurrentUserBranchId();
      if (userBranchId) {
        await this.hubConnection.invoke('JoinBranchGroup', userBranchId);
      }

      console.log('Joined SignalR groups successfully');
    } catch (error) {
      console.log('Failed to join SignalR groups:', error);
    }
  }

  private handleEmployeeUpdate(type: AttendanceUpdate['type'], data: any): void {
    const update: AttendanceUpdate = {
      type,
      employeeId: data.employeeId,
      branchId: data.record?.branchId || 0,
      data: data.record,
      timestamp: new Date().toISOString()
    };

    this.attendanceUpdates.next(update);
  }

  // Public methods
  async connect(): Promise<void> {
    if (!this.hubConnection) {
      await this.initializeConnection();
    } else if (this.hubConnection.state === signalR.HubConnectionState.Disconnected) {
      await this.startConnection();
    }
  }

  async disconnect(): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.hubConnection.stop();
        console.log('SignalR connection stopped');
      } catch (error) {
        console.error('Error stopping SignalR connection:', error);
      }
    }
  }

  isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }

  // Subscribe to specific employee updates
  async subscribeToEmployee(employeeId: number): Promise<void> {
    if (this.isConnected()) {
      try {
        await this.hubConnection!.invoke('SubscribeToEmployee', employeeId);
      } catch (error) {
        console.error('Failed to subscribe to employee updates:', error);
      }
    }
  }

  // Unsubscribe from specific employee updates
  async unsubscribeFromEmployee(employeeId: number): Promise<void> {
    if (this.isConnected()) {
      try {
        await this.hubConnection!.invoke('UnsubscribeFromEmployee', employeeId);
      } catch (error) {
        console.error('Failed to unsubscribe from employee updates:', error);
      }
    }
  }

  // Subscribe to branch updates
  async subscribeToBranch(branchId: number): Promise<void> {
    if (this.isConnected()) {
      try {
        await this.hubConnection!.invoke('JoinBranchGroup', branchId);
      } catch (error) {
        console.error('Failed to subscribe to branch updates:', error);
      }
    }
  }

  // Send attendance action notifications
  async notifyAttendanceAction(action: string, data: any): Promise<void> {
    if (this.isConnected()) {
      try {
        await this.hubConnection!.invoke('NotifyAttendanceAction', action, data);
      } catch (error) {
        console.error('Failed to notify attendance action:', error);
      }
    }
  }

  // Request real-time status update
  async requestStatusUpdate(): Promise<void> {
    if (this.isConnected()) {
      try {
        await this.hubConnection!.invoke('RequestStatusUpdate');
      } catch (error) {
        console.error('Failed to request status update:', error);
      }
    }
  }

  // Request team overview update
  async requestTeamOverviewUpdate(branchId?: number): Promise<void> {
    if (this.isConnected()) {
      try {
        await this.hubConnection!.invoke('RequestTeamOverviewUpdate', branchId);
      } catch (error) {
        console.error('Failed to request team overview update:', error);
      }
    }
  }

  // Utility methods
  private getCurrentUserBranchId(): number | null {
    // This would get the current user's branch ID from auth service
    // For now, return null as a placeholder
    return null;
  }

  // Cleanup
  ngOnDestroy(): void {
    this.disconnect();
  }
}