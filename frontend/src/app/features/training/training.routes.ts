import { Routes } from '@angular/router';

export const TRAINING_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./training-list/training-list.component').then(m => m.TrainingListComponent)
  },
  {
    path: 'modules',
    loadComponent: () => import('./training-modules/training-modules.component').then(m => m.TrainingModulesComponent)
  },
  {
    path: 'assignments',
    loadComponent: () => import('./training-assignments/training-assignments.component').then(m => m.TrainingAssignmentsComponent)
  },
  {
    path: 'progress',
    loadComponent: () => import('./training-progress/training-progress.component').then(m => m.TrainingProgressComponent)
  },
  {
    path: 'assessments',
    loadComponent: () => import('./training-assessments/training-assessments.component').then(m => m.TrainingAssessmentsComponent)
  }
];