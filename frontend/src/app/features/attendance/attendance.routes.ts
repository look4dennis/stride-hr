import { Routes } from '@angular/router';

export const ATTENDANCE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./attendance-tracker/attendance-tracker.component').then(m => m.AttendanceTrackerComponent)
  },
  {
    path: 'now',
    loadComponent: () => import('./attendance-now/attendance-now.component').then(m => m.AttendanceNowComponent)
  },
  {
    path: 'calendar',
    loadComponent: () => import('./attendance-calendar/attendance-calendar.component').then(m => m.AttendanceCalendarComponent)
  },
  {
    path: 'reports',
    loadComponent: () => import('./attendance-reports/attendance-reports.component').then(m => m.AttendanceReportsComponent)
  },
  {
    path: 'corrections',
    loadComponent: () => import('./attendance-corrections/attendance-corrections.component').then(m => m.AttendanceCorrectionsComponent)
  }
];