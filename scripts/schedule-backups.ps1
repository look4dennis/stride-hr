#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Backup scheduling script for StrideHR production
.DESCRIPTION
    This script sets up automated backup schedules using Windows Task Scheduler
    or cron jobs on Linux systems.
.PARAMETER Action
    Action to perform: install, uninstall, or status (default: install)
.PARAMETER BackupType
    Type of backup schedule to set up: full, incremental, or all (default: all)
#>

param(
    [ValidateSet("install", "uninstall", "status")]
    [string]$Action = "install",
    [ValidateSet("full", "incremental", "all")]
    [string]$BackupType = "all"
)

$ErrorActionPreference = "Stop"

# Configuration
$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
$PROJECT_ROOT = Split-Path -Parent $SCRIPT_DIR
$LOG_FILE = "$PROJECT_ROOT/logs/backup-scheduler.log"

# Task definitions
$BACKUP_TASKS = @{
    "StrideHR-FullBackup" = @{
        Description = "StrideHR Full Database Backup"
        Script = "$SCRIPT_DIR/backup-database.ps1"
        Arguments = "-BackupType full -Compress `$true -Encrypt `$true -UploadToCloud `$true"
        Schedule = "Daily"
        Time = "02:00"
        Enabled = $true
    }
    "StrideHR-IncrementalBackup" = @{
        Description = "StrideHR Incremental Database Backup"
        Script = "$SCRIPT_DIR/backup-database.ps1"
        Arguments = "-BackupType incremental -Compress `$true -Encrypt `$true -UploadToCloud `$true"
        Schedule = "Every6Hours"
        Time = "06:00"
        Enabled = $true
    }
    "StrideHR-BackupIntegrityTest" = @{
        Description = "StrideHR Backup Integrity Test"
        Script = "$SCRIPT_DIR/test-backup-integrity.ps1"
        Arguments = "-FullTest `$false"
        Schedule = "Weekly"
        Time = "03:00"
        DayOfWeek = "Sunday"
        Enabled = $true
    }
    "StrideHR-BackupCleanup" = @{
        Description = "StrideHR Backup Cleanup"
        Script = "$SCRIPT_DIR/cleanup-old-backups.ps1"
        Arguments = "-RetentionDays 30"
        Schedule = "Weekly"
        Time = "04:00"
        DayOfWeek = "Sunday"
        Enabled = $true
    }
}

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

# Function to detect operating system
function Get-OperatingSystem {
    if ($IsWindows -or $env:OS -eq "Windows_NT") {
        return "Windows"
    }
    elseif ($IsLinux) {
        return "Linux"
    }
    elseif ($IsMacOS) {
        return "macOS"
    }
    else {
        return "Unknown"
    }
}

# Function to install Windows scheduled tasks
function Install-WindowsScheduledTasks {
    Write-Log "Installing Windows scheduled tasks..."
    
    foreach ($taskName in $BACKUP_TASKS.Keys) {
        $task = $BACKUP_TASKS[$taskName]
        
        # Skip if not enabled or not matching backup type filter
        if (-not $task.Enabled) { continue }
        if ($BackupType -ne "all" -and $taskName -notmatch $BackupType) { continue }
        
        try {
            Write-Log "Creating scheduled task: $taskName"
            
            # Create task action
            $action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-ExecutionPolicy Bypass -File `"$($task.Script)`" $($task.Arguments)"
            
            # Create task trigger based on schedule type
            switch ($task.Schedule) {
                "Daily" {
                    $trigger = New-ScheduledTaskTrigger -Daily -At $task.Time
                }
                "Weekly" {
                    $trigger = New-ScheduledTaskTrigger -Weekly -DaysOfWeek $task.DayOfWeek -At $task.Time
                }
                "Every6Hours" {
                    # Create multiple daily triggers for every 6 hours
                    $triggers = @()
                    for ($hour = 0; $hour -lt 24; $hour += 6) {
                        $time = "{0:D2}:00" -f $hour
                        $triggers += New-ScheduledTaskTrigger -Daily -At $time
                    }
                    $trigger = $triggers
                }
                default {
                    $trigger = New-ScheduledTaskTrigger -Daily -At $task.Time
                }
            }
            
            # Create task settings
            $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -RunOnlyIfNetworkAvailable
            
            # Create task principal (run as SYSTEM)
            $principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest
            
            # Register the task
            Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Settings $settings -Principal $principal -Description $task.Description -Force
            
            Write-Log "Successfully created scheduled task: $taskName"
        }
        catch {
            Write-Log "Failed to create scheduled task $taskName`: $_" "ERROR"
        }
    }
}

