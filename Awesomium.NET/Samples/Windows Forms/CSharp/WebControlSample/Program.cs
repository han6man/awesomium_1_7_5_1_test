/********************************************************************************
 *    Project   : Awesomium.NET (TabbedFormsSample)
 *    File      : Program.cs
 *    Version   : 1.7.0.0 
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
 *    Entry point of the TabbedFormsSample application. This file also
 *    includes the global ShowCreatedWebView event handler.
 *    
 *    
 ********************************************************************************/

#region Using
using System;
using System.IO;
using System.Linq;
using Awesomium.Core;
using System.Drawing;
using System.Windows.Forms;
using Awesomium.Windows.Forms;
#endregion

namespace TabbedFormsSample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main( string[] args )
        {
            // Force single instance application.
            SingleInstance.Make( SecondInstance );

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            // Set some initialization settings.
            WebConfig webConfig = new WebConfig()
            {
                // For this sample, we use our custom managed child process
                // as Awesomium rendering process. See how we create this
                // under the Core\CSharp solution folder (CustomProcess).
                ChildProcessPath = String.Format( "{0}{1}CustomAwesomiumProcess.exe", My.Application.Info.DirectoryPath, Path.DirectorySeparatorChar ),
                HomeURL = new Uri( "http://www.google.com" ),
                // Let's gather some extra info for this sample.
                LogLevel = LogLevel.Verbose
            };

            // Initialize the core. This performs lazy initialization.
            // The core will actually be initialized, when the first
            // view (WebControl) or WebSession is created.
            WebCore.Initialize( webConfig );

            Application.ApplicationExit += OnApplicationExit;
            Application.Run( new MainForm() );
        }

        private static void OnApplicationExit( object sender, EventArgs e )
        {
            // Make sure we shutdown the core last.
            if ( WebCore.IsInitialized )
                WebCore.Shutdown();
        }

        private static void SecondInstance( object obj )
        {
            var forms = Application.OpenForms.OfType<MainForm>();

            if ( forms.Count() > 0 )
                forms.First().BeginInvoke( (Action<MainForm>)RestoreMainForm, forms.First() );
        }

        private static void RestoreMainForm( MainForm mainForm )
        {
            if ( mainForm.WindowState == FormWindowState.Minimized )
                mainForm.WindowState = FormWindowState.Normal;

            mainForm.Focus();
            mainForm.OpenTab();
        }

        // Common handler of all ShowCreatedWebView events.
        internal static void OnShowNewView( object sender, ShowCreatedWebViewEventArgs e )
        {
            IWebView view = sender as IWebView;

            if ( view == null )
                return;

            if ( !view.IsLive )
                return;

            MainForm mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();

            if ( mainForm == null )
                return;

            // Treat popups differently. If IsPopup is true, the event is always
            // the result of 'window.open' (IsWindowOpen is also true, so no need to check it).
            // Our application does not recognize user defined, non-standard specs. 
            // Therefore child views opened with non-standard specs, will not be presented as 
            // popups but as regular new windows (still wrapping the child view however -- see below).
            if ( e.IsPopup && !e.IsUserSpecsOnly )
            {
                // JSWindowOpenSpecs.InitialPosition indicates screen coordinates.
                Rectangle screenRect = e.Specs.InitialPosition.ToRectangle();

                // Set the created native view as the underlying view of the
                // WebControl. This will maintain the relationship between
                // the parent view and the child, usually required when the new view
                // is the result of 'window.open' (JS can access the parent window through
                // 'window.opener'; the parent window can manipulate the child through the 'window'
                // object returned from the 'window.open' call).
                WebDocument newWindow = new WebDocument( e.NewViewInstance )
                {
                    ShowInTaskbar = false,
                    ClientSize = screenRect.Size != Size.Empty ? screenRect.Size : new Size( 640, 480 )
                };

                // If the caller has not indicated a valid size for the new popup window,
                // let it be opened with the default size specified at design time.
                if ( ( screenRect.Width > 0 ) && ( screenRect.Height > 0 ) )
                {
                    // Assign the indicated size.
                    newWindow.Width = screenRect.Width;
                    newWindow.Height = screenRect.Height;
                }

                // Show the window.
                newWindow.Show();

                // If the caller has not indicated a valid position for the new popup window,
                // let it be opened in the default position specified at design time.
                if ( screenRect.Location != Point.Empty )
                    // Move it to the specified coordinates.
                    newWindow.DesktopLocation = screenRect.Location;
            }
            else if ( e.IsWindowOpen || e.IsPost )
            {
                // No specs or only non-standard specs were specified, but the event is still 
                // the result of 'window.open' or of an HTML form with target="_blank" and method="post".
                // We will open a normal window but we will still wrap the new native child view, 
                // maintaining its relationship with the parent window.
                WebDocument doc = new WebDocument( e.NewViewInstance );
                mainForm.OpenTab( doc );
            }
            else
            {
                // The event is not the result of 'window.open' or of an HTML form with target="_blank" 
                // and method="post"., therefore it's most probably the result of a link with target='_blank'. 
                // We will not be wrapping the created view; we let the WebControl hosted in ChildWindow 
                // create its own underlying view. Setting Cancel to true tells the core to destroy the 
                // created child view.
                //
                // Why don't we always wrap the native view passed to ShowCreatedWebView?
                //
                // - In order to maintain the relationship with their parent view,
                // child views execute and render under the same process (awesomium_process)
                // as their parent view. If for any reason this child process crashes,
                // all views related to it will be affected. When maintaining a parent-child 
                // relationship is not important, we prefer taking advantage of the isolated process 
                // architecture of Awesomium and let each view be rendered in a separate process.
                e.Cancel = true;
                // Note that we only explicitly navigate to the target URL, when a new view is 
                // about to be created, not when we wrap the created child view. This is because 
                // navigation to the target URL (if any), is already queued on created child views. 
                // We must not interrupt this navigation as we would still be breaking the parent-child
                // relationship.
                WebDocument doc = new WebDocument( e.TargetURL );
                mainForm.OpenTab( doc );
            }
        }
    }
}
