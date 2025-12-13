Imports System.Threading.Tasks
Imports System.Text.Json
Imports System.Net.Http
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.FileIO

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
        Private Shared _cachedData As Dictionary(Of String, HomeAffairsApiResponse)
        ' Reverted to CSV due to .bak file incompatibility
        Private Const _sheetUrl As String = "https://docs.google.com/spreadsheets/d/1eQjxSsuOuXU20xG0gGgmR0Agn7WvudJd/export?format=csv&gid=572729852"

        ''' <summary>
        ''' Simulates an HTTP GET request to the Home Affairs API.
        ''' URI: https://api.home-affairs.gov.za/verify/{idNumber}
        ''' </summary>
        Public Async Function GetCitizenDetailsAsync(idNumber As String) As Task(Of HomeAffairsApiResponse)
            ' Simulate Network Latency (reduced as we might be fetching real data first time)
            Await Task.Delay(200)

            ' Ensure data is loaded from Google Sheet
            Await EnsureDataLoadedAsync()

            Dim response As New HomeAffairsApiResponse()
            response.NationalID = idNumber

            ' 1. Special Test Case for Deceased (Keep for testing purposes)
            If idNumber = "9999999999999" Then
                response.Status = "Deceased"
                response.Message = "Person is marked as DECEASED."
                Return response
            End If

            ' New Test Case requested by User
            If idNumber = "0001010000001" Then
                response.Status = "Deceased"
                response.FirstName = "Any"
                response.Surname = "Any"
                response.Message = "Person is marked as DECEASED."
                Return response
            End If

            ' 2. Lookup in Google Sheet Data
            If _cachedData IsNot Nothing AndAlso _cachedData.ContainsKey(idNumber) Then
                Dim record = _cachedData(idNumber)
                response.Status = "Alive"
                response.FirstName = record.FirstName
                response.Surname = record.Surname
                response.DateOfBirth = record.DateOfBirth
                response.Message = "Citizen found and status is ACTIVE."
            Else
                ' 3. Not Found
                response.Status = "NotFound"
                response.Message = "ID Number not found in National Register (Google Sheet)."
            End If

            Return response
        End Function

        Private Async Function EnsureDataLoadedAsync() As Task
            If _cachedData IsNot Nothing Then Return

            _cachedData = New Dictionary(Of String, HomeAffairsApiResponse)()

            Try
                Using client As New HttpClient()
                    Dim csvContent As String = Await client.GetStringAsync(_sheetUrl)
                    
                    Using reader As New StringReader(csvContent)
                        Using parser As New TextFieldParser(reader)
                            parser.TextFieldType = FieldType.Delimited
                            parser.SetDelimiters(",")
                            
                            ' Skip Header
                            If Not parser.EndOfData Then parser.ReadFields()
                            
                            While Not parser.EndOfData
                                Dim fields = parser.ReadFields()
                                ' Expected Columns: First Name / s, Surname, Date of Birth, Identity Number, ...
                                ' Index: 0, 1, 2, 3
                                If fields IsNot Nothing AndAlso fields.Length >= 4 Then
                                    Dim id = fields(3).Trim()
                                    ' Basic validation to ensure we capture valid IDs
                                    If Not String.IsNullOrEmpty(id) AndAlso Not _cachedData.ContainsKey(id) Then
                                        Dim p As New HomeAffairsApiResponse With {
                                            .NationalID = id,
                                            .FirstName = fields(0).Trim(),
                                            .Surname = fields(1).Trim(),
                                            .DateOfBirth = fields(2).Trim()
                                        }
                                        _cachedData(id) = p
                                    End If
                                End If
                            End While
                            Console.WriteLine($"[MockHomeAffairsClient] Successfully loaded {_cachedData.Count} records from Google Sheet.")
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                ' In case of network error or parsing error, we log (to console for now)
                ' and leave cache empty or partial.
                Console.WriteLine($"Error fetching Home Affairs Database: {ex.Message}")
            End Try
        End Function
    End Class
End Namespace
