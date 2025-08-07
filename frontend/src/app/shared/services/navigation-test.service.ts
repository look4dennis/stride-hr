import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

export interface NavigationTestResult {
  route: string;
  name: string;
  success: boolean;
  error?: string;
  requiredRoles?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class NavigationTestService {
  private testRoutes = [
    { name: 'Dashboard', route: '/dashboard' },
    { name: 'Employee List', route: '/employees', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Create Employee', route: '/employees/add', requiredRoles: ['HR', 'Admin', 'SuperAdmin'] },
    { name: 'Organization Chart', route: '/employees/org-chart', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Attendance Tracker', route: '/attendance' },
    { name: 'Attendance Now', route: '/attendance/now', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Attendance Calendar', route: '/attendance/calendar', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Attendance Reports', route: '/attendance/reports', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Attendance Corrections', route: '/attendance/corrections', requiredRoles: ['HR', 'Admin', 'SuperAdmin'] },
    { name: 'Projects', route: '/projects' },
    { name: 'Kanban Board', route: '/projects/kanban' },
    { name: 'Project Monitoring', route: '/projects/monitoring', requiredRoles: ['Manager', 'Admin', 'SuperAdmin'] },
    { name: 'Payroll', route: '/payroll', requiredRoles: ['HR', 'Admin', 'Finance', 'SuperAdmin'] },
    { name: 'Payroll Processing', route: '/payroll/processing', requiredRoles: ['HR', 'Admin', 'Finance', 'SuperAdmin'] },
    { name: 'Payroll Approval', route: '/payroll/approval', requiredRoles: ['Admin', 'Finance', 'SuperAdmin'] },
    { name: 'Payroll Reports', route: '/payroll/reports', requiredRoles: ['HR', 'Admin', 'Finance', 'SuperAdmin'] },
    { name: 'Leave Management', route: '/leave' },
    { name: 'Leave Request', route: '/leave/request' },
    { name: 'Leave Balance', route: '/leave/balance' },
    { name: 'Leave Calendar', route: '/leave/calendar' },
    { name: 'Performance', route: '/performance', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Performance Review', route: '/performance/review', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'PIP Management', route: '/performance/pip', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Certifications', route: '/performance/certifications' },
    { name: 'Reports', route: '/reports', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Report Builder', route: '/reports/builder', requiredRoles: ['HR', 'Admin', 'SuperAdmin'] },
    { name: 'Analytics Dashboard', route: '/reports/analytics', requiredRoles: ['Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Settings', route: '/settings', requiredRoles: ['Admin', 'SuperAdmin'] },
    { name: 'Organization Settings', route: '/settings/organization', requiredRoles: ['Admin', 'SuperAdmin'] },
    { name: 'Branch Management', route: '/settings/branches', requiredRoles: ['Admin', 'SuperAdmin'] },
    { name: 'Role Management', route: '/settings/roles', requiredRoles: ['Admin', 'SuperAdmin'] },
    { name: 'System Config', route: '/settings/system', requiredRoles: ['SuperAdmin'] },
    { name: 'Admin Settings', route: '/settings/admin', requiredRoles: ['SuperAdmin'] },
    { name: 'Training', route: '/training' },
    { name: 'Profile', route: '/profile' }
  ];

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  async testAllRoutes(): Promise<NavigationTestResult[]> {
    const results: NavigationTestResult[] = [];
    const originalRoute = this.router.url;

    console.log('🚀 Starting comprehensive navigation test...');

    for (const testRoute of this.testRoutes) {
      const result = await this.testSingleRoute(testRoute);
      results.push(result);
      
      // Small delay between tests
      await new Promise(resolve => setTimeout(resolve, 50));
    }

    // Return to original route
    try {
      await this.router.navigate([originalRoute]);
    } catch (error) {
      console.warn('Could not return to original route:', originalRoute);
    }

    console.log('✅ Navigation test completed');
    return results;
  }

  private async testSingleRoute(testRoute: any): Promise<NavigationTestResult> {
    const result: NavigationTestResult = {
      route: testRoute.route,
      name: testRoute.name,
      success: false,
      requiredRoles: testRoute.requiredRoles
    };

    try {
      // Check role access first
      if (testRoute.requiredRoles && testRoute.requiredRoles.length > 0) {
        if (!this.authService.hasAnyRole(testRoute.requiredRoles)) {
          result.success = false;
          result.error = 'Insufficient permissions - this is expected behavior';
          console.log(`⚠️  ${testRoute.name} (${testRoute.route}) - Access denied (expected)`);
          return result;
        }
      }

      console.log(`🧪 Testing: ${testRoute.name} (${testRoute.route})`);

      // Attempt navigation
      const navigationResult = await this.router.navigate([testRoute.route]);
      
      if (navigationResult) {
        // Wait for component to load
        await new Promise(resolve => setTimeout(resolve, 300));
        
        // Verify we're at the correct route
        const currentUrl = this.router.url;
        if (currentUrl === testRoute.route || currentUrl.startsWith(testRoute.route)) {
          result.success = true;
          console.log(`✅ ${testRoute.name} (${testRoute.route}) - Success`);
        } else {
          result.success = false;
          result.error = `Navigation succeeded but ended up at ${currentUrl}`;
          console.log(`❌ ${testRoute.name} (${testRoute.route}) - Wrong destination: ${currentUrl}`);
        }
      } else {
        // Navigation returned false, but check if we're actually at the route
        // This can happen with redirects
        await new Promise(resolve => setTimeout(resolve, 300));
        const currentUrl = this.router.url;
        
        if (currentUrl === testRoute.route || currentUrl.startsWith(testRoute.route)) {
          result.success = true;
          result.error = 'Navigation returned false but route is accessible (likely due to redirect)';
          console.log(`✅ ${testRoute.name} (${testRoute.route}) - Success (via redirect)`);
        } else {
          result.success = false;
          result.error = 'Navigation returned false';
          console.log(`❌ ${testRoute.name} (${testRoute.route}) - Navigation failed`);
        }
      }
    } catch (error: any) {
      result.success = false;
      result.error = error.message || 'Unknown navigation error';
      console.log(`❌ ${testRoute.name} (${testRoute.route}) - Error: ${result.error}`);
    }

    return result;
  }

  async testComponentImports(): Promise<{ success: boolean; errors: string[] }> {
    const errors: string[] = [];
    
    const componentTests = [
      {
        name: 'EmployeeListComponent',
        import: () => import('../../features/employees/employee-list/employee-list.component').then(m => m.EmployeeListComponent)
      },
      {
        name: 'EmployeeCreateComponent', 
        import: () => import('../../features/employees/employee-create/employee-create.component').then(m => m.EmployeeCreateComponent)
      },
      {
        name: 'OrgChartComponent',
        import: () => import('../../features/employees/org-chart/org-chart.component').then(m => m.OrgChartComponent)
      },
      {
        name: 'AttendanceTrackerComponent',
        import: () => import('../../features/attendance/attendance-tracker/attendance-tracker.component').then(m => m.AttendanceTrackerComponent)
      },
      {
        name: 'AttendanceNowComponent',
        import: () => import('../../features/attendance/attendance-now/attendance-now.component').then(m => m.AttendanceNowComponent)
      },
      {
        name: 'AttendanceCalendarComponent',
        import: () => import('../../features/attendance/attendance-calendar/attendance-calendar.component').then(m => m.AttendanceCalendarComponent)
      },
      {
        name: 'AttendanceReportsComponent',
        import: () => import('../../features/attendance/attendance-reports/attendance-reports.component').then(m => m.AttendanceReportsComponent)
      },
      {
        name: 'AttendanceCorrectionsComponent',
        import: () => import('../../features/attendance/attendance-corrections/attendance-corrections.component').then(m => m.AttendanceCorrectionsComponent)
      },
      {
        name: 'ProjectListComponent',
        import: () => import('../../features/projects/project-list/project-list.component').then(m => m.ProjectListComponent)
      },
      {
        name: 'KanbanBoardComponent',
        import: () => import('../../features/projects/kanban-board/kanban-board.component').then(m => m.KanbanBoardComponent)
      },
      {
        name: 'ProjectMonitoringComponent',
        import: () => import('../../features/projects/project-monitoring/project-monitoring.component').then(m => m.ProjectMonitoringComponent)
      },
      {
        name: 'PayrollListComponent',
        import: () => import('../../features/payroll/payroll-list/payroll-list.component').then(m => m.PayrollListComponent)
      },
      {
        name: 'PayrollProcessingComponent',
        import: () => import('../../features/payroll/payroll-processing/payroll-processing.component').then(m => m.PayrollProcessingComponent)
      },
      {
        name: 'PayrollApprovalComponent',
        import: () => import('../../features/payroll/payroll-approval/payroll-approval.component').then(m => m.PayrollApprovalComponent)
      },
      {
        name: 'PayrollReportsComponent',
        import: () => import('../../features/payroll/payroll-reports/payroll-reports.component').then(m => m.PayrollReportsComponent)
      },
      {
        name: 'LeaveListComponent',
        import: () => import('../../features/leave/leave-list/leave-list.component').then(m => m.LeaveListComponent)
      },
      {
        name: 'LeaveRequestFormComponent',
        import: () => import('../../features/leave/leave-request-form/leave-request-form.component').then(m => m.LeaveRequestFormComponent)
      },
      {
        name: 'LeaveBalanceComponent',
        import: () => import('../../features/leave/leave-balance/leave-balance.component').then(m => m.LeaveBalanceComponent)
      },
      {
        name: 'LeaveCalendarComponent',
        import: () => import('../../features/leave/leave-calendar/leave-calendar.component').then(m => m.LeaveCalendarComponent)
      },
      {
        name: 'PerformanceListComponent',
        import: () => import('../../features/performance/performance-list/performance-list.component').then(m => m.PerformanceListComponent)
      },
      {
        name: 'PerformanceReviewComponent',
        import: () => import('../../features/performance/performance-review/performance-review.component').then(m => m.PerformanceReviewComponent)
      },
      {
        name: 'PIPManagementComponent',
        import: () => import('../../features/performance/pip-management/pip-management.component').then(m => m.PIPManagementComponent)
      },
      {
        name: 'CertificationsComponent',
        import: () => import('../../features/performance/certifications/certifications.component').then(m => m.CertificationsComponent)
      },
      {
        name: 'ReportListComponent',
        import: () => import('../../features/reports/report-list/report-list.component').then(m => m.ReportListComponent)
      },
      {
        name: 'ReportBuilderComponent',
        import: () => import('../../features/reports/report-builder/report-builder.component').then(m => m.ReportBuilderComponent)
      },
      {
        name: 'AnalyticsDashboardComponent',
        import: () => import('../../features/reports/analytics-dashboard/analytics-dashboard.component').then(m => m.AnalyticsDashboardComponent)
      },
      {
        name: 'SettingsComponent',
        import: () => import('../../features/settings/settings.component').then(m => m.SettingsComponent)
      },
      {
        name: 'OrganizationSettingsComponent',
        import: () => import('../../features/settings/organization-settings.component').then(m => m.OrganizationSettingsComponent)
      },
      {
        name: 'BranchManagementComponent',
        import: () => import('../../features/settings/branch-management.component').then(m => m.BranchManagementComponent)
      },
      {
        name: 'RoleManagementComponent',
        import: () => import('../../features/settings/role-management.component').then(m => m.RoleManagementComponent)
      },
      {
        name: 'SystemConfigComponent',
        import: () => import('../../features/settings/system-config.component').then(m => m.SystemConfigComponent)
      },
      {
        name: 'AdminSettingsComponent',
        import: () => import('../../features/settings/admin-settings.component').then(m => m.AdminSettingsComponent)
      },
      {
        name: 'TrainingListComponent',
        import: () => import('../../features/training/training-list/training-list.component').then(m => m.TrainingListComponent)
      },
      {
        name: 'ProfileComponent',
        import: () => import('../../features/profile/profile.component').then(m => m.ProfileComponent)
      }
    ];

    console.log('🔍 Testing component imports...');

    for (const test of componentTests) {
      try {
        await test.import();
        console.log(`✅ ${test.name} imported successfully`);
      } catch (error: any) {
        const errorMsg = `❌ ${test.name} failed to import: ${error.message}`;
        errors.push(errorMsg);
        console.error(errorMsg, error);
      }
    }

    return {
      success: errors.length === 0,
      errors
    };
  }
}