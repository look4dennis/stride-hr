import { Routes } from '@angular/router';
import { AuthGuard } from './core/auth/auth.guard';
import { LayoutComponent } from './shared/components/layout/layout.component';
import { LoginComponent } from './features/auth/login/login.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';

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
        loadComponent: () => import('./features/employees/employee-list').then(m => m.EmployeeListComponent),
        data: { roles: ['HR', 'Admin', 'Manager'] }
      },
      {
        path: 'attendance',
        loadComponent: () => import('./features/attendance/attendance-tracker').then(m => m.AttendanceTrackerComponent)
      },
      {
        path: 'projects',
        loadComponent: () => import('./features/projects/project-list').then(m => m.ProjectListComponent)
      },
      {
        path: 'payroll',
        loadComponent: () => import('./features/payroll/payroll-list').then(m => m.PayrollListComponent),
        data: { roles: ['HR', 'Admin', 'Finance'] }
      },
      {
        path: 'leave',
        loadComponent: () => import('./features/leave/leave-list').then(m => m.LeaveListComponent)
      },
      {
        path: 'performance',
        loadComponent: () => import('./features/performance/performance-list').then(m => m.PerformanceListComponent),
        data: { roles: ['HR', 'Admin', 'Manager'] }
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/report-list').then(m => m.ReportListComponent),
        data: { roles: ['HR', 'Admin', 'Manager'] }
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings').then(m => m.SettingsComponent),
        data: { roles: ['Admin'] }
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile').then(m => m.ProfileComponent)
      }
    ]
  },

  // Unauthorized page
  {
    path: 'unauthorized',
    loadComponent: () => import('./shared/components/unauthorized').then(m => m.UnauthorizedComponent)
  },

  // Wildcard route - must be last
  {
    path: '**',
    loadComponent: () => import('./shared/components/not-found').then(m => m.NotFoundComponent)
  }
];