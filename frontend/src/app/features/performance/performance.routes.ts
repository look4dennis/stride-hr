import { Routes } from '@angular/router';

export const PERFORMANCE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./performance-list/performance-list.component').then(m => m.PerformanceListComponent)
  },
  {
    path: 'review/:id',
    loadComponent: () => import('./performance-review/performance-review.component').then(m => m.PerformanceReviewComponent)
  },
  {
    path: 'pip-management',
    loadComponent: () => import('./pip-management/pip-management.component').then(m => m.PipManagementComponent)
  },
  {
    path: 'training-modules',
    loadComponent: () => import('./training-modules/training-modules.component').then(m => m.TrainingModulesComponent)
  },
  {
    path: 'certifications',
    loadComponent: () => import('./certifications/certifications.component').then(m => m.CertificationsComponent)
  }
];