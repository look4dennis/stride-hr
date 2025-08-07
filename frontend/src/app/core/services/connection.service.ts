import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, fromEvent, merge, of, timer } from 'rxjs';
import { map, startWith, switchMap, catchError, distinctUntilChanged } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface ConnectionStatus {
  isOnline: boolean;
  isConnectedToServer: boolean;
  lastChecked: Date;
  latency?: number;
  connectionType?: string;
}

export interface OfflineAction {
  id: string;
  method: string;
  url: string;
  data?: any;
  timestamp: Date;
  retryCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class ConnectionService {
  private connectionStatusSubject = new BehaviorSubject<ConnectionStatus>({
    isOnline: navigator.onLine,
    isConnectedToServer: false,
    lastChecked: new Date()
  });

  private offlineActionsSubject = new BehaviorSubject<OfflineAction[]>([]);
  
  public connectionStatus$ = this.connectionStatusSubject.asObservable();
  public offlineActions$ = this.offlineActionsSubject.asObservable();
  
  private readonly healthCheckUrl = `${environment.apiUrl}/health`;
  private readonly healthCheckInterval = 30000; // 30 seconds
  private readonly maxOfflineActions = 50;
  private readonly maxRetryAttempts = 3;

  constructor(private http: HttpClient) {
    this.initializeConnectionMonitoring();
  }

  /**
   * Initialize connection monitoring
   */
  private initializeConnectionMonitoring(): void {
    // Monitor browser online/offline events
    const online$ = fromEvent(window, 'online').pipe(map(() => true));
    const offline$ = fromEvent(window, 'offline').pipe(map(() => false));
    
    merge(online$, offline$).pipe(
      startWith(navigator.onLine),
      distinctUntilChanged()
    ).subscribe(isOnline => {
      this.updateConnectionStatus({ isOnline });
      
      if (isOnline) {
        this.checkServerConnection();
        this.processOfflineActions();
      }
    });

    // Periodic server health checks
    timer(0, this.healthCheckInterval).subscribe(() => {
      if (navigator.onLine) {
        this.checkServerConnection();
      }
    });

    // Monitor connection type changes (if supported)
    if ('connection' in navigator) {
      const connection = (navigator as any).connection;
      if (connection) {
        fromEvent(connection, 'change').subscribe(() => {
          this.updateConnectionStatus({
            connectionType: connection.effectiveType
          });
        });
      }
    }
  }

  /**
   * Check server connection health
   */
  private checkServerConnection(): void {
    const startTime = Date.now();
    
    this.http.get(this.healthCheckUrl, { 
      headers: { 'X-Skip-Error-Notification': 'true' }
    }).pipe(
      catchError(() => of(null))
    ).subscribe(response => {
      const latency = Date.now() - startTime;
      const isConnectedToServer = response !== null;
      
      this.updateConnectionStatus({
        isConnectedToServer,
        latency,
        lastChecked: new Date()
      });
    });
  }

  /**
   * Update connection status
   */
  private updateConnectionStatus(updates: Partial<ConnectionStatus>): void {
    const currentStatus = this.connectionStatusSubject.value;
    const newStatus = { ...currentStatus, ...updates };
    this.connectionStatusSubject.next(newStatus);
  }

  /**
   * Get current connection status
   */
  getConnectionStatus(): ConnectionStatus {
    return this.connectionStatusSubject.value;
  }

  /**
   * Check if currently online
   */
  isOnline(): boolean {
    const status = this.getConnectionStatus();
    return status.isOnline && status.isConnectedToServer;
  }

  /**
   * Check if currently offline
   */
  isOffline(): boolean {
    return !this.isOnline();
  }

  /**
   * Queue action for offline processing
   */
  queueOfflineAction(method: string, url: string, data?: any): string {
    const action: OfflineAction = {
      id: this.generateId(),
      method: method.toUpperCase(),
      url,
      data,
      timestamp: new Date(),
      retryCount: 0
    };

    const currentActions = this.offlineActionsSubject.value;
    const updatedActions = [action, ...currentActions].slice(0, this.maxOfflineActions);
    this.offlineActionsSubject.next(updatedActions);

    console.log('Action queued for offline processing:', action);
    return action.id;
  }

  /**
   * Process queued offline actions when connection is restored
   */
  private processOfflineActions(): void {
    const actions = this.offlineActionsSubject.value;
    
    if (actions.length === 0) {
      return;
    }

    console.log(`Processing ${actions.length} offline actions...`);

    actions.forEach(action => {
      this.retryOfflineAction(action);
    });
  }

  /**
   * Retry a specific offline action
   */
  private retryOfflineAction(action: OfflineAction): void {
    if (action.retryCount >= this.maxRetryAttempts) {
      console.warn('Max retry attempts reached for action:', action);
      this.removeOfflineAction(action.id);
      return;
    }

    let request: Observable<any>;

    switch (action.method) {
      case 'GET':
        request = this.http.get(action.url);
        break;
      case 'POST':
        request = this.http.post(action.url, action.data);
        break;
      case 'PUT':
        request = this.http.put(action.url, action.data);
        break;
      case 'DELETE':
        request = this.http.delete(action.url);
        break;
      default:
        console.error('Unsupported HTTP method for offline action:', action.method);
        this.removeOfflineAction(action.id);
        return;
    }

    request.pipe(
      catchError(error => {
        console.error('Failed to retry offline action:', action, error);
        
        // Update retry count
        const updatedAction = { ...action, retryCount: action.retryCount + 1 };
        this.updateOfflineAction(updatedAction);
        
        return of(null);
      })
    ).subscribe(response => {
      if (response !== null) {
        console.log('Offline action completed successfully:', action);
        this.removeOfflineAction(action.id);
      }
    });
  }

  /**
   * Update an offline action
   */
  private updateOfflineAction(updatedAction: OfflineAction): void {
    const currentActions = this.offlineActionsSubject.value;
    const actionIndex = currentActions.findIndex(a => a.id === updatedAction.id);
    
    if (actionIndex !== -1) {
      const updatedActions = [...currentActions];
      updatedActions[actionIndex] = updatedAction;
      this.offlineActionsSubject.next(updatedActions);
    }
  }

  /**
   * Remove an offline action
   */
  private removeOfflineAction(actionId: string): void {
    const currentActions = this.offlineActionsSubject.value;
    const updatedActions = currentActions.filter(a => a.id !== actionId);
    this.offlineActionsSubject.next(updatedActions);
  }

  /**
   * Clear all offline actions
   */
  clearOfflineActions(): void {
    this.offlineActionsSubject.next([]);
  }

  /**
   * Get pending offline actions count
   */
  getPendingActionsCount(): number {
    return this.offlineActionsSubject.value.length;
  }

  /**
   * Manual connection test
   */
  testConnection(): Observable<boolean> {
    return this.http.get(this.healthCheckUrl, {
      headers: { 'X-Skip-Error-Notification': 'true' }
    }).pipe(
      map(() => true),
      catchError(() => of(false))
    );
  }

  /**
   * Get connection quality description
   */
  getConnectionQuality(): string {
    const status = this.getConnectionStatus();
    
    if (!status.isOnline) {
      return 'Offline';
    }
    
    if (!status.isConnectedToServer) {
      return 'No server connection';
    }
    
    if (status.latency) {
      if (status.latency < 100) {
        return 'Excellent';
      } else if (status.latency < 300) {
        return 'Good';
      } else if (status.latency < 1000) {
        return 'Fair';
      } else {
        return 'Poor';
      }
    }
    
    return 'Connected';
  }

  /**
   * Generate unique ID
   */
  private generateId(): string {
    return Math.random().toString(36).substr(2, 9) + Date.now().toString(36);
  }
}