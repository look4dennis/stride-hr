import { Routes } from '@angular/router';

export const PROJECT_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./project-list/project-list.component').then(m => m.ProjectListComponent)
  },
  {
    path: 'kanban',
    loadComponent: () => import('./kanban-board/kanban-board.component').then(m => m.KanbanBoardComponent)
  },
  {
    path: 'collaboration',
    loadComponent: () => import('./project-collaboration/project-collaboration.component').then(m => m.ProjectCollaborationComponent)
  },
  {
    path: 'monitoring',
    loadComponent: () => import('./project-monitoring/project-monitoring.component').then(m => m.ProjectMonitoringComponent)
  },
  {
    path: 'progress/:id',
    loadComponent: () => import('./project-progress/project-progress.component').then(m => m.ProjectProgressComponent)
  }
];