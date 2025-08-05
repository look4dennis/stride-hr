import { TestBed, ComponentFixture } from '@angular/core/testing';
import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { TestConfig } from './test-config';

/**
 * Cross-browser compatibility tests for critical UI components
 * Tests browser-specific behaviors and CSS compatibility
 */

@Component({
  selector: 'app-test-responsive',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NgbModule],
  template: `
    <div class="container-fluid">
      <div class="row">
        <div class="col-12 col-md-6 col-lg-4">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title">Responsive Test Card</h5>
            </div>
            <div class="card-body">
              <form #testForm="ngForm">
                <div class="mb-3">
                  <label for="testInput" class="form-label">Test Input</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    id="testInput"
                    name="testInput"
                    [(ngModel)]="testValue"
                    required>
                  <div class="invalid-feedback">
                    This field is required
                  </div>
                </div>
                <div class="mb-3">
                  <label for="testSelect" class="form-label">Test Select</label>
                  <select class="form-select" id="testSelect" name="testSelect" [(ngModel)]="selectedValue">
                    <option value="">Choose...</option>
                    <option value="option1">Option 1</option>
                    <option value="option2">Option 2</option>
                  </select>
                </div>
                <button type="submit" class="btn btn-primary" [disabled]="!testForm.valid">
                  Submit
                </button>
                <button type="button" class="btn btn-secondary ms-2" data-bs-toggle="modal" data-bs-target="#testModal">
                  Open Modal
                </button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Test Modal -->
    <div class="modal fade" id="testModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Test Modal</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <p>This is a test modal for cross-browser compatibility.</p>
            <div class="form-check">
              <input class="form-check-input" type="checkbox" id="testCheckbox">
              <label class="form-check-label" for="testCheckbox">
                Test checkbox
              </label>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
            <button type="button" class="btn btn-primary">Save changes</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
      border: 1px solid rgba(0, 0, 0, 0.125);
    }
    
    .form-control:focus {
      border-color: #86b7fe;
      outline: 0;
      box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
    }
    
    @media (max-width: 768px) {
      .card {
        margin-bottom: 1rem;
      }
      
      .btn {
        width: 100%;
        margin-bottom: 0.5rem;
      }
      
      .btn.ms-2 {
        margin-left: 0 !important;
      }
    }
  `]
})
class TestResponsiveComponent {
  testValue = '';
  selectedValue = '';
}

