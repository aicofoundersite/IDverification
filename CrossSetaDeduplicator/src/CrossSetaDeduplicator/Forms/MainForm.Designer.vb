<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.txtNationalID = New System.Windows.Forms.TextBox()
        Me.txtFirstName = New System.Windows.Forms.TextBox()
        Me.txtLastName = New System.Windows.Forms.TextBox()
        Me.dtpDOB = New System.Windows.Forms.DateTimePicker()
        Me.cmbGender = New System.Windows.Forms.ComboBox()
        Me.btnCheck = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnBulkUpload = New System.Windows.Forms.Button()
        Me.dgvResults = New System.Windows.Forms.DataGridView()
        Me.lblNationalID = New System.Windows.Forms.Label()
        Me.lblFirstName = New System.Windows.Forms.Label()
        Me.lblLastName = New System.Windows.Forms.Label()
        
        CType(Me.dgvResults, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        
        '
        ' txtNationalID
        '
        Me.txtNationalID.Location = New System.Drawing.Point(120, 30)
        Me.txtNationalID.Name = "txtNationalID"
        Me.txtNationalID.Size = New System.Drawing.Size(200, 20)
        Me.txtNationalID.TabIndex = 0
        
        '
        ' txtFirstName
        '
        Me.txtFirstName.Location = New System.Drawing.Point(120, 60)
        Me.txtFirstName.Name = "txtFirstName"
        Me.txtFirstName.Size = New System.Drawing.Size(200, 20)
        Me.txtFirstName.TabIndex = 1
        
        '
        ' txtLastName
        '
        Me.txtLastName.Location = New System.Drawing.Point(120, 90)
        Me.txtLastName.Name = "txtLastName"
        Me.txtLastName.Size = New System.Drawing.Size(200, 20)
        Me.txtLastName.TabIndex = 2
        
        '
        ' dtpDOB
        '
        Me.dtpDOB.Location = New System.Drawing.Point(120, 120)
        Me.dtpDOB.Name = "dtpDOB"
        Me.dtpDOB.Size = New System.Drawing.Size(200, 20)
        Me.dtpDOB.TabIndex = 3
        
        '
        ' cmbGender
        '
        Me.cmbGender.FormattingEnabled = True
        Me.cmbGender.Items.AddRange(New Object() {"Male", "Female", "Other"})
        Me.cmbGender.Location = New System.Drawing.Point(120, 150)
        Me.cmbGender.Name = "cmbGender"
        Me.cmbGender.Size = New System.Drawing.Size(200, 21)
        Me.cmbGender.TabIndex = 4
        
        '
        ' btnCheck
        '
        Me.btnCheck.Location = New System.Drawing.Point(120, 190)
        Me.btnCheck.Name = "btnCheck"
        Me.btnCheck.Size = New System.Drawing.Size(150, 30)
        Me.btnCheck.TabIndex = 5
        Me.btnCheck.Text = "Check for Duplicates"
        Me.btnCheck.UseVisualStyleBackColor = True
        
        '
        ' btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(280, 190)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(100, 30)
        Me.btnSave.TabIndex = 6
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = True
        
        '
        ' btnBulkUpload
        '
        Me.btnBulkUpload.Location = New System.Drawing.Point(400, 190)
        Me.btnBulkUpload.Name = "btnBulkUpload"
        Me.btnBulkUpload.Size = New System.Drawing.Size(150, 30)
        Me.btnBulkUpload.TabIndex = 7
        Me.btnBulkUpload.Text = "Bulk ID Verification"
        Me.btnBulkUpload.UseVisualStyleBackColor = True

        '
        ' dgvResults
        '
        Me.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvResults.Location = New System.Drawing.Point(30, 250)
        Me.dgvResults.Name = "dgvResults"
        Me.dgvResults.Size = New System.Drawing.Size(700, 200)
        Me.dgvResults.TabIndex = 8
        
        '
        ' Labels (Simplified)
        '
        Me.lblNationalID.AutoSize = True
        Me.lblNationalID.Location = New System.Drawing.Point(30, 33)
        Me.lblNationalID.Name = "lblNationalID"
        Me.lblNationalID.Size = New System.Drawing.Size(63, 13)
        Me.lblNationalID.Text = "National ID:"
        
        Me.lblFirstName.AutoSize = True
        Me.lblFirstName.Location = New System.Drawing.Point(30, 63)
        Me.lblFirstName.Name = "lblFirstName"
        Me.lblFirstName.Size = New System.Drawing.Size(60, 13)
        Me.lblFirstName.Text = "First Name:"
        
        Me.lblLastName.AutoSize = True
        Me.lblLastName.Location = New System.Drawing.Point(30, 93)
        Me.lblLastName.Name = "lblLastName"
        Me.lblLastName.Size = New System.Drawing.Size(61, 13)
        Me.lblLastName.Text = "Last Name:"
        
        '
        ' MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 500)
        Me.Controls.Add(Me.lblLastName)
        Me.Controls.Add(Me.lblFirstName)
        Me.Controls.Add(Me.lblNationalID)
        Me.Controls.Add(Me.dgvResults)
        Me.Controls.Add(Me.btnBulkUpload)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.btnCheck)
        Me.Controls.Add(Me.cmbGender)
        Me.Controls.Add(Me.dtpDOB)
        Me.Controls.Add(Me.txtLastName)
        Me.Controls.Add(Me.txtFirstName)
        Me.Controls.Add(Me.txtNationalID)
        Me.Name = "MainForm"
        Me.Text = "CrossSeta Deduplicator"
        CType(Me.dgvResults, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents txtNationalID As System.Windows.Forms.TextBox
    Friend WithEvents txtFirstName As System.Windows.Forms.TextBox
    Friend WithEvents txtLastName As System.Windows.Forms.TextBox
    Friend WithEvents dtpDOB As System.Windows.Forms.DateTimePicker
    Friend WithEvents cmbGender As System.Windows.Forms.ComboBox
    Friend WithEvents btnCheck As System.Windows.Forms.Button
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnBulkUpload As System.Windows.Forms.Button
    Friend WithEvents dgvResults As System.Windows.Forms.DataGridView
    Friend WithEvents lblNationalID As System.Windows.Forms.Label
    Friend WithEvents lblFirstName As System.Windows.Forms.Label
    Friend WithEvents lblLastName As System.Windows.Forms.Label
End Class
