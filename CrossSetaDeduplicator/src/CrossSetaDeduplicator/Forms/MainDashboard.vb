Imports CrossSetaDeduplicator.Services
Imports CrossSetaDeduplicator.DataAccess

Public Class MainDashboard
    Inherits Form

    Private btnNewLearner As Button
    Private btnBulk As Button
    Private btnExit As Button
    
    Private lblTotalChecks As Label
    Private lblDuplicatesFound As Label
    Private lstActivityLog As ListBox
    
    Private chkDemoNarrative As CheckBox
    Private _demoMode As New DemoMode()
    Private _dbHelper As New DatabaseHelper(Nothing)

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "Cross-SETA Deduplicator - Dashboard"
        Me.Size = New Size(900, 600)
        Me.WindowState = FormWindowState.Maximized
        Me.IsMdiContainer = False

        ' --- Sidebar ---
        Dim pnlSidebar As New Panel() With {
            .Dock = DockStyle.Left,
            .Width = 220,
            .BackColor = Color.FromArgb(40, 40, 40)
        }
        
        Dim lblBrand As New Label() With {
            .Text = "MLX Ventures" & vbCrLf & "ID Verify",
            .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .Location = New Point(20, 20),
            .AutoSize = True
        }
        
        btnNewLearner = CreateSidebarButton("New Learner Check", 100)
        btnBulk = CreateSidebarButton("Bulk Upload", 160)
        btnExit = CreateSidebarButton("Exit", 500)
        
        chkDemoNarrative = New CheckBox() With {
            .Text = "Live Demo Mode",
            .ForeColor = Color.Yellow,
            .Location = New Point(20, 450),
            .AutoSize = True,
            .Font = New Font("Segoe UI", 10)
        }
        
        pnlSidebar.Controls.AddRange({lblBrand, btnNewLearner, btnBulk, chkDemoNarrative, btnExit})
        
        ' --- Main Content ---
        Dim pnlContent As New Panel() With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.WhiteSmoke
        }
        
        ' Status Panel
        Dim pnlStats As New Panel() With {
            .Dock = DockStyle.Top,
            .Height = 150,
            .Padding = New Padding(20)
        }
        
        Dim card1 = CreateStatCard("Total Checks", "1,245", Color.CornflowerBlue, 20)
        Dim card2 = CreateStatCard("Duplicates Found", "12", Color.IndianRed, 240)
        Dim card3 = CreateStatCard("System Status", "Online", Color.MediumSeaGreen, 460)
        
        pnlStats.Controls.AddRange({card1, card2, card3})
        
        ' Activity Log
        Dim grpLog As New GroupBox() With {
            .Text = "System Activity Log",
            .Dock = DockStyle.Bottom,
            .Height = 200,
            .Padding = New Padding(10)
        }
        
        lstActivityLog = New ListBox() With {.Dock = DockStyle.Fill, .BorderStyle = BorderStyle.None}
        lstActivityLog.Items.Add(DateTime.Now & " - System Initialized.")
        lstActivityLog.Items.Add(DateTime.Now & " - Connected to SQL Server 2019.")
        
        grpLog.Controls.Add(lstActivityLog)
        
        ' Seed Data Button (Hidden/Small)
        Dim btnSeed As New Button() With {.Text = "Seed Data", .Location = New Point(700, 20)}
        AddHandler btnSeed.Click, Sub() 
            Try
                _demoMode.SeedDatabase()
                LogActivity("Database seeded with 50 demo records.")
                MessageBox.Show("Data Seeded.")
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End Sub
        pnlStats.Controls.Add(btnSeed)

        pnlContent.Controls.Add(grpLog)
        pnlContent.Controls.Add(pnlStats)

        Me.Controls.Add(pnlContent)
        Me.Controls.Add(pnlSidebar)

        ' Events
        AddHandler btnNewLearner.Click, AddressOf BtnNewLearner_Click
        AddHandler btnBulk.Click, AddressOf BtnBulk_Click
        AddHandler btnExit.Click, Sub() Application.Exit()
    End Sub

    Private Function CreateSidebarButton(text As String, y As Integer) As Button
        Dim btn As New Button() With {
            .Text = text,
            .Location = New Point(10, y),
            .Size = New Size(200, 40),
            .FlatStyle = FlatStyle.Flat,
            .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 11),
            .TextAlign = ContentAlignment.MiddleLeft,
            .Padding = New Padding(10, 0, 0, 0)
        }
        btn.FlatAppearance.BorderSize = 0
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60)
        Return btn
    End Function

    Private Function CreateStatCard(title As String, value As String, color As Color, x As Integer) As Panel
        Dim pnl As New Panel() With {
            .Location = New Point(x, 20),
            .Size = New Size(200, 100),
            .BackColor = color
        }
        Dim lblTitle As New Label() With {
            .Text = title,
            .ForeColor = Color.White,
            .Location = New Point(10, 10),
            .AutoSize = True,
            .Font = New Font("Segoe UI", 10)
        }
        Dim lblValue As New Label() With {
            .Text = value,
            .ForeColor = Color.White,
            .Location = New Point(10, 40),
            .AutoSize = True,
            .Font = New Font("Segoe UI", 20, FontStyle.Bold)
        }
        pnl.Controls.AddRange({lblTitle, lblValue})
        Return pnl
    End Function

    Private Sub LogActivity(msg As String)
        lstActivityLog.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} - {msg}")
    End Sub

    Private Sub BtnNewLearner_Click(sender As Object, e As EventArgs)
        LogActivity("Opening New Learner Wizard...")
        Dim wizard As New RegistrationWizard()
        wizard.IsDemoNarrative = chkDemoNarrative.Checked
        wizard.ShowDialog()
        LogActivity("Learner Registration workflow completed.")
    End Sub

    Private Sub BtnBulk_Click(sender As Object, e As EventArgs)
        LogActivity("Opening Bulk Verification...")
        Dim bulkForm As New BulkVerification()
        bulkForm.ShowDialog()
        LogActivity("Bulk Verification closed.")
    End Sub

End Class
