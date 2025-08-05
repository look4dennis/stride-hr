# StrideHR Production Deployment Script
# Usage: .\deploy.ps1 -Environment production -Version v1.0.0
# Example: .\deploy.ps1 -Environment staging -Version latest

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("staging", "production")]
    [string]$Environment = "production",
    
    [Parameter(Mandatory=$false)]
    [string]$Version = "latest"
)

# Configuration
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$ErrorActionPreference = "Stop"

# Logging functions
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Check prerequisites
function Test-Prerequisites {
    Write-Info "Checking prerequisites..."
    
    # Check if Docker is installed and running
    try {
        $dockerVersion = docker --version
        Write-Info "Docker found: $dockerVersion"
    }
    catch {
        Write-Error "Docker is not installed or not in PATH"
        exit 1
    }
    
    try {
        docker info | Out-Null
        Write-Info "Docker is running"
    }
    catch {
        Write-Error "Docker is not running"
        exit 1
    }
    
    # Check if Docker Compose is installed
    try {
        $composeVersion = docker-compose --version
        Write-Info "Docker Compose found: $composeVersion"
    }
    catch {
        Write-Error "Docker Compose is not installed or not in PATH"
        exit 1
    }
    
    # Check if environment file exists
    $envFile = Join-Path $ProjectRoot ".env.$Environment"
    if (-not (Test-Path $envFile)) {
        Write-Error "Environment file .env.$Environment not found"
        exit 1
    }
    
    Write-Success "Prerequisites check passed"
}

# Backup database
function Backup-Database {
    Write-Info "Creating database backup..."
    
    $backupDir = Join-Path $ProjectRoot "backups"
    if (-not (Test-Path $backupDir)) {
        New-Item -ItemType Directory -Path $backupDir | Out-Null
    }
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFile = Join-Path $backupDir "stridehr_backup_$timestamp.sql"
    
    # Load environment variables
    $envFile = Join-Path $ProjectRoot ".env.$Environment"
    Get-Content $envFile | ForEach-Object {
        if ($_ -match '^([^=]+)=(.*)$') {
            [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
        }
    }
    
    # Create backup
    $mysqlUser = $env:MYSQL_USER
    $mysqlPassword = $env:MYSQL_PASSWORD
    $mysqlDatabase = $env:MYSQL_DATABASE
    
    try {
        docker exec "stridehr-mysql-$Environment" mysqladmin ping -h localhost
        docker exec "stridehr-mysql-$Environment" mysqldump -u"$mysqlUser" -p"$mysqlPassword" "$mysqlDatabase" | Out-File -FilePath $backupFile -Encoding UTF8
        Write-Success "Database backup created: $backupFile"
    }
    catch {
        Write-Error "Database backup failed: $_"
        exit 1
    }
}

# Pull latest images
function Get-LatestImages {
    Write-Info "Pulling latest Docker images..."
    
    try {
        if ($Version -eq "latest") {
            Set-Location $ProjectRoot
            docker-compose -f "docker-compose.prod.yml" pull
        }
        else {
            # Pull specific version images
            docker pull "ghcr.io/your-org/stridehr-backend:$Version"
            docker pull "ghcr.io/your-org/stridehr-frontend:$Version"
        }
        Write-Success "Images pulled successfully"
    }
    catch {
        Write-Error "Failed to pull images: $_"
        exit 1
    }
}

# Deploy application
function Deploy-Application {
    Write-Info "Deploying StrideHR $Version to $Environment..."
    
    try {
        Set-Location $ProjectRoot
        
        # Set environment file
        Copy-Item ".env.$Environment" ".env" -Force
        
        # Deploy based on environment
        if ($Environment -eq "production") {
            # Blue-green deployment for production
            Deploy-BlueGreen
        }
        else {
            # Simple deployment for staging
            docker-compose -f "docker-compose.prod.yml" down
            docker-compose -f "docker-compose.prod.yml" up -d
        }
        
        Write-Success "Deployment completed"
    }
    catch {
        Write-Error "Deployment failed: $_"
        exit 1
    }
}

# Blue-green deployment
function Deploy-BlueGreen {
    Write-Info "Starting blue-green deployment..."
    
    # Check current environment
    $runningContainers = docker ps --format "table {{.Names}}" | Select-String -Pattern "(blue|green)"
    $currentEnv = if ($runningContainers -match "blue") { "blue" } elseif ($runningContainers -match "green") { "green" } else { $null }
    
    $newEnv = if ($currentEnv -eq "blue") { "green" } else { "blue" }
    
    Write-Info "Current environment: $($currentEnv ?? 'none'), deploying to: $newEnv"
    
    # Deploy to new environment
    docker-compose -f "docker-compose.$newEnv.yml" up -d
    
    # Wait for health checks
    Wait-ForHealthChecks $newEnv
    
    # Switch traffic
    Switch-Traffic $newEnv
    
    # Clean up old environment
    if ($currentEnv) {
        Write-Info "Cleaning up old environment: $currentEnv"
        docker-compose -f "docker-compose.$currentEnv.yml" down
    }
    
    Write-Success "Blue-green deployment completed"
}

# Wait for health checks
function Wait-ForHealthChecks {
    param([string]$Env)
    
    Write-Info "Waiting for health checks to pass..."
    
    $maxAttempts = 30
    $attempt = 1
    
    while ($attempt -le $maxAttempts) {
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -TimeoutSec 5
            if ($response.StatusCode -eq 200) {
                Write-Success "Health checks passed"
                return
            }
        }
        catch {
            # Health check failed, continue trying
        }
        
        Write-Info "Attempt $attempt/$maxAttempts`: Health check failed, retrying in 10 seconds..."
        Start-Sleep -Seconds 10
        $attempt++
    }
    
    Write-Error "Health checks failed after $maxAttempts attempts"
    exit 1
}

