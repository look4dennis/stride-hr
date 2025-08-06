#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Database restore script for StrideHR production
.DESCRIPTION
    This script restores MySQL database from backup files with
    support for compressed and encrypted backups.
.PARAMETER BackupFile
    Path to the backup file to restore
.PARAMETER RestoreDatabase
    Target database name (default: from environment)
.PARAMETER CreateDatabase
    Create database if it doesn't exist (default: true)
.PARAMETER DropExisting
    Drop existing database before restore (default: false)
.PARAMETER PointInTime
    Restore to specific point in time (for incremental backups)
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$BackupFile,
    [string]$RestoreDatabase = "",
    [bool]$CreateDatabase = $true,
    [bool]$DropExisting = $false,
    [string]$PointInTime = ""
)

$ErrorActionPreference = "Stop"

# Configuration
$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
$PROJECT_ROOT = Split-Path -Parent $SCRIPT_DIR
$ENV_FILE = "$PROJECT_ROOT/.env.production"
$LOG_FILE = "$PROJECT_ROOT/logs/restore.log"
$TEMP_DIR = "$PROJECT_ROOT/temp/restore"

# Load environment variables
if (Test-Path $ENV_FILE) {
    Get-Content $ENV_FILE | ForEach-Object {
        if ($_ -match '^([^#][^=]+)=(.*)$') {
            [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
        }
    }
}

# Database configuration
$DB_HOST = $env:DB_HOST ?? "localhost"
$DB_NAME = if ($RestoreDatabase) { $RestoreDatabase } else { $env:DB_NAME ?? "StrideHR" }
$DB_USER = $env:DB_USER ?? "root"
$DB_PASSWORD = $env:DB_PASSWORD
$DB_PORT = $env:DB_PORT ?? "3306"

# Backup configuration
$BACKUP_ENCRYPTION_KEY = $env:BACKUP_ENCRYPTION_KEY

# Function to write log messages
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    Write-Host $logMessage
    
    if (-not (Test-Path (Split-Path $LOG_FILE))) {
        New-Item -ItemType Directory -Path (Split-Path $LOG_FILE) -Force | Out-Null
    }
    $logMessage | Out-File -FilePath $LOG_FILE -Append -Encoding UTF8
}

# Function to validate prerequisites
function Test-RestorePrerequisites {
    Write-Log "Validating restore prerequisites..."
    
    # Check backup file exists
    if (-not (Test-Path $BackupFile)) {
        Write-Log "Backup file not found: $BackupFile" "ERROR"
        exit 1
    }
    
    # Check mysql client
    try {
        $null = Get-Command "mysql" -ErrorAction Stop
        Write-Log "MySQL client found"
    }
    catch {
        Write-Log "MySQL client not found. Please install MySQL client tools." "ERROR"
        exit 1
    }
    
    # Check database connectivity
    try {
        $testQuery = "SELECT 1"
        $connectionString = "mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e `"$testQuery`""
        Invoke-Expression $connectionString > $null 2>&1
        Write-Log "Database connectivity verified"
    }
    catch {
        Write-Log "Failed to connect to database: $_" "ERROR"
        exit 1
    }
    
    # Create temp directory
    if (-not (Test-Path $TEMP_DIR)) {
        New-Item -ItemType Directory -Path $TEMP_DIR -Force | Out-Null
        Write-Log "Created temp directory: $TEMP_DIR"
    }
    
    Write-Log "Prerequisites validation completed"
}

# Function to decrypt backup file
function Unprotect-Backup {
    param([string]$BackupPath)
    
    if ($BackupPath -notmatch "\.enc$") {
        return $BackupPath
    }
    
    Write-Log "Decrypting backup file..."
    
    try {
        $decryptedPath = $BackupPath -replace "\.enc$", ""
        
        # Use OpenSSL for decryption if available
        if (Get-Command "openssl" -ErrorAction SilentlyContinue) {
            $opensslCmd = "openssl enc -aes-256-cbc -d -in `"$BackupPath`" -out `"$decryptedPath`" -pass pass:$BACKUP_ENCRYPTION_KEY"
            Invoke-Expression $opensslCmd
        }
        else {
            # Fallback to .NET decryption
            $key = [System.Text.Encoding]::UTF8.GetBytes($BACKUP_ENCRYPTION_KEY.PadRight(32).Substring(0, 32))
            $encryptedBytes = [System.IO.File]::ReadAllBytes($BackupPath)
            
            # Extract IV from the beginning
            $iv = $encryptedBytes[0..15]
            $cipherText = $encryptedBytes[16..($encryptedBytes.Length - 1)]
            
            $aes = [System.Security.Cryptography.Aes]::Create()
            $aes.Key = $key
            $aes.IV = $iv
            
            $decryptor = $aes.CreateDecryptor()
            $decryptedBytes = $decryptor.TransformFinalBlock($cipherText, 0, $cipherText.Length)
            
            [System.IO.File]::WriteAllBytes($decryptedPath, $decryptedBytes)
        }
        
        Write-Log "Backup decrypted successfully"
        return $decryptedPath
    }
    catch {
        Write-Log "Failed to decrypt backup: $_" "ERROR"
        throw
    }
}

