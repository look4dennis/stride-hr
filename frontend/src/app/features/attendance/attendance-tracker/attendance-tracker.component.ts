import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-attendance-tracker',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-header">
      <h1>Attendance Tracking</h1>
      <p class="text-muted">Track your daily attendance and working hours</p>
    </div>
    
    <div class="card">
      <div class="card-body text-center py-5">
        <i class="fas fa-clock text-muted mb-3" style="font-size: 3rem;"></i>
        <h3>Attendance Tracker</h3>
        <p class="text-muted">This feature will be implemented in upcoming tasks.</p>
      </div>
    </div>
  `,
  styles: [`
    .page-header {
      margin-bottom: 2rem;
    }
    
    .page-header h1 {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }
  `]
})
export class AttendanceTrackerComponent {}