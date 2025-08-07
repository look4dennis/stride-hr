// Dashboard Route Test Utility
// Test specifically for dashboard navigation issues

export async function testDashboardRoute(): Promise<{ success: boolean, error?: string }> {
    try {
        console.log('🧪 Testing dashboard component import...');

        // Test component import
        const dashboardModule = await import('../../features/dashboard/dashboard.component');
        if (!dashboardModule.DashboardComponent) {
            return {
                success: false,
                error: 'DashboardComponent not found in module'
            };
        }

        console.log('✅ Dashboard component imported successfully');

        // Test component dependencies
        console.log('🧪 Testing dashboard component dependencies...');

        try {
            await import('../../shared/components/weather-time-widget/weather-time-widget.component');
            console.log('✅ WeatherTimeWidget imported successfully');
        } catch (error: any) {
            console.warn('⚠️ WeatherTimeWidget import failed:', error.message);
        }

        try {
            await import('../../shared/components/birthday-widget/birthday-widget.component');
            console.log('✅ BirthdayWidget imported successfully');
        } catch (error: any) {
            console.warn('⚠️ BirthdayWidget import failed:', error.message);
        }

        try {
            await import('../../shared/components/quick-actions/quick-actions.component');
            console.log('✅ QuickActions imported successfully');
        } catch (error: any) {
            console.warn('⚠️ QuickActions import failed:', error.message);
        }

        return {
            success: true
        };

    } catch (error: any) {
        console.error('❌ Dashboard test failed:', error);
        return {
            success: false,
            error: error.message || 'Unknown error'
        };
    }
}

// Make it available globally for console testing
declare global {
    interface Window {
        testDashboard: () => Promise<void>;
    }
}

if (typeof window !== 'undefined') {
    window.testDashboard = async () => {
        console.log('🚀 Starting dashboard test...');
        const result = await testDashboardRoute();

        if (result.success) {
            console.log('✅ Dashboard test passed!');
            alert('✅ Dashboard test passed!');
        } else {
            console.error('❌ Dashboard test failed:', result.error);
            alert(`❌ Dashboard test failed: ${result.error}`);
        }
    };

    console.log('🧪 Dashboard test function loaded! Use testDashboard() to test.');
}