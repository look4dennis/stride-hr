# StrideHR Monitoring Stack Startup Script
# Usage: .\start-monitoring.ps1

param(
    [Parameter(Mandatory=$false)]
    [switch]$Down,
    
    [Parameter(Mandatory=$false)]
    [switch]$Logs
)

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$MonitoringDir = Join-Path $ProjectRoot "docker\monitoring"
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

function Start-MonitoringStack {
    Write-Info "Starting StrideHR monitoring stack..."
    
    try {
        Set-Location $MonitoringDir
        
        # Start monitoring services
        docker-compose -f docker-compose.monitoring.yml up -d
        
        Write-Success "Monitoring stack started successfully!"
        Write-Info ""
        Write-Info "Access URLs:"
        Write-Info "- Grafana Dashboard: http://localhost:3000 (admin/admin)"
        Write-Info "- Prometheus: http://localhost:9090"
        Write-Info "- Alertmanager: http://localhost:9093"
        Write-Info "- Node Exporter: http://localhost:9100"
        Write-Info "- cAdvisor: http://localhost:8080"
        Write-Info "- Loki: http://localhost:3100"
        Write-Info ""
        Write-Info "To view logs: .\start-monitoring.ps1 -Logs"
        Write-Info "To stop: .\start-monitoring.ps1 -Down"
    }
    catch {
        Write-Error "Failed to start monitoring stack: $_"
        exit 1
    }
}

function Stop-MonitoringStack {
    Write-Info "Stopping StrideHR monitoring stack..."
    
    try {
        Set-Location $MonitoringDir
        docker-compose -f docker-compose.monitoring.yml down
        Write-Success "Monitoring stack stopped successfully!"
    }
    catch {
        Write-Error "Failed to stop monitoring stack: $_"
        exit 1
    }
}

function Show-Logs {
    Write-Info "Showing monitoring stack logs..."
    
    try {
        Set-Location $MonitoringDir
        docker-compose -f docker-compose.monitoring.yml logs -f
    }
    catch {
        Write-Error "Failed to show logs: $_"
        exit 1
    }
}

# Main execution
if ($Down) {
    Stop-MonitoringStack
}
elseif ($Logs) {
    Show-Logs
}
else {
    Start-MonitoringStack
}