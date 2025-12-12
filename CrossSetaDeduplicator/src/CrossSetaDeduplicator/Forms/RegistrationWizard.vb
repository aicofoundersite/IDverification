Imports CrossSetaDeduplicator.Models
Imports CrossSetaDeduplicator.Services
Imports CrossSetaDeduplicator.DataAccess

Public Class RegistrationWizard
    Inherits Form

    Private _currentStep As Integer = 1
    Private _learner As New LearnerModel()
    Private _demoMode As New DemoMode()
    Private _dedupService As New DeduplicationService()
    Private _kycService As New KYCService()
    Private _dbHelper As New DatabaseHelper(Nothing)
    
    Public Property IsDemoNarrative As Boolean = False

    ' UI Controls
    Private pnlStep1 As Panel
    Private pnlStep2 As Panel
    Private pnlStep3 As Panel
    
    Private txtNationalID As TextBox
    Private lblKycStatus As Label
    Private btnVerifyKyc As Button
    Private btnScanDoc As Button
    Private btnNext1 As Button

    Private txtFirstName As TextBox
    Private txtLastName As TextBox
    Private dtpDOB As DateTimePicker
    Private cmbGender As ComboBox
    Private cmbRole As ComboBox
    Private btnNext2 As Button
    Private btnBack2 As Button

    Private lblResultTitle As Label
    Private lblResultDetail As Label
    Private btnFinish As Button
    Private btnBack3 As Button
    Private dgvMatches As DataGridView
    
    ' Narrative Controls
    Private lblNarrativeTooltip As Label

    Public Sub New()
        InitializeComponent()
        ShowStep(1)
    End Sub
    
    Private Sub RegistrationWizard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If IsDemoNarrative Then
            StartNarrative()
        End If
    End Sub

    Private Sub StartNarrative()
        ' Auto-fill for the "Live Demo" story
        lblNarrativeTooltip.Visible = True
        lblNarrativeTooltip.Text = "Step 1: Enter National ID. Watch as we simulate a connection to Home Affairs."
        
        txtNationalID.Text = "9505055000081" ' The ID we seeded as "Thabo Molefe"
        
        Dim t As New Timer()
        t.Interval = 1500
        AddHandler t.Tick, Sub(s, args)
             t.Stop()
             btnVerifyKyc.PerformClick()
             lblNarrativeTooltip.Text = "KYC Verified! Proceeding to capture details..."
        End Sub
        t.Start()
    End Sub

    Private Sub InitializeComponent()
        Me.Size = New Size(600, 500)
        Me.Text = "New Learner Registration Wizard"
        Me.StartPosition = FormStartPosition.CenterParent

        ' Common styles
        Dim titleFont As New Font("Segoe UI", 14, FontStyle.Bold)
        Dim bodyFont As New Font("Segoe UI", 10)

        ' Narrative Tooltip
        lblNarrativeTooltip = New Label() With {
            .Dock = DockStyle.Top,
            .Height = 40,
            .BackColor = Color.LightYellow,
            .ForeColor = Color.DarkBlue,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Font = New Font("Segoe UI", 10, FontStyle.Italic),
            .Visible = False
        }
        Me.Controls.Add(lblNarrativeTooltip)

        ' --- Step 1 Panel ---
        pnlStep1 = New Panel() With {.Dock = DockStyle.Fill, .Visible = True}
        Dim lblTitle1 As New Label() With {.Text = "Step 1: Identity Verification", .Font = titleFont, .Location = New Point(20, 50), .AutoSize = True}
        Dim lblIdPrompt As New Label() With {.Text = "National ID Number:", .Location = New Point(20, 100), .AutoSize = True}
        txtNationalID = New TextBox() With {.Location = New Point(20, 125), .Width = 250}
        btnScanDoc = New Button() With {.Text = "📷 Scan Document", .Location = New Point(280, 80), .Width = 150, .BackColor = Color.LightBlue}
        btnVerifyKyc = New Button() With {.Text = "Verify ID (KYC)", .Location = New Point(280, 123), .Width = 150}
        lblKycStatus = New Label() With {.Text = "", .Location = New Point(20, 160), .AutoSize = True, .Font = New Font("Segoe UI", 10, FontStyle.Bold)}
        btnNext1 = New Button() With {.Text = "Next >", .Location = New Point(450, 400), .Enabled = False}

        pnlStep1.Controls.AddRange({lblTitle1, lblIdPrompt, txtNationalID, btnScanDoc, btnVerifyKyc, lblKycStatus, btnNext1})
        Me.Controls.Add(pnlStep1)

        ' --- Step 2 Panel ---
        pnlStep2 = New Panel() With {.Dock = DockStyle.Fill, .Visible = False}
        Dim lblTitle2 As New Label() With {.Text = "Step 2: Learner Details", .Font = titleFont, .Location = New Point(20, 50), .AutoSize = True}
        
        Dim lblFn As New Label() With {.Text = "First Name:", .Location = New Point(20, 100)}
        txtFirstName = New TextBox() With {.Location = New Point(150, 100), .Width = 200}
        
        Dim lblLn As New Label() With {.Text = "Last Name:", .Location = New Point(20, 140)}
        txtLastName = New TextBox() With {.Location = New Point(150, 140), .Width = 200}
        
        Dim lblDob As New Label() With {.Text = "Date of Birth:", .Location = New Point(20, 180)}
        dtpDOB = New DateTimePicker() With {.Location = New Point(150, 180), .Width = 200}
        
        Dim lblGen As New Label() With {.Text = "Gender:", .Location = New Point(20, 220)}
        cmbGender = New ComboBox() With {.Location = New Point(150, 220), .Width = 200}
        cmbGender.Items.AddRange({"Male", "Female", "Other"})

        Dim lblRole As New Label() With {.Text = "Role:", .Location = New Point(20, 260)}
        cmbRole = New ComboBox() With {.Location = New Point(150, 260), .Width = 200}
        cmbRole.Items.AddRange({"Learner", "Assessor", "Moderator"})
        cmbRole.SelectedIndex = 0

        btnBack2 = New Button() With {.Text = "< Back", .Location = New Point(20, 400)}
        btnNext2 = New Button() With {.Text = "Check Duplicates >", .Location = New Point(420, 400), .Width = 140}

        pnlStep2.Controls.AddRange({lblTitle2, lblFn, txtFirstName, lblLn, txtLastName, lblDob, dtpDOB, lblGen, cmbGender, lblRole, cmbRole, btnBack2, btnNext2})
        Me.Controls.Add(pnlStep2)

        ' --- Step 3 Panel ---
        pnlStep3 = New Panel() With {.Dock = DockStyle.Fill, .Visible = False}
        Dim lblTitle3 As New Label() With {.Text = "Step 3: Verification Results", .Font = titleFont, .Location = New Point(20, 50), .AutoSize = True}
        lblResultTitle = New Label() With {.Location = New Point(20, 100), .AutoSize = True, .Font = New Font("Segoe UI", 12, FontStyle.Bold)}
        lblResultDetail = New Label() With {.Location = New Point(20, 130), .AutoSize = True, .Width = 500}
        dgvMatches = New DataGridView() With {.Location = New Point(20, 160), .Size = New Size(540, 200), .ReadOnly = True}
        
        btnBack3 = New Button() With {.Text = "< Back", .Location = New Point(20, 400)}
        btnFinish = New Button() With {.Text = "Finish", .Location = New Point(450, 400)}

        pnlStep3.Controls.AddRange({lblTitle3, lblResultTitle, lblResultDetail, dgvMatches, btnBack3, btnFinish})
        Me.Controls.Add(pnlStep3)

        ' Events
        AddHandler btnScanDoc.Click, AddressOf BtnScanDoc_Click
        AddHandler btnVerifyKyc.Click, AddressOf BtnVerifyKyc_Click
        AddHandler btnNext1.Click, Sub() ShowStep(2)
        AddHandler btnBack2.Click, Sub() ShowStep(1)
        AddHandler btnNext2.Click, AddressOf BtnCheckDuplicates_Click
        AddHandler btnBack3.Click, Sub() ShowStep(2)
        AddHandler btnFinish.Click, AddressOf BtnFinish_Click
    End Sub

    Private Sub ShowStep(stepNum As Integer)
        _currentStep = stepNum
        pnlStep1.Visible = (stepNum = 1)
        pnlStep2.Visible = (stepNum = 2)
        pnlStep3.Visible = (stepNum = 3)
        
        ' Narrative progress
        If IsDemoNarrative Then
            If stepNum = 2 Then
                 lblNarrativeTooltip.Text = "Step 2: Confirming details pulled from Home Affairs (Simulated)..."
                 ' Auto fill details
                 txtFirstName.Text = "Thabo"
                 txtLastName.Text = "Molefe" ' Matches the seeded record
                 cmbGender.SelectedIndex = 0
            ElseIf stepNum = 3 Then
                 lblNarrativeTooltip.Text = "Step 3: Analyzing Cross-SETA database for duplicates..."
            End If
        End If
    End Sub

    Private Sub BtnScanDoc_Click(sender As Object, e As EventArgs)
        Using openFileDialog As New OpenFileDialog()
            openFileDialog.Title = "Select ID Document (Passport, ID Card, Driver's License)"
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            
            If openFileDialog.ShowDialog() = DialogResult.OK Then
                lblKycStatus.Text = "Scanning Document..."
                lblKycStatus.ForeColor = Color.Orange
                Application.DoEvents() ' Force UI update
                
                Dim result = _kycService.VerifyDocument(openFileDialog.FileName)
                
                If result.IsSuccess Then
                    ' Auto-populate Step 1 ID if found
                    If result.ExtractedFields.ContainsKey("NationalID") Then
                        txtNationalID.Text = result.ExtractedFields("NationalID")
                    End If
                    
                    ' Populate Step 2 Fields (Pre-fill logic)
                    If result.ExtractedFields.ContainsKey("FirstNames") Then txtFirstName.Text = result.ExtractedFields("FirstNames")
                    If result.ExtractedFields.ContainsKey("Surname") Then txtLastName.Text = result.ExtractedFields("Surname")
                    
                    ' Try parse DOB
                    If result.ExtractedFields.ContainsKey("DateOfBirth") Then
                         Dim dob As DateTime
                         If DateTime.TryParse(result.ExtractedFields("DateOfBirth"), dob) Then
                             dtpDOB.Value = dob
                         End If
                    End If
                    
                    ' Try parse Gender
                     If result.ExtractedFields.ContainsKey("Gender") Then
                         Dim g = result.ExtractedFields("Gender").ToLower()
                         If g.StartsWith("m") Then cmbGender.SelectedIndex = 0
                         If g.StartsWith("f") Then cmbGender.SelectedIndex = 1
                     End If
                    
                    lblKycStatus.Text = $"✅ Document Verified: {result.DocumentType} ({result.IssuingCountry})"
                    lblKycStatus.ForeColor = Color.Green
                    btnNext1.Enabled = True
                    
                    ' Trigger internal verify logic to set state
                    ' _demoMode.SimulateKYC(txtNationalID.Text) ' Optional, if we want to run the other checks
                Else
                    lblKycStatus.Text = "❌ Verification Failed"
                    lblKycStatus.ForeColor = Color.Red
                    MessageBox.Show($"Document Scan Failed: {result.ErrorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End If
        End Using
    End Sub

    Private Sub BtnVerifyKyc_Click(sender As Object, e As EventArgs)
        Dim status = _demoMode.SimulateKYC(txtNationalID.Text)
        lblKycStatus.Text = status
        
        If status = "Verification Successful" Then
            lblKycStatus.ForeColor = Color.Green
            btnNext1.Enabled = True
            
            ' In a real app, we might populate DOB/Gender from ID here
            ' For demo, we leave it for Step 2 or Narrative
        Else
            lblKycStatus.ForeColor = Color.Red
            btnNext1.Enabled = False
        End If
    End Sub

    Private Sub BtnCheckDuplicates_Click(sender As Object, e As EventArgs)
        _learner.NationalID = txtNationalID.Text
        _learner.FirstName = txtFirstName.Text
        _learner.LastName = txtLastName.Text
        _learner.DateOfBirth = dtpDOB.Value
        _learner.Gender = If(cmbGender.SelectedItem IsNot Nothing, cmbGender.SelectedItem.ToString(), "")
        _learner.Role = If(cmbRole.SelectedItem IsNot Nothing, cmbRole.SelectedItem.ToString(), "Learner")
        _learner.IsVerified = True

        Dim result = _dedupService.CheckForDuplicates(_learner)
        
        ShowStep(3)
        
        If result.IsDuplicate Then
            lblResultTitle.Text = "⚠️ Potential Duplicate Found"
            lblResultTitle.ForeColor = Color.Red
            lblResultDetail.Text = $"Match Type: {result.MatchType} (Score: {result.MatchScore}%). Record found in system."
            
            Dim list As New List(Of Object)
            list.Add(New With {
                .ID = result.MatchedLearner.NationalID,
                .Name = result.MatchedLearner.FirstName & " " & result.MatchedLearner.LastName,
                .Score = result.MatchScore,
                .Type = result.MatchType
            })
            dgvMatches.DataSource = list
            btnFinish.Text = "Cancel"
        Else
            lblResultTitle.Text = "✅ No Duplicates Found"
            lblResultTitle.ForeColor = Color.Green
            lblResultDetail.Text = "This learner is eligible for registration."
            dgvMatches.DataSource = Nothing
            btnFinish.Text = "Register Learner"
        End If
    End Sub

    Private Sub BtnFinish_Click(sender As Object, e As EventArgs)
        If btnFinish.Text = "Register Learner" Then
            Try
                _dbHelper.InsertLearner(_learner)
                MessageBox.Show("Learner Registered Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Me.Close()
            Catch ex As Exception
                MessageBox.Show("Error saving: " & ex.Message)
            End Try
        Else
            Me.Close()
        End If
    End Sub

End Class
