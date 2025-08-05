# StrideHR DevOps Guide

This document provides comprehensive guidance for deploying, monitoring, and maintaining the StrideHR application in production environments.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Docker Containers](#docker-containers)
3. [Environment Configuration](#environment-configuration)
4. [Deployment](#deployment)
5. [Monitoring and Logging](#monitoring-and-logging)
6. [CI/CD Pipelines](#cicd-pipelines)
7. [Backup and Recovery](#backup-and-recovery)
8. [Security](#security)
9. [Troubleshooting](#troubleshooting)

## Quick Start

### Prerequisites

- Docker 20.10+
- Docker Compose 2.0+
- PowerShell 5.1+ (Windows) or Bash (Linux/macOS)
- 4GB+ RAM
- 20GB+ disk space

### Local Development Setup

```powershell
# Clone the repository
git clone https://github.com/your-org/stridehr.git
cd stridehr

# Copy environment file
cp .env.example .env

# Start all services
docker-compose up -d

# Access the application
# Frontend: http://localhost:4200
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Production Deployment

```powershell
# Deploy to production
.\scripts\deploy.ps1 -Environment production -Version latest

# Start monitoring
.\scripts\start-monitoring.ps1
```

## Docker Containers

### Container Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Nginx Proxy   │    │   Frontend      │    │   Backend API   │
│   (Port 80/443) │────│   (Angular)     │────│   (.NET 8)      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                        │
                       ┌─────────────────┐    ┌─────────────────┐
                       │     Redis       │    │     MySQL       │
                       │   (Caching)     │────│   (Database)    │
                       └─────────────────┘    └─────────────────┘
```

### Available Containers

| Container | Purpose | Port | Health Check |
|-----------|---------|------|--------------|
| stridehr-nginx-prod | Reverse proxy & SSL termination | 80, 443 | `/health` |
| stridehr-frontend-prod | Angular SPA | 80 | `/` |
| stridehr-api-prod | .NET Web API | 8080 | `/health` |
| stridehr-mysql-prod | MySQL database | 3306 | `mysqladmin ping` |
| stridehr-redis-prod | Redis cache | 6379 | `redis-cli ping` |

### Container Images

#### Development Images
- Built locally from source
- Include development tools
- Larger image size

#### Production Images
- Multi-stage builds for optimization
- Security hardened
- Minimal attack surface
- Non-root users

## Environment Configuration

### Environment Files

Create environment-specific configuration files:

- `.env.development` - Local development
- `.env.staging` - Staging environment
- `.env.production` - Production environment

### Required Environment Variables

```bash
# Database Configuration
MYSQL_ROOT_PASSWORD=secure-root-password
MYSQL_DATABASE=StrideHR
MYSQL_USER=stridehr
MYSQL_PASSWORD=secure-db-password

# Redis Configuration
REDIS_PASSWORD=secure-redis-password

# JWT Configuration
JWT_SECRET_KEY=your-256-bit-secret-key-here
JWT_ISSUER=StrideHR
JWT_AUDIENCE=StrideHR-Users

# Email Configuration
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@company.com
SMTP_PASSWORD=your-app-password

# Monitoring
GRAFANA_USER=admin
GRAFANA_PASSWORD=secure-grafana-password
```

### SSL Configuration

For production deployments with HTTPS:

1. Place SSL certificates in `docker/nginx/ssl/`
2. Update nginx configuration
3. Ensure certificates are renewed automatically

## Deployment

### Deployment Strategies

#### 1. Simple Deployment (Staging)
- Direct container replacement
- Brief downtime during deployment
- Suitable for staging environments

#### 2. Blue-Green Deployment (Production)
- Zero-downtime deployment
- Two identical environments (blue/green)
- Traffic switching after validation

#### 3. Rolling Deployment
- Gradual container replacement
- Minimal downtime
- Suitable for high-availability requirements

### Deployment Scripts

#### PowerShell (Windows)
```powershell
# Deploy to production
.\scripts\deploy.ps1 -Environment production -Version v1.2.0

# Deploy to staging
.\scripts\deploy.ps1 -Environment staging -Version latest

# Rollback deployment
.\scripts\deploy.ps1 -Environment production -Rollback
```

#### Bash (Linux/macOS)
```bash
# Deploy to production
./scripts/deploy.sh production v1.2.0

# Deploy to staging
./scripts/deploy.sh staging latest
```

### Pre-deployment Checklist

- [ ] Database backup completed
- [ ] Environment variables configured
- [ ] SSL certificates valid
- [ ] Health checks passing
- [ ] Monitoring alerts configured
- [ ] Rollback plan prepared

### Post-deployment Verification

1. **Health Checks**
   ```bash
   curl -f http://localhost:5000/health
   curl -f http://localhost:4200
   ```

2. **Database Connectivity**
   ```bash
   docker exec stridehr-mysql-prod mysqladmin ping
   ```

3. **Application Functionality**
   - Login functionality
   - API endpoints
   - Real-time features (SignalR)

## Monitoring and Logging

### Monitoring Stack

The monitoring stack includes:

- **Prometheus** - Metrics collection
- **Grafana** - Visualization and dashboards
- **Alertmanager** - Alert management
- **Loki** - Log aggregation
- **Promtail** - Log shipping
- **Node Exporter** - System metrics
- **cAdvisor** - Container metrics

### Starting Monitoring

```powershell
# Start monitoring stack
.\scripts\start-monitoring.ps1

# View logs
.\scripts\start-monitoring.ps1 -Logs

# Stop monitoring
.\scripts\start-monitoring.ps1 -Down
```

### Key Metrics to Monitor

1. **Application Metrics**
   - Response time (95th percentile < 2s)
   - Error rate (< 1%)
   - Throughput (requests/second)
   - Active users

2. **Infrastructure Metrics**
   - CPU usage (< 80%)
   - Memory usage (< 85%)
   - Disk usage (< 85%)
   - Network I/O

3. **Database Metrics**
   - Connection count
   - Query performance
   - Replication lag
   - Lock waits

### Alerting Rules

Critical alerts are configured for:
- Container down
- High error rate (> 5%)
- Database connectivity issues
- SSL certificate expiry
- High resource usage

### Log Management

Logs are collected from:
- Application logs (structured JSON)
- Web server logs (nginx)
- Database logs (MySQL)
- System logs

Log retention: 30 days (configurable)

## CI/CD Pipelines

### GitHub Actions

The CI/CD pipeline includes:

1. **Test Stage**
   - Backend unit tests
   - Frontend unit tests
   - Security scanning

2. **Build Stage**
   - Docker image building
   - Image scanning
   - Registry push

3. **Deploy Stage**
   - Staging deployment (develop branch)
   - Production deployment (main branch)

### Pipeline Configuration

```yaml
# .github/workflows/ci-cd.yml
name: StrideHR CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]
```

### Azure DevOps

Alternative pipeline configuration available in `azure-pipelines.yml`.

## Backup and Recovery

### Database Backup

#### Automated Backups
```powershell
# Create backup
.\scripts\backup-db.ps1 -Environment production

# Compressed backup
.\scripts\backup-db.ps1 -Environment production -Compress
```

#### Backup Schedule
- Daily backups at 2 AM UTC
- Weekly full backups
- Monthly archive backups
- Retention: 30 days daily, 12 weeks weekly, 12 months monthly

#### Backup Verification
- Automated backup testing
- Restore verification
- Integrity checks

### Disaster Recovery

#### Recovery Time Objectives (RTO)
- Database: 15 minutes
- Application: 30 minutes
- Full system: 1 hour

#### Recovery Point Objectives (RPO)
- Database: 1 hour
- File uploads: 24 hours

#### Recovery Procedures

1. **Database Recovery**
   ```bash
   # Restore from backup
   docker exec -i stridehr-mysql-prod mysql -u root -p StrideHR < backup.sql
   ```

2. **Application Recovery**
   ```bash
   # Redeploy application
   ./scripts/deploy.sh production latest
   ```

## Security

### Security Measures

1. **Container Security**
   - Non-root users
   - Minimal base images
   - Regular security updates
   - Vulnerability scanning

2. **Network Security**
   - Private networks
   - Firewall rules
   - SSL/TLS encryption
   - Rate limiting

3. **Data Security**
   - Encrypted data at rest
   - Encrypted data in transit
   - Regular security audits
   - Access logging

### Security Checklist

- [ ] SSL certificates configured
- [ ] Database credentials secured
- [ ] API keys rotated regularly
- [ ] Security headers configured
- [ ] Rate limiting enabled
- [ ] Audit logging active

## Troubleshooting

### Common Issues

#### 1. Container Won't Start
```bash
# Check container logs
docker logs stridehr-api-prod

# Check container status
docker ps -a

# Restart container
docker restart stridehr-api-prod
```

#### 2. Database Connection Issues
```bash
# Test database connectivity
docker exec stridehr-mysql-prod mysqladmin ping

# Check database logs
docker logs stridehr-mysql-prod

# Verify credentials
docker exec -it stridehr-mysql-prod mysql -u stridehr -p
```

#### 3. High Memory Usage
```bash
# Check memory usage
docker stats

# Restart services
docker-compose restart
```

#### 4. SSL Certificate Issues
```bash
# Check certificate expiry
openssl x509 -in cert.pem -text -noout | grep "Not After"

# Renew certificate
certbot renew
```

### Performance Optimization

1. **Database Optimization**
   - Index optimization
   - Query performance tuning
   - Connection pooling

2. **Application Optimization**
   - Caching strategies
   - Code optimization
   - Resource compression

3. **Infrastructure Optimization**
   - Load balancing
   - CDN implementation
   - Auto-scaling

### Support Contacts

- **DevOps Team**: devops@stridehr.com
- **Development Team**: dev@stridehr.com
- **Emergency**: +1-555-STRIDEHR

### Useful Commands

```bash
# View all containers
docker ps -a

# View container logs
docker logs -f container-name

# Execute command in container
docker exec -it container-name bash

# View system resources
docker system df

# Clean up unused resources
docker system prune -f

# Backup database
docker exec mysql-container mysqldump -u user -p database > backup.sql

# Restore database
docker exec -i mysql-container mysql -u user -p database < backup.sql
```

---

For additional support or questions, please refer to the project documentation or contact the DevOps team.