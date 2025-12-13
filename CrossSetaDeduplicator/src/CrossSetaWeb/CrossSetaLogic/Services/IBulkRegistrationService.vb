Imports Microsoft.AspNetCore.Http
Imports CrossSetaLogic.Models

Namespace Services
    Public Interface IBulkRegistrationService
        Function ProcessBulkFile(file As IFormFile) As BulkImportResult
        Sub SeedLearners(filePath As String)
    End Interface
End Namespace
