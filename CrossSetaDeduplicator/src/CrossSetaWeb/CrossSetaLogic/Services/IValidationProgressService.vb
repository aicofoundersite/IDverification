Imports CrossSetaLogic.Models

Namespace Services
    Public Class ValidationProgress
        Public Property Total As Integer
        Public Property Processed As Integer
        Public ReadOnly Property Percentage As Integer
            Get
                Return If(Total = 0, 0, CInt((CDbl(Processed) / Total) * 100))
            End Get
        End Property
        Public Property Status As String
        Public Property IsComplete As Boolean
        Public Property Result As DatabaseValidationResult
    End Class

    Public Interface IValidationProgressService
        Sub StartValidation(jobId As String)
        Sub UpdateProgress(jobId As String, processed As Integer, total As Integer, status As String)
        Sub CompleteValidation(jobId As String, result As DatabaseValidationResult)
        Function GetProgress(jobId As String) As ValidationProgress
    End Interface
End Namespace
