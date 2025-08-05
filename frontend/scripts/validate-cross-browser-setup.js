const fs = require('fs');
const path = require('path');

/**
 * Validation script for cross-browser testing setup
 * Checks if all required files and configurations are in place
 */

console.log('🔍 Validating Cross-Browser Testing Setup...\n');

const validationResults = [];

// Check package.json for required dependencies
function checkPackageJson() {
  console.log('📦 Checking package.json dependencies...');
  
  try {
    const packagePath = path.join(__dirname, '..', 'package.json');
    const packageJson = JSON.parse(fs.readFileSync(packagePath, 'utf8'));
    
    const requiredDeps = [
      'karma-firefox-launcher',
      'karma-safari-launcher', 
      'karma-edge-launcher',
      'selenium-webdriver',
      '@types/selenium-webdriver'
    ];
    
    const missingDeps = requiredDeps.filter(dep => 
      !packageJson.devDependencies[dep] && !packageJson.dependencies[dep]
    );
    
    if (missingDeps.length === 0) {
      console.log('  ✅ All required dependencies found');
      validationResults.push({ test: 'Package Dependencies', status: 'PASS' });
    } else {
      console.log('  ❌ Missing dependencies:', missingDeps.join(', '));
      validationResults.push({ test: 'Package Dependencies', status: 'FAIL', details: missingDeps });
    }
    
    // Check scripts
    const requiredScripts = [
      'test:cross-browser',
      'test:chrome',
      'test:firefox',
      'test:edge',
      'e2e:cross-browser'
    ];
    
    const missingScripts = requiredScripts.filter(script => !packageJson.scripts[script]);
    
    if (missingScripts.length === 0) {
      console.log('  ✅ All required scripts found');
      validationResults.push({ test: 'NPM Scripts', status: 'PASS' });
    } else {
      console.log('  ❌ Missing scripts:', missingScripts.join(', '));
      validationResults.push({ test: 'NPM Scripts', status: 'FAIL', details: missingScripts });
    }
    
  } catch (error) {
    console.log('  ❌ Error reading package.json:', error.message);
    validationResults.push({ test: 'Package Dependencies', status: 'ERROR', details: error.message });
  }
}

// Check Karma configuration
function checkKarmaConfig() {
  console.log('\n⚙️  Checking Karma configuration...');
  
  try {
    const karmaPath = path.join(__dirname, '..', 'karma.conf.js');
    const karmaConfig = fs.readFileSync(karmaPath, 'utf8');
    
    const requiredLaunchers = [
      'karma-chrome-launcher',
      'karma-firefox-launcher',
      'karma-safari-launcher',
      'karma-edge-launcher'
    ];
    
    const missingLaunchers = requiredLaunchers.filter(launcher => 
      !karmaConfig.includes(launcher)
    );
    
    if (missingLaunchers.length === 0) {
      console.log('  ✅ All browser launchers configured');
      validationResults.push({ test: 'Karma Launchers', status: 'PASS' });
    } else {
      console.log('  ❌ Missing launchers:', missingLaunchers.join(', '));
      validationResults.push({ test: 'Karma Launchers', status: 'FAIL', details: missingLaunchers });
    }
    
    // Check custom launchers
    if (karmaConfig.includes('ChromeMobile') && karmaConfig.includes('FirefoxHeadless')) {
      console.log('  ✅ Custom launchers configured');
      validationResults.push({ test: 'Custom Launchers', status: 'PASS' });
    } else {
      console.log('  ❌ Custom launchers not properly configured');
      validationResults.push({ test: 'Custom Launchers', status: 'FAIL' });
    }
    
  } catch (error) {
    console.log('  ❌ Error reading karma.conf.js:', error.message);
    validationResults.push({ test: 'Karma Configuration', status: 'ERROR', details: error.message });
  }
}

// Check test files
function checkTestFiles() {
  console.log('\n🧪 Checking test files...');
  
  const testFiles = [
    'src/app/testing/cross-browser-test.spec.ts',
    'src/app/testing/mobile-pwa-test.spec.ts',
    'src/app/testing/browser-compatibility-config.ts',
    'src/app/testing/browser-compatibility.spec.ts',
    'src/app/testing/mobile-validation.spec.ts',
    'src/app/testing/pwa-installation.spec.ts'
  ];
  
  const missingFiles = [];
  const existingFiles = [];
  
  testFiles.forEach(file => {
    const filePath = path.join(__dirname, '..', file);
    if (fs.existsSync(filePath)) {
      existingFiles.push(file);
    } else {
      missingFiles.push(file);
    }
  });
  
  console.log(`  ✅ Found ${existingFiles.length} test files`);
  existingFiles.forEach(file => console.log(`    - ${file}`));
  
  if (missingFiles.length > 0) {
    console.log(`  ❌ Missing ${missingFiles.length} test files`);
    missingFiles.forEach(file => console.log(`    - ${file}`));
    validationResults.push({ test: 'Test Files', status: 'FAIL', details: missingFiles });
  } else {
    validationResults.push({ test: 'Test Files', status: 'PASS' });
  }
}

