Imports CrossSetaDeduplicator.Models
Imports CrossSetaDeduplicator.DataAccess

Public Class DeduplicationService
    Private _dbHelper As DatabaseHelper

    Public Sub New()
        _dbHelper = New DatabaseHelper(Nothing) ' Use default connection string
    End Sub

    Public Function CheckForDuplicates(learner As LearnerModel) As DuplicateResult
        Dim result As New DuplicateResult()
        
        ' 1. Fetch potential matches from DB (Optimization: don't fetch all, just fetch those that match ID or Name roughly)
        ' For prototype, we rely on FindPotentialDuplicates which gets exact ID matches or exact Name matches.
        ' However, for true Fuzzy search on name against ALL records, we might need to fetch all or use SQL fuzzy search.
        ' To keep it simple and efficient as per prompt "FindPotentialDuplicates (which queries ... based on NationalID and name)",
        ' we will use the results from that. If we want broader fuzzy matching (e.g. "Jon" vs "John"), 
        ' we would typically need a broader search or load all names into memory cache. 
        ' Let's assume we load all for the "Demo Dedupe" logic if the dataset is small (Hackathon scale).
        ' Or better, let's just use the FindPotentialDuplicates and then apply fuzzy logic on that subset 
        ' AND maybe a few others if we want to demonstrate fuzzy. 
        ' Actually, the prompt says: "Perform a fuzzy/partial match ... using simple string comparison ... for a prototype."
        ' So let's fetch all learners to do a proper in-memory fuzzy check for the prototype.
        
        Dim allLearners = _dbHelper.GetAllLearners() 

        For Each existingLearner In allLearners
            ' Skip self if updating (not applicable here as we capture new data)
            
            ' a) Exact Match on National ID
            If existingLearner.NationalID = learner.NationalID Then
                result.IsDuplicate = True
                result.MatchType = "Exact"
                result.MatchScore = 100
                result.MatchedLearner = existingLearner
                result.Message = "Exact match found on National ID."
                
                ' Log it
                _dbHelper.LogDuplicateCheck(learner.NationalID, existingLearner.LearnerID, 100, "Exact")
                Return result
            End If

            ' b) Fuzzy Match on Name
            Dim firstNameDist = LevenshteinDistance(existingLearner.FirstName.ToLower(), learner.FirstName.ToLower())
            Dim lastNameDist = LevenshteinDistance(existingLearner.LastName.ToLower(), learner.LastName.ToLower())
            
            ' Threshold: e.g. distance <= 2 means very close
            If firstNameDist <= 2 AndAlso lastNameDist <= 2 Then
                 ' Calculate a simple score based on length
                 Dim maxLen = Math.Max(existingLearner.FirstName.Length + existingLearner.LastName.Length, learner.FirstName.Length + learner.LastName.Length)
                 Dim totalDist = firstNameDist + lastNameDist
                 Dim score = CInt((1.0 - (totalDist / maxLen)) * 100)
                 
                 If score > 80 Then ' Threshold for duplicate
                    result.IsDuplicate = True
                    result.MatchType = "Fuzzy"
                    result.MatchScore = score
                    result.MatchedLearner = existingLearner
                    result.Message = $"Potential fuzzy match found on Name. Score: {score}"
                    
                    _dbHelper.LogDuplicateCheck(learner.NationalID, existingLearner.LearnerID, score, "Fuzzy")
                    Return result
                 End If
            End If
        Next

        ' No match found
        _dbHelper.LogDuplicateCheck(learner.NationalID, Nothing, 0, "None")
        Return result
    End Function

    Private Function LevenshteinDistance(s As String, t As String) As Integer
        Dim n As Integer = s.Length
        Dim m As Integer = t.Length
        Dim d(n, m) As Integer

        If n = 0 Then Return m
        If m = 0 Then Return n

        For i As Integer = 0 To n
            d(i, 0) = i
        Next

        For j As Integer = 0 To m
            d(0, j) = j
        Next

        For i As Integer = 1 To n
            For j As Integer = 1 To m
                Dim cost As Integer = If(t(j - 1) = s(i - 1), 0, 1)
                d(i, j) = Math.Min(Math.Min(d(i - 1, j) + 1, d(i, j - 1) + 1), d(i - 1, j - 1) + cost)
            Next
        Next

        Return d(n, m)
    End Function
End Class
