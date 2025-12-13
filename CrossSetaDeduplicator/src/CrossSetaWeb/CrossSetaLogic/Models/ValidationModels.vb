Imports System.Collections.Generic

Namespace Models
    Public Class DatabaseValidationResult
        Public Property TotalRecords As Integer
        Public Property ValidCount As Integer
        Public Property DeceasedCount As Integer
        Public Property NotFoundCount As Integer
        Public Property SurnameMismatchCount As Integer
        Public Property InvalidFormatCount As Integer
        Public Property ReportFileName As String
        Public Property Details As New List(Of ValidationDetail)()
    End Class

    Public Class ValidationDetail
        Public Property NationalID As String
        Public Property FirstName As String
        Public Property LastName As String
        Public Property Status As String ' Valid, Deceased, NotFound, SurnameMismatch, InvalidFormat
        Public Property Message As String
    End Class
End Namespace
