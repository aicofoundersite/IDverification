Imports CrossSetaDeduplicator.DataAccess
Imports CrossSetaDeduplicator.Models

Public Class DemoMode
    Private _dbHelper As DatabaseHelper

    Public Sub New()
        _dbHelper = New DatabaseHelper(Nothing)
    End Sub

    ''' <summary>
    ''' Simulates a KYC check. Returns a realistic message or "Success".
    ''' </summary>
    Public Function SimulateKYC(nationalId As String) As String
        ' Mock logic for demo purposes
        If String.IsNullOrWhiteSpace(nationalId) Then Return "Invalid Input"
        
        ' Simulate processing delay if needed, but keeping it synchronous for UI responsiveness in simple demo
        
        ' Specific ID for "Expired" case
        If nationalId.EndsWith("999") Then
            Return "Document Expired"
        End If

        ' Specific ID for "Not Found" case
        If nationalId.EndsWith("000") Then
            Return "ID Not Found in Home Affairs"
        End If

        ' Standard valid format check (South African ID: 13 digits)
        If nationalId.Length <> 13 OrElse Not IsNumeric(nationalId) Then
             Return "Invalid ID Format"
        End If

        Return "Verification Successful"
    End Function

    ''' <summary>
    ''' Pre-populates the database with 50 records including duplicates.
    ''' </summary>
    Public Sub SeedDatabase()
        Dim learners As New List(Of LearnerModel)()

        ' 1. Create base unique records
        For i As Integer = 1 To 45
            learners.Add(New LearnerModel() With {
                .NationalID = $"800101{i.ToString("D4")}08{i Mod 2}", ' e.g., 8001010001081
                .FirstName = $"Learner{i}",
                .LastName = $"TestLast{i}",
                .DateOfBirth = New DateTime(1980, 1, 1).AddDays(i),
                .Gender = If(i Mod 2 = 0, "Male", "Female"),
                .IsVerified = True
            })
        Next

        ' 2. Add specific duplicates (Exact Match candidates)
        ' Duplicate of Learner1
        learners.Add(New LearnerModel() With {
            .NationalID = "8001010001081",
            .FirstName = "Learner1",
            .LastName = "TestLast1",
            .DateOfBirth = New DateTime(1980, 1, 2),
            .Gender = "Male",
            .IsVerified = True
        })

        ' 3. Add specific Fuzzy Match candidates
        ' Similar to Learner2 (Learner2 vs LearnerTwo)
        learners.Add(New LearnerModel() With {
            .NationalID = "9901010002081", ' Different ID
            .FirstName = "LearnerTwo",
            .LastName = "TestLast2", ' Same Last Name
            .DateOfBirth = New DateTime(1980, 1, 3),
            .Gender = "Male",
            .IsVerified = True
        })

         ' 4. Add the "Live Demo" narrative target
        learners.Add(New LearnerModel() With {
            .NationalID = "9505055000081",
            .FirstName = "Thabo",
            .LastName = "Molefe",
            .DateOfBirth = New DateTime(1995, 5, 5),
            .Gender = "Male",
            .IsVerified = True
        })

        Try
            For Each l In learners
                ' We use a try-catch per insert to avoid crashing on existing keys if run multiple times
                Try
                    _dbHelper.InsertLearner(l)
                Catch ex As Exception
                    ' Ignore duplicate key errors during seeding
                End Try
            Next
        Catch ex As Exception
            Throw New Exception("Error seeding database: " & ex.Message)
        End Try
    End Sub
End Class
