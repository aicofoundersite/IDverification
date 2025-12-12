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

        ' Priority 2: Environment Variable (Production/Docker)
        Dim envConn As String = Environment.GetEnvironmentVariable("CROSS_SETA_DB_CONNECTION")
        If Not String.IsNullOrEmpty(envConn) Then
            _connectionString = envConn
            Return
        End If

        ' Priority 3: Default Localhost (Dev)
        ' _connectionString is already set to default
    End Sub

    Public Sub InsertLearner(learner As LearnerModel)
        Using conn As New SqlConnection(_connectionString)
            Dim cmd As New SqlCommand("sp_InsertLearner", conn)
            cmd.CommandType = CommandType.StoredProcedure
            
            cmd.Parameters.AddWithValue("@NationalID", learner.NationalID)
            cmd.Parameters.AddWithValue("@FirstName", learner.FirstName)
            cmd.Parameters.AddWithValue("@LastName", learner.LastName)
            cmd.Parameters.AddWithValue("@DateOfBirth", learner.DateOfBirth)
            cmd.Parameters.AddWithValue("@Gender", If(learner.Gender, DBNull.Value))
            cmd.Parameters.AddWithValue("@Role", If(learner.Role, "Learner"))
            cmd.Parameters.AddWithValue("@BiometricHash", If(learner.BiometricHash, DBNull.Value))
            cmd.Parameters.AddWithValue("@IsVerified", learner.IsVerified)
            cmd.Parameters.AddWithValue("@SetaName", If(String.IsNullOrEmpty(learner.SetaName), "W&RSETA", learner.SetaName))

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
