import { Directive, ElementRef, Input, OnInit, OnDestroy, Renderer2 } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { ResponsiveService, BreakpointState } from '../../core/services/responsive.service';

@Directive({
  selector: '[appResponsive]',
  standalone: true
})
export class ResponsiveDirective implements OnInit, OnDestroy {
  @Input() mobileClass: string = '';
  @Input() tabletClass: string = '';
  @Input() desktopClass: string = '';
  @Input() touchClass: string = '';
  @Input() portraitClass: string = '';
  @Input() landscapeClass: string = '';

  private destroy$ = new Subject<void>();
  private currentClasses: string[] = [];

  constructor(
    private el: ElementRef,
    private renderer: Renderer2,
    private responsiveService: ResponsiveService
  ) {}

  ngOnInit(): void {
    this.responsiveService.breakpoint$
      .pipe(takeUntil(this.destroy$))
      .subscribe(breakpoint => {
        this.updateClasses(breakpoint);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateClasses(breakpoint: BreakpointState): void {
    // Remove previous classes
    this.currentClasses.forEach(className => {
      if (className) {
        this.renderer.removeClass(this.el.nativeElement, className);
      }
    });

    // Determine new classes
    const newClasses: string[] = [];

    if (breakpoint.isMobile && this.mobileClass) {
      newClasses.push(this.mobileClass);
    }

    if (breakpoint.isTablet && this.tabletClass) {
      newClasses.push(this.tabletClass);
    }

    if ((breakpoint.isDesktop || breakpoint.isLargeDesktop) && this.desktopClass) {
      newClasses.push(this.desktopClass);
    }

    if (breakpoint.isTouch && this.touchClass) {
      newClasses.push(this.touchClass);
    }

    if (breakpoint.orientation === 'portrait' && this.portraitClass) {
      newClasses.push(this.portraitClass);
    }

    if (breakpoint.orientation === 'landscape' && this.landscapeClass) {
      newClasses.push(this.landscapeClass);
    }

    // Add new classes
    newClasses.forEach(className => {
      if (className) {
        this.renderer.addClass(this.el.nativeElement, className);
      }
    });

    this.currentClasses = newClasses;
  }
}

@Directive({
  selector: '[appHideOnMobile]',
  standalone: true
})
export class HideOnMobileDirective implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  constructor(
    private el: ElementRef,
    private renderer: Renderer2,
    private responsiveService: ResponsiveService
  ) {}

  ngOnInit(): void {
    this.responsiveService.isMobile$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isMobile => {
        if (isMobile) {
          this.renderer.setStyle(this.el.nativeElement, 'display', 'none');
        } else {
          this.renderer.removeStyle(this.el.nativeElement, 'display');
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}

@Directive({
  selector: '[appShowOnMobile]',
  standalone: true
})
export class ShowOnMobileDirective implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  constructor(
    private el: ElementRef,
    private renderer: Renderer2,
    private responsiveService: ResponsiveService
  ) {}

  ngOnInit(): void {
    this.responsiveService.isMobile$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isMobile => {
        if (!isMobile) {
          this.renderer.setStyle(this.el.nativeElement, 'display', 'none');
        } else {
          this.renderer.removeStyle(this.el.nativeElement, 'display');
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}

@Directive({
  selector: '[appTouchFriendly]',
  standalone: true
})
export class TouchFriendlyDirective implements OnInit, OnDestroy {
  @Input() minTouchTarget: number = 44; // Minimum touch target size in pixels

  private destroy$ = new Subject<void>();

  constructor(
    private el: ElementRef,
    private renderer: Renderer2,
    private responsiveService: ResponsiveService
  ) {}

  ngOnInit(): void {
    this.responsiveService.isTouch$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isTouch => {
        if (isTouch) {
          this.applyTouchFriendlyStyles();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private applyTouchFriendlyStyles(): void {
    const element = this.el.nativeElement;
    
    // Ensure minimum touch target size
    this.renderer.setStyle(element, 'min-height', `${this.minTouchTarget}px`);
    this.renderer.setStyle(element, 'min-width', `${this.minTouchTarget}px`);
    
    // Improve touch interaction
    this.renderer.setStyle(element, 'touch-action', 'manipulation');
    this.renderer.setStyle(element, '-webkit-tap-highlight-color', 'rgba(0, 0, 0, 0.1)');
    
    // Add touch-friendly cursor
    if (element.tagName.toLowerCase() === 'button' || 
        element.classList.contains('btn') ||
        element.getAttribute('role') === 'button') {
      this.renderer.setStyle(element, 'cursor', 'pointer');
    }
  }
}