# Function to decompress backup file
function Expand-Backup {
    param([string]$BackupPath)
    
    if ($BackupPath -notmatch "\.(gz|zip)$") {
        return $BackupPath
    }
    
    Write-Log "Decompressing backup file..."
    
    try {
        if ($BackupPath -match "\.gz$") {
            $decompressedPath = $BackupPath -replace "\.gz$", ""
            
            # Use gunzip for decompression
            if (Get-Command "gunzip" -ErrorAction SilentlyContinue) {
                gunzip -c "$BackupPath" > "$decompressedPath"
            }
            else {
                # Fallback to PowerShell decompression
                $compressed = [System.IO.File]::OpenRead($BackupPath)
                $gzip = [System.IO.Compression.GzipStream]::new($compressed, [System.IO.Compression.CompressionMode]::Decompress)
                $output = [System.IO.File]::Create($decompressedPath)
                $gzip.CopyTo($output)
                $output.Close()
                $gzip.Close()
                $compressed.Close()
            }
        }
        elseif ($BackupPath -match "\.zip$") {
            $decompressedPath = $BackupPath -replace "\.zip$", ""
            Expand-Archive -Path $BackupPath -DestinationPath (Split-Path $decompressedPath) -Force
        }
        
        Write-Log "Backup decompressed successfully"
        return $decompressedPath
    }
    catch {
        Write-Log "Failed to decompress backup: $_" "ERROR"
        throw
    }
}

# Function to prepare database for restore
function Initialize-DatabaseForRestore {
    Write-Log "Preparing database for restore..."
    
    try {
        # Drop existing database if requested
        if ($DropExisting) {
            Write-Log "Dropping existing database: $DB_NAME"
            $dropCmd = "mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e `"DROP DATABASE IF EXISTS \`$DB_NAME\`;`""
            Invoke-Expression $dropCmd
        }
        
        # Create database if requested
        if ($CreateDatabase) {
            Write-Log "Creating database: $DB_NAME"
            $createCmd = "mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e `"CREATE DATABASE IF NOT EXISTS \`$DB_NAME\` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;`""
            Invoke-Expression $createCmd
        }
        
        Write-Log "Database preparation completed"
    }
    catch {
        Write-Log "Failed to prepare database: $_" "ERROR"
        throw
    }
}

