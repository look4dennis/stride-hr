import { Routes } from '@angular/router';

export const REPORTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./report-list/report-list.component').then(m => m.ReportListComponent)
  },
  {
    path: 'builder',
    loadComponent: () => import('./report-builder/report-builder.component').then(m => m.ReportBuilderComponent)
  },
  {
    path: 'analytics',
    loadComponent: () => import('./analytics-dashboard/analytics-dashboard.component').then(m => m.AnalyticsDashboardComponent)
  },
  {
    path: 'visualization',
    loadComponent: () => import('./data-visualization/data-visualization.component').then(m => m.DataVisualizationComponent)
  }
];