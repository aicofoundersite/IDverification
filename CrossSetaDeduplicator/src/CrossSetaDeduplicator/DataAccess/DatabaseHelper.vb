Imports System.Data.SqlClient
Imports CrossSetaDeduplicator.Models

Public Class DatabaseHelper
    Private _connectionString As String = "Server=localhost;Database=CrossSetaDB;Trusted_Connection=True;"

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

        ' Priority 4: Default Localhost (Dev - Windows Auth)
        ' _connectionString is already set to default
    End Sub

    Public Sub InsertLearner(learner As LearnerModel)
        Using conn As New SqlConnection(_connectionString)
            Dim cmd As New SqlCommand("sp_InsertLearner", conn)
            cmd.CommandType = CommandType.StoredProcedure
            
            ' Basic Fields
            cmd.Parameters.AddWithValue("@NationalID", learner.NationalID)
            cmd.Parameters.AddWithValue("@FirstName", learner.FirstName)
            cmd.Parameters.AddWithValue("@LastName", learner.LastName)
            cmd.Parameters.AddWithValue("@DateOfBirth", learner.DateOfBirth)
            cmd.Parameters.AddWithValue("@Gender", If(String.IsNullOrEmpty(learner.Gender), DBNull.Value, learner.Gender))
            cmd.Parameters.AddWithValue("@Role", If(String.IsNullOrEmpty(learner.Role), "Learner", learner.Role))
            cmd.Parameters.AddWithValue("@BiometricHash", If(String.IsNullOrEmpty(learner.BiometricHash), DBNull.Value, learner.BiometricHash))
            cmd.Parameters.AddWithValue("@IsVerified", learner.IsVerified)
            cmd.Parameters.AddWithValue("@SetaName", If(String.IsNullOrEmpty(learner.SetaName), "W&RSETA", learner.SetaName))

            ' New Personal Info Fields
            cmd.Parameters.AddWithValue("@Nationality", If(String.IsNullOrEmpty(learner.Nationality), DBNull.Value, learner.Nationality))
            cmd.Parameters.AddWithValue("@Title", If(String.IsNullOrEmpty(learner.Title), DBNull.Value, learner.Title))
            cmd.Parameters.AddWithValue("@MiddleName", If(String.IsNullOrEmpty(learner.MiddleName), DBNull.Value, learner.MiddleName))
            cmd.Parameters.AddWithValue("@Age", If(learner.Age = 0, DBNull.Value, learner.Age))
            cmd.Parameters.AddWithValue("@EquityCode", If(String.IsNullOrEmpty(learner.EquityCode), DBNull.Value, learner.EquityCode))
            cmd.Parameters.AddWithValue("@HomeLanguage", If(String.IsNullOrEmpty(learner.HomeLanguage), DBNull.Value, learner.HomeLanguage))
            cmd.Parameters.AddWithValue("@PreviousLastName", If(String.IsNullOrEmpty(learner.PreviousLastName), DBNull.Value, learner.PreviousLastName))
            cmd.Parameters.AddWithValue("@Municipality", If(String.IsNullOrEmpty(learner.Municipality), DBNull.Value, learner.Municipality))
            cmd.Parameters.AddWithValue("@DisabilityStatus", If(String.IsNullOrEmpty(learner.DisabilityStatus), DBNull.Value, learner.DisabilityStatus))
            cmd.Parameters.AddWithValue("@CitizenStatus", If(String.IsNullOrEmpty(learner.CitizenStatus), DBNull.Value, learner.CitizenStatus))
            cmd.Parameters.AddWithValue("@StatsAreaCode", If(String.IsNullOrEmpty(learner.StatsAreaCode), DBNull.Value, learner.StatsAreaCode))
            cmd.Parameters.AddWithValue("@SocioEconomicStatus", If(String.IsNullOrEmpty(learner.SocioEconomicStatus), DBNull.Value, learner.SocioEconomicStatus))
            cmd.Parameters.AddWithValue("@PopiActConsent", learner.PopiActConsent)
            cmd.Parameters.AddWithValue("@PopiActDate", If(learner.PopiActDate = DateTime.MinValue, DBNull.Value, learner.PopiActDate))

            ' Contact Details
            cmd.Parameters.AddWithValue("@PhoneNumber", If(String.IsNullOrEmpty(learner.PhoneNumber), DBNull.Value, learner.PhoneNumber))
            cmd.Parameters.AddWithValue("@POBox", If(String.IsNullOrEmpty(learner.POBox), DBNull.Value, learner.POBox))
            cmd.Parameters.AddWithValue("@CellphoneNumber", If(String.IsNullOrEmpty(learner.CellphoneNumber), DBNull.Value, learner.CellphoneNumber))
            cmd.Parameters.AddWithValue("@StreetName", If(String.IsNullOrEmpty(learner.StreetName), DBNull.Value, learner.StreetName))
            cmd.Parameters.AddWithValue("@PostalSuburb", If(String.IsNullOrEmpty(learner.PostalSuburb), DBNull.Value, learner.PostalSuburb))
            cmd.Parameters.AddWithValue("@StreetHouseNo", If(String.IsNullOrEmpty(learner.StreetHouseNo), DBNull.Value, learner.StreetHouseNo))
            cmd.Parameters.AddWithValue("@PhysicalSuburb", If(String.IsNullOrEmpty(learner.PhysicalSuburb), DBNull.Value, learner.PhysicalSuburb))
            cmd.Parameters.AddWithValue("@City", If(String.IsNullOrEmpty(learner.City), DBNull.Value, learner.City))
            cmd.Parameters.AddWithValue("@FaxNumber", If(String.IsNullOrEmpty(learner.FaxNumber), DBNull.Value, learner.FaxNumber))
            cmd.Parameters.AddWithValue("@PostalCode", If(String.IsNullOrEmpty(learner.PostalCode), DBNull.Value, learner.PostalCode))
            cmd.Parameters.AddWithValue("@EmailAddress", If(String.IsNullOrEmpty(learner.EmailAddress), DBNull.Value, learner.EmailAddress))
            cmd.Parameters.AddWithValue("@Province", If(String.IsNullOrEmpty(learner.Province), DBNull.Value, learner.Province))
            cmd.Parameters.AddWithValue("@UrbanRural", If(String.IsNullOrEmpty(learner.UrbanRural), DBNull.Value, learner.UrbanRural))
            cmd.Parameters.AddWithValue("@IsResidentialAddressSameAsPostal", learner.IsResidentialAddressSameAsPostal)

            ' Disability Details
            cmd.Parameters.AddWithValue("@Disability_Communication", If(String.IsNullOrEmpty(learner.Disability_Communication), DBNull.Value, learner.Disability_Communication))
            cmd.Parameters.AddWithValue("@Disability_Hearing", If(String.IsNullOrEmpty(learner.Disability_Hearing), DBNull.Value, learner.Disability_Hearing))
            cmd.Parameters.AddWithValue("@Disability_Remembering", If(String.IsNullOrEmpty(learner.Disability_Remembering), DBNull.Value, learner.Disability_Remembering))
            cmd.Parameters.AddWithValue("@Disability_Seeing", If(String.IsNullOrEmpty(learner.Disability_Seeing), DBNull.Value, learner.Disability_Seeing))
            cmd.Parameters.AddWithValue("@Disability_SelfCare", If(String.IsNullOrEmpty(learner.Disability_SelfCare), DBNull.Value, learner.Disability_SelfCare))
            cmd.Parameters.AddWithValue("@Disability_Walking", If(String.IsNullOrEmpty(learner.Disability_Walking), DBNull.Value, learner.Disability_Walking))

            ' Education Details
            cmd.Parameters.AddWithValue("@LastSchoolAttended", If(String.IsNullOrEmpty(learner.LastSchoolAttended), DBNull.Value, learner.LastSchoolAttended))
            cmd.Parameters.AddWithValue("@LastSchoolYear", If(String.IsNullOrEmpty(learner.LastSchoolYear), DBNull.Value, learner.LastSchoolYear))

            conn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub InsertUser(user As UserModel)
        Using conn As New SqlConnection(_connectionString)
            Dim cmd As New SqlCommand("sp_InsertUser", conn)
            cmd.CommandType = CommandType.StoredProcedure
            
            cmd.Parameters.AddWithValue("@IDType", If(String.IsNullOrEmpty(user.IDType), DBNull.Value, user.IDType))
            cmd.Parameters.AddWithValue("@NationalID", If(String.IsNullOrEmpty(user.NationalID), DBNull.Value, user.NationalID))
            cmd.Parameters.AddWithValue("@Title", If(String.IsNullOrEmpty(user.Title), DBNull.Value, user.Title))
            cmd.Parameters.AddWithValue("@FirstName", If(String.IsNullOrEmpty(user.FirstName), DBNull.Value, user.FirstName))
            cmd.Parameters.AddWithValue("@LastName", If(String.IsNullOrEmpty(user.LastName), DBNull.Value, user.LastName))
            cmd.Parameters.AddWithValue("@Email", If(String.IsNullOrEmpty(user.Email), DBNull.Value, user.Email))
            cmd.Parameters.AddWithValue("@Province", If(String.IsNullOrEmpty(user.Province), DBNull.Value, user.Province))
            cmd.Parameters.AddWithValue("@UserName", If(String.IsNullOrEmpty(user.UserName), DBNull.Value, user.UserName))
            cmd.Parameters.AddWithValue("@PasswordHash", If(String.IsNullOrEmpty(user.PasswordHash), DBNull.Value, user.PasswordHash))
            cmd.Parameters.AddWithValue("@SecurityQuestion", If(String.IsNullOrEmpty(user.SecurityQuestion), DBNull.Value, user.SecurityQuestion))
            cmd.Parameters.AddWithValue("@SecurityAnswer", If(String.IsNullOrEmpty(user.SecurityAnswer), DBNull.Value, user.SecurityAnswer))

            conn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub


    Public Function FindPotentialDuplicates(nationalId As String, firstName As String, lastName As String) As List(Of LearnerModel)
        Dim potentialDuplicates As New List(Of LearnerModel)

        Using conn As New SqlConnection(_connectionString)
            Dim cmd As New SqlCommand("sp_FindPotentialDuplicates", conn)
            cmd.CommandType = CommandType.StoredProcedure

            cmd.Parameters.AddWithValue("@NationalID", nationalId)
            cmd.Parameters.AddWithValue("@FirstName", firstName)
            cmd.Parameters.AddWithValue("@LastName", lastName)

            conn.Open()
            Using reader As SqlDataReader = cmd.ExecuteReader()
                While reader.Read()
                    Dim learner As New LearnerModel()
                    learner.LearnerID = Convert.ToInt32(reader("LearnerID"))
                    learner.NationalID = reader("NationalID").ToString()
                    learner.FirstName = reader("FirstName").ToString()
                    learner.LastName = reader("LastName").ToString()
                    learner.DateOfBirth = Convert.ToDateTime(reader("DateOfBirth"))
                    learner.Gender = If(IsDBNull(reader("Gender")), Nothing, reader("Gender").ToString())
                    learner.Role = If(IsDBNull(reader("Role")), "Learner", reader("Role").ToString())
                    learner.BiometricHash = If(IsDBNull(reader("BiometricHash")), Nothing, reader("BiometricHash").ToString())
                    learner.IsVerified = Convert.ToBoolean(reader("IsVerified"))
                    ' Safe check for SetaName in case DB wasn't updated yet or column is missing in older versions
                    If reader.GetSchemaTable().Select("ColumnName = 'SetaName'").Length > 0 Then
                         learner.SetaName = If(IsDBNull(reader("SetaName")), "Unknown", reader("SetaName").ToString())
                    End If
                    potentialDuplicates.Add(learner)
                End While
            End Using
        End Using

        Return potentialDuplicates
    End Function
    
    Public Function GetAllLearners() As List(Of LearnerModel)
        Dim learners As New List(Of LearnerModel)

        Using conn As New SqlConnection(_connectionString)
            Dim query As String = "SELECT * FROM Learners"
            Dim cmd As New SqlCommand(query, conn)

            conn.Open()
            Using reader As SqlDataReader = cmd.ExecuteReader()
                While reader.Read()
                    Dim learner As New LearnerModel()
                    learner.LearnerID = Convert.ToInt32(reader("LearnerID"))
                    learner.NationalID = reader("NationalID").ToString()
                    learner.FirstName = reader("FirstName").ToString()
                    learner.LastName = reader("LastName").ToString()
                    learner.DateOfBirth = Convert.ToDateTime(reader("DateOfBirth"))
                    learner.Gender = If(IsDBNull(reader("Gender")), Nothing, reader("Gender").ToString())
                    learner.Role = If(IsDBNull(reader("Role")), "Learner", reader("Role").ToString())
                    learner.BiometricHash = If(IsDBNull(reader("BiometricHash")), Nothing, reader("BiometricHash").ToString())
                    learner.IsVerified = Convert.ToBoolean(reader("IsVerified"))
                    ' Safe check for SetaName
                    If reader.GetSchemaTable().Select("ColumnName = 'SetaName'").Length > 0 Then
                        learner.SetaName = If(IsDBNull(reader("SetaName")), "Unknown", reader("SetaName").ToString())
                    End If
                    learners.Add(learner)
                End While
            End Using
        End Using

        Return learners
    End Function

    Public Sub LogDuplicateCheck(testedId As String, matchedId As Integer?, score As Integer, matchType As String)
        Using conn As New SqlConnection(_connectionString)
            Dim cmd As New SqlCommand("INSERT INTO DuplicateAuditLog (TestedNationalID, MatchedLearnerID, MatchScore, MatchType) VALUES (@TestedNationalID, @MatchedLearnerID, @MatchScore, @MatchType)", conn)
            cmd.Parameters.AddWithValue("@TestedNationalID", testedId)
            If matchedId.HasValue Then
                cmd.Parameters.AddWithValue("@MatchedLearnerID", matchedId.Value)
            Else
                cmd.Parameters.AddWithValue("@MatchedLearnerID", DBNull.Value)
            End If
            cmd.Parameters.AddWithValue("@MatchScore", score)
            cmd.Parameters.AddWithValue("@MatchType", matchType)

            conn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub LogExternalVerification(nationalId As String, source As String, status As String, details As String, startTime As DateTime, Optional performedBy As String = "System")
        Using conn As New SqlConnection(_connectionString)
            Dim cmd As New SqlCommand("INSERT INTO ExternalVerificationLog (NationalID, VerificationSource, Status, Details, RequestTimestamp, ResponseTimestamp, PerformedBy) VALUES (@NationalID, @VerificationSource, @Status, @Details, @RequestTimestamp, @ResponseTimestamp, @PerformedBy)", conn)
            
            cmd.Parameters.AddWithValue("@NationalID", nationalId)
            cmd.Parameters.AddWithValue("@VerificationSource", source)
            cmd.Parameters.AddWithValue("@Status", status)
            cmd.Parameters.AddWithValue("@Details", details)
            cmd.Parameters.AddWithValue("@RequestTimestamp", startTime)
            cmd.Parameters.AddWithValue("@ResponseTimestamp", DateTime.Now)
            cmd.Parameters.AddWithValue("@PerformedBy", performedBy)

            conn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub
End Class
