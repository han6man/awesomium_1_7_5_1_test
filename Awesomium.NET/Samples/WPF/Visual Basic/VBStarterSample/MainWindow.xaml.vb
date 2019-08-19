'*******************************************************************************
'    Project   : Awesomium.NET (VBStarterSample)
'    File      : MainWindow.xaml.vb
'    Version   : 1.7.4.1
'    Date      : 03/21/2014
'    Author    : Perikles C. Stephanidis (perikles@awesomium.com)
'    Copyright : ©2014 Awesomium Technologies LLC
'    
'    This code is provided "AS IS" and for demonstration purposes only,
'    without warranty of any kind.
'     
'-------------------------------------------------------------------------------
'
'    Notes     :
'
'    Starter sample demonstration of the basic code in a Window hosting
'    a WPF WebControl.
'    
'    
'*******************************************************************************

Option Strict Off
Option Explicit Off

Imports System.IO
Imports Awesomium.Core
Imports System.ComponentModel
Imports Awesomium.Windows.Controls

Namespace StarterSample

    Class MainWindow

#Region "Fields"
        Private Const CHANGE_IMG_SRC As String = "(function() { " &
            "var image = document.getElementById('awe-logo-silhouette'); " &
            "if (image == null) { return; } " &
            "image.addEventListener('load', function() { myObject.onImageLoaded('awe-logo-silhouette', image.src) }, false ); " &
            "image.src='http://upload.wikimedia.org/wikipedia/commons/1/1e/Lascaux_painting.jpg'; })()"
#End Region


#Region "Ctors"
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            ' Always handle ShowCreatedWebView. This is fired for
            ' links and forms with |target="_blank"| or for JavaScript
            ' 'window.open' calls.
            AddHandler webControl.ShowCreatedWebView, AddressOf webControl_ShowCreatedWebView
            ' We demonstrate interaction with the page. We handle these events
            ' and execute the examples, only on the initial main window.
            AddHandler webControl.NativeViewInitialized, AddressOf OnNativeViewInitialized
            AddHandler webControl.DocumentReady, AddressOf OnDocumentReady
            AddHandler webControl.ConsoleMessage, AddressOf OnConsoleMessage
            ' Start with the specified Home URL.
            Me.Source = WebCore.Configuration.HomeURL
        End Sub

        Public Sub New(nativeView As IntPtr)

            ' This call is required by the designer.
            InitializeComponent()

            ' Always handle ShowCreatedWebView. This is fired for
            ' links and forms with |target="_blank"| or for JavaScript
            ' 'window.open' calls.
            AddHandler webControl.ShowCreatedWebView, AddressOf webControl_ShowCreatedWebView
            ' For popups, you usually want to handle WindowClose,
            ' fired when the page calls 'window.close'.
            AddHandler webControl.WindowClose, AddressOf webControl_WindowClose
            ' Tell the WebControl that it should wrap a created child view.
            Me.NativeView = nativeView
            ' This window will host a WebControl that is the result of 
            ' JavaScript 'window.open'. Hide the address and status bar.
            Me.IsRegularWindow = False
        End Sub

        Public Sub New(url As Uri)

            ' This call is required by the designer.
            InitializeComponent()

            ' Always handle ShowCreatedWebView. This is fired for
            ' links and forms with |target="_blank"| or for JavaScript
            ' 'window.open' calls.
            AddHandler webControl.ShowCreatedWebView, AddressOf webControl_ShowCreatedWebView
            ' For popups, you usually want to handle WindowClose,
            ' fired when the page calls 'window.close'.
            AddHandler webControl.WindowClose, AddressOf webControl_WindowClose
            ' Tell the WebControl to load a specified target URL.
            Me.Source = url
        End Sub
#End Region


#Region "Overrides"
        Protected Overrides Sub OnClosed(e As EventArgs)
            MyBase.OnClosed(e)

            ' Dispose the WebControl.
            webControl.Dispose()

            ' We shutdown the core in Application.OnExit.
        End Sub
#End Region

