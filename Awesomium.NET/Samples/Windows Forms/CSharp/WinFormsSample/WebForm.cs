/********************************************************************************
 *    Project   : Awesomium.NET (WinFormsSample)
 *    File      : WebForm.cs
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
 *    Demonstrates rendering an Offscreen Awesomium WebView to a 
 *    Windows Forms UI.
 *  
 *    This sample also demonstrates:
 *    
 *      - The use of IResourceInterceptor to provide
 *        local resources.
 *      - How to customize the WebControlContextMenu
 *        and in this scenario, also use it with a WebView.
 *    
 *    
 ********************************************************************************/

#region Using
using System;
using System.IO;
using Awesomium.Core;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using Awesomium.Windows.Forms;
using System.Runtime.InteropServices;
using Awesomium.Core.Data;
using System.Threading;
#endregion

namespace WinFormsSample
{
    public partial class WebForm : Form, IResourceInterceptor
    {
        #region Fields
        private WebView webView;
        private ImageSurface surface;
        private WebSession session;
        private BindingSource bindingSource;
        #endregion


        #region Ctors
        public WebForm()
        {
            // Initialize the core and get a WebSession.
            WebSession session = InitializeCoreAndSession();

            // Notice that 'Control.DoubleBuffered' has been set to true
            // in the designer, to prevent flickering.

            InitializeComponent();

            // Initialize a new view.
            InitializeView( WebCore.CreateWebView( this.ClientSize.Width, this.ClientSize.Height, session ) );
        }

        public WebForm( Uri targetURL )
        {
            // Initialize the core and get a WebSession.
            WebSession session = InitializeCoreAndSession();

            // Notice that 'Control.DoubleBuffered' has been set to true
            // in the designer, to prevent flickering.

            InitializeComponent();

            // Initialize a new view.
            InitializeView( WebCore.CreateWebView( this.ClientSize.Width, this.ClientSize.Height, session ), false, targetURL );
        }

        // Used to create child (popup) windows.
        internal WebForm( WebView view, int width, int height )
        {
            this.Width = width;
            this.Height = height;

            InitializeComponent();

            // Initialize the view.
            InitializeView( view, true );

            // We should immediately call a resize,
            // after wrapping child views.
            if ( view != null )
                view.Resize( width, height );
        }
        #endregion


        #region Methods
        private WebSession InitializeCoreAndSession()
        {
            if ( !WebCore.IsInitialized )
                WebCore.Initialize( new WebConfig()
                {
                    AssetProtocol = "https",
                    LogLevel = LogLevel.Normal
                } );

            // Build a data path string. In this case, a Cache folder under our executing directory.
            // - If the folder does not exist, it will be created.
            // - The path should always point to a writeable location.
            string dataPath = String.Format( "{0}{1}Cache", Path.GetDirectoryName( Application.ExecutablePath ), Path.DirectorySeparatorChar );

            // Check if a session synchronizing to this data path, is already created;
            // if not, create a new one.
            session = WebCore.Sessions[ dataPath ] ??
                WebCore.CreateWebSession( dataPath, new WebPreferences() { } );

            session.AddDataSource( DataSource.CATCH_ALL, new MyDataSource() );

            // The core must be initialized by now. Print the core version.
            Debug.Print( WebCore.Version.ToString() );

            // Return the session.
            return session;
        }

