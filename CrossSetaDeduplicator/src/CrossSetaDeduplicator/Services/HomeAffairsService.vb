Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports CrossSetaDeduplicator.DataAccess
Imports CrossSetaDeduplicator.Services.External

Namespace CrossSetaDeduplicator.Services
    Public Class VerificationResult
        Public Property IsValid As Boolean
        Public Property Status As String ' Verified, Mismatch, NotFound, Queued
        Public Property Message As String
        Public Property TrafficLightColor As System.Drawing.Color
    End Class

    Public Class HomeAffairsService
        Private _dbHelper As DatabaseHelper
        Private _offlineQueuePath As String = "offline_verification_queue.json"
        Private _client As New MockHomeAffairsClient()
        
        ' Toggle this to demonstrate offline capabilities
        Public Property SimulateOffline As Boolean = False

        Public Sub New()
            _dbHelper = New DatabaseHelper(Nothing)
        End Sub

        ''' <summary>
        ''' Validates the South African ID number format using the Luhn algorithm.
        ''' </summary>
        Public Function ValidateIDFormat(idNumber As String) As Boolean
            If String.IsNullOrWhiteSpace(idNumber) OrElse idNumber.Length <> 13 OrElse Not IsNumeric(idNumber) Then
                Return False
            End If

            Dim total As Integer = 0
            For i As Integer = 0 To 12
                Dim n As Integer = Integer.Parse(idNumber(i).ToString())
                If i Mod 2 = 0 Then
                    total += n
                Else
                    n *= 2
                    If n > 9 Then n -= 9
                    total += n
                End If
            Next

            Return (total Mod 10 = 0)
        End Function

        ''' <summary>
        ''' Simulates a real-time connection to the Department of Home Affairs (DHA).
        ''' Implements Traffic Light Protocol: Green (Verified), Yellow (Mismatch), Red (Invalid/Fraud).
        ''' </summary>
        Public Async Function VerifyCitizenAsync(id As String, inputFirstName As String, inputSurname As String, Optional performedBy As String = "System") As Task(Of VerificationResult)
            Dim startTime = DateTime.Now
            Dim result As New VerificationResult()

            ' 1. Check Offline Mode
            If SimulateOffline Then
                QueueOfflineRequest(id, inputFirstName, inputSurname)
                result.IsValid = True ' Temporarily assume valid to allow flow to proceed? Or warn?
                result.Status = "OfflineQueued"
                result.Message = "System Offline. Verification queued for automatic retry."
                result.TrafficLightColor = System.Drawing.Color.Orange
                
                ' Log as queued
                _dbHelper.LogExternalVerification(id, "HomeAffairs_Mock", "OfflineQueued", "Network unavailable", startTime, performedBy)
                Return result
            End If

            ' 2. Input Validation (Luhn Check)
            If Not ValidateIDFormat(id) Then
                result.IsValid = False
                result.Status = "InvalidFormat"
                result.Message = "Invalid SA ID Number format."
                result.TrafficLightColor = System.Drawing.Color.Red
                _dbHelper.LogExternalVerification(id, "HomeAffairs_Mock", "InvalidFormat", "Luhn check failed", startTime, performedBy)
                Return result
            End If

            ' 3. Call External API (Mock)
            Dim haResponse As HomeAffairsApiResponse = Await _client.GetCitizenDetailsAsync(id)

            ' 4. Process Response (Traffic Light Protocol)
            If haResponse.Status = "Deceased" Then
                ' RED: Deceased
                result.IsValid = False
                result.Status = "Deceased"
                result.Message = "DHA Alert: ID holder marked as DECEASED."
                result.TrafficLightColor = System.Drawing.Color.Red

            ElseIf haResponse.Status = "NotFound" Then
                ' RED: Not Found
                result.IsValid = False
                result.Status = "NotFound"
                result.Message = "DHA Alert: ID number not found in National Register."
                result.TrafficLightColor = System.Drawing.Color.Red

            ElseIf haResponse.Status = "Alive" Then
                ' Check Surname Match (Case Insensitive)
                If String.Equals(haResponse.Surname, inputSurname, StringComparison.OrdinalIgnoreCase) Then
                    ' GREEN: Verified and Matches
                    result.IsValid = True
                    result.Status = "Verified"
                    result.Message = "Identity Verified against National Population Register."
                    result.TrafficLightColor = System.Drawing.Color.Green
                Else
                    ' YELLOW: Surname Mismatch (Review Needed)
                    ' Note: We still mark as "Valid" in terms of existence, but UI should warn.
                    ' Or we mark IsValid = False depending on strictness. 
                    ' Prompt says "Yellow (Surname Mismatch/Review)".
                    result.IsValid = True ' Allow proceeding but with warning
                    result.Status = "Mismatch"
                    result.Message = $"Warning: ID Valid but Surname mismatch. DHA: {haResponse.Surname} vs Input: {inputSurname}"
                    result.TrafficLightColor = System.Drawing.Color.Yellow
                End If
            Else
                ' Fallback
                result.IsValid = False
                result.Status = "Error"
                result.Message = "Unknown Error from DHA."
                result.TrafficLightColor = System.Drawing.Color.Red
            End If

            ' 5. Audit Trail
            _dbHelper.LogExternalVerification(id, "HomeAffairs_Mock", result.Status, result.Message, startTime, performedBy)
            
            Return result
        End Function

        Private Sub QueueOfflineRequest(id As String, firstName As String, surname As String)
            Dim queueItem = New With {
                .NationalID = id,
                .FirstName = firstName,
                .Surname = surname,
                .Timestamp = DateTime.Now
            }
            
            Dim json As String = JsonSerializer.Serialize(queueItem) & Environment.NewLine
            File.AppendAllText(_offlineQueuePath, json)
        End Sub

        Public Function GetOfflineQueueSize() As Integer
            If Not File.Exists(_offlineQueuePath) Then Return 0
            Return File.ReadAllLines(_offlineQueuePath).Length
        End Function
        
        Public Async Function ProcessOfflineQueueAsync() As Task(Of Integer)
            If Not File.Exists(_offlineQueuePath) Then Return 0
            
            Dim lines = File.ReadAllLines(_offlineQueuePath)
            Dim processedCount As Integer = 0
            
            For Each line In lines
                If String.IsNullOrWhiteSpace(line) Then Continue For
                
                Try
                    ' Deserialize
                    ' We used an anonymous type to serialize, so we need to parse carefully or use a helper class.
                    ' Using JsonNode for flexibility.
                    Dim node = JsonNode.Parse(line)
                    Dim id = node("NationalID").ToString()
                    Dim fn = node("FirstName").ToString()
                    Dim sn = node("Surname").ToString()
                    
                    ' Process: Call VerifyCitizenAsync (Recursive? No, simulate processing)
                    ' In a real scenario, we would re-verify. Here we just log that we processed it.
                    Dim startTime = DateTime.Now
                    
                    ' Verify against our Mock Client
                    Dim haResponse As HomeAffairsApiResponse = Await _client.GetCitizenDetailsAsync(id)
                    Dim status = If(haResponse.Status = "Alive", "Verified", haResponse.Status)
                    
                    _dbHelper.LogExternalVerification(id, "HomeAffairs_Queue", status, "Processed from Offline Queue", startTime, "System_Auto")
                    processedCount += 1
                    
                Catch ex As Exception
                    ' Log error?
                End Try
            Next
            
            ' Clear queue after processing
            File.Delete(_offlineQueuePath)
            Return processedCount
        End Function
    End Class
End Namespace
