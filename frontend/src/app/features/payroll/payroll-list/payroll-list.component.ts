import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-payroll-list',
    imports: [CommonModule],
    template: `
    <div class="page-header">
      <h1>Payroll Management</h1>
      <p class="text-muted">Process payroll and manage employee compensation</p>
    </div>
    
    <div class="card">
      <div class="card-body text-center py-5">
        <i class="fas fa-money-bill-wave text-muted mb-3" style="font-size: 3rem;"></i>
        <h3>Payroll System</h3>
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
export class PayrollListComponent {}