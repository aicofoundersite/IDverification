Imports System.Collections.Concurrent
Imports CrossSetaLogic.Models

Namespace Services
    Public Class ValidationProgressService
        Implements IValidationProgressService

        Private Shared ReadOnly _jobs As New ConcurrentDictionary(Of String, ValidationProgress)()

        Public Sub StartValidation(jobId As String) Implements IValidationProgressService.StartValidation
            _jobs(jobId) = New ValidationProgress With {
                .Total = 0,
                .Processed = 0,
                .Status = "Starting...",
                .IsComplete = False
            }
        End Sub

        Public Sub UpdateProgress(jobId As String, processed As Integer, total As Integer, status As String) Implements IValidationProgressService.UpdateProgress
            Dim progress As ValidationProgress = Nothing
            If _jobs.TryGetValue(jobId, progress) Then
                progress.Processed = processed
                progress.Total = total
                progress.Status = status
            End If
        End Sub

        Public Sub CompleteValidation(jobId As String, result As DatabaseValidationResult) Implements IValidationProgressService.CompleteValidation
            Dim progress As ValidationProgress = Nothing
            If _jobs.TryGetValue(jobId, progress) Then
                progress.IsComplete = True
                progress.Status = "Completed"
                progress.Result = result
                progress.Processed = progress.Total ' Ensure 100%
            End If
        End Sub

        Public Function GetProgress(jobId As String) As ValidationProgress Implements IValidationProgressService.GetProgress
            Dim progress As ValidationProgress = Nothing
            If _jobs.TryGetValue(jobId, progress) Then
                Return progress
            End If
            Return Nothing
        End Function
    End Class
End Namespace
