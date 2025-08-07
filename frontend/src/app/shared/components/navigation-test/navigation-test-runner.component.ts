import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { testComponentImports } from './component-import-test';

interface NavigationTest {
  label: string;
  route: string;
  status: 'pending' | 'success' | 'error';
  error?: string;
  requiredRoles?: string[];
}

@Component({
  selector: 'app-navigation-test-runner',
  imports: [CommonModule],
  template: `
    <div class="navigation-test-runner">
      <div class="card">
        <div class="card-header">
          <h5 class="mb-0">
            <i class="fas fa-route me-2"></i>
            Navigation Menu Test Runner
          </h5>
        </div>
        <div class="card-body">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <p class="mb-0">Testing all navigation menu items for errors</p>
            <div class="btn-group">
              <button 
                class="btn btn-outline-secondary" 
                (click)="testComponentImports()" 
                [disabled]="testing">
                <i class="fas fa-puzzle-piece me-1"></i>
                Test Imports
              </button>
              <button 
                class="btn btn-primary" 
                (click)="runNavigationTests()" 
                [disabled]="testing">
                <span *ngIf="testing" class="spinner-border spinner-border-sm me-2"></span>
                {{ testing ? 'Testing...' : 'Run Tests' }}
              </button>
            </div>
          </div>

          <div class="alert alert-info" *ngIf="importTestResults">
            <h6 class="mb-2">Component Import Test Results</h6>
            <div *ngIf="importTestResults.success" class="text-success">
              ✅ All components can be imported successfully
            </div>
            <div *ngIf="!importTestResults.success">
              <div class="text-danger mb-2">❌ Some components failed to import:</div>
              <ul class="mb-0">
                <li *ngFor="let error of importTestResults.errors" class="small">{{ error }}</li>
              </ul>
            </div>
          </div>

          <div class="test-results" *ngIf="tests.length > 0">
            <div 
              *ngFor="let test of tests" 
              class="test-item"
              [class.success]="test.status === 'success'"
              [class.error]="test.status === 'error'"
              [class.pending]="test.status === 'pending'">
              
              <div class="d-flex justify-content-between align-items-center">
                <div>
                  <strong>{{ test.label }}</strong>
                  <div class="text-muted small">{{ test.route }}</div>
                  <div *ngIf="test.requiredRoles" class="text-muted small">
                    Roles: {{ test.requiredRoles.join(', ') }}
                  </div>
                </div>
                <div class="status-icon">
                  <i *ngIf="test.status === 'pending'" class="fas fa-clock text-muted"></i>
                  <i *ngIf="test.status === 'success'" class="fas fa-check-circle text-success"></i>
                  <i *ngIf="test.status === 'error'" class="fas fa-times-circle text-danger"></i>
                </div>
              </div>
              
              <div *ngIf="test.error" class="error-message mt-2">
                <small class="text-danger">{{ test.error }}</small>
              </div>
            </div>
          </div>

          <div class="test-summary mt-4" *ngIf="testCompleted">
            <div class="alert" [class.alert-success]="allTestsPassed" [class.alert-danger]="!allTestsPassed">
              <h6 class="mb-2">Test Results Summary</h6>
              <div class="row">
                <div class="col-4">
                  <strong>{{ getTestCount('success') }}</strong> Passed
                </div>
                <div class="col-4">
                  <strong>{{ getTestCount('error') }}</strong> Failed
                </div>
                <div class="col-4">
                  <strong>{{ tests.length }}</strong> Total
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .navigation-test-runner {
      padding: 1rem;
    }

    .test-item {
      padding: 1rem;
      border: 1px solid #e9ecef;
      border-radius: 8px;
      margin-bottom: 0.5rem;
      transition: all 0.2s ease;
    }

    .test-item.success {
      border-color: #28a745;
      background-color: #f8fff9;
    }

    .test-item.error {
      border-color: #dc3545;
      background-color: #fff8f8;
    }

    .test-item.pending {
      border-color: #6c757d;
      background-color: #f8f9fa;
    }

    .error-message {
      padding: 0.5rem;
      background-color: #f8d7da;
      border: 1px solid #f5c6cb;
      border-radius: 4px;
    }

    .status-icon {
      font-size: 1.2rem;
    }
  `]
})
export class NavigationTestRunnerComponent implements OnInit {
  testing = false;
  testCompleted = false;
  tests: NavigationTest[] = [];
  importTestResults: { success: boolean; errors: string[] } | null = null;

