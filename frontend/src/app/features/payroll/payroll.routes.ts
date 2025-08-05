import { Routes } from '@angular/router';

export const PAYROLL_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./payroll-list/payroll-list.component').then(m => m.PayrollListComponent)
  },
  {
    path: 'processing',
    loadComponent: () => import('./payroll-processing/payroll-processing.component').then(m => m.PayrollProcessingComponent)
  },
  {
    path: 'approval',
    loadComponent: () => import('./payroll-approval/payroll-approval.component').then(m => m.PayrollApprovalComponent)
  },
  {
    path: 'reports',
    loadComponent: () => import('./payroll-reports/payroll-reports.component').then(m => m.PayrollReportsComponent)
  },
  {
    path: 'payslip-designer',
    loadComponent: () => import('./payslip-designer/payslip-designer.component').then(m => m.PayslipDesignerComponent)
  },
  {
    path: 'financial-analytics',
    loadComponent: () => import('./financial-analytics/financial-analytics.component').then(m => m.FinancialAnalyticsComponent)
  }
];