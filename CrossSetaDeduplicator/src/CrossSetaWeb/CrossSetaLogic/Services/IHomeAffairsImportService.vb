Imports System.Threading.Tasks
Imports CrossSetaLogic.Models

Namespace Services
    Public Interface IHomeAffairsImportService
        Function ImportFromUrlAsync(url As String) As Task(Of ImportResult)
        Function ImportFromContent(content As String) As ImportResult
    End Interface
End Namespace
