import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, interval, combineLatest } from 'rxjs';
import { map, catchError, switchMap, startWith, filter } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/auth/auth.service';
import { RealTimeAttendanceService } from '../../services/real-time-attendance.service';
import { DashboardSignalRService } from './dashboard-signalr.service';

export interface DashboardStats {
  // Employee stats
  employeeStats?: {
    todayHours: string;
    activeTasks: number;
    leaveBalance: number;
    productivity: number;
    currentStatus: string;
    checkInTime?: string;
    checkOutTime?: string;
  };

  // Manager stats
  managerStats?: {
    teamSize: number;
    presentToday: number;
    activeProjects: number;
    pendingApprovals: number;
    teamProductivity: number;
    overdueTasksCount: number;
  };

  // HR stats
  hrStats?: {
    totalEmployees: number;
    presentToday: number;
    pendingLeaves: number;
    payrollStatus: string;
    newHiresThisMonth: number;
    upcomingBirthdays: number;
  };

  // Admin stats
  adminStats?: {
    totalBranches: number;
    totalEmployees: number;
    systemHealth: string;
    activeUsers: number;
    systemUptime: string;
    criticalAlerts: number;
  };

  // Super Admin stats
  superAdminStats?: {
    totalOrganizations: number;
    totalBranches: number;
    totalEmployees: number;
    systemHealth: string;
    activeUsers: number;
    systemUptime: string;
    criticalAlerts: number;
    databaseHealth: string;
    serverLoad: number;
  };
}

export interface DashboardActivity {
  id: string;
  type: 'success' | 'primary' | 'warning' | 'info' | 'danger';
  icon: string;
  message: string;
  timestamp: string;
  userId?: string;
  employeeId?: string;
  branchId?: string;
}

export interface QuickAction {
  id: string;
  title: string;
  description: string;
  icon: string;
  route: string;
  color: string;
  roles: string[];
  isEnabled: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly apiUrl = `${environment.apiUrl}`;
  
  // Real-time data subjects
  private dashboardStatsSubject = new BehaviorSubject<DashboardStats>({});
  private recentActivitiesSubject = new BehaviorSubject<DashboardActivity[]>([]);
  private quickActionsSubject = new BehaviorSubject<QuickAction[]>([]);
  
