import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { TouchButtonComponent } from './touch-button/touch-button.component';
import { ResponsiveService } from '../../core/services/responsive.service';

@Component({
    template: `
    <app-touch-button 
      [variant]="'primary'"
      [size]="'md'"
      (buttonClick)="onButtonClick()">
      Test Button
    </app-touch-button>
  `
})
class TestTouchButtonComponent {
    onButtonClick() {
        console.log('Button clicked');
    }
}

describe('Mobile Responsive Implementation', () => {

    describe('TouchButtonComponent', () => {
        let component: TestTouchButtonComponent;
        let fixture: ComponentFixture<TestTouchButtonComponent>;

        beforeEach(async () => {
            await TestBed.configureTestingModule({
                imports: [TouchButtonComponent],
                declarations: [TestTouchButtonComponent]
            }).compileComponents();

            fixture = TestBed.createComponent(TestTouchButtonComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should create touch button component', () => {
            expect(component).toBeTruthy();
        });

        it('should have minimum touch target size', () => {
            const button = fixture.nativeElement.querySelector('button');
            expect(button).toBeTruthy();

            const styles = window.getComputedStyle(button);
            const minHeight = parseInt(styles.minHeight);

            expect(minHeight).toBeGreaterThanOrEqual(44);
        });

        it('should have touch-friendly styles', () => {
            const button = fixture.nativeElement.querySelector('button');
            const styles = window.getComputedStyle(button);

            expect(styles.touchAction).toBe('manipulation');
            expect(styles.cursor).toBe('pointer');
        });

        it('should emit click events', () => {
            spyOn(component, 'onButtonClick');

            const button = fixture.nativeElement.querySelector('button');
            button.click();

            expect(component.onButtonClick).toHaveBeenCalled();
        });
    });

    describe('ResponsiveService', () => {
        let service: ResponsiveService;

        beforeEach(() => {
            TestBed.configureTestingModule({});
            service = TestBed.inject(ResponsiveService);
        });

        it('should be created', () => {
            expect(service).toBeTruthy();
        });

        it('should detect mobile viewport', () => {
            // Mock mobile viewport
            Object.defineProperty(window, 'innerWidth', {
                writable: true,
                configurable: true,
                value: 375,
            });

            const state = service.getCurrentState();
            expect(state.isMobile).toBeTruthy();
        });

        it('should detect desktop viewport', () => {
            // Mock desktop viewport
            Object.defineProperty(window, 'innerWidth', {
                writable: true,
                configurable: true,
                value: 1200,
            });

            const state = service.getCurrentState();
            expect(state.isDesktop).toBeTruthy();
        });

        it('should provide responsive classes', () => {
            const classes = service.getResponsiveClasses();
            expect(classes).toContain('is-desktop');
        });
    });

    describe('Global Mobile Styles', () => {
        it('should have mobile-friendly form controls', () => {
            // Create a test input element
            const input = document.createElement('input');
            input.className = 'form-control';
            document.body.appendChild(input);

            const styles = window.getComputedStyle(input);

            // Check minimum height for touch targets
            expect(parseInt(styles.minHeight)).toBeGreaterThanOrEqual(44);

            // Check font size to prevent zoom on iOS
            expect(parseInt(styles.fontSize)).toBeGreaterThanOrEqual(16);

            document.body.removeChild(input);
        });

        it('should have touch-friendly buttons', () => {
            const button = document.createElement('button');
            button.className = 'btn btn-primary';
            document.body.appendChild(button);

            const styles = window.getComputedStyle(button);

            // Check minimum height for touch targets
            expect(parseInt(styles.minHeight)).toBeGreaterThanOrEqual(44);

            document.body.removeChild(button);
        });
    });
});