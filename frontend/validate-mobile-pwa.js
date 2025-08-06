#!/usr/bin/env node

/**
 * Simple Mobile and PWA Validation Script
 * This script validates mobile and PWA functionality by checking files and configurations
 */

const fs = require('fs');
const path = require('path');

class MobilePWAValidator {
  constructor() {
    this.results = {
      timestamp: new Date().toISOString(),
      tests: {
        manifestValidation: { status: 'pending', results: [] },
        serviceWorkerConfig: { status: 'pending', results: [] },
        responsiveDesign: { status: 'pending', results: [] },
        touchOptimization: { status: 'pending', results: [] },
        offlineCapability: { status: 'pending', results: [] },
        pwaServices: { status: 'pending', results: [] }
      },
      summary: {
        total: 0,
        passed: 0,
        failed: 0,
        passRate: 0
      }
    };
  }

  async runValidation() {
    console.log('🚀 Starting Mobile & PWA File-based Validation...\n');
    console.log(`Timestamp: ${new Date().toLocaleString()}\n`);

    try {
      // Test 1: Validate PWA Manifest
      await this.validateManifest();
      
      // Test 2: Validate Service Worker Configuration
      await this.validateServiceWorkerConfig();
      
      // Test 3: Validate Responsive Design Setup
      await this.validateResponsiveDesign();
      
      // Test 4: Validate Touch Optimization
      await this.validateTouchOptimization();
      
      // Test 5: Validate Offline Capability
      await this.validateOfflineCapability();
      
      // Test 6: Validate PWA Services
      await this.validatePWAServices();
      
      // Generate summary
      this.generateSummary();
      
      // Display results
      this.displayResults();
      
      return this.results;
      
    } catch (error) {
      console.error('❌ Validation failed:', error.message);
      process.exit(1);
    }
  }

  async validateManifest() {
    console.log('📋 Validating PWA Manifest...');
    this.results.tests.manifestValidation.status = 'running';
    
    const manifestTests = [];
    
    try {
      // Check if manifest file exists
      const manifestPath = path.join('public', 'manifest.webmanifest');
      const manifestExists = fs.existsSync(manifestPath);
      
      manifestTests.push({
        test: 'Manifest file exists',
        passed: manifestExists,
        value: manifestExists ? 'Found' : 'Missing',
        path: manifestPath
      });
      
      if (manifestExists) {
        const manifestContent = fs.readFileSync(manifestPath, 'utf8');
        const manifest = JSON.parse(manifestContent);
        
        // Test required fields
        const requiredFields = ['name', 'short_name', 'display', 'start_url', 'icons'];
        requiredFields.forEach(field => {
          manifestTests.push({
            test: `Manifest has ${field}`,
            passed: manifest.hasOwnProperty(field) && manifest[field],
            value: manifest[field] || 'missing'
          });
        });
        
        // Test display mode
        manifestTests.push({
          test: 'Display mode is standalone',
          passed: manifest.display === 'standalone',
          value: manifest.display || 'not set'
        });
        
        // Test theme and background colors
        manifestTests.push({
          test: 'Theme color is set',
          passed: !!manifest.theme_color,
          value: manifest.theme_color || 'not set'
        });
        
        manifestTests.push({
          test: 'Background color is set',
          passed: !!manifest.background_color,
          value: manifest.background_color || 'not set'
        });
        
        // Test icons
        if (manifest.icons && Array.isArray(manifest.icons)) {
          const requiredSizes = ['192x192', '512x512'];
          const availableSizes = manifest.icons.map(icon => icon.sizes);
          
          requiredSizes.forEach(size => {
            manifestTests.push({
              test: `Icon size ${size} available`,
              passed: availableSizes.includes(size),
              value: availableSizes.includes(size) ? 'available' : 'missing'
            });
          });
          
          // Check if icon files exist
          manifest.icons.forEach((icon, index) => {
            const iconPath = path.join('public', icon.src);
            const iconExists = fs.existsSync(iconPath);
            manifestTests.push({
              test: `Icon file exists: ${icon.src}`,
              passed: iconExists,
              value: iconExists ? 'exists' : 'missing',
              path: iconPath
            });
          });
        }
      }
      
    } catch (error) {
      manifestTests.push({
        test: 'Manifest validation',
        passed: false,
        value: `Error: ${error.message}`
      });
    }
    
    this.results.tests.manifestValidation.results = manifestTests;
    this.results.tests.manifestValidation.status = 'completed';
    
    manifestTests.forEach(test => {
      console.log(test.passed ? '✓' : '✗', `${test.test}: ${test.value}`);
    });
    
    const passed = manifestTests.filter(r => r.passed).length;
    console.log(`📋 Manifest: ${passed}/${manifestTests.length} tests passed\n`);
  }

