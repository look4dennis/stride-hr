import { Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./employee-list/employee-list.component').then(m => m.EmployeeListComponent)
  },
  {
    path: 'create',
    loadComponent: () => import('./employee-create/employee-create.component').then(m => m.EmployeeCreateComponent)
  },
  {
    path: 'profile/:id',
    loadComponent: () => import('./employee-profile/employee-profile.component').then(m => m.EmployeeProfileComponent)
  },
  {
    path: 'onboarding',
    loadComponent: () => import('./employee-onboarding/employee-onboarding.component').then(m => m.EmployeeOnboardingComponent)
  },
  {
    path: 'exit/:id',
    loadComponent: () => import('./employee-exit/employee-exit.component').then(m => m.EmployeeExitComponent)
  },
  {
    path: 'org-chart',
    loadComponent: () => import('./org-chart/org-chart.component').then(m => m.OrgChartComponent)
  }
];

export const EMPLOYEE_ROUTES = routes;
export default routes;