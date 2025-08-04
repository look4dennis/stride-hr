import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface OfflineAction {
  id: string;
  type: 'attendance' | 'dsr' | 'leave' | 'profile';
  action: string;
  data: any;
  timestamp: string;
  synced: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class OfflineStorageService {
  private readonly STORAGE_KEY = 'stride-hr-offline-actions';
  private readonly CACHE_KEY = 'stride-hr-cache';
  
  private pendingActionsSubject = new BehaviorSubject<OfflineAction[]>([]);
  public readonly pendingActions$ = this.pendingActionsSubject.asObservable();

  constructor() {
    this.loadPendingActions();
  }

  /**
   * Store an action for offline sync
   */
  storeAction(type: OfflineAction['type'], action: string, data: any): string {
    const offlineAction: OfflineAction = {
      id: this.generateId(),
      type,
      action,
      data,
      timestamp: new Date().toISOString(),
      synced: false
    };

    const actions = this.getPendingActions();
    actions.push(offlineAction);
    this.savePendingActions(actions);
    this.pendingActionsSubject.next(actions);

    return offlineAction.id;
  }

  /**
   * Get all pending actions
   */
  getPendingActions(): OfflineAction[] {
    const data = localStorage.getItem(this.STORAGE_KEY);
    return data ? JSON.parse(data) : [];
  }

  /**
   * Mark action as synced
   */
  markActionSynced(actionId: string): void {
    const actions = this.getPendingActions();
    const actionIndex = actions.findIndex(a => a.id === actionId);
    
    if (actionIndex !== -1) {
      actions[actionIndex].synced = true;
      this.savePendingActions(actions);
      this.pendingActionsSubject.next(actions);
    }
  }

  /**
   * Remove synced actions
   */
  clearSyncedActions(): void {
    const actions = this.getPendingActions().filter(a => !a.synced);
    this.savePendingActions(actions);
    this.pendingActionsSubject.next(actions);
  }

  /**
   * Clear all pending actions
   */
  clearAllActions(): void {
    localStorage.removeItem(this.STORAGE_KEY);
    this.pendingActionsSubject.next([]);
  }

  /**
   * Cache data for offline access
   */
  cacheData(key: string, data: any, expiryMinutes: number = 60): void {
    const cacheItem = {
      data,
      timestamp: Date.now(),
      expiry: Date.now() + (expiryMinutes * 60 * 1000)
    };

    const cache = this.getCache();
    cache[key] = cacheItem;
    localStorage.setItem(this.CACHE_KEY, JSON.stringify(cache));
  }

  /**
   * Get cached data
   */
  getCachedData(key: string): any | null {
    const cache = this.getCache();
    const item = cache[key];

    if (!item) {
      return null;
    }

    if (Date.now() > item.expiry) {
      delete cache[key];
      localStorage.setItem(this.CACHE_KEY, JSON.stringify(cache));
      return null;
    }

    return item.data;
  }

  /**
   * Check if data is cached and valid
   */
  isCached(key: string): boolean {
    return this.getCachedData(key) !== null;
  }

  /**
   * Clear expired cache items
   */
  clearExpiredCache(): void {
    const cache = this.getCache();
    const now = Date.now();
    let hasExpired = false;

    Object.keys(cache).forEach(key => {
      if (cache[key].expiry < now) {
        delete cache[key];
        hasExpired = true;
      }
    });

    if (hasExpired) {
      localStorage.setItem(this.CACHE_KEY, JSON.stringify(cache));
    }
  }

  /**
   * Get cache size in bytes
   */
  getCacheSize(): number {
    const cache = localStorage.getItem(this.CACHE_KEY);
    return cache ? new Blob([cache]).size : 0;
  }

  /**
   * Clear all cache
   */
  clearCache(): void {
    localStorage.removeItem(this.CACHE_KEY);
  }

  /**
   * Store attendance data for offline access
   */
  storeAttendanceCheckIn(location: string): string {
    return this.storeAction('attendance', 'check-in', {
      location,
      timestamp: new Date().toISOString()
    });
  }

  /**
   * Store attendance check-out for offline access
   */
  storeAttendanceCheckOut(): string {
    return this.storeAction('attendance', 'check-out', {
      timestamp: new Date().toISOString()
    });
  }

  /**
   * Store break start for offline access
   */
  storeBreakStart(breakType: string): string {
    return this.storeAction('attendance', 'break-start', {
      breakType,
      timestamp: new Date().toISOString()
    });
  }

  /**
   * Store break end for offline access
   */
  storeBreakEnd(): string {
    return this.storeAction('attendance', 'break-end', {
      timestamp: new Date().toISOString()
    });
  }

  /**
   * Store DSR submission for offline access
   */
  storeDSRSubmission(projectId: number, taskId: number, hours: number, description: string): string {
    return this.storeAction('dsr', 'submit', {
      projectId,
      taskId,
      hours,
      description,
      date: new Date().toISOString().split('T')[0]
    });
  }

  /**
   * Store leave request for offline access
   */
  storeLeaveRequest(leaveType: string, startDate: string, endDate: string, reason: string): string {
    return this.storeAction('leave', 'request', {
      leaveType,
      startDate,
      endDate,
      reason
    });
  }

  /**
   * Get employee profile from cache
   */
  getCachedEmployeeProfile(): any | null {
    return this.getCachedData('employee-profile');
  }

  /**
   * Cache employee profile
   */
  cacheEmployeeProfile(profile: any): void {
    this.cacheData('employee-profile', profile, 120); // Cache for 2 hours
  }

  /**
   * Get dashboard data from cache
   */
  getCachedDashboardData(): any | null {
    return this.getCachedData('dashboard-data');
  }

  /**
   * Cache dashboard data
   */
  cacheDashboardData(data: any): void {
    this.cacheData('dashboard-data', data, 30); // Cache for 30 minutes
  }

  /**
   * Get attendance status from cache
   */
  getCachedAttendanceStatus(): any | null {
    return this.getCachedData('attendance-status');
  }

  /**
   * Cache attendance status
   */
  cacheAttendanceStatus(status: any): void {
    this.cacheData('attendance-status', status, 15); // Cache for 15 minutes
  }

  /**
   * Private helper methods
   */
  private loadPendingActions(): void {
    const actions = this.getPendingActions();
    this.pendingActionsSubject.next(actions);
  }

  private savePendingActions(actions: OfflineAction[]): void {
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(actions));
  }

  private getCache(): { [key: string]: any } {
    const data = localStorage.getItem(this.CACHE_KEY);
    return data ? JSON.parse(data) : {};
  }

  private generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  }
}