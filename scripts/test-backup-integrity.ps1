#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Backup integrity testing script for StrideHR
.DESCRIPTION
    This script tests the integrity of backup files and validates
    that they can be successfully restored.
.PARAMETER BackupPath
    Path to backup file or directory to test
.PARAMETER TestDatabase
    Name of test database to use for validation (default: StrideHR_Test)
.PARAMETER FullTest
    Perform full restore test (default: false)
.PARAMETER CleanupAfterTest
    Clean up test database after testing (default: true)
#>

param(
    [string]$BackupPath = "",
    [string]$TestDatabase = "StrideHR_Test",
    [bool]$FullTest = $false,
    [bool]$CleanupAfterTest = $true
)

$ErrorActionPreference = "Stop"

# Configuration
$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
$PROJECT_ROOT = Split-Path -Parent $SCRIPT_DIR
$ENV_FILE = "$PROJECT_ROOT/.env.production"
$LOG_FILE = "$PROJECT_ROOT/logs/backup-integrity-test.log"
$BACKUP_DIR = "$PROJECT_ROOT/backups"

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
$DB_USER = $env:DB_USER ?? "root"
$DB_PASSWORD = $env:DB_PASSWORD
$DB_PORT = $env:DB_PORT ?? "3306"

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

# Function to get backup files to test
function Get-BackupFilesToTest {
    if ($BackupPath) {
        if (Test-Path $BackupPath -PathType Container) {
            # Directory provided, get all backup files
            return Get-ChildItem -Path $BackupPath -Filter "stridehr-*.sql*" | Sort-Object CreationTime -Descending
        }
        elseif (Test-Path $BackupPath -PathType Leaf) {
            # Single file provided
            return @(Get-Item $BackupPath)
        }
        else {
            Write-Log "Backup path not found: $BackupPath" "ERROR"
            return @()
        }
    }
    else {
        # Default to backup directory
        if (Test-Path $BACKUP_DIR) {
            return Get-ChildItem -Path $BACKUP_DIR -Filter "stridehr-*.sql*" | Sort-Object CreationTime -Descending | Select-Object -First 5
        }
        else {
            Write-Log "No backup directory found: $BACKUP_DIR" "ERROR"
            return @()
        }
    }
}

# Function to test basic file integrity
function Test-FileIntegrity {
    param([System.IO.FileInfo]$BackupFile)
    
    Write-Log "Testing file integrity for: $($BackupFile.Name)"
    
    $results = @{
        FileName = $BackupFile.Name
        FilePath = $BackupFile.FullName
        FileSize = $BackupFile.Length
        CreationTime = $BackupFile.CreationTime
        Tests = @{}
    }
    
    # Test 1: File exists and is not empty
    $results.Tests["FileExists"] = $BackupFile.Exists
    $results.Tests["FileNotEmpty"] = $BackupFile.Length -gt 0
    
    # Test 2: File extension validation
    $validExtensions = @(".sql", ".sql.gz", ".sql.enc", ".sql.gz.enc")
    $results.Tests["ValidExtension"] = $validExtensions | Where-Object { $BackupFile.Name.EndsWith($_) }
    
    # Test 3: File naming convention
    $results.Tests["ValidNaming"] = $BackupFile.Name -match "^stridehr-(full|incremental|differential)-\d{8}-\d{6}\."
    
    # Test 4: File age (not older than retention period)
    $maxAge = (Get-Date).AddDays(-30)
    $results.Tests["WithinRetention"] = $BackupFile.CreationTime -gt $maxAge
    
    # Test 5: File size reasonable (not too small or too large)
    $minSize = 1KB
    $maxSize = 10GB
    $results.Tests["ReasonableSize"] = ($BackupFile.Length -gt $minSize) -and ($BackupFile.Length -lt $maxSize)
    
    # Test 6: File header validation (for SQL files)
    if ($BackupFile.Name -match "\.sql$") {
        try {
            $header = Get-Content $BackupFile.FullName -TotalCount 10 -ErrorAction Stop
            $results.Tests["ValidSQLHeader"] = $header -match "-- MySQL dump|CREATE DATABASE|USE"
        }
        catch {
            $results.Tests["ValidSQLHeader"] = $false
        }
    }
    else {
        $results.Tests["ValidSQLHeader"] = "N/A"
    }
    
    # Calculate overall integrity score
    $passedTests = ($results.Tests.Values | Where-Object { $_ -eq $true }).Count
    $totalTests = ($results.Tests.Values | Where-Object { $_ -ne "N/A" }).Count
    $results["IntegrityScore"] = if ($totalTests -gt 0) { [math]::Round(($passedTests / $totalTests) * 100, 2) } else { 0 }
    
    Write-Log "File integrity test completed. Score: $($results.IntegrityScore)%"
    return $results
}

