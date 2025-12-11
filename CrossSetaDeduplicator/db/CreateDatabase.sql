-- Create Database
CREATE DATABASE CrossSetaDB;
GO

USE CrossSetaDB;
GO

-- Table: Learners
-- Stores the demographic and biometric hash data for each learner.
CREATE TABLE Learners (
    LearnerID INT IDENTITY(1,1) PRIMARY KEY,
    NationalID NVARCHAR(20) NOT NULL UNIQUE, -- Unique National ID
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(20),
    Role NVARCHAR(50) DEFAULT 'Learner', -- Learner, Assessor, Moderator
    BiometricHash NVARCHAR(MAX), -- Placeholder for biometric data hash
    RegistrationDate DATETIME DEFAULT GETDATE(),
    IsVerified BIT DEFAULT 0 -- Status of KYC verification
);
GO

-- Index on NationalID for fast exact lookups
CREATE INDEX IX_Learners_NationalID ON Learners(NationalID);

-- Index on Name fields for searching (though fuzzy logic will be in App, this helps filtering)
CREATE INDEX IX_Learners_Name ON Learners(FirstName, LastName);
GO

-- Table: DuplicateAuditLog
-- Tracks every duplicate check execution and its result.
CREATE TABLE DuplicateAuditLog (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    TestedNationalID NVARCHAR(20),
    MatchedLearnerID INT, -- Foreign Key to Learners if a match is found
    MatchScore INT, -- 100 for Exact, <100 for Fuzzy
    MatchType NVARCHAR(50), -- 'Exact', 'Fuzzy', 'Biometric'
    CheckDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MatchedLearnerID) REFERENCES Learners(LearnerID)
);
GO
