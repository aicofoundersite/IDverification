USE CrossSetaDB;
GO

-- Stored Procedure: sp_FindPotentialDuplicates
-- Logic: Search for learners based on National ID (Exact) or Name (Fuzzy-ish via SQL).
-- SQL 2019 Feature Usage: We can use standard string functions. 
-- For a hackathon, a simple OR condition with wildcards or exact match is sufficient 
-- to feed the application layer which handles the heavy Levenshtein logic.
CREATE PROCEDURE sp_FindPotentialDuplicates
    @NationalID NVARCHAR(20),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        LearnerID,
        NationalID,
        FirstName,
        LastName,
        DateOfBirth,
        Gender,
        Role,
        BiometricHash,
        IsVerified
    FROM Learners
    WHERE 
        NationalID = @NationalID
        OR 
        (FirstName = @FirstName AND LastName = @LastName);
END;
GO

-- Stored Procedure: sp_InsertLearner
-- Logic: Encapsulates insertion to ensure data integrity.
CREATE PROCEDURE sp_InsertLearner
    @NationalID NVARCHAR(20),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @DateOfBirth DATE,
    @Gender NVARCHAR(20),
    @Role NVARCHAR(50),
    @BiometricHash NVARCHAR(MAX),
    @IsVerified BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Learners WHERE NationalID = @NationalID)
    BEGIN
        -- In a real app, we might throw an error or update. 
        -- For now, we strictly follow the 'Duplicate' check logic in the app 
        -- so we shouldn't get here if the app does its job.
        -- But as a safeguard:
        THROW 51000, 'National ID already exists.', 1;
    END

    INSERT INTO Learners (
        NationalID, FirstName, LastName, DateOfBirth, Gender, Role, BiometricHash, IsVerified
    )
    VALUES (
        @NationalID, @FirstName, @LastName, @DateOfBirth, @Gender, @Role, @BiometricHash, @IsVerified
    );
END;
GO
