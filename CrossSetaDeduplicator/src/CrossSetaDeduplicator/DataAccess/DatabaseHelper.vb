Imports System.Data.SqlClient
Imports CrossSetaDeduplicator.Models

Public Class DatabaseHelper
    Private _connectionString As String = "Server=localhost;Database=CrossSetaDB;Trusted_Connection=True;"

    Public Sub New(connectionString As String)
        If Not String.IsNullOrEmpty(connectionString) Then
            _connectionString = connectionString
        End If
    End Sub

    Public Sub InsertLearner(learner As LearnerModel)
        Using conn As New SqlConnection(_connectionString)
            Dim cmd As New SqlCommand("INSERT INTO Learners (NationalID, FirstName, LastName, DateOfBirth, Gender, BiometricHash, IsVerified) VALUES (@NationalID, @FirstName, @LastName, @DateOfBirth, @Gender, @BiometricHash, @IsVerified)", conn)
            cmd.Parameters.AddWithValue("@NationalID", learner.NationalID)
            cmd.Parameters.AddWithValue("@FirstName", learner.FirstName)
            cmd.Parameters.AddWithValue("@LastName", learner.LastName)
            cmd.Parameters.AddWithValue("@DateOfBirth", learner.DateOfBirth)
            cmd.Parameters.AddWithValue("@Gender", If(learner.Gender, DBNull.Value))
            cmd.Parameters.AddWithValue("@BiometricHash", If(learner.BiometricHash, DBNull.Value))
            cmd.Parameters.AddWithValue("@IsVerified", learner.IsVerified)

            conn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Function FindPotentialDuplicates(nationalId As String, firstName As String, lastName As String) As List(Of LearnerModel)
        Dim potentialDuplicates As New List(Of LearnerModel)

        Using conn As New SqlConnection(_connectionString)
            ' Basic query to fetch records that might match.
            ' In a real scenario, we might use Full Text Search or fetch a broader set for in-memory fuzzy matching if dataset is small.
            ' For this prototype, we fetch by NationalID OR (FirstName AND LastName) to cover both exact and potential name matches.
            Dim query As String = "SELECT * FROM Learners WHERE NationalID = @NationalID OR (FirstName = @FirstName AND LastName = @LastName)"
            Dim cmd As New SqlCommand(query, conn)
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
                    learner.BiometricHash = If(IsDBNull(reader("BiometricHash")), Nothing, reader("BiometricHash").ToString())
                    learner.IsVerified = Convert.ToBoolean(reader("IsVerified"))
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
                    learner.BiometricHash = If(IsDBNull(reader("BiometricHash")), Nothing, reader("BiometricHash").ToString())
                    learner.IsVerified = Convert.ToBoolean(reader("IsVerified"))
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
End Class
