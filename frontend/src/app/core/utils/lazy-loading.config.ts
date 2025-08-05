import { Routes } from '@angular/router';

/**
 * Lazy loading configuration for optimal performance
 * This configuration implements route-based code splitting
 */
// Simple placeholder component for missing features
const PlaceholderComponent = () => import('../../shared/components/placeholder/placeholder.component').then(m => m.PlaceholderComponent);

export const LAZY_ROUTES: Routes = [
    {
        path: 'employees',
        loadChildren: () => import('../../features/employees/employees.routes')
            .then(m => m.EMPLOYEE_ROUTES)
            .catch(() => {
                console.warn('Employee routes not found');
                return [];
            }),
        data: { preload: true }
    }
];

/**
 * Feature routes configuration - these will be loaded dynamically as features are implemented
 * Using string-based configuration to avoid TypeScript import errors
 */
export const FEATURE_ROUTES_CONFIG = [
    {
        path: 'dashboard',
        type: 'component',
        preload: true
    },
    {
        path: 'attendance',
        type: 'routes',
        preload: true
    },
    {
        path: 'payroll',
        type: 'routes',
        preload: false
    },
    {
        path: 'projects',
        type: 'routes',
        preload: false
    },
    {
        path: 'performance',
        type: 'routes',
        preload: false
    },
    {
        path: 'training',
        type: 'routes',
        preload: false
    },
    {
        path: 'reports',
        type: 'routes',
        preload: false
    },
    {
        path: 'settings',
        type: 'routes',
        preload: false
    },
    {
        path: 'admin',
        type: 'routes',
        preload: false,
        roles: ['Admin']
    }
];

/**
 * Generate complete routes with error handling for missing modules
 */
export function generateLazyRoutes(): Routes {
    const routes: Routes = [...LAZY_ROUTES];

    FEATURE_ROUTES_CONFIG.forEach(config => {
        if (config.type === 'component') {
            // Handle component-based routes
            routes.push({
                path: config.path,
                loadComponent: () => {
                    const componentPath = `../../features/${config.path}/${config.path}.component`;
                    return import(componentPath)
                        .then((m: any) => m[`${config.path.charAt(0).toUpperCase() + config.path.slice(1)}Component`])
                        .catch(() => {
                            console.warn(`Component for ${config.path} not found, using placeholder`);
                            // Return a simple inline component as fallback
                            return class PlaceholderComponent {
                                template = `<div class="placeholder">
                                    <h3>${config.path.charAt(0).toUpperCase() + config.path.slice(1)} Feature</h3>
                                    <p>This feature is coming soon!</p>
                                </div>`;
                            };
                        });
                },
                data: {
                    preload: config.preload,
                    roles: config.roles
                }
            });
        } else {
            // Handle route-based lazy loading
            routes.push({
                path: config.path,
                loadChildren: () => {
                    const routePath = `../../features/${config.path}/${config.path}.routes`;
                    const exportName = `${config.path.toUpperCase()}_ROUTES`;
                    
                    return import(routePath)
                        .then((m: any) => m[exportName])
                        .catch(() => {
                            console.warn(`Feature module ${config.path} not found, using empty routes`);
                            return [];
                        });
                },
                data: {
                    preload: config.preload,
                    roles: config.roles
                }
            });
        }
    });

    return routes;
}

/**
 * Custom preloading strategy for selective module preloading
 */
import { Injectable } from '@angular/core';
import { PreloadingStrategy, Route } from '@angular/router';
import { Observable, of } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class SelectivePreloadingStrategy implements PreloadingStrategy {
    private preloadedModules: string[] = [];

    preload(route: Route, load: () => Observable<any>): Observable<any> {
        // Check if route should be preloaded
        if (route.data && route.data['preload']) {
            console.log('Preloading: ' + route.path);
            this.preloadedModules.push(route.path || '');
            return load();
        }

        return of(null);
    }

    getPreloadedModules(): string[] {
        return this.preloadedModules;
    }
}

/**
 * Network-aware preloading strategy
 */
