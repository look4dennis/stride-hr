// Module loading test utility
export function testModuleLoading() {
    console.log('Testing module loading...');

    // Test critical imports
    const tests = [
        // Core components
        () => import('../../features/dashboard/dashboard.component').then(m => m.DashboardComponent),
        () => import('../../features/auth/login/login.component').then(m => m.LoginComponent),
        () => import('../../shared/components/layout/layout.component').then(m => m.LayoutComponent),

        // Employee components
        () => import('../../features/employees/employee-list/employee-list.component').then(m => m.EmployeeListComponent),
        () => import('../../features/employees/employee-create/employee-create.component').then(m => m.EmployeeCreateComponent),

        // Attendance components
        () => import('../../features/attendance/attendance-tracker/attendance-tracker.component').then(m => m.AttendanceTrackerComponent),
        () => import('../../features/attendance/attendance-now/attendance-now.component').then(m => m.AttendanceNowComponent),

        // Settings components
        () => import('../../features/settings/branch-management.component').then(m => m.BranchManagementComponent),
        () => import('../../features/settings/settings.component').then(m => m.SettingsComponent),

        // Error components
        () => import('../../shared/components/not-found/not-found.component').then(m => m.NotFoundComponent),
        () => import('../../shared/components/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent),
        () => import('../../shared/components/route-error-page/route-error-page.component').then(m => m.RouteErrorPageComponent),
    ];

    const results: { component: string; success: boolean; error?: any }[] = [];

    Promise.allSettled(tests.map((test, index) =>
        test().then(component => {
            results[index] = {
                component: getComponentName(index),
                success: true
            };
            return component;
        }).catch(error => {
            results[index] = {
                component: getComponentName(index),
                success: false,
                error
            };
            throw error;
        })
    )).then(() => {
        console.log('Module loading test results:', results);

        const failed = results.filter(r => !r.success);
        if (failed.length > 0) {
            console.error('Failed to load modules:', failed);
        } else {
            console.log('All modules loaded successfully!');
        }
    });
}

function getComponentName(index: number): string {
    const names = [
        'DashboardComponent',
        'LoginComponent',
        'LayoutComponent',
        'EmployeeListComponent',
        'EmployeeCreateComponent',
        'AttendanceTrackerComponent',
        'AttendanceNowComponent',
        'BranchManagementComponent',
        'SettingsComponent',
        'NotFoundComponent',
        'UnauthorizedComponent',
        'RouteErrorPageComponent'
    ];
    return names[index] || `Component${index}`;
}

// Make it available globally for console testing
if (typeof window !== 'undefined') {
    (window as any).testModuleLoading = testModuleLoading;
}