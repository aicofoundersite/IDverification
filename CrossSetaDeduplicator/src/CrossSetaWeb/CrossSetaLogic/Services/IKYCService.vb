Imports System.Threading.Tasks
Imports Microsoft.AspNetCore.Http
Imports CrossSetaLogic.Models

Namespace Services
    Public Interface IKYCService
        Function VerifyDocumentAsync(file As IFormFile, claimedNationalID As String) As Task(Of KYCResult)
    End Interface
End Namespace
