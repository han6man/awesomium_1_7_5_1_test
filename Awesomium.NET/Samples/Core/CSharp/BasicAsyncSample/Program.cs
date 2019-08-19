/********************************************************************************
 *    Project   : Awesomium.NET (BasicAsyncSample)
 *    File      : Program.cs
 *    Version   : 1.7.4.1
 *    Date      : 02/25/2014
 *    Author    : Perikles C. Stephanidis (perikles@awesomium.com)
 *    Copyright : ©2014 Awesomium Technologies LLC
 *    
 *    This code is provided "AS IS" and for demonstration purposes only,
 *    without warranty of any kind.
 *     
 *-------------------------------------------------------------------------------
 *
 *    Notes     :
 *
 *    This sample demonstrates starting Awesomium in a dedicated thread,
 *    and interacting with using the various available synchronization
 *    methods.
 *    
 *    The simple includes an example to asynchronously save snapshots of
 *    sites, and a simple JavaScript console simulation.
 *    
 *    
 ********************************************************************************/

#region Using
using System;
using System.Linq;
using Awesomium.Core;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
#endregion

namespace BasicAsyncSample
{
    class Program
    {
        #region Fields
        // JavaScript that will get a reliable value 
        // for the full height of the document loaded.
        const string PAGE_HEIGHT_FUNC = "(function() { " +
            "var bodyElmnt = document.body; var html = document.documentElement; " +
            "var height = Math.max( bodyElmnt.scrollHeight, bodyElmnt.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight ); " +
            "return height; })();";

        static volatile bool webCoreStarted;
        static volatile int savingSnapshots;
        #endregion


        #region Entry Point
        static void Main( string[] args )
        {
            Console.SetWindowSize( Math.Min( Console.LargestWindowWidth, 100 ), Math.Min( Console.LargestWindowHeight, 40 ) );
            Console.Title = String.Format( "Awesomium.NET - {0}", AssemblyInfo.AssemblyTitle );
            WriteHeader();

            Console.Write( "Starting Awesomium..." );

            // Start a dedicated thread for Awesomium.
            Thread awesomiumThread = new Thread( AwesomiumThread ) { Name = "Awesomium Thread" };
            awesomiumThread.Start();

            // Wait for the WebCore to start.
            while ( !webCoreStarted )
                Thread.Sleep( 10 );

            Console.Write( "Complete" );
            ConsoleBuffer.ClearLine( (short)Console.CursorTop );
            Console.SetCursorPosition( 0, Console.CursorTop );

            // Show menu.
            ListenUserChoice();
        }
        #endregion

        #region Awesomium Thread
        private static void AwesomiumThread()
        {
            // Initialize the WebCore with some configuration settings.
            WebCore.Initialize( new WebConfig()
            {
                LogPath = Environment.CurrentDirectory + "/awesomium.log",
                LogLevel = LogLevel.Verbose,
            } );

            // We demonstrate an easy way to hide the scrollbars by providing
            // custom CSS. Read more about how to style the scrollbars here:
            // http://www.webkit.org/blog/363/styling-scrollbars/.
            // Just consider that this setting is WebSession-wide. If you want to apply
            // a similar effect for single pages, you can use ExecuteJavascript
            // and pass: document.documentElement.style.overflow = 'hidden';
            // (Unfortunately WebKit's scrollbar does not have a DOM equivalent yet)
            WebCore.CreateWebSession( new WebPreferences() { CustomCSS = "::-webkit-scrollbar { visibility: hidden; }" } );

            // Check if the WebCore is already automatically updating.
            // This check is only here for demonstration. A background
            // thread has no message loop and synchronization context.
            if ( WebCore.UpdateState != WebCoreUpdateState.NotUpdating )
                return;

            // Tell the WebCore to create an Awesomium-specific
            // synchronization context and start an update loop.
            // The current thread will be blocked until WebCore.Shutdown
            // is called. For details about the new auto-updating and 
            // synchronization model of Awesomium.NET, read the documentation
            // of WebCore.Run.
            WebCore.Run( ( s, e ) => webCoreStarted = true );
        }
        #endregion

