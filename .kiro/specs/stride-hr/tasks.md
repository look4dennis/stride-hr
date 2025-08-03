# Implementation Plan

Convert the feature design into a series of prompts for a code-generation LLM that will implement each step in a test-driven manner. Prioritize best practices, incremental progress, and early testing, ensuring no big jumps in complexity at any stage. Make sure that each prompt builds on the previous prompts, and ends with wiring things together. There should be no hanging or orphaned code that isn't integrated into a previous step. Focus ONLY on tasks that involve writing, modifying, or testing code.

## Task List

- [x] 1. Project Structure and Core Infrastructure Setup

  - Create solution structure with separate projects for API, Core, Infrastructure, and Tests
  - Set up Entity Framework Core with MySQL connection
  - Configure dependency injection container and basic middleware
  - Create base entity classes and repository patterns
  - Set up logging with Serilog and basic error handling
  - _Requirements: All requirements depend on solid foundation_

- [x] 2. Database Schema and Entity Models

  - [x] 2.1 Create core entity models for Organization, Branch, and Employee

    - Implement Organization entity with logo support and configuration settings
    - Create Branch entity with multi-country and currency support
    - Design Employee entity with profile photo and role relationships
    - Add Entity Framework configurations and relationships
    - _Requirements: 1.1, 1.2, 1.5, 42.1_


  - [x] 2.2 Create authentication and authorization entities

    - Implement User, Role, and Permission entities
    - Create RolePermission and EmployeeRole junction tables
    - Add JWT token and refresh token entities
    - Configure Entity Framework relationships and constraints

    - _Requirements: 8.1, 8.2, 33.1, 33.3_

  - [x] 2.3 Create attendance and time tracking entities

    - Implement AttendanceRecord entity with check-in/out and location tracking
    - Create BreakRecord entity with break types and duration tracking
    - Add Shift and ShiftAssignment entities for multiple shift support
    - Configure timezone and working hours support
    - _Requirements: 4.1, 4.2, 4.9, 24.3, 45.1_

- [x] 3. Authentication and Security Implementation

  - [x] 3.1 Implement JWT authentication service

    - Create JWT token generation and validation logic
    - Implement refresh token mechanism
    - Add password hashing and validation
    - Create authentication middleware and policies
    - Write unit tests for authentication service
    - _Requirements: 8.1, 16.1, 16.2, 25.1_


  - [x] 3.2 Implement role-based authorization system

    - Create permission-based authorization handlers
    - Implement dynamic role and permission management
    - Add branch-based data isolation logic
    - Create authorization policies for different modules
    - Write unit tests for authorization system


    - _Requirements: 8.2, 33.4, 33.5, 35.2, 35.3_

  - [x] 3.3 Create user management and security services

    - Implement user registration and profile management
    - Add password reset and change functionality
    - Create audit logging service for security events
    - Implement data encryption service for sensitive data
    - Write integration tests for security workflows
    - _Requirements: 16.3, 16.6, 16.7, 8.3_

- [x] 4. Core Business Services Implementation

  - [x] 4.1 Employee management service

    - Create employee CRUD operations with validation
    - Implement employee search and filtering functionality
    - Add employee profile photo upload and management
    - Create employee onboarding and exit workflows
    - Write unit tests for employee service operations
    - _Requirements: 2.4, 2.5, 39.1, 42.1, 42.4_

  - [x] 4.2 Organization and branch management service

    - Implement organization configuration and logo upload
    - Create branch management with multi-country support
    - Add currency and timezone handling services
    - Implement branch-based data isolation logic
    - Write unit tests for organization services
    - _Requirements: 1.1, 1.3, 24.1, 34.1, 35.1_

  - [x] 4.3 Attendance tracking service

    - Create check-in/check-out functionality with location tracking
    - Implement break management with type selection
    - Add real-time attendance status tracking
    - Create attendance correction workflows for HR
    - Write unit tests for attendance operations
    - _Requirements: 4.1, 4.2, 4.9, 4.10, 21.8, 21.9_

