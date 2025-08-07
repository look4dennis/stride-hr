import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, ChartConfiguration, ChartType, registerables } from 'chart.js';
import { ReportService } from '../../../services/report.service';
import { AIAnalyticsService } from '../../../services/ai-analytics.service';
import { ReportExecutionResult, ReportChartConfiguration } from '../../../models/report.models';
import { DataVisualizationComponent } from '../data-visualization/data-visualization.component';

Chart.register(...registerables);

interface DashboardWidget {
  id: string;
  title: string;
  type: 'metric' | 'chart' | 'table' | 'insight' | 'list';
  data: any;
  chartConfig?: ReportChartConfiguration;
  loading: boolean;
  error?: string;
  size: 'small' | 'medium' | 'large' | 'full';
  position: { row: number; col: number; width: number; height: number };
  refreshInterval?: number;
}

interface MetricCard {
  title: string;
  value: number | string;
  unit?: string;
  change?: number;
  changeType: 'positive' | 'negative' | 'neutral';
  icon: string;
  color: string;
  trend?: number[];
  loading?: boolean;
}

interface AIInsight {
  id: string;
  title: string;
  description: string;
  type: 'trend' | 'anomaly' | 'prediction' | 'recommendation';
  severity: 'low' | 'medium' | 'high';
  confidence: number;
  actionable: boolean;
  createdAt: Date;
}

