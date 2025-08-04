#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Runs end-to-end tests for StrideHR frontend application
.DESCRIPTION
    This script executes Angular e2e tests with various configurations
.PARAMETER TestSuite
    Specific test suite to run (unit, e2e, all)
.PARAMETER Browser
    Browser to use for testing (Chrome, Firefox, Edge)
.PARAMETER Headless
    Whether to run tests in headless mode
.PARAMETER Coverage
    Whether to generate code coverage
.PARAMETER Watch
    Whether to run tests in watch mode
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("unit", "e2e", "all")]
    [string]$TestSuite = "all",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Chrome", "Firefox", "Edge")]
    [string]$Browser = "Chrome",
    
    [Parameter(Mandatory=$false)]
    [switch]$Headless = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$Coverage = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Watch = $false
)

# Script configuration
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Paths
$ScriptRoot = $PSScriptRoot
$ProjectRoot = $ScriptRoot
$NodeModules = Join-Path $ProjectRoot "node_modules"
$CoverageDir = Join-Path $ProjectRoot "coverage"
$E2EResultsDir = Join-Path $ProjectRoot "e2e-results"

Write-Host "🚀 Starting StrideHR Frontend Test Suite" -ForegroundColor Green
Write-Host "Test Suite: $TestSuite" -ForegroundColor Cyan
Write-Host "Browser: $Browser" -ForegroundColor Cyan
Write-Host "Headless: $Headless" -ForegroundColor Cyan
Write-Host "Coverage: $Coverage" -ForegroundColor Cyan
Write-Host "Watch Mode: $Watch" -ForegroundColor Cyan
Write-Host ""

