import { TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';

/**
 * Standard test configuration for Angular components
 */
export class TestConfig {
  
  /**
   * Get standard test configuration for components
   */
  static getStandardConfig(additionalImports: any[] = [], additionalProviders: any[] = []) {
    return {
      imports: [
        NoopAnimationsModule,
        NgbModule,
        FormsModule,
        ReactiveFormsModule,
        ...additionalImports
      ],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            params: of({}),
            queryParams: of({}),
            snapshot: {
              params: {},
              queryParams: {},
              data: {}
            }
          }
        },
        ...additionalProviders
      ]
    };
  }

  /**
   * Configure TestBed for standalone components
   */
  static async configureStandaloneComponent(
    componentType: any,
    additionalImports: any[] = [],
    additionalProviders: any[] = []
  ) {
    const config = this.getStandardConfig(additionalImports, additionalProviders);
    config.imports.push(componentType);
    
    await TestBed.configureTestingModule(config).compileComponents();
    return TestBed.createComponent(componentType);
  }

  /**
   * Configure TestBed for non-standalone components
   */
  static async configureComponent(
    componentType: any,
    additionalImports: any[] = [],
    additionalProviders: any[] = []
  ) {
    const config = this.getStandardConfig(additionalImports, additionalProviders);
    const testConfig = {
      ...config,
      declarations: [componentType]
    };
    
    await TestBed.configureTestingModule(testConfig).compileComponents();
    return TestBed.createComponent(componentType);
  }

  /**
   * Mock common services
   */
  static getMockServices() {
    return {
      notificationService: {
        showSuccess: jasmine.createSpy('showSuccess'),
        showError: jasmine.createSpy('showError'),
        showWarning: jasmine.createSpy('showWarning'),
        showInfo: jasmine.createSpy('showInfo')
      },
      authService: {
        getCurrentUser: jasmine.createSpy('getCurrentUser').and.returnValue(of({
          id: 1,
          username: 'testuser',
          email: 'test@example.com',
          roles: ['user']
        })),
        isAuthenticated: jasmine.createSpy('isAuthenticated').and.returnValue(true),
        hasRole: jasmine.createSpy('hasRole').and.returnValue(true),
        logout: jasmine.createSpy('logout')
      },
      signalRService: {
        startConnection: jasmine.createSpy('startConnection').and.returnValue(Promise.resolve()),
        stopConnection: jasmine.createSpy('stopConnection').and.returnValue(Promise.resolve()),
        addListener: jasmine.createSpy('addListener'),
        removeListener: jasmine.createSpy('removeListener'),
        sendMessage: jasmine.createSpy('sendMessage').and.returnValue(Promise.resolve())
      }
    };
  }

  /**
   * Setup browser API mocks
   */
  static setupBrowserMocks() {
    // Mock matchMedia
    Object.defineProperty(window, 'matchMedia', {
      writable: true,
      value: jasmine.createSpy('matchMedia').and.returnValue({
        matches: false,
        media: '',
        onchange: null,
        addListener: jasmine.createSpy('addListener'),
        removeListener: jasmine.createSpy('removeListener'),
        addEventListener: jasmine.createSpy('addEventListener'),
        removeEventListener: jasmine.createSpy('removeEventListener'),
        dispatchEvent: jasmine.createSpy('dispatchEvent')
      })
    });

    // Mock ResizeObserver
    Object.defineProperty(window, 'ResizeObserver', {
      writable: true,
      value: jasmine.createSpy('ResizeObserver').and.returnValue({
        observe: jasmine.createSpy('observe'),
        unobserve: jasmine.createSpy('unobserve'),
        disconnect: jasmine.createSpy('disconnect')
      })
    });

    // Mock IntersectionObserver
    Object.defineProperty(window, 'IntersectionObserver', {
      writable: true,
      value: jasmine.createSpy('IntersectionObserver').and.returnValue({
        observe: jasmine.createSpy('observe'),
        unobserve: jasmine.createSpy('unobserve'),
        disconnect: jasmine.createSpy('disconnect')
      })
    });

    // Mock CSS.supports
    if (!CSS.supports) {
      Object.defineProperty(CSS, 'supports', {
        writable: true,
        value: jasmine.createSpy('supports').and.returnValue(false)
      });
    }
  }

  /**
   * Setup PWA mocks
   */
  static setupPWAMocks() {
    // Mock service worker
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

    // Mock PushManager
    Object.defineProperty(window, 'PushManager', {
      writable: true,
      value: {
        supportedContentEncodings: ['aes128gcm']
      }
    });

    // Mock Notification API
    Object.defineProperty(window, 'Notification', {
      writable: true,
      value: {
        permission: 'default',
        requestPermission: () => Promise.resolve('granted'),
        maxActions: 2
      }
    });

    // Mock beforeinstallprompt event
    Object.defineProperty(window, 'BeforeInstallPromptEvent', {
      writable: true,
      value: class {
        prompt() { return Promise.resolve(); }
        preventDefault() {}
      }
    });
  }

  /**
   * Setup all common mocks
   */
  static setupAllMocks() {
    this.setupBrowserMocks();
    this.setupPWAMocks();
  }
}