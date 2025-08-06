-- StrideHR Database Initialization Script
-- This script creates the database and sets up initial configuration

-- Create database if it doesn't exist
CREATE DATABASE IF NOT EXISTS StrideHR_Dev 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE StrideHR_Dev;

-- Set SQL mode for better compatibility
SET sql_mode = 'STRICT_TRANS_TABLES,NO_ZERO_DATE,NO_ZERO_IN_DATE,ERROR_FOR_DIVISION_BY_ZERO';

-- Create a user for the application if it doesn't exist
-- Note: In production, use a dedicated user with limited privileges
CREATE USER IF NOT EXISTS 'stridehr_user'@'localhost' IDENTIFIED BY 'StrideHR_2025!';
GRANT ALL PRIVILEGES ON StrideHR_Dev.* TO 'stridehr_user'@'localhost';

-- Create a user for remote connections (if needed)
CREATE USER IF NOT EXISTS 'stridehr_user'@'%' IDENTIFIED BY 'StrideHR_2025!';
GRANT ALL PRIVILEGES ON StrideHR_Dev.* TO 'stridehr_user'@'%';

-- Flush privileges to ensure changes take effect
FLUSH PRIVILEGES;

-- Set timezone to UTC for consistency
SET time_zone = '+00:00';

-- Create initial configuration table for system settings
CREATE TABLE IF NOT EXISTS SystemConfiguration (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ConfigKey VARCHAR(255) NOT NULL UNIQUE,
    ConfigValue TEXT,
    Description TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Insert initial system configuration
INSERT IGNORE INTO SystemConfiguration (ConfigKey, ConfigValue, Description) VALUES
('DatabaseVersion', '1.0.0', 'Current database schema version'),
('InitialSetupCompleted', 'false', 'Indicates if initial setup wizard has been completed'),
('SuperAdminCreated', 'false', 'Indicates if super admin user has been created'),
('DefaultOrganizationCreated', 'false', 'Indicates if default organization has been created'),
('SystemInstallDate', NOW(), 'Date when the system was first installed');

-- Create audit log table for tracking database changes
CREATE TABLE IF NOT EXISTS DatabaseAuditLog (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TableName VARCHAR(255) NOT NULL,
    Operation ENUM('INSERT', 'UPDATE', 'DELETE') NOT NULL,
    RecordId VARCHAR(255),
    OldValues JSON,
    NewValues JSON,
    UserId INT,
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    IPAddress VARCHAR(45),
    UserAgent TEXT
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_audit_table_name ON DatabaseAuditLog(TableName);
CREATE INDEX IF NOT EXISTS idx_audit_timestamp ON DatabaseAuditLog(Timestamp);
CREATE INDEX IF NOT EXISTS idx_audit_user_id ON DatabaseAuditLog(UserId);

-- Create a stored procedure for database health check
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS CheckDatabaseHealth()
BEGIN
    DECLARE table_count INT DEFAULT 0;
    DECLARE user_count INT DEFAULT 0;
    DECLARE config_count INT DEFAULT 0;
    
    -- Count tables
    SELECT COUNT(*) INTO table_count 
    FROM information_schema.tables 
    WHERE table_schema = 'StrideHR_Dev';
    
    -- Count users (if Users table exists)
    SELECT COUNT(*) INTO user_count 
    FROM information_schema.tables 
    WHERE table_schema = 'StrideHR_Dev' AND table_name = 'Users';
    
    IF user_count > 0 THEN
        SELECT COUNT(*) INTO user_count FROM Users;
    END IF;
    
    -- Count configuration entries
    SELECT COUNT(*) INTO config_count FROM SystemConfiguration;
    
    -- Return health status
    SELECT 
        'Database Health Check' as Status,
        table_count as TableCount,
        user_count as UserCount,
        config_count as ConfigurationCount,
        NOW() as CheckedAt;
END //
DELIMITER ;

-- Create a function to get system configuration
DELIMITER //
CREATE FUNCTION IF NOT EXISTS GetSystemConfig(config_key VARCHAR(255))
RETURNS TEXT
READS SQL DATA
DETERMINISTIC
BEGIN
    DECLARE config_value TEXT DEFAULT NULL;
    
    SELECT ConfigValue INTO config_value 
    FROM SystemConfiguration 
    WHERE ConfigKey = config_key 
    LIMIT 1;
    
    RETURN config_value;
END //
DELIMITER ;

-- Log the initialization
INSERT INTO DatabaseAuditLog (TableName, Operation, RecordId, NewValues, Timestamp) 
VALUES ('SystemConfiguration', 'INSERT', 'INIT', JSON_OBJECT('action', 'Database initialized'), NOW());

-- Display initialization status
SELECT 
    'StrideHR Database Initialization Complete' as Message,
    DATABASE() as DatabaseName,
    USER() as CurrentUser,
    NOW() as InitializedAt;

-- Show system configuration
SELECT * FROM SystemConfiguration ORDER BY ConfigKey;