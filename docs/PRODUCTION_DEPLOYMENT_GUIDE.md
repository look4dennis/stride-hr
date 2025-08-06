# StrideHR Production Deployment Guide

## Overview

This guide provides step-by-step instructions for deploying StrideHR to a production environment. It covers infrastructure setup, security configuration, environment preparation, and deployment procedures to ensure a reliable and secure production deployment.

## Prerequisites

### System Requirements

#### Server Specifications
- **CPU**: Minimum 4 cores, Recommended 8+ cores
- **RAM**: Minimum 16GB, Recommended 32GB+
- **Storage**: Minimum 500GB SSD, Recommended 1TB+ NVMe SSD
- **Network**: Minimum 1Gbps connection
- **OS**: Ubuntu 20.04 LTS or CentOS 8+ (Linux recommended)

#### Software Dependencies
- Docker Engine 24.0+
- Docker Compose 2.20+
- Git 2.30+
- OpenSSL 1.1.1+
- Nginx 1.18+ (if using as reverse proxy)

### Domain and SSL Requirements
- Registered domain name
- Valid SSL certificate (Let's Encrypt or commercial)
- DNS access for domain configuration

### External Services
- MySQL 8.0+ database server (or cloud equivalent)
- Redis 7.0+ cache server (or cloud equivalent)
- SMTP server for email notifications
- Cloud storage for file uploads (AWS S3, Azure Blob, etc.)

## Pre-Deployment Checklist

### Infrastructure Preparation
- [ ] Server provisioned with required specifications
- [ ] Operating system updated and secured
- [ ] Firewall configured with required ports
- [ ] Domain DNS configured to point to server
- [ ] SSL certificate obtained and validated
- [ ] Database server accessible and configured
- [ ] Redis server accessible and configured
- [ ] SMTP server configured for email delivery
- [ ] Cloud storage configured for file uploads

### Security Preparation
- [ ] SSH key-based authentication configured
- [ ] Root access disabled
- [ ] Fail2ban or equivalent intrusion prevention installed
- [ ] System monitoring tools installed
- [ ] Backup procedures configured
- [ ] Security headers configured
- [ ] Rate limiting configured

## Environment Configuration

### 1. Server Setup

#### Initial Server Configuration
```bash
# Update system packages
sudo apt update && sudo apt upgrade -y

# Install required packages
sudo apt install -y curl wget git unzip software-properties-common

# Create application user
sudo useradd -m -s /bin/bash stridehr
sudo usermod -aG sudo stridehr

# Configure SSH for application user
sudo mkdir -p /home/stridehr/.ssh
sudo cp ~/.ssh/authorized_keys /home/stridehr/.ssh/
sudo chown -R stridehr:stridehr /home/stridehr/.ssh
sudo chmod 700 /home/stridehr/.ssh
sudo chmod 600 /home/stridehr/.ssh/authorized_keys
```

#### Docker Installation
```bash
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Add user to docker group
sudo usermod -aG docker stridehr

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Verify installation
docker --version
docker-compose --version
```

### 2. Application Setup

#### Clone Repository
```bash
# Switch to application user
sudo su - stridehr

# Clone the repository
git clone https://github.com/your-organization/stridehr.git
cd stridehr

# Create production branch if needed
git checkout -b production
```

#### Environment Configuration
```bash
# Copy production environment template
cp .env.production.template .env.production

# Edit production environment file
nano .env.production
```

#### Production Environment Variables (.env.production)
```bash
# Application Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000
API_BASE_URL=https://your-domain.com/api

# Database Configuration
DB_CONNECTION_STRING="Server=your-db-server;Database=stridehr_prod;User=stridehr_user;Password=your-secure-password;SslMode=Required;"
DB_HOST=your-db-server
DB_PORT=3306
DB_NAME=stridehr_prod
DB_USER=stridehr_user
DB_PASSWORD=your-secure-password

# Redis Configuration
REDIS_CONNECTION_STRING=your-redis-server:6379
REDIS_PASSWORD=your-redis-password

# JWT Configuration
JWT_SECRET_KEY=your-256-bit-secret-key-here
JWT_ISSUER=https://your-domain.com
JWT_AUDIENCE=https://your-domain.com
JWT_EXPIRY_MINUTES=60

# Email Configuration
SMTP_HOST=your-smtp-server
SMTP_PORT=587
SMTP_USERNAME=your-smtp-username
SMTP_PASSWORD=your-smtp-password
SMTP_FROM_EMAIL=noreply@your-domain.com
SMTP_FROM_NAME=StrideHR System

# File Storage Configuration
STORAGE_TYPE=S3  # Options: Local, S3, Azure
AWS_ACCESS_KEY_ID=your-aws-access-key
AWS_SECRET_ACCESS_KEY=your-aws-secret-key
AWS_REGION=us-east-1
AWS_S3_BUCKET=your-s3-bucket-name

# Security Configuration
CORS_ORIGINS=https://your-domain.com
ALLOWED_HOSTS=your-domain.com,www.your-domain.com
ENABLE_HTTPS_REDIRECT=true
ENABLE_HSTS=true

# Monitoring Configuration
ENABLE_HEALTH_CHECKS=true
ENABLE_METRICS=true
LOG_LEVEL=Information

# SignalR Configuration
SIGNALR_REDIS_CONNECTION=your-redis-server:6379
```

### 3. SSL Certificate Configuration

#### Using Let's Encrypt (Recommended)
```bash
# Install Certbot
sudo apt install -y certbot python3-certbot-nginx

# Obtain SSL certificate
sudo certbot certonly --standalone -d your-domain.com -d www.your-domain.com

# Verify certificate
sudo certbot certificates
```

#### SSL Certificate Files
```bash
# Create SSL directory
sudo mkdir -p /home/stridehr/stridehr/docker/nginx/ssl

# Copy certificates
sudo cp /etc/letsencrypt/live/your-domain.com/fullchain.pem /home/stridehr/stridehr/docker/nginx/ssl/
sudo cp /etc/letsencrypt/live/your-domain.com/privkey.pem /home/stridehr/stridehr/docker/nginx/ssl/

# Set proper permissions
sudo chown -R stridehr:stridehr /home/stridehr/stridehr/docker/nginx/ssl
sudo chmod 600 /home/stridehr/stridehr/docker/nginx/ssl/*
```

### 4. Database Setup

#### Database Creation
```sql
-- Connect to MySQL as root
CREATE DATABASE stridehr_prod CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create application user
CREATE USER 'stridehr_user'@'%' IDENTIFIED BY 'your-secure-password';

-- Grant permissions
GRANT ALL PRIVILEGES ON stridehr_prod.* TO 'stridehr_user'@'%';
FLUSH PRIVILEGES;

-- Verify connection
SELECT User, Host FROM mysql.user WHERE User = 'stridehr_user';
```

#### Database Migration
```bash
# Run database migrations
cd /home/stridehr/stridehr/backend
dotnet ef database update --connection "Server=your-db-server;Database=stridehr_prod;User=stridehr_user;Password=your-secure-password;SslMode=Required;"
```

## Deployment Process

### 1. Pre-Deployment Validation

#### System Health Check
```bash
# Check system resources
df -h
free -m
docker system df

# Check network connectivity
ping -c 4 your-db-server
ping -c 4 your-redis-server

# Validate environment configuration
cd /home/stridehr/stridehr
./scripts/validate-production-env.ps1
```

#### Security Validation
```bash
# Check SSL certificate validity
openssl x509 -in docker/nginx/ssl/fullchain.pem -text -noout

# Validate firewall rules
sudo ufw status

# Check file permissions
ls -la .env.production
ls -la docker/nginx/ssl/
```

### 2. Build and Deploy

#### Build Application
```bash
# Build backend
cd backend
dotnet publish -c Release -o ../publish/backend

# Build frontend
cd ../frontend
npm ci --production
npm run build:prod

# Build Docker images
cd ..
docker-compose -f docker-compose.prod.yml build --no-cache
```

#### Deploy Services
```bash
# Start services
docker-compose -f docker-compose.prod.yml up -d

# Verify services are running
docker-compose -f docker-compose.prod.yml ps

# Check service logs
docker-compose -f docker-compose.prod.yml logs -f
```

### 3. Post-Deployment Validation

#### Health Check Validation
```bash
# Test API health endpoint
curl -f https://your-domain.com/api/health

# Test database connectivity
curl -f https://your-domain.com/api/health/database

# Test Redis connectivity
curl -f https://your-domain.com/api/health/redis

# Test authentication endpoint
curl -X POST https://your-domain.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@your-domain.com","password":"admin-password"}'
```

#### Frontend Validation
```bash
# Test frontend accessibility
curl -f https://your-domain.com

# Test API integration
curl -f https://your-domain.com/api/swagger

# Test file upload functionality
# (Use appropriate test file and authentication)
```

#### Performance Validation
```bash
# Test response times
curl -w "@curl-format.txt" -o /dev/null -s https://your-domain.com/api/health

# Test concurrent connections
ab -n 100 -c 10 https://your-domain.com/api/health
```

## Security Configuration

### 1. Firewall Configuration

#### UFW (Ubuntu Firewall)
```bash
# Enable firewall
sudo ufw enable

# Allow SSH
sudo ufw allow 22/tcp

# Allow HTTP and HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Allow specific application ports if needed
sudo ufw allow 5000/tcp  # API port (if directly exposed)

# Check firewall status
sudo ufw status verbose
```

### 2. Nginx Security Configuration

#### Security Headers
```nginx
# Add to nginx configuration
add_header X-Frame-Options "SAMEORIGIN" always;
add_header X-Content-Type-Options "nosniff" always;
add_header X-XSS-Protection "1; mode=block" always;
add_header Referrer-Policy "strict-origin-when-cross-origin" always;
add_header Content-Security-Policy "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self' wss: https:;" always;
add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
```

### 3. Application Security

#### JWT Security
- Use strong, randomly generated secret keys (minimum 256 bits)
- Set appropriate token expiration times
- Implement token refresh mechanisms
- Use secure HTTP-only cookies for token storage

#### Database Security
- Use strong, unique passwords
- Enable SSL/TLS for database connections
- Implement connection pooling with limits
- Regular security updates and patches

## Monitoring and Maintenance

### 1. System Monitoring

#### Health Check Endpoints
- `/api/health` - Overall system health
- `/api/health/database` - Database connectivity
- `/api/health/redis` - Cache system status
- `/api/health/storage` - File storage connectivity

#### Log Monitoring
```bash
# Application logs
docker-compose -f docker-compose.prod.yml logs -f api

# Nginx access logs
tail -f /var/log/nginx/access.log

# System logs
journalctl -f -u docker
```

### 2. Backup Procedures

#### Automated Backups
```bash
# Database backup (daily)
0 2 * * * /home/stridehr/stridehr/scripts/backup-database.ps1

# File backup (weekly)
0 3 * * 0 /home/stridehr/stridehr/scripts/backup-files.ps1

# Configuration backup (monthly)
0 4 1 * * /home/stridehr/stridehr/scripts/backup-config.ps1
```

### 3. Update Procedures

#### Application Updates
```bash
# Pull latest changes
git pull origin production

# Rebuild and deploy
docker-compose -f docker-compose.prod.yml build --no-cache
docker-compose -f docker-compose.prod.yml up -d

# Verify deployment
./scripts/validate-production-env.ps1
```

#### Security Updates
```bash
# System updates
sudo apt update && sudo apt upgrade -y

# Docker updates
sudo apt update docker-ce docker-ce-cli containerd.io

# SSL certificate renewal
sudo certbot renew --dry-run
```

## Troubleshooting

### Common Issues

#### Service Won't Start
```bash
# Check service status
docker-compose -f docker-compose.prod.yml ps

# Check service logs
docker-compose -f docker-compose.prod.yml logs service-name

# Check system resources
df -h
free -m
docker system df
```

#### Database Connection Issues
```bash
# Test database connectivity
mysql -h your-db-server -u stridehr_user -p

# Check database service status
systemctl status mysql  # If running locally

# Verify connection string
grep DB_CONNECTION_STRING .env.production
```

#### SSL Certificate Issues
```bash
# Check certificate validity
openssl x509 -in docker/nginx/ssl/fullchain.pem -text -noout

# Test SSL configuration
openssl s_client -connect your-domain.com:443

# Renew certificate if expired
sudo certbot renew
```

### Performance Issues

#### High CPU Usage
```bash
# Check container resource usage
docker stats

# Check system processes
top -p $(docker inspect --format='{{.State.Pid}}' container-name)

# Scale services if needed
docker-compose -f docker-compose.prod.yml up -d --scale api=2
```

#### High Memory Usage
```bash
# Check memory usage by container
docker stats --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}"

# Check for memory leaks
docker exec container-name cat /proc/meminfo

# Restart services if needed
docker-compose -f docker-compose.prod.yml restart
```

## Rollback Procedures

### Application Rollback
```bash
# Stop current services
docker-compose -f docker-compose.prod.yml down

# Revert to previous version
git checkout previous-stable-tag

# Rebuild and deploy
docker-compose -f docker-compose.prod.yml build --no-cache
docker-compose -f docker-compose.prod.yml up -d

# Verify rollback
./scripts/validate-production-env.ps1
```

### Database Rollback
```bash
# Stop application services
docker-compose -f docker-compose.prod.yml stop api

# Restore database from backup
./scripts/restore-database.ps1 -BackupFile "backups/pre-deployment-backup.sql.gz"

# Restart services
docker-compose -f docker-compose.prod.yml start api
```

## Support and Escalation

### Contact Information
- **Primary Administrator**: [Name, Phone, Email]
- **Database Administrator**: [Name, Phone, Email]
- **Security Officer**: [Name, Phone, Email]
- **Development Team Lead**: [Name, Phone, Email]

### Escalation Matrix
| Issue Severity | Response Time | Contact |
|----------------|---------------|---------|
| Critical (System Down) | 15 minutes | All team members |
| High (Major Feature Down) | 30 minutes | Primary Admin + DBA |
| Medium (Minor Issues) | 2 hours | Primary Admin |
| Low (Enhancement Requests) | 24 hours | Development Team |

### External Support
- **Cloud Provider**: [Support contact and procedures]
- **Domain Registrar**: [Support contact]
- **SSL Certificate Authority**: [Support contact]
- **Third-party Services**: [List of all external service contacts]

---

**Document Version**: 1.0  
**Last Updated**: August 6, 2025  
**Next Review**: February 6, 2026  
**Approved By**: [System Administrator]