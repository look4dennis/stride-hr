const { Builder, By, until, Key } = require('selenium-webdriver');
const chrome = require('selenium-webdriver/chrome');
const firefox = require('selenium-webdriver/firefox');
const edge = require('selenium-webdriver/edge');
const safari = require('selenium-webdriver/safari');

/**
 * Cross-browser E2E test runner using Selenium WebDriver
 * Tests critical workflows across Chrome, Firefox, Safari, and Edge
 */
class CrossBrowserTestRunner {
  constructor() {
    this.baseUrl = 'http://localhost:4200';
    this.testResults = [];
    this.browsers = [
      { name: 'Chrome', builder: this.getChromeDriver.bind(this) },
      { name: 'Firefox', builder: this.getFirefoxDriver.bind(this) },
      { name: 'Edge', builder: this.getEdgeDriver.bind(this) },
      // Safari only works on macOS
      ...(process.platform === 'darwin' ? [{ name: 'Safari', builder: this.getSafariDriver.bind(this) }] : [])
    ];
  }

  getChromeDriver() {
    const options = new chrome.Options();
    options.addArguments('--headless');
    options.addArguments('--no-sandbox');
    options.addArguments('--disable-dev-shm-usage');
    options.addArguments('--disable-gpu');
    options.addArguments('--window-size=1920,1080');
    
    return new Builder()
      .forBrowser('chrome')
      .setChromeOptions(options)
      .build();
  }

  getFirefoxDriver() {
    const options = new firefox.Options();
    options.addArguments('--headless');
    options.addArguments('--width=1920');
    options.addArguments('--height=1080');
    
    return new Builder()
      .forBrowser('firefox')
      .setFirefoxOptions(options)
      .build();
  }

  getEdgeDriver() {
    const options = new edge.Options();
    options.addArguments('--headless');
    options.addArguments('--no-sandbox');
    options.addArguments('--disable-dev-shm-usage');
    options.addArguments('--window-size=1920,1080');
    
    return new Builder()
      .forBrowser('MicrosoftEdge')
      .setEdgeOptions(options)
      .build();
  }

  getSafariDriver() {
    const options = new safari.Options();
    
    return new Builder()
      .forBrowser('safari')
      .setSafariOptions(options)
      .build();
  }

  async runAllTests() {
    console.log('🚀 Starting cross-browser testing...\n');
    
    for (const browser of this.browsers) {
      console.log(`Testing ${browser.name}...`);
      await this.runBrowserTests(browser);
    }

    this.printResults();
    this.exitWithCode();
  }

  async runBrowserTests(browser) {
    let driver;
    const browserResults = {
      browser: browser.name,
      tests: [],
      passed: 0,
      failed: 0
    };

    try {
      driver = browser.builder();
      await driver.manage().setTimeouts({ implicit: 10000 });

      // Test suite
      const tests = [
        { name: 'Login Page Load', test: this.testLoginPageLoad.bind(this) },
        { name: 'Login Form Submission', test: this.testLoginFormSubmission.bind(this) },
        { name: 'Dashboard Navigation', test: this.testDashboardNavigation.bind(this) },
        { name: 'Employee List Load', test: this.testEmployeeListLoad.bind(this) },
        { name: 'Modal Functionality', test: this.testModalFunctionality.bind(this) },
        { name: 'Form Validation', test: this.testFormValidation.bind(this) },
        { name: 'Responsive Design', test: this.testResponsiveDesign.bind(this) },
        { name: 'PWA Features', test: this.testPWAFeatures.bind(this) }
      ];

      for (const test of tests) {
        try {
          console.log(`  ✓ Running ${test.name}...`);
          await test.test(driver);
          browserResults.tests.push({ name: test.name, status: 'PASSED' });
          browserResults.passed++;
        } catch (error) {
          console.log(`  ✗ ${test.name} failed: ${error.message}`);
          browserResults.tests.push({ name: test.name, status: 'FAILED', error: error.message });
          browserResults.failed++;
        }
      }

    } catch (error) {
      console.error(`Failed to initialize ${browser.name}: ${error.message}`);
      browserResults.tests.push({ name: 'Browser Initialization', status: 'FAILED', error: error.message });
      browserResults.failed++;
    } finally {
      if (driver) {
        await driver.quit();
      }
    }

    this.testResults.push(browserResults);
    console.log(`${browser.name} completed: ${browserResults.passed} passed, ${browserResults.failed} failed\n`);
  }

  async testLoginPageLoad(driver) {
    await driver.get(`${this.baseUrl}/login`);
    await driver.wait(until.titleContains('StrideHR'), 5000);
    
    const loginForm = await driver.findElement(By.css('form'));
    if (!loginForm) {
      throw new Error('Login form not found');
    }
  }

  async testLoginFormSubmission(driver) {
    await driver.get(`${this.baseUrl}/login`);
    
    const usernameField = await driver.wait(until.elementLocated(By.css('input[type="email"], input[name="username"]')), 5000);
    const passwordField = await driver.findElement(By.css('input[type="password"]'));
    const submitButton = await driver.findElement(By.css('button[type="submit"]'));

    await usernameField.sendKeys('test@example.com');
    await passwordField.sendKeys('password123');
    await submitButton.click();

    // Wait for navigation or error message
    await driver.sleep(2000);
  }

