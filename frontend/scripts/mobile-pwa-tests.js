#!/usr/bin/env node

/**
 * Comprehensive Mobile and PWA Testing Script
 * This script runs automated tests for mobile responsiveness, PWA functionality,
 * and cross-browser compatibility on mobile devices.
 */

const { chromium, firefox, webkit } = require('playwright');
const fs = require('fs').promises;
const path = require('path');

class MobilePWATestSuite {
  constructor(config = {}) {
    this.config = {
      baseUrl: config.baseUrl || 'http://localhost:4200',
      timeout: config.timeout || 30000,
      screenshotDir: config.screenshotDir || './test-results/screenshots',
      reportDir: config.reportDir || './test-results/reports',
      ...config
    };
    
    this.results = {
      mobile: [],
      pwa: [],
      performance: [],
      crossBrowser: []
    };
  }

  /**
   * Run the complete mobile and PWA test suite
   */
  async runTestSuite() {
    console.log('🚀 Starting Mobile & PWA Test Suite...\n');
    
    try {
      // Ensure directories exist
      await this.ensureDirectories();
      
      // Run mobile responsiveness tests
      console.log('📱 Running Mobile Responsiveness Tests...');
      await this.runMobileResponsivenessTests();
      
      // Run PWA functionality tests
      console.log('⚙️ Running PWA Functionality Tests...');
      await this.runPWATests();
      
      // Run touch interaction tests
      console.log('👆 Running Touch Interaction Tests...');
      await this.runTouchInteractionTests();
      
      // Run cross-browser mobile tests
      console.log('🌐 Running Cross-Browser Mobile Tests...');
      await this.runCrossBrowserTests();
      
      // Run mobile performance tests
      console.log('⚡ Running Mobile Performance Tests...');
      await this.runMobilePerformanceTests();
      
      // Generate comprehensive report
      console.log('📊 Generating Test Report...');
      await this.generateReport();
      
      console.log('\n✅ Mobile & PWA Test Suite Completed!');
      this.printSummary();
      
    } catch (error) {
      console.error('❌ Test suite failed:', error);
      process.exit(1);
    }
  }

  /**
   * Test mobile responsiveness across different devices
   */
  async runMobileResponsivenessTests() {
    const devices = [
      { name: 'iPhone SE', width: 375, height: 667, userAgent: 'iPhone' },
      { name: 'iPhone 12', width: 390, height: 844, userAgent: 'iPhone' },
      { name: 'iPhone 12 Pro Max', width: 428, height: 926, userAgent: 'iPhone' },
      { name: 'Samsung Galaxy S21', width: 360, height: 800, userAgent: 'Android' },
      { name: 'Samsung Galaxy S21 Ultra', width: 384, height: 854, userAgent: 'Android' },
      { name: 'iPad', width: 768, height: 1024, userAgent: 'iPad' },
      { name: 'iPad Pro', width: 1024, height: 1366, userAgent: 'iPad' }
    ];

    const browser = await chromium.launch();
    
    for (const device of devices) {
      const context = await browser.newContext({
        viewport: { width: device.width, height: device.height },
        userAgent: `Mozilla/5.0 (${device.userAgent}) AppleWebKit/537.36`
      });
      
      const page = await context.newPage();
      
      try {
        const startTime = Date.now();
        await page.goto(this.config.baseUrl, { waitUntil: 'networkidle' });
        
        // Test critical pages
        const pages = ['/', '/dashboard', '/employees', '/attendance'];
        
        for (const pagePath of pages) {
          await this.testPageResponsiveness(page, device, pagePath);
        }
        
        const duration = Date.now() - startTime;
        
        this.results.mobile.push({
          device: device.name,
          viewport: `${device.width}x${device.height}`,
          passed: true,
          duration,
          issues: []
        });
        
        console.log(`  ✓ ${device.name} (${device.width}x${device.height})`);
        
      } catch (error) {
        this.results.mobile.push({
          device: device.name,
          viewport: `${device.width}x${device.height}`,
          passed: false,
          duration: 0,
          issues: [error.message]
        });
        
        console.log(`  ❌ ${device.name}: ${error.message}`);
      }
      
      await context.close();
    }
    
    await browser.close();
  }

