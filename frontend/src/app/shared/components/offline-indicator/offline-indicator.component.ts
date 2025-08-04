import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { PwaService } from '../../../services/pwa.service';
import { OfflineStorageService } from '../../../services/offline-storage.service';

@Component({
  selector: 'app-offline-indicator',
  standalone: true,
  imports: [CommonModule],
  template: `
    <!-- Offline Status Bar -->
    <div class="offline-status-bar" *ngIf="!isOnline" [@slideDown]>
      <div class="container-fluid">
        <div class="row align-items-center">
          <div class="col-auto">
            <i class="fas fa-wifi-slash text-warning"></i>
          </div>
          <div class="col">
            <span class="fw-bold">You're offline</span>
            <span class="ms-2 small">Some features may be limited</span>
          </div>
          <div class="col-auto" *ngIf="pendingActions > 0">
            <span class="badge bg-warning text-dark">
              {{ pendingActions }} pending
            </span>
          </div>
          <div class="col-auto">
            <button 
              class="btn btn-sm btn-outline-light"
              (click)="showOfflineDetails = !showOfflineDetails">
              <i class="fas" [class.fa-chevron-down]="!showOfflineDetails" 
                  [class.fa-chevron-up]="showOfflineDetails"></i>
            </button>
          </div>
        </div>
        
        <!-- Offline Details -->
        <div class="offline-details mt-2" *ngIf="showOfflineDetails" [@expandCollapse]>
          <div class="row">
            <div class="col-md-6">
              <h6 class="small fw-bold mb-2">Available Offline:</h6>
              <ul class="small mb-0">
                <li>View cached dashboard</li>
                <li>Check attendance status</li>
                <li>Submit attendance (will sync later)</li>
                <li>View employee profile</li>
                <li>Submit DSR (will sync later)</li>
              </ul>
            </div>
            <div class="col-md-6" *ngIf="pendingActions > 0">
              <h6 class="small fw-bold mb-2">Pending Sync ({{ pendingActions }}):</h6>
              <ul class="small mb-0">
                <li *ngFor="let action of pendingActionsList | slice:0:3">
                  {{ getActionDescription(action) }}
                  <span class="text-muted">({{ getTimeAgo(action.timestamp) }})</span>
                </li>
                <li *ngIf="pendingActions > 3" class="text-muted">
                  +{{ pendingActions - 3 }} more...
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Connection Restored Notification -->
    <div class="connection-restored" *ngIf="showConnectionRestored" [@slideDown]>
      <div class="container-fluid">
        <div class="row align-items-center">
          <div class="col-auto">
            <i class="fas fa-wifi text-success"></i>
          </div>
          <div class="col">
            <span class="fw-bold text-success">Connection restored!</span>
            <span class="ms-2 small" *ngIf="syncInProgress">Syncing offline data...</span>
            <span class="ms-2 small" *ngIf="!syncInProgress && pendingActions === 0">All data is up to date</span>
          </div>
          <div class="col-auto" *ngIf="syncInProgress">
            <div class="spinner-border spinner-border-sm text-success" role="status">
              <span class="visually-hidden">Syncing...</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Sync Progress -->
    <div class="sync-progress" *ngIf="syncInProgress && isOnline">
      <div class="container-fluid">
        <div class="row align-items-center">
          <div class="col-auto">
            <div class="spinner-border spinner-border-sm text-primary" role="status">
              <span class="visually-hidden">Syncing...</span>
            </div>
          </div>
          <div class="col">
            <span class="small">Syncing {{ syncedCount }}/{{ totalSyncItems }} items...</span>
          </div>
          <div class="col-auto">
            <div class="progress" style="width: 100px; height: 6px;">
              <div class="progress-bar" 
                   [style.width.%]="syncProgress"
                   role="progressbar"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .offline-status-bar {
      background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
      color: white;
      padding: 0.75rem 0;
      position: sticky;
      top: 0;
      z-index: 1040;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .connection-restored {
      background: linear-gradient(135deg, #10b981 0%, #059669 100%);
      color: white;
      padding: 0.75rem 0;
      position: sticky;
      top: 0;
      z-index: 1040;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .sync-progress {
      background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
      color: white;
      padding: 0.5rem 0;
      position: sticky;
      top: 0;
      z-index: 1039;
    }

    .offline-details {
      border-top: 1px solid rgba(255, 255, 255, 0.2);
      padding-top: 0.75rem;
    }

    .badge {
      font-size: 0.75rem;
    }

    .progress {
      background-color: rgba(255, 255, 255, 0.2);
    }

    .progress-bar {
      background-color: rgba(255, 255, 255, 0.8);
    }

    ul {
      list-style-type: none;
      padding-left: 0;
    }

    ul li {
      padding: 0.125rem 0;
    }

    ul li:before {
      content: "•";
      margin-right: 0.5rem;
      color: rgba(255, 255, 255, 0.7);
    }

    @media (max-width: 768px) {
      .offline-details .col-md-6 {
        margin-bottom: 1rem;
      }
    }
  `],
  animations: [
    // You would need to import animations from @angular/animations
    // For now, using CSS transitions
  ]
})
export class OfflineIndicatorComponent implements OnInit, OnDestroy {
  isOnline = true;
  showOfflineDetails = false;
  showConnectionRestored = false;
  syncInProgress = false;
  pendingActions = 0;
  pendingActionsList: any[] = [];
  syncedCount = 0;
  totalSyncItems = 0;
  syncProgress = 0;

