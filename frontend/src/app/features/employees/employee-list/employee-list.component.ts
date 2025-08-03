import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-header">
      <h1>Employee Management</h1>
      <p class="text-muted">Manage your organization's employees</p>
    </div>
    
    <div class="card">
      <div class="card-body text-center py-5">
        <i class="fas fa-users text-muted mb-3" style="font-size: 3rem;"></i>
        <h3>Employee List</h3>
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
export class EmployeeListComponent {}