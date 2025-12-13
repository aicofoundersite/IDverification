Imports System.Collections.Generic

Namespace Models
    Public Class ImportResult
        Public Property Success As Boolean
        Public Property RecordsProcessed As Integer
        Public Property Errors As List(Of String)
    End Class
End Namespace
