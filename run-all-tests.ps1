# StrideHR Complete Test Suite Execution Script
# This script runs all backend and frontend tests with coverage reporting

param(
    [switch]$Coverage = $true,
    [switch]$BackendOnly = $false,
    [switch]$FrontendOnly = $false,
    [switch]$Watch = $false,
    [string]$Filter = "",
    [switch]$OpenReports = $true
)

Write-Host "StrideHR Complete Test Suite" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green

$startTime = Get-Date
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Initialize results tracking
$backendSuccess = $true
$frontendSuccess = $true
$coverageReports = @()

# Run Backend Tests
if (-not $FrontendOnly) {
    Write-Host "`n🔧 Running Backend Tests..." -ForegroundColor Cyan
    Write-Host "=============================" -ForegroundColor Cyan
    
    try {
        Set-Location "backend"
        
        # Build backend first
        Write-Host "Building backend solution..." -ForegroundColor Blue
        dotnet restore
        dotnet build --configuration Release --no-restore
        
        if ($LASTEXITCODE -ne 0) {
            throw "Backend build failed"
        }
        
        # Run backend tests
        $backendTestCommand = ".\run-tests.ps1"
        if ($Coverage) { $backendTestCommand += " -Coverage" }
        if ($Watch) { $backendTestCommand += " -Watch" }
        if ($Filter) { $backendTestCommand += " -Filter `"$Filter`"" }
        
        Invoke-Expression $backendTestCommand
        
        if ($LASTEXITCODE -ne 0) {
            throw "Backend tests failed"
        }
        
        # Check for coverage report
        if ($Coverage -and (Test-Path "TestResults")) {
            $backendCoverageReport = Get-ChildItem -Path "TestResults" -Filter "index.html" -Recurse | Select-Object -First 1
            if ($backendCoverageReport) {
                $coverageReports += @{
                    Type = "Backend"
                    Path = $backendCoverageReport.FullName
                }
            }
        }
        
        Write-Host "✅ Backend tests completed successfully!" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Backend tests failed: $_" -ForegroundColor Red
        $backendSuccess = $false
    }
    finally {
        Set-Location $scriptPath
    }
}

# Run Frontend Tests
if (-not $BackendOnly) {
    Write-Host "`n🎨 Running Frontend Tests..." -ForegroundColor Cyan
    Write-Host "==============================" -ForegroundColor Cyan
    
    try {
        Set-Location "frontend"
        
        # Check if dependencies are installed
        if (-not (Test-Path "node_modules")) {
            Write-Host "Installing frontend dependencies..." -ForegroundColor Blue
            npm install
            if ($LASTEXITCODE -ne 0) {
                throw "Failed to install frontend dependencies"
            }
        }
        
        # Run frontend tests
        $frontendTestCommand = ".\run-tests.ps1"
        if ($Coverage) { $frontendTestCommand += " -Coverage" }
        if ($Watch) { $frontendTestCommand += " -Watch" }
        
        Invoke-Expression $frontendTestCommand
        
        if ($LASTEXITCODE -ne 0) {
            throw "Frontend tests failed"
        }
        
        # Check for coverage report
        if ($Coverage -and (Test-Path "coverage/stride-hr-frontend/index.html")) {
            $coverageReports += @{
                Type = "Frontend"
                Path = (Resolve-Path "coverage/stride-hr-frontend/index.html").Path
            }
        }
        
        Write-Host "✅ Frontend tests completed successfully!" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Frontend tests failed: $_" -ForegroundColor Red
        $frontendSuccess = $false
    }
    finally {
        Set-Location $scriptPath
    }
}

# Generate Combined Report Summary
Write-Host "`n📊 Test Execution Summary" -ForegroundColor Magenta
Write-Host "==========================" -ForegroundColor Magenta

$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host "Execution Time: $($duration.ToString('mm\:ss'))" -ForegroundColor Gray

if (-not $FrontendOnly) {
    if ($backendSuccess) {
        Write-Host "✅ Backend Tests: PASSED" -ForegroundColor Green
    } else {
        Write-Host "❌ Backend Tests: FAILED" -ForegroundColor Red
    }
}

if (-not $BackendOnly) {
    if ($frontendSuccess) {
        Write-Host "✅ Frontend Tests: PASSED" -ForegroundColor Green
    } else {
        Write-Host "❌ Frontend Tests: FAILED" -ForegroundColor Red
    }
}

# Display coverage reports
if ($Coverage -and $coverageReports.Count -gt 0) {
    Write-Host "`n📈 Coverage Reports Generated:" -ForegroundColor Cyan
    foreach ($report in $coverageReports) {
        Write-Host "  $($report.Type): $($report.Path)" -ForegroundColor Gray
        
        if ($OpenReports -and -not $Watch) {
            try {
                Start-Process $report.Path
            }
            catch {
                Write-Host "    Could not open report automatically" -ForegroundColor Yellow
            }
        }
    }
}

# Overall result
$overallSuccess = $backendSuccess -and $frontendSuccess

if ($overallSuccess) {
    Write-Host "`n🎉 All tests completed successfully!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n💥 Some tests failed. Please check the output above." -ForegroundColor Red
    exit 1
}

# Additional helpful information
Write-Host "`n💡 Helpful Commands:" -ForegroundColor Yellow
Write-Host "  Run with coverage: .\run-all-tests.ps1 -Coverage" -ForegroundColor Gray
Write-Host "  Run in watch mode: .\run-all-tests.ps1 -Watch" -ForegroundColor Gray
Write-Host "  Run backend only: .\run-all-tests.ps1 -BackendOnly" -ForegroundColor Gray
Write-Host "  Run frontend only: .\run-all-tests.ps1 -FrontendOnly" -ForegroundColor Gray
Write-Host "  Filter backend tests: .\run-all-tests.ps1 -Filter `"EmployeeService`"" -ForegroundColor Gray