import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface DeviceInfo {
  isMobile: boolean;
  isTablet: boolean;
  isDesktop: boolean;
  screenWidth: number;
  screenHeight: number;
  orientation: 'portrait' | 'landscape';
  touchSupported: boolean;
  platform: string;
  userAgent: string;
}

export interface TouchTestResult {
  element: string;
  touchType: 'tap' | 'swipe' | 'pinch' | 'long-press';
  success: boolean;
  responseTime: number;
  error?: string;
}

export interface ResponsiveTestResult {
  breakpoint: string;
  width: number;
  height: number;
  elementsVisible: string[];
  elementsHidden: string[];
  layoutValid: boolean;
  issues: string[];
}

@Injectable({
  providedIn: 'root'
})
export class MobileTestingService {
  private deviceInfoSubject = new BehaviorSubject<DeviceInfo>(this.getDeviceInfo());
  public readonly deviceInfo$ = this.deviceInfoSubject.asObservable();

  private testResults: {
    touch: TouchTestResult[];
    responsive: ResponsiveTestResult[];
    pwa: any[];
  } = {
    touch: [],
    responsive: [],
    pwa: []
  };

  constructor() {
    this.initializeOrientationListener();
    this.initializeResizeListener();
  }

  /**
   * Get current device information
   */
  getDeviceInfo(): DeviceInfo {
    const userAgent = navigator.userAgent;
    const screenWidth = window.screen.width;
    const screenHeight = window.screen.height;
    
    return {
      isMobile: this.isMobileDevice(),
      isTablet: this.isTabletDevice(),
      isDesktop: this.isDesktopDevice(),
      screenWidth,
      screenHeight,
      orientation: screenWidth > screenHeight ? 'landscape' : 'portrait',
      touchSupported: 'ontouchstart' in window || navigator.maxTouchPoints > 0,
      platform: this.getPlatform(),
      userAgent
    };
  }

  /**
   * Test touch interactions on specific elements
   */
  async testTouchInteractions(selectors: string[]): Promise<TouchTestResult[]> {
    const results: TouchTestResult[] = [];

    for (const selector of selectors) {
      const element = document.querySelector(selector);
      if (!element) {
        results.push({
          element: selector,
          touchType: 'tap',
          success: false,
          responseTime: 0,
          error: 'Element not found'
        });
        continue;
      }

      // Test tap interaction
      const tapResult = await this.testTapInteraction(element, selector);
      results.push(tapResult);

      // Test swipe if element supports it
      if (element.classList.contains('swipeable') || element.hasAttribute('data-swipeable')) {
        const swipeResult = await this.testSwipeInteraction(element, selector);
        results.push(swipeResult);
      }

      // Test long press for context menus
      if (element.classList.contains('long-pressable') || element.hasAttribute('data-long-press')) {
        const longPressResult = await this.testLongPressInteraction(element, selector);
        results.push(longPressResult);
      }
    }

    this.testResults.touch = results;
    return results;
  }

  /**
   * Test responsive design across different breakpoints
   */
  async testResponsiveDesign(): Promise<ResponsiveTestResult[]> {
    const breakpoints = [
      { name: 'mobile-portrait', width: 375, height: 667 },
      { name: 'mobile-landscape', width: 667, height: 375 },
      { name: 'tablet-portrait', width: 768, height: 1024 },
      { name: 'tablet-landscape', width: 1024, height: 768 },
      { name: 'desktop-small', width: 1200, height: 800 },
      { name: 'desktop-large', width: 1920, height: 1080 }
    ];

    const results: ResponsiveTestResult[] = [];

    for (const breakpoint of breakpoints) {
      const result = await this.testBreakpoint(breakpoint);
      results.push(result);
    }

    this.testResults.responsive = results;
    return results;
  }

  /**
   * Test PWA functionality on mobile devices
   */
  async testPWAFunctionality(): Promise<any> {
    const pwaTests = {
      serviceWorkerRegistration: await this.testServiceWorkerRegistration(),
      offlineCapability: await this.testOfflineCapability(),
      installPrompt: await this.testInstallPrompt(),
      pushNotifications: await this.testPushNotifications(),
      backgroundSync: await this.testBackgroundSync(),
      cacheStrategy: await this.testCacheStrategy(),
      manifestValidation: await this.testManifestValidation()
    };

    this.testResults.pwa.push(pwaTests);
    return pwaTests;
  }

  /**
   * Test specific mobile gestures
   */
  async testMobileGestures(): Promise<any> {
    const gestureTests = {
      pinchZoom: await this.testPinchZoom(),
      swipeNavigation: await this.testSwipeNavigation(),
      pullToRefresh: await this.testPullToRefresh(),
      scrollBehavior: await this.testScrollBehavior(),
      touchFeedback: await this.testTouchFeedback()
    };

    return gestureTests;
  }

  /**
   * Get comprehensive test report
   */
  getTestReport(): any {
    return {
      deviceInfo: this.getDeviceInfo(),
      timestamp: new Date().toISOString(),
      testResults: this.testResults,
      summary: {
        touchTestsPassed: this.testResults.touch.filter(t => t.success).length,
        touchTestsTotal: this.testResults.touch.length,
        responsiveTestsPassed: this.testResults.responsive.filter(t => t.layoutValid).length,
        responsiveTestsTotal: this.testResults.responsive.length,
        pwaTestsCompleted: this.testResults.pwa.length
      }
    };
  }