  async validateServiceWorkerConfig() {
    console.log('⚙️ Validating Service Worker Configuration...');
    this.results.tests.serviceWorkerConfig.status = 'running';
    
    const swTests = [];
    
    try {
      // Check service worker config file
      const swConfigPath = 'ngsw-config.json';
      const swConfigExists = fs.existsSync(swConfigPath);
      
      swTests.push({
        test: 'Service Worker config file exists',
        passed: swConfigExists,
        value: swConfigExists ? 'Found' : 'Missing',
        path: swConfigPath
      });
      
      if (swConfigExists) {
        const swConfigContent = fs.readFileSync(swConfigPath, 'utf8');
        const swConfig = JSON.parse(swConfigContent);
        
        // Test asset groups
        swTests.push({
          test: 'Asset groups configured',
          passed: swConfig.assetGroups && swConfig.assetGroups.length > 0,
          value: swConfig.assetGroups ? `${swConfig.assetGroups.length} groups` : 'none'
        });
        
        // Test data groups
        swTests.push({
          test: 'Data groups configured',
          passed: swConfig.dataGroups && swConfig.dataGroups.length > 0,
          value: swConfig.dataGroups ? `${swConfig.dataGroups.length} groups` : 'none'
        });
        
        // Test API caching
        if (swConfig.dataGroups) {
          const hasApiCache = swConfig.dataGroups.some(group => 
            group.urls && group.urls.some(url => url.includes('/api/'))
          );
          
          swTests.push({
            test: 'API caching configured',
            passed: hasApiCache,
            value: hasApiCache ? 'configured' : 'not configured'
          });
        }
        
        // Test navigation URLs
        swTests.push({
          test: 'Navigation URLs configured',
          passed: swConfig.navigationUrls && swConfig.navigationUrls.length > 0,
          value: swConfig.navigationUrls ? 'configured' : 'not configured'
        });
      }
      
      // Check Angular configuration for service worker
      const angularConfigPath = 'angular.json';
      if (fs.existsSync(angularConfigPath)) {
        const angularConfig = JSON.parse(fs.readFileSync(angularConfigPath, 'utf8'));
        const project = Object.values(angularConfig.projects)[0];
        const prodConfig = project && project.architect && project.architect.build && project.architect.build.configurations && project.architect.build.configurations.production;
        
        swTests.push({
          test: 'Service Worker enabled in production build',
          passed: !!prodConfig?.serviceWorker,
          value: prodConfig?.serviceWorker ? 'enabled' : 'disabled'
        });
      }
      
    } catch (error) {
      swTests.push({
        test: 'Service Worker config validation',
        passed: false,
        value: `Error: ${error.message}`
      });
    }
    
    this.results.tests.serviceWorkerConfig.results = swTests;
    this.results.tests.serviceWorkerConfig.status = 'completed';
    
    swTests.forEach(test => {
      console.log(test.passed ? '✓' : '✗', `${test.test}: ${test.value}`);
    });
    
    const passed = swTests.filter(r => r.passed).length;
    console.log(`⚙️ Service Worker: ${passed}/${swTests.length} tests passed\n`);
  }

