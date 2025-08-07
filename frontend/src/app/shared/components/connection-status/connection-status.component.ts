import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ConnectionService, ConnectionStatus } from '../../../core/services/connection.service';

@Component({
    selector: 'app-connection-status',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="connection-status" [class]="getStatusClass()">
      <div class="connection-indicator">
        <i [class]="getStatusIcon()"></i>
        <span class="status-text">{{ getStatusText() }}</span>
        
        <div class="connection-details" *ngIf="showDetails">
          <small>
            {{ getConnectionQuality() }}
            <span *ngIf="connectionStatus.latency"> - {{ connectionStatus.latency }}ms</span>
          </small>
        </div>
      </div>
      
      <!-- Offline actions indicator -->
      <div class="offline-actions" *ngIf="pendingActionsCount > 0">
        <i class="fas fa-clock"></i>
        <span>{{ pendingActionsCount }} pending</span>
      </div>
      
      <!-- Retry button for failed connections -->
      <button 
        *ngIf="!connectionStatus.isConnectedToServer && connectionStatus.isOnline"
        class="btn btn-sm btn-outline-primary retry-btn"
        (click)="testConnection()"
        [disabled]="isTestingConnection">
        <i class="fas fa-redo" *ngIf="!isTestingConnection"></i>
        <i class="fas fa-spinner fa-spin" *ngIf="isTestingConnection"></i>
        Retry
      </button>
    </div>
  `,
    styles: [`
    .connection-status {
      display: flex;
      align-items: center;
      padding: 0.5rem 1rem;
      border-radius: 0.25rem;
      font-size: 0.875rem;
      transition: all 0.3s ease;
    }

    .connection-status.online {
      background-color: #d1edff;
      color: #0c5460;
      border: 1px solid #b8daff;
    }

    .connection-status.offline {
      background-color: #f8d7da;
      color: #721c24;
      border: 1px solid #f5c6cb;
    }

    .connection-status.poor-connection {
      background-color: #fff3cd;
      color: #856404;
      border: 1px solid #ffeaa7;
    }

    .connection-indicator {
      display: flex;
      align-items: center;
      flex: 1;
    }

    .connection-indicator i {
      margin-right: 0.5rem;
      font-size: 1rem;
    }

    .status-text {
      font-weight: 500;
    }

    .connection-details {
      margin-left: 0.5rem;
      opacity: 0.8;
    }

    .offline-actions {
      display: flex;
      align-items: center;
      margin-left: 1rem;
      padding-left: 1rem;
      border-left: 1px solid rgba(0, 0, 0, 0.1);
    }

    .offline-actions i {
      margin-right: 0.25rem;
      font-size: 0.875rem;
    }

    .retry-btn {
      margin-left: 1rem;
      padding: 0.25rem 0.5rem;
      font-size: 0.75rem;
    }

    /* Status-specific icon colors */
    .online .fa-wifi,
    .online .fa-check-circle {
      color: #28a745;
    }

    .offline .fa-wifi-slash,
    .offline .fa-exclamation-triangle {
      color: #dc3545;
    }

    .poor-connection .fa-wifi,
    .poor-connection .fa-exclamation-triangle {
      color: #ffc107;
    }

    /* Responsive design */
    @media (max-width: 768px) {
      .connection-status {
        padding: 0.25rem 0.5rem;
        font-size: 0.8125rem;
      }

      .connection-details {
        display: none;
      }

      .offline-actions {
        margin-left: 0.5rem;
        padding-left: 0.5rem;
      }

      .retry-btn {
        margin-left: 0.5rem;
        padding: 0.125rem 0.25rem;
      }
    }

    /* Animation for status changes */
    .connection-status {
      animation: statusChange 0.3s ease-in-out;
    }

    @keyframes statusChange {
      0% { opacity: 0.7; transform: scale(0.98); }
      100% { opacity: 1; transform: scale(1); }
    }

    /* Pulse animation for poor connection */
    .poor-connection {
      animation: pulse 2s infinite;
    }

    @keyframes pulse {
      0% { opacity: 1; }
      50% { opacity: 0.8; }
      100% { opacity: 1; }
    }
  `]
})
export class ConnectionStatusComponent implements OnInit, OnDestroy {
    private readonly connectionService = inject(ConnectionService);
    private readonly destroy$ = new Subject<void>();

    connectionStatus: ConnectionStatus = {
        isOnline: false,
        isConnectedToServer: false,
        lastChecked: new Date()
    };

    pendingActionsCount = 0;
    showDetails = false;
    isTestingConnection = false;

    ngOnInit(): void {
        // Subscribe to connection status changes
        this.connectionService.connectionStatus$.pipe(
            takeUntil(this.destroy$)
        ).subscribe(status => {
            this.connectionStatus = status;
        });

        // Subscribe to offline actions count
        this.connectionService.offlineActions$.pipe(
            takeUntil(this.destroy$)
        ).subscribe(actions => {
            this.pendingActionsCount = actions.length;
        });

        // Show details on desktop, hide on mobile
        this.showDetails = window.innerWidth > 768;
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    /**
     * Get CSS class based on connection status
     */
    getStatusClass(): string {
        if (!this.connectionStatus.isOnline) {
            return 'offline';
        }

        if (!this.connectionStatus.isConnectedToServer) {
            return 'poor-connection';
        }

        if (this.connectionStatus.latency && this.connectionStatus.latency > 1000) {
            return 'poor-connection';
        }

        return 'online';
    }

    /**
     * Get status icon based on connection status
     */
    getStatusIcon(): string {
        if (!this.connectionStatus.isOnline) {
            return 'fas fa-wifi-slash';
        }

        if (!this.connectionStatus.isConnectedToServer) {
            return 'fas fa-exclamation-triangle';
        }

        if (this.connectionStatus.latency && this.connectionStatus.latency > 1000) {
            return 'fas fa-wifi text-warning';
        }

        return 'fas fa-wifi';
    }

    /**
     * Get status text based on connection status
     */
    getStatusText(): string {
        if (!this.connectionStatus.isOnline) {
            return 'Offline';
        }

        if (!this.connectionStatus.isConnectedToServer) {
            return 'Server Disconnected';
        }

        return 'Connected';
    }

    /**
     * Get connection quality description
     */
    getConnectionQuality(): string {
        return this.connectionService.getConnectionQuality();
    }

    /**
     * Test connection manually
     */
    testConnection(): void {
        if (this.isTestingConnection) {
            return;
        }

        this.isTestingConnection = true;

        this.connectionService.testConnection().subscribe({
            next: (isConnected) => {
                this.isTestingConnection = false;

                if (isConnected) {
                    console.log('Connection test successful');
                } else {
                    console.log('Connection test failed');
                }
            },
            error: (error) => {
                this.isTestingConnection = false;
                console.error('Connection test error:', error);
            }
        });
    }

    /**
     * Toggle details visibility
     */
    toggleDetails(): void {
        this.showDetails = !this.showDetails;
    }
}