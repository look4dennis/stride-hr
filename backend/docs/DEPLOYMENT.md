# StrideHR Deployment Guide

This guide covers deployment strategies and configurations for StrideHR in various environments.

## Table of Contents

- [Environment Overview](#environment-overview)
- [Prerequisites](#prerequisites)
- [Configuration Management](#configuration-management)
- [Database Setup](#database-setup)
- [Docker Deployment](#docker-deployment)
- [Cloud Deployment](#cloud-deployment)
- [Security Configuration](#security-configuration)
- [Monitoring and Logging](#monitoring-and-logging)
- [Backup and Recovery](#backup-and-recovery)
- [Troubleshooting](#troubleshooting)

## Environment Overview

StrideHR supports multiple deployment environments:

- **Development**: Local development with hot reload
- **Staging**: Pre-production testing environment
- **Production**: Live production environment
- **Docker**: Containerized deployment
- **Cloud**: AWS, Azure, or GCP deployment

## Prerequisites

### System Requirements

#### Minimum Requirements
- **CPU**: 2 cores
- **RAM**: 4GB
- **Storage**: 20GB SSD
- **Network**: 100 Mbps

#### Recommended Requirements
- **CPU**: 4+ cores
- **RAM**: 8GB+
- **Storage**: 50GB+ SSD
- **Network**: 1 Gbps

### Software Dependencies

- **.NET 8 Runtime**
- **MySQL 8.0+**
- **Redis 7.0+** (optional but recommended)
- **Nginx** (for reverse proxy)
- **SSL Certificate** (for HTTPS)

## Configuration Management

### Environment-Specific Configuration

#### Development (appsettings.Development.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=stridehr_dev;User=stridehr_user;Password=dev_password;"
  },
  "JwtSettings": {
    "SecretKey": "development-secret-key-minimum-32-characters-long",
    "Issuer": "StrideHR-Dev",
    "Audience": "StrideHR-Users-Dev",
    "ExpirationHours": 24,
    "RefreshTokenExpirationDays": 7
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Database": 0
  }
}
```

#### Staging (appsettings.Staging.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "StrideHR": "Debug"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=staging-db.internal;Database=stridehr_staging;User=stridehr_staging;Password=${DB_PASSWORD};"
  },
  "JwtSettings": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "StrideHR-Staging",
    "Audience": "StrideHR-Users-Staging",
    "ExpirationHours": 8,
    "RefreshTokenExpirationDays": 3
  },
  "Redis": {
    "ConnectionString": "staging-redis.internal:6379",
    "Database": 1
  }
}
```

#### Production (appsettings.Production.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error",
      "StrideHR": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=${DB_HOST};Database=${DB_NAME};User=${DB_USER};Password=${DB_PASSWORD};SslMode=Required;"
  },
  "JwtSettings": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "StrideHR-Production",
    "Audience": "StrideHR-Users-Production",
    "ExpirationHours": 4,
    "RefreshTokenExpirationDays": 1,
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "ValidateIssuerSigningKey": true
  },
  "Redis": {
    "ConnectionString": "${REDIS_CONNECTION_STRING}",
    "Database": 0
  },
  "EncryptionSettings": {
    "Key": "${ENCRYPTION_KEY}",
    "IV": "${ENCRYPTION_IV}"
  }
}
```

### Environment Variables

Create a `.env` file for environment-specific variables:

```bash
# Database Configuration
DB_HOST=production-db.company.com
DB_NAME=stridehr_production
DB_USER=stridehr_prod_user
DB_PASSWORD=super_secure_password_here

# JWT Configuration
JWT_SECRET_KEY=production-jwt-secret-key-minimum-64-characters-long-for-security

# Redis Configuration
REDIS_CONNECTION_STRING=production-redis.company.com:6379

# Encryption Configuration
ENCRYPTION_KEY=32-character-encryption-key-here
ENCRYPTION_IV=16-character-iv-here

# Email Configuration
SMTP_HOST=smtp.company.com
SMTP_PORT=587
SMTP_USERNAME=noreply@company.com
SMTP_PASSWORD=smtp_password_here

# File Storage
FILE_STORAGE_PATH=/var/stridehr/uploads
MAX_FILE_SIZE_MB=10

# External API Keys
OPENWEATHER_API_KEY=your_openweather_api_key
GOOGLE_CALENDAR_CLIENT_ID=your_google_client_id
GOOGLE_CALENDAR_CLIENT_SECRET=your_google_client_secret

# Monitoring
APPLICATION_INSIGHTS_KEY=your_app_insights_key
SENTRY_DSN=your_sentry_dsn
```

## Database Setup

### MySQL Configuration

#### Create Production Database
```sql
-- Create database
CREATE DATABASE stridehr_production CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create user with limited privileges
CREATE USER 'stridehr_prod'@'%' IDENTIFIED BY 'secure_password_here';
GRANT SELECT, INSERT, UPDATE, DELETE ON stridehr_production.* TO 'stridehr_prod'@'%';
GRANT CREATE, ALTER, INDEX, DROP ON stridehr_production.* TO 'stridehr_prod'@'%';
FLUSH PRIVILEGES;

-- Configure MySQL for production
SET GLOBAL innodb_buffer_pool_size = 2G;
SET GLOBAL max_connections = 500;
SET GLOBAL query_cache_size = 256M;
```

#### Database Migration
```bash
# Run migrations in production
cd backend/src/StrideHR.API
dotnet ef database update --configuration Production

# Seed initial data
dotnet run --configuration Production --seed-data
```

### Database Backup Strategy

#### Automated Backup Script
```bash
#!/bin/bash
# backup-database.sh

DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/var/backups/stridehr"
DB_NAME="stridehr_production"
DB_USER="backup_user"
DB_PASSWORD="backup_password"

# Create backup directory
mkdir -p $BACKUP_DIR

# Create database backup
mysqldump -u $DB_USER -p$DB_PASSWORD \
  --single-transaction \
  --routines \
  --triggers \
  $DB_NAME > $BACKUP_DIR/stridehr_backup_$DATE.sql

# Compress backup
gzip $BACKUP_DIR/stridehr_backup_$DATE.sql

# Remove backups older than 30 days
find $BACKUP_DIR -name "*.sql.gz" -mtime +30 -delete

echo "Database backup completed: stridehr_backup_$DATE.sql.gz"
```

## Docker Deployment

### Production Docker Compose

```yaml
# docker-compose.prod.yml
version: '3.8'

services:
  stridehr-api:
    build:
      context: ./backend
      dockerfile: Dockerfile.prod
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=mysql;Database=stridehr_production;User=stridehr_user;Password=${DB_PASSWORD};
      - JwtSettings__SecretKey=${JWT_SECRET_KEY}
      - Redis__ConnectionString=redis:6379
    depends_on:
      - mysql
      - redis
    volumes:
      - ./uploads:/app/uploads
      - ./logs:/app/logs
    restart: unless-stopped
    networks:
      - stridehr-network

  stridehr-frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile.prod
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - stridehr-api
    restart: unless-stopped
    networks:
      - stridehr-network

  mysql:
    image: mysql:8.0
    environment:
      - MYSQL_ROOT_PASSWORD=${MYSQL_ROOT_PASSWORD}
      - MYSQL_DATABASE=stridehr_production
      - MYSQL_USER=stridehr_user
      - MYSQL_PASSWORD=${DB_PASSWORD}
    volumes:
      - mysql-data:/var/lib/mysql
      - ./mysql/my.cnf:/etc/mysql/conf.d/my.cnf
    ports:
      - "3306:3306"
    restart: unless-stopped
    networks:
      - stridehr-network

  redis:
    image: redis:7-alpine
    command: redis-server --appendonly yes --requirepass ${REDIS_PASSWORD}
    volumes:
      - redis-data:/data
    ports:
      - "6379:6379"
    restart: unless-stopped
    networks:
      - stridehr-network

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
      - ./nginx/logs:/var/log/nginx
    depends_on:
      - stridehr-api
      - stridehr-frontend
    restart: unless-stopped
    networks:
      - stridehr-network

volumes:
  mysql-data:
  redis-data:

networks:
  stridehr-network:
    driver: bridge
```

### Production Dockerfile

```dockerfile
# backend/Dockerfile.prod
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/StrideHR.API/StrideHR.API.csproj", "src/StrideHR.API/"]
COPY ["src/StrideHR.Core/StrideHR.Core.csproj", "src/StrideHR.Core/"]
COPY ["src/StrideHR.Infrastructure/StrideHR.Infrastructure.csproj", "src/StrideHR.Infrastructure/"]
RUN dotnet restore "src/StrideHR.API/StrideHR.API.csproj"
COPY . .
WORKDIR "/src/src/StrideHR.API"
RUN dotnet build "StrideHR.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StrideHR.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directories for uploads and logs
RUN mkdir -p /app/uploads /app/logs

# Set permissions
RUN chown -R www-data:www-data /app/uploads /app/logs

ENTRYPOINT ["dotnet", "StrideHR.API.dll"]
```

### Nginx Configuration

```nginx
# nginx/nginx.conf
events {
    worker_connections 1024;
}

http {
    upstream stridehr-api {
        server stridehr-api:80;
    }

    upstream stridehr-frontend {
        server stridehr-frontend:80;
    }

    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api:10m rate=10r/s;
    limit_req_zone $binary_remote_addr zone=login:10m rate=1r/s;

    server {
        listen 80;
        server_name your-domain.com;
        return 301 https://$server_name$request_uri;
    }

    server {
        listen 443 ssl http2;
        server_name your-domain.com;

        ssl_certificate /etc/nginx/ssl/cert.pem;
        ssl_certificate_key /etc/nginx/ssl/key.pem;
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
        ssl_prefer_server_ciphers off;

        # Security headers
        add_header X-Frame-Options DENY;
        add_header X-Content-Type-Options nosniff;
        add_header X-XSS-Protection "1; mode=block";
        add_header Strict-Transport-Security "max-age=63072000; includeSubDomains; preload";

        # API routes
        location /api/ {
            limit_req zone=api burst=20 nodelay;
            proxy_pass http://stridehr-api;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        # Login endpoint with stricter rate limiting
        location /api/auth/login {
            limit_req zone=login burst=5 nodelay;
            proxy_pass http://stridehr-api;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        # SignalR WebSocket support
        location /hubs/ {
            proxy_pass http://stridehr-api;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
            proxy_set_header Host $host;
            proxy_cache_bypass $http_upgrade;
        }

        # Frontend routes
        location / {
            proxy_pass http://stridehr-frontend;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        # Static files caching
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }
}
```

## Cloud Deployment

### AWS Deployment

#### Using AWS ECS with Fargate

```yaml
# aws-ecs-task-definition.json
{
  "family": "stridehr-task",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "1024",
  "memory": "2048",
  "executionRoleArn": "arn:aws:iam::account:role/ecsTaskExecutionRole",
  "taskRoleArn": "arn:aws:iam::account:role/ecsTaskRole",
  "containerDefinitions": [
    {
      "name": "stridehr-api",
      "image": "your-account.dkr.ecr.region.amazonaws.com/stridehr-api:latest",
      "portMappings": [
        {
          "containerPort": 80,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        }
      ],
      "secrets": [
        {
          "name": "ConnectionStrings__DefaultConnection",
          "valueFrom": "arn:aws:secretsmanager:region:account:secret:stridehr/db-connection"
        },
        {
          "name": "JwtSettings__SecretKey",
          "valueFrom": "arn:aws:secretsmanager:region:account:secret:stridehr/jwt-secret"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/stridehr",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

#### CloudFormation Template

```yaml
# cloudformation-template.yml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'StrideHR Infrastructure'

Parameters:
  Environment:
    Type: String
    Default: production
    AllowedValues: [development, staging, production]

Resources:
  # VPC and Networking
  VPC:
    Type: AWS::EC2::VPC
    Properties:
      CidrBlock: 10.0.0.0/16
      EnableDnsHostnames: true
      EnableDnsSupport: true
      Tags:
        - Key: Name
          Value: !Sub 'stridehr-vpc-${Environment}'

  # RDS MySQL Instance
  DBInstance:
    Type: AWS::RDS::DBInstance
    Properties:
      DBInstanceIdentifier: !Sub 'stridehr-db-${Environment}'
      DBInstanceClass: db.t3.medium
      Engine: mysql
      EngineVersion: '8.0'
      MasterUsername: stridehr_admin
      MasterUserPassword: !Ref DBPassword
      AllocatedStorage: 100
      StorageType: gp2
      StorageEncrypted: true
      BackupRetentionPeriod: 7
      MultiAZ: !If [IsProduction, true, false]
      VPCSecurityGroups:
        - !Ref DBSecurityGroup

  # ElastiCache Redis
  RedisCluster:
    Type: AWS::ElastiCache::CacheCluster
    Properties:
      CacheNodeType: cache.t3.micro
      Engine: redis
      NumCacheNodes: 1
      VpcSecurityGroupIds:
        - !Ref RedisSecurityGroup

  # Application Load Balancer
  LoadBalancer:
    Type: AWS::ElasticLoadBalancingV2::LoadBalancer
    Properties:
      Name: !Sub 'stridehr-alb-${Environment}'
      Scheme: internet-facing
      Type: application
      Subnets:
        - !Ref PublicSubnet1
        - !Ref PublicSubnet2
      SecurityGroups:
        - !Ref ALBSecurityGroup

Conditions:
  IsProduction: !Equals [!Ref Environment, production]

Outputs:
  LoadBalancerDNS:
    Description: 'Load Balancer DNS Name'
    Value: !GetAtt LoadBalancer.DNSName
    Export:
      Name: !Sub '${AWS::StackName}-LoadBalancerDNS'
```

### Azure Deployment

#### Using Azure Container Instances

```yaml
# azure-container-group.yml
apiVersion: 2019-12-01
location: eastus
name: stridehr-container-group
properties:
  containers:
  - name: stridehr-api
    properties:
      image: your-registry.azurecr.io/stridehr-api:latest
      resources:
        requests:
          cpu: 1
          memoryInGb: 2
      ports:
      - port: 80
      environmentVariables:
      - name: ASPNETCORE_ENVIRONMENT
        value: Production
      - name: ConnectionStrings__DefaultConnection
        secureValue: "Server=your-mysql-server.mysql.database.azure.com;Database=stridehr;User=stridehr@your-mysql-server;Password=your-password;SslMode=Required;"
  osType: Linux
  ipAddress:
    type: Public
    ports:
    - protocol: tcp
      port: 80
  restartPolicy: Always
```

## Security Configuration

### SSL/TLS Configuration

#### Let's Encrypt with Certbot

```bash
# Install Certbot
sudo apt-get update
sudo apt-get install certbot python3-certbot-nginx

# Obtain SSL certificate
sudo certbot --nginx -d your-domain.com -d www.your-domain.com

# Auto-renewal
sudo crontab -e
# Add: 0 12 * * * /usr/bin/certbot renew --quiet
```

### Firewall Configuration

```bash
# UFW Firewall rules
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow from 10.0.0.0/8 to any port 3306  # MySQL
sudo ufw allow from 10.0.0.0/8 to any port 6379  # Redis
sudo ufw enable
```

### Security Headers

```csharp
// In Program.cs
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    
    if (context.Request.IsHttps)
    {
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }
    
    await next();
});
```

## Monitoring and Logging

### Application Insights Configuration

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key",
    "EnableAdaptiveSampling": true,
    "EnableQuickPulseMetricStream": true
  }
}
```

### Serilog Configuration

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.File", "Serilog.Sinks.Console"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "/app/logs/stridehr-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

### Health Checks

```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddMySql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .AddRedis(builder.Configuration.GetConnectionString("Redis"))
    .AddCheck("api", () => HealthCheckResult.Healthy("API is running"));

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

## Backup and Recovery

### Automated Backup Strategy

```bash
#!/bin/bash
# full-backup.sh

# Configuration
BACKUP_DIR="/var/backups/stridehr"
DATE=$(date +%Y%m%d_%H%M%S)
RETENTION_DAYS=30

# Create backup directory
mkdir -p $BACKUP_DIR

# Database backup
echo "Starting database backup..."
mysqldump -u backup_user -p$DB_PASSWORD \
  --single-transaction \
  --routines \
  --triggers \
  stridehr_production > $BACKUP_DIR/db_backup_$DATE.sql

# File system backup
echo "Starting file system backup..."
tar -czf $BACKUP_DIR/files_backup_$DATE.tar.gz \
  /var/stridehr/uploads \
  /var/stridehr/logs \
  /etc/nginx/sites-available \
  /etc/ssl/certs

# Upload to cloud storage (AWS S3)
aws s3 cp $BACKUP_DIR/db_backup_$DATE.sql s3://stridehr-backups/database/
aws s3 cp $BACKUP_DIR/files_backup_$DATE.tar.gz s3://stridehr-backups/files/

# Cleanup old backups
find $BACKUP_DIR -name "*.sql" -mtime +$RETENTION_DAYS -delete
find $BACKUP_DIR -name "*.tar.gz" -mtime +$RETENTION_DAYS -delete

echo "Backup completed successfully"
```

### Disaster Recovery Plan

1. **Recovery Time Objective (RTO)**: 4 hours
2. **Recovery Point Objective (RPO)**: 1 hour
3. **Backup Frequency**: Every 6 hours
4. **Testing**: Monthly disaster recovery drills

## Troubleshooting

### Common Issues

#### Application Won't Start
```bash
# Check logs
docker logs stridehr-api
tail -f /var/log/stridehr/application.log

# Check configuration
dotnet run --configuration Production --dry-run
```

#### Database Connection Issues
```bash
# Test database connectivity
mysql -h db-host -u username -p database_name

# Check connection string
echo $ConnectionStrings__DefaultConnection
```

#### High Memory Usage
```bash
# Monitor memory usage
docker stats stridehr-api
htop

# Check for memory leaks
dotnet-dump collect -p $(pgrep -f StrideHR.API)
```

#### Performance Issues
```bash
# Check application metrics
curl http://localhost:5000/health
curl http://localhost:5000/metrics

# Monitor database performance
SHOW PROCESSLIST;
SHOW ENGINE INNODB STATUS;
```

### Monitoring Commands

```bash
# System monitoring
htop
iotop
netstat -tulpn

# Docker monitoring
docker stats
docker logs --tail 100 -f stridehr-api

# Database monitoring
mysqladmin -u root -p processlist
mysqladmin -u root -p status
```

### Log Analysis

```bash
# Application logs
tail -f /var/log/stridehr/application.log | grep ERROR
grep -i "exception" /var/log/stridehr/application.log

# Nginx logs
tail -f /var/log/nginx/access.log
tail -f /var/log/nginx/error.log

# System logs
journalctl -u stridehr-api -f
dmesg | tail
```

## Performance Optimization

### Database Optimization

```sql
-- Add indexes for frequently queried columns
CREATE INDEX idx_employee_branch_id ON Employees(BranchId);
CREATE INDEX idx_attendance_employee_date ON AttendanceRecords(EmployeeId, Date);
CREATE INDEX idx_payroll_employee_period ON PayrollRecords(EmployeeId, PayrollPeriod);

-- Optimize MySQL configuration
SET GLOBAL innodb_buffer_pool_size = 2G;
SET GLOBAL query_cache_size = 256M;
SET GLOBAL max_connections = 500;
```

### Application Optimization

```csharp
// Enable response caching
builder.Services.AddResponseCaching();
app.UseResponseCaching();

// Enable compression
builder.Services.AddResponseCompression();
app.UseResponseCompression();

// Configure Entity Framework
builder.Services.AddDbContext<StrideHRDbContext>(options =>
{
    options.UseMySql(connectionString, serverVersion, mysqlOptions =>
    {
        mysqlOptions.EnableRetryOnFailure(3);
        mysqlOptions.CommandTimeout(30);
    });
    options.EnableSensitiveDataLogging(false);
    options.EnableServiceProviderCaching();
});
```

This deployment guide provides comprehensive instructions for deploying StrideHR in various environments with proper security, monitoring, and backup strategies.

## Quick Start Deployment

### Development Environment (5 minutes)

```bash
# Clone repository
git clone https://github.com/your-org/stridehr.git
cd stridehr

# Start with Docker Compose
docker-compose up -d

# Access the application
# Frontend: http://localhost:4200
# API: http://localhost:5000
# Swagger: http://localhost:5000/api-docs
```

### Production Environment (30 minutes)

```bash
# 1. Prepare environment
sudo apt update && sudo apt upgrade -y
sudo apt install docker.io docker-compose nginx certbot -y

# 2. Clone and configure
git clone https://github.com/your-org/stridehr.git
cd stridehr
cp .env.example .env
# Edit .env with production values

# 3. Deploy
docker-compose -f docker-compose.prod.yml up -d

# 4. Setup SSL
sudo certbot --nginx -d your-domain.com

# 5. Verify deployment
curl https://your-domain.com/health
```

This deployment guide provides comprehensive instructions for deploying StrideHR in various environments with proper security, monitoring, and backup strategies.