  async validateResponsiveDesign() {
    console.log('📱 Validating Responsive Design Setup...');
    this.results.tests.responsiveDesign.status = 'running';
    
    const responsiveTests = [];
    
    try {
      // Check main HTML file for viewport meta tag
      const indexPath = path.join('src', 'index.html');
      if (fs.existsSync(indexPath)) {
        const indexContent = fs.readFileSync(indexPath, 'utf8');
        
        responsiveTests.push({
          test: 'Viewport meta tag present',
          passed: indexContent.includes('name="viewport"'),
          value: indexContent.includes('name="viewport"') ? 'present' : 'missing'
        });
        
        responsiveTests.push({
          test: 'Mobile-friendly viewport configuration',
          passed: indexContent.includes('width=device-width') && indexContent.includes('initial-scale=1'),
          value: indexContent.includes('width=device-width') ? 'mobile-friendly' : 'not optimized'
        });
      }
      
      // Check for Bootstrap or responsive CSS framework
      const packageJsonPath = 'package.json';
      if (fs.existsSync(packageJsonPath)) {
        const packageJson = JSON.parse(fs.readFileSync(packageJsonPath, 'utf8'));
        const dependencies = { ...packageJson.dependencies, ...packageJson.devDependencies };
        
        const hasBootstrap = !!dependencies.bootstrap || !!dependencies['ng-bootstrap'];
        responsiveTests.push({
          test: 'Responsive CSS framework installed',
          passed: hasBootstrap,
          value: hasBootstrap ? 'Bootstrap detected' : 'No responsive framework'
        });
      }
      
      // Check Angular configuration for responsive assets
      const angularConfigPath = 'angular.json';
      if (fs.existsSync(angularConfigPath)) {
        const angularConfig = JSON.parse(fs.readFileSync(angularConfigPath, 'utf8'));
        const project = Object.values(angularConfig.projects)[0];
        const styles = (project && project.architect && project.architect.build && project.architect.build.options && project.architect.build.options.styles) || [];
        
        const hasBootstrapStyles = styles.some(style => 
          style.includes('bootstrap') || style.includes('responsive')
        );
        
        responsiveTests.push({
          test: 'Responsive styles configured',
          passed: hasBootstrapStyles,
          value: hasBootstrapStyles ? 'configured' : 'not configured'
        });
      }
      
      // Check for mobile-specific test files
      const mobileTestFiles = [
        'src/app/mobile-responsive.spec.ts',
        'src/app/pwa-installation.spec.ts'
      ];
      
      mobileTestFiles.forEach(testFile => {
        const exists = fs.existsSync(testFile);
        responsiveTests.push({
          test: `Mobile test file exists: ${path.basename(testFile)}`,
          passed: exists,
          value: exists ? 'exists' : 'missing',
          path: testFile
        });
      });
      
    } catch (error) {
      responsiveTests.push({
        test: 'Responsive design validation',
        passed: false,
        value: `Error: ${error.message}`
      });
    }
    
    this.results.tests.responsiveDesign.results = responsiveTests;
    this.results.tests.responsiveDesign.status = 'completed';
    
    responsiveTests.forEach(test => {
      console.log(test.passed ? '✓' : '✗', `${test.test}: ${test.value}`);
    });
    
    const passed = responsiveTests.filter(r => r.passed).length;
    console.log(`📱 Responsive Design: ${passed}/${responsiveTests.length} tests passed\n`);
  }

