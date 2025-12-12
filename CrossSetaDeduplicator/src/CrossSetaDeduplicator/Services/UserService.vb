Imports System.Security.Cryptography
Imports System.Text
Imports CrossSetaDeduplicator.Models
Imports CrossSetaDeduplicator.DataAccess

Namespace CrossSetaDeduplicator.Services
    Public Class UserService
        Private _dbHelper As DatabaseHelper

        Public Sub New()
            _dbHelper = New DatabaseHelper(Nothing)
        End Sub

        Public Sub RegisterUser(user As UserModel, password As String)
            ' Validate
            If String.IsNullOrWhiteSpace(user.UserName) OrElse String.IsNullOrWhiteSpace(password) Then
                Throw New ArgumentException("Username and Password are required.")
            End If

            ' Hash Password
            user.PasswordHash = HashPassword(password)

            ' Save
            _dbHelper.InsertUser(user)
        End Sub

        Private Function HashPassword(password As String) As String
            Using sha256 As SHA256 = SHA256.Create()
                Dim bytes As Byte() = sha256.ComputeHash(Encoding.UTF8.GetBytes(password))
                Dim builder As New StringBuilder()
                For Each b As Byte In bytes
                    builder.Append(b.ToString("x2"))
                Next
                Return builder.ToString()
            End Using
        End Function
    End Class
End Namespace
