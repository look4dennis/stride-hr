#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Database backup script for StrideHR production
.DESCRIPTION
    This script creates automated backups of the MySQL database with
    compression, encryption, and cloud storage support.
.PARAMETER BackupType
    Type of backup: full, incremental, or differential (default: full)
.PARAMETER Compress
    Compress the backup file (default: true)
.PARAMETER Encrypt
    Encrypt the backup file (default: true)
.PARAMETER UploadToCloud
    Upload backup to cloud storage (default: true)
.PARAMETER RetentionDays
    Number of days to retain backups (default: 30)
#>

param(
    [ValidateSet("full", "incremental", "differential")]
    [string]$BackupType = "full",
    [bool]$Compress = $true,
    [bool]$Encrypt = $true,
    [bool]$UploadToCloud = $true,
    [int]$RetentionDays = 30
)

$ErrorActionPreference = "Stop"

# Configuration
$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
$PROJECT_ROOT = Split-Path -Parent $SCRIPT_DIR
$ENV_FILE = "$PROJECT_ROOT/.env.production"
$BACKUP_DIR = "$PROJECT_ROOT/backups"
$LOG_FILE = "$PROJECT_ROOT/logs/backup.log"

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
$DB_NAME = $env:DB_NAME ?? "StrideHR"
$DB_USER = $env:DB_USER ?? "root"
$DB_PASSWORD = $env:DB_PASSWORD
$DB_PORT = $env:DB_PORT ?? "3306"

# Backup configuration
$BACKUP_ENCRYPTION_KEY = $env:BACKUP_ENCRYPTION_KEY
$AWS_ACCESS_KEY_ID = $env:AWS_ACCESS_KEY_ID
$AWS_SECRET_ACCESS_KEY = $env:AWS_SECRET_ACCESS_KEY
$AWS_REGION = $env:AWS_REGION ?? "us-east-1"
$S3_BUCKET = $env:BACKUP_S3_BUCKET

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
function Test-BackupPrerequisites {
    Write-Log "Validating backup prerequisites..."
    
    # Check backup directory
    if (-not (Test-Path $BACKUP_DIR)) {
        New-Item -ItemType Directory -Path $BACKUP_DIR -Force | Out-Null
        Write-Log "Created backup directory: $BACKUP_DIR"
    }
    
    # Check mysqldump
    try {
        $null = Get-Command "mysqldump" -ErrorAction Stop
        Write-Log "mysqldump found"
    }
    catch {
        Write-Log "mysqldump not found. Please install MySQL client tools." "ERROR"
        exit 1
    }
    
    # Check database connectivity
    try {
        $testQuery = "SELECT 1"
        $connectionString = "mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e `"$testQuery`" $DB_NAME"
        Invoke-Expression $connectionString > $null 2>&1
        Write-Log "Database connectivity verified"
    }
    catch {
        Write-Log "Failed to connect to database: $_" "ERROR"
        exit 1
    }
    
    # Check encryption key if encryption is enabled
    if ($Encrypt -and [string]::IsNullOrEmpty($BACKUP_ENCRYPTION_KEY)) {
        Write-Log "Encryption enabled but BACKUP_ENCRYPTION_KEY not set" "ERROR"
        exit 1
    }
    
    # Check AWS credentials if cloud upload is enabled
    if ($UploadToCloud) {
        if ([string]::IsNullOrEmpty($AWS_ACCESS_KEY_ID) -or [string]::IsNullOrEmpty($AWS_SECRET_ACCESS_KEY)) {
            Write-Log "Cloud upload enabled but AWS credentials not set" "ERROR"
            exit 1
        }
        
        if ([string]::IsNullOrEmpty($S3_BUCKET)) {
            Write-Log "Cloud upload enabled but S3_BUCKET not set" "ERROR"
            exit 1
        }
    }
    
    Write-Log "Prerequisites validation completed"
}

# Function to create database backup
function New-DatabaseBackup {
    Write-Log "Starting $BackupType database backup..."
    
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $backupFileName = "stridehr-$BackupType-$timestamp.sql"
    $backupPath = "$BACKUP_DIR/$backupFileName"
    
    try {
        # Create mysqldump command based on backup type
        $mysqldumpArgs = @(
            "-h", $DB_HOST,
            "-P", $DB_PORT,
            "-u", $DB_USER,
            "-p$DB_PASSWORD",
            "--single-transaction",
            "--routines",
            "--triggers",
            "--events",
            "--add-drop-database",
            "--add-drop-table",
            "--create-options",
            "--disable-keys",
            "--extended-insert",
            "--quick",
            "--lock-tables=false"
        )
        
        if ($BackupType -eq "full") {
            $mysqldumpArgs += @("--databases", $DB_NAME)
        }
        elseif ($BackupType -eq "incremental") {
            # For incremental backups, we'll use binary log position
            $mysqldumpArgs += @("--master-data=2", "--databases", $DB_NAME)
        }
        
        # Execute mysqldump
        Write-Log "Creating database dump: $backupPath"
        $mysqldumpCmd = "mysqldump " + ($mysqldumpArgs -join " ") + " > `"$backupPath`""
        Invoke-Expression $mysqldumpCmd
        
        if (-not (Test-Path $backupPath)) {
            throw "Backup file was not created"
        }
        
        $backupSize = (Get-Item $backupPath).Length
        Write-Log "Database backup created successfully. Size: $([math]::Round($backupSize / 1MB, 2)) MB"
        
        return $backupPath
    }
    catch {
        Write-Log "Failed to create database backup: $_" "ERROR"
        throw
    }
}

