import { Routes } from '@angular/router';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
  },
  {
    path: 'users',
    loadComponent: () => import('./user-management/user-management.component').then(m => m.UserManagementComponent)
  },
  {
    path: 'system-logs',
    loadComponent: () => import('./system-logs/system-logs.component').then(m => m.SystemLogsComponent)
  },
  {
    path: 'audit-trail',
    loadComponent: () => import('./audit-trail/audit-trail.component').then(m => m.AuditTrailComponent)
  },
  {
    path: 'system-health',
    loadComponent: () => import('./system-health/system-health.component').then(m => m.SystemHealthComponent)
  },
  {
    path: 'backup-restore',
    loadComponent: () => import('./backup-restore/backup-restore.component').then(m => m.BackupRestoreComponent)
  }
];