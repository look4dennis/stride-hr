#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Runs comprehensive integration and end-to-end tests for StrideHR system
.DESCRIPTION
    This script executes all integration tests, performance tests, and generates coverage reports
.PARAMETER TestCategory
    Specific test category to run (Integration, Performance, E2E, CI, All)
.PARAMETER GenerateCoverage
    Whether to generate code coverage reports
.PARAMETER Parallel
    Whether to run tests in parallel
.PARAMETER OutputFormat
    Output format for test results (trx, xml, json)
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Integration", "Performance", "E2E", "CI", "All")]
    [string]$TestCategory = "All",
    
    [Parameter(Mandatory=$false)]
    [switch]$GenerateCoverage = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Parallel = $true,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("trx", "xml", "json")]
    [string]$OutputFormat = "trx"
)

# Script configuration
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Paths
$ScriptRoot = $PSScriptRoot
$SolutionRoot = Split-Path $ScriptRoot -Parent
$TestProject = Join-Path $ScriptRoot "tests\StrideHR.Tests\StrideHR.Tests.csproj"
$ResultsDir = Join-Path $ScriptRoot "TestResults"
$CoverageDir = Join-Path $ResultsDir "Coverage"

# Ensure directories exist
if (!(Test-Path $ResultsDir)) {
    New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null
}

if ($GenerateCoverage -and !(Test-Path $CoverageDir)) {
    New-Item -ItemType Directory -Path $CoverageDir -Force | Out-Null
}

Write-Host "🚀 Starting StrideHR Integration Test Suite" -ForegroundColor Green
Write-Host "Test Category: $TestCategory" -ForegroundColor Cyan
Write-Host "Generate Coverage: $GenerateCoverage" -ForegroundColor Cyan
Write-Host "Parallel Execution: $Parallel" -ForegroundColor Cyan
Write-Host "Output Format: $OutputFormat" -ForegroundColor Cyan
Write-Host ""