  /**
   * Test PWA functionality
   */
  async runPWATests() {
    const browser = await chromium.launch();
    const context = await browser.newContext({
      viewport: { width: 390, height: 844 } // iPhone 12 size
    });
    const page = await context.newPage();
    
    try {
      await page.goto(this.config.baseUrl);
      
      // Test Service Worker registration
      const swTest = await this.testServiceWorker(page);
      
      // Test Web App Manifest
      const manifestTest = await this.testWebAppManifest(page);
      
      // Test offline capability
      const offlineTest = await this.testOfflineCapability(page);
      
      // Test installability
      const installTest = await this.testInstallability(page);
      
      // Test push notifications
      const pushTest = await this.testPushNotifications(page);
      
      this.results.pwa.push({
        serviceWorker: swTest,
        manifest: manifestTest,
        offline: offlineTest,
        installable: installTest,
        pushNotifications: pushTest,
        overall: swTest.passed && manifestTest.passed && offlineTest.passed
      });
      
      console.log(`  Service Worker: ${swTest.passed ? '✓' : '❌'}`);
      console.log(`  Manifest: ${manifestTest.passed ? '✓' : '❌'}`);
      console.log(`  Offline: ${offlineTest.passed ? '✓' : '❌'}`);
      console.log(`  Installable: ${installTest.passed ? '✓' : '❌'}`);
      console.log(`  Push Notifications: ${pushTest.passed ? '✓' : '❌'}`);
      
    } catch (error) {
      console.error(`  ❌ PWA tests failed: ${error.message}`);
    }
    
    await context.close();
    await browser.close();
  }

  /**
   * Test touch interactions
   */
  async runTouchInteractionTests() {
    const browser = await chromium.launch();
    const context = await browser.newContext({
      viewport: { width: 390, height: 844 },
      hasTouch: true
    });
    const page = await context.newPage();
    
    try {
      await page.goto(this.config.baseUrl);
      
      // Test button taps
      await this.testTouchElement(page, 'button', 'tap');
      
      // Test link taps
      await this.testTouchElement(page, 'a', 'tap');
      
      // Test form input focus
      await this.testTouchElement(page, 'input', 'tap');
      
      // Test swipe gestures (if implemented)
      await this.testSwipeGestures(page);
      
      // Test scroll behavior
      await this.testScrollBehavior(page);
      
      console.log('  ✓ Touch interactions tested');
      
    } catch (error) {
      console.error(`  ❌ Touch interaction tests failed: ${error.message}`);
    }
    
    await context.close();
    await browser.close();
  }

  /**
   * Test across different mobile browsers
   */
  async runCrossBrowserTests() {
    const browsers = [
      { name: 'Chromium', engine: chromium },
      { name: 'WebKit (Safari)', engine: webkit },
      { name: 'Firefox', engine: firefox }
    ];
    
    const criticalPaths = [
      '/',
      '/dashboard',
      '/employees',
      '/attendance/checkin'
    ];
    
    for (const browserInfo of browsers) {
      try {
        const browser = await browserInfo.engine.launch();
        const context = await browser.newContext({
          viewport: { width: 390, height: 844 }
        });
        const page = await context.newPage();
        
        const browserResults = {
          browser: browserInfo.name,
          paths: [],
          passed: true
        };
        
        for (const path of criticalPaths) {
          try {
            const startTime = Date.now();
            await page.goto(`${this.config.baseUrl}${path}`, { 
              waitUntil: 'networkidle',
              timeout: this.config.timeout 
            });
            
            // Check for JavaScript errors
            const errors = await page.evaluate(() => {
              return window.errors || [];
            });
            
            const duration = Date.now() - startTime;
            
            browserResults.paths.push({
              path,
              passed: errors.length === 0,
              duration,
              errors
            });
            
            if (errors.length > 0) {
              browserResults.passed = false;
            }
            
          } catch (error) {
            browserResults.paths.push({
              path,
              passed: false,
              duration: 0,
              errors: [error.message]
            });
            browserResults.passed = false;
          }
        }
        
        this.results.crossBrowser.push(browserResults);
        
        console.log(`  ${browserInfo.name}: ${browserResults.passed ? '✓' : '❌'}`);
        
        await context.close();
        await browser.close();
        
      } catch (error) {
        console.error(`  ❌ ${browserInfo.name} failed: ${error.message}`);
      }
    }
  }