# Function to check if Node.js and npm are installed
function Test-Prerequisites {
    Write-Host "🔍 Checking prerequisites..." -ForegroundColor Yellow
    
    # Check Node.js
    try {
        $NodeVersion = & node --version 2>$null
        Write-Host "✅ Node.js version: $NodeVersion" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Node.js is not installed or not in PATH" -ForegroundColor Red
        exit 1
    }
    
    # Check npm
    try {
        $NpmVersion = & npm --version 2>$null
        Write-Host "✅ npm version: $NpmVersion" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ npm is not installed or not in PATH" -ForegroundColor Red
        exit 1
    }
    
    # Check if node_modules exists
    if (!(Test-Path $NodeModules)) {
        Write-Host "📦 Installing dependencies..." -ForegroundColor Yellow
        & npm install
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Failed to install dependencies" -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host "✅ Prerequisites check completed" -ForegroundColor Green
    Write-Host ""
}

# Function to run unit tests
function Invoke-UnitTests {
    Write-Host "🧪 Running Unit Tests..." -ForegroundColor Yellow
    
    $TestArgs = @("test")
    
    if (!$Watch) {
        $TestArgs += "--watch=false"
    }
    
    if ($Coverage) {
        $TestArgs += "--code-coverage"
    }
    
    if ($Headless) {
        $TestArgs += "--browsers=ChromeHeadless"
    } else {
        $TestArgs += "--browsers=$Browser"
    }
    
    $StartTime = Get-Date
    
    try {
        & npm run ng @TestArgs
        $ExitCode = $LASTEXITCODE
        
        $Duration = (Get-Date) - $StartTime
        
        if ($ExitCode -eq 0) {
            Write-Host "✅ Unit tests completed successfully in $($Duration.TotalSeconds.ToString('F2')) seconds" -ForegroundColor Green
            return $true
        } else {
            Write-Host "❌ Unit tests failed with exit code $ExitCode" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "❌ Error running unit tests: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Function to run e2e tests
function Invoke-E2ETests {
    Write-Host "🎭 Running End-to-End Tests..." -ForegroundColor Yellow
    
    # Ensure e2e results directory exists
    if (!(Test-Path $E2EResultsDir)) {
        New-Item -ItemType Directory -Path $E2EResultsDir -Force | Out-Null
    }
    
    # Configure Karma for e2e tests
    $KarmaConfig = @"
module.exports = function (config) {
  config.set({
    basePath: '',
    frameworks: ['jasmine', '@angular-devkit/build-angular'],
    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-jasmine-html-reporter'),
      require('karma-coverage'),
      require('@angular-devkit/build-angular/plugins/karma')
    ],
    client: {
      jasmine: {
        random: false
      },
      clearContext: false
    },
    jasmineHtmlReporter: {
      suppressAll: true
    },
    coverageReporter: {
      dir: require('path').join(__dirname, './coverage/e2e'),
      subdir: '.',
      reporters: [
        { type: 'html' },
        { type: 'text-summary' },
        { type: 'lcov' }
      ]
    },
    reporters: ['progress', 'kjhtml', 'coverage'],
    port: 9876,
    colors: true,
    logLevel: config.LOG_INFO,
    autoWatch: false,
    browsers: ['$(if ($Headless) { "ChromeHeadlessCI" } else { $Browser })'],
    singleRun: true,
    restartOnFileChange: false,
    customLaunchers: {
      ChromeHeadlessCI: {
        base: 'ChromeHeadless',
        flags: ['--no-sandbox', '--disable-web-security', '--disable-gpu', '--remote-debugging-port=9222']
      }
    },
    files: [
      'src/app/e2e/**/*.e2e.spec.ts'
    ]
  });
};
"@
    
    # Write temporary karma config for e2e tests
    $E2EKarmaConfig = Join-Path $ProjectRoot "karma.e2e.conf.js"
    $KarmaConfig | Out-File -FilePath $E2EKarmaConfig -Encoding UTF8
    
    $StartTime = Get-Date
    
    try {
        # Run e2e tests with custom karma config
        & npx karma start $E2EKarmaConfig
        $ExitCode = $LASTEXITCODE
        
        $Duration = (Get-Date) - $StartTime
        
        if ($ExitCode -eq 0) {
            Write-Host "✅ E2E tests completed successfully in $($Duration.TotalSeconds.ToString('F2')) seconds" -ForegroundColor Green
            $Success = $true
        } else {
            Write-Host "❌ E2E tests failed with exit code $ExitCode" -ForegroundColor Red
            $Success = $false
        }
        
        # Cleanup temporary config
        if (Test-Path $E2EKarmaConfig) {
            Remove-Item $E2EKarmaConfig -Force
        }
        
        return $Success
    }
    catch {
        Write-Host "❌ Error running E2E tests: $($_.Exception.Message)" -ForegroundColor Red
        
        # Cleanup temporary config
        if (Test-Path $E2EKarmaConfig) {
            Remove-Item $E2EKarmaConfig -Force
        }
        
        return $false
    }
}

# Function to run linting
function Invoke-Linting {
    Write-Host "🔍 Running Code Linting..." -ForegroundColor Yellow
    
    try {
        & npm run lint
        $ExitCode = $LASTEXITCODE
        
        if ($ExitCode -eq 0) {
            Write-Host "✅ Linting completed successfully" -ForegroundColor Green
            return $true
        } else {
            Write-Host "❌ Linting failed with exit code $ExitCode" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "❌ Error running linting: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Function to generate coverage report
function New-CoverageReport {
    Write-Host "📊 Generating Coverage Report..." -ForegroundColor Yellow
    
    if (Test-Path $CoverageDir) {
        $CoverageIndex = Join-Path $CoverageDir "index.html"
        if (Test-Path $CoverageIndex) {
            Write-Host "✅ Coverage report generated at: file://$CoverageIndex" -ForegroundColor Green
            
            # Display coverage summary
            $CoverageSummary = Join-Path $CoverageDir "coverage-summary.json"
            if (Test-Path $CoverageSummary) {
                try {
                    $Summary = Get-Content $CoverageSummary | ConvertFrom-Json
                    Write-Host "📈 Coverage Summary:" -ForegroundColor Cyan
                    Write-Host "  • Lines: $($Summary.total.lines.pct)%" -ForegroundColor White
                    Write-Host "  • Functions: $($Summary.total.functions.pct)%" -ForegroundColor White
                    Write-Host "  • Branches: $($Summary.total.branches.pct)%" -ForegroundColor White
                    Write-Host "  • Statements: $($Summary.total.statements.pct)%" -ForegroundColor White
                }
                catch {
                    Write-Host "⚠️ Could not parse coverage summary" -ForegroundColor Yellow
                }
            }
        }
    } else {
        Write-Host "⚠️ No coverage report found" -ForegroundColor Yellow
    }
}

# Function to run build verification
function Invoke-BuildVerification {
    Write-Host "🔨 Running Build Verification..." -ForegroundColor Yellow
    
    try {
        & npm run build
        $ExitCode = $LASTEXITCODE
        
        if ($ExitCode -eq 0) {
            Write-Host "✅ Build verification completed successfully" -ForegroundColor Green
            return $true
        } else {
            Write-Host "❌ Build verification failed with exit code $ExitCode" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "❌ Error during build verification: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Main execution
$OverallSuccess = $true
$StartTime = Get-Date

try {
    # Check prerequisites
    Test-Prerequisites
    
    # Run linting first
    Write-Host "🔍 Running preliminary checks..." -ForegroundColor Magenta
    $LintSuccess = Invoke-Linting
    if (!$LintSuccess) {
        Write-Host "⚠️ Linting failed, but continuing with tests..." -ForegroundColor Yellow
    }
    Write-Host ""
    
    # Run build verification
    $BuildSuccess = Invoke-BuildVerification
    if (!$BuildSuccess) {
        Write-Host "❌ Build verification failed. Cannot proceed with tests." -ForegroundColor Red
        exit 1
    }
    Write-Host ""
    
    # Run tests based on suite selection
    switch ($TestSuite) {
        "unit" {
            $OverallSuccess = Invoke-UnitTests
        }
        "e2e" {
            $OverallSuccess = Invoke-E2ETests
        }
        "all" {
            Write-Host "🧪 Running Complete Test Suite..." -ForegroundColor Magenta
            Write-Host ""
            
            # Run unit tests first
            $UnitSuccess = Invoke-UnitTests
            Write-Host ""
            
            # Run e2e tests
            $E2ESuccess = Invoke-E2ETests
            Write-Host ""
            
            $OverallSuccess = $UnitSuccess -and $E2ESuccess
        }
    }
    
    # Generate coverage report if requested and tests passed
    if ($Coverage -and $OverallSuccess) {
        Write-Host ""
        New-CoverageReport
    }
    
    # Summary
    $Duration = (Get-Date) - $StartTime
    Write-Host ""
    Write-Host "🏁 Frontend Test Execution Summary" -ForegroundColor Magenta
    Write-Host "Total Duration: $($Duration.TotalMinutes.ToString('F2')) minutes" -ForegroundColor White
    Write-Host "Test Suite: $TestSuite" -ForegroundColor White
    Write-Host "Browser: $Browser" -ForegroundColor White
    
    if ($Coverage -and (Test-Path $CoverageDir)) {
        Write-Host "Coverage Report: file://$(Join-Path $CoverageDir 'index.html')" -ForegroundColor White
    }
    
    if ($OverallSuccess) {
        Write-Host "✅ All frontend tests passed successfully!" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "❌ Some frontend tests failed. Check the output for details." -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "💥 Fatal error during frontend test execution: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkRed
    exit 1
}
finally {
    # Cleanup
    $ProgressPreference = "Continue"
}