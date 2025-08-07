import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, Subject, combineLatest, timer } from 'rxjs';
import { map, filter, takeUntil, switchMap, startWith } from 'rxjs/operators';
import { RealTimeService, DashboardStatistics, RealTimeUpdate } from '../../core/services/real-time.service';
import { AuthService } from '../../core/auth/auth.service';

export interface DashboardMetrics {
  attendance: AttendanceMetrics;
  employees: EmployeeMetrics;
  productivity: ProductivityMetrics;
  alerts: AlertMetrics;
  lastUpdated: Date;
}

export interface AttendanceMetrics {
  totalEmployees: number;
  presentToday: number;
  absentToday: number;
  lateArrivals: number;
  earlyDepartures: number;
  onBreak: number;
  overtime: number;
  attendanceRate: number;
}

export interface EmployeeMetrics {
  totalActive: number;
  newHires: number;
  birthdays: number;
  anniversaries: number;
  onLeave: number;
  pendingApprovals: number;
}

export interface ProductivityMetrics {
  averageProductivity: number;
  topPerformers: number;
  lowPerformers: number;
  tasksCompleted: number;
  projectsOnTrack: number;
  overdueTasks: number;
}

export interface AlertMetrics {
  critical: number;
  warnings: number;
  info: number;
  unread: number;
}

export interface LiveAttendanceUpdate {
  employeeId: number;
  employeeName: string;
  profilePhoto?: string;
  action: 'checkin' | 'checkout' | 'break_start' | 'break_end';
  timestamp: Date;
  location?: string;
  department?: string;
}

export interface DashboardAlert {
  id: string;
  type: 'attendance' | 'employee' | 'system' | 'productivity';
  severity: 'info' | 'warning' | 'error' | 'critical';
  title: string;
  message: string;
  timestamp: Date;
  actionRequired: boolean;
  actionUrl?: string;
  metadata?: any;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardRealTimeService implements OnDestroy {
  private destroy$ = new Subject<void>();
  
  // Dashboard data streams
  private dashboardMetrics = new BehaviorSubject<DashboardMetrics | null>(null);
  private liveAttendanceUpdates = new BehaviorSubject<LiveAttendanceUpdate[]>([]);
  private dashboardAlerts = new BehaviorSubject<DashboardAlert[]>([]);
  private onlineEmployees = new BehaviorSubject<number>(0);
  
  // Public observables
  public dashboardMetrics$ = this.dashboardMetrics.asObservable();
  public liveAttendanceUpdates$ = this.liveAttendanceUpdates.asObservable();
  public dashboardAlerts$ = this.dashboardAlerts.asObservable();
  public onlineEmployees$ = this.onlineEmployees.asObservable();
  
  // Configuration
  private readonly maxLiveUpdates = 10; // Keep last 10 live updates
  private readonly maxAlerts = 20; // Keep last 20 alerts
  private readonly refreshInterval = 30000; // 30 seconds

  constructor(
    private realTimeService: RealTimeService,
    private authService: AuthService
  ) {
    this.initializeDashboardUpdates();
    this.startPeriodicRefresh();
  }

  /**
   * Initialize dashboard real-time updates
   */
  private initializeDashboardUpdates(): void {
    // Listen to dashboard statistics updates
    this.realTimeService.dashboardStatistics$.pipe(
      filter(stats => stats !== null),
      takeUntil(this.destroy$)
    ).subscribe(stats => {
      this.updateDashboardMetrics(stats!);
    });

    // Listen to attendance updates
    this.realTimeService.attendanceUpdates$.pipe(
      filter(update => update !== null),
      takeUntil(this.destroy$)
    ).subscribe(update => {
      this.handleAttendanceUpdate(update!);
    });

    // Listen to system notifications for alerts
    this.realTimeService.systemNotifications$.pipe(
      filter(notification => notification !== null),
      takeUntil(this.destroy$)
    ).subscribe(notification => {
      this.handleSystemNotification(notification!);
    });

    // Listen to real-time updates for various dashboard events
    this.realTimeService.getUpdatesByType('dashboard').pipe(
      takeUntil(this.destroy$)
    ).subscribe(update => {
      this.handleDashboardUpdate(update);
    });

    // Listen to employee updates
    this.realTimeService.getUpdatesByType('employee').pipe(
      takeUntil(this.destroy$)
    ).subscribe(update => {
      this.handleEmployeeUpdate(update);
    });

    // Monitor online users count
    this.realTimeService.onlineUsers$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(users => {
      this.onlineEmployees.next(users.length);
    });

    // Monitor connection state for dashboard health
    this.realTimeService.connectionState$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(state => {
      this.handleConnectionStateChange(state);
    });
  }

  /**
   * Start periodic refresh of dashboard data
   */
  private startPeriodicRefresh(): void {
    // Refresh dashboard data periodically
    timer(0, this.refreshInterval).pipe(
      switchMap(() => this.realTimeService.connectionState$),
      filter(state => state.isConnected),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.requestDashboardRefresh();
    });
  }

