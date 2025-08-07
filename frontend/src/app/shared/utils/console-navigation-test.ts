// Console Navigation Test Utility
// Run this in the browser console to test all navigation routes

declare global {
  interface Window {
    testNavigation: () => Promise<void>;
    testSingleRoute: (route: string) => Promise<boolean>;
  }
}

export function setupConsoleNavigationTest() {
  // Get Angular router from the global scope
  const getRouter = () => {
    const appElement = document.querySelector('app-root');
    if (appElement && (appElement as any).__ngContext__) {
      const context = (appElement as any).__ngContext__;
      const injector = context[0];
      return injector.get('Router');
    }
    return null;
  };

  // Test routes
  const testRoutes = [
    { name: 'Dashboard', route: '/dashboard' },
    { name: 'Employee List', route: '/employees' },
    { name: 'Create Employee', route: '/employees/add' },
    { name: 'Organization Chart', route: '/employees/org-chart' },
    { name: 'Attendance Tracker', route: '/attendance' },
    { name: 'Attendance Now', route: '/attendance/now' },
    { name: 'Attendance Calendar', route: '/attendance/calendar' },
    { name: 'Attendance Reports', route: '/attendance/reports' },
    { name: 'Attendance Corrections', route: '/attendance/corrections' },
    { name: 'Projects', route: '/projects' },
    { name: 'Kanban Board', route: '/projects/kanban' },
    { name: 'Project Monitoring', route: '/projects/monitoring' },
    { name: 'Payroll', route: '/payroll' },
    { name: 'Payroll Processing', route: '/payroll/processing' },
    { name: 'Payroll Approval', route: '/payroll/approval' },
    { name: 'Payroll Reports', route: '/payroll/reports' },
    { name: 'Leave Management', route: '/leave' },
    { name: 'Leave Request', route: '/leave/request' },
    { name: 'Leave Balance', route: '/leave/balance' },
    { name: 'Leave Calendar', route: '/leave/calendar' },
    { name: 'Performance', route: '/performance' },
    { name: 'Performance Review', route: '/performance/review' },
    { name: 'PIP Management', route: '/performance/pip' },
    { name: 'Certifications', route: '/performance/certifications' },
    { name: 'Reports', route: '/reports' },
    { name: 'Report Builder', route: '/reports/builder' },
    { name: 'Analytics Dashboard', route: '/reports/analytics' },
    { name: 'Settings', route: '/settings' },
    { name: 'Organization Settings', route: '/settings/organization' },
    { name: 'Branch Management', route: '/settings/branches' },
    { name: 'Role Management', route: '/settings/roles' },
    { name: 'System Config', route: '/settings/system' },
    { name: 'Admin Settings', route: '/settings/admin' },
    { name: 'Training', route: '/training' },
    { name: 'Profile', route: '/profile' }
  ];

  // Test single route function
  window.testSingleRoute = async (route: string): Promise<boolean> => {
    const router = getRouter();
    if (!router) {
      console.error('❌ Could not get Angular router');
      return false;
    }

    try {
      console.log(`🧪 Testing route: ${route}`);
      const result = await router.navigate([route]);
      
      if (result) {
        console.log(`✅ ${route} - Navigation successful`);
        return true;
      } else {
        console.log(`❌ ${route} - Navigation failed`);
        return false;
      }
    } catch (error: any) {
      console.log(`❌ ${route} - Error: ${error.message}`);
      return false;
    }
  };

  // Test all routes function
  window.testNavigation = async (): Promise<void> => {
    console.log('🚀 Starting comprehensive navigation test...');
    console.log('📋 Testing', testRoutes.length, 'routes');
    
    const router = getRouter();
    if (!router) {
      console.error('❌ Could not get Angular router. Make sure you are on the StrideHR application page.');
      return;
    }

    const originalRoute = router.url;
    const results: Array<{name: string, route: string, success: boolean, error?: string}> = [];

    for (const testRoute of testRoutes) {
      try {
        console.log(`🧪 Testing: ${testRoute.name} (${testRoute.route})`);
        
        const navigationResult = await router.navigate([testRoute.route]);
        
        if (navigationResult) {
          console.log(`✅ ${testRoute.name} - SUCCESS`);
          results.push({ name: testRoute.name, route: testRoute.route, success: true });
        } else {
          console.log(`❌ ${testRoute.name} - FAILED (navigation returned false)`);
          results.push({ 
            name: testRoute.name, 
            route: testRoute.route, 
            success: false, 
            error: 'Navigation returned false' 
          });
        }
      } catch (error: any) {
        console.log(`❌ ${testRoute.name} - ERROR: ${error.message}`);
        results.push({ 
          name: testRoute.name, 
          route: testRoute.route, 
          success: false, 
          error: error.message 
        });
      }

      // Small delay between tests
      await new Promise(resolve => setTimeout(resolve, 100));
    }

    // Return to original route
    try {
      await router.navigate([originalRoute]);
      console.log(`🔄 Returned to original route: ${originalRoute}`);
    } catch (error) {
      console.warn(`⚠️ Could not return to original route: ${originalRoute}`);
    }

    // Summary
    const successful = results.filter(r => r.success).length;
    const failed = results.filter(r => !r.success).length;
    
    console.log('\n📊 NAVIGATION TEST SUMMARY');
    console.log('========================');
    console.log(`✅ Successful: ${successful}`);
    console.log(`❌ Failed: ${failed}`);
    console.log(`📋 Total: ${results.length}`);
    
    if (failed > 0) {
      console.log('\n❌ FAILED ROUTES:');
      results.filter(r => !r.success).forEach(r => {
        console.log(`   • ${r.name} (${r.route}) - ${r.error || 'Unknown error'}`);
      });
    }

    console.log('\n🎯 To test a specific route, use: testSingleRoute("/route-path")');
    console.log('🔄 To run this test again, use: testNavigation()');
  };

  console.log('🧪 Navigation test functions loaded!');
  console.log('📋 Available commands:');
  console.log('   • testNavigation() - Test all routes');
  console.log('   • testSingleRoute("/route-path") - Test specific route');
  console.log('');
  console.log('🚀 Run testNavigation() to start testing all routes');
}

// Auto-setup when this file is loaded
if (typeof window !== 'undefined') {
  setupConsoleNavigationTest();
}