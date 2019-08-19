/********************************************************************************
 *    Project   : Awesomium.NET (JavascriptSample)
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
 *    Demonstrates the usage of the new Javascript Interoperation API,
 *    using a windowed WebView.
 *  
 *    The WebView used in this example, is of type WebViewType.Window.
 *    This new type of view, renders directly to a HWND and handles
 *    user input itself.
 *    
 *    
 ********************************************************************************/

#region Using
using System;
using System.IO;
using System.Drawing;
using Awesomium.Core;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using Awesomium.Windows.Forms;
#endregion

namespace JavascriptSample
{
    public class WebForm : Form
    {
        #region Fields
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;
        private BindingSource IWebViewBindingSource;

        private WebView webView;
        private WebSession session;
        bool needsResize;

        // We are caching the delegates for our synchronous and asynchronous
        // callbacks, to use them easily and avoid continuous casting.
        private JavascriptAsyncMethodHandler asyncCallback;
        private JavascriptMethodHandler syncCallback;
        private JavascriptAsyncMethodHandler _onMyInterval;
        private JSFunctionAsyncHandler _onDOMMouseMove;
        #endregion


        #region Ctors
        public WebForm()
        {
            // Initialize delegates.
            asyncCallback = (JavascriptAsyncMethodHandler)OnCustomJavascriptAsyncMethod;
            syncCallback = (JavascriptMethodHandler)OnCustomJavascriptMethod;
            _onMyInterval = (JavascriptAsyncMethodHandler)onMyInterval;
            _onDOMMouseMove = (JSFunctionAsyncHandler)onDOMMouseMove;

            if ( !WebCore.IsInitialized )
            {
                WebConfig webConfig = WebConfig.Default;

                // Using our executable as a child rendering process, is not
                // available when debugging in VS.
                if ( !Process.GetCurrentProcess().ProcessName.EndsWith( "vshost" ) )
                    // We demonstrate using our own executable as child rendering process.
                    webConfig.ChildProcessPath = Assembly.GetExecutingAssembly().Location;

                // Initialize the WebCore.
                WebCore.Initialize( webConfig );
            }

            // Create and cache a WebSession.
            session = WebCore.CreateWebSession(
                String.Format( "{0}{1}Cache", Path.GetDirectoryName( Application.ExecutablePath ), Path.DirectorySeparatorChar ),
                new WebPreferences()
                {
                    SmoothScrolling = true,
                    CustomCSS = Properties.Resources.FontFace_1,
                    WebGL = true,
                    // Windowed views, support full hardware acceleration.
                    EnableGPUAcceleration = true
                } );

            InitializeComponent();

            // We create the view in OnHandleCreated.
        }

        // Used to create child (popup) windows.
        internal WebForm( WebView view, int width, int height )
        {
            // Initialize delegates.
            asyncCallback = (JavascriptAsyncMethodHandler)OnCustomJavascriptAsyncMethod;
            syncCallback = (JavascriptMethodHandler)OnCustomJavascriptMethod;
            _onDOMMouseMove = (JSFunctionAsyncHandler)onDOMMouseMove;
            // Cache the view.
            webView = view;

            this.Width = width;
            this.Height = height;

            InitializeComponent();
        }
        #endregion

        #region Dtors
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }

            // For windowed WebViews only, it's important that the parent
            // window is disposed before we dispose the WebView. This is why in this
            // sample, we moved the designer code file into the form's main code file
            // and we do not dispose the view in OnFormClosed.
            base.Dispose( disposing );