- [x] 5. Project Management and Task Tracking

  - [x] 5.1 Project and task management service

    - Create project creation and configuration functionality
    - Implement task creation and assignment workflows
    - Add project hours estimation and tracking
    - Create team assignment and management features
    - Write unit tests for project management operations
    - _Requirements: 11.1, 11.2, 27.3, 27.4, 32.1, 32.2_

  - [x] 5.2 Daily Status Report (DSR) system

    - Implement DSR submission with project and task selection
    - Create DSR review and approval workflows
    - Add productivity tracking and idle time calculation
    - Implement project hours vs estimates tracking
    - Write unit tests for DSR functionality
    - _Requirements: 12.1, 12.8, 12.9, 12.10, 31.1, 31.4_

  - [x] 5.3 Project progress monitoring and reporting

    - Create real-time project progress tracking
    - Implement project hours analysis and variance reporting
    - Add team leader dashboard for project oversight
    - Create automated alerts for project delays
    - Write unit tests for project monitoring features
    - _Requirements: 31.2, 31.5, 31.7, 31.11, 11.7_

- [x] 6. Payroll System with Custom Formula Engine

  - [x] 6.1 Payroll calculation engine

    - Create flexible payroll formula engine with mathematical expressions
    - Implement salary calculation with allowances and deductions
    - Add multi-currency support and exchange rate handling
    - Create overtime calculation based on attendance data
    - Write unit tests for payroll calculations
    - _Requirements: 3.1, 3.2, 3.4, 24.8, 34.8_

  - [x] 6.2 Payslip generation and approval workflow

    - Implement drag-and-drop payslip designer
    - Create payslip generation with organization branding
    - Add multi-level approval workflow (HR → Finance)
    - Implement payroll release and employee notifications
    - Write unit tests for payslip generation
    - _Requirements: 3.3, 3.6, 3.7, 3.8, 3.9_

  - [x] 6.3 Payroll reporting and compliance

    - Create payroll reports with currency conversion
    - Implement statutory compliance reporting
    - Add payroll audit trails and error correction workflows
    - Create payroll analytics and budget variance tracking
    - Write unit tests for payroll reporting
    - _Requirements: 3.10, 1.6, 6.3, 3.7_

- [x] 7. Leave Management System
  - [x] 7.1 Leave request and approval workflow

    - Create leave request submission with balance validation
    - Implement multi-level approval workflows
    - Add leave conflict detection and team scheduling
    - Create leave calendar integration and visualization
    - Write unit tests for leave management
    - _Requirements: 4.6, 4.5, 37.1_

  - [x] 7.2 Leave balance tracking and reporting

    - Implement leave balance calculation and tracking
    - Create leave accrual rules and policy management
    - Add leave history and analytics reporting
    - Implement leave encashment calculations
    - Write unit tests for leave balance operations
    - _Requirements: 4.6, 39.4_

- [x] 8. Performance Management and Training System
  - [x] 8.1 Performance review and PIP management

    - Create performance goal setting and tracking
    - Implement 360-degree feedback system
    - Add Performance Improvement Plan (PIP) workflows
    - Create performance analytics and reporting
    - Write unit tests for performance management
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.10_

  - [x] 8.2 Training and certification system

    - Create training module creation with content upload
    - Implement assessment and testing functionality
    - Add certification generation and tracking
    - Create training progress monitoring and reporting
    - Write unit tests for training system
    - _Requirements: 36.1, 36.2, 36.6, 36.12, 36.15_

- [x] 9. Asset Management and IT Support
  - [x] 9.1 Asset tracking and management

    - Create asset registration and tracking system
    - Implement asset assignment to employees and projects
    - Add asset maintenance scheduling and tracking
    - Create asset handover workflows for employee exits
    - Write unit tests for asset management
    - _Requirements: 13.1, 13.2, 13.5, 13.6_

  - [x] 9.2 IT support ticket system


    - Create support ticket creation and categorization
    - Implement ticket assignment and resolution workflows
    - Add ticket communication and status tracking
    - Create IT support analytics and reporting
    - Write unit tests for support system
    - _Requirements: 13.3, 13.4, 13.7_

- [x] 10. Communication and Notification System
  - [x] 10.1 Real-time notification service with SignalR

    - Implement SignalR hubs for real-time communication
    - Create notification delivery service with multiple channels
    - Add birthday notifications and celebration features
    - Implement attendance and productivity alerts
    - Write unit tests for notification system
    - _Requirements: 14.8, 14.9, 14.10, 40.1, 40.2_

  - [x] 10.2 AI chatbot for HR support

    - Create chatbot service with natural language processing
    - Implement knowledge base integration and FAQ responses
    - Add escalation to human support workflows
    - Create chatbot learning and improvement mechanisms
    - Write unit tests for chatbot functionality
    - _Requirements: 43.1, 43.2, 43.3, 43.9_

  - [x] 10.3 Email service and template management

    - Implement email service with template support
    - Create email templates for various HR processes
    - Add bulk email functionality for announcements
    - Implement email delivery tracking and analytics
    - Write unit tests for email services
    - _Requirements: 15.3, 40.4, 40.9_

