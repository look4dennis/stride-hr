# StrideHR Database Setup Script
# This script sets up the MySQL database for StrideHR

param(
    [string]$MySQLPath = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe",
    [string]$RootPassword = "Passwordtharoola007$",
    [string]$DatabaseName = "StrideHR_Dev",
    [switch]$Force
)

Write-Host "StrideHR Database Setup Script" -ForegroundColor Green
Write-Host "==============================" -ForegroundColor Green

# Check if MySQL is installed and accessible
if (-not (Test-Path $MySQLPath)) {
    Write-Host "Error: MySQL not found at $MySQLPath" -ForegroundColor Red
    Write-Host "Please install MySQL 8.0 or update the MySQLPath parameter" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found MySQL at: $MySQLPath" -ForegroundColor Green

# Test MySQL connection
Write-Host "Testing MySQL connection..." -ForegroundColor Yellow
try {
    $testConnection = & $MySQLPath -u root -p$RootPassword -e "SELECT 1;" 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Cannot connect to MySQL with provided credentials" -ForegroundColor Red
        Write-Host "Please check your root password" -ForegroundColor Yellow
        exit 1
    }
    Write-Host "MySQL connection successful!" -ForegroundColor Green
} catch {
    Write-Host "Error connecting to MySQL: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Check if database already exists
Write-Host "Checking if database '$DatabaseName' exists..." -ForegroundColor Yellow
$dbExists = & $MySQLPath -u root -p$RootPassword -e "SHOW DATABASES LIKE '$DatabaseName';" 2>&1
if ($dbExists -match $DatabaseName -and -not $Force) {
    Write-Host "Database '$DatabaseName' already exists!" -ForegroundColor Yellow
    $response = Read-Host "Do you want to continue? This will not drop the existing database. (y/N)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        Write-Host "Setup cancelled by user" -ForegroundColor Yellow
        exit 0
    }
}

# Run the database initialization script
$scriptPath = Join-Path $PSScriptRoot "..\backend\database-init.sql"
if (-not (Test-Path $scriptPath)) {
    Write-Host "Error: Database initialization script not found at $scriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "Running database initialization script..." -ForegroundColor Yellow
try {
    & $MySQLPath -u root -p$RootPassword < $scriptPath
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database initialization completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Error during database initialization" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Error running initialization script: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Verify the setup
Write-Host "Verifying database setup..." -ForegroundColor Yellow
$verification = & $MySQLPath -u root -p$RootPassword -D $DatabaseName -e "CALL CheckDatabaseHealth();" 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "Database verification successful!" -ForegroundColor Green
    Write-Host $verification
} else {
    Write-Host "Warning: Database verification failed, but setup may still be successful" -ForegroundColor Yellow
}

# Display connection information
Write-Host ""
Write-Host "Database Setup Complete!" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green
Write-Host "Database Name: $DatabaseName" -ForegroundColor Cyan
Write-Host "Connection String: Server=localhost;Database=$DatabaseName;User=root;Password=***;Port=3306;" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Update your appsettings.json with the correct connection string" -ForegroundColor White
Write-Host "2. Run Entity Framework migrations: dotnet ef database update" -ForegroundColor White
Write-Host "3. Start the StrideHR API application" -ForegroundColor White
Write-Host ""
Write-Host "Super Admin Credentials (will be created on first run):" -ForegroundColor Yellow
Write-Host "Username: Superadmin" -ForegroundColor White
Write-Host "Password: adminsuper2025$" -ForegroundColor White
Write-Host "Email: superadmin@stridehr.com" -ForegroundColor White