# Function to install Linux cron jobs
function Install-LinuxCronJobs {
    Write-Log "Installing Linux cron jobs..."
    
    $cronEntries = @()
    
    foreach ($taskName in $BACKUP_TASKS.Keys) {
        $task = $BACKUP_TASKS[$taskName]
        
        # Skip if not enabled or not matching backup type filter
        if (-not $task.Enabled) { continue }
        if ($BackupType -ne "all" -and $taskName -notmatch $BackupType) { continue }
        
        try {
            Write-Log "Creating cron job for: $taskName"
            
            # Convert schedule to cron format
            $cronSchedule = switch ($task.Schedule) {
                "Daily" {
                    $hour = [int]($task.Time.Split(':')[0])
                    $minute = [int]($task.Time.Split(':')[1])
                    "$minute $hour * * *"
                }
                "Weekly" {
                    $hour = [int]($task.Time.Split(':')[0])
                    $minute = [int]($task.Time.Split(':')[1])
                    $dayNum = switch ($task.DayOfWeek) {
                        "Sunday" { 0 }
                        "Monday" { 1 }
                        "Tuesday" { 2 }
                        "Wednesday" { 3 }
                        "Thursday" { 4 }
                        "Friday" { 5 }
                        "Saturday" { 6 }
                    }
                    "$minute $hour * * $dayNum"
                }
                "Every6Hours" {
                    "0 */6 * * *"
                }
                default {
                    $hour = [int]($task.Time.Split(':')[0])
                    $minute = [int]($task.Time.Split(':')[1])
                    "$minute $hour * * *"
                }
            }
            
            # Create cron entry
            $cronCommand = "pwsh -ExecutionPolicy Bypass -File `"$($task.Script)`" $($task.Arguments) >> `"$LOG_FILE`" 2>&1"
            $cronEntry = "$cronSchedule $cronCommand # $taskName"
            $cronEntries += $cronEntry
            
            Write-Log "Cron entry created: $cronEntry"
        }
        catch {
            Write-Log "Failed to create cron job for $taskName`: $_" "ERROR"
        }
    }
    
    if ($cronEntries.Count -gt 0) {
        try {
            # Get existing crontab
            $existingCron = crontab -l 2>/dev/null | Where-Object { $_ -notmatch "# StrideHR-" }
            
            # Combine existing and new entries
            $allEntries = @()
            if ($existingCron) {
                $allEntries += $existingCron
            }
            $allEntries += "# StrideHR Backup Jobs - Generated $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
            $allEntries += $cronEntries
            
            # Install new crontab
            $allEntries | crontab -
            
            Write-Log "Cron jobs installed successfully"
        }
        catch {
            Write-Log "Failed to install cron jobs: $_" "ERROR"
        }
    }
}

# Function to uninstall Windows scheduled tasks
function Uninstall-WindowsScheduledTasks {
    Write-Log "Uninstalling Windows scheduled tasks..."
    
    foreach ($taskName in $BACKUP_TASKS.Keys) {
        try {
            $existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
            if ($existingTask) {
                Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
                Write-Log "Removed scheduled task: $taskName"
            }
            else {
                Write-Log "Scheduled task not found: $taskName"
            }
        }
        catch {
            Write-Log "Failed to remove scheduled task $taskName`: $_" "ERROR"
        }
    }
}

