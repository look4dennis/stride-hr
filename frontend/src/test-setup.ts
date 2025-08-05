// Test setup for PWA and service worker mocking
import 'zone.js/testing';
import { getTestBed } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';

// Initialize the Angular testing environment
getTestBed().initTestEnvironment(
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting()
);

// Mock service worker and PWA APIs globally
Object.defineProperty(navigator, 'serviceWorker', {
  writable: true,
  value: {
    ready: Promise.resolve({
      pushManager: {
        getSubscription: () => Promise.resolve(null),
        subscribe: () => Promise.resolve(null)
      },
      showNotification: () => Promise.resolve(),
      update: () => Promise.resolve(),
      unregister: () => Promise.resolve(true)
    }),
    register: () => Promise.resolve({
      pushManager: {
        getSubscription: () => Promise.resolve(null),
        subscribe: () => Promise.resolve(null)
      },
      showNotification: () => Promise.resolve(),
      update: () => Promise.resolve(),
      unregister: () => Promise.resolve(true)
    }),
    getRegistration: () => Promise.resolve(null),
    addEventListener: () => {},
    removeEventListener: () => {}
  }
});

Object.defineProperty(window, 'PushManager', {
  writable: true,
  value: {
    supportedContentEncodings: ['aes128gcm']
  }
});

Object.defineProperty(window, 'Notification', {
  writable: true,
  value: {
    permission: 'default',
    requestPermission: () => Promise.resolve('granted'),
    maxActions: 2
  }
});

// Mock geolocation
Object.defineProperty(navigator, 'geolocation', {
  writable: true,
  value: {
    getCurrentPosition: (success: any) => {
      success({
        coords: {
          latitude: 40.7128,
          longitude: -74.0060,
          accuracy: 10
        }
      });
    },
    watchPosition: () => 1,
    clearWatch: () => {}
  }
});

// Mock IndexedDB for offline storage
Object.defineProperty(window, 'indexedDB', {
  writable: true,
  value: {
    open: () => ({
      onsuccess: null,
      onerror: null,
      result: {
        createObjectStore: () => ({}),
        transaction: () => ({
          objectStore: () => ({
            add: () => ({ onsuccess: null, onerror: null }),
            get: () => ({ onsuccess: null, onerror: null }),
            put: () => ({ onsuccess: null, onerror: null }),
            delete: () => ({ onsuccess: null, onerror: null })
          })
        })
      }
    })
  }
});

// Mock Web Share API
Object.defineProperty(navigator, 'share', {
  writable: true,
  value: () => Promise.resolve()
});

// Mock beforeinstallprompt event
Object.defineProperty(window, 'BeforeInstallPromptEvent', {
  writable: true,
  value: class {
    prompt() { return Promise.resolve(); }
    preventDefault() {}
  }
});

// Mock ResizeObserver
Object.defineProperty(window, 'ResizeObserver', {
  writable: true,
  value: class {
    observe() {}
    unobserve() {}
    disconnect() {}
  }
});

// Mock IntersectionObserver
Object.defineProperty(window, 'IntersectionObserver', {
  writable: true,
  value: class {
    constructor(callback: any) {}
    observe() {}
    unobserve() {}
    disconnect() {}
  }
});

// Mock matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => {}
  })
});

// Mock CSS.supports
Object.defineProperty(CSS, 'supports', {
  writable: true,
  value: () => false
});

// Suppress console errors in tests
const originalError = console.error;
const originalWarn = console.warn;

console.error = (...args: any[]) => {
  // Suppress specific Angular testing errors
  if (args[0]?.includes?.('ChangeDetectionSchedulerImpl') || 
      args[0]?.includes?.('Cannot read properties of undefined') ||
      args[0]?.includes?.('ExpressionChangedAfterItHasBeenCheckedError') ||
      args[0]?.includes?.('NG0100') ||
      args[0]?.includes?.('NG0304')) {
    return;
  }
  originalError.apply(console, args);
};

console.warn = (...args: any[]) => {
  // Suppress specific warnings
  if (args[0]?.includes?.('HttpClientTestingModule') ||
      args[0]?.includes?.('deprecated')) {
    return;
  }
  originalWarn.apply(console, args);
};