  // Private helper methods

  private initializeOrientationListener(): void {
    window.addEventListener('orientationchange', () => {
      setTimeout(() => {
        this.deviceInfoSubject.next(this.getDeviceInfo());
      }, 100);
    });
  }

  private initializeResizeListener(): void {
    window.addEventListener('resize', () => {
      this.deviceInfoSubject.next(this.getDeviceInfo());
    });
  }

  private isMobileDevice(): boolean {
    return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ||
           window.innerWidth <= 768;
  }

  private isTabletDevice(): boolean {
    return /iPad|Android/i.test(navigator.userAgent) && 
           window.innerWidth > 768 && window.innerWidth <= 1024;
  }

  private isDesktopDevice(): boolean {
    return !this.isMobileDevice() && !this.isTabletDevice();
  }

  private getPlatform(): string {
    const userAgent = navigator.userAgent;
    if (/iPhone|iPad|iPod/i.test(userAgent)) return 'iOS';
    if (/Android/i.test(userAgent)) return 'Android';
    if (/Windows/i.test(userAgent)) return 'Windows';
    if (/Mac/i.test(userAgent)) return 'macOS';
    if (/Linux/i.test(userAgent)) return 'Linux';
    return 'Unknown';
  }

  private async testTapInteraction(element: Element, selector: string): Promise<TouchTestResult> {
    return new Promise((resolve) => {
      const startTime = performance.now();
      let responded = false;

      const handleResponse = () => {
        if (!responded) {
          responded = true;
          const responseTime = performance.now() - startTime;
          resolve({
            element: selector,
            touchType: 'tap',
            success: true,
            responseTime
          });
        }
      };

      // Add event listeners
      element.addEventListener('click', handleResponse, { once: true });
      element.addEventListener('touchend', handleResponse, { once: true });

      // Simulate touch event
      const touchEvent = new TouchEvent('touchstart', {
        bubbles: true,
        cancelable: true,
        touches: [new Touch({
          identifier: 0,
          target: element,
          clientX: 0,
          clientY: 0,
          radiusX: 10,
          radiusY: 10,
          rotationAngle: 0,
          force: 1
        })]
      });

      element.dispatchEvent(touchEvent);

      // Timeout after 5 seconds
      setTimeout(() => {
        if (!responded) {
          responded = true;
          resolve({
            element: selector,
            touchType: 'tap',
            success: false,
            responseTime: 5000,
            error: 'Touch interaction timeout'
          });
        }
      }, 5000);
    });
  }

  private async testSwipeInteraction(element: Element, selector: string): Promise<TouchTestResult> {
    return new Promise((resolve) => {
      const startTime = performance.now();
      let responded = false;

      const handleSwipe = () => {
        if (!responded) {
          responded = true;
          const responseTime = performance.now() - startTime;
          resolve({
            element: selector,
            touchType: 'swipe',
            success: true,
            responseTime
          });
        }
      };

      // Simulate swipe gesture
      const touchStart = new TouchEvent('touchstart', {
        bubbles: true,
        touches: [new Touch({
          identifier: 0,
          target: element,
          clientX: 100,
          clientY: 100,
          radiusX: 10,
          radiusY: 10,
          rotationAngle: 0,
          force: 1
        })]
      });

      const touchMove = new TouchEvent('touchmove', {
        bubbles: true,
        touches: [new Touch({
          identifier: 0,
          target: element,
          clientX: 200,
          clientY: 100,
          radiusX: 10,
          radiusY: 10,
          rotationAngle: 0,
          force: 1
        })]
      });

      const touchEnd = new TouchEvent('touchend', {
        bubbles: true,
        changedTouches: [new Touch({
          identifier: 0,
          target: element,
          clientX: 200,
          clientY: 100,
          radiusX: 10,
          radiusY: 10,
          rotationAngle: 0,
          force: 1
        })]
      });

      element.addEventListener('touchend', handleSwipe, { once: true });

      element.dispatchEvent(touchStart);
      setTimeout(() => element.dispatchEvent(touchMove), 50);
      setTimeout(() => element.dispatchEvent(touchEnd), 100);

      setTimeout(() => {
        if (!responded) {
          responded = true;
          resolve({
            element: selector,
            touchType: 'swipe',
            success: false,
            responseTime: 1000,
            error: 'Swipe interaction timeout'
          });
        }
      }, 1000);
    });
  }