#Region "Methods"
        ' We demonstrate getting a screenshot of the view, in WPF.
        Private Sub GetScreenshot(ByVal fileName As String)
            Dim surface As WebViewPresenter = TryCast(webControl.Surface, WebViewPresenter)
            If surface Is Nothing Then Return

            Dim bitmap As BitmapSource = TryCast(surface.Image, BitmapSource)
            If bitmap Is Nothing Then Return

            ' For the sample, we use a PNG encoder. WPF provides
            ' other too, such as a JpegBitmapEncoder.
            Dim encoder As New PngBitmapEncoder()
            encoder.Frames.Add(BitmapFrame.Create(bitmap))

            ' Open/Create the file to save the image to.
            Using fs As FileStream = File.Open(fileName, FileMode.OpenOrCreate)
                encoder.Save(fs)
            End Using
        End Sub

        ' We demonstrate thread-safe execution against the view.
        Private Sub PrintPageHTML()
            ' Code potentially executed in another (non-UI) thread.
            ' This code is valid for any web-view component. For WPF only,
            ' you can also use webControl.Dispatcher.Invoke.
            Dim sync As ISynchronizeInvoke = DirectCast(webControl, ISynchronizeInvoke)
            Dim html As String = If(sync.InvokeRequired,
                                    TryCast(sync.Invoke(CType(AddressOf ExecuteJavascriptOnView, Func(Of String)), Nothing), String),
                                    ExecuteJavascriptOnView())

            If Not String.IsNullOrEmpty(html) Then
                ' Managed IWebView instances already provide an HTML property
                ' that is updated when the DOM is loaded. At this point,
                ' these two should be equal.
                Debug.Assert(html = webControl.HTML)
                ' Print it.
                Debug.Print(html)
            End If
        End Sub

        ' This method will be called on the WebControl's thread (the UI thread).
        Private Function ExecuteJavascriptOnView() As String
            ' We demonstrate the use of the DLR.
            ' NOTE: Option Explicit Off is required in VB.NET for this to work.
            ' This method is called from within DocumentReady so it already
            ' executes in an asynchronous Javascript Execution Context (JEC).
            Dim js = [Global].Current
            If Not CBool(js) Then Return String.Empty

            Dim document As Object = js.document
            If Not document Then Return String.Empty

            ' JSObject supports the DLR. You can dynamically call methods,
            ' access arrays or lists and get or set properties.
            Return document.getElementsByTagName("html")(0).outerHTML

            ' Any binding errors or exceptions that occur in a Javascript Execution 
            ' Context (JEC) are propagated to the JavaScript console.
        End Function
#End Region

#Region "Properties"
        ' This will be set to the target URL, when this window does not
        ' host a created child view. The WebControl, is bound to this property.
        Public Property Source As Uri
            Get
                Return GetValue(SourceProperty)
            End Get

            Set(ByVal value As Uri)
                SetValue(SourceProperty, value)
            End Set
        End Property

        Public Shared ReadOnly SourceProperty As DependencyProperty = _
                               DependencyProperty.Register("Source", _
                               GetType(Uri), GetType(MainWindow), _
                               New FrameworkPropertyMetadata(Nothing))


        ' This will be set to the created child view that the WebControl will wrap,
        ' when ShowCreatedWebView is the result of 'window.open'. The WebControl, 
        ' is bound to this property.
        Public Property NativeView As IntPtr
            Get
                Return GetValue(MainWindow.NativeViewProperty)
            End Get
            Private Set(value As IntPtr)
                Me.SetValue(MainWindow.NativeViewPropertyKey, value)
            End Set
        End Property

        Private Shared ReadOnly NativeViewPropertyKey As DependencyPropertyKey = _
                                DependencyProperty.RegisterReadOnly("NativeView", _
                                GetType(IntPtr), GetType(MainWindow), _
                                New FrameworkPropertyMetadata(IntPtr.Zero))

        Public Shared ReadOnly NativeViewProperty As DependencyProperty = _
                               NativeViewPropertyKey.DependencyProperty


        ' The visibility of the address bar and status bar, depends
        ' on the value of this property. We set this to false when
        ' the window wraps a WebControl that is the result of JavaScript
        ' 'window.open'.
        Public Property IsRegularWindow As Boolean
            Get
                Return GetValue(MainWindow.IsRegularWindowProperty)
            End Get
            Private Set(value As Boolean)
                Me.SetValue(MainWindow.IsRegularWindowPropertyKey, value)
            End Set
        End Property

        Private Shared ReadOnly IsRegularWindowPropertyKey As DependencyPropertyKey = _
                                DependencyProperty.RegisterReadOnly("IsRegularWindow", _
                                GetType(Boolean), GetType(MainWindow), _
                                New FrameworkPropertyMetadata(True))

        Public Shared ReadOnly IsRegularWindowProperty As DependencyProperty = _
                               IsRegularWindowPropertyKey.DependencyProperty
#End Region