  /**
   * Test mobile performance metrics
   */
  async runMobilePerformanceTests() {
    const browser = await chromium.launch();
    const context = await browser.newContext({
      viewport: { width: 390, height: 844 }
    });
    const page = await context.newPage();
    
    const testPages = [
      { name: 'Home', path: '/' },
      { name: 'Dashboard', path: '/dashboard' },
      { name: 'Employee List', path: '/employees' },
      { name: 'Attendance', path: '/attendance' }
    ];
    
    for (const testPage of testPages) {
      try {
        // Start performance monitoring
        await page.goto(`${this.config.baseUrl}${testPage.path}`, { 
          waitUntil: 'networkidle' 
        });
        
        // Get performance metrics
        const metrics = await page.evaluate(() => {
          const navigation = performance.getEntriesByType('navigation')[0];
          const paint = performance.getEntriesByType('paint');
          
          return {
            loadTime: navigation.loadEventEnd - navigation.loadEventStart,
            domContentLoaded: navigation.domContentLoadedEventEnd - navigation.domContentLoadedEventStart,
            firstPaint: paint.find(p => p.name === 'first-paint')?.startTime || 0,
            firstContentfulPaint: paint.find(p => p.name === 'first-contentful-paint')?.startTime || 0
          };
        });
        
        // Check if metrics meet mobile performance standards
        const passed = 
          metrics.loadTime < 3000 && // 3 seconds
          metrics.firstContentfulPaint < 1500; // 1.5 seconds
        
        this.results.performance.push({
          page: testPage.name,
          path: testPage.path,
          metrics,
          passed,
          standards: {
            loadTime: '< 3000ms',
            firstContentfulPaint: '< 1500ms'
          }
        });
        
        console.log(`  ${testPage.name}: ${passed ? '✓' : '❌'} (Load: ${Math.round(metrics.loadTime)}ms, FCP: ${Math.round(metrics.firstContentfulPaint)}ms)`);
        
      } catch (error) {
        console.error(`  ❌ ${testPage.name} performance test failed: ${error.message}`);
      }
    }
    
    await context.close();
    await browser.close();
  }

  // Helper methods for specific tests

  async testPageResponsiveness(page, device, pagePath) {
    await page.goto(`${this.config.baseUrl}${pagePath}`);
    
    // Take screenshot
    const screenshotPath = path.join(
      this.config.screenshotDir, 
      `${device.name.replace(/\s+/g, '-')}-${pagePath.replace(/\//g, '-') || 'home'}.png`
    );
    await page.screenshot({ path: screenshotPath, fullPage: true });
    
    // Check for responsive design issues
    const issues = await page.evaluate(() => {
      const issues = [];
      
      // Check for horizontal scrollbars
      if (document.body.scrollWidth > window.innerWidth) {
        issues.push('Horizontal scrollbar detected');
      }
      
      // Check for elements extending beyond viewport
      const elements = document.querySelectorAll('*');
      for (const element of elements) {
        const rect = element.getBoundingClientRect();
        if (rect.right > window.innerWidth) {
          issues.push(`Element extends beyond viewport: ${element.tagName}`);
          break; // Only report first occurrence
        }
      }
      
      return issues;
    });
    
    if (issues.length > 0) {
      throw new Error(`Responsive issues: ${issues.join(', ')}`);
    }
  }

  async testServiceWorker(page) {
    try {
      const swRegistered = await page.evaluate(async () => {
        if ('serviceWorker' in navigator) {
          const registration = await navigator.serviceWorker.getRegistration();
          return !!registration;
        }
        return false;
      });
      
      return { passed: swRegistered, message: swRegistered ? 'Service Worker registered' : 'Service Worker not registered' };
    } catch (error) {
      return { passed: false, message: error.message };
    }
  }

  async testWebAppManifest(page) {
    try {
      const manifestValid = await page.evaluate(async () => {
        try {
          const response = await fetch('/manifest.webmanifest');
          const manifest = await response.json();
          return !!(manifest.name && manifest.icons && manifest.display);
        } catch {
          return false;
        }
      });
      
      return { passed: manifestValid, message: manifestValid ? 'Valid manifest found' : 'Invalid or missing manifest' };
    } catch (error) {
      return { passed: false, message: error.message };
    }
  }