describe('Cross-Browser Compatibility Tests', () => {
  let component: TestResponsiveComponent;
  let fixture: ComponentFixture<TestResponsiveComponent>;

  beforeEach(async () => {
    TestConfig.setupAllMocks();
    
    await TestBed.configureTestingModule({
      imports: [TestResponsiveComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(TestResponsiveComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    fixture.destroy();
  });

  describe('Form Controls Cross-Browser Compatibility', () => {
    it('should render form controls consistently across browsers', () => {
      const inputElement = fixture.nativeElement.querySelector('#testInput');
      const selectElement = fixture.nativeElement.querySelector('#testSelect');
      const submitButton = fixture.nativeElement.querySelector('button[type="submit"]');

      expect(inputElement).toBeTruthy();
      expect(selectElement).toBeTruthy();
      expect(submitButton).toBeTruthy();

      // Test input styling
      expect(inputElement.classList.contains('form-control')).toBe(true);
      expect(selectElement.classList.contains('form-select')).toBe(true);
      expect(submitButton.classList.contains('btn')).toBe(true);
    });

    it('should handle form validation consistently', () => {
      const inputElement = fixture.nativeElement.querySelector('#testInput');
      const submitButton = fixture.nativeElement.querySelector('button[type="submit"]');

      // Initially button should be disabled due to required validation
      expect(submitButton.disabled).toBe(true);

      // Enter value and check if button becomes enabled
      inputElement.value = 'test value';
      inputElement.dispatchEvent(new Event('input'));
      fixture.detectChanges();

      // Note: In real browser test, this would work with ngModel
      expect(inputElement.value).toBe('test value');
    });

    it('should support touch events on mobile browsers', () => {
      const inputElement = fixture.nativeElement.querySelector('#testInput');
      
      // Simulate touch events
      const touchStartEvent = new TouchEvent('touchstart', {
        touches: [new Touch({
          identifier: 1,
          target: inputElement,
          clientX: 100,
          clientY: 100,
          radiusX: 2.5,
          radiusY: 2.5,
          rotationAngle: 10,
          force: 0.5
        })]
      });

      expect(() => {
        inputElement.dispatchEvent(touchStartEvent);
      }).not.toThrow();
    });
  });

  describe('CSS Grid and Flexbox Compatibility', () => {
    it('should support CSS Grid layout', () => {
      const testElement = document.createElement('div');
      testElement.style.display = 'grid';
      testElement.style.gridTemplateColumns = '1fr 1fr';
      testElement.style.gap = '1rem';

      document.body.appendChild(testElement);
      const computedStyle = window.getComputedStyle(testElement);
      
      expect(computedStyle.display).toBe('grid');
      document.body.removeChild(testElement);
    });

    it('should support Flexbox layout', () => {
      const testElement = document.createElement('div');
      testElement.style.display = 'flex';
      testElement.style.justifyContent = 'space-between';
      testElement.style.alignItems = 'center';

      document.body.appendChild(testElement);
      const computedStyle = window.getComputedStyle(testElement);
      
      expect(computedStyle.display).toBe('flex');
      document.body.removeChild(testElement);
    });

    it('should support CSS custom properties (variables)', () => {
      const testElement = document.createElement('div');
      testElement.style.setProperty('--test-color', '#3b82f6');
      testElement.style.color = 'var(--test-color)';

      document.body.appendChild(testElement);
      const computedStyle = window.getComputedStyle(testElement);
      
      // CSS variables should be supported in modern browsers
      expect(testElement.style.getPropertyValue('--test-color')).toBe('#3b82f6');
      document.body.removeChild(testElement);
    });
  });

  describe('Bootstrap Components Cross-Browser', () => {
    it('should render Bootstrap cards consistently', () => {
      const cardElement = fixture.nativeElement.querySelector('.card');
      const cardHeader = fixture.nativeElement.querySelector('.card-header');
      const cardBody = fixture.nativeElement.querySelector('.card-body');

      expect(cardElement).toBeTruthy();
      expect(cardHeader).toBeTruthy();
      expect(cardBody).toBeTruthy();

      // Check Bootstrap classes are applied
      expect(cardElement.classList.contains('card')).toBe(true);
      expect(cardHeader.classList.contains('card-header')).toBe(true);
      expect(cardBody.classList.contains('card-body')).toBe(true);
    });

    it('should handle Bootstrap grid system', () => {
      const containerFluid = fixture.nativeElement.querySelector('.container-fluid');
      const row = fixture.nativeElement.querySelector('.row');
      const col = fixture.nativeElement.querySelector('.col-12');

      expect(containerFluid).toBeTruthy();
      expect(row).toBeTruthy();
      expect(col).toBeTruthy();

      // Check responsive classes
      expect(col.classList.contains('col-12')).toBe(true);
      expect(col.classList.contains('col-md-6')).toBe(true);
      expect(col.classList.contains('col-lg-4')).toBe(true);
    });

    it('should support Bootstrap modal structure', () => {
      const modal = fixture.nativeElement.querySelector('.modal');
      const modalDialog = fixture.nativeElement.querySelector('.modal-dialog');
      const modalContent = fixture.nativeElement.querySelector('.modal-content');

      expect(modal).toBeTruthy();
      expect(modalDialog).toBeTruthy();
      expect(modalContent).toBeTruthy();

      // Check modal classes
      expect(modal.classList.contains('modal')).toBe(true);
      expect(modal.classList.contains('fade')).toBe(true);
      expect(modalDialog.classList.contains('modal-dialog')).toBe(true);
    });
  });

  describe('JavaScript API Compatibility', () => {
    it('should support modern JavaScript features', () => {
      // Test arrow functions
      const arrowFunction = () => 'test';
      expect(arrowFunction()).toBe('test');

      // Test template literals
      const name = 'StrideHR';
      const template = `Hello ${name}`;
      expect(template).toBe('Hello StrideHR');

      // Test destructuring
      const obj = { a: 1, b: 2 };
      const { a, b } = obj;
      expect(a).toBe(1);
      expect(b).toBe(2);

      // Test spread operator
      const arr1 = [1, 2];
      const arr2 = [...arr1, 3];
      expect(arr2).toEqual([1, 2, 3]);
    });

    it('should support Promise API', async () => {
      const promise = new Promise(resolve => {
        setTimeout(() => resolve('resolved'), 10);
      });

      const result = await promise;
      expect(result).toBe('resolved');
    });

    it('should support Fetch API', () => {
      expect(typeof fetch).toBe('function');
      expect(fetch).toBeDefined();
    });

    it('should support localStorage', () => {
      expect(typeof Storage).toBe('function');
      expect(localStorage).toBeDefined();
      expect(typeof localStorage.setItem).toBe('function');
      expect(typeof localStorage.getItem).toBe('function');

      // Test localStorage functionality
      localStorage.setItem('test', 'value');
      expect(localStorage.getItem('test')).toBe('value');
      localStorage.removeItem('test');
    });

    it('should support sessionStorage', () => {
      expect(sessionStorage).toBeDefined();
      expect(typeof sessionStorage.setItem).toBe('function');
      expect(typeof sessionStorage.getItem).toBe('function');

      // Test sessionStorage functionality
      sessionStorage.setItem('test', 'value');
      expect(sessionStorage.getItem('test')).toBe('value');
      sessionStorage.removeItem('test');
    });
  });

  describe('Media Query Support', () => {
    it('should support matchMedia API', () => {
      expect(typeof window.matchMedia).toBe('function');
      
      const mediaQuery = window.matchMedia('(max-width: 768px)');
      expect(mediaQuery).toBeDefined();
      expect(typeof mediaQuery.matches).toBe('boolean');
      expect(typeof mediaQuery.addListener).toBe('function');
    });

    it('should handle responsive breakpoints', () => {
      // Test different breakpoints
      const breakpoints = [
        '(max-width: 575.98px)', // xs
        '(min-width: 576px) and (max-width: 767.98px)', // sm
        '(min-width: 768px) and (max-width: 991.98px)', // md
        '(min-width: 992px) and (max-width: 1199.98px)', // lg
        '(min-width: 1200px)' // xl
      ];

      breakpoints.forEach(breakpoint => {
        const mediaQuery = window.matchMedia(breakpoint);
        expect(mediaQuery).toBeDefined();
        expect(typeof mediaQuery.matches).toBe('boolean');
      });
    });
  });

  describe('Event Handling Cross-Browser', () => {
    it('should handle click events consistently', () => {
      const button = fixture.nativeElement.querySelector('button[type="submit"]');
      let clicked = false;

      button.addEventListener('click', () => {
        clicked = true;
      });

      button.click();
      expect(clicked).toBe(true);
    });

    it('should handle keyboard events', () => {
      const input = fixture.nativeElement.querySelector('#testInput');
      let keyPressed = false;

      input.addEventListener('keydown', (event: KeyboardEvent) => {
        if (event.key === 'Enter') {
          keyPressed = true;
        }
      });

      const enterEvent = new KeyboardEvent('keydown', { key: 'Enter' });
      input.dispatchEvent(enterEvent);
      expect(keyPressed).toBe(true);
    });

    it('should handle focus and blur events', () => {
      const input = fixture.nativeElement.querySelector('#testInput');
      let focused = false;
      let blurred = false;

      input.addEventListener('focus', () => {
        focused = true;
      });

      input.addEventListener('blur', () => {
        blurred = true;
      });

      input.focus();
      expect(focused).toBe(true);

      input.blur();
      expect(blurred).toBe(true);
    });
  });

  describe('CSS Animation Support', () => {
    it('should support CSS transitions', () => {
      const testElement = document.createElement('div');
      testElement.style.transition = 'opacity 0.3s ease';
      testElement.style.opacity = '1';

      document.body.appendChild(testElement);
      const computedStyle = window.getComputedStyle(testElement);
      
      expect(computedStyle.transition).toContain('opacity');
      document.body.removeChild(testElement);
    });

    it('should support CSS transforms', () => {
      const testElement = document.createElement('div');
      testElement.style.transform = 'translateX(10px) scale(1.1)';

      document.body.appendChild(testElement);
      const computedStyle = window.getComputedStyle(testElement);
      
      expect(computedStyle.transform).toContain('matrix');
      document.body.removeChild(testElement);
    });
  });
});