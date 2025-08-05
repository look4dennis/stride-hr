import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, fromEvent } from 'rxjs';
import { map, startWith, distinctUntilChanged, debounceTime } from 'rxjs/operators';

export interface BreakpointState {
  isMobile: boolean;
  isTablet: boolean;
  isDesktop: boolean;
  isLargeDesktop: boolean;
  screenWidth: number;
  screenHeight: number;
  orientation: 'portrait' | 'landscape';
  isTouch: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ResponsiveService {
  private breakpointSubject = new BehaviorSubject<BreakpointState>(this.getCurrentBreakpoint());
  
  // Breakpoint observables
  public breakpoint$ = this.breakpointSubject.asObservable();
  public isMobile$ = this.breakpoint$.pipe(map(bp => bp.isMobile));
  public isTablet$ = this.breakpoint$.pipe(map(bp => bp.isTablet));
  public isDesktop$ = this.breakpoint$.pipe(map(bp => bp.isDesktop));
  public isLargeDesktop$ = this.breakpoint$.pipe(map(bp => bp.isLargeDesktop));
  public orientation$ = this.breakpoint$.pipe(map(bp => bp.orientation));
  public isTouch$ = this.breakpoint$.pipe(map(bp => bp.isTouch));

  // Breakpoint constants
  private readonly BREAKPOINTS = {
    mobile: 768,
    tablet: 992,
    desktop: 1200,
    largeDesktop: 1400
  } as const;

  constructor() {
    // Initialize with current breakpoint to ensure the subject has a value
    this.breakpointSubject.next(this.getCurrentBreakpoint());
    this.initializeResponsiveListener();
  }

  private initializeResponsiveListener(): void {
    if (typeof window !== 'undefined') {
      // Listen to window resize events
      fromEvent(window, 'resize')
        .pipe(
          debounceTime(100),
          startWith(null),
          map(() => this.getCurrentBreakpoint()),
          distinctUntilChanged((prev, curr) => 
            prev.isMobile === curr.isMobile &&
            prev.isTablet === curr.isTablet &&
            prev.isDesktop === curr.isDesktop &&
            prev.orientation === curr.orientation
          )
        )
        .subscribe(breakpoint => {
          this.breakpointSubject.next(breakpoint);
        });

      // Listen to orientation changes
      fromEvent(window, 'orientationchange')
        .pipe(
          debounceTime(200),
          map(() => this.getCurrentBreakpoint())
        )
        .subscribe(breakpoint => {
          this.breakpointSubject.next(breakpoint);
        });
    }
  }

  private getCurrentBreakpoint(): BreakpointState {
    if (typeof window === 'undefined') {
      return {
        isMobile: false,
        isTablet: false,
        isDesktop: true,
        isLargeDesktop: false,
        screenWidth: 1200,
        screenHeight: 800,
        orientation: 'landscape',
        isTouch: false
      };
    }

    // Ensure BREAKPOINTS is defined
    if (!this.BREAKPOINTS) {
      return {
        isMobile: false,
        isTablet: false,
        isDesktop: true,
        isLargeDesktop: false,
        screenWidth: window.innerWidth || 1200,
        screenHeight: window.innerHeight || 800,
        orientation: 'landscape',
        isTouch: false
      };
    }

    const width = window.innerWidth;
    const height = window.innerHeight;
    const orientation = width > height ? 'landscape' : 'portrait';
    const isTouch = this.isTouchDevice();

    return {
      isMobile: width < this.BREAKPOINTS.mobile,
      isTablet: width >= this.BREAKPOINTS.mobile && width < this.BREAKPOINTS.tablet,
      isDesktop: width >= this.BREAKPOINTS.tablet && width < this.BREAKPOINTS.largeDesktop,
      isLargeDesktop: width >= this.BREAKPOINTS.largeDesktop,
      screenWidth: width,
      screenHeight: height,
      orientation,
      isTouch
    };
  }

  private isTouchDevice(): boolean {
    if (typeof window === 'undefined') return false;
    
    return (
      'ontouchstart' in window ||
      navigator.maxTouchPoints > 0 ||
      // @ts-ignore
      navigator.msMaxTouchPoints > 0
    );
  }

  // Utility methods
  public getCurrentState(): BreakpointState {
    return this.breakpointSubject.value;
  }

  public isMobile(): boolean {
    return this.getCurrentState().isMobile;
  }

  public isTablet(): boolean {
    return this.getCurrentState().isTablet;
  }

  public isDesktop(): boolean {
    return this.getCurrentState().isDesktop;
  }

  public isLargeDesktop(): boolean {
    return this.getCurrentState().isLargeDesktop;
  }

  public isMobileOrTablet(): boolean {
    const state = this.getCurrentState();
    return state.isMobile || state.isTablet;
  }

  public isDesktopOrLarger(): boolean {
    const state = this.getCurrentState();
    return state.isDesktop || state.isLargeDesktop;
  }

  public getOrientation(): 'portrait' | 'landscape' {
    return this.getCurrentState().orientation;
  }

  public isPortrait(): boolean {
    return this.getOrientation() === 'portrait';
  }

  public isLandscape(): boolean {
    return this.getOrientation() === 'landscape';
  }

  public isTouchEnabled(): boolean {
    return this.getCurrentState().isTouch;
  }

  public getScreenSize(): { width: number; height: number } {
    const state = this.getCurrentState();
    return {
      width: state.screenWidth,
      height: state.screenHeight
    };
  }

  // CSS class helpers
  public getResponsiveClasses(): string[] {
    const state = this.getCurrentState();
    const classes: string[] = [];

    if (state.isMobile) classes.push('is-mobile');
    if (state.isTablet) classes.push('is-tablet');
    if (state.isDesktop) classes.push('is-desktop');
    if (state.isLargeDesktop) classes.push('is-large-desktop');
    if (state.isTouch) classes.push('is-touch');
    classes.push(`is-${state.orientation}`);

    return classes;
  }

  // Media query helpers
  public matchesMediaQuery(query: string): boolean {
    if (typeof window === 'undefined') return false;
    return window.matchMedia(query).matches;
  }

  public createMediaQueryObservable(query: string): Observable<boolean> {
    if (typeof window === 'undefined') {
      return new BehaviorSubject(false).asObservable();
    }

    const mediaQuery = window.matchMedia(query);
    return fromEvent<MediaQueryListEvent>(mediaQuery, 'change')
      .pipe(
        startWith(mediaQuery),
        map((mq: MediaQueryList | MediaQueryListEvent) => mq.matches)
      );
  }

  // Viewport helpers
  public getViewportHeight(): number {
    if (typeof window === 'undefined') return 800;
    return window.innerHeight;
  }

  public getViewportWidth(): number {
    if (typeof window === 'undefined') return 1200;
    return window.innerWidth;
  }

  public getScrollbarWidth(): number {
    if (typeof window === 'undefined') return 0;
    
    const outer = document.createElement('div');
    outer.style.visibility = 'hidden';
    outer.style.overflow = 'scroll';
    // @ts-ignore - msOverflowStyle is IE specific
    outer.style.msOverflowStyle = 'scrollbar';
    document.body.appendChild(outer);

    const inner = document.createElement('div');
    outer.appendChild(inner);

    const scrollbarWidth = outer.offsetWidth - inner.offsetWidth;
    outer.parentNode?.removeChild(outer);

    return scrollbarWidth;
  }

  // Safe area helpers (for devices with notches)
  public getSafeAreaInsets(): {
    top: number;
    right: number;
    bottom: number;
    left: number;
  } {
    if (typeof window === 'undefined' || !CSS.supports('padding-top', 'env(safe-area-inset-top)')) {
      return { top: 0, right: 0, bottom: 0, left: 0 };
    }

    const computedStyle = getComputedStyle(document.documentElement);
    
    return {
      top: parseInt(computedStyle.getPropertyValue('--safe-area-inset-top') || '0'),
      right: parseInt(computedStyle.getPropertyValue('--safe-area-inset-right') || '0'),
      bottom: parseInt(computedStyle.getPropertyValue('--safe-area-inset-bottom') || '0'),
      left: parseInt(computedStyle.getPropertyValue('--safe-area-inset-left') || '0')
    };
  }

  // Performance helpers
  public shouldUseReducedMotion(): boolean {
    if (typeof window === 'undefined') return false;
    return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  }

  public shouldUseHighContrast(): boolean {
    if (typeof window === 'undefined') return false;
    return window.matchMedia('(prefers-contrast: high)').matches;
  }

  public shouldUseDarkMode(): boolean {
    if (typeof window === 'undefined') return false;
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
  }
}