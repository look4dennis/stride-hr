# StrideHR Frontend Test Execution Script
# This script runs all Angular unit tests and generates coverage reports

param(
    [switch]$Coverage = $false,
    [switch]$Watch = $false,
    [switch]$Headless = $true,
    [string]$Browsers = "ChromeHeadless"
)

Write-Host "StrideHR Frontend Test Execution" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

# Ensure we're in the correct directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Check if node_modules exists
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing dependencies..." -ForegroundColor Blue
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install dependencies!" -ForegroundColor Red
        exit 1
    }
}

# Prepare test command
$testCommand = "ng test"

# Configure browsers
if ($Headless) {
    $testCommand += " --browsers=ChromeHeadless"
} else {
    $testCommand += " --browsers=$Browsers"
}

# Add watch mode if requested
if ($Watch) {
    Write-Host "Running tests in watch mode..." -ForegroundColor Blue
    $testCommand += " --watch=true"
} else {
    Write-Host "Running tests..." -ForegroundColor Blue
    $testCommand += " --watch=false"
    $testCommand += " --single-run=true"
}

# Add coverage collection if requested
if ($Coverage) {
    Write-Host "Running tests with coverage collection..." -ForegroundColor Blue
    $testCommand += " --code-coverage=true"
    $testCommand += " --source-map=true"
}

# Execute tests
Write-Host "Executing: $testCommand" -ForegroundColor Gray
Invoke-Expression $testCommand

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

# Display coverage report location if coverage was collected
if ($Coverage -and (Test-Path "coverage")) {
    Write-Host "`nCoverage report generated at: coverage/stride-hr-frontend/index.html" -ForegroundColor Green
    
    # Try to open the coverage report
    $coverageIndex = "coverage/stride-hr-frontend/index.html"
    if (Test-Path $coverageIndex) {
        Write-Host "Opening coverage report..." -ForegroundColor Blue
        Start-Process $coverageIndex
    }
    
    # Display coverage summary
    $coverageSummary = "coverage/stride-hr-frontend/coverage-summary.json"
    if (Test-Path $coverageSummary) {
        Write-Host "`nCoverage Summary:" -ForegroundColor Cyan
        $summary = Get-Content $coverageSummary | ConvertFrom-Json
        Write-Host "  Lines: $($summary.total.lines.pct)%" -ForegroundColor Gray
        Write-Host "  Functions: $($summary.total.functions.pct)%" -ForegroundColor Gray
        Write-Host "  Branches: $($summary.total.branches.pct)%" -ForegroundColor Gray
        Write-Host "  Statements: $($summary.total.statements.pct)%" -ForegroundColor Gray
    }
}

Write-Host "`nFrontend test execution completed successfully!" -ForegroundColor Green