<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class WebForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim WebPreferences1 As Awesomium.Core.WebPreferences = New Awesomium.Core.WebPreferences(True)
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.IWebViewBindingSource = New System.Windows.Forms.BindingSource(Me.components)
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.MainWebSessionProvider = New Awesomium.Windows.Forms.WebSessionProvider(Me.components)
        Me.MyWebControl = New Awesomium.Windows.Forms.WebControl(Me.components)
        Me.StatusStrip1.SuspendLayout()
        CType(Me.IWebViewBindingSource, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'StatusStrip1
        '
        Me.StatusStrip1.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.IWebViewBindingSource, "TargetURL", True))
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripStatusLabel1})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 619)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(1134, 22)
        Me.StatusStrip1.TabIndex = 1
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'IWebViewBindingSource
        '
        Me.IWebViewBindingSource.DataSource = GetType(Awesomium.Core.IWebView)
        '
        'ToolStripStatusLabel1
        '
        Me.ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        Me.ToolStripStatusLabel1.Size = New System.Drawing.Size(121, 17)
        Me.ToolStripStatusLabel1.Text = "ToolStripStatusLabel1"
        '
        'MainWebSessionProvider
        '
        Me.MainWebSessionProvider.DataPath = ".\Cache"
        WebPreferences1.EnableGPUAcceleration = True
        WebPreferences1.WebGL = True
        Me.MainWebSessionProvider.Preferences = WebPreferences1
        Me.MainWebSessionProvider.Views.Add(Me.MyWebControl)
        '
        'MyWebControl
        '
        Me.MyWebControl.Dock = System.Windows.Forms.DockStyle.Fill
        Me.MyWebControl.Location = New System.Drawing.Point(0, 0)
        Me.MyWebControl.Size = New System.Drawing.Size(1134, 619)
        Me.MyWebControl.TabIndex = 2
        '
        'WebForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.ActiveCaption
        Me.ClientSize = New System.Drawing.Size(1134, 641)
        Me.Controls.Add(Me.MyWebControl)
        Me.Controls.Add(Me.StatusStrip1)
        Me.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.IWebViewBindingSource, "Title", True))
        Me.Name = "WebForm"
        Me.Text = "Form1"
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        CType(Me.IWebViewBindingSource, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents IWebViewBindingSource As System.Windows.Forms.BindingSource
    Friend WithEvents ToolStripStatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents MainWebSessionProvider As Awesomium.Windows.Forms.WebSessionProvider
    Private WithEvents MyWebControl As Awesomium.Windows.Forms.WebControl

End Class
