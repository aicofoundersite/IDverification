Imports CrossSetaLogic.Models

Namespace DataAccess
    Public Interface IDatabaseHelper
        Sub InsertLearner(learner As LearnerModel)
        Sub InitializeHomeAffairsTable()
        Sub InitializeUserSchema()
        Sub UpdateStoredProcedures()
        Sub InitializeUserActivitySchema()
        Sub LogUserActivity(email As String, activityType As String, ipAddress As String, details As String)
        Function GetUserActivityLogs() As List(Of UserActivityLog)
        Sub BatchImportHomeAffairsData(citizens As List(Of HomeAffairsCitizen))
        Function BatchInsertLearners(learners As List(Of LearnerModel)) As List(Of DatabaseHelper.BulkInsertError)
        Sub InsertUser(user As UserModel)
        Function GetAllLearners() As List(Of LearnerModel)
        Function GetLearnerByNationalID(nationalID As String) As LearnerModel
        Function GetHomeAffairsCitizen(nationalID As String) As HomeAffairsCitizen
        Function GetLearnerValidationResults() As List(Of LearnerValidationResult)
        Sub DeleteLearner(firstName As String, lastName As String)
    End Interface
End Namespace
