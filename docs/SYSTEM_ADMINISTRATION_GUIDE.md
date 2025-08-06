# StrideHR System Administration Guide

## Table of Contents

1. [System Overview](#system-overview)
2. [User Management](#user-management)
3. [Organization Configuration](#organization-configuration)
4. [Security Management](#security-management)
5. [Integration Management](#integration-management)
6. [System Monitoring](#system-monitoring)
7. [Backup and Recovery](#backup-and-recovery)
8. [Performance Optimization](#performance-optimization)
9. [Troubleshooting](#troubleshooting)
10. [Maintenance Procedures](#maintenance-procedures)

## System Overview

### Architecture Components

#### Application Tier
- **Frontend**: Angular 17+ application served via Nginx
- **Backend**: .NET 8 Web API with Entity Framework Core
- **Real-time**: SignalR hubs for live notifications
- **Authentication**: JWT-based authentication with refresh tokens

#### Data Tier
- **Primary Database**: MySQL 8.0+ for application data
- **Cache Layer**: Redis 7.0+ for session and application caching
- **File Storage**: Local filesystem or cloud storage (S3, Azure Blob)

#### Infrastructure
- **Containerization**: Docker containers with Docker Compose
- **Reverse Proxy**: Nginx for SSL termination and load balancing
- **Monitoring**: Prometheus, Grafana, and custom health checks

### System Requirements

#### Production Environment
- **CPU**: 8+ cores (Intel Xeon or AMD EPYC)
- **RAM**: 32GB+ (64GB recommended for large organizations)
- **Storage**: 1TB+ NVMe SSD with RAID 1 configuration
- **Network**: 1Gbps+ connection with redundancy
- **OS**: Ubuntu 20.04 LTS or CentOS 8+ (Linux recommended)

#### Development Environment
- **CPU**: 4+ cores
- **RAM**: 16GB+
- **Storage**: 500GB+ SSD
- **Network**: Stable broadband connection
- **OS**: Windows 10/11, macOS 10.15+, or Linux

## User Management

### User Account Administration

#### Creating User Accounts

1. **Via Web Interface**
   ```
   Navigate to: Administration > User Management > Create User
   
   Required Information:
   - Email address (unique identifier)
   - First and last name
   - Employee ID (if applicable)
   - Branch assignment
   - Department and designation
   - Reporting manager
   - Role assignment
   ```

2. **Via Bulk Import**
   ```
   Navigate to: Administration > Data Import > Users
   
   CSV Format:
   Email,FirstName,LastName,EmployeeId,BranchId,Department,Designation,ManagerEmail,Role
   john.doe@company.com,John,Doe,EMP001,1,IT,Developer,manager@company.com,Employee
   ```

3. **Via API**
   ```bash
   curl -X POST "https://api.stridehr.com/api/admin/users" \
     -H "Authorization: Bearer {admin-token}" \
     -H "Content-Type: application/json" \
     -d '{
       "email": "user@company.com",
       "firstName": "John",
       "lastName": "Doe",
       "employeeId": "EMP001",
       "branchId": 1,
       "department": "IT",
       "designation": "Developer",
       "managerId": 123,
       "roleId": 2
     }'
   ```

#### User Role Management

**Available Roles:**
- **Employee**: Basic user with self-service capabilities
- **Team Lead**: Employee + team management functions
- **Manager**: Team Lead + department oversight
- **HR Manager**: Manager + HR administrative functions
- **Branch Manager**: HR Manager + branch-wide authority
- **System Administrator**: Full system access and configuration

**Role Assignment Process:**
1. Navigate to **Administration** > **User Management**
2. Select the user to modify
3. Click **Edit User** > **Role Assignment**
4. Select appropriate role(s)
5. Set effective dates if needed
6. Save changes and notify user

#### User Deactivation and Reactivation

**Deactivation Process:**
1. Navigate to **Administration** > **User Management**
2. Find the user account
3. Click **Deactivate User**
4. Select deactivation reason:
   - Resignation
   - Termination
   - Leave of absence
   - Other (specify)
5. Set effective date
6. Choose data retention options
7. Confirm deactivation

**Reactivation Process:**
1. Navigate to **Administration** > **Inactive Users**
2. Find the deactivated user
3. Click **Reactivate User**
4. Update user information if needed
5. Reassign roles and permissions
6. Set new effective date
7. Send reactivation notification

### Permission Management

#### Role-Based Access Control (RBAC)

**Permission Categories:**
- **Employee Data**: View, edit, create, delete employee records
- **Attendance**: Manage attendance policies and records
- **Leave**: Approve leave requests and manage policies
- **Payroll**: Process payroll and manage salary structures
- **Reports**: Generate and access various reports
- **System**: Configure system settings and integrations

**Custom Permission Sets:**
1. Navigate to **Administration** > **Permissions** > **Custom Roles**
2. Click **Create Custom Role**
3. Define role name and description
4. Select specific permissions:
   ```
   Employee Management:
   ☑ View Employee Profiles
   ☑ Edit Employee Basic Info
   ☐ Edit Employee Salary
   ☐ Delete Employee Records
   
   Attendance Management:
   ☑ View Team Attendance
   ☑ Approve Attendance Corrections
   ☐ Modify Attendance Policies
   
   Leave Management:
   ☑ Approve Leave Requests
   ☑ View Leave Reports
   ☐ Modify Leave Policies
   ```
5. Save custom role
6. Assign to users as needed

#### Branch-Based Access Control

**Multi-Branch Configuration:**
1. Navigate to **Administration** > **Organization** > **Branches**
2. Configure branch hierarchy
3. Set branch-specific permissions
4. Assign users to branches
5. Configure cross-branch access rules

**Branch Isolation Settings:**
```json
{
  "branchIsolation": {
    "enabled": true,
    "allowCrossBranchView": false,
    "allowCrossBranchReports": true,
    "exemptRoles": ["SystemAdmin", "HRManager"],
    "sharedResources": ["Policies", "Templates"]
  }
}
```

## Organization Configuration

### Company Setup

#### Basic Organization Information
1. Navigate to **Administration** > **Organization** > **Company Profile**
2. Configure basic details:
   ```
   Company Name: Your Company Ltd.
   Registration Number: REG123456
   Tax ID: TAX789012
   Address: Complete business address
   Contact Information: Phone, email, website
   Logo: Upload company logo (PNG/JPG, max 2MB)
   ```

#### Branch Management
1. Navigate to **Administration** > **Organization** > **Branches**
2. Add new branch:
   ```
   Branch Name: Head Office
   Branch Code: HO001
   Address: Branch address
   Manager: Assign branch manager
   Timezone: Local timezone
   Working Hours: Configure standard hours
   Currency: Local currency
   ```

#### Department Structure
1. Navigate to **Administration** > **Organization** > **Departments**
2. Create department hierarchy:
   ```
   IT Department
   ├── Software Development
   │   ├── Frontend Team
   │   └── Backend Team
   ├── Infrastructure
   └── Quality Assurance
   
   HR Department
   ├── Recruitment
   ├── Employee Relations
   └── Payroll
   ```

### Policy Configuration

#### Attendance Policies
1. Navigate to **Administration** > **Policies** > **Attendance**
2. Configure working hours:
   ```json
   {
     "standardWorkingHours": {
       "monday": {"start": "09:00", "end": "18:00"},
       "tuesday": {"start": "09:00", "end": "18:00"},
       "wednesday": {"start": "09:00", "end": "18:00"},
       "thursday": {"start": "09:00", "end": "18:00"},
       "friday": {"start": "09:00", "end": "18:00"},
       "saturday": {"enabled": false},
       "sunday": {"enabled": false}
     },
     "flexibleHours": {
       "enabled": true,
       "coreHours": {"start": "10:00", "end": "16:00"},
       "maxFlexTime": 2
     },
     "overtime": {
       "enabled": true,
       "autoCalculate": true,
       "multiplier": 1.5
     }
   }
   ```

3. Configure attendance rules:
   ```json
   {
     "lateArrival": {
       "graceMinutes": 15,
       "penaltyAfter": 30,
       "maxLatePerMonth": 3
     },
     "earlyDeparture": {
       "graceMinutes": 15,
       "penaltyAfter": 30
     },
     "breakRules": {
       "maxBreakTime": 60,
       "mandatoryBreaks": ["12:00-13:00"]
     }
   }
   ```

#### Leave Policies
1. Navigate to **Administration** > **Policies** > **Leave**
2. Configure leave types:
   ```json
   {
     "leaveTypes": [
       {
         "name": "Annual Leave",
         "code": "AL",
         "accrualRate": 2.5,
         "maxAccrual": 30,
         "carryForward": 5,
         "encashable": true
       },
       {
         "name": "Sick Leave",
         "code": "SL",
         "accrualRate": 1.0,
         "maxAccrual": 12,
         "carryForward": 0,
         "encashable": false
       },
       {
         "name": "Maternity Leave",
         "code": "ML",
         "entitlement": 90,
         "eligibilityMonths": 12,
         "paidDays": 90
       }
     ]
   }
   ```

3. Configure approval workflows:
   ```json
   {
     "approvalWorkflow": {
       "singleLevel": {
         "approver": "DirectManager",
         "autoApprove": false
       },
       "multiLevel": {
         "level1": "DirectManager",
         "level2": "HRManager",
         "escalationDays": 3
       },
       "emergencyLeave": {
         "autoApprove": true,
         "maxDays": 2,
         "requiresDocumentation": true
       }
     }
   }
   ```

#### Payroll Policies
1. Navigate to **Administration** > **Policies** > **Payroll**
2. Configure salary components:
   ```json
   {
     "salaryComponents": {
       "basic": {
         "percentage": 50,
         "taxable": true,
         "pfApplicable": true
       },
       "hra": {
         "percentage": 30,
         "taxExempt": true,
         "condition": "rentPaid"
       },
       "transport": {
         "fixedAmount": 2000,
         "taxExempt": true,
         "maxExemption": 1600
       }
     },
     "deductions": {
       "pf": {
         "employeeContribution": 12,
         "employerContribution": 12,
         "maxSalary": 15000
       },
       "esi": {
         "rate": 0.75,
         "maxSalary": 21000
       }
     }
   }
   ```

## Security Management

### Authentication Configuration

#### Password Policies
1. Navigate to **Administration** > **Security** > **Password Policy**
2. Configure password requirements:
   ```json
   {
     "passwordPolicy": {
       "minLength": 8,
       "maxLength": 128,
       "requireUppercase": true,
       "requireLowercase": true,
       "requireNumbers": true,
       "requireSpecialChars": true,
       "preventCommonPasswords": true,
       "preventUserInfoInPassword": true,
       "passwordHistory": 5,
       "maxAge": 90,
       "warningDays": 7
     }
   }
   ```

#### Multi-Factor Authentication (MFA)
1. Navigate to **Administration** > **Security** > **MFA Settings**
2. Configure MFA options:
   ```json
   {
     "mfaSettings": {
       "enabled": true,
       "mandatory": false,
       "methods": ["TOTP", "SMS", "Email"],
       "backupCodes": {
         "enabled": true,
         "count": 10
       },
       "trustedDevices": {
         "enabled": true,
         "maxDevices": 5,
         "trustDuration": 30
       }
     }
   }
   ```

#### Session Management
1. Navigate to **Administration** > **Security** > **Session Settings**
2. Configure session parameters:
   ```json
   {
     "sessionSettings": {
       "timeout": 480,
       "warningMinutes": 5,
       "maxConcurrentSessions": 3,
       "rememberMeDuration": 30,
       "secureOnly": true,
       "sameSite": "Strict"
     }
   }
   ```

### Access Control

#### IP Restrictions
1. Navigate to **Administration** > **Security** > **IP Access Control**
2. Configure IP whitelist/blacklist:
   ```json
   {
     "ipAccessControl": {
       "enabled": true,
       "mode": "whitelist",
       "allowedRanges": [
         "192.168.1.0/24",
         "10.0.0.0/8",
         "203.0.113.0/24"
       ],
       "blockedIPs": [
         "192.168.1.100",
         "10.0.0.50"
       ],
       "exemptRoles": ["SystemAdmin"]
     }
   }
   ```

#### API Security
1. Navigate to **Administration** > **Security** > **API Settings**
2. Configure API security:
   ```json
   {
     "apiSecurity": {
       "rateLimiting": {
         "enabled": true,
         "requestsPerMinute": 100,
         "burstLimit": 200
       },
       "cors": {
         "enabled": true,
         "allowedOrigins": [
           "https://yourcompany.stridehr.com",
           "https://mobile.yourcompany.com"
         ]
       },
       "apiKeys": {
         "required": true,
         "rotation": 90
       }
     }
   }
   ```

### Audit and Compliance

#### Audit Logging
1. Navigate to **Administration** > **Security** > **Audit Settings**
2. Configure audit logging:
   ```json
   {
     "auditSettings": {
       "enabled": true,
       "logLevel": "Information",
       "events": [
         "UserLogin",
         "UserLogout",
         "PasswordChange",
         "RoleChange",
         "DataModification",
         "SystemConfiguration"
       ],
       "retention": 365,
       "encryption": true
     }
   }
   ```

#### Compliance Reports
1. Navigate to **Administration** > **Compliance** > **Reports**
2. Generate compliance reports:
   - User access reports
   - Data modification logs
   - Security incident reports
   - Policy compliance reports

## Integration Management

### External System Integrations

#### Payroll System Integration
1. Navigate to **Administration** > **Integrations** > **Payroll Systems**
2. Configure payroll integration:
   ```json
   {
     "payrollIntegration": {
       "system": "ADP",
       "apiEndpoint": "https://api.adp.com/hr/v2",
       "authentication": {
         "type": "OAuth2",
         "clientId": "your-client-id",
         "clientSecret": "your-client-secret"
       },
       "syncSchedule": "0 2 * * *",
       "dataMapping": {
         "employeeId": "associateOID",
         "salary": "basePay.payAmount",
         "department": "businessCommunication.department"
       }
     }
   }
   ```

#### Calendar Integration
1. Navigate to **Administration** > **Integrations** > **Calendar**
2. Configure calendar integration:
   ```json
   {
     "calendarIntegration": {
       "providers": ["Google", "Outlook"],
       "google": {
         "clientId": "google-client-id",
         "clientSecret": "google-client-secret",
         "scopes": ["calendar.readonly", "calendar.events"]
       },
       "outlook": {
         "clientId": "outlook-client-id",
         "clientSecret": "outlook-client-secret",
         "tenantId": "tenant-id"
       },
       "syncFrequency": 15
     }
   }
   ```

#### Single Sign-On (SSO)
1. Navigate to **Administration** > **Integrations** > **SSO**
2. Configure SSO providers:
   ```json
   {
     "ssoProviders": {
       "saml": {
         "enabled": true,
         "identityProvider": "https://sso.yourcompany.com",
         "certificate": "-----BEGIN CERTIFICATE-----...",
         "attributeMapping": {
           "email": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
           "firstName": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname",
           "lastName": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"
         }
       },
       "oauth": {
         "enabled": true,
         "provider": "Azure AD",
         "clientId": "azure-client-id",
         "tenantId": "azure-tenant-id"
       }
     }
   }
   ```

### Webhook Management

#### Webhook Configuration
1. Navigate to **Administration** > **Integrations** > **Webhooks**
2. Create webhook subscriptions:
   ```json
   {
     "webhook": {
       "name": "HR System Integration",
       "url": "https://external-system.com/webhooks/stridehr",
       "secret": "webhook-secret-key",
       "events": [
         "employee.created",
         "employee.updated",
         "attendance.checked_in",
         "leave.approved"
       ],
       "retryPolicy": {
         "maxRetries": 5,
         "backoffMultiplier": 2,
         "initialDelay": 60
       }
     }
   }
   ```

#### Webhook Testing
```bash
# Test webhook delivery
curl -X POST "https://api.stridehr.com/api/admin/webhooks/test" \
  -H "Authorization: Bearer {admin-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "webhookId": "webhook-id",
    "eventType": "test.event",
    "testData": {
      "message": "Test webhook delivery"
    }
  }'
```

## System Monitoring

### Health Monitoring

#### Health Check Endpoints
Monitor these endpoints for system health:

```bash
# Overall system health
curl https://api.stridehr.com/health

# Database connectivity
curl https://api.stridehr.com/health/database

# Redis cache status
curl https://api.stridehr.com/health/redis

# External integrations
curl https://api.stridehr.com/health/integrations
```

#### Performance Metrics
1. Navigate to **Administration** > **Monitoring** > **Performance**
2. Monitor key metrics:
   - Response times
   - Error rates
   - Database performance
   - Memory usage
   - CPU utilization

### Alerting Configuration

#### Alert Rules
1. Navigate to **Administration** > **Monitoring** > **Alerts**
2. Configure alert rules:
   ```json
   {
     "alertRules": [
       {
         "name": "High Error Rate",
         "condition": "error_rate > 5%",
         "duration": "5m",
         "severity": "critical",
         "notifications": ["email", "slack"]
       },
       {
         "name": "Database Connection Issues",
         "condition": "database_connections < 1",
         "duration": "1m",
         "severity": "critical",
         "notifications": ["email", "sms"]
       },
       {
         "name": "High Memory Usage",
         "condition": "memory_usage > 85%",
         "duration": "10m",
         "severity": "warning",
         "notifications": ["email"]
       }
     ]
   }
   ```

#### Notification Channels
Configure notification channels:
```json
{
  "notificationChannels": {
    "email": {
      "enabled": true,
      "recipients": ["admin@company.com", "ops@company.com"],
      "smtpServer": "smtp.company.com"
    },
    "slack": {
      "enabled": true,
      "webhookUrl": "https://hooks.slack.com/services/...",
      "channel": "#alerts"
    },
    "sms": {
      "enabled": true,
      "provider": "Twilio",
      "recipients": ["+1234567890"]
    }
  }
}
```

### Log Management

#### Log Configuration
1. Navigate to **Administration** > **System** > **Logging**
2. Configure log levels and destinations:
   ```json
   {
     "logging": {
       "level": "Information",
       "destinations": ["File", "Database", "ElasticSearch"],
       "fileLogging": {
         "path": "/var/log/stridehr/",
         "maxFileSize": "100MB",
         "maxFiles": 10
       },
       "databaseLogging": {
         "enabled": true,
         "retention": 90
       },
       "elasticSearch": {
         "enabled": true,
         "endpoint": "https://elasticsearch.company.com",
         "index": "stridehr-logs"
       }
     }
   }
   ```

## Backup and Recovery

### Backup Configuration

#### Automated Backup Setup
1. Navigate to **Administration** > **System** > **Backup**
2. Configure backup schedules:
   ```json
   {
     "backupSchedule": {
       "database": {
         "frequency": "daily",
         "time": "02:00",
         "retention": 30,
         "compression": true,
         "encryption": true
       },
       "files": {
         "frequency": "weekly",
         "time": "03:00",
         "retention": 12,
         "includeUploads": true
       },
       "configuration": {
         "frequency": "daily",
         "time": "01:00",
         "retention": 90
       }
     }
   }
   ```

#### Backup Storage
Configure backup storage locations:
```json
{
  "backupStorage": {
    "local": {
      "enabled": true,
      "path": "/backup/stridehr/",
      "maxSize": "500GB"
    },
    "cloud": {
      "enabled": true,
      "provider": "AWS S3",
      "bucket": "stridehr-backups",
      "region": "us-east-1",
      "encryption": "AES256"
    },
    "offsite": {
      "enabled": true,
      "provider": "Azure Blob",
      "container": "stridehr-offsite",
      "replication": "GRS"
    }
  }
}
```

### Recovery Procedures

#### Database Recovery
```bash
# Stop application services
docker-compose -f docker-compose.prod.yml stop api

# Restore database from backup
mysql -u root -p stridehr_prod < /backup/stridehr_db_20250806.sql

# Restart services
docker-compose -f docker-compose.prod.yml start api

# Verify recovery
curl -f https://api.stridehr.com/health/database
```

#### File Recovery
```bash
# Stop application
docker-compose -f docker-compose.prod.yml down

# Restore files
tar -xzf /backup/stridehr_files_20250806.tar.gz -C /app/

# Restart application
docker-compose -f docker-compose.prod.yml up -d

# Verify recovery
curl -f https://api.stridehr.com/health
```

## Performance Optimization

### Database Optimization

#### Query Performance
1. Navigate to **Administration** > **System** > **Database**
2. Monitor slow queries:
   ```sql
   -- Enable slow query log
   SET GLOBAL slow_query_log = 'ON';
   SET GLOBAL long_query_time = 2;
   
   -- Analyze slow queries
   SELECT 
     query_time,
     lock_time,
     rows_sent,
     rows_examined,
     sql_text
   FROM mysql.slow_log
   ORDER BY query_time DESC
   LIMIT 10;
   ```

#### Index Optimization
```sql
-- Check for missing indexes
SELECT 
  table_schema,
  table_name,
  column_name,
  cardinality
FROM information_schema.statistics
WHERE table_schema = 'stridehr_prod'
  AND cardinality < 100;

-- Add recommended indexes
CREATE INDEX idx_employee_branch ON Employees(BranchId);
CREATE INDEX idx_attendance_date ON Attendance(Date, EmployeeId);
CREATE INDEX idx_leave_status ON LeaveRequests(Status, SubmittedDate);
```

### Application Performance

#### Caching Strategy
1. Navigate to **Administration** > **System** > **Caching**
2. Configure cache settings:
   ```json
   {
     "caching": {
       "redis": {
         "enabled": true,
         "connectionString": "localhost:6379",
         "database": 0,
         "keyPrefix": "stridehr:",
         "defaultExpiration": 3600
       },
       "memoryCache": {
         "enabled": true,
         "maxSize": "500MB",
         "slidingExpiration": 1800
       },
       "cacheStrategies": {
         "employees": {
           "type": "redis",
           "expiration": 7200,
           "refreshOnAccess": true
         },
         "policies": {
           "type": "memory",
           "expiration": 86400,
           "refreshOnUpdate": true
         }
       }
     }
   }
   ```

#### Connection Pooling
Configure database connection pooling:
```json
{
  "connectionPooling": {
    "minPoolSize": 5,
    "maxPoolSize": 100,
    "connectionTimeout": 30,
    "commandTimeout": 300,
    "connectionLifetime": 3600,
    "retryCount": 3
  }
}
```

## Troubleshooting

### Common Issues

#### Authentication Problems
**Issue**: Users cannot login
**Diagnosis**:
```bash
# Check authentication service
curl -X POST "https://api.stridehr.com/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@company.com","password":"test123"}'

# Check JWT configuration
grep JWT_SECRET /app/.env.production

# Check database connectivity
mysql -h db-server -u stridehr_user -p -e "SELECT COUNT(*) FROM Users;"
```

**Solutions**:
1. Verify JWT secret key configuration
2. Check database connectivity
3. Validate user account status
4. Review authentication logs

#### Performance Issues
**Issue**: Slow response times
**Diagnosis**:
```bash
# Check system resources
top
free -m
df -h

# Check database performance
mysql -e "SHOW PROCESSLIST;"
mysql -e "SHOW ENGINE INNODB STATUS;"

# Check application logs
docker logs stridehr-api | grep -i "slow\|timeout\|error"
```

**Solutions**:
1. Optimize database queries
2. Increase server resources
3. Configure caching
4. Review application code

#### Integration Failures
**Issue**: External integrations not working
**Diagnosis**:
```bash
# Test external API connectivity
curl -v https://external-api.com/health

# Check integration logs
grep "integration" /var/log/stridehr/application.log

# Verify credentials
curl -X POST "https://external-api.com/auth" \
  -H "Content-Type: application/json" \
  -d '{"clientId":"your-id","clientSecret":"your-secret"}'
```

**Solutions**:
1. Verify API credentials
2. Check network connectivity
3. Review API rate limits
4. Update integration configuration

### Diagnostic Tools

#### System Health Check Script
```bash
#!/bin/bash
# system-health-check.sh

echo "=== StrideHR System Health Check ==="
echo "Date: $(date)"
echo

# Check services
echo "=== Service Status ==="
docker-compose -f docker-compose.prod.yml ps

# Check disk space
echo "=== Disk Usage ==="
df -h

# Check memory usage
echo "=== Memory Usage ==="
free -m

# Check database connectivity
echo "=== Database Connectivity ==="
mysql -h db-server -u stridehr_user -p -e "SELECT 1;" 2>/dev/null && echo "Database: OK" || echo "Database: FAILED"

# Check Redis connectivity
echo "=== Redis Connectivity ==="
redis-cli -h cache-server ping 2>/dev/null && echo "Redis: OK" || echo "Redis: FAILED"

# Check API health
echo "=== API Health ==="
curl -s -f https://api.stridehr.com/health >/dev/null && echo "API: OK" || echo "API: FAILED"

echo "=== Health Check Complete ==="
```

## Maintenance Procedures

### Regular Maintenance Tasks

#### Daily Tasks
```bash
#!/bin/bash
# daily-maintenance.sh

# Check system health
./system-health-check.sh

# Backup database
./backup-database.sh

# Clean up logs
find /var/log/stridehr/ -name "*.log" -mtime +7 -delete

# Check disk space
df -h | awk '$5 > 80 {print "Warning: " $1 " is " $5 " full"}'

# Update system packages (security only)
apt update && apt list --upgradable | grep -i security
```

#### Weekly Tasks
```bash
#!/bin/bash
# weekly-maintenance.sh

# Full system backup
./backup-full-system.sh

# Database optimization
mysql -u root -p -e "OPTIMIZE TABLE stridehr_prod.Users, stridehr_prod.Employees, stridehr_prod.Attendance;"

# Clean up temporary files
docker system prune -f

# Check SSL certificate expiration
certbot certificates | grep -i "expires"

# Generate weekly reports
./generate-weekly-reports.sh
```

#### Monthly Tasks
```bash
#!/bin/bash
# monthly-maintenance.sh

# System updates
apt update && apt upgrade -y

# Database maintenance
mysql -u root -p -e "ANALYZE TABLE stridehr_prod.Users, stridehr_prod.Employees, stridehr_prod.Attendance;"

# Security audit
./security-audit.sh

# Performance review
./performance-analysis.sh

# Backup verification
./verify-backups.sh
```

### Update Procedures

#### Application Updates
```bash
#!/bin/bash
# update-application.sh

# Backup current version
./backup-full-system.sh

# Pull latest code
git pull origin production

# Build new images
docker-compose -f docker-compose.prod.yml build --no-cache

# Run database migrations
cd backend
dotnet ef database update

# Deploy new version
docker-compose -f docker-compose.prod.yml up -d

# Verify deployment
./verify-deployment.sh

# Rollback if needed
# ./rollback-deployment.sh
```

#### Security Updates
```bash
#!/bin/bash
# security-updates.sh

# Update system packages
apt update && apt upgrade -y

# Update Docker images
docker-compose -f docker-compose.prod.yml pull

# Restart services
docker-compose -f docker-compose.prod.yml up -d

# Verify security
./security-check.sh
```

---

**Document Version**: 1.0  
**Last Updated**: August 6, 2025  
**Next Review**: February 6, 2026  
**Maintained By**: System Administration Team