  async validateTouchOptimization() {
    console.log('👆 Validating Touch Optimization...');
    this.results.tests.touchOptimization.status = 'running';
    
    const touchTests = [];
    
    try {
      // Check for touch-specific CSS classes and styles
      const stylesPath = path.join('src', 'styles.scss');
      if (fs.existsSync(stylesPath)) {
        const stylesContent = fs.readFileSync(stylesPath, 'utf8');
        
        touchTests.push({
          test: 'Touch-friendly button styles',
          passed: stylesContent.includes('min-height') || stylesContent.includes('touch'),
          value: stylesContent.includes('min-height') ? 'touch-friendly styles found' : 'no touch optimizations'
        });
      }
      
      // Check for mobile-specific components
      const appPath = path.join('src', 'app');
      if (fs.existsSync(appPath)) {
        const files = this.getAllFiles(appPath, '.ts');
        
        // Look for touch event handlers
        let hasTouchEvents = false;
        let hasMobileComponents = false;
        
        files.forEach(file => {
          const content = fs.readFileSync(file, 'utf8');
          if (content.includes('touchstart') || content.includes('touchend') || content.includes('touchmove')) {
            hasTouchEvents = true;
          }
          if (content.includes('mobile') || content.includes('responsive')) {
            hasMobileComponents = true;
          }
        });
        
        touchTests.push({
          test: 'Touch event handlers implemented',
          passed: hasTouchEvents,
          value: hasTouchEvents ? 'touch events found' : 'no touch events'
        });
        
        touchTests.push({
          test: 'Mobile-specific components',
          passed: hasMobileComponents,
          value: hasMobileComponents ? 'mobile components found' : 'no mobile components'
        });
      }
      
      // Check for mobile input types in templates
      const templateFiles = this.getAllFiles(path.join('src', 'app'), '.html');
      let hasMobileInputs = false;
      let hasLargeButtons = false;
      
      templateFiles.forEach(file => {
        const content = fs.readFileSync(file, 'utf8');
        if (content.includes('type="tel"') || content.includes('type="email"') || content.includes('inputmode=')) {
          hasMobileInputs = true;
        }
        if (content.includes('btn-lg') || content.includes('btn-block') || content.includes('w-100')) {
          hasLargeButtons = true;
        }
      });
      
      touchTests.push({
        test: 'Mobile-optimized form inputs',
        passed: hasMobileInputs,
        value: hasMobileInputs ? 'mobile inputs found' : 'standard inputs only'
      });
      
      touchTests.push({
        test: 'Touch-friendly button sizes',
        passed: hasLargeButtons,
        value: hasLargeButtons ? 'large buttons found' : 'standard buttons only'
      });
      
    } catch (error) {
      touchTests.push({
        test: 'Touch optimization validation',
        passed: false,
        value: `Error: ${error.message}`
      });
    }
    
    this.results.tests.touchOptimization.results = touchTests;
    this.results.tests.touchOptimization.status = 'completed';
    
    touchTests.forEach(test => {
      console.log(test.passed ? '✓' : '✗', `${test.test}: ${test.value}`);
    });
    
    const passed = touchTests.filter(r => r.passed).length;
    console.log(`👆 Touch Optimization: ${passed}/${touchTests.length} tests passed\n`);
  }

  async validateOfflineCapability() {
    console.log('📴 Validating Offline Capability...');
    this.results.tests.offlineCapability.status = 'running';
    
    const offlineTests = [];
    
    try {
      // Check for offline storage service
      const offlineServicePath = path.join('src', 'app', 'services', 'offline-storage.service.ts');
      const offlineServiceExists = fs.existsSync(offlineServicePath);
      
      offlineTests.push({
        test: 'Offline storage service exists',
        passed: offlineServiceExists,
        value: offlineServiceExists ? 'exists' : 'missing',
        path: offlineServicePath
      });
      
      if (offlineServiceExists) {
        const serviceContent = fs.readFileSync(offlineServicePath, 'utf8');
        
        offlineTests.push({
          test: 'Local storage implementation',
          passed: serviceContent.includes('localStorage'),
          value: serviceContent.includes('localStorage') ? 'implemented' : 'not implemented'
        });
        
        offlineTests.push({
          test: 'Cache management',
          passed: serviceContent.includes('cache') && serviceContent.includes('expiry'),
          value: serviceContent.includes('cache') ? 'implemented' : 'not implemented'
        });
        
        offlineTests.push({
          test: 'Offline action storage',
          passed: serviceContent.includes('storeAction') || serviceContent.includes('pendingActions'),
          value: serviceContent.includes('storeAction') ? 'implemented' : 'not implemented'
        });
      }
      
      // Check for PWA service
      const pwaServicePath = path.join('src', 'app', 'services', 'pwa.service.ts');
      const pwaServiceExists = fs.existsSync(pwaServicePath);
      
      offlineTests.push({
        test: 'PWA service exists',
        passed: pwaServiceExists,
        value: pwaServiceExists ? 'exists' : 'missing',
        path: pwaServicePath
      });
      
      if (pwaServiceExists) {
        const pwaContent = fs.readFileSync(pwaServicePath, 'utf8');
        
        offlineTests.push({
          test: 'Network status detection',
          passed: pwaContent.includes('navigator.onLine') || pwaContent.includes('online'),
          value: pwaContent.includes('navigator.onLine') ? 'implemented' : 'not implemented'
        });
        
        offlineTests.push({
          test: 'Offline data sync',
          passed: pwaContent.includes('sync') && pwaContent.includes('offline'),
          value: pwaContent.includes('sync') ? 'implemented' : 'not implemented'
        });
      }
      
      // Check for custom service worker
      const customSwPath = path.join('src', 'custom-sw.js');
      const customSwExists = fs.existsSync(customSwPath);
      
      offlineTests.push({
        test: 'Custom service worker exists',
        passed: customSwExists,
        value: customSwExists ? 'exists' : 'missing',
        path: customSwPath
      });
      
    } catch (error) {
      offlineTests.push({
        test: 'Offline capability validation',
        passed: false,
        value: `Error: ${error.message}`
      });
    }
    
    this.results.tests.offlineCapability.results = offlineTests;
    this.results.tests.offlineCapability.status = 'completed';
    
    offlineTests.forEach(test => {
      console.log(test.passed ? '✓' : '✗', `${test.test}: ${test.value}`);
    });
    
    const passed = offlineTests.filter(r => r.passed).length;
    console.log(`📴 Offline Capability: ${passed}/${offlineTests.length} tests passed\n`);
  }