        private void InitializeView( WebView view, bool isChild = false, Uri targetURL = null )
        {
            if ( view == null )
                return;

            // We demonstrate the use of a resource interceptor.
            if ( WebCore.ResourceInterceptor == null )
                WebCore.ResourceInterceptor = this;

            // Create an image surface to render the
            // WebView's pixel buffer.
            surface = new ImageSurface();
            surface.Updated += OnSurfaceUpdated;

            webView = view;

            // Assign our surface.
            webView.Surface = surface;
            // Assign a context menu.
            webControlContextMenu.View = webView;

            // Handle some important events.
            webView.CursorChanged += OnCursorChanged;
            webView.AddressChanged += OnAddressChanged;
            webView.ShowCreatedWebView += OnShowNewView;
            webView.ShowContextMenu += OnShowContextMenu;
            webView.PrintRequest += OnPrintRequest;
            webView.PrintComplete += OnPrintComplete;
            webView.PrintFailed += OnPrintFailed;
            webView.Crashed += OnCrashed;
            webView.ShowJavascriptDialog += OnJavascriptDialog;
            webView.WindowClose += OnWindowClose;

            // We demonstrate binding to properties.
            bindingSource = new BindingSource() { DataSource = webView };
            this.DataBindings.Add( new Binding( "Text", bindingSource, "Title", true ) );

            if ( !isChild )
                // Tip: /ncr = No Country Redirect ;-)
                webView.Source = targetURL ?? new Uri( "http://www.google.com/ncr" );

            // Give focus to the view.
            webView.FocusView();
        }

        protected override void OnPaint( PaintEventArgs e )
        {
            if ( ( surface != null ) && ( surface.Image != null ) )
                e.Graphics.DrawImageUnscaled( surface.Image, 0, 0 );
            else
                base.OnPaint( e );
        }

        protected override void OnActivated( EventArgs e )
        {
            base.OnActivated( e );
            this.Opacity = 1.0D;

            if ( ( webView == null ) || !webView.IsLive )
                return;

            webView.FocusView();
        }

        protected override void OnDeactivate( EventArgs e )
        {
            base.OnDeactivate( e );

            if ( ( webView == null ) || !webView.IsLive )
                return;

            // Let popup windows be semi-transparent,
            // when they are not active.
            if ( webView.ParentView != null )
                this.Opacity = 0.8D;

            webView.UnfocusView();
        }

        protected override void OnFormClosed( FormClosedEventArgs e )
        {
            // Destroy the WebView.
            if ( webView != null )
            {
                webView.Dispose();
                webView = null;
            }

            // Destroy our customized Context Menu.
            if ( webControlContextMenu != null )
            {
                webControlContextMenu.Dispose();
                webControlContextMenu = null;
            }

            // The surface that is currently assigned to the view,
            // does not need to be disposed. It will be disposed 
            // internally.

            base.OnFormClosed( e );

            // For WebCore.Shutdown, see OnApplicationExit in Program.cs.
        }

        protected override void OnResize( EventArgs e )
        {
            base.OnResize( e );

            if ( ( webView == null ) || !webView.IsLive )
                return;

            // Never resize the view to a width or height equal to 0;
            // instead, you can pause internal rendering.
            webView.IsRendering = ( this.ClientSize.Width > 0 ) && ( this.ClientSize.Height > 0 );

            if ( webView.IsRendering )
                // Request a resize.
                webView.Resize( this.ClientSize.Width, this.ClientSize.Height );
        }

        protected override void OnKeyPress( KeyPressEventArgs e )
        {
            base.OnKeyPress( e );

            if ( ( webView == null ) || !webView.IsLive )
                return;

            webView.InjectKeyboardEvent( e.GetKeyboardEvent() );
        }

        protected override void OnKeyDown( KeyEventArgs e )
        {
            base.OnKeyDown( e );

            if ( ( webView == null ) || !webView.IsLive )
                return;

            webView.InjectKeyboardEvent( e.GetKeyboardEvent( WebKeyboardEventType.KeyDown ) );
        }

        protected override void OnKeyUp( KeyEventArgs e )
        {
            base.OnKeyUp( e );

            if ( ( webView == null ) || !webView.IsLive )
                return;

            webView.InjectKeyboardEvent( e.GetKeyboardEvent( WebKeyboardEventType.KeyUp ) );
        }

        protected override void OnMouseDown( MouseEventArgs e )
        {
            base.OnMouseDown( e );

            if ( ( webView == null ) || !webView.IsLive )
                return;

            webView.InjectMouseDown( e.Button.GetMouseButton() );
        }

