Imports System
Imports System.IO
Imports System.Threading.Tasks
Imports Microsoft.AspNetCore.Http
Imports CrossSetaLogic.Models

Namespace Services
    Public Class KYCResult
        Public Property IsSuccess As Boolean
        Public Property DocumentType As String
        Public Property ErrorMessage As String
        Public Property ExtractedNationalID As String
        Public Property ExtractedSurname As String
    End Class

    Public Class KYCService
        Implements IKYCService

        Private ReadOnly _uploadPath As String

        Public Sub New()
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "kyc")
            If Not Directory.Exists(_uploadPath) Then
                Directory.CreateDirectory(_uploadPath)
            End If
        End Sub

        Public Async Function VerifyDocumentAsync(file As IFormFile, claimedNationalID As String) As Task(Of KYCResult) Implements IKYCService.VerifyDocumentAsync
            If file Is Nothing OrElse file.Length = 0 Then
                Return New KYCResult With {.IsSuccess = False, .ErrorMessage = "No file uploaded."}
            End If

            ' 1. Save File Securely (Rename to prevent collisions)
            Dim fileName As String = $"{claimedNationalID}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}"
            Dim fullPath As String = Path.Combine(_uploadPath, fileName)

            Using stream As New FileStream(fullPath, FileMode.Create)
                Await file.CopyToAsync(stream)
            End Using

            ' 2. Mock Verification Logic (Linux/Docker Environment)
            ' In a real scenario, we would call the Doubango SDK binary here.
            ' Since we are running in a Linux Container without the specific binaries,
            ' we will simulate the "Verification" step.
            
            ' Simulation: Assume if file is uploaded, it's a valid ID for this Hackathon demo.
            Await Task.Delay(1000) ' Simulate processing time

            Return New KYCResult With {
                .IsSuccess = True,
                .DocumentType = "South African ID Document",
                .ExtractedNationalID = claimedNationalID, ' Simulate successful extraction
                .ExtractedSurname = "SimulatedSurname"
            }
        End Function
    End Class
End Namespace