# Function to restore database from backup
function Restore-DatabaseFromBackup {
    param([string]$BackupPath)
    
    Write-Log "Starting database restore from: $BackupPath"
    
    try {
        # Validate backup file
        if (-not (Test-Path $BackupPath)) {
            throw "Processed backup file not found: $BackupPath"
        }
        
        $backupSize = (Get-Item $BackupPath).Length
        Write-Log "Backup file size: $([math]::Round($backupSize / 1MB, 2)) MB"
        
        # Create restore command
        $restoreCmd = "mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD $DB_NAME < `"$BackupPath`""
        
        Write-Log "Executing database restore..."
        $startTime = Get-Date
        
        # Execute restore
        Invoke-Expression $restoreCmd
        
        $endTime = Get-Date
        $duration = $endTime - $startTime
        
        Write-Log "Database restore completed successfully"
        Write-Log "Restore duration: $($duration.TotalMinutes.ToString('F2')) minutes"
        
        # Verify restore
        Test-RestoreIntegrity
        
    }
    catch {
        Write-Log "Failed to restore database: $_" "ERROR"
        throw
    }
}

# Function to verify restore integrity
function Test-RestoreIntegrity {
    Write-Log "Verifying restore integrity..."
    
    try {
        # Check if database exists and has tables
        $tablesQuery = "SELECT COUNT(*) as table_count FROM information_schema.tables WHERE table_schema = '$DB_NAME'"
        $tablesCmd = "mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e `"$tablesQuery`" -s -N"
        $tableCount = Invoke-Expression $tablesCmd
        
        if ([int]$tableCount -eq 0) {
            throw "No tables found in restored database"
        }
        
        Write-Log "Found $tableCount tables in restored database"
        
        # Check for critical tables (adjust based on your schema)
        $criticalTables = @("Users", "Organizations", "Branches", "Employees")
        foreach ($table in $criticalTables) {
            $checkQuery = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '$DB_NAME' AND table_name = '$table'"
            $checkCmd = "mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e `"$checkQuery`" -s -N"
            $exists = Invoke-Expression $checkCmd
            
            if ([int]$exists -eq 0) {
                Write-Log "Critical table '$table' not found in restored database" "WARNING"
            }
            else {
                Write-Log "Critical table '$table' verified"
            }
        }
        
        Write-Log "Restore integrity verification completed"
    }
    catch {
        Write-Log "Restore integrity verification failed: $_" "ERROR"
        throw
    }
}

# Function to clean up temporary files
function Remove-TempFiles {
    Write-Log "Cleaning up temporary files..."
    
    try {
        if (Test-Path $TEMP_DIR) {
            Get-ChildItem -Path $TEMP_DIR -Recurse | Remove-Item -Force -Recurse
            Write-Log "Temporary files cleaned up"
        }
    }
    catch {
        Write-Log "Failed to clean up temporary files: $_" "WARNING"
    }
}

# Function to create post-restore report
function New-RestoreReport {
    param([string]$BackupFile, [datetime]$StartTime, [datetime]$EndTime)
    
    $reportPath = "$PROJECT_ROOT/logs/restore-report-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
    
    $report = @"
StrideHR Database Restore Report
================================

Restore Details:
- Backup File: $BackupFile
- Target Database: $DB_NAME
- Database Host: $DB_HOST
- Start Time: $($StartTime.ToString('yyyy-MM-dd HH:mm:ss'))
- End Time: $($EndTime.ToString('yyyy-MM-dd HH:mm:ss'))
- Duration: $($($EndTime - $StartTime).TotalMinutes.ToString('F2')) minutes

Configuration:
- Create Database: $CreateDatabase
- Drop Existing: $DropExisting
- Point in Time: $PointInTime

Status: SUCCESS

Next Steps:
1. Verify application connectivity
2. Run application health checks
3. Test critical business functions
4. Update monitoring systems if needed

Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
"@
    
    $report | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Log "Restore report created: $reportPath"
}

# Main restore process
$startTime = Get-Date

try {
    Write-Log "Starting StrideHR database restore process"
    Write-Log "Backup file: $BackupFile"
    Write-Log "Target database: $DB_NAME"
    Write-Log "Create database: $CreateDatabase, Drop existing: $DropExisting"
    
    # Validate prerequisites
    Test-RestorePrerequisites
    
    # Process backup file (decrypt and decompress if needed)
    $processedBackupPath = $BackupFile
    
    # Copy to temp directory for processing
    $tempBackupPath = "$TEMP_DIR/$(Split-Path $BackupFile -Leaf)"
    Copy-Item $BackupFile $tempBackupPath -Force
    $processedBackupPath = $tempBackupPath
    
    # Decrypt if encrypted
    $processedBackupPath = Unprotect-Backup $processedBackupPath
    
    # Decompress if compressed
    $processedBackupPath = Expand-Backup $processedBackupPath
    
    # Prepare database
    Initialize-DatabaseForRestore
    
    # Restore database
    Restore-DatabaseFromBackup $processedBackupPath
    
    $endTime = Get-Date
    
    # Create restore report
    New-RestoreReport $BackupFile $startTime $endTime
    
    Write-Log "Database restore process completed successfully"
    Write-Log "Total duration: $($($endTime - $startTime).TotalMinutes.ToString('F2')) minutes"
    
    exit 0
}
catch {
    $endTime = Get-Date
    Write-Log "Database restore process failed: $_" "ERROR"
    Write-Log "Total duration: $($($endTime - $startTime).TotalMinutes.ToString('F2')) minutes"
    exit 1
}
finally {
    # Clean up temporary files
    Remove-TempFiles
}