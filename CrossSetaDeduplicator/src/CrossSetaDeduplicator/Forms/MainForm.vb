Imports System.IO
Imports CrossSetaDeduplicator.Models
Imports CrossSetaDeduplicator.Services
Imports CrossSetaDeduplicator.DataAccess

Public Class MainForm
    Inherits Form

    Private _dedupService As DeduplicationService
    Private _dbHelper As DatabaseHelper

    Public Sub New()
        InitializeComponent()
        _dedupService = New DeduplicationService()
        _dbHelper = New DatabaseHelper(Nothing)
    End Sub

    ' Event Handler for "Check for Duplicates" button
    Private Sub btnCheck_Click(sender As Object, e As EventArgs) Handles btnCheck.Click
        Dim learner As New LearnerModel() With {
            .NationalID = txtNationalID.Text,
            .FirstName = txtFirstName.Text,
            .LastName = txtLastName.Text,
            .DateOfBirth = dtpDOB.Value,
            .Gender = cmbGender.SelectedItem.ToString()
        }

        Dim result = _dedupService.CheckForDuplicates(learner)

        If result.IsDuplicate Then
            MessageBox.Show(result.Message, "Duplicate Found", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            ' Display in Grid
            Dim list = New List(Of Object)
            list.Add(New With {
                .Match = "YES",
                .Type = result.MatchType,
                .Score = result.MatchScore,
                .ExistingID = result.MatchedLearner.NationalID,
                .ExistingName = result.MatchedLearner.FirstName & " " & result.MatchedLearner.LastName
            })
            dgvResults.DataSource = list
        Else
            MessageBox.Show("No duplicates found. You can proceed to save.", "Clear", MessageBoxButtons.OK, MessageBoxIcon.Information)
            dgvResults.DataSource = Nothing
        End If
    End Sub

    ' Event Handler for "Save" button
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        ' Basic validation
        If String.IsNullOrWhiteSpace(txtNationalID.Text) Then
            MessageBox.Show("National ID is required.")
            Return
        End If

        Dim learner As New LearnerModel() With {
            .NationalID = txtNationalID.Text,
            .FirstName = txtFirstName.Text,
            .LastName = txtLastName.Text,
            .DateOfBirth = dtpDOB.Value,
            .Gender = cmbGender.SelectedItem.ToString(),
            .IsVerified = True ' Assume verified if manually entered and checked
        }

        Try
            _dbHelper.InsertLearner(learner)
            MessageBox.Show("Learner saved successfully.")
            ClearForm()
        Catch ex As Exception
            MessageBox.Show("Error saving learner: " & ex.Message)
        End Try
    End Sub

    Private Sub ClearForm()
        txtNationalID.Clear()
        txtFirstName.Clear()
        txtLastName.Clear()
        dgvResults.DataSource = Nothing
    End Sub

    ' --- Phase 3: Bulk ID Verification ---
    Private Sub btnBulkUpload_Click(sender As Object, e As EventArgs) Handles btnBulkUpload.Click
        Using openFileDialog As New OpenFileDialog()
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv"
            If openFileDialog.ShowDialog() = DialogResult.OK Then
                ProcessBulkFile(openFileDialog.FileName)
            End If
        End Using
    End Sub

    Private Sub ProcessBulkFile(filePath As String)
        Dim lines = File.ReadAllLines(filePath)
        Dim report As New List(Of Object)

        ' Skip header if exists (assume first row is header)
        For i As Integer = 1 To lines.Length - 1
            Dim line = lines(i)
            Dim parts = line.Split(","c)
            If parts.Length >= 3 Then
                Dim learner As New LearnerModel() With {
                    .NationalID = parts(0).Trim(),
                    .FirstName = parts(1).Trim(),
                    .LastName = parts(2).Trim()
                    ' .DateOfBirth = ... parse if available
                }

                ' 1. Simulate KYC Check
                Dim isKycVerified = SimulateKYC(learner.NationalID)
                
                ' 2. Run Deduplication
                Dim dupResult = _dedupService.CheckForDuplicates(learner)
                
                Dim status As String = "Verified"
                If Not isKycVerified Then
                    status = "KYC Failed"
                ElseIf dupResult.IsDuplicate Then
                    status = "Duplicate: " & dupResult.MatchType
                Else
                    ' If valid, auto-save? Or just report? Prompt says "Generate a final report".
                    ' We can optionally save valid ones.
                    ' _dbHelper.InsertLearner(learner) 
                End If

                report.Add(New With {
                    .NationalID = learner.NationalID,
                    .Name = learner.FirstName & " " & learner.LastName,
                    .KYC = If(isKycVerified, "Pass", "Fail"),
                    .Dedupe = If(dupResult.IsDuplicate, "Fail", "Pass"),
                    .FinalStatus = status
                })
            End If
        Next

        dgvResults.DataSource = report
        MessageBox.Show("Bulk processing complete.")
    End Sub

    Private Function SimulateKYC(nationalId As String) As Boolean
        ' Logic: Returns True if ID length is valid (e.g., > 5) for simulation
        Return nationalId.Length > 5
    End Function

End Class
