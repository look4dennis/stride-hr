import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { RealTimeService, ConnectionState } from '../../../core/services/real-time.service';

@Component({
  selector: 'app-connection-status',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="connection-status" [ngClass]="getStatusClass()">
      <div class="status-indicator">
        <i class="status-icon" [ngClass]="getIconClass()"></i>
        <span class="status-text">{{ getStatusText() }}</span>
      </div>
      
      <div class="status-details" *ngIf="showDetails">
        <div class="detail-item" *ngIf="connectionState.lastConnected">
          <small>Last connected: {{ formatTime(connectionState.lastConnected) }}</small>
        </div>
        <div class="detail-item" *ngIf="connectionState.reconnectAttempts > 0">
          <small>Reconnect attempts: {{ connectionState.reconnectAttempts }}</small>
        </div>
        <div class="detail-item" *ngIf="connectionState.error">
          <small class="error-text">{{ connectionState.error }}</small>
        </div>
      </div>
      
      <button 
        *ngIf="!connectionState.isConnected && !connectionState.isConnecting" 
        class="btn btn-sm btn-outline-primary reconnect-btn"
        (click)="reconnect()"
        [disabled]="isReconnecting">
        <i class="fas fa-sync-alt" [class.fa-spin]="isReconnecting"></i>
        {{ isReconnecting ? 'Reconnecting...' : 'Reconnect' }}
      </button>
    </div>
  `,
  styles: [`
    .connection-status {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 8px 12px;
      border-radius: 6px;
      font-size: 0.875rem;
      transition: all 0.3s ease;
      border: 1px solid transparent;
    }

    .connection-status.connected {
      background-color: #d1fae5;
      border-color: #10b981;
      color: #065f46;
    }

    .connection-status.connecting {
      background-color: #fef3c7;
      border-color: #f59e0b;
      color: #92400e;
    }

    .connection-status.reconnecting {
      background-color: #fef3c7;
      border-color: #f59e0b;
      color: #92400e;
    }

    .connection-status.disconnected {
      background-color: #fee2e2;
      border-color: #ef4444;
      color: #991b1b;
    }

    .status-indicator {
      display: flex;
      align-items: center;
      gap: 6px;
    }

    .status-icon {
      font-size: 0.875rem;
    }

    .status-icon.connected {
      color: #10b981;
    }

    .status-icon.connecting {
      color: #f59e0b;
      animation: pulse 1.5s infinite;
    }

    .status-icon.reconnecting {
      color: #f59e0b;
      animation: spin 1s linear infinite;
    }

    .status-icon.disconnected {
      color: #ef4444;
    }

    .status-text {
      font-weight: 500;
    }

    .status-details {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }

    .detail-item {
      font-size: 0.75rem;
      opacity: 0.8;
    }

    .error-text {
      color: #dc2626;
      font-weight: 500;
    }

    .reconnect-btn {
      font-size: 0.75rem;
      padding: 4px 8px;
      border-radius: 4px;
    }

    @keyframes pulse {
      0%, 100% {
        opacity: 1;
      }
      50% {
        opacity: 0.5;
      }
    }

    @keyframes spin {
      from {
        transform: rotate(0deg);
      }
      to {
        transform: rotate(360deg);
      }
    }

    /* Responsive design */
    @media (max-width: 768px) {
      .connection-status {
        padding: 6px 8px;
        font-size: 0.8rem;
      }

      .status-details {
        display: none;
      }

      .reconnect-btn {
        font-size: 0.7rem;
        padding: 3px 6px;
      }
    }

    /* Compact mode */
    .connection-status.compact {
      padding: 4px 8px;
      font-size: 0.75rem;
    }

    .connection-status.compact .status-details {
      display: none;
    }

    .connection-status.compact .reconnect-btn {
      font-size: 0.7rem;
      padding: 2px 6px;
    }
  `]
})
export class ConnectionStatusComponent implements OnInit, OnDestroy {
  connectionState: ConnectionState = {
    isConnected: false,
    isConnecting: false,
    isReconnecting: false,
    reconnectAttempts: 0
  };
  
  showDetails = false;
  isReconnecting = false;
  
  private subscription: Subscription = new Subscription();

  constructor(private realTimeService: RealTimeService) {}

  ngOnInit(): void {
    // Subscribe to connection state changes
    this.subscription.add(
      this.realTimeService.connectionState$.subscribe(state => {
        this.connectionState = state;
        this.isReconnecting = state.isReconnecting;
      })
    );

    // Auto-hide details after a delay when connected
    this.subscription.add(
      this.realTimeService.connectionState$.subscribe(state => {
        if (state.isConnected && this.showDetails) {
          setTimeout(() => {
            this.showDetails = false;
          }, 3000);
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  getStatusClass(): string {
    if (this.connectionState.isConnected) {
      return 'connected';
    } else if (this.connectionState.isConnecting) {
      return 'connecting';
    } else if (this.connectionState.isReconnecting) {
      return 'reconnecting';
    } else {
      return 'disconnected';
    }
  }

  getIconClass(): string {
    const baseClass = 'fas';
    
    if (this.connectionState.isConnected) {
      return `${baseClass} fa-wifi connected`;
    } else if (this.connectionState.isConnecting) {
      return `${baseClass} fa-circle-notch connecting`;
    } else if (this.connectionState.isReconnecting) {
      return `${baseClass} fa-sync-alt reconnecting`;
    } else {
      return `${baseClass} fa-wifi-slash disconnected`;
    }
  }

  getStatusText(): string {
    if (this.connectionState.isConnected) {
      return 'Connected';
    } else if (this.connectionState.isConnecting) {
      return 'Connecting...';
    } else if (this.connectionState.isReconnecting) {
      return 'Reconnecting...';
    } else {
      return 'Offline';
    }
  }

  async reconnect(): Promise<void> {
    if (this.isReconnecting) return;
    
    this.isReconnecting = true;
    this.showDetails = true;
    
    try {
      await this.realTimeService.reconnect();
    } catch (error) {
      console.error('Manual reconnection failed:', error);
    } finally {
      // Reset reconnecting state after a delay
      setTimeout(() => {
        this.isReconnecting = false;
      }, 2000);
    }
  }

  toggleDetails(): void {
    this.showDetails = !this.showDetails;
  }

  formatTime(date: Date): string {
    return new Date(date).toLocaleTimeString();
  }
}