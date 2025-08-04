# StrideHR Test Execution Script
# This script runs all unit tests and generates coverage reports

param(
    [switch]$Coverage = $false,
    [switch]$Watch = $false,
    [string]$Filter = "",
    [string]$Output = "TestResults"
)

Write-Host "StrideHR Test Execution" -ForegroundColor Green
Write-Host "======================" -ForegroundColor Green

# Ensure we're in the correct directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Clean previous test results
if (Test-Path $Output) {
    Remove-Item $Output -Recurse -Force
    Write-Host "Cleaned previous test results" -ForegroundColor Yellow
}

# Build the solution first
Write-Host "Building solution..." -ForegroundColor Blue
dotnet build --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Prepare test command
$testCommand = "dotnet test"
$testCommand += " --configuration Release"
$testCommand += " --no-build"
$testCommand += " --verbosity normal"
$testCommand += " --results-directory $Output"

# Add filter if specified
if ($Filter) {
    $testCommand += " --filter `"$Filter`""
}

# Add coverage collection if requested
if ($Coverage) {
    Write-Host "Running tests with coverage collection..." -ForegroundColor Blue
    $testCommand += " --collect:`"XPlat Code Coverage`""
    $testCommand += " --settings coverlet.runsettings"
} else {
    Write-Host "Running tests..." -ForegroundColor Blue
}

# Add watch mode if requested
if ($Watch) {
    $testCommand += " --watch"
}

# Execute tests
Write-Host "Executing: $testCommand" -ForegroundColor Gray
Invoke-Expression $testCommand

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

# Generate coverage report if coverage was collected
if ($Coverage -and (Test-Path "$Output")) {
    Write-Host "Generating coverage report..." -ForegroundColor Blue
    
    # Find coverage files
    $coverageFiles = Get-ChildItem -Path $Output -Filter "coverage.cobertura.xml" -Recurse
    
    if ($coverageFiles.Count -gt 0) {
        # Install ReportGenerator if not already installed
        $reportGenerator = Get-Command "reportgenerator" -ErrorAction SilentlyContinue
        if (-not $reportGenerator) {
            Write-Host "Installing ReportGenerator..." -ForegroundColor Yellow
            dotnet tool install -g dotnet-reportgenerator-globaltool
        }
        
        # Generate HTML report
        $reportPath = Join-Path $Output "CoverageReport"
        $coverageFile = $coverageFiles[0].FullName
        
        reportgenerator -reports:$coverageFile -targetdir:$reportPath -reporttypes:Html
        
        Write-Host "Coverage report generated at: $reportPath" -ForegroundColor Green
        
        # Try to open the report
        $indexFile = Join-Path $reportPath "index.html"
        if (Test-Path $indexFile) {
            Write-Host "Opening coverage report..." -ForegroundColor Blue
            Start-Process $indexFile
        }
    } else {
        Write-Host "No coverage files found!" -ForegroundColor Yellow
    }
}

Write-Host "Test execution completed!" -ForegroundColor Green

# Display test summary
if (Test-Path "$Output") {
    $testResults = Get-ChildItem -Path $Output -Filter "*.trx" -Recurse
    if ($testResults.Count -gt 0) {
        Write-Host "`nTest Results Summary:" -ForegroundColor Cyan
        foreach ($result in $testResults) {
            Write-Host "  - $($result.Name)" -ForegroundColor Gray
        }
    }
}

Write-Host "`nTest execution script completed successfully!" -ForegroundColor Green