- [x] 11. Reporting and Analytics System
  - [x] 11.1 Report builder and data visualization

    - Create drag-and-drop report builder interface
    - Implement data visualization with charts and graphs
    - Add custom report creation and scheduling
    - Create report export functionality in multiple formats
    - Write unit tests for reporting engine
    - _Requirements: 7.2, 17.5, 41.2, 41.9_

  - [x] 11.2 AI-powered analytics and insights

    - Implement predictive analytics for workforce planning
    - Create sentiment analysis for employee feedback
    - Add performance forecasting and trend analysis
    - Create automated insights and recommendations
    - Write unit tests for analytics features
    - _Requirements: 17.1, 17.2, 17.4, 17.7_

- [x] 12. Survey and Feedback System
  - [x] 12.1 Employee survey creation and management

    - Create survey builder with multiple question types
    - Implement survey distribution and response collection
    - Add anonymous survey support and privacy controls
    - Create survey analytics and sentiment analysis
    - Write unit tests for survey system
    - _Requirements: 44.1, 44.2, 44.3, 44.8_

  - [x] 12.2 Grievance management system

    - Create grievance submission with anonymity options
    - Implement escalation workflows and status tracking
    - Add grievance resolution and follow-up processes
    - Create grievance analytics and reporting
    - Write unit tests for grievance management
    - _Requirements: 14.3, 14.4, 14.7_

- [x] 13. Expense Management System
  - [x] 13.1 Expense claim submission and processing

    - Create expense claim forms with receipt upload
    - Implement expense categorization and policy validation
    - Add multi-level approval workflows
    - Create expense reimbursement tracking
    - Write unit tests for expense management
    - _Requirements: 38.1, 38.2, 38.4, 38.7_

  - [x] 13.2 Travel and expense reporting

    - Implement travel expense management with mileage calculation
    - Create expense analytics and budget tracking
    - Add expense policy compliance monitoring
    - Create expense reporting and analytics
    - Write unit tests for expense reporting
    - _Requirements: 38.3, 38.8, 38.9_

- [x] 14. Shift Management and Scheduling
  - [x] 14.1 Shift creation and assignment

    - Create shift templates and scheduling system
    - Implement employee shift assignments
    - Add shift pattern management (day/night/rotating)
    - Create shift coverage and conflict detection
    - Write unit tests for shift management
    - _Requirements: 24.3, 24.5, 45.4, 45.6_

  - [x] 14.2 Shift swapping and schedule management

    - Implement shift swap request workflows
    - Create manager approval for shift changes
    - Add emergency shift coverage broadcasting
    - Create shift analytics and reporting
    - Write unit tests for shift swapping
    - _Requirements: 45.1, 45.2, 45.7, 45.12_

- [x] 15. Knowledge Base and Document Management
  - [x] 15.1 Knowledge base creation and management

    - Create document creation with rich text editor
    - Implement document approval workflows
    - Add document search and categorization
    - Create document version control and history
    - Write unit tests for knowledge base
    - _Requirements: 26.1, 26.2, 26.4, 26.5_

  - [x] 15.2 Document templates and digital workflows

    - Create document templates with merge fields
    - Implement digital signature workflows
    - Add document generation and automation
    - Create document compliance and retention policies
    - Write unit tests for document management
    - _Requirements: 18.1, 18.3, 18.4, 18.5_

- [x] 16. Data Import/Export and Integration
  - [x] 16.1 Data import and export functionality

    - Create bulk data import from Excel/CSV files
    - Implement data validation and error handling
    - Add data export in multiple formats
    - Create data migration tools and utilities
    - Write unit tests for data operations
    - _Requirements: 41.1, 41.2, 41.5, 41.10_

  - [x] 16.2 API development and third-party integrations

    - Create comprehensive REST API with Swagger documentation
    - Implement webhook support for real-time integrations
    - Add calendar integration (Google Calendar, Outlook)
    - Create integration with external payroll and accounting systems
    - Write unit tests for API endpoints
    - _Requirements: 9.1, 9.4, 15.5, 41.7_