  async validatePWAServices() {
    console.log('🔧 Validating PWA Services...');
    this.results.tests.pwaServices.status = 'running';
    
    const serviceTests = [];
    
    try {
      // Check for push notification service
      const pushServicePath = path.join('src', 'app', 'services', 'push-notification.service.ts');
      const pushServiceExists = fs.existsSync(pushServicePath);
      
      serviceTests.push({
        test: 'Push notification service exists',
        passed: pushServiceExists,
        value: pushServiceExists ? 'exists' : 'missing',
        path: pushServicePath
      });
      
      if (pushServiceExists) {
        const pushContent = fs.readFileSync(pushServicePath, 'utf8');
        
        serviceTests.push({
          test: 'Notification permission handling',
          passed: pushContent.includes('requestPermission'),
          value: pushContent.includes('requestPermission') ? 'implemented' : 'not implemented'
        });
        
        serviceTests.push({
          test: 'Push subscription management',
          passed: pushContent.includes('subscribe') && pushContent.includes('unsubscribe'),
          value: pushContent.includes('subscribe') ? 'implemented' : 'not implemented'
        });
      }
      
      // Check for PWA test files
      const pwaTestFiles = [
        'src/app/pwa-basic.spec.ts',
        'src/app/pwa-functionality.spec.ts',
        'src/app/pwa-integration.spec.ts',
        'src/app/pwa-installation.spec.ts'
      ];
      
      pwaTestFiles.forEach(testFile => {
        const exists = fs.existsSync(testFile);
        serviceTests.push({
          test: `PWA test file exists: ${path.basename(testFile)}`,
          passed: exists,
          value: exists ? 'exists' : 'missing',
          path: testFile
        });
      });
      
      // Check environment configuration
      const envPath = path.join('src', 'environments', 'environment.ts');
      if (fs.existsSync(envPath)) {
        const envContent = fs.readFileSync(envPath, 'utf8');
        
        serviceTests.push({
          test: 'VAPID key configuration',
          passed: envContent.includes('vapid') || envContent.includes('push'),
          value: envContent.includes('vapid') ? 'configured' : 'not configured'
        });
      }
      
    } catch (error) {
      serviceTests.push({
        test: 'PWA services validation',
        passed: false,
        value: `Error: ${error.message}`
      });
    }
    
    this.results.tests.pwaServices.results = serviceTests;
    this.results.tests.pwaServices.status = 'completed';
    
    serviceTests.forEach(test => {
      console.log(test.passed ? '✓' : '✗', `${test.test}: ${test.value}`);
    });
    
    const passed = serviceTests.filter(r => r.passed).length;
    console.log(`🔧 PWA Services: ${passed}/${serviceTests.length} tests passed\n`);
  }

