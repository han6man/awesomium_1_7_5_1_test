'*******************************************************************************
'    Project   : Awesomium.NET (VBWPFSample)
'    File      : MainWindow.xaml.vb
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
'    Main application window of the VBWPFSample.
'
'    The sample also presents the usage of the new INavigationInterceptor
'    service.
'    
'    
'*******************************************************************************

Imports Awesomium.Core

Class MainWindow

#Region " Fields "
    Private Const JS_FAVICON As String = "(function(){links = document.getElementsByTagName('link'); wHref=window.location.protocol + '//' + window.location.hostname + '/favicon.ico'; for(i=0; i<links.length; i++){s=links[i].rel; if(s.indexOf('icon') != -1){ wHref = links[i].href }; }; return wHref; })();"

    Private m_HomePage As Uri = New Uri("http://www.google.com")
    Private m_Icon As ImageSource
#End Region


#Region " Constructors "
    Public Sub New()
        If Not WebCore.IsInitialized Then
            ' Initialize the WebCore before creating the control. Just make sure that you
            ' specify False for the start parameter. This will ensure lazy initialization of
            ' the core; if this is your main window, a synchronization context necessary
            ' for WebCore's auto-update, may not be created yet. Specifying false will ensure
            ' that the core will start when the first WebView or WebControl is created. By that
            ' time, the dispatcher will definitely be running.
            WebCore.Initialize(New WebConfig() With {.LogLevel = LogLevel.Verbose})
        End If

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_Icon = New ImageSourceConverter().ConvertFromString("pack://application:,,,/VBWPFSample;component/Awesomium.ico")
    End Sub
#End Region


#Region " Overrides "
    Protected Overrides Sub OnSourceInitialized(e As EventArgs)
        MyBase.OnSourceInitialized(e)

        If GlassUtilities.IsCompositionEnabled Then
            GlassUtilities.SetTransparentBackground(Me)
            GlassUtilities.ExtendGlassIntoClientArea(Me, New Thickness(0, 35, 0, 30))
        End If
    End Sub

    Protected Overrides Sub OnClosed(e As EventArgs)
        MyBase.OnClosed(e)

        ' Dispose the WebControl.
        Browser.Dispose()

        ' We shutdown the core in Application.OnExit.
    End Sub
#End Region

#Region " Methods "
    Private Sub UpdateFavicon()
        Try
            Dim val As String = Browser.ExecuteJavascriptWithResult(JS_FAVICON)
            Dim lastError As [Error] = Browser.GetLastError()

            If (lastError <> [Error].None) Then
                Debug.Print(lastError.ToString())
                Return
            End If

            If String.IsNullOrEmpty(val) OrElse (Not Uri.IsWellFormedUriString(val, UriKind.Absolute)) Then
                Return
            End If

            Dim decoder As BitmapDecoder = BitmapDecoder.Create(val.ToUri(), BitmapCreateOptions.None, BitmapCacheOption.None)

            If decoder.IsDownloading Then
                AddHandler decoder.DownloadCompleted, Sub(s, e) UpdateFavicon(TryCast(s, BitmapDecoder))
            Else
                UpdateFavicon(decoder)
            End If
        Catch
        End Try
    End Sub

    Private Sub UpdateFavicon(decoder As BitmapDecoder)
        If (decoder Is Nothing) OrElse (decoder.Frames.Count = 0) Then
            Return
        End If

        Me.Icon = If(decoder.Frames.FirstOrDefault(Function(f) f.PixelWidth = 16 OrElse f.PixelHeight = 16), decoder.Frames(0))
    End Sub

    Private Sub RestoreFavicon()
        Me.Icon = m_Icon
        GC.Collect()
    End Sub
#End Region

