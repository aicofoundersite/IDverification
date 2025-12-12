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
    Private _homeAffairsService As New HomeAffairsService()
    Private _learnerService As New LearnerService()
    
    Public Property IsDemoNarrative As Boolean = False

    ' UI Controls
    Private pnlStep1 As Panel
    Private pnlStep2 As Panel
    Private pnlStep3 As Panel
    
    Private txtNationalID As MaskedTextBox
    Private lblKycStatus As Label
    Private btnVerifyKyc As Button
    Private btnScanDoc As Button
    Private btnCaptureSelfie As Button
    Private btnVerifyFace As Button
    Private lblFaceMatchStatus As Label
    Private btnNext1 As Button
    Private chkConsent As CheckBox
    Private chkOffline As CheckBox

    Private _idDocumentPath As String
    Private _selfiePath As String
    
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
    
    ' Status Bar
    Private statusStrip As StatusStrip
    Private lblSystemStatus As ToolStripStatusLabel

    ' Narrative Controls
    Private lblNarrativeTooltip As Label

    Public Sub New()
        InitializeComponent()
        UpdateSystemStatus()
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

        ' Status Strip
        statusStrip = New StatusStrip()
        lblSystemStatus = New ToolStripStatusLabel() With {.Text = "System Status: Online", .ForeColor = Color.Green, .Font = New Font("Segoe UI", 9, FontStyle.Bold)}
        statusStrip.Items.Add(lblSystemStatus)
        Me.Controls.Add(statusStrip)

        ' --- Step 1 Panel ---
        pnlStep1 = New Panel() With {.Dock = DockStyle.Fill, .Visible = True}
        Dim lblTitle1 As New Label() With {.Text = "Step 1: Identity Verification", .Font = titleFont, .Location = New Point(20, 20), .AutoSize = True}
        
        Dim lblIdPrompt As New Label() With {.Text = "National ID Number:", .Location = New Point(20, 60), .AutoSize = True}
        txtNationalID = New MaskedTextBox() With {.Location = New Point(20, 80), .Width = 250, .Mask = "0000000000000", .PromptChar = "_"c}
        
        Dim lblFn As New Label() With {.Text = "First Name:", .Location = New Point(20, 110)}
        txtFirstName = New TextBox() With {.Location = New Point(20, 130), .Width = 250}
        
        Dim lblLn As New Label() With {.Text = "Last Name:", .Location = New Point(20, 160)}
        txtLastName = New TextBox() With {.Location = New Point(20, 180), .Width = 250}

        ' Document & Selfie Buttons
        btnScanDoc = New Button() With {.Text = "📷 Scan Document", .Location = New Point(300, 80), .Width = 140, .BackColor = Color.LightBlue}
        btnCaptureSelfie = New Button() With {.Text = "👤 Capture Selfie", .Location = New Point(450, 80), .Width = 140, .BackColor = Color.LightBlue}
        
        ' Verification Buttons
        btnVerifyKyc = New Button() With {.Text = "Verify Identity (HA)", .Location = New Point(300, 130), .Width = 140}
        btnVerifyFace = New Button() With {.Text = "Biometric Match", .Location = New Point(450, 130), .Width = 140}

        chkConsent = New CheckBox() With {.Text = "I consent to ID Verification (POPIA)", .Location = New Point(20, 220), .AutoSize = True}
        chkOffline = New CheckBox() With {.Text = "Simulate Offline", .Location = New Point(300, 220), .AutoSize = True, .ForeColor = Color.DarkGray}
        
        lblKycStatus = New Label() With {.Text = "", .Location = New Point(20, 250), .AutoSize = True, .Font = New Font("Segoe UI", 10, FontStyle.Bold)}
        lblFaceMatchStatus = New Label() With {.Text = "", .Location = New Point(300, 250), .AutoSize = True, .Font = New Font("Segoe UI", 10, FontStyle.Bold)}

        btnNext1 = New Button() With {.Text = "Next >", .Location = New Point(450, 400), .Enabled = False}

        pnlStep1.Controls.AddRange({lblTitle1, lblIdPrompt, txtNationalID, lblFn, txtFirstName, lblLn, txtLastName, btnScanDoc, btnCaptureSelfie, btnVerifyKyc, btnVerifyFace, chkConsent, chkOffline, lblKycStatus, lblFaceMatchStatus, btnNext1})
        Me.Controls.Add(pnlStep1)

        ' --- Step 2 Panel ---
        pnlStep2 = New Panel() With {.Dock = DockStyle.Fill, .Visible = False}
        Dim lblTitle2 As New Label() With {.Text = "Step 2: Learner Details", .Font = titleFont, .Location = New Point(20, 50), .AutoSize = True}
        
        ' Moved Name/Surname to Step 1
        
        Dim lblDob As New Label() With {.Text = "Date of Birth:", .Location = New Point(20, 100)}
        dtpDOB = New DateTimePicker() With {.Location = New Point(150, 100), .Width = 200}
        
        Dim lblGen As New Label() With {.Text = "Gender:", .Location = New Point(20, 140)}
        cmbGender = New ComboBox() With {.Location = New Point(150, 140), .Width = 200}
        cmbGender.Items.AddRange({"Male", "Female", "Other"})

        Dim lblRole As New Label() With {.Text = "Role:", .Location = New Point(20, 180)}
        cmbRole = New ComboBox() With {.Location = New Point(150, 180), .Width = 200}
        cmbRole.Items.AddRange({"Learner", "Assessor", "Moderator"})
        cmbRole.SelectedIndex = 0

        btnBack2 = New Button() With {.Text = "< Back", .Location = New Point(20, 400)}
        btnNext2 = New Button() With {.Text = "Check Duplicates >", .Location = New Point(420, 400), .Width = 140}

        pnlStep2.Controls.AddRange({lblTitle2, lblDob, dtpDOB, lblGen, cmbGender, lblRole, cmbRole, btnBack2, btnNext2})
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
        AddHandler btnCaptureSelfie.Click, AddressOf BtnCaptureSelfie_Click
        AddHandler btnVerifyKyc.Click, AddressOf BtnVerifyKyc_Click
        AddHandler btnVerifyFace.Click, AddressOf BtnVerifyFace_Click
        AddHandler btnNext1.Click, Sub() ShowStep(2)
        AddHandler btnBack2.Click, Sub() ShowStep(1)
        AddHandler btnNext2.Click, AddressOf BtnValidateDetails_Click
        AddHandler btnBack3.Click, Sub() ShowStep(2)
        AddHandler btnFinish.Click, AddressOf BtnFinish_Click
        AddHandler chkOffline.CheckedChanged, AddressOf UpdateSystemStatus
    End Sub
    
    Private Sub UpdateSystemStatus(Optional sender As Object = Nothing, Optional e As EventArgs = Nothing)
        If chkOffline.Checked Then
            lblSystemStatus.Text = "System Status: OFFLINE (Queueing Mode)"
            lblSystemStatus.ForeColor = Color.Red
            statusStrip.BackColor = Color.MistyRose
        Else
            lblSystemStatus.Text = "System Status: ONLINE (Connected to Home Affairs)"
            lblSystemStatus.ForeColor = Color.Green
            statusStrip.BackColor = SystemColors.Control
        End If
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
                _idDocumentPath = openFileDialog.FileName
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

                    lblKycStatus.Text = $"Document Verified: {result.DocumentType}"
                    lblKycStatus.ForeColor = Color.Green
                Else
                    lblKycStatus.Text = "Error: " & result.ErrorMessage
                    lblKycStatus.ForeColor = Color.Red
                End If
            End If
        End Using
    End Sub

    Private Sub BtnCaptureSelfie_Click(sender As Object, e As EventArgs)
        Using openFileDialog As New OpenFileDialog()
            openFileDialog.Title = "Select Selfie / Capture Face"
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            
            If openFileDialog.ShowDialog() = DialogResult.OK Then
                _selfiePath = openFileDialog.FileName
                lblFaceMatchStatus.Text = "Selfie Loaded."
                lblFaceMatchStatus.ForeColor = Color.Blue
            End If
        End Using
    End Sub

    Private Sub BtnVerifyFace_Click(sender As Object, e As EventArgs)
        If String.IsNullOrEmpty(_idDocumentPath) Then
             MessageBox.Show("Please scan an ID Document first.", "Missing Document", MessageBoxButtons.OK, MessageBoxIcon.Warning)
             Return
        End If

        If String.IsNullOrEmpty(_selfiePath) Then
             MessageBox.Show("Please capture or upload a selfie first.", "Missing Selfie", MessageBoxButtons.OK, MessageBoxIcon.Warning)
             Return
        End If
        
        lblFaceMatchStatus.Text = "Comparing Faces..."
        lblFaceMatchStatus.ForeColor = Color.Orange
        Application.DoEvents()

        Dim result = _kycService.CompareFaces(_idDocumentPath, _selfiePath)
        
        lblFaceMatchStatus.Text = result.Message & $" ({result.ConfidenceScore}%)"
        
        If result.IsMatch Then
            lblFaceMatchStatus.ForeColor = Color.Green
        Else
            lblFaceMatchStatus.ForeColor = Color.Red
        End If
    End Sub

    Private Async Sub BtnVerifyKyc_Click(sender As Object, e As EventArgs)
        ' POPIA Consent Check
        If Not chkConsent.Checked Then
            MessageBox.Show("Please obtain consent from the learner before verifying their identity (POPIA Requirement).", "Consent Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Require Name/Surname for Traffic Light "Mismatch" check
        If String.IsNullOrWhiteSpace(txtFirstName.Text) OrElse String.IsNullOrWhiteSpace(txtLastName.Text) Then
             MessageBox.Show("Please enter First Name and Last Name to verify against Home Affairs.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Information)
             Return
        End If

        lblKycStatus.Text = "Verifying with Home Affairs..."
        lblKycStatus.ForeColor = Color.Orange
        Application.DoEvents()

        ' Set Offline Mode based on Checkbox
        _homeAffairsService.SimulateOffline = chkOffline.Checked

        ' Call Service Async
        Dim currentUser As String = If(Not String.IsNullOrEmpty(Environment.UserName), Environment.UserName, "UnknownUser")
        Dim result = Await _homeAffairsService.VerifyCitizenAsync(txtNationalID.Text, txtFirstName.Text, txtLastName.Text, currentUser)

        ' Update UI
        lblKycStatus.Text = result.Message
        lblKycStatus.ForeColor = result.TrafficLightColor
        
        If result.IsValid Then
             ' Allow to proceed if Valid (Green) or Mismatch (Yellow)
             btnNext1.Enabled = True
             
             If result.Status = "Mismatch" Then
                 MessageBox.Show(result.Message, "Surname Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Warning)
             End If
             
        ElseIf result.Status = "OfflineQueued" Then
             MessageBox.Show("System is offline. The request has been queued and will be processed automatically when connectivity is restored.", "Offline Mode", MessageBoxButtons.OK, MessageBoxIcon.Information)
             btnNext1.Enabled = True ' Allow capture in offline mode
        Else
             btnNext1.Enabled = False
        End If
    End Sub

    Private Sub BtnValidateDetails_Click(sender As Object, e As EventArgs)
        ' Input Validation: Name and Surname already checked in Step 1, but good to double check or check other fields
        BtnCheckDuplicates_Click(sender, e)
    End Sub

    Private Sub BtnCheckDuplicates_Click(sender As Object, e As EventArgs)
        _learner.NationalID = txtNationalID.Text
        _learner.FirstName = txtFirstName.Text
        _learner.LastName = txtLastName.Text
        _learner.DateOfBirth = dtpDOB.Value
        _learner.Gender = If(cmbGender.SelectedItem IsNot Nothing, cmbGender.SelectedItem.ToString(), "")
        _learner.Role = If(cmbRole.SelectedItem IsNot Nothing, cmbRole.SelectedItem.ToString(), "Learner")
        _learner.IsVerified = True

        ' Use Service for logic if needed, but currently logic is in DedupService
        ' _learnerService.CheckForDuplicates(_learner) could be a wrapper
        
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
                ' STRICT N-TIER: Use LearnerService
                _learnerService.RegisterLearner(_learner)
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
