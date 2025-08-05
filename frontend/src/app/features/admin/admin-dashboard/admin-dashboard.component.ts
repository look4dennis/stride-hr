import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="admin-dashboard">
      <h2>Admin Dashboard</h2>
      <p>Administrative dashboard content will be implemented here.</p>
    </div>
  `,
  styles: [`
    .admin-dashboard {
      padding: 20px;
    }
  `]
})
export class AdminDashboardComponent {
}