  async testOfflineCapability(page) {
    try {
      const offlineCapable = await page.evaluate(async () => {
        try {
          const cache = await caches.open('stride-hr-cache');
          const keys = await cache.keys();
          return keys.length > 0;
        } catch {
          return false;
        }
      });
      
      return { passed: offlineCapable, message: offlineCapable ? 'Offline capability detected' : 'No offline capability' };
    } catch (error) {
      return { passed: false, message: error.message };
    }
  }

  async testInstallability(page) {
    try {
      const installable = await page.evaluate(() => {
        return 'beforeinstallprompt' in window;
      });
      
      return { passed: installable, message: installable ? 'App is installable' : 'App install prompt not supported' };
    } catch (error) {
      return { passed: false, message: error.message };
    }
  }

  async testPushNotifications(page) {
    try {
      const pushSupported = await page.evaluate(() => {
        return 'Notification' in window && 'serviceWorker' in navigator;
      });
      
      return { passed: pushSupported, message: pushSupported ? 'Push notifications supported' : 'Push notifications not supported' };
    } catch (error) {
      return { passed: false, message: error.message };
    }
  }

  async testTouchElement(page, selector, action) {
    const elements = await page.$$(selector);
    if (elements.length > 0) {
      const element = elements[0];
      
      if (action === 'tap') {
        await element.tap();
      }
      
      // Add small delay to allow for response
      await page.waitForTimeout(100);
    }
  }

  async testSwipeGestures(page) {
    // Test swipe gestures if implemented
    // This would depend on your specific swipe implementation
    await page.evaluate(() => {
      // Simulate swipe gesture
      const startX = window.innerWidth / 2;
      const startY = window.innerHeight / 2;
      const endX = startX + 100;
      const endY = startY;
      
      const touchStart = new TouchEvent('touchstart', {
        touches: [new Touch({
          identifier: 0,
          target: document.body,
          clientX: startX,
          clientY: startY
        })]
      });
      
      const touchEnd = new TouchEvent('touchend', {
        changedTouches: [new Touch({
          identifier: 0,
          target: document.body,
          clientX: endX,
          clientY: endY
        })]
      });
      
      document.body.dispatchEvent(touchStart);
      document.body.dispatchEvent(touchEnd);
    });
  }

  async testScrollBehavior(page) {
    // Test smooth scrolling
    await page.evaluate(() => {
      window.scrollTo({ top: 500, behavior: 'smooth' });
    });
    
    await page.waitForTimeout(500);
    
    await page.evaluate(() => {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    });
  }

  async ensureDirectories() {
    await fs.mkdir(this.config.screenshotDir, { recursive: true });
    await fs.mkdir(this.config.reportDir, { recursive: true });
  }

  async generateReport() {
    const report = {
      timestamp: new Date().toISOString(),
      summary: this.generateSummary(),
      results: this.results
    };
    
    const reportPath = path.join(this.config.reportDir, 'mobile-pwa-test-report.json');
    await fs.writeFile(reportPath, JSON.stringify(report, null, 2));
    
    // Generate HTML report
    const htmlReport = this.generateHTMLReport(report);
    const htmlReportPath = path.join(this.config.reportDir, 'mobile-pwa-test-report.html');
    await fs.writeFile(htmlReportPath, htmlReport);
    
    console.log(`📄 Report saved to: ${reportPath}`);
    console.log(`🌐 HTML Report saved to: ${htmlReportPath}`);
  }

  generateSummary() {
    const mobilePassed = this.results.mobile.filter(r => r.passed).length;
    const mobileTotal = this.results.mobile.length;
    
    const pwaPassed = this.results.pwa.filter(r => r.overall).length;
    const pwaTotal = this.results.pwa.length;
    
    const performancePassed = this.results.performance.filter(r => r.passed).length;
    const performanceTotal = this.results.performance.length;
    
    const crossBrowserPassed = this.results.crossBrowser.filter(r => r.passed).length;
    const crossBrowserTotal = this.results.crossBrowser.length;
    
    return {
      mobile: { passed: mobilePassed, total: mobileTotal, rate: (mobilePassed / mobileTotal) * 100 },
      pwa: { passed: pwaPassed, total: pwaTotal, rate: (pwaPassed / pwaTotal) * 100 },
      performance: { passed: performancePassed, total: performanceTotal, rate: (performancePassed / performanceTotal) * 100 },
      crossBrowser: { passed: crossBrowserPassed, total: crossBrowserTotal, rate: (crossBrowserPassed / crossBrowserTotal) * 100 }
    };
  }

