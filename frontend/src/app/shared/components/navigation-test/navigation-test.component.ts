import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { NavigationTestRunnerComponent } from './navigation-test-runner.component';
import { NavigationTestService, NavigationTestResult } from '../../services/navigation-test.service';

interface RouteTest {
  name: string;
  route: string;
  requiredRoles?: string[];
  status: 'pending' | 'success' | 'error';
  error?: string;
}

@Component({
  selector: 'app-navigation-test',
  imports: [CommonModule, NavigationTestRunnerComponent],
  template: `
    <div class="navigation-test-container">
      <!-- Quick Navigation Test Runner -->
      <app-navigation-test-runner></app-navigation-test-runner>
      
      <!-- Detailed Route Testing -->
      <div class="card mt-4">
        <div class="card-header">
          <h5 class="card-title mb-0">
            <i class="fas fa-route me-2"></i>
            Detailed Navigation & Routing Test
          </h5>
        </div>
        <div class="card-body">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <p class="mb-0">Test all navigation routes to ensure they load correctly</p>
            <div class="btn-group">
              <button class="btn btn-primary" (click)="testAllRoutes()" [disabled]="testing">
                <span *ngIf="testing" class="spinner-border spinner-border-sm me-2"></span>
                <i *ngIf="!testing" class="fas fa-play me-2"></i>
                {{ testing ? 'Testing...' : 'Test All Routes' }}
              </button>
              <button class="btn btn-outline-primary" (click)="testComponentImports()" [disabled]="testing">
                <i class="fas fa-puzzle-piece me-2"></i>
                Test Imports
              </button>
              <button class="btn btn-outline-secondary" (click)="runComprehensiveTest()" [disabled]="testing">
                <i class="fas fa-cogs me-2"></i>
                Full Test
              </button>
            </div>
          </div>

          <div class="route-tests">
            <div 
              *ngFor="let test of routeTests" 
              class="route-test-item"
              [class.success]="test.status === 'success'"
              [class.error]="test.status === 'error'"
              [class.pending]="test.status === 'pending'">
              
              <div class="d-flex justify-content-between align-items-center">
                <div class="route-info">
                  <div class="route-name">{{ test.name }}</div>
                  <div class="route-path">{{ test.route }}</div>
                  <div class="route-roles" *ngIf="test.requiredRoles && test.requiredRoles.length > 0">
                    <small class="text-muted">
                      Roles: {{ test.requiredRoles.join(', ') }}
                    </small>
                  </div>
                </div>
                
                <div class="route-status">
                  <i *ngIf="test.status === 'pending'" class="fas fa-clock text-muted"></i>
                  <i *ngIf="test.status === 'success'" class="fas fa-check-circle text-success"></i>
                  <i *ngIf="test.status === 'error'" class="fas fa-times-circle text-danger"></i>
                  
                  <button 
                    class="btn btn-sm btn-outline-primary ms-2" 
                    (click)="testRoute(test)"
                    [disabled]="testing">
                    Test
                  </button>
                  
                  <button 
                    class="btn btn-sm btn-outline-secondary ms-1" 
                    (click)="navigateToRoute(test.route)"
                    [disabled]="testing">
                    Go
                  </button>
                </div>
              </div>
              
              <div *ngIf="test.error" class="route-error mt-2">
                <small class="text-danger">{{ test.error }}</small>
              </div>
            </div>
          </div>

          <div class="test-summary mt-4" *ngIf="testCompleted">
            <div class="row">
              <div class="col-md-4">
                <div class="summary-card success">
                  <div class="summary-number">{{ getTestCount('success') }}</div>
                  <div class="summary-label">Passed</div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="summary-card error">
                  <div class="summary-number">{{ getTestCount('error') }}</div>
                  <div class="summary-label">Failed</div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="summary-card total">
                  <div class="summary-number">{{ routeTests.length }}</div>
                  <div class="summary-label">Total</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .navigation-test-container {
      padding: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    .route-test-item {
      padding: 1rem;
      border: 1px solid #e9ecef;
      border-radius: 8px;
      margin-bottom: 0.5rem;
      transition: all 0.2s ease;
    }

    .route-test-item.success {
      border-color: #28a745;
      background-color: #f8fff9;
    }

    .route-test-item.error {
      border-color: #dc3545;
      background-color: #fff8f8;
    }

    .route-test-item.pending {
      border-color: #6c757d;
      background-color: #f8f9fa;
    }

    .route-name {
      font-weight: 600;
      color: #495057;
    }

    .route-path {
      font-family: 'Courier New', monospace;
      color: #6c757d;
      font-size: 0.9rem;
    }

    .route-roles {
      margin-top: 0.25rem;
    }

    .route-error {
      padding: 0.5rem;
      background-color: #f8d7da;
      border: 1px solid #f5c6cb;
      border-radius: 4px;
    }

    .test-summary {
      border-top: 1px solid #e9ecef;
      padding-top: 1.5rem;
    }

    .summary-card {
      text-align: center;
      padding: 1rem;
      border-radius: 8px;
      margin-bottom: 1rem;
    }

    .summary-card.success {
      background-color: #d4edda;
      border: 1px solid #c3e6cb;
    }

    .summary-card.error {
      background-color: #f8d7da;
      border: 1px solid #f5c6cb;
    }

    .summary-card.total {
      background-color: #d1ecf1;
      border: 1px solid #bee5eb;
    }

    .summary-number {
      font-size: 2rem;
      font-weight: 700;
      color: #495057;
    }

    .summary-label {
      font-weight: 500;
      color: #6c757d;
    }

    .btn-sm {
      font-size: 0.8rem;
      padding: 0.25rem 0.5rem;
    }
  `]
})
export class NavigationTestComponent {
  testing = false;
  testCompleted = false;

