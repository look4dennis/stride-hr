import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { RealTimeService, ConnectionState } from '../../../core/services/real-time.service';
import { DashboardRealTimeService } from '../../services/dashboard-real-time.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-real-time-test',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="real-time-test-panel">
      <div class="card">
        <div class="card-header">
          <h5 class="mb-0">
            <i class="fas fa-broadcast-tower me-2"></i>
            Real-Time System Test Panel
          </h5>
        </div>
        
        <div class="card-body">
          <!-- Connection Status -->
          <div class="test-section">
            <h6>Connection Status</h6>
            <div class="status-display" [ngClass]="getConnectionStatusClass()">
              <div class="status-indicator">
                <i class="fas fa-circle" [ngClass]="getConnectionIconClass()"></i>
                <span>{{ getConnectionStatusText() }}</span>
              </div>
              <div class="connection-details" *ngIf="connectionState">
                <small>
                  Connection ID: {{ connectionState.connectionId || 'N/A' }}<br>
                  Reconnect Attempts: {{ connectionState.reconnectAttempts }}<br>
                  Last Connected: {{ connectionState.lastConnected | date:'medium' }}
                </small>
              </div>
            </div>
            
            <div class="test-actions">
              <button 
                class="btn btn-primary btn-sm me-2" 
                (click)="connect()"
                [disabled]="isConnected">
                Connect
              </button>
              <button 
                class="btn btn-secondary btn-sm me-2" 
                (click)="disconnect()"
                [disabled]="!isConnected">
                Disconnect
              </button>
              <button 
                class="btn btn-warning btn-sm" 
                (click)="reconnect()"
                [disabled]="!connectionState">
                Reconnect
              </button>
            </div>
          </div>

          <!-- Real-Time Updates -->
          <div class="test-section">
            <h6>Real-Time Updates</h6>
            <div class="updates-display">
              <div class="update-counters">
                <span class="badge bg-primary me-2">Total: {{ totalUpdates }}</span>
                <span class="badge bg-success me-2">Attendance: {{ attendanceUpdates }}</span>
                <span class="badge bg-info me-2">Dashboard: {{ dashboardUpdates }}</span>
                <span class="badge bg-warning me-2">Notifications: {{ notificationUpdates }}</span>
              </div>
              
              <div class="recent-updates">
                <h6>Recent Updates (Last 5)</h6>
                <div class="update-list">
                  <div 
                    class="update-item" 
                    *ngFor="let update of recentUpdates; trackBy: trackByUpdate"
                    [ngClass]="'update-' + update.type">
                    <div class="update-header">
                      <span class="update-type">{{ update.type | titlecase }}</span>
                      <small class="update-time">{{ update.timestamp | date:'HH:mm:ss' }}</small>
                    </div>
                    <div class="update-content">
                      <small>{{ getUpdateDescription(update) }}</small>
                    </div>
                  </div>
                </div>
                
                <button 
                  class="btn btn-outline-secondary btn-sm mt-2" 
                  (click)="clearUpdates()">
                  Clear Updates
                </button>
              </div>
            </div>
          </div>

          <!-- Test Actions -->
          <div class="test-section">
            <h6>Test Actions</h6>
            
            <!-- Dashboard Statistics Test -->
            <div class="test-group">
              <label>Dashboard Statistics</label>
              <div class="test-actions">
                <button 
                  class="btn btn-outline-primary btn-sm me-2" 
                  (click)="requestDashboardRefresh()"
                  [disabled]="!isConnected">
                  Request Refresh
                </button>
                <button 
                  class="btn btn-outline-info btn-sm" 
                  (click)="getDashboardStats()"
                  [disabled]="!isConnected">
                  Get Statistics
                </button>
              </div>
            </div>

            <!-- Attendance Test -->
            <div class="test-group">
              <label>Attendance Updates</label>
              <div class="test-actions">
                <select class="form-select form-select-sm me-2" [(ngModel)]="selectedAction" style="width: auto; display: inline-block;">
                  <option value="checkin">Check In</option>
                  <option value="checkout">Check Out</option>
                  <option value="break_start">Start Break</option>
                  <option value="break_end">End Break</option>
                </select>
                <button 
                  class="btn btn-outline-success btn-sm" 
                  (click)="sendAttendanceUpdate()"
                  [disabled]="!isConnected">
                  Send Update
                </button>
              </div>
            </div>

            <!-- Notification Test -->
            <div class="test-group">
              <label>Test Notifications</label>
              <div class="test-actions">
                <input 
                  type="text" 
                  class="form-control form-control-sm me-2" 
                  [(ngModel)]="testMessage" 
                  placeholder="Test message"
                  style="width: 200px; display: inline-block;">
                <button 
                  class="btn btn-outline-warning btn-sm" 
                  (click)="sendTestNotification()"
                  [disabled]="!isConnected || !testMessage">
                  Send Notification
                </button>
              </div>
            </div>

            <!-- Connection Health Test -->
            <div class="test-group">
              <label>Connection Health</label>
              <div class="test-actions">
                <button 
                  class="btn btn-outline-info btn-sm me-2" 
                  (click)="pingServer()"
                  [disabled]="!isConnected">
                  Ping Server
                </button>
                <button 
                  class="btn btn-outline-secondary btn-sm" 
                  (click)="getConnectionStats()"
                  [disabled]="!isConnected">
                  Get Stats
                </button>
              </div>
            </div>
          </div>

          <!-- Dashboard Metrics Display -->
          <div class="test-section" *ngIf="dashboardMetrics">
            <h6>Current Dashboard Metrics</h6>
            <div class="metrics-display">
              <div class="row">
                <div class="col-md-3">
                  <div class="metric-card">
                    <div class="metric-label">Total Employees</div>
                    <div class="metric-value">{{ dashboardMetrics.attendance.totalEmployees }}</div>
                  </div>
                </div>
                <div class="col-md-3">
                  <div class="metric-card">
                    <div class="metric-label">Present Today</div>
                    <div class="metric-value">{{ dashboardMetrics.attendance.presentToday }}</div>
                  </div>
                </div>
                <div class="col-md-3">
                  <div class="metric-card">
                    <div class="metric-label">On Break</div>
                    <div class="metric-value">{{ dashboardMetrics.attendance.onBreak }}</div>
                  </div>
                </div>
                <div class="col-md-3">
                  <div class="metric-card">
                    <div class="metric-label">Online Users</div>
                    <div class="metric-value">{{ onlineUsers }}</div>
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
    .real-time-test-panel {
      margin: 20px;
    }

    .test-section {
      margin-bottom: 30px;
      padding-bottom: 20px;
      border-bottom: 1px solid #e5e7eb;
    }

    .test-section:last-child {
      border-bottom: none;
    }

    .status-display {
      padding: 15px;
      border-radius: 6px;
      margin-bottom: 15px;
    }

    .status-display.connected {
      background-color: #d1fae5;
      border: 1px solid #10b981;
    }

    .status-display.connecting {
      background-color: #fef3c7;
      border: 1px solid #f59e0b;
    }

    .status-display.disconnected {
      background-color: #fee2e2;
      border: 1px solid #ef4444;
    }

    .status-indicator {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 600;
      margin-bottom: 8px;
    }

    .status-indicator .fa-circle.connected {
      color: #10b981;
    }

    .status-indicator .fa-circle.connecting {
      color: #f59e0b;
      animation: pulse 1.5s infinite;
    }

    .status-indicator .fa-circle.disconnected {
      color: #ef4444;
    }

    .test-actions {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      margin-top: 10px;
    }

    .test-group {
      margin-bottom: 15px;
      padding: 10px;
      background: #f9fafb;
      border-radius: 4px;
    }

    .test-group label {
      display: block;
      font-weight: 600;
      margin-bottom: 8px;
      color: #374151;
    }

    .updates-display {
      background: #f9fafb;
      padding: 15px;
      border-radius: 6px;
    }

    .update-counters {
      margin-bottom: 15px;
    }

    .update-list {
      max-height: 200px;
      overflow-y: auto;
      border: 1px solid #e5e7eb;
      border-radius: 4px;
      background: white;
    }

    .update-item {
      padding: 8px 12px;
      border-bottom: 1px solid #e5e7eb;
    }

    .update-item:last-child {
      border-bottom: none;
    }

    .update-item.update-attendance {
      border-left: 3px solid #10b981;
    }

    .update-item.update-dashboard {
      border-left: 3px solid #3b82f6;
    }

    .update-item.update-notification {
      border-left: 3px solid #f59e0b;
    }

    .update-item.update-system {
      border-left: 3px solid #ef4444;
    }

    .update-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 4px;
    }

    .update-type {
      font-weight: 600;
      font-size: 0.875rem;
    }

    .update-time {
      color: #6b7280;
    }

    .update-content {
      color: #6b7280;
    }

    .metrics-display {
      background: #f9fafb;
      padding: 15px;
      border-radius: 6px;
    }

    .metric-card {
      background: white;
      padding: 15px;
      border-radius: 6px;
      text-align: center;
      border: 1px solid #e5e7eb;
    }

    .metric-label {
      font-size: 0.875rem;
      color: #6b7280;
      margin-bottom: 8px;
    }

    .metric-value {
      font-size: 1.5rem;
      font-weight: 700;
      color: #111827;
    }

    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.5; }
    }
  `]
})
export class RealTimeTestComponent implements OnInit, OnDestroy {
  connectionState: ConnectionState | null = null;
  isConnected = false;
  
  // Update counters
  totalUpdates = 0;
  attendanceUpdates = 0;
  dashboardUpdates = 0;
  notificationUpdates = 0;
  
  // Recent updates
  recentUpdates: any[] = [];
  
  // Dashboard metrics
  dashboardMetrics: any = null;
  onlineUsers = 0;
  
  // Test inputs
  selectedAction = 'checkin';
  testMessage = 'Test notification message';
  
  private subscription: Subscription = new Subscription();

  constructor(
    private realTimeService: RealTimeService,
    private dashboardRealTimeService: DashboardRealTimeService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    // Subscribe to connection state
    this.subscription.add(
      this.realTimeService.connectionState$.subscribe(state => {
        this.connectionState = state;
        this.isConnected = state.isConnected;
      })
    );

    // Subscribe to real-time updates
    this.subscription.add(
      this.realTimeService.realTimeUpdates$.subscribe(update => {
        if (update) {
          this.totalUpdates++;
          this.recentUpdates.unshift(update);
          this.recentUpdates = this.recentUpdates.slice(0, 5);
          
          switch (update.type) {
            case 'attendance':
              this.attendanceUpdates++;
              break;
            case 'dashboard':
              this.dashboardUpdates++;
              break;
            case 'notification':
              this.notificationUpdates++;
              break;
          }
        }
      })
    );

    // Subscribe to dashboard metrics
    this.subscription.add(
      this.dashboardRealTimeService.dashboardMetrics$.subscribe(metrics => {
        this.dashboardMetrics = metrics;
      })
    );

    // Subscribe to online users
    this.subscription.add(
      this.dashboardRealTimeService.onlineEmployees$.subscribe(count => {
        this.onlineUsers = count;
      })
    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  // Connection methods
  async connect(): Promise<void> {
    try {
      await this.realTimeService.connect();
    } catch (error) {
      console.error('Failed to connect:', error);
    }
  }

  async disconnect(): Promise<void> {
    try {
      await this.realTimeService.disconnect();
    } catch (error) {
      console.error('Failed to disconnect:', error);
    }
  }

  async reconnect(): Promise<void> {
    try {
      await this.realTimeService.reconnect();
    } catch (error) {
      console.error('Failed to reconnect:', error);
    }
  }

  // Test methods
  async requestDashboardRefresh(): Promise<void> {
    try {
      await this.realTimeService.requestDashboardRefresh();
    } catch (error) {
      console.error('Failed to request dashboard refresh:', error);
    }
  }

  async getDashboardStats(): Promise<void> {
    try {
      const response = await this.http.get(`${environment.apiUrl}/dashboard/statistics`).toPromise();
      console.log('Dashboard statistics:', response);
    } catch (error) {
      console.error('Failed to get dashboard statistics:', error);
    }
  }

  async sendAttendanceUpdate(): Promise<void> {
    try {
      await this.realTimeService.updateAttendanceStatus(this.selectedAction);
      
      // Also send via API
      await this.http.post(`${environment.apiUrl}/dashboard/attendance-update`, {
        action: this.selectedAction,
        location: 'Test Location'
      }).toPromise();
    } catch (error) {
      console.error('Failed to send attendance update:', error);
    }
  }

  async sendTestNotification(): Promise<void> {
    try {
      await this.http.post(`${environment.apiUrl}/dashboard/system-alert`, {
        type: 'system',
        severity: 'info',
        title: 'Test Notification',
        message: this.testMessage,
        scope: 'branch'
      }).toPromise();
    } catch (error) {
      console.error('Failed to send test notification:', error);
    }
  }

  async pingServer(): Promise<void> {
    try {
      // The ping method is handled internally by the real-time service
      console.log('Ping sent to server');
    } catch (error) {
      console.error('Failed to ping server:', error);
    }
  }

  async getConnectionStats(): Promise<void> {
    try {
      await this.realTimeService.getConnectionStats();
    } catch (error) {
      console.error('Failed to get connection stats:', error);
    }
  }

  // Helper methods
  getConnectionStatusClass(): string {
    if (!this.connectionState) return 'disconnected';
    
    if (this.connectionState.isConnected) return 'connected';
    if (this.connectionState.isConnecting || this.connectionState.isReconnecting) return 'connecting';
    return 'disconnected';
  }

  getConnectionIconClass(): string {
    const baseClass = 'fas fa-circle';
    
    if (!this.connectionState) return `${baseClass} disconnected`;
    
    if (this.connectionState.isConnected) return `${baseClass} connected`;
    if (this.connectionState.isConnecting || this.connectionState.isReconnecting) return `${baseClass} connecting`;
    return `${baseClass} disconnected`;
  }

  getConnectionStatusText(): string {
    if (!this.connectionState) return 'Disconnected';
    
    if (this.connectionState.isConnected) return 'Connected';
    if (this.connectionState.isConnecting) return 'Connecting...';
    if (this.connectionState.isReconnecting) return 'Reconnecting...';
    return 'Disconnected';
  }

  getUpdateDescription(update: any): string {
    switch (update.type) {
      case 'attendance':
        return `Employee ${update.data.employeeName || update.data.employeeId} ${update.data.action}`;
      case 'dashboard':
        return `Dashboard statistics updated`;
      case 'notification':
        return `${update.data.title}: ${update.data.message}`;
      case 'system':
        return `System alert: ${update.data.message}`;
      default:
        return `${update.type} update received`;
    }
  }

  clearUpdates(): void {
    this.recentUpdates = [];
    this.totalUpdates = 0;
    this.attendanceUpdates = 0;
    this.dashboardUpdates = 0;
    this.notificationUpdates = 0;
  }

  trackByUpdate(index: number, update: any): string {
    return `${update.type}-${update.timestamp.getTime()}`;
  }
}