import { Routes } from '@angular/router';

export const SETTINGS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./settings.component').then(m => m.SettingsComponent)
  },
  {
    path: 'organization',
    loadComponent: () => import('./organization-settings.component').then(m => m.OrganizationSettingsComponent)
  },
  {
    path: 'branches',
    loadComponent: () => import('./branch-management.component').then(m => m.BranchManagementComponent)
  },
  {
    path: 'roles',
    loadComponent: () => import('./role-management.component').then(m => m.RoleManagementComponent)
  },
  {
    path: 'system',
    loadComponent: () => import('./system-config.component').then(m => m.SystemConfigComponent)
  }
];