        #region Menu
        private static void ListenUserChoice()
        {
            Console.WriteLine( "Hit:" );
            Console.WriteLine( "\t0: to clear the Console." );
            Console.WriteLine( "\t1: to take snapshots of a URL." );
            Console.WriteLine( "\t2: to start a JavaScript console." );
            Console.WriteLine( "\t3: to exit." );

            switch ( Console.ReadKey( true ).KeyChar )
            {
                case '0':
                    if ( savingSnapshots <= 0 )
                        WriteHeader();
                    else
                        GotoMenuStart();
                    break;

                case '1':
                    GotoMenuStart();
                    ListenForURL();
                    break;

                case '2':
                    GotoMenuStart();
                    StartJavascriptConsole();
                    break;

                case '3':
                    // Shutdown the WebCore before we exit.
                    // This method is thread-safe and can be
                    // called from any thread.
                    WebCore.Shutdown();
                    return;

                default:
                    break;
            }

            ListenUserChoice();
        }

        private static void GotoMenuStart()
        {
            ConsoleBuffer.ClearLines( Console.CursorTop - 5, Console.CursorTop );
            Console.SetCursorPosition( 0, Console.CursorTop - 5 );
        }
        #endregion


        #region Snapshots
        private static void ListenForURL()
        {
            int messageLine = WriteMessageBox();

            // Print message.
            Console.Write( "Type a URL to take snapshots or type \"back\" to return to menu: " );
            // Wait for user input.
            string userInput = Console.ReadLine();

            if ( userInput.ToLower() == "back" )
            {
                // Clear Console and Reset cursor.
                ConsoleBuffer.ClearLines( Console.CursorTop - 7, Console.CursorTop );
                Console.SetCursorPosition( 0, Console.CursorTop - 7 );
                // Back to menu.
                return;
            }

            // Get the URL. ToUri is a String extension provided
            // by Awesomium.Core.Utilities that can help you easily
            // convert strings to a URL.
            Uri url = userInput.ToUri();
            // ToUri is errors-free. If an invalid string is specified,
            // a blank URI (about:blank) will be returned that can be
            // checked with IsBlank, a helper Uri extension.
            if ( url.IsBlank() )
            {
                ReplaceLine( "Badly formatted URL!", messageLine );
                // Go back.
                Console.SetCursorPosition( 0, Console.CursorTop - 1 );
                ListenForURL();
                return;
            }

            // We demonstrate using WebCore.QueueWork to queue
            // work to be executed on Awesomium's thread.
            WebCore.QueueWork( () => NavigateAndTakeSnapshots( url, messageLine ) );

            // Go back.
            Console.SetCursorPosition( 0, Console.CursorTop - 1 );
            ListenForURL();
        }

        // This is executed on Awesomium's thread.
        private static void NavigateAndTakeSnapshots( Uri url, int messageLine )
        {
            savingSnapshots++;

            // Create a view that uses our WebSession.
            WebView view = WebCore.CreateWebView( 1100, 600, WebCore.Sessions.Last() );
            ReplaceLine( String.Format( "Loading: {0} ... ", url ), messageLine );

            // Prevent new windows.
            view.ShowCreatedWebView += ( s, e ) => e.Cancel = true;

            // Respond to failures.
            view.LoadingFrameFailed += ( s, e ) =>
            {
                // We do not mind about child frames.
                if ( !e.IsMainFrame )
                    return;

                // Print some more information.
                ReplaceLine( String.Format( "Failed to load URL: {0}", view.Source ), messageLine );
                ReplaceLine( String.Format( "Error: {0}", e.ErrorCode ), messageLine + 1 );
            };

            // This event is fired when a frame in the
            // page finishes loading.
            view.LoadingFrameComplete += ( s, e ) =>
            {
                // The main frame usually finishes loading last for a given page load.
                if ( !e.IsMainFrame )
                    return;

                // Print some more information.
                ReplaceLine( String.Format( "Loaded URL: {0}", view.Source ), messageLine );

                // Take snapshots of the page.
                TakeSnapshots( (WebView)s, messageLine );
            };

            // Load the URL.
            view.Source = url;
        }

