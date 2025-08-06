USE StrideHR_Dev;

-- First ensure we have a default organization
INSERT IGNORE INTO Organizations (Id, Name, Address, Email) 
VALUES (1, 'StrideHR Default', '123 Main St', 'admin@stridehr.com');

-- Create default branch
INSERT IGNORE INTO Branches (Id, Name, OrganizationId, Address)
VALUES (1, 'Main Branch', 1, '123 Main St');

-- Create admin role if not exists
INSERT IGNORE INTO Roles (Id, Name, HierarchyLevel, Description, CreatedAt)
VALUES (1, 'Administrator', 100, 'System Administrator', NOW());

-- Create admin user with password 'Admin123!'
-- Password hash is for 'Admin123!' - you should change this in production
INSERT IGNORE INTO Users (
    Email,
    PasswordHash,
    FirstName,
    LastName,
    EmployeeId,
    BranchId,
    IsActive
)
VALUES (
    'admin@stridehr.com',
    '$2a$11$K3g6XpyZZ5t0P9P8h5O7iu5nHIOB9RyZJ3ECMSq.WkfqK0MJGg5aC',
    'System',
    'Admin',
    'ADMIN001',
    1,
    1
);

-- Assign admin role to the admin user
INSERT IGNORE INTO UserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM Users u
CROSS JOIN Roles r
WHERE u.Email = 'admin@stridehr.com'
AND r.Name = 'Administrator';

-- Grant all permissions to admin role
INSERT IGNORE INTO RolePermissions (RoleId, Permission)
SELECT r.Id, p.Name
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'Administrator';
