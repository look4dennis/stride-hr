import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService, User } from '../../core/auth/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dashboard-container">
      <div class="row">
        <div class="col-12">
          <div class="welcome-card">
            <div class="card-body">
              <h1 class="card-title">
                Welcome back, {{ currentUser?.firstName }}!
              </h1>
              <p class="card-text text-muted">
                Here's what's happening in your organization today.
              </p>
            </div>
          </div>
        </div>
      </div>
      
      <div class="row mt-4">
        <div class="col-md-3 col-sm-6 mb-4">
          <div class="dashboard-widget">
            <div class="widget-icon bg-primary">
              <i class="fas fa-users"></i>
            </div>
            <div class="widget-content">
              <h3 class="widget-value">150</h3>
              <p class="widget-label">Total Employees</p>
            </div>
          </div>
        </div>
        
        <div class="col-md-3 col-sm-6 mb-4">
          <div class="dashboard-widget">
            <div class="widget-icon bg-success">
              <i class="fas fa-clock"></i>
            </div>
            <div class="widget-content">
              <h3 class="widget-value">142</h3>
              <p class="widget-label">Present Today</p>
            </div>
          </div>
        </div>
        
        <div class="col-md-3 col-sm-6 mb-4">
          <div class="dashboard-widget">
            <div class="widget-icon bg-warning">
              <i class="fas fa-project-diagram"></i>
            </div>
            <div class="widget-content">
              <h3 class="widget-value">23</h3>
              <p class="widget-label">Active Projects</p>
            </div>
          </div>
        </div>
        
        <div class="col-md-3 col-sm-6 mb-4">
          <div class="dashboard-widget">
            <div class="widget-icon bg-info">
              <i class="fas fa-calendar-alt"></i>
            </div>
            <div class="widget-content">
              <h3 class="widget-value">8</h3>
              <p class="widget-label">Pending Leaves</p>
            </div>
          </div>
        </div>
      </div>
      
      <div class="row">
        <div class="col-lg-8 mb-4">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">Recent Activities</h5>
            </div>
            <div class="card-body">
              <div class="activity-item">
                <div class="activity-icon bg-success">
                  <i class="fas fa-user-plus"></i>
                </div>
                <div class="activity-content">
                  <p class="mb-1"><strong>John Doe</strong> joined the Development team</p>
                  <small class="text-muted">2 hours ago</small>
                </div>
              </div>
              
              <div class="activity-item">
                <div class="activity-icon bg-primary">
                  <i class="fas fa-project-diagram"></i>
                </div>
                <div class="activity-content">
                  <p class="mb-1">New project <strong>"Mobile App Redesign"</strong> created</p>
                  <small class="text-muted">4 hours ago</small>
                </div>
              </div>
              
              <div class="activity-item">
                <div class="activity-icon bg-warning">
                  <i class="fas fa-calendar-alt"></i>
                </div>
                <div class="activity-content">
                  <p class="mb-1"><strong>Jane Smith</strong> requested leave for next week</p>
                  <small class="text-muted">6 hours ago</small>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <div class="col-lg-4 mb-4">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">Quick Actions</h5>
            </div>
            <div class="card-body">
              <div class="d-grid gap-2">
                <button class="btn btn-primary">
                  <i class="fas fa-clock me-2"></i>
                  Check In/Out
                </button>
                <button class="btn btn-outline-primary">
                  <i class="fas fa-calendar-plus me-2"></i>
                  Request Leave
                </button>
                <button class="btn btn-outline-primary">
                  <i class="fas fa-file-alt me-2"></i>
                  Submit DSR
                </button>
                <button class="btn btn-outline-primary">
                  <i class="fas fa-chart-bar me-2"></i>
                  View Reports
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 0;
    }

    .welcome-card {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      color: white;
      border: none;
      border-radius: 16px;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    }

    .welcome-card .card-title {
      font-size: 2rem;
      font-weight: 700;
      margin-bottom: 0.5rem;
    }

    .dashboard-widget {
      background: white;
      border-radius: 16px;
      padding: 1.5rem;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
      border: 1px solid var(--gray-100);
      transition: all 0.2s ease-in-out;
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .dashboard-widget:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
    }

    .widget-icon {
      width: 60px;
      height: 60px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 1.5rem;
    }

    .widget-content {
      flex: 1;
    }

    .widget-value {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 0.25rem;
    }

    .widget-label {
      color: var(--text-secondary);
      margin-bottom: 0;
      font-weight: 500;
    }

    .card {
      border: none;
      border-radius: 16px;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      background: var(--bg-secondary);
      border-bottom: 1px solid var(--gray-200);
      border-radius: 16px 16px 0 0 !important;
      padding: 1.25rem 1.5rem;
    }

    .card-title {
      font-weight: 600;
      color: var(--text-primary);
    }

    .activity-item {
      display: flex;
      align-items: flex-start;
      gap: 1rem;
      padding: 1rem 0;
      border-bottom: 1px solid var(--gray-100);
    }

    .activity-item:last-child {
      border-bottom: none;
    }

    .activity-icon {
      width: 40px;
      height: 40px;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 1rem;
      flex-shrink: 0;
    }

    .activity-content {
      flex: 1;
    }

    .btn {
      border-radius: 8px;
      font-weight: 500;
      padding: 0.75rem 1.25rem;
    }

    .btn-primary {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      border: none;
    }

    .btn-primary:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(59, 130, 246, 0.4);
    }

    .btn-outline-primary {
      border: 2px solid var(--primary);
      color: var(--primary);
    }

    .btn-outline-primary:hover {
      background: var(--primary);
      border-color: var(--primary);
      transform: translateY(-1px);
    }
  `]
})
export class DashboardComponent implements OnInit {
  currentUser: User | null = null;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.currentUser = this.authService.currentUser;
  }
}