# StrideHR Infrastructure Setup and Maintenance Guide

## Overview

This document provides comprehensive instructions for setting up and maintaining the infrastructure required for StrideHR production deployment. It covers server provisioning, network configuration, security hardening, and ongoing maintenance procedures.

## Infrastructure Architecture

### Production Architecture Overview
```
Internet
    ↓
Load Balancer (Optional)
    ↓
Reverse Proxy (Nginx)
    ↓
┌─────────────────────────────────────┐
│           Application Server        │
│  ┌─────────────┐  ┌─────────────┐  │
│  │   Frontend  │  │   Backend   │  │
│  │  (Angular)  │  │  (.NET API) │  │
│  └─────────────┘  └─────────────┘  │
└─────────────────────────────────────┘
    ↓                    ↓
┌─────────────┐    ┌─────────────┐
│   Database  │    │    Redis    │
│   (MySQL)   │    │   (Cache)   │
└─────────────┘    └─────────────┘
```

### Component Requirements

#### Application Server
- **Purpose**: Hosts the main StrideHR application
- **Specifications**: 8+ CPU cores, 32GB+ RAM, 1TB+ SSD
- **OS**: Ubuntu 20.04 LTS or CentOS 8+
- **Services**: Docker, Nginx, Application containers

#### Database Server
- **Purpose**: Primary data storage
- **Specifications**: 4+ CPU cores, 16GB+ RAM, 500GB+ SSD
- **OS**: Ubuntu 20.04 LTS or CentOS 8+
- **Services**: MySQL 8.0+, Automated backups

#### Cache Server
- **Purpose**: Session storage and application caching
- **Specifications**: 2+ CPU cores, 8GB+ RAM, 100GB+ SSD
- **OS**: Ubuntu 20.04 LTS or CentOS 8+
- **Services**: Redis 7.0+

## Server Provisioning

### 1. Application Server Setup

#### Initial Server Configuration
```bash
#!/bin/bash
# Application Server Setup Script

# Update system
apt update && apt upgrade -y

# Install essential packages
apt install -y curl wget git unzip software-properties-common \
    htop iotop nethogs fail2ban ufw logrotate

# Configure timezone
timedatectl set-timezone UTC

# Create application user
useradd -m -s /bin/bash stridehr
usermod -aG sudo stridehr

# Configure SSH security
sed -i 's/#PermitRootLogin yes/PermitRootLogin no/' /etc/ssh/sshd_config
sed -i 's/#PasswordAuthentication yes/PasswordAuthentication no/' /etc/ssh/sshd_config
systemctl restart sshd

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh
usermod -aG docker stridehr

# Install Docker Compose
curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose

# Install Nginx
apt install -y nginx
systemctl enable nginx

# Configure firewall
ufw --force enable
ufw allow 22/tcp
ufw allow 80/tcp
ufw allow 443/tcp

echo "Application server setup complete"
```

#### Docker Configuration
```bash
# Configure Docker daemon
cat > /etc/docker/daemon.json << EOF
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  },
  "storage-driver": "overlay2",
  "live-restore": true
}
EOF

# Restart Docker
systemctl restart docker
systemctl enable docker
```

### 2. Database Server Setup

#### MySQL Installation and Configuration
```bash
#!/bin/bash
# Database Server Setup Script

# Update system
apt update && apt upgrade -y

# Install MySQL 8.0
apt install -y mysql-server-8.0

# Secure MySQL installation
mysql_secure_installation

# Configure MySQL
cat > /etc/mysql/mysql.conf.d/stridehr.cnf << EOF
[mysqld]
# Basic Settings
bind-address = 0.0.0.0
port = 3306
max_connections = 200
max_allowed_packet = 64M

# InnoDB Settings
innodb_buffer_pool_size = 8G
innodb_log_file_size = 512M
innodb_flush_log_at_trx_commit = 2
innodb_flush_method = O_DIRECT

# Query Cache
query_cache_type = 1
query_cache_size = 256M

# Logging
log_error = /var/log/mysql/error.log
slow_query_log = 1
slow_query_log_file = /var/log/mysql/slow.log
long_query_time = 2

# Binary Logging for Replication
log_bin = /var/log/mysql/mysql-bin.log
binlog_format = ROW
expire_logs_days = 7

# SSL Configuration
ssl-ca = /etc/mysql/ssl/ca-cert.pem
ssl-cert = /etc/mysql/ssl/server-cert.pem
ssl-key = /etc/mysql/ssl/server-key.pem
EOF

# Restart MySQL
systemctl restart mysql
systemctl enable mysql

# Configure firewall
ufw allow from application-server-ip to any port 3306

echo "Database server setup complete"
```

