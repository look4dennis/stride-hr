import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService, User } from '../../../core/auth/auth.service';

export interface QuickAction {
  id: string;
  title: string;
  description: string;
  icon: string;
  route: string;
  color: string;
  roles: string[];
  badge?: string;
  disabled?: boolean;
}

@Component({
  selector: 'app-quick-actions',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="quick-actions-widget">
      <div class="widget-header">
        <h5 class="widget-title">
          <i class="fas fa-bolt"></i>
          Quick Actions
        </h5>
      </div>

      <div class="actions-grid">
        <div class="action-item" 
             *ngFor="let action of availableActions"
             (click)="executeAction(action)"
             [class.disabled]="action.disabled">
          <div class="action-icon" [ngClass]="'bg-' + action.color">
            <i [class]="action.icon"></i>
            <span class="action-badge" *ngIf="action.badge">{{ action.badge }}</span>
          </div>
          <div class="action-content">
            <div class="action-title">{{ action.title }}</div>
            <div class="action-description">{{ action.description }}</div>
          </div>
          <div class="action-arrow">
            <i class="fas fa-chevron-right"></i>
          </div>
        </div>
      </div>

      <!-- Role-specific action sections -->
      <div class="role-specific-actions" *ngIf="hasManagerActions">
        <h6 class="section-title">Management Actions</h6>
        <div class="actions-grid">
          <div class="action-item" 
               *ngFor="let action of managerActions"
               (click)="executeAction(action)"
               [class.disabled]="action.disabled">
            <div class="action-icon" [ngClass]="'bg-' + action.color">
              <i [class]="action.icon"></i>
              <span class="action-badge" *ngIf="action.badge">{{ action.badge }}</span>
            </div>
            <div class="action-content">
              <div class="action-title">{{ action.title }}</div>
              <div class="action-description">{{ action.description }}</div>
            </div>
            <div class="action-arrow">
              <i class="fas fa-chevron-right"></i>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .quick-actions-widget {
      background: white;
      border-radius: 16px;
      padding: 1.5rem;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
      border: 1px solid var(--gray-100);
      transition: all 0.2s ease-in-out;
    }

    .quick-actions-widget:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
    }

    .widget-header {
      margin-bottom: 1.5rem;
    }

    .widget-title {
      font-weight: 600;
      color: var(--text-primary);
      margin: 0;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .widget-title i {
      color: #fbbf24;
    }

    .actions-grid {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .action-item {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 1rem;
      background: var(--bg-secondary);
      border-radius: 12px;
      border: 1px solid var(--gray-200);
      cursor: pointer;
      transition: all 0.2s ease;
      position: relative;
    }

    .action-item:hover {
      background: var(--bg-tertiary);
      transform: translateX(4px);
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .action-item.disabled {
      opacity: 0.6;
      cursor: not-allowed;
      pointer-events: none;
    }

    .action-icon {
      width: 48px;
      height: 48px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 1.25rem;
      flex-shrink: 0;
      position: relative;
    }

    .action-badge {
      position: absolute;
      top: -8px;
      right: -8px;
      background: #ef4444;
      color: white;
      font-size: 0.75rem;
      font-weight: 600;
      padding: 0.125rem 0.375rem;
      border-radius: 10px;
      min-width: 20px;
      text-align: center;
      border: 2px solid white;
    }

    .action-content {
      flex: 1;
    }

    .action-title {
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 0.25rem;
      font-size: 0.95rem;
    }

    .action-description {
      font-size: 0.85rem;
      color: var(--text-secondary);
      line-height: 1.3;
    }

    .action-arrow {
      color: var(--gray-400);
      font-size: 0.875rem;
      transition: all 0.2s ease;
    }

    .action-item:hover .action-arrow {
      color: var(--primary);
      transform: translateX(2px);
    }

    .role-specific-actions {
      margin-top: 2rem;
      padding-top: 1.5rem;
      border-top: 1px solid var(--gray-200);
    }

    .section-title {
      font-size: 0.875rem;
      font-weight: 600;
      color: var(--text-secondary);
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 1rem;
    }

    /* Color classes for action icons */
    .bg-primary { background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%); }
    .bg-success { background: linear-gradient(135deg, #10b981 0%, #059669 100%); }
    .bg-warning { background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); }
    .bg-info { background: linear-gradient(135deg, #06b6d4 0%, #0891b2 100%); }
    .bg-danger { background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%); }
    .bg-purple { background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%); }
    .bg-pink { background: linear-gradient(135deg, #ec4899 0%, #db2777 100%); }
    .bg-indigo { background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%); }

    @media (max-width: 768px) {
      .quick-actions-widget {
        padding: 1rem;
      }

      .action-item {
        padding: 0.875rem;
      }

      .action-icon {
        width: 40px;
        height: 40px;
        font-size: 1rem;
      }

      .action-title {
        font-size: 0.9rem;
      }

      .action-description {
        font-size: 0.8rem;
      }
    }
  `]
})
export class QuickActionsComponent implements OnInit {
  currentUser: User | null = null;
  availableActions: QuickAction[] = [];
  managerActions: QuickAction[] = [];
  hasManagerActions: boolean = false;

  private allActions: QuickAction[] = [
    // Employee Actions
    {
      id: 'check-in-out',
      title: 'Check In/Out',
      description: 'Record your attendance for today',
      icon: 'fas fa-clock',
      route: '/attendance/check-in',
      color: 'success',
      roles: ['Employee', 'Manager', 'HR', 'Admin']
    },
    {
      id: 'request-leave',
      title: 'Request Leave',
      description: 'Submit a new leave request',
      icon: 'fas fa-calendar-plus',
      route: '/leave/request',
      color: 'primary',
      roles: ['Employee', 'Manager', 'HR', 'Admin']
    },
    {
      id: 'submit-dsr',
      title: 'Submit DSR',
      description: 'Submit your daily status report',
      icon: 'fas fa-file-alt',
      route: '/projects/dsr',
      color: 'info',
      roles: ['Employee', 'Manager', 'HR', 'Admin']
    },
    {
      id: 'view-payslip',
      title: 'View Payslip',
      description: 'Access your latest payslip',
      icon: 'fas fa-money-bill-wave',
      route: '/payroll/payslip',
      color: 'warning',
      roles: ['Employee', 'Manager', 'HR', 'Admin']
    },
    {
      id: 'my-profile',
      title: 'My Profile',
      description: 'Update your profile information',
      icon: 'fas fa-user-edit',
      route: '/profile',
      color: 'purple',
      roles: ['Employee', 'Manager', 'HR', 'Admin']
    },
    {
      id: 'attendance-now',
      title: 'Attendance Now',
      description: 'View real-time attendance status',
      icon: 'fas fa-users',
      route: '/attendance/now',
      color: 'indigo',
      roles: ['Employee', 'Manager', 'HR', 'Admin']
    },

    // Manager Actions
    {
      id: 'approve-leaves',
      title: 'Approve Leaves',
      description: 'Review and approve leave requests',
      icon: 'fas fa-check-circle',
      route: '/leave/approvals',
      color: 'success',
      roles: ['Manager', 'HR', 'Admin'],
      badge: '3'
    },
    {
      id: 'team-performance',
      title: 'Team Performance',
      description: 'Monitor your team\'s performance',
      icon: 'fas fa-chart-line',
      route: '/performance/team',
      color: 'primary',
      roles: ['Manager', 'HR', 'Admin']
    },
    {
      id: 'project-management',
      title: 'Manage Projects',
      description: 'Create and manage team projects',
      icon: 'fas fa-project-diagram',
      route: '/projects/manage',
      color: 'info',
      roles: ['Manager', 'HR', 'Admin']
    },

    // HR Actions
    {
      id: 'employee-management',
      title: 'Manage Employees',
      description: 'Add, edit, and manage employees',
      icon: 'fas fa-users-cog',
      route: '/employees',
      color: 'primary',
      roles: ['HR', 'Admin']
    },
    {
      id: 'payroll-processing',
      title: 'Process Payroll',
      description: 'Generate and process payroll',
      icon: 'fas fa-calculator',
      route: '/payroll/process',
      color: 'warning',
      roles: ['HR', 'Admin'],
      badge: '2'
    },
    {
      id: 'reports',
      title: 'Generate Reports',
      description: 'Create comprehensive HR reports',
      icon: 'fas fa-chart-bar',
      route: '/reports',
      color: 'info',
      roles: ['HR', 'Admin']
    },

    // Admin Actions
    {
      id: 'system-settings',
      title: 'System Settings',
      description: 'Configure system parameters',
      icon: 'fas fa-cogs',
      route: '/settings',
      color: 'danger',
      roles: ['Admin']
    },
    {
      id: 'user-management',
      title: 'User Management',
      description: 'Manage user accounts and roles',
      icon: 'fas fa-user-shield',
      route: '/users',
      color: 'purple',
      roles: ['Admin']
    }
  ];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.currentUser;
    this.loadAvailableActions();
  }

  private loadAvailableActions(): void {
    if (!this.currentUser) return;

    // Filter actions based on user roles
    const userRoles = this.currentUser.roles;
    
    // Get employee-level actions
    this.availableActions = this.allActions.filter(action => 
      action.roles.some(role => userRoles.includes(role)) &&
      !this.isManagerAction(action)
    );

    // Get manager-level actions
    this.managerActions = this.allActions.filter(action => 
      action.roles.some(role => userRoles.includes(role)) &&
      this.isManagerAction(action)
    );

    this.hasManagerActions = this.managerActions.length > 0;
  }

  private isManagerAction(action: QuickAction): boolean {
    const managerActionIds = [
      'approve-leaves', 'team-performance', 'project-management',
      'employee-management', 'payroll-processing', 'reports',
      'system-settings', 'user-management'
    ];
    return managerActionIds.includes(action.id);
  }

  executeAction(action: QuickAction): void {
    if (action.disabled) return;

    // Handle special actions
    switch (action.id) {
      case 'check-in-out':
        this.handleCheckInOut();
        break;
      case 'attendance-now':
        this.router.navigate([action.route]);
        break;
      default:
        this.router.navigate([action.route]);
        break;
    }
  }

  private handleCheckInOut(): void {
    // This could open a modal or navigate to attendance page
    // For now, we'll navigate to the attendance page
    this.router.navigate(['/attendance']);
  }
}