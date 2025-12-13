Imports CrossSetaLogic.Models

Namespace Services
    Public Interface IUserService
        Sub RegisterUser(user As UserModel, password As String)
    End Interface
End Namespace
