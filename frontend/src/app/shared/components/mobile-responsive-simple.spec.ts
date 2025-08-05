import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { TouchButtonComponent } from './touch-button/touch-button.component';
import { ResponsiveService } from '../../core/services/responsive.service';

@Component({
    standalone: true,
    imports: [TouchButtonComponent],
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
                imports: [TouchButtonComponent, TestTouchButtonComponent]
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
            // Mock window object for testing
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

            TestBed.configureTestingModule({});
            service = TestBed.inject(ResponsiveService);
        });

        it('should be created', () => {
            expect(service).toBeTruthy();
        });

        it('should detect mobile viewport', (done) => {
            // Mock mobile viewport
            Object.defineProperty(window, 'innerWidth', {
                writable: true,
                configurable: true,
                value: 375,
            });

            // Trigger resize event to update service state
            window.dispatchEvent(new Event('resize'));
            
            // Wait for debounced update
            setTimeout(() => {
                const state = service.getCurrentState();
                expect(state.isMobile).toBeTruthy();
                done();
            }, 150);
        });

        it('should detect desktop viewport', (done) => {
            // Mock desktop viewport
            Object.defineProperty(window, 'innerWidth', {
                writable: true,
                configurable: true,
                value: 1200,
            });

            // Trigger resize event to update service state
            window.dispatchEvent(new Event('resize'));
            
            // Wait for debounced update
            setTimeout(() => {
                const state = service.getCurrentState();
                expect(state.isDesktop).toBeTruthy();
                done();
            }, 150);
        });

        it('should provide responsive classes', (done) => {
            // Mock desktop viewport
            Object.defineProperty(window, 'innerWidth', {
                writable: true,
                configurable: true,
                value: 1200,
            });

            // Trigger resize event to update service state
            window.dispatchEvent(new Event('resize'));
            
            // Wait for debounced update
            setTimeout(() => {
                const classes = service.getResponsiveClasses();
                expect(classes).toContain('is-desktop');
                done();
            }, 150);
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