            if ( disposing )
            {
                bool isChild = false;

                // Get if this is form hosting a child view.
                if ( webView != null )
                    isChild = webView.ParentView != null;

                // Destroy the WebView.
                if ( webView != null )
                    webView.Dispose();

                // For WebCore.Shutdown, see OnApplicationExit in Program.cs.
            }
        }
        #endregion


        #region Methods

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.IWebViewBindingSource = new System.Windows.Forms.BindingSource( this.components );
            ( (System.ComponentModel.ISupportInitialize)( this.IWebViewBindingSource ) ).BeginInit();
            this.SuspendLayout();
            // 
            // IWebViewBindingSource
            // 
            this.IWebViewBindingSource.DataSource = typeof( Awesomium.Core.IWebView );
            // 
            // WebForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size( 624, 442 );
            this.DataBindings.Add( new System.Windows.Forms.Binding( "Text", this.IWebViewBindingSource, "Title", true ) );
            this.Name = "WebForm";
            this.Text = "Javascript Sample";
            ( (System.ComponentModel.ISupportInitialize)( this.IWebViewBindingSource ) ).EndInit();
            this.ResumeLayout( false );

        }
        #endregion

        protected override void OnHandleCreated( EventArgs e )
        {
            base.OnHandleCreated( e );

            if ( webView == null )
                // Create a windowed view.
                webView = WebCore.CreateWebView(
                    this.ClientSize.Width,
                    this.ClientSize.Height,
                    session,
                    WebViewType.Window );

            // Initialize the view.
            InitializeView();
        }

        private void InitializeView()
        {
            // Set this window as parent. For windowed views,
            // this must be set before accessing any member of the view.
            webView.ParentWindow = this.Handle;

            // Handle some important events.
            // Also Check that we have added a Binding
            // in the Form's designer, that binds the Form's
            // Text, to WebView.Title.
            webView.DocumentReady += OnDocumentReady;
            webView.ShowCreatedWebView += OnShowNewView;
            webView.ConsoleMessage += OnConsoleMessage;
            webView.Crashed += OnCrashed;

            // Assign our view to the BindingSource,
            // so that our bindings work.
            IWebViewBindingSource.DataSource = webView;

            // Load a URL, if this is not a child view.
            if ( webView.ParentView == null )
                webView.LoadHTML( "<h2>Opening a popup window...</h2>" );

            // Move focus to the view.
            webView.FocusView();
        }

        private void ResizeView()
        {
            if ( !this.IsHandleCreated )
                return;

            if ( ( webView == null ) || !webView.IsLive )
                return;

            if ( needsResize )
            {
                // Request a resize.
                webView.Resize( this.ClientSize.Width, this.ClientSize.Height );
                needsResize = false;
            }
        }

        protected override void OnResize( EventArgs e )
        {
            base.OnResize( e );

            if ( ( webView == null ) || !webView.IsLive )
                return;

            if ( this.ClientSize.Width > 0 && this.ClientSize.Height > 0 )
                needsResize = true;

            // Request resize, if needed.
            this.ResizeView();
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
        #endregion

        #region Event Handlers
        private void OnShowNewView( object sender, ShowCreatedWebViewEventArgs e )
        {
            if ( ( webView == null ) || !webView.IsLive )
                return;

            if ( e.IsPopup && !e.IsUserSpecsOnly )
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
                    ClientSize = screenRect.Size
                };

                // Show the form.
                childForm.Show( this );
                // Move it to the specified coordinates.
                childForm.DesktopLocation = screenRect.Location;
            }
            else if ( e.IsWindowOpen || e.IsPost )
            {
                // Still window.open or an HTML form with target="_blank" and method="post" 
                // but not a popup which means that no specs have been specified to a
                // window.open call.

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

        private void OnDocumentReady( object sender, DocumentReadyEventArgs e )
        {
            // Wait for the DOM to be loaded.
            if ( e.ReadyState != DocumentReadyState.Loaded )
                return;

            // Make sure the view is alive.
            if ( ( webView == null ) || !webView.IsLive )
                return;

            // We only want this called once.
            webView.DocumentReady -= OnDocumentReady;

            // Do not do anything for child windows.
            if ( webView.ParentView != null )
                return;

            // Do nothing if Javascript is disabled.
            if ( !webView.IsJavascriptEnabled )
                return;

            // DocumentReady is called in an asynchronous Javascript Execution Context (JEC).
            // In instance of Global available in every asynchronous JEC, provides access to
            // essential objects in the currently loaded page. Using Global we avoid unnecessary
            // synchronous calls to acquire these objects.
            var global = e.Environment;

            // Global supports implicit casting to Boolean.
            if ( !global )
                return;

            // You can now access 'window', 'document' etc., through 'global'. All members of Global 
            // are exposed as 'dynamic' so you can work with them using the Dynamic Language Runtime (DLR).
            // Use of Global is demonstrated in the 'ChangeHTML' method in the 'GlobalJavascriptObjectSample'
            // region below. We will not use it here, so that we present the regular CLR API for acquiring
            // and working with JavaScript objects.

            // NOTE THAT STARTING WITH v1.7.5, YOU NO LONGER NEED TO EXPLICITLY DISPOSE OBJECTS CREATED
            // OR ACQUIRED WITHIN A JAVASCRIPT EXECUTION CONTEXT (JEC). THESE ARE AUTOMATICALLY DISPOSED
            // UPON EXITING THE EVENT HANDLER OR ROUTINE ASSOCIATED WITH THE CONTEXT.

            // Gets the HTML Document Object Model (DOM) window object. No explicit cast is needed here.
            // JSValue supports implicit casting.
            JSObject window = webView.ExecuteJavascriptWithResult( "window" );

            // Make sure we have the new window. All synchronous calls of the Awesomium.NET API,
            // return a JSValue. Until version 1.7.5, attempting to cast an invalid ('null' or 'undefined')
            // JSValue to JSObject, would return a null reference (so you had to test against null).
            // Starting with Awesomium.NET 1.7.5, attempting to cast an invalid JSValue to JSObject,
            // will return a JSObject that is equivalent to JSValue.Undefined. Like JSValue, the JSObject type
            // now also contains an implicit operator to Boolean that when called upon this special undefined
            // JSObject or a null JSObject, will return false. So from now on, you can check if you got
            // a valid JSObject, like you would in JavaScript:
            if ( !window )
                return;

            // Get the available properties.
            string[] props = window.GetPropertyNames();

            // Print them to the output.
            Debug.Print( "=================== Window Properties ===================" );
            foreach ( string prop in props )
                Debug.Print( prop );
            Debug.Print( "===========================================================" );
            Debug.Print( "Summary: " + props.Length );
            Debug.Print( "===========================================================" );

            // Invoke 'window.open' passing some parameters and get the new window.
            // Awesomium will immediately create a new view for this window and fire
            // the 'IWebView.ShowCreatedWebView' event. See 'OnShowNewView' above.
            JSObject newWindow = window.Invoke( "open", "", "", "width=300, height=200, top=50, left=50" );

            // Make sure we have the new window object.
            if ( !newWindow )
                return;

            // It must have a 'document' property. Note that this is here only for demonstration.
            // HasProperty, HasMethod etc., perform a synchronous call to V8 (in the child process)
            // and you should avoid using them if you care about performance. See below.
            if ( !newWindow.HasProperty( "document" ) )
                return;

            // The following examples demonstrate using JSObjects with the Dynamic Language Runtime (DLR).
            // Note that here you must make sure you explicitly cast the returned JSValue to JSObject.
            // JSObject supports the DLR. Also note that this call is guaranteed to return a JSObject
            // in all cases. If the property is 'null' or 'undefined', a JSObject that is equivalent 
            // to JSValue.Undefined is returned. Like JSValue, the JSObject type now also contains 
            // an implicit operator to Boolean that when called upon a special 'undefined'
            // JSObject or a null JSObject, will return false.
            dynamic document = (JSObject)newWindow[ "document" ];

            // Make sure we have the object. Since we perform this test here, it is not
            // not necessary to check for HasProperty above.
            if ( !document )
                return;

            // Invoke 'write' using the DLR, just as you would in JavaScript!
            // Note that JavaScript is case sensitive; 'document.Write' would fail.
            document.write( "<h2>Hello World!</h2><p>Have a nice day!</p>" );
            // Without this, a DocumentReady event on the child view with ReadyState = Loaded,
            // will never be fired on the child view.
            document.close();

            // Here we demonstrate invoking JavaScript Function objects. We create one and acquire it.
            dynamic myFunction = (JSFunction)webView.ExecuteJavascriptWithResult( "new Function('html', 'document.write(html);')" );

            // Make sure we have the object.
            if ( !myFunction )
                return;

            // Invoke it directly just as you would in JavaScript!
            myFunction(
                "<h2>Successfully opened a popup window!</h2>" +
                "<p><button onclick=\"myGlobalObject.changeHTML( '" +
                    "<h3>New Content</h3>" +
                    "<p><a href=\\'http://webglsamples.googlecode.com/hg/aquarium/aquarium.html\\'>Load WebGL Sample</a></p>" +
                    "<p><a href=\\'http://www.google.com\\'>Go to: Google</a></p>" +
                    "' )\">" +
                "Click Me</button></p>" );

            // Create a Global Javascript Object sample:
            GlobalJavascriptObjectSample();

            // Various Dynamic Language Runtime (DLR) examples:
            DynamicJavascriptSample();

            // Passing Function objects to JavaScript demo:
            PassFunctionBackToJavascriptSample();
        }

        // Fired when a message is added to the JavaScript console.
        private void OnConsoleMessage( object sender, ConsoleMessageEventArgs e )
        {
            Debug.Print( "[Line: " + e.LineNumber + "] " + e.Message );
        }

        private void OnCrashed( object sender, CrashedEventArgs e )
        {
            // Oops! The WebView crashed.
            Debug.Print( e.Status.ToString() );

            MessageBox.Show( this,
                "The WebView crashed! Status: " + e.Status,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error );
        }
        #endregion


        #region Javascript Interoperation API Examples

        #region GlobalJavascriptObjectSample
        // Demonstrates usage of Global Javascript objects,
        // using the regular API.
        private void GlobalJavascriptObjectSample()
        {
            if ( ( webView == null ) || !webView.IsLive )
                return;

            // This sample demonstrates creating and acquiring a Global Javascript object.
            // These objects persist for the lifetime of the web-view.
            JSObject myGlobalObject = webView.CreateGlobalJavascriptObject( "myGlobalObject" );

            if ( !myGlobalObject )
                return;

            // 'Bind' is the method of the regular API, that needs to be used to create
            // a custom method on our global object and bind it to a managed handler.
            // The handler is of type JavascriptAsyncMethodHandler. Here we define it
            // using a lambda expression.
            myGlobalObject.BindAsync( "changeHTML", ( s, e ) =>
            {
                // Get the new content.
                string newContent = e.Arguments[ 0 ];
                ChangeHTML( newContent );
            } );
        }

        // Called asynchronously.
        private void ChangeHTML( string newContent )
        {
            // This is called in an asynchronous Javascript Execution Context (JEC) therefore an instance 
            // of 'Global' providing access to essential objects on the web-page, must already be available.
            // We can use it to avoid more synchronous calls to acquire essential objects.
            var global = Global.Current;

            // Check if it's valid. 'Global' supports implicit casting to 'Boolean'.
            if ( !global )
                return;

            // Some extreme security demonstration (see: IsHtmlDocument).
            bool isDocument = IsHtmlDocument( global.document );
            Debug.Print( "Is this a valid HTMLDocument? : " + isDocument );

            if ( !isDocument )
                return;

            // We demonstrate setting properties on child objects directly.
            global.document.body.innerHTML = newContent;
            // Here is an advanced scenario. Lists are supported too (NodeList in this case).
            // Notice that we use 'var' as we would in JavaScript. Values acquired through a 
            // dynamic expression, are also treated as dynamic.
            var header = global.document.getElementsByTagName( "h3" )[ 0 ];
            // Make sure we have the element. All synchronous calls of the Awesomium.NET API,
            // return a JSValue. Until version 1.7.5, attempting to cast an invalid ('null' or 'undefined')
            // JSValue to JSObject, would return a null reference (so you had to test against null).
            // Starting with Awesomium.NET 1.7.5, attempting to cast an invalid JSValue to JSObject,
            // will return a JSObject that is equivalent to JSValue.Undefined. Like JSValue, the JSObject type
            // now also contains an implicit operator to Boolean that when called upon this special undefined
            // JSObject or a null JSObject, will return false. So from now on, you can check if you got
            // a valid JSObject, like you would in JavaScript:
            if ( !header )
                return;
            // Use the content of the 'b' tag ("New Content") as title for the document.
            global.document.title = header.innerText;
            // Here is another important new feature of Awesomium's Javascript Integration API.
            // You can pass the delegate of a managed method in your application, directly to the 
            // dynamic JavaScript expression. This method only involves a single IPC message to 
            // the Awesomium process and since no result is expected from the 'addEventListener' 
            // call below, even this message will be sent asynchronously.
            header.addEventListener( "mousemove", _onDOMMouseMove );
        }

        // Static function that performs some validation
        // on 'document' objects.
        public static bool IsHtmlDocument( JSObject jsObject )
        {
            if ( !jsObject || !jsObject.Owner.IsLive )
                return false;

            // Create a helper Function.
            JSFunction evaluatorFunction = jsObject.Owner.ExecuteJavascriptWithResult( "new Function('return this instanceof HTMLDocument;')" );

            if ( !evaluatorFunction )
                return false;

            // Call it as a member of the passed object.
            return evaluatorFunction.Call( jsObject );
        }
        #endregion

        #region DynamicJavascriptSample
        // Demonstrates usage of Global Javascript objects,
        // using the dynamic API.
        private void DynamicJavascriptSample()
        {
            if ( ( webView == null ) || !webView.IsLive )
                return;

            // Get the global object we had previously created and assign it to a dynamic.
            dynamic myGlobalObject = (JSObject)webView.ExecuteJavascriptWithResult( "myGlobalObject" );

            // Make sure we have the object.
            if ( !myGlobalObject )
                return;

            // Create and bind a custom method dynamically. We pass the delegate to a managed handler
            // directly. The method is of type JavascriptMethodHandler which means that the method
            // that will be added to the global object, will be a method that is called synchronously
            // and the managed handler can return a value back to JavaScript.
            myGlobalObject.myMethod = syncCallback;

            // Our method can now be executed from JavaScript. It could be JavaScript already in the page.
            // For this sample, we will inject the JavaScript that executes our method, ourselves.
            var jsResponse = webView.ExecuteJavascriptWithResult( "myGlobalObject.myMethod( 'This is a call from Javascript.' );" );
            // All synchronous calls of the Awesomium.NET API, return a JSValue. JSValue includes an
            // implicit operator to Boolean so you can test for a valid response as you would in JavaScript:
            if ( jsResponse )
                // Print the response.
                Debug.Print( String.Format( "And this is the response: {0}", jsResponse ) );

            // You can also invoke the method dynamically, just as you would in JavaScript.
            var response = myGlobalObject.myMethod( "I can do this!" );

            if ( response )
                // Print the response.
                Debug.Print( String.Format( "And this is the response: {0}", (String)response ) );

            // Create and bind an asynchronous custom method dynamically.
            myGlobalObject.myAsyncMethod = asyncCallback;

            // Invoke the asynchronous as well. Note that when you use the DLR, you can call
            // any function asynchronously (equivalent of JSObject.InvokeAsync) by simply not using the
            // returned value in any way.
            myGlobalObject.myAsyncMethod( "This was sent asynchronously." );

            // Assigning complex arrays directly.
            myGlobalObject.myFirstNumber = new object[] { 1, new int[] { 5, 6 }, 3 };
            // Assigning arrays created through Javascript.
            myGlobalObject.mySecondNumber = webView.ExecuteJavascriptWithResult( "new Array(6,7,8);" );

            // Retrieving elements and performing binary or unary operations. Note that although
            // typed arrays can be passed to remote objects, when retrieved, they will always be of 
            // type 'object[]' where each member is either a JSValue or JSObject instance. JSValue now 
            // however supports binary and unary operations between JavaScript values of any type.
            var result = myGlobalObject.myFirstNumber[ 1 ][ 0 ] + myGlobalObject.mySecondNumber[ 0 ];

            // Unbox and print the result.
            Debug.Print( String.Format( "Unary operation result: {0}", (String)result ) );
        }

        // Called from DocumentReady.
        private void PassFunctionBackToJavascriptSample()
        {
            // DocumentReady is called in an asynchronous Javascript Execution Context (JEC) therefore 
            // an instance of 'Global' providing access to essential objects on the web-page, must already 
            // be available. We can use it to avoid more synchronous calls to acquire essential objects.
            var global = Global.Current;

            // Check if it's valid. 'Global' supports implicit casting to 'Boolean'.
            if ( !global )
                return;

            // Pass the function to 'window.setInterval'. Assign the returned value used to
            // cancel the timer, to a global variable ('myTimer'). This will be used to optionally
            // cancel the timer when our custom method's handler is called.
            //
            // Here is another important new feature of Awesomium's Javascript Integration API.
            // You can pass the delegate of a managed method in your application, directly to the 
            // dynamic JavaScript expression. Actually this method is even faster than creating and
            // binding a to JavaScript method, then acquiring it to pass it back to JavaScript.
            // This method only involves a single synchronous IPC message to the Awesomium process.
            global.window.myTimer = global.window.setInterval( _onMyInterval, 3000 );
        }
        #endregion

        #region OnCustomJavascriptMethod
        // Synchronous JavaScript methods' handler.
        private JSValue OnCustomJavascriptMethod( object sender, JavascriptMethodEventArgs e )
        {
            // We can have the same handler handling many remote methods.
            // Check here the method that is calling the handler.
            switch ( e.MethodName )
            {
                case "myMethod":
                    // Print the text passed.
                    Debug.Print( e.Arguments[ 0 ] );
                    // Synchronously return a response.
                    return "Message Received!";

                default:
                    Debug.Print( String.Format( 
                        "OnCustomJavascriptMethod is called for unknown method: {0}", e.MethodName ) );
                    // We are not bound to this method. Return 'undefined'.
                    return JSValue.Undefined;
            }
        }

        // Asynchronous JavaScript methods' handler.
        private void OnCustomJavascriptAsyncMethod( object sender, JavascriptMethodEventArgs e )
        {
            // We can have the same handler handling many remote methods.
            // Check here the method that is calling the handler.
            switch ( e.MethodName )
            {
                case "myAsyncMethod":
                    // Print the text passed.
                    Debug.Print( e.Arguments[ 0 ] );
                    break;

                default:
                    Debug.Print( String.Format( 
                        "OnCustomJavascriptAsyncMethod is called for unknown method: {0}", e.MethodName ) );
                    break;
            }
        }

        // Called asynchronously from DOM.
        private void onMyInterval( object sender, JavascriptMethodEventArgs e )
        {
            // For this example, 'onMyInterval' is called asynchronously. An instance
            // of Global providing access to essential objects, must be available.
            var global = e.Environment;

            if ( !global )
                return;

            if ( MessageBox.Show( "3 seconds passed.\n\nCancel this timer?", "Window Interval", MessageBoxButtons.YesNo ) != System.Windows.Forms.DialogResult.Yes )
                return;

            // We can make synchronous calls from inside an asynchronous custom method handler.
            // Note that properties of 'Global' are already of type: 'dynamic'.
            global.window.clearInterval( global.window.myTimer );

            // If 'onMyInterval' was created as synchronous, we would have to cancel our interval 
            // using only asynchronous calls, since synchronous calls from inside a synchronous
            // custom method handler, are not allowed. Here is a demonstration:
            // webView.ExecuteJavascript( "window.clearInterval( window.myTimer );" );
        }

        // Called asynchronously from DOM. Here we demonstrate using the new JSFunctionAsyncHandler
        // that only accepts an array of arguments passed to the function from JavaScript.
        private void onDOMMouseMove( JSValue[] arguments )
        {
            // This is an event handler of 'mousemove'. The first and only
            // argument, should be a JavaScript 'MouseEvent' instance.
            if ( arguments.Length < 1 || !arguments[ 0 ].IsObject )
                return;

            // Assign to a dynamic to use the DLR.
            dynamic mouseEvent = (JSObject)arguments[ 0 ];

            if ( !mouseEvent )
                return;

            // Print some info for the event.
            String message = String.Format( "Mouse over header at: {0}x{1}", mouseEvent.clientX, mouseEvent.clientY );
            Debug.Print( message );
        }
        #endregion

        #endregion
    }
}
