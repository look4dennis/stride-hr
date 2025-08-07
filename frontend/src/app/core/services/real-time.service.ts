import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, Subject, combineLatest } from 'rxjs';
import { map, filter, takeUntil } from 'rxjs/operators';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';
import { NotificationService } from './notification.service';

export interface RealTimeUpdate {
    type: 'attendance' | 'employee' | 'dashboard' | 'notification' | 'system' | 'connection';
    data: any;
    timestamp: Date;
    source: string;
    branchId?: string;
    organizationId?: string;
    userId?: string;
}

export interface ConnectionState {
    isConnected: boolean;
    isConnecting: boolean;
    isReconnecting: boolean;
    connectionId?: string;
    lastConnected?: Date;
    reconnectAttempts: number;
    error?: string;
}

export interface DashboardStatistics {
    totalEmployees: number;
    presentToday: number;
    absentToday: number;
    onBreak: number;
    lateArrivals: number;
    earlyDepartures: number;
    overtime: number;
    productivity: number;
    lastUpdated: Date;
}

export interface AttendanceUpdate {
    employeeId: number;
    employeeName: string;
    action: 'checkin' | 'checkout' | 'break_start' | 'break_end';
    timestamp: Date;
    location?: string;
    branchId: number;
}

export interface SystemNotification {
    id: string;
    type: 'info' | 'warning' | 'error' | 'success';
    title: string;
    message: string;
    priority: 'low' | 'normal' | 'high' | 'critical';
    timestamp: Date;
    actionUrl?: string;
    metadata?: any;
}

@Injectable({
    providedIn: 'root'
})
export class RealTimeService implements OnDestroy {
    private hubConnection: signalR.HubConnection | null = null;
    private destroy$ = new Subject<void>();

    // Connection state management
    private connectionState = new BehaviorSubject<ConnectionState>({
        isConnected: false,
        isConnecting: false,
        isReconnecting: false,
        reconnectAttempts: 0
    });

    // Real-time data streams
    private realTimeUpdates = new BehaviorSubject<RealTimeUpdate | null>(null);
    private dashboardStatistics = new BehaviorSubject<DashboardStatistics | null>(null);
    private attendanceUpdates = new BehaviorSubject<AttendanceUpdate | null>(null);
    private systemNotifications = new BehaviorSubject<SystemNotification | null>(null);
    private onlineUsers = new BehaviorSubject<string[]>([]);

    // Public observables
    public connectionState$ = this.connectionState.asObservable();
    public realTimeUpdates$ = this.realTimeUpdates.asObservable();
    public dashboardStatistics$ = this.dashboardStatistics.asObservable();
    public attendanceUpdates$ = this.attendanceUpdates.asObservable();
    public systemNotifications$ = this.systemNotifications.asObservable();
    public onlineUsers$ = this.onlineUsers.asObservable();

    // Configuration
    private readonly hubUrl = `${environment.signalRUrl}/notification`;
    private readonly maxReconnectAttempts = 10;
    private readonly reconnectDelays = [1000, 2000, 5000, 10000, 15000, 30000]; // Progressive delays

    // Heartbeat management
    private heartbeatInterval: any;
    private lastHeartbeat: Date | null = null;
    private missedHeartbeats = 0;
    private readonly maxMissedHeartbeats = 3;

    constructor(
        private authService: AuthService,
        private notificationService: NotificationService
    ) {
        // Initialize connection when user is authenticated
        this.authService.currentUser$.pipe(
            takeUntil(this.destroy$)
        ).subscribe((user: any) => {
            if (user && this.authService.isAuthenticated) {
                this.connect();
            } else {
                this.disconnect();
            }
        });

        // Monitor connection health
        this.startConnectionHealthMonitoring();
    }

    /**
     * Initialize and start SignalR connection
     */
    public async connect(): Promise<void> {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            return;
        }

