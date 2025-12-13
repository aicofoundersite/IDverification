Imports System.ComponentModel.DataAnnotations

Namespace Models
    Public Class RegisterUserViewModel
        Public Property IDType As String
        Public Property NationalID As String
        Public Property Title As String
        Public Property FirstName As String
        Public Property LastName As String
        
        <EmailAddress>
        Public Property Email As String
        
        <Compare("Email", ErrorMessage:="Emails do not match")>
        Public Property ConfirmEmail As String
        
        Public Property Province As String

        Public Property UserName As String
        
        <DataType(DataType.Password)>
        Public Property Password As String
        
        <DataType(DataType.Password)>
        <Compare("Password", ErrorMessage:="Passwords do not match")>
        Public Property ConfirmPassword As String

        Public Property SecurityQuestion As String
        Public Property SecurityAnswer As String

        Public Function ToUserModel() As UserModel
            Return New UserModel With {
                .IDType = IDType,
                .NationalID = NationalID,
                .Title = Title,
                .FirstName = FirstName,
                .LastName = LastName,
                .Email = Email,
                .Province = Province,
                .UserName = UserName,
                .SecurityQuestion = SecurityQuestion,
                .SecurityAnswer = SecurityAnswer
            }
        End Function
    End Class
End Namespace
