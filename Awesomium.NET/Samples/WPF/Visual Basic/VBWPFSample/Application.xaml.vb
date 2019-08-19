Imports Awesomium.Core

Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    Protected Overrides Sub OnExit(e As ExitEventArgs)
        If WebCore.IsInitialized Then
            WebCore.Shutdown()
        End If

        MyBase.OnExit(e)
    End Sub

End Class
