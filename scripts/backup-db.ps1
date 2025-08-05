# StrideHR Database Backup Script
# Usage: .\backup-db.ps1 -Environment production

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("development", "staging", "production")]
    [string]$Environment = "production",
    
    [Parameter(Mandatory=$false)]
    [string]$BackupPath = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$Compress
)

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$ErrorActionPreference = "Stop"

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Backup-Database {
    Write-Info "Starting database backup for $Environment environment..."
    
    # Set backup directory
    if ([string]::IsNullOrEmpty($BackupPath)) {
        $backupDir = Join-Path $ProjectRoot "backups"
    } else {
        $backupDir = $BackupPath
    }
    
    if (-not (Test-Path $backupDir)) {
        New-Item -ItemType Directory -Path $backupDir | Out-Null
        Write-Info "Created backup directory: $backupDir"
    }
    
    # Load environment variables
    $envFile = Join-Path $ProjectRoot ".env.$Environment"
    if (-not (Test-Path $envFile)) {
        Write-Error "Environment file not found: $envFile"
        exit 1
    }
    
    Get-Content $envFile | ForEach-Object {
        if ($_ -match '^([^=]+)=(.*)$') {
            [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
        }
    }
    
    $mysqlUser = $env:MYSQL_USER
    $mysqlPassword = $env:MYSQL_PASSWORD
    $mysqlDatabase = $env:MYSQL_DATABASE
    
    if ([string]::IsNullOrEmpty($mysqlUser) -or [string]::IsNullOrEmpty($mysqlPassword) -or [string]::IsNullOrEmpty($mysqlDatabase)) {
        Write-Error "Database credentials not found in environment file"
        exit 1
    }
    
    # Create backup filename
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFileName = "stridehr_${Environment}_backup_$timestamp.sql"
    $backupFile = Join-Path $backupDir $backupFileName
    
    try {
        # Test database connection
        Write-Info "Testing database connection..."
        docker exec "stridehr-mysql-$Environment" mysqladmin ping -h localhost -u"$mysqlUser" -p"$mysqlPassword"
        Write-Success "Database connection successful"
        
        # Create backup
        Write-Info "Creating database backup..."
        docker exec "stridehr-mysql-$Environment" mysqldump `
            --single-transaction `
            --routines `
            --triggers `
            --events `
            --add-drop-database `
            --databases `
            -u"$mysqlUser" `
            -p"$mysqlPassword" `
            "$mysqlDatabase" | Out-File -FilePath $backupFile -Encoding UTF8
        
        Write-Success "Database backup created: $backupFile"
        
        # Get backup file size
        $fileSize = (Get-Item $backupFile).Length
        $fileSizeMB = [math]::Round($fileSize / 1MB, 2)
        Write-Info "Backup file size: $fileSizeMB MB"
        
        # Compress if requested
        if ($Compress) {
            Write-Info "Compressing backup file..."
            $compressedFile = "$backupFile.zip"
            Compress-Archive -Path $backupFile -DestinationPath $compressedFile
            Remove-Item $backupFile
            
            $compressedSize = (Get-Item $compressedFile).Length
            $compressedSizeMB = [math]::Round($compressedSize / 1MB, 2)
            $compressionRatio = [math]::Round((1 - ($compressedSize / $fileSize)) * 100, 1)
            
            Write-Success "Backup compressed: $compressedFile"
            Write-Info "Compressed size: $compressedSizeMB MB (${compressionRatio}% reduction)"
        }
        
        # Clean up old backups (keep last 7 days)
        Write-Info "Cleaning up old backups..."
        $cutoffDate = (Get-Date).AddDays(-7)
        $oldBackups = Get-ChildItem -Path $backupDir -Filter "stridehr_${Environment}_backup_*.sql*" | Where-Object { $_.LastWriteTime -lt $cutoffDate }
        
        foreach ($oldBackup in $oldBackups) {
            Remove-Item $oldBackup.FullName
            Write-Info "Removed old backup: $($oldBackup.Name)"
        }
        
        Write-Success "Database backup completed successfully!"
        
    }
    catch {
        Write-Error "Database backup failed: $_"
        exit 1
    }
}

# Main execution
Backup-Database