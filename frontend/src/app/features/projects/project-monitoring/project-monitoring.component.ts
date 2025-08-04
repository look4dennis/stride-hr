import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProjectService } from '../../../services/project.service';
import { ProjectAnalytics, ProjectDashboard, ProjectHoursReport, ProjectAlert, ProjectRisk } from '../../../models/project.models';
import { Subject, takeUntil, interval, firstValueFrom } from 'rxjs';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-project-monitoring',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbModule],
  template: `
    <div class="project-monitoring-container">
      <!-- Header -->
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="mb-0">
          <i class="fas fa-chart-line text-primary me-2"></i>
          Project Monitoring Dashboard
        </h2>
        <div class="d-flex gap-2">
          <button class="btn btn-outline-primary" (click)="refreshData()">
            <i class="fas fa-sync-alt" [class.fa-spin]="isLoading"></i>
            Refresh
          </button>
          <div class="dropdown">
            <button class="btn btn-outline-secondary dropdown-toggle" type="button" data-bs-toggle="dropdown">
              <i class="fas fa-filter me-1"></i>
              Filter
            </button>
            <ul class="dropdown-menu">
              <li><a class="dropdown-item" href="#" (click)="setDateRange('today')">Today</a></li>
              <li><a class="dropdown-item" href="#" (click)="setDateRange('week')">This Week</a></li>
              <li><a class="dropdown-item" href="#" (click)="setDateRange('month')">This Month</a></li>
              <li><a class="dropdown-item" href="#" (click)="setDateRange('quarter')">This Quarter</a></li>
            </ul>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div *ngIf="isLoading" class="text-center py-5">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <p class="mt-2 text-muted">Loading project monitoring data...</p>
      </div>

      <!-- Dashboard Content -->
      <div *ngIf="!isLoading && dashboard" class="row">
        <!-- Team Overview Cards -->
        <div class="col-12 mb-4">
          <div class="row g-3">
            <div class="col-md-3">
              <div class="card bg-primary text-white h-100">
                <div class="card-body">
                  <div class="d-flex justify-content-between">
                    <div>
                      <h6 class="card-title">Total Projects</h6>
                      <h3 class="mb-0">{{ dashboard.teamOverview.totalProjects }}</h3>
                    </div>
                    <i class="fas fa-project-diagram fa-2x opacity-75"></i>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-md-3">
              <div class="card bg-success text-white h-100">
                <div class="card-body">
                  <div class="d-flex justify-content-between">
                    <div>
                      <h6 class="card-title">Active Projects</h6>
                      <h3 class="mb-0">{{ dashboard.teamOverview.activeProjects }}</h3>
                    </div>
                    <i class="fas fa-play-circle fa-2x opacity-75"></i>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-md-3">
              <div class="card bg-warning text-white h-100">
                <div class="card-body">
                  <div class="d-flex justify-content-between">
                    <div>
                      <h6 class="card-title">Delayed Projects</h6>
                      <h3 class="mb-0">{{ dashboard.teamOverview.delayedProjects }}</h3>
                    </div>
                    <i class="fas fa-exclamation-triangle fa-2x opacity-75"></i>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-md-3">
              <div class="card bg-info text-white h-100">
                <div class="card-body">
                  <div class="d-flex justify-content-between">
                    <div>
                      <h6 class="card-title">Team Members</h6>
                      <h3 class="mb-0">{{ dashboard.teamOverview.totalTeamMembers }}</h3>
                    </div>
                    <i class="fas fa-users fa-2x opacity-75"></i>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Critical Alerts -->
        <div class="col-md-6 mb-4" *ngIf="dashboard.criticalAlerts.length > 0">
          <div class="card h-100">
            <div class="card-header bg-danger text-white">
              <h5 class="mb-0">
                <i class="fas fa-exclamation-circle me-2"></i>
                Critical Alerts
              </h5>
            </div>
            <div class="card-body p-0">
              <div class="list-group list-group-flush">
                <div *ngFor="let alert of dashboard.criticalAlerts.slice(0, 5)" 
                     class="list-group-item d-flex justify-content-between align-items-start">
                  <div class="ms-2 me-auto">
                    <div class="fw-bold">{{ alert.alertType }}</div>
                    <small class="text-muted">{{ alert.message }}</small>
                    <br>
                    <small class="text-muted">{{ alert.createdAt | date:'short' }}</small>
                  </div>
                  <span class="badge bg-danger rounded-pill">{{ alert.severity }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- High Risks -->
        <div class="col-md-6 mb-4" *ngIf="dashboard.highRisks.length > 0">
          <div class="card h-100">
            <div class="card-header bg-warning text-dark">
              <h5 class="mb-0">
                <i class="fas fa-shield-alt me-2"></i>
                High Risks
              </h5>
            </div>
            <div class="card-body p-0">
              <div class="list-group list-group-flush">
                <div *ngFor="let risk of dashboard.highRisks.slice(0, 5)" 
                     class="list-group-item d-flex justify-content-between align-items-start">
                  <div class="ms-2 me-auto">
                    <div class="fw-bold">{{ risk.riskType }}</div>
                    <small class="text-muted">{{ risk.description }}</small>
                    <br>
                    <small class="text-muted">Impact: {{ risk.impact }}, Probability: {{ risk.probability }}</small>
                  </div>
                  <span class="badge bg-warning rounded-pill">{{ risk.severity }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Project Analytics -->
        <div class="col-12 mb-4">
          <div class="card">
            <div class="card-header">
              <h5 class="mb-0">
                <i class="fas fa-analytics me-2"></i>
                Project Analytics
              </h5>
            </div>
            <div class="card-body">
              <div class="row" *ngIf="dashboard.projectAnalytics.length > 0">
                <div class="col-md-6 col-lg-4 mb-3" *ngFor="let project of dashboard.projectAnalytics">
                  <div class="card border-start border-4" 
                       [class.border-success]="project.performance.overallEfficiency >= 80"
                       [class.border-warning]="project.performance.overallEfficiency >= 60 && project.performance.overallEfficiency < 80"
                       [class.border-danger]="project.performance.overallEfficiency < 60">
                    <div class="card-body">
                      <h6 class="card-title">{{ project.projectName }}</h6>
                      <div class="row g-2">
                        <div class="col-6">
                          <small class="text-muted">Completion</small>
                          <div class="progress" style="height: 8px;">
                            <div class="progress-bar" 
                                 [style.width.%]="project.metrics.completionPercentage"
                                 [class.bg-success]="project.metrics.completionPercentage >= 80"
                                 [class.bg-warning]="project.metrics.completionPercentage >= 60 && project.metrics.completionPercentage < 80"
                                 [class.bg-danger]="project.metrics.completionPercentage < 60">
                            </div>
                          </div>
                          <small>{{ project.metrics.completionPercentage | number:'1.1-1' }}%</small>
                        </div>
                        <div class="col-6">
                          <small class="text-muted">Efficiency</small>
                          <div class="progress" style="height: 8px;">
                            <div class="progress-bar bg-info" 
                                 [style.width.%]="project.performance.overallEfficiency">
                            </div>
                          </div>
                          <small>{{ project.performance.overallEfficiency | number:'1.1-1' }}%</small>
                        </div>
                      </div>
                      <div class="mt-2">
                        <small class="text-muted">
                          {{ project.metrics.completedTasks }}/{{ project.metrics.totalTasks }} tasks completed
                        </small>
                      </div>
                      <div class="mt-1">
                        <span class="badge" 
                              [class.bg-success]="project.performance.performanceGrade.startsWith('A')"
                              [class.bg-warning]="project.performance.performanceGrade.startsWith('B')"
                              [class.bg-danger]="project.performance.performanceGrade.startsWith('C') || project.performance.performanceGrade.startsWith('D')">
                          Grade: {{ project.performance.performanceGrade }}
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div *ngIf="dashboard.projectAnalytics.length === 0" class="text-center py-4">
                <i class="fas fa-chart-bar fa-3x text-muted mb-3"></i>
                <p class="text-muted">No project analytics available</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Hours Tracking -->
        <div class="col-12 mb-4">
          <div class="card">
            <div class="card-header">
              <h5 class="mb-0">
                <i class="fas fa-clock me-2"></i>
                Hours Tracking Summary
              </h5>
            </div>
            <div class="card-body">
              <div class="row" *ngIf="hoursReports.length > 0">
                <div class="col-md-6 col-lg-4 mb-3" *ngFor="let report of hoursReports">
                  <div class="card bg-light">
                    <div class="card-body">
                      <h6 class="card-title">{{ report.projectName }}</h6>
                      <div class="d-flex justify-content-between mb-2">
                        <span>Estimated Hours:</span>
                        <strong>{{ report.estimatedHours }}</strong>
                      </div>
                      <div class="d-flex justify-content-between mb-2">
                        <span>Actual Hours:</span>
                        <strong>{{ report.totalHoursWorked | number:'1.1-1' }}</strong>
                      </div>
                      <div class="d-flex justify-content-between mb-2">
                        <span>Variance:</span>
                        <strong [class.text-success]="report.hoursVariance <= 0"
                                [class.text-danger]="report.hoursVariance > 0">
                          {{ report.hoursVariance > 0 ? '+' : '' }}{{ report.hoursVariance | number:'1.1-1' }}
                        </strong>
                      </div>
                      <div class="progress" style="height: 10px;">
                        <div class="progress-bar" 
                             [class.bg-success]="report.hoursVariance <= 0"
                             [class.bg-warning]="report.hoursVariance > 0 && report.hoursVariance <= report.estimatedHours * 0.2"
                             [class.bg-danger]="report.hoursVariance > report.estimatedHours * 0.2"
                             [style.width.%]="Math.min(100, (report.totalHoursWorked / report.estimatedHours) * 100)">
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div *ngIf="hoursReports.length === 0" class="text-center py-4">
                <i class="fas fa-clock fa-3x text-muted mb-3"></i>
                <p class="text-muted">No hours tracking data available</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Error State -->
      <div *ngIf="!isLoading && error" class="alert alert-danger">
        <i class="fas fa-exclamation-triangle me-2"></i>
        {{ error }}
      </div>

      <!-- Empty State -->
      <div *ngIf="!isLoading && !dashboard && !error" class="text-center py-5">
        <i class="fas fa-chart-line fa-4x text-muted mb-4"></i>
        <h4 class="text-muted">No Monitoring Data Available</h4>
        <p class="text-muted">Start by creating projects and tracking hours to see monitoring data.</p>
      </div>
    </div>
  `,
  styles: [`
    .project-monitoring-container {
      padding: 1.5rem;
    }

    .card {
      box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
      border: 1px solid rgba(0, 0, 0, 0.125);
      transition: all 0.15s ease-in-out;
    }

    .card:hover {
      box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
      transform: translateY(-2px);
    }

    .progress {
      background-color: #e9ecef;
    }

    .list-group-item {
      border-left: none;
      border-right: none;
    }

    .list-group-item:first-child {
      border-top: none;
    }

    .list-group-item:last-child {
      border-bottom: none;
    }

    .border-start {
      border-left-width: 4px !important;
    }

    .opacity-75 {
      opacity: 0.75;
    }

    .fa-spin {
      animation: fa-spin 2s infinite linear;
    }

    @keyframes fa-spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
  `]
})
export class ProjectMonitoringComponent implements OnInit, OnDestroy {
  dashboard: ProjectDashboard | null = null;
  hoursReports: ProjectHoursReport[] = [];
  isLoading = false;
  error: string | null = null;
  Math = Math;

