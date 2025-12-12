USE CrossSetaDB;
GO

-- 1. Add SetaName column to Learners table
ALTER TABLE Learners
ADD SetaName NVARCHAR(50) DEFAULT 'W&RSETA';
GO

-- 2. Create SetaRegistry table for validation (Standardized List)
CREATE TABLE SetaRegistry (
    SetaID INT IDENTITY(1,1) PRIMARY KEY,
    SetaName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255)
);
GO

INSERT INTO SetaRegistry (SetaName, Description) VALUES 
('W&RSETA', 'Wholesale and Retail SETA'),
('CHIETA', 'Chemical Industries SETA'),
('MERSETA', 'Manufacturing, Engineering and Related Services SETA'),
('AGRISETA', 'Agricultural Sector SETA'),
('BANKSETA', 'Banking Sector SETA');
GO

-- 3. Create CrossSetaMatches table for Audit Trail of Cross-SETA detections
CREATE TABLE CrossSetaMatches (
    MatchID INT IDENTITY(1,1) PRIMARY KEY,
    NationalID NVARCHAR(20) NOT NULL,
    DetectedTimestamp DATETIME DEFAULT GETDATE(),
    PrimarySeta NVARCHAR(50), -- The SETA attempting registration
    ExistingSeta NVARCHAR(50), -- The SETA where it was found
    MatchType NVARCHAR(20), -- 'Exact' or 'Fuzzy'
    MatchScore INT
);
GO

-- 4. Update Stored Procedure to include SetaName
DROP PROCEDURE IF EXISTS sp_InsertLearner;
GO

CREATE PROCEDURE sp_InsertLearner
    @NationalID NVARCHAR(20),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @DateOfBirth DATE,
    @Gender NVARCHAR(20),
    @Role NVARCHAR(50),
    @BiometricHash NVARCHAR(MAX),
    @IsVerified BIT,
    @SetaName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Check for duplicates (logic handled in app, but this is a final safeguard)
    IF EXISTS (SELECT 1 FROM Learners WHERE NationalID = @NationalID)
    BEGIN
        THROW 51000, 'National ID already exists.', 1;
    END

    INSERT INTO Learners (
        NationalID, FirstName, LastName, DateOfBirth, Gender, Role, BiometricHash, IsVerified, SetaName
    )
    VALUES (
        @NationalID, @FirstName, @LastName, @DateOfBirth, @Gender, @Role, @BiometricHash, @IsVerified, @SetaName
    );
END;
GO
