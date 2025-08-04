import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-admin-settings',
  imports: [CommonModule, RouterModule],
  template: `
    <div class="admin-settings-container">
      <div class="page-header">
        <h1>Administrative Settings</h1>
        <p class="text-muted">Manage organization, branches, roles, and system configuration</p>
      </div>

      <div class="row g-4">
        <!-- Organization Management -->
        <div class="col-md-6 col-lg-4">
          <div class="settings-card" routerLink="/settings/organization">
            <div class="settings-card-icon">
              <i class="fas fa-building text-primary"></i>
            </div>
            <div class="settings-card-content">
              <h3>Organization Settings</h3>
              <p>Configure organization details, logo, and global settings</p>
            </div>
            <div class="settings-card-arrow">
              <i class="fas fa-chevron-right"></i>
            </div>
          </div>
        </div>

        <!-- Branch Management -->
        <div class="col-md-6 col-lg-4">
          <div class="settings-card" routerLink="/settings/branches">
            <div class="settings-card-icon">
              <i class="fas fa-map-marker-alt text-success"></i>
            </div>
            <div class="settings-card-content">
              <h3>Branch Management</h3>
              <p>Manage branches, locations, and regional settings</p>
            </div>
            <div class="settings-card-arrow">
              <i class="fas fa-chevron-right"></i>
            </div>
          </div>
        </div>

        <!-- Role & Permission Management -->
        <div class="col-md-6 col-lg-4">
          <div class="settings-card" routerLink="/settings/roles">
            <div class="settings-card-icon">
              <i class="fas fa-users-cog text-warning"></i>
            </div>
            <div class="settings-card-content">
              <h3>Roles & Permissions</h3>
              <p>Configure user roles and access permissions</p>
            </div>
            <div class="settings-card-arrow">
              <i class="fas fa-chevron-right"></i>
            </div>
          </div>
        </div>

        <!-- System Configuration -->
        <div class="col-md-6 col-lg-4">
          <div class="settings-card" routerLink="/settings/system">
            <div class="settings-card-icon">
              <i class="fas fa-cogs text-info"></i>
            </div>
            <div class="settings-card-content">
              <h3>System Configuration</h3>
              <p>Configure system-wide settings and preferences</p>
            </div>
            <div class="settings-card-arrow">
              <i class="fas fa-chevron-right"></i>
            </div>
          </div>
        </div>

        <!-- Security Settings -->
        <div class="col-md-6 col-lg-4">
          <div class="settings-card" routerLink="/settings/security">
            <div class="settings-card-icon">
              <i class="fas fa-shield-alt text-danger"></i>
            </div>
            <div class="settings-card-content">
              <h3>Security Settings</h3>
              <p>Configure authentication and security policies</p>
            </div>
            <div class="settings-card-arrow">
              <i class="fas fa-chevron-right"></i>
            </div>
          </div>
        </div>

        <!-- Integration Settings -->
        <div class="col-md-6 col-lg-4">
          <div class="settings-card" routerLink="/settings/integrations">
            <div class="settings-card-icon">
              <i class="fas fa-plug text-secondary"></i>
            </div>
            <div class="settings-card-content">
              <h3>Integrations</h3>
              <p>Configure third-party integrations and APIs</p>
            </div>
            <div class="settings-card-arrow">
              <i class="fas fa-chevron-right"></i>
            </div>
          </div>
        </div>
      </div>

      <!-- Quick Stats -->
      <div class="row mt-5">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">System Overview</h5>
            </div>
            <div class="card-body">
              <div class="row g-4">
                <div class="col-md-3">
                  <div class="stat-item">
                    <div class="stat-value">{{ systemStats.totalOrganizations }}</div>
                    <div class="stat-label">Organizations</div>
                  </div>
                </div>
                <div class="col-md-3">
                  <div class="stat-item">
                    <div class="stat-value">{{ systemStats.totalBranches }}</div>
                    <div class="stat-label">Branches</div>
                  </div>
                </div>
                <div class="col-md-3">
                  <div class="stat-item">
                    <div class="stat-value">{{ systemStats.totalRoles }}</div>
                    <div class="stat-label">Active Roles</div>
                  </div>
                </div>
                <div class="col-md-3">
                  <div class="stat-item">
                    <div class="stat-value">{{ systemStats.totalUsers }}</div>
                    <div class="stat-label">System Users</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .admin-settings-container {
      padding: 2rem;
    }

    .page-header {
      margin-bottom: 2rem;
    }

    .page-header h1 {
      font-size: 2.5rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .settings-card {
      background: white;
      border: 1px solid var(--gray-200);
      border-radius: 12px;
      padding: 1.5rem;
      cursor: pointer;
      transition: all 0.2s ease-in-out;
      display: flex;
      align-items: center;
      gap: 1rem;
      height: 100%;
      text-decoration: none;
      color: inherit;
    }

    .settings-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
      text-decoration: none;
      color: inherit;
    }

    .settings-card-icon {
      flex-shrink: 0;
    }

    .settings-card-icon i {
      font-size: 2.5rem;
    }

    .settings-card-content {
      flex: 1;
    }

    .settings-card-content h3 {
      font-size: 1.25rem;
      font-weight: 600;
      margin-bottom: 0.5rem;
      color: var(--text-primary);
    }

    .settings-card-content p {
      color: var(--text-secondary);
      margin-bottom: 0;
      font-size: 0.9rem;
    }

    .settings-card-arrow {
      flex-shrink: 0;
      color: var(--gray-400);
    }

    .stat-item {
      text-align: center;
    }

    .stat-value {
      font-size: 2rem;
      font-weight: 700;
      color: var(--primary);
      margin-bottom: 0.5rem;
    }

    .stat-label {
      color: var(--text-secondary);
      font-size: 0.9rem;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .card {
      border: 1px solid var(--gray-200);
      border-radius: 12px;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-tertiary) 100%);
      border-bottom: 1px solid var(--gray-200);
      padding: 1.25rem 1.5rem;
      border-radius: 12px 12px 0 0;
    }

    .card-title {
      font-weight: 600;
      color: var(--text-primary);
    }
  `]
})
export class AdminSettingsComponent implements OnInit {
  systemStats = {
    totalOrganizations: 1,
    totalBranches: 3,
    totalRoles: 8,
    totalUsers: 125
  };

  ngOnInit(): void {
    this.loadSystemStats();
  }

  private loadSystemStats(): void {
    // In a real implementation, this would load actual stats from the API
    // For now, we'll use mock data
  }
}