# Function to compress backup
function Compress-Backup {
    param([string]$BackupPath)
    
    if (-not $Compress) {
        return $BackupPath
    }
    
    Write-Log "Compressing backup file..."
    
    try {
        $compressedPath = "$BackupPath.gz"
        
        # Use gzip for compression
        if (Get-Command "gzip" -ErrorAction SilentlyContinue) {
            gzip -9 "$BackupPath"
            $finalPath = $compressedPath
        }
        else {
            # Fallback to PowerShell compression
            $bytes = [System.IO.File]::ReadAllBytes($BackupPath)
            $compressed = [System.IO.Compression.GzipStream]::new(
                [System.IO.File]::Create($compressedPath),
                [System.IO.Compression.CompressionMode]::Compress
            )
            $compressed.Write($bytes, 0, $bytes.Length)
            $compressed.Close()
            Remove-Item $BackupPath -Force
            $finalPath = $compressedPath
        }
        
        $originalSize = (Get-Item $BackupPath -ErrorAction SilentlyContinue)?.Length ?? 0
        $compressedSize = (Get-Item $finalPath).Length
        $compressionRatio = if ($originalSize -gt 0) { [math]::Round((1 - $compressedSize / $originalSize) * 100, 2) } else { 0 }
        
        Write-Log "Backup compressed successfully. Compression ratio: $compressionRatio%"
        return $finalPath
    }
    catch {
        Write-Log "Failed to compress backup: $_" "ERROR"
        throw
    }
}

# Function to encrypt backup
function Protect-Backup {
    param([string]$BackupPath)
    
    if (-not $Encrypt) {
        return $BackupPath
    }
    
    Write-Log "Encrypting backup file..."
    
    try {
        $encryptedPath = "$BackupPath.enc"
        
        # Use OpenSSL for encryption if available
        if (Get-Command "openssl" -ErrorAction SilentlyContinue) {
            $opensslCmd = "openssl enc -aes-256-cbc -salt -in `"$BackupPath`" -out `"$encryptedPath`" -pass pass:$BACKUP_ENCRYPTION_KEY"
            Invoke-Expression $opensslCmd
            Remove-Item $BackupPath -Force
        }
        else {
            # Fallback to .NET encryption
            $key = [System.Text.Encoding]::UTF8.GetBytes($BACKUP_ENCRYPTION_KEY.PadRight(32).Substring(0, 32))
            $iv = [System.Security.Cryptography.RNGCryptoServiceProvider]::new().GetBytes(16)
            
            $aes = [System.Security.Cryptography.Aes]::Create()
            $aes.Key = $key
            $aes.IV = $iv
            
            $encryptor = $aes.CreateEncryptor()
            $inputBytes = [System.IO.File]::ReadAllBytes($BackupPath)
            $encryptedBytes = $encryptor.TransformFinalBlock($inputBytes, 0, $inputBytes.Length)
            
            # Prepend IV to encrypted data
            $finalBytes = $iv + $encryptedBytes
            [System.IO.File]::WriteAllBytes($encryptedPath, $finalBytes)
            
            Remove-Item $BackupPath -Force
        }
        
        Write-Log "Backup encrypted successfully"
        return $encryptedPath
    }
    catch {
        Write-Log "Failed to encrypt backup: $_" "ERROR"
        throw
    }
}