- [ ] 17. Frontend Application Development
  - [x] 17.1 Angular project setup and core components

    - Create Angular project with Bootstrap 5 integration
    - Implement authentication guards and interceptors
    - Create shared components and services
    - Set up routing and navigation structure
    - Write unit tests for core components
    - _Requirements: 22.1, 25.1, 8.1_

  - [x] 17.2 Dashboard components for all user roles


    - Create role-based dashboard layouts
    - Implement weather and time widget with OpenWeatherMap integration
    - Add birthday notification widget
    - Create quick action buttons and navigation
    - Write unit tests for dashboard components
    - _Requirements: 20.1, 20.2, 20.5, 14.8, 14.9_

  - [x] 17.3 Employee management interfaces




    - Create employee list with search and filtering
    - Implement employee profile management with photo upload
    - Add employee onboarding and exit workflows
    - Create employee directory with organizational chart
    - Write unit tests for employee components
    - _Requirements: 42.1, 42.5, 19.1, 19.4, 2.1_

- [ ] 18. Attendance and Time Tracking UI
  - [ ] 18.1 Attendance tracking interface
    - Create check-in/check-out buttons with location tracking
    - Implement break management with type selection
    - Add real-time attendance status display
    - Create "Attendance Now" page for organization-wide view
    - Write unit tests for attendance components
    - _Requirements: 4.1, 4.9, 4.10, 29.2, 29.5_

  - [ ] 18.2 Attendance management and reporting
    - Create attendance reports and analytics
    - Implement attendance correction workflows for HR
    - Add attendance calendar and visualization
    - Create attendance alerts and notifications
    - Write unit tests for attendance management
    - _Requirements: 4.7, 21.8, 21.9, 4.8_

- [ ] 19. Project Management UI with Kanban Board
  - [ ] 19.1 Kanban board and list view implementation
    - Create drag-and-drop Kanban board with customizable columns
    - Implement list view toggle with advanced filtering
    - Add task creation and assignment interfaces
    - Create project progress visualization
    - Write unit tests for project management components
    - _Requirements: 23.1, 23.2, 23.4, 11.1, 11.2_

  - [ ] 19.2 Project monitoring and team collaboration
    - Create project hours tracking dashboard
    - Implement team collaboration features
    - Add project analytics and reporting
    - Create project alerts and notifications
    - Write unit tests for project monitoring
    - _Requirements: 31.1, 31.4, 11.3, 11.7_

- [ ] 20. Payroll and Financial Management UI
  - [ ] 20.1 Payroll processing interface
    - Create payroll calculation and processing screens
    - Implement payslip designer with drag-and-drop functionality
    - Add payroll approval workflows
    - Create payroll reports and analytics
    - Write unit tests for payroll components
    - _Requirements: 3.3, 3.6, 3.7, 3.8_

  - [ ] 20.2 Financial reporting and currency management
    - Create financial reports with multi-currency support
    - Implement currency conversion and display
    - Add budget tracking and variance analysis
    - Create financial analytics dashboard
    - Write unit tests for financial components
    - _Requirements: 34.2, 34.5, 34.11, 1.4_

- [ ] 21. Leave and Performance Management UI
  - [ ] 21.1 Leave management interface
    - Create leave request forms with calendar integration
    - Implement leave approval workflows
    - Add leave balance tracking and visualization
    - Create leave calendar and conflict detection
    - Write unit tests for leave components
    - _Requirements: 4.6, 37.1_

  - [ ] 21.2 Performance and training management
    - Create performance review interfaces
    - Implement PIP management workflows
    - Add training module interfaces with assessments
    - Create certification tracking and display
    - Write unit tests for performance components
    - _Requirements: 5.1, 5.3, 36.2, 36.6_

- [ ] 22. Administrative and Configuration UI
  - [ ] 22.1 Organization and system configuration
    - Create organization settings with logo upload
    - Implement branch management interfaces
    - Add user role and permission management
    - Create system configuration panels
    - Write unit tests for admin components
    - _Requirements: 24.1, 33.1, 33.3, 30.4_

  - [ ] 22.2 Reporting and analytics dashboard
    - Create report builder interface
    - Implement data visualization components
    - Add analytics dashboard with AI insights
    - Create export and scheduling functionality
    - Write unit tests for reporting components
    - _Requirements: 7.2, 17.1, 17.5, 41.2_

