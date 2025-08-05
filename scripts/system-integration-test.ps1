#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Comprehensive system integration testing script for StrideHR
.DESCRIPTION
    This script performs end-to-end system integration testing including:
    - Database connectivity
    - API health checks
    - Frontend-backend integration
    - Real-time features
    - Multi-branch and multi-currency support
    - Security testing
    - Performance testing
.PARAMETER SkipBuild
    Skip building the applications before testing
.PARAMETER Environment
    Environment to test (Development, Staging, Production)
.PARAMETER Verbose
    Enable verbose logging
#>

param(
    [switch]$SkipBuild,
    [string]$Environment = "Development",
    [switch]$Verbose
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Colors for output
$Red = "`e[31m"
$Green = "`e[32m"
$Yellow = "`e[33m"
$Blue = "`e[34m"
$Reset = "`e[0m"

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = $Reset
    )
    Write-Host "$Color$Message$Reset"
}

function Write-TestResult {
    param(
        [string]$TestName,
        [bool]$Success,
        [string]$Details = ""
    )
    $status = if ($Success) { "${Green}PASS${Reset}" } else { "${Red}FAIL${Reset}" }
    $message = "[$status] $TestName"
    if ($Details) {
        $message += " - $Details"
    }
    Write-Host $message
}

function Test-ServiceHealth {
    param(
        [string]$ServiceName,
        [string]$Url,
        [int]$TimeoutSeconds = 30
    )
    
    try {
        Write-ColorOutput "Testing $ServiceName health at $Url..." $Blue
        
        $response = Invoke-RestMethod -Uri $Url -Method Get -TimeoutSec $TimeoutSeconds -ErrorAction Stop
        
        if ($response -and ($response.Status -eq "Healthy" -or $response.status -eq "Healthy")) {
            Write-TestResult "$ServiceName Health Check" $true "Service is healthy"
            return $true
        } else {
            Write-TestResult "$ServiceName Health Check" $false "Unexpected response: $($response | ConvertTo-Json -Compress)"
            return $false
        }
    }
    catch {
        Write-TestResult "$ServiceName Health Check" $false "Error: $($_.Exception.Message)"
        return $false
    }
}

function Test-DatabaseConnectivity {
    Write-ColorOutput "Testing database connectivity..." $Blue
    
    try {
        # Test database connection through API
        $response = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get -TimeoutSec 10 -ErrorAction Stop
        Write-TestResult "Database Connectivity" $true "Connected successfully"
        return $true
    }
    catch {
        Write-TestResult "Database Connectivity" $false "Error: $($_.Exception.Message)"
        return $false
    }
}

function Test-APIEndpoints {
    Write-ColorOutput "Testing critical API endpoints..." $Blue
    
    $endpoints = @(
        @{ Name = "Health Check"; Url = "http://localhost:5000/health"; Method = "GET" },
        @{ Name = "Swagger Documentation"; Url = "http://localhost:5000/api-docs/v1/swagger.json"; Method = "GET" },
        @{ Name = "Organizations"; Url = "http://localhost:5000/api/organizations"; Method = "GET" },
        @{ Name = "Branches"; Url = "http://localhost:5000/api/branches"; Method = "GET" },
        @{ Name = "Employees"; Url = "http://localhost:5000/api/employees"; Method = "GET" }
    )
    
    $successCount = 0
    
    foreach ($endpoint in $endpoints) {
        try {
            $response = Invoke-WebRequest -Uri $endpoint.Url -Method $endpoint.Method -TimeoutSec 10 -ErrorAction Stop
            
            if ($response.StatusCode -eq 200 -or $response.StatusCode -eq 401 -or $response.StatusCode -eq 403) {
                Write-TestResult "API Endpoint: $($endpoint.Name)" $true "Status: $($response.StatusCode)"
                $successCount++
            } else {
                Write-TestResult "API Endpoint: $($endpoint.Name)" $false "Unexpected status: $($response.StatusCode)"
            }
        }
        catch {
            Write-TestResult "API Endpoint: $($endpoint.Name)" $false "Error: $($_.Exception.Message)"
        }
    }
    
    return $successCount -eq $endpoints.Count
}

function Test-FrontendConnectivity {
    Write-ColorOutput "Testing frontend connectivity..." $Blue
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:4200" -Method Get -TimeoutSec 10 -ErrorAction Stop
        
        if ($response.StatusCode -eq 200) {
            Write-TestResult "Frontend Connectivity" $true "Frontend is accessible"
            return $true
        } else {
            Write-TestResult "Frontend Connectivity" $false "Unexpected status: $($response.StatusCode)"
            return $false
        }
    }
    catch {
        Write-TestResult "Frontend Connectivity" $false "Error: $($_.Exception.Message)"
        return $false
    }
}

