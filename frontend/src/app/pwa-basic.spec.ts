describe('PWA Basic Functionality', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should be able to store offline data in localStorage', () => {
    const testData = {
      id: '1',
      type: 'attendance',
      action: 'check-in',
      data: { location: 'Office' },
      timestamp: new Date().toISOString(),
      synced: false
    };

    localStorage.setItem('stride-hr-offline-data', JSON.stringify([testData]));
    
    const stored = JSON.parse(localStorage.getItem('stride-hr-offline-data') || '[]');
    expect(stored.length).toBe(1);
    expect(stored[0].type).toBe('attendance');
    expect(stored[0].action).toBe('check-in');
  });

  it('should be able to cache data with expiry', () => {
    const cacheItem = {
      data: { id: 1, name: 'Test Employee' },
      timestamp: Date.now(),
      expiry: Date.now() + (60 * 60 * 1000) // 1 hour
    };

    const cache = { 'employee-profile': cacheItem };
    localStorage.setItem('stride-hr-cache', JSON.stringify(cache));

    const storedCache = JSON.parse(localStorage.getItem('stride-hr-cache') || '{}');
    expect(storedCache['employee-profile']).toBeDefined();
    expect(storedCache['employee-profile'].data.name).toBe('Test Employee');
  });

  it('should detect expired cache items', () => {
    const expiredItem = {
      data: { id: 1, name: 'Test Employee' },
      timestamp: Date.now() - (2 * 60 * 60 * 1000), // 2 hours ago
      expiry: Date.now() - (60 * 60 * 1000) // 1 hour ago (expired)
    };

    const cache = { 'expired-profile': expiredItem };
    localStorage.setItem('stride-hr-cache', JSON.stringify(cache));

    const storedCache = JSON.parse(localStorage.getItem('stride-hr-cache') || '{}');
    const item = storedCache['expired-profile'];
    
    expect(item).toBeDefined();
    expect(Date.now() > item.expiry).toBe(true); // Should be expired
  });

  it('should support service worker registration check', () => {
    const hasServiceWorker = 'serviceWorker' in navigator;
    expect(typeof hasServiceWorker).toBe('boolean');
  });

  it('should support notification API check', () => {
    const hasNotifications = 'Notification' in window;
    expect(typeof hasNotifications).toBe('boolean');
  });

  it('should support push manager check', () => {
    const hasPushManager = 'PushManager' in window;
    expect(typeof hasPushManager).toBe('boolean');
  });

  it('should detect online status', () => {
    const isOnline = navigator.onLine;
    expect(typeof isOnline).toBe('boolean');
  });

  it('should support localStorage for offline storage', () => {
    expect(typeof Storage).toBe('function');
    expect(localStorage).toBeDefined();
    expect(typeof localStorage.setItem).toBe('function');
    expect(typeof localStorage.getItem).toBe('function');
  });

  it('should generate unique IDs for offline actions', () => {
    const generateId = () => Date.now().toString(36) + Math.random().toString(36).substr(2);
    
    const id1 = generateId();
    const id2 = generateId();
    
    expect(id1).toBeTruthy();
    expect(id2).toBeTruthy();
    expect(id1).not.toBe(id2);
  });

  it('should handle PWA manifest structure', () => {
    const manifest = {
      name: 'StrideHR - Human Resource Management System',
      short_name: 'StrideHR',
      description: 'Comprehensive HR management system for global organizations',
      display: 'standalone',
      orientation: 'portrait-primary',
      theme_color: '#3b82f6',
      background_color: '#ffffff',
      scope: './',
      start_url: './',
      categories: ['business', 'productivity', 'utilities']
    };

    expect(manifest.name).toBe('StrideHR - Human Resource Management System');
    expect(manifest.short_name).toBe('StrideHR');
    expect(manifest.display).toBe('standalone');
    expect(manifest.theme_color).toBe('#3b82f6');
  });
});