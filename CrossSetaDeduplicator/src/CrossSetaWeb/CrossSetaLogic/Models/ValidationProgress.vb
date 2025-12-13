Namespace Models
    Public Class ValidationProgress
        Public Property Total As Integer
        Public Property Processed As Integer
        Public ReadOnly Property Percentage As Integer
            Get
                If Total = 0 Then Return 0
                Return CInt((Processed / Total) * 100)
            End Get
        End Property
        Public Property Status As String
        Public Property IsComplete As Boolean
        Public Property Result As DatabaseValidationResult
    End Class
End Namespace
