-- StrideHR Database Setup Script
-- Run this in MySQL Workbench as root user (Username: root, Password: Passwordtharoola007$)

-- Create database
CREATE DATABASE IF NOT EXISTS StrideHR_Dev CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create user for the application
CREATE USER IF NOT EXISTS 'stridehr'@'localhost' IDENTIFIED BY 'stridehr123';

-- Grant privileges
GRANT ALL PRIVILEGES ON StrideHR_Dev.* TO 'stridehr'@'localhost';

-- Also allow connections from any host (for development)
CREATE USER IF NOT EXISTS 'stridehr'@'%' IDENTIFIED BY 'stridehr123';
GRANT ALL PRIVILEGES ON StrideHR_Dev.* TO 'stridehr'@'%';

-- Flush privileges
FLUSH PRIVILEGES;

-- Show databases to confirm
SHOW DATABASES;

-- Show users to confirm
SELECT User, Host FROM mysql.user WHERE User = 'stridehr';