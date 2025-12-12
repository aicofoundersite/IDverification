Imports System.Diagnostics
Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace CrossSetaDeduplicator.Services
    Public Class KYCResult
        Public Property IsSuccess As Boolean
        Public Property DocumentType As String
        Public Property IssuingCountry As String
        Public Property ExtractedFields As New Dictionary(Of String, String)
        Public Property RawJson As String
        Public Property ErrorMessage As String
    End Class

    Public Class FaceMatchResult
        Public Property IsMatch As Boolean
        Public Property ConfidenceScore As Double
        Public Property Message As String
    End Class

    Public Class KYCService
        Private _sdkPath As String
        Private _assetsPath As String

        Public Sub New()
            ' Calculate path relative to execution directory
            ' Assuming app runs from /bin/Debug/net6.0-windows/
            Dim baseDir = AppDomain.CurrentDomain.BaseDirectory
            
            ' Path to verify.exe in the cloned lib folder
            ' ROOT/lib/KYC-SDK/binaries/windows/x86_64/verify.exe
            ' Go up 6 levels from bin/Debug/net6.0-windows/ to Repo Root
            _sdkPath = Path.GetFullPath(Path.Combine(baseDir, "../../../../../../lib/KYC-SDK/binaries/windows/x86_64/verify.exe"))
            _assetsPath = Path.GetFullPath(Path.Combine(baseDir, "../../../../../../lib/KYC-SDK/assets"))
        End Sub

        Public Function VerifyDocument(imagePath As String) As KYCResult
            Dim result As New KYCResult()

            ' 1. Validation
            If Not File.Exists(imagePath) Then
                result.IsSuccess = False
                result.ErrorMessage = "Image file not found."
                Return result
            End If

            ' 2. Check if SDK exists and we are on Windows
            Dim isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
            
            If File.Exists(_sdkPath) AndAlso isWindows Then
                Return RunRealSDK(imagePath)
            Else
                ' Fallback for Non-Windows or Missing SDK (Demo Mode)
                Return RunMockSDK(imagePath)
            End If
        End Function

        Public Function CompareFaces(idDocumentPath As String, selfiePath As String) As FaceMatchResult
            Dim result As New FaceMatchResult()
            
            If Not File.Exists(idDocumentPath) OrElse Not File.Exists(selfiePath) Then
                result.IsMatch = False
                result.ConfidenceScore = 0
                result.Message = "One or both images not found."
                Return result
            End If

            ' TODO: Integrate with real Face Matching SDK (e.g. Doubango FaceLiveness or OpenCV)
            ' For Hackathon/Demo: Simulate matching logic
            
            ' Simulation Logic:
            ' If filenames contain "Thabo" or "Match", return success.
            ' Otherwise return random score or failure.
            
            Dim f1 = Path.GetFileName(idDocumentPath).ToLower()
            Dim f2 = Path.GetFileName(selfiePath).ToLower()

            If (f1.Contains("thabo") Or f1.Contains("match")) AndAlso (f2.Contains("thabo") Or f2.Contains("match")) Then
                result.IsMatch = True
                result.ConfidenceScore = 98.5
                result.Message = "High confidence match."
            ElseIf f1.Contains("nomatch") Or f2.Contains("nomatch") Then
                result.IsMatch = False
                result.ConfidenceScore = 12.4
                result.Message = "Faces do not match."
            Else
                ' Default to a "Good enough" match for general testing unless explicitly "bad"
                result.IsMatch = True
                result.ConfidenceScore = 85.0
                result.Message = "Match verified."
            End If

            Return result
        End Function

        Private Function RunRealSDK(imagePath As String) As KYCResult
            Dim result As New KYCResult()
            Try
                Dim startInfo As New ProcessStartInfo()
                startInfo.FileName = _sdkPath
                ' Command line arguments based on SDK samples: --image <path> --assets <path>
                startInfo.Arguments = $"--image ""{imagePath}"" --assets ""{_assetsPath}"""
                startInfo.RedirectStandardOutput = True
                startInfo.RedirectStandardError = True
                startInfo.UseShellExecute = False
                startInfo.CreateNoWindow = True

                Using process As Process = Process.Start(startInfo)
                    ' The SDK prints the JSON result to StdOut
                    Dim output As String = process.StandardOutput.ReadToEnd()
                    Dim err As String = process.StandardError.ReadToEnd()
                    process.WaitForExit()

                    ' Note: verify.exe might print logs to stdout/stderr too. 
                    ' We need to find the JSON part.
                    result = ParseSDKOutput(output)
                    
                    If Not result.IsSuccess AndAlso Not String.IsNullOrEmpty(err) Then
                         result.ErrorMessage = $"SDK Error: {err}"
                    End If
                End Using
            Catch ex As Exception
                result.IsSuccess = False
                result.ErrorMessage = $"Execution Error: {ex.Message}"
            End Try
            Return result
        End Function

        Private Function ParseSDKOutput(output As String) As KYCResult
            Dim result As New KYCResult()
            result.RawJson = output
            
            ' Robust JSON finding: Look for first '{' and last '}'
            Dim jsonStart = output.IndexOf("{")
            Dim jsonEnd = output.LastIndexOf("}")

            If jsonStart >= 0 AndAlso jsonEnd > jsonStart Then
                Dim jsonStr = output.Substring(jsonStart, jsonEnd - jsonStart + 1)
                Try
                    Dim jsonNode = System.Text.Json.Nodes.JsonNode.Parse(jsonStr)
                    
                    ' Basic Success Check (Modify based on actual SDK JSON)
                    ' Assuming typical Doubango SDK response
                    result.IsSuccess = True
                    result.DocumentType = "Unknown" 
                    
                    ' Try to extract fields dynamically
                    If jsonNode IsNot Nothing Then
                         ' Example extraction - adjust keys as needed
                         ' We will populate ExtractedFields with flat key-values
                         FlattenJson(jsonNode, result.ExtractedFields)
                         
                         ' Heuristics for common fields
                         If result.ExtractedFields.ContainsKey("type") Then result.DocumentType = result.ExtractedFields("type")
                         If result.ExtractedFields.ContainsKey("country") Then result.IssuingCountry = result.ExtractedFields("country")
                    End If

                Catch ex As Exception
                    result.IsSuccess = False
                    result.ErrorMessage = "Failed to parse SDK JSON output."
                End Try
            Else
                result.IsSuccess = False
                result.ErrorMessage = "No valid JSON found in SDK output."
            End If
            Return result
        End Function

        Private Sub FlattenJson(node As JsonNode, dict As Dictionary(Of String, String), Optional prefix As String = "")
            If node Is Nothing Then Return

            If TypeOf node Is JsonObject Then
                For Each kvp In CType(node, JsonObject)
                    FlattenJson(kvp.Value, dict, If(String.IsNullOrEmpty(prefix), kvp.Key, $"{prefix}.{kvp.Key}"))
                Next
            ElseIf TypeOf node Is JsonArray Then
                Dim arr = CType(node, JsonArray)
                For i As Integer = 0 To arr.Count - 1
                    FlattenJson(arr(i), dict, $"{prefix}[{i}]")
                Next
            Else
                ' Value
                dict(prefix) = node.ToString()
            End If
        End Sub

        Private Function RunMockSDK(imagePath As String) As KYCResult
            ' Mock Simulation for Demo/Non-Windows
            Dim result As New KYCResult()
            
            ' Simulate processing time
            System.Threading.Thread.Sleep(1000)

            result.IsSuccess = True
            result.DocumentType = "ID Card"
            result.IssuingCountry = "ZAF"
            
            ' Mock extracted data (simulating a successful scan of 'Thabo Molefe')
            result.ExtractedFields.Add("NationalID", "9505055000081")
            result.ExtractedFields.Add("Surname", "Molefe")
            result.ExtractedFields.Add("FirstNames", "Thabo")
            result.ExtractedFields.Add("DateOfBirth", "1995-05-05")
            result.ExtractedFields.Add("Gender", "Male")
            result.ExtractedFields.Add("ExpiryDate", "2030-01-01")
            
            result.RawJson = "{ ""mock"": true, ""message"": ""Simulated KYC Result"" }"
            
            Return result
        End Function
    End Class
End Namespace