@Component({
  selector: 'app-analytics-dashboard-main',
  standalone: true,
  imports: [CommonModule, FormsModule, DataVisualizationComponent],
  template: `
    <div class="analytics-dashboard-main">
      <!-- Dashboard Header -->
      <div class="dashboard-header">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <div>
            <h1 class="dashboard-title">
              <i class="fas fa-chart-line me-2"></i>
              Analytics Dashboard
            </h1>
            <p class="text-muted">Comprehensive HR analytics and insights</p>
          </div>
          <div class="dashboard-controls">
            <div class="btn-group me-2">
              <button class="btn btn-outline-primary" 
                      [class.active]="selectedTimeRange === 'today'"
                      (click)="setTimeRange('today')">Today</button>
              <button class="btn btn-outline-primary" 
                      [class.active]="selectedTimeRange === 'week'"
                      (click)="setTimeRange('week')">This Week</button>
              <button class="btn btn-outline-primary" 
                      [class.active]="selectedTimeRange === 'month'"
                      (click)="setTimeRange('month')">This Month</button>
              <button class="btn btn-outline-primary" 
                      [class.active]="selectedTimeRange === 'quarter'"
                      (click)="setTimeRange('quarter')">This Quarter</button>
              <button class="btn btn-outline-primary" 
                      [class.active]="selectedTimeRange === 'year'"
                      (click)="setTimeRange('year')">This Year</button>
            </div>
            <button class="btn btn-primary me-2" (click)="refreshDashboard()">
              <i class="fas fa-sync-alt" [class.fa-spin]="isRefreshing"></i>
              Refresh
            </button>
            <div class="dropdown">
              <button class="btn btn-outline-secondary dropdown-toggle" 
                      type="button" 
                      data-bs-toggle="dropdown">
                <i class="fas fa-cog"></i>
                Settings
              </button>
              <ul class="dropdown-menu">
                <li><a class="dropdown-item" href="#" (click)="customizeLayout()">
                  <i class="fas fa-layout me-2"></i>Customize Layout
                </a></li>
                <li><a class="dropdown-item" href="#" (click)="exportDashboard()">
                  <i class="fas fa-download me-2"></i>Export Dashboard
                </a></li>
                <li><a class="dropdown-item" href="#" (click)="scheduleDashboard()">
                  <i class="fas fa-clock me-2"></i>Schedule Reports
                </a></li>
                <li><hr class="dropdown-divider"></li>
                <li><a class="dropdown-item" href="#" (click)="resetLayout()">
                  <i class="fas fa-undo me-2"></i>Reset Layout
                </a></li>
              </ul>
            </div>
          </div>
        </div>

        <!-- AI Insights Banner -->
        <div class="ai-insights-banner" *ngIf="aiInsights.length > 0">
          <div class="alert alert-info d-flex align-items-center">
            <i class="fas fa-brain me-2"></i>
            <div class="flex-grow-1">
              <strong>AI Insights Available:</strong>
              <span class="ms-2">{{aiInsights.length}} new insights detected</span>
            </div>
            <button class="btn btn-sm btn-outline-info" (click)="showAIInsights()">
              View Insights
            </button>
          </div>
        </div>
      </div>

      <!-- Key Metrics Row -->
      <div class="metrics-section mb-4">
        <div class="row">
          <div class="col-lg-3 col-md-6 mb-3" *ngFor="let metric of keyMetrics">
            <div class="metric-card" [class.loading]="metric.loading">
              <div class="metric-header">
                <div class="metric-icon" [style.background-color]="metric.color">
                  <i [class]="metric.icon"></i>
                </div>
                <div class="metric-trend" *ngIf="metric.trend">
                  <canvas #trendChart [id]="getTrendChartId(metric.title)"></canvas>
                </div>
              </div>
              <div class="metric-content">
                <div class="metric-value" *ngIf="!metric.loading">
                  {{metric.value | number}}
                  <span class="metric-unit" *ngIf="metric.unit">{{metric.unit}}</span>
                </div>
                <div class="metric-loading" *ngIf="metric.loading">
                  <div class="spinner-border spinner-border-sm"></div>
                </div>
                <div class="metric-label">{{metric.title}}</div>
                <div class="metric-change" [class]="metric.changeType" *ngIf="metric.change !== undefined">
                  <i [class]="getChangeIcon(metric.changeType)"></i>
                  {{metric.change | number:'1.1-1'}}% vs last {{selectedTimeRange}}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Dashboard Grid -->
      <div class="dashboard-grid">
        <div class="row">
          <!-- Main Charts Column -->
          <div class="col-lg-8">
            <!-- Attendance Trends Chart -->
            <div class="widget-card mb-4">
              <div class="widget-header">
                <h5 class="widget-title">
                  <i class="fas fa-clock me-2"></i>
                  Attendance Trends
                </h5>
                <div class="widget-controls">
                  <div class="btn-group btn-group-sm me-2">
                    <button class="btn btn-outline-primary" 
                            [class.active]="attendanceView === 'daily'"
                            (click)="setAttendanceView('daily')">Daily</button>
                    <button class="btn btn-outline-primary" 
                            [class.active]="attendanceView === 'weekly'"
                            (click)="setAttendanceView('weekly')">Weekly</button>
                    <button class="btn btn-outline-primary" 
                            [class.active]="attendanceView === 'monthly'"
                            (click)="setAttendanceView('monthly')">Monthly</button>
                  </div>
                  <button class="btn btn-sm btn-outline-secondary" (click)="refreshWidget('attendance-trends')">
                    <i class="fas fa-sync-alt"></i>
                  </button>
                  <div class="dropdown">
                    <button class="btn btn-sm btn-outline-secondary dropdown-toggle" 
                            data-bs-toggle="dropdown">
                      <i class="fas fa-ellipsis-v"></i>
                    </button>
                    <ul class="dropdown-menu">
                      <li><a class="dropdown-item" href="#" (click)="exportWidget('attendance-trends')">Export</a></li>
                      <li><a class="dropdown-item" href="#" (click)="drillDown('attendance-trends')">Drill Down</a></li>
                      <li><a class="dropdown-item" href="#" (click)="configureWidget('attendance-trends')">Configure</a></li>
                    </ul>
                  </div>
                </div>
              </div>
              <div class="widget-body">
                <app-data-visualization
                  [data]="attendanceTrendsData"
                  [chartConfiguration]="attendanceTrendsConfig"
                  [showControls]="false"
                  [showStatistics]="false"
                  [showExportOptions]="false"
                  chartHeight="300px">
                </app-data-visualization>
              </div>
            </div>

            <!-- Performance Analytics Chart -->
            <div class="widget-card mb-4">
              <div class="widget-header">
                <h5 class="widget-title">
                  <i class="fas fa-chart-bar me-2"></i>
                  Performance Analytics
                </h5>
                <div class="widget-controls">
                  <div class="btn-group btn-group-sm me-2">
                    <button class="btn btn-outline-primary" 
                            [class.active]="performanceView === 'department'"
                            (click)="setPerformanceView('department')">By Department</button>
                    <button class="btn btn-outline-primary" 
                            [class.active]="performanceView === 'individual'"
                            (click)="setPerformanceView('individual')">Individual</button>
                    <button class="btn btn-outline-primary" 
                            [class.active]="performanceView === 'team'"
                            (click)="setPerformanceView('team')">By Team</button>
                  </div>
                  <button class="btn btn-sm btn-outline-secondary" (click)="refreshWidget('performance-analytics')">
                    <i class="fas fa-sync-alt"></i>
                  </button>
                </div>
              </div>
              <div class="widget-body">
                <app-data-visualization
                  [data]="performanceData"
                  [chartConfiguration]="performanceConfig"
                  [showControls]="false"
                  [showStatistics]="false"
                  [showExportOptions]="false"
                  chartHeight="300px">
                </app-data-visualization>
              </div>
            </div>

            <!-- Project Progress Overview -->
            <div class="widget-card">
              <div class="widget-header">
                <h5 class="widget-title">
                  <i class="fas fa-tasks me-2"></i>
                  Project Progress Overview
                </h5>
                <div class="widget-controls">
                  <button class="btn btn-sm btn-outline-secondary" (click)="refreshWidget('project-progress')">
                    <i class="fas fa-sync-alt"></i>
                  </button>
                </div>
              </div>
              <div class="widget-body">
                <div class="project-progress-list">
                  <div class="project-item" *ngFor="let project of projectProgress">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                      <div class="project-info">
                        <h6 class="mb-0">{{project.name}}</h6>
                        <small class="text-muted">{{project.team}} • Due: {{project.dueDate | date:'shortDate'}}</small>
                      </div>
                      <div class="project-status">
                        <span class="badge" [class]="'badge-' + project.status">{{project.status}}</span>
                      </div>
                    </div>
                    <div class="progress mb-2">
                      <div class="progress-bar" 
                           [style.width.%]="project.progress"
                           [class]="'bg-' + getProgressColor(project.progress)">
                      </div>
                    </div>
                    <div class="d-flex justify-content-between">
                      <small class="text-muted">{{project.progress}}% Complete</small>
                      <small class="text-muted">{{project.hoursSpent}}/{{project.estimatedHours}}h</small>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Sidebar Widgets -->
          <div class="col-lg-4">
            <!-- AI Insights Widget -->
            <div class="widget-card mb-4">
              <div class="widget-header">
                <h5 class="widget-title">
                  <i class="fas fa-brain me-2"></i>
                  AI Insights
                </h5>
                <div class="widget-controls">
                  <button class="btn btn-sm btn-outline-secondary" (click)="refreshAIInsights()">
                    <i class="fas fa-sync-alt"></i>
                  </button>
                </div>
              </div>
              <div class="widget-body">
                <div class="ai-insights-list">
                  <div class="insight-item" *ngFor="let insight of aiInsights.slice(0, 3)">
                    <div class="insight-header">
                      <div class="insight-type" [class]="'insight-' + insight.type">
                        <i [class]="getInsightIcon(insight.type)"></i>
                      </div>
                      <div class="insight-meta">
                        <div class="insight-title">{{insight.title}}</div>
                        <div class="insight-confidence">
                          Confidence: {{insight.confidence}}%
                        </div>
                      </div>
                    </div>
                    <div class="insight-description">{{insight.description}}</div>
                    <div class="insight-actions" *ngIf="insight.actionable">
                      <button class="btn btn-sm btn-outline-primary">Take Action</button>
                    </div>
                  </div>
                  <div class="text-center mt-3" *ngIf="aiInsights.length > 3">
                    <button class="btn btn-sm btn-link" (click)="showAllInsights()">
                      View All {{aiInsights.length}} Insights
                    </button>
                  </div>
                </div>
              </div>
            </div>

            <!-- Top Performers Widget -->
            <div class="widget-card mb-4">
              <div class="widget-header">
                <h5 class="widget-title">
                  <i class="fas fa-trophy me-2"></i>
                  Top Performers
                </h5>
                <div class="widget-controls">
                  <div class="btn-group btn-group-sm">
                    <button class="btn btn-outline-primary" 
                            [class.active]="performersPeriod === 'month'"
                            (click)="setPerformersPeriod('month')">Month</button>
                    <button class="btn btn-outline-primary" 
                            [class.active]="performersPeriod === 'quarter'"
                            (click)="setPerformersPeriod('quarter')">Quarter</button>
                  </div>
                </div>
              </div>
              <div class="widget-body">
                <div class="performer-list">
                  <div class="performer-item" *ngFor="let performer of topPerformers; let i = index">
                    <div class="performer-rank">
                      <span class="rank-badge" [class]="'rank-' + (i + 1)">{{i + 1}}</span>
                    </div>
                    <div class="performer-avatar">
                      <img [src]="performer.avatar || '/assets/images/default-avatar.png'" 
                           [alt]="performer.name"
                           class="rounded-circle">
                    </div>
                    <div class="performer-info">
                      <div class="performer-name">{{performer.name}}</div>
                      <div class="performer-department">{{performer.department}}</div>
                    </div>
                    <div class="performer-score">
                      <div class="score-value">{{performer.score}}</div>
                      <div class="score-label">Score</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Recent Activities Widget -->
            <div class="widget-card">
              <div class="widget-header">
                <h5 class="widget-title">
                  <i class="fas fa-history me-2"></i>
                  Recent Activities
                </h5>
                <div class="widget-controls">
                  <button class="btn btn-sm btn-outline-secondary" (click)="refreshActivities()">
                    <i class="fas fa-sync-alt"></i>
                  </button>
                </div>
              </div>
              <div class="widget-body">
                <div class="activity-timeline">
                  <div class="activity-item" *ngFor="let activity of recentActivities">
                    <div class="activity-icon" [class]="'activity-' + activity.type">
                      <i [class]="getActivityIcon(activity.type)"></i>
                    </div>
                    <div class="activity-content">
                      <div class="activity-title">{{activity.title}}</div>
                      <div class="activity-description">{{activity.description}}</div>
                      <div class="activity-time">{{activity.timestamp | date:'short'}}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- AI Insights Modal -->
    <div class="modal fade" id="aiInsightsModal" tabindex="-1">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="fas fa-brain me-2"></i>
              AI-Powered Insights
            </h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div class="insights-detailed">
              <div class="insight-detailed" *ngFor="let insight of aiInsights">
                <div class="insight-card">
                  <div class="insight-header-detailed">
                    <div class="insight-type-badge" [class]="'badge-' + insight.type">
                      <i [class]="getInsightIcon(insight.type)"></i>
                      {{insight.type | titlecase}}
                    </div>
                    <div class="insight-severity" [class]="'severity-' + insight.severity">
                      {{insight.severity | titlecase}} Priority
                    </div>
                  </div>
                  <h6 class="insight-title-detailed">{{insight.title}}</h6>
                  <p class="insight-description-detailed">{{insight.description}}</p>
                  <div class="insight-metrics">
                    <div class="metric">
                      <span class="metric-label">Confidence:</span>
                      <span class="metric-value">{{insight.confidence}}%</span>
                    </div>
                    <div class="metric">
                      <span class="metric-label">Generated:</span>
                      <span class="metric-value">{{insight.createdAt | date:'short'}}</span>
                    </div>
                  </div>
                  <div class="insight-actions-detailed" *ngIf="insight.actionable">
                    <button class="btn btn-primary btn-sm me-2">Implement Recommendation</button>
                    <button class="btn btn-outline-secondary btn-sm">Learn More</button>
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
    .analytics-dashboard-main {
      padding: 1.5rem;
      background-color: #f8f9fa;
      min-height: 100vh;
    }

    .dashboard-title {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .dashboard-controls .btn-group .btn {
      font-size: 0.875rem;
    }

    .ai-insights-banner {
      margin-bottom: 1.5rem;
    }

    .metrics-section {
      margin-bottom: 2rem;
    }

    .metric-card {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      border: 1px solid #e9ecef;
      transition: all 0.3s ease;
      position: relative;
      overflow: hidden;
      height: 140px;
    }

    .metric-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    }

    .metric-card.loading::before {
      content: '';
      position: absolute;
      top: 0;
      left: -100%;
      width: 100%;
      height: 100%;
      background: linear-gradient(90deg, transparent, rgba(255,255,255,0.4), transparent);
      animation: loading 1.5s infinite;
    }

    @keyframes loading {
      0% { left: -100%; }
      100% { left: 100%; }
    }

    .metric-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 1rem;
    }

    .metric-icon {
      width: 50px;
      height: 50px;
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 1.25rem;
    }

    .metric-trend {
      width: 60px;
      height: 30px;
    }

    .metric-trend canvas {
      width: 100% !important;
      height: 100% !important;
    }

    .metric-value {
      font-size: 1.75rem;
      font-weight: 700;
      color: var(--text-primary);
      line-height: 1;
    }

    .metric-unit {
      font-size: 1rem;
      font-weight: 400;
      color: var(--text-secondary);
    }

    .metric-label {
      font-size: 0.875rem;
      color: var(--text-secondary);
      margin-bottom: 0.5rem;
      font-weight: 500;
    }

    .metric-change {
      font-size: 0.75rem;
      font-weight: 500;
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }

    .metric-change.positive {
      color: #10b981;
    }

    .metric-change.negative {
      color: #ef4444;
    }

    .metric-change.neutral {
      color: #6b7280;
    }

    .widget-card {
      background: white;
      border-radius: 12px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      border: 1px solid #e9ecef;
      overflow: hidden;
    }

    .widget-header {
      padding: 1.25rem 1.5rem;
      border-bottom: 1px solid #e9ecef;
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
    }

    .widget-title {
      font-size: 1.125rem;
      font-weight: 600;
      color: var(--text-primary);
      margin: 0;
      flex-grow: 1;
    }

    .widget-controls {
      display: flex;
      gap: 0.5rem;
      align-items: center;
    }

    .widget-body {
      padding: 1.5rem;
    }

    .project-item {
      padding: 1rem;
      border: 1px solid #e9ecef;
      border-radius: 8px;
      margin-bottom: 1rem;
      transition: all 0.2s ease;
    }

    .project-item:hover {
      border-color: var(--primary);
      box-shadow: 0 2px 8px rgba(59, 130, 246, 0.1);
    }

    .badge-on-track { background-color: #10b981; }
    .badge-at-risk { background-color: #f59e0b; }
    .badge-delayed { background-color: #ef4444; }
    .badge-completed { background-color: #6b7280; }

    .bg-success { background-color: #10b981 !important; }
    .bg-warning { background-color: #f59e0b !important; }
    .bg-danger { background-color: #ef4444 !important; }

    .ai-insights-list {
      max-height: 400px;
      overflow-y: auto;
    }

    .insight-item {
      padding: 1rem;
      border: 1px solid #e9ecef;
      border-radius: 8px;
      margin-bottom: 1rem;
      background: linear-gradient(135deg, #f8f9fa 0%, #ffffff 100%);
    }

    .insight-header {
      display: flex;
      align-items: flex-start;
      margin-bottom: 0.75rem;
    }

    .insight-type {
      width: 40px;
      height: 40px;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-right: 0.75rem;
      color: white;
      font-size: 1.125rem;
    }

    .insight-trend { background-color: #3b82f6; }
    .insight-anomaly { background-color: #ef4444; }
    .insight-prediction { background-color: #8b5cf6; }
    .insight-recommendation { background-color: #10b981; }

    .insight-title {
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 0.25rem;
    }

    .insight-confidence {
      font-size: 0.75rem;
      color: var(--text-secondary);
    }

    .insight-description {
      font-size: 0.875rem;
      color: var(--text-secondary);
      margin-bottom: 0.75rem;
    }

    .performer-list {
      max-height: 300px;
      overflow-y: auto;
    }

    .performer-item {
      display: flex;
      align-items: center;
      padding: 0.75rem;
      border-radius: 8px;
      margin-bottom: 0.5rem;
      transition: background-color 0.2s ease;
    }

    .performer-item:hover {
      background-color: #f8f9fa;
    }

    .performer-rank {
      margin-right: 0.75rem;
    }

    .rank-badge {
      width: 24px;
      height: 24px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.75rem;
      font-weight: 600;
      color: white;
    }

    .rank-1 { background-color: #ffd700; }
    .rank-2 { background-color: #c0c0c0; }
    .rank-3 { background-color: #cd7f32; }

    .performer-avatar {
      margin-right: 0.75rem;
    }

    .performer-avatar img {
      width: 40px;
      height: 40px;
      object-fit: cover;
    }

    .performer-info {
      flex-grow: 1;
    }

    .performer-name {
      font-weight: 600;
      color: var(--text-primary);
      font-size: 0.875rem;
    }

    .performer-department {
      font-size: 0.75rem;
      color: var(--text-secondary);
    }

    .performer-score {
      text-align: center;
    }

    .score-value {
      font-size: 1.25rem;
      font-weight: 700;
      color: var(--primary);
    }

    .score-label {
      font-size: 0.75rem;
      color: var(--text-secondary);
    }

    .activity-timeline {
      max-height: 300px;
      overflow-y: auto;
    }

    .activity-item {
      display: flex;
      align-items: flex-start;
      padding: 0.75rem;
      border-radius: 8px;
      margin-bottom: 0.5rem;
      transition: background-color 0.2s ease;
    }

    .activity-item:hover {
      background-color: #f8f9fa;
    }

    .activity-icon {
      width: 32px;
      height: 32px;
      border-radius: 6px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-right: 0.75rem;
      color: white;
      font-size: 0.875rem;
    }

    .activity-checkin { background-color: #10b981; }
    .activity-leave { background-color: #f59e0b; }
    .activity-project { background-color: #3b82f6; }
    .activity-performance { background-color: #8b5cf6; }

    .activity-title {
      font-weight: 600;
      color: var(--text-primary);
      font-size: 0.875rem;
      margin-bottom: 0.25rem;
    }

    .activity-description {
      font-size: 0.75rem;
      color: var(--text-secondary);
      margin-bottom: 0.25rem;
    }

    .activity-time {
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    .insights-detailed {
      max-height: 60vh;
      overflow-y: auto;
    }

    .insight-card {
      border: 1px solid #e9ecef;
      border-radius: 8px;
      padding: 1.5rem;
      margin-bottom: 1rem;
      background: white;
    }

    .insight-header-detailed {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }

    .insight-type-badge {
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      font-size: 0.75rem;
      font-weight: 600;
      color: white;
    }

    .badge-trend { background-color: #3b82f6; }
    .badge-anomaly { background-color: #ef4444; }
    .badge-prediction { background-color: #8b5cf6; }
    .badge-recommendation { background-color: #10b981; }

    .severity-high { color: #ef4444; font-weight: 600; }
    .severity-medium { color: #f59e0b; font-weight: 600; }
    .severity-low { color: #10b981; font-weight: 600; }

    .insight-title-detailed {
      font-size: 1.125rem;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 0.75rem;
    }

    .insight-description-detailed {
      color: var(--text-secondary);
      margin-bottom: 1rem;
    }

    .insight-metrics {
      display: flex;
      gap: 2rem;
      margin-bottom: 1rem;
    }

    .metric {
      display: flex;
      flex-direction: column;
    }

    .metric-label {
      font-size: 0.75rem;
      color: var(--text-secondary);
      margin-bottom: 0.25rem;
    }

    .metric-value {
      font-weight: 600;
      color: var(--text-primary);
    }
  `]
})
export class AnalyticsDashboardMainComponent implements OnInit, AfterViewInit {
  selectedTimeRange: string = 'month';
  isRefreshing: boolean = false;
  attendanceView: string = 'daily';
  performanceView: string = 'department';
  performersPeriod: string = 'month';

