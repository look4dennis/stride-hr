import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { AuthService, User } from '../../core/auth/auth.service';
import { DashboardService, DashboardStats, DashboardActivity } from '../../shared/services/dashboard.service';
import { RealTimeAttendanceService } from '../../services/real-time-attendance.service';
import { WeatherTimeWidgetComponent } from '../../shared/components/weather-time-widget/weather-time-widget.component';
import { BirthdayWidgetComponent } from '../../shared/components/birthday-widget/birthday-widget.component';
import { QuickActionsComponent } from '../../shared/components/quick-actions/quick-actions.component';
import { AttendanceWidgetComponent } from '../../shared/components/attendance-widget/attendance-widget.component';
import { ProgressiveDashboardComponent } from '../../shared/components/progressive-dashboard/progressive-dashboard.component';

@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule,
    WeatherTimeWidgetComponent,
    BirthdayWidgetComponent,
    QuickActionsComponent,
    AttendanceWidgetComponent,
    ProgressiveDashboardComponent
  ],
  template: `
    <app-progressive-dashboard 
      [showProgress]="true"
      [showTaskDetails]="false"
      [showSkeletons]="true"
      [userRole]="getPrimaryRole()">
      
      <!-- Dashboard Content -->
      <div class="dashboard-container">
        <!-- Error State -->
        <div *ngIf="error" class="alert alert-danger" role="alert">
          <i class="fas fa-exclamation-triangle me-2"></i>
          {{ error }}
          <button class="btn btn-outline-danger btn-sm ms-3" (click)="refreshDashboard()">
            <i class="fas fa-refresh me-1"></i>
            Retry
          </button>
        </div>
        <!-- Welcome Section with Weather Widget -->
        <div class="row mb-4">
        <div class="col-lg-8 mb-3">
          <div class="welcome-card">
            <div class="welcome-content">
              <div class="welcome-text">
                <h1 class="welcome-title">
                  Welcome back, {{ currentUser?.fullName }}!
                </h1>
                <p class="welcome-subtitle">
                  {{ getRoleBasedWelcomeMessage() }}
                </p>
                <div class="user-info">
                  <span class="user-role">{{ getUserRoleDisplay() }}</span>
                  <span class="user-branch">{{ currentUser?.branchId ? 'Branch ' + currentUser?.branchId : '' }}</span>
                </div>
              </div>
              <div class="welcome-avatar" *ngIf="currentUser?.profilePhoto">
                <img [src]="currentUser?.profilePhoto" [alt]="currentUser?.fullName" class="avatar-img">
              </div>
            </div>
          </div>
        </div>
        <div class="col-lg-4 mb-3">
          <app-weather-time-widget></app-weather-time-widget>
        </div>
      </div>

      <!-- Birthday Widget -->
      <div class="row mb-4">
        <div class="col-12">
          <app-birthday-widget></app-birthday-widget>
        </div>
      </div>

      <!-- Role-based Dashboard Content -->
      <div class="row" [ngSwitch]="getPrimaryRole()">
        
        <!-- Employee Dashboard -->
        <ng-container *ngSwitchCase="'Employee'">
          <div class="col-lg-8 mb-4">
            <div class="row">
              <!-- Attendance Widget for Employee -->
              <div class="col-12 mb-3">
                <app-attendance-widget 
                  [showPersonalStatus]="true"
                  [showTeamOverview]="false"
                  [showQuickActions]="true">
                </app-attendance-widget>
              </div>
              
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-primary">
                    <i class="fas fa-tasks"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ employeeStats.activeTasks || '0' }}</h3>
                    <p class="widget-label">Active Tasks</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-warning">
                    <i class="fas fa-calendar-alt"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ employeeStats.leaveBalance || '0' }}</h3>
                    <p class="widget-label">Leave Balance</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-info">
                    <i class="fas fa-chart-line"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ employeeStats.productivity || '0' }}%</h3>
                    <p class="widget-label">Productivity</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-4 mb-4">
            <app-quick-actions></app-quick-actions>
          </div>
        </ng-container>

        <!-- Manager Dashboard -->
        <ng-container *ngSwitchCase="'Manager'">
          <div class="col-lg-8 mb-4">
            <div class="row">
              <!-- Attendance Widget for Manager -->
              <div class="col-12 mb-3">
                <app-attendance-widget 
                  [showPersonalStatus]="true"
                  [showTeamOverview]="true"
                  [showQuickActions]="true"
                  [branchId]="currentUser?.branchId">
                </app-attendance-widget>
              </div>
              
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-primary">
                    <i class="fas fa-users"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ managerStats.teamSize || '0' }}</h3>
                    <p class="widget-label">Team Members</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-warning">
                    <i class="fas fa-project-diagram"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ managerStats.activeProjects || '0' }}</h3>
                    <p class="widget-label">Active Projects</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-danger">
                    <i class="fas fa-exclamation-triangle"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ managerStats.pendingApprovals || '0' }}</h3>
                    <p class="widget-label">Pending Approvals</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-4 mb-4">
            <app-quick-actions></app-quick-actions>
          </div>
        </ng-container>

        <!-- HR Dashboard -->
        <ng-container *ngSwitchCase="'HR'">
          <div class="col-lg-8 mb-4">
            <div class="row">
              <!-- Attendance Widget for HR -->
              <div class="col-12 mb-3">
                <app-attendance-widget 
                  [showPersonalStatus]="true"
                  [showTeamOverview]="true"
                  [showQuickActions]="true"
                  [branchId]="currentUser?.branchId">
                </app-attendance-widget>
              </div>
              
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-primary">
                    <i class="fas fa-users"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ hrStats.totalEmployees || '0' }}</h3>
                    <p class="widget-label">Total Employees</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-warning">
                    <i class="fas fa-calendar-alt"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ hrStats.pendingLeaves || '0' }}</h3>
                    <p class="widget-label">Pending Leaves</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-info">
                    <i class="fas fa-money-bill-wave"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ hrStats.payrollStatus || 'Pending' }}</h3>
                    <p class="widget-label">Payroll Status</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-4 mb-4">
            <app-quick-actions></app-quick-actions>
          </div>
        </ng-container>

        <!-- Admin Dashboard -->
        <ng-container *ngSwitchCase="'Admin'">
          <div class="col-lg-8 mb-4">
            <div class="row">
              <!-- Attendance Widget for Admin -->
              <div class="col-12 mb-3">
                <app-attendance-widget 
                  [showPersonalStatus]="true"
                  [showTeamOverview]="true"
                  [showQuickActions]="true"
                  [branchId]="currentUser?.branchId">
                </app-attendance-widget>
              </div>
              
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-primary">
                    <i class="fas fa-building"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ adminStats.totalBranches || '0' }}</h3>
                    <p class="widget-label">Total Branches</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-success">
                    <i class="fas fa-users"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ adminStats.totalEmployees || '0' }}</h3>
                    <p class="widget-label">Total Employees</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-warning">
                    <i class="fas fa-server"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ adminStats.systemHealth || 'Good' }}</h3>
                    <p class="widget-label">System Health</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-info">
                    <i class="fas fa-chart-bar"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ adminStats.activeUsers || '0' }}</h3>
                    <p class="widget-label">Active Users</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-4 mb-4">
            <app-quick-actions></app-quick-actions>
          </div>
        </ng-container>

        <!-- Super Admin Dashboard -->
        <ng-container *ngSwitchCase="'SuperAdmin'">
          <div class="col-lg-8 mb-4">
            <div class="row">
              <!-- Role Switching Widget for Super Admin -->
              <div class="col-12 mb-3">
                <div class="card">
                  <div class="card-header">
                    <h5 class="card-title mb-0">
                      <i class="fas fa-user-cog me-2"></i>
                      Role Management
                    </h5>
                  </div>
                  <div class="card-body">
                    <p class="mb-3">Switch between different role views to access specific dashboards:</p>
                    <div class="role-switch-buttons">
                      <button class="btn btn-outline-primary btn-sm me-2 mb-2" (click)="switchToRole('Employee')">
                        <i class="fas fa-user me-1"></i> Employee View
                      </button>
                      <button class="btn btn-outline-success btn-sm me-2 mb-2" (click)="switchToRole('Manager')">
                        <i class="fas fa-users me-1"></i> Manager View
                      </button>
                      <button class="btn btn-outline-info btn-sm me-2 mb-2" (click)="switchToRole('HR')">
                        <i class="fas fa-user-tie me-1"></i> HR View
                      </button>
                      <button class="btn btn-outline-warning btn-sm me-2 mb-2" (click)="switchToRole('Admin')">
                        <i class="fas fa-cog me-1"></i> Admin View
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Attendance Widget for Super Admin -->
              <div class="col-12 mb-3">
                <app-attendance-widget 
                  [showPersonalStatus]="true"
                  [showTeamOverview]="true"
                  [showQuickActions]="true"
                  [branchId]="currentUser?.branchId">
                </app-attendance-widget>
              </div>
              
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-primary">
                    <i class="fas fa-building"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ superAdminStats.totalOrganizations || '0' }}</h3>
                    <p class="widget-label">Organizations</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-success">
                    <i class="fas fa-code-branch"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ superAdminStats.totalBranches || '0' }}</h3>
                    <p class="widget-label">Total Branches</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-info">
                    <i class="fas fa-users"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ superAdminStats.totalEmployees || '0' }}</h3>
                    <p class="widget-label">Total Employees</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-warning">
                    <i class="fas fa-server"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ superAdminStats.systemHealth || 'Good' }}</h3>
                    <p class="widget-label">System Health</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-danger">
                    <i class="fas fa-exclamation-triangle"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ superAdminStats.criticalAlerts || '0' }}</h3>
                    <p class="widget-label">Critical Alerts</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-secondary">
                    <i class="fas fa-database"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ superAdminStats.databaseHealth || 'Good' }}</h3>
                    <p class="widget-label">Database Health</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-dark">
                    <i class="fas fa-tachometer-alt"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ superAdminStats.serverLoad || '0' }}%</h3>
                    <p class="widget-label">Server Load</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-success">
                    <i class="fas fa-clock"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ superAdminStats.systemUptime || '0%' }}</h3>
                    <p class="widget-label">System Uptime</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-4 mb-4">
            <app-quick-actions></app-quick-actions>
          </div>
        </ng-container>

        <!-- Default Dashboard -->
        <ng-container *ngSwitchDefault>
          <div class="col-lg-8 mb-4">
            <div class="row">
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-primary">
                    <i class="fas fa-users"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">150</h3>
                    <p class="widget-label">Total Employees</p>
                  </div>
                </div>
              </div>
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-success">
                    <i class="fas fa-clock"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">142</h3>
                    <p class="widget-label">Present Today</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-4 mb-4">
            <app-quick-actions></app-quick-actions>
          </div>
        </ng-container>
      </div>

      <!-- Recent Activities Section -->
      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">Recent Activities</h5>
            </div>
            <div class="card-body">
              <div class="activity-item" *ngFor="let activity of recentActivities">
                <div class="activity-icon" [ngClass]="'bg-' + activity.type">
                  <i [class]="activity.icon"></i>
                </div>
                <div class="activity-content">
                  <p class="mb-1" [innerHTML]="activity.message"></p>
                  <small class="text-muted">{{ activity.timestamp }}</small>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      </div>
    </app-progressive-dashboard>
  `,
  styles: [`
    .dashboard-container {
      padding: 0;
    }

    .welcome-card {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      color: white;
      border: none;
      border-radius: 16px;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
      height: 100%;
      display: flex;
      align-items: center;
    }

    .welcome-content {
      display: flex;
      align-items: center;
      justify-content: space-between;
      width: 100%;
      padding: 1.5rem;
    }

    .welcome-text {
      flex: 1;
    }

    .welcome-title {
      font-size: 2rem;
      font-weight: 700;
      margin-bottom: 0.5rem;
      line-height: 1.2;
    }

    .welcome-subtitle {
      font-size: 1rem;
      opacity: 0.9;
      margin-bottom: 1rem;
      line-height: 1.4;
    }

    .user-info {
      display: flex;
      gap: 1rem;
      font-size: 0.875rem;
      opacity: 0.8;
    }

    .user-role {
      background: rgba(255, 255, 255, 0.2);
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      font-weight: 500;
    }

    .user-branch {
      background: rgba(255, 255, 255, 0.1);
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
    }

    .welcome-avatar {
      flex-shrink: 0;
      margin-left: 1rem;
    }

    .avatar-img {
      width: 80px;
      height: 80px;
      border-radius: 50%;
      object-fit: cover;
      border: 3px solid rgba(255, 255, 255, 0.3);
    }

    .dashboard-widget {
      background: white;
      border-radius: 16px;
      padding: 1.5rem;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
      border: 1px solid var(--gray-100);
      transition: all 0.2s ease-in-out;
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .dashboard-widget:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
    }

    .widget-icon {
      width: 60px;
      height: 60px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 1.5rem;
    }

    .widget-content {
      flex: 1;
    }

    .widget-value {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 0.25rem;
    }

    .widget-label {
      color: var(--text-secondary);
      margin-bottom: 0;
      font-weight: 500;
    }

    .card {
      border: none;
      border-radius: 16px;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      background: var(--bg-secondary);
      border-bottom: 1px solid var(--gray-200);
      border-radius: 16px 16px 0 0 !important;
      padding: 1.25rem 1.5rem;
    }

    .card-title {
      font-weight: 600;
      color: var(--text-primary);
    }

    .activity-item {
      display: flex;
      align-items: flex-start;
      gap: 1rem;
      padding: 1rem 0;
      border-bottom: 1px solid var(--gray-100);
    }

    .activity-item:last-child {
      border-bottom: none;
    }

    .activity-icon {
      width: 40px;
      height: 40px;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 1rem;
      flex-shrink: 0;
    }

    .activity-content {
      flex: 1;
    }

    .btn {
      border-radius: 8px;
      font-weight: 500;
      padding: 0.75rem 1.25rem;
    }

    .btn-primary {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      border: none;
    }

    .btn-primary:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(59, 130, 246, 0.4);
    }

    .btn-outline-primary {
      border: 2px solid var(--primary);
      color: var(--primary);
    }

    .btn-outline-primary:hover {
      background: var(--primary);
      border-color: var(--primary);
      transform: translateY(-1px);
    }

    /* Mobile-responsive dashboard */
    @media (max-width: 768px) {
      .dashboard-container {
        padding: 0;
      }
      
      .welcome-content {
        flex-direction: column;
        text-align: center;
        gap: 1rem;
      }
      
      .welcome-text {
        order: 2;
      }
      
      .welcome-avatar {
        order: 1;
        margin-left: 0;
      }
      
      .welcome-title {
        font-size: 1.5rem;
      }
      
      .welcome-subtitle {
        font-size: 0.9rem;
      }
      
      .user-info {
        justify-content: center;
        flex-wrap: wrap;
        gap: 0.5rem;
      }
      
      .dashboard-widget {
        padding: 1rem;
        margin-bottom: 1rem;
      }
      
      .widget-icon {
        width: 50px;
        height: 50px;
        font-size: 1.25rem;
      }
      
      .widget-value {
        font-size: 1.5rem;
      }
      
      .activity-item {
        padding: 0.75rem 0;
      }
      
      .activity-icon {
        width: 35px;
        height: 35px;
        font-size: 0.875rem;
      }
    }

    /* Extra small screens */
    @media (max-width: 576px) {
      .welcome-card {
        border-radius: 12px;
      }
      
      .welcome-content {
        padding: 1rem;
      }
      
      .welcome-title {
        font-size: 1.25rem;
      }
      
      .dashboard-widget {
        padding: 0.75rem;
        flex-direction: column;
        text-align: center;
        gap: 0.75rem;
      }
      
      .widget-content {
        text-align: center;
      }
      
      .card-header {
        padding: 1rem;
      }
      
      .card-body {
        padding: 1rem;
      }
    }

    /* Touch-friendly improvements */
    .dashboard-widget {
      cursor: pointer;
      -webkit-tap-highlight-color: rgba(0, 0, 0, 0.05);
    }

    .dashboard-widget:active {
      transform: translateY(-1px) scale(0.98);
    }
  `]
})
export class DashboardComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  dashboardStats: DashboardStats = {};
  recentActivities: DashboardActivity[] = [];
  isLoading = false;
  error: string | null = null;

  private destroy$ = new Subject<void>();

  // Role-based statistics (fallback)
  employeeStats = {
    todayHours: '0.0',
    activeTasks: 0,
    leaveBalance: 0,
    productivity: 0
  };

  managerStats = {
    teamSize: 0,
    presentToday: 0,
    activeProjects: 0,
    pendingApprovals: 0
  };

  hrStats = {
    totalEmployees: 0,
    presentToday: 0,
    pendingLeaves: 0,
    payrollStatus: 'Pending'
  };

  adminStats = {
    totalBranches: 0,
    totalEmployees: 0,
    systemHealth: 'Unknown',
    activeUsers: 0
  };

  superAdminStats = {
    totalOrganizations: 0,
    totalBranches: 0,
    totalEmployees: 0,
    systemHealth: 'Unknown',
    activeUsers: 0,
    systemUptime: '0%',
    criticalAlerts: 0,
    databaseHealth: 'Unknown',
    serverLoad: 0
  };

  constructor(
    private authService: AuthService,
    private dashboardService: DashboardService,
    private realTimeService: RealTimeAttendanceService
  ) { }

  ngOnInit(): void {
    this.currentUser = this.authService.currentUser;
    this.initializeDashboard();
    this.setupRealTimeUpdates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Role switching for SuperAdmin
  private currentViewRole: string | null = null;

  getPrimaryRole(): string {
    // If SuperAdmin has switched to a specific role view, use that
    if (this.currentViewRole && this.currentUser?.roles.includes('SuperAdmin')) {
      return this.currentViewRole;
    }

    if (!this.currentUser?.roles || this.currentUser.roles.length === 0) {
      return 'Employee';
    }

    // Priority order: SuperAdmin > Admin > HR > Manager > Employee
    const rolePriority = ['SuperAdmin', 'Admin', 'HR', 'Manager', 'Employee'];

    for (const role of rolePriority) {
      if (this.currentUser.roles.includes(role)) {
        return role;
      }
    }

    return 'Employee';
  }

  /**
   * Switch to a specific role view (SuperAdmin only)
   */
  switchToRole(role: string): void {
    if (!this.currentUser?.roles.includes('SuperAdmin')) {
      return;
    }

    if (!this.currentUser.roles.includes(role) && role !== 'SuperAdmin') {
      console.warn(`User does not have ${role} role`);
      return;
    }

    this.currentViewRole = role;

    // Reload dashboard data for the new role
    this.initializeDashboard();

    // Show success message
    this.showSuccess(`Switched to ${role} view`);
  }

  /**
   * Reset to default SuperAdmin view
   */
  resetToSuperAdminView(): void {
    this.currentViewRole = null;
    this.initializeDashboard();
  }

  /**
   * Check if currently viewing a specific role
   */
  isViewingRole(role: string): boolean {
    return this.currentViewRole === role;
  }

  /**
   * Show success message
   */
  private showSuccess(message: string): void {
    // Implement toast notification or similar
    console.log('Success:', message);
  }

  getUserRoleDisplay(): string {
    const primaryRole = this.getPrimaryRole();
    const additionalRoles = this.currentUser?.roles?.filter(role => role !== primaryRole) || [];

    if (additionalRoles.length > 0) {
      return `${primaryRole} (+${additionalRoles.length} more)`;
    }

    return primaryRole;
  }

  getRoleBasedWelcomeMessage(): string {
    const role = this.getPrimaryRole();
    const messages = {
      'SuperAdmin': 'Oversee and manage the entire system across all organizations.',
      'Admin': 'Monitor and manage your organization\'s HR operations across all branches.',
      'HR': 'Manage employee lifecycle, payroll, and organizational policies effectively.',
      'Manager': 'Lead your team to success and track project progress efficiently.',
      'Employee': 'Stay productive and manage your work-life balance effectively.'
    };

    return messages[role as keyof typeof messages] || messages['Employee'];
  }

  /**
   * Initialize dashboard with real data
   */
  private initializeDashboard(): void {
    this.isLoading = true;
    this.error = null;

    // Load dashboard statistics
    this.dashboardService.dashboardStats$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (stats) => {
          this.dashboardStats = stats;
          this.updateLocalStats(stats);
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading dashboard stats:', error);
          this.error = 'Failed to load dashboard data';
          this.isLoading = false;
        }
      });

    // Load recent activities
    this.dashboardService.recentActivities$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (activities) => {
          this.recentActivities = activities;
        },
        error: (error) => {
          console.error('Error loading recent activities:', error);
        }
      });

    // Trigger initial data load
    this.dashboardService.loadDashboardData().subscribe();
    this.dashboardService.loadRecentActivities().subscribe();
  }

  /**
   * Setup real-time updates
   */
  private setupRealTimeUpdates(): void {
    // Connect to SignalR for real-time updates
    this.realTimeService.connect().catch(error => {
      console.log('SignalR connection failed, using polling fallback:', error);
    });

    // Listen for real-time attendance updates
    this.realTimeService.personalStatusUpdates$
      .pipe(takeUntil(this.destroy$))
      .subscribe(status => {
        if (status && this.dashboardStats.employeeStats) {
          this.dashboardStats.employeeStats.currentStatus = status.currentStatus;
          this.dashboardStats.employeeStats.checkInTime = status.checkInTime;
          // Note: checkOutTime is not available in AttendanceStatus interface
          // this.dashboardStats.employeeStats.checkOutTime = status.checkOutTime;
        }
      });

    // Listen for team overview updates (for managers)
    this.realTimeService.teamOverviewUpdates$
      .pipe(takeUntil(this.destroy$))
      .subscribe(overview => {
        if (overview && this.dashboardStats.managerStats) {
          this.dashboardStats.managerStats.presentToday = overview.summary?.presentCount || 0;
          this.dashboardStats.managerStats.teamSize = overview.summary?.totalEmployees || 0;
        }
      });
  }

  /**
   * Update local stats from dashboard service
   */
  private updateLocalStats(stats: DashboardStats): void {
    if (stats.employeeStats) {
      this.employeeStats = {
        todayHours: stats.employeeStats.todayHours || '0.0',
        activeTasks: stats.employeeStats.activeTasks || 0,
        leaveBalance: stats.employeeStats.leaveBalance || 0,
        productivity: stats.employeeStats.productivity || 0
      };
    }

    if (stats.managerStats) {
      this.managerStats = {
        teamSize: stats.managerStats.teamSize || 0,
        presentToday: stats.managerStats.presentToday || 0,
        activeProjects: stats.managerStats.activeProjects || 0,
        pendingApprovals: stats.managerStats.pendingApprovals || 0
      };
    }

    if (stats.hrStats) {
      this.hrStats = {
        totalEmployees: stats.hrStats.totalEmployees || 0,
        presentToday: stats.hrStats.presentToday || 0,
        pendingLeaves: stats.hrStats.pendingLeaves || 0,
        payrollStatus: stats.hrStats.payrollStatus || 'Pending'
      };
    }

    if (stats.adminStats) {
      this.adminStats = {
        totalBranches: stats.adminStats.totalBranches || 0,
        totalEmployees: stats.adminStats.totalEmployees || 0,
        systemHealth: stats.adminStats.systemHealth || 'Unknown',
        activeUsers: stats.adminStats.activeUsers || 0
      };
    }

    if (stats.superAdminStats) {
      this.superAdminStats = {
        totalOrganizations: stats.superAdminStats.totalOrganizations || 0,
        totalBranches: stats.superAdminStats.totalBranches || 0,
        totalEmployees: stats.superAdminStats.totalEmployees || 0,
        systemHealth: stats.superAdminStats.systemHealth || 'Unknown',
        activeUsers: stats.superAdminStats.activeUsers || 0,
        systemUptime: stats.superAdminStats.systemUptime || '0%',
        criticalAlerts: stats.superAdminStats.criticalAlerts || 0,
        databaseHealth: stats.superAdminStats.databaseHealth || 'Unknown',
        serverLoad: stats.superAdminStats.serverLoad || 0
      };
    }
  }

  /**
   * Refresh dashboard data manually
   */
  public refreshDashboard(): void {
    this.initializeDashboard();
  }

}