  // Define all navigation menu items from the sidebar
  private navigationItems: NavigationTest[] = [
    { label: 'Dashboard', route: '/dashboard', status: 'pending' },
    { label: 'Employees', route: '/employees', status: 'pending', requiredRoles: ['HR', 'Admin', 'Manager'] },
    { label: 'Employee List', route: '/employees', status: 'pending', requiredRoles: ['HR', 'Admin', 'Manager'] },
    { label: 'Org Chart', route: '/employees/org-chart', status: 'pending', requiredRoles: ['HR', 'Admin', 'Manager'] },
    { label: 'Attendance', route: '/attendance', status: 'pending' },
    { label: 'Projects', route: '/projects', status: 'pending' },
    { label: 'Payroll', route: '/payroll', status: 'pending', requiredRoles: ['HR', 'Admin', 'Finance'] },
    { label: 'Leave Management', route: '/leave', status: 'pending' },
    { label: 'Performance', route: '/performance', status: 'pending', requiredRoles: ['HR', 'Admin', 'Manager'] },
    { label: 'Reports', route: '/reports', status: 'pending', requiredRoles: ['HR', 'Admin', 'Manager'] },
    { label: 'Settings', route: '/settings', status: 'pending', requiredRoles: ['Admin', 'SuperAdmin'] },
    { label: 'Navigation Test', route: '/navigation-test', status: 'pending', requiredRoles: ['Admin', 'SuperAdmin'] }
  ];

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.tests = [...this.navigationItems];
  }

  async runNavigationTests(): Promise<void> {
    this.testing = true;
    this.testCompleted = false;

    // Reset all tests
    this.tests.forEach(test => {
      test.status = 'pending';
      test.error = undefined;
    });

    // Test each navigation item
    for (const test of this.tests) {
      await this.testNavigationItem(test);
      // Small delay between tests
      await new Promise(resolve => setTimeout(resolve, 200));
    }

    this.testing = false;
    this.testCompleted = true;
  }

  private async testNavigationItem(test: NavigationTest): Promise<void> {
    try {
      // Check role access first
      if (test.requiredRoles && test.requiredRoles.length > 0) {
        if (!this.authService.hasAnyRole(test.requiredRoles)) {
          test.status = 'error';
          test.error = 'Insufficient permissions for current user role';
          return;
        }
      }

      // Test navigation
      const canNavigate = await this.router.navigate([test.route]);
      
      if (canNavigate) {
        test.status = 'success';
        test.error = undefined;
      } else {
        test.status = 'error';
        test.error = 'Navigation returned false';
      }
    } catch (error: any) {
      test.status = 'error';
      test.error = error.message || 'Navigation failed with exception';
      console.error(`Navigation test failed for ${test.route}:`, error);
    }
  }

  getTestCount(status: 'success' | 'error' | 'pending'): number {
    return this.tests.filter(test => test.status === status).length;
  }

  get allTestsPassed(): boolean {
    return this.tests.every(test => test.status === 'success');
  }

  async testComponentImports(): Promise<void> {
    this.testing = true;
    try {
      this.importTestResults = await testComponentImports();
    } catch (error) {
      console.error('Component import test failed:', error);
      this.importTestResults = {
        success: false,
        errors: ['Failed to run component import test']
      };
    } finally {
      this.testing = false;
    }
  }
}