  generateSummary() {
    const allResults = [
      ...this.results.tests.manifestValidation.results,
      ...this.results.tests.serviceWorkerConfig.results,
      ...this.results.tests.responsiveDesign.results,
      ...this.results.tests.touchOptimization.results,
      ...this.results.tests.offlineCapability.results,
      ...this.results.tests.pwaServices.results
    ];
    
    this.results.summary.total = allResults.length;
    this.results.summary.passed = allResults.filter(r => r.passed).length;
    this.results.summary.failed = this.results.summary.total - this.results.summary.passed;
    this.results.summary.passRate = Math.round((this.results.summary.passed / this.results.summary.total) * 100);
  }

  displayResults() {
    console.log('📊 MOBILE & PWA VALIDATION SUMMARY');
    console.log('='.repeat(50));
    console.log(`Total Tests: ${this.results.summary.total}`);
    console.log(`Passed: ${this.results.summary.passed}`);
    console.log(`Failed: ${this.results.summary.failed}`);
    console.log(`Pass Rate: ${this.results.summary.passRate}%`);
    console.log('='.repeat(50));
    
    // Detailed breakdown
    console.log('\nDetailed Test Results:');
    Object.entries(this.results.tests).forEach(([category, test]) => {
      const passed = test.results.filter(r => r.passed).length;
      const total = test.results.length;
      const percentage = total > 0 ? Math.round((passed / total) * 100) : 0;
      console.log(`  ${category}: ${passed}/${total} (${percentage}%)`);
    });
    
    console.log('\n' + '='.repeat(50));
    
    if (this.results.summary.failed === 0) {
      console.log('🎉 Excellent! All mobile and PWA validations passed!');
      console.log('   The application is ready for mobile deployment.');
    } else if (this.results.summary.failed <= 3) {
      console.log('⚠️  Minor issues detected. Review failed tests.');
      console.log('   Most mobile and PWA functionality is properly configured.');
    } else if (this.results.summary.failed <= 8) {
      console.log('🔧 Several configuration issues need attention.');
      console.log('   Address failed tests to ensure optimal mobile experience.');
    } else {
      console.log('❌ Significant mobile and PWA configuration issues detected.');
      console.log('   Major fixes required before mobile deployment.');
    }
    
    // Show recommendations
    console.log('\n📝 Recommendations:');
    const failedTests = [];
    Object.values(this.results.tests).forEach(test => {
      failedTests.push(...test.results.filter(r => !r.passed));
    });
    
    if (failedTests.length === 0) {
      console.log('  ✓ All validations passed - no immediate actions required');
    } else {
      failedTests.slice(0, 5).forEach(test => {
        console.log(`  • Fix: ${test.test} - ${test.value}`);
      });
      
      if (failedTests.length > 5) {
        console.log(`  • ... and ${failedTests.length - 5} more issues`);
      }
    }
  }

  getAllFiles(dir, extension) {
    const files = [];
    
    if (!fs.existsSync(dir)) {
      return files;
    }
    
    const items = fs.readdirSync(dir);
    
    items.forEach(item => {
      const fullPath = path.join(dir, item);
      const stat = fs.statSync(fullPath);
      
      if (stat.isDirectory()) {
        files.push(...this.getAllFiles(fullPath, extension));
      } else if (fullPath.endsWith(extension)) {
        files.push(fullPath);
      }
    });
    
    return files;
  }
}

// Run validation if called directly
if (require.main === module) {
  const validator = new MobilePWAValidator();
  
  validator.runValidation()
    .then(results => {
      const exitCode = results.summary.failed === 0 ? 0 : 
                      results.summary.failed <= 3 ? 0 : 1;
      process.exit(exitCode);
    })
    .catch(error => {
      console.error('Validation failed:', error);
      process.exit(1);
    });
}

module.exports = MobilePWAValidator;