- [ ] 23. Mobile Responsiveness and PWA Features
  - [ ] 23.1 Mobile-responsive design implementation
    - Ensure all components are mobile-responsive with Bootstrap 5
    - Implement touch-friendly interfaces and gestures
    - Add mobile-specific navigation and layouts
    - Create mobile-optimized forms and inputs
    - Write tests for mobile responsiveness
    - _Requirements: 10.1, 10.8, 22.7_

  - [ ] 23.2 Progressive Web App (PWA) features
    - Implement service workers for offline functionality
    - Add push notifications for mobile devices
    - Create app manifest and installation prompts
    - Implement offline data synchronization
    - Write tests for PWA functionality
    - _Requirements: 10.4, 10.5, 20.12_

- [ ] 24. Testing and Quality Assurance
  - [ ] 24.1 Comprehensive unit testing




    - Create unit tests for all service classes
    - Implement unit tests for all components
    - Add unit tests for utilities and helpers
    - Achieve minimum 80% code coverage
    - Set up automated test execution
    - _Requirements: All requirements need testing coverage_

  - [ ] 24.2 Integration and end-to-end testing
    - Create integration tests for API endpoints
    - Implement end-to-end tests for critical workflows
    - Add database integration tests
    - Create performance and load testing
    - Set up continuous integration testing
    - _Requirements: All requirements need integration testing_

- [ ] 25. Documentation and Deployment
  - [ ] 25.1 API documentation and developer guides
    - Generate comprehensive Swagger/OpenAPI documentation
    - Create developer setup and contribution guides
    - Add code documentation and inline comments
    - Create deployment and configuration guides
    - Write user manuals for all roles
    - _Requirements: Documentation requirements from design_

  - [ ] 25.2 Production deployment and DevOps
    - Create Docker containers for all services
    - Set up Docker Compose for local development
    - Implement CI/CD pipelines
    - Create production deployment scripts
    - Set up monitoring and logging
    - _Requirements: Production deployment needs_

- [ ] 26. Final Integration and System Testing
  - [ ] 26.1 End-to-end system integration
    - Integrate all modules and ensure seamless operation
    - Test all user workflows from start to finish
    - Verify real-time features and notifications work correctly
    - Test multi-branch and multi-currency functionality
    - Perform security and performance testing
    - _Requirements: All requirements must work together_

  - [ ] 26.2 User acceptance testing and bug fixes
    - Conduct user acceptance testing with demo accounts
    - Fix any bugs or issues discovered during testing
    - Optimize performance and user experience
    - Finalize documentation and deployment guides
    - Prepare system for production release
    - _Requirements: System must meet all acceptance criteria_## UI/U
X Design Standards and Google Fonts Integration

### Google Fonts Recommendations for Business-Friendly Interface

#### Primary Font: **Inter** (Highly Recommended)
- **Why Inter**: Designed specifically for user interfaces, excellent readability, modern and professional
- **Usage**: Body text, forms, tables, general content
- **Weights**: 300 (Light), 400 (Regular), 500 (Medium), 600 (Semi-Bold), 700 (Bold)

#### Secondary Font: **Poppins** (Alternative Option)
- **Why Poppins**: Geometric sans-serif, friendly yet professional, great for headings
- **Usage**: Headings, buttons, navigation, dashboard titles
- **Weights**: 400 (Regular), 500 (Medium), 600 (Semi-Bold), 700 (Bold)

#### Accent Font: **Roboto** (System Fallback)
- **Why Roboto**: Google's flagship font, excellent cross-platform consistency
- **Usage**: Fallback font, system messages, technical content