        // This is executed on Awesomium's thread.
        static void TakeSnapshots( WebView view, int messageLine, bool exit = true )
        {
            if ( !view.IsLive )
            {
                // Dispose the view.
                view.Dispose();
                return;
            }

            // Get the hostname. If it's empty, it must be our JS Console that saves.
            string host = String.IsNullOrEmpty( view.Source.Host ) ? "JS" : view.Source.Host;

            // A BitmapSurface is assigned by default to all WebViews.
            BitmapSurface surface = (BitmapSurface)view.Surface;
            // Build a name for the saved image.
            string imageFile = String.Format( "{0}.{1:yyyyMMddHHmmss}.png", host, DateTime.Now );
            // Save the buffer to a PNG image.
            surface.SaveToPNG( imageFile, true );
            // Print message.
            ReplaceLine( String.Format( "Saved: {0}", imageFile ), messageLine + 2 );

            // Check if we can execute JavaScript
            // against the DOM.
            if ( !view.IsDocumentReady )
            {
                // Print message.
                ReplaceLine( "DOM not available.", messageLine + 1 );

                if ( !exit )
                    return;

                // Dispose the view.
                view.Dispose();
                savingSnapshots--;
                return;
            }

            // We demonstrate resizing to full height.
            ReplaceLine( "Attempting to resize to full height... ", messageLine + 1 );

            // This JS call will normally return the full height
            // of the page loaded.
            int docHeight = (int)view.ExecuteJavascriptWithResult( PAGE_HEIGHT_FUNC );

            // ExecuteJavascriptWithResult is a synchronous call. Synchronous
            // calls may fail. We check for errors that may occur. Note that
            // if you often get a Error.TimedOut, you may need to set the 
            // IWebView.SynchronousMessageTimeout property to a higher value
            // (the default is 800ms).
            Error lastError = view.GetLastError();

            // Report errors.
            if ( lastError != Error.None )
                ReplaceLine( String.Format( "Error: {0} occurred while getting the page's height.", lastError ), messageLine + 1 );

            // Exit if the operation failed or the height is 0.
            if ( docHeight == 0 )
                return;

            // No more content to display.
            if ( docHeight == view.Height )
            {
                // Print message.
                ReplaceLine( "Full height already loaded.", messageLine + 1 );

                if ( !exit )
                    return;

                // Dispose the view.
                view.Dispose();
                savingSnapshots--;
                return;
            }

            // All predefined surfaces of Awesomium.NET,
            // support resizing. Here is a demonstration.
            surface.Resized += ( s, e ) =>
            {
                // Print message.
                ReplaceLine( "Surface Resized", messageLine + 1 );
                // Build a name for the saved image.
                string fullImageFile = String.Format( "{0}.{1:yyyyMMddHHmmss}.full.png", host, DateTime.Now );
                // Save the updated buffer to a new PNG image.
                surface.SaveToPNG( fullImageFile, true );
                // Print message.
                ReplaceLine( String.Format( "Saved: {0}", fullImageFile ), messageLine + 2 );

                // Get Awesomium's synchronization context. You can only
                // acquire this from Awesomium's thread, but you can then
                // cache it or pass it to another thread to be used for
                // safe cross-thread calls to Awesomium. Here we just
                // demonstrate using Post to postpone the view's destruction
                // till the next update pass of the WebCore.
                SynchronizationContext syncCtx = SynchronizationContext.Current;

                // Check if a valid SynchronizationContext is available.
                if ( ( syncCtx != null ) && ( syncCtx.GetType() != typeof( SynchronizationContext ) ) )
                {
                    // Queue the destruction of the view. Code execution
                    // will resume immediately (so we exit the event handler),
                    // and the anonymous handler will be executed in the
                    // next update pass.
                    syncCtx.Post( ( v ) =>
                    {
                        WebView completedView = (WebView)v;

                        if ( !exit )
                            return;

                        // Dispose the view.
                        completedView.Dispose();
                        savingSnapshots--;
                    }, view );
                }
            };

            // Call resize on the view. This will resize
            // and update the surface.
            view.Resize( view.Width, docHeight );
        }
        #endregion