  routeTests: RouteTest[] = [
    { name: 'Dashboard', route: '/dashboard', status: 'pending' },
    { name: 'Employee List', route: '/employees', status: 'pending', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Create Employee', route: '/employees/add', status: 'pending', requiredRoles: ['HR', 'Admin', 'SuperAdmin'] },
    { name: 'Organization Chart', route: '/employees/org-chart', status: 'pending', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Attendance Tracker', route: '/attendance', status: 'pending' },
    { name: 'Attendance Now', route: '/attendance/now', status: 'pending', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Attendance Calendar', route: '/attendance/calendar', status: 'pending', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Projects', route: '/projects', status: 'pending' },
    { name: 'Kanban Board', route: '/projects/kanban', status: 'pending' },
    { name: 'Payroll', route: '/payroll', status: 'pending', requiredRoles: ['HR', 'Admin', 'Finance', 'SuperAdmin'] },
    { name: 'Leave Management', route: '/leave', status: 'pending' },
    { name: 'Leave Request', route: '/leave/request', status: 'pending' },
    { name: 'Performance', route: '/performance', status: 'pending', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Reports', route: '/reports', status: 'pending', requiredRoles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] },
    { name: 'Settings', route: '/settings', status: 'pending', requiredRoles: ['Admin', 'SuperAdmin'] },
    { name: 'Branch Management', route: '/settings/branches', status: 'pending', requiredRoles: ['Admin', 'SuperAdmin'] },
    { name: 'Role Management', route: '/settings/roles', status: 'pending', requiredRoles: ['Admin', 'SuperAdmin'] },
    { name: 'Profile', route: '/profile', status: 'pending' }
  ];

  constructor(
    private router: Router,
    private authService: AuthService,
    private navigationTestService: NavigationTestService
  ) {}

  async testAllRoutes(): Promise<void> {
    this.testing = true;
    this.testCompleted = false;

    // Reset all tests
    this.routeTests.forEach(test => {
      test.status = 'pending';
      test.error = undefined;
    });

    // Test each route
    for (const test of this.routeTests) {
      await this.testRoute(test);
      // Small delay between tests
      await new Promise(resolve => setTimeout(resolve, 100));
    }

    this.testing = false;
    this.testCompleted = true;
  }

  async testRoute(test: RouteTest): Promise<void> {
    try {
      // Check role access first
      if (test.requiredRoles && test.requiredRoles.length > 0) {
        if (!this.authService.hasAnyRole(test.requiredRoles)) {
          test.status = 'error';
          test.error = 'Insufficient permissions';
          return;
        }
      }

      // Attempt to navigate to the route
      const navigationResult = await this.router.navigate([test.route]);
      
      if (navigationResult) {
        test.status = 'success';
        test.error = undefined;
      } else {
        test.status = 'error';
        test.error = 'Navigation failed';
      }
    } catch (error: any) {
      test.status = 'error';
      test.error = error.message || 'Unknown error occurred';
    }
  }

  navigateToRoute(route: string): void {
    this.router.navigate([route]);
  }

  getTestCount(status: 'success' | 'error' | 'pending'): number {
    return this.routeTests.filter(test => test.status === status).length;
  }

  async testComponentImports(): Promise<void> {
    this.testing = true;
    console.log('🔍 Starting component import test...');
    
    try {
      const result = await this.navigationTestService.testComponentImports();
      
      if (result.success) {
        console.log('✅ All components imported successfully!');
        alert('✅ All components imported successfully!');
      } else {
        console.error('❌ Some components failed to import:', result.errors);
        alert(`❌ ${result.errors.length} components failed to import. Check console for details.`);
      }
    } catch (error) {
      console.error('Error during component import test:', error);
      alert('❌ Component import test failed. Check console for details.');
    } finally {
      this.testing = false;
    }
  }

  async runComprehensiveTest(): Promise<void> {
    this.testing = true;
    this.testCompleted = false;
    
    console.log('🚀 Starting comprehensive navigation test...');
    
    try {
      // First test component imports
      console.log('Phase 1: Testing component imports...');
      const importResult = await this.navigationTestService.testComponentImports();
      
      if (!importResult.success) {
        console.warn('⚠️ Some components failed to import, but continuing with navigation test...');
      }
      
      // Then test navigation
      console.log('Phase 2: Testing navigation routes...');
      const navigationResults = await this.navigationTestService.testAllRoutes();
      
      // Update our local route tests with results
      this.routeTests.forEach(localTest => {
        const result = navigationResults.find(r => r.route === localTest.route);
        if (result) {
          localTest.status = result.success ? 'success' : 'error';
          localTest.error = result.error;
        }
      });
      
      this.testCompleted = true;
      
      const successCount = navigationResults.filter(r => r.success).length;
      const totalCount = navigationResults.length;
      
      console.log(`✅ Comprehensive test completed: ${successCount}/${totalCount} routes working`);
      alert(`Test completed: ${successCount}/${totalCount} routes working. Check console for details.`);
      
    } catch (error) {
      console.error('Error during comprehensive test:', error);
      alert('❌ Comprehensive test failed. Check console for details.');
    } finally {
      this.testing = false;
    }
  }
}