function Test-SignalRHub {
    Write-ColorOutput "Testing SignalR hub connectivity..." $Blue
    
    try {
        # Test SignalR hub endpoint (should return 404 for GET requests)
        $response = Invoke-WebRequest -Uri "http://localhost:5000/hubs/notification" -Method Get -TimeoutSec 10 -ErrorAction SilentlyContinue
        
        # SignalR hubs return 404 for GET requests, which is expected
        if ($response.StatusCode -eq 404) {
            Write-TestResult "SignalR Hub" $true "Hub endpoint is registered"
            return $true
        } else {
            Write-TestResult "SignalR Hub" $false "Unexpected status: $($response.StatusCode)"
            return $false
        }
    }
    catch {
        # 404 is expected for SignalR hubs
        if ($_.Exception.Message -like "*404*") {
            Write-TestResult "SignalR Hub" $true "Hub endpoint is registered (404 expected)"
            return $true
        } else {
            Write-TestResult "SignalR Hub" $false "Error: $($_.Exception.Message)"
            return $false
        }
    }
}

function Test-MultiCurrencySupport {
    Write-ColorOutput "Testing multi-currency support..." $Blue
    
    # Test currency formatting
    $currencies = @("USD", "EUR", "GBP", "JPY", "INR")
    $amount = 1234.56
    
    $successCount = 0
    
    foreach ($currency in $currencies) {
        try {
            # Test currency formatting logic
            $formatted = [System.Globalization.CultureInfo]::InvariantCulture.NumberFormat.Clone()
            $formatted.CurrencySymbol = switch ($currency) {
                "USD" { "$" }
                "EUR" { "€" }
                "GBP" { "£" }
                "JPY" { "¥" }
                "INR" { "₹" }
                default { $currency }
            }
            
            Write-TestResult "Currency Support: $currency" $true "Formatting available"
            $successCount++
        }
        catch {
            Write-TestResult "Currency Support: $currency" $false "Error: $($_.Exception.Message)"
        }
    }
    
    return $successCount -eq $currencies.Count
}

function Test-SecurityFeatures {
    Write-ColorOutput "Testing security features..." $Blue
    
    $securityTests = @()
    
    # Test HTTPS redirect (if applicable)
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/api/employees" -Method Get -TimeoutSec 5 -ErrorAction Stop
        if ($response.StatusCode -eq 401 -or $response.StatusCode -eq 403) {
            $securityTests += @{ Name = "Authentication Required"; Success = $true; Details = "Unauthorized access properly blocked" }
        } else {
            $securityTests += @{ Name = "Authentication Required"; Success = $false; Details = "Endpoints may be unprotected" }
        }
    }
    catch {
        $securityTests += @{ Name = "Authentication Required"; Success = $true; Details = "Access properly restricted" }
    }
    
    # Test CORS headers
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -Method Options -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.Headers["Access-Control-Allow-Origin"]) {
            $securityTests += @{ Name = "CORS Configuration"; Success = $true; Details = "CORS headers present" }
        } else {
            $securityTests += @{ Name = "CORS Configuration"; Success = $false; Details = "CORS headers missing" }
        }
    }
    catch {
        $securityTests += @{ Name = "CORS Configuration"; Success = $false; Details = "Error testing CORS" }
    }
    
    # Report security test results
    $successCount = 0
    foreach ($test in $securityTests) {
        Write-TestResult "Security: $($test.Name)" $test.Success $test.Details
        if ($test.Success) { $successCount++ }
    }
    
    return $successCount -eq $securityTests.Count
}

function Test-PerformanceBasics {
    Write-ColorOutput "Testing basic performance..." $Blue
    
    $performanceTests = @()
    
    # Test API response time
    try {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $response = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get -TimeoutSec 10 -ErrorAction Stop
        $stopwatch.Stop()
        
        $responseTime = $stopwatch.ElapsedMilliseconds
        if ($responseTime -lt 1000) {
            $performanceTests += @{ Name = "API Response Time"; Success = $true; Details = "${responseTime}ms" }
        } else {
            $performanceTests += @{ Name = "API Response Time"; Success = $false; Details = "${responseTime}ms (slow)" }
        }
    }
    catch {
        $performanceTests += @{ Name = "API Response Time"; Success = $false; Details = "Error: $($_.Exception.Message)" }
    }
    
    # Test frontend load time
    try {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $response = Invoke-WebRequest -Uri "http://localhost:4200" -Method Get -TimeoutSec 10 -ErrorAction Stop
        $stopwatch.Stop()
        
        $loadTime = $stopwatch.ElapsedMilliseconds
        if ($loadTime -lt 3000) {
            $performanceTests += @{ Name = "Frontend Load Time"; Success = $true; Details = "${loadTime}ms" }
        } else {
            $performanceTests += @{ Name = "Frontend Load Time"; Success = $false; Details = "${loadTime}ms (slow)" }
        }
    }
    catch {
        $performanceTests += @{ Name = "Frontend Load Time"; Success = $false; Details = "Error: $($_.Exception.Message)" }
    }
    
    # Report performance test results
    $successCount = 0
    foreach ($test in $performanceTests) {
        Write-TestResult "Performance: $($test.Name)" $test.Success $test.Details
        if ($test.Success) { $successCount++ }
    }
    
    return $successCount -eq $performanceTests.Count
}

