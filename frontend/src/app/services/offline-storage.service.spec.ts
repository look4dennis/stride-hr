import { TestBed } from '@angular/core/testing';
import { OfflineStorageService, OfflineAction } from './offline-storage.service';

describe('OfflineStorageService', () => {
  let service: OfflineStorageService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(OfflineStorageService);
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should store actions correctly', () => {
    const actionId = service.storeAction('attendance', 'check-in', { location: 'Office' });
    
    expect(actionId).toBeTruthy();
    
    const actions = service.getPendingActions();
    expect(actions.length).toBe(1);
    expect(actions[0].type).toBe('attendance');
    expect(actions[0].action).toBe('check-in');
    expect(actions[0].data.location).toBe('Office');
    expect(actions[0].synced).toBe(false);
  });

  it('should mark actions as synced', () => {
    const actionId = service.storeAction('dsr', 'submit', { hours: 8 });
    
    service.markActionSynced(actionId);
    
    const actions = service.getPendingActions();
    expect(actions[0].synced).toBe(true);
  });

  it('should clear synced actions', () => {
    const actionId1 = service.storeAction('attendance', 'check-in', {});
    const actionId2 = service.storeAction('attendance', 'check-out', {});
    
    service.markActionSynced(actionId1);
    
    service.clearSyncedActions();
    
    const actions = service.getPendingActions();
    expect(actions.length).toBe(1);
    expect(actions[0].id).toBe(actionId2);
  });

  it('should clear all actions', () => {
    service.storeAction('attendance', 'check-in', {});
    service.storeAction('dsr', 'submit', {});
    
    service.clearAllActions();
    
    const actions = service.getPendingActions();
    expect(actions.length).toBe(0);
  });

  it('should cache data with expiry', () => {
    const testData = { name: 'John Doe', id: 1 };
    
    service.cacheData('test-key', testData, 60);
    
    const cachedData = service.getCachedData('test-key');
    expect(cachedData).toEqual(testData);
    expect(service.isCached('test-key')).toBe(true);
  });

  it('should return null for expired cache', () => {
    const testData = { name: 'John Doe', id: 1 };
    
    // Cache with 0 minutes expiry (immediately expired)
    service.cacheData('test-key', testData, 0);
    
    // Wait a bit to ensure expiry
    setTimeout(() => {
      const cachedData = service.getCachedData('test-key');
      expect(cachedData).toBeNull();
      expect(service.isCached('test-key')).toBe(false);
    }, 10);
  });

  it('should store attendance check-in', () => {
    const actionId = service.storeAttendanceCheckIn('Office');
    
    const actions = service.getPendingActions();
    expect(actions.length).toBe(1);
    expect(actions[0].type).toBe('attendance');
    expect(actions[0].action).toBe('check-in');
    expect(actions[0].data.location).toBe('Office');
  });

  it('should store DSR submission', () => {
    const actionId = service.storeDSRSubmission(1, 2, 8, 'Worked on feature X');
    
    const actions = service.getPendingActions();
    expect(actions.length).toBe(1);
    expect(actions[0].type).toBe('dsr');
    expect(actions[0].action).toBe('submit');
    expect(actions[0].data.projectId).toBe(1);
    expect(actions[0].data.taskId).toBe(2);
    expect(actions[0].data.hours).toBe(8);
    expect(actions[0].data.description).toBe('Worked on feature X');
  });

  it('should store leave request', () => {
    const actionId = service.storeLeaveRequest('Annual', '2024-01-15', '2024-01-17', 'Vacation');
    
    const actions = service.getPendingActions();
    expect(actions.length).toBe(1);
    expect(actions[0].type).toBe('leave');
    expect(actions[0].action).toBe('request');
    expect(actions[0].data.leaveType).toBe('Annual');
    expect(actions[0].data.startDate).toBe('2024-01-15');
    expect(actions[0].data.endDate).toBe('2024-01-17');
    expect(actions[0].data.reason).toBe('Vacation');
  });

  it('should cache and retrieve employee profile', () => {
    const profile = { id: 1, name: 'John Doe', email: 'john@example.com' };
    
    service.cacheEmployeeProfile(profile);
    
    const cachedProfile = service.getCachedEmployeeProfile();
    expect(cachedProfile).toEqual(profile);
  });

  it('should cache and retrieve dashboard data', () => {
    const dashboardData = { widgets: [], metrics: {} };
    
    service.cacheDashboardData(dashboardData);
    
    const cachedData = service.getCachedDashboardData();
    expect(cachedData).toEqual(dashboardData);
  });

  it('should cache and retrieve attendance status', () => {
    const status = { isCheckedIn: true, checkInTime: '09:00' };
    
    service.cacheAttendanceStatus(status);
    
    const cachedStatus = service.getCachedAttendanceStatus();
    expect(cachedStatus).toEqual(status);
  });

  it('should clear expired cache items', () => {
    // Cache item with 0 minutes expiry (immediately expired)
    service.cacheData('expired-key', { data: 'test' }, 0);
    service.cacheData('valid-key', { data: 'test' }, 60);
    
    setTimeout(() => {
      service.clearExpiredCache();
      
      expect(service.isCached('expired-key')).toBe(false);
      expect(service.isCached('valid-key')).toBe(true);
    }, 10);
  });

  it('should calculate cache size', () => {
    service.cacheData('test-key', { data: 'test' }, 60);
    
    const size = service.getCacheSize();
    expect(size).toBeGreaterThan(0);
  });

  it('should clear all cache', () => {
    service.cacheData('key1', { data: 'test1' }, 60);
    service.cacheData('key2', { data: 'test2' }, 60);
    
    service.clearCache();
    
    expect(service.isCached('key1')).toBe(false);
    expect(service.isCached('key2')).toBe(false);
    expect(service.getCacheSize()).toBe(0);
  });

  it('should emit pending actions changes', () => {
    let emittedActions: OfflineAction[] = [];
    
    service.pendingActions$.subscribe(actions => {
      emittedActions = actions;
    });
    
    service.storeAction('attendance', 'check-in', {});
    
    expect(emittedActions.length).toBe(1);
    expect(emittedActions[0].type).toBe('attendance');
  });
});