#### Database Security Configuration
```sql
-- Create application database and user
CREATE DATABASE stridehr_prod CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create application user with limited privileges
CREATE USER 'stridehr_app'@'application-server-ip' IDENTIFIED BY 'secure-random-password';
GRANT SELECT, INSERT, UPDATE, DELETE ON stridehr_prod.* TO 'stridehr_app'@'application-server-ip';

-- Create backup user
CREATE USER 'stridehr_backup'@'localhost' IDENTIFIED BY 'backup-password';
GRANT SELECT, LOCK TABLES, SHOW VIEW, EVENT, TRIGGER ON stridehr_prod.* TO 'stridehr_backup'@'localhost';

-- Create monitoring user
CREATE USER 'stridehr_monitor'@'%' IDENTIFIED BY 'monitor-password';
GRANT PROCESS, REPLICATION CLIENT ON *.* TO 'stridehr_monitor'@'%';

FLUSH PRIVILEGES;
```

### 3. Cache Server Setup

#### Redis Installation and Configuration
```bash
#!/bin/bash
# Cache Server Setup Script

# Update system
apt update && apt upgrade -y

# Install Redis
apt install -y redis-server

# Configure Redis
cat > /etc/redis/redis.conf << EOF
# Network Configuration
bind 0.0.0.0
port 6379
protected-mode yes
requirepass secure-redis-password

# Memory Configuration
maxmemory 6gb
maxmemory-policy allkeys-lru

# Persistence Configuration
save 900 1
save 300 10
save 60 10000
rdbcompression yes
rdbchecksum yes
dbfilename dump.rdb
dir /var/lib/redis

# Security
rename-command FLUSHDB ""
rename-command FLUSHALL ""
rename-command DEBUG ""
rename-command CONFIG "CONFIG_b835729c9f"

# Logging
loglevel notice
logfile /var/log/redis/redis-server.log

# Client Configuration
timeout 300
tcp-keepalive 300
maxclients 10000
EOF

# Set proper permissions
chown redis:redis /etc/redis/redis.conf
chmod 640 /etc/redis/redis.conf

# Restart Redis
systemctl restart redis-server
systemctl enable redis-server

# Configure firewall
ufw allow from application-server-ip to any port 6379

echo "Cache server setup complete"
```

## Network Configuration

### 1. DNS Configuration

#### Domain Setup
```bash
# Example DNS records for your-domain.com
# A Record: your-domain.com → server-ip-address
# A Record: www.your-domain.com → server-ip-address
# CNAME Record: api.your-domain.com → your-domain.com
```

#### SSL Certificate Setup
```bash
# Install Certbot
apt install -y certbot python3-certbot-nginx

# Obtain SSL certificate
certbot --nginx -d your-domain.com -d www.your-domain.com -d api.your-domain.com

# Set up automatic renewal
echo "0 12 * * * /usr/bin/certbot renew --quiet" | crontab -
```

### 2. Load Balancer Configuration (Optional)

#### Nginx Load Balancer
```nginx
# /etc/nginx/sites-available/stridehr-lb
upstream stridehr_backend {
    server app-server-1:5000 weight=1 max_fails=3 fail_timeout=30s;
    server app-server-2:5000 weight=1 max_fails=3 fail_timeout=30s;
    keepalive 32;
}

server {
    listen 80;
    server_name your-domain.com www.your-domain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name your-domain.com www.your-domain.com;

    ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;

    # SSL Configuration
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;

    # Security Headers
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    # API Proxy
    location /api/ {
        proxy_pass http://stridehr_backend/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 86400;
    }

    # Frontend
    location / {
        proxy_pass http://stridehr_backend/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

## Security Hardening

### 1. System Security

#### Fail2Ban Configuration
```bash
# Install Fail2Ban
apt install -y fail2ban

# Configure Fail2Ban for SSH
cat > /etc/fail2ban/jail.local << EOF
[DEFAULT]
bantime = 3600
findtime = 600
maxretry = 3
backend = systemd

[sshd]
enabled = true
port = ssh
logpath = %(sshd_log)s
maxretry = 3

[nginx-http-auth]
enabled = true
filter = nginx-http-auth
logpath = /var/log/nginx/error.log
maxretry = 3

[nginx-limit-req]
enabled = true
filter = nginx-limit-req
logpath = /var/log/nginx/error.log
maxretry = 3
EOF

