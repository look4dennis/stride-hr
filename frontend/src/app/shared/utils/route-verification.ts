// Route Verification Utility
// Verify that all routes are properly configured

export interface RouteVerificationResult {
  route: string;
  componentExists: boolean;
  importWorks: boolean;
  error?: string;
}

export async function verifyRoutes(): Promise<RouteVerificationResult[]> {
  const routesToTest = [
    { route: '/dashboard', import: () => import('../../features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
    { route: '/employees', import: () => import('../../features/employees/employee-list/employee-list.component').then(m => m.EmployeeListComponent) },
    { route: '/employees/add', import: () => import('../../features/employees/employee-create/employee-create.component').then(m => m.EmployeeCreateComponent) },
    { route: '/employees/org-chart', import: () => import('../../features/employees/org-chart/org-chart.component').then(m => m.OrgChartComponent) },
    { route: '/attendance', import: () => import('../../features/attendance/attendance-tracker/attendance-tracker.component').then(m => m.AttendanceTrackerComponent) },
    { route: '/attendance/now', import: () => import('../../features/attendance/attendance-now/attendance-now.component').then(m => m.AttendanceNowComponent) },
    { route: '/projects', import: () => import('../../features/projects/project-list/project-list.component').then(m => m.ProjectListComponent) },
    { route: '/projects/kanban', import: () => import('../../features/projects/kanban-board/kanban-board.component').then(m => m.KanbanBoardComponent) },
    { route: '/payroll', import: () => import('../../features/payroll/payroll-list/payroll-list.component').then(m => m.PayrollListComponent) },
    { route: '/leave', import: () => import('../../features/leave/leave-list/leave-list.component').then(m => m.LeaveListComponent) },
    { route: '/performance', import: () => import('../../features/performance/performance-list/performance-list.component').then(m => m.PerformanceListComponent) },
    { route: '/reports', import: () => import('../../features/reports/report-list/report-list.component').then(m => m.ReportListComponent) },
    { route: '/settings', import: () => import('../../features/settings/settings.component').then(m => m.SettingsComponent) },
    { route: '/settings/branches', import: () => import('../../features/settings/branch-management.component').then(m => m.BranchManagementComponent) },
    { route: '/profile', import: () => import('../../features/profile/profile.component').then(m => m.ProfileComponent) }
  ];

  const results: RouteVerificationResult[] = [];

  console.log('🔍 Starting route verification...');

  for (const routeTest of routesToTest) {
    const result: RouteVerificationResult = {
      route: routeTest.route,
      componentExists: false,
      importWorks: false
    };

    try {
      console.log(`🧪 Testing ${routeTest.route}...`);
      
      const component = await routeTest.import();
      
      if (component) {
        result.componentExists = true;
        result.importWorks = true;
        console.log(`✅ ${routeTest.route} - Component exists and imports successfully`);
      } else {
        result.componentExists = false;
        result.importWorks = false;
        result.error = 'Component import returned undefined';
        console.log(`❌ ${routeTest.route} - Component import returned undefined`);
      }
    } catch (error: any) {
      result.componentExists = false;
      result.importWorks = false;
      result.error = error.message;
      console.log(`❌ ${routeTest.route} - Import failed: ${error.message}`);
    }

    results.push(result);
  }

  console.log('📊 Route verification completed');
  return results;
}

// Make it available globally
declare global {
  interface Window {
    verifyRoutes: () => Promise<void>;
  }
}

if (typeof window !== 'undefined') {
  window.verifyRoutes = async () => {
    console.log('🚀 Starting route verification...');
    const results = await verifyRoutes();
    
    const successful = results.filter(r => r.importWorks).length;
    const failed = results.filter(r => !r.importWorks).length;
    
    console.log('\n📊 ROUTE VERIFICATION SUMMARY');
    console.log('============================');
    console.log(`✅ Working: ${successful}`);
    console.log(`❌ Failed: ${failed}`);
    console.log(`📋 Total: ${results.length}`);
    
    if (failed > 0) {
      console.log('\n❌ FAILED ROUTES:');
      results.filter(r => !r.importWorks).forEach(r => {
        console.log(`   • ${r.route} - ${r.error || 'Unknown error'}`);
      });
    }
    
    alert(`Route verification completed: ${successful}/${results.length} routes working`);
  };
  
  console.log('🔍 Route verification loaded! Use verifyRoutes() to test all route imports.');
}