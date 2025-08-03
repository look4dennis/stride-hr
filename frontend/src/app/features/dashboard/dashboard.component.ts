import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService, User } from '../../core/auth/auth.service';
import { WeatherTimeWidgetComponent } from '../../shared/components/weather-time-widget/weather-time-widget.component';
import { BirthdayWidgetComponent } from '../../shared/components/birthday-widget/birthday-widget.component';
import { QuickActionsComponent } from '../../shared/components/quick-actions/quick-actions.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    WeatherTimeWidgetComponent,
    BirthdayWidgetComponent,
    QuickActionsComponent
  ],
  template: `
    <div class="dashboard-container">
      <!-- Welcome Section with Weather Widget -->
      <div class="row mb-4">
        <div class="col-lg-8 mb-3">
          <div class="welcome-card">
            <div class="welcome-content">
              <div class="welcome-text">
                <h1 class="welcome-title">
                  Welcome back, {{ currentUser?.firstName }}!
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
                <img [src]="currentUser?.profilePhoto" [alt]="currentUser?.firstName" class="avatar-img">
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
              <div class="col-md-6 mb-3">
                <div class="dashboard-widget">
                  <div class="widget-icon bg-success">
                    <i class="fas fa-clock"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ employeeStats.todayHours || '0.0' }}</h3>
                    <p class="widget-label">Hours Today</p>
                  </div>
                </div>
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
                  <div class="widget-icon bg-success">
                    <i class="fas fa-user-check"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ managerStats.presentToday || '0' }}</h3>
                    <p class="widget-label">Present Today</p>
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
                  <div class="widget-icon bg-success">
                    <i class="fas fa-user-check"></i>
                  </div>
                  <div class="widget-content">
                    <h3 class="widget-value">{{ hrStats.presentToday || '0' }}</h3>
                    <p class="widget-label">Present Today</p>
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
  `]
})
export class DashboardComponent implements OnInit {
  currentUser: User | null = null;
  
  // Role-based statistics
  employeeStats = {
    todayHours: '7.5',
    activeTasks: '5',
    leaveBalance: '12',
    productivity: '85'
  };

  managerStats = {
    teamSize: '8',
    presentToday: '7',
    activeProjects: '3',
    pendingApprovals: '4'
  };

  hrStats = {
    totalEmployees: '150',
    presentToday: '142',
    pendingLeaves: '8',
    payrollStatus: 'In Progress'
  };

  adminStats = {
    totalBranches: '5',
    totalEmployees: '150',
    systemHealth: 'Excellent',
    activeUsers: '98'
  };

  recentActivities = [
    {
      type: 'success',
      icon: 'fas fa-user-plus',
      message: '<strong>John Doe</strong> joined the Development team',
      timestamp: '2 hours ago'
    },
    {
      type: 'primary',
      icon: 'fas fa-project-diagram',
      message: 'New project <strong>"Mobile App Redesign"</strong> created',
      timestamp: '4 hours ago'
    },
    {
      type: 'warning',
      icon: 'fas fa-calendar-alt',
      message: '<strong>Jane Smith</strong> requested leave for next week',
      timestamp: '6 hours ago'
    },
    {
      type: 'info',
      icon: 'fas fa-clock',
      message: '<strong>Mike Johnson</strong> checked in at 9:15 AM',
      timestamp: '8 hours ago'
    }
  ];

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.currentUser = this.authService.currentUser;
    this.loadDashboardData();
  }

  getPrimaryRole(): string {
    if (!this.currentUser?.roles || this.currentUser.roles.length === 0) {
      return 'Employee';
    }

    // Priority order: Admin > HR > Manager > Employee
    const rolePriority = ['Admin', 'HR', 'Manager', 'Employee'];
    
    for (const role of rolePriority) {
      if (this.currentUser.roles.includes(role)) {
        return role;
      }
    }

    return 'Employee';
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
      'Admin': 'Monitor and manage your organization\'s HR operations across all branches.',
      'HR': 'Manage employee lifecycle, payroll, and organizational policies effectively.',
      'Manager': 'Lead your team to success and track project progress efficiently.',
      'Employee': 'Stay productive and manage your work-life balance effectively.'
    };

    return messages[role as keyof typeof messages] || messages['Employee'];
  }

  private loadDashboardData(): void {
    // In a real application, this would load data from services
    // For now, we're using mock data defined above
    
    // TODO: Implement actual data loading based on user role
    // Example:
    // if (this.getPrimaryRole() === 'Manager') {
    //   this.loadManagerStats();
    // } else if (this.getPrimaryRole() === 'HR') {
    //   this.loadHRStats();
    // }
  }
}