# Function to run tests with specific filter
function Invoke-TestCategory {
    param(
        [string]$Category,
        [string]$DisplayName,
        [string]$Filter = ""
    )
    
    Write-Host "📋 Running $DisplayName Tests..." -ForegroundColor Yellow
    
    $TestArgs = @(
        "test"
        $TestProject
        "--configuration", "Release"
        "--logger", "$OutputFormat;LogFileName=${Category}Tests.${OutputFormat}"
        "--results-directory", $ResultsDir
        "--verbosity", "normal"
    )
    
    if ($Filter) {
        $TestArgs += "--filter", $Filter
    }
    
    if ($Parallel) {
        $TestArgs += "--parallel"
    }
    
    if ($GenerateCoverage) {
        $TestArgs += @(
            "--collect", "XPlat Code Coverage"
            "--settings", (Join-Path $ScriptRoot "coverlet.runsettings")
        )
    }
    
    $StartTime = Get-Date
    
    try {
        & dotnet @TestArgs
        $ExitCode = $LASTEXITCODE
        
        $Duration = (Get-Date) - $StartTime
        
        if ($ExitCode -eq 0) {
            Write-Host "✅ $DisplayName Tests completed successfully in $($Duration.TotalSeconds.ToString('F2')) seconds" -ForegroundColor Green
        } else {
            Write-Host "❌ $DisplayName Tests failed with exit code $ExitCode" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "❌ Error running $DisplayName Tests: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    
    return $true
}

# Function to generate coverage report
function New-CoverageReport {
    Write-Host "📊 Generating Code Coverage Report..." -ForegroundColor Yellow
    
    try {
        # Find coverage files
        $CoverageFiles = Get-ChildItem -Path $ResultsDir -Recurse -Filter "coverage.cobertura.xml"
        
        if ($CoverageFiles.Count -eq 0) {
            Write-Host "⚠️ No coverage files found" -ForegroundColor Yellow
            return
        }
        
        # Install ReportGenerator if not present
        $ReportGenerator = "reportgenerator"
        if (!(Get-Command $ReportGenerator -ErrorAction SilentlyContinue)) {
            Write-Host "Installing ReportGenerator..." -ForegroundColor Cyan
            & dotnet tool install --global dotnet-reportgenerator-globaltool
        }
        
        # Generate HTML report
        $CoverageFile = $CoverageFiles[0].FullName
        & $ReportGenerator "-reports:$CoverageFile" "-targetdir:$CoverageDir" "-reporttypes:Html;Badges"
        
        Write-Host "✅ Coverage report generated at: $CoverageDir" -ForegroundColor Green
        
        # Display coverage summary
        if (Test-Path (Join-Path $CoverageDir "index.html")) {
            Write-Host "📈 Coverage report available at: file://$(Join-Path $CoverageDir 'index.html')" -ForegroundColor Cyan
        }
    }
    catch {
        Write-Host "❌ Error generating coverage report: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Function to run performance analysis
function Invoke-PerformanceAnalysis {
    Write-Host "⚡ Running Performance Analysis..." -ForegroundColor Yellow
    
    $PerformanceResults = @()
    
    # Run performance tests and capture metrics
    $TestArgs = @(
        "test"
        $TestProject
        "--configuration", "Release"
        "--filter", "Category=Performance"
        "--logger", "console;verbosity=detailed"
    )
    
    $Output = & dotnet @TestArgs 2>&1
    
    # Parse performance metrics from output
    $Output | ForEach-Object {
        if ($_ -match "Average time per.*: (\d+)ms") {
            $PerformanceResults += "Average Response Time: $($Matches[1])ms"
        }
        if ($_ -match "Memory increase.*: (\d+)") {
            $PerformanceResults += "Memory Usage: $($Matches[1]) bytes"
        }
    }
    
    if ($PerformanceResults.Count -gt 0) {
        Write-Host "📊 Performance Metrics:" -ForegroundColor Cyan
        $PerformanceResults | ForEach-Object {
            Write-Host "  • $_" -ForegroundColor White
        }
    }
}

# Main execution
$OverallSuccess = $true
$StartTime = Get-Date

try {
    # Clean previous results
    if (Test-Path $ResultsDir) {
        Remove-Item -Path $ResultsDir -Recurse -Force
        New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null
    }
    
    # Restore packages
    Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Cyan
    & dotnet restore $TestProject
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to restore packages"
    }
    
    # Build test project
    Write-Host "🔨 Building test project..." -ForegroundColor Cyan
    & dotnet build $TestProject --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build test project"
    }
    
    # Run tests based on category
    switch ($TestCategory) {
        "Integration" {
            $OverallSuccess = Invoke-TestCategory -Category "Integration" -DisplayName "Integration" -Filter "Category=Integration"
        }
        "Performance" {
            $OverallSuccess = Invoke-TestCategory -Category "Performance" -DisplayName "Performance" -Filter "Category=Performance"
            if ($OverallSuccess) {
                Invoke-PerformanceAnalysis
            }
        }
        "E2E" {
            $OverallSuccess = Invoke-TestCategory -Category "E2E" -DisplayName "End-to-End" -Filter "Category=E2E"
        }
        "CI" {
            $OverallSuccess = Invoke-TestCategory -Category "CI" -DisplayName "Continuous Integration" -Filter "Category=HealthCheck|Category=API|Category=Database"
        }
        "All" {
            # Run all test categories
            $Categories = @(
                @{ Name = "CI"; Display = "Continuous Integration"; Filter = "Category=HealthCheck|Category=API|Category=Database" },
                @{ Name = "Integration"; Display = "Integration"; Filter = "Category=Integration" },
                @{ Name = "Performance"; Display = "Performance"; Filter = "Category=Performance" },
                @{ Name = "E2E"; Display = "End-to-End"; Filter = "Category=E2E" }
            )
            
            foreach ($Cat in $Categories) {
                $Success = Invoke-TestCategory -Category $Cat.Name -DisplayName $Cat.Display -Filter $Cat.Filter
                if (!$Success) {
                    $OverallSuccess = $false
                }
                Write-Host ""
            }
            
            # Run performance analysis for comprehensive run
            if ($OverallSuccess) {
                Invoke-PerformanceAnalysis
            }
        }
    }
    
    # Generate coverage report if requested
    if ($GenerateCoverage -and $OverallSuccess) {
        Write-Host ""
        New-CoverageReport
    }
    
    # Summary
    $Duration = (Get-Date) - $StartTime
    Write-Host ""
    Write-Host "🏁 Test Execution Summary" -ForegroundColor Magenta
    Write-Host "Total Duration: $($Duration.TotalMinutes.ToString('F2')) minutes" -ForegroundColor White
    Write-Host "Results Directory: $ResultsDir" -ForegroundColor White
    
    if ($OverallSuccess) {
        Write-Host "✅ All tests passed successfully!" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "❌ Some tests failed. Check the results for details." -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "💥 Fatal error during test execution: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkRed
    exit 1
}
finally {
    # Cleanup
    $ProgressPreference = "Continue"
}