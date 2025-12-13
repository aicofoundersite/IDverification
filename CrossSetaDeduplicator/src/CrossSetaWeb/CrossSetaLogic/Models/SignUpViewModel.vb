Imports System.ComponentModel.DataAnnotations

Namespace Models
    Public Class SignUpViewModel
        <Required>
        <EmailAddress>
        Public Property Email As String

        <Required>
        <DataType(DataType.Password)>
        Public Property Password As String

        <DataType(DataType.Password)>
        <Display(Name:="Confirm password")>
        <Compare("Password", ErrorMessage:="The password and confirmation password do not match.")>
        Public Property ConfirmPassword As String
    End Class
End Namespace