        #region JavaScript Console
        const string simpleHTML = @"<html><body></body><html>";

        private static void StartJavascriptConsole()
        {
            // Queue the creation of the view and wait for it.
            // Notice that we pass a reference to the new view,
            // back to the application's thread. We must then use
            // the view's implementation of ISynchronizeInvoke,
            // to safely execute code on the view.
            WebView view = WebCore.DoWork( () => WebCore.CreateWebView( 640, 480 ) );

            // Listen for Javascript console messages.
            view.ConsoleMessage += OnConsoleMessage;
            // Prevent spawning new windows from this view.
            view.ShowCreatedWebView += ( s, e ) => e.Cancel = true;

            // This check is here only for demonstration.
            // Awesomium is running on its own thread and
            // attempting to access the view from another thread,
            // requires that we use Invoke/BeginInvoke so 
            // InvokeRequired will return true.
            if ( view.InvokeRequired )
                view.Invoke( (Func<String, Boolean>)view.LoadHTML, simpleHTML );
            else
                view.LoadHTML( simpleHTML );

            Console.WriteLine( "Console started" );
            Console.WriteLine();
            Console.WriteLine( "\t- Type \"back\" to return to menu." );
            Console.WriteLine( "\t- Type \"save\" to get snapshots of the page and return to menu." );
            Console.WriteLine();

            // Start the Javascript console simulation.
            ListenForJavascript( view, Console.CursorTop - 5 );
        }

        // This is executed on Awesomium's thread.
        private static void OnConsoleMessage( object sender, ConsoleMessageEventArgs e )
        {
            // It's safe to write to the console from another thread.
            // If this was a UI application, you would have to marshal
            // the message's printing back to the UI thread.
            Console.WriteLine( e.Message );
            Console.Write( "> " );
        }

        private static void ListenForJavascript( WebView view, int startLine )
        {
            Console.Write( "> " );
            string userInput = Console.ReadLine();

            if ( userInput.ToLower() == "back" )
            {
                // Queue the destruction of the view and return.
                WebCore.QueueWork( view, view.Dispose );
                // Clear Console and Reset cursor.
                ConsoleBuffer.ClearLines( startLine, Console.CursorTop );
                Console.SetCursorPosition( 0, startLine );
                // Back to menu.
                return;
            }
            else if ( userInput.ToLower() == "save" )
            {
                int messageLine = WriteMessageBox();
                ReplaceLine( "Saving JavaScript Console Page", messageLine );

                // Queue TakeSnapshots to be executed on Awesomium's thread.
                WebCore.QueueWork( () => TakeSnapshots( view, messageLine, false ) );
                // Back to JavaScript Console.
                ListenForJavascript( view, startLine );
                return;
            }

            // This check is here only for demonstration.
            // Awesomium is running on its own thread and
            // attempting to access the view from another thread,
            // requires that we use Invoke/BeginInvoke so 
            // InvokeRequired will return true.
            string result = view.InvokeRequired ?
                (string)view.Invoke( (Func<WebView, String, String>)ExecuteJavascript, view, userInput ) :
                ExecuteJavascript( view, userInput );

            // If we got a valid result, print it.
            if ( !String.IsNullOrEmpty( result ) )
                Console.WriteLine( "> " + result );

            // Loop until the user exits the JS console.
            ListenForJavascript( view, startLine );
        }

