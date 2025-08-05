-- StrideHR Initial Setup Script
-- This script creates the basic data needed to log into the system

-- Insert Organization
INSERT INTO Organizations (Name, Address, Email, Phone, NormalWorkingHours, OvertimeRate, ProductiveHoursThreshold, BranchIsolationEnabled, ConfigurationSettings, CreatedAt, UpdatedAt)
VALUES ('Demo Organization', '123 Business Street', 'admin@demo.com', '+1-555-0123', '08:00:00', 1.5, 6, 1, '{}', NOW(), NOW());

-- Insert Branch
INSERT INTO Branches (OrganizationId, Name, Country, CountryCode, Currency, CurrencySymbol, TimeZone, Address, City, State, PostalCode, Phone, Email, LocalHolidays, ComplianceSettings, IsActive, CreatedAt, UpdatedAt)
VALUES (1, 'Main Branch', 'United States', 'US', 'USD', '$', 'America/New_York', '123 Business Street', 'New York', 'NY', '10001', '+1-555-0123', 'main@demo.com', '[]', '{}', 1, NOW(), NOW());

-- Insert Employee (Admin)
INSERT INTO Employees (EmployeeId, BranchId, FirstName, LastName, Email, Phone, DateOfBirth, JoiningDate, Designation, Department, BasicSalary, Status, CreatedAt, UpdatedAt)
VALUES ('ADMIN-001', 1, 'System', 'Administrator', 'admin@demo.com', '+1-555-0123', '1990-01-01', NOW(), 'System Administrator', 'IT', 100000.00, 0, NOW(), NOW());

-- Insert User (Password is 'admin123' - hashed with BCrypt)
INSERT INTO Users (EmployeeId, Username, Email, PasswordHash, IsActive, IsEmailVerified, IsFirstLogin, ForcePasswordChange, CreatedAt, UpdatedAt)
VALUES (1, 'admin', 'admin@demo.com', '$2a$11$8K1p/a0dL2LkqvQOuiOX2uy7YhFaihxwjPp/f2laVGWqOczss8Jka', 1, 1, 0, 0, NOW(), NOW());

-- Insert Roles
INSERT INTO Roles (Name, Description, HierarchyLevel, IsActive, CreatedAt, UpdatedAt)
VALUES 
('SuperAdmin', 'Super Administrator with full system access', 1, 1, NOW(), NOW()),
('HRManager', 'HR Manager with HR module access', 2, 1, NOW(), NOW()),
('Manager', 'Department Manager', 3, 1, NOW(), NOW()),
('Employee', 'Regular Employee', 4, 1, NOW(), NOW());

-- Insert Permissions (Basic set)
INSERT INTO Permissions (Name, Module, Action, Resource, Description, IsActive, CreatedAt, UpdatedAt)
VALUES 
('User.View', 'User', 'View', 'User', 'View users', 1, NOW(), NOW()),
('User.Create', 'User', 'Create', 'User', 'Create users', 1, NOW(), NOW()),
('User.Update', 'User', 'Update', 'User', 'Update users', 1, NOW(), NOW()),
('User.Delete', 'User', 'Delete', 'User', 'Delete users', 1, NOW(), NOW()),
('Employee.View', 'Employee', 'View', 'Employee', 'View employees', 1, NOW(), NOW()),
('Employee.Create', 'Employee', 'Create', 'Employee', 'Create employees', 1, NOW(), NOW()),
('Employee.Update', 'Employee', 'Update', 'Employee', 'Update employees', 1, NOW(), NOW()),
('Employee.Delete', 'Employee', 'Delete', 'Employee', 'Delete employees', 1, NOW(), NOW());

-- Assign all permissions to SuperAdmin role
INSERT INTO RolePermissions (RoleId, PermissionId, IsGranted, CreatedAt, UpdatedAt)
SELECT 1, Id, 1, NOW(), NOW() FROM Permissions;

-- Assign SuperAdmin role to admin user
INSERT INTO EmployeeRoles (EmployeeId, RoleId, AssignedDate, IsActive, CreatedAt, UpdatedAt)
VALUES (1, 1, NOW(), 1, NOW(), NOW());