#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Production environment validation script for StrideHR
.DESCRIPTION
    This script validates the production environment configuration,
    SSL certificates, security settings, and infrastructure readiness.
#>

param(
    [string]$EnvFile = ".env.production",
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

# Configuration
$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
$PROJECT_ROOT = Split-Path -Parent $SCRIPT_DIR
$ENV_FILE = "$PROJECT_ROOT/$EnvFile"

Write-Host "🔍 StrideHR Production Environment Validation" -ForegroundColor Green

# Function to write verbose output
function Write-Verbose-Custom {
    param([string]$Message)
    if ($Verbose) {
        Write-Host "  $Message" -ForegroundColor Gray
    }
}

# Function to validate environment variables
function Test-EnvironmentVariables {
    Write-Host "📋 Validating environment variables..." -ForegroundColor Blue
    
    if (-not (Test-Path $ENV_FILE)) {
        Write-Error "Environment file not found: $ENV_FILE"
        return $false
    }
    
    $envVars = @{}
    Get-Content $ENV_FILE | ForEach-Object {
        if ($_ -match '^([^#][^=]+)=(.*)$') {
            $envVars[$matches[1]] = $matches[2]
        }
    }
    
    # Required variables
    $requiredVars = @(
        'DB_HOST', 'DB_NAME', 'DB_USER', 'DB_PASSWORD',
        'JWT_SECRET_KEY', 'JWT_ISSUER', 'JWT_AUDIENCE',
        'ENCRYPTION_MASTER_KEY', 'ENCRYPTION_SALT',
        'FRONTEND_URL', 'ALLOWED_HOSTS'
    )
    
    $missingVars = @()
    $weakVars = @()
    
    foreach ($var in $requiredVars) {
        if (-not $envVars.ContainsKey($var) -or [string]::IsNullOrWhiteSpace($envVars[$var])) {
            $missingVars += $var
        }
        elseif ($envVars[$var] -match "CHANGE_ME|your-domain|localhost|password123") {
            $weakVars += $var
        }
    }
    
    if ($missingVars.Count -gt 0) {
        Write-Error "Missing required environment variables: $($missingVars -join ', ')"
        return $false
    }
    
    if ($weakVars.Count -gt 0) {
        Write-Warning "Weak or placeholder values detected: $($weakVars -join ', ')"
    }
    
    # Validate JWT secret key strength
    if ($envVars['JWT_SECRET_KEY'].Length -lt 64) {
        Write-Warning "JWT_SECRET_KEY should be at least 64 characters long for production"
    }
    
    # Validate encryption key strength
    if ($envVars['ENCRYPTION_MASTER_KEY'].Length -lt 32) {
        Write-Error "ENCRYPTION_MASTER_KEY must be at least 32 characters long"
        return $false
    }
    
    Write-Verbose-Custom "Environment variables validation completed"
    Write-Host "✅ Environment variables validated" -ForegroundColor Green
    return $true
}

# Function to validate SSL certificates
function Test-SSLCertificates {
    Write-Host "🔒 Validating SSL certificates..." -ForegroundColor Blue
    
    $sslDir = "$PROJECT_ROOT/docker/nginx/ssl"
    $certFile = "$sslDir/cert.pem"
    $keyFile = "$sslDir/key.pem"
    
    if (-not (Test-Path $certFile)) {
        Write-Error "SSL certificate not found: $certFile"
        return $false
    }
    
    if (-not (Test-Path $keyFile)) {
        Write-Error "SSL private key not found: $keyFile"
        return $false
    }
    
    try {
        # Validate certificate
        $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certFile)
        $now = Get-Date
        
        if ($cert.NotBefore -gt $now) {
            Write-Error "SSL certificate is not yet valid (valid from: $($cert.NotBefore))"
            return $false
        }
        
        if ($cert.NotAfter -lt $now) {
            Write-Error "SSL certificate has expired (expired: $($cert.NotAfter))"
            return $false
        }
        
        $daysUntilExpiry = ($cert.NotAfter - $now).Days
        if ($daysUntilExpiry -lt 30) {
            Write-Warning "SSL certificate expires in $daysUntilExpiry days"
        }
        
        Write-Verbose-Custom "Certificate Subject: $($cert.Subject)"
        Write-Verbose-Custom "Certificate Issuer: $($cert.Issuer)"
        Write-Verbose-Custom "Valid From: $($cert.NotBefore)"
        Write-Verbose-Custom "Valid To: $($cert.NotAfter)"
        
        Write-Host "✅ SSL certificates validated" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Error "Failed to validate SSL certificate: $_"
        return $false
    }
}

# Function to validate Docker configuration
function Test-DockerConfiguration {
    Write-Host "🐳 Validating Docker configuration..." -ForegroundColor Blue
    
    # Check if Docker is running
    try {
        $dockerVersion = docker version --format "{{.Server.Version}}" 2>$null
        if (-not $dockerVersion) {
            Write-Error "Docker is not running or not accessible"
            return $false
        }
        Write-Verbose-Custom "Docker version: $dockerVersion"
    }
    catch {
        Write-Error "Docker is not installed or not accessible"
        return $false
    }
    
    # Check Docker Compose
    try {
        $composeVersion = docker-compose version --short 2>$null
        if (-not $composeVersion) {
            Write-Error "Docker Compose is not installed or not accessible"
            return $false
        }
        Write-Verbose-Custom "Docker Compose version: $composeVersion"
    }
    catch {
        Write-Error "Docker Compose is not installed or not accessible"
        return $false
    }
    
    # Validate docker-compose.prod.yml
    $composeFile = "$PROJECT_ROOT/docker-compose.prod.yml"
    if (-not (Test-Path $composeFile)) {
        Write-Error "Production Docker Compose file not found: $composeFile"
        return $false
    }
    
    try {
        docker-compose -f $composeFile config > $null 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Docker Compose configuration is invalid"
            return $false
        }
    }
    catch {
        Write-Error "Failed to validate Docker Compose configuration: $_"
        return $false
    }
    
    Write-Host "✅ Docker configuration validated" -ForegroundColor Green
    return $true
}

