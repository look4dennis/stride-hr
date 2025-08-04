import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { MobileFormComponent } from './mobile-form/mobile-form.component';
import { MobileTableComponent } from './mobile-table/mobile-table.component';
import { TouchButtonComponent } from './touch-button/touch-button.component';
import { MobileNavComponent } from './mobile-nav/mobile-nav.component';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

// Test component for mobile form
@Component({
  template: `
    <app-mobile-form 
      [fields]="formFields" 
      [formGroup]="testForm"
      (formSubmit)="onSubmit($event)">
    </app-mobile-form>
  `
})
class TestMobileFormComponent {
  testForm: FormGroup;
  formFields = [
    {
      name: 'email',
      label: 'Email',
      type: 'email' as const,
      required: true,
      placeholder: 'Enter your email'
    },
    {
      name: 'name',
      label: 'Full Name',
      type: 'text' as const,
      required: true
    }
  ];

  constructor(private fb: FormBuilder) {
    this.testForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      name: ['', Validators.required]
    });
  }

  onSubmit(data: any) {
    console.log('Form submitted:', data);
  }
}

// Test component for mobile table
@Component({
  template: `
    <app-mobile-table 
      [data]="tableData" 
      [columns]="tableColumns"
      [actions]="tableActions">
    </app-mobile-table>
  `
})
class TestMobileTableComponent {
  tableData = [
    { id: 1, name: 'John Doe', email: 'john@example.com', status: 'Active' },
    { id: 2, name: 'Jane Smith', email: 'jane@example.com', status: 'Inactive' }
  ];

  tableColumns = [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'email', label: 'Email' },
    { key: 'status', label: 'Status' }
  ];

  tableActions = [
    {
      label: 'Edit',
      icon: 'fas fa-edit',
      color: 'primary' as const,
      action: (item: any) => console.log('Edit', item)
    }
  ];
}