  /**
   * Update dashboard metrics from statistics
   */
  private updateDashboardMetrics(stats: DashboardStatistics): void {
    const currentMetrics = this.dashboardMetrics.value;
    
    const updatedMetrics: DashboardMetrics = {
      attendance: {
        totalEmployees: stats.totalEmployees,
        presentToday: stats.presentToday,
        absentToday: stats.absentToday,
        lateArrivals: stats.lateArrivals,
        earlyDepartures: stats.earlyDepartures,
        onBreak: stats.onBreak,
        overtime: stats.overtime,
        attendanceRate: stats.totalEmployees > 0 ? (stats.presentToday / stats.totalEmployees) * 100 : 0
      },
      employees: currentMetrics?.employees || {
        totalActive: stats.totalEmployees,
        newHires: 0,
        birthdays: 0,
        anniversaries: 0,
        onLeave: 0,
        pendingApprovals: 0
      },
      productivity: {
        averageProductivity: stats.productivity,
        topPerformers: 0,
        lowPerformers: 0,
        tasksCompleted: 0,
        projectsOnTrack: 0,
        overdueTasks: 0
      },
      alerts: currentMetrics?.alerts || {
        critical: 0,
        warnings: 0,
        info: 0,
        unread: 0
      },
      lastUpdated: stats.lastUpdated
    };

    this.dashboardMetrics.next(updatedMetrics);
  }

  /**
   * Handle attendance updates for live feed
   */
  private handleAttendanceUpdate(update: any): void {
    const liveUpdate: LiveAttendanceUpdate = {
      employeeId: update.employeeId,
      employeeName: update.employeeName,
      profilePhoto: update.profilePhoto,
      action: update.action,
      timestamp: update.timestamp,
      location: update.location,
      department: update.department
    };

    const currentUpdates = this.liveAttendanceUpdates.value;
    const updatedList = [liveUpdate, ...currentUpdates].slice(0, this.maxLiveUpdates);
    this.liveAttendanceUpdates.next(updatedList);

    // Update attendance metrics
    this.updateAttendanceMetricsFromUpdate(update);
  }

  /**
   * Handle system notifications as dashboard alerts
   */
  private handleSystemNotification(notification: any): void {
    const alert: DashboardAlert = {
      id: notification.id,
      type: this.mapNotificationTypeToAlertType(notification.type),
      severity: this.mapPriorityToSeverity(notification.priority),
      title: notification.title,
      message: notification.message,
      timestamp: notification.timestamp,
      actionRequired: notification.actionUrl !== undefined,
      actionUrl: notification.actionUrl,
      metadata: notification.metadata
    };

    const currentAlerts = this.dashboardAlerts.value;
    const updatedAlerts = [alert, ...currentAlerts].slice(0, this.maxAlerts);
    this.dashboardAlerts.next(updatedAlerts);

    // Update alert metrics
    this.updateAlertMetrics();
  }