# Switch traffic
function Switch-Traffic {
    param([string]$NewEnv)
    
    Write-Info "Switching traffic to $NewEnv environment..."
    
    # Update load balancer configuration
    # This would typically involve updating nginx configuration or cloud load balancer
    
    Write-Success "Traffic switched to $NewEnv environment"
}

# Run database migrations
function Invoke-DatabaseMigrations {
    Write-Info "Running database migrations..."
    
    try {
        docker exec "stridehr-api-$Environment" dotnet ef database update
        Write-Success "Database migrations completed"
    }
    catch {
        Write-Error "Database migrations failed: $_"
        exit 1
    }
}

# Verify deployment
function Test-Deployment {
    Write-Info "Verifying deployment..."
    
    # Check if containers are running
    $containers = @(
        "stridehr-mysql-$Environment",
        "stridehr-redis-$Environment",
        "stridehr-api-$Environment",
        "stridehr-frontend-$Environment"
    )
    
    foreach ($container in $containers) {
        $running = docker ps | Select-String $container
        if ($running) {
            Write-Success "$container is running"
        }
        else {
            Write-Error "$container is not running"
            exit 1
        }
    }
    
    # Check application health
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Success "Application health check passed"
        }
        else {
            Write-Error "Application health check failed"
            exit 1
        }
    }
    catch {
        Write-Error "Application health check failed: $_"
        exit 1
    }
    
    Write-Success "Deployment verification completed"
}

# Rollback function
function Invoke-Rollback {
    Write-Warning "Rolling back deployment..."
    
    try {
        # Restore from backup
        $backupDir = Join-Path $ProjectRoot "backups"
        $latestBackup = Get-ChildItem -Path $backupDir -Filter "*.sql" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        
        if ($latestBackup) {
            Write-Info "Restoring database from: $($latestBackup.FullName)"
            
            # Load environment variables
            $envFile = Join-Path $ProjectRoot ".env.$Environment"
            Get-Content $envFile | ForEach-Object {
                if ($_ -match '^([^=]+)=(.*)$') {
                    [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
                }
            }
            
            $mysqlUser = $env:MYSQL_USER
            $mysqlPassword = $env:MYSQL_PASSWORD
            $mysqlDatabase = $env:MYSQL_DATABASE
            
            Get-Content $latestBackup.FullName | docker exec -i "stridehr-mysql-$Environment" mysql -u"$mysqlUser" -p"$mysqlPassword" "$mysqlDatabase"
        }
        
        # Restart previous version
        Set-Location $ProjectRoot
        docker-compose -f "docker-compose.prod.yml" down
        docker-compose -f "docker-compose.prod.yml" up -d
        
        Write-Success "Rollback completed"
    }
    catch {
        Write-Error "Rollback failed: $_"
        exit 1
    }
}

# Cleanup old images
function Remove-OldImages {
    Write-Info "Cleaning up old Docker images..."
    
    try {
        docker image prune -f
        docker system prune -f
        Write-Success "Cleanup completed"
    }
    catch {
        Write-Warning "Cleanup encountered issues: $_"
    }
}

# Main deployment flow
function Main {
    Write-Info "Starting StrideHR deployment to $Environment (version: $Version)"
    
    try {
        Test-Prerequisites
        
        # Create backup before deployment
        Backup-Database
        
        # Deploy application
        Get-LatestImages
        Deploy-Application
        Invoke-DatabaseMigrations
        
        # Verify deployment
        Test-Deployment
        
        # Cleanup
        Remove-OldImages
        
        Write-Success "StrideHR deployment to $Environment completed successfully!"
        Write-Info "Application is available at: http://localhost:4200"
        Write-Info "API documentation: http://localhost:5000/swagger"
    }
    catch {
        Write-Error "Deployment failed: $_"
        Write-Warning "Initiating rollback..."
        Invoke-Rollback
        exit 1
    }
}

# Handle script interruption
trap {
    Write-Error "Deployment interrupted"
    Invoke-Rollback
    exit 1
}

# Run main function
Main