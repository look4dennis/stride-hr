// Custom Service Worker for StrideHR PWA
// This extends the Angular service worker with additional functionality

importScripts('./ngsw-worker.js');

// Handle push notifications
self.addEventListener('push', event => {
  console.log('Push notification received:', event);
  
  if (!event.data) {
    return;
  }

  const data = event.data.json();
  const options = {
    body: data.body,
    icon: data.icon || '/assets/icons/icon-192x192.png',
    badge: '/assets/icons/icon-192x192.png',
    vibrate: [200, 100, 200],
    data: data.data,
    actions: data.actions || [],
    requireInteraction: data.requireInteraction || false,
    tag: data.tag || 'stride-hr-notification'
  };

  event.waitUntil(
    self.registration.showNotification(data.title, options)
  );
});

// Handle notification clicks
self.addEventListener('notificationclick', event => {
  console.log('Notification clicked:', event);
  
  event.notification.close();
  
  const action = event.action;
  const data = event.notification.data;
  
  if (action === 'checkin') {
    // Handle check-in action
    event.waitUntil(
      clients.openWindow('/attendance')
    );
  } else if (action === 'submit-dsr') {
    // Handle DSR submission action
    event.waitUntil(
      clients.openWindow('/dsr')
    );
  } else if (action === 'approve') {
    // Handle approval action
    event.waitUntil(
      clients.openWindow('/approvals')
    );
  } else if (action === 'send-wishes') {
    // Handle birthday wishes action
    event.waitUntil(
      clients.openWindow('/dashboard')
    );
  } else {
    // Default action - open the app
    event.waitUntil(
      clients.openWindow('/')
    );
  }
});

// Handle background sync
self.addEventListener('sync', event => {
  console.log('Background sync triggered:', event.tag);
  
  if (event.tag === 'stride-hr-sync') {
    event.waitUntil(syncOfflineData());
  }
});

// Sync offline data
async function syncOfflineData() {
  try {
    console.log('Starting offline data sync...');
    
    // Get offline data from IndexedDB
    const offlineData = await getOfflineData();
    
    if (offlineData.length === 0) {
      console.log('No offline data to sync');
      return;
    }
    
    // Process each offline action
    for (const item of offlineData) {
      try {
        await syncSingleItem(item);
        await markItemSynced(item.id);
      } catch (error) {
        console.error('Error syncing item:', item, error);
      }
    }
    
    // Show sync completion notification
    await self.registration.showNotification('Data Synchronized', {
      body: `${offlineData.length} offline actions have been synchronized.`,
      icon: '/assets/icons/icon-192x192.png',
      badge: '/assets/icons/icon-192x192.png',
      tag: 'sync-complete'
    });
    
    console.log('Offline data sync completed');
  } catch (error) {
    console.error('Error during offline sync:', error);
  }
}

// Sync a single offline item
async function syncSingleItem(item) {
  const apiUrl = 'http://localhost:5000/api'; // This should come from environment
  
  switch (item.action) {
    case 'attendance-checkin':
      await fetch(`${apiUrl}/attendance/checkin`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${await getStoredToken()}`
        },
        body: JSON.stringify(item.data)
      });
      break;
      
    case 'attendance-checkout':
      await fetch(`${apiUrl}/attendance/checkout`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${await getStoredToken()}`
        },
        body: JSON.stringify(item.data)
      });
      break;
      
    case 'dsr-submit':
      await fetch(`${apiUrl}/dsr`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${await getStoredToken()}`
        },
        body: JSON.stringify(item.data)
      });
      break;
      
    case 'leave-request':
      await fetch(`${apiUrl}/leave/request`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${await getStoredToken()}`
        },
        body: JSON.stringify(item.data)
      });
      break;
      
    default:
      console.warn('Unknown sync action:', item.action);
  }
}