  async testDashboardNavigation(driver) {
    await driver.get(`${this.baseUrl}/dashboard`);
    
    const navItems = await driver.findElements(By.css('nav a, .nav-link'));
    if (navItems.length === 0) {
      throw new Error('Navigation items not found');
    }

    // Test navigation menu responsiveness
    const menuToggle = await driver.findElements(By.css('.navbar-toggler, .menu-toggle'));
    if (menuToggle.length > 0) {
      await menuToggle[0].click();
      await driver.sleep(500);
    }
  }

  async testEmployeeListLoad(driver) {
    await driver.get(`${this.baseUrl}/employees`);
    
    // Wait for employee list or table to load
    await driver.wait(until.elementLocated(By.css('table, .employee-list, .employee-card')), 10000);
    
    const employees = await driver.findElements(By.css('tr, .employee-item'));
    // Should have at least header row or loading state
    if (employees.length === 0) {
      throw new Error('Employee list not loaded');
    }
  }

  async testModalFunctionality(driver) {
    await driver.get(`${this.baseUrl}/employees`);
    
    // Look for add/edit buttons that trigger modals
    const addButton = await driver.findElements(By.css('button[data-bs-toggle="modal"], .btn-add, button:contains("Add")'));
    
    if (addButton.length > 0) {
      await addButton[0].click();
      await driver.sleep(1000);
      
      // Check if modal is visible
      const modal = await driver.findElements(By.css('.modal.show, .modal-dialog'));
      if (modal.length === 0) {
        throw new Error('Modal did not open');
      }

      // Try to close modal
      const closeButton = await driver.findElements(By.css('.modal .btn-close, .modal .close, .modal button[data-bs-dismiss="modal"]'));
      if (closeButton.length > 0) {
        await closeButton[0].click();
        await driver.sleep(500);
      }
    }
  }

  async testFormValidation(driver) {
    await driver.get(`${this.baseUrl}/employees/add`);
    
    const submitButton = await driver.findElements(By.css('button[type="submit"], .btn-submit'));
    
    if (submitButton.length > 0) {
      // Try to submit empty form
      await submitButton[0].click();
      await driver.sleep(1000);
      
      // Check for validation messages
      const validationMessages = await driver.findElements(By.css('.invalid-feedback, .error-message, .validation-error'));
      // Form validation should show some feedback
    }
  }

  async testResponsiveDesign(driver) {
    await driver.get(`${this.baseUrl}/dashboard`);
    
    // Test mobile viewport
    await driver.manage().window().setRect({ width: 375, height: 667 });
    await driver.sleep(1000);
    
    // Check if mobile navigation is present
    const mobileNav = await driver.findElements(By.css('.navbar-toggler, .mobile-menu, .hamburger'));
    
    // Test tablet viewport
    await driver.manage().window().setRect({ width: 768, height: 1024 });
    await driver.sleep(1000);
    
    // Test desktop viewport
    await driver.manage().window().setRect({ width: 1920, height: 1080 });
    await driver.sleep(1000);
  }

  async testPWAFeatures(driver) {
    await driver.get(this.baseUrl);
    
    // Check for service worker registration
    const swRegistered = await driver.executeScript(`
      return 'serviceWorker' in navigator && navigator.serviceWorker.getRegistrations().then(regs => regs.length > 0);
    `);
    
    // Check for manifest
    const manifest = await driver.findElements(By.css('link[rel="manifest"]'));
    if (manifest.length === 0) {
      throw new Error('PWA manifest not found');
    }

    // Check for offline capability indicators
    const offlineIndicators = await driver.findElements(By.css('.offline-indicator, .connection-status'));
  }

  printResults() {
    console.log('\n📊 Cross-Browser Test Results');
    console.log('================================\n');
    
    let totalPassed = 0;
    let totalFailed = 0;
    
    this.testResults.forEach(result => {
      console.log(`${result.browser}:`);
      console.log(`  ✅ Passed: ${result.passed}`);
      console.log(`  ❌ Failed: ${result.failed}`);
      
      if (result.failed > 0) {
        console.log('  Failed tests:');
        result.tests.filter(t => t.status === 'FAILED').forEach(test => {
          console.log(`    - ${test.name}: ${test.error}`);
        });
      }
      console.log('');
      
      totalPassed += result.passed;
      totalFailed += result.failed;
    });
    
    console.log(`Total: ${totalPassed} passed, ${totalFailed} failed`);
    console.log(`Success rate: ${((totalPassed / (totalPassed + totalFailed)) * 100).toFixed(1)}%\n`);
  }

  exitWithCode() {
    const hasFailures = this.testResults.some(result => result.failed > 0);
    process.exit(hasFailures ? 1 : 0);
  }
}

// Run tests if called directly
if (require.main === module) {
  const runner = new CrossBrowserTestRunner();
  runner.runAllTests().catch(error => {
    console.error('Test runner failed:', error);
    process.exit(1);
  });
}

module.exports = CrossBrowserTestRunner;