systemctl enable fail2ban
systemctl start fail2ban
```

#### System Updates and Patches
```bash
# Configure automatic security updates
apt install -y unattended-upgrades

cat > /etc/apt/apt.conf.d/50unattended-upgrades << EOF
Unattended-Upgrade::Allowed-Origins {
    "\${distro_id}:\${distro_codename}-security";
    "\${distro_id}ESMApps:\${distro_codename}-apps-security";
    "\${distro_id}ESM:\${distro_codename}-infra-security";
};

Unattended-Upgrade::AutoFixInterruptedDpkg "true";
Unattended-Upgrade::MinimalSteps "true";
Unattended-Upgrade::Remove-Unused-Dependencies "true";
Unattended-Upgrade::Automatic-Reboot "false";
EOF

# Enable automatic updates
echo 'APT::Periodic::Update-Package-Lists "1";' > /etc/apt/apt.conf.d/20auto-upgrades
echo 'APT::Periodic::Unattended-Upgrade "1";' >> /etc/apt/apt.conf.d/20auto-upgrades
```

### 2. Application Security

#### Environment Security
```bash
# Secure environment files
chmod 600 /home/stridehr/stridehr/.env.production
chown stridehr:stridehr /home/stridehr/stridehr/.env.production

# Secure SSL certificates
chmod 600 /home/stridehr/stridehr/docker/nginx/ssl/*
chown -R stridehr:stridehr /home/stridehr/stridehr/docker/nginx/ssl/
```

#### Database Security
```sql
-- Regular security maintenance
-- Remove unused accounts
DROP USER IF EXISTS 'test'@'localhost';
DROP USER IF EXISTS ''@'localhost';

-- Update passwords regularly
ALTER USER 'stridehr_app'@'application-server-ip' IDENTIFIED BY 'new-secure-password';

-- Review and audit permissions
SELECT User, Host, authentication_string FROM mysql.user;
SHOW GRANTS FOR 'stridehr_app'@'application-server-ip';
```

## Monitoring and Alerting

### 1. System Monitoring

#### Prometheus Configuration
```yaml
# prometheus.yml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  - "alert_rules.yml"

alerting:
  alertmanagers:
    - static_configs:
        - targets:
          - alertmanager:9093

scrape_configs:
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']

  - job_name: 'node-exporter'
    static_configs:
      - targets: ['localhost:9100']

  - job_name: 'mysql-exporter'
    static_configs:
      - targets: ['db-server:9104']

  - job_name: 'redis-exporter'
    static_configs:
      - targets: ['cache-server:9121']

  - job_name: 'stridehr-api'
    static_configs:
      - targets: ['app-server:5000']
    metrics_path: '/metrics'
```

#### Alert Rules
```yaml
# alert_rules.yml
groups:
  - name: system_alerts
    rules:
      - alert: HighCPUUsage
        expr: 100 - (avg by(instance) (irate(node_cpu_seconds_total{mode="idle"}[5m])) * 100) > 80
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High CPU usage detected"
          description: "CPU usage is above 80% for more than 5 minutes"

      - alert: HighMemoryUsage
        expr: (node_memory_MemTotal_bytes - node_memory_MemAvailable_bytes) / node_memory_MemTotal_bytes * 100 > 85
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High memory usage detected"
          description: "Memory usage is above 85% for more than 5 minutes"

      - alert: DiskSpaceLow
        expr: (node_filesystem_avail_bytes / node_filesystem_size_bytes) * 100 < 10
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "Low disk space"
          description: "Disk space is below 10%"

  - name: application_alerts
    rules:
      - alert: ApplicationDown
        expr: up{job="stridehr-api"} == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "StrideHR application is down"
          description: "The StrideHR API is not responding"

      - alert: DatabaseDown
        expr: mysql_up == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Database is down"
          description: "MySQL database is not responding"

      - alert: RedisDown
        expr: redis_up == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Redis is down"
          description: "Redis cache server is not responding"
```

### 2. Log Management

#### Centralized Logging with ELK Stack
```yaml
# docker-compose.monitoring.yml
version: '3.8'
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.8.0
    environment:
      - discovery.type=single-node
      - "ES_JAVA_OPTS=-Xms1g -Xmx1g"
    volumes:
      - elasticsearch_data:/usr/share/elasticsearch/data
    ports:
      - "9200:9200"

  logstash:
    image: docker.elastic.co/logstash/logstash:8.8.0
    volumes:
      - ./logstash/config:/usr/share/logstash/pipeline
    ports:
      - "5044:5044"
    depends_on:
      - elasticsearch

  kibana:
    image: docker.elastic.co/kibana/kibana:8.8.0
    ports:
      - "5601:5601"
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    depends_on:
      - elasticsearch

volumes:
  elasticsearch_data:
```

## Backup and Recovery

### 1. Automated Backup Scripts

#### Database Backup Script
```bash
#!/bin/bash
# /home/stridehr/scripts/backup-database.sh

# Configuration
DB_HOST="database-server"
DB_USER="stridehr_backup"
DB_PASSWORD="backup-password"
DB_NAME="stridehr_prod"
BACKUP_DIR="/home/stridehr/backups/database"
RETENTION_DAYS=30

# Create backup directory
mkdir -p $BACKUP_DIR

# Generate backup filename
BACKUP_FILE="stridehr_db_$(date +%Y%m%d_%H%M%S).sql.gz"

# Create backup
mysqldump -h $DB_HOST -u $DB_USER -p$DB_PASSWORD \
    --single-transaction \
    --routines \
    --triggers \
    --events \
    $DB_NAME | gzip > $BACKUP_DIR/$BACKUP_FILE

# Verify backup
if [ $? -eq 0 ]; then
    echo "Database backup completed successfully: $BACKUP_FILE"
    
    # Upload to cloud storage
    aws s3 cp $BACKUP_DIR/$BACKUP_FILE s3://your-backup-bucket/database/
    
    # Clean up old backups
    find $BACKUP_DIR -name "stridehr_db_*.sql.gz" -mtime +$RETENTION_DAYS -delete
else
    echo "Database backup failed!"
    exit 1
fi
```

#### Application Files Backup Script
```bash
#!/bin/bash
# /home/stridehr/scripts/backup-files.sh

# Configuration
APP_DIR="/home/stridehr/stridehr"
BACKUP_DIR="/home/stridehr/backups/files"
RETENTION_DAYS=7

# Create backup directory
mkdir -p $BACKUP_DIR

# Generate backup filename
BACKUP_FILE="stridehr_files_$(date +%Y%m%d_%H%M%S).tar.gz"

# Create backup excluding unnecessary files
tar -czf $BACKUP_DIR/$BACKUP_FILE \
    --exclude='node_modules' \
    --exclude='bin' \
    --exclude='obj' \
    --exclude='.git' \
    --exclude='logs' \
    -C $APP_DIR .

# Verify backup
if [ $? -eq 0 ]; then
    echo "Files backup completed successfully: $BACKUP_FILE"
    
    # Upload to cloud storage
    aws s3 cp $BACKUP_DIR/$BACKUP_FILE s3://your-backup-bucket/files/
    
    # Clean up old backups
    find $BACKUP_DIR -name "stridehr_files_*.tar.gz" -mtime +$RETENTION_DAYS -delete
else
    echo "Files backup failed!"
    exit 1
fi
```

### 2. Backup Scheduling

#### Crontab Configuration
```bash
# Edit crontab for stridehr user
crontab -e

# Add backup schedules
# Database backup - daily at 2:00 AM
0 2 * * * /home/stridehr/scripts/backup-database.sh >> /home/stridehr/logs/backup.log 2>&1

# Files backup - weekly on Sunday at 3:00 AM
0 3 * * 0 /home/stridehr/scripts/backup-files.sh >> /home/stridehr/logs/backup.log 2>&1

# Configuration backup - monthly on 1st at 4:00 AM
0 4 1 * * /home/stridehr/scripts/backup-config.sh >> /home/stridehr/logs/backup.log 2>&1
```

## Maintenance Procedures

### 1. Regular Maintenance Tasks

#### Weekly Maintenance Checklist
```bash
#!/bin/bash
# /home/stridehr/scripts/weekly-maintenance.sh

echo "Starting weekly maintenance - $(date)"

# System updates
apt update && apt list --upgradable

# Docker cleanup
docker system prune -f
docker volume prune -f

# Log rotation
logrotate -f /etc/logrotate.conf

# Check disk space
df -h

# Check service status
systemctl status nginx mysql redis-server docker

# Check SSL certificate expiration
certbot certificates

# Database optimization
mysql -u root -p -e "OPTIMIZE TABLE stridehr_prod.Users, stridehr_prod.Organizations, stridehr_prod.Employees;"

# Check backup integrity
ls -la /home/stridehr/backups/database/ | tail -5
ls -la /home/stridehr/backups/files/ | tail -5

echo "Weekly maintenance completed - $(date)"
```

#### Monthly Maintenance Checklist
```bash
#!/bin/bash
# /home/stridehr/scripts/monthly-maintenance.sh

echo "Starting monthly maintenance - $(date)"

# Security updates
unattended-upgrade -d

# Full system backup verification
/home/stridehr/scripts/test-backup-restore.sh

# Performance analysis
iostat -x 1 10 > /home/stridehr/logs/iostat-$(date +%Y%m).log
sar -u 1 10 > /home/stridehr/logs/sar-$(date +%Y%m).log

# Security audit
lynis audit system --quick

# SSL certificate renewal test
certbot renew --dry-run

# Database maintenance
mysql -u root -p -e "ANALYZE TABLE stridehr_prod.Users, stridehr_prod.Organizations, stridehr_prod.Employees;"

echo "Monthly maintenance completed - $(date)"
```

### 2. Performance Optimization

#### Database Performance Tuning
```sql
-- Monthly database optimization queries
-- Analyze table statistics
ANALYZE TABLE Users, Organizations, Employees, Attendance, Payroll;

-- Check for unused indexes
SELECT 
    s.table_schema,
    s.table_name,
    s.index_name,
    s.cardinality
FROM information_schema.statistics s
LEFT JOIN information_schema.index_statistics i 
    ON s.table_schema = i.table_schema 
    AND s.table_name = i.table_name 
    AND s.index_name = i.index_name
WHERE s.table_schema = 'stridehr_prod'
    AND i.index_name IS NULL
    AND s.index_name != 'PRIMARY';

-- Check slow queries
SELECT 
    query_time,
    lock_time,
    rows_sent,
    rows_examined,
    sql_text
FROM mysql.slow_log
WHERE start_time > DATE_SUB(NOW(), INTERVAL 7 DAY)
ORDER BY query_time DESC
LIMIT 10;
```

#### Application Performance Monitoring
```bash
# Monitor application performance
docker stats --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}\t{{.BlockIO}}"

# Check response times
curl -w "@curl-format.txt" -o /dev/null -s https://your-domain.com/api/health

# Monitor database connections
mysql -u stridehr_monitor -p -e "SHOW PROCESSLIST;"

# Check Redis performance
redis-cli --latency-history -i 1
```

## Disaster Recovery

### 1. Recovery Procedures

#### Complete System Recovery
```bash
#!/bin/bash
# /home/stridehr/scripts/disaster-recovery.sh

echo "Starting disaster recovery procedure - $(date)"

# Step 1: Prepare new server
# (Assumes new server is provisioned and basic setup is complete)

# Step 2: Restore application files
aws s3 cp s3://your-backup-bucket/files/latest-backup.tar.gz /tmp/
tar -xzf /tmp/latest-backup.tar.gz -C /home/stridehr/stridehr/

# Step 3: Restore database
aws s3 cp s3://your-backup-bucket/database/latest-backup.sql.gz /tmp/
gunzip /tmp/latest-backup.sql.gz
mysql -u root -p stridehr_prod < /tmp/latest-backup.sql

# Step 4: Restore SSL certificates
certbot --nginx -d your-domain.com -d www.your-domain.com

# Step 5: Start services
cd /home/stridehr/stridehr
docker-compose -f docker-compose.prod.yml up -d

# Step 6: Verify recovery
sleep 30
curl -f https://your-domain.com/api/health

echo "Disaster recovery completed - $(date)"
```

### 2. Recovery Testing

#### Monthly Recovery Test
```bash
#!/bin/bash
# /home/stridehr/scripts/test-recovery.sh

# Create test environment
docker-compose -f docker-compose.test.yml up -d

# Restore latest backup to test environment
latest_backup=$(ls -t /home/stridehr/backups/database/*.sql.gz | head -1)
gunzip -c $latest_backup | mysql -h test-db -u root -p stridehr_test

# Test application functionality
curl -f http://test-server:5000/api/health

# Cleanup test environment
docker-compose -f docker-compose.test.yml down -v

echo "Recovery test completed successfully"
```

## Support and Documentation

### Contact Information
- **Infrastructure Team**: infrastructure@your-company.com
- **Database Administrator**: dba@your-company.com
- **Security Team**: security@your-company.com
- **On-call Support**: +1-XXX-XXX-XXXX

### External Vendors
- **Cloud Provider**: [Support contact and SLA details]
- **Domain Registrar**: [Support contact]
- **SSL Certificate Authority**: [Support contact]
- **Monitoring Service**: [Support contact]

---

**Document Version**: 1.0  
**Last Updated**: August 6, 2025  
**Next Review**: February 6, 2026  
**Approved By**: [Infrastructure Team Lead]