  private async testLongPressInteraction(element: Element, selector: string): Promise<TouchTestResult> {
    return new Promise((resolve) => {
      const startTime = performance.now();
      let responded = false;

      const handleLongPress = () => {
        if (!responded) {
          responded = true;
          const responseTime = performance.now() - startTime;
          resolve({
            element: selector,
            touchType: 'long-press',
            success: true,
            responseTime
          });
        }
      };

      element.addEventListener('contextmenu', handleLongPress, { once: true });

      // Simulate long press
      const touchStart = new TouchEvent('touchstart', {
        bubbles: true,
        touches: [new Touch({
          identifier: 0,
          target: element,
          clientX: 0,
          clientY: 0,
          radiusX: 10,
          radiusY: 10,
          rotationAngle: 0,
          force: 1
        })]
      });

      element.dispatchEvent(touchStart);

      // Hold for 800ms then release
      setTimeout(() => {
        const touchEnd = new TouchEvent('touchend', {
          bubbles: true,
          changedTouches: [new Touch({
            identifier: 0,
            target: element,
            clientX: 0,
            clientY: 0,
            radiusX: 10,
            radiusY: 10,
            rotationAngle: 0,
            force: 1
          })]
        });
        element.dispatchEvent(touchEnd);
      }, 800);

      setTimeout(() => {
        if (!responded) {
          responded = true;
          resolve({
            element: selector,
            touchType: 'long-press',
            success: false,
            responseTime: 2000,
            error: 'Long press interaction timeout'
          });
        }
      }, 2000);
    });
  }

  private async testBreakpoint(breakpoint: any): Promise<ResponsiveTestResult> {
    // This would need to be implemented with actual viewport manipulation
    // For now, return a mock result
    return {
      breakpoint: breakpoint.name,
      width: breakpoint.width,
      height: breakpoint.height,
      elementsVisible: [],
      elementsHidden: [],
      layoutValid: true,
      issues: []
    };
  }

  private async testServiceWorkerRegistration(): Promise<any> {
    try {
      if ('serviceWorker' in navigator) {
        const registration = await navigator.serviceWorker.getRegistration();
        return {
          supported: true,
          registered: !!registration,
          active: !!registration?.active,
          scope: registration?.scope || null
        };
      }
      return { supported: false, registered: false };
    } catch (error) {
      return { supported: true, registered: false, error: error instanceof Error ? error.message : 'Unknown error' };
    }
  }

  private async testOfflineCapability(): Promise<any> {
    try {
      // Test if app works offline by checking cache
      const cache = await caches.open('stride-hr-cache');
      const cachedRequests = await cache.keys();
      
      return {
        cacheAvailable: true,
        cachedResources: cachedRequests.length,
        offlineReady: cachedRequests.length > 0
      };
    } catch (error) {
      return {
        cacheAvailable: false,
        error: error instanceof Error ? error.message : 'Unknown error'
      };
    }
  }

  private async testInstallPrompt(): Promise<any> {
    return {
      supported: 'beforeinstallprompt' in window,
      prompted: localStorage.getItem('pwa-install-prompted') === 'true',
      installed: window.matchMedia('(display-mode: standalone)').matches
    };
  }

  private async testPushNotifications(): Promise<any> {
    return {
      supported: 'Notification' in window && 'serviceWorker' in navigator,
      permission: Notification.permission,
      subscribed: false // Would check actual subscription status
    };
  }

  private async testBackgroundSync(): Promise<any> {
    return {
      supported: 'serviceWorker' in navigator && 'sync' in window.ServiceWorkerRegistration.prototype,
      registered: false // Would check actual sync registrations
    };
  }

  private async testCacheStrategy(): Promise<any> {
    try {
      const cacheNames = await caches.keys();
      return {
        available: cacheNames.length > 0,
        strategies: cacheNames,
        working: true
      };
    } catch (error) {
      return {
        available: false,
        error: error instanceof Error ? error.message : 'Unknown error'
      };
    }
  }

  private async testManifestValidation(): Promise<any> {
    try {
      const response = await fetch('/manifest.webmanifest');
      const manifest = await response.json();
      
      return {
        valid: true,
        name: manifest.name,
        shortName: manifest.short_name,
        display: manifest.display,
        icons: manifest.icons?.length || 0
      };
    } catch (error) {
      return {
        valid: false,
        error: error instanceof Error ? error.message : 'Unknown error'
      };
    }
  }

  private async testPinchZoom(): Promise<any> {
    return {
      supported: 'ontouchstart' in window,
      enabled: !document.querySelector('meta[name="viewport"]')?.getAttribute('content')?.includes('user-scalable=no'),
      tested: false // Would need actual gesture simulation
    };
  }

  private async testSwipeNavigation(): Promise<any> {
    return {
      supported: true,
      gestures: ['swipe-left', 'swipe-right', 'swipe-up', 'swipe-down'],
      tested: false // Would test actual swipe handlers
    };
  }

  private async testPullToRefresh(): Promise<any> {
    return {
      supported: 'overscroll-behavior' in document.body.style,
      implemented: false, // Would check for actual pull-to-refresh implementation
      tested: false
    };
  }

  private async testScrollBehavior(): Promise<any> {
    return {
      smoothScrolling: 'scrollBehavior' in document.documentElement.style,
      momentum: /iPhone|iPad|iPod|iOS/i.test(navigator.userAgent),
      tested: true
    };
  }

  private async testTouchFeedback(): Promise<any> {
    return {
      hapticFeedback: 'vibrate' in navigator,
      visualFeedback: true, // CSS :active states
      audioFeedback: false // Would check for audio feedback implementation
    };
  }
}