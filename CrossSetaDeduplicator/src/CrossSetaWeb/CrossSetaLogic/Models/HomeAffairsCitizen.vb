Imports System.ComponentModel.DataAnnotations

Namespace Models
    Public Class HomeAffairsCitizen
        <Required>
        <StringLength(13, MinimumLength:=13)>
        Public Property NationalID As String

        <Required>
        <StringLength(100)>
        Public Property FirstName As String

        <Required>
        <StringLength(100)>
        Public Property Surname As String

        <Required>
        Public Property DateOfBirth As DateTime

        Public Property IsDeceased As Boolean
        
        Public Property VerificationSource As String
    End Class
End Namespace
