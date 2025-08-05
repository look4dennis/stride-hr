-- Minimal StrideHR Setup for Testing
-- Run this in MySQL Workbench to create basic tables and admin user

USE StrideHR_Dev;

-- Create Organizations table
CREATE TABLE Organizations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Address TEXT,
    Email VARCHAR(255),
    Phone VARCHAR(50),
    Logo VARCHAR(500),
    NormalWorkingHours TIME DEFAULT '08:00:00',
    OvertimeRate DECIMAL(5,2) DEFAULT 1.5,
    ProductiveHoursThreshold INT DEFAULT 6,
    BranchIsolationEnabled BOOLEAN DEFAULT TRUE,
    ConfigurationSettings JSON,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create Branches table
CREATE TABLE Branches (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrganizationId INT NOT NULL,
    Name VARCHAR(255) NOT NULL,
    Country VARCHAR(100) NOT NULL,
    CountryCode VARCHAR(10) NOT NULL,
    Currency VARCHAR(10) NOT NULL,
    CurrencySymbol VARCHAR(10) NOT NULL,
    TimeZone VARCHAR(100) NOT NULL,
    Address TEXT,
    City VARCHAR(100),
    State VARCHAR(100),
    PostalCode VARCHAR(20),
    Phone VARCHAR(50),
    Email VARCHAR(255),
    LocalHolidays JSON,
    ComplianceSettings JSON,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

-- Create Employees table
CREATE TABLE Employees (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EmployeeId VARCHAR(50) UNIQUE NOT NULL,
    BranchId INT NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    Phone VARCHAR(50),
    ProfilePhoto VARCHAR(500),
    DateOfBirth DATE,
    JoiningDate DATE NOT NULL,
    Designation VARCHAR(100) NOT NULL,
    Department VARCHAR(100) NOT NULL,
    BasicSalary DECIMAL(15,2) DEFAULT 0,
    Status INT DEFAULT 0,
    ReportingManagerId INT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id),
    FOREIGN KEY (ReportingManagerId) REFERENCES Employees(Id)
);

-- Create Users table
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Username VARCHAR(100) UNIQUE NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    IsEmailVerified BOOLEAN DEFAULT FALSE,
    IsFirstLogin BOOLEAN DEFAULT TRUE,
    ForcePasswordChange BOOLEAN DEFAULT FALSE,
    LastLoginAt DATETIME,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
);

-- Create Roles table
CREATE TABLE Roles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) UNIQUE NOT NULL,
    Description TEXT,
    HierarchyLevel INT DEFAULT 1,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create Permissions table
CREATE TABLE Permissions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Module VARCHAR(50) NOT NULL,
    Action VARCHAR(50) NOT NULL,
    Resource VARCHAR(50) NOT NULL,
    Description TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create RolePermissions table
CREATE TABLE RolePermissions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    IsGranted BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id),
    UNIQUE KEY unique_role_permission (RoleId, PermissionId)
);

-- Create EmployeeRoles table
CREATE TABLE EmployeeRoles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EmployeeId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

-- Insert sample data
INSERT INTO Organizations (Name, Address, Email, Phone, ConfigurationSettings) 
VALUES ('Demo Organization', '123 Business Street', 'admin@demo.com', '+1-555-0123', '{}');

INSERT INTO Branches (OrganizationId, Name, Country, CountryCode, Currency, CurrencySymbol, TimeZone, Address, City, State, PostalCode, Phone, Email, LocalHolidays, ComplianceSettings) 
VALUES (1, 'Main Branch', 'United States', 'US', 'USD', '$', 'America/New_York', '123 Business Street', 'New York', 'NY', '10001', '+1-555-0123', 'main@demo.com', '[]', '{}');

INSERT INTO Employees (EmployeeId, BranchId, FirstName, LastName, Email, Phone, DateOfBirth, JoiningDate, Designation, Department, BasicSalary, Status) 
VALUES ('ADMIN-001', 1, 'System', 'Administrator', 'admin@demo.com', '+1-555-0123', '1990-01-01', CURDATE(), 'System Administrator', 'IT', 100000.00, 0);

-- Password is 'admin123' - hashed with BCrypt
INSERT INTO Users (EmployeeId, Username, Email, PasswordHash, IsActive, IsEmailVerified, IsFirstLogin, ForcePasswordChange) 
VALUES (1, 'admin', 'admin@demo.com', '$2a$11$8K1p/a0dL2LkqvQOuiOX2uy7YhFaihxwjPp/f2laVGWqOczss8Jka', 1, 1, 0, 0);

INSERT INTO Roles (Name, Description, HierarchyLevel, IsActive) 
VALUES 
('SuperAdmin', 'Super Administrator with full system access', 1, 1),
('HRManager', 'HR Manager with HR module access', 2, 1),
('Manager', 'Department Manager', 3, 1),
('Employee', 'Regular Employee', 4, 1);

INSERT INTO Permissions (Name, Module, Action, Resource, Description, IsActive) 
VALUES 
('User.View', 'User', 'View', 'User', 'View users', 1),
('User.Create', 'User', 'Create', 'User', 'Create users', 1),
('User.Update', 'User', 'Update', 'User', 'Update users', 1),
('User.Delete', 'User', 'Delete', 'User', 'Delete users', 1),
('Employee.View', 'Employee', 'View', 'Employee', 'View employees', 1),
('Employee.Create', 'Employee', 'Create', 'Employee', 'Create employees', 1),
('Employee.Update', 'Employee', 'Update', 'Employee', 'Update employees', 1),
('Employee.Delete', 'Employee', 'Delete', 'Employee', 'Delete employees', 1);

-- Assign all permissions to SuperAdmin role
INSERT INTO RolePermissions (RoleId, PermissionId, IsGranted) 
SELECT 1, Id, 1 FROM Permissions;

-- Assign SuperAdmin role to admin user
INSERT INTO EmployeeRoles (EmployeeId, RoleId, AssignedDate, IsActive) 
VALUES (1, 1, NOW(), 1);

SELECT 'Setup completed successfully!' as Status;