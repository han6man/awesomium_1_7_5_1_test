'*******************************************************************************
'    Project   : Awesomium.NET (BasicVBSample)
'    File      : Module1.vb
'    Version   : 1.7.0.0 
'    Date      : 4/13/2013
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
'    Simple sample that demonstrates taking a snapshot of a site and saving
'    it to an image file.
'     
'     Things you should note about this sample:
'     
'       1. This is a Console application. In applications without UI
'          we call WebCore.Run which tells the WebCore to create an
'          Awesomium-specific synchronization context and start an
'          auto-update loop.
'          
'       2. This sample also demonstrates taking a snapshot of the site's
'          full height. After the first snapshot is saved, we attempt to
'          resize the view to the page's full height and when this is 
'          complete, we get another snapshot.
'    
'    
'*******************************************************************************

Imports Awesomium.Core

Module Module1

    ' JavaScript that will get a reliable value 
    ' for the full height of the document loaded.
    Private Const PAGE_HEIGHT_FUNC As String = "(function() { " &
        "var bodyElmnt = document.body; var html = document.documentElement; " &
        "var height = Math.max( bodyElmnt.scrollHeight, bodyElmnt.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight ); " &
        "return height; })();"

    Sub Main(args() As String)
        Console.Title = My.Application.Info.Title

        ' Initialize the WebCore with some configuration settings.
        WebCore.Initialize(New WebConfig() With {
            .LogPath = Environment.CurrentDirectory,
            .LogLevel = LogLevel.Verbose
        })

        ' Attempt get the URL from the command line,
        ' or use the default for demonstration.
        Dim url As Uri = If((args IsNot Nothing) AndAlso
                            (args.Length > 0) AndAlso
                            Not String.IsNullOrEmpty(args(0)) AndAlso
                            (args(0).ToUri() IsNot Nothing) AndAlso
                            (Not args(0).ToUri().IsBlank()),
                            args(0).ToUri(),
                            "http://www.awesomium.com".ToUri())

        ' Take snapshots of the site.
        NavigateAndTakeSnapshots(url)
    End Sub

    Private Sub NavigateAndTakeSnapshots(url As Uri)
        ' We demonstrate an easy way to hide the scrollbars by providing
        ' custom CSS. Read more about how to style the scrollbars here:
        ' http://www.webkit.org/blog/363/styling-scrollbars/.
        ' Just consider that this setting is WebSession-wide. If you want to apply
        ' a similar effect for single pages, you can use ExecuteJavascript
        ' and pass: document.documentElement.style.overflow = 'hidden';
        ' (Unfortunately WebKit's scrollbar does not have a DOM equivalent yet)
        Using session As WebSession = WebCore.CreateWebSession(New WebPreferences() With {.CustomCSS = "::-webkit-scrollbar { visibility: hidden; }"})
            ' WebView implements IDisposable. Here we demonstrate
            ' wrapping it in a using statement.
            Using view As WebView = WebCore.CreateWebView(1100, 600, session)
                Console.WriteLine(String.Format("Loading: {0} ...", url))

                ' Load a URL.
                view.Source = url

                ' This event is fired when a frame in the
                ' page finished loading.
                AddHandler view.LoadingFrameComplete,
                    Sub(s, e)
                        Console.WriteLine([String].Format("Frame Loaded: {0}", e.FrameId))

                        ' The main frame usually finishes loading last for a given page load.
                        If Not e.IsMainFrame Then
                            Return
                        End If

                        ' Print some more information.
                        Console.WriteLine(String.Format("Page Title: {0}", view.Title))
                        Console.WriteLine(String.Format("Loaded URL: {0}", view.Source))

                        ' Take snapshots of the page.
                        TakeSnapshots(CType(s, WebView))
                    End Sub

                ' Check if the WebCore is already automatically updating.
                ' This check is only here for demonstration. Console applications
                ' are not UI applications and have no synchronization context.
                ' Without a valid synchronization context, we need to call Run.
                ' This will tell the WebCore to create an Awesomium-specific
                ' synchronization context and start an update loop.
                ' The current thread will be blocked until WebCore.Shutdown
                ' is called. You can use the same model by creating a dedicated
                ' thread for Awesomium. For details about the new auto-updating
                ' and synchronization model of Awesomium.NET, read the documentation
                ' of WebCore.Run.
                If WebCore.UpdateState = WebCoreUpdateState.NotUpdating Then
                    ' The point of no return. This will only exit
                    ' when we call Shutdown.
                    WebCore.Run()
                End If
            End Using
        End Using
    End Sub

    Private Sub TakeSnapshots(view As WebView)
        ' A BitmapSurface is assigned by default to all WebViews.
        Dim surface As BitmapSurface = DirectCast(view.Surface, BitmapSurface)
        ' Save the buffer to a PNG image.
        surface.SaveToPNG("result.png", True)

        ' Show image.
        ShowImage("result.png")

        ' We demonstrate resizing to full height.
        Console.WriteLine("Attempting to resize to full height...")

        ' This JS call will normally return the full height
        ' of the page loaded.
        Dim docHeight As Integer = CInt(view.ExecuteJavascriptWithResult(PAGE_HEIGHT_FUNC))

        ' ExecuteJavascriptWithResult is a synchronous call. Synchronous
        ' calls may fail. We check for errors that may occur. Note that
        ' if you often get a Error.TimedOut, you may need to set the 
        ' IWebView.SynchronousMessageTimeout property to a higher value
        ' (the default is 800ms).
        Dim lastError As [Error] = view.GetLastError()

        ' Report errors.
        If lastError <> [Error].None Then
            Console.WriteLine(String.Format("Error: {0} occurred while getting the page's height.", lastError))
        End If

        ' Exit if the operation failed or the height is 0.
        If docHeight = 0 Then
            Return
        End If

        ' All predefined surfaces of Awesomium.NET,
        ' support resizing. Here is a demonstration.
        AddHandler surface.Resized,
            Sub(s, e)
                Console.WriteLine("Surface Resized")

                ' Save the updated buffer to a new PNG image.
                surface.SaveToPNG("result2.png", True)
                ' Show image.
                ShowImage("result2.png")

                ' Exit the update loop and shutdown the core.
                WebCore.Shutdown()

                ' Note that when Shutdown is called from
                ' Awesomium's thread, anything after Shutdown
                ' will not be executed since the thread exits
                ' immediately.
            End Sub

        ' Call resize on the view. This will resize
        ' and update the surface.
        view.Resize(view.Width, docHeight)
    End Sub

    Private Sub ShowImage(imageFile As String)
        ' Announce.
        Console.WriteLine("Hit any key to see the result...")
        Console.ReadKey(True)

        ' Start the application associated with .png files
        ' and display the file.
        Process.Start(imageFile)
    End Sub

End Module
