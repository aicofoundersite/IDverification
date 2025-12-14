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
    @IsVerified BIT,
    
    -- New Params
    @Nationality NVARCHAR(50) = NULL,
    @Title NVARCHAR(20) = NULL,
    @MiddleName NVARCHAR(100) = NULL,
    @Age INT = NULL,
    @EquityCode NVARCHAR(50) = NULL,
    @HomeLanguage NVARCHAR(50) = NULL,
    @PreviousLastName NVARCHAR(100) = NULL,
    @Municipality NVARCHAR(100) = NULL,
    @DisabilityStatus NVARCHAR(50) = NULL,
    @CitizenStatus NVARCHAR(50) = NULL,
    @StatsAreaCode NVARCHAR(50) = NULL,
    @SocioEconomicStatus NVARCHAR(50) = NULL,
    @PopiActConsent BIT = 0,
    @PopiActDate DATETIME = NULL,
    @SetaName NVARCHAR(50) = NULL,
    @PhoneNumber NVARCHAR(20) = NULL,
    @POBox NVARCHAR(100) = NULL,
    @CellphoneNumber NVARCHAR(20) = NULL,
    @StreetName NVARCHAR(100) = NULL,
    @PostalSuburb NVARCHAR(100) = NULL,
    @StreetHouseNo NVARCHAR(20) = NULL,
    @PhysicalSuburb NVARCHAR(100) = NULL,
    @City NVARCHAR(100) = NULL,
    @FaxNumber NVARCHAR(20) = NULL,
    @PostalCode NVARCHAR(20) = NULL,
    @EmailAddress NVARCHAR(100) = NULL,
    @Province NVARCHAR(50) = NULL,
    @UrbanRural NVARCHAR(50) = NULL,
    @IsResidentialAddressSameAsPostal BIT = 0,
    @Disability_Communication NVARCHAR(50) = NULL,
    @Disability_Hearing NVARCHAR(50) = NULL,
    @Disability_Remembering NVARCHAR(50) = NULL,
    @Disability_Seeing NVARCHAR(50) = NULL,
    @Disability_SelfCare NVARCHAR(50) = NULL,
    @Disability_Walking NVARCHAR(50) = NULL,
    @LastSchoolAttended NVARCHAR(200) = NULL,
    @LastSchoolYear NVARCHAR(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Learners WHERE NationalID = @NationalID)
    BEGIN
        THROW 51000, 'National ID already exists.', 1;
    END

    INSERT INTO Learners (
        NationalID, FirstName, LastName, DateOfBirth, Gender, Role, BiometricHash, IsVerified,
        Nationality, Title, MiddleName, Age, EquityCode, HomeLanguage, PreviousLastName, Municipality,
        DisabilityStatus, CitizenStatus, StatsAreaCode, SocioEconomicStatus, PopiActConsent, PopiActDate, SetaName,
        PhoneNumber, POBox, CellphoneNumber, StreetName, PostalSuburb, StreetHouseNo, PhysicalSuburb, City,
        FaxNumber, PostalCode, EmailAddress, Province, UrbanRural, IsResidentialAddressSameAsPostal,
        Disability_Communication, Disability_Hearing, Disability_Remembering, Disability_Seeing, Disability_SelfCare, Disability_Walking,
        LastSchoolAttended, LastSchoolYear
    )
    VALUES (
        @NationalID, @FirstName, @LastName, @DateOfBirth, @Gender, @Role, @BiometricHash, @IsVerified,
        @Nationality, @Title, @MiddleName, @Age, @EquityCode, @HomeLanguage, @PreviousLastName, @Municipality,
        @DisabilityStatus, @CitizenStatus, @StatsAreaCode, @SocioEconomicStatus, @PopiActConsent, @PopiActDate, @SetaName,
        @PhoneNumber, @POBox, @CellphoneNumber, @StreetName, @PostalSuburb, @StreetHouseNo, @PhysicalSuburb, @City,
        @FaxNumber, @PostalCode, @EmailAddress, @Province, @UrbanRural, @IsResidentialAddressSameAsPostal,
        @Disability_Communication, @Disability_Hearing, @Disability_Remembering, @Disability_Seeing, @Disability_SelfCare, @Disability_Walking,
        @LastSchoolAttended, @LastSchoolYear
    );
END;
GO

-- Stored Procedure: sp_InsertUser
CREATE PROCEDURE sp_InsertUser
    @IDType NVARCHAR(50),
    @NationalID NVARCHAR(50),
    @Title NVARCHAR(20),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(100),
    @Province NVARCHAR(50),
    @UserName NVARCHAR(100),
    @PasswordHash NVARCHAR(MAX),
    @SecurityQuestion NVARCHAR(200),
    @SecurityAnswer NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Users WHERE UserName = @UserName)
    BEGIN
        THROW 51000, 'Username already exists.', 1;
    END

    INSERT INTO Users (
        IDType, NationalID, Title, FirstName, LastName, Email, Province, UserName, PasswordHash, SecurityQuestion, SecurityAnswer
    )
    VALUES (
        @IDType, @NationalID, @Title, @FirstName, @LastName, @Email, @Province, @UserName, @PasswordHash, @SecurityQuestion, @SecurityAnswer
    );
END;
GO
