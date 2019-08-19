/********************************************************************************
 *    Project   : Awesomium.NET (WebControlSample)
 *    File      : MainWindow.xaml.cs
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
 *    WebControlSample main window. This simple Web UI sample, uses a
 *    ResourceDataSource to load a web page included to the executable as
 *    application resource.
 *    
 *    This sample also demonstrates JavaScript pushing some heavy work to
 *    the native application, which then responds using a provided JavaScript
 *    callback.
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
#endregion

namespace WebControlSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IResourceInterceptor
    {

        #region Ctor
        public MainWindow()
        {
            // Initialize the Core before InitializeComponent.
            // When the WebControl is created, it will automatically
            // initialize the Core with default configuration.
            WebCore.Initialize( new WebConfig()
            {
                HomeURL = new Uri( "asset://local/web/index.html" ),
                LogLevel = LogLevel.Verbose,
            } );

            InitializeComponent();

            // Since we perform lazy initialization, this is a
            // safe way to assign the ResourceInterceptor, only when
            // the WebCore is initialized.
            if ( WebCore.IsInitialized )
                WebCore.ResourceInterceptor = this;
            else
            {
                // We could simply write this like that:
                //WebCore.Started += ( s, e ) => WebCore.ResourceInterceptor = this;
                // See below why we don't do it.

                CoreStartEventHandler handler = null;
                handler = ( s, e ) =>
                {
                    // Though this example shuts down the core when this
                    // MainWindow closes, in a normal application scenario,
                    // there could be many instances of MainWindow. We don't
                    // want them all referenced by the WebCore singleton
                    // and kept from garbage collection, so the handler
                    // has to be removed.
                    WebCore.Started -= handler;
                    WebCore.ResourceInterceptor = this;
                };
                WebCore.Started += handler;
            }

            // Assign our global ShowCreatedWebView handler.
            webControl.ShowCreatedWebView += App.OnShowNewView;
        }
        #endregion


        #region Methods
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            // Don't keep me there.
            WebCore.ResourceInterceptor = null;
            // Destroy the WebControl and its underlying view.
            webControl.Dispose();
        }
        #endregion

        #region Event Handlers
        private void webControl_ConsoleMessage( object sender, ConsoleMessageEventArgs e )
        {
            Debug.Print( String.Format( "[Line: {0}]> {1}", e.LineNumber, e.Message ) );
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
                    // Create and bind to an asynchronous custom method. This is called
                    // by JavaScript to have the native app perform some heavy work.
                    // (See: /web/index.html)
                    app.BindAsync( "performHeavyWork", OnWork );
            }
        }

        private void webControl_DocumentReady( object sender, DocumentReadyEventArgs e )
        {
            // When ReadyState is Ready, you can execute JavaScript against
            // the DOM but all resources are not yet loaded. Wait for Loaded.
            if ( e.ReadyState == DocumentReadyState.Ready )
                return;

            // NOTE THAT STARTING WITH v1.7.5, YOU NO LONGER NEED TO EXPLICITLY DISPOSE OBJECTS CREATED
            // OR ACQUIRED WITHIN A JAVASCRIPT EXECUTION CONTEXT (JEC). THESE ARE AUTOMATICALLY DISPOSED
            // UPON EXITING THE EVENT HANDLER OR ROUTINE ASSOCIATED WITH THE CONTEXT.

            // DocumentReady is called in an asynchronous Javascript Execution Context (JEC) therefore 
            // an instance of 'Global' providing access to essential objects on the web-page, must already 
            // be available. We can use it to avoid more synchronous calls to acquire essential objects.
            dynamic document = e.Environment.document;

            // Make sure we have the element. All synchronous calls of the Awesomium.NET API,
            // return a JSValue. Until version 1.7.5, attempting to cast an invalid ('null' or 'undefined')
            // JSValue to JSObject, would return a null reference (so you had to test against null).
            // Starting with Awesomium.NET 1.7.5, attempting to cast an invalid JSValue to JSObject,
            // will return a JSObject that is equivalent to JSValue.Undefined. Like JSValue, the JSObject type
            // now also contains an implicit operator to Boolean that when called upon this special undefined
            // JSObject or a null JSObject, will return false. So from now on, you can check if you got
            // a valid JSObject, like you would in JavaScript:
            if ( !document )
                return;

            // This is an invocation. If the object does not contain
            // a method named 'getElementById', you would get a 
            // RuntimeBinderException. However starting with v.1.7.5
            // errors or exceptions that occur within a Javascript Execution Context (JEC)
            // are silently handled and propagated to the JavaScript console.
            var elem1 = document.getElementById( "postfield1" );

            if ( !elem1 )
                return;

            // This is a property setting. If the object does not
            // contain a property named 'value', you should NOT get an
            // exception. JavaScript is dynamic so this code would just
            // add a 'value' property to the object, even if one was not
            // there before, and set its value to "Some Value 1".
            elem1.value = "Some Value 1";

            var elem2 = document.getElementById( "postfield2" );

            if ( !elem2 )
                return;

            elem2.value = "Some Value 2";

            // All JSObjects implement IEnumerable to enumerate their
            // properties, just as in JavaScript.
            foreach ( var property in elem2 )
                Debug.Print( (String)property );
        }

        private void webControl_JavascriptRequest( object sender, JavascriptRequestEventArgs e )
        {
            // Synchronously called through OSMJIF.
            if ( e.Request != JavascriptRequest.Minimize )
                return;

            // Will be true if called from the top window. We ignore
            // requests from frames.
            if ( !e.RequestToken )
                return;

            // Minimize the window.
            this.WindowState = System.Windows.WindowState.Minimized;
            // JavascriptRequest is called synchronously. Set Handled to true
            // to let JavaScript callers know that we accepted the request.
            e.Handled = true;
        }

        private void webControl_WindowClose( object sender, WindowCloseEventArgs e )
        {
            if ( !e.IsCalledFromFrame )
                this.Close();
        }
        #endregion


        #region Heavy Work & Callback Example
        private void OnWork( JSValue[] arguments )
        {
            // Must be a function object.
            if ( !arguments[ 0 ].IsFunctionObject )
                return;

            // You can cache this callback and call it only when your application 
            // has performed all work necessary and has a result ready to send.
            // Note that this callback object is valid for as long as the current 
            // page is loaded. A navigation will invalidate it.
            JSFunction callbackArg = arguments[ 0 ];

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

            // Perform your heavy work.
            Task.Factory.StartNew(
                (Func<object, string>)PerformWork, callback ).ContinueWith(
                /* Send a response when complete. */
                SendResponse,
                /* Make sure the response is sent on the 
                 * initial thread. */
                TaskScheduler.FromCurrentSynchronizationContext() );
        }

        private string PerformWork( object callback )
        {
            // Perform heavy work.
            Thread.Sleep( 3000 );

            return "Some Result";
        }

        private void SendResponse( Task<string> t )
        {
            // Get the callback. JSObject supports the DLR so for 
            // this example we assign to a dynamic which makes invoking the
            // callback, pretty straightforward (just as you would in JS).
            dynamic callback = t.AsyncState;

            if ( callback == null )
                return;

            using ( callback )
            {

                if ( ( t.Exception != null ) || String.IsNullOrEmpty( t.Result ) )
                    // We failed.
                    return;

                // Invoke the callback.
                callback( t.Result );

                // JSObject supports the DLR. However, if you choose to explicitly cast back to 
                // JSFunction, this is the technique for invoking the callback, using regular API:
                //callback.Call( callback, t.Result );
            }
        }
        #endregion

        #region IResourceInterceptor
        ResourceResponse IResourceInterceptor.OnRequest( ResourceRequest request )
        {
            // Resume normal processing.
            return null;
        }

        bool IResourceInterceptor.OnFilterNavigation( NavigationRequest request )
        {
            // ResourceInterceptor is global. It applies to all views
            // maintained by the WebCore. We are only interested in 
            // requests targeting our WebControl.
            // CAUTION: IResourceInterceptor members are called on the I/O thread.
            // Cast to IWebView to perform thread-safe access to the Identifier property!
            if ( request.ViewId != ( (IWebView)webControl ).Identifier )
                return false;

            if ( ( request.Url == null ) || ( request.Url.Scheme != "asset" ) )
                // Block navigations to resources outside our 
                // application's asset stores.
                return true;

            // Allow navigation.
            return false;
        }
        #endregion
    }
}