### CSS Font Implementation
```css
/* Google Fonts Import */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&family=Poppins:wght@400;500;600;700&display=swap');

/* Font Variables */
:root {
  --font-primary: 'Inter', 'Roboto', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  --font-headings: 'Poppins', 'Inter', sans-serif;
  --font-system: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', sans-serif;
}

/* Typography Scale */
body {
  font-family: var(--font-primary);
  font-size: 14px;
  font-weight: 400;
  line-height: 1.5;
  color: #2c3e50;
}

h1, h2, h3, h4, h5, h6 {
  font-family: var(--font-headings);
  font-weight: 600;
  line-height: 1.3;
  color: #1a202c;
}

.display-1 { font-size: 3.5rem; font-weight: 700; }
.display-2 { font-size: 3rem; font-weight: 700; }
.display-3 { font-size: 2.5rem; font-weight: 600; }

h1 { font-size: 2.25rem; }
h2 { font-size: 1.875rem; }
h3 { font-size: 1.5rem; }
h4 { font-size: 1.25rem; }
h5 { font-size: 1.125rem; }
h6 { font-size: 1rem; }

/* Button Typography */
.btn {
  font-family: var(--font-primary);
  font-weight: 500;
  letter-spacing: 0.025em;
}

/* Form Typography */
.form-label {
  font-weight: 500;
  color: #374151;
}

.form-control {
  font-family: var(--font-primary);
  font-size: 14px;
}
```

### Professional Color Palette

#### Primary Colors
```css
:root {
  /* Primary Brand Colors */
  --primary: #3b82f6;        /* Professional Blue */
  --primary-dark: #2563eb;   /* Darker Blue for hover */
  --primary-light: #dbeafe; /* Light Blue for backgrounds */
  
  /* Secondary Colors */
  --secondary: #6b7280;      /* Professional Gray */
  --success: #10b981;        /* Success Green */
  --warning: #f59e0b;        /* Warning Amber */
  --danger: #ef4444;         /* Error Red */
  --info: #06b6d4;          /* Info Cyan */
  
  /* Neutral Colors */
  --gray-50: #f9fafb;
  --gray-100: #f3f4f6;
  --gray-200: #e5e7eb;
  --gray-300: #d1d5db;
  --gray-400: #9ca3af;
  --gray-500: #6b7280;
  --gray-600: #4b5563;
  --gray-700: #374151;
  --gray-800: #1f2937;
  --gray-900: #111827;
  
  /* Background Colors */
  --bg-primary: #ffffff;
  --bg-secondary: #f8fafc;
  --bg-tertiary: #f1f5f9;
  
  /* Text Colors */
  --text-primary: #1f2937;
  --text-secondary: #6b7280;
  --text-muted: #9ca3af;
}
```

### Component Design Standards

#### Cards and Panels
```css
.card {
  border: 1px solid var(--gray-200);
  border-radius: 12px;
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
  transition: all 0.2s ease-in-out;
}

.card:hover {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
  transform: translateY(-1px);
}

.card-header {
  background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-tertiary) 100%);
  border-bottom: 1px solid var(--gray-200);
  padding: 1.25rem 1.5rem;
  border-radius: 12px 12px 0 0;
}

.card-title {
  font-family: var(--font-headings);
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 0;
}
```

#### Buttons with Modern Styling
```css
.btn {
  border-radius: 8px;
  font-weight: 500;
  padding: 0.625rem 1.25rem;
  transition: all 0.15s ease-in-out;
  border: none;
  position: relative;
  overflow: hidden;
}

.btn-primary {
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
  color: white;
  box-shadow: 0 2px 4px rgba(59, 130, 246, 0.3);
}

.btn-primary:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(59, 130, 246, 0.4);
}

.btn-success {
  background: linear-gradient(135deg, var(--success) 0%, #059669 100%);
  color: white;
}

.btn-outline-primary {
  border: 2px solid var(--primary);
  color: var(--primary);
  background: transparent;
}

.btn-outline-primary:hover {
  background: var(--primary);
  color: white;
  transform: translateY(-1px);
}

/* Rounded buttons as specified */
.btn-rounded {
  border-radius: 50px;
}
```

#### Form Controls
```css
.form-control {
  border: 2px solid var(--gray-200);
  border-radius: 8px;
  padding: 0.75rem 1rem;
  font-size: 14px;
  transition: all 0.15s ease-in-out;
}

.form-control:focus {
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  outline: none;
}

.form-label {
  font-weight: 500;
  color: var(--text-primary);
  margin-bottom: 0.5rem;
}

.form-floating > .form-control {
  height: calc(3.5rem + 2px);
  line-height: 1.25;
}
```

#### Navigation and Header
```css
.navbar {
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  padding: 1rem 0;
}

.navbar-brand {
  font-family: var(--font-headings);
  font-weight: 700;
  font-size: 1.5rem;
  color: white !important;
}

.nav-link {
  font-weight: 500;
  color: rgba(255, 255, 255, 0.9) !important;
  transition: color 0.15s ease-in-out;
}

.nav-link:hover {
  color: white !important;
}
```

