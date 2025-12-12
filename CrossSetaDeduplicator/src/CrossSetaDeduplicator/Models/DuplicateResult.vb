Public Class DuplicateResult
    Public Property IsDuplicate As Boolean
    Public Property MatchType As String ' "Exact", "Fuzzy", "None"
    Public Property MatchScore As Integer ' 0-100
    Public Property MatchedLearner As LearnerModel
    Public Property FoundInSeta As String
    Public Property Message As String

    Public Sub New()
        IsDuplicate = False
        MatchType = "None"
        MatchScore = 0
        MatchedLearner = Nothing
        Message = "No duplicate found."
    End Sub
End Class
