# StrideHR Troubleshooting Guide

## Table of Contents

1. [Quick Diagnostic Steps](#quick-diagnostic-steps)
2. [Login and Authentication Issues](#login-and-authentication-issues)
3. [Attendance Management Issues](#attendance-management-issues)
4. [Leave Management Issues](#leave-management-issues)
5. [Payroll Issues](#payroll-issues)
6. [Performance Issues](#performance-issues)
7. [Mobile App Issues](#mobile-app-issues)
8. [Integration Issues](#integration-issues)
9. [System Administration Issues](#system-administration-issues)
10. [Emergency Procedures](#emergency-procedures)

## Quick Diagnostic Steps

### Before You Start Troubleshooting

1. **Check System Status**
   - Visit the system status page (if available)
   - Check for scheduled maintenance notifications
   - Verify your internet connection

2. **Basic Browser Checks**
   - Clear browser cache and cookies
   - Disable browser extensions temporarily
   - Try incognito/private browsing mode
   - Test with a different browser

3. **Gather Information**
   - Note the exact error message
   - Record the time when the issue occurred
   - Document the steps that led to the problem
   - Take screenshots if applicable

### System Health Quick Check

```bash
# For administrators - quick system health check
curl -f https://your-domain.com/api/health
curl -f https://your-domain.com/api/health/database
curl -f https://your-domain.com/api/health/redis
```

## Login and Authentication Issues

### Issue: Cannot Login to the System

#### Symptoms
- Login page shows "Invalid credentials" error
- Page redirects back to login after entering credentials
- "Account locked" or "Account disabled" messages

#### Troubleshooting Steps

**Step 1: Verify Credentials**
1. Check if Caps Lock is enabled
2. Ensure you're using the correct email address
3. Try typing password in a text editor first to verify
4. Check for extra spaces before/after email or password

**Step 2: Password Reset**
1. Click "Forgot Password" on login page
2. Enter your email address
3. Check email inbox (including spam/junk folder)
4. Follow password reset instructions
5. Create a new strong password

**Step 3: Browser Issues**
1. Clear browser cache and cookies:
   - Chrome: Ctrl+Shift+Delete (Windows) or Cmd+Shift+Delete (Mac)
   - Firefox: Ctrl+Shift+Delete (Windows) or Cmd+Shift+Delete (Mac)
   - Safari: Cmd+Option+E (Mac)
2. Disable browser extensions
3. Try incognito/private browsing mode
4. Test with a different browser

**Step 4: Network and Firewall**
1. Check if you can access other websites
2. Try connecting from a different network
3. Contact IT if using corporate network with restrictions

**Step 5: Account Status**
1. Contact your HR administrator to verify account status
2. Check if account has been deactivated
3. Verify if you're assigned to the correct organization/branch

#### Common Solutions

| Problem | Solution |
|---------|----------|
| Forgot password | Use password reset feature |
| Account locked | Contact HR administrator |
| Browser cache issues | Clear cache and cookies |
| Network restrictions | Contact IT support |
| Account deactivated | Contact HR administrator |

### Issue: Multi-Factor Authentication (MFA) Problems

#### Symptoms
- MFA code not working
- Not receiving SMS/email codes
- Authenticator app showing wrong codes

#### Troubleshooting Steps

**Step 1: Time Synchronization**
1. Ensure your device time is correct
2. For authenticator apps, sync time in app settings
3. Check timezone settings

**Step 2: Code Issues**
1. Wait for new code generation (codes expire every 30 seconds)
2. Ensure you're entering the complete 6-digit code
3. Don't include spaces when entering the code

**Step 3: SMS/Email Issues**
1. Check spam/junk folders for email codes
2. Verify phone number is correct in your profile
3. Try requesting a new code

**Step 4: Backup Options**
1. Use backup codes if available
2. Try alternative MFA method (email instead of SMS)
3. Contact administrator for MFA reset

### Issue: Session Timeout Problems

#### Symptoms
- Frequent logouts during work
- "Session expired" messages
- Having to login multiple times per day

#### Troubleshooting Steps

**Step 1: Browser Settings**
1. Enable cookies for the StrideHR domain
2. Don't use "Always use incognito mode"
3. Check if browser is set to clear cookies on exit

**Step 2: Network Issues**
1. Stable internet connection required
2. Avoid switching between networks frequently
3. Contact IT if using VPN with connection drops

**Step 3: Multiple Tabs/Windows**
1. Avoid opening StrideHR in multiple tabs
2. Close unused browser tabs
3. Don't use multiple browsers simultaneously

## Attendance Management Issues

### Issue: Cannot Clock In/Out

#### Symptoms
- Clock in/out button not responding
- Error messages when trying to record attendance
- Location verification failures

#### Troubleshooting Steps

**Step 1: Basic Checks**
1. Refresh the page (F5 or Ctrl+R)
2. Check internet connection
3. Verify current time and date on your device
4. Ensure you're not already clocked in (for clock in issues)

**Step 2: Location Issues (if GPS is required)**
1. Enable location services in your browser:
   - Chrome: Click lock icon in address bar > Location > Allow
   - Firefox: Click shield icon > Permissions > Location > Allow
   - Safari: Safari menu > Preferences > Websites > Location
2. For mobile: Enable location services in device settings
3. Ensure you're within the allowed location radius
4. Try refreshing location by moving slightly

**Step 3: Browser Permissions**
1. Check if browser is blocking location access
2. Clear browser cache and cookies
3. Try different browser
4. Disable ad blockers temporarily

**Step 4: Mobile App Alternative**
1. Download StrideHR mobile app
2. Use app for attendance if web version fails
3. Ensure app has location permissions

#### Common Solutions

| Problem | Solution |
|---------|----------|
| Location not detected | Enable browser location services |
| Already clocked in | Check current status, clock out first |
| Network timeout | Check internet connection, retry |
| GPS accuracy issues | Move to open area, refresh location |

### Issue: Incorrect Attendance Times

#### Symptoms
- Wrong clock in/out times displayed
- Times showing in different timezone
- Missing attendance records

#### Troubleshooting Steps

**Step 1: Timezone Verification**
1. Check your profile timezone settings:
   - Go to My Profile > Settings
   - Verify timezone matches your location
   - Update if incorrect
2. Check browser timezone:
   - Visit timeanddate.com to verify
   - Ensure system time is correct

**Step 2: Attendance Correction**
1. Navigate to Attendance > My Records
2. Find the incorrect entry
3. Click "Request Correction"
4. Provide correct time and explanation
5. Submit for manager approval

**Step 3: System Time Issues**
1. Verify your computer/device time is correct
2. Enable automatic time synchronization
3. Restart browser after time changes

### Issue: Break Time Not Recording

#### Symptoms
- Break timer not starting
- Break time not being calculated
- Cannot end break

#### Troubleshooting Steps

**Step 1: Break Status Check**
1. Verify you're currently clocked in
2. Check if you're already on a break
3. Ensure previous break was properly ended

**Step 2: Browser Issues**
1. Refresh the page
2. Clear browser cache
3. Try different browser

**Step 3: Manual Break Entry**
1. Contact your supervisor
2. Request manual break time entry
3. Provide start and end times

## Leave Management Issues

### Issue: Cannot Submit Leave Request

#### Symptoms
- Submit button not working
- Form validation errors
- "Insufficient leave balance" messages

#### Troubleshooting Steps

**Step 1: Form Validation**
1. Check all required fields are filled:
   - Leave type
   - Start and end dates
   - Reason for leave
   - Emergency contact (if required)
2. Ensure dates are in the future
3. Verify end date is after start date

**Step 2: Leave Balance Check**
1. Navigate to Leave > Leave Balance
2. Verify you have sufficient days for the leave type
3. Check if leave conflicts with existing approved leave

**Step 3: Date Conflicts**
1. Check company calendar for blackout dates
2. Verify no overlapping leave requests
3. Ensure dates don't conflict with public holidays

**Step 4: File Attachments**
1. Check file size limits (usually 5MB max)
2. Ensure file format is supported (PDF, JPG, PNG)
3. Try uploading files one at a time

#### Common Solutions

| Problem | Solution |
|---------|----------|
| Insufficient balance | Check leave balance, adjust dates |
| Date conflicts | Choose different dates |
| File upload issues | Check file size and format |
| Form validation | Complete all required fields |

### Issue: Leave Request Status Not Updating

#### Symptoms
- Request stuck in "Pending" status
- No notification of approval/rejection
- Cannot see manager comments

#### Troubleshooting Steps

**Step 1: Check Request Status**
1. Navigate to Leave > My Requests
2. Click on the specific request for details
3. Check submission date and approval timeline

**Step 2: Manager Notification**
1. Verify your manager received notification
2. Follow up with manager if needed
3. Check if manager is available (not on leave)

**Step 3: Escalation Process**
1. Contact HR if request is overdue
2. Check company policy for approval timelines
3. Provide request reference number

### Issue: Leave Balance Incorrect

#### Symptoms
- Balance doesn't match expected amount
- Recent leave not reflected in balance
- Accrual not updating

#### Troubleshooting Steps

**Step 1: Balance Calculation**
1. Review leave policy for accrual rates
2. Check employment start date
3. Verify leave taken vs. balance

**Step 2: Recent Changes**
1. Check if recent leave has been processed
2. Verify approved leave is deducted
3. Look for any manual adjustments

**Step 3: HR Verification**
1. Contact HR with specific balance concerns
2. Provide employment details and leave history
3. Request balance recalculation if needed

## Payroll Issues

### Issue: Cannot Access Payslips

#### Symptoms
- Payslip not available for current month
- Download link not working
- "Access denied" errors

#### Troubleshooting Steps

**Step 1: Payroll Processing Status**
1. Check if payroll has been processed for the month
2. Verify payroll processing schedule with HR
3. Confirm your employment status during the period

**Step 2: Browser Issues**
1. Try different browser
2. Clear browser cache and cookies
3. Disable popup blockers
4. Enable PDF viewing in browser

**Step 3: File Download Issues**
1. Right-click download link and "Save as"
2. Try downloading during off-peak hours
3. Check available disk space on device

### Issue: Salary Information Incorrect

#### Symptoms
- Wrong salary amount displayed
- Missing allowances or deductions
- Tax calculations seem incorrect

#### Troubleshooting Steps

**Step 1: Salary Structure Review**
1. Navigate to Payroll > Salary Structure
2. Verify basic salary and components
3. Check effective dates for any changes

**Step 2: Payroll Components**
1. Review all allowances (HRA, Transport, etc.)
2. Check deductions (Tax, PF, Insurance)
3. Verify overtime calculations if applicable

**Step 3: HR Consultation**
1. Contact HR with specific discrepancies
2. Provide previous payslips for comparison
3. Request detailed salary breakdown

### Issue: Reimbursement Claims Issues

#### Symptoms
- Cannot submit expense claims
- Claims stuck in pending status
- Reimbursement amount incorrect

#### Troubleshooting Steps

**Step 1: Claim Submission**
1. Ensure all required fields are completed
2. Upload clear, readable receipts
3. Check file size and format requirements
4. Verify expense category is correct

**Step 2: Approval Process**
1. Check claim approval workflow
2. Follow up with approving manager
3. Verify receipt authenticity and amounts

**Step 3: Payment Processing**
1. Check reimbursement processing schedule
2. Verify bank account details are correct
3. Contact finance team for payment status

## Performance Issues

### Issue: System Running Slowly

#### Symptoms
- Pages taking long time to load
- Timeouts when submitting forms
- Unresponsive interface elements

#### Troubleshooting Steps

**Step 1: Network Check**
1. Test internet speed (use speedtest.net)
2. Try accessing other websites
3. Switch to different network if possible
4. Contact ISP if network issues persist

**Step 2: Browser Optimization**
1. Close unnecessary browser tabs
2. Clear browser cache and cookies
3. Disable unnecessary browser extensions
4. Restart browser completely

**Step 3: Device Performance**
1. Close other applications
2. Check available RAM and CPU usage
3. Restart computer if necessary
4. Update browser to latest version

**Step 4: Peak Usage Times**
1. Try accessing during off-peak hours
2. Contact IT if performance issues persist
3. Report specific slow pages or functions

### Issue: Pages Not Loading Properly

#### Symptoms
- Blank or partially loaded pages
- Missing images or styling
- JavaScript errors

#### Troubleshooting Steps

**Step 1: Browser Refresh**
1. Hard refresh: Ctrl+F5 (Windows) or Cmd+Shift+R (Mac)
2. Clear browser cache completely
3. Try incognito/private browsing mode

**Step 2: Browser Compatibility**
1. Update browser to latest version
2. Try different browser (Chrome, Firefox, Safari, Edge)
3. Check if JavaScript is enabled
4. Disable ad blockers temporarily

**Step 3: Network Issues**
1. Check firewall settings
2. Try different network connection
3. Contact IT if using corporate network

## Mobile App Issues

### Issue: App Won't Install or Update

#### Symptoms
- Installation fails from app store
- Update process gets stuck
- "App not compatible" messages

#### Troubleshooting Steps

**Step 1: Device Compatibility**
1. Check minimum OS requirements:
   - iOS: 13.0 or later
   - Android: 8.0 (API level 26) or later
2. Update device OS if needed
3. Ensure sufficient storage space

**Step 2: App Store Issues**
1. Restart device
2. Sign out and back into app store
3. Clear app store cache (Android)
4. Try downloading over Wi-Fi

**Step 3: Alternative Installation**
1. Try downloading at different time
2. Contact IT for enterprise app distribution
3. Use web version as temporary solution

### Issue: GPS/Location Not Working

#### Symptoms
- Cannot clock in due to location issues
- "Location not found" errors
- GPS accuracy problems

#### Troubleshooting Steps

**Step 1: Location Permissions**
1. Check app permissions in device settings
2. Enable "Precise Location" (iOS) or "High Accuracy" (Android)
3. Allow location access "While Using App"

**Step 2: GPS Settings**
1. Enable GPS/Location Services in device settings
2. Ensure location accuracy is set to high
3. Try toggling location services off and on

**Step 3: Environmental Factors**
1. Move to open area with clear sky view
2. Avoid areas with tall buildings or underground locations
3. Wait for GPS to acquire signal (may take 1-2 minutes)

**Step 4: App Troubleshooting**
1. Force close and restart app
2. Log out and log back in
3. Reinstall app if issues persist

### Issue: Push Notifications Not Working

#### Symptoms
- Not receiving leave approval notifications
- Missing attendance reminders
- No alerts for important updates

#### Troubleshooting Steps

**Step 1: Notification Permissions**
1. Check app notification permissions in device settings
2. Enable all notification types for StrideHR app
3. Ensure "Do Not Disturb" is not blocking notifications

**Step 2: App Settings**
1. Open StrideHR app settings
2. Enable push notifications
3. Select notification types you want to receive

**Step 3: Device Settings**
1. Check battery optimization settings
2. Add StrideHR to "unrestricted" apps list
3. Ensure background app refresh is enabled

## Integration Issues

### Issue: Calendar Integration Not Working

#### Symptoms
- Events not syncing to calendar
- Cannot connect Google/Outlook calendar
- Meeting invites not appearing

#### Troubleshooting Steps

**Step 1: Authentication**
1. Disconnect and reconnect calendar integration
2. Check if calendar account password has changed
3. Verify calendar permissions are granted

**Step 2: Sync Settings**
1. Check calendar sync frequency settings
2. Manually trigger sync if available
3. Verify calendar selection (work vs. personal)

**Step 3: Calendar Permissions**
1. Ensure StrideHR has calendar read/write permissions
2. Check calendar sharing settings
3. Verify calendar is not set to private

### Issue: Payroll System Integration Errors

#### Symptoms
- Salary data not updating
- Payroll export failures
- Integration sync errors

#### Troubleshooting Steps

**Step 1: Administrator Tasks**
1. Check integration status in admin panel
2. Verify API credentials are current
3. Test connection to external payroll system

**Step 2: Data Validation**
1. Check for data format issues
2. Verify employee mapping between systems
3. Look for missing required fields

**Step 3: Escalation**
1. Contact system administrator
2. Provide specific error messages
3. Check integration logs for details

## System Administration Issues

### Issue: Database Connection Problems

#### Symptoms
- "Database connection failed" errors
- Slow query performance
- Data not saving properly

#### Troubleshooting Steps

**Step 1: Connection Verification**
```bash
# Test database connectivity
mysql -h database-server -u username -p -e "SELECT 1;"

# Check connection pool status
mysql -e "SHOW PROCESSLIST;"
```

**Step 2: Database Performance**
```bash
# Check database size and usage
mysql -e "SELECT table_schema, ROUND(SUM(data_length + index_length) / 1024 / 1024, 1) AS 'DB Size in MB' FROM information_schema.tables GROUP BY table_schema;"

# Check for slow queries
mysql -e "SELECT * FROM mysql.slow_log ORDER BY start_time DESC LIMIT 10;"
```

**Step 3: Configuration Check**
1. Verify database server is running
2. Check connection string configuration
3. Verify database user permissions
4. Check firewall rules

### Issue: Redis Cache Problems

#### Symptoms
- Session data lost frequently
- Performance degradation
- Cache-related errors

#### Troubleshooting Steps

**Step 1: Redis Status**
```bash
# Check Redis connectivity
redis-cli ping

# Check Redis memory usage
redis-cli info memory

# Check connected clients
redis-cli info clients
```

**Step 2: Cache Configuration**
1. Verify Redis server is running
2. Check cache configuration settings
3. Monitor cache hit/miss ratios
4. Clear cache if corrupted

### Issue: SSL Certificate Problems

#### Symptoms
- "Certificate expired" warnings
- "Insecure connection" messages
- HTTPS not working

#### Troubleshooting Steps

**Step 1: Certificate Verification**
```bash
# Check certificate expiration
openssl x509 -in /path/to/certificate.pem -text -noout | grep "Not After"

# Test SSL configuration
openssl s_client -connect your-domain.com:443
```

**Step 2: Certificate Renewal**
```bash
# Renew Let's Encrypt certificate
certbot renew --dry-run

# Restart web server after renewal
systemctl restart nginx
```

## Emergency Procedures

### System Down Emergency Response

#### Immediate Actions (0-15 minutes)
1. **Verify Outage Scope**
   - Check if issue affects all users or specific users
   - Test from different networks/locations
   - Check system status page

2. **Initial Communication**
   - Notify key stakeholders
   - Post status update if available
   - Prepare communication template

3. **Quick Diagnostics**
   ```bash
   # Check service status
   docker-compose ps
   
   # Check system resources
   df -h
   free -m
   top
   
   # Check logs
   tail -f /var/log/stridehr/application.log
   ```

#### Escalation Procedures (15-30 minutes)
1. **Contact System Administrator**
   - Provide detailed issue description
   - Share diagnostic information
   - Coordinate response efforts

2. **Backup System Activation**
   - Switch to backup systems if available
   - Implement manual processes if needed
   - Communicate alternative procedures

#### Recovery Actions (30+ minutes)
1. **System Restoration**
   - Follow disaster recovery procedures
   - Restore from backups if necessary
   - Verify data integrity

2. **Post-Incident**
   - Document incident details
   - Conduct post-mortem analysis
   - Update procedures based on lessons learned

### Data Loss Emergency Response

#### Immediate Actions
1. **Stop Further Operations**
   - Prevent additional data loss
   - Isolate affected systems
   - Preserve current state

2. **Assess Damage**
   - Identify scope of data loss
   - Determine last known good state
   - Check backup availability

3. **Recovery Process**
   - Restore from most recent backup
   - Verify data integrity
   - Test system functionality

### Security Incident Response

#### Immediate Actions
1. **Isolate Systems**
   - Disconnect affected systems from network
   - Preserve evidence
   - Document incident details

2. **Assess Impact**
   - Identify compromised data
   - Determine breach scope
   - Check for ongoing threats

3. **Recovery and Hardening**
   - Restore from clean backups
   - Apply security patches
   - Update security configurations

## Getting Additional Help

### Internal Support Contacts

#### IT Helpdesk
- **Email**: it-support@yourcompany.com
- **Phone**: [Your IT Support Number]
- **Hours**: Monday-Friday, 9 AM - 6 PM
- **Emergency**: [Emergency IT Number] (24/7)

#### HR Support
- **Email**: hr-support@yourcompany.com
- **Phone**: [Your HR Support Number]
- **Hours**: Monday-Friday, 9 AM - 5 PM

#### System Administrator
- **Email**: admin@yourcompany.com
- **Phone**: [Admin Contact Number]
- **Emergency**: [Emergency Admin Number]

### External Support

#### StrideHR Support
- **Email**: support@stridehr.com
- **Documentation**: https://docs.stridehr.com
- **Community Forum**: https://community.stridehr.com

### When Contacting Support

**Please Provide:**
1. Your full name and employee ID
2. Detailed description of the issue
3. Steps you've already tried
4. Error messages (exact text or screenshots)
5. Browser/device information
6. Time when issue occurred

**Information to Gather:**
- Browser version and operating system
- Screenshots of error messages
- Network information (if relevant)
- Recent changes to system or account

### Self-Help Resources

#### Documentation
- User Manual: Complete system documentation
- Quick Start Guides: Role-based getting started guides
- Video Tutorials: Step-by-step visual guides
- FAQ Section: Answers to common questions

#### Training Materials
- Online training modules
- Webinar recordings
- Best practices guides
- Feature update announcements

---

**Document Version**: 1.0  
**Last Updated**: August 6, 2025  
**Next Review**: February 6, 2026  
**Emergency Contact**: [Emergency Support Number]