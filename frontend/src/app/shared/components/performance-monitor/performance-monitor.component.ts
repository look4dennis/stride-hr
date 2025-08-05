import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PerformanceService, PerformanceMetrics } from '../../../core/services/performance.service';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-performance-monitor',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="performance-monitor" *ngIf="showMonitor">
      <div class="performance-header">
        <h6>Performance Monitor</h6>
        <button class="btn btn-sm btn-outline-secondary" (click)="toggleDetails()">
          {{ showDetails ? 'Hide' : 'Show' }} Details
        </button>
        <button class="btn btn-sm btn-outline-danger" (click)="closeMonitor()">×</button>
      </div>
      
      <div class="performance-summary">
        <div class="metric" [class.warning]="isSlowMetric('pageLoadTime', currentMetrics?.pageLoadTime)">
          <span class="label">Load Time:</span>
          <span class="value">{{ currentMetrics?.pageLoadTime | number:'1.0-0' }}ms</span>
        </div>
        <div class="metric" [class.warning]="isSlowMetric('firstContentfulPaint', currentMetrics?.firstContentfulPaint)">
          <span class="label">FCP:</span>
          <span class="value">{{ currentMetrics?.firstContentfulPaint | number:'1.0-0' }}ms</span>
        </div>
        <div class="metric" [class.warning]="isSlowMetric('memoryUsage', currentMetrics?.memoryUsage)">
          <span class="label">Memory:</span>
          <span class="value">{{ (currentMetrics?.memoryUsage || 0) / 1024 / 1024 | number:'1.0-1' }}MB</span>
        </div>
      </div>

      <div class="performance-details" *ngIf="showDetails">
        <div class="metrics-grid">
          <div class="metric-item">
            <label>Largest Contentful Paint:</label>
            <span [class.warning]="(currentMetrics?.largestContentfulPaint || 0) > 2500">
              {{ currentMetrics?.largestContentfulPaint | number:'1.0-0' }}ms
            </span>
          </div>
          <div class="metric-item">
            <label>Cumulative Layout Shift:</label>
            <span [class.warning]="(currentMetrics?.cumulativeLayoutShift || 0) > 0.1">
              {{ currentMetrics?.cumulativeLayoutShift | number:'1.0-4' }}
            </span>
          </div>
          <div class="metric-item">
            <label>First Input Delay:</label>
            <span [class.warning]="(currentMetrics?.firstInputDelay || 0) > 100">
              {{ currentMetrics?.firstInputDelay | number:'1.0-0' }}ms
            </span>
          </div>
          <div class="metric-item">
            <label>Time to Interactive:</label>
            <span [class.warning]="(currentMetrics?.timeToInteractive || 0) > 3800">
              {{ currentMetrics?.timeToInteractive | number:'1.0-0' }}ms
            </span>
          </div>
          <div class="metric-item">
            <label>Bundle Size:</label>
            <span [class.warning]="(currentMetrics?.bundleSize || 0) > 1024 * 1024">
              {{ (currentMetrics?.bundleSize || 0) / 1024 | number:'1.0-0' }}KB
            </span>
          </div>
        </div>

        <div class="component-metrics" *ngIf="componentMetrics.length > 0">
          <h6>Component Performance</h6>
          <div class="component-list">
            <div class="component-item" *ngFor="let component of componentMetrics">
              <span class="component-name">{{ component.componentName }}</span>
              <span class="component-time" [class.warning]="component.renderTime > 16">
                {{ component.renderTime | number:'1.0-2' }}ms
              </span>
            </div>
          </div>
        </div>

        <div class="performance-actions">
          <button class="btn btn-sm btn-primary" (click)="generateReport()">
            Generate Report
          </button>
          <button class="btn btn-sm btn-secondary" (click)="clearMetrics()">
            Clear Metrics
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .performance-monitor {
      position: fixed;
      top: 10px;
      right: 10px;
      background: rgba(0, 0, 0, 0.9);
      color: white;
      padding: 10px;
      border-radius: 8px;
      font-size: 12px;
      z-index: 9999;
      min-width: 250px;
      max-width: 400px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
    }

    .performance-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 10px;
      border-bottom: 1px solid #333;
      padding-bottom: 5px;
    }

    .performance-header h6 {
      margin: 0;
      font-size: 14px;
    }

    .performance-summary {
      display: flex;
      gap: 15px;
      margin-bottom: 10px;
    }

    .metric {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .metric .label {
      font-size: 10px;
      opacity: 0.7;
    }

    .metric .value {
      font-weight: bold;
      font-size: 12px;
    }

    .metric.warning .value {
      color: #ff6b6b;
    }

    .performance-details {
      border-top: 1px solid #333;
      padding-top: 10px;
    }

    .metrics-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 8px;
      margin-bottom: 15px;
    }

    .metric-item {
      display: flex;
      justify-content: space-between;
      font-size: 11px;
    }

    .metric-item label {
      opacity: 0.8;
    }

    .metric-item span.warning {
      color: #ff6b6b;
    }

    .component-metrics {
      margin-bottom: 15px;
    }

    .component-metrics h6 {
      font-size: 12px;
      margin-bottom: 8px;
    }

    .component-list {
      max-height: 120px;
      overflow-y: auto;
    }

    .component-item {
      display: flex;
      justify-content: space-between;
      padding: 2px 0;
      font-size: 10px;
    }

    .component-time.warning {
      color: #ff6b6b;
    }

    .performance-actions {
      display: flex;
      gap: 8px;
    }

    .btn {
      padding: 4px 8px;
      font-size: 10px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
    }

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-secondary {
      background: #6c757d;
      color: white;
    }

    .btn-outline-secondary {
      background: transparent;
      color: #6c757d;
      border: 1px solid #6c757d;
    }

    .btn-outline-danger {
      background: transparent;
      color: #dc3545;
      border: 1px solid #dc3545;
    }

    .btn:hover {
      opacity: 0.8;
    }
  `]
})
export class PerformanceMonitorComponent implements OnInit, OnDestroy {
  @Input() enabled = false;
  @Input() showByDefault = false;

  showMonitor = false;
  showDetails = false;
  currentMetrics: PerformanceMetrics | null = null;
  componentMetrics: any[] = [];

  private subscription = new Subscription();

  constructor(private performanceService: PerformanceService) {}

  ngOnInit(): void {
    if (this.enabled) {
      this.showMonitor = this.showByDefault;
      this.startMonitoring();
    }
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  private startMonitoring(): void {
    // Update metrics every 2 seconds
    this.subscription.add(
      interval(2000).subscribe(() => {
        this.currentMetrics = this.performanceService.getCurrentMetrics();
        this.componentMetrics = this.performanceService.getComponentMetrics();
      })
    );

    // Subscribe to real-time metrics updates
    this.subscription.add(
      this.performanceService.metrics$.subscribe(metrics => {
        if (metrics) {
          this.currentMetrics = { ...this.currentMetrics, ...metrics };
        }
      })
    );
  }

  toggleDetails(): void {
    this.showDetails = !this.showDetails;
  }

  closeMonitor(): void {
    this.showMonitor = false;
  }

  isSlowMetric(metricName: string, value?: number): boolean {
    if (!value) return false;

    const thresholds: { [key: string]: number } = {
      pageLoadTime: 3000,
      firstContentfulPaint: 1800,
      largestContentfulPaint: 2500,
      firstInputDelay: 100,
      timeToInteractive: 3800,
      memoryUsage: 100 * 1024 * 1024 // 100MB
    };

    return value > (thresholds[metricName] || Infinity);
  }

  generateReport(): void {
    const report = this.performanceService.generateReport();
    console.log(report);
    
    // Create downloadable report
    const blob = new Blob([report], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `performance-report-${new Date().toISOString().split('T')[0]}.txt`;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  clearMetrics(): void {
    this.performanceService.clearComponentMetrics();
    this.componentMetrics = [];
  }
}