        protected override void OnMouseUp( MouseEventArgs e )
        {
            base.OnMouseUp( e );

            if ( ( webView == null ) || !webView.IsLive )
                return;

            webView.InjectMouseUp( e.Button.GetMouseButton() );
        }

        protected override void OnMouseMove( MouseEventArgs e )
        {
            base.OnMouseMove( e );

            if ( ( webView == null ) || !webView.IsLive )
                return;

            webView.InjectMouseMove( e.X, e.Y );
        }

        protected override void OnMouseWheel( MouseEventArgs e )
        {
            base.OnMouseWheel( e );

            if ( ( webView == null ) || !webView.IsLive )
                return;

            webView.InjectMouseWheel( e.Delta, 0 );
        }
        #endregion

        #region Event Handlers
        private void OnAddressChanged( object sender, UrlEventArgs e )
        {
            // Reflect the current URL to the window text.
            // Normally, after the page loads, we will get a title.
            // But a page may as well not specify a title.
            this.Text = e.Url.AbsoluteUri;
        }

        private void OnCursorChanged( object sender, CursorChangedEventArgs e )
        {
            // Update the cursor.
            this.Cursor = Awesomium.Windows.Forms.Utilities.GetCursor( e.CursorType );
        }

        private void OnSurfaceUpdated( object sender, SurfaceUpdatedEventArgs e )
        {
            // When the surface is updated, invalidate the 'dirty' region.
            // This will force the form to repaint that region.
            Invalidate( e.DirtyRegion.ToRectangle(), false );
        }

        private void OnShowContextMenu( object sender, ContextMenuEventArgs e )
        {
            // A context menu is requested, typically as a result of the user
            // right-clicking in the view. Open our extended WebControlContextMenu.
            webControlContextMenu.Show( this );
        }

        private void OnShowNewView( object sender, ShowCreatedWebViewEventArgs e )
        {
            if ( ( webView == null ) || !webView.IsLive )
                return;

            if ( e.IsPopup )
            {
                // Create a WebView wrapping the view created by Awesomium.
                WebView view = new WebView( e.NewViewInstance );
                // ShowCreatedWebViewEventArgs.InitialPos indicates screen coordinates.
                Rectangle screenRect = e.Specs.InitialPosition.ToRectangle();
                // Create a new WebForm to render the new view and size it.
                WebForm childForm = new WebForm( view, screenRect.Width, screenRect.Height )
                {
                    ShowInTaskbar = false,
                    FormBorderStyle = FormBorderStyle.FixedToolWindow,
                    ClientSize = screenRect.Size != Size.Empty ? screenRect.Size : new Size( 640, 480 )
                };

                // Show the form.
                childForm.Show( this );

                if ( screenRect.Location != Point.Empty )
                    // Move it to the specified coordinates.
                    childForm.DesktopLocation = screenRect.Location;
            }
            else if ( e.IsWindowOpen || e.IsPost )
            {
                // Create a WebView wrapping the view created by Awesomium.
                WebView view = new WebView( e.NewViewInstance );
                // Create a new WebForm to render the new view and size it.
                WebForm childForm = new WebForm( view, 640, 480 );
                // Show the form.
                childForm.Show( this );
            }
            else
            {
                // Let the new view be destroyed. It is important to set Cancel to true 
                // if you are not wrapping the new view, to avoid keeping it alive along
                // with a reference to its parent.
                e.Cancel = true;

                // Load the url to the existing view.
                webView.Source = e.TargetURL;
            }
        }

        private void OnCrashed( object sender, CrashedEventArgs e )
        {
            Debug.Print( "Crashed! Status: " + e.Status );
        }

        // Called in response to JavaScript: 'window.close'.
        private void OnWindowClose( object sender, WindowCloseEventArgs e )
        {
            // If this is a child form, respect the request and close it.
            if ( ( webView != null ) && ( webView.ParentView != null ) )
                this.Close();
        }

