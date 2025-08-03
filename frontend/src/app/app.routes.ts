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
        loadComponent: () => import('./features/employees/employee-list/employee-list.component').then(m => m.EmployeeListComponent),
        data: { roles: ['HR', 'Admin', 'Manager'] }
      },
      {
        path: 'attendance',
        loadComponent: () => import('./features/attendance/attendance-tracker/attendance-tracker.component').then(m => m.AttendanceTrackerComponent)
      },
      {
        path: 'projects',
        loadComponent: () => import('./features/projects/project-list/project-list.component').then(m => m.ProjectListComponent)
      },
      {
        path: 'payroll',
        loadComponent: () => import('./features/payroll/payroll-list/payroll-list.component').then(m => m.PayrollListComponent),
        data: { roles: ['HR', 'Admin', 'Finance'] }
      },
      {
        path: 'leave',
        loadComponent: () => import('./features/leave/leave-list/leave-list.component').then(m => m.LeaveListComponent)
      },
      {
        path: 'performance',
        loadComponent: () => import('./features/performance/performance-list/performance-list.component').then(m => m.PerformanceListComponent),
        data: { roles: ['HR', 'Admin', 'Manager'] }
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/report-list/report-list.component').then(m => m.ReportListComponent),
        data: { roles: ['HR', 'Admin', 'Manager'] }
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent),
        data: { roles: ['Admin'] }
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent)
      }
    ]
  },

  // Unauthorized page
  {
    path: 'unauthorized',
    loadComponent: () => import('./shared/components/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent)
  },

  // Wildcard route - must be last
  {
    path: '**',
    loadComponent: () => import('./shared/components/not-found/not-found.component').then(m => m.NotFoundComponent)
  }
];