  /**
   * Handle general dashboard updates
   */
  private handleDashboardUpdate(update: RealTimeUpdate): void {
    console.log('Dashboard update received:', update);
    
    // Handle specific dashboard update types
    switch (update.source) {
      case 'statistics':
        this.updateDashboardMetrics(update.data);
        break;
      case 'metrics':
        this.handleMetricsUpdate(update.data);
        break;
      case 'alerts':
        this.handleAlertsUpdate(update.data);
        break;
    }
  }

  /**
   * Handle employee updates
   */
  private handleEmployeeUpdate(update: RealTimeUpdate): void {
    console.log('Employee update received:', update);
    
    // Update employee metrics based on the update
    const currentMetrics = this.dashboardMetrics.value;
    if (currentMetrics) {
      // Update employee-related metrics
      this.updateEmployeeMetrics(update.data);
    }
  }

  /**
   * Handle connection state changes
   */
  private handleConnectionStateChange(state: any): void {
    if (!state.isConnected && state.error) {
      // Add connection error as alert
      const alert: DashboardAlert = {
        id: `connection-${Date.now()}`,
        type: 'system',
        severity: 'error',
        title: 'Connection Lost',
        message: 'Real-time updates are temporarily unavailable. Attempting to reconnect...',
        timestamp: new Date(),
        actionRequired: false
      };

      const currentAlerts = this.dashboardAlerts.value;
      this.dashboardAlerts.next([alert, ...currentAlerts].slice(0, this.maxAlerts));
    } else if (state.isConnected && state.lastConnected) {
      // Remove connection error alerts
      const currentAlerts = this.dashboardAlerts.value;
      const filteredAlerts = currentAlerts.filter(alert => !alert.id.startsWith('connection-'));
      this.dashboardAlerts.next(filteredAlerts);
    }
  }

  /**
   * Update attendance metrics from live update
   */
  private updateAttendanceMetricsFromUpdate(update: any): void {
    const currentMetrics = this.dashboardMetrics.value;
    if (!currentMetrics) return;

    const updatedAttendance = { ...currentMetrics.attendance };

    switch (update.action) {
      case 'checkin':
        updatedAttendance.presentToday++;
        updatedAttendance.absentToday = Math.max(0, updatedAttendance.absentToday - 1);
        break;
      case 'checkout':
        // Don't decrease present count as they're still counted for the day
        break;
      case 'break_start':
        updatedAttendance.onBreak++;
        break;
      case 'break_end':
        updatedAttendance.onBreak = Math.max(0, updatedAttendance.onBreak - 1);
        break;
    }

    // Recalculate attendance rate
    updatedAttendance.attendanceRate = updatedAttendance.totalEmployees > 0 
      ? (updatedAttendance.presentToday / updatedAttendance.totalEmployees) * 100 
      : 0;

    const updatedMetrics = {
      ...currentMetrics,
      attendance: updatedAttendance,
      lastUpdated: new Date()
    };

    this.dashboardMetrics.next(updatedMetrics);
  }

  /**
   * Update employee metrics
   */
  private updateEmployeeMetrics(data: any): void {
    const currentMetrics = this.dashboardMetrics.value;
    if (!currentMetrics) return;

    // Update employee metrics based on the type of update
    const updatedEmployees = { ...currentMetrics.employees };

    if (data.type === 'new_hire') {
      updatedEmployees.newHires++;
    } else if (data.type === 'birthday') {
      updatedEmployees.birthdays++;
    } else if (data.type === 'anniversary') {
      updatedEmployees.anniversaries++;
    } else if (data.type === 'leave_request') {
      updatedEmployees.pendingApprovals++;
    }

    const updatedMetrics = {
      ...currentMetrics,
      employees: updatedEmployees,
      lastUpdated: new Date()
    };

    this.dashboardMetrics.next(updatedMetrics);
  }

  /**
   * Handle metrics updates
   */
  private handleMetricsUpdate(data: any): void {
    const currentMetrics = this.dashboardMetrics.value;
    if (!currentMetrics) return;

    // Update specific metrics based on data
    const updatedMetrics = {
      ...currentMetrics,
      ...data,
      lastUpdated: new Date()
    };

    this.dashboardMetrics.next(updatedMetrics);
  }

