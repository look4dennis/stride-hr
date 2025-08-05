-- Create a simple test to verify our user data is correct
USE StrideHR_Dev;

SELECT 
    u.Id,
    u.Username,
    u.Email,
    u.PasswordHash,
    u.IsActive,
    u.IsDeleted,
    e.Id as EmployeeId,
    e.FirstName,
    e.LastName,
    e.BranchId,
    b.Name as BranchName,
    b.OrganizationId
FROM Users u
JOIN Employees e ON u.EmployeeId = e.Id
JOIN Branches b ON e.BranchId = b.Id
WHERE u.Username = 'admin';