// Check Selenium script
function checkSeleniumScript() {
  console.log('\n🌐 Checking Selenium cross-browser script...');
  
  try {
    const scriptPath = path.join(__dirname, 'run-cross-browser-tests.js');
    if (fs.existsSync(scriptPath)) {
      console.log('  ✅ Cross-browser test runner found');
      
      const scriptContent = fs.readFileSync(scriptPath, 'utf8');
      
      // Check for browser support
      const browsers = ['Chrome', 'Firefox', 'Edge', 'Safari'];
      const supportedBrowsers = browsers.filter(browser => 
        scriptContent.includes(browser)
      );
      
      console.log(`  ✅ Supports ${supportedBrowsers.length}/4 browsers: ${supportedBrowsers.join(', ')}`);
      validationResults.push({ test: 'Selenium Script', status: 'PASS', details: supportedBrowsers });
      
    } else {
      console.log('  ❌ Cross-browser test runner not found');
      validationResults.push({ test: 'Selenium Script', status: 'FAIL' });
    }
  } catch (error) {
    console.log('  ❌ Error checking Selenium script:', error.message);
    validationResults.push({ test: 'Selenium Script', status: 'ERROR', details: error.message });
  }
}

// Check PWA configuration
function checkPWAConfig() {
  console.log('\n📱 Checking PWA configuration...');
  
  const pwaFiles = [
    'ngsw-config.json',
    'public/manifest.webmanifest',
    'src/custom-sw.js'
  ];
  
  const missingPWAFiles = [];
  const existingPWAFiles = [];
  
  pwaFiles.forEach(file => {
    const filePath = path.join(__dirname, '..', file);
    if (fs.existsSync(filePath)) {
      existingPWAFiles.push(file);
    } else {
      missingPWAFiles.push(file);
    }
  });
  
  console.log(`  ✅ Found ${existingPWAFiles.length}/3 PWA files`);
  existingPWAFiles.forEach(file => console.log(`    - ${file}`));
  
  if (missingPWAFiles.length > 0) {
    console.log(`  ⚠️  Missing ${missingPWAFiles.length} PWA files`);
    missingPWAFiles.forEach(file => console.log(`    - ${file}`));
    validationResults.push({ test: 'PWA Configuration', status: 'PARTIAL', details: missingPWAFiles });
  } else {
    validationResults.push({ test: 'PWA Configuration', status: 'PASS' });
  }
}

// Check browser compatibility features
function checkBrowserFeatures() {
  console.log('\n🔧 Checking browser compatibility features...');
  
  try {
    const configPath = path.join(__dirname, '..', 'src/app/testing/browser-compatibility-config.ts');
    const configContent = fs.readFileSync(configPath, 'utf8');
    
    const features = [
      'SUPPORTED_BROWSERS',
      'MOBILE_BROWSERS', 
      'CORE_FEATURES',
      'CSS_FEATURES',
      'PWA_FEATURES'
    ];
    
    const foundFeatures = features.filter(feature => 
      configContent.includes(feature)
    );
    
    console.log(`  ✅ Found ${foundFeatures.length}/5 feature sets`);
    foundFeatures.forEach(feature => console.log(`    - ${feature}`));
    
    if (foundFeatures.length === features.length) {
      validationResults.push({ test: 'Browser Features', status: 'PASS' });
    } else {
      const missing = features.filter(f => !foundFeatures.includes(f));
      validationResults.push({ test: 'Browser Features', status: 'PARTIAL', details: missing });
    }
    
  } catch (error) {
    console.log('  ❌ Error checking browser features:', error.message);
    validationResults.push({ test: 'Browser Features', status: 'ERROR', details: error.message });
  }
}

// Print summary
function printSummary() {
  console.log('\n📊 Validation Summary');
  console.log('='.repeat(50));
  
  const passed = validationResults.filter(r => r.status === 'PASS').length;
  const failed = validationResults.filter(r => r.status === 'FAIL').length;
  const partial = validationResults.filter(r => r.status === 'PARTIAL').length;
  const errors = validationResults.filter(r => r.status === 'ERROR').length;
  
  console.log(`✅ Passed: ${passed}`);
  console.log(`❌ Failed: ${failed}`);
  console.log(`⚠️  Partial: ${partial}`);
  console.log(`🚫 Errors: ${errors}`);
  console.log(`📈 Total: ${validationResults.length}`);
  
  console.log('\nDetailed Results:');
  validationResults.forEach(result => {
    const icon = {
      'PASS': '✅',
      'FAIL': '❌', 
      'PARTIAL': '⚠️',
      'ERROR': '🚫'
    }[result.status];
    
    console.log(`${icon} ${result.test}: ${result.status}`);
    if (result.details) {
      console.log(`   Details: ${Array.isArray(result.details) ? result.details.join(', ') : result.details}`);
    }
  });
  
  const overallStatus = failed === 0 && errors === 0 ? 'READY' : 'NEEDS_WORK';
  console.log(`\n🎯 Overall Status: ${overallStatus}`);
  
  if (overallStatus === 'READY') {
    console.log('\n🚀 Cross-browser testing setup is ready!');
    console.log('You can now run:');
    console.log('  npm run test:cross-browser');
    console.log('  npm run e2e:cross-browser');
  } else {
    console.log('\n🔧 Please address the issues above before running cross-browser tests.');
  }
}

// Run all validations
async function runValidation() {
  checkPackageJson();
  checkKarmaConfig();
  checkTestFiles();
  checkSeleniumScript();
  checkPWAConfig();
  checkBrowserFeatures();
  printSummary();
}

// Execute validation
runValidation().catch(error => {
  console.error('Validation failed:', error);
  process.exit(1);
});