  generateHTMLReport(report) {
    return `
<!DOCTYPE html>
<html>
<head>
    <title>Mobile & PWA Test Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .summary { background: #f5f5f5; padding: 20px; border-radius: 5px; margin-bottom: 20px; }
        .test-section { margin-bottom: 30px; }
        .passed { color: green; }
        .failed { color: red; }
        table { width: 100%; border-collapse: collapse; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; }
    </style>
</head>
<body>
    <h1>Mobile & PWA Test Report</h1>
    <p>Generated: ${report.timestamp}</p>
    
    <div class="summary">
        <h2>Summary</h2>
        <p>Mobile Tests: ${report.summary.mobile.passed}/${report.summary.mobile.total} (${report.summary.mobile.rate.toFixed(1)}%)</p>
        <p>PWA Tests: ${report.summary.pwa.passed}/${report.summary.pwa.total} (${report.summary.pwa.rate.toFixed(1)}%)</p>
        <p>Performance Tests: ${report.summary.performance.passed}/${report.summary.performance.total} (${report.summary.performance.rate.toFixed(1)}%)</p>
        <p>Cross-Browser Tests: ${report.summary.crossBrowser.passed}/${report.summary.crossBrowser.total} (${report.summary.crossBrowser.rate.toFixed(1)}%)</p>
    </div>
    
    <div class="test-section">
        <h2>Mobile Responsiveness Results</h2>
        <table>
            <tr><th>Device</th><th>Viewport</th><th>Status</th><th>Duration</th><th>Issues</th></tr>
            ${report.results.mobile.map(r => `
                <tr>
                    <td>${r.device}</td>
                    <td>${r.viewport}</td>
                    <td class="${r.passed ? 'passed' : 'failed'}">${r.passed ? 'PASS' : 'FAIL'}</td>
                    <td>${r.duration}ms</td>
                    <td>${r.issues.join(', ') || 'None'}</td>
                </tr>
            `).join('')}
        </table>
    </div>
    
    <div class="test-section">
        <h2>Performance Results</h2>
        <table>
            <tr><th>Page</th><th>Load Time</th><th>First Contentful Paint</th><th>Status</th></tr>
            ${report.results.performance.map(r => `
                <tr>
                    <td>${r.page}</td>
                    <td>${Math.round(r.metrics.loadTime)}ms</td>
                    <td>${Math.round(r.metrics.firstContentfulPaint)}ms</td>
                    <td class="${r.passed ? 'passed' : 'failed'}">${r.passed ? 'PASS' : 'FAIL'}</td>
                </tr>
            `).join('')}
        </table>
    </div>
</body>
</html>
    `;
  }

  printSummary() {
    const summary = this.generateSummary();
    
    console.log('\n📊 Test Summary:');
    console.log(`📱 Mobile Responsiveness: ${summary.mobile.passed}/${summary.mobile.total} (${summary.mobile.rate.toFixed(1)}%)`);
    console.log(`⚙️ PWA Functionality: ${summary.pwa.passed}/${summary.pwa.total} (${summary.pwa.rate.toFixed(1)}%)`);
    console.log(`⚡ Performance: ${summary.performance.passed}/${summary.performance.total} (${summary.performance.rate.toFixed(1)}%)`);
    console.log(`🌐 Cross-Browser: ${summary.crossBrowser.passed}/${summary.crossBrowser.total} (${summary.crossBrowser.rate.toFixed(1)}%)`);
  }
}

// Run the test suite if this script is executed directly
if (require.main === module) {
  const config = {
    baseUrl: process.env.BASE_URL || 'http://localhost:4200',
    timeout: parseInt(process.env.TIMEOUT) || 30000,
    screenshotDir: './test-results/screenshots',
    reportDir: './test-results/reports'
  };
  
  const testSuite = new MobilePWATestSuite(config);
  testSuite.runTestSuite().catch(console.error);
}

module.exports = { MobilePWATestSuite };