        try {
            this.updateConnectionState({ isConnecting: true, error: undefined });

            await this.initializeConnection();
            await this.startConnection();
        } catch (error) {
            console.error('Failed to connect to SignalR hub:', error);
            this.updateConnectionState({
                isConnecting: false,
                error: error instanceof Error ? error.message : 'Connection failed'
            });
        }
    }

    /**
     * Disconnect from SignalR hub
     */
    public async disconnect(): Promise<void> {
        this.stopHeartbeat();

        if (this.hubConnection) {
            try {
                await this.hubConnection.stop();
                console.log('SignalR connection stopped');
            } catch (error) {
                console.error('Error stopping SignalR connection:', error);
            }
        }

        this.updateConnectionState({
            isConnected: false,
            isConnecting: false,
            isReconnecting: false,
            connectionId: undefined
        });
    }

    /**
     * Initialize SignalR connection with configuration
     */
    private async initializeConnection(): Promise<void> {
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
                    const delay = this.reconnectDelays[Math.min(retryContext.previousRetryCount, this.reconnectDelays.length - 1)];
                    console.log(`SignalR reconnect attempt ${retryContext.previousRetryCount + 1}, delay: ${delay}ms`);
                    return delay;
                }
            })
            .configureLogging(environment.production ? signalR.LogLevel.Warning : signalR.LogLevel.Information)
            .build();

        this.setupEventHandlers();
    }

    /**
     * Setup SignalR event handlers
     */
    private setupEventHandlers(): void {
        if (!this.hubConnection) return;

        // Connection state events
        this.hubConnection.onclose((error) => {
            console.log('SignalR connection closed:', error);
            this.stopHeartbeat();
            this.updateConnectionState({
                isConnected: false,
                isReconnecting: false,
                error: error?.message
            });
        });

        this.hubConnection.onreconnecting((error) => {
            console.log('SignalR reconnecting:', error);
            this.updateConnectionState({
                isReconnecting: true,
                isConnected: false
            });
        });

        this.hubConnection.onreconnected((connectionId) => {
            console.log('SignalR reconnected:', connectionId);
            const currentState = this.connectionState.value;
            this.updateConnectionState({
                isConnected: true,
                isReconnecting: false,
                connectionId,
                lastConnected: new Date(),
                reconnectAttempts: 0,
                error: undefined
            });

            this.startHeartbeat();
            this.rejoinGroups();
        });

        // Real-time event handlers
        this.setupRealTimeEventHandlers();
    }

    /**
     * Setup real-time event handlers for different types of updates
     */
    private setupRealTimeEventHandlers(): void {
        if (!this.hubConnection) return;

        // Connection establishment
        this.hubConnection.on('ConnectionEstablished', (data: any) => {
            console.log('SignalR connection established:', data);
            this.updateConnectionState({
                isConnected: true,
                isConnecting: false,
                connectionId: data.connectionId,
                lastConnected: new Date(data.connectedAt),
                reconnectAttempts: 0
            });

            this.startHeartbeat();
            this.emitRealTimeUpdate('connection', data, 'hub');
        });

        // Dashboard statistics updates
        this.hubConnection.on('DashboardStatisticsUpdate', (statistics: DashboardStatistics) => {
            console.log('Dashboard statistics updated:', statistics);
            this.dashboardStatistics.next({
                ...statistics,
                lastUpdated: new Date()
            });

            this.emitRealTimeUpdate('dashboard', statistics, 'statistics');
        });

        // Attendance updates
        this.hubConnection.on('AttendanceStatusUpdated', (data: any) => {
            console.log('Attendance status updated:', data);
            const attendanceUpdate: AttendanceUpdate = {
                employeeId: data.employeeId || data.userId,
                employeeName: data.employeeName || `Employee ${data.employeeId}`,
                action: this.mapAttendanceAction(data.status),
                timestamp: new Date(data.timestamp),
                location: data.location,
                branchId: data.branchId
            };

            this.attendanceUpdates.next(attendanceUpdate);
            this.emitRealTimeUpdate('attendance', attendanceUpdate, 'attendance');
        });

        // System notifications
        this.hubConnection.on('NotificationReceived', (notification: any) => {
            console.log('Notification received:', notification);
            const systemNotification: SystemNotification = {
                id: notification.id || this.generateId(),
                type: this.mapNotificationType(notification.type),
                title: notification.title,
                message: notification.message,
                priority: this.mapNotificationPriority(notification.priority),
                timestamp: new Date(notification.deliveredAt || notification.createdAt),
                actionUrl: notification.actionUrl,
                metadata: notification.metadata
            };

            this.systemNotifications.next(systemNotification);
            this.emitRealTimeUpdate('notification', systemNotification, 'notification');

            // Also show in UI notification system
            this.showUINotification(systemNotification);
        });

        // Employee updates
        this.hubConnection.on('EmployeeUpdate', (data: any) => {
            console.log('Employee update received:', data);
            this.emitRealTimeUpdate('employee', data, 'employee');
        });

        // System alerts
        this.hubConnection.on('SystemAlert', (alert: any) => {
            console.log('System alert received:', alert);
            this.emitRealTimeUpdate('system', alert, 'system');

            // Show critical alerts immediately
            if (alert.priority === 'critical' || alert.priority === 'high') {
                this.notificationService.showError(alert.message, alert.title);
            }
        });

        // Birthday wishes
        this.hubConnection.on('BirthdayWishReceived', (wish: any) => {
            console.log('Birthday wish received:', wish);
            this.notificationService.showSuccess(
                `${wish.fromUserName} sent you birthday wishes: "${wish.message}"`,
                'Birthday Wishes! 🎉'
            );
        });

        // Heartbeat handling
        this.hubConnection.on('Heartbeat', (data: any) => {
            this.lastHeartbeat = new Date(data.timestamp);
            this.missedHeartbeats = 0;
        });

        this.hubConnection.on('Pong', (data: any) => {
            this.lastHeartbeat = new Date(data.timestamp);
            this.missedHeartbeats = 0;
        });

        // Connection health
        this.hubConnection.on('ConnectionHealth', (health: any) => {
            console.log('Connection health:', health);
        });

        // Online users updates
        this.hubConnection.on('OnlineUsersUpdate', (users: string[]) => {
            this.onlineUsers.next(users);
        });

        // Error handling
        this.hubConnection.on('Error', (error: string) => {
            console.error('SignalR error:', error);
            this.notificationService.showError(error, 'Connection Error');
        });
    }

    /**
     * Start SignalR connection
     */
    private async startConnection(): Promise<void> {
        if (!this.hubConnection) return;

        try {
            await this.hubConnection.start();
            console.log('SignalR connection established');

            this.updateConnectionState({
                isConnected: true,
                isConnecting: false,
                lastConnected: new Date(),
                reconnectAttempts: 0
            });

            this.startHeartbeat();
            await this.joinGroups();
        } catch (error) {
            console.error('SignalR connection failed:', error);
            this.updateConnectionState({
                isConnecting: false,
                error: error instanceof Error ? error.message : 'Connection failed'
            });

            // Retry connection
            await this.retryConnection();
        }
    }

    /**
     * Join SignalR groups based on user context
     */
    private async joinGroups(): Promise<void> {
        if (!this.isConnected()) return;

        try {
            const currentUser = this.authService.currentUser;
            if (!currentUser) return;

            // Join user-specific group
            await this.hubConnection!.invoke('JoinGroup', `User_${currentUser.id}`);

            // Join branch group
            if (currentUser.branchId) {
                await this.hubConnection!.invoke('JoinGroup', `Branch_${currentUser.branchId}`);
            }

            // Join organization group
            if (currentUser.organizationId) {
                await this.hubConnection!.invoke('JoinGroup', `Organization_${currentUser.organizationId}`);
            }

            // Join role groups
            for (const role of currentUser.roles) {
                await this.hubConnection!.invoke('JoinGroup', `Role_${role}`);
            }

            console.log('Successfully joined SignalR groups');
        } catch (error) {
            console.error('Failed to join SignalR groups:', error);
        }
    }

    /**
     * Rejoin groups after reconnection
     */
    private async rejoinGroups(): Promise<void> {
        await this.joinGroups();
    }

    /**
     * Retry connection with exponential backoff
     */
    private async retryConnection(): Promise<void> {
        const currentState = this.connectionState.value;
        if (currentState.reconnectAttempts >= this.maxReconnectAttempts) {
            console.error('Max reconnection attempts reached');
            return;
        }

        const delay = this.reconnectDelays[Math.min(currentState.reconnectAttempts, this.reconnectDelays.length - 1)];

        this.updateConnectionState({
            reconnectAttempts: currentState.reconnectAttempts + 1
        });

        setTimeout(async () => {
            try {
                await this.startConnection();
            } catch (error) {
                console.error('Retry connection failed:', error);
                await this.retryConnection();
            }
        }, delay);
    }

    /**
     * Start heartbeat monitoring
     */
    private startHeartbeat(): void {
        this.stopHeartbeat();

        this.heartbeatInterval = setInterval(async () => {
            if (this.isConnected()) {
                try {
                    await this.hubConnection!.invoke('Ping');

                    // Check for missed heartbeats
                    if (this.lastHeartbeat) {
                        const timeSinceLastHeartbeat = Date.now() - this.lastHeartbeat.getTime();
                        if (timeSinceLastHeartbeat > 60000) { // 1 minute
                            this.missedHeartbeats++;

                            if (this.missedHeartbeats >= this.maxMissedHeartbeats) {
                                console.warn('Too many missed heartbeats, attempting reconnection');
                                await this.reconnect();
                            }
                        }
                    }
                } catch (error) {
                    console.error('Heartbeat failed:', error);
                    this.missedHeartbeats++;
                }
            }
        }, 30000); // Send heartbeat every 30 seconds
    }

    /**
     * Stop heartbeat monitoring
     */
    private stopHeartbeat(): void {
        if (this.heartbeatInterval) {
            clearInterval(this.heartbeatInterval);
            this.heartbeatInterval = null;
        }
    }

    /**
     * Start connection health monitoring
     */
    private startConnectionHealthMonitoring(): void {
        // Monitor connection state and emit updates
        this.connectionState$.pipe(
            takeUntil(this.destroy$)
        ).subscribe(state => {
            this.emitRealTimeUpdate('connection', state, 'health');
        });
    }

    /**
     * Reconnect to SignalR hub
     */
    public async reconnect(): Promise<void> {
        await this.disconnect();
        await this.connect();
    }

    /**
     * Check if connected to SignalR
     */
    public isConnected(): boolean {
        return this.hubConnection?.state === signalR.HubConnectionState.Connected;
    }

    /**
     * Get current connection state
     */
    public getConnectionState(): ConnectionState {
        return this.connectionState.value;
    }

    /**
     * Send attendance status update
     */
    public async updateAttendanceStatus(status: string): Promise<void> {
        if (!this.isConnected()) return;

        try {
            await this.hubConnection!.invoke('UpdateAttendanceStatus', status);
        } catch (error) {
            console.error('Failed to update attendance status:', error);
            throw error;
        }
    }

    /**
     * Send birthday wish
     */
    public async sendBirthdayWish(toUserId: number, message: string): Promise<void> {
        if (!this.isConnected()) return;

        try {
            await this.hubConnection!.invoke('SendBirthdayWish', toUserId, message);
        } catch (error) {
            console.error('Failed to send birthday wish:', error);
            throw error;
        }
    }

    /**
     * Request dashboard refresh
     */
    public async requestDashboardRefresh(): Promise<void> {
        if (!this.isConnected()) return;

        try {
            await this.hubConnection!.invoke('RequestDashboardRefresh');
        } catch (error) {
            console.error('Failed to request dashboard refresh:', error);
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
            console.error('Failed to get connection stats:', error);
        }
    }

    /**
     * Filter real-time updates by type
     */
    public getUpdatesByType(type: RealTimeUpdate['type']): Observable<RealTimeUpdate> {
        return this.realTimeUpdates$.pipe(
            filter(update => update !== null && update.type === type),
            map(update => update!)
        );
    }

    /**
     * Filter real-time updates by source
     */
    public getUpdatesBySource(source: string): Observable<RealTimeUpdate> {
        return this.realTimeUpdates$.pipe(
            filter(update => update !== null && update.source === source),
            map(update => update!)
        );
    }

    /**
     * Get online status of users
     */
    public isUserOnline(userId: string): Observable<boolean> {
        return this.onlineUsers$.pipe(
            map(users => users.includes(userId))
        );
    }

    // Private helper methods

    private updateConnectionState(updates: Partial<ConnectionState>): void {
        const currentState = this.connectionState.value;
        this.connectionState.next({ ...currentState, ...updates });
    }

    private emitRealTimeUpdate(type: RealTimeUpdate['type'], data: any, source: string): void {
        const update: RealTimeUpdate = {
            type,
            data,
            timestamp: new Date(),
            source,
            branchId: data.branchId,
            organizationId: data.organizationId,
            userId: data.userId
        };

        this.realTimeUpdates.next(update);
    }

    private mapAttendanceAction(status: string): AttendanceUpdate['action'] {
        switch (status?.toLowerCase()) {
            case 'checkin': return 'checkin';
            case 'checkout': return 'checkout';
            case 'breakstart': return 'break_start';
            case 'breakend': return 'break_end';
            default: return 'checkin';
        }
    }

    private mapNotificationType(type: string): SystemNotification['type'] {
        switch (type?.toLowerCase()) {
            case 'error': return 'error';
            case 'warning': return 'warning';
            case 'success': return 'success';
            default: return 'info';
        }
    }

    private mapNotificationPriority(priority: string): SystemNotification['priority'] {
        switch (priority?.toLowerCase()) {
            case 'critical': return 'critical';
            case 'high': return 'high';
            case 'low': return 'low';
            default: return 'normal';
        }
    }

    private showUINotification(notification: SystemNotification): void {
        switch (notification.type) {
            case 'error':
                this.notificationService.showError(notification.message, notification.title);
                break;
            case 'warning':
                this.notificationService.showWarning(notification.message, notification.title);
                break;
            case 'success':
                this.notificationService.showSuccess(notification.message, notification.title);
                break;
            default:
                this.notificationService.showInfo(notification.message, notification.title);
                break;
        }
    }

    private generateId(): string {
        return Math.random().toString(36).substr(2, 9);
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.disconnect();
    }
}