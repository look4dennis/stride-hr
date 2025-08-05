#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Runs load tests for StrideHR system
.DESCRIPTION
    This script runs comprehensive load tests to validate system performance
    under various load conditions including concurrent users and sustained load.
.PARAMETER TestFilter
    Optional filter to run specific test classes or methods
.PARAMETER Parallel
    Run tests in parallel (default: true)
.PARAMETER OutputPath
    Path to save test results (default: ./TestResults)
#>

param(
    [string]$TestFilter = "",
    [bool]$Parallel = $true,
    [string]$OutputPath = "./TestResults"
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "=== StrideHR Load Testing Suite ===" -ForegroundColor Green
Write-Host "Starting load tests..." -ForegroundColor Yellow

# Create output directory if it doesn't exist
if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

# Set test parameters
$testProject = "tests/StrideHR.LoadTests/StrideHR.LoadTests.csproj"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$resultsFile = "$OutputPath/LoadTestResults_$timestamp.xml"
$logFile = "$OutputPath/LoadTestLog_$timestamp.log"

try {
    # Build the load test project
    Write-Host "Building load test project..." -ForegroundColor Yellow
    dotnet build $testProject --configuration Release --verbosity minimal
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }

    # Prepare test command
    $testCommand = @(
        "test"
        $testProject
        "--configuration", "Release"
        "--logger", "trx;LogFileName=$resultsFile"
        "--logger", "console;verbosity=detailed"
        "--verbosity", "normal"
        "--collect", "XPlat Code Coverage"
        "--results-directory", $OutputPath
    )

    # Add test filter if specified
    if ($TestFilter) {
        $testCommand += "--filter"
        $testCommand += $TestFilter
    }

    # Add parallel execution settings
    if ($Parallel) {
        $testCommand += "--parallel"
    }

    # Run the tests
    Write-Host "Running load tests..." -ForegroundColor Yellow
    Write-Host "Command: dotnet $($testCommand -join ' ')" -ForegroundColor Gray
    
    $startTime = Get-Date
    & dotnet @testCommand 2>&1 | Tee-Object -FilePath $logFile
    $endTime = Get-Date
    $duration = $endTime - $startTime

    # Check test results
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Load tests completed successfully!" -ForegroundColor Green
        Write-Host "Duration: $($duration.ToString('hh\:mm\:ss'))" -ForegroundColor Green
    } else {
        Write-Host "❌ Some load tests failed!" -ForegroundColor Red
        Write-Host "Duration: $($duration.ToString('hh\:mm\:ss'))" -ForegroundColor Red
    }

    # Display results summary
    Write-Host "`n=== Test Results Summary ===" -ForegroundColor Cyan
    Write-Host "Results file: $resultsFile" -ForegroundColor Gray
    Write-Host "Log file: $logFile" -ForegroundColor Gray
    
    # Try to parse and display basic results
    if (Test-Path $resultsFile) {
        try {
            [xml]$testResults = Get-Content $resultsFile
            $testRun = $testResults.TestRun
            
            Write-Host "Total tests: $($testRun.ResultSummary.Counters.total)" -ForegroundColor White
            Write-Host "Passed: $($testRun.ResultSummary.Counters.passed)" -ForegroundColor Green
            Write-Host "Failed: $($testRun.ResultSummary.Counters.failed)" -ForegroundColor Red
            Write-Host "Skipped: $($testRun.ResultSummary.Counters.skipped)" -ForegroundColor Yellow
        }
        catch {
            Write-Host "Could not parse test results XML" -ForegroundColor Yellow
        }
    }

    # Performance recommendations
    Write-Host "`n=== Performance Recommendations ===" -ForegroundColor Cyan
    Write-Host "1. Review test output for performance metrics" -ForegroundColor White
    Write-Host "2. Check for any tests that exceeded SLA thresholds" -ForegroundColor White
    Write-Host "3. Monitor memory usage patterns during extended tests" -ForegroundColor White
    Write-Host "4. Analyze response time distributions for optimization opportunities" -ForegroundColor White

    exit $LASTEXITCODE
}
catch {
    Write-Host "❌ Error running load tests: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Check the log file for details: $logFile" -ForegroundColor Yellow
    exit 1
}
finally {
    Write-Host "`nLoad testing completed at $(Get-Date)" -ForegroundColor Gray
}