  // Public observables
  public dashboardStats$ = this.dashboardStatsSubject.asObservable();
  public recentActivities$ = this.recentActivitiesSubject.asObservable();
  public quickActions$ = this.quickActionsSubject.asObservable();

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private realTimeService: RealTimeAttendanceService,
    private dashboardSignalR: DashboardSignalRService
  ) {
    this.initializeRealTimeUpdates();
    this.loadQuickActions();
    this.setupSignalRIntegration();
  }

  /**
   * Initialize real-time dashboard updates
   */
  private initializeRealTimeUpdates(): void {
    // Update dashboard every 30 seconds
    interval(30000).pipe(
      startWith(0),
      switchMap(() => this.loadDashboardData())
    ).subscribe();

    // Listen to real-time attendance updates
    this.realTimeService.personalStatusUpdates$.subscribe(status => {
      if (status) {
        this.updateEmployeeAttendanceStats(status);
      }
    });

    this.realTimeService.teamOverviewUpdates$.subscribe(overview => {
      if (overview) {
        this.updateTeamStats(overview);
      }
    });
  }

  /**
   * Setup SignalR integration for real-time dashboard updates
   */
  private setupSignalRIntegration(): void {
    // Connect to SignalR
    this.dashboardSignalR.connect().catch(error => {
      console.log('Dashboard SignalR connection failed, using polling fallback:', error);
    });

    // Listen for dashboard updates
    this.dashboardSignalR.dashboardUpdates$
      .pipe(filter(update => update !== null))
      .subscribe(update => {
        if (update) {
          this.handleSignalRUpdate(update);
        }
      });

    // Listen for attendance updates
    this.dashboardSignalR.attendanceUpdates$
      .pipe(filter(update => update !== null))
      .subscribe(update => {
        if (update) {
          this.updateEmployeeAttendanceStats(update);
        }
      });

    // Listen for system alerts
    this.dashboardSignalR.systemAlerts$
      .pipe(filter(alert => alert !== null))
      .subscribe(alert => {
        if (alert) {
          this.handleSystemAlert(alert);
        }
      });
  }

  /**
   * Handle SignalR dashboard updates
   */
  private handleSignalRUpdate(update: any): void {
    switch (update.type) {
      case 'attendance':
        this.updateEmployeeAttendanceStats(update.data);
        break;
      case 'employee':
        this.handleEmployeeUpdate(update.data);
        break;
      case 'system':
        this.handleSystemUpdate(update.data);
        break;
      case 'notification':
        this.handleNotificationUpdate(update.data);
        break;
    }
  }

  /**
   * Handle employee updates
   */
  private handleEmployeeUpdate(_data: any): void {
    // Refresh relevant dashboard stats when employee data changes
    this.loadDashboardData().subscribe();
  }

  /**
   * Handle system updates
   */
  private handleSystemUpdate(data: any): void {
    const currentStats = this.dashboardStatsSubject.value;
    
    if (currentStats.adminStats) {
      currentStats.adminStats.systemHealth = data.systemHealth || currentStats.adminStats.systemHealth;
      currentStats.adminStats.criticalAlerts = data.criticalAlerts || currentStats.adminStats.criticalAlerts;
    }

    if (currentStats.superAdminStats) {
      currentStats.superAdminStats.systemHealth = data.systemHealth || currentStats.superAdminStats.systemHealth;
      currentStats.superAdminStats.criticalAlerts = data.criticalAlerts || currentStats.superAdminStats.criticalAlerts;
      currentStats.superAdminStats.databaseHealth = data.databaseHealth || currentStats.superAdminStats.databaseHealth;
      currentStats.superAdminStats.serverLoad = data.serverLoad || currentStats.superAdminStats.serverLoad;
    }

    this.dashboardStatsSubject.next(currentStats);
  }

  /**
   * Handle notification updates
   */
  private handleNotificationUpdate(data: any): void {
    // Add new activity to recent activities
    const newActivity: DashboardActivity = {
      id: data.id || Date.now().toString(),
      type: this.mapActivityType(data.type),
      icon: this.mapActivityIcon(data.type),
      message: data.message,
      timestamp: this.formatTimestamp(data.timestamp || new Date().toISOString()),
      userId: data.userId,
      employeeId: data.employeeId,
      branchId: data.branchId
    };

    const currentActivities = this.recentActivitiesSubject.value;
    const updatedActivities = [newActivity, ...currentActivities.slice(0, 9)]; // Keep only 10 most recent
    this.recentActivitiesSubject.next(updatedActivities);
  }

  /**
   * Handle system alerts
   */
  private handleSystemAlert(alert: any): void {
    console.warn('System alert received:', alert);
    
    // Update critical alerts count
    const currentStats = this.dashboardStatsSubject.value;
    
    if (currentStats.adminStats) {
      currentStats.adminStats.criticalAlerts = (currentStats.adminStats.criticalAlerts || 0) + 1;
    }

    if (currentStats.superAdminStats) {
      currentStats.superAdminStats.criticalAlerts = (currentStats.superAdminStats.criticalAlerts || 0) + 1;
    }

    this.dashboardStatsSubject.next(currentStats);
  }

  /**
   * Load dashboard data based on user role
   */
  public loadDashboardData(): Observable<DashboardStats> {
    const currentUser = this.authService.currentUser;
    if (!currentUser) {
      return new Observable<DashboardStats>(observer => {
        observer.next({});
        observer.complete();
      });
    }

    const primaryRole = this.getPrimaryRole(currentUser.roles);
    
    return combineLatest([
      this.loadEmployeeStats(),
      this.loadManagerStats(),
      this.loadHRStats(),
      this.loadAdminStats(),
      this.loadSuperAdminStats()
    ]).pipe(
      map(([employeeStats, managerStats, hrStats, adminStats, superAdminStats]) => {
        const stats: DashboardStats = {};
        
        // Always include employee stats for personal data
        stats.employeeStats = employeeStats;
        
        // Add role-specific stats
        if (primaryRole === 'Manager' || currentUser.roles.includes('Manager')) {
          stats.managerStats = managerStats;
        }
        
        if (primaryRole === 'HR' || currentUser.roles.includes('HR')) {
          stats.hrStats = hrStats;
        }
        
        if (primaryRole === 'Admin' || currentUser.roles.includes('Admin')) {
          stats.adminStats = adminStats;
        }
        
        if (primaryRole === 'SuperAdmin' || currentUser.roles.includes('SuperAdmin')) {
          stats.superAdminStats = superAdminStats;
        }
        
        this.dashboardStatsSubject.next(stats);
        return stats;
      }),
      catchError(error => {
        console.error('Error loading dashboard data:', error);
        const mockStats = this.getMockDashboardStats();
        return new Observable<DashboardStats>(observer => {
          observer.next(mockStats);
          observer.complete();
        });
      })
    );
  }

  /**
   * Load employee-specific statistics
   */
  private loadEmployeeStats(): Observable<any> {
    const currentUser = this.authService.currentUser;
    if (!currentUser?.employeeId) {
      return new Observable<any>(observer => {
        observer.next(this.getMockEmployeeStats());
        observer.complete();
      });
    }

    return this.http.get(`${this.apiUrl}/dashboard/employee/${currentUser.employeeId}/stats`).pipe(
      map((response: any) => response.data || response),
      catchError(() => new Observable<any>(observer => {
        observer.next(this.getMockEmployeeStats());
        observer.complete();
      }))
    );
  }

  /**
   * Load manager-specific statistics
   */
  private loadManagerStats(): Observable<any> {
    const currentUser = this.authService.currentUser;
    if (!currentUser?.employeeId) {
      return new Observable<any>(observer => {
        observer.next(this.getMockManagerStats());
        observer.complete();
      });
    }

    return this.http.get(`${this.apiUrl}/dashboard/manager/${currentUser.employeeId}/stats`).pipe(
      map((response: any) => response.data || response),
      catchError(() => new Observable<any>(observer => {
        observer.next(this.getMockManagerStats());
        observer.complete();
      }))
    );
  }

  /**
   * Load HR-specific statistics
   */
  private loadHRStats(): Observable<any> {
    const currentUser = this.authService.currentUser;
    const branchId = currentUser?.branchId;

    if (!branchId) {
      return new Observable<any>(observer => {
        observer.next(this.getMockHRStats());
        observer.complete();
      });
    }

    return this.http.get(`${this.apiUrl}/dashboard/hr/branch/${branchId}/stats`).pipe(
      map((response: any) => response.data || response),
      catchError(() => new Observable<any>(observer => {
        observer.next(this.getMockHRStats());
        observer.complete();
      }))
    );
  }

  /**
   * Load admin-specific statistics
   */
  private loadAdminStats(): Observable<any> {
    const currentUser = this.authService.currentUser;
    const organizationId = currentUser?.organizationId;

    if (!organizationId) {
      return new Observable<any>(observer => {
        observer.next(this.getMockAdminStats());
        observer.complete();
      });
    }

    return this.http.get(`${this.apiUrl}/dashboard/admin/organization/${organizationId}/stats`).pipe(
      map((response: any) => response.data || response),
      catchError(() => new Observable<any>(observer => {
        observer.next(this.getMockAdminStats());
        observer.complete();
      }))
    );
  }

  /**
   * Load super admin statistics
   */
  private loadSuperAdminStats(): Observable<any> {
    return this.http.get(`${this.apiUrl}/dashboard/superadmin/stats`).pipe(
      map((response: any) => response.data || response),
      catchError(() => new Observable<any>(observer => {
        observer.next(this.getMockSuperAdminStats());
        observer.complete();
      }))
    );
  }

  /**
   * Load recent activities
   */
  public loadRecentActivities(): Observable<DashboardActivity[]> {
    return this.http.get<any>(`${this.apiUrl}/dashboard/activities/recent`).pipe(
      map((response: any) => {
        const activities = response.data || response;
        if (Array.isArray(activities)) {
          return activities.map((activity: any) => ({
            id: activity.Id || activity.id,
            type: this.mapActivityType(activity.Type || activity.type),
            icon: this.mapActivityIcon(activity.Type || activity.type),
            message: activity.Message || activity.message,
            timestamp: this.formatTimestamp(activity.CreatedAt || activity.createdAt),
            userId: activity.UserId || activity.userId,
            employeeId: activity.EmployeeId || activity.employeeId,
            branchId: activity.BranchId || activity.branchId
          }));
        }
        return [];
      }),
      catchError(() => new Observable<DashboardActivity[]>(observer => {
        observer.next(this.getMockRecentActivities());
        observer.complete();
      }))
    );
  }

  /**
   * Load quick actions based on user role
   */
  private loadQuickActions(): void {
    const currentUser = this.authService.currentUser;
    if (!currentUser) return;

    const actions = this.getQuickActionsByRole(currentUser.roles);
    this.quickActionsSubject.next(actions);
  }

  /**
   * Update employee attendance stats from real-time data
   */
  private updateEmployeeAttendanceStats(status: any): void {
    const currentStats = this.dashboardStatsSubject.value;
    if (currentStats.employeeStats) {
      currentStats.employeeStats.currentStatus = status.currentStatus || status.status;
      currentStats.employeeStats.checkInTime = status.checkInTime;
      currentStats.employeeStats.checkOutTime = status.checkOutTime;
      currentStats.employeeStats.todayHours = this.calculateWorkingHours(status);
      
      this.dashboardStatsSubject.next(currentStats);
    }
  }

  /**
   * Update team stats from real-time data
   */
  private updateTeamStats(overview: any): void {
    const currentStats = this.dashboardStatsSubject.value;
    if (currentStats.managerStats) {
      currentStats.managerStats.presentToday = overview.summary?.presentCount || overview.presentCount;
      currentStats.managerStats.teamSize = overview.summary?.totalEmployees || overview.totalCount;
      
      this.dashboardStatsSubject.next(currentStats);
    }
  }

  /**
   * Get primary role for dashboard display
   */
  private getPrimaryRole(roles: string[]): string {
    const rolePriority = ['SuperAdmin', 'Admin', 'HR', 'Manager', 'Employee'];
    
    for (const role of rolePriority) {
      if (roles.includes(role)) {
        return role;
      }
    }
    
    return 'Employee';
  }

  /**
   * Calculate working hours from attendance data
   */
  private calculateWorkingHours(attendance: any): string {
    if (!attendance?.checkInTime) return '0.0';
    
    const checkIn = new Date(attendance.checkInTime);
    const checkOut = attendance.checkOutTime ? new Date(attendance.checkOutTime) : new Date();
    
    const diffMs = checkOut.getTime() - checkIn.getTime();
    const hours = diffMs / (1000 * 60 * 60);
    
    return Math.max(0, hours).toFixed(1);
  }



  /**
   * Map activity type to UI type
   */
  private mapActivityType(type: string): 'success' | 'primary' | 'warning' | 'info' | 'danger' {
    const typeMap: { [key: string]: 'success' | 'primary' | 'warning' | 'info' | 'danger' } = {
      'employee_joined': 'success',
      'project_created': 'primary',
      'leave_requested': 'warning',
      'attendance_checkin': 'info',
      'system_alert': 'danger'
    };
    
    return typeMap[type] || 'info';
  }

  /**
   * Map activity type to icon
   */
  private mapActivityIcon(type: string): string {
    const iconMap: { [key: string]: string } = {
      'employee_joined': 'fas fa-user-plus',
      'project_created': 'fas fa-project-diagram',
      'leave_requested': 'fas fa-calendar-alt',
      'attendance_checkin': 'fas fa-clock',
      'system_alert': 'fas fa-exclamation-triangle'
    };
    
    return iconMap[type] || 'fas fa-info-circle';
  }

  /**
   * Format timestamp for display
   */
  private formatTimestamp(timestamp: string): string {
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffHours = diffMs / (1000 * 60 * 60);
    
    if (diffHours < 1) {
      const diffMinutes = Math.floor(diffMs / (1000 * 60));
      return `${diffMinutes} minutes ago`;
    } else if (diffHours < 24) {
      return `${Math.floor(diffHours)} hours ago`;
    } else {
      const diffDays = Math.floor(diffHours / 24);
      return `${diffDays} days ago`;
    }
  }

  /**
   * Get quick actions based on user roles
   */
  private getQuickActionsByRole(roles: string[]): QuickAction[] {
    const allActions: QuickAction[] = [
      {
        id: 'check-attendance',
        title: 'Check In/Out',
        description: 'Manage your attendance',
        icon: 'fas fa-clock',
        route: '/attendance',
        color: 'primary',
        roles: ['Employee', 'Manager', 'HR', 'Admin', 'SuperAdmin'],
        isEnabled: true
      },
      {
        id: 'view-employees',
        title: 'View Employees',
        description: 'Manage employee records',
        icon: 'fas fa-users',
        route: '/employees',
        color: 'success',
        roles: ['Manager', 'HR', 'Admin', 'SuperAdmin'],
        isEnabled: true
      },
      {
        id: 'create-employee',
        title: 'Add Employee',
        description: 'Add new employee',
        icon: 'fas fa-user-plus',
        route: '/employees/add',
        color: 'info',
        roles: ['HR', 'Admin', 'SuperAdmin'],
        isEnabled: true
      },
      {
        id: 'manage-projects',
        title: 'Projects',
        description: 'Manage projects and tasks',
        icon: 'fas fa-project-diagram',
        route: '/projects',
        color: 'warning',
        roles: ['Manager', 'Admin', 'SuperAdmin'],
        isEnabled: true
      },
      {
        id: 'view-reports',
        title: 'Reports',
        description: 'View analytics and reports',
        icon: 'fas fa-chart-bar',
        route: '/reports',
        color: 'secondary',
        roles: ['Manager', 'HR', 'Admin', 'SuperAdmin'],
        isEnabled: true
      },
      {
        id: 'system-settings',
        title: 'Settings',
        description: 'System configuration',
        icon: 'fas fa-cog',
        route: '/settings',
        color: 'dark',
        roles: ['Admin', 'SuperAdmin'],
        isEnabled: true
      }
    ];

    return allActions.filter(action => 
      action.roles.some(role => roles.includes(role))
    );
  }

  // Mock data methods for fallback
  private getMockDashboardStats(): DashboardStats {
    return {
      employeeStats: this.getMockEmployeeStats(),
      managerStats: this.getMockManagerStats(),
      hrStats: this.getMockHRStats(),
      adminStats: this.getMockAdminStats(),
      superAdminStats: this.getMockSuperAdminStats()
    };
  }

  private getMockEmployeeStats() {
    return {
      todayHours: '7.5',
      activeTasks: 5,
      leaveBalance: 12,
      productivity: 85,
      currentStatus: 'Checked In',
      checkInTime: '09:00 AM'
    };
  }

  private getMockManagerStats() {
    return {
      teamSize: 8,
      presentToday: 7,
      activeProjects: 3,
      pendingApprovals: 4,
      teamProductivity: 82,
      overdueTasksCount: 2
    };
  }

  private getMockHRStats() {
    return {
      totalEmployees: 150,
      presentToday: 142,
      pendingLeaves: 8,
      payrollStatus: 'In Progress',
      newHiresThisMonth: 5,
      upcomingBirthdays: 3
    };
  }

  private getMockAdminStats() {
    return {
      totalBranches: 5,
      totalEmployees: 150,
      systemHealth: 'Excellent',
      activeUsers: 98,
      systemUptime: '99.9%',
      criticalAlerts: 0
    };
  }

  private getMockSuperAdminStats() {
    return {
      totalOrganizations: 1,
      totalBranches: 5,
      totalEmployees: 150,
      systemHealth: 'Excellent',
      activeUsers: 98,
      systemUptime: '99.9%',
      criticalAlerts: 0,
      databaseHealth: 'Good',
      serverLoad: 25
    };
  }

  private getMockRecentActivities(): DashboardActivity[] {
    return [
      {
        id: '1',
        type: 'success',
        icon: 'fas fa-user-plus',
        message: '<strong>John Doe</strong> joined the Development team',
        timestamp: '2 hours ago'
      },
      {
        id: '2',
        type: 'primary',
        icon: 'fas fa-project-diagram',
        message: 'New project <strong>"Mobile App Redesign"</strong> created',
        timestamp: '4 hours ago'
      },
      {
        id: '3',
        type: 'warning',
        icon: 'fas fa-calendar-alt',
        message: '<strong>Jane Smith</strong> requested leave for next week',
        timestamp: '6 hours ago'
      },
      {
        id: '4',
        type: 'info',
        icon: 'fas fa-clock',
        message: '<strong>Mike Johnson</strong> checked in at 9:15 AM',
        timestamp: '8 hours ago'
      }
    ];
  }
}