#Region "Event Handlers"
        Private Sub OnNativeViewInitialized(sender As Object, e As WebViewEventArgs)
            ' The native view is created. You can create global JavaScript objects
            ' at this point. These objects persist throughout the lifetime of the view
            ' and are available to all pages loaded by this view.
            Using myObject As JSObject = webControl.CreateGlobalJavascriptObject("myObject")
                ' Add a custom method to the global object and bind to it.
                ' Notice that we do not specify a method name. It will be determined
                ' from the name of our handler (onImageLoaded).
                myObject.BindAsync(AddressOf onImageLoaded)
            End Using
        End Sub

        Private Sub OnDocumentReady(sender As Object, e As DocumentReadyEventArgs)
            ' When ReadyState is Ready, you can execute JavaScript against
            ' the DOM but all resources are not yet loaded. Wait for Loaded.
            If e.ReadyState = DocumentReadyState.Ready Then Return

            ' Get a screenshot of the view.
            GetScreenshot("wpf_screenshot_before.png")

            ' Print the page's HTML source.
            PrintPageHTML()

            ' Asynchronously execute some script on the page,
            ' that will change the source of an image and wait
            ' for the image load to complete.
            webControl.ExecuteJavascript(CHANGE_IMG_SRC)
        End Sub

        Private Sub onImageLoaded(sender As Object, e As JavascriptMethodEventArgs)
            Debug.Print(String.Format("IMG with id: '{0}' completed loading: {1}", e.Arguments(0), e.Arguments(1)))
            ' Get another screenshot.
            GetScreenshot("wpf_screenshot_after.png")
        End Sub

        ' Any JavaScript errors or JavaScript console.log calls,
        ' will call this method.
        Private Sub OnConsoleMessage(sender As Object, e As ConsoleMessageEventArgs)
            Debug.Print(e.Message)
        End Sub

        ' This static handler, will handle the ShowCreatedWebView event for both the 
        ' WebControl of our main application window, as well as for any other windows
        ' hosting WebControls.
        Friend Sub webControl_ShowCreatedWebView(sender As Object, e As ShowCreatedWebViewEventArgs)
            If Not webControl.IsLive Then Return

            ' Create an instance of our application's child window, that will
            ' host the new view instance, either we wrap the created child view,
            ' or we let the WebControl create a new underlying web-view.
            Dim newWindow As MainWindow

            ' Treat popups differently. If IsPopup is true, the event is always
            ' the result of 'window.open' (IsWindowOpen is also true, so no need to check it).
            ' Our application does not recognize user defined, non-standard specs. 
            ' Therefore child views opened with non-standard specs, will not be presented as 
            ' popups but as regular new windows (still wrapping the child view however -- see below).
            If e.IsPopup AndAlso (Not e.IsUserSpecsOnly) Then
                ' JSWindowOpenSpecs.InitialPosition indicates screen coordinates.
                Dim screenRect As Int32Rect = e.Specs.InitialPosition.GetInt32Rect()

                ' Set the created native view as the underlying view of the
                ' WebControl. This will maintain the relationship between
                ' the parent view and the child, usually required when the new view
                ' is the result of 'window.open' (JS can access the parent window through
                ' 'window.opener'; the parent window can manipulate the child through the 'window'
                ' object returned from the 'window.open' call).
                newWindow = New MainWindow(e.NewViewInstance)
                ' Do not show in the taskbar.
                newWindow.ShowInTaskbar = False
                ' Set a border-style to indicate a popup.
                newWindow.WindowStyle = System.Windows.WindowStyle.ToolWindow
                ' Set resizing mode depending on the indicated specs.
                newWindow.ResizeMode = If(e.Specs.Resizable, ResizeMode.CanResizeWithGrip, ResizeMode.NoResize)

                ' If the caller has not indicated a valid size for the new popup window,
                ' let it be opened with the default size specified at design time.
                If (screenRect.Width > 0) AndAlso (screenRect.Height > 0) Then
                    ' The indicated size, is client size.
                    Dim horizontalBorderHeight As Double = SystemParameters.ResizeFrameHorizontalBorderHeight
                    Dim verticalBorderWidth As Double = SystemParameters.ResizeFrameVerticalBorderWidth
                    Dim captionHeight As Double = SystemParameters.CaptionHeight

                    ' Assign the indicated size.
                    newWindow.Width = screenRect.Width + (verticalBorderWidth * 2)
                    newWindow.Height = screenRect.Height + captionHeight + (horizontalBorderHeight * 2)
                End If

                ' Show the window.
                newWindow.Show()

                ' If the caller has not indicated a valid position for the new popup window,
                ' let it be opened in the default position specified at design time.
                If (screenRect.Y > 0) AndAlso (screenRect.X > 0) Then
                    ' Move it to the indicated coordinates.
                    newWindow.Top = screenRect.Y
                    newWindow.Left = screenRect.X
                End If
            ElseIf (e.IsWindowOpen OrElse e.IsPost) Then
                ' No specs or only non-standard specs were specified, but the event is still 
                ' the result of 'window.open' or of an HTML form with target="_blank" and method="post".
                ' We will open a normal window but we will still wrap the new native child view, 
                ' maintaining its relationship with the parent window.
                newWindow = New MainWindow(e.NewViewInstance)
                ' Show the window.
                newWindow.Show()
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
                newWindow = New MainWindow(e.TargetURL)
                ' Show the window.
                newWindow.Show()
            End If
        End Sub

        Private Sub webControl_WindowClose(sender As Object, e As WindowCloseEventArgs)
            ' The page called 'window.close'. If the call
            ' comes from a frame, ignore it.
            If Not e.IsCalledFromFrame Then
                Me.Close()
            End If
        End Sub
#End Region

    End Class

End Namespace