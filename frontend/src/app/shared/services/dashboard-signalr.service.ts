import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/auth/auth.service';

export interface DashboardUpdate {
  type: 'attendance' | 'employee' | 'system' | 'notification';
  data: any;
  timestamp: Date;
  branchId?: string;
  organizationId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardSignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private connectionState = new BehaviorSubject<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected);
  
  // Real-time update streams
  private dashboardUpdatesSubject = new BehaviorSubject<DashboardUpdate | null>(null);
  private attendanceUpdatesSubject = new BehaviorSubject<any>(null);
  private systemAlertsSubject = new BehaviorSubject<any>(null);
  private notificationsSubject = new BehaviorSubject<any>(null);

  // Public observables
  public connectionState$ = this.connectionState.asObservable();
  public dashboardUpdates$ = this.dashboardUpdatesSubject.asObservable();
  public attendanceUpdates$ = this.attendanceUpdatesSubject.asObservable();
  public systemAlerts$ = this.systemAlertsSubject.asObservable();
  public notifications$ = this.notificationsSubject.asObservable();

  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private hubUrl = `${environment.signalRUrl}/notificationHub`;

  constructor(private authService: AuthService) {
    this.initializeConnection();
  }

  /**
   * Initialize SignalR connection
   */
  private async initializeConnection(): Promise<void> {
    try {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(this.hubUrl, {
          withCredentials: true,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents,
          accessTokenFactory: () => {
            const token = this.authService.getToken();
            return token || '';
          }
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.previousRetryCount < 3) {
              return Math.random() * 10000;
            } else {
              return null; // Stop retrying
            }
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

  /**
   * Setup SignalR event handlers
   */
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
      this.joinDashboardGroups();
    });

    // Dashboard-specific events
    this.hubConnection.on('DashboardUpdate', (update: DashboardUpdate) => {
      console.log('Dashboard update received:', update);
      this.dashboardUpdatesSubject.next({
        ...update,
        timestamp: new Date(update.timestamp)
      });
    });

    this.hubConnection.on('AttendanceStatusUpdated', (data: any) => {
      console.log('Attendance status updated:', data);
      this.attendanceUpdatesSubject.next(data);
      
      // Also emit as dashboard update
      this.dashboardUpdatesSubject.next({
        type: 'attendance',
        data: data,
        timestamp: new Date(),
        branchId: data.branchId
      });
    });

    this.hubConnection.on('SystemAlert', (alert: any) => {
      console.log('System alert received:', alert);
      this.systemAlertsSubject.next(alert);
      
      // Also emit as dashboard update
      this.dashboardUpdatesSubject.next({
        type: 'system',
        data: alert,
        timestamp: new Date()
      });
    });

    this.hubConnection.on('EmployeeUpdate', (data: any) => {
      console.log('Employee update received:', data);
      
      // Emit as dashboard update
      this.dashboardUpdatesSubject.next({
        type: 'employee',
        data: data,
        timestamp: new Date(),
        branchId: data.branchId,
        organizationId: data.organizationId
      });
    });

    this.hubConnection.on('NotificationReceived', (notification: any) => {
      console.log('Notification received:', notification);
      this.notificationsSubject.next(notification);
      
      // Also emit as dashboard update
      this.dashboardUpdatesSubject.next({
        type: 'notification',
        data: notification,
        timestamp: new Date()
      });
    });

    // Connection confirmation
    this.hubConnection.on('ConnectionEstablished', (data: any) => {
      console.log('SignalR connection established:', data);
    });

    // Error handling
    this.hubConnection.on('Error', (error: string) => {
      console.error('SignalR error:', error);
    });
  }

  /**
   * Start SignalR connection
   */
  private async startConnection(): Promise<void> {
    if (!this.hubConnection) return;

    try {
      await this.hubConnection.start();
      console.log('SignalR connection established for dashboard');
      this.connectionState.next(signalR.HubConnectionState.Connected);
      this.reconnectAttempts = 0;

      // Join dashboard-specific groups
      await this.joinDashboardGroups();
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

  /**
   * Join dashboard-specific SignalR groups
   */
  private async joinDashboardGroups(): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      return;
    }

    try {
      const currentUser = this.authService.currentUser;
      if (!currentUser) return;

      // Join user-specific group
      await this.hubConnection.invoke('JoinGroup', `User_${currentUser.id}`);

      // Join branch group if available
      if (currentUser.branchId) {
        await this.hubConnection.invoke('JoinGroup', `Branch_${currentUser.branchId}`);
      }

      // Join organization group if available
      if (currentUser.organizationId) {
        await this.hubConnection.invoke('JoinGroup', `Organization_${currentUser.organizationId}`);
      }

      // Join role-specific groups
      for (const role of currentUser.roles) {
        await this.hubConnection.invoke('JoinGroup', `Role_${role}`);
      }

      console.log('Joined dashboard SignalR groups successfully');
    } catch (error) {
      console.log('Failed to join dashboard SignalR groups:', error);
    }
  }

  /**
   * Connect to SignalR hub
   */
  public async connect(): Promise<void> {
    if (!this.hubConnection) {
      await this.initializeConnection();
    } else if (this.hubConnection.state === signalR.HubConnectionState.Disconnected) {
      await this.startConnection();
    }
  }

  /**
   * Disconnect from SignalR hub
   */
  public async disconnect(): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.hubConnection.stop();
        console.log('SignalR dashboard connection stopped');
      } catch (error) {
        console.error('Error stopping SignalR dashboard connection:', error);
      }
    }
  }

  /**
   * Check if connected to SignalR
   */
  public isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }

  /**
   * Send dashboard refresh request
   */
  public async requestDashboardRefresh(): Promise<void> {
    if (!this.isConnected()) return;

    try {
      await this.hubConnection!.invoke('RequestDashboardRefresh');
    } catch (error) {
      console.error('Error requesting dashboard refresh:', error);
    }
  }

  /**
   * Send attendance status update
   */
  public async updateAttendanceStatus(status: string): Promise<void> {
    if (!this.isConnected()) return;

    try {
      await this.hubConnection!.invoke('UpdateAttendanceStatus', status);
    } catch (error) {
      console.error('Error updating attendance status:', error);
    }
  }

  /**
   * Acknowledge system alert
   */
  public async acknowledgeAlert(alertId: number): Promise<void> {
    if (!this.isConnected()) return;

    try {
      await this.hubConnection!.invoke('AcknowledgeSystemAlert', alertId);
    } catch (error) {
      console.error('Error acknowledging alert:', error);
    }
  }

  /**
   * Get connection statistics (admin only)
   */
  public async getConnectionStats(): Promise<void> {
    if (!this.isConnected()) return;

    try {
      await this.hubConnection!.invoke('GetConnectionStats');
    } catch (error) {
      console.error('Error getting connection stats:', error);
    }
  }

  /**
   * Ping server for connection health check
   */
  public async ping(): Promise<void> {
    if (!this.isConnected()) return;

    try {
      await this.hubConnection!.invoke('Ping');
    } catch (error) {
      console.error('Error pinging server:', error);
    }
  }

  /**
   * Get current connection state
   */
  public getConnectionState(): signalR.HubConnectionState {
    return this.hubConnection?.state || signalR.HubConnectionState.Disconnected;
  }

  /**
   * Filter dashboard updates by type
   */
  public getDashboardUpdatesByType(type: DashboardUpdate['type']): Observable<DashboardUpdate> {
    return new Observable(observer => {
      this.dashboardUpdates$.subscribe(update => {
        if (update && update.type === type) {
          observer.next(update);
        }
      });
    });
  }

  /**
   * Filter dashboard updates by branch
   */
  public getDashboardUpdatesByBranch(branchId: string): Observable<DashboardUpdate> {
    return new Observable(observer => {
      this.dashboardUpdates$.subscribe(update => {
        if (update && update.branchId === branchId) {
          observer.next(update);
        }
      });
    });
  }
}