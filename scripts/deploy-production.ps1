#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Production deployment script for StrideHR
.DESCRIPTION
    This script handles the complete production deployment process including
    SSL certificate validation, environment setup, and service deployment.
.PARAMETER Environment
    The environment to deploy to (default: production)
.PARAMETER SkipSSLCheck
    Skip SSL certificate validation
.PARAMETER SkipBackup
    Skip database backup before deployment
#>

param(
    [string]$Environment = "production",
    [switch]$SkipSSLCheck,
    [switch]$SkipBackup
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Configuration
$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
$PROJECT_ROOT = Split-Path -Parent $SCRIPT_DIR
$ENV_FILE = "$PROJECT_ROOT/.env.production"
$COMPOSE_FILE = "$PROJECT_ROOT/docker-compose.prod.yml"
$SSL_DIR = "$PROJECT_ROOT/docker/nginx/ssl"
$BACKUP_DIR = "$PROJECT_ROOT/backups"

Write-Host "🚀 Starting StrideHR Production Deployment" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow

# Function to check if command exists
function Test-Command {
    param([string]$Command)
    try {
        Get-Command $Command -ErrorAction Stop | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

# Function to validate environment file
function Test-EnvironmentFile {
    Write-Host "📋 Validating environment configuration..." -ForegroundColor Blue
    
    if (-not (Test-Path $ENV_FILE)) {
        Write-Error "Environment file not found: $ENV_FILE"
        Write-Host "Please copy .env.production.template to .env.production and configure it."
        exit 1
    }
    
    # Check for placeholder values
    $envContent = Get-Content $ENV_FILE -Raw
    $placeholders = @("CHANGE_ME", "your-domain.com", "your-provider.com")
    
    foreach ($placeholder in $placeholders) {
        if ($envContent -match $placeholder) {
            Write-Error "Found placeholder value '$placeholder' in $ENV_FILE"
            Write-Host "Please update all placeholder values in the environment file."
            exit 1
        }
    }
    
    Write-Host "✅ Environment configuration validated" -ForegroundColor Green
}

# Function to validate SSL certificates
function Test-SSLCertificates {
    if ($SkipSSLCheck) {
        Write-Host "⚠️  Skipping SSL certificate validation" -ForegroundColor Yellow
        return
    }
    
    Write-Host "🔒 Validating SSL certificates..." -ForegroundColor Blue
    
    $certFile = "$SSL_DIR/cert.pem"
    $keyFile = "$SSL_DIR/key.pem"
    
    if (-not (Test-Path $certFile)) {
        Write-Error "SSL certificate not found: $certFile"
        Write-Host "Please place your SSL certificate at $certFile"
        exit 1
    }
    
    if (-not (Test-Path $keyFile)) {
        Write-Error "SSL private key not found: $keyFile"
        Write-Host "Please place your SSL private key at $keyFile"
        exit 1
    }
    
    # Validate certificate expiration
    try {
        $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certFile)
        $daysUntilExpiry = ($cert.NotAfter - (Get-Date)).Days
        
        if ($daysUntilExpiry -lt 30) {
            Write-Warning "SSL certificate expires in $daysUntilExpiry days. Consider renewing soon."
        }
        
        Write-Host "✅ SSL certificates validated (expires: $($cert.NotAfter))" -ForegroundColor Green
    }
    catch {
        Write-Error "Failed to validate SSL certificate: $_"
        exit 1
    }
}

# Function to create backup
function Invoke-DatabaseBackup {
    if ($SkipBackup) {
        Write-Host "⚠️  Skipping database backup" -ForegroundColor Yellow
        return
    }
    
    Write-Host "💾 Creating database backup..." -ForegroundColor Blue
    
    if (-not (Test-Path $BACKUP_DIR)) {
        New-Item -ItemType Directory -Path $BACKUP_DIR -Force | Out-Null
    }
    
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $backupFile = "$BACKUP_DIR/stridehr-backup-$timestamp.sql"
    
    try {
        # Load environment variables
        Get-Content $ENV_FILE | ForEach-Object {
            if ($_ -match '^([^#][^=]+)=(.*)$') {
                [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
            }
        }
        
        $dbHost = $env:DB_HOST
        $dbName = $env:DB_NAME
        $dbUser = $env:DB_USER
        $dbPassword = $env:DB_PASSWORD
        
        # Create backup using mysqldump
        $mysqldumpCmd = "mysqldump -h $dbHost -u $dbUser -p$dbPassword $dbName > $backupFile"
        Invoke-Expression $mysqldumpCmd
        
        Write-Host "✅ Database backup created: $backupFile" -ForegroundColor Green
    }
    catch {
        Write-Warning "Failed to create database backup: $_"
        Write-Host "Continuing with deployment..."
    }
}

# Function to deploy services
function Invoke-ServiceDeployment {
    Write-Host "🐳 Deploying services..." -ForegroundColor Blue
    
    # Pull latest images
    Write-Host "Pulling latest images..." -ForegroundColor Yellow
    docker-compose -f $COMPOSE_FILE pull
    
    # Build and start services
    Write-Host "Building and starting services..." -ForegroundColor Yellow
    docker-compose -f $COMPOSE_FILE --env-file $ENV_FILE up -d --build
    
    # Wait for services to be healthy
    Write-Host "Waiting for services to be healthy..." -ForegroundColor Yellow
    $maxWaitTime = 300 # 5 minutes
    $waitTime = 0
    
    do {
        Start-Sleep -Seconds 10
        $waitTime += 10
        
        $healthyServices = docker-compose -f $COMPOSE_FILE ps --services --filter "status=running" | Measure-Object | Select-Object -ExpandProperty Count
        $totalServices = docker-compose -f $COMPOSE_FILE config --services | Measure-Object | Select-Object -ExpandProperty Count
        
        Write-Host "Healthy services: $healthyServices/$totalServices" -ForegroundColor Yellow
        
        if ($healthyServices -eq $totalServices) {
            Write-Host "✅ All services are healthy" -ForegroundColor Green
            break
        }
        
        if ($waitTime -ge $maxWaitTime) {
            Write-Error "Services failed to become healthy within $maxWaitTime seconds"
            docker-compose -f $COMPOSE_FILE logs --tail=50
            exit 1
        }
    } while ($true)
}

# Function to run post-deployment tests
function Invoke-PostDeploymentTests {
    Write-Host "🧪 Running post-deployment tests..." -ForegroundColor Blue
    
    # Load environment variables
    Get-Content $ENV_FILE | ForEach-Object {
        if ($_ -match '^([^#][^=]+)=(.*)$') {
            [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
        }
    }
    
    $frontendUrl = $env:FRONTEND_URL
    $apiUrl = "$frontendUrl/api"
    
    # Test frontend availability
    try {
        $response = Invoke-WebRequest -Uri $frontendUrl -Method GET -TimeoutSec 30
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ Frontend is accessible" -ForegroundColor Green
        }
    }
    catch {
        Write-Error "Frontend health check failed: $_"
    }
    
    # Test API health endpoint
    try {
        $response = Invoke-WebRequest -Uri "$apiUrl/health" -Method GET -TimeoutSec 30
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ API health check passed" -ForegroundColor Green
        }
    }
    catch {
        Write-Error "API health check failed: $_"
    }
    
    # Test database connectivity
    try {
        $response = Invoke-WebRequest -Uri "$apiUrl/health/database" -Method GET -TimeoutSec 30
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ Database connectivity verified" -ForegroundColor Green
        }
    }
    catch {
        Write-Warning "Database health check failed: $_"
    }
    
    # Test Redis connectivity
    try {
        $response = Invoke-WebRequest -Uri "$apiUrl/health/redis" -Method GET -TimeoutSec 30
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ Redis connectivity verified" -ForegroundColor Green
        }
    }
    catch {
        Write-Warning "Redis health check failed: $_"
    }
}

