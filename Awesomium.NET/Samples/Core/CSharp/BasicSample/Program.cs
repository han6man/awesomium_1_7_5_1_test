/********************************************************************************
 *    Project   : Awesomium.NET (BasicSample)
 *    File      : Program.cs
 *    Version   : 1.7.4.1
 *    Date      : 3/5/2013
 *    Author    : Perikles C. Stephanidis (perikles@awesomium.com)
 *    Copyright : ©2013 Awesomium Technologies LLC
 *    
 *    This code is provided "AS IS" and for demonstration purposes only,
 *    without warranty of any kind.
 *    
 *-------------------------------------------------------------------------------
 *
 *    Notes     :
 *
 *    Simple sample that demonstrates taking a snapshot of a site and saving
 *    it to an image file.
 *    
 *    Things you should note about this sample:
 *    
 *      1. This is a Console application. In applications without UI
 *         we call WebCore.Run which tells the WebCore to create an
 *         Awesomium-specific synchronization context and start an
 *         auto-update loop.
 *         
 *      2. This sample also demonstrates taking a snapshot of the site's
 *         full height. After the first snapshot is saved, we attempt to
 *         resize the view to the page's full height and when this is complete,
 *         we get another snapshot.
 * 
 *    
 *    
 ********************************************************************************/

using System;
using Awesomium.Core;
using System.Diagnostics;
using System.Reflection;

namespace BasicSample
{
    class Program
    {
        // JavaScript that will get a reliable value 
        // for the full height of the document loaded.
        const string PAGE_HEIGHT_FUNC = "(function() { " +
            "var bodyElmnt = document.body; var html = document.documentElement; " +
            "var height = Math.max( bodyElmnt.scrollHeight, bodyElmnt.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight ); " +
            "return height; })();";

        static void Main( string[] args )
        {
            Console.Title = ( (AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyTitleAttribute ), false )[ 0 ] ).Title;

            // Initialize the WebCore with some configuration settings.
            WebCore.Initialize( new WebConfig()
            {
                LogPath = Environment.CurrentDirectory + "/awesomium.log",
                LogLevel = LogLevel.Verbose,
            } );

            // Attempt get the URL from the command line,
            // or use the default for demonstration.
            Uri url = ( args != null ) &&
                ( args.Length > 0 ) &&
                !String.IsNullOrEmpty( args[ 0 ] ) &&
                ( args[ 0 ].ToUri() != null ) &&
                ( !args[ 0 ].ToUri().IsBlank() ) ?
                args[ 0 ].ToUri() :
                "http://www.awesomium.com".ToUri();

            // Take snapshots of the site.
            NavigateAndTakeSnapshots( url );
        }

        static void NavigateAndTakeSnapshots( Uri url )
        {
            // We demonstrate an easy way to hide the scrollbars by providing
            // custom CSS. Read more about how to style the scrollbars here:
            // http://www.webkit.org/blog/363/styling-scrollbars/.
            // Just consider that this setting is WebSession-wide. If you want to apply
            // a similar effect for single pages, you can use ExecuteJavascript
            // and pass: document.documentElement.style.overflow = 'hidden';
            // (Unfortunately WebKit's scrollbar does not have a DOM equivalent yet)
            using ( WebSession session = WebCore.CreateWebSession( new WebPreferences() { CustomCSS = "::-webkit-scrollbar { visibility: hidden; }" } ) )
            {
                // WebView implements IDisposable. Here we demonstrate
                // wrapping it in a using statement.
                using ( WebView view = WebCore.CreateWebView( 1100, 600, session ) )
                {
                    Console.WriteLine( String.Format( "Loading: {0} ...", url ) );

                    // Load a URL.
                    view.Source = url;

                    // This event is fired when a frame in the
                    // page finishes loading.
                    view.LoadingFrameComplete += ( s, e ) =>
                    {
                        Console.WriteLine( String.Format( "Frame Loaded: {0}", e.FrameId ) );

                        // The main frame usually finishes loading last for a given page load.
                        if ( !e.IsMainFrame )
                            return;

                        // Print some more information.
                        Console.WriteLine( String.Format( "Page Title: {0}", view.Title ) );
                        Console.WriteLine( String.Format( "Loaded URL: {0}", view.Source ) );

                        // Take snapshots of the page.
                        TakeSnapshots( (WebView)s );
                    };

                    // Check if the WebCore is already automatically updating.
                    // This check is only here for demonstration. Console applications
                    // are not UI applications and have no synchronization context.
                    // Without a valid synchronization context, we need to call Run.
                    // This will tell the WebCore to create an Awesomium-specific
                    // synchronization context and start an update loop.
                    // The current thread will be blocked until WebCore.Shutdown
                    // is called. You can use the same model by creating a dedicated
                    // thread for Awesomium. For details about the new auto-updating
                    // and synchronization model of Awesomium.NET, read the documentation
                    // of WebCore.Run.
                    if ( WebCore.UpdateState == WebCoreUpdateState.NotUpdating )
                        // The point of no return. This will only exit
                        // when we call Shutdown.
                        WebCore.Run();
                }
            }
        }

        static void TakeSnapshots( WebView view )
        {
            // A BitmapSurface is assigned by default to all WebViews.
            BitmapSurface surface = (BitmapSurface)view.Surface;
            // Save the buffer to a PNG image.
            surface.SaveToPNG( "result.png", true );

            // Show image.
            ShowImage( "result.png" );

            // We demonstrate resizing to full height.
            Console.WriteLine( "Attempting to resize to full height..." );

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
                Console.WriteLine( String.Format( "Error: {0} occurred while getting the page's height.", lastError ) );

            // Exit if the operation failed or the height is 0.
            if ( docHeight == 0 )
                return;

            // All predefined surfaces of Awesomium.NET,
            // support resizing. Here is a demonstration.
            surface.Resized += ( s, e ) =>
            {
                Console.WriteLine( "Surface Resized" );

                // Save the updated buffer to a new PNG image.
                surface.SaveToPNG( "result2.png", true );
                // Show image.
                ShowImage( "result2.png" );

                // Exit the update loop and shutdown the core.
                WebCore.Shutdown();

                // Note that when Shutdown is called from
                // Awesomium's thread, anything after Shutdown
                // will not be executed since the thread exits
                // immediately.
            };

            // Call resize on the view. This will resize
            // and update the surface.
            view.Resize( view.Width, docHeight );
        }

        static void ShowImage( string imageFile )
        {
            // Announce.
            Console.WriteLine( "Hit any key to see the result..." );
            Console.ReadKey( true );

            // Start the application associated with .png files
            // and display the file.
            Process.Start( imageFile );
        }
    }
}
