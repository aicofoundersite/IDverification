USE CrossSetaDB;
GO

-- =============================================
-- 1. Update Learners Table
-- =============================================
ALTER TABLE Learners ADD
    -- Personal Info
    Nationality NVARCHAR(50),
    Title NVARCHAR(20),
    MiddleName NVARCHAR(100),
    Age INT,
    EquityCode NVARCHAR(50),
    HomeLanguage NVARCHAR(50),
    PreviousLastName NVARCHAR(100),
    Municipality NVARCHAR(100),
    DisabilityStatus NVARCHAR(50),
    CitizenStatus NVARCHAR(50),
    StatsAreaCode NVARCHAR(50),
    SocioEconomicStatus NVARCHAR(50),
    PopiActConsent BIT DEFAULT 0,
    PopiActDate DATETIME,
    SetaName NVARCHAR(50),

    -- Contact Details
    PhoneNumber NVARCHAR(20),
    POBox NVARCHAR(100),
    CellphoneNumber NVARCHAR(20),
    StreetName NVARCHAR(100),
    PostalSuburb NVARCHAR(100),
    StreetHouseNo NVARCHAR(20),
    PhysicalSuburb NVARCHAR(100),
    City NVARCHAR(100),
    FaxNumber NVARCHAR(20),
    PostalCode NVARCHAR(20),
    EmailAddress NVARCHAR(100),
    Province NVARCHAR(50),
    UrbanRural NVARCHAR(50),
    IsResidentialAddressSameAsPostal BIT DEFAULT 0,

    -- Disability Details
    Disability_Communication NVARCHAR(50),
    Disability_Hearing NVARCHAR(50),
    Disability_Remembering NVARCHAR(50),
    Disability_Seeing NVARCHAR(50),
    Disability_SelfCare NVARCHAR(50),
    Disability_Walking NVARCHAR(50),

    -- Education Details
    LastSchoolAttended NVARCHAR(200),
    LastSchoolYear NVARCHAR(10);
GO

-- =============================================
-- 2. Create Users Table
-- =============================================
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    IDType NVARCHAR(50),
    NationalID NVARCHAR(50), -- Can be Passport or ID
    Title NVARCHAR(20),
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Email NVARCHAR(100),
    Province NVARCHAR(50),
    UserName NVARCHAR(100) UNIQUE,
    PasswordHash NVARCHAR(MAX), -- Store hashed password
    SecurityQuestion NVARCHAR(200),
    SecurityAnswer NVARCHAR(MAX), -- Store hashed answer ideally, but plain for now based on requirements
    RegistrationDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);
GO

-- =============================================
-- 3. Update sp_InsertLearner
-- =============================================
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

-- =============================================
-- 4. Create sp_InsertUser
-- =============================================
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
