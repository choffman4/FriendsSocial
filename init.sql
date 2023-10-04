CREATE TABLE UserAccounts (
    UserId CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    IsActive BIT DEFAULT 1 NOT NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO UserAccounts (Email, PasswordHash) VALUES
    ('alex@123.com', '5d41402abc4b2a76b9719d911017c592'),  -- Note: This is a hash for the string "hello", just for demonstration purposes.
    ('bob.smith@123.com', '098f6bcd4621d373cade4e832627b4f6'),     -- Note: This is a hash for the string "test".
    ('connor@123.edu', 'a1b2c3d4e5f678901234567890abcdef');  -- Note: This is a made-up hash for demonstration.