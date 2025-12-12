Imports CrossSetaDeduplicator.Models
Imports CrossSetaDeduplicator.Services
Imports System.Drawing

Public Class UserRegistrationForm
    Inherits Form

    Private _userService As New UserService()

    ' UI Controls
    Private txtIDType As ComboBox
    Private txtNationalID As TextBox
    Private txtTitle As ComboBox
    Private txtFirstName As TextBox
    Private txtLastName As TextBox
    Private txtEmail As TextBox
    Private txtConfirmEmail As TextBox
    Private txtProvince As ComboBox
    
    Private txtUserName As TextBox
    Private txtPassword As TextBox
    Private txtConfirmPassword As TextBox
    
    Private txtSecurityQuestion As TextBox
    Private txtSecurityAnswer As TextBox
    
    Private btnRegister As Button
    Private btnBack As Button

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "Register User"
        Me.Size = New Size(900, 600)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.AutoScroll = True
        Me.BackColor = Color.White

        Dim mainFont As New Font("Segoe UI", 9)
        Dim headerFont As New Font("Segoe UI", 12, FontStyle.Bold)
        Dim sectionHeaderColor As Color = Color.SeaGreen
        Dim sectionHeaderTextColor As Color = Color.White

        ' --- Header ---
        Dim lblHeader As New Label() With {
            .Text = "Register User",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .ForeColor = Color.SeaGreen,
            .Location = New Point(20, 20),
            .AutoSize = True
        }
        Me.Controls.Add(lblHeader)

        ' --- Personal Information Section ---
        Dim pnlPersonal As New Panel() With {
            .Location = New Point(20, 60),
            .Size = New Size(840, 180),
            .BorderStyle = BorderStyle.None
        }
        
        Dim lblPersonalHeader As New Label() With {
            .Text = "Personal Information",
            .BackColor = sectionHeaderColor,
            .ForeColor = sectionHeaderTextColor,
            .Dock = DockStyle.Top,
            .Height = 30,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = headerFont,
            .Padding = New Padding(5, 0, 0, 0)
        }
        pnlPersonal.Controls.Add(lblPersonalHeader)

        ' Row 1
        Dim yPos As Integer = 40
        AddLabelAndControl(pnlPersonal, "Select ID Type*", New ComboBox() With {.Name = "cmbIDType", .DataSource = {"South African ID", "Passport"}}, 20, yPos)
        txtIDType = pnlPersonal.Controls("cmbIDType")
        
        AddLabelAndControl(pnlPersonal, "Enter ID Number*", New TextBox() With {.Name = "txtIDNum"}, 220, yPos)
        txtNationalID = pnlPersonal.Controls("txtIDNum")

        AddLabelAndControl(pnlPersonal, "Select Title*", New ComboBox() With {.Name = "cmbTitle", .DataSource = {"Mr", "Mrs", "Ms", "Dr", "Prof"}}, 420, yPos)
        txtTitle = pnlPersonal.Controls("cmbTitle")

        AddLabelAndControl(pnlPersonal, "Enter First Name*", New TextBox() With {.Name = "txtFName"}, 620, yPos)
        txtFirstName = pnlPersonal.Controls("txtFName")

        ' Row 2
        yPos += 60
        AddLabelAndControl(pnlPersonal, "Enter Last Name*", New TextBox() With {.Name = "txtLName"}, 20, yPos)
        txtLastName = pnlPersonal.Controls("txtLName")

        AddLabelAndControl(pnlPersonal, "Enter Email*", New TextBox() With {.Name = "txtEmail"}, 220, yPos)
        txtEmail = pnlPersonal.Controls("txtEmail")

        AddLabelAndControl(pnlPersonal, "Confirm Email*", New TextBox() With {.Name = "txtConfEmail"}, 420, yPos)
        txtConfirmEmail = pnlPersonal.Controls("txtConfEmail")

        AddLabelAndControl(pnlPersonal, "Enter Province*", New ComboBox() With {.Name = "cmbProv", .DataSource = {"Gauteng", "Western Cape", "KwaZulu-Natal", "Eastern Cape", "Free State", "Limpopo", "Mpumalanga", "North West", "Northern Cape"}}, 620, yPos)
        txtProvince = pnlPersonal.Controls("cmbProv")

        Me.Controls.Add(pnlPersonal)

        ' --- Login Credentials Section ---
        Dim pnlLogin As New Panel() With {
            .Location = New Point(20, 250),
            .Size = New Size(840, 100),
            .BorderStyle = BorderStyle.None
        }
        Dim lblLoginHeader As New Label() With {
            .Text = "Login Credentials",
            .BackColor = sectionHeaderColor,
            .ForeColor = sectionHeaderTextColor,
            .Dock = DockStyle.Top,
            .Height = 30,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = headerFont,
            .Padding = New Padding(5, 0, 0, 0)
        }
        pnlLogin.Controls.Add(lblLoginHeader)

        yPos = 40
        AddLabelAndControl(pnlLogin, "Enter User Name*", New TextBox() With {.Name = "txtUName"}, 20, yPos)
        txtUserName = pnlLogin.Controls("txtUName")

        AddLabelAndControl(pnlLogin, "Enter Password*", New TextBox() With {.Name = "txtPass", .PasswordChar = "*"c}, 320, yPos)
        txtPassword = pnlLogin.Controls("txtPass")

        AddLabelAndControl(pnlLogin, "Confirm Password*", New TextBox() With {.Name = "txtConfPass", .PasswordChar = "*"c}, 620, yPos)
        txtConfirmPassword = pnlLogin.Controls("txtConfPass")

        Me.Controls.Add(pnlLogin)

        ' --- Security Question Section ---
        Dim pnlSec As New Panel() With {
            .Location = New Point(20, 360),
            .Size = New Size(840, 100),
            .BorderStyle = BorderStyle.None
        }
        Dim lblSecHeader As New Label() With {
            .Text = "Login Security Question",
            .BackColor = sectionHeaderColor,
            .ForeColor = sectionHeaderTextColor,
            .Dock = DockStyle.Top,
            .Height = 30,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = headerFont,
            .Padding = New Padding(5, 0, 0, 0)
        }
        pnlSec.Controls.Add(lblSecHeader)

        yPos = 40
        AddLabelAndControl(pnlSec, "Question*", New TextBox() With {.Name = "txtQuest", .Width = 400}, 20, yPos)
        txtSecurityQuestion = pnlSec.Controls("txtQuest")

        AddLabelAndControl(pnlSec, "Answer*", New TextBox() With {.Name = "txtAns", .Width = 380}, 440, yPos)
        txtSecurityAnswer = pnlSec.Controls("txtAns")

        Me.Controls.Add(pnlSec)

        ' --- Buttons ---
        btnRegister = New Button() With {
            .Text = "Register",
            .Location = New Point(20, 480),
            .Size = New Size(200, 40),
            .BackColor = sectionHeaderColor,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = headerFont
        }
        AddHandler btnRegister.Click, AddressOf BtnRegister_Click
        Me.Controls.Add(btnRegister)

        btnBack = New Button() With {
            .Text = "Back",
            .Location = New Point(240, 480),
            .Size = New Size(200, 40),
            .BackColor = Color.Gray,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = headerFont
        }
        AddHandler btnBack.Click, AddressOf BtnBack_Click
        Me.Controls.Add(btnBack)
    End Sub

    Private Sub AddLabelAndControl(parent As Control, labelText As String, ctrl As Control, x As Integer, y As Integer)
        Dim lbl As New Label() With {
            .Text = labelText,
            .Location = New Point(x, y),
            .AutoSize = True,
            .Font = New Font("Segoe UI", 9)
        }
        parent.Controls.Add(lbl)

        ctrl.Location = New Point(x, y + 20)
        If ctrl.Width = 100 Then ctrl.Width = 180 ' Default width if not set
        ctrl.Font = New Font("Segoe UI", 10)
        parent.Controls.Add(ctrl)
    End Sub

    Private Sub BtnRegister_Click(sender As Object, e As EventArgs)
        ' Validation Logic
        If txtPassword.Text <> txtConfirmPassword.Text Then
            MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If txtEmail.Text <> txtConfirmEmail.Text Then
            MessageBox.Show("Emails do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        
        If String.IsNullOrWhiteSpace(txtUserName.Text) OrElse String.IsNullOrWhiteSpace(txtPassword.Text) Then
             MessageBox.Show("Username and Password are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
             Return
        End If

        Dim user As New UserModel() With {
            .IDType = If(txtIDType.SelectedItem IsNot Nothing, txtIDType.SelectedItem.ToString(), ""),
            .NationalID = txtNationalID.Text,
            .Title = If(txtTitle.SelectedItem IsNot Nothing, txtTitle.SelectedItem.ToString(), ""),
            .FirstName = txtFirstName.Text,
            .LastName = txtLastName.Text,
            .Email = txtEmail.Text,
            .Province = If(txtProvince.SelectedItem IsNot Nothing, txtProvince.SelectedItem.ToString(), ""),
            .UserName = txtUserName.Text,
            .SecurityQuestion = txtSecurityQuestion.Text,
            .SecurityAnswer = txtSecurityAnswer.Text
        }

        Try
            _userService.RegisterUser(user, txtPassword.Text)
            MessageBox.Show("User Registered Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.Close()
        Catch ex As Exception
            MessageBox.Show("Error registering user: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnBack_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub
End Class