  private destroy$ = new Subject<void>();
  private connectionRestoredTimer?: number;

  constructor(
    private pwaService: PwaService,
    private offlineStorageService: OfflineStorageService
  ) {}

  ngOnInit(): void {
    // Monitor online status
    this.pwaService.isOnline$
      .pipe(takeUntil(this.destroy$))
      .subscribe(online => {
        const wasOffline = !this.isOnline;
        this.isOnline = online;
        
        if (online && wasOffline) {
          this.onConnectionRestored();
        }
      });

    // Monitor pending actions
    this.offlineStorageService.pendingActions$
      .pipe(takeUntil(this.destroy$))
      .subscribe(actions => {
        this.pendingActionsList = actions.filter(a => !a.synced);
        this.pendingActions = this.pendingActionsList.length;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    
    if (this.connectionRestoredTimer) {
      clearTimeout(this.connectionRestoredTimer);
    }
  }

  private onConnectionRestored(): void {
    this.showConnectionRestored = true;
    
    if (this.pendingActions > 0) {
      this.startSync();
    }
    
    // Hide connection restored message after 3 seconds
    this.connectionRestoredTimer = window.setTimeout(() => {
      this.showConnectionRestored = false;
    }, 3000);
  }

  private startSync(): void {
    this.syncInProgress = true;
    this.totalSyncItems = this.pendingActions;
    this.syncedCount = 0;
    this.syncProgress = 0;

    // Simulate sync progress (in real implementation, this would be connected to actual sync)
    const syncInterval = setInterval(() => {
      this.syncedCount++;
      this.syncProgress = (this.syncedCount / this.totalSyncItems) * 100;
      
      if (this.syncedCount >= this.totalSyncItems) {
        clearInterval(syncInterval);
        this.syncInProgress = false;
        this.syncedCount = 0;
        this.totalSyncItems = 0;
        this.syncProgress = 0;
      }
    }, 1000);
  }

  getActionDescription(action: any): string {
    switch (action.type) {
      case 'attendance':
        return action.action === 'check-in' ? 'Check-in' : 
               action.action === 'check-out' ? 'Check-out' :
               action.action === 'break-start' ? 'Break start' : 'Break end';
      case 'dsr':
        return 'DSR submission';
      case 'leave':
        return 'Leave request';
      case 'profile':
        return 'Profile update';
      default:
        return 'Unknown action';
    }
  }

  getTimeAgo(timestamp: string): string {
    const now = new Date();
    const time = new Date(timestamp);
    const diffMs = now.getTime() - time.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    
    if (diffMins < 1) return 'just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;
    
    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays}d ago`;
  }
}