Imports CrossSetaDeduplicator.Models
Imports CrossSetaDeduplicator.Services
Imports CrossSetaDeduplicator.DataAccess
Imports System.Drawing

Public Class RegistrationWizard
    Inherits Form

    Private _learner As New LearnerModel()
    Private _dedupService As New DeduplicationService()
    Private _learnerService As New LearnerService()
    
    ' UI Controls - Biographic
    Private txtNationality As TextBox
    Private cmbTitle As ComboBox
    Private txtFirstName As TextBox
    Private txtMiddleName As TextBox
    Private txtLastName As TextBox
    Private txtNationalID As TextBox
    Private dtpDOB As DateTimePicker
    Private txtAge As TextBox
    Private cmbEquity As ComboBox
    Private cmbGender As ComboBox
    Private cmbHomeLang As ComboBox
    Private txtPrevLastName As TextBox
    Private cmbMunicipality As ComboBox
    Private cmbDisabilityStatus As ComboBox
    Private cmbCitizenStatus As ComboBox
    Private txtStatsAreaCode As TextBox
    Private cmbSocioEcon As ComboBox
    Private chkPopi As CheckBox
    Private dtpPopiDate As DateTimePicker

    ' UI Controls - Contact
    Private txtPhone As TextBox
    Private txtPOBox As TextBox
    Private txtCell As TextBox
    Private txtStreetName As TextBox
    Private txtPostalSuburb As TextBox
    Private txtStreetNo As TextBox
    Private txtPhysSuburb As TextBox
    Private txtCity As TextBox
    Private txtFax As TextBox
    Private txtPostalCode As TextBox
    Private txtEmail As TextBox
    Private cmbProvince As ComboBox
    Private chkResSamePostal As CheckBox
    Private cmbUrbanRural As ComboBox

    ' UI Controls - Disability
    Private txtDisComm As TextBox
    Private txtDisHearing As TextBox
    Private txtDisRemembering As TextBox
    Private txtDisSeeing As TextBox
    Private txtDisSelfCare As TextBox
    Private txtDisWalking As TextBox

    ' UI Controls - Education
    Private txtSchoolName As TextBox
    Private txtSchoolYear As TextBox

    Private btnSubmit As Button
    Private btnClose As Button

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "Learner Registration Form"
        Me.Size = New Size(1100, 800)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.White
        Me.AutoScroll = True

        Dim sectionHeaderColor As Color = Color.SeaGreen
        Dim sectionHeaderTextColor As Color = Color.White
        Dim mainFont As New Font("Segoe UI", 9)
        Dim boldFont As New Font("Segoe UI", 9, FontStyle.Bold)

        ' --- Top Header ---
        Dim pnlHeader As New Panel() With {
            .Dock = DockStyle.Top,
            .Height = 50,
            .BackColor = Color.DimGray
        }
        Dim lblTitle As New Label() With {
            .Text = "ADD NEW APPLICATION",
            .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .Location = New Point(10, 15),
            .AutoSize = True
        }
        pnlHeader.Controls.Add(lblTitle)

        btnClose = New Button() With {
            .Text = "Close Form",
            .BackColor = Color.IndianRed,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(800, 10),
            .Size = New Size(100, 30),
            .Anchor = AnchorStyles.Right Or AnchorStyles.Top
        }
        AddHandler btnClose.Click, AddressOf BtnClose_Click
        pnlHeader.Controls.Add(btnClose)

        btnSubmit = New Button() With {
            .Text = "Submit Application",
            .BackColor = Color.SeaGreen,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(910, 10),
            .Size = New Size(140, 30),
            .Anchor = AnchorStyles.Right Or AnchorStyles.Top
        }
        AddHandler btnSubmit.Click, AddressOf BtnSubmit_Click
        pnlHeader.Controls.Add(btnSubmit)

        Me.Controls.Add(pnlHeader)

        ' --- Scrollable Content Panel ---
        Dim pnlContent As New Panel() With {
            .Dock = DockStyle.Fill,
            .AutoScroll = True,
            .Padding = New Padding(10)
        }
        Me.Controls.Add(pnlContent)
        pnlContent.BringToFront()

        Dim currentY As Integer = 60

        ' === Biographic Details ===
        currentY = AddSectionHeader(pnlContent, "Biographic details", currentY)
        
        Dim pnlBio As New Panel() With {.Location = New Point(10, currentY), .Size = New Size(1050, 260), .BorderStyle = BorderStyle.None}
        pnlContent.Controls.Add(pnlBio)
        
        ' Row 1
        AddControl(pnlBio, "Nationality*", New TextBox(), txtNationality, 0, 0)
        AddControl(pnlBio, "Title*", New ComboBox() With {.DataSource = {"Mr", "Ms", "Mrs"}}, cmbTitle, 260, 0)
        AddControl(pnlBio, "First Name*", New TextBox(), txtFirstName, 520, 0)
        AddControl(pnlBio, "Middle Name", New TextBox(), txtMiddleName, 780, 0)

        ' Row 2
        AddControl(pnlBio, "Last Name*", New TextBox(), txtLastName, 0, 50)
        AddControl(pnlBio, "National ID/PassportNo*", New TextBox(), txtNationalID, 260, 50)
        AddControl(pnlBio, "Date Of Birth*", New DateTimePicker(), dtpDOB, 520, 50)
        AddHandler dtpDOB.ValueChanged, AddressOf dtpDOB_ValueChanged
        AddControl(pnlBio, "Age*", New TextBox() With {.ReadOnly = True}, txtAge, 780, 50)

        ' Row 3
        AddControl(pnlBio, "Equity Code*", New ComboBox() With {.DataSource = {"Black", "Coloured", "Indian", "White"}}, cmbEquity, 0, 100)
        AddControl(pnlBio, "Gender*", New ComboBox() With {.DataSource = {"Male", "Female"}}, cmbGender, 260, 100)
        AddControl(pnlBio, "Home Language*", New ComboBox() With {.DataSource = {"English", "Zulu", "Xhosa", "Afrikaans"}}, cmbHomeLang, 520, 100)
        AddControl(pnlBio, "Previous Last Name", New TextBox(), txtPrevLastName, 780, 100)

        ' Row 4
        AddControl(pnlBio, "Municipality/District*", New ComboBox() With {.DataSource = {"City of Joburg", "Ekurhuleni", "Tshwane"}}, cmbMunicipality, 0, 150)
        AddControl(pnlBio, "Disability Status*", New ComboBox() With {.DataSource = {"None", "Disabled"}}, cmbDisabilityStatus, 260, 150)
        AddControl(pnlBio, "Citizen Status*", New ComboBox() With {.DataSource = {"SA Citizen", "Permanent Resident", "Other"}}, cmbCitizenStatus, 520, 150)
        AddControl(pnlBio, "Stats Area Code*", New TextBox(), txtStatsAreaCode, 780, 150)

        ' Row 5
        AddControl(pnlBio, "Socio economic status*", New ComboBox() With {.DataSource = {"Employed", "Unemployed", "Student"}}, cmbSocioEcon, 0, 200)
        
        Dim chk As New CheckBox() With {.Text = "Agree to POPI Act", .Location = New Point(260, 220), .AutoSize = True}
        pnlBio.Controls.Add(chk)
        chkPopi = chk
        
        AddControl(pnlBio, "POPI Act Agreement Date*", New DateTimePicker(), dtpPopiDate, 520, 200)

        currentY += 270

        ' === Contact Details ===
        currentY = AddSectionHeader(pnlContent, "Contact details", currentY)
        Dim pnlContact As New Panel() With {.Location = New Point(10, currentY), .Size = New Size(1050, 220), .BorderStyle = BorderStyle.None}
        pnlContent.Controls.Add(pnlContact)

        ' Row 1
        AddControl(pnlContact, "Phone number", New TextBox(), txtPhone, 0, 0)
        AddControl(pnlContact, "P.O Box", New TextBox(), txtPOBox, 260, 0)
        AddControl(pnlContact, "Cellphone number*", New TextBox(), txtCell, 520, 0)
        AddControl(pnlContact, "Street Name*", New TextBox(), txtStreetName, 780, 0)

        ' Row 2
        AddControl(pnlContact, "Postal Suburb", New TextBox(), txtPostalSuburb, 0, 50)
        AddControl(pnlContact, "Street/House No*", New TextBox(), txtStreetNo, 260, 50)
        AddControl(pnlContact, "Physical Suburb*", New TextBox(), txtPhysSuburb, 520, 50)
        AddControl(pnlContact, "City*", New TextBox(), txtCity, 780, 50)

        ' Row 3
        AddControl(pnlContact, "Fax number", New TextBox(), txtFax, 0, 100)
        ' AddControl(pnlContact, "City (dup)", New TextBox(), Nothing, 260, 100) ' Removed Duplicate
        AddControl(pnlContact, "Postal code*", New TextBox(), txtPostalCode, 520, 100)
        
        Dim chkRes As New CheckBox() With {.Text = "Is Residential Addr Same as Postal", .Location = New Point(780, 120), .AutoSize = True}
        pnlContact.Controls.Add(chkRes)
        chkResSamePostal = chkRes

        ' Row 4
        ' AddControl(pnlContact, "Postal code (dup)", New TextBox(), Nothing, 0, 150) ' Removed Duplicate
        AddControl(pnlContact, "Email address*", New TextBox(), txtEmail, 260, 150)
        AddControl(pnlContact, "Province*", New ComboBox() With {.DataSource = {"Gauteng", "KZN", "WC"}}, cmbProvince, 520, 150)
        AddControl(pnlContact, "Urban/Rural*", New ComboBox() With {.DataSource = {"Urban", "Rural"}}, cmbUrbanRural, 780, 150)

        currentY += 230

        ' === Disability Details ===
        currentY = AddSectionHeader(pnlContent, "Disability details", currentY)
        Dim pnlDis As New Panel() With {.Location = New Point(10, currentY), .Size = New Size(1050, 120), .BorderStyle = BorderStyle.None}
        pnlContent.Controls.Add(pnlDis)

        ' Row 1
        AddControl(pnlDis, "Communication*", New TextBox(), txtDisComm, 0, 0)
        AddControl(pnlDis, "Hearing*", New TextBox(), txtDisHearing, 260, 0)
        AddControl(pnlDis, "Remembering*", New TextBox(), txtDisRemembering, 520, 0)
        AddControl(pnlDis, "Seeing*", New TextBox(), txtDisSeeing, 780, 0)

        ' Row 2
        AddControl(pnlDis, "Self Care*", New TextBox(), txtDisSelfCare, 0, 50)
        AddControl(pnlDis, "Walking*", New TextBox(), txtDisWalking, 260, 50)

        currentY += 130

        ' === Education Details ===
        currentY = AddSectionHeader(pnlContent, "Education details", currentY)
        Dim pnlEdu As New Panel() With {.Location = New Point(10, currentY), .Size = New Size(1050, 180), .BorderStyle = BorderStyle.None}
        pnlContent.Controls.Add(pnlEdu)
        
        ' Radio buttons
        Dim rad1 As New RadioButton() With {.Text = "I have selected my last school attended.", .Location = New Point(0, 0), .AutoSize = True}
        Dim rad2 As New RadioButton() With {.Text = "Unable to find the last school attended, last school attended was in South Africa.", .Location = New Point(0, 25), .AutoSize = True}
        Dim rad3 As New RadioButton() With {.Text = "Unable to find the last school attended, last school attended was not in South Africa.", .Location = New Point(0, 50), .AutoSize = True}
        pnlEdu.Controls.AddRange({rad1, rad2, rad3})

        AddControl(pnlEdu, "Last School Attended*", New TextBox(), txtSchoolName, 0, 80)
        ' Add Button for Search School
        Dim btnSearchSchool As New Button() With {
            .Text = "Search School",
            .Location = New Point(0, 140),
            .Size = New Size(240, 30),
            .BackColor = Color.SeaGreen,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat
        }
        pnlEdu.Controls.Add(btnSearchSchool)

        AddControl(pnlEdu, "Last School Year*", New TextBox(), txtSchoolYear, 520, 80)

        currentY += 160
    End Sub

    Private Function AddSectionHeader(parent As Panel, text As String, y As Integer) As Integer
        Dim lbl As New Label() With {
            .Text = text,
            .BackColor = Color.SeaGreen,
            .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .Location = New Point(10, y),
            .Size = New Size(1050, 30),
            .TextAlign = ContentAlignment.MiddleLeft,
            .Padding = New Padding(5, 0, 0, 0)
        }
        parent.Controls.Add(lbl)
        Return y + 40
    End Function

    Private Sub AddControl(parent As Panel, label As String, ctrl As Control, ByRef refCtrl As Control, x As Integer, y As Integer)
        Dim lbl As New Label() With {
            .Text = label,
            .Location = New Point(x, y),
            .AutoSize = True,
            .Font = New Font("Segoe UI", 8)
        }
        parent.Controls.Add(lbl)
        
        ctrl.Location = New Point(x, y + 15)
        ctrl.Width = 240
        ctrl.Font = New Font("Segoe UI", 10)
        parent.Controls.Add(ctrl)
        
        ' Manually assigning to the class field if needed by logic
        refCtrl = ctrl
    End Sub

    Private Sub dtpDOB_ValueChanged(sender As Object, e As EventArgs)
        Dim dob = dtpDOB.Value
        Dim age = DateTime.Now.Year - dob.Year
        If DateTime.Now < dob.AddYears(age) Then age -= 1
        txtAge.Text = age.ToString()
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub

    Private Sub BtnSubmit_Click(sender As Object, e As EventArgs)
        ' 1. Map UI to Model
        _learner.Nationality = txtNationality.Text
        _learner.Title = If(cmbTitle.SelectedItem IsNot Nothing, cmbTitle.SelectedItem.ToString(), "")
        _learner.FirstName = txtFirstName.Text
        _learner.MiddleName = txtMiddleName.Text
        _learner.LastName = txtLastName.Text
        _learner.NationalID = txtNationalID.Text
        _learner.DateOfBirth = dtpDOB.Value
        
        Dim ageVal As Integer
        If Integer.TryParse(txtAge.Text, ageVal) Then
            _learner.Age = ageVal
        Else
            _learner.Age = 0
        End If

        _learner.EquityCode = If(cmbEquity.SelectedItem IsNot Nothing, cmbEquity.SelectedItem.ToString(), "")
        _learner.Gender = If(cmbGender.SelectedItem IsNot Nothing, cmbGender.SelectedItem.ToString(), "")
        _learner.HomeLanguage = If(cmbHomeLang.SelectedItem IsNot Nothing, cmbHomeLang.SelectedItem.ToString(), "")
        _learner.Municipality = If(cmbMunicipality.SelectedItem IsNot Nothing, cmbMunicipality.SelectedItem.ToString(), "")
        _learner.DisabilityStatus = If(cmbDisabilityStatus.SelectedItem IsNot Nothing, cmbDisabilityStatus.SelectedItem.ToString(), "")
        _learner.CitizenStatus = If(cmbCitizenStatus.SelectedItem IsNot Nothing, cmbCitizenStatus.SelectedItem.ToString(), "")
        _learner.StatsAreaCode = txtStatsAreaCode.Text
        _learner.SocioEconomicStatus = If(cmbSocioEcon.SelectedItem IsNot Nothing, cmbSocioEcon.SelectedItem.ToString(), "")
        _learner.PopiActConsent = chkPopi.Checked
        _learner.PopiActDate = dtpPopiDate.Value
        
        _learner.PhoneNumber = txtPhone.Text
        _learner.POBox = txtPOBox.Text
        _learner.CellphoneNumber = txtCell.Text
        _learner.StreetName = txtStreetName.Text
        _learner.PostalSuburb = txtPostalSuburb.Text
        _learner.StreetHouseNo = txtStreetNo.Text
        _learner.PhysicalSuburb = txtPhysSuburb.Text
        _learner.City = txtCity.Text
        _learner.FaxNumber = txtFax.Text
        _learner.PostalCode = txtPostalCode.Text
        _learner.EmailAddress = txtEmail.Text
        _learner.Province = If(cmbProvince.SelectedItem IsNot Nothing, cmbProvince.SelectedItem.ToString(), "")
        _learner.UrbanRural = If(cmbUrbanRural.SelectedItem IsNot Nothing, cmbUrbanRural.SelectedItem.ToString(), "")
        
        _learner.Disability_Communication = txtDisComm.Text
        _learner.Disability_Hearing = txtDisHearing.Text
        _learner.Disability_Remembering = txtDisRemembering.Text
        _learner.Disability_Seeing = txtDisSeeing.Text
        _learner.Disability_SelfCare = txtDisSelfCare.Text
        _learner.Disability_Walking = txtDisWalking.Text
        
        _learner.LastSchoolAttended = txtSchoolName.Text
        _learner.LastSchoolYear = txtSchoolYear.Text
        
        ' 2. Check for Duplicates
        Dim result = _dedupService.CheckForDuplicates(_learner)
        If result.IsDuplicate Then
            MessageBox.Show($"Potential Duplicate Found!{vbCrLf}Match Type: {result.MatchType}{vbCrLf}Score: {result.MatchScore}%", "Duplicate Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Else
             ' 3. Register
             Try
                 _learnerService.RegisterLearner(_learner)
                 MessageBox.Show("Learner Registered Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                 Me.Close()
             Catch ex As Exception
                 MessageBox.Show("Error saving: " & ex.Message)
             End Try
        End If
    End Sub

End Class