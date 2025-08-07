// Component Import Test - Check if all lazy-loaded components can be imported
export async function testComponentImports(): Promise<{ success: boolean; errors: string[] }> {
  const errors: string[] = [];
  
  const componentTests = [
    {
      name: 'EmployeeListComponent',
      import: () => import('../../../features/employees/employee-list/employee-list.component').then(m => m.EmployeeListComponent)
    },
    {
      name: 'EmployeeCreateComponent', 
      import: () => import('../../../features/employees/employee-create/employee-create.component').then(m => m.EmployeeCreateComponent)
    },
    {
      name: 'OrgChartComponent',
      import: () => import('../../../features/employees/org-chart/org-chart.component').then(m => m.OrgChartComponent)
    },
    {
      name: 'AttendanceTrackerComponent',
      import: () => import('../../../features/attendance/attendance-tracker/attendance-tracker.component').then(m => m.AttendanceTrackerComponent)
    },
    {
      name: 'AttendanceNowComponent',
      import: () => import('../../../features/attendance/attendance-now/attendance-now.component').then(m => m.AttendanceNowComponent)
    },
    {
      name: 'ProjectListComponent',
      import: () => import('../../../features/projects/project-list/project-list.component').then(m => m.ProjectListComponent)
    },
    {
      name: 'PayrollListComponent',
      import: () => import('../../../features/payroll/payroll-list/payroll-list.component').then(m => m.PayrollListComponent)
    },
    {
      name: 'LeaveListComponent',
      import: () => import('../../../features/leave/leave-list/leave-list.component').then(m => m.LeaveListComponent)
    },
    {
      name: 'PerformanceListComponent',
      import: () => import('../../../features/performance/performance-list/performance-list.component').then(m => m.PerformanceListComponent)
    },
    {
      name: 'ReportListComponent',
      import: () => import('../../../features/reports/report-list/report-list.component').then(m => m.ReportListComponent)
    },
    {
      name: 'SettingsComponent',
      import: () => import('../../../features/settings/settings.component').then(m => m.SettingsComponent)
    },
    {
      name: 'BranchManagementComponent',
      import: () => import('../../../features/settings/branch-management.component').then(m => m.BranchManagementComponent)
    },
    {
      name: 'ProfileComponent',
      import: () => import('../../../features/profile/profile.component').then(m => m.ProfileComponent)
    }
  ];

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