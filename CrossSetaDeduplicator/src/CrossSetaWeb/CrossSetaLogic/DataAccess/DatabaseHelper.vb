Imports System
Imports System.Data
Imports Microsoft.Data.SqlClient
Imports CrossSetaLogic.Models
Imports System.Collections.Generic

Namespace DataAccess
    Public Class DatabaseHelper
        Implements IDatabaseHelper

        Private _connectionString As String = "Server=localhost;Database=CrossSetaDB;Trusted_Connection=True;TrustServerCertificate=True;"

        Public Sub New(Optional connectionString As String = Nothing)
            ' Priority 1: Constructor Argument
            If Not String.IsNullOrEmpty(connectionString) Then
                _connectionString = connectionString
                Return
            End If

            ' Priority 2: Environment Variable (Production/Docker/Cloud Proxy)
            Dim envConn As String = Environment.GetEnvironmentVariable("CROSS_SETA_DB_CONNECTION")
            If Not String.IsNullOrEmpty(envConn) Then
                _connectionString = envConn
                Return
            End If

            ' Priority 3: Environment Variable for Password only (Local App -> Cloud DB Proxy)
            Dim dbPassword As String = Environment.GetEnvironmentVariable("CROSS_SETA_DB_PASSWORD")
            If Not String.IsNullOrEmpty(dbPassword) Then
                ' Assumes localhost proxy on default port 1433
                _connectionString = $"Server=127.0.0.1,1433;Database=CrossSetaDB;User Id=sa;Password={dbPassword};Encrypt=False;"
                Return
            End If
        End Sub

        Public Sub InsertLearner(learner As LearnerModel) Implements IDatabaseHelper.InsertLearner
            Using conn As New SqlConnection(_connectionString)
                Dim cmd As New SqlCommand("sp_InsertLearner", conn)
                cmd.CommandType = CommandType.StoredProcedure

                ' Basic Fields
                cmd.Parameters.AddWithValue("@NationalID", GetValue(learner.NationalID))
                cmd.Parameters.AddWithValue("@FirstName", GetValue(learner.FirstName))
                cmd.Parameters.AddWithValue("@LastName", GetValue(learner.LastName))
                
                ' Explicitly use SqlDbType.Date for DateOfBirth to support full range (0001-9999)
                ' AddWithValue defaults to SqlDbType.DateTime which only supports 1753+
                cmd.Parameters.Add("@DateOfBirth", SqlDbType.Date).Value = learner.DateOfBirth

                cmd.Parameters.AddWithValue("@Gender", GetValue(learner.Gender))
                cmd.Parameters.AddWithValue("@Role", If(String.IsNullOrEmpty(learner.Role), "Learner", learner.Role))
                cmd.Parameters.AddWithValue("@BiometricHash", GetValue(learner.BiometricHash))
                cmd.Parameters.AddWithValue("@IsVerified", learner.IsVerified)
                cmd.Parameters.AddWithValue("@SetaName", If(String.IsNullOrEmpty(learner.SetaName), "W&RSETA", learner.SetaName))

                ' New Personal Info Fields
                cmd.Parameters.AddWithValue("@Nationality", GetValue(learner.Nationality))
                cmd.Parameters.AddWithValue("@Title", GetValue(learner.Title))
                cmd.Parameters.AddWithValue("@MiddleName", GetValue(learner.MiddleName))
                cmd.Parameters.AddWithValue("@Age", If(learner.Age = 0, DBNull.Value, CObj(learner.Age)))
                cmd.Parameters.AddWithValue("@EquityCode", GetValue(learner.EquityCode))
                cmd.Parameters.AddWithValue("@HomeLanguage", GetValue(learner.HomeLanguage))
                cmd.Parameters.AddWithValue("@PreviousLastName", GetValue(learner.PreviousLastName))
                cmd.Parameters.AddWithValue("@Municipality", GetValue(learner.Municipality))
                cmd.Parameters.AddWithValue("@DisabilityStatus", GetValue(learner.DisabilityStatus))
                cmd.Parameters.AddWithValue("@CitizenStatus", GetValue(learner.CitizenStatus))
                cmd.Parameters.AddWithValue("@StatsAreaCode", GetValue(learner.StatsAreaCode))
                cmd.Parameters.AddWithValue("@SocioEconomicStatus", GetValue(learner.SocioEconomicStatus))
                cmd.Parameters.AddWithValue("@PopiActConsent", learner.PopiActConsent)
                
                ' PopiActDate is DATETIME in SQL (1753+), unlike DateOfBirth which is DATE (0001+)
                Dim popiVal As Object = DBNull.Value
                If learner.PopiActDate >= New DateTime(1753, 1, 1) Then
                    popiVal = learner.PopiActDate
                End If
                cmd.Parameters.AddWithValue("@PopiActDate", popiVal)

                ' Contact Details
                cmd.Parameters.AddWithValue("@PhoneNumber", GetValue(learner.PhoneNumber))
                cmd.Parameters.AddWithValue("@POBox", GetValue(learner.POBox))
                cmd.Parameters.AddWithValue("@CellphoneNumber", GetValue(learner.CellphoneNumber))
                cmd.Parameters.AddWithValue("@StreetName", GetValue(learner.StreetName))
                cmd.Parameters.AddWithValue("@PostalSuburb", GetValue(learner.PostalSuburb))
                cmd.Parameters.AddWithValue("@StreetHouseNo", GetValue(learner.StreetHouseNo))
                cmd.Parameters.AddWithValue("@PhysicalSuburb", GetValue(learner.PhysicalSuburb))
                cmd.Parameters.AddWithValue("@City", GetValue(learner.City))
                cmd.Parameters.AddWithValue("@FaxNumber", GetValue(learner.FaxNumber))
                cmd.Parameters.AddWithValue("@PostalCode", GetValue(learner.PostalCode))
                cmd.Parameters.AddWithValue("@EmailAddress", GetValue(learner.EmailAddress))
                cmd.Parameters.AddWithValue("@Province", GetValue(learner.Province))
                cmd.Parameters.AddWithValue("@UrbanRural", GetValue(learner.UrbanRural))
                cmd.Parameters.AddWithValue("@IsResidentialAddressSameAsPostal", learner.IsResidentialAddressSameAsPostal)

                ' Disability Details
                cmd.Parameters.AddWithValue("@Disability_Communication", GetValue(learner.Disability_Communication))
                cmd.Parameters.AddWithValue("@Disability_Hearing", GetValue(learner.Disability_Hearing))
                cmd.Parameters.AddWithValue("@Disability_Remembering", GetValue(learner.Disability_Remembering))
                cmd.Parameters.AddWithValue("@Disability_Seeing", GetValue(learner.Disability_Seeing))
                cmd.Parameters.AddWithValue("@Disability_SelfCare", GetValue(learner.Disability_SelfCare))
                cmd.Parameters.AddWithValue("@Disability_Walking", GetValue(learner.Disability_Walking))

                ' Education Details
                cmd.Parameters.AddWithValue("@LastSchoolAttended", GetValue(learner.LastSchoolAttended))
                cmd.Parameters.AddWithValue("@LastSchoolYear", GetValue(learner.LastSchoolYear))

                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Public Sub InitializeHomeAffairsTable() Implements IDatabaseHelper.InitializeHomeAffairsTable
            Using conn As New SqlConnection(_connectionString)
                conn.Open()
                Dim sql As String = "
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HomeAffairsCitizens' AND xtype='U')
                    BEGIN
                        CREATE TABLE HomeAffairsCitizens (
                            NationalID NVARCHAR(13) PRIMARY KEY,
                            FirstName NVARCHAR(100),
                            Surname NVARCHAR(100),
                            DateOfBirth DATE,
                            IsDeceased BIT DEFAULT 0,
                            LastUpdated DATETIME DEFAULT GETDATE(),
                            VerificationSource NVARCHAR(50),
                            RowVersion TIMESTAMP
                        );
                        CREATE INDEX IX_HomeAffairsCitizens_Surname ON HomeAffairsCitizens(Surname);
                    END"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        Public Sub InitializeUserSchema() Implements IDatabaseHelper.InitializeUserSchema
            Using conn As New SqlConnection(_connectionString)
                conn.Open()

                ' 1. Create Users Table
                Dim sqlTable As String = "
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                    BEGIN
                        CREATE TABLE Users (
                            UserID INT IDENTITY(1,1) PRIMARY KEY,
                            IDType NVARCHAR(50),
                            NationalID NVARCHAR(50),
                            Title NVARCHAR(20),
                            FirstName NVARCHAR(100),
                            LastName NVARCHAR(100),
                            Email NVARCHAR(100),
                            Province NVARCHAR(50),
                            UserName NVARCHAR(100) UNIQUE,
                            PasswordHash NVARCHAR(MAX),
                            SecurityQuestion NVARCHAR(200),
                            SecurityAnswer NVARCHAR(MAX),
                            RegistrationDate DATETIME DEFAULT GETDATE(),
                            IsActive BIT DEFAULT 1
                        );
                    END"

                Using cmd As New SqlCommand(sqlTable, conn)
                    cmd.ExecuteNonQuery()
                End Using

                ' 2. Create sp_InsertUser
                Dim sqlProcDrop As String = "IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_InsertUser' AND xtype='P') DROP PROCEDURE sp_InsertUser"
                Using cmd As New SqlCommand(sqlProcDrop, conn)
                    cmd.ExecuteNonQuery()
                End Using

                Dim sqlProcCreate As String = "
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
                    END"

                Using cmd As New SqlCommand(sqlProcCreate, conn)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        Public Sub BatchImportHomeAffairsData(citizens As List(Of HomeAffairsCitizen)) Implements IDatabaseHelper.BatchImportHomeAffairsData
            Using conn As New SqlConnection(_connectionString)
                conn.Open()
                Using transaction As SqlTransaction = conn.BeginTransaction()
                    Try
                        For Each citizen In citizens
                            Dim sql As String = "
                                MERGE HomeAffairsCitizens AS target
                                USING (SELECT @NationalID, @FirstName, @Surname, @DateOfBirth, @IsDeceased, @VerificationSource) AS source (NationalID, FirstName, Surname, DateOfBirth, IsDeceased, VerificationSource)
                                ON (target.NationalID = source.NationalID)
                                WHEN MATCHED THEN
                                    UPDATE SET FirstName = source.FirstName, 
                                               Surname = source.Surname, 
                                               DateOfBirth = source.DateOfBirth,
                                               IsDeceased = source.IsDeceased,
                                               LastUpdated = GETDATE(),
                                               VerificationSource = source.VerificationSource
                                WHEN NOT MATCHED THEN
                                    INSERT (NationalID, FirstName, Surname, DateOfBirth, IsDeceased, VerificationSource)
                                    VALUES (source.NationalID, source.FirstName, source.Surname, source.DateOfBirth, source.IsDeceased, source.VerificationSource);"

                            Using cmd As New SqlCommand(sql, conn, transaction)
                                cmd.Parameters.AddWithValue("@NationalID", citizen.NationalID)
                                cmd.Parameters.AddWithValue("@FirstName", citizen.FirstName)
                                cmd.Parameters.AddWithValue("@Surname", citizen.Surname)
                                cmd.Parameters.AddWithValue("@DateOfBirth", citizen.DateOfBirth)
                                cmd.Parameters.AddWithValue("@IsDeceased", citizen.IsDeceased)
                                cmd.Parameters.AddWithValue("@VerificationSource", "BulkImport")
                                cmd.ExecuteNonQuery()
                            End Using
                        Next
                        transaction.Commit()
                    Catch ex As Exception
                        transaction.Rollback()
                        Throw
                    End Try
                End Using
            End Using
        End Sub

        Public Function BatchInsertLearners(learners As List(Of LearnerModel)) As List(Of BulkInsertError) Implements IDatabaseHelper.BatchInsertLearners
            Dim errors As New List(Of BulkInsertError)()

            Using conn As New SqlConnection(_connectionString)
                conn.Open()
                
                For Each learner In learners
                    Try
                        Using cmd As New SqlCommand("sp_InsertLearner", conn)
                            cmd.CommandType = CommandType.StoredProcedure

                            ' Basic Fields
                            cmd.Parameters.AddWithValue("@NationalID", GetValue(learner.NationalID))
                            cmd.Parameters.AddWithValue("@FirstName", GetValue(learner.FirstName))
                            cmd.Parameters.AddWithValue("@LastName", GetValue(learner.LastName))
                            cmd.Parameters.AddWithValue("@DateOfBirth", learner.DateOfBirth)
                            cmd.Parameters.AddWithValue("@Gender", GetValue(learner.Gender))
                            cmd.Parameters.AddWithValue("@Role", If(String.IsNullOrEmpty(learner.Role), "Learner", learner.Role))
                            cmd.Parameters.AddWithValue("@BiometricHash", GetValue(learner.BiometricHash))
                            cmd.Parameters.AddWithValue("@IsVerified", learner.IsVerified)
                            cmd.Parameters.AddWithValue("@SetaName", If(String.IsNullOrEmpty(learner.SetaName), "BulkImport", learner.SetaName))

                            ' New Personal Info Fields
                            cmd.Parameters.AddWithValue("@Nationality", GetValue(learner.Nationality))
                            cmd.Parameters.AddWithValue("@Title", GetValue(learner.Title))
                            cmd.Parameters.AddWithValue("@MiddleName", GetValue(learner.MiddleName))
                            cmd.Parameters.AddWithValue("@Age", If(learner.Age = 0, DBNull.Value, CObj(learner.Age)))
                            cmd.Parameters.AddWithValue("@EquityCode", GetValue(learner.EquityCode))
                            cmd.Parameters.AddWithValue("@HomeLanguage", GetValue(learner.HomeLanguage))
                            cmd.Parameters.AddWithValue("@PreviousLastName", GetValue(learner.PreviousLastName))
                            cmd.Parameters.AddWithValue("@Municipality", GetValue(learner.Municipality))
                            cmd.Parameters.AddWithValue("@DisabilityStatus", GetValue(learner.DisabilityStatus))
                            cmd.Parameters.AddWithValue("@CitizenStatus", GetValue(learner.CitizenStatus))
                            cmd.Parameters.AddWithValue("@StatsAreaCode", GetValue(learner.StatsAreaCode))
                            cmd.Parameters.AddWithValue("@SocioEconomicStatus", GetValue(learner.SocioEconomicStatus))
                            cmd.Parameters.AddWithValue("@PopiActConsent", learner.PopiActConsent)
                            cmd.Parameters.AddWithValue("@PopiActDate", If(learner.PopiActDate = DateTime.MinValue, DBNull.Value, CObj(learner.PopiActDate)))
                            cmd.Parameters.AddWithValue("@IsResidentialAddressSameAsPostal", learner.IsResidentialAddressSameAsPostal)

                            ' Contact Details
                            cmd.Parameters.AddWithValue("@PhoneNumber", GetValue(learner.PhoneNumber))
                            cmd.Parameters.AddWithValue("@EmailAddress", GetValue(learner.EmailAddress))

                            ' Fill optional fields with nulls
                            cmd.Parameters.AddWithValue("@POBox", DBNull.Value)
                            cmd.Parameters.AddWithValue("@CellphoneNumber", DBNull.Value)
                            cmd.Parameters.AddWithValue("@StreetName", DBNull.Value)
                            cmd.Parameters.AddWithValue("@PostalSuburb", DBNull.Value)
                            cmd.Parameters.AddWithValue("@StreetHouseNo", DBNull.Value)
                            cmd.Parameters.AddWithValue("@PhysicalSuburb", DBNull.Value)
                            cmd.Parameters.AddWithValue("@City", DBNull.Value)
                            cmd.Parameters.AddWithValue("@FaxNumber", DBNull.Value)
                            cmd.Parameters.AddWithValue("@PostalCode", DBNull.Value)
                            cmd.Parameters.AddWithValue("@Province", DBNull.Value)
                            cmd.Parameters.AddWithValue("@UrbanRural", DBNull.Value)

                            cmd.Parameters.AddWithValue("@Disability_Communication", DBNull.Value)
                            cmd.Parameters.AddWithValue("@Disability_Hearing", DBNull.Value)
                            cmd.Parameters.AddWithValue("@Disability_Remembering", DBNull.Value)
                            cmd.Parameters.AddWithValue("@Disability_Seeing", DBNull.Value)
                            cmd.Parameters.AddWithValue("@Disability_SelfCare", DBNull.Value)
                            cmd.Parameters.AddWithValue("@Disability_Walking", DBNull.Value)

                            cmd.Parameters.AddWithValue("@LastSchoolAttended", DBNull.Value)
                            cmd.Parameters.AddWithValue("@LastSchoolYear", DBNull.Value)

                            cmd.ExecuteNonQuery()
                        End Using
                    Catch ex As SqlException
                        If ex.Number = 51000 OrElse ex.Number = 2627 OrElse ex.Number = 2601 Then
                            errors.Add(New BulkInsertError With {.NationalID = learner.NationalID, .Message = "Duplicate Record", .IsDuplicate = True})
                        Else
                            Try
                                Using cmd2 As New SqlCommand("INSERT INTO Learners (NationalID, FirstName, LastName, DateOfBirth, Gender, Role, BiometricHash, IsVerified, SetaName) VALUES (@NationalID, @FirstName, @LastName, @DateOfBirth, @Gender, @Role, @BiometricHash, @IsVerified, @SetaName)", conn)
                                    cmd2.Parameters.AddWithValue("@NationalID", GetValue(learner.NationalID))
                                    cmd2.Parameters.AddWithValue("@FirstName", GetValue(learner.FirstName))
                                    cmd2.Parameters.AddWithValue("@LastName", GetValue(learner.LastName))
                                    cmd2.Parameters.AddWithValue("@DateOfBirth", If(learner.DateOfBirth = DateTime.MinValue, CObj(DBNull.Value), learner.DateOfBirth))
                                    cmd2.Parameters.AddWithValue("@Gender", GetValue(If(String.IsNullOrEmpty(learner.Gender), "Unknown", learner.Gender)))
                                    cmd2.Parameters.AddWithValue("@Role", GetValue(If(String.IsNullOrEmpty(learner.Role), "Learner", learner.Role)))
                                    cmd2.Parameters.AddWithValue("@BiometricHash", GetValue(learner.BiometricHash))
                                    cmd2.Parameters.AddWithValue("@IsVerified", learner.IsVerified)
                                    cmd2.Parameters.AddWithValue("@SetaName", GetValue(If(String.IsNullOrEmpty(learner.SetaName), "BulkImport", learner.SetaName)))
                                    cmd2.ExecuteNonQuery()
                                End Using
                            Catch ex2 As SqlException
                                If ex2.Number = 2627 OrElse ex2.Number = 2601 Then
                                    errors.Add(New BulkInsertError With {.NationalID = learner.NationalID, .Message = "Duplicate Record", .IsDuplicate = True})
                                Else
                                    errors.Add(New BulkInsertError With {.NationalID = learner.NationalID, .Message = $"Database Error: {ex2.Message}", .IsDuplicate = False})
                                End If
                            End Try
                        End If
                    Catch ex As Exception
                        errors.Add(New BulkInsertError With {.NationalID = learner.NationalID, .Message = $"System Error: {ex.Message}", .IsDuplicate = False})
                    End Try
                Next
            End Using
            Return errors
        End Function

        Public Class BulkInsertError
            Public Property NationalID As String
            Public Property Message As String
            Public Property IsDuplicate As Boolean
        End Class

        Public Sub InsertUser(user As UserModel) Implements IDatabaseHelper.InsertUser
            Using conn As New SqlConnection(_connectionString)
                Dim cmd As New SqlCommand("sp_InsertUser", conn)
                cmd.CommandType = CommandType.StoredProcedure

                cmd.Parameters.AddWithValue("@IDType", GetValue(user.IDType))
                cmd.Parameters.AddWithValue("@NationalID", GetValue(user.NationalID))
                cmd.Parameters.AddWithValue("@Title", GetValue(user.Title))
                cmd.Parameters.AddWithValue("@FirstName", GetValue(user.FirstName))
                cmd.Parameters.AddWithValue("@LastName", GetValue(user.LastName))
                cmd.Parameters.AddWithValue("@Email", GetValue(user.Email))
                cmd.Parameters.AddWithValue("@Province", GetValue(user.Province))
                cmd.Parameters.AddWithValue("@UserName", GetValue(user.UserName))
                cmd.Parameters.AddWithValue("@PasswordHash", GetValue(user.PasswordHash))
                cmd.Parameters.AddWithValue("@SecurityQuestion", GetValue(user.SecurityQuestion))
                cmd.Parameters.AddWithValue("@SecurityAnswer", GetValue(user.SecurityAnswer))

                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Public Function GetAllLearners() As List(Of LearnerModel) Implements IDatabaseHelper.GetAllLearners
            Dim learners As New List(Of LearnerModel)()
            Using conn As New SqlConnection(_connectionString)
                Dim query As String = "SELECT * FROM Learners"
                Dim cmd As New SqlCommand(query, conn)

                conn.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        learners.Add(MapReaderToLearner(reader))
                    End While
                End Using
            End Using
            Return learners
        End Function

        Public Function GetLearnerByNationalID(nationalID As String) As LearnerModel Implements IDatabaseHelper.GetLearnerByNationalID
            Using conn As New SqlConnection(_connectionString)
                Dim query As String = "SELECT TOP 1 * FROM Learners WHERE NationalID = @NationalID"
                Dim cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@NationalID", nationalID)

                conn.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        Return MapReaderToLearner(reader)
                    End If
                End Using
            End Using
            Return Nothing
        End Function

        Public Function GetHomeAffairsCitizen(nationalID As String) As HomeAffairsCitizen Implements IDatabaseHelper.GetHomeAffairsCitizen
            Using conn As New SqlConnection(_connectionString)
                Dim sql As String = "SELECT * FROM HomeAffairsCitizens WHERE NationalID = @NationalID"
                Dim cmd As New SqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@NationalID", nationalID)

                Try
                    conn.Open()
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return New HomeAffairsCitizen With {
                                .NationalID = reader("NationalID").ToString(),
                                .FirstName = reader("FirstName").ToString(),
                                .Surname = reader("Surname").ToString(),
                                .DateOfBirth = Convert.ToDateTime(reader("DateOfBirth")),
                                .IsDeceased = Convert.ToBoolean(reader("IsDeceased")),
                                .VerificationSource = If(reader("VerificationSource") Is DBNull.Value, "Unknown", reader("VerificationSource").ToString())
                            }
                        End If
                    End Using
                Catch ex As SqlException
                    ' Table might not exist if import hasn't run.
                    Return Nothing
                End Try
            End Using
            Return Nothing
        End Function

        Public Function GetLearnerValidationResults() As List(Of LearnerValidationResult) Implements IDatabaseHelper.GetLearnerValidationResults
            Dim results As New List(Of LearnerValidationResult)()
            Using conn As New SqlConnection(_connectionString)
                Dim query As String = "
                    SELECT 
                        L.NationalID, 
                        L.FirstName, 
                        L.LastName, 
                        H.FirstName AS HA_FirstName, 
                        H.Surname AS HA_Surname, 
                        ISNULL(H.IsDeceased, 0) AS IsDeceased,
                        CASE WHEN H.NationalID IS NOT NULL THEN 1 ELSE 0 END AS IsFound
                    FROM Learners L
                    LEFT JOIN HomeAffairsCitizens H ON L.NationalID = H.NationalID"

                Dim cmd As New SqlCommand(query, conn)
                cmd.CommandTimeout = 300 ' Increase timeout for large datasets

                Try
                    conn.Open()
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            results.Add(New LearnerValidationResult With {
                                .NationalID = reader("NationalID").ToString(),
                                .FirstName = reader("FirstName").ToString(),
                                .LastName = reader("LastName").ToString(),
                                .HomeAffairsFirstName = If(reader("HA_FirstName") Is DBNull.Value, Nothing, reader("HA_FirstName").ToString()),
                                .HomeAffairsSurname = If(reader("HA_Surname") Is DBNull.Value, Nothing, reader("HA_Surname").ToString()),
                                .IsDeceased = Convert.ToBoolean(reader("IsDeceased")),
                                .IsFoundInHomeAffairs = Convert.ToInt32(reader("IsFound")) = 1
                            })
                        End While
                    End Using
                Catch ex As SqlException
                    ' If HomeAffairsCitizens table doesn't exist, return basic list with IsFound=false
                    Return GetAllLearners().ConvertAll(Function(l) New LearnerValidationResult With {
                        .NationalID = l.NationalID,
                        .FirstName = l.FirstName,
                        .LastName = l.LastName,
                        .IsFoundInHomeAffairs = False
                    })
                End Try
            End Using
            Return results
        End Function

        Public Sub InitializeUserActivitySchema() Implements IDatabaseHelper.InitializeUserActivitySchema
            Using conn As New SqlConnection(_connectionString)
                conn.Open()
                Dim sql As String = "
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserActivityLogs' AND xtype='U')
                    BEGIN
                        CREATE TABLE UserActivityLogs (
                            LogID INT IDENTITY(1,1) PRIMARY KEY,
                            UserName NVARCHAR(100),
                            ActivityType NVARCHAR(50),
                            ActivityDate DATETIME DEFAULT GETDATE(),
                            IPAddress NVARCHAR(50),
                            Details NVARCHAR(MAX)
                        );
                        CREATE INDEX IX_UserActivityLogs_UserName ON UserActivityLogs(UserName);
                        CREATE INDEX IX_UserActivityLogs_Date ON UserActivityLogs(ActivityDate);
                    END"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        Public Sub LogUserActivity(email As String, activityType As String, ipAddress As String, details As String) Implements IDatabaseHelper.LogUserActivity
            Try
                Using conn As New SqlConnection(_connectionString)
                    Dim sql As String = "INSERT INTO UserActivityLogs (UserName, ActivityType, IPAddress, Details) VALUES (@UserName, @ActivityType, @IPAddress, @Details)"
                    Using cmd As New SqlCommand(sql, conn)
                        cmd.Parameters.AddWithValue("@UserName", If(email, CObj(DBNull.Value)))
                        cmd.Parameters.AddWithValue("@ActivityType", If(activityType, CObj(DBNull.Value)))
                        cmd.Parameters.AddWithValue("@IPAddress", If(ipAddress, CObj(DBNull.Value)))
                        cmd.Parameters.AddWithValue("@Details", If(details, CObj(DBNull.Value)))

                        conn.Open()
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
            Catch ex As Exception
                ' Silently fail logging to not disrupt user flow
            End Try
        End Sub

        Public Function GetUserActivityLogs() As List(Of UserActivityLog) Implements IDatabaseHelper.GetUserActivityLogs
            Dim logs As New List(Of UserActivityLog)()
            Using conn As New SqlConnection(_connectionString)
                ' Get latest 100 logs
                Dim sql As String = "SELECT TOP 100 * FROM UserActivityLogs ORDER BY ActivityDate DESC"
                Using cmd As New SqlCommand(sql, conn)
                    Try
                        conn.Open()
                        Using reader As SqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                logs.Add(New UserActivityLog With {
                                    .LogID = Convert.ToInt32(reader("LogID")),
                                    .UserName = If(reader("UserName") Is DBNull.Value, "", reader("UserName").ToString()),
                                    .ActivityType = If(reader("ActivityType") Is DBNull.Value, "", reader("ActivityType").ToString()),
                                    .ActivityDate = Convert.ToDateTime(reader("ActivityDate")),
                                    .IPAddress = If(reader("IPAddress") Is DBNull.Value, "", reader("IPAddress").ToString()),
                                    .Details = If(reader("Details") Is DBNull.Value, "", reader("Details").ToString())
                                })
                            End While
                        End Using
                    Catch ex As SqlException
                        ' Table might not exist yet
                    End Try
                End Using
            End Using
            Return logs
        End Function

        Private Function MapReaderToLearner(reader As SqlDataReader) As LearnerModel
            Return New LearnerModel With {
                .LearnerID = Convert.ToInt32(reader("LearnerID")),
                .NationalID = reader("NationalID").ToString(),
                .FirstName = reader("FirstName").ToString(),
                .LastName = reader("LastName").ToString(),
                .DateOfBirth = Convert.ToDateTime(reader("DateOfBirth")),
                .Gender = If(reader("Gender") Is DBNull.Value, Nothing, reader("Gender").ToString()),
                .Role = If(reader("Role") Is DBNull.Value, "Learner", reader("Role").ToString()),
                .BiometricHash = If(reader("BiometricHash") Is DBNull.Value, Nothing, reader("BiometricHash").ToString()),
                .IsVerified = Convert.ToBoolean(reader("IsVerified")),
                .SetaName = If(HasColumn(reader, "SetaName"), If(reader("SetaName") Is DBNull.Value, "Unknown", reader("SetaName").ToString()), "Unknown")
            }
        End Function

        Private Function HasColumn(reader As SqlDataReader, columnName As String) As Boolean
            For i As Integer = 0 To reader.FieldCount - 1
                If reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Private Function GetValue(value As String) As Object
            Return If(String.IsNullOrEmpty(value), DBNull.Value, CObj(value))
        End Function
    End Class
End Namespace
