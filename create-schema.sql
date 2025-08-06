USE StrideHR_Dev;

-- Create Organizations table
CREATE TABLE IF NOT EXISTS Organizations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Address VARCHAR(500) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    Logo VARCHAR(500),
    Website VARCHAR(200),
    TaxId VARCHAR(50),
    RegistrationNumber VARCHAR(50),
    NormalWorkingHours TIME NOT NULL DEFAULT '08:00:00',
    OvertimeRate DECIMAL(5,2) NOT NULL DEFAULT 1.5,
    ProductiveHoursThreshold INT NOT NULL DEFAULT 6,
    BranchIsolationEnabled BOOLEAN NOT NULL DEFAULT TRUE,
    ConfigurationSettings JSON NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    CreatedBy VARCHAR(100) NULL,
    UpdatedBy VARCHAR(100) NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    DeletedAt DATETIME NULL,
    DeletedBy VARCHAR(100) NULL
);

-- Create Branches table
CREATE TABLE IF NOT EXISTS Branches (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrganizationId INT NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Address VARCHAR(500) NOT NULL,
    Phone VARCHAR(20),
    Email VARCHAR(100),
    Manager VARCHAR(100),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    CreatedBy VARCHAR(100) NULL,
    UpdatedBy VARCHAR(100) NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    DeletedAt DATETIME NULL,
    DeletedBy VARCHAR(100) NULL,
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

-- Create Roles table
CREATE TABLE IF NOT EXISTS Roles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL,
    HierarchyLevel INT NOT NULL,
    Description VARCHAR(200),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    CreatedBy VARCHAR(100) NULL,
    UpdatedBy VARCHAR(100) NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    DeletedAt DATETIME NULL,
    DeletedBy VARCHAR(100) NULL
);

-- Create Permissions table
CREATE TABLE IF NOT EXISTS Permissions (
    Name VARCHAR(100) PRIMARY KEY,
    Description VARCHAR(200),
    Category VARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create Users table
CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(200) NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    EmployeeId VARCHAR(50) NOT NULL UNIQUE,
    BranchId INT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    LastLoginAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    CreatedBy VARCHAR(100) NULL,
    UpdatedBy VARCHAR(100) NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    DeletedAt DATETIME NULL,
    DeletedBy VARCHAR(100) NULL,
    FOREIGN KEY (BranchId) REFERENCES Branches(Id)
);

-- Create UserRoles table
CREATE TABLE IF NOT EXISTS UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

-- Create RolePermissions table
CREATE TABLE IF NOT EXISTS RolePermissions (
    RoleId INT NOT NULL,
    Permission VARCHAR(100) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (RoleId, Permission),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    FOREIGN KEY (Permission) REFERENCES Permissions(Name)
);

-- Insert basic permissions
INSERT IGNORE INTO Permissions (Name, Description, Category, CreatedAt) VALUES
('users.view', 'View users', 'Users', NOW()),
('users.create', 'Create users', 'Users', NOW()),
('users.edit', 'Edit users', 'Users', NOW()),
('users.delete', 'Delete users', 'Users', NOW()),
('roles.view', 'View roles', 'Roles', NOW()),
('roles.create', 'Create roles', 'Roles', NOW()),
('roles.edit', 'Edit roles', 'Roles', NOW()),
('roles.delete', 'Delete roles', 'Roles', NOW()),
('permissions.view', 'View permissions', 'Permissions', NOW()),
('permissions.assign', 'Assign permissions', 'Permissions', NOW());