@Injectable({
    providedIn: 'root'
})
export class NetworkAwarePreloadingStrategy implements PreloadingStrategy {
    preload(route: Route, load: () => Observable<any>): Observable<any> {
        // Check network conditions
        const connection = (navigator as any).connection;

        if (connection) {
            // Don't preload on slow connections
            if (connection.effectiveType === 'slow-2g' || connection.effectiveType === '2g') {
                return of(null);
            }

            // Don't preload if user has data saver enabled
            if (connection.saveData) {
                return of(null);
            }
        }

        // Preload if route is marked for preloading and network conditions are good
        if (route.data && route.data['preload']) {
            return load();
        }

        return of(null);
    }
}

/**
 * Bundle optimization configuration
 */
export const BUNDLE_CONFIG = {
    // Vendor chunks configuration
    vendor: [
        '@angular/core',
        '@angular/common',
        '@angular/forms',
        '@angular/router',
        '@angular/platform-browser',
        'rxjs'
    ],

    // Common chunks that should be shared
    common: [
        './shared/components',
        './shared/services',
        './core/services'
    ],

    // Lazy chunks for specific features
    lazy: {
        'employee-management': [
            './features/employees',
            './features/attendance'
        ],
        'payroll-management': [
            './features/payroll',
            './features/reports'
        ],
        'project-management': [
            './features/projects',
            './features/performance'
        ],
        'admin-features': [
            './features/admin',
            './features/settings'
        ]
    }
};

/**
 * Performance optimization utilities
 */
export class LazyLoadingOptimizer {
    /**
     * Preload critical modules based on user role
     */
    static preloadByRole(userRole: string): string[] {
        const roleBasedModules: { [key: string]: string[] } = {
            'Employee': ['dashboard', 'attendance', 'performance'],
            'Manager': ['dashboard', 'employees', 'attendance', 'performance', 'reports'],
            'HR': ['dashboard', 'employees', 'attendance', 'payroll', 'performance', 'reports'],
            'Admin': ['dashboard', 'employees', 'attendance', 'payroll', 'performance', 'reports', 'settings', 'admin']
        };

        return roleBasedModules[userRole] || ['dashboard'];
    }

    /**
     * Get modules to preload based on usage patterns
     */
    static getPreloadModules(usageStats: { [module: string]: number }): string[] {
        // Sort modules by usage frequency and preload top 3
        return Object.entries(usageStats)
            .sort(([, a], [, b]) => b - a)
            .slice(0, 3)
            .map(([module]) => module);
    }

    /**
     * Check if module should be preloaded based on device capabilities
     */
    static shouldPreload(): boolean {
        // Check device memory
        const memory = (navigator as any).deviceMemory;
        if (memory && memory < 4) {
            return false; // Don't preload on low-memory devices
        }

        // Check CPU cores
        const cores = navigator.hardwareConcurrency;
        if (cores && cores < 4) {
            return false; // Don't preload on low-end devices
        }

        return true;
    }
}

/**
 * Image optimization configuration
 */
export const IMAGE_OPTIMIZATION = {
    // Lazy loading configuration
    lazyLoading: {
        rootMargin: '50px 0px',
        threshold: 0.01
    },

    // Image formats by priority
    formats: ['webp', 'avif', 'jpg', 'png'],

    // Responsive image breakpoints
    breakpoints: [320, 640, 768, 1024, 1280, 1920],

    // Quality settings
    quality: {
        thumbnail: 60,
        medium: 75,
        large: 85,
        original: 95
    }
};

/**
 * Service Worker optimization
 */
export const SW_CONFIG = {
    // Cache strategies
    cacheStrategies: {
        api: 'NetworkFirst',
        assets: 'CacheFirst',
        pages: 'StaleWhileRevalidate'
    },

    // Cache durations
    cacheDurations: {
        api: 5 * 60 * 1000, // 5 minutes
        assets: 30 * 24 * 60 * 60 * 1000, // 30 days
        pages: 24 * 60 * 60 * 1000 // 24 hours
    },

    // Resources to precache
    precache: [
        '/assets/icons/icon-192x192.png',
        '/assets/icons/icon-512x512.png',
        '/assets/css/critical.css'
    ]
};