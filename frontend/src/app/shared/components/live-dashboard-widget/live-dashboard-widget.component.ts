import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { DashboardRealTimeService, DashboardMetrics, LiveAttendanceUpdate, DashboardAlert } from '../../services/dashboard-real-time.service';

@Component({
  selector: 'app-live-dashboard-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="live-dashboard-widget">
      <!-- Dashboard Health Indicator -->
      <div class="widget-header">
        <h5 class="widget-title">
          <i class="fas fa-tachometer-alt me-2"></i>
          Live Dashboard
        </h5>
        <div class="health-indicator" [ngClass]="getHealthClass()">
          <i class="fas fa-circle" [ngClass]="getHealthIconClass()"></i>
          <span class="health-text">{{ getHealthText() }}</span>
        </div>
      </div>

      <!-- Dashboard Metrics -->
      <div class="metrics-grid" *ngIf="dashboardMetrics">
        <div class="metric-card attendance">
          <div class="metric-header">
            <i class="fas fa-users"></i>
            <span>Attendance</span>
          </div>
          <div class="metric-value">
            {{ dashboardMetrics.attendance.presentToday }}/{{ dashboardMetrics.attendance.totalEmployees }}
          </div>
          <div class="metric-subtitle">
            {{ dashboardMetrics.attendance.attendanceRate | number:'1.1-1' }}% Present
          </div>
          <div class="metric-details">
            <small>
              <span class="text-warning">{{ dashboardMetrics.attendance.lateArrivals }} Late</span> |
              <span class="text-info">{{ dashboardMetrics.attendance.onBreak }} On Break</span>
            </small>
          </div>
        </div>

        <div class="metric-card productivity">
          <div class="metric-header">
            <i class="fas fa-chart-line"></i>
            <span>Productivity</span>
          </div>
          <div class="metric-value">
            {{ dashboardMetrics.productivity.averageProductivity | number:'1.1-1' }}%
          </div>
          <div class="metric-subtitle">
            Average Performance
          </div>
          <div class="metric-details">
            <small>
              <span class="text-success">{{ dashboardMetrics.productivity.tasksCompleted }} Tasks Done</span>
            </small>
          </div>
        </div>

        <div class="metric-card alerts">
          <div class="metric-header">
            <i class="fas fa-exclamation-triangle"></i>
            <span>Alerts</span>
          </div>
          <div class="metric-value">
            {{ getTotalAlerts() }}
          </div>
          <div class="metric-subtitle">
            Active Alerts
          </div>
          <div class="metric-details">
            <small>
              <span class="text-danger" *ngIf="dashboardMetrics.alerts.critical > 0">
                {{ dashboardMetrics.alerts.critical }} Critical
              </span>
              <span class="text-warning" *ngIf="dashboardMetrics.alerts.warnings > 0">
                {{ dashboardMetrics.alerts.warnings }} Warnings
              </span>
            </small>
          </div>
        </div>

        <div class="metric-card online">
          <div class="metric-header">
            <i class="fas fa-wifi"></i>
            <span>Online</span>
          </div>
          <div class="metric-value">
            {{ onlineEmployees }}
          </div>
          <div class="metric-subtitle">
            Employees Online
          </div>
          <div class="metric-details">
            <small class="text-success">Real-time count</small>
          </div>
        </div>
      </div>

      <!-- Live Attendance Updates -->
      <div class="live-updates-section" *ngIf="showLiveUpdates">
        <div class="section-header">
          <h6>
            <i class="fas fa-clock me-2"></i>
            Live Attendance Updates
          </h6>
          <button 
            class="btn btn-sm btn-outline-secondary"
            (click)="clearLiveUpdates()"
            *ngIf="liveAttendanceUpdates.length > 0">
            <i class="fas fa-times"></i>
          </button>
        </div>
        
        <div class="live-updates-list" *ngIf="liveAttendanceUpdates.length > 0; else noUpdates">
          <div 
            class="live-update-item" 
            *ngFor="let update of liveAttendanceUpdates; trackBy: trackByUpdateId"
            [ngClass]="getUpdateClass(update.action)">
            <div class="update-avatar">
              <img 
                *ngIf="update.profilePhoto" 
                [src]="update.profilePhoto" 
                [alt]="update.employeeName"
                class="avatar-img">
              <div *ngIf="!update.profilePhoto" class="avatar-placeholder">
                {{ getInitials(update.employeeName) }}
              </div>
            </div>
            <div class="update-content">
              <div class="update-name">{{ update.employeeName }}</div>
              <div class="update-action">{{ getActionText(update.action) }}</div>
              <div class="update-time">{{ formatTime(update.timestamp) }}</div>
            </div>
            <div class="update-icon">
              <i [ngClass]="getActionIcon(update.action)"></i>
            </div>
          </div>
        </div>
        
        <ng-template #noUpdates>
          <div class="no-updates">
            <i class="fas fa-clock text-muted"></i>
            <p class="text-muted mb-0">No recent attendance updates</p>
          </div>
        </ng-template>
      </div>

      <!-- Dashboard Alerts -->
      <div class="alerts-section" *ngIf="showAlerts && dashboardAlerts.length > 0">
        <div class="section-header">
          <h6>
            <i class="fas fa-bell me-2"></i>
            Recent Alerts
          </h6>
          <button 
            class="btn btn-sm btn-outline-secondary"
            (click)="clearAlerts()">
            <i class="fas fa-times"></i>
          </button>
        </div>
        
        <div class="alerts-list">
          <div 
            class="alert-item" 
            *ngFor="let alert of getRecentAlerts(); trackBy: trackByAlertId"
            [ngClass]="getAlertClass(alert.severity)">
            <div class="alert-icon">
              <i [ngClass]="getAlertIcon(alert.severity)"></i>
            </div>
            <div class="alert-content">
              <div class="alert-title">{{ alert.title }}</div>
              <div class="alert-message">{{ alert.message }}</div>
              <div class="alert-time">{{ formatTime(alert.timestamp) }}</div>
            </div>
            <button 
              class="btn btn-sm btn-outline-secondary alert-dismiss"
              (click)="dismissAlert(alert.id)">
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>
      </div>

      <!-- Last Updated -->
      <div class="widget-footer" *ngIf="dashboardMetrics">
        <small class="text-muted">
          <i class="fas fa-sync-alt me-1"></i>
          Last updated: {{ formatTime(dashboardMetrics.lastUpdated) }}
        </small>
      </div>
    </div>
  `,
  styles: [`
    .live-dashboard-widget {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      padding: 20px;
      margin-bottom: 20px;
    }

    .widget-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      padding-bottom: 10px;
      border-bottom: 1px solid #e5e7eb;
    }

    .widget-title {
      margin: 0;
      color: #374151;
      font-weight: 600;
    }

    .health-indicator {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 0.875rem;
    }

    .health-indicator.healthy {
      background-color: #d1fae5;
      color: #065f46;
    }

    .health-indicator.unhealthy {
      background-color: #fee2e2;
      color: #991b1b;
    }

    .health-icon.healthy {
      color: #10b981;
    }

    .health-icon.unhealthy {
      color: #ef4444;
    }

    .metrics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
      margin-bottom: 24px;
    }

    .metric-card {
      background: #f9fafb;
      border-radius: 6px;
      padding: 16px;
      border-left: 4px solid #e5e7eb;
    }

    .metric-card.attendance {
      border-left-color: #3b82f6;
    }

    .metric-card.productivity {
      border-left-color: #10b981;
    }

    .metric-card.alerts {
      border-left-color: #f59e0b;
    }

    .metric-card.online {
      border-left-color: #8b5cf6;
    }

    .metric-header {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 8px;
      color: #6b7280;
      font-size: 0.875rem;
      font-weight: 500;
    }

    .metric-value {
      font-size: 1.5rem;
      font-weight: 700;
      color: #111827;
      margin-bottom: 4px;
    }

    .metric-subtitle {
      font-size: 0.875rem;
      color: #6b7280;
      margin-bottom: 8px;
    }

    .metric-details {
      font-size: 0.75rem;
    }

    .section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 12px;
    }

    .section-header h6 {
      margin: 0;
      color: #374151;
      font-weight: 600;
    }

    .live-updates-section,
    .alerts-section {
      margin-bottom: 20px;
    }

    .live-updates-list,
    .alerts-list {
      max-height: 300px;
      overflow-y: auto;
    }

    .live-update-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px;
      border-radius: 6px;
      margin-bottom: 8px;
      background: #f9fafb;
      border-left: 3px solid #e5e7eb;
    }

    .live-update-item.checkin {
      border-left-color: #10b981;
      background: #ecfdf5;
    }

    .live-update-item.checkout {
      border-left-color: #ef4444;
      background: #fef2f2;
    }

    .live-update-item.break_start {
      border-left-color: #f59e0b;
      background: #fffbeb;
    }

    .live-update-item.break_end {
      border-left-color: #3b82f6;
      background: #eff6ff;
    }

    .update-avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      overflow: hidden;
      flex-shrink: 0;
    }

    .avatar-img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .avatar-placeholder {
      width: 100%;
      height: 100%;
      background: #e5e7eb;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.75rem;
      font-weight: 600;
      color: #6b7280;
    }

    .update-content {
      flex: 1;
    }

    .update-name {
      font-weight: 600;
      color: #111827;
      font-size: 0.875rem;
    }

    .update-action {
      color: #6b7280;
      font-size: 0.75rem;
    }

    .update-time {
      color: #9ca3af;
      font-size: 0.75rem;
    }

    .update-icon {
      color: #6b7280;
    }

    .alert-item {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 12px;
      border-radius: 6px;
      margin-bottom: 8px;
      background: #f9fafb;
      border-left: 3px solid #e5e7eb;
    }

    .alert-item.critical {
      border-left-color: #dc2626;
      background: #fef2f2;
    }

    .alert-item.error {
      border-left-color: #ef4444;
      background: #fef2f2;
    }

    .alert-item.warning {
      border-left-color: #f59e0b;
      background: #fffbeb;
    }

    .alert-item.info {
      border-left-color: #3b82f6;
      background: #eff6ff;
    }

    .alert-icon {
      margin-top: 2px;
    }

    .alert-content {
      flex: 1;
    }

    .alert-title {
      font-weight: 600;
      color: #111827;
      font-size: 0.875rem;
      margin-bottom: 4px;
    }

    .alert-message {
      color: #6b7280;
      font-size: 0.75rem;
      margin-bottom: 4px;
    }

    .alert-time {
      color: #9ca3af;
      font-size: 0.75rem;
    }

    .alert-dismiss {
      padding: 4px 6px;
      font-size: 0.75rem;
    }

    .no-updates {
      text-align: center;
      padding: 20px;
    }

    .no-updates i {
      font-size: 2rem;
      margin-bottom: 8px;
    }

    .widget-footer {
      text-align: center;
      padding-top: 12px;
      border-top: 1px solid #e5e7eb;
    }

    /* Responsive design */
    @media (max-width: 768px) {
      .metrics-grid {
        grid-template-columns: repeat(2, 1fr);
        gap: 12px;
      }

      .metric-card {
        padding: 12px;
      }

      .metric-value {
        font-size: 1.25rem;
      }

      .live-updates-list,
      .alerts-list {
        max-height: 200px;
      }
    }

    @media (max-width: 480px) {
      .metrics-grid {
        grid-template-columns: 1fr;
      }

      .widget-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 8px;
      }
    }
  `]
})
export class LiveDashboardWidgetComponent implements OnInit, OnDestroy {
  @Input() showLiveUpdates = true;
  @Input() showAlerts = true;
  @Input() maxLiveUpdates = 5;
  @Input() maxAlerts = 3;

  dashboardMetrics: DashboardMetrics | null = null;
  liveAttendanceUpdates: LiveAttendanceUpdate[] = [];
  dashboardAlerts: DashboardAlert[] = [];
  onlineEmployees = 0;
  isDashboardHealthy = true;
  lastUpdate: Date | null = null;

  private subscription: Subscription = new Subscription();

  constructor(private dashboardRealTimeService: DashboardRealTimeService) {}

  ngOnInit(): void {
    // Subscribe to dashboard metrics
    this.subscription.add(
      this.dashboardRealTimeService.dashboardMetrics$.subscribe(metrics => {
        this.dashboardMetrics = metrics;
      })
    );

    // Subscribe to live attendance updates
    this.subscription.add(
      this.dashboardRealTimeService.getRecentAttendanceUpdates(this.maxLiveUpdates).subscribe(updates => {
        this.liveAttendanceUpdates = updates;
      })
    );

    // Subscribe to dashboard alerts
    this.subscription.add(
      this.dashboardRealTimeService.dashboardAlerts$.subscribe(alerts => {
        this.dashboardAlerts = alerts;
      })
    );

    // Subscribe to online employees count
    this.subscription.add(
      this.dashboardRealTimeService.onlineEmployees$.subscribe(count => {
        this.onlineEmployees = count;
      })
    );

    // Subscribe to dashboard health
    this.subscription.add(
      this.dashboardRealTimeService.getDashboardHealth().subscribe(health => {
        this.isDashboardHealthy = health.isHealthy;
        this.lastUpdate = health.lastUpdate;
      })
    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  getHealthClass(): string {
    return this.isDashboardHealthy ? 'healthy' : 'unhealthy';
  }

  getHealthIconClass(): string {
    return this.isDashboardHealthy ? 'health-icon healthy' : 'health-icon unhealthy';
  }

  getHealthText(): string {
    return this.isDashboardHealthy ? 'Live' : 'Offline';
  }

  getTotalAlerts(): number {
    if (!this.dashboardMetrics) return 0;
    const alerts = this.dashboardMetrics.alerts;
    return alerts.critical + alerts.warnings + alerts.info;
  }

  getUpdateClass(action: string): string {
    return action.toLowerCase();
  }

  getActionText(action: string): string {
    switch (action) {
      case 'checkin': return 'Checked in';
      case 'checkout': return 'Checked out';
      case 'break_start': return 'Started break';
      case 'break_end': return 'Ended break';
      default: return action;
    }
  }

  getActionIcon(action: string): string {
    switch (action) {
      case 'checkin': return 'fas fa-sign-in-alt text-success';
      case 'checkout': return 'fas fa-sign-out-alt text-danger';
      case 'break_start': return 'fas fa-pause text-warning';
      case 'break_end': return 'fas fa-play text-primary';
      default: return 'fas fa-clock';
    }
  }

  getAlertClass(severity: string): string {
    return severity.toLowerCase();
  }

  getAlertIcon(severity: string): string {
    switch (severity) {
      case 'critical': return 'fas fa-exclamation-circle text-danger';
      case 'error': return 'fas fa-times-circle text-danger';
      case 'warning': return 'fas fa-exclamation-triangle text-warning';
      case 'info': return 'fas fa-info-circle text-primary';
      default: return 'fas fa-bell';
    }
  }

  getRecentAlerts(): DashboardAlert[] {
    return this.dashboardAlerts.slice(0, this.maxAlerts);
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map(word => word.charAt(0))
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }

  formatTime(date: Date): string {
    return new Date(date).toLocaleTimeString([], { 
      hour: '2-digit', 
      minute: '2-digit' 
    });
  }

  clearLiveUpdates(): void {
    this.dashboardRealTimeService.clearLiveUpdates();
  }

  clearAlerts(): void {
    this.dashboardRealTimeService.clearAlerts();
  }

  dismissAlert(alertId: string): void {
    this.dashboardRealTimeService.dismissAlert(alertId);
  }

  trackByUpdateId(index: number, update: LiveAttendanceUpdate): string {
    return `${update.employeeId}-${update.timestamp.getTime()}`;
  }

  trackByAlertId(index: number, alert: DashboardAlert): string {
    return alert.id;
  }
}