        // This is called when the page asks to be printed, usually as result of
        // a window.print().
        private void OnPrintRequest( object sender, PrintRequestEventArgs e )
        {
            if ( !webView.IsLive )
                return;

            // You can actually call PrintToFile anytime after the ProcessCreated
            // event is fired (or the DocumentReady or LoadingFrameComplete in 
            // subsequent navigations), but you usually call it in response to
            // a print request. You should possibly display a dialog to the user
            // such as a FolderBrowserDialog, to allow them select the output directory
            // and verify printing.
            int requestId = webView.PrintToFile( @".\Prints", PrintConfig.Default );

            Debug.Print( String.Format( "Print request {0} is being printed to {1}.", requestId, @".\Prints" ) );
        }

        private void OnPrintComplete( object sender, PrintCompleteEventArgs e )
        {
            Debug.Print( String.Format( "Print request {0} completed. The following files were created:", e.RequestId ) );

            foreach ( string file in e.Files )
                Debug.Print( String.Format( "\t {0}", file ) );
        }

        private void OnPrintFailed( object sender, PrintOperationEventArgs e )
        {
            Debug.Print( String.Format( "Printing request {0} failed! Make sure the provided outputDirectory is writable.", e.RequestId ) );
        }

        private void OnJavascriptDialog( object sender, JavascriptDialogEventArgs e )
        {
            if ( !e.DialogFlags.HasFlag( JSDialogFlags.HasPromptField ) &&
                !e.DialogFlags.HasFlag( JSDialogFlags.HasCancelButton ) )
            {
                // It's a 'window.alert'
                MessageBox.Show( this, e.Message );
                e.Handled = true;
            }
        }

        private void webControlContextMenu_Opening( object sender, ContextMenuOpeningEventArgs e )
        {
            // Update the visibility of our menu items based on the
            // latest context data.
            openSeparator.Visible =
                openMenuItem.Visible = !e.Info.IsEditable && ( webView.Source != null );
        }

        private void webControlContextMenu_ItemClicked( object sender, ToolStripItemClickedEventArgs e )
        {
            if ( ( webView == null ) || !webView.IsLive )
                return;

            // We only process the menu item added by us. The WebControlContextMenu
            // will handle the predefined items.
            if ( (string)e.ClickedItem.Tag != "open" )
                return;

            WebForm webForm = new WebForm( webView.Source );
            webForm.Show( this );
        }
        #endregion


        #region IResourceInterceptor & DataSource
        private const string LOGO_RESOURCE = "WinFormsSample.osm_logo_550.png";

        #region MyDataSource
        // Custom DataSource assigned to our WebSession as 'catch-all' which
        // means that all requests targeting the specified WebConfig.AssetProtocol
        // (for this example: 'https'), irrespective of hostname, will be redirected 
        // to this DataSource unless ResourceRequest.IgnoreDataSources is set to true
        // in a IResourceInterceptor.OnRequest implementation.
        private class MyDataSource : AsyncDataSource
        {
            // Called on a background thread.
            protected override void LoadResourceAsync( DataSourceRequest request )
            {
                // Get the embedded resource of the Awesomium logo.
                var assembly = Assembly.GetExecutingAssembly();
                var info = assembly.GetManifestResourceInfo( LOGO_RESOURCE );

                // The resource does not exist.
                if ( info == null )
                    this.SendRequestFailed( request );

                using ( var stream = assembly.GetManifestResourceStream( LOGO_RESOURCE ) )
                {
                    // Get a byte array of the resource.
                    byte[] buffer = new byte[ stream.Length ];
                    stream.Read( buffer, 0, buffer.Length );

                    // Initialize unmanaged memory to hold the array.
                    int size = Marshal.SizeOf( buffer[ 0 ] ) * buffer.Length;
                    IntPtr pnt = Marshal.AllocHGlobal( size );

                    try
                    {
                        // Copy the array to unmanaged memory.
                        Marshal.Copy( buffer, 0, pnt, buffer.Length );

                        // Alternatively, you can pin the array in the managed heap.
                        // Note however that pinning objects seriously disrupts GC operation. 
                        // Being able to move objects around in the heap is one of the reasons 
                        // why modern GCs can (somewhat) keep up with manual memory management. 
                        // By pinning objects in the managed heap, the GC looses it's one 
                        // performance advantage over manual memory management: 
                        // a relatively un-fragmented heap.
                        //GCHandle handle = GCHandle.Alloc( buffer, GCHandleType.Pinned );
                        //IntPtr pnt = handle.AddrOfPinnedObject();

                        // Simulate some delay to demonstrate asynchronous loading of resources.
                        Thread.Sleep( 2000 );

                        // Create a ResourceResponse. A copy is made of the supplied buffer.
                        this.SendResponse( request, new DataSourceResponse() { Buffer = pnt, Size = (uint)buffer.Length } );
                    }
                    catch
                    {
                        this.SendRequestFailed( request );
                    }
                    finally
                    {
                        // Data is not owned by the ResourceResponse. A copy is made 
                        // of the supplied buffer. We can safely free the unmanaged memory.
                        Marshal.FreeHGlobal( pnt );
                    }
                }
            }
        }
        #endregion