# Function to uninstall Linux cron jobs
function Uninstall-LinuxCronJobs {
    Write-Log "Uninstalling Linux cron jobs..."
    
    try {
        # Get existing crontab and remove StrideHR entries
        $existingCron = crontab -l 2>/dev/null | Where-Object { $_ -notmatch "# StrideHR" -and $_ -notmatch "StrideHR-" }
        
        if ($existingCron) {
            $existingCron | crontab -
        }
        else {
            # Remove entire crontab if only StrideHR entries existed
            crontab -r 2>/dev/null
        }
        
        Write-Log "Cron jobs uninstalled successfully"
    }
    catch {
        Write-Log "Failed to uninstall cron jobs: $_" "ERROR"
    }
}

# Function to show status of scheduled tasks
function Show-ScheduleStatus {
    Write-Log "Checking backup schedule status..."
    
    $os = Get-OperatingSystem
    
    if ($os -eq "Windows") {
        Write-Host "`nWindows Scheduled Tasks:" -ForegroundColor Yellow
        Write-Host "========================" -ForegroundColor Yellow
        
        foreach ($taskName in $BACKUP_TASKS.Keys) {
            try {
                $task = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
                if ($task) {
                    $info = Get-ScheduledTaskInfo -TaskName $taskName
                    $status = if ($task.State -eq "Ready") { "✅ Enabled" } else { "❌ Disabled" }
                    $lastRun = if ($info.LastRunTime -eq (Get-Date "1/1/1900")) { "Never" } else { $info.LastRunTime.ToString("yyyy-MM-dd HH:mm:ss") }
                    $nextRun = if ($info.NextRunTime -eq (Get-Date "1/1/1900")) { "Not scheduled" } else { $info.NextRunTime.ToString("yyyy-MM-dd HH:mm:ss") }
                    
                    Write-Host "Task: $taskName" -ForegroundColor White
                    Write-Host "  Status: $status" -ForegroundColor $(if ($task.State -eq "Ready") { "Green" } else { "Red" })
                    Write-Host "  Last Run: $lastRun" -ForegroundColor Gray
                    Write-Host "  Next Run: $nextRun" -ForegroundColor Gray
                    Write-Host ""
                }
                else {
                    Write-Host "Task: $taskName" -ForegroundColor White
                    Write-Host "  Status: ❌ Not installed" -ForegroundColor Red
                    Write-Host ""
                }
            }
            catch {
                Write-Host "Task: $taskName" -ForegroundColor White
                Write-Host "  Status: ❌ Error checking status" -ForegroundColor Red
                Write-Host ""
            }
        }
    }
    elseif ($os -eq "Linux") {
        Write-Host "`nLinux Cron Jobs:" -ForegroundColor Yellow
        Write-Host "================" -ForegroundColor Yellow
        
        try {
            $cronJobs = crontab -l 2>/dev/null | Where-Object { $_ -match "StrideHR-" }
            if ($cronJobs) {
                foreach ($job in $cronJobs) {
                    Write-Host $job -ForegroundColor White
                }
            }
            else {
                Write-Host "No StrideHR cron jobs found" -ForegroundColor Red
            }
        }
        catch {
            Write-Host "Error checking cron jobs: $_" -ForegroundColor Red
        }
    }
    else {
        Write-Host "Unsupported operating system: $os" -ForegroundColor Red
    }
}