        // This is executed on Awesomium's thread.
        private static string ExecuteJavascript( WebView view, string js )
        {
            // JSValue supports implicit casting to and from the supported
            // types so we could actually assign directly to string.
            // In this sample however, we need to check for undefined.
            JSValue result = view.ExecuteJavascriptWithResult( js );

            if ( result.IsUndefined )
                return "[undefined]";
            else
                return result; // Implicit casting here.
        }
        #endregion


        #region Helpers
        private static void WriteHeader()
        {
            Console.Clear();
            Console.WriteLine( String.Format( "{0} v{1}", AssemblyInfo.AssemblyTitle, AssemblyInfo.AssemblyVersion ) );
            Console.WriteLine( AssemblyInfo.AssemblyCopyright );
            Console.WriteLine();
        }

        private static int WriteMessageBox()
        {
            FillConsoleLine( '=', Console.CursorTop );
            Console.WriteLine();
            int messageLine = Console.CursorTop;
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            FillConsoleLine( '=', Console.CursorTop );
            Console.WriteLine();
            Console.WriteLine();
            return messageLine;
        }

        private static void FillConsoleLine( char c, int line )
        {
            ConsoleBuffer.FillLine( c, line );
            Console.SetCursorPosition( 0, line );
        }

        private static void ReplaceLine( string text, int line )
        {
            text = text.Length > Console.WindowWidth ? text.Substring( 0, Console.WindowWidth - 3 ) + "..." : text;
            ConsoleBuffer.ClearLine( (short)line );
            ConsoleBuffer.WriteAt( 0, (short)line, text );
        }
        #endregion
    }

    #region ConsoleBuffer
    static class ConsoleBuffer
    {
        #region P/Invoke
        private static SafeFileHandle _hBuffer = null;
        private const int STD_OUTPUT_HANDLE = -11;

        [DllImport( "kernel32.dll", SetLastError = true )]
        private static extern SafeFileHandle GetStdHandle( int nStdHandle );

        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool CloseHandle( IntPtr hObject );

        [DllImport( "kernel32.dll", SetLastError = true )]
        private static extern bool WriteConsoleOutputCharacterA(
          SafeFileHandle hConsoleOutput,
          string lpCharacter,
          int nLength,
          Coord dwWriteCoord,
          ref int lpumberOfCharsWritten );

        [StructLayout( LayoutKind.Sequential )]
        private struct Coord
        {
            public short X;
            public short Y;

            public Coord( short X, short Y )
            {
                this.X = X;
                this.Y = Y;
            }
        }
        #endregion

        #region Ctors
        static ConsoleBuffer()
        {
            _hBuffer = GetStdHandle( STD_OUTPUT_HANDLE );

            if ( _hBuffer.IsInvalid )
                throw new Exception( "Failed to open console buffer" );
        }
        #endregion

        #region Methods
        public static void WriteAt( short x, short y, string value )
        {
            int n = 0;
            WriteConsoleOutputCharacterA( _hBuffer, value, value.Length, new Coord( x, y ), ref n );
        }

        public static void FillLine( char c, int top )
        {
            int n = 0;
            string spaces = new String( c, Console.WindowWidth );
            WriteConsoleOutputCharacterA( _hBuffer, spaces, spaces.Length, new Coord( 0, (short)top ), ref n );
        }

        public static void ClearLine( int top )
        {
            int n = 0;
            string spaces = new String( ' ', Console.WindowWidth );
            WriteConsoleOutputCharacterA( _hBuffer, spaces, spaces.Length, new Coord( 0, (short)top ), ref n );
        }

        public static void ClearLines( int start, int end )
        {
            int n = 0;
            string spaces = new String( ' ', Console.WindowWidth );
            for ( int i = start; i <= end; i++ )
                WriteConsoleOutputCharacterA( _hBuffer, spaces, spaces.Length, new Coord( 0, (short)i ), ref n );
        }
        #endregion
    }
    #endregion
}