  keyMetrics: MetricCard[] = [];
  aiInsights: AIInsight[] = [];
  topPerformers: any[] = [];
  recentActivities: any[] = [];
  projectProgress: any[] = [];

  // Chart data
  attendanceTrendsData: ReportExecutionResult | null = null;
  attendanceTrendsConfig: ReportChartConfiguration = {
    type: 'line',
    title: 'Attendance Trends',
    xAxisColumn: 'date',
    yAxisColumn: 'attendance_rate'
  };

  performanceData: ReportExecutionResult | null = null;
  performanceConfig: ReportChartConfiguration = {
    type: 'bar',
    title: 'Performance by Department',
    xAxisColumn: 'department',
    yAxisColumn: 'performance_score'
  };

  constructor(
    private reportService: ReportService,
    private aiAnalyticsService: AIAnalyticsService
  ) {}

  ngOnInit() {
    this.loadDashboardData();
  }

  ngAfterViewInit() {
    // Initialize trend charts for metrics
    setTimeout(() => {
      this.initializeTrendCharts();
    }, 100);
  }

  async loadDashboardData() {
    this.isRefreshing = true;
    
    try {
      await Promise.all([
        this.loadKeyMetrics(),
        this.loadAIInsights(),
        this.loadTopPerformers(),
        this.loadRecentActivities(),
        this.loadProjectProgress(),
        this.loadAttendanceTrends(),
        this.loadPerformanceData()
      ]);
    } catch (error) {
      console.error('Failed to load dashboard data:', error);
    } finally {
      this.isRefreshing = false;
    }
  }