#Region " Event Handlers "
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If WebCore.IsInitialized Then
            WebCore.HomeURL = m_HomePage
        End If
    End Sub

    ' This event is fired on the I/O thread. Do not attempt to access the WebControl 
    ' or any other not thread-safe Awesomium API from inside this event handler.
    Private Sub Browser_BeginNavigation(sender As Object, e As NavigationEventArgs)
        ' We demonstrate canceling navigation to
        ' specified URLs. This example will also
        ' prevent firing a download for the ZIP file.
        If e.Url.ToString().EndsWith(".zip") Then
            e.Cancel = True
        End If
    End Sub

    Private Sub Browser_Crashed(sender As Object, e As CrashedEventArgs) Handles Browser.Crashed
        ' If the control crashes, move focus to parent.
        Me.Focus()
    End Sub

    Private Sub Browser_LoadingFrame(sender As Object, e As LoadingFrameEventArgs) Handles Browser.LoadingFrame
        If e.IsMainFrame Then
            RestoreFavicon()
        End If
    End Sub

    Private Sub Browser_DomReady(sender As Object, e As DocumentReadyEventArgs) Handles Browser.DocumentReady
        If e.ReadyState = DocumentReadyState.Ready Then Return
        UpdateFavicon()
    End Sub

    Private Sub Browser_NativeViewInitialized(sender As Object, e As WebViewEventArgs) Handles Browser.NativeViewInitialized
        ' This event is fired right when IsLive is set to True.
        If Not Browser.IsLive Then Return

        ' Acquire the NavigationInterceptor service.
        Dim navigationInterceptor As INavigationInterceptor = TryCast(Browser.GetService(GetType(INavigationInterceptor)), INavigationInterceptor)

        If navigationInterceptor Is Nothing Then
            Return
        End If

        ' Add a handler for the BeginNavigation event. This will allow us
        ' to explicitly cancel navigations.
        AddHandler navigationInterceptor.BeginNavigation, AddressOf Browser_BeginNavigation

        ' Declare a filtering rule. This accepts wildcards. In this example,
        ' we deny navigations to any Google site. Note that this will not
        ' prevent the initial loading of the WebControl; the value defined
        ' to Source at design-time, will always be loaded. After the program
        ' starts, try to navigate to any of the other Google services (Images, Mail etc.).
        navigationInterceptor.AddRule("http?://*.google.com/*", NavigationRule.Deny)
    End Sub

    Private Sub Browser_LoadingFrameFailed(sender As Object, e As LoadingFrameFailedEventArgs)
        If (Not Browser.IsLive) Then Return

        If (e.IsMainFrame AndAlso (e.ErrorCode = NetError.ABORTED)) Then
            ' This condition usually indicates a navigation blocked by the INavigationInterceptor
            ' or an IResourceInterceptor. For this example, we add an additional check, to be sure.

            ' If you don't want the WebControl's (and window's) title to show ABORTED for
            ' navigations that were aborted due to INavigationInterceptor filtering, set
            ' the WebControl's NavigationInfo property to None, then use the info acquired
            ' by this example to distinguish navigations canceled by the INavigationInterceptor
            ' or actually aborted for some other reason.

            ' Get the NavigationInterceptor service.
            Dim navigationInterceptor As INavigationInterceptor = TryCast(Browser.GetService(GetType(INavigationInterceptor)), INavigationInterceptor)

            ' Check if this URL is actually blocked by the NavigationInterceptor service.
            If ((navigationInterceptor IsNot Nothing) AndAlso (navigationInterceptor.GetRule(e.Url.AbsoluteUri) = NavigationRule.Deny)) Then
                ' You can display your error message anyway you want, here.
                Debug.Print(String.Format("Navigation to {0} was blocked.", e.Url))
            Else
                ' You can display your error message anyway you want, here.
                Debug.Print(String.Format("Navigation to {0} was aborted.", e.Url))
            End If
        End If
    End Sub

    Private Sub Browser_ShowCreatedWebView(sender As Object, e As ShowCreatedWebViewEventArgs) Handles Browser.ShowCreatedWebView
        Debug.Print(e.IsWindowOpen)

        ' Let the new view be destroyed. It is important to set Cancel to true 
        ' if you are not wrapping the new view, to avoid keeping it alive along
        ' with a reference to its parent.
        e.Cancel = True

        ' Load the url to the existing view.
        Browser.Source = e.TargetURL
    End Sub
#End Region

End Class
