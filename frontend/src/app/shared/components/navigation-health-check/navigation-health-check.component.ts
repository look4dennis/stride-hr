import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-navigation-health-check',
  imports: [CommonModule],
  template: `
    <div class="navigation-health-check" *ngIf="showHealthCheck">
      <div class="alert" [class.alert-success]="navigationHealthy" [class.alert-warning]="!navigationHealthy">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <i [class]="navigationHealthy ? 'fas fa-check-circle text-success' : 'fas fa-exclamation-triangle text-warning'"></i>
            <strong class="ms-2">Navigation Status:</strong>
            <span class="ms-1">{{ navigationHealthy ? 'All menu items working' : 'Some issues detected' }}</span>
          </div>
          <div class="btn-group">
            <button class="btn btn-sm btn-outline-secondary" (click)="runHealthCheck()">
              <i class="fas fa-sync-alt me-1"></i>
              Recheck
            </button>
            <button class="btn btn-sm btn-outline-primary" (click)="testDashboardSpecifically()">
              <i class="fas fa-tachometer-alt me-1"></i>
              Test Dashboard
            </button>
          </div>
        </div>
        
        <div *ngIf="healthCheckResults.length > 0" class="mt-2">
          <small>
            <div *ngFor="let result of healthCheckResults" 
                 [class.text-success]="result.success" 
                 [class.text-danger]="!result.success">
              {{ result.success ? '✅' : '❌' }} {{ result.item }} - {{ result.message }}
            </div>
          </small>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .navigation-health-check {
      position: fixed;
      top: 70px;
      right: 20px;
      z-index: 1000;
      max-width: 400px;
      animation: slideIn 0.3s ease-out;
    }

    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    .alert {
      margin-bottom: 0;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    }

    small div {
      margin-bottom: 0.25rem;
    }
  `]
})
export class NavigationHealthCheckComponent implements OnInit {
  showHealthCheck = false;
  navigationHealthy = true;
  healthCheckResults: Array<{item: string, success: boolean, message: string}> = [];

  private menuItems = [
    { label: 'Dashboard', route: '/dashboard' },
    { label: 'Employees', route: '/employees' },
    { label: 'Attendance', route: '/attendance' },
    { label: 'Projects', route: '/projects' },
    { label: 'Settings', route: '/settings' }
  ];

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Run health check after user is authenticated
    this.authService.currentUser$.subscribe(user => {
      if (user && this.authService.isAuthenticated) {
        setTimeout(() => {
          this.runHealthCheck();
        }, 3000); // Wait 3 seconds after login to ensure everything is loaded
      }
    });

    // Listen for navigation events to detect errors
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      console.log('Navigation successful to:', event.url);
    });
  }

  async runHealthCheck(): Promise<void> {
    this.healthCheckResults = [];
    this.showHealthCheck = true;

    let allHealthy = true;

    for (const item of this.menuItems) {
      try {
        // Test if the route can be navigated to
        const canNavigate = await this.testRoute(item.route);
        
        this.healthCheckResults.push({
          item: item.label,
          success: canNavigate,
          message: canNavigate ? 'Route accessible' : 'Route navigation failed'
        });

        if (!canNavigate) {
          allHealthy = false;
        }
      } catch (error: any) {
        this.healthCheckResults.push({
          item: item.label,
          success: false,
          message: `Error: ${error.message}`
        });
        allHealthy = false;
      }
    }

    this.navigationHealthy = allHealthy;

    // Auto-hide after 10 seconds if everything is healthy
    if (allHealthy) {
      setTimeout(() => {
        this.showHealthCheck = false;
      }, 10000);
    }
  }

  async testDashboardSpecifically(): Promise<void> {
    console.log('🧪 Testing Dashboard route specifically...');
    
    if (!this.authService.isAuthenticated) {
      console.error('❌ User not authenticated');
      alert('❌ User not authenticated. Please log in first.');
      return;
    }

    const currentRoute = this.router.url;
    
    try {
      console.log('🔍 Current route:', currentRoute);
      console.log('🔍 User authenticated:', this.authService.isAuthenticated);
      console.log('🔍 Current user:', this.authService.currentUser);
      
      // First test component import
      console.log('🧪 Testing dashboard component import...');
      try {
        const dashboardModule = await import('../../../features/dashboard/dashboard.component');
        if (dashboardModule.DashboardComponent) {
          console.log('✅ Dashboard component imported successfully');
        } else {
          console.error('❌ Dashboard component not found in module');
          alert('❌ Dashboard component not found in module');
          return;
        }
      } catch (importError: any) {
        console.error('❌ Dashboard component import failed:', importError);
        alert(`❌ Dashboard component import failed: ${importError.message}`);
        return;
      }
      
      // Test navigation
      console.log('🧪 Testing dashboard navigation...');
      const result = await this.router.navigate(['/dashboard']);
      console.log('🔍 Navigation result:', result);
      
      if (result) {
        // Wait for component to load
        await new Promise(resolve => setTimeout(resolve, 1000));
        
        const actualRoute = this.router.url;
        console.log('🔍 Actual route after navigation:', actualRoute);
        
        if (actualRoute === '/dashboard') {
          console.log('✅ Dashboard navigation successful!');
          alert('✅ Dashboard navigation successful!');
        } else {
          console.log(`❌ Expected /dashboard but got ${actualRoute}`);
          alert(`❌ Expected /dashboard but got ${actualRoute}`);
        }
        
        // Navigate back
        if (currentRoute !== '/dashboard') {
          await this.router.navigate([currentRoute]);
        }
      } else {
        console.log('❌ Dashboard navigation returned false - this might be due to redirect configuration');
        
        // Check if we're already on dashboard due to redirect
        if (this.router.url === '/dashboard') {
          console.log('✅ Actually, we are on dashboard! Navigation worked via redirect.');
          alert('✅ Dashboard is accessible! (Navigation worked via redirect)');
        } else {
          console.log('❌ Dashboard navigation genuinely failed');
          alert('❌ Dashboard navigation failed. Check console for details.');
        }
      }
    } catch (error: any) {
      console.error('❌ Dashboard navigation error:', error);
      alert(`❌ Dashboard navigation error: ${error.message}`);
    }
  }

  private async testRoute(route: string): Promise<boolean> {
    try {
      // Store current route
      const currentRoute = this.router.url;
      
      console.log(`Testing route: ${route}`);
      
      // Try to navigate
      const result = await this.router.navigate([route]);
      
      // Wait a bit for the component to load and any redirects to complete
      await new Promise(resolve => setTimeout(resolve, 800));
      
      // Check if we actually navigated to the route or a valid redirect
      const actualRoute = this.router.url;
      
      // For dashboard, accept both /dashboard and / (root) as valid
      const isValidRoute = actualRoute === route || 
                          actualRoute.startsWith(route) ||
                          (route === '/dashboard' && (actualRoute === '/' || actualRoute === '/dashboard'));
      
      console.log(`Route test: ${route} -> ${actualRoute}, valid: ${isValidRoute}`);
      
      if (isValidRoute) {
        console.log(`✅ Route ${route} loaded successfully (actual: ${actualRoute})`);
        
        // Navigate back to original route if different
        if (currentRoute !== actualRoute && currentRoute !== route) {
          await this.router.navigate([currentRoute]);
        }
        
        return true;
      } else {
        console.log(`❌ Route ${route} navigation failed - ended up at ${actualRoute}`);
        return false;
      }
    } catch (error: any) {
      console.error(`❌ Route test failed for ${route}:`, error);
      return false;
    }
  }
}