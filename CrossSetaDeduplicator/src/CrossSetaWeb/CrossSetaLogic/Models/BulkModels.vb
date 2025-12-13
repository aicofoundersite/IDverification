Imports System.Collections.Generic
Imports System.Linq

Namespace Models
    Public Class BulkImportResult
        Public Property SuccessCount As Integer
        Public Property FailureCount As Integer
        Public Property ErrorDetails As New List(Of BulkErrorDetail)()

        ' Compatibility property if needed, maps details to strings
        Public ReadOnly Property Errors As List(Of String)
            Get
                Return ErrorDetails.Select(Function(e) $"Row {e.RowNumber}: {e.Message}").ToList()
            End Get
        End Property
    End Class

    Public Class BulkErrorDetail
        Public Property RowNumber As Integer
        Public Property NationalID As String
        Public Property Message As String
    End Class
End Namespace