        #region IResourceInterceptor
        // Note that this is called on the I/O thread.
        ResourceResponse IResourceInterceptor.OnRequest( ResourceRequest request )
        {
            // We are only interested in GET requests.
            if ( !String.Equals( request.Method, "GET", StringComparison.OrdinalIgnoreCase ) )
                return null;

            bool isGoogleHost = request.Url.Host.EndsWith( "google.com" ) ||
                request.Url.Host.EndsWith( "ggpht.com" ) ||
                request.Url.Host.EndsWith( "gstatic.com" ) ||
                request.Url.Host.EndsWith( "googleapis.com" ) ||
                request.Url.Host.EndsWith( "googleusercontent.com" );

            // For this example, we have set WebConfig.AssetProtocol to 'https'
            // which means that all requests with an 'https' protocol, will be
            // redirected to a DataSource bound to a host that matches the domain
            // after 'https://', or a to a 'catch-all' DataSource (if any). 
            // IResourceInterceptor.OnRequest is called before any DataSources and
            // the new ResourceRequest.IgnoreDataSources property allows you to tell
            // Awesomium to handle this request normally (send the request to the
            // remote server), bypassing any DataSources. This way you can simulate
            // an asynchronous IResourceInterceptor since DataSources are asynchronous
            // and you can now let only certain requests be processed by a DataSource.
            request.IgnoreDataSources = true;

            if ( isGoogleHost && ( request.Url.Segments.Length > 1 ) )
            {
                // Get the last segment of the Uri. This is usually the file name.
                string fileName = request.Url.Segments[ request.Url.Segments.Length - 1 ];

                //Debug.Print( "Possible file-name: " + fileName );

                // Check if this is a request for 'logo4w.png' (this is currently 
                // the name of the 'Google' logo file).
                if ( String.Equals( fileName, "logo11w.png", StringComparison.OrdinalIgnoreCase ) )
                {
                    // Let the request be processed by our DataSource. This simulates
                    // an asynchronous IResourceInterceptor since DataSources are asynchronous
                    // and you can now let only certain requests be processed by a DataSource.
                    request.IgnoreDataSources = false;
                }
            }

            // Return NULL to allow normal behavior.
            return null;
        }

        // Note that this is called on the I/O thread.
        bool IResourceInterceptor.OnFilterNavigation( NavigationRequest request )
        {
            bool isGoogleHost = request.Url.Host.EndsWith( "google.com" ) ||
                request.Url.Host.EndsWith( "ggpht.com" ) ||
                request.Url.Host.EndsWith( "gstatic.com" ) ||
                request.Url.Host.EndsWith( "googleapis.com" ) ||
                request.Url.Host.EndsWith( "googleusercontent.com" );

            // Uncomment the following line, to block (almost) everything
            // outside Google. This will cancel any attempt to navigate away 
            // from Google sites.
            // return !isGoogleHost;

            return false;
        }
        #endregion

        #endregion
    }
}