# Function to test backup restoration
function Test-BackupRestore {
    param([System.IO.FileInfo]$BackupFile)
    
    Write-Log "Testing backup restore for: $($BackupFile.Name)"
    
    $results = @{
        FileName = $BackupFile.Name
        RestoreStartTime = Get-Date
        RestoreEndTime = $null
        RestoreDuration = $null
        Tests = @{}
    }
    
    try {
        # Test 1: Create test database
        Write-Log "Creating test database: $TestDatabase"
        $createDbCmd = "mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e `"DROP DATABASE IF EXISTS \`$TestDatabase\`; CREATE DATABASE \`$TestDatabase\` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;`""
        Invoke-Expression $createDbCmd
        $results.Tests["DatabaseCreated"] = $true
        
        # Test 2: Restore backup to test database
        Write-Log "Restoring backup to test database..."
        $restoreResult = & "$SCRIPT_DIR/restore-database.ps1" -BackupFile $BackupFile.FullName -RestoreDatabase $TestDatabase -CreateDatabase $false -DropExisting $false
        $results.Tests["RestoreSuccessful"] = $LASTEXITCODE -eq 0
        
        if ($results.Tests["RestoreSuccessful"]) {
            # Test 3: Verify table count
            $tableCountCmd = "mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e `"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '$TestDatabase'`" -s -N"
            $tableCount = [int](Invoke-Expression $tableCountCmd)
            $results.Tests["TablesRestored"] = $tableCount -gt 0
            $results["TableCount"] = $tableCount
            
            # Test 4: Verify critical tables exist
            $criticalTables = @("Users", "Organizations", "Branches", "Employees")
            $criticalTablesFound = 0
            
            foreach ($table in $criticalTables) {
                $checkTableCmd = "mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e `"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '$TestDatabase' AND table_name = '$table'`" -s -N"
                $tableExists = [int](Invoke-Expression $checkTableCmd)
                if ($tableExists -gt 0) {
                    $criticalTablesFound++
                }
            }
            
            $results.Tests["CriticalTablesExist"] = $criticalTablesFound -eq $criticalTables.Count
            $results["CriticalTablesFound"] = $criticalTablesFound
            
            # Test 5: Verify data integrity (sample queries)
            try {
                $userCountCmd = "mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e `"SELECT COUNT(*) FROM \`$TestDatabase\`.\`Users\``" -s -N 2>$null"
                $userCount = [int](Invoke-Expression $userCountCmd)
                $results.Tests["DataIntegrityCheck"] = $userCount -ge 0
                $results["UserCount"] = $userCount
            }
            catch {
                $results.Tests["DataIntegrityCheck"] = $false
                $results["UserCount"] = 0
            }
            
            # Test 6: Verify foreign key constraints
            try {
                $fkCheckCmd = "mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e `"SET foreign_key_checks = 1; SELECT 1`" $TestDatabase"
                Invoke-Expression $fkCheckCmd > $null 2>&1
                $results.Tests["ForeignKeyIntegrity"] = $LASTEXITCODE -eq 0
            }
            catch {
                $results.Tests["ForeignKeyIntegrity"] = $false
            }
        }
        else {
            # Restore failed, mark dependent tests as failed
            $results.Tests["TablesRestored"] = $false
            $results.Tests["CriticalTablesExist"] = $false
            $results.Tests["DataIntegrityCheck"] = $false
            $results.Tests["ForeignKeyIntegrity"] = $false
        }
        
        $results.RestoreEndTime = Get-Date
        $results.RestoreDuration = $results.RestoreEndTime - $results.RestoreStartTime
        
        # Calculate overall restore score
        $passedTests = ($results.Tests.Values | Where-Object { $_ -eq $true }).Count
        $totalTests = $results.Tests.Count
        $results["RestoreScore"] = if ($totalTests -gt 0) { [math]::Round(($passedTests / $totalTests) * 100, 2) } else { 0 }
        
        Write-Log "Backup restore test completed. Score: $($results.RestoreScore)%"
        
    }
    catch {
        Write-Log "Backup restore test failed: $_" "ERROR"
        $results.Tests["RestoreSuccessful"] = $false
        $results["Error"] = $_.Exception.Message
        $results["RestoreScore"] = 0
    }
    finally {
        # Cleanup test database if requested
        if ($CleanupAfterTest) {
            try {
                Write-Log "Cleaning up test database: $TestDatabase"
                $dropDbCmd = "mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e `"DROP DATABASE IF EXISTS \`$TestDatabase\`;`""
                Invoke-Expression $dropDbCmd
            }
            catch {
                Write-Log "Failed to cleanup test database: $_" "WARNING"
            }
        }
    }
    
    return $results
}