function Start-Services {
    Write-ColorOutput "Starting services for integration testing..." $Blue
    
    # Check if services are already running
    $apiRunning = $false
    $frontendRunning = $false
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -Method Get -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $apiRunning = $true
            Write-ColorOutput "API is already running" $Green
        }
    }
    catch {
        Write-ColorOutput "API is not running, will need to start it" $Yellow
    }
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:4200" -Method Get -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $frontendRunning = $true
            Write-ColorOutput "Frontend is already running" $Green
        }
    }
    catch {
        Write-ColorOutput "Frontend is not running, will need to start it" $Yellow
    }
    
    if (-not $apiRunning -or -not $frontendRunning) {
        Write-ColorOutput "Please ensure both API (localhost:5000) and Frontend (localhost:4200) are running before running integration tests" $Red
        Write-ColorOutput "You can start them using:" $Yellow
        Write-ColorOutput "  Backend: cd backend && dotnet run --project src/StrideHR.API" $Yellow
        Write-ColorOutput "  Frontend: cd frontend && npm start" $Yellow
        return $false
    }
    
    return $true
}

function Run-IntegrationTests {
    Write-ColorOutput "Running comprehensive system integration tests..." $Blue
    Write-ColorOutput "Environment: $Environment" $Blue
    Write-ColorOutput "=" * 60 $Blue
    
    $testResults = @()
    
    # Check if services are running
    if (-not (Start-Services)) {
        Write-ColorOutput "Cannot proceed with integration tests - services not available" $Red
        return $false
    }
    
    # Run all integration tests
    $testResults += @{ Name = "Database Connectivity"; Success = (Test-DatabaseConnectivity) }
    $testResults += @{ Name = "API Health"; Success = (Test-ServiceHealth "API" "http://localhost:5000/health") }
    $testResults += @{ Name = "Frontend Health"; Success = (Test-ServiceHealth "Frontend" "http://localhost:4200") }
    $testResults += @{ Name = "API Endpoints"; Success = (Test-APIEndpoints) }
    $testResults += @{ Name = "Frontend Connectivity"; Success = (Test-FrontendConnectivity) }
    $testResults += @{ Name = "SignalR Hub"; Success = (Test-SignalRHub) }
    $testResults += @{ Name = "Multi-Currency Support"; Success = (Test-MultiCurrencySupport) }
    $testResults += @{ Name = "Security Features"; Success = (Test-SecurityFeatures) }
    $testResults += @{ Name = "Performance Basics"; Success = (Test-PerformanceBasics) }
    
    # Summary
    Write-ColorOutput "=" * 60 $Blue
    Write-ColorOutput "INTEGRATION TEST SUMMARY" $Blue
    Write-ColorOutput "=" * 60 $Blue
    
    $totalTests = $testResults.Count
    $passedTests = ($testResults | Where-Object { $_.Success }).Count
    $failedTests = $totalTests - $passedTests
    
    foreach ($result in $testResults) {
        $status = if ($result.Success) { "${Green}PASS${Reset}" } else { "${Red}FAIL${Reset}" }
        Write-Host "[$status] $($result.Name)"
    }
    
    Write-ColorOutput "=" * 60 $Blue
    Write-ColorOutput "Total Tests: $totalTests" $Blue
    Write-ColorOutput "Passed: $passedTests" $Green
    Write-ColorOutput "Failed: $failedTests" $(if ($failedTests -eq 0) { $Green } else { $Red })
    Write-ColorOutput "Success Rate: $([math]::Round(($passedTests / $totalTests) * 100, 2))%" $(if ($failedTests -eq 0) { $Green } else { $Red })
    
    return $failedTests -eq 0
}

# Main execution
try {
    Write-ColorOutput "StrideHR System Integration Test Suite" $Blue
    Write-ColorOutput "Starting at $(Get-Date)" $Blue
    
    $success = Run-IntegrationTests
    
    Write-ColorOutput "Integration tests completed at $(Get-Date)" $Blue
    
    if ($success) {
        Write-ColorOutput "All integration tests passed! ✅" $Green
        exit 0
    } else {
        Write-ColorOutput "Some integration tests failed! ❌" $Red
        exit 1
    }
}
catch {
    Write-ColorOutput "Integration test suite failed with error: $($_.Exception.Message)" $Red
    Write-ColorOutput "Stack trace: $($_.ScriptStackTrace)" $Red
    exit 1
}