#### Dashboard Widgets
```css
.dashboard-widget {
  background: white;
  border-radius: 16px;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  border: 1px solid var(--gray-100);
  transition: all 0.2s ease-in-out;
}

.dashboard-widget:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
}

.widget-title {
  font-family: var(--font-headings);
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 1rem;
}

.widget-value {
  font-size: 2rem;
  font-weight: 700;
  color: var(--primary);
}
```

#### Tables and Data Display
```css
.table {
  font-size: 14px;
}

.table th {
  font-family: var(--font-headings);
  font-weight: 600;
  color: var(--text-primary);
  background-color: var(--bg-secondary);
  border-bottom: 2px solid var(--gray-200);
  padding: 1rem 0.75rem;
}

.table td {
  padding: 0.875rem 0.75rem;
  vertical-align: middle;
  border-bottom: 1px solid var(--gray-100);
}

.table-hover tbody tr:hover {
  background-color: var(--bg-tertiary);
}
```

#### Badges and Status Indicators
```css
.badge {
  font-weight: 500;
  font-size: 0.75rem;
  padding: 0.375rem 0.75rem;
  border-radius: 50px;
}

.badge-success {
  background: linear-gradient(135deg, var(--success) 0%, #059669 100%);
  color: white;
}

.badge-warning {
  background: linear-gradient(135deg, var(--warning) 0%, #d97706 100%);
  color: white;
}

.badge-danger {
  background: linear-gradient(135deg, var(--danger) 0%, #dc2626 100%);
  color: white;
}
```

### Animation and Micro-interactions
```css
/* Smooth transitions for all interactive elements */
* {
  transition: all 0.15s ease-in-out;
}

/* Loading animations */
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.loading {
  animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}

/* Fade in animation for content */
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.fade-in {
  animation: fadeIn 0.3s ease-out;
}

/* Hover effects for interactive elements */
.interactive:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}
```

### Responsive Design Breakpoints
```css
/* Mobile First Approach */
/* Extra small devices (phones, 576px and down) */
@media (max-width: 575.98px) {
  .card { margin-bottom: 1rem; }
  .btn { padding: 0.5rem 1rem; font-size: 14px; }
  h1 { font-size: 1.75rem; }
  h2 { font-size: 1.5rem; }
}

/* Small devices (landscape phones, 576px and up) */
@media (min-width: 576px) {
  .container-sm { max-width: 540px; }
}

/* Medium devices (tablets, 768px and up) */
@media (min-width: 768px) {
  .container-md { max-width: 720px; }
}

/* Large devices (desktops, 992px and up) */
@media (min-width: 992px) {
  .container-lg { max-width: 960px; }
}

/* Extra large devices (large desktops, 1200px and up) */
@media (min-width: 1200px) {
  .container-xl { max-width: 1140px; }
}
```

### Dark Mode Support (Optional)
```css
[data-theme="dark"] {
  --bg-primary: #1f2937;
  --bg-secondary: #111827;
  --bg-tertiary: #374151;
  --text-primary: #f9fafb;
  --text-secondary: #d1d5db;
  --text-muted: #9ca3af;
}

.theme-toggle {
  background: none;
  border: 2px solid var(--gray-300);
  border-radius: 50px;
  padding: 0.5rem;
  cursor: pointer;
  transition: all 0.2s ease-in-out;
}
```

### User Experience Enhancements

#### Loading States
```css
.skeleton {
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  animation: loading 1.5s infinite;
}

@keyframes loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
```

#### Success States and Celebrations
```css
.success-animation {
  animation: celebrate 0.6s ease-in-out;
}

@keyframes celebrate {
  0% { transform: scale(1); }
  50% { transform: scale(1.05); }
  100% { transform: scale(1); }
}

.confetti {
  position: relative;
  overflow: hidden;
}

.confetti::before {
  content: '🎉';
  position: absolute;
  top: -10px;
  left: 50%;
  transform: translateX(-50%);
  animation: confetti-fall 1s ease-out;
}
```

This comprehensive design system ensures strideHR will have a modern, professional, and delightful user experience that employees and organizations will love to use daily. The Inter + Poppins combination provides excellent readability while maintaining a contemporary business-friendly appearance.