// Get offline data from IndexedDB
async function getOfflineData() {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open('stride-hr-offline', 1);
    
    request.onerror = () => reject(request.error);
    
    request.onsuccess = () => {
      const db = request.result;
      const transaction = db.transaction(['offline-actions'], 'readonly');
      const store = transaction.objectStore('offline-actions');
      const getAllRequest = store.getAll();
      
      getAllRequest.onsuccess = () => {
        resolve(getAllRequest.result.filter(item => !item.synced));
      };
      
      getAllRequest.onerror = () => reject(getAllRequest.error);
    };
    
    request.onupgradeneeded = () => {
      const db = request.result;
      if (!db.objectStoreNames.contains('offline-actions')) {
        const store = db.createObjectStore('offline-actions', { keyPath: 'id' });
        store.createIndex('synced', 'synced', { unique: false });
      }
    };
  });
}

// Mark item as synced in IndexedDB
async function markItemSynced(itemId) {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open('stride-hr-offline', 1);
    
    request.onsuccess = () => {
      const db = request.result;
      const transaction = db.transaction(['offline-actions'], 'readwrite');
      const store = transaction.objectStore('offline-actions');
      
      const getRequest = store.get(itemId);
      getRequest.onsuccess = () => {
        const item = getRequest.result;
        if (item) {
          item.synced = true;
          const putRequest = store.put(item);
          putRequest.onsuccess = () => resolve();
          putRequest.onerror = () => reject(putRequest.error);
        } else {
          resolve();
        }
      };
    };
  });
}

// Get stored authentication token
async function getStoredToken() {
  // This would retrieve the token from localStorage or IndexedDB
  // For now, return a placeholder
  return localStorage.getItem('auth-token') || '';
}

// Handle periodic background sync for reminders
self.addEventListener('periodicsync', event => {
  if (event.tag === 'attendance-reminder') {
    event.waitUntil(sendAttendanceReminder());
  } else if (event.tag === 'dsr-reminder') {
    event.waitUntil(sendDSRReminder());
  }
});

// Send attendance reminder
async function sendAttendanceReminder() {
  const now = new Date();
  const hour = now.getHours();
  
  // Send reminder at 9 AM if not checked in
  if (hour === 9) {
    await self.registration.showNotification('Attendance Reminder', {
      body: 'Don\'t forget to check in for today!',
      icon: '/assets/icons/icon-192x192.png',
      badge: '/assets/icons/icon-192x192.png',
      tag: 'attendance-reminder',
      actions: [
        {
          action: 'checkin',
          title: 'Check In Now',
          icon: '/assets/icons/icon-192x192.png'
        }
      ]
    });
  }
}

// Send DSR reminder
async function sendDSRReminder() {
  const now = new Date();
  const hour = now.getHours();
  
  // Send reminder at 6 PM
  if (hour === 18) {
    await self.registration.showNotification('DSR Reminder', {
      body: 'Please submit your Daily Status Report',
      icon: '/assets/icons/icon-192x192.png',
      badge: '/assets/icons/icon-192x192.png',
      tag: 'dsr-reminder',
      actions: [
        {
          action: 'submit-dsr',
          title: 'Submit DSR',
          icon: '/assets/icons/icon-192x192.png'
        }
      ]
    });
  }
}

// Handle fetch events for offline functionality
self.addEventListener('fetch', event => {
  // Let the Angular service worker handle most requests
  // Only intercept specific requests that need custom offline handling
  
  if (event.request.url.includes('/api/attendance/status')) {
    event.respondWith(
      caches.match(event.request)
        .then(response => {
          if (response) {
            return response;
          }
          return fetch(event.request)
            .then(response => {
              const responseClone = response.clone();
              caches.open('stride-hr-api-cache')
                .then(cache => {
                  cache.put(event.request, responseClone);
                });
              return response;
            });
        })
        .catch(() => {
          // Return cached offline status if available
          return caches.match('/offline-status.json');
        })
    );
  }
});

console.log('StrideHR Custom Service Worker loaded');