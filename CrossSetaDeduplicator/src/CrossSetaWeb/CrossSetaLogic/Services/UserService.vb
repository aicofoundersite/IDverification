Imports System
Imports System.Security.Cryptography
Imports System.Text
Imports CrossSetaLogic.DataAccess
Imports CrossSetaLogic.Models

Namespace Services
    Public Class UserService
        Implements IUserService

        Private ReadOnly _dbHelper As IDatabaseHelper

        Public Sub New(dbHelper As IDatabaseHelper)
            _dbHelper = dbHelper
        End Sub

        Public Sub RegisterUser(user As UserModel, password As String) Implements IUserService.RegisterUser
            If String.IsNullOrWhiteSpace(user.UserName) OrElse String.IsNullOrWhiteSpace(password) Then
                Throw New ArgumentException("Username and Password are required.")
            End If

            user.PasswordHash = HashPassword(password)
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
