Imports System.ComponentModel.DataAnnotations

Namespace Validation
    Public Class LuhnAttribute
        Inherits ValidationAttribute

        Protected Overrides Function IsValid(value As Object, validationContext As ValidationContext) As ValidationResult
            If value Is Nothing Then
                Return ValidationResult.Success
            End If

            Dim idNumber As String = value.ToString()

            ' Check if it is a valid number
            Dim dummy As Long
            If Not Long.TryParse(idNumber, dummy) Then
                Return New ValidationResult("ID Number must contain only digits.")
            End If

            ' Check length (SA ID is 13 digits)
            If idNumber.Length <> 13 Then
                Return New ValidationResult("ID Number must be exactly 13 digits.")
            End If

            If Not IsValidLuhn(idNumber) Then
                Return New ValidationResult("Invalid ID Number (Luhn Check Failed).")
            End If

            Return ValidationResult.Success
        End Function

        Private Function IsValidLuhn(id As String) As Boolean
            Dim sum As Integer = 0
            Dim alternate As Boolean = False

            For i As Integer = id.Length - 1 To 0 Step -1
                Dim c As Char = id(i)
                If Not Char.IsDigit(c) Then
                    Return False
                End If

                Dim n As Integer = Integer.Parse(c.ToString())

                If alternate Then
                    n *= 2
                    If n > 9 Then
                        n = (n Mod 10) + 1
                    End If
                End If

                sum += n
                alternate = Not alternate
            Next

            Return (sum Mod 10 = 0)
        End Function
    End Class
End Namespace