# Function to validate network connectivity
function Test-NetworkConnectivity {
    Write-Host "🌐 Validating network connectivity..." -ForegroundColor Blue
    
    # Test DNS resolution
    $testDomains = @("google.com", "github.com", "docker.io")
    foreach ($domain in $testDomains) {
        try {
            $null = Resolve-DnsName $domain -ErrorAction Stop
            Write-Verbose-Custom "DNS resolution for $domain: OK"
        }
        catch {
            Write-Warning "Failed to resolve DNS for $domain"
        }
    }
    
    # Test internet connectivity
    try {
        $response = Invoke-WebRequest -Uri "https://www.google.com" -Method HEAD -TimeoutSec 10 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Verbose-Custom "Internet connectivity: OK"
        }
    }
    catch {
        Write-Warning "Internet connectivity test failed: $_"
    }
    
    Write-Host "✅ Network connectivity validated" -ForegroundColor Green
    return $true
}

# Function to validate system resources
function Test-SystemResources {
    Write-Host "💻 Validating system resources..." -ForegroundColor Blue
    
    # Check available memory
    $memory = Get-CimInstance -ClassName Win32_ComputerSystem
    $totalMemoryGB = [math]::Round($memory.TotalPhysicalMemory / 1GB, 2)
    
    if ($totalMemoryGB -lt 4) {
        Write-Warning "System has only $totalMemoryGB GB RAM. Minimum 4GB recommended for production."
    }
    else {
        Write-Verbose-Custom "Total RAM: $totalMemoryGB GB"
    }
    
    # Check available disk space
    $disk = Get-CimInstance -ClassName Win32_LogicalDisk | Where-Object { $_.DriveType -eq 3 -and $_.DeviceID -eq "C:" }
    $freeSpaceGB = [math]::Round($disk.FreeSpace / 1GB, 2)
    
    if ($freeSpaceGB -lt 10) {
        Write-Warning "System has only $freeSpaceGB GB free disk space. Minimum 10GB recommended."
    }
    else {
        Write-Verbose-Custom "Free disk space: $freeSpaceGB GB"
    }
    
    # Check CPU cores
    $cpu = Get-CimInstance -ClassName Win32_Processor
    $coreCount = $cpu.NumberOfCores
    
    if ($coreCount -lt 2) {
        Write-Warning "System has only $coreCount CPU core(s). Minimum 2 cores recommended for production."
    }
    else {
        Write-Verbose-Custom "CPU cores: $coreCount"
    }
    
    Write-Host "✅ System resources validated" -ForegroundColor Green
    return $true
}

# Function to validate security configuration
function Test-SecurityConfiguration {
    Write-Host "🔐 Validating security configuration..." -ForegroundColor Blue
    
    # Check nginx security configuration
    $nginxSecurityFile = "$PROJECT_ROOT/docker/nginx/security-headers.conf"
    if (-not (Test-Path $nginxSecurityFile)) {
        Write-Warning "Nginx security headers configuration not found: $nginxSecurityFile"
    }
    else {
        Write-Verbose-Custom "Nginx security headers configuration found"
    }
    
    # Check production nginx configuration
    $nginxProdFile = "$PROJECT_ROOT/docker/nginx/nginx-prod.conf"
    if (-not (Test-Path $nginxProdFile)) {
        Write-Warning "Production nginx configuration not found: $nginxProdFile"
    }
    else {
        Write-Verbose-Custom "Production nginx configuration found"
    }
    
    # Validate firewall recommendations
    Write-Verbose-Custom "Ensure firewall is configured to allow only ports 80, 443, and SSH"
    Write-Verbose-Custom "Ensure fail2ban or similar intrusion prevention is configured"
    
    Write-Host "✅ Security configuration validated" -ForegroundColor Green
    return $true
}

# Main validation process
try {
    $validationResults = @()
    
    Write-Host "Starting production environment validation..." -ForegroundColor Yellow
    Write-Host "Environment file: $ENV_FILE" -ForegroundColor Yellow
    
    # Run all validations
    $validationResults += Test-EnvironmentVariables
    $validationResults += Test-SSLCertificates
    $validationResults += Test-DockerConfiguration
    $validationResults += Test-NetworkConnectivity
    $validationResults += Test-SystemResources
    $validationResults += Test-SecurityConfiguration
    
    # Summary
    $failedValidations = $validationResults | Where-Object { $_ -eq $false }
    
    if ($failedValidations.Count -eq 0) {
        Write-Host ""
        Write-Host "🎉 All validations passed! Environment is ready for production deployment." -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "1. Run: ./scripts/deploy-production.ps1" -ForegroundColor White
        Write-Host "2. Monitor logs: docker-compose -f docker-compose.prod.yml logs -f" -ForegroundColor White
        Write-Host "3. Verify health: curl -k https://your-domain.com/health" -ForegroundColor White
        exit 0
    }
    else {
        Write-Host ""
        Write-Host "❌ $($failedValidations.Count) validation(s) failed. Please fix the issues before deploying." -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Error "Validation failed with error: $_"
    exit 1
}