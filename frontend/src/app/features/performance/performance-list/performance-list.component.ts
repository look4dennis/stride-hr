import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
    selector: 'app-performance-list',
    imports: [CommonModule],
    template: `
    <div class="container-fluid">
      <div class="page-header mb-4">
        <h1 class="h3 mb-0">Performance Management</h1>
        <p class="text-muted">Comprehensive performance tracking and development tools</p>
      </div>
      
      <div class="row">
        <!-- Performance Reviews Card -->
        <div class="col-lg-3 col-md-6 mb-4">
          <div class="card h-100 performance-card" (click)="navigateTo('/performance/reviews')">
            <div class="card-body text-center">
              <div class="performance-icon mb-3">
                <i class="fas fa-star text-primary" style="font-size: 2.5rem;"></i>
              </div>
              <h5 class="card-title">Performance Reviews</h5>
              <p class="card-text text-muted">Conduct 360-degree performance evaluations with goal tracking</p>
              <div class="mt-auto">
                <span class="badge bg-primary">Reviews & Goals</span>
              </div>
            </div>
            <div class="card-footer bg-transparent">
              <small class="text-muted">
                <i class="fas fa-users me-1"></i>Employee Evaluations
              </small>
            </div>
          </div>
        </div>

        <!-- PIP Management Card -->
        <div class="col-lg-3 col-md-6 mb-4">
          <div class="card h-100 performance-card" (click)="navigateTo('/performance/pips')">
            <div class="card-body text-center">
              <div class="performance-icon mb-3">
                <i class="fas fa-clipboard-list text-warning" style="font-size: 2.5rem;"></i>
              </div>
              <h5 class="card-title">Performance Improvement Plans</h5>
              <p class="card-text text-muted">Create and track PIPs with milestones and progress monitoring</p>
              <div class="mt-auto">
                <span class="badge bg-warning">Improvement Plans</span>
              </div>
            </div>
            <div class="card-footer bg-transparent">
              <small class="text-muted">
                <i class="fas fa-chart-line me-1"></i>Progress Tracking
              </small>
            </div>
          </div>
        </div>

        <!-- Training Modules Card -->
        <div class="col-lg-3 col-md-6 mb-4">
          <div class="card h-100 performance-card" (click)="navigateTo('/performance/training/modules')">
            <div class="card-body text-center">
              <div class="performance-icon mb-3">
                <i class="fas fa-graduation-cap text-success" style="font-size: 2.5rem;"></i>
              </div>
              <h5 class="card-title">Training Modules</h5>
              <p class="card-text text-muted">Create training content with assessments and track employee progress</p>
              <div class="mt-auto">
                <span class="badge bg-success">Learning & Development</span>
              </div>
            </div>
            <div class="card-footer bg-transparent">
              <small class="text-muted">
                <i class="fas fa-book me-1"></i>Training Content
              </small>
            </div>
          </div>
        </div>

        <!-- Certifications Card -->
        <div class="col-lg-3 col-md-6 mb-4">
          <div class="card h-100 performance-card" (click)="navigateTo('/performance/certifications')">
            <div class="card-body text-center">
              <div class="performance-icon mb-3">
                <i class="fas fa-certificate text-info" style="font-size: 2.5rem;"></i>
              </div>
              <h5 class="card-title">Certifications</h5>
              <p class="card-text text-muted">Track employee certifications and training completion status</p>
              <div class="mt-auto">
                <span class="badge bg-info">Achievements</span>
              </div>
            </div>
            <div class="card-footer bg-transparent">
              <small class="text-muted">
                <i class="fas fa-award me-1"></i>Certificates & Progress
              </small>
            </div>
          </div>
        </div>
      </div>

      <!-- Quick Stats Row -->
      <div class="row mt-4">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">Performance Overview</h5>
            </div>
            <div class="card-body">
              <div class="row text-center">
                <div class="col-md-3">
                  <div class="stat-item">
                    <div class="stat-number text-primary">0</div>
                    <div class="stat-label">Active Reviews</div>
                  </div>
                </div>
                <div class="col-md-3">
                  <div class="stat-item">
                    <div class="stat-number text-warning">0</div>
                    <div class="stat-label">Active PIPs</div>
                  </div>
                </div>
                <div class="col-md-3">
                  <div class="stat-item">
                    <div class="stat-number text-success">0</div>
                    <div class="stat-label">Training Modules</div>
                  </div>
                </div>
                <div class="col-md-3">
                  <div class="stat-item">
                    <div class="stat-number text-info">0</div>
                    <div class="stat-label">Certifications Issued</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Activity -->
      <div class="row mt-4">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">Recent Activity</h5>
            </div>
            <div class="card-body">
              <div class="text-center py-4">
                <i class="fas fa-clock text-muted mb-3" style="font-size: 2rem;"></i>
                <p class="text-muted">No recent performance activities</p>
                <small class="text-muted">Activities will appear here as employees engage with performance management features</small>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .page-header h1 {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
    }
    
    .performance-card {
      cursor: pointer;
      transition: all 0.3s ease;
      border: 1px solid #e9ecef;
    }
    
    .performance-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
      border-color: var(--primary);
    }
    
    .performance-icon {
      height: 80px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    
    .card-title {
      color: var(--text-primary);
      font-weight: 600;
      margin-bottom: 1rem;
    }
    
    .card-text {
      font-size: 0.9rem;
      line-height: 1.5;
      min-height: 3rem;
    }
    
    .badge {
      font-size: 0.75rem;
      padding: 0.5rem 0.75rem;
    }
    
    .stat-item {
      padding: 1rem;
    }
    
    .stat-number {
      font-size: 2rem;
      font-weight: 700;
      margin-bottom: 0.5rem;
    }
    
    .stat-label {
      font-size: 0.9rem;
      color: var(--text-secondary);
      font-weight: 500;
    }
    
    .card-footer {
      border-top: 1px solid #e9ecef;
      padding: 0.75rem 1.25rem;
    }
  `]
})
export class PerformanceListComponent {
  constructor(private router: Router) {}

  navigateTo(route: string) {
    this.router.navigate([route]);
  }
}