Imports CrossSetaLogic.Models

Namespace Services
    Public Interface IDatabaseValidationService
        Function ValidateDatabase(Optional jobId As String = Nothing) As DatabaseValidationResult
    End Interface
End Namespace
