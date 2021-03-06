<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
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
        Me.btnScan = New System.Windows.Forms.Button()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.txtScanLocation = New System.Windows.Forms.TextBox()
        Me.btnProcess = New System.Windows.Forms.Button()
        Me.btnCheckAll = New System.Windows.Forms.Button()
        Me.btnCheckNone = New System.Windows.Forms.Button()
        Me.btnPreviousPage = New System.Windows.Forms.Button()
        Me.btnNextPage = New System.Windows.Forms.Button()
        Me.lblPage = New System.Windows.Forms.Label()
        Me.gbxMedia = New System.Windows.Forms.GroupBox()
        Me.SuspendLayout()
        '
        'btnScan
        '
        Me.btnScan.Location = New System.Drawing.Point(10, 10)
        Me.btnScan.Name = "btnScan"
        Me.btnScan.Size = New System.Drawing.Size(75, 23)
        Me.btnScan.TabIndex = 0
        Me.btnScan.Text = "Scan Folder"
        Me.btnScan.UseVisualStyleBackColor = True
        '
        'lblStatus
        '
        Me.lblStatus.AutoSize = True
        Me.lblStatus.Location = New System.Drawing.Point(7, 36)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(40, 13)
        Me.lblStatus.TabIndex = 1
        Me.lblStatus.Text = "Status:"
        '
        'txtScanLocation
        '
        Me.txtScanLocation.Location = New System.Drawing.Point(91, 13)
        Me.txtScanLocation.Name = "txtScanLocation"
        Me.txtScanLocation.Size = New System.Drawing.Size(320, 20)
        Me.txtScanLocation.TabIndex = 2
        Me.txtScanLocation.Text = "G:\Videos\Movies\"
        '
        'btnProcess
        '
        Me.btnProcess.Location = New System.Drawing.Point(1174, 11)
        Me.btnProcess.Name = "btnProcess"
        Me.btnProcess.Size = New System.Drawing.Size(125, 23)
        Me.btnProcess.TabIndex = 3
        Me.btnProcess.Text = "Process Selected"
        Me.btnProcess.UseVisualStyleBackColor = True
        '
        'btnCheckAll
        '
        Me.btnCheckAll.Location = New System.Drawing.Point(1093, 11)
        Me.btnCheckAll.Name = "btnCheckAll"
        Me.btnCheckAll.Size = New System.Drawing.Size(75, 23)
        Me.btnCheckAll.TabIndex = 4
        Me.btnCheckAll.Text = "Check All"
        Me.btnCheckAll.UseVisualStyleBackColor = True
        '
        'btnCheckNone
        '
        Me.btnCheckNone.Location = New System.Drawing.Point(1012, 11)
        Me.btnCheckNone.Name = "btnCheckNone"
        Me.btnCheckNone.Size = New System.Drawing.Size(75, 23)
        Me.btnCheckNone.TabIndex = 5
        Me.btnCheckNone.Text = "Check None"
        Me.btnCheckNone.UseVisualStyleBackColor = True
        '
        'btnPreviousPage
        '
        Me.btnPreviousPage.Location = New System.Drawing.Point(414, 11)
        Me.btnPreviousPage.Name = "btnPreviousPage"
        Me.btnPreviousPage.Size = New System.Drawing.Size(89, 23)
        Me.btnPreviousPage.TabIndex = 6
        Me.btnPreviousPage.Text = "Previous Page"
        Me.btnPreviousPage.UseVisualStyleBackColor = True
        '
        'btnNextPage
        '
        Me.btnNextPage.Location = New System.Drawing.Point(509, 11)
        Me.btnNextPage.Name = "btnNextPage"
        Me.btnNextPage.Size = New System.Drawing.Size(89, 23)
        Me.btnNextPage.TabIndex = 7
        Me.btnNextPage.Text = "Next Page"
        Me.btnNextPage.UseVisualStyleBackColor = True
        '
        'lblPage
        '
        Me.lblPage.AutoSize = True
        Me.lblPage.Location = New System.Drawing.Point(604, 15)
        Me.lblPage.Name = "lblPage"
        Me.lblPage.Size = New System.Drawing.Size(55, 13)
        Me.lblPage.TabIndex = 8
        Me.lblPage.Text = "Page: 0/0"
        '
        'gbxMedia
        '
        Me.gbxMedia.Location = New System.Drawing.Point(10, 52)
        Me.gbxMedia.Name = "gbxMedia"
        Me.gbxMedia.Size = New System.Drawing.Size(1345, 797)
        Me.gbxMedia.TabIndex = 9
        Me.gbxMedia.TabStop = False
        Me.gbxMedia.Text = "Media Files"
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1384, 861)
        Me.Controls.Add(Me.gbxMedia)
        Me.Controls.Add(Me.lblPage)
        Me.Controls.Add(Me.btnNextPage)
        Me.Controls.Add(Me.btnPreviousPage)
        Me.Controls.Add(Me.btnCheckNone)
        Me.Controls.Add(Me.btnCheckAll)
        Me.Controls.Add(Me.btnProcess)
        Me.Controls.Add(Me.txtScanLocation)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.btnScan)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnScan As Button
    Friend WithEvents lblStatus As Label
    Friend WithEvents txtScanLocation As TextBox
    Friend WithEvents btnProcess As Button
    Friend WithEvents btnCheckAll As Button
    Friend WithEvents btnCheckNone As Button
    Friend WithEvents btnPreviousPage As Button
    Friend WithEvents btnNextPage As Button
    Friend WithEvents lblPage As Label
    Friend WithEvents gbxMedia As GroupBox
End Class
