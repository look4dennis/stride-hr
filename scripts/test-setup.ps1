# StrideHR Setup Verification Script
# This script tests the database connection and API setup

param(
    [string]$ApiUrl = "http://localhost:5000",
    [switch]$Verbose
)

Write-Host "StrideHR Setup Verification" -ForegroundColor Green
Write-Host "===========================" -ForegroundColor Green

# Test 1: Check if API is running
Write-Host "1. Testing API connectivity..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "$ApiUrl/health" -Method Get -TimeoutSec 10
    if ($healthResponse.Status -eq "Healthy") {
        Write-Host "   ✓ API is running and healthy" -ForegroundColor Green
        if ($Verbose) {
            Write-Host "   Database Status: $($healthResponse.Database.Status)" -ForegroundColor Cyan
            Write-Host "   System Status: $($healthResponse.System.Status)" -ForegroundColor Cyan
        }
    } else {
        Write-Host "   ⚠ API is running but not healthy" -ForegroundColor Yellow
        Write-Host "   Status: $($healthResponse.Status)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ✗ API is not accessible at $ApiUrl" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Make sure the API is running with: dotnet run" -ForegroundColor Yellow
}

# Test 2: Test database connection specifically
Write-Host "2. Testing database connection..." -ForegroundColor Yellow
try {
    $dbTestResponse = Invoke-RestMethod -Uri "$ApiUrl/api/DatabaseTest/connection-test" -Method Get -TimeoutSec 10
    if ($dbTestResponse.CanConnect) {
        Write-Host "   ✓ Database connection successful" -ForegroundColor Green
        if ($Verbose) {
            Write-Host "   Message: $($dbTestResponse.Message)" -ForegroundColor Cyan
            Write-Host "   Timestamp: $($dbTestResponse.Timestamp)" -ForegroundColor Cyan
        }
    } else {
        Write-Host "   ✗ Database connection failed" -ForegroundColor Red
        Write-Host "   Message: $($dbTestResponse.Message)" -ForegroundColor Red
    }
} catch {
    Write-Host "   ✗ Cannot test database connection" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Check database health details
Write-Host "3. Checking database health details..." -ForegroundColor Yellow
try {
    $dbHealthResponse = Invoke-RestMethod -Uri "$ApiUrl/api/DatabaseTest/health" -Method Get -TimeoutSec 10
    if ($dbHealthResponse.IsHealthy) {
        Write-Host "   ✓ Database is healthy" -ForegroundColor Green
        if ($Verbose) {
            Write-Host "   User Count: $($dbHealthResponse.Details.UserCount)" -ForegroundColor Cyan
            Write-Host "   Query Response Time: $($dbHealthResponse.Details.QueryResponseTime)" -ForegroundColor Cyan
            Write-Host "   Can Connect: $($dbHealthResponse.Details.CanConnect)" -ForegroundColor Cyan
        }
    } else {
        Write-Host "   ✗ Database is not healthy" -ForegroundColor Red
        Write-Host "   Error: $($dbHealthResponse.ErrorMessage)" -ForegroundColor Red
    }
} catch {
    Write-Host "   ✗ Cannot check database health" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Check if frontend is accessible
Write-Host "4. Testing frontend accessibility..." -ForegroundColor Yellow
try {
    $frontendResponse = Invoke-WebRequest -Uri "http://localhost:4200" -Method Get -TimeoutSec 5 -UseBasicParsing
    if ($frontendResponse.StatusCode -eq 200) {
        Write-Host "   ✓ Frontend is accessible" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ Frontend returned status code: $($frontendResponse.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ✗ Frontend is not accessible at http://localhost:4200" -ForegroundColor Red
    Write-Host "   Make sure the frontend is running with: ng serve" -ForegroundColor Yellow
}

# Test 5: Check API documentation
Write-Host "5. Testing API documentation..." -ForegroundColor Yellow
try {
    $swaggerResponse = Invoke-WebRequest -Uri "$ApiUrl/api-docs" -Method Get -TimeoutSec 5 -UseBasicParsing
    if ($swaggerResponse.StatusCode -eq 200) {
        Write-Host "   ✓ API documentation is accessible at $ApiUrl/api-docs" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ API documentation returned status code: $($swaggerResponse.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ✗ API documentation is not accessible" -ForegroundColor Red
}

Write-Host ""
Write-Host "Setup Verification Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. If all tests passed, you can access the application at:" -ForegroundColor White
Write-Host "   - Frontend: http://localhost:4200" -ForegroundColor Cyan
Write-Host "   - API: $ApiUrl" -ForegroundColor Cyan
Write-Host "   - API Docs: $ApiUrl/api-docs" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Default Super Admin Credentials:" -ForegroundColor White
Write-Host "   - Username: Superadmin" -ForegroundColor Cyan
Write-Host "   - Password: adminsuper2025$" -ForegroundColor Cyan
Write-Host "   - Email: superadmin@stridehr.com" -ForegroundColor Cyan
Write-Host ""
Write-Host "3. If any tests failed, check the logs and ensure:" -ForegroundColor White
Write-Host "   - MySQL is running on localhost:3306" -ForegroundColor Cyan
Write-Host "   - Database credentials are correct" -ForegroundColor Cyan
Write-Host "   - API is running (dotnet run from backend/src/StrideHR.API)" -ForegroundColor Cyan
Write-Host "   - Frontend is running (ng serve from frontend)" -ForegroundColor Cyan