  async loadKeyMetrics() {
    // Simulate loading key metrics with trend data
    this.keyMetrics = [
      {
        title: 'Total Employees',
        value: 1247,
        unit: '',
        change: 5.2,
        changeType: 'positive',
        icon: 'fas fa-users',
        color: '#3b82f6',
        trend: [1180, 1195, 1210, 1225, 1240, 1247],
        loading: false
      },
      {
        title: 'Attendance Rate',
        value: 94.8,
        unit: '%',
        change: 2.1,
        changeType: 'positive',
        icon: 'fas fa-clock',
        color: '#10b981',
        trend: [92.1, 93.2, 93.8, 94.1, 94.5, 94.8],
        loading: false
      },
      {
        title: 'Performance Score',
        value: 87.3,
        unit: '/100',
        change: -1.5,
        changeType: 'negative',
        icon: 'fas fa-chart-line',
        color: '#f59e0b',
        trend: [89.2, 88.8, 88.1, 87.9, 87.6, 87.3],
        loading: false
      },
      {
        title: 'Active Projects',
        value: 42,
        unit: '',
        change: 8.7,
        changeType: 'positive',
        icon: 'fas fa-tasks',
        color: '#8b5cf6',
        trend: [35, 37, 38, 39, 41, 42],
        loading: false
      }
    ];
  }