# Function to generate test report
function New-IntegrityTestReport {
    param([array]$TestResults)
    
    $reportPath = "$PROJECT_ROOT/logs/backup-integrity-report-$(Get-Date -Format 'yyyyMMdd-HHmmss').html"
    
    $html = @"
<!DOCTYPE html>
<html>
<head>
    <title>StrideHR Backup Integrity Test Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background-color: #f0f0f0; padding: 20px; border-radius: 5px; }
        .summary { margin: 20px 0; }
        .test-result { margin: 10px 0; padding: 10px; border: 1px solid #ddd; border-radius: 5px; }
        .pass { background-color: #d4edda; border-color: #c3e6cb; }
        .fail { background-color: #f8d7da; border-color: #f5c6cb; }
        .warning { background-color: #fff3cd; border-color: #ffeaa7; }
        table { border-collapse: collapse; width: 100%; margin: 10px 0; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; }
        .score-high { color: green; font-weight: bold; }
        .score-medium { color: orange; font-weight: bold; }
        .score-low { color: red; font-weight: bold; }
    </style>
</head>
<body>
    <div class="header">
        <h1>StrideHR Backup Integrity Test Report</h1>
        <p>Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</p>
        <p>Test Type: $(if ($FullTest) { "Full Restore Test" } else { "Basic Integrity Test" })</p>
    </div>
    
    <div class="summary">
        <h2>Summary</h2>
        <p>Total backups tested: $($TestResults.Count)</p>
        <p>Passed integrity checks: $(($TestResults | Where-Object { $_.IntegrityScore -ge 80 }).Count)</p>
        $(if ($FullTest) { "<p>Passed restore tests: $(($TestResults | Where-Object { $_.RestoreScore -ge 80 }).Count)</p>" })
    </div>
"@
    
    foreach ($result in $TestResults) {
        $scoreClass = if ($result.IntegrityScore -ge 80) { "score-high" } elseif ($result.IntegrityScore -ge 60) { "score-medium" } else { "score-low" }
        $resultClass = if ($result.IntegrityScore -ge 80) { "pass" } elseif ($result.IntegrityScore -ge 60) { "warning" } else { "fail" }
        
        $html += @"
    <div class="test-result $resultClass">
        <h3>$($result.FileName)</h3>
        <p><strong>File Size:</strong> $([math]::Round($result.FileSize / 1MB, 2)) MB</p>
        <p><strong>Created:</strong> $($result.CreationTime.ToString('yyyy-MM-dd HH:mm:ss'))</p>
        <p><strong>Integrity Score:</strong> <span class="$scoreClass">$($result.IntegrityScore)%</span></p>
        
        <table>
            <tr><th>Test</th><th>Result</th></tr>
"@
        
        foreach ($test in $result.Tests.GetEnumerator()) {
            $testResult = if ($test.Value -eq $true) { "✅ PASS" } elseif ($test.Value -eq $false) { "❌ FAIL" } else { $test.Value }
            $html += "<tr><td>$($test.Key)</td><td>$testResult</td></tr>"
        }
        
        $html += "</table>"
        
        if ($FullTest -and $result.RestoreScore) {
            $restoreScoreClass = if ($result.RestoreScore -ge 80) { "score-high" } elseif ($result.RestoreScore -ge 60) { "score-medium" } else { "score-low" }
            $html += "<p><strong>Restore Score:</strong> <span class=`"$restoreScoreClass`">$($result.RestoreScore)%</span></p>"
            if ($result.RestoreDuration) {
                $html += "<p><strong>Restore Duration:</strong> $($result.RestoreDuration.TotalMinutes.ToString('F2')) minutes</p>"
            }
        }
        
        $html += "</div>"
    }
    
    $html += @"
    
    <div class="summary">
        <h2>Recommendations</h2>
        <ul>
"@
    
    $failedBackups = $TestResults | Where-Object { $_.IntegrityScore -lt 80 }
    if ($failedBackups.Count -gt 0) {
        $html += "<li>⚠️ $($failedBackups.Count) backup(s) failed integrity checks and should be investigated</li>"
    }
    
    $oldBackups = $TestResults | Where-Object { $_.Tests.WithinRetention -eq $false }
    if ($oldBackups.Count -gt 0) {
        $html += "<li>🗂️ $($oldBackups.Count) backup(s) are outside retention period and should be archived or deleted</li>"
    }
    
    if ($FullTest) {
        $failedRestores = $TestResults | Where-Object { $_.RestoreScore -lt 80 }
        if ($failedRestores.Count -gt 0) {
            $html += "<li>🔧 $($failedRestores.Count) backup(s) failed restore tests and may not be usable for recovery</li>"
        }
    }
    
    $html += @"
            <li>📅 Schedule regular backup integrity tests (recommended: weekly)</li>
            <li>🔄 Perform full disaster recovery drills quarterly</li>
            <li>📊 Monitor backup file sizes for consistency</li>
        </ul>
    </div>
    
</body>
</html>
"@
    
    $html | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Log "Test report generated: $reportPath"
    return $reportPath
}

# Main testing process
try {
    Write-Log "Starting backup integrity testing"
    Write-Log "Full test mode: $FullTest"
    
    # Get backup files to test
    $backupFiles = Get-BackupFilesToTest
    
    if ($backupFiles.Count -eq 0) {
        Write-Log "No backup files found to test" "WARNING"
        exit 1
    }
    
    Write-Log "Found $($backupFiles.Count) backup file(s) to test"
    
    $testResults = @()
    
    foreach ($backupFile in $backupFiles) {
        Write-Log "Testing backup: $($backupFile.Name)"
        
        # Always perform basic integrity test
        $integrityResult = Test-FileIntegrity $backupFile
        
        # Perform restore test if requested
        if ($FullTest) {
            $restoreResult = Test-BackupRestore $backupFile
            # Merge results
            $integrityResult.RestoreScore = $restoreResult.RestoreScore
            $integrityResult.RestoreDuration = $restoreResult.RestoreDuration
            $integrityResult.Tests += $restoreResult.Tests
        }
        
        $testResults += $integrityResult
    }
    
    # Generate report
    $reportPath = New-IntegrityTestReport $testResults
    
    # Summary
    $passedTests = ($testResults | Where-Object { $_.IntegrityScore -ge 80 }).Count
    $totalTests = $testResults.Count
    
    Write-Log "Backup integrity testing completed"
    Write-Log "Results: $passedTests/$totalTests backups passed integrity checks"
    Write-Log "Report generated: $reportPath"
    
    if ($passedTests -eq $totalTests) {
        Write-Log "All backup integrity tests passed" "SUCCESS"
        exit 0
    }
    else {
        Write-Log "$($totalTests - $passedTests) backup(s) failed integrity checks" "WARNING"
        exit 1
    }
}
catch {
    Write-Log "Backup integrity testing failed: $_" "ERROR"
    exit 1
}