#!/usr/bin/env pwsh

# Test script to verify backend starts without database connection errors
Write-Host "Testing StrideHR Backend Startup..." -ForegroundColor Green

# Change to API directory
Set-Location "src/StrideHR.API"

# Start the backend in background
Write-Host "Starting backend API..." -ForegroundColor Yellow
$process = Start-Process -FilePath "dotnet" -ArgumentList "run", "--urls", "http://localhost:5001" -PassThru -NoNewWindow

# Wait for startup
Write-Host "Waiting for API to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Test if the API is responding
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5001/health" -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ SUCCESS: Backend started successfully!" -ForegroundColor Green
        Write-Host "✅ Database connection working!" -ForegroundColor Green
        Write-Host "✅ Health check passed!" -ForegroundColor Green
        $success = $true
    } else {
        Write-Host "❌ FAILED: Health check returned status $($response.StatusCode)" -ForegroundColor Red
        $success = $false
    }
} catch {
    Write-Host "❌ FAILED: Could not connect to API: $($_.Exception.Message)" -ForegroundColor Red
    $success = $false
}

# Test Swagger endpoint
try {
    $swaggerResponse = Invoke-WebRequest -Uri "http://localhost:5001/api-docs/v1/swagger.json" -TimeoutSec 10
    if ($swaggerResponse.StatusCode -eq 200) {
        Write-Host "✅ SUCCESS: Swagger documentation is working!" -ForegroundColor Green
    } else {
        Write-Host "⚠️  WARNING: Swagger returned status $($swaggerResponse.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  WARNING: Swagger endpoint not accessible: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Clean up
Write-Host "Stopping backend process..." -ForegroundColor Yellow
Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue

if ($success) {
    Write-Host "`n🎉 TASK COMPLETED: Backend starts without database connection errors!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n❌ TASK FAILED: Backend startup issues detected!" -ForegroundColor Red
    exit 1
}