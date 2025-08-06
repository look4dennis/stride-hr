#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Start StrideHR production monitoring stack
.DESCRIPTION
    This script starts the complete monitoring infrastructure including
    Prometheus, Grafana, Alertmanager, and log aggregation services.
.PARAMETER Environment
    The environment to start monitoring for (default: production)
.PARAMETER Services
    Specific services to start (comma-separated)
#>

param(
    [string]$Environment = "production",
    [string]$Services = ""
)

$ErrorActionPreference = "Stop"

# Configuration
$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
$PROJECT_ROOT = Split-Path -Parent $SCRIPT_DIR
$ENV_FILE = "$PROJECT_ROOT/.env.production"
$MONITORING_COMPOSE_FILE = "$PROJECT_ROOT/docker-compose.monitoring.yml"

Write-Host "🔍 Starting StrideHR Monitoring Stack" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow

# Function to check prerequisites
function Test-Prerequisites {
    Write-Host "📋 Checking prerequisites..." -ForegroundColor Blue
    
    # Check Docker
    if (-not (Get-Command "docker" -ErrorAction SilentlyContinue)) {
        Write-Error "Docker is not installed or not in PATH"
        exit 1
    }
    
    # Check Docker Compose
    if (-not (Get-Command "docker-compose" -ErrorAction SilentlyContinue)) {
        Write-Error "Docker Compose is not installed or not in PATH"
        exit 1
    }
    
    # Check environment file
    if (-not (Test-Path $ENV_FILE)) {
        Write-Error "Environment file not found: $ENV_FILE"
        Write-Host "Please create the environment file from .env.production.template"
        exit 1
    }
    
    # Check monitoring compose file
    if (-not (Test-Path $MONITORING_COMPOSE_FILE)) {
        Write-Error "Monitoring compose file not found: $MONITORING_COMPOSE_FILE"
        exit 1
    }
    
    Write-Host "✅ Prerequisites validated" -ForegroundColor Green
}

# Function to create required directories
function New-MonitoringDirectories {
    Write-Host "📁 Creating monitoring directories..." -ForegroundColor Blue
    
    $directories = @(
        "$PROJECT_ROOT/docker/monitoring/prometheus",
        "$PROJECT_ROOT/docker/monitoring/grafana/dashboards",
        "$PROJECT_ROOT/docker/monitoring/grafana/provisioning/dashboards",
        "$PROJECT_ROOT/docker/monitoring/grafana/provisioning/datasources",
        "$PROJECT_ROOT/docker/monitoring/alertmanager",
        "$PROJECT_ROOT/docker/monitoring/loki",
        "$PROJECT_ROOT/docker/monitoring/promtail",
        "$PROJECT_ROOT/docker/monitoring/blackbox",
        "$PROJECT_ROOT/logs/monitoring"
    )
    
    foreach ($dir in $directories) {
        if (-not (Test-Path $dir)) {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            Write-Host "Created directory: $dir" -ForegroundColor Gray
        }
    }
    
    Write-Host "✅ Monitoring directories created" -ForegroundColor Green
}

# Function to create Grafana provisioning files
function New-GrafanaProvisioning {
    Write-Host "📊 Setting up Grafana provisioning..." -ForegroundColor Blue
    
    # Datasources provisioning
    $datasourcesConfig = @"
apiVersion: 1

datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://prometheus:9090
    isDefault: true
    editable: true
    
  - name: Loki
    type: loki
    access: proxy
    url: http://loki:3100
    editable: true
"@
    
    $datasourcesPath = "$PROJECT_ROOT/docker/monitoring/grafana/provisioning/datasources/datasources.yml"
    $datasourcesConfig | Out-File -FilePath $datasourcesPath -Encoding UTF8
    
    # Dashboards provisioning
    $dashboardsConfig = @"
apiVersion: 1

providers:
  - name: 'default'
    orgId: 1
    folder: ''
    type: file
    disableDeletion: false
    updateIntervalSeconds: 10
    allowUiUpdates: true
    options:
      path: /var/lib/grafana/dashboards
"@
    
    $dashboardsPath = "$PROJECT_ROOT/docker/monitoring/grafana/provisioning/dashboards/dashboards.yml"
    $dashboardsConfig | Out-File -FilePath $dashboardsPath -Encoding UTF8
    
    Write-Host "✅ Grafana provisioning configured" -ForegroundColor Green
}

