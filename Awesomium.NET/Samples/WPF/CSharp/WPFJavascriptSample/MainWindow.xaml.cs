/********************************************************************************
 *    Project   : Awesomium.NET (WPFJavascriptSample)
 *    File      : MainWindow.xaml.cs
 *    Version   : 1.7.0.0 
 *    Date      : 4/13/2013
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
 *    This sample demonstrates an advanced scenarios of asynchronously 
 *    inter-operating with a loaded page, using Awesomium JavaScript 
 *    Interoperation.
 *    
 *    
 ********************************************************************************/

#region Using
using System;
using System.Linq;
using Awesomium.Core;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reflection;
#endregion

namespace WPFJavascriptSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        private JSFunctionAsyncHandler onResponse;
        #endregion

        #region JavascriptCallbackState
        private class JavascriptCallbackState : IDisposable
        {
            #region Fields
            private Timer timer;
            private CancellationTokenSource cts;
            INavigationInterceptor navigationInterceptor;
            private JSObject callback;
            private bool timedOut;
            private object syncRoot;
            private object data;
            #endregion


            #region Ctors
            public JavascriptCallbackState( INavigationInterceptor navigationInterceptor, JSObject callback, object data, int timeout )
            {
                this.callback = callback;
                this.data = data;

                cts = new CancellationTokenSource();
                cts.Token.Register( OnCanceled );

                this.navigationInterceptor = navigationInterceptor;
                navigationInterceptor.BeginNavigation += OnNavigation;

                timer = new Timer( OnTimeout, null, timeout, Timeout.Infinite );
            }
            #endregion

            #region Dtors
            ~JavascriptCallbackState()
            {
                this.OnDispose( false );
            }

            protected virtual void OnDispose( bool disposing )
            {
                lock ( SyncRoot )
                {
                    // To avoid race conditions, the first to
                    // be disposed must be the timer...
                    if ( timer != null )
                        timer.Dispose();

                    timer = null;

                    // .. and then remove the event handler
                    // for the same reason.
                    if ( disposing )
                    {
                        GC.SuppressFinalize( this );

                        if ( navigationInterceptor != null )
                            navigationInterceptor.BeginNavigation -= OnNavigation;

                        navigationInterceptor = null;

                        if ( cts != null )
                            cts.Dispose();

                        cts = null;

                        if ( callback != null )
                            callback.Dispose();

                        callback = null;
                    }
                }
            }
            #endregion


            #region Properties
            public CancellationToken Token
            {
                get
                {
                    return cts.Token;
                }
            }

            public JSObject Callback
            {
                get
                {
                    return callback;
                }
            }

            public object Data
            {
                get
                {
                    return data;
                }
            }

            public bool TimedOut
            {
                get
                {
                    return timedOut;
                }
            }

            public object SyncRoot
            {
                get
                {
                    if ( syncRoot == null )
                        Interlocked.CompareExchange( ref syncRoot, new object(), null );

                    return syncRoot;
                }
            }
            #endregion

            #region Event Handlers
            private void OnCanceled()
            {
                if ( navigationInterceptor == null )
                    return;

                navigationInterceptor.BeginNavigation -= OnNavigation;
            }

            private void OnNavigation( object sender, NavigationEventArgs e )
            {
                lock ( SyncRoot )
                {
                    if ( cts == null )
                        return;

                    cts.Cancel();
                }
            }

            private void OnTimeout( object state )
            {
                lock ( SyncRoot )
                {
                    if ( cts == null )
                        return;

                    timedOut = true;
                    cts.Cancel();
                }
            }
            #endregion


            #region IDisposable
            void IDisposable.Dispose()
            {
                this.OnDispose( true );
            }
            #endregion
        }
        #endregion


        #region Ctors
        public MainWindow()
        {
            // Initialize our delegate.
            onResponse = OnResponse;

            InitializeComponent();
        }
        #endregion


        #region Methods
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            // Dispose our control.
            webControl.Dispose();
        }
        #endregion

        #region Properties
        public ObservableCollection<ImageSource> PageImages
        {
            get { return (ObservableCollection<ImageSource>)GetValue( PageImagesProperty ); }
        }

        private static readonly DependencyPropertyKey PageImagesPropertyKey =
            DependencyProperty.RegisterReadOnly( "PageImages",
            typeof( ObservableCollection<ImageSource> ), typeof( MainWindow ),
            new FrameworkPropertyMetadata( new ObservableCollection<ImageSource>() ) );

        /// <summary>
        /// Identifies the <see cref="PageImages"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PageImagesProperty =
            PageImagesPropertyKey.DependencyProperty;
        #endregion

        #region Event Handlers
        private void webControl_ConsoleMessage( object sender, ConsoleMessageEventArgs e )
        {
            consoleBox.AppendText( String.Format( ">{0}\n", e.Message ) );
            consoleBox.ScrollToEnd();
        }

        private void webControl_NativeViewInitialized( object sender, WebViewEventArgs e )
        {
            // We demonstrate the creation of a child global object.
            // Acquire the parent first.
            JSObject external = webControl.CreateGlobalJavascriptObject( "external" );

            if ( external == null )
                return;

            // NativeViewInitialized is not called in a Javascript Execution Context (JEC).
            // We explicitly dispose any created or acquired JSObjects.
            using ( external )
            {
                // Create a child using fully qualified name. This only succeeds if
                // the parent is created first.
                JSObject app = webControl.CreateGlobalJavascriptObject( "external.app" );

                if ( app == null )
                    return;

                using ( app )
                {
                    // Create and bind to an asynchronous custom method that will be called
                    // by JavaScript, when the page is ready to provide us with a response
                    // after performing some time consuming operation.
                    // (See: /web/index.html)
                    app.BindAsync( "sendResponse", OnResponse );
                    // Create and bind to an asynchronous custom method that is called
                    // by JavaScript to have our native app perform some heavy, time consuming 
                    // work and provide a response asynchronously through a callback.
                    // (See: /web/index.html)
                    app.BindAsync( "performHeavyWork", OnWork );
                }
            }
        }

        private void scenario1_Click( object sender, RoutedEventArgs e )
        {
            PageImages.Clear();

            if ( !webControl.IsLive || !webControl.IsDocumentReady )
                return;

            webControl.ExecuteJavascript( "getImagesAppAware();" );
        }

        private void scenario2_Click( object sender, RoutedEventArgs e )
        {
            PageImages.Clear();

            // Make sure we can create a Javascript Execution Context (JEC).
            if ( !webControl.IsLive || !webControl.IsDocumentReady )
                return;

            // Create an asynchronous Javascript Execution Context (JEC) and 
            // execute our method in it. Javascript Execution Contexts provide: 
            //
            //   1. Easy access to essential objects of the loaded web-page's 
            //      current JavaScript environment (through Global).
            //   2. Automatic disposal of any JSObject instances created or
            //      acquired within the JEC, when the routine associated with
            //      the context exits.
            //   3. Code execution isolation. Any errors or exceptions that
            //      occur in a Javascript Execution Context, are silently handled 
            //      and propagated to the JavaScript console.
            //
            webControl.CreateJavascriptExecutionContext( OnScenario2 );
        }

        private void OnScenario2( Global global )
        {
            // Check if there's a global method called 'getImages'. You can now get function members 
            // through JSObject safely when your code executes in a JEC. This works even if you're 
            // using the regular CLR API (window[ "getImages" ] will return a JSValue that has 
            // IsFunctionObject set to true and can be implicitly casted to JSFunction).
            if ( !( global.window.getImages is JSFunction ) )
                return;
            // Note that we could simply convert to Boolean when checking for a valid member
            // (if ( !global.window.getImages ) return;). This would not necessarily mean that 
            // 'getImages' is a function. It could be any valid value (not 'undefined' or 'null') 
            // that is still converted to True. If it's not a function, an error will occur when 
            // we try to invoke it below. However any errors or exceptions that occur in a JEC,
            // are now silently handled and propagated to the JavaScript console.

            // Here is another important new feature of Awesomium's Javascript Integration API.
            // You can pass the delegate of a managed method in your application, directly to the 
            // dynamic JavaScript expression. This technique only involves a single IPC message to 
            // the Awesomium process and since no result is expected from the 'getImages' 
            // call below, even this message will be sent asynchronously.
            global.window.getImages( onResponse );
        }

        private void scenario3_Click( object sender, RoutedEventArgs e )
        {
            PageImages.Clear();

            // Check if we can execute JavaScript on the page.
            if ( !webControl.IsLive || !webControl.IsDocumentReady )
                return;

            // Injects JavaScript to the page that will acquire all the images.
            webControl.ExecuteJavascript( Properties.Resources.GetImagesScript );
        }
        #endregion


        #region Page Heavy Work Response
        private void OnResponse( JSValue[] arguments )
        {
            if ( arguments.Length < 1 )
                return;

            if ( !arguments[ 0 ].IsString )
                return;

            byte[] binaryData = Convert.FromBase64String( arguments[ 0 ] );

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream( binaryData );
            bi.EndInit();

            PageImages.Add( bi );
        }
        #endregion

        #region Native App Heavy Work
        // Handler of the external.app.performHeavyWork method.
        private void OnWork( object sender, JavascriptMethodEventArgs e )
        {
            // Must have 2 arguments passed.
            if ( e.Arguments.Length != 3 )
                return;

            // Must be a function object.
            if ( !e.Arguments[ 0 ].IsFunctionObject )
                return;

            // Must be a timeout value.
            if ( !e.Arguments[ 1 ].IsNumber )
                return;

            // Must be the image id.
            if ( !e.Arguments[ 2 ].IsNumber )
                return;

            // Get the NavigationInterceptor. This will allow us to cancel the operation
            // if the user navigates away.
            INavigationInterceptor navigationInterceptor = webControl.GetService( typeof( INavigationInterceptor ) ) as INavigationInterceptor;

            // Without this, do not proceed.
            if ( navigationInterceptor == null )
                return;

            // You can cache the callback and call it only when your application 
            // has performed all work necessary and has a result ready to send.
            // Note that this callback object is valid for as long as the current 
            // page is loaded; a navigation will invalidate it. This is why we
            // need the NavigationInterceptor service acquired above.
            JSFunction callbackArg = e.Arguments[ 0 ];
            // Get the image id.
            int id = (int)e.Arguments[ 1 ];
            // Get the timeout specified.
            int timeout = (int)e.Arguments[ 2 ];

            // Make sure it's a function object.
            if ( !callbackArg )
                return;

            // See it!
            Debug.Print( callbackArg.ToString() );

            // You need to copy the object if you intend to cache it. The original
            // object argument passed to the handler is destroyed by default when 
            // the handler returns. A copy will keep it alive. Note that the clone
            // of a JSFunction, will definitely be a JSFunction as well.
            JSFunction callback = (JSFunction)callbackArg.Clone();

            // Create a state object. This holds the callback and controls
            // cancellation. It is passed to both the background operation procedure
            // and the response procedure.
            JavascriptCallbackState state = new JavascriptCallbackState( navigationInterceptor, callback, id, timeout );

            // Perform your heavy work.
            Task.Factory.StartNew(
                /* The function that will perform the work. */
                (Func<Object, String>)PerformWork,
                /* Pass the state object. */
                state,
                /* The CancellationToken that will let us know if we
                 * should cancel passing back the result. */
                state.Token ).ContinueWith(
                /* Send the result when complete. */
                SendResponse,
                /* Make sure the result is sent on the 
                 * initial thread. */
                TaskScheduler.FromCurrentSynchronizationContext() );
        }

        // Procedure executed on background thread to perform a task.
        private static string PerformWork( object stateObj )
        {
            // Get the state object.
            JavascriptCallbackState state = (JavascriptCallbackState)stateObj;

            if ( state.Token.IsCancellationRequested )
                return null;

            int id = (int)state.Data;
            string imageResource = String.Format( "WPFJavascriptSample.images.alphanumeric-number-{0}.png", id );
            string result = null;

            // Get the embedded resource of the Awesomium logo.
            var assembly = Assembly.GetExecutingAssembly();
            var info = assembly.GetManifestResourceInfo( imageResource );

            // The resource does not exist.
            if ( info == null )
                return null;

            if ( state.Token.IsCancellationRequested )
                return null;

            using ( var stream = assembly.GetManifestResourceStream( imageResource ) )
            {
                // Get a byte array of the resource.
                byte[] buffer = new byte[ stream.Length ];
                stream.Read( buffer, 0, buffer.Length );

                if ( state.Token.IsCancellationRequested )
                    return null;

                result = Convert.ToBase64String( buffer );
            }

            // Throws an OperationCanceledException if this token has had 
            // cancellation requested. When a task instance observes an
            // OperationCanceledException thrown by user code, it compares
            // the exception's token to its associated token. If they are the same
            // and the token's IsCancellationRequested property returns true,
            // the task interprets this as acknowledging cancellation and transitions
            // to the Canceled state.
            state.Token.ThrowIfCancellationRequested();

            return result;
        }

        // Called on the UI thread after the working procedure is complete, or canceled.
        private void SendResponse( Task<String> t )
        {
            using ( JavascriptCallbackState state = (JavascriptCallbackState)t.AsyncState )
            {
                if ( !webControl.IsLive )
                    return;

                // Check if the user has navigated away,
                // or the operation timed out.
                if ( t.IsCanceled )
                {
                    // If we timed out, send an indication.
                    if ( state.TimedOut )
                        InvokeCallback( state.Callback, "ERR_TIMEOUT" );

                    // Disposing the JavascriptCallbackState
                    // will dispose the callback as well.
                    return;
                }

                if ( ( t.Exception != null ) || String.IsNullOrEmpty( t.Result ) )
                {
                    // The operation failed.
                    // Disposing the JavascriptCallbackState
                    // will dispose the callback as well.
                    return;
                }

                // Invoke the callback. JSObject supports the DLR so for 
                // this example we assign to a dynamic which makes invoking the
                // callback, pretty straightforward (just as you would in JS).
                InvokeCallback( state.Callback, t.Result );
            }
        }

        // Helper that executes the JavaScript callback.
        private static void InvokeCallback( dynamic callback, string result )
        {
            // Invoke the callback.
            callback( result );

            // JSObject supports the DLR. However, if you choose to explicitly cast back to 
            // JSFunction, this is the technique for invoking the callback, using regular API:
            //callback.Call( callback, result );
        }
        #endregion
    }
}
