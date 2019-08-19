'*******************************************************************************
'    Project   : Awesomium.NET (VBWebControlSample)
'    File      : ApplicationEvents.vb
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
'    Application exit.
'    
'    
'*******************************************************************************

Imports Awesomium.Core

Namespace My

    ' The following events are available for MyApplication:
    ' 
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
    Partial Friend Class MyApplication

        Private Sub MyApplication_Shutdown(sender As Object, e As EventArgs) Handles Me.Shutdown
            If WebCore.IsInitialized Then
                WebCore.Shutdown()
            End If
        End Sub

    End Class


End Namespace

