import { Routes } from '@angular/router';
import { AuthGuard } from './core/auth/auth.guard';
import { RoleGuard } from './core/guards/role.guard';
import { LayoutComponent } from './shared/components/layout/layout.component';
import { LoginComponent } from './features/auth/login/login.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { RouteLoadingService } from './core/services/route-loading.service';
import { inject } from '@angular/core';

// Helper function to create safe lazy loading with error handling
function createSafeLazyLoad(route: string, importFn: () => Promise<any>) {
  return () => {
    const routeLoadingService = inject(RouteLoadingService);
    return importFn().catch(error => {
      console.error(`Failed to load route: ${route}`, error);
      return routeLoadingService.handleLazyLoadError(route, error);
    });
  };
}

export const routes: Routes = [
  // Public routes
  {
    path: 'login',
    component: LoginComponent
  },

  // Protected routes with layout
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    canActivateChild: [RoleGuard],
    children: [
      {
        path: '',
        redirectTo: '/dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        component: DashboardComponent
      },
      {
        path: 'employees',
        children: [
          {
            path: '',
            loadComponent: createSafeLazyLoad(
              'employees',
              () => import('./features/employees/employee-list/employee-list.component').then(m => m.EmployeeListComponent)
            ),
            data: { roles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] }
          },
          {
            path: 'add',
            loadComponent: createSafeLazyLoad(
              'employees/add',
              () => import('./features/employees/employee-create/employee-create.component').then(m => m.EmployeeCreateComponent)
            ),
            data: { roles: ['HR', 'Admin', 'SuperAdmin'] }
          },
          {
            path: 'org-chart',
            loadComponent: createSafeLazyLoad(
              'employees/org-chart',
              () => import('./features/employees/org-chart/org-chart.component').then(m => m.OrgChartComponent)
            ),
            data: { roles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] }
          },
          {
            path: ':id',
            loadComponent: createSafeLazyLoad(
              'employees/:id',
              () => import('./features/employees/employee-profile/employee-profile.component').then(m => m.EmployeeProfileComponent)
            ),
            data: { roles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] }
          },
          {
            path: ':id/edit',
            loadComponent: createSafeLazyLoad(
              'employees/:id/edit',
              () => import('./features/employees/employee-profile/employee-profile.component').then(m => m.EmployeeProfileComponent)
            ),
            data: { roles: ['HR', 'Admin', 'SuperAdmin'] }
          },
          {
            path: ':id/onboarding',
            loadComponent: createSafeLazyLoad(
              'employees/:id/onboarding',
              () => import('./features/employees/employee-onboarding/employee-onboarding.component').then(m => m.EmployeeOnboardingComponent)
            ),
            data: { roles: ['HR', 'Admin', 'SuperAdmin'] }
          },
          {
            path: ':id/exit',
            loadComponent: createSafeLazyLoad(
              'employees/:id/exit',
              () => import('./features/employees/employee-exit/employee-exit.component').then(m => m.EmployeeExitComponent)
            ),
            data: { roles: ['HR', 'Admin', 'SuperAdmin'] }
          }
        ]
      },
      {
        path: 'attendance',
        children: [
          {
            path: '',
            loadComponent: createSafeLazyLoad(
              'attendance',
              () => import('./features/attendance/attendance-tracker/attendance-tracker.component').then(m => m.AttendanceTrackerComponent)
            )
          },
          {
            path: 'now',
            loadComponent: createSafeLazyLoad(
              'attendance/now',
              () => import('./features/attendance/attendance-now/attendance-now.component').then(m => m.AttendanceNowComponent)
            ),
            data: { roles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] }
          },
          {
            path: 'calendar',
            loadComponent: createSafeLazyLoad(
              'attendance/calendar',
              () => import('./features/attendance/attendance-calendar/attendance-calendar.component').then(m => m.AttendanceCalendarComponent)
            ),
            data: { roles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] }
          },
          {
            path: 'reports',
            loadComponent: createSafeLazyLoad(
              'attendance/reports',
              () => import('./features/attendance/attendance-reports/attendance-reports.component').then(m => m.AttendanceReportsComponent)
            ),
            data: { roles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] }
          },
          {
            path: 'corrections',
            loadComponent: createSafeLazyLoad(
              'attendance/corrections',
              () => import('./features/attendance/attendance-corrections/attendance-corrections.component').then(m => m.AttendanceCorrectionsComponent)
            ),
            data: { roles: ['HR', 'Admin', 'SuperAdmin'] }
          }
        ]
      },
      {
        path: 'projects',
        children: [
          {
            path: '',
            loadComponent: createSafeLazyLoad(
              'projects',
              () => import('./features/projects/project-list/project-list.component').then(m => m.ProjectListComponent)
            )
          },
          {
            path: 'kanban',
            loadComponent: createSafeLazyLoad(
              'projects/kanban',
              () => import('./features/projects/kanban-board/kanban-board.component').then(m => m.KanbanBoardComponent)
            )
          },
          {
            path: 'monitoring',
            loadComponent: createSafeLazyLoad(
              'projects/monitoring',
              () => import('./features/projects/project-monitoring/project-monitoring.component').then(m => m.ProjectMonitoringComponent)
            ),
            data: { roles: ['Manager', 'Admin', 'SuperAdmin'] }
          }
        ]
      },
      {
        path: 'payroll',
        children: [
          {
            path: '',
            loadComponent: createSafeLazyLoad(
              'payroll',
              () => import('./features/payroll/payroll-list/payroll-list.component').then(m => m.PayrollListComponent)
            ),
            data: { roles: ['HR', 'Admin', 'Finance', 'SuperAdmin'] }
          },
          {
            path: 'processing',
            loadComponent: createSafeLazyLoad(
              'payroll/processing',
              () => import('./features/payroll/payroll-processing/payroll-processing.component').then(m => m.PayrollProcessingComponent)
            ),
            data: { roles: ['HR', 'Admin', 'Finance', 'SuperAdmin'] }
          },
          {
            path: 'approval',
            loadComponent: createSafeLazyLoad(
              'payroll/approval',
              () => import('./features/payroll/payroll-approval/payroll-approval.component').then(m => m.PayrollApprovalComponent)
            ),
            data: { roles: ['Admin', 'Finance', 'SuperAdmin'] }
          },
          {
            path: 'reports',
            loadComponent: createSafeLazyLoad(
              'payroll/reports',
              () => import('./features/payroll/payroll-reports/payroll-reports.component').then(m => m.PayrollReportsComponent)
            ),
            data: { roles: ['HR', 'Admin', 'Finance', 'SuperAdmin'] }
          }
        ]
      },
      {
        path: 'leave',
        children: [
          {
            path: '',
            loadComponent: createSafeLazyLoad(
              'leave',
              () => import('./features/leave/leave-list/leave-list.component').then(m => m.LeaveListComponent)
            )
          },
          {
            path: 'request',
            loadComponent: createSafeLazyLoad(
              'leave/request',
              () => import('./features/leave/leave-request-form/leave-request-form.component').then(m => m.LeaveRequestFormComponent)
            )
          },
          {
            path: 'balance',
            loadComponent: createSafeLazyLoad(
              'leave/balance',
              () => import('./features/leave/leave-balance/leave-balance.component').then(m => m.LeaveBalanceComponent)
            )
          },
          {
            path: 'calendar',
            loadComponent: createSafeLazyLoad(
              'leave/calendar',
              () => import('./features/leave/leave-calendar/leave-calendar.component').then(m => m.LeaveCalendarComponent)
            )
          }
        ]
      },
      {
        path: 'performance',
        children: [
          {
            path: '',
            loadComponent: createSafeLazyLoad(
              'performance',
              () => import('./features/performance/performance-list/performance-list.component').then(m => m.PerformanceListComponent)
            ),
            data: { roles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] }
          },
          {
            path: 'review',
            loadComponent: createSafeLazyLoad(
              'performance/review',
              () => import('./features/performance/performance-review/performance-review.component').then(m => m.PerformanceReviewComponent)
            ),
            data: { roles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] }
          },
          {
            path: 'pip',
            loadComponent: createSafeLazyLoad(
              'performance/pip',
              () => import('./features/performance/pip-management/pip-management.component').then(m => m.PIPManagementComponent)
            ),
            data: { roles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] }
          },
          {
            path: 'certifications',
            loadComponent: createSafeLazyLoad(
              'performance/certifications',
              () => import('./features/performance/certifications/certifications.component').then(m => m.CertificationsComponent)
            )
          }
        ]
      },
      {
        path: 'reports',
        children: [
          {
            path: '',
            loadComponent: createSafeLazyLoad(
              'reports',
              () => import('./features/reports/report-list/report-list.component').then(m => m.ReportListComponent)
            ),
            data: { roles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] }
          },
          {
            path: 'builder',
            loadComponent: createSafeLazyLoad(
              'reports/builder',
              () => import('./features/reports/report-builder/report-builder.component').then(m => m.ReportBuilderComponent)
            ),
            data: { roles: ['HR', 'Admin', 'SuperAdmin'] }
          },
          {
            path: 'analytics',
            loadComponent: createSafeLazyLoad(
              'reports/analytics',
              () => import('./features/reports/analytics-dashboard/analytics-dashboard.component').then(m => m.AnalyticsDashboardComponent)
            ),
            data: { roles: ['Admin', 'Manager', 'SuperAdmin'] }
          }
        ]
      },
      {
        path: 'settings',
        children: [
          {
            path: '',
            loadComponent: createSafeLazyLoad(
              'settings',
              () => import('./features/settings/settings.component').then(m => m.SettingsComponent)
            ),
            data: { roles: ['Admin', 'SuperAdmin'] }
          },
          {
            path: 'organization',
            loadComponent: createSafeLazyLoad(
              'settings/organization',
              () => import('./features/settings/organization-settings.component').then(m => m.OrganizationSettingsComponent)
            ),
            data: { roles: ['Admin', 'SuperAdmin'] }
          },
          {
            path: 'branches',
            loadComponent: createSafeLazyLoad(
              'settings/branches',
              () => import('./features/settings/branch-management.component').then(m => m.BranchManagementComponent)
            ),
            data: { roles: ['Admin', 'SuperAdmin'] }
          },
          {
            path: 'roles',
            loadComponent: createSafeLazyLoad(
              'settings/roles',
              () => import('./features/settings/role-management.component').then(m => m.RoleManagementComponent)
            ),
            data: { roles: ['Admin', 'SuperAdmin'] }
          },
          {
            path: 'system',
            loadComponent: createSafeLazyLoad(
              'settings/system',
              () => import('./features/settings/system-config.component').then(m => m.SystemConfigComponent)
            ),
            data: { roles: ['SuperAdmin'] }
          },
          {
            path: 'admin',
            loadComponent: createSafeLazyLoad(
              'settings/admin',
              () => import('./features/settings/admin-settings.component').then(m => m.AdminSettingsComponent)
            ),
            data: { roles: ['SuperAdmin'] }
          }
        ]
      },
      {
        path: 'training',
        children: [
          {
            path: '',
            loadComponent: createSafeLazyLoad(
              'training',
              () => import('./features/training/training-list/training-list.component').then(m => m.TrainingListComponent)
            )
          }
        ]
      },
      {
        path: 'profile',
        loadComponent: createSafeLazyLoad(
          'profile',
          () => import('./features/profile/profile.component').then(m => m.ProfileComponent)
        )
      },
      {
        path: 'navigation-test',
        loadComponent: createSafeLazyLoad(
          'navigation-test',
          () => import('./shared/components/navigation-test/navigation-test.component').then(m => m.NavigationTestComponent)
        ),
        data: { roles: ['Admin', 'SuperAdmin'] }
      }
    ]
  },

  // Error pages
  {
    path: 'unauthorized',
    loadComponent: createSafeLazyLoad(
      'unauthorized',
      () => import('./shared/components/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent)
    )
  },
  {
    path: 'route-error',
    loadComponent: createSafeLazyLoad(
      'route-error',
      () => import('./shared/components/route-error-page/route-error-page.component').then(m => m.RouteErrorPageComponent)
    )
  },

  // Wildcard route - must be last
  {
    path: '**',
    loadComponent: createSafeLazyLoad(
      'not-found',
      () => import('./shared/components/not-found/not-found.component').then(m => m.NotFoundComponent)
    )
  }
];