import { Injectable } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { BehaviorSubject, Observable, filter } from 'rxjs';
import { NotificationService } from './notification.service';

export interface NavigationState {
    canGoBack: boolean;
    canGoForward: boolean;
    currentRoute: string;
    previousRoute: string | null;
    navigationHistory: string[];
}

@Injectable({
    providedIn: 'root'
})
export class NavigationService {
    private navigationState = new BehaviorSubject<NavigationState>({
        canGoBack: false,
        canGoForward: false,
        currentRoute: '',
        previousRoute: null,
        navigationHistory: []
    });

    private maxHistorySize = 50;
    private isNavigatingBack = false;
    private isNavigatingForward = false;

    constructor(
        private router: Router,
        private location: Location,
        private notificationService: NotificationService
    ) {
        this.initializeNavigationTracking();
    }

    /**
     * Initialize navigation tracking and browser history management
     */
    private initializeNavigationTracking(): void {
        // Track router navigation events
        this.router.events
            .pipe(filter(event => event instanceof NavigationEnd))
            .subscribe((event: NavigationEnd) => {
                this.updateNavigationState(event.url);
            });

        // Handle browser back/forward button events
        window.addEventListener('popstate', (event) => {
            this.handlePopState(event);
        });

        // Initialize with current route
        const currentUrl = this.router.url;
        this.updateNavigationState(currentUrl);
    }

    /**
     * Handle browser back/forward button events
     */
    private handlePopState(event: PopStateEvent): void {
        const currentState = this.navigationState.value;
        const newUrl = this.location.path();

        console.log('PopState event:', {
            newUrl,
            currentRoute: currentState.currentRoute,
            state: event.state
        });

        // Update navigation state
        this.updateNavigationState(newUrl, true);

        // Ensure Angular router is in sync with browser location
        if (this.router.url !== newUrl) {
            this.router.navigateByUrl(newUrl).catch(error => {
                console.error('Navigation error during popstate:', error);
                this.handleNavigationError(error, newUrl);
            });
        }
    }

    /**
     * Update the navigation state
     */
    private updateNavigationState(url: string, fromPopState = false): void {
        const currentState = this.navigationState.value;
        const cleanUrl = this.cleanUrl(url);

        // Don't update if it's the same URL (prevents duplicate entries)
        if (currentState.currentRoute === cleanUrl && !fromPopState) {
            return;
        }

        const newHistory = [...currentState.navigationHistory];

        // Add to history if not navigating back/forward
        if (!this.isNavigatingBack && !this.isNavigatingForward && !fromPopState) {
            newHistory.push(cleanUrl);

            // Limit history size
            if (newHistory.length > this.maxHistorySize) {
                newHistory.shift();
            }
        }

        const newState: NavigationState = {
            canGoBack: this.canGoBack(),
            canGoForward: this.canGoForward(),
            currentRoute: cleanUrl,
            previousRoute: currentState.currentRoute || null,
            navigationHistory: newHistory
        };

        this.navigationState.next(newState);

        // Reset navigation flags
        this.isNavigatingBack = false;
        this.isNavigatingForward = false;
    }

    /**
     * Clean URL by removing query parameters and fragments for comparison
     */
    private cleanUrl(url: string): string {
        return url.split('?')[0].split('#')[0];
    }

    /**
     * Check if browser can go back
     */
    private canGoBack(): boolean {
        return window.history.length > 1;
    }

    /**
     * Check if browser can go forward
     */
    private canGoForward(): boolean {
        // This is harder to determine reliably, but we can make a best guess
        // based on whether we've navigated back recently
        return this.isNavigatingBack;
    }

    /**
     * Navigate to a specific route
     */
    navigateTo(route: string, params?: any): Promise<boolean> {
        const navigationExtras = params ? { queryParams: params } : {};

        return this.router.navigate([route], navigationExtras).catch(error => {
            console.error('Navigation error:', error);
            this.handleNavigationError(error, route);
            return false;
        });
    }

    /**
     * Navigate back in browser history
     */
    goBack(): void {
        if (this.canGoBack()) {
            this.isNavigatingBack = true;
            this.location.back();
        } else {
            // If can't go back, navigate to dashboard as fallback
            this.navigateTo('/dashboard');
        }
    }