# Function to create backup cleanup script
function New-BackupCleanupScript {
    $cleanupScript = @"
#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Cleanup old backup files for StrideHR
.DESCRIPTION
    This script removes backup files older than the specified retention period.
.PARAMETER RetentionDays
    Number of days to retain backups (default: 30)
#>

param(
    [int]`$RetentionDays = 30
)

`$ErrorActionPreference = "Stop"

# Configuration
`$SCRIPT_DIR = Split-Path -Parent `$MyInvocation.MyCommand.Path
`$PROJECT_ROOT = Split-Path -Parent `$SCRIPT_DIR
`$BACKUP_DIR = "`$PROJECT_ROOT/backups"
`$LOG_FILE = "`$PROJECT_ROOT/logs/backup-cleanup.log"

# Function to write log messages
function Write-Log {
    param([string]`$Message, [string]`$Level = "INFO")
    `$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    `$logMessage = "[`$timestamp] [`$Level] `$Message"
    Write-Host `$logMessage
    
    if (-not (Test-Path (Split-Path `$LOG_FILE))) {
        New-Item -ItemType Directory -Path (Split-Path `$LOG_FILE) -Force | Out-Null
    }
    `$logMessage | Out-File -FilePath `$LOG_FILE -Append -Encoding UTF8
}

try {
    Write-Log "Starting backup cleanup (retention: `$RetentionDays days)"
    
    if (-not (Test-Path `$BACKUP_DIR)) {
        Write-Log "Backup directory not found: `$BACKUP_DIR" "WARNING"
        exit 0
    }
    
    `$cutoffDate = (Get-Date).AddDays(-`$RetentionDays)
    `$oldBackups = Get-ChildItem -Path `$BACKUP_DIR -Filter "stridehr-*" | Where-Object { `$_.CreationTime -lt `$cutoffDate }
    
    if (`$oldBackups.Count -eq 0) {
        Write-Log "No old backups found to clean up"
        exit 0
    }
    
    `$totalSize = (`$oldBackups | Measure-Object -Property Length -Sum).Sum
    Write-Log "Found `$(`$oldBackups.Count) old backup(s) totaling `$([math]::Round(`$totalSize / 1MB, 2)) MB"
    
    foreach (`$backup in `$oldBackups) {
        try {
            Remove-Item `$backup.FullName -Force
            Write-Log "Removed old backup: `$(`$backup.Name)"
        }
        catch {
            Write-Log "Failed to remove backup `$(`$backup.Name): `$_" "ERROR"
        }
    }
    
    Write-Log "Backup cleanup completed successfully"
}
catch {
    Write-Log "Backup cleanup failed: `$_" "ERROR"
    exit 1
}
"@
    
    $cleanupScriptPath = "$SCRIPT_DIR/cleanup-old-backups.ps1"
    $cleanupScript | Out-File -FilePath $cleanupScriptPath -Encoding UTF8
    Write-Log "Created backup cleanup script: $cleanupScriptPath"
}

# Main execution
try {
    Write-Log "Starting backup scheduler - Action: $Action, Type: $BackupType"
    
    $os = Get-OperatingSystem
    Write-Log "Detected operating system: $os"
    
    # Create backup cleanup script if it doesn't exist
    if (-not (Test-Path "$SCRIPT_DIR/cleanup-old-backups.ps1")) {
        New-BackupCleanupScript
    }
    
    switch ($Action) {
        "install" {
            if ($os -eq "Windows") {
                Install-WindowsScheduledTasks
            }
            elseif ($os -eq "Linux") {
                Install-LinuxCronJobs
            }
            else {
                Write-Log "Unsupported operating system for automatic scheduling: $os" "ERROR"
                exit 1
            }
        }
        "uninstall" {
            if ($os -eq "Windows") {
                Uninstall-WindowsScheduledTasks
            }
            elseif ($os -eq "Linux") {
                Uninstall-LinuxCronJobs
            }
            else {
                Write-Log "Unsupported operating system: $os" "ERROR"
                exit 1
            }
        }
        "status" {
            Show-ScheduleStatus
        }
    }
    
    Write-Log "Backup scheduler operation completed successfully"
}
catch {
    Write-Log "Backup scheduler operation failed: $_" "ERROR"
    exit 1
}