  async loadAIInsights() {
    // Simulate AI insights
    this.aiInsights = [
      {
        id: '1',
        title: 'Attendance Pattern Anomaly Detected',
        description: 'Unusual drop in attendance on Mondays in the Engineering department. Consider flexible work arrangements.',
        type: 'anomaly',
        severity: 'medium',
        confidence: 87,
        actionable: true,
        createdAt: new Date()
      },
      {
        id: '2',
        title: 'Performance Improvement Trend',
        description: 'Sales team showing consistent 15% improvement in performance metrics over the last quarter.',
        type: 'trend',
        severity: 'low',
        confidence: 92,
        actionable: false,
        createdAt: new Date()
      },
      {
        id: '3',
        title: 'Predicted Turnover Risk',
        description: '3 employees in Marketing department show high turnover risk based on engagement patterns.',
        type: 'prediction',
        severity: 'high',
        confidence: 78,
        actionable: true,
        createdAt: new Date()
      }
    ];
  }

  async loadTopPerformers() {
    // Simulate top performers data
    this.topPerformers = [
      {
        name: 'Sarah Johnson',
        department: 'Sales',
        score: 98.5,
        avatar: '/assets/images/avatars/sarah.jpg'
      },
      {
        name: 'Michael Chen',
        department: 'Engineering',
        score: 96.2,
        avatar: '/assets/images/avatars/michael.jpg'
      },
      {
        name: 'Emily Davis',
        department: 'Marketing',
        score: 94.8,
        avatar: '/assets/images/avatars/emily.jpg'
      }
    ];
  }