  /**
   * Handle alerts updates
   */
  private handleAlertsUpdate(data: any): void {
    if (Array.isArray(data)) {
      this.dashboardAlerts.next(data);
    } else {
      const currentAlerts = this.dashboardAlerts.value;
      this.dashboardAlerts.next([data, ...currentAlerts].slice(0, this.maxAlerts));
    }
    
    this.updateAlertMetrics();
  }

  /**
   * Update alert metrics
   */
  private updateAlertMetrics(): void {
    const alerts = this.dashboardAlerts.value;
    const alertMetrics: AlertMetrics = {
      critical: alerts.filter(a => a.severity === 'critical').length,
      warnings: alerts.filter(a => a.severity === 'warning').length,
      info: alerts.filter(a => a.severity === 'info').length,
      unread: alerts.length // Assuming all alerts are unread initially
    };

    const currentMetrics = this.dashboardMetrics.value;
    if (currentMetrics) {
      const updatedMetrics = {
        ...currentMetrics,
        alerts: alertMetrics,
        lastUpdated: new Date()
      };

      this.dashboardMetrics.next(updatedMetrics);
    }
  }

  /**
   * Request dashboard refresh
   */
  public async requestDashboardRefresh(): Promise<void> {
    try {
      await this.realTimeService.requestDashboardRefresh();
    } catch (error) {
      console.error('Failed to request dashboard refresh:', error);
    }
  }

  /**
   * Clear live attendance updates
   */
  public clearLiveUpdates(): void {
    this.liveAttendanceUpdates.next([]);
  }

  /**
   * Clear dashboard alerts
   */
  public clearAlerts(): void {
    this.dashboardAlerts.next([]);
    this.updateAlertMetrics();
  }

  /**
   * Dismiss specific alert
   */
  public dismissAlert(alertId: string): void {
    const currentAlerts = this.dashboardAlerts.value;
    const updatedAlerts = currentAlerts.filter(alert => alert.id !== alertId);
    this.dashboardAlerts.next(updatedAlerts);
    this.updateAlertMetrics();
  }

  /**
   * Get alerts by severity
   */
  public getAlertsBySeverity(severity: DashboardAlert['severity']): Observable<DashboardAlert[]> {
    return this.dashboardAlerts$.pipe(
      map(alerts => alerts.filter(alert => alert.severity === severity))
    );
  }

  /**
   * Get alerts by type
   */
  public getAlertsByType(type: DashboardAlert['type']): Observable<DashboardAlert[]> {
    return this.dashboardAlerts$.pipe(
      map(alerts => alerts.filter(alert => alert.type === type))
    );
  }

  /**
   * Get recent attendance updates
   */
  public getRecentAttendanceUpdates(limit: number = 5): Observable<LiveAttendanceUpdate[]> {
    return this.liveAttendanceUpdates$.pipe(
      map(updates => updates.slice(0, limit))
    );
  }

  /**
   * Check if dashboard is receiving real-time updates
   */
  public isDashboardLive(): Observable<boolean> {
    return this.realTimeService.connectionState$.pipe(
      map(state => state.isConnected)
    );
  }

  /**
   * Get dashboard health status
   */
  public getDashboardHealth(): Observable<{isHealthy: boolean, lastUpdate: Date | null}> {
    return this.dashboardMetrics$.pipe(
      map(metrics => ({
        isHealthy: metrics !== null && (Date.now() - metrics.lastUpdated.getTime()) < 60000, // Healthy if updated within 1 minute
        lastUpdate: metrics?.lastUpdated || null
      }))
    );
  }

  // Private helper methods

  private mapNotificationTypeToAlertType(type: string): DashboardAlert['type'] {
    switch (type.toLowerCase()) {
      case 'attendance': return 'attendance';
      case 'employee': return 'employee';
      case 'productivity': return 'productivity';
      default: return 'system';
    }
  }

  private mapPriorityToSeverity(priority: string): DashboardAlert['severity'] {
    switch (priority.toLowerCase()) {
      case 'critical': return 'critical';
      case 'high': return 'error';
      case 'low': return 'info';
      default: return 'warning';
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}