import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { MobileTestingService, DeviceInfo, TouchTestResult, ResponsiveTestResult } from '../../services/mobile-testing.service';
import { PwaService } from '../../services/pwa.service';

@Component({
  selector: 'app-mobile-testing',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="mobile-testing-container">
      <div class="container-fluid py-4">
        <div class="row">
          <div class="col-12">
            <div class="d-flex justify-content-between align-items-center mb-4">
              <h2>
                <i class="fas fa-mobile-alt me-2"></i>
                Mobile & PWA Testing Dashboard
              </h2>
              <div class="btn-group">
                <button 
                  class="btn btn-primary" 
                  (click)="runAllTests()"
                  [disabled]="testing">
                  <i class="fas fa-play me-1" *ngIf="!testing"></i>
                  <i class="fas fa-spinner fa-spin me-1" *ngIf="testing"></i>
                  {{ testing ? 'Running Tests...' : 'Run All Tests' }}
                </button>
                <button 
                  class="btn btn-outline-secondary" 
                  (click)="exportResults()"
                  [disabled]="!hasResults">
                  <i class="fas fa-download me-1"></i>
                  Export Results
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Device Information -->
        <div class="row mb-4">
          <div class="col-12">
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">
                  <i class="fas fa-info-circle me-2"></i>
                  Device Information
                </h5>
              </div>
              <div class="card-body">
                <div class="row" *ngIf="deviceInfo">
                  <div class="col-md-6">
                    <table class="table table-sm">
                      <tr>
                        <td><strong>Device Type:</strong></td>
                        <td>
                          <span class="badge bg-primary" *ngIf="deviceInfo.isMobile">Mobile</span>
                          <span class="badge bg-info" *ngIf="deviceInfo.isTablet">Tablet</span>
                          <span class="badge bg-secondary" *ngIf="deviceInfo.isDesktop">Desktop</span>
                        </td>
                      </tr>
                      <tr>
                        <td><strong>Screen Size:</strong></td>
                        <td>{{ deviceInfo.screenWidth }} × {{ deviceInfo.screenHeight }}</td>
                      </tr>
                      <tr>
                        <td><strong>Orientation:</strong></td>
                        <td>
                          <span class="badge" 
                                [class.bg-success]="deviceInfo.orientation === 'portrait'"
                                [class.bg-warning]="deviceInfo.orientation === 'landscape'">
                            {{ deviceInfo.orientation | titlecase }}
                          </span>
                        </td>
                      </tr>
                    </table>
                  </div>
                  <div class="col-md-6">
                    <table class="table table-sm">
                      <tr>
                        <td><strong>Touch Support:</strong></td>
                        <td>
                          <span class="badge" 
                                [class.bg-success]="deviceInfo.touchSupported"
                                [class.bg-danger]="!deviceInfo.touchSupported">
                            {{ deviceInfo.touchSupported ? 'Yes' : 'No' }}
                          </span>
                        </td>
                      </tr>
                      <tr>
                        <td><strong>Platform:</strong></td>
                        <td>{{ deviceInfo.platform }}</td>
                      </tr>
                      <tr>
                        <td><strong>PWA Mode:</strong></td>
                        <td>
                          <span class="badge" 
                                [class.bg-success]="isStandalone"
                                [class.bg-secondary]="!isStandalone">
                            {{ isStandalone ? 'Standalone' : 'Browser' }}
                          </span>
                        </td>
                      </tr>
                    </table>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Test Categories -->
        <div class="row">
          <!-- Touch Interaction Tests -->
          <div class="col-lg-6 mb-4">
            <div class="card h-100">
              <div class="card-header d-flex justify-content-between align-items-center">
                <h5 class="mb-0">
                  <i class="fas fa-hand-pointer me-2"></i>
                  Touch Interaction Tests
                </h5>
                <button 
                  class="btn btn-sm btn-outline-primary" 
                  (click)="runTouchTests()"
                  [disabled]="testingTouch">
                  <i class="fas fa-play me-1" *ngIf="!testingTouch"></i>
                  <i class="fas fa-spinner fa-spin me-1" *ngIf="testingTouch"></i>
                  Test
                </button>
              </div>
              <div class="card-body">
                <div class="mb-3">
                  <label class="form-label">Test Elements (CSS Selectors):</label>
                  <textarea 
                    class="form-control" 
                    rows="3" 
                    [(ngModel)]="touchTestSelectors"
                    placeholder="Enter CSS selectors, one per line">
                  </textarea>
                </div>
                
                <div *ngIf="touchResults.length > 0">
                  <h6>Results:</h6>
                  <div class="table-responsive">
                    <table class="table table-sm">
                      <thead>
                        <tr>
                          <th>Element</th>
                          <th>Type</th>
                          <th>Status</th>
                          <th>Response Time</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr *ngFor="let result of touchResults">
                          <td><code>{{ result.element }}</code></td>
                          <td>{{ result.touchType }}</td>
                          <td>
                            <span class="badge" 
                                  [class.bg-success]="result.success"
                                  [class.bg-danger]="!result.success">
                              {{ result.success ? 'Pass' : 'Fail' }}
                            </span>
                          </td>
                          <td>{{ result.responseTime | number:'1.0-0' }}ms</td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Responsive Design Tests -->
          <div class="col-lg-6 mb-4">
            <div class="card h-100">
              <div class="card-header d-flex justify-content-between align-items-center">
                <h5 class="mb-0">
                  <i class="fas fa-mobile-alt me-2"></i>
                  Responsive Design Tests
                </h5>
                <button 
                  class="btn btn-sm btn-outline-primary" 
                  (click)="runResponsiveTests()"
                  [disabled]="testingResponsive">
                  <i class="fas fa-play me-1" *ngIf="!testingResponsive"></i>
                  <i class="fas fa-spinner fa-spin me-1" *ngIf="testingResponsive"></i>
                  Test
                </button>
              </div>
              <div class="card-body">
                <div *ngIf="responsiveResults.length > 0">
                  <div class="table-responsive">
                    <table class="table table-sm">
                      <thead>
                        <tr>
                          <th>Breakpoint</th>
                          <th>Dimensions</th>
                          <th>Status</th>
                          <th>Issues</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr *ngFor="let result of responsiveResults">
                          <td>{{ result.breakpoint }}</td>
                          <td>{{ result.width }} × {{ result.height }}</td>
                          <td>
                            <span class="badge" 
                                  [class.bg-success]="result.layoutValid"
                                  [class.bg-danger]="!result.layoutValid">
                              {{ result.layoutValid ? 'Valid' : 'Invalid' }}
                            </span>
                          </td>
                          <td>
                            <span class="badge bg-warning" *ngIf="result.issues.length > 0">
                              {{ result.issues.length }}
                            </span>
                            <span class="text-muted" *ngIf="result.issues.length === 0">None</span>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- PWA Functionality Tests -->
        <div class="row">
          <div class="col-12 mb-4">
            <div class="card">
              <div class="card-header d-flex justify-content-between align-items-center">
                <h5 class="mb-0">
                  <i class="fas fa-cog me-2"></i>
                  PWA Functionality Tests
                </h5>
                <button 
                  class="btn btn-sm btn-outline-primary" 
                  (click)="runPWATests()"
                  [disabled]="testingPWA">
                  <i class="fas fa-play me-1" *ngIf="!testingPWA"></i>
                  <i class="fas fa-spinner fa-spin me-1" *ngIf="testingPWA"></i>
                  Test
                </button>
              </div>
              <div class="card-body">
                <div class="row" *ngIf="pwaResults">
                  <div class="col-md-6">
                    <h6>Service Worker</h6>
                    <ul class="list-group list-group-flush">
                      <li class="list-group-item d-flex justify-content-between align-items-center">
                        Supported
                        <span class="badge" 
                              [class.bg-success]="pwaResults.serviceWorkerRegistration?.supported"
                              [class.bg-danger]="!pwaResults.serviceWorkerRegistration?.supported">
                          {{ pwaResults.serviceWorkerRegistration?.supported ? 'Yes' : 'No' }}
                        </span>
                      </li>
                      <li class="list-group-item d-flex justify-content-between align-items-center">
                        Registered
                        <span class="badge" 
                              [class.bg-success]="pwaResults.serviceWorkerRegistration?.registered"
                              [class.bg-danger]="!pwaResults.serviceWorkerRegistration?.registered">
                          {{ pwaResults.serviceWorkerRegistration?.registered ? 'Yes' : 'No' }}
                        </span>
                      </li>
                      <li class="list-group-item d-flex justify-content-between align-items-center">
                        Active
                        <span class="badge" 
                              [class.bg-success]="pwaResults.serviceWorkerRegistration?.active"
                              [class.bg-danger]="!pwaResults.serviceWorkerRegistration?.active">
                          {{ pwaResults.serviceWorkerRegistration?.active ? 'Yes' : 'No' }}
                        </span>
                      </li>
                    </ul>
                  </div>
                  <div class="col-md-6">
                    <h6>Offline Capability</h6>
                    <ul class="list-group list-group-flush">
                      <li class="list-group-item d-flex justify-content-between align-items-center">
                        Cache Available
                        <span class="badge" 
                              [class.bg-success]="pwaResults.offlineCapability?.cacheAvailable"
                              [class.bg-danger]="!pwaResults.offlineCapability?.cacheAvailable">
                          {{ pwaResults.offlineCapability?.cacheAvailable ? 'Yes' : 'No' }}
                        </span>
                      </li>
                      <li class="list-group-item d-flex justify-content-between align-items-center">
                        Cached Resources
                        <span class="badge bg-info">
                          {{ pwaResults.offlineCapability?.cachedResources || 0 }}
                        </span>
                      </li>
                      <li class="list-group-item d-flex justify-content-between align-items-center">
                        Offline Ready
                        <span class="badge" 
                              [class.bg-success]="pwaResults.offlineCapability?.offlineReady"
                              [class.bg-danger]="!pwaResults.offlineCapability?.offlineReady">
                          {{ pwaResults.offlineCapability?.offlineReady ? 'Yes' : 'No' }}
                        </span>
                      </li>
                    </ul>
                  </div>
                </div>

                <div class="row mt-3" *ngIf="pwaResults">
                  <div class="col-md-6">
                    <h6>Install Prompt</h6>
                    <ul class="list-group list-group-flush">
                      <li class="list-group-item d-flex justify-content-between align-items-center">
                        Supported
                        <span class="badge" 
                              [class.bg-success]="pwaResults.installPrompt?.supported"
                              [class.bg-danger]="!pwaResults.installPrompt?.supported">
                          {{ pwaResults.installPrompt?.supported ? 'Yes' : 'No' }}
                        </span>
                      </li>
                      <li class="list-group-item d-flex justify-content-between align-items-center">
                        Installed
                        <span class="badge" 
                              [class.bg-success]="pwaResults.installPrompt?.installed"
                              [class.bg-secondary]="!pwaResults.installPrompt?.installed">
                          {{ pwaResults.installPrompt?.installed ? 'Yes' : 'No' }}
                        </span>
                      </li>
                    </ul>
                  </div>
                  <div class="col-md-6">
                    <h6>Push Notifications</h6>
                    <ul class="list-group list-group-flush">
                      <li class="list-group-item d-flex justify-content-between align-items-center">
                        Supported
                        <span class="badge" 
                              [class.bg-success]="pwaResults.pushNotifications?.supported"
                              [class.bg-danger]="!pwaResults.pushNotifications?.supported">
                          {{ pwaResults.pushNotifications?.supported ? 'Yes' : 'No' }}
                        </span>
                      </li>
                      <li class="list-group-item d-flex justify-content-between align-items-center">
                        Permission
                        <span class="badge" 
                              [class.bg-success]="pwaResults.pushNotifications?.permission === 'granted'"
                              [class.bg-warning]="pwaResults.pushNotifications?.permission === 'default'"
                              [class.bg-danger]="pwaResults.pushNotifications?.permission === 'denied'">
                          {{ pwaResults.pushNotifications?.permission || 'Unknown' }}
                        </span>
                      </li>
                    </ul>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Mobile Gesture Tests -->
        <div class="row">
          <div class="col-12 mb-4">
            <div class="card">
              <div class="card-header d-flex justify-content-between align-items-center">
                <h5 class="mb-0">
                  <i class="fas fa-hand-rock me-2"></i>
                  Mobile Gesture Tests
                </h5>
                <button 
                  class="btn btn-sm btn-outline-primary" 
                  (click)="runGestureTests()"
                  [disabled]="testingGestures">
                  <i class="fas fa-play me-1" *ngIf="!testingGestures"></i>
                  <i class="fas fa-spinner fa-spin me-1" *ngIf="testingGestures"></i>
                  Test
                </button>
              </div>
              <div class="card-body">
                <div class="row" *ngIf="gestureResults">
                  <div class="col-md-3">
                    <div class="text-center">
                      <h6>Pinch Zoom</h6>
                      <div class="mb-2">
                        <span class="badge" 
                              [class.bg-success]="gestureResults.pinchZoom?.supported"
                              [class.bg-danger]="!gestureResults.pinchZoom?.supported">
                          {{ gestureResults.pinchZoom?.supported ? 'Supported' : 'Not Supported' }}
                        </span>
                      </div>
                      <small class="text-muted">
                        Enabled: {{ gestureResults.pinchZoom?.enabled ? 'Yes' : 'No' }}
                      </small>
                    </div>
                  </div>
                  <div class="col-md-3">
                    <div class="text-center">
                      <h6>Swipe Navigation</h6>
                      <div class="mb-2">
                        <span class="badge bg-success">
                          Supported
                        </span>
                      </div>
                      <small class="text-muted">
                        Gestures: {{ gestureResults.swipeNavigation?.gestures?.length || 0 }}
                      </small>
                    </div>
                  </div>
                  <div class="col-md-3">
                    <div class="text-center">
                      <h6>Pull to Refresh</h6>
                      <div class="mb-2">
                        <span class="badge" 
                              [class.bg-success]="gestureResults.pullToRefresh?.supported"
                              [class.bg-danger]="!gestureResults.pullToRefresh?.supported">
                          {{ gestureResults.pullToRefresh?.supported ? 'Supported' : 'Not Supported' }}
                        </span>
                      </div>
                      <small class="text-muted">
                        Implemented: {{ gestureResults.pullToRefresh?.implemented ? 'Yes' : 'No' }}
                      </small>
                    </div>
                  </div>
                  <div class="col-md-3">
                    <div class="text-center">
                      <h6>Touch Feedback</h6>
                      <div class="mb-2">
                        <span class="badge" 
                              [class.bg-success]="gestureResults.touchFeedback?.hapticFeedback"
                              [class.bg-warning]="!gestureResults.touchFeedback?.hapticFeedback">
                          {{ gestureResults.touchFeedback?.hapticFeedback ? 'Haptic' : 'Visual Only' }}
                        </span>
                      </div>
                      <small class="text-muted">
                        Visual: {{ gestureResults.touchFeedback?.visualFeedback ? 'Yes' : 'No' }}
                      </small>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Test Summary -->
        <div class="row" *ngIf="hasResults">
          <div class="col-12">
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">
                  <i class="fas fa-chart-bar me-2"></i>
                  Test Summary
                </h5>
              </div>
              <div class="card-body">
                <div class="row text-center">
                  <div class="col-md-3">
                    <div class="border rounded p-3">
                      <h3 class="text-primary">{{ touchResults.filter(t => t.success).length }}/{{ touchResults.length }}</h3>
                      <p class="mb-0">Touch Tests Passed</p>
                    </div>
                  </div>
                  <div class="col-md-3">
                    <div class="border rounded p-3">
                      <h3 class="text-info">{{ responsiveResults.filter(r => r.layoutValid).length }}/{{ responsiveResults.length }}</h3>
                      <p class="mb-0">Responsive Tests Passed</p>
                    </div>
                  </div>
                  <div class="col-md-3">
                    <div class="border rounded p-3">
                      <h3 class="text-success">{{ getPWATestsPassed() }}/{{ getPWATestsTotal() }}</h3>
                      <p class="mb-0">PWA Features Working</p>
                    </div>
                  </div>
                  <div class="col-md-3">
                    <div class="border rounded p-3">
                      <h3 class="text-warning">{{ getGestureTestsPassed() }}/{{ getGestureTestsTotal() }}</h3>
                      <p class="mb-0">Gesture Tests Passed</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .mobile-testing-container {
      min-height: 100vh;
      background-color: #f8f9fa;
    }

    .card {
      box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
      border: 1px solid rgba(0, 0, 0, 0.125);
    }

    .card-header {
      background-color: #fff;
      border-bottom: 1px solid rgba(0, 0, 0, 0.125);
    }

    .table-responsive {
      max-height: 300px;
      overflow-y: auto;
    }

    .badge {
      font-size: 0.75em;
    }

    code {
      font-size: 0.875em;
      color: #e83e8c;
      background-color: #f8f9fa;
      padding: 0.125rem 0.25rem;
      border-radius: 0.25rem;
    }

    .list-group-item {
      border-left: none;
      border-right: none;
      padding: 0.5rem 0;
    }

    .list-group-item:first-child {
      border-top: none;
    }

    .list-group-item:last-child {
      border-bottom: none;
    }

    @media (max-width: 768px) {
      .btn-group {
        flex-direction: column;
        width: 100%;
      }
      
      .btn-group .btn {
        margin-bottom: 0.5rem;
      }
    }
  `]
})
export class MobileTestingComponent implements OnInit, OnDestroy {
  deviceInfo: DeviceInfo | null = null;
  isStandalone = false;
  
  // Test states
  testing = false;
  testingTouch = false;
  testingResponsive = false;
  testingPWA = false;
  testingGestures = false;
  
  // Test configuration
  touchTestSelectors = `button
.btn
.nav-link
.card
.list-group-item
input[type="text"]
input[type="email"]
textarea
.form-control`;

  // Test results
  touchResults: TouchTestResult[] = [];
  responsiveResults: ResponsiveTestResult[] = [];
  pwaResults: any = null;
  gestureResults: any = null;

  private destroy$ = new Subject<void>();

  constructor(
    private mobileTestingService: MobileTestingService,
    private pwaService: PwaService
  ) {}

  ngOnInit(): void {
    this.isStandalone = this.pwaService.isStandalone();
    
    // Subscribe to device info changes
    this.mobileTestingService.deviceInfo$
      .pipe(takeUntil(this.destroy$))
      .subscribe(deviceInfo => {
        this.deviceInfo = deviceInfo;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  async runAllTests(): Promise<void> {
    this.testing = true;
    
    try {
      await Promise.all([
        this.runTouchTests(),
        this.runResponsiveTests(),
        this.runPWATests(),
        this.runGestureTests()
      ]);
    } catch (error) {
      console.error('Error running all tests:', error);
    } finally {
      this.testing = false;
    }
  }

  async runTouchTests(): Promise<void> {
    this.testingTouch = true;
    
    try {
      const selectors = this.touchTestSelectors
        .split('\n')
        .map(s => s.trim())
        .filter(s => s.length > 0);
      
      this.touchResults = await this.mobileTestingService.testTouchInteractions(selectors);
    } catch (error) {
      console.error('Error running touch tests:', error);
    } finally {
      this.testingTouch = false;
    }
  }

  async runResponsiveTests(): Promise<void> {
    this.testingResponsive = true;
    
    try {
      this.responsiveResults = await this.mobileTestingService.testResponsiveDesign();
    } catch (error) {
      console.error('Error running responsive tests:', error);
    } finally {
      this.testingResponsive = false;
    }
  }

  async runPWATests(): Promise<void> {
    this.testingPWA = true;
    
    try {
      this.pwaResults = await this.mobileTestingService.testPWAFunctionality();
    } catch (error) {
      console.error('Error running PWA tests:', error);
    } finally {
      this.testingPWA = false;
    }
  }

  async runGestureTests(): Promise<void> {
    this.testingGestures = true;
    
    try {
      this.gestureResults = await this.mobileTestingService.testMobileGestures();
    } catch (error) {
      console.error('Error running gesture tests:', error);
    } finally {
      this.testingGestures = false;
    }
  }

  exportResults(): void {
    const report = this.mobileTestingService.getTestReport();
    const blob = new Blob([JSON.stringify(report, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    
    const a = document.createElement('a');
    a.href = url;
    a.download = `mobile-pwa-test-report-${new Date().toISOString().split('T')[0]}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    
    URL.revokeObjectURL(url);
  }

  get hasResults(): boolean {
    return this.touchResults.length > 0 || 
           this.responsiveResults.length > 0 || 
           this.pwaResults !== null || 
           this.gestureResults !== null;
  }

  getPWATestsPassed(): number {
    if (!this.pwaResults) return 0;
    
    let passed = 0;
    if (this.pwaResults.serviceWorkerRegistration?.registered) passed++;
    if (this.pwaResults.offlineCapability?.offlineReady) passed++;
    if (this.pwaResults.installPrompt?.supported) passed++;
    if (this.pwaResults.pushNotifications?.supported) passed++;
    if (this.pwaResults.manifestValidation?.valid) passed++;
    
    return passed;
  }

  getPWATestsTotal(): number {
    return 5; // Total number of PWA tests
  }

  getGestureTestsPassed(): number {
    if (!this.gestureResults) return 0;
    
    let passed = 0;
    if (this.gestureResults.pinchZoom?.supported) passed++;
    if (this.gestureResults.swipeNavigation?.supported) passed++;
    if (this.gestureResults.scrollBehavior?.smoothScrolling) passed++;
    if (this.gestureResults.touchFeedback?.visualFeedback) passed++;
    
    return passed;
  }

  getGestureTestsTotal(): number {
    return 4; // Total number of gesture tests
  }
}