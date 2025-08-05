import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';

/**
 * Common test utilities and setup helpers
 */
export class TestUtils {

  /**
   * Standard test bed configuration for components
   */
  static getStandardTestConfig(additionalImports: any[] = [], additionalProviders: any[] = []) {
    return {
      imports: [
        NoopAnimationsModule, // Use NoopAnimationsModule instead of BrowserAnimationsModule for tests
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
   * Setup for standalone components
   */
  static async setupStandaloneComponent<T>(
    componentType: any,
    additionalImports: any[] = [],
    additionalProviders: any[] = []
  ): Promise<{ fixture: ComponentFixture<T>; component: T; httpMock: HttpTestingController }> {

    const config = this.getStandardTestConfig(additionalImports, additionalProviders);
    config.imports.push(componentType);

    await TestBed.configureTestingModule(config).compileComponents();

    const fixture = TestBed.createComponent(componentType) as ComponentFixture<T>;
    const component = fixture.componentInstance;
    const httpMock = TestBed.inject(HttpTestingController);

    return { fixture, component, httpMock };
  }

  /**
   * Setup for non-standalone components
   */
  static async setupComponent<T>(
    componentType: any,
    additionalImports: any[] = [],
    additionalProviders: any[] = []
  ): Promise<{ fixture: ComponentFixture<T>; component: T; httpMock: HttpTestingController }> {

    const config = this.getStandardTestConfig(additionalImports, additionalProviders);
    const testConfig = {
      ...config,
      declarations: [componentType]
    };

    await TestBed.configureTestingModule(testConfig).compileComponents();

    const fixture = TestBed.createComponent(componentType) as ComponentFixture<T>;
    const component = fixture.componentInstance;
    const httpMock = TestBed.inject(HttpTestingController);

    return { fixture, component, httpMock };
  }

  /**
   * Setup for services with HTTP testing
   */
  static setupServiceWithHttp<T>(serviceType: any, additionalProviders: any[] = []): { service: T; httpMock: HttpTestingController } {
    TestBed.configureTestingModule({
      providers: [
        serviceType,
        provideHttpClient(),
        provideHttpClientTesting(),
        ...additionalProviders
      ]
    });

    const service = TestBed.inject(serviceType) as T;
    const httpMock = TestBed.inject(HttpTestingController);

    return { service, httpMock };
  }

  /**
   * Mock ActivatedRoute with custom data
   */
  static mockActivatedRoute(params: any = {}, queryParams: any = {}, data: any = {}) {
    return {
      provide: ActivatedRoute,
      useValue: {
        params: of(params),
        queryParams: of(queryParams),
        snapshot: {
          params,
          queryParams,
          data
        }
      }
    };
  }

  /**
   * Mock common services
   */
  static mockNotificationService() {
    return {
      showSuccess: jasmine.createSpy('showSuccess'),
      showError: jasmine.createSpy('showError'),
      showWarning: jasmine.createSpy('showWarning'),
      showInfo: jasmine.createSpy('showInfo')
    };
  }

  static mockAuthService() {
    return {
      getCurrentUser: jasmine.createSpy('getCurrentUser').and.returnValue(of({
        id: 1,
        username: 'testuser',
        email: 'test@example.com',
        roles: ['user']
      })),
      isAuthenticated: jasmine.createSpy('isAuthenticated').and.returnValue(true),
      hasRole: jasmine.createSpy('hasRole').and.returnValue(true),
      logout: jasmine.createSpy('logout')
    };
  }

  static mockSignalRService() {
    return {
      startConnection: jasmine.createSpy('startConnection').and.returnValue(Promise.resolve()),
      stopConnection: jasmine.createSpy('stopConnection').and.returnValue(Promise.resolve()),
      addListener: jasmine.createSpy('addListener'),
      removeListener: jasmine.createSpy('removeListener'),
      sendMessage: jasmine.createSpy('sendMessage').and.returnValue(Promise.resolve())
    };
  }

  /**
   * Cleanup HTTP mocks safely
   */
  static cleanupHttpMock(httpMock: HttpTestingController) {
    try {
      httpMock.verify();
    } catch (error) {
      // Flush any pending requests
      const pendingRequests = httpMock.match(() => true);
      pendingRequests.forEach(req => {
        try {
          req.flush({});
        } catch (flushError) {
          // Ignore flush errors
        }
      });
    }
  }

  /**
   * Wait for async operations to complete
   */
  static async waitForAsync(fixture: ComponentFixture<any>, fn?: () => void): Promise<void> {
    if (fn) fn();
    await fixture.whenStable();
    fixture.detectChanges();
  }

  /**
   * Simulate user input
   */
  static setInputValue(fixture: ComponentFixture<any>, selector: string, value: string) {
    const input = fixture.nativeElement.querySelector(selector);
    if (input) {
      input.value = value;
      input.dispatchEvent(new Event('input'));
      input.dispatchEvent(new Event('blur'));
      fixture.detectChanges();
    }
  }

  /**
   * Simulate button click
   */
  static clickButton(fixture: ComponentFixture<any>, selector: string) {
    const button = fixture.nativeElement.querySelector(selector);
    if (button) {
      button.click();
      fixture.detectChanges();
    }
  }

  /**
   * Get element text content
   */
  static getElementText(fixture: ComponentFixture<any>, selector: string): string {
    const element = fixture.nativeElement.querySelector(selector);
    return element ? element.textContent.trim() : '';
  }

  /**
   * Check if element exists
   */
  static elementExists(fixture: ComponentFixture<any>, selector: string): boolean {
    return !!fixture.nativeElement.querySelector(selector);
  }

  /**
   * Mock window.matchMedia for responsive tests
   */
  static mockMatchMedia(matches: boolean = false) {
    Object.defineProperty(window, 'matchMedia', {
      writable: true,
      value: jasmine.createSpy('matchMedia').and.returnValue({
        matches,
        media: '',
        onchange: null,
        addListener: jasmine.createSpy('addListener'),
        removeListener: jasmine.createSpy('removeListener'),
        addEventListener: jasmine.createSpy('addEventListener'),
        removeEventListener: jasmine.createSpy('removeEventListener'),
        dispatchEvent: jasmine.createSpy('dispatchEvent')
      })
    });
  }

  /**
   * Mock ResizeObserver
   */
  static mockResizeObserver() {
    Object.defineProperty(window, 'ResizeObserver', {
      writable: true,
      value: jasmine.createSpy('ResizeObserver').and.returnValue({
        observe: jasmine.createSpy('observe'),
        unobserve: jasmine.createSpy('unobserve'),
        disconnect: jasmine.createSpy('disconnect')
      })
    });
  }

  /**
   * Mock IntersectionObserver
   */
  static mockIntersectionObserver() {
    Object.defineProperty(window, 'IntersectionObserver', {
      writable: true,
      value: jasmine.createSpy('IntersectionObserver').and.returnValue({
        observe: jasmine.createSpy('observe'),
        unobserve: jasmine.createSpy('unobserve'),
        disconnect: jasmine.createSpy('disconnect')
      })
    });
  }

  /**
   * Create mock HTTP response
   */
  static mockHttpResponse(httpMock: HttpTestingController, url: string, method: string, responseData: any, statusCode: number = 200) {
    const req = httpMock.expectOne(req => req.url.includes(url) && req.method === method);
    if (statusCode >= 200 && statusCode < 300) {
      req.flush(responseData);
    } else {
      req.flush(responseData, { status: statusCode, statusText: 'Error' });
    }
  }

  /**
   * Create mock HTTP error
   */
  static mockHttpError(httpMock: HttpTestingController, url: string, method: string, errorMessage: string, statusCode: number = 500) {
    const req = httpMock.expectOne(req => req.url.includes(url) && req.method === method);
    req.flush({ message: errorMessage }, { status: statusCode, statusText: 'Error' });
  }
}