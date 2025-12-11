Imports System.IO
Imports CrossSetaDeduplicator.Models
Imports CrossSetaDeduplicator.Services

Public Class BulkVerification
    Inherits Form

    Private btnBrowse As Button
    Private lblFile As Label
    Private dgvData As DataGridView
    Private btnProcess As Button
    Private progressBar As ProgressBar
    Private btnDownload As Button
    
    Private _dedupService As New DeduplicationService()
    Private _demoMode As New DemoMode()
    Private _loadedFilePath As String

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "Bulk ID Verification"
        Me.Size = New Size(800, 600)
        Me.StartPosition = FormStartPosition.CenterParent

        Dim topPanel As New Panel() With {.Dock = DockStyle.Top, .Height = 60}
        btnBrowse = New Button() With {.Text = "Browse CSV...", .Location = New Point(20, 15), .Width = 120}
        lblFile = New Label() With {.Text = "No file selected", .Location = New Point(150, 20), .AutoSize = True}
        topPanel.Controls.AddRange({btnBrowse, lblFile})

        Dim bottomPanel As New Panel() With {.Dock = DockStyle.Bottom, .Height = 80}
        btnProcess = New Button() With {.Text = "Process Batch", .Location = New Point(20, 20), .Width = 120, .Enabled = False}
        progressBar = New ProgressBar() With {.Location = New Point(150, 20), .Width = 400, .Height = 23}
        btnDownload = New Button() With {.Text = "Download Report", .Location = New Point(570, 20), .Width = 120, .Enabled = False}
        bottomPanel.Controls.AddRange({btnProcess, progressBar, btnDownload})

        dgvData = New DataGridView() With {.Dock = DockStyle.Fill, .AllowUserToAddRows = False}

        Me.Controls.Add(dgvData)
        Me.Controls.Add(topPanel)
        Me.Controls.Add(bottomPanel)

        AddHandler btnBrowse.Click, AddressOf BtnBrowse_Click
        AddHandler btnProcess.Click, AddressOf BtnProcess_Click
        AddHandler btnDownload.Click, AddressOf BtnDownload_Click
    End Sub

    Private Sub BtnBrowse_Click(sender As Object, e As EventArgs)
        Using ofd As New OpenFileDialog()
            ofd.Filter = "CSV Files (*.csv)|*.csv"
            If ofd.ShowDialog() = DialogResult.OK Then
                _loadedFilePath = ofd.FileName
                lblFile.Text = Path.GetFileName(_loadedFilePath)
                LoadCsvPreview(_loadedFilePath)
                btnProcess.Enabled = True
            End If
        End Using
    End Sub

    Private Sub LoadCsvPreview(filePath As String)
        Dim dt As New DataTable()
        dt.Columns.Add("NationalID")
        dt.Columns.Add("FirstName")
        dt.Columns.Add("LastName")
        dt.Columns.Add("KYC_Status")
        dt.Columns.Add("Dedupe_Status")
        dt.Columns.Add("Final_Result")

        Dim lines = File.ReadAllLines(filePath)
        ' Skip header if present (simple check)
        Dim startIdx = If(lines(0).Contains("NationalID"), 1, 0)

        For i As Integer = startIdx To lines.Length - 1
            Dim parts = lines(i).Split(","c)
            If parts.Length >= 3 Then
                dt.Rows.Add(parts(0).Trim(), parts(1).Trim(), parts(2).Trim(), "Pending", "Pending", "Pending")
            End If
        Next
        dgvData.DataSource = dt
    End Sub

    Private Sub BtnProcess_Click(sender As Object, e As EventArgs)
        btnProcess.Enabled = False
        progressBar.Value = 0
        progressBar.Maximum = dgvData.Rows.Count
        
        Dim dt As DataTable = CType(dgvData.DataSource, DataTable)

        For Each row As DataRow In dt.Rows
            Dim id = row("NationalID").ToString()
            Dim fname = row("FirstName").ToString()
            Dim lname = row("LastName").ToString()

            ' 1. KYC Check
            Dim kycRes = _demoMode.SimulateKYC(id)
            row("KYC_Status") = kycRes

            ' 2. Dedupe Check
            Dim learner As New LearnerModel() With {
                .NationalID = id, .FirstName = fname, .LastName = lname
            }
            Dim dedupRes = _dedupService.CheckForDuplicates(learner)
            
            If dedupRes.IsDuplicate Then
                row("Dedupe_Status") = $"Duplicate ({dedupRes.MatchType})"
            Else
                row("Dedupe_Status") = "Passed"
            End If

            ' 3. Final Result & Coloring logic (handled in RowPrePaint usually, but we set text here)
            If kycRes = "Verification Successful" AndAlso Not dedupRes.IsDuplicate Then
                row("Final_Result") = "Verified"
            ElseIf dedupRes.IsDuplicate Then
                row("Final_Result") = "Duplicate"
            Else
                row("Final_Result") = "Invalid"
            End If

            progressBar.Value += 1
            Application.DoEvents() ' Keep UI responsive
            System.Threading.Thread.Sleep(50) ' Artificial delay for visual effect
        Next

        ' Update colors
        For Each r As DataGridViewRow In dgvData.Rows
            Dim res = r.Cells("Final_Result").Value.ToString()
            If res = "Verified" Then
                r.DefaultCellStyle.BackColor = Color.LightGreen
            ElseIf res = "Duplicate" Then
                r.DefaultCellStyle.BackColor = Color.LightCoral
            Else
                r.DefaultCellStyle.BackColor = Color.Orange
            End If
        Next
        
        btnDownload.Enabled = True
        MessageBox.Show("Batch processing complete.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub BtnDownload_Click(sender As Object, e As EventArgs)
        Using sfd As New SaveFileDialog()
            sfd.Filter = "CSV Files|*.csv"
            sfd.FileName = "VerificationReport.csv"
            If sfd.ShowDialog() = DialogResult.OK Then
                Dim sb As New Text.StringBuilder()
                sb.AppendLine("NationalID,FirstName,LastName,KYC_Status,Dedupe_Status,Final_Result")
                
                For Each row As DataGridViewRow In dgvData.Rows
                     Dim cells = row.Cells
                     sb.AppendLine($"{cells(0).Value},{cells(1).Value},{cells(2).Value},{cells(3).Value},{cells(4).Value},{cells(5).Value}")
                Next
                
                File.WriteAllText(sfd.FileName, sb.ToString())
                MessageBox.Show("Report saved.")
            End If
        End Using
    End Sub

End Class
