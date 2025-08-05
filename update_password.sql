-- Update admin password to a simple one that works with PBKDF2
-- Password will be "password" (simple for testing)

USE StrideHR_Dev;

-- First, let's create a simple hash for "password"
-- Using a known salt and the PBKDF2 algorithm
UPDATE Users 
SET PasswordHash = 'Jg45HuwT58PpLGHX+9+dIiow4pqANANdF5h4p1TzOtc=:c2FsdDEyMzQ1Njc4OTA='
WHERE Username = 'admin';

SELECT 'Password updated to "password"' as Status;