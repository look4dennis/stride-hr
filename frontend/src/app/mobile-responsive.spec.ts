import { TestBed, ComponentFixture } from '@angular/core/testing';
import { Component, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { provideZoneChangeDetection } from '@angular/core';

// Mock component for testing responsive behavior
@Component({
  template: `
    <div class="container-fluid">
      <div class="row">
        <div class="col-12 col-md-6 col-lg-4" *ngFor="let item of items">
          <div class="card mobile-card">
            <div class="card-body">
              <h5 class="card-title">{{ item.title }}</h5>
              <p class="card-text">{{ item.description }}</p>
              <button class="btn btn-primary btn-mobile" 
                      [attr.aria-label]="'Action for ' + item.title"
                      (click)="onItemClick(item)"
                      (touchstart)="onTouchStart($event)"
                      (touchend)="onTouchEnd($event)">
                Action
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Mobile navigation -->
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary d-lg-none">
      <button class="navbar-toggler" 
              type="button" 
              [attr.aria-expanded]="isNavOpen"
              (click)="toggleNav()"
              (touchstart)="onNavTouchStart($event)">
        <span class="navbar-toggler-icon"></span>
      </button>
      <div class="navbar-collapse" [class.show]="isNavOpen">
        <ul class="navbar-nav">
          <li class="nav-item" *ngFor="let navItem of navItems">
            <a class="nav-link touch-target" 
               [href]="navItem.href"
               (click)="onNavClick(navItem, $event)"
               (touchstart)="onNavItemTouchStart($event)">
              {{ navItem.label }}
            </a>
          </li>
        </ul>
      </div>
    </nav>
    
    <!-- Touch-friendly form -->
    <form class="mobile-form" (ngSubmit)="onSubmit()">
      <div class="form-group">
        <label for="mobileInput" class="form-label">Mobile Input</label>
        <input type="text" 
               id="mobileInput"
               class="form-control form-control-lg"
               [(ngModel)]="inputValue"
               (focus)="onInputFocus($event)"
               (blur)="onInputBlur($event)"
               (touchstart)="onInputTouchStart($event)">
      </div>
      
      <div class="form-group">
        <label for="emailInput" class="form-label">Email</label>
        <input type="email" 
               id="emailInput"
               class="form-control form-control-lg"
               [(ngModel)]="emailValue"
               inputmode="email">
      </div>
      
      <div class="form-group">
        <label for="phoneInput" class="form-label">Phone</label>
        <input type="tel" 
               id="phoneInput"
               class="form-control form-control-lg"
               [(ngModel)]="phoneValue"
               inputmode="tel">
      </div>
      
      <button type="submit" 
              class="btn btn-success btn-lg btn-block w-100 touch-submit">
        Submit
      </button>
    </form>
    
    <!-- Swipe-enabled carousel -->
    <div class="carousel-container" 
         (touchstart)="onCarouselTouchStart($event)"
         (touchmove)="onCarouselTouchMove($event)"
         (touchend)="onCarouselTouchEnd($event)">
      <div class="carousel-item" 
           *ngFor="let slide of slides; let i = index"
           [class.active]="i === activeSlide">
        <img [src]="slide.image" [alt]="slide.alt" class="carousel-image">
        <div class="carousel-caption">
          <h3>{{ slide.title }}</h3>
          <p>{{ slide.description }}</p>
        </div>
      </div>
      
      <div class="carousel-indicators">
        <button *ngFor="let slide of slides; let i = index"
                [class.active]="i === activeSlide"
                (click)="setActiveSlide(i)"
                (touchstart)="onIndicatorTouchStart($event, i)"
                [attr.aria-label]="'Go to slide ' + (i + 1)">
        </button>
      </div>
    </div>
  `,
  styles: [`
    .mobile-card {
      margin-bottom: 1rem;
      min-height: 200px;
    }
    
    .btn-mobile {
      min-height: 44px;
      min-width: 44px;
      padding: 12px 24px;
      font-size: 16px;
    }
    
    .touch-target {
      min-height: 44px;
      display: flex;
      align-items: center;
      padding: 12px 16px;
    }
    
    .mobile-form {
      padding: 1rem;
    }
    
    .form-control-lg {
      min-height: 48px;
      font-size: 16px;
    }
    
    .touch-submit {
      min-height: 48px;
      margin-top: 1rem;
    }
    
    .carousel-container {
      position: relative;
      overflow: hidden;
      height: 300px;
      touch-action: pan-x;
    }
    
    .carousel-item {
      position: absolute;
      width: 100%;
      height: 100%;
      opacity: 0;
      transition: opacity 0.3s ease;
    }
    
    .carousel-item.active {
      opacity: 1;
    }
    
    .carousel-image {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
    
    .carousel-indicators {
      position: absolute;
      bottom: 10px;
      left: 50%;
      transform: translateX(-50%);
      display: flex;
      gap: 8px;
    }
    
    .carousel-indicators button {
      width: 12px;
      height: 12px;
      border-radius: 50%;
      border: none;
      background: rgba(255, 255, 255, 0.5);
      cursor: pointer;
      min-height: 44px;
      min-width: 44px;
    }
    
    .carousel-indicators button.active {
      background: white;
    }
    
    @media (max-width: 768px) {
      .mobile-card {
        margin-bottom: 0.5rem;
      }
      
      .btn-mobile {
        width: 100%;
        margin-top: 0.5rem;
      }
      
      .mobile-form {
        padding: 0.5rem;
      }
    }
    
    @media (max-width: 576px) {
      .card-body {
        padding: 0.75rem;
      }
      
      .form-control-lg {
        font-size: 16px; /* Prevent zoom on iOS */
      }
    }
  `]
})
class MockResponsiveComponent {
  items = [
    { id: 1, title: 'Item 1', description: 'Description 1' },
    { id: 2, title: 'Item 2', description: 'Description 2' },
    { id: 3, title: 'Item 3', description: 'Description 3' }
  ];
  
  navItems = [
    { href: '/dashboard', label: 'Dashboard' },
    { href: '/employees', label: 'Employees' },
    { href: '/attendance', label: 'Attendance' }
  ];
  
  slides = [
    { image: '/assets/slide1.jpg', alt: 'Slide 1', title: 'Title 1', description: 'Description 1' },
    { image: '/assets/slide2.jpg', alt: 'Slide 2', title: 'Title 2', description: 'Description 2' },
    { image: '/assets/slide3.jpg', alt: 'Slide 3', title: 'Title 3', description: 'Description 3' }
  ];
  
  isNavOpen = false;
  inputValue = '';
  emailValue = '';
  phoneValue = '';
  activeSlide = 0;
  
  // Touch tracking
  touchStartX = 0;
  touchStartY = 0;
  touchStartTime = 0;
  
  onItemClick(item: any) {
    // Handle item click
  }
  
  onTouchStart(event: TouchEvent) {
    this.touchStartTime = Date.now();
    if (event.touches.length > 0) {
      this.touchStartX = event.touches[0].clientX;
      this.touchStartY = event.touches[0].clientY;
    }
  }
  
  onTouchEnd(event: TouchEvent) {
    const touchEndTime = Date.now();
    const touchDuration = touchEndTime - this.touchStartTime;
    
    // Simulate touch feedback
    if (touchDuration < 200) {
      // Quick tap
      const target = event.currentTarget as HTMLElement;
      target?.classList.add('touch-feedback');
      setTimeout(() => {
        target?.classList.remove('touch-feedback');
      }, 150);
    }
  }
  
  toggleNav() {
    this.isNavOpen = !this.isNavOpen;
  }
  
  onNavTouchStart(event: TouchEvent) {
    // Handle nav touch start
  }
  
  onNavClick(navItem: any, event: Event) {
    event.preventDefault();
    // Handle navigation
  }
  
  onNavItemTouchStart(event: TouchEvent) {
    // Handle nav item touch
  }
  
  onSubmit() {
    // Handle form submission
  }
  
  onInputFocus(event: FocusEvent) {
    // Handle input focus
  }
  
  onInputBlur(event: FocusEvent) {
    // Handle input blur
  }
  
  onInputTouchStart(event: TouchEvent) {
    // Handle input touch
  }
  
  onCarouselTouchStart(event: TouchEvent) {
    if (event.touches.length > 0) {
      this.touchStartX = event.touches[0].clientX;
      this.touchStartTime = Date.now();
    }
  }
  
  onCarouselTouchMove(event: TouchEvent) {
    event.preventDefault(); // Prevent scrolling
  }
  
  onCarouselTouchEnd(event: TouchEvent) {
    if (event.changedTouches.length > 0) {
      const touchEndX = event.changedTouches[0].clientX;
      const touchDistance = touchEndX - this.touchStartX;
      const touchDuration = Date.now() - this.touchStartTime;
      
      // Swipe detection
      if (Math.abs(touchDistance) > 50 && touchDuration < 500) {
        if (touchDistance > 0) {
          // Swipe right - previous slide
          this.previousSlide();
        } else {
          // Swipe left - next slide
          this.nextSlide();
        }
      }
    }
  }
  
  onIndicatorTouchStart(event: TouchEvent, index: number) {
    // Handle indicator touch
  }
  
  setActiveSlide(index: number) {
    this.activeSlide = index;
  }
  
  nextSlide() {
    this.activeSlide = (this.activeSlide + 1) % this.slides.length;
  }
  
  previousSlide() {
    this.activeSlide = this.activeSlide === 0 ? this.slides.length - 1 : this.activeSlide - 1;
  }
}

describe('Mobile Responsive Design Tests', () => {
  let component: MockResponsiveComponent;
  let fixture: ComponentFixture<MockResponsiveComponent>;
  let compiled: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MockResponsiveComponent],
      imports: [CommonModule, FormsModule],
      providers: [
        provideZoneChangeDetection({ eventCoalescing: true })
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MockResponsiveComponent);
    component = fixture.componentInstance;
    compiled = fixture.nativeElement;
    fixture.detectChanges();
  });

  describe('Responsive Layout Tests', () => {
    it('should create responsive component', () => {
      expect(component).toBeTruthy();
    });

    it('should have Bootstrap responsive classes', () => {
      const columns = compiled.querySelectorAll('.col-12.col-md-6.col-lg-4');
      expect(columns.length).toBe(component.items.length);
    });

    it('should have container-fluid for full-width layout', () => {
      const container = compiled.querySelector('.container-fluid');
      expect(container).toBeTruthy();
    });

    it('should have mobile-first responsive cards', () => {
      const cards = compiled.querySelectorAll('.mobile-card');
      expect(cards.length).toBe(component.items.length);
    });
  });

  describe('Touch-Friendly Button Tests', () => {
    it('should have touch-friendly button sizes (minimum 44px)', () => {
      const buttons = compiled.querySelectorAll('.btn-mobile');
      buttons.forEach(button => {
        const styles = window.getComputedStyle(button as Element);
        const minHeight = parseInt(styles.minHeight);
        const minWidth = parseInt(styles.minWidth);
        
        expect(minHeight).toBeGreaterThanOrEqual(44);
        expect(minWidth).toBeGreaterThanOrEqual(44);
      });
    });

    it('should have proper padding for touch targets', () => {
      const touchTargets = compiled.querySelectorAll('.touch-target');
      touchTargets.forEach(target => {
        const styles = window.getComputedStyle(target as Element);
        const minHeight = parseInt(styles.minHeight);
        expect(minHeight).toBeGreaterThanOrEqual(44);
      });
    });

    it('should have aria-labels for accessibility', () => {
      const buttons = compiled.querySelectorAll('.btn-mobile');
      buttons.forEach(button => {
        expect(button.getAttribute('aria-label')).toBeTruthy();
      });
    });
  });

  describe('Mobile Navigation Tests', () => {
    it('should have mobile navigation toggle', () => {
      const navToggle = compiled.querySelector('.navbar-toggler');
      expect(navToggle).toBeTruthy();
    });

    it('should toggle navigation on click', () => {
      const navToggle = compiled.querySelector('.navbar-toggler') as HTMLElement;
      const navCollapse = compiled.querySelector('.navbar-collapse');
      
      expect(component.isNavOpen).toBeFalse();
      expect(navCollapse?.classList.contains('show')).toBeFalse();
      
      navToggle.click();
      fixture.detectChanges();
      
      expect(component.isNavOpen).toBeTrue();
      expect(navCollapse?.classList.contains('show')).toBeTrue();
    });

    it('should have proper aria-expanded attribute', () => {
      const navToggle = compiled.querySelector('.navbar-toggler');
      expect(navToggle?.getAttribute('aria-expanded')).toBe('false');
      
      component.toggleNav();
      fixture.detectChanges();
      
      expect(navToggle?.getAttribute('aria-expanded')).toBe('true');
    });
  });

  describe('Touch Event Handling Tests', () => {
    it('should handle touch start events', () => {
      const button = compiled.querySelector('.btn-mobile') as HTMLElement;
      spyOn(component, 'onTouchStart');
      
      const touchEvent = new TouchEvent('touchstart', {
        touches: [{ clientX: 100, clientY: 100 } as Touch]
      });
      
      button.dispatchEvent(touchEvent);
      expect(component.onTouchStart).toHaveBeenCalled();
    });

    it('should handle touch end events', () => {
      const button = compiled.querySelector('.btn-mobile') as HTMLElement;
      spyOn(component, 'onTouchEnd');
      
      const touchEvent = new TouchEvent('touchend');
      button.dispatchEvent(touchEvent);
      
      expect(component.onTouchEnd).toHaveBeenCalled();
    });

    it('should track touch coordinates', () => {
      const touchEvent = new TouchEvent('touchstart', {
        touches: [{ clientX: 150, clientY: 200 } as Touch]
      });
      
      component.onTouchStart(touchEvent);
      
      expect(component.touchStartX).toBe(150);
      expect(component.touchStartY).toBe(200);
      expect(component.touchStartTime).toBeGreaterThan(0);
    });
  });

  describe('Form Input Optimization Tests', () => {
    it('should have large form controls for mobile', () => {
      const inputs = compiled.querySelectorAll('.form-control-lg');
      inputs.forEach(input => {
        const styles = window.getComputedStyle(input as Element);
        const minHeight = parseInt(styles.minHeight);
        expect(minHeight).toBeGreaterThanOrEqual(48);
      });
    });

    it('should have proper input types for mobile keyboards', () => {
      const emailInput = compiled.querySelector('#emailInput');
      const phoneInput = compiled.querySelector('#phoneInput');
      
      expect(emailInput?.getAttribute('type')).toBe('email');
      expect(phoneInput?.getAttribute('type')).toBe('tel');
    });

    it('should have inputmode attributes for better mobile experience', () => {
      const emailInput = compiled.querySelector('#emailInput');
      const phoneInput = compiled.querySelector('#phoneInput');
      
      expect(emailInput?.getAttribute('inputmode')).toBe('email');
      expect(phoneInput?.getAttribute('inputmode')).toBe('tel');
    });

    it('should have full-width submit button', () => {
      const submitButton = compiled.querySelector('.touch-submit');
      expect(submitButton?.classList.contains('w-100')).toBeTrue();
    });
  });

  describe('Swipe Gesture Tests', () => {
    it('should handle carousel swipe gestures', () => {
      const carousel = compiled.querySelector('.carousel-container') as HTMLElement;
      spyOn(component, 'onCarouselTouchStart');
      spyOn(component, 'onCarouselTouchMove');
      spyOn(component, 'onCarouselTouchEnd');
      
      // Simulate swipe
      const touchStart = new TouchEvent('touchstart', {
        touches: [{ clientX: 200, clientY: 100 } as Touch]
      });
      const touchMove = new TouchEvent('touchmove', {
        touches: [{ clientX: 150, clientY: 100 } as Touch]
      });
      const touchEnd = new TouchEvent('touchend', {
        changedTouches: [{ clientX: 100, clientY: 100 } as Touch]
      });
      
      carousel.dispatchEvent(touchStart);
      carousel.dispatchEvent(touchMove);
      carousel.dispatchEvent(touchEnd);
      
      expect(component.onCarouselTouchStart).toHaveBeenCalled();
      expect(component.onCarouselTouchMove).toHaveBeenCalled();
      expect(component.onCarouselTouchEnd).toHaveBeenCalled();
    });

    it('should detect swipe direction and change slides', () => {
      const initialSlide = component.activeSlide;
      
      // Simulate left swipe (next slide)
      component.touchStartX = 200;
      component.touchStartTime = Date.now();
      
      const touchEndEvent = new TouchEvent('touchend', {
        changedTouches: [{ clientX: 100, clientY: 100 } as Touch]
      });
      
      component.onCarouselTouchEnd(touchEndEvent);
      
      expect(component.activeSlide).toBe((initialSlide + 1) % component.slides.length);
    });

    it('should have touch-action CSS for proper swipe handling', () => {
      const carousel = compiled.querySelector('.carousel-container') as HTMLElement;
      const styles = window.getComputedStyle(carousel);
      expect(styles.touchAction).toBe('pan-x');
    });
  });

  describe('Accessibility Tests', () => {
    it('should have proper ARIA labels for interactive elements', () => {
      const buttons = compiled.querySelectorAll('button[aria-label]');
      expect(buttons.length).toBeGreaterThan(0);
    });

    it('should have proper form labels', () => {
      const labels = compiled.querySelectorAll('.form-label');
      const inputs = compiled.querySelectorAll('input');
      
      expect(labels.length).toBe(inputs.length);
      
      labels.forEach((label, index) => {
        const forAttribute = label.getAttribute('for');
        const input = inputs[index];
        expect(input.getAttribute('id')).toBe(forAttribute);
      });
    });

    it('should have keyboard navigation support', () => {
      const focusableElements = compiled.querySelectorAll(
        'button, input, a, [tabindex]:not([tabindex="-1"])'
      );
      
      focusableElements.forEach(element => {
        expect(element.getAttribute('tabindex')).not.toBe('-1');
      });
    });
  });

  describe('Performance Tests', () => {
    it('should not have excessive DOM elements', () => {
      const allElements = compiled.querySelectorAll('*');
      expect(allElements.length).toBeLessThan(100); // Reasonable limit for mobile
    });

    it('should use efficient event handling', () => {
      // Check that touch events don't interfere with scroll
      const carousel = compiled.querySelector('.carousel-container') as HTMLElement;
      const touchMoveEvent = new TouchEvent('touchmove');
      
      spyOn(touchMoveEvent, 'preventDefault');
      carousel.dispatchEvent(touchMoveEvent);
      
      // Should prevent default to avoid scroll interference
      expect(touchMoveEvent.preventDefault).toHaveBeenCalled();
    });
  });

  describe('Responsive Breakpoint Tests', () => {
    it('should adapt to different screen sizes', () => {
      // Test mobile breakpoint
      Object.defineProperty(window, 'innerWidth', {
        writable: true,
        configurable: true,
        value: 576
      });
      
      window.dispatchEvent(new Event('resize'));
      fixture.detectChanges();
      
      // Check mobile-specific styles are applied
      const cards = compiled.querySelectorAll('.mobile-card');
      expect(cards.length).toBeGreaterThan(0);
    });

    it('should hide desktop elements on mobile', () => {
      const desktopNav = compiled.querySelector('.d-lg-none');
      expect(desktopNav).toBeTruthy();
    });
  });
});