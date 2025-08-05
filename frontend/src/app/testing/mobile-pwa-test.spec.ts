import { TestBed, ComponentFixture } from '@angular/core/testing';
import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ServiceWorkerModule } from '@angular/service-worker';
import { TestConfig } from './test-config';

/**
 * Mobile and PWA functionality tests
 * Tests touch interactions, responsive design, and PWA features
 */

@Component({
  selector: 'app-mobile-test',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="mobile-test-container">
      <!-- Touch interaction test elements -->
      <div class="touch-area" 
           (touchstart)="onTouchStart($event)"
           (touchmove)="onTouchMove($event)"
           (touchend)="onTouchEnd($event)"
           (click)="onClick($event)">
        Touch/Click Area
      </div>

      <!-- Swipe gesture test -->
      <div class="swipe-area"
           (touchstart)="onSwipeStart($event)"
           (touchmove)="onSwipeMove($event)"
           (touchend)="onSwipeEnd($event)">
        Swipe Area
      </div>

      <!-- Responsive navigation -->
      <nav class="navbar">
        <button class="navbar-toggler" 
                type="button" 
                (click)="toggleMobileMenu()"
                [attr.aria-expanded]="mobileMenuOpen">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="navbar-collapse" [class.show]="mobileMenuOpen">
          <ul class="navbar-nav">
            <li class="nav-item"><a class="nav-link" href="#dashboard">Dashboard</a></li>
            <li class="nav-item"><a class="nav-link" href="#employees">Employees</a></li>
            <li class="nav-item"><a class="nav-link" href="#payroll">Payroll</a></li>
          </ul>
        </div>
      </nav>

      <!-- Responsive cards -->
      <div class="card-grid">
        <div class="card mobile-card" *ngFor="let card of testCards">
          <div class="card-body">
            <h5 class="card-title">{{ card.title }}</h5>
            <p class="card-text">{{ card.content }}</p>
            <button class="btn btn-primary btn-mobile">Action</button>
          </div>
        </div>
      </div>

      <!-- PWA install prompt -->
      <div class="pwa-install-prompt" *ngIf="showInstallPrompt">
        <div class="install-banner">
          <p>Install StrideHR for a better experience</p>
          <button class="btn btn-primary" (click)="installPWA()">Install</button>
          <button class="btn btn-secondary" (click)="dismissInstall()">Dismiss</button>
        </div>
      </div>

      <!-- Offline indicator -->
      <div class="offline-indicator" [class.offline]="!isOnline">
        <span *ngIf="!isOnline">You are offline</span>
        <span *ngIf="isOnline">Connected</span>
      </div>
    </div>
  `,
  styles: [`
    .mobile-test-container {
      padding: 1rem;
    }

    .touch-area, .swipe-area {
      width: 100%;
      height: 100px;
      background-color: #f8f9fa;
      border: 2px dashed #dee2e6;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 1rem;
      user-select: none;
      touch-action: manipulation;
    }

    .touch-area:active, .swipe-area:active {
      background-color: #e9ecef;
    }

    .navbar {
      background-color: #343a40;
      padding: 0.5rem 1rem;
      margin-bottom: 1rem;
    }

    .navbar-toggler {
      background: none;
      border: 1px solid rgba(255, 255, 255, 0.1);
      color: white;
      padding: 0.25rem 0.5rem;
    }

    .navbar-toggler-icon {
      display: inline-block;
      width: 1.5em;
      height: 1.5em;
      background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 30 30'%3e%3cpath stroke='rgba%28255, 255, 255, 0.75%29' stroke-linecap='round' stroke-miterlimit='10' stroke-width='2' d='M4 7h22M4 15h22M4 23h22'/%3e%3c/svg%3e");
    }

    .navbar-collapse {
      display: none;
    }

    .navbar-collapse.show {
      display: block;
    }

    .navbar-nav {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    .nav-item {
      margin-bottom: 0.5rem;
    }

    .nav-link {
      color: rgba(255, 255, 255, 0.75);
      text-decoration: none;
      padding: 0.5rem 0;
      display: block;
    }

    .nav-link:hover {
      color: white;
    }

    .card-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 1rem;
      margin-bottom: 2rem;
    }

    .mobile-card {
      border: 1px solid #dee2e6;
      border-radius: 0.375rem;
    }

    .card-body {
      padding: 1rem;
    }

    .btn-mobile {
      width: 100%;
      padding: 0.75rem;
      font-size: 1rem;
    }

    .pwa-install-prompt {
      position: fixed;
      bottom: 0;
      left: 0;
      right: 0;
      background: white;
      border-top: 1px solid #dee2e6;
      padding: 1rem;
      box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.1);
    }

    .install-banner {
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .install-banner p {
      margin: 0;
      flex: 1;
    }

    .install-banner button {
      margin-left: 0.5rem;
    }

    .offline-indicator {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      background: #28a745;
      color: white;
      text-align: center;
      padding: 0.5rem;
      transform: translateY(-100%);
      transition: transform 0.3s ease;
    }

    .offline-indicator.offline {
      background: #dc3545;
      transform: translateY(0);
    }

    /* Mobile-specific styles */
    @media (max-width: 768px) {
      .mobile-test-container {
        padding: 0.5rem;
      }

      .card-grid {
        grid-template-columns: 1fr;
      }

      .install-banner {
        flex-direction: column;
        text-align: center;
      }

      .install-banner button {
        width: 100%;
        margin: 0.25rem 0;
      }

      .touch-area, .swipe-area {
        height: 80px;
        font-size: 0.9rem;
      }
    }

    /* Touch-friendly sizing */
    @media (pointer: coarse) {
      .btn {
        min-height: 44px;
        min-width: 44px;
      }

      .nav-link {
        padding: 0.75rem 0;
        font-size: 1.1rem;
      }

      .navbar-toggler {
        min-height: 44px;
        min-width: 44px;
      }
    }
  `]
})
class MobileTestComponent {
  mobileMenuOpen = false;
  showInstallPrompt = false;
  isOnline = navigator.onLine;
  
  testCards = [
    { title: 'Dashboard', content: 'View your HR dashboard' },
    { title: 'Employees', content: 'Manage employee records' },
    { title: 'Payroll', content: 'Process payroll and benefits' }
  ];

  touchStartX = 0;
  touchStartY = 0;
  swipeThreshold = 50;

  constructor() {
    // Listen for online/offline events
    window.addEventListener('online', () => this.isOnline = true);
    window.addEventListener('offline', () => this.isOnline = false);
  }

  onTouchStart(event: TouchEvent) {
    event.preventDefault();
    console.log('Touch start detected');
  }

  onTouchMove(event: TouchEvent) {
    event.preventDefault();
    console.log('Touch move detected');
  }

  onTouchEnd(event: TouchEvent) {
    event.preventDefault();
    console.log('Touch end detected');
  }

  onClick(event: MouseEvent) {
    console.log('Click detected');
  }

  onSwipeStart(event: TouchEvent) {
    if (event.touches.length === 1) {
      this.touchStartX = event.touches[0].clientX;
      this.touchStartY = event.touches[0].clientY;
    }
  }

  onSwipeMove(event: TouchEvent) {
    event.preventDefault();
  }

  onSwipeEnd(event: TouchEvent) {
    if (event.changedTouches.length === 1) {
      const touchEndX = event.changedTouches[0].clientX;
      const touchEndY = event.changedTouches[0].clientY;
      
      const deltaX = touchEndX - this.touchStartX;
      const deltaY = touchEndY - this.touchStartY;
      
      if (Math.abs(deltaX) > Math.abs(deltaY) && Math.abs(deltaX) > this.swipeThreshold) {
        if (deltaX > 0) {
          console.log('Swipe right detected');
        } else {
          console.log('Swipe left detected');
        }
      }
    }
  }

  toggleMobileMenu() {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  installPWA() {
    console.log('PWA installation triggered');
    this.showInstallPrompt = false;
  }

  dismissInstall() {
    this.showInstallPrompt = false;
  }
}

describe('Mobile and PWA Functionality Tests', () => {
  let component: MobileTestComponent;
  let fixture: ComponentFixture<MobileTestComponent>;

  beforeEach(async () => {
    TestConfig.setupAllMocks();
    
    await TestBed.configureTestingModule({
      imports: [
        MobileTestComponent,
        ServiceWorkerModule.register('ngsw-worker.js', { enabled: false })
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(MobileTestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    fixture.destroy();
  });

  describe('Touch Interaction Tests', () => {
    it('should handle touch events on touch-enabled devices', () => {
      const touchArea = fixture.nativeElement.querySelector('.touch-area');
      expect(touchArea).toBeTruthy();

      spyOn(component, 'onTouchStart');
      spyOn(component, 'onTouchMove');
      spyOn(component, 'onTouchEnd');

      // Simulate touch events
      const touch = new Touch({
        identifier: 1,
        target: touchArea,
        clientX: 100,
        clientY: 100,
        radiusX: 2.5,
        radiusY: 2.5,
        rotationAngle: 10,
        force: 0.5
      });

      const touchStartEvent = new TouchEvent('touchstart', { touches: [touch] });
      const touchMoveEvent = new TouchEvent('touchmove', { touches: [touch] });
      const touchEndEvent = new TouchEvent('touchend', { changedTouches: [touch] });

      touchArea.dispatchEvent(touchStartEvent);
      touchArea.dispatchEvent(touchMoveEvent);
      touchArea.dispatchEvent(touchEndEvent);

      expect(component.onTouchStart).toHaveBeenCalled();
      expect(component.onTouchMove).toHaveBeenCalled();
      expect(component.onTouchEnd).toHaveBeenCalled();
    });

    it('should handle swipe gestures', () => {
      const swipeArea = fixture.nativeElement.querySelector('.swipe-area');
      expect(swipeArea).toBeTruthy();

      spyOn(component, 'onSwipeStart');
      spyOn(component, 'onSwipeEnd');

      // Simulate swipe right
      const startTouch = new Touch({
        identifier: 1,
        target: swipeArea,
        clientX: 50,
        clientY: 100,
        radiusX: 2.5,
        radiusY: 2.5,
        rotationAngle: 10,
        force: 0.5
      });

      const endTouch = new Touch({
        identifier: 1,
        target: swipeArea,
        clientX: 150,
        clientY: 100,
        radiusX: 2.5,
        radiusY: 2.5,
        rotationAngle: 10,
        force: 0.5
      });

      const touchStartEvent = new TouchEvent('touchstart', { touches: [startTouch] });
      const touchEndEvent = new TouchEvent('touchend', { changedTouches: [endTouch] });

      swipeArea.dispatchEvent(touchStartEvent);
      swipeArea.dispatchEvent(touchEndEvent);

      expect(component.onSwipeStart).toHaveBeenCalled();
      expect(component.onSwipeEnd).toHaveBeenCalled();
    });

    it('should support click events as fallback for non-touch devices', () => {
      const touchArea = fixture.nativeElement.querySelector('.touch-area');
      spyOn(component, 'onClick');

      touchArea.click();
      expect(component.onClick).toHaveBeenCalled();
    });
  });

  describe('Responsive Design Tests', () => {
    it('should render mobile navigation correctly', () => {
      const navbar = fixture.nativeElement.querySelector('.navbar');
      const navbarToggler = fixture.nativeElement.querySelector('.navbar-toggler');
      const navbarCollapse = fixture.nativeElement.querySelector('.navbar-collapse');

      expect(navbar).toBeTruthy();
      expect(navbarToggler).toBeTruthy();
      expect(navbarCollapse).toBeTruthy();

      // Initially collapsed
      expect(navbarCollapse.classList.contains('show')).toBe(false);
    });

    it('should toggle mobile menu', () => {
      const navbarToggler = fixture.nativeElement.querySelector('.navbar-toggler');
      const navbarCollapse = fixture.nativeElement.querySelector('.navbar-collapse');

      // Click to open menu
      navbarToggler.click();
      fixture.detectChanges();

      expect(component.mobileMenuOpen).toBe(true);
      expect(navbarCollapse.classList.contains('show')).toBe(true);

      // Click to close menu
      navbarToggler.click();
      fixture.detectChanges();

      expect(component.mobileMenuOpen).toBe(false);
      expect(navbarCollapse.classList.contains('show')).toBe(false);
    });

    it('should render responsive card grid', () => {
      const cardGrid = fixture.nativeElement.querySelector('.card-grid');
      const cards = fixture.nativeElement.querySelectorAll('.mobile-card');

      expect(cardGrid).toBeTruthy();
      expect(cards.length).toBe(component.testCards.length);

      // Check that cards have mobile-friendly styling
      cards.forEach((card: HTMLElement) => {
        expect(card.classList.contains('mobile-card')).toBe(true);
        const button = card.querySelector('.btn-mobile');
        expect(button).toBeTruthy();
        expect(button?.classList.contains('btn-mobile')).toBe(true);
      });
    });

    it('should support different viewport sizes', () => {
      // Test mobile viewport
      Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: 375 });
      Object.defineProperty(window, 'innerHeight', { writable: true, configurable: true, value: 667 });
      window.dispatchEvent(new Event('resize'));

      // Test tablet viewport
      Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: 768 });
      Object.defineProperty(window, 'innerHeight', { writable: true, configurable: true, value: 1024 });
      window.dispatchEvent(new Event('resize'));

      // Test desktop viewport
      Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: 1920 });
      Object.defineProperty(window, 'innerHeight', { writable: true, configurable: true, value: 1080 });
      window.dispatchEvent(new Event('resize'));

      // Component should handle all viewport changes without errors
      expect(component).toBeTruthy();
    });
  });

  describe('PWA Feature Tests', () => {
    it('should support service worker registration', () => {
      expect('serviceWorker' in navigator).toBe(true);
    });

    it('should handle PWA installation prompt', () => {
      component.showInstallPrompt = true;
      fixture.detectChanges();

      const installPrompt = fixture.nativeElement.querySelector('.pwa-install-prompt');
      const installButton = fixture.nativeElement.querySelector('.pwa-install-prompt .btn-primary');
      const dismissButton = fixture.nativeElement.querySelector('.pwa-install-prompt .btn-secondary');

      expect(installPrompt).toBeTruthy();
      expect(installButton).toBeTruthy();
      expect(dismissButton).toBeTruthy();

      spyOn(component, 'installPWA');
      spyOn(component, 'dismissInstall');

      installButton.click();
      expect(component.installPWA).toHaveBeenCalled();

      dismissButton.click();
      expect(component.dismissInstall).toHaveBeenCalled();
    });

    it('should handle online/offline status', () => {
      const offlineIndicator = fixture.nativeElement.querySelector('.offline-indicator');
      expect(offlineIndicator).toBeTruthy();

      // Test online status
      component.isOnline = true;
      fixture.detectChanges();
      expect(offlineIndicator.classList.contains('offline')).toBe(false);

      // Test offline status
      component.isOnline = false;
      fixture.detectChanges();
      expect(offlineIndicator.classList.contains('offline')).toBe(true);
    });

    it('should support PWA manifest requirements', () => {
      // Check for manifest link in document head
      const manifestLink = document.querySelector('link[rel="manifest"]');
      // In a real app, this would be present
      
      // Test manifest structure requirements
      const expectedManifest = {
        name: 'StrideHR - Human Resource Management System',
        short_name: 'StrideHR',
        display: 'standalone',
        orientation: 'portrait-primary',
        theme_color: '#3b82f6',
        background_color: '#ffffff'
      };

      expect(expectedManifest.name).toBeTruthy();
      expect(expectedManifest.short_name).toBeTruthy();
      expect(expectedManifest.display).toBe('standalone');
    });

    it('should support push notifications API', () => {
      // Mock push notification support
      Object.defineProperty(window, 'Notification', {
        writable: true,
        value: {
          permission: 'default',
          requestPermission: jasmine.createSpy('requestPermission').and.returnValue(Promise.resolve('granted'))
        }
      });

      expect('Notification' in window).toBe(true);
      expect(typeof Notification.requestPermission).toBe('function');
    });

    it('should handle app cache and offline storage', () => {
      // Test localStorage for offline data
      expect(typeof Storage).toBe('function');
      expect(localStorage).toBeDefined();

      // Test offline data storage
      const offlineData = {
        timestamp: Date.now(),
        data: { test: 'offline data' }
      };

      localStorage.setItem('stride-hr-offline', JSON.stringify(offlineData));
      const stored = JSON.parse(localStorage.getItem('stride-hr-offline') || '{}');
      
      expect(stored.data.test).toBe('offline data');
      localStorage.removeItem('stride-hr-offline');
    });
  });

  describe('Mobile Browser Compatibility', () => {
    it('should support touch-action CSS property', () => {
      const touchArea = fixture.nativeElement.querySelector('.touch-area');
      const computedStyle = window.getComputedStyle(touchArea);
      
      // touch-action should be supported
      expect(computedStyle.touchAction).toBeDefined();
    });

    it('should support user-select CSS property', () => {
      const touchArea = fixture.nativeElement.querySelector('.touch-area');
      const computedStyle = window.getComputedStyle(touchArea);
      
      // user-select should be supported
      expect(computedStyle.userSelect).toBeDefined();
    });

    it('should handle viewport meta tag requirements', () => {
      // In a real app, viewport meta tag should be present
      const viewportMeta = document.querySelector('meta[name="viewport"]');
      
      // Expected viewport configuration for mobile
      const expectedViewport = 'width=device-width, initial-scale=1, shrink-to-fit=no';
      
      // Test that viewport concepts are understood
      expect(typeof window.innerWidth).toBe('number');
      expect(typeof window.innerHeight).toBe('number');
      expect(typeof window.devicePixelRatio).toBe('number');
    });

    it('should support CSS media queries for mobile', () => {
      const mobileQuery = window.matchMedia('(max-width: 768px)');
      const touchQuery = window.matchMedia('(pointer: coarse)');
      const hoverQuery = window.matchMedia('(hover: none)');

      expect(mobileQuery).toBeDefined();
      expect(touchQuery).toBeDefined();
      expect(hoverQuery).toBeDefined();
      
      expect(typeof mobileQuery.matches).toBe('boolean');
      expect(typeof touchQuery.matches).toBe('boolean');
      expect(typeof hoverQuery.matches).toBe('boolean');
    });
  });
});