  async loadRecentActivities() {
    // Simulate recent activities
    this.recentActivities = [
      {
        type: 'checkin',
        title: 'John Smith checked in',
        description: 'Arrived at 9:00 AM',
        timestamp: new Date(Date.now() - 30 * 60 * 1000)
      },
      {
        type: 'leave',
        title: 'Leave request approved',
        description: 'Maria Garcia - Vacation leave for 3 days',
        timestamp: new Date(Date.now() - 45 * 60 * 1000)
      },
      {
        type: 'project',
        title: 'Project milestone completed',
        description: 'Website Redesign - Phase 1 completed',
        timestamp: new Date(Date.now() - 60 * 60 * 1000)
      }
    ];
  }

  async loadProjectProgress() {
    // Simulate project progress data
    this.projectProgress = [
      {
        name: 'Website Redesign',
        team: 'Design Team',
        dueDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000),
        progress: 75,
        status: 'on-track',
        hoursSpent: 120,
        estimatedHours: 160
      },
      {
        name: 'Mobile App Development',
        team: 'Development Team',
        dueDate: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000),
        progress: 45,
        status: 'at-risk',
        hoursSpent: 180,
        estimatedHours: 400
      },
      {
        name: 'HR System Integration',
        team: 'IT Team',
        dueDate: new Date(Date.now() + 21 * 24 * 60 * 60 * 1000),
        progress: 90,
        status: 'on-track',
        hoursSpent: 270,
        estimatedHours: 300
      }
    ];
  }

  async loadAttendanceTrends() {
    // Simulate attendance trends data
    const data = [
      { date: '2024-01-01', attendance_rate: 92.5 },
      { date: '2024-01-02', attendance_rate: 94.2 },
      { date: '2024-01-03', attendance_rate: 93.8 },
      { date: '2024-01-04', attendance_rate: 95.1 },
      { date: '2024-01-05', attendance_rate: 94.7 },
      { date: '2024-01-06', attendance_rate: 96.3 },
      { date: '2024-01-07', attendance_rate: 95.9 }
    ];

    this.attendanceTrendsData = {
      success: true,
      data: data,
      totalRecords: data.length,
      executionTime: 150,
      metadata: {}
    };
  }

  async loadPerformanceData() {
    // Simulate performance data
    const data = [
      { department: 'Sales', performance_score: 88.5 },
      { department: 'Engineering', performance_score: 92.1 },
      { department: 'Marketing', performance_score: 85.7 },
      { department: 'HR', performance_score: 90.3 },
      { department: 'Finance', performance_score: 87.9 }
    ];

    this.performanceData = {
      success: true,
      data: data,
      totalRecords: data.length,
      executionTime: 120,
      metadata: {}
    };
  }

  private initializeTrendCharts() {
    this.keyMetrics.forEach((metric, index) => {
      if (metric.trend) {
        const canvasId = `trend-${metric.title.replace(/\s+/g, '-').toLowerCase()}`;
        const canvas = document.getElementById(canvasId) as HTMLCanvasElement;
        
        if (canvas) {
          const ctx = canvas.getContext('2d');
          if (ctx) {
            new Chart(ctx, {
              type: 'line',
              data: {
                labels: metric.trend.map((_, i) => `${i + 1}`),
                datasets: [{
                  data: metric.trend,
                  borderColor: metric.color,
                  backgroundColor: metric.color + '20',
                  borderWidth: 2,
                  fill: true,
                  tension: 0.4,
                  pointRadius: 0
                }]
              },
              options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                  legend: { display: false },
                  tooltip: { enabled: false }
                },
                scales: {
                  x: { display: false },
                  y: { display: false }
                },
                elements: {
                  point: { radius: 0 }
                }
              }
            });
          }
        }
      }
    });
  }

  // Event handlers
  setTimeRange(range: string) {
    this.selectedTimeRange = range;
    this.loadDashboardData();
  }

  setAttendanceView(view: string) {
    this.attendanceView = view;
    this.loadAttendanceTrends();
  }

  setPerformanceView(view: string) {
    this.performanceView = view;
    this.loadPerformanceData();
  }

  setPerformersPeriod(period: string) {
    this.performersPeriod = period;
    this.loadTopPerformers();
  }

  refreshDashboard() {
    this.loadDashboardData();
  }

  refreshWidget(widgetId: string) {
    switch (widgetId) {
      case 'attendance-trends':
        this.loadAttendanceTrends();
        break;
      case 'performance-analytics':
        this.loadPerformanceData();
        break;
      case 'project-progress':
        this.loadProjectProgress();
        break;
    }
  }

  refreshAIInsights() {
    this.loadAIInsights();
  }

  refreshActivities() {
    this.loadRecentActivities();
  }

  // Utility methods
  getChangeIcon(changeType: string): string {
    switch (changeType) {
      case 'positive': return 'fas fa-arrow-up';
      case 'negative': return 'fas fa-arrow-down';
      default: return 'fas fa-minus';
    }
  }

  getProgressColor(progress: number): string {
    if (progress >= 80) return 'success';
    if (progress >= 60) return 'warning';
    return 'danger';
  }

  getInsightIcon(type: string): string {
    switch (type) {
      case 'trend': return 'fas fa-chart-line';
      case 'anomaly': return 'fas fa-exclamation-triangle';
      case 'prediction': return 'fas fa-crystal-ball';
      case 'recommendation': return 'fas fa-lightbulb';
      default: return 'fas fa-info-circle';
    }
  }

  getActivityIcon(type: string): string {
    switch (type) {
      case 'checkin': return 'fas fa-sign-in-alt';
      case 'leave': return 'fas fa-calendar-times';
      case 'project': return 'fas fa-tasks';
      case 'performance': return 'fas fa-chart-bar';
      default: return 'fas fa-bell';
    }
  }

  // Modal and action handlers
  showAIInsights() {
    // Show AI insights modal
    const modal = document.getElementById('aiInsightsModal');
    if (modal) {
      const bootstrapModal = new (window as any).bootstrap.Modal(modal);
      bootstrapModal.show();
    }
  }

  showAllInsights() {
    this.showAIInsights();
  }

  customizeLayout() {
    // Implement layout customization
    console.log('Customize layout');
  }

  exportDashboard() {
    // Implement dashboard export
    console.log('Export dashboard');
  }

  scheduleDashboard() {
    // Implement dashboard scheduling
    console.log('Schedule dashboard');
  }

  resetLayout() {
    // Implement layout reset
    console.log('Reset layout');
  }

  exportWidget(widgetId: string) {
    // Implement widget export
    console.log('Export widget:', widgetId);
  }

  drillDown(widgetId: string) {
    // Implement drill down functionality
    console.log('Drill down:', widgetId);
  }

  configureWidget(widgetId: string) {
    // Implement widget configuration
    console.log('Configure widget:', widgetId);
  }

  getTrendChartId(title: string): string {
    return 'trend-' + title.replace(/\s+/g, '-').toLowerCase();
  }
}