  private destroy$ = new Subject<void>();

  constructor(private projectService: ProjectService) {}

  ngOnInit(): void {
    this.loadDashboardData();
    
    // Auto-refresh every 5 minutes
    interval(300000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.refreshData());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  async loadDashboardData(): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      // Load team leader dashboard
      this.dashboard = await firstValueFrom(this.projectService.getTeamLeaderDashboard()) || null;
      
      // Load hours tracking reports
      this.hoursReports = await firstValueFrom(this.projectService.getTeamHoursTracking()) || [];
      
    } catch (error: any) {
      console.error('Error loading dashboard data:', error);
      this.error = error.message || 'Failed to load monitoring data';
    } finally {
      this.isLoading = false;
    }
  }

  refreshData(): void {
    this.loadDashboardData();
  }

  setDateRange(range: string): void {
    let startDate: Date;
    const endDate = new Date();

    switch (range) {
      case 'today':
        startDate = new Date();
        startDate.setHours(0, 0, 0, 0);
        break;
      case 'week':
        startDate = new Date();
        startDate.setDate(startDate.getDate() - 7);
        break;
      case 'month':
        startDate = new Date();
        startDate.setMonth(startDate.getMonth() - 1);
        break;
      case 'quarter':
        startDate = new Date();
        startDate.setMonth(startDate.getMonth() - 3);
        break;
      default:
        return;
    }

    this.loadHoursTrackingWithDateRange(startDate, endDate);
  }

  private async loadHoursTrackingWithDateRange(startDate: Date, endDate: Date): Promise<void> {
    try {
      this.hoursReports = await this.projectService.getTeamHoursTracking(startDate, endDate).toPromise() || [];
    } catch (error: any) {
      console.error('Error loading hours tracking with date range:', error);
    }
  }
}