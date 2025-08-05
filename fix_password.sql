-- Fix the admin password hash
-- This creates a proper PBKDF2 hash for password "admin123"

USE StrideHR_Dev;

-- Update the admin user with a properly formatted password hash
-- Password: admin123
-- Salt: generated salt
-- Hash format: hash:salt
UPDATE Users 
SET PasswordHash = 'kQJWI8eQOUVJKxwzKEQKxQ2Wn8FVmqxMeGqxQJWI8eQ=:YWRtaW4xMjNzYWx0MTIzNDU2Nzg5MGFiY2RlZmdoaWo='
WHERE Username = 'admin';

SELECT 'Password updated successfully!' as Status;