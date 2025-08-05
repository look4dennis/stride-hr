# StrideHR User Manual

Welcome to StrideHR - your comprehensive Human Resource Management System designed for global organizations. This manual provides detailed instructions for all user roles and system features.

## Table of Contents

- [Getting Started](#getting-started)
- [User Roles Overview](#user-roles-overview)
- [Super Admin Guide](#super-admin-guide)
- [HR Manager Guide](#hr-manager-guide)
- [Department Manager Guide](#department-manager-guide)
- [Employee Guide](#employee-guide)
- [Common Features](#common-features)
- [Mobile App Usage](#mobile-app-usage)
- [Troubleshooting](#troubleshooting)
- [FAQ](#faq)

## Getting Started

### System Requirements

#### Web Browser Requirements
- **Recommended**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- **Minimum**: Chrome 80+, Firefox 75+, Safari 13+, Edge 80+
- JavaScript must be enabled
- Cookies must be enabled for authentication

#### Mobile Requirements
- **iOS**: 13.0 or later
- **Android**: 8.0 (API level 26) or later
- Internet connection required for real-time features

### First Time Login

1. **Receive Welcome Email**
   - Check your email for the welcome message from StrideHR
   - Click the "Set Password" link in the email
   - The link expires in 24 hours

2. **Set Your Password**
   - Create a strong password with at least 8 characters
   - Include uppercase, lowercase, numbers, and special characters
   - Confirm your password

3. **Complete Profile Setup**
   - Upload your profile photo
   - Verify your personal information
   - Set your notification preferences
   - Configure your timezone

4. **Dashboard Overview**
   - Familiarize yourself with the dashboard layout
   - Review your quick actions
   - Check your pending tasks and notifications

### Navigation Basics

#### Main Navigation Menu
- **Dashboard**: Your personalized home screen
- **Employees**: Employee directory and management
- **Attendance**: Time tracking and attendance records
- **Leave**: Leave requests and balance management
- **Payroll**: Salary information and payslips
- **Projects**: Project management and task tracking
- **Reports**: Analytics and reporting tools
- **Settings**: System and personal preferences

#### Quick Actions
- **Check In/Out**: Located prominently on the dashboard
- **Request Leave**: Quick leave request form
- **Submit DSR**: Daily status report submission
- **View Payslip**: Access latest payslip
- **Emergency Contacts**: Quick access to important contacts

## User Roles Overview

### Role Hierarchy and Permissions

#### Super Admin
- **Full System Access**: Complete control over all system features
- **Organization Management**: Create and manage multiple branches
- **User Management**: Create, modify, and deactivate user accounts
- **System Configuration**: Configure global settings and policies
- **Security Management**: Manage roles, permissions, and security settings
- **Data Management**: Access to all organizational data across branches

#### HR Manager
- **Employee Management**: Full employee lifecycle management
- **Recruitment**: Job posting, candidate management, and hiring
- **Payroll Management**: Process payroll, manage formulas, and generate reports
- **Leave Management**: Approve leave requests and manage policies
- **Performance Management**: Conduct reviews and manage PIPs
- **Compliance**: Ensure regulatory compliance and generate statutory reports
- **Branch Access**: Access to assigned branch(es) data

#### Department Manager
- **Team Management**: Manage direct reports and team members
- **Project Management**: Create and manage department projects
- **Attendance Oversight**: Monitor team attendance and productivity
- **Leave Approval**: Approve leave requests for team members
- **Performance Reviews**: Conduct performance evaluations
- **Budget Management**: Track department expenses and budgets
- **Reporting**: Generate team and department reports

#### Team Lead
- **Project Leadership**: Lead specific projects and initiatives
- **Task Assignment**: Assign and track tasks for team members
- **Progress Monitoring**: Monitor project progress and deadlines
- **Team Collaboration**: Facilitate team communication and coordination
- **Resource Management**: Manage project resources and timelines
- **Status Reporting**: Provide regular updates to management

#### Employee
- **Personal Management**: Manage personal profile and preferences
- **Time Tracking**: Check in/out and manage attendance
- **Leave Requests**: Submit and track leave applications
- **Task Management**: View and update assigned tasks
- **DSR Submission**: Submit daily status reports
- **Payslip Access**: View and download payslips
- **Training**: Access training modules and certifications

## Super Admin Guide

### Organization Setup

#### Initial Configuration
1. **Organization Profile**
   ```
   Settings → Organization → Profile
   ```
   - Upload organization logo
   - Set organization name and details
   - Configure contact information
   - Set default working hours
   - Configure overtime policies

2. **Branch Management**
   ```
   Settings → Organization → Branches
   ```
   - Create new branches
   - Set branch-specific configurations
   - Configure local holidays and working hours
   - Set currency and timezone
   - Assign branch administrators

3. **System Policies**
   ```
   Settings → Policies
   ```
   - Password policies
   - Session timeout settings
   - File upload restrictions
   - Data retention policies
   - Backup configurations

#### User Management

1. **Creating User Accounts**
   ```
   Users → Create New User
   ```
   - Enter basic user information
   - Assign roles and permissions
   - Set branch assignments
   - Configure account settings
   - Send welcome email

2. **Role Management**
   ```
   Settings → Roles & Permissions
   ```
   - Create custom roles
   - Assign permissions to roles
   - Set role hierarchy
   - Configure branch-based access
   - Manage role inheritance

3. **Bulk User Operations**
   ```
   Users → Bulk Operations
   ```
   - Import users from Excel/CSV
   - Bulk role assignments
   - Mass password resets
   - Bulk account activation/deactivation

#### Security Management

1. **Access Control**
   ```
   Settings → Security → Access Control
   ```
   - Configure IP whitelisting
   - Set up multi-factor authentication
   - Manage session policies
   - Configure login restrictions

2. **Audit Logs**
   ```
   Settings → Security → Audit Logs
   ```
   - View system access logs
   - Monitor user activities
   - Track data changes
   - Export audit reports

3. **Data Encryption**
   ```
   Settings → Security → Encryption
   ```
   - Configure encryption keys
   - Manage data encryption policies
   - Set up secure file storage
   - Configure database encryption

### System Monitoring

#### Performance Monitoring
1. **System Health**
   ```
   Admin → System Health
   ```
   - Monitor server performance
   - Check database connectivity
   - View system resource usage
   - Monitor API response times

2. **User Activity**
   ```
   Admin → User Activity
   ```
   - Track active users
   - Monitor login patterns
   - View feature usage statistics
   - Identify inactive accounts

#### Backup and Recovery
1. **Backup Configuration**
   ```
   Admin → Backup → Configuration
   ```
   - Schedule automatic backups
   - Configure backup retention
   - Set backup storage locations
   - Test backup integrity

2. **Data Recovery**
   ```
   Admin → Backup → Recovery
   ```
   - Restore from backups
   - Selective data recovery
   - Point-in-time recovery
   - Disaster recovery procedures

## HR Manager Guide

### Employee Lifecycle Management

#### Recruitment and Onboarding

1. **Job Posting Management**
   ```
   Recruitment → Job Postings
   ```
   - Create job descriptions
   - Set application requirements
   - Configure approval workflows
   - Publish to job boards
   - Track application metrics

2. **Candidate Management**
   ```
   Recruitment → Candidates
   ```
   - Review applications
   - Schedule interviews
   - Conduct assessments
   - Track candidate progress
   - Generate offer letters

3. **Employee Onboarding**
   ```
   Employees → Onboarding
   ```
   - Create onboarding checklists
   - Assign mentors
   - Schedule orientation sessions
   - Track completion status
   - Collect required documents

#### Employee Data Management

1. **Employee Profiles**
   ```
   Employees → Employee Directory
   ```
   - Maintain employee records
   - Update personal information
   - Manage employment history
   - Track certifications
   - Handle document storage

2. **Organizational Structure**
   ```
   Employees → Organization Chart
   ```
   - Design org chart layouts
   - Update reporting relationships
   - Manage department structures
   - Track role changes
   - Visualize hierarchy

### Payroll Management

#### Payroll Processing

1. **Payroll Setup**
   ```
   Payroll → Configuration
   ```
   - Configure salary structures
   - Set up allowances and deductions
   - Create payroll formulas
   - Configure tax settings
   - Set up statutory compliances

2. **Monthly Payroll Processing**
   ```
   Payroll → Process Payroll
   ```
   - Import attendance data
   - Calculate salaries
   - Review calculations
   - Generate payslips
   - Process approvals
   - Release payments

3. **Payslip Management**
   ```
   Payroll → Payslips
   ```
   - Design payslip templates
   - Customize layouts
   - Add company branding
   - Configure multi-language support
   - Manage distribution

#### Compliance and Reporting

1. **Statutory Compliance**
   ```
   Payroll → Compliance
   ```
   - Generate PF reports
   - Create ESI statements
   - Prepare tax filings
   - Handle audit requirements
   - Manage regulatory updates

2. **Payroll Analytics**
   ```
   Reports → Payroll Analytics
   ```
   - Cost center analysis
   - Department-wise reports
   - Trend analysis
   - Budget variance reports
   - Compliance dashboards

### Leave Management

#### Leave Policy Configuration

1. **Leave Types Setup**
   ```
   Leave → Configuration → Leave Types
   ```
   - Define leave categories
   - Set accrual rules
   - Configure carry-forward policies
   - Set maximum limits
   - Define encashment rules

2. **Approval Workflows**
   ```
   Leave → Configuration → Workflows
   ```
   - Design approval hierarchies
   - Set delegation rules
   - Configure escalation policies
   - Define emergency approvals
   - Set notification triggers

#### Leave Processing

1. **Leave Request Management**
   ```
   Leave → Requests
   ```
   - Review pending requests
   - Approve/reject applications
   - Handle backdated requests
   - Manage cancellations
   - Process emergency leaves

2. **Leave Balance Management**
   ```
   Leave → Balances
   ```
   - Monitor leave balances
   - Process accruals
   - Handle adjustments
   - Manage encashments
   - Generate balance reports

### Performance Management

#### Performance Review System

1. **Review Cycle Setup**
   ```
   Performance → Configuration
   ```
   - Define review periods
   - Create evaluation forms
   - Set performance metrics
   - Configure rating scales
   - Design feedback templates

2. **Review Process Management**
   ```
   Performance → Reviews
   ```
   - Initiate review cycles
   - Monitor completion status
   - Facilitate 360-degree feedback
   - Generate performance reports
   - Track improvement plans

#### Performance Improvement Plans (PIPs)

1. **PIP Creation and Management**
   ```
   Performance → PIPs
   ```
   - Create improvement plans
   - Set measurable goals
   - Define timelines
   - Assign mentors
   - Track progress
   - Document outcomes

## Department Manager Guide

### Team Management

#### Team Overview and Monitoring

1. **Team Dashboard**
   ```
   Dashboard → Team View
   ```
   - View team attendance
   - Monitor productivity metrics
   - Track project progress
   - Review pending approvals
   - Check team announcements

2. **Employee Supervision**
   ```
   Team → Employee Management
   ```
   - View direct reports
   - Monitor attendance patterns
   - Review performance metrics
   - Handle disciplinary actions
   - Manage team schedules

#### Project and Task Management

1. **Project Creation and Planning**
   ```
   Projects → Create Project
   ```
   - Define project scope
   - Set timelines and milestones
   - Assign team members
   - Allocate resources
   - Set budget parameters

2. **Task Assignment and Tracking**
   ```
   Projects → Task Management
   ```
   - Create and assign tasks
   - Set priorities and deadlines
   - Monitor task progress
   - Review deliverables
   - Track time spent

3. **Kanban Board Management**
   ```
   Projects → Kanban Board
   ```
   - Organize tasks by status
   - Drag and drop task updates
   - Monitor workflow bottlenecks
   - Assign tasks to team members
   - Track completion rates

### Attendance and Leave Management

#### Team Attendance Monitoring

1. **Attendance Overview**
   ```
   Attendance → Team Attendance
   ```
   - View real-time attendance
   - Monitor late arrivals
   - Track break durations
   - Review overtime hours
   - Generate attendance reports

2. **Attendance Corrections**
   ```
   Attendance → Corrections
   ```
   - Review correction requests
   - Approve/reject modifications
   - Handle missed punches
   - Manage retroactive changes
   - Document approval reasons

#### Leave Approval Process

1. **Leave Request Review**
   ```
   Leave → Pending Approvals
   ```
   - Review team leave requests
   - Check leave balances
   - Verify coverage arrangements
   - Approve/reject requests
   - Add approval comments

2. **Team Leave Planning**
   ```
   Leave → Team Calendar
   ```
   - View team leave calendar
   - Plan coverage for absences
   - Identify scheduling conflicts
   - Coordinate with other departments
   - Manage holiday schedules

### Performance and Development

#### Team Performance Monitoring

1. **Performance Dashboard**
   ```
   Performance → Team Performance
   ```
   - View individual performance metrics
   - Track goal completion
   - Monitor productivity trends
   - Review feedback scores
   - Identify improvement areas

2. **One-on-One Meetings**
   ```
   Performance → 1:1 Meetings
   ```
   - Schedule regular meetings
   - Set meeting agendas
   - Document discussion points
   - Track action items
   - Monitor follow-ups

#### Training and Development

1. **Training Assignment**
   ```
   Training → Assign Training
   ```
   - Identify skill gaps
   - Assign relevant courses
   - Set completion deadlines
   - Monitor progress
   - Evaluate effectiveness

2. **Career Development Planning**
   ```
   Performance → Development Plans
   ```
   - Create development roadmaps
   - Set career goals
   - Identify growth opportunities
   - Plan skill development
   - Track progress

## Employee Guide

### Daily Operations

#### Time and Attendance

1. **Check-In/Check-Out Process**
   ```
   Dashboard → Attendance Widget
   ```
   - Click "Check In" when arriving at work
   - Select location if working remotely
   - Use "Take Break" for breaks (select type: tea, lunch, personal, meeting)
   - Click "End Break" when returning
   - Click "Check Out" when leaving work

2. **Attendance History**
   ```
   Attendance → My Attendance
   ```
   - View daily attendance records
   - Check total working hours
   - Review break durations
   - Monitor overtime hours
   - Request attendance corrections

#### Daily Status Reports (DSR)

1. **DSR Submission**
   ```
   Dashboard → Submit DSR
   ```
   - Select project from dropdown
   - Choose associated tasks
   - Enter hours worked on each task
   - Provide work description
   - Submit before end of day

2. **DSR History and Tracking**
   ```
   Reports → My DSRs
   ```
   - View submitted DSRs
   - Track productivity metrics
   - Review feedback from managers
   - Monitor project contributions
   - Export DSR reports

### Leave Management

#### Leave Request Process

1. **Submitting Leave Requests**
   ```
   Leave → Request Leave
   ```
   - Select leave type
   - Choose dates (from/to)
   - Enter reason for leave
   - Attach supporting documents if required
   - Submit for approval

2. **Leave Balance Tracking**
   ```
   Leave → My Balances
   ```
   - View available leave balances
   - Check accrual history
   - Monitor used leaves
   - Plan future leaves
   - Request balance statements

3. **Leave Calendar**
   ```
   Leave → Calendar View
   ```
   - View personal leave calendar
   - See team leave schedules
   - Check holiday calendar
   - Plan leave requests
   - Avoid scheduling conflicts

### Personal Profile Management

#### Profile Information

1. **Personal Details**
   ```
   Profile → Personal Information
   ```
   - Update contact information
   - Change profile photo
   - Modify emergency contacts
   - Update address details
   - Manage personal documents

2. **Professional Information**
   ```
   Profile → Professional Details
   ```
   - View job information
   - Check reporting manager
   - Review role and responsibilities
   - Track employment history
   - Manage certifications

#### Account Settings

1. **Security Settings**
   ```
   Settings → Security
   ```
   - Change password
   - Enable two-factor authentication
   - Manage active sessions
   - Review login history
   - Set security questions

2. **Notification Preferences**
   ```
   Settings → Notifications
   ```
   - Configure email notifications
   - Set mobile push notifications
   - Choose notification frequency
   - Select notification types
   - Manage quiet hours

### Payroll and Benefits

#### Payslip Access

1. **Viewing Payslips**
   ```
   Payroll → My Payslips
   ```
   - Access current month payslip
   - View historical payslips
   - Download PDF copies
   - Check salary components
   - Review deductions and taxes

2. **Salary Information**
   ```
   Payroll → Salary Details
   ```
   - View salary structure
   - Check allowances and benefits
   - Review tax calculations
   - Monitor year-to-date earnings
   - Access tax documents

#### Investment and Tax Planning

1. **Tax Declaration**
   ```
   Payroll → Tax Declaration
   ```
   - Submit investment proofs
   - Declare tax-saving investments
   - Upload supporting documents
   - Review tax calculations
   - Track submission status

### Training and Development

#### Training Modules

1. **Assigned Training**
   ```
   Training → My Training
   ```
   - View assigned courses
   - Track completion progress
   - Access training materials
   - Take assessments
   - Download certificates

2. **Self-Learning**
   ```
   Training → Course Catalog
   ```
   - Browse available courses
   - Enroll in optional training
   - Set learning goals
   - Track skill development
   - Request additional training

## Common Features

### Dashboard Overview

#### Personalized Dashboard

1. **Dashboard Widgets**
   - **Weather & Time**: Current weather and time for your location
   - **Attendance Status**: Current check-in status and working hours
   - **Quick Actions**: Frequently used functions
   - **Notifications**: Recent alerts and messages
   - **Birthday Wishes**: Today's birthdays in the organization
   - **Project Status**: Current project progress and tasks
   - **Leave Balance**: Available leave balances
   - **Upcoming Events**: Calendar events and deadlines

2. **Customization Options**
   ```
   Dashboard → Customize
   ```
   - Rearrange widgets
   - Show/hide widgets
   - Set refresh intervals
   - Choose color themes
   - Configure quick actions

### Notification System

#### Real-Time Notifications

1. **Notification Types**
   - **System Alerts**: Important system messages
   - **Approval Requests**: Items requiring your approval
   - **Status Updates**: Changes to your requests
   - **Reminders**: Upcoming deadlines and events
   - **Birthday Notifications**: Colleague birthdays
   - **Project Updates**: Task assignments and updates

2. **Notification Management**
   ```
   Notifications → Manage
   ```
   - Mark as read/unread
   - Archive old notifications
   - Set notification preferences
   - Configure delivery methods
   - Manage notification history

### Search and Filters

#### Global Search

1. **Search Functionality**
   ```
   Top Navigation → Search Bar
   ```
   - Search employees by name, ID, or email
   - Find projects and tasks
   - Locate documents and files
   - Search knowledge base articles
   - Find system settings

2. **Advanced Filters**
   - **Date Range Filters**: Filter by specific date ranges
   - **Status Filters**: Filter by status (active, pending, completed)
   - **Department Filters**: Filter by department or branch
   - **Role Filters**: Filter by user roles
   - **Custom Filters**: Create and save custom filter combinations

### Reports and Analytics

#### Standard Reports

1. **Pre-built Reports**
   ```
   Reports → Standard Reports
   ```
   - Attendance reports
   - Leave reports
   - Payroll reports
   - Performance reports
   - Project reports
   - Asset reports

2. **Custom Report Builder**
   ```
   Reports → Custom Reports
   ```
   - Drag-and-drop report builder
   - Select data sources
   - Choose visualization types
   - Set filters and parameters
   - Schedule automated reports
   - Share reports with teams

#### Analytics Dashboard

1. **Key Metrics**
   - Employee headcount trends
   - Attendance patterns
   - Leave utilization
   - Project completion rates
   - Performance metrics
   - Cost analysis

2. **Interactive Charts**
   - Drill-down capabilities
   - Filter by time periods
   - Compare across departments
   - Export chart data
   - Share visualizations

## Mobile App Usage

### Mobile App Features

#### Core Functionality

1. **Attendance Management**
   - Quick check-in/check-out
   - Location-based attendance
   - Break management
   - Attendance history
   - Correction requests

2. **Leave Management**
   - Submit leave requests
   - Check leave balances
   - View leave calendar
   - Track request status
   - Emergency leave requests

3. **Notifications**
   - Push notifications
   - Real-time alerts
   - Approval notifications
   - Birthday reminders
   - System announcements

#### Mobile-Specific Features

1. **Offline Capability**
   - Offline attendance marking
   - Sync when connected
   - Cached data access
   - Offline form submission
   - Background synchronization

2. **Location Services**
   - GPS-based check-in
   - Geofencing for office locations
   - Remote work tracking
   - Location history
   - Privacy controls

### Mobile App Setup

#### Installation and Setup

1. **Download and Install**
   - Download from App Store (iOS) or Google Play (Android)
   - Install the StrideHR mobile app
   - Allow required permissions
   - Enable notifications
   - Set up biometric authentication

2. **Initial Configuration**
   - Login with your credentials
   - Enable location services
   - Configure notification preferences
   - Set up offline sync
   - Test core functionality

## Troubleshooting

### Common Issues and Solutions

#### Login and Authentication Issues

1. **Cannot Login**
   - **Issue**: Invalid credentials error
   - **Solution**: 
     - Verify username/email and password
     - Check caps lock status
     - Try password reset if needed
     - Contact IT support if account is locked

2. **Password Reset Problems**
   - **Issue**: Password reset email not received
   - **Solution**:
     - Check spam/junk folder
     - Verify email address is correct
     - Wait 5-10 minutes for delivery
     - Contact HR if email is not received

3. **Two-Factor Authentication Issues**
   - **Issue**: 2FA code not working
   - **Solution**:
     - Ensure device time is synchronized
     - Generate new code
     - Check if backup codes are available
     - Contact IT support for reset

#### Attendance and Time Tracking Issues

1. **Check-in/Check-out Problems**
   - **Issue**: Cannot check in or check out
   - **Solution**:
     - Refresh the page/app
     - Check internet connection
     - Clear browser cache
     - Try from different device
     - Contact IT support

2. **Location Issues**
   - **Issue**: Location not detected for attendance
   - **Solution**:
     - Enable location services
     - Allow browser/app location access
     - Check GPS signal strength
     - Try manual location entry
     - Contact IT for location setup

3. **Attendance Corrections**
   - **Issue**: Need to correct attendance record
   - **Solution**:
     - Submit correction request through system
     - Provide reason for correction
     - Attach supporting documents
     - Follow up with manager
     - Check approval status

#### Leave Management Issues

1. **Leave Request Problems**
   - **Issue**: Cannot submit leave request
   - **Solution**:
     - Check leave balance availability
     - Verify date selection
     - Ensure all required fields are filled
     - Check for conflicting requests
     - Contact HR for assistance

2. **Leave Balance Discrepancies**
   - **Issue**: Leave balance appears incorrect
   - **Solution**:
     - Review leave history
     - Check accrual calculations
     - Verify policy changes
     - Contact HR for balance verification
     - Request balance statement

#### System Performance Issues

1. **Slow Loading**
   - **Issue**: System loading slowly
   - **Solution**:
     - Check internet connection speed
     - Clear browser cache and cookies
     - Disable browser extensions
     - Try different browser
     - Contact IT support

2. **Mobile App Issues**
   - **Issue**: Mobile app not working properly
   - **Solution**:
     - Update app to latest version
     - Restart the app
     - Clear app cache/data
     - Reinstall the app
     - Check device compatibility

### Getting Help

#### Support Channels

1. **Self-Service Options**
   - **Knowledge Base**: Search for solutions in the help center
   - **FAQ Section**: Common questions and answers
   - **Video Tutorials**: Step-by-step video guides
   - **User Forums**: Community discussions and solutions

2. **Direct Support**
   - **IT Help Desk**: Technical issues and system problems
   - **HR Support**: Policy questions and process guidance
   - **Manager Escalation**: Approval and workflow issues
   - **Emergency Support**: Critical issues requiring immediate attention

#### Contact Information

1. **Internal Support**
   - **IT Help Desk**: ext. 1234 or it-support@company.com
   - **HR Department**: ext. 5678 or hr@company.com
   - **System Administrator**: ext. 9012 or admin@company.com

2. **External Support**
   - **StrideHR Support**: support@stridehr.com
   - **Technical Support**: 1-800-STRIDEHR
   - **Emergency Support**: Available 24/7 for critical issues

## FAQ

### General Questions

**Q: How do I change my password?**
A: Go to Settings → Security → Change Password. Enter your current password and set a new one following the password policy requirements.

**Q: Can I access StrideHR from my mobile device?**
A: Yes, StrideHR is fully responsive and works on all devices. You can also download our mobile app from the App Store or Google Play.

**Q: How do I update my personal information?**
A: Navigate to Profile → Personal Information and update the required fields. Some changes may require manager or HR approval.

**Q: What should I do if I forget to check in or check out?**
A: Submit an attendance correction request through Attendance → Corrections. Provide the reason and any supporting documentation.

### Leave Management

**Q: How far in advance should I submit leave requests?**
A: Submit leave requests at least 2 weeks in advance for planned leaves. Emergency leaves can be submitted as soon as possible.

**Q: Can I cancel a leave request after it's approved?**
A: Yes, you can cancel approved leave requests, but it requires manager approval. The earlier you cancel, the better.

**Q: How are leave balances calculated?**
A: Leave balances are calculated based on your employment date, leave policy, and accrual rules. Check Leave → My Balances for detailed information.

### Payroll Questions

**Q: When are payslips available?**
A: Payslips are typically available by the 5th of each month. You'll receive a notification when your payslip is ready.

**Q: How do I download my payslip?**
A: Go to Payroll → My Payslips, select the month, and click the download button to get a PDF copy.

**Q: What should I do if there's an error in my payslip?**
A: Contact HR immediately with details of the error. They will investigate and make corrections if necessary.

### Technical Issues

**Q: The system is running slowly. What should I do?**
A: Try clearing your browser cache, using a different browser, or checking your internet connection. Contact IT support if the issue persists.

**Q: I'm not receiving email notifications. How do I fix this?**
A: Check your spam folder, verify your email address in your profile, and ensure notifications are enabled in Settings → Notifications.

**Q: Can I use StrideHR offline?**
A: Limited offline functionality is available through the mobile app for attendance marking and viewing cached data. Full functionality requires an internet connection.

---

*This user manual is regularly updated. For the latest version and additional resources, visit the StrideHR help center or contact your system administrator.*
