'*******************************************************************************
'    Project   : Awesomium.NET (VBWebControlSample)
'    File      : WebForm.vb
'    Version   : 1.7.0.0 
'    Date      : 3/5/2013
'    Author    : Perikles C. Stephanidis (perikles@awesomium.com)
'    Copyright : ©2013 Awesomium Technologies LLC
'    
'    This code is provided "AS IS" and for demonstration purposes only,
'    without warranty of any kind.
'     
'-------------------------------------------------------------------------------
'
'    Notes     :
'
'    Simple Visual Basic sample demonstrating the use of the Windows Forms
'    WebControl
'    
'    
'*******************************************************************************

Imports Awesomium.Core
Imports Awesomium.Windows.Forms

Public Class WebForm

    Private _IsChild As Boolean

    Public Sub New()
        MyClass.New(False)
    End Sub

    Public Sub New(isChild As Boolean)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        _IsChild = isChild

        If Not isChild Then
            If Not WebCore.IsInitialized Then
                WebCore.Initialize(New WebConfig() With {.LogLevel = LogLevel.Verbose})
            End If

            MyWebControl.Source = New Uri("http://awesomium.com")
        End If

        ' Note how we set a WebSession for our WebControl, by using the new WebSessionProvider
        ' in the designer.

        ' Assign the source for our bindings.
        IWebViewBindingSource.DataSource = CType(MyWebControl, IWebView)
    End Sub

    Public ReadOnly Property IsChild() As Boolean
        Get
            Return _IsChild
        End Get
    End Property

    Private Sub MyWebControl_ShowCreatedWebView(sender As Object, e As ShowCreatedWebViewEventArgs) Handles MyWebControl.ShowCreatedWebView
        ' Create an instance of our application web form, that will
        ' host the new view instance, either we wrap the created child view,
        ' or we let the WebControl create a new underlying web-view.
        Dim newForm As New WebForm(True)

        ' Treat popups differently. If IsPopup is true, the event is always
        ' the result of 'window.open' (IsWindowOpen is also true, so no need to check it).
        ' Our application does not recognize user defined, non-standard specs. 
        ' Therefore child views opened with non-standard specs, will not be presented as 
        ' popups but as regular new windows (still wrapping the child view however -- see below).
        If e.IsPopup AndAlso Not e.IsUserSpecsOnly Then
            ' JSWindowOpenSpecs.InitialPosition indicates screen coordinates.
            Dim screenRect As Rectangle = e.Specs.InitialPosition.ToRectangle()

            ' Set the created native view as the underlying view of the
            ' WebControl. This will maintain the relationship between
            ' the parent view and the child, usually required when the new view
            ' is the result of 'window.open' (JS can access the parent window through
            ' 'window.opener'; the parent window can manipulate the child through the 'window'
            ' object returned from the 'window.open' call).
            newForm.MyWebControl.NativeView = e.NewViewInstance
            ' Do not show in the taskbar.
            newForm.ShowInTaskbar = False
            ' Set border style based on specs.
            newForm.FormBorderStyle = If(e.Specs.Resizable, FormBorderStyle.SizableToolWindow, FormBorderStyle.FixedToolWindow)

            ' Show the form.
            newForm.Show()

            ' If the caller has not indicated a valid size for the new popup window,
            ' let it be opened with the default size specified at design time.
            If Not screenRect.Size.IsEmpty Then
                ' Specify the indicated size.
                newForm.ClientSize = screenRect.Size
            End If

            ' If the caller has not indicated a valid position for the new popup window,
            ' let it be opened in the default position specified at design time.
            If Not screenRect.Location.IsEmpty Then
                ' Move it to the specified coordinates.
                newForm.DesktopLocation = e.Specs.InitialPosition.ToRectangle().Location
            End If

        ElseIf e.IsWindowOpen OrElse e.IsPost Then
            ' No specs or only non-standard specs were specified, but the event is still 
            ' the result of 'window.open' or of an HTML form with target="_blank" and method="post".
            ' We will open a normal window but we will still wrap the new native child view, 
            ' maintaining its relationship with the parent window.
            newForm.MyWebControl.NativeView = e.NewViewInstance
            ' Show the form.
            newForm.Show()
        Else
            ' The event is not the result of 'window.open' or of an HTML form with target="_blank" 
            ' and method="post"., therefore it's most probably the result of a link with target='_blank'. 
            ' We will not be wrapping the created view; we let the WebControl hosted in ChildWindow 
            ' create its own underlying view. Setting Cancel to true tells the core to destroy the 
            ' created child view.
            '
            ' Why don't we always wrap the native view passed to ShowCreatedWebView?
            '
            ' - In order to maintain the relationship with their parent view,
            ' child views execute and render under the same process (awesomium_process)
            ' as their parent view. If for any reason this child process crashes,
            ' all views related to it will be affected. When maintaining a parent-child 
            ' relationship is not important, we prefer taking advantage of the isolated process 
            ' architecture of Awesomium and let each view be rendered in a separate process.
            e.Cancel = True
            ' Note that we only explicitly navigate to the target URL, when a new view is 
            ' about to be created, not when we wrap the created child view. This is because 
            ' navigation to the target URL (if any), is already queued on created child views. 
            ' We must not interrupt this navigation as we would still be breaking the parent-child
            ' relationship.
            newForm.MyWebControl.Source = e.TargetURL
            ' Show the form.
            newForm.Show()
        End If
    End Sub

    Private Sub MyWebControl_WindowClose(sender As Object, e As WindowCloseEventArgs) Handles MyWebControl.WindowClose
        If IsChild Then
            Me.Close()
        ElseIf Not e.IsCalledFromFrame AndAlso (MsgBox("This page is asking to close the application window. Do you confirm?", MsgBoxStyle.YesNo, MyWebControl.Source) = MsgBoxResult.Yes) Then
            Me.Close()
        End If
    End Sub
End Class