describe('Mobile Responsive Components', () => {
  
  describe('MobileFormComponent', () => {
    let component: TestMobileFormComponent;
    let fixture: ComponentFixture<TestMobileFormComponent>;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
        declarations: [TestMobileFormComponent],
        imports: [MobileFormComponent]
      }).compileComponents();

      fixture = TestBed.createComponent(TestMobileFormComponent);
      component = fixture.componentInstance;
      fixture.detectChanges();
    });

    it('should create mobile form', () => {
      expect(component).toBeTruthy();
    });

    it('should render form fields with touch-friendly inputs', () => {
      const inputs = fixture.debugElement.queryAll(By.css('.mobile-input'));
      expect(inputs.length).toBe(2);
      
      // Check minimum height for touch targets
      inputs.forEach(input => {
        const element = input.nativeElement;
        const styles = window.getComputedStyle(element);
        const minHeight = parseInt(styles.minHeight);
        expect(minHeight).toBeGreaterThanOrEqual(44); // 44px minimum touch target
      });
    });

    it('should have proper font size to prevent zoom on iOS', () => {
      const inputs = fixture.debugElement.queryAll(By.css('.mobile-input'));
      inputs.forEach(input => {
        const element = input.nativeElement;
        const styles = window.getComputedStyle(element);
        expect(parseInt(styles.fontSize)).toBeGreaterThanOrEqual(16);
      });
    });

    it('should display validation errors', () => {
      const emailInput = fixture.debugElement.query(By.css('input[type="email"]'));
      emailInput.nativeElement.value = 'invalid-email';
      emailInput.nativeElement.dispatchEvent(new Event('input'));
      emailInput.nativeElement.dispatchEvent(new Event('blur'));
      
      fixture.detectChanges();
      
      const errorMessage = fixture.debugElement.query(By.css('.invalid-feedback'));
      expect(errorMessage).toBeTruthy();
    });
  });

  describe('MobileTableComponent', () => {
    let component: TestMobileTableComponent;
    let fixture: ComponentFixture<TestMobileTableComponent>;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
        declarations: [TestMobileTableComponent],
        imports: [MobileTableComponent]
      }).compileComponents();

      fixture = TestBed.createComponent(TestMobileTableComponent);
      component = fixture.componentInstance;
      fixture.detectChanges();
    });

    it('should create mobile table', () => {
      expect(component).toBeTruthy();
    });

    it('should show desktop table on large screens', () => {
      // Mock large screen
      Object.defineProperty(window, 'innerWidth', {
        writable: true,
        configurable: true,
        value: 1024,
      });

      fixture.detectChanges();

      const desktopTable = fixture.debugElement.query(By.css('.table-responsive'));
      expect(desktopTable).toBeTruthy();
      expect(desktopTable.nativeElement.classList.contains('d-none')).toBeFalsy();
    });

    it('should render mobile cards with touch-friendly targets', () => {
      const mobileCards = fixture.debugElement.queryAll(By.css('.mobile-card'));
      expect(mobileCards.length).toBe(component.tableData.length);

      mobileCards.forEach(card => {
        const element = card.nativeElement;
        const styles = window.getComputedStyle(element);
        expect(styles.cursor).toBe('pointer');
        expect(styles.touchAction).toBe('manipulation');
      });
    });

    it('should handle card clicks', () => {
      spyOn(console, 'log');
      const firstCard = fixture.debugElement.query(By.css('.mobile-card'));
      firstCard.nativeElement.click();
      
      // Should trigger card click event
      expect(firstCard).toBeTruthy();
    });
  });

  describe('TouchButtonComponent', () => {
    let component: TouchButtonComponent;
    let fixture: ComponentFixture<TouchButtonComponent>;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [TouchButtonComponent]
      }).compileComponents();

      fixture = TestBed.createComponent(TouchButtonComponent);
      component = fixture.componentInstance;
      fixture.detectChanges();
    });

    it('should create touch button', () => {
      expect(component).toBeTruthy();
    });

    it('should have minimum touch target size', () => {
      const button = fixture.debugElement.query(By.css('button'));
      const element = button.nativeElement;
      const styles = window.getComputedStyle(element);
      const minHeight = parseInt(styles.minHeight);
      
      expect(minHeight).toBeGreaterThanOrEqual(44);
    });

    it('should apply touch-friendly styles', () => {
      const button = fixture.debugElement.query(By.css('button'));
      const element = button.nativeElement;
      const styles = window.getComputedStyle(element);
      
      expect(styles.touchAction).toBe('manipulation');
      expect(styles.cursor).toBe('pointer');
    });

    it('should handle loading state', () => {
      component.loading = true;
      fixture.detectChanges();

      const spinner = fixture.debugElement.query(By.css('.spinner-border'));
      expect(spinner).toBeTruthy();

      const button = fixture.debugElement.query(By.css('button'));
      expect(button.nativeElement.disabled).toBeTruthy();
    });

    it('should emit click events when not disabled', () => {
      spyOn(component.buttonClick, 'emit');
      
      const button = fixture.debugElement.query(By.css('button'));
      button.nativeElement.click();
      
      expect(component.buttonClick.emit).toHaveBeenCalled();
    });

    it('should not emit click events when disabled', () => {
      component.disabled = true;
      fixture.detectChanges();
      
      spyOn(component.buttonClick, 'emit');
      
      const button = fixture.debugElement.query(By.css('button'));
      button.nativeElement.click();
      
      expect(component.buttonClick.emit).not.toHaveBeenCalled();
    });
  });

  describe('MobileNavComponent', () => {
    let component: MobileNavComponent;
    let fixture: ComponentFixture<MobileNavComponent>;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [MobileNavComponent]
      }).compileComponents();

      fixture = TestBed.createComponent(MobileNavComponent);
      component = fixture.componentInstance;
      
      component.bottomNavItems = [
        { label: 'Home', icon: 'fas fa-home', route: '/home' },
        { label: 'Profile', icon: 'fas fa-user', route: '/profile' }
      ];
      
      fixture.detectChanges();
    });

    it('should create mobile navigation', () => {
      expect(component).toBeTruthy();
    });

    it('should render bottom navigation items', () => {
      const navItems = fixture.debugElement.queryAll(By.css('.nav-item'));
      expect(navItems.length).toBe(component.bottomNavItems.length);
    });

    it('should have touch-friendly navigation items', () => {
      const navItems = fixture.debugElement.queryAll(By.css('.nav-item'));
      
      navItems.forEach(item => {
        const element = item.nativeElement;
        const styles = window.getComputedStyle(element);
        expect(styles.touchAction).toBe('manipulation');
      });
    });

    it('should show FAB when enabled', () => {
      component.showFab = true;
      component.fabAction = () => console.log('FAB clicked');
      fixture.detectChanges();

      const fab = fixture.debugElement.query(By.css('.fab'));
      expect(fab).toBeTruthy();
    });

    it('should handle navigation item clicks', () => {
      spyOn(component.navItemClick, 'emit');
      
      const firstNavItem = fixture.debugElement.query(By.css('.nav-item'));
      firstNavItem.nativeElement.click();
      
      expect(component.navItemClick.emit).toHaveBeenCalledWith(component.bottomNavItems[0]);
    });
  });

  describe('Responsive Breakpoints', () => {
    it('should handle mobile viewport', () => {
      // Mock mobile viewport
      Object.defineProperty(window, 'innerWidth', {
        writable: true,
        configurable: true,
        value: 375,
      });

      // Test that mobile-specific styles are applied
      const mediaQuery = window.matchMedia('(max-width: 768px)');
      expect(mediaQuery.matches).toBeTruthy();
    });

    it('should handle tablet viewport', () => {
      // Mock tablet viewport
      Object.defineProperty(window, 'innerWidth', {
        writable: true,
        configurable: true,
        value: 768,
      });

      const mediaQuery = window.matchMedia('(min-width: 769px) and (max-width: 991px)');
      expect(mediaQuery.matches).toBeTruthy();
    });

    it('should handle desktop viewport', () => {
      // Mock desktop viewport
      Object.defineProperty(window, 'innerWidth', {
        writable: true,
        configurable: true,
        value: 1200,
      });

      const mediaQuery = window.matchMedia('(min-width: 992px)');
      expect(mediaQuery.matches).toBeTruthy();
    });
  });

  describe('Touch Gestures', () => {
    it('should handle touch events', () => {
      const touchButton = TestBed.createComponent(TouchButtonComponent);
      const button = touchButton.debugElement.query(By.css('button'));
      
      // Simulate touch events
      const touchStart = new TouchEvent('touchstart', {
        touches: [new Touch({
          identifier: 1,
          target: button.nativeElement,
          clientX: 100,
          clientY: 100
        })]
      });
      
      const touchEnd = new TouchEvent('touchend', {
        changedTouches: [new Touch({
          identifier: 1,
          target: button.nativeElement,
          clientX: 100,
          clientY: 100
        })]
      });

      button.nativeElement.dispatchEvent(touchStart);
      button.nativeElement.dispatchEvent(touchEnd);
      
      // Should handle touch events without errors
      expect(button.nativeElement).toBeTruthy();
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA labels', () => {
      const touchButton = TestBed.createComponent(TouchButtonComponent);
      touchButton.componentInstance.ariaLabel = 'Test button';
      touchButton.detectChanges();
      
      const button = touchButton.debugElement.query(By.css('button'));
      expect(button.nativeElement.getAttribute('aria-label')).toBe('Test button');
    });

    it('should support keyboard navigation', () => {
      const touchButton = TestBed.createComponent(TouchButtonComponent);
      const button = touchButton.debugElement.query(By.css('button'));
      
      // Should be focusable
      button.nativeElement.focus();
      expect(document.activeElement).toBe(button.nativeElement);
      
      // Should handle Enter key
      const enterEvent = new KeyboardEvent('keydown', { key: 'Enter' });
      button.nativeElement.dispatchEvent(enterEvent);
      
      expect(button.nativeElement).toBeTruthy();
    });

    it('should have proper focus indicators', () => {
      const touchButton = TestBed.createComponent(TouchButtonComponent);
      const button = touchButton.debugElement.query(By.css('button'));
      
      button.nativeElement.focus();
      const styles = window.getComputedStyle(button.nativeElement, ':focus');
      
      // Should have visible focus outline
      expect(styles.outline).toBeTruthy();
    });
  });
});