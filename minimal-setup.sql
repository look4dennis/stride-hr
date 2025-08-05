-- Minimal StrideHR Database Setup
-- Run this in MySQL Workbench after creating the empty database

USE StrideHR_Dev;

-- Set permissive SQL mode
SET sql_mode = '';

-- Create essential tables for basic functionality
CREATE TABLE `Organizations` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(200) NOT NULL,
    `Description` text,
    `LogoUrl` varchar(500),
    `Website` varchar(200),
    `Email` varchar(100),
    `Phone` varchar(20),
    `Address` text,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime NULL,
    `CreatedBy` varchar(100),
    `UpdatedBy` varchar(100),
    `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100),
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Branches` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OrganizationId` int NOT NULL,
    `Name` varchar(200) NOT NULL,
    `Code` varchar(10) NOT NULL,
    `Address` text,
    `City` varchar(100),
    `State` varchar(100),
    `Country` varchar(100),
    `PostalCode` varchar(20),
    `Phone` varchar(20),
    `Email` varchar(100),
    `TimeZone` varchar(50),
    `Currency` varchar(3),
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime NULL,
    `CreatedBy` varchar(100),
    `UpdatedBy` varchar(100),
    `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100),
    PRIMARY KEY (`Id`),
    FOREIGN KEY (`OrganizationId`) REFERENCES `Organizations`(`Id`)
);

CREATE TABLE `Employees` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `BranchId` int NOT NULL,
    `EmployeeCode` varchar(20) NOT NULL,
    `FirstName` varchar(100) NOT NULL,
    `LastName` varchar(100) NOT NULL,
    `Email` varchar(100) NOT NULL,
    `Phone` varchar(20),
    `DateOfBirth` date,
    `HireDate` date NOT NULL,
    `JobTitle` varchar(100),
    `Department` varchar(100),
    `Salary` decimal(18,2),
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime NULL,
    `CreatedBy` varchar(100),
    `UpdatedBy` varchar(100),
    `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UK_Employees_EmployeeCode` (`EmployeeCode`),
    UNIQUE KEY `UK_Employees_Email` (`Email`),
    FOREIGN KEY (`BranchId`) REFERENCES `Branches`(`Id`)
);

CREATE TABLE `Users` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `EmployeeId` int NOT NULL,
    `Username` varchar(50) NOT NULL,
    `PasswordHash` varchar(255) NOT NULL,
    `Email` varchar(100) NOT NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `LastLoginAt` datetime NULL,
    `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime NULL,
    `CreatedBy` varchar(100),
    `UpdatedBy` varchar(100),
    `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UK_Users_Username` (`Username`),
    UNIQUE KEY `UK_Users_Email` (`Email`),
    FOREIGN KEY (`EmployeeId`) REFERENCES `Employees`(`Id`)
);

CREATE TABLE `Roles` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(100) NOT NULL,
    `Description` text,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime NULL,
    `CreatedBy` varchar(100),
    `UpdatedBy` varchar(100),
    `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UK_Roles_Name` (`Name`)
);

CREATE TABLE `EmployeeRoles` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `EmployeeId` int NOT NULL,
    `RoleId` int NOT NULL,
    `AssignedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `AssignedBy` varchar(100),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UK_EmployeeRoles` (`EmployeeId`, `RoleId`),
    FOREIGN KEY (`EmployeeId`) REFERENCES `Employees`(`Id`),
    FOREIGN KEY (`RoleId`) REFERENCES `Roles`(`Id`)
);

-- Insert sample data
INSERT INTO `Organizations` (`Name`, `Description`, `Email`, `Phone`) 
VALUES ('StrideHR Demo Company', 'Demo organization for StrideHR system', 'admin@stridehr.com', '+1-555-0123');

INSERT INTO `Branches` (`OrganizationId`, `Name`, `Code`, `City`, `Country`, `TimeZone`, `Currency`) 
VALUES (1, 'Head Office', 'HO', 'New York', 'USA', 'America/New_York', 'USD');

INSERT INTO `Roles` (`Name`, `Description`) VALUES 
('SuperAdmin', 'System administrator with full access'),
('HRManager', 'HR manager with employee management access'),
('Manager', 'Department manager'),
('Employee', 'Regular employee');

-- Create EF Migration History table
CREATE TABLE `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

-- Insert migration record
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20250805104540_InitialSetup', '8.0.13');

SELECT 'Database setup completed successfully!' as Status;