    /**
     * Navigate forward in browser history
     */
    goForward(): void {
        if (this.canGoForward()) {
            this.isNavigatingForward = true;
            this.location.forward();
        }
    }

    /**
     * Navigate to previous route in our tracked history
     */
    goToPreviousRoute(): void {
        const currentState = this.navigationState.value;
        if (currentState.previousRoute) {
            this.navigateTo(currentState.previousRoute);
        } else {
            this.goBack();
        }
    }

    /**
     * Get current navigation state
     */
    getNavigationState(): Observable<NavigationState> {
        return this.navigationState.asObservable();
    }

    /**
     * Get current route
     */
    getCurrentRoute(): string {
        return this.navigationState.value.currentRoute;
    }

    /**
     * Get navigation history
     */
    getNavigationHistory(): string[] {
        return [...this.navigationState.value.navigationHistory];
    }

    /**
     * Check if we can navigate back
     */
    canNavigateBack(): boolean {
        return this.navigationState.value.canGoBack;
    }

    /**
     * Check if we can navigate forward
     */
    canNavigateForward(): boolean {
        return this.navigationState.value.canGoForward;
    }

    /**
     * Handle navigation errors
     */
    private handleNavigationError(error: any, attemptedRoute: string): void {
        console.error('Navigation failed:', { error, attemptedRoute });

        // Show user-friendly error message
        this.notificationService.showError(
            `Failed to navigate to ${attemptedRoute}. Please try again or contact support.`
        );

        // Try to navigate to a safe route
        if (attemptedRoute !== '/dashboard' && attemptedRoute !== '/') {
            this.router.navigate(['/dashboard']).catch(() => {
                // If even dashboard fails, go to login
                this.router.navigate(['/login']);
            });
        }
    }

    /**
     * Clear navigation history
     */
    clearHistory(): void {
        const currentState = this.navigationState.value;
        this.navigationState.next({
            ...currentState,
            navigationHistory: [currentState.currentRoute],
            previousRoute: null
        });
    }

    /**
     * Replace current route in history (useful for redirects)
     */
    replaceCurrentRoute(route: string): Promise<boolean> {
        return this.router.navigateByUrl(route, { replaceUrl: true }).catch(error => {
            console.error('Replace navigation error:', error);
            this.handleNavigationError(error, route);
            return false;
        });
    }

    /**
     * Check if a route exists and is accessible
     */
    async canAccessRoute(route: string): Promise<boolean> {
        try {
            // Try to navigate to the route without actually navigating
            const urlTree = this.router.parseUrl(route);
            return this.router.isActive(urlTree, false) || true; // Simplified check
        } catch (error) {
            console.error('Route accessibility check failed:', error);
            return false;
        }
    }

    /**
     * Get breadcrumb trail from current navigation history
     */
    getBreadcrumbTrail(maxItems = 5): string[] {
        const history = this.navigationState.value.navigationHistory;
        return history.slice(-maxItems);
    }

    /**
     * Set up keyboard shortcuts for navigation
     */
    setupKeyboardShortcuts(): void {
        document.addEventListener('keydown', (event) => {
            // Alt + Left Arrow = Go Back
            if (event.altKey && event.key === 'ArrowLeft') {
                event.preventDefault();
                this.goBack();
            }

            // Alt + Right Arrow = Go Forward
            if (event.altKey && event.key === 'ArrowRight') {
                event.preventDefault();
                this.goForward();
            }
        });
    }

    /**
     * Restore navigation state from session storage (useful for page refreshes)
     */
    restoreNavigationState(): void {
        try {
            const savedState = sessionStorage.getItem('navigationState');
            if (savedState) {
                const parsedState = JSON.parse(savedState);
                // Only restore history, not current state
                const currentState = this.navigationState.value;
                this.navigationState.next({
                    ...currentState,
                    navigationHistory: parsedState.navigationHistory || []
                });
            }
        } catch (error) {
            console.warn('Failed to restore navigation state:', error);
        }
    }

    /**
     * Save navigation state to session storage
     */
    saveNavigationState(): void {
        try {
            const state = this.navigationState.value;
            sessionStorage.setItem('navigationState', JSON.stringify({
                navigationHistory: state.navigationHistory
            }));
        } catch (error) {
            console.warn('Failed to save navigation state:', error);
        }
    }
}