# Main deployment process
try {
    # Prerequisites check
    Write-Host "🔍 Checking prerequisites..." -ForegroundColor Blue
    
    if (-not (Test-Command "docker")) {
        Write-Error "Docker is not installed or not in PATH"
        exit 1
    }
    
    if (-not (Test-Command "docker-compose")) {
        Write-Error "Docker Compose is not installed or not in PATH"
        exit 1
    }
    
    # Validation steps
    Test-EnvironmentFile
    Test-SSLCertificates
    
    # Backup
    Invoke-DatabaseBackup
    
    # Deployment
    Invoke-ServiceDeployment
    
    # Post-deployment tests
    Invoke-PostDeploymentTests
    
    Write-Host "🎉 Production deployment completed successfully!" -ForegroundColor Green
    Write-Host "Frontend URL: $((Get-Content $ENV_FILE | Where-Object { $_ -match '^FRONTEND_URL=' }) -replace 'FRONTEND_URL=', '')" -ForegroundColor Yellow
    Write-Host "Monitor logs with: docker-compose -f $COMPOSE_FILE logs -f" -ForegroundColor Yellow
}
catch {
    Write-Error "Deployment failed: $_"
    Write-Host "Rolling back..." -ForegroundColor Red
    
    try {
        docker-compose -f $COMPOSE_FILE down
        Write-Host "Services stopped. Check logs and fix issues before retrying." -ForegroundColor Yellow
    }
    catch {
        Write-Error "Rollback failed: $_"
    }
    
    exit 1
}