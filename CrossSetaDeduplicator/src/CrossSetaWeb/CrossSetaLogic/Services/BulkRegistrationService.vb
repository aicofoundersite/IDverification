Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.AspNetCore.Http
Imports Microsoft.Extensions.Logging
Imports CrossSetaLogic.DataAccess
Imports CrossSetaLogic.Models

Namespace Services
    Public Class BulkRegistrationService
        Implements IBulkRegistrationService

        Private ReadOnly _dbHelper As IDatabaseHelper
        Private ReadOnly _logger As ILogger(Of BulkRegistrationService)

        Public Sub New(dbHelper As IDatabaseHelper, logger As ILogger(Of BulkRegistrationService))
            _dbHelper = dbHelper
            _logger = logger
        End Sub

        Public Function ProcessBulkFile(file As IFormFile) As BulkImportResult Implements IBulkRegistrationService.ProcessBulkFile
            _logger.LogInformation("Starting bulk import for file: {FileName}, Size: {Length}", file.FileName, file.Length)
            Dim result As New BulkImportResult()
            Dim validLearners As New List(Of LearnerModel)()
            Dim rowMap As New Dictionary(Of String, Integer)() ' Map NationalID to RowNumber for error reporting

            Try
                Using reader As New StreamReader(file.OpenReadStream())
                    Dim line As String
                    Dim isHeader As Boolean = True
                    Dim rowNumber As Integer = 0

                    line = reader.ReadLine()
                    While line IsNot Nothing
                        rowNumber += 1
                        If String.IsNullOrWhiteSpace(line) Then
                            line = reader.ReadLine()
                            Continue While
                        End If

                        ' Heuristic check: if the first line contains "Identity Number", treat as header
                        If isHeader Then
                            If line.Contains("Identity Number", StringComparison.OrdinalIgnoreCase) OrElse
                               line.Contains("First Name", StringComparison.OrdinalIgnoreCase) Then
                                isHeader = False
                                line = reader.ReadLine()
                                Continue While
                            End If
                            isHeader = False ' If not header-like, treat as data? Or just skip 1st line always?
                            ' Safe bet: Always skip first line if it's the very first line read.
                            ' But if user uploads NO header, we lose data.
                            ' Given the requirement "use the csv format in this file", which HAS headers, we stick to skipping first line.
                            line = reader.ReadLine()
                            Continue While
                        End If

                        Try
                            Dim learner = ParseLine(line, rowNumber)

                            ' Check for duplicates within the file itself
                            If rowMap.ContainsKey(learner.NationalID) Then
                                Dim msg As String = "Duplicate ID within the same file."
                                result.ErrorDetails.Add(New BulkErrorDetail With {
                                    .RowNumber = rowNumber,
                                    .NationalID = learner.NationalID,
                                    .Message = msg
                                })
                                result.FailureCount += 1
                                _logger.LogWarning("Row {RowNumber}: {Message}", rowNumber, msg)
                                line = reader.ReadLine()
                                Continue While
                            End If

                            validLearners.Add(learner)
                            rowMap(learner.NationalID) = rowNumber
                        Catch ex As Exception
                            result.ErrorDetails.Add(New BulkErrorDetail With {
                                .RowNumber = rowNumber,
                                .NationalID = "Unknown",
                                .Message = ex.Message
                            })
                            result.FailureCount += 1
                            _logger.LogWarning("Row {RowNumber} Parse Error: {Message}", rowNumber, ex.Message)
                        End Try

                        line = reader.ReadLine()
                    End While
                End Using

                If validLearners.Count > 0 Then
                    _logger.LogInformation("Parsed {Count} valid records. Attempting DB insertion.", validLearners.Count)

                    ' Attempt DB Insertion
                    Dim dbErrors = _dbHelper.BatchInsertLearners(validLearners)

                    ' Map DB errors back to results
                    For Each errorItem In dbErrors
                        Dim row As Integer = If(rowMap.ContainsKey(errorItem.NationalID), rowMap(errorItem.NationalID), 0)
                        result.ErrorDetails.Add(New BulkErrorDetail With {
                            .RowNumber = row,
                            .NationalID = errorItem.NationalID,
                            .Message = errorItem.Message
                        })
                        result.FailureCount += 1
                        _logger.LogError("DB Error Row {RowNumber} (ID: {NationalID}): {Message}", row, errorItem.NationalID, errorItem.Message)
                    Next

                    ' Success count is total valid sent - total db errors
                    result.SuccessCount = validLearners.Count - dbErrors.Count
                End If

                _logger.LogInformation("Bulk import completed. Success: {Success}, Failed: {Failed}", result.SuccessCount, result.FailureCount)
            Catch ex As Exception
                Dim fatalMsg As String = $"Fatal Error: {ex.Message}"
                result.ErrorDetails.Add(New BulkErrorDetail With {.RowNumber = 0, .Message = fatalMsg})
                _logger.LogCritical(ex, "Fatal error during bulk import.")
            End Try

            Return result
        End Function

        Public Sub SeedLearners(filePath As String) Implements IBulkRegistrationService.SeedLearners
            If Not File.Exists(filePath) Then
                _logger.LogWarning("Seed file not found: {FilePath}", filePath)
                Return
            End If

            _logger.LogInformation("Seeding learners from: {FilePath}", filePath)
            Dim validLearners As New List(Of LearnerModel)()

            Try
                Using reader As New StreamReader(filePath)
                    Dim line As String
                    Dim isHeader As Boolean = True
                    Dim rowNumber As Integer = 0

                    line = reader.ReadLine()
                    While line IsNot Nothing
                        rowNumber += 1
                        If String.IsNullOrWhiteSpace(line) Then
                            line = reader.ReadLine()
                            Continue While
                        End If

                        ' Header check logic same as ProcessBulkFile
                        If isHeader Then
                            If line.Contains("Identity Number", StringComparison.OrdinalIgnoreCase) OrElse
                               line.Contains("First Name", StringComparison.OrdinalIgnoreCase) Then
                                isHeader = False
                                line = reader.ReadLine()
                                Continue While
                            End If
                            isHeader = False
                            line = reader.ReadLine()
                            Continue While
                        End If

                        Try
                            Dim learner = ParseLine(line, rowNumber)
                            validLearners.Add(learner)
                        Catch ex As Exception
                            _logger.LogWarning("Seed Row {RowNumber} Error: {Message}", rowNumber, ex.Message)
                        End Try

                        line = reader.ReadLine()
                    End While
                End Using

                If validLearners.Count > 0 Then
                    _logger.LogInformation("Found {Count} valid records to seed. Inserting...", validLearners.Count)
                    Dim errors = _dbHelper.BatchInsertLearners(validLearners)
                    _logger.LogInformation("Seeding completed. Errors: {Count}", errors.Count)
                End If
            Catch ex As Exception
                _logger.LogError(ex, "Error seeding learners.")
            End Try
        End Sub

        Private Function ParseLine(line As String, rowNumber As Integer) As LearnerModel
            ' Robust CSV Parsing to handle quotes
            Dim parts = ParseCsvLine(line)

            ' Expected CSV Format from Google Sheet:
            ' First Name / s, Surname, Date of Birth, Identity Number, Target #, Intervention Name, Period
            ' Index: 0, 1, 2, 3
            If parts.Count < 4 Then Throw New Exception("Insufficient columns. Required: First Name, Surname, Date of Birth, Identity Number")

            Dim learner As New LearnerModel With {
                .FirstName = parts(0).Trim(),
                .LastName = parts(1).Trim(),
                .NationalID = parts(3).Trim(), ' Identity Number is at index 3
                .IsVerified = False,
                .PopiActConsent = True,
                .PopiActDate = DateTime.Now
            }

            ' Validate ID (Luhn)
            If Not IsValidLuhn(learner.NationalID) Then
                Throw New Exception($"Invalid ID Number: {learner.NationalID}")
            End If

            ' Date Parsing (Format: dd/MM/yy)
            Dim dobStr As String = parts(2).Trim()
            If Not String.IsNullOrWhiteSpace(dobStr) Then
                Dim formats As String() = {"dd/MM/yy", "dd/MM/yyyy", "yyyy-MM-dd"}
                Dim dob As DateTime
                If DateTime.TryParseExact(dobStr, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, dob) Then
                    learner.DateOfBirth = dob
                ElseIf DateTime.TryParse(dobStr, dob) Then
                    learner.DateOfBirth = dob
                Else
                    Dim serial As Double
                    If Double.TryParse(dobStr, serial) Then
                        learner.DateOfBirth = DateTime.FromOADate(serial)
                    Else
                        Throw New Exception($"Invalid Date Format: {dobStr} (Expected dd/MM/yy)")
                    End If
                End If
            End If

            ' Default values for fields not in this specific CSV format
            learner.Gender = "Unknown" ' Not in CSV

            Return learner
        End Function

        Private Function ParseCsvLine(line As String) As List(Of String)
            Dim values As New List(Of String)()
            Dim inQuotes As Boolean = False
            Dim currentValue As New StringBuilder()

            For i As Integer = 0 To line.Length - 1
                Dim c As Char = line(i)
                If c = """"c Then
                    If inQuotes AndAlso i + 1 < line.Length AndAlso line(i + 1) = """"c Then
                        currentValue.Append(""""c) ' Escaped quote
                        i += 1
                    Else
                        inQuotes = Not inQuotes
                    End If
                ElseIf c = ","c AndAlso Not inQuotes Then
                    values.Add(currentValue.ToString())
                    currentValue.Clear()
                Else
                    currentValue.Append(c)
                End If
            Next
            values.Add(currentValue.ToString())
            Return values
        End Function

        Private Function IsValidEmail(email As String) As Boolean
            Try
                Dim addr = New System.Net.Mail.MailAddress(email)
                Return addr.Address = email
            Catch
                Return False
            End Try
        End Function

        Private Function IsValidPhone(phone As String) As Boolean
            ' Simple check: starts with 0 or +27, contains only digits and spaces, length 10-15
            Return Regex.IsMatch(phone, "^(\+27|0)[0-9\s]{8,15}$")
        End Function

        Public Shared Function IsValidLuhn(id As String) As Boolean
            If String.IsNullOrEmpty(id) Then Return False
            ' Avoid allocation if possible, but Trim() allocates.
            ' ID length check first to fail fast.
            If id.Length < 13 Then Return False

            ' We can operate on the string directly if we assume it's clean, 
            ' but Trim() is safer for user input.
            Dim trimmed = id.Trim()
            If trimmed.Length <> 13 Then Return False

            ' Manual digit check to avoid long.TryParse overhead if not needed, 
            ' but long.TryParse is good for verifying all digits.
            ' However, looping and checking IsDigit is faster than TryParse + loop.
            ' Let's stick to the loop logic which implicitly checks digits via subtraction.

            Dim sum As Integer = 0
            Dim alternate As Boolean = False
            For i As Integer = trimmed.Length - 1 To 0 Step -1
                Dim c As Char = trimmed(i)
                If c < "0"c OrElse c > "9"c Then Return False ' Non-digit check

                Dim n As Integer = Asc(c) - Asc("0"c) ' Optimized: No ToString/Parse
                If alternate Then
                    n *= 2
                    If n > 9 Then n = (n Mod 10) + 1
                End If
                sum += n
                alternate = Not alternate
            Next
            Return (sum Mod 10 = 0)
        End Function
    End Class
End Namespace
