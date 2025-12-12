Imports System.Threading.Tasks
Imports System.Text.Json

Namespace CrossSetaDeduplicator.Services.External
    ''' <summary>
    ''' Represents the data contract for the Home Affairs API response.
    ''' </summary>
    Public Class HomeAffairsApiResponse
        Public Property NationalID As String
        Public Property FirstName As String
        Public Property Surname As String
        Public Property DateOfBirth As String
        Public Property Status As String ' "Alive", "Deceased", "NotFound"
        Public Property Message As String
    End Class

    ''' <summary>
    ''' Simulates a standard HTTP Client consuming a REST API.
    ''' </summary>
    Public Class MockHomeAffairsClient
        ''' <summary>
        ''' Simulates an HTTP GET request to the Home Affairs API.
        ''' URI: https://api.home-affairs.gov.za/verify/{idNumber}
        ''' </summary>
        Public Async Function GetCitizenDetailsAsync(idNumber As String) As Task(Of HomeAffairsApiResponse)
            ' Simulate Network Latency
            Await Task.Delay(800)

            Dim response As New HomeAffairsApiResponse()
            response.NationalID = idNumber

            ' Mock Data Logic (simulating Server-Side DB lookup)
            If idNumber = "9999999999999" Then
                response.Status = "Deceased"
                response.Message = "Person is marked as DECEASED."
            ElseIf idNumber.StartsWith("00") Then
                response.Status = "NotFound"
                response.Message = "ID Number not found in registry."
            Else
                ' Happy Path
                response.Status = "Alive"
                response.FirstName = "Thabo" ' Seeded for demo
                response.Surname = "Molefe"
                response.DateOfBirth = "1995-05-05"
                response.Message = "Citizen found and status is ACTIVE."
            End If

            Return response
        End Function
    End Class
End Namespace