# Function to validate configuration files
function Test-MonitoringConfiguration {
    Write-Host "🔧 Validating monitoring configuration..." -ForegroundColor Blue
    
    # Validate Prometheus config
    $prometheusConfig = "$PROJECT_ROOT/docker/monitoring/prometheus/prometheus.yml"
    if (-not (Test-Path $prometheusConfig)) {
        Write-Warning "Prometheus configuration not found: $prometheusConfig"
    }
    
    # Validate Grafana config
    $grafanaConfig = "$PROJECT_ROOT/docker/monitoring/grafana/grafana.ini"
    if (-not (Test-Path $grafanaConfig)) {
        Write-Warning "Grafana configuration not found: $grafanaConfig"
    }
    
    # Validate Alertmanager config
    $alertmanagerConfig = "$PROJECT_ROOT/docker/monitoring/alertmanager/alertmanager.yml"
    if (-not (Test-Path $alertmanagerConfig)) {
        Write-Warning "Alertmanager configuration not found: $alertmanagerConfig"
    }
    
    Write-Host "✅ Configuration validation completed" -ForegroundColor Green
}

# Function to start monitoring services
function Start-MonitoringServices {
    Write-Host "🚀 Starting monitoring services..." -ForegroundColor Blue
    
    try {
        # Create network if it doesn't exist
        $networkExists = docker network ls --filter name=stridehr-network --format "{{.Name}}" | Select-String "stridehr-network"
        if (-not $networkExists) {
            Write-Host "Creating Docker network..." -ForegroundColor Yellow
            docker network create stridehr-network
        }
        
        # Start services
        if ($Services) {
            $serviceList = $Services -split ","
            foreach ($service in $serviceList) {
                Write-Host "Starting service: $service" -ForegroundColor Yellow
                docker-compose -f $MONITORING_COMPOSE_FILE --env-file $ENV_FILE up -d $service.Trim()
            }
        }
        else {
            Write-Host "Starting all monitoring services..." -ForegroundColor Yellow
            docker-compose -f $MONITORING_COMPOSE_FILE --env-file $ENV_FILE up -d
        }
        
        # Wait for services to be ready
        Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
        Start-Sleep -Seconds 30
        
        # Check service health
        $services = docker-compose -f $MONITORING_COMPOSE_FILE ps --services
        foreach ($service in $services) {
            $status = docker-compose -f $MONITORING_COMPOSE_FILE ps $service --format "{{.State}}"
            if ($status -eq "running") {
                Write-Host "✅ $service is running" -ForegroundColor Green
            }
            else {
                Write-Warning "$service status: $status"
            }
        }
        
        Write-Host "✅ Monitoring services started successfully" -ForegroundColor Green
    }
    catch {
        Write-Error "Failed to start monitoring services: $_"
        exit 1
    }
}

# Function to display access information
function Show-AccessInformation {
    Write-Host ""
    Write-Host "🎉 Monitoring stack is ready!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Access URLs:" -ForegroundColor Yellow
    Write-Host "  Grafana:      http://localhost:3000" -ForegroundColor White
    Write-Host "  Prometheus:   http://localhost:9090" -ForegroundColor White
    Write-Host "  Alertmanager: http://localhost:9093" -ForegroundColor White
    Write-Host "  Node Exporter: http://localhost:9100" -ForegroundColor White
    Write-Host "  cAdvisor:     http://localhost:8080" -ForegroundColor White
    Write-Host ""
    Write-Host "Default Credentials:" -ForegroundColor Yellow
    Write-Host "  Grafana: admin / (check GRAFANA_ADMIN_PASSWORD in .env.production)" -ForegroundColor White
    Write-Host ""
    Write-Host "Useful Commands:" -ForegroundColor Yellow
    Write-Host "  View logs: docker-compose -f $MONITORING_COMPOSE_FILE logs -f [service]" -ForegroundColor White
    Write-Host "  Stop monitoring: docker-compose -f $MONITORING_COMPOSE_FILE down" -ForegroundColor White
    Write-Host "  Restart service: docker-compose -f $MONITORING_COMPOSE_FILE restart [service]" -ForegroundColor White
    Write-Host ""
}

# Main execution
try {
    Test-Prerequisites
    New-MonitoringDirectories
    New-GrafanaProvisioning
    Test-MonitoringConfiguration
    Start-MonitoringServices
    Show-AccessInformation
    
    Write-Host "Monitoring stack startup completed successfully!" -ForegroundColor Green
}
catch {
    Write-Error "Monitoring startup failed: $_"
    Write-Host "Check logs with: docker-compose -f $MONITORING_COMPOSE_FILE logs" -ForegroundColor Yellow
    exit 1
}