# Function to upload backup to cloud storage
function Send-BackupToCloud {
    param([string]$BackupPath)
    
    if (-not $UploadToCloud) {
        return
    }
    
    Write-Log "Uploading backup to cloud storage..."
    
    try {
        $fileName = Split-Path $BackupPath -Leaf
        $s3Key = "stridehr-backups/$(Get-Date -Format 'yyyy/MM/dd')/$fileName"
        
        # Use AWS CLI if available
        if (Get-Command "aws" -ErrorAction SilentlyContinue) {
            $env:AWS_ACCESS_KEY_ID = $AWS_ACCESS_KEY_ID
            $env:AWS_SECRET_ACCESS_KEY = $AWS_SECRET_ACCESS_KEY
            $env:AWS_DEFAULT_REGION = $AWS_REGION
            
            aws s3 cp "$BackupPath" "s3://$S3_BUCKET/$s3Key" --storage-class STANDARD_IA
            Write-Log "Backup uploaded to S3: s3://$S3_BUCKET/$s3Key"
        }
        else {
            Write-Log "AWS CLI not found. Skipping cloud upload." "WARNING"
        }
    }
    catch {
        Write-Log "Failed to upload backup to cloud: $_" "ERROR"
        # Don't throw here as local backup is still valid
    }
}

# Function to clean up old backups
function Remove-OldBackups {
    Write-Log "Cleaning up old backups (retention: $RetentionDays days)..."
    
    try {
        $cutoffDate = (Get-Date).AddDays(-$RetentionDays)
        $oldBackups = Get-ChildItem -Path $BACKUP_DIR -Filter "stridehr-*" | Where-Object { $_.CreationTime -lt $cutoffDate }
        
        foreach ($backup in $oldBackups) {
            Remove-Item $backup.FullName -Force
            Write-Log "Removed old backup: $($backup.Name)"
        }
        
        Write-Log "Cleanup completed. Removed $($oldBackups.Count) old backup(s)"
    }
    catch {
        Write-Log "Failed to clean up old backups: $_" "ERROR"
    }
}

# Function to verify backup integrity
function Test-BackupIntegrity {
    param([string]$BackupPath)
    
    Write-Log "Verifying backup integrity..."
    
    try {
        # Basic file existence and size check
        if (-not (Test-Path $BackupPath)) {
            throw "Backup file does not exist"
        }
        
        $fileSize = (Get-Item $BackupPath).Length
        if ($fileSize -eq 0) {
            throw "Backup file is empty"
        }
        
        # For SQL files, check for basic structure
        if ($BackupPath -match "\.sql$") {
            $content = Get-Content $BackupPath -TotalCount 10
            if ($content -notmatch "-- MySQL dump|CREATE DATABASE|USE `") {
                throw "Backup file does not appear to be a valid MySQL dump"
            }
        }
        
        Write-Log "Backup integrity verification passed"
        return $true
    }
    catch {
        Write-Log "Backup integrity verification failed: $_" "ERROR"
        return $false
    }
}

# Main backup process
try {
    Write-Log "Starting StrideHR database backup process"
    Write-Log "Backup type: $BackupType, Compress: $Compress, Encrypt: $Encrypt, Upload: $UploadToCloud"
    
    # Validate prerequisites
    Test-BackupPrerequisites
    
    # Create database backup
    $backupPath = New-DatabaseBackup
    
    # Verify backup integrity
    if (-not (Test-BackupIntegrity $backupPath)) {
        throw "Backup integrity verification failed"
    }
    
    # Compress backup if requested
    $backupPath = Compress-Backup $backupPath
    
    # Encrypt backup if requested
    $backupPath = Protect-Backup $backupPath
    
    # Upload to cloud storage if requested
    Send-BackupToCloud $backupPath
    
    # Clean up old backups
    Remove-OldBackups
    
    $finalSize = (Get-Item $backupPath).Length
    Write-Log "Backup process completed successfully"
    Write-Log "Final backup file: $backupPath"
    Write-Log "Final backup size: $([math]::Round($finalSize / 1MB, 2)) MB"
    
    exit 0
}
catch {
    Write-Log "Backup process failed: $_" "ERROR"
    exit 1
}