import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-settings',
    imports: [CommonModule],
    template: `
    <div class="page-header">
      <h1>System Settings</h1>
      <p class="text-muted">Configure system settings and preferences</p>
    </div>
    
    <div class="card">
      <div class="card-body text-center py-5">
        <i class="fas fa-cog text-muted mb-3" style="font-size: 3rem;"></i>
        <h3>Settings</h3>
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
export class SettingsComponent {}