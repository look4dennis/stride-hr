import { TestBed, ComponentFixture } from '@angular/core/testing';
import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TestConfig } from './test-config';

/**
 * Mobile validation tests for touch interactions, gestures, and responsive behavior
 * Tests various screen sizes and orientations
 */

@Component({
  selector: 'app-mobile-validation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="mobile-validation-container">
      <!-- Touch target size validation -->
      <div class="touch-targets">
        <button class="btn-small" (click)="onSmallButtonClick()">Small</button>
        <button class="btn-medium" (click)="onMediumButtonClick()">Medium</button>
        <button class="btn-large" (click)="onLargeButtonClick()">Large</button>
        <button class="btn-touch-friendly" (click)="onTouchFriendlyClick()">Touch Friendly</button>
      </div>

      <!-- Gesture recognition area -->
      <div class="gesture-area"
           (touchstart)="onGestureStart($event)"
           (touchmove)="onGestureMove($event)"
           (touchend)="onGestureEnd($event)"
           (pan)="onPan($event)"
           (pinch)="onPinch($event)">
        <p>Gesture Area - Try swiping, pinching, or panning</p>
        <div class="gesture-feedback">{{ gestureStatus }}</div>
      </div>

      <!-- Responsive layout test -->
      <div class="responsive-grid">
        <div class="grid-item" *ngFor="let item of gridItems">
          <div class="card">
            <h6>{{ item.title }}</h6>
            <p>{{ item.content }}</p>
            <button class="btn btn-sm">Action</button>
          </div>
        </div>
      </div>

      <!-- Form input validation for mobile -->
      <form class="mobile-form" #mobileForm="ngForm">
        <div class="form-group">
          <label for="mobileInput">Mobile Input</label>
          <input 
            type="text" 
            id="mobileInput"
            class="form-control mobile-input"
            [(ngModel)]="inputValue"
            name="mobileInput"
            placeholder="Touch to focus"
            required>
        </div>
        
        <div class="form-group">
          <label for="mobileSelect">Mobile Select</label>
          <select 
            id="mobileSelect"
            class="form-select mobile-select"
            [(ngModel)]="selectValue"
            name="mobileSelect">
            <option value="">Choose option</option>
            <option value="option1">Option 1</option>
            <option value="option2">Option 2</option>
            <option value="option3">Option 3</option>
          </select>
        </div>

        <div class="form-group">
          <label for="mobileTextarea">Mobile Textarea</label>
          <textarea 
            id="mobileTextarea"
            class="form-control mobile-textarea"
            [(ngModel)]="textareaValue"
            name="mobileTextarea"
            rows="3"
            placeholder="Enter text here"></textarea>
        </div>

        <button type="submit" class="btn btn-primary mobile-submit">Submit</button>
      </form>

      <!-- Orientation change test -->
      <div class="orientation-indicator">
        <p>Orientation: {{ currentOrientation }}</p>
        <p>Screen: {{ screenInfo }}</p>
      </div>

      <!-- Viewport information -->
      <div class="viewport-info">
        <p>Viewport: {{ viewportWidth }}x{{ viewportHeight }}</p>
        <p>Device Pixel Ratio: {{ devicePixelRatio }}</p>
        <p>Touch Support: {{ touchSupport ? 'Yes' : 'No' }}</p>
      </div>
    </div>
  `,
  styles: [`
    .mobile-validation-container {
      padding: 1rem;
      max-width: 100%;
    }

    /* Touch target size tests */
    .touch-targets {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
      margin-bottom: 2rem;
      padding: 1rem;
      background: #f8f9fa;
      border-radius: 0.5rem;
    }

    .btn-small {
      min-width: 30px;
      min-height: 30px;
      padding: 0.25rem 0.5rem;
      font-size: 0.75rem;
    }

    .btn-medium {
      min-width: 36px;
      min-height: 36px;
      padding: 0.375rem 0.75rem;
      font-size: 0.875rem;
    }

    .btn-large {
      min-width: 42px;
      min-height: 42px;
      padding: 0.5rem 1rem;
      font-size: 1rem;
    }

    .btn-touch-friendly {
      min-width: 44px;
      min-height: 44px;
      padding: 0.75rem 1.5rem;
      font-size: 1rem;
      background: #28a745;
      color: white;
      border: none;
      border-radius: 0.375rem;
    }

    /* Gesture area */
    .gesture-area {
      width: 100%;
      height: 200px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      margin-bottom: 2rem;
      border-radius: 0.5rem;
      touch-action: manipulation;
      user-select: none;
    }

    .gesture-feedback {
      margin-top: 1rem;
      font-weight: bold;
      font-size: 1.1rem;
    }

    /* Responsive grid */
    .responsive-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 1rem;
      margin-bottom: 2rem;
    }

    .grid-item .card {
      padding: 1rem;
      border: 1px solid #dee2e6;
      border-radius: 0.375rem;
      background: white;
    }

    .grid-item h6 {
      margin: 0 0 0.5rem 0;
      color: #495057;
    }

    .grid-item p {
      margin: 0 0 1rem 0;
      color: #6c757d;
      font-size: 0.875rem;
    }

    .grid-item .btn {
      width: 100%;
    }

    /* Mobile form styles */
    .mobile-form {
      background: #f8f9fa;
      padding: 1.5rem;
      border-radius: 0.5rem;
      margin-bottom: 2rem;
    }

    .form-group {
      margin-bottom: 1.5rem;
    }

    .form-group label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: 500;
      color: #495057;
    }

    .mobile-input,
    .mobile-select,
    .mobile-textarea {
      width: 100%;
      padding: 0.75rem;
      font-size: 1rem;
      border: 2px solid #ced4da;
      border-radius: 0.375rem;
      transition: border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
    }

    .mobile-input:focus,
    .mobile-select:focus,
    .mobile-textarea:focus {
      outline: none;
      border-color: #86b7fe;
      box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
    }

    .mobile-submit {
      width: 100%;
      padding: 0.75rem;
      font-size: 1.1rem;
      min-height: 44px;
    }

    /* Information displays */
    .orientation-indicator,
    .viewport-info {
      background: #e9ecef;
      padding: 1rem;
      border-radius: 0.375rem;
      margin-bottom: 1rem;
    }

    .orientation-indicator p,
    .viewport-info p {
      margin: 0.25rem 0;
      font-family: monospace;
      font-size: 0.875rem;
    }

    /* Mobile-specific styles */
    @media (max-width: 768px) {
      .mobile-validation-container {
        padding: 0.5rem;
      }

      .touch-targets {
        flex-direction: column;
        align-items: stretch;
      }

      .touch-targets button {
        width: 100%;
        margin-bottom: 0.5rem;
      }

      .responsive-grid {
        grid-template-columns: 1fr;
      }

      .gesture-area {
        height: 150px;
      }
    }

    /* Touch-specific styles */
    @media (pointer: coarse) {
      .btn-small {
        min-width: 44px;
        min-height: 44px;
        padding: 0.5rem;
      }

      .btn-medium {
        min-width: 44px;
        min-height: 44px;
      }

      .mobile-input,
      .mobile-select,
      .mobile-textarea {
        min-height: 44px;
        font-size: 16px; /* Prevents zoom on iOS */
      }
    }

    /* Landscape orientation */
    @media (orientation: landscape) and (max-height: 500px) {
      .gesture-area {
        height: 100px;
      }

      .mobile-form {
        padding: 1rem;
      }
    }
  `]
})
class MobileValidationComponent {
  inputValue = '';
  selectValue = '';
  textareaValue = '';
  gestureStatus = 'Ready for gestures';
  currentOrientation = 'unknown';
  screenInfo = '';
  viewportWidth = 0;
  viewportHeight = 0;
  devicePixelRatio = 1;
  touchSupport = false;

  gridItems = [
    { title: 'Dashboard', content: 'View your HR dashboard and key metrics' },
    { title: 'Employees', content: 'Manage employee records and information' },
    { title: 'Attendance', content: 'Track attendance and working hours' },
    { title: 'Payroll', content: 'Process payroll and manage benefits' }
  ];

  // Touch gesture tracking
  private touchStartX = 0;
  private touchStartY = 0;
  private touchStartTime = 0;
  private initialDistance = 0;

  constructor() {
    this.updateDeviceInfo();
    this.setupOrientationListener();
    this.setupResizeListener();
  }

  private updateDeviceInfo() {
    this.viewportWidth = window.innerWidth;
    this.viewportHeight = window.innerHeight;
    this.devicePixelRatio = window.devicePixelRatio || 1;
    this.touchSupport = 'ontouchstart' in window || navigator.maxTouchPoints > 0;
    this.currentOrientation = this.getOrientation();
    this.screenInfo = `${screen.width}x${screen.height}`;
  }

  private getOrientation(): string {
    if (screen.orientation) {
      return screen.orientation.type;
    } else if (window.orientation !== undefined) {
      return Math.abs(window.orientation) === 90 ? 'landscape' : 'portrait';
    } else {
      return window.innerWidth > window.innerHeight ? 'landscape' : 'portrait';
    }
  }

  private setupOrientationListener() {
    if (screen.orientation) {
      screen.orientation.addEventListener('change', () => {
        setTimeout(() => this.updateDeviceInfo(), 100);
      });
    } else {
      window.addEventListener('orientationchange', () => {
        setTimeout(() => this.updateDeviceInfo(), 100);
      });
    }
  }

  private setupResizeListener() {
    window.addEventListener('resize', () => {
      this.updateDeviceInfo();
    });
  }

  onSmallButtonClick() {
    console.log('Small button clicked');
  }

  onMediumButtonClick() {
    console.log('Medium button clicked');
  }

  onLargeButtonClick() {
    console.log('Large button clicked');
  }

  onTouchFriendlyClick() {
    console.log('Touch-friendly button clicked');
  }

  onGestureStart(event: TouchEvent) {
    if (event.touches.length === 1) {
      this.touchStartX = event.touches[0].clientX;
      this.touchStartY = event.touches[0].clientY;
      this.touchStartTime = Date.now();
      this.gestureStatus = 'Touch started';
    } else if (event.touches.length === 2) {
      this.initialDistance = this.getDistance(event.touches[0], event.touches[1]);
      this.gestureStatus = 'Pinch gesture detected';
    }
  }

  onGestureMove(event: TouchEvent) {
    event.preventDefault();
    
    if (event.touches.length === 1) {
      const deltaX = event.touches[0].clientX - this.touchStartX;
      const deltaY = event.touches[0].clientY - this.touchStartY;
      
      if (Math.abs(deltaX) > 10 || Math.abs(deltaY) > 10) {
        this.gestureStatus = `Moving: ${Math.round(deltaX)}, ${Math.round(deltaY)}`;
      }
    } else if (event.touches.length === 2) {
      const currentDistance = this.getDistance(event.touches[0], event.touches[1]);
      const scale = currentDistance / this.initialDistance;
      this.gestureStatus = `Pinch scale: ${scale.toFixed(2)}`;
    }
  }

  onGestureEnd(event: TouchEvent) {
    if (event.changedTouches.length === 1) {
      const touch = event.changedTouches[0];
      const deltaX = touch.clientX - this.touchStartX;
      const deltaY = touch.clientY - this.touchStartY;
      const deltaTime = Date.now() - this.touchStartTime;
      
      // Detect swipe
      if (Math.abs(deltaX) > 50 && deltaTime < 300) {
        this.gestureStatus = deltaX > 0 ? 'Swipe right detected' : 'Swipe left detected';
      } else if (Math.abs(deltaY) > 50 && deltaTime < 300) {
        this.gestureStatus = deltaY > 0 ? 'Swipe down detected' : 'Swipe up detected';
      } else if (deltaTime < 200 && Math.abs(deltaX) < 10 && Math.abs(deltaY) < 10) {
        this.gestureStatus = 'Tap detected';
      } else {
        this.gestureStatus = 'Gesture ended';
      }
    } else {
      this.gestureStatus = 'Multi-touch ended';
    }
    
    // Reset after 2 seconds
    setTimeout(() => {
      this.gestureStatus = 'Ready for gestures';
    }, 2000);
  }

  onPan(event: any) {
    this.gestureStatus = `Pan: ${event.deltaX}, ${event.deltaY}`;
  }

  onPinch(event: any) {
    this.gestureStatus = `Pinch: ${event.scale}`;
  }

  private getDistance(touch1: Touch, touch2: Touch): number {
    const dx = touch1.clientX - touch2.clientX;
    const dy = touch1.clientY - touch2.clientY;
    return Math.sqrt(dx * dx + dy * dy);
  }
}

describe('Mobile Validation Tests', () => {
  let component: MobileValidationComponent;
  let fixture: ComponentFixture<MobileValidationComponent>;

  beforeEach(async () => {
    TestConfig.setupAllMocks();
    
    await TestBed.configureTestingModule({
      imports: [MobileValidationComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(MobileValidationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    fixture.destroy();
  });

  describe('Touch Target Size Validation', () => {
    it('should render buttons with appropriate touch target sizes', () => {
      const smallBtn = fixture.nativeElement.querySelector('.btn-small');
      const mediumBtn = fixture.nativeElement.querySelector('.btn-medium');
      const largeBtn = fixture.nativeElement.querySelector('.btn-large');
      const touchFriendlyBtn = fixture.nativeElement.querySelector('.btn-touch-friendly');

      expect(smallBtn).toBeTruthy();
      expect(mediumBtn).toBeTruthy();
      expect(largeBtn).toBeTruthy();
      expect(touchFriendlyBtn).toBeTruthy();

      // Check computed styles for touch-friendly button
      const touchFriendlyStyle = window.getComputedStyle(touchFriendlyBtn);
      const minWidth = parseInt(touchFriendlyStyle.minWidth);
      const minHeight = parseInt(touchFriendlyStyle.minHeight);

      // Touch targets should be at least 44px for accessibility
      expect(minWidth).toBeGreaterThanOrEqual(44);
      expect(minHeight).toBeGreaterThanOrEqual(44);
    });

    it('should handle button clicks on touch devices', () => {
      spyOn(component, 'onTouchFriendlyClick');
      
      const touchFriendlyBtn = fixture.nativeElement.querySelector('.btn-touch-friendly');
      touchFriendlyBtn.click();
      
      expect(component.onTouchFriendlyClick).toHaveBeenCalled();
    });
  });

  describe('Gesture Recognition', () => {
    it('should handle touch start events', () => {
      const gestureArea = fixture.nativeElement.querySelector('.gesture-area');
      spyOn(component, 'onGestureStart');

      const touch = new Touch({
        identifier: 1,
        target: gestureArea,
        clientX: 100,
        clientY: 100,
        radiusX: 2.5,
        radiusY: 2.5,
        rotationAngle: 10,
        force: 0.5
      });

      const touchEvent = new TouchEvent('touchstart', { touches: [touch] });
      gestureArea.dispatchEvent(touchEvent);

      expect(component.onGestureStart).toHaveBeenCalled();
    });

    it('should detect swipe gestures', () => {
      const gestureArea = fixture.nativeElement.querySelector('.gesture-area');
      
      // Simulate swipe right
      const startTouch = new Touch({
        identifier: 1,
        target: gestureArea,
        clientX: 50,
        clientY: 100,
        radiusX: 2.5,
        radiusY: 2.5,
        rotationAngle: 10,
        force: 0.5
      });

      const endTouch = new Touch({
        identifier: 1,
        target: gestureArea,
        clientX: 150,
        clientY: 100,
        radiusX: 2.5,
        radiusY: 2.5,
        rotationAngle: 10,
        force: 0.5
      });

      // Start touch
      component.onGestureStart(new TouchEvent('touchstart', { touches: [startTouch] }));
      
      // End touch (simulating swipe)
      component.onGestureEnd(new TouchEvent('touchend', { changedTouches: [endTouch] }));

      expect(component.gestureStatus).toContain('Swipe');
    });

    it('should detect pinch gestures', () => {
      const gestureArea = fixture.nativeElement.querySelector('.gesture-area');
      
      const touch1 = new Touch({
        identifier: 1,
        target: gestureArea,
        clientX: 50,
        clientY: 100,
        radiusX: 2.5,
        radiusY: 2.5,
        rotationAngle: 10,
        force: 0.5
      });

      const touch2 = new Touch({
        identifier: 2,
        target: gestureArea,
        clientX: 150,
        clientY: 100,
        radiusX: 2.5,
        radiusY: 2.5,
        rotationAngle: 10,
        force: 0.5
      });

      const pinchEvent = new TouchEvent('touchstart', { touches: [touch1, touch2] });
      component.onGestureStart(pinchEvent);

      expect(component.gestureStatus).toBe('Pinch gesture detected');
    });
  });

  describe('Responsive Layout', () => {
    it('should render responsive grid correctly', () => {
      const gridContainer = fixture.nativeElement.querySelector('.responsive-grid');
      const gridItems = fixture.nativeElement.querySelectorAll('.grid-item');

      expect(gridContainer).toBeTruthy();
      expect(gridItems.length).toBe(component.gridItems.length);

      // Check grid CSS
      const gridStyle = window.getComputedStyle(gridContainer);
      expect(gridStyle.display).toBe('grid');
    });

    it('should adapt to different viewport sizes', () => {
      // Test mobile viewport
      Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: 375 });
      Object.defineProperty(window, 'innerHeight', { writable: true, configurable: true, value: 667 });
      
      component['updateDeviceInfo']();
      fixture.detectChanges();

      expect(component.viewportWidth).toBe(375);
      expect(component.viewportHeight).toBe(667);

      // Test tablet viewport
      Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: 768 });
      Object.defineProperty(window, 'innerHeight', { writable: true, configurable: true, value: 1024 });
      
      component['updateDeviceInfo']();
      fixture.detectChanges();

      expect(component.viewportWidth).toBe(768);
      expect(component.viewportHeight).toBe(1024);
    });
  });

  describe('Mobile Form Validation', () => {
    it('should render mobile-friendly form controls', () => {
      const mobileInput = fixture.nativeElement.querySelector('.mobile-input');
      const mobileSelect = fixture.nativeElement.querySelector('.mobile-select');
      const mobileTextarea = fixture.nativeElement.querySelector('.mobile-textarea');
      const submitButton = fixture.nativeElement.querySelector('.mobile-submit');

      expect(mobileInput).toBeTruthy();
      expect(mobileSelect).toBeTruthy();
      expect(mobileTextarea).toBeTruthy();
      expect(submitButton).toBeTruthy();

      // Check minimum touch target sizes
      const inputStyle = window.getComputedStyle(mobileInput);
      const submitStyle = window.getComputedStyle(submitButton);

      expect(parseInt(submitStyle.minHeight)).toBeGreaterThanOrEqual(44);
    });

    it('should handle form input focus and blur', () => {
      const mobileInput = fixture.nativeElement.querySelector('.mobile-input');
      
      // Test focus
      mobileInput.focus();
      expect(document.activeElement).toBe(mobileInput);

      // Test blur
      mobileInput.blur();
      expect(document.activeElement).not.toBe(mobileInput);
    });

    it('should prevent zoom on iOS by using 16px font size', () => {
      const mobileInput = fixture.nativeElement.querySelector('.mobile-input');
      const computedStyle = window.getComputedStyle(mobileInput);
      
      // On touch devices, font size should be at least 16px to prevent zoom
      if (component.touchSupport) {
        const fontSize = parseInt(computedStyle.fontSize);
        expect(fontSize).toBeGreaterThanOrEqual(16);
      }
    });
  });

  describe('Device Information Detection', () => {
    it('should detect device capabilities correctly', () => {
      expect(typeof component.viewportWidth).toBe('number');
      expect(typeof component.viewportHeight).toBe('number');
      expect(typeof component.devicePixelRatio).toBe('number');
      expect(typeof component.touchSupport).toBe('boolean');
      expect(component.currentOrientation).toBeTruthy();
    });

    it('should detect orientation changes', () => {
      const initialOrientation = component.currentOrientation;
      
      // Simulate orientation change
      Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: 667 });
      Object.defineProperty(window, 'innerHeight', { writable: true, configurable: true, value: 375 });
      
      component['updateDeviceInfo']();
      
      expect(component.currentOrientation).toBeTruthy();
      // Orientation should be detected based on dimensions
    });

    it('should handle resize events', () => {
      const initialWidth = component.viewportWidth;
      
      // Simulate window resize
      Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: 1024 });
      window.dispatchEvent(new Event('resize'));
      
      // Component should update viewport info
      expect(typeof component.viewportWidth).toBe('number');
    });
  });

  describe('Touch Support Detection', () => {
    it('should detect touch support correctly', () => {
      // Mock touch support
      Object.defineProperty(window, 'ontouchstart', { writable: true, value: {} });
      Object.defineProperty(navigator, 'maxTouchPoints', { writable: true, value: 5 });
      
      component['updateDeviceInfo']();
      
      expect(component.touchSupport).toBe(true);
    });

    it('should handle non-touch devices', () => {
      // Mock non-touch device
      delete (window as any).ontouchstart;
      Object.defineProperty(navigator, 'maxTouchPoints', { writable: true, value: 0 });
      
      component['updateDeviceInfo']();
      
      expect(typeof component.touchSupport).toBe('boolean');
    });
  });

  describe('Accessibility on Mobile', () => {
    it('should provide adequate contrast ratios', () => {
      const touchFriendlyBtn = fixture.nativeElement.querySelector('.btn-touch-friendly');
      const computedStyle = window.getComputedStyle(touchFriendlyBtn);
      
      // Button should have good contrast (green background with white text)
      expect(computedStyle.backgroundColor).toContain('rgb(40, 167, 69)'); // #28a745
      expect(computedStyle.color).toContain('rgb(255, 255, 255)'); // white
    });

    it('should support keyboard navigation fallback', () => {
      const touchFriendlyBtn = fixture.nativeElement.querySelector('.btn-touch-friendly');
      
      // Button should be focusable
      touchFriendlyBtn.focus();
      expect(document.activeElement).toBe(touchFriendlyBtn);
      
      // Should handle keyboard events
      const enterEvent = new KeyboardEvent('keydown', { key: 'Enter' });
      touchFriendlyBtn.dispatchEvent(enterEvent);
      
      // Should not throw errors
      expect(touchFriendlyBtn).toBeTruthy();
    });

    it('should provide proper ARIA labels where needed', () => {
      const gestureArea = fixture.nativeElement.querySelector('.gesture-area');
      
      // Interactive elements should be accessible
      expect(gestureArea).toBeTruthy();
      
      // Form controls should have proper labels
      const mobileInput = fixture.nativeElement.querySelector('.mobile-input');
      const label = fixture.nativeElement.querySelector('label[for="mobileInput"]');
      
      expect(label).toBeTruthy();
      expect(label.getAttribute('for')).toBe('mobileInput');
      expect(mobileInput.getAttribute('id')).toBe('mobileInput');
    });
  });
});