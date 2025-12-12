Imports CrossSetaDeduplicator.Models
Imports CrossSetaDeduplicator.DataAccess

Namespace CrossSetaDeduplicator.Services
    ''' <summary>
    ''' Business Logic Layer for Learner Management.
    ''' Enforces strict N-Tier architecture by acting as the intermediary between UI and Data Access.
    ''' </summary>
    Public Class LearnerService
        Private _dbHelper As DatabaseHelper

        Public Sub New()
            _dbHelper = New DatabaseHelper(Nothing)
        End Sub

        ''' <summary>
        ''' Registers a new learner after validating business rules.
        ''' </summary>
        Public Sub RegisterLearner(learner As LearnerModel)
            ' Business Rule 1: Validate Mandatory Fields
            If String.IsNullOrWhiteSpace(learner.NationalID) OrElse String.IsNullOrWhiteSpace(learner.FirstName) OrElse String.IsNullOrWhiteSpace(learner.LastName) Then
                Throw New ArgumentException("All mandatory fields (ID, Name, Surname) must be provided.")
            End If

            ' Business Rule 2: Ensure ID is verified (implied by workflow, but good to check)
            If Not learner.IsVerified Then
                Throw New InvalidOperationException("Cannot register a learner without verifying their identity first.")
            End If

            ' Proceed to Data Access
            _dbHelper.InsertLearner(learner)
        End Sub

        ''' <summary>
        ''' Checks for duplicates using the Deduplication Logic.
        ''' </summary>
        Public Function CheckForDuplicates(learner As LearnerModel) As DuplicateResult
            Dim dedupService As New DeduplicationService()
            Return dedupService.CheckForDuplicates(learner)
        End Function
    End Class
End Namespace
