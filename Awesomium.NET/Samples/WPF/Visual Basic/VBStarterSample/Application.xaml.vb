Imports Awesomium.Core

Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    Protected Overrides Sub OnStartup(e As StartupEventArgs)
        ' Initialization must be performed here,
        ' before creating a WebControl.
        If Not WebCore.IsInitialized Then
            WebCore.Initialize(New WebConfig() With
            {
                .HomeURL = "http://www.awesomium.com".ToUri(),
                .LogPath = ".\starter.log",
                .LogLevel = LogLevel.Verbose,
                .RemoteDebuggingPort = 9033
            })
        End If

        MyBase.OnStartup(e)
    End Sub

    Protected Overrides Sub OnExit(e As ExitEventArgs)
        ' Make sure we shutdown the core last.
        If WebCore.IsInitialized Then
            WebCore.Shutdown()
        End If

        MyBase.OnExit(e)
    End Sub

End Class
