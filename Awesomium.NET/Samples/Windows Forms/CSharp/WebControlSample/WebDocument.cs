/********************************************************************************
 *    Project   : Awesomium.NET (TabbedFormsSample)
 *    File      : WebDocument.cs
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
 *    Represents the contents of a tab in an application window. This control
 *    contains the WebControl and an independent bar with the address-box,
 *    navigation buttons etc..
 *    
 *    
 ********************************************************************************/

#region Using
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using Awesomium.Core;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using WeifenLuo.WinFormsUI.Docking;
#endregion

namespace TabbedFormsSample
{
    partial class WebDocument : DockContent
    {
        #region Fields
        private const String JS_FAVICON = "(function(){links = document.getElementsByTagName('link'); wHref=window.location.protocol + '//' + window.location.hostname + '/favicon.ico'; for(i=0; i<links.length; i++){s=links[i].rel; if(s.indexOf('icon') != -1){ wHref = links[i].href }; }; return wHref; })();";
        private bool goToHome, fixedUrl;
        private MainForm mainForm;
        #endregion


        #region Ctors
        // Since we are displaying our controls in a tabbed dock-manager,
        // we can pause and resume rendering ourselves.
        // (See documentation of: IsRendering, OnEnabledChanged)

        public WebDocument()
        {
            InitializeComponent();

            goToHome = true;
            Initialize();
        }

        public WebDocument( Uri url )
        {
            InitializeComponent();

            Initialize();
            webControl.Source = url;
        }

        public WebDocument( IntPtr nativeView )
        {
            InitializeComponent();

            Initialize();
            webControl.NativeView = nativeView;
            // Hide the Downloads button.
            toolStripButton5.Visible = false;
        }

        public WebDocument( Uri url, string title )
        {
            InitializeComponent();

            Initialize();

            fixedUrl = true;
            webControl.Source = url;
            this.Text = title;

            //this.toolStripButton4.Visible = false;
            //this.toolStripButton5.Visible = false;
            //this.addressBox.ReadOnly = true;
            this.pageToolStrip.Visible = false;
        }
        #endregion


        #region Methods
        private void Initialize()
        {
            // Set the source for our data bindings.
            webControlBindingSource.DataSource = webControl;
            // In this example, ShowCreatedWebView of all WebControls, 
            // is handled by a common handler.
            webControl.ShowCreatedWebView += Program.OnShowNewView;
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            cutToolStripMenuItem.Enabled = ( webControl.FocusedElementType == FocusedElementType.EditableContent ) && webControl.HasSelection;
            copyToolStripMenuItem.Enabled = webControl.HasSelection;
            copyHTMLToolStripMenuItem.Enabled = webControl.HasSelection;
            pasteToolStripMenuItem.Enabled = ( webControl.FocusedElementType == FocusedElementType.EditableContent ) && Clipboard.ContainsText();

            if ( goToHome )
                webControl.GoToHome();
        }

        protected override void OnDockStateChanged( EventArgs e )
        {
            base.OnDockStateChanged( e );

            switch ( this.DockState )
            {
                case DockState.Hidden:
                    // Pause rendering when this window is hidden.
                    webControl.Enabled = false;
                    break;

                case DockState.Document:
                    // A non-activated document, should be covered
                    // by others since it is displayed in a tab-control;
                    // we can safely pause rendering when it is not activated.
                    // This is not the case with tool-windows docked on
                    // the sides of the main window that may be visible while not activated;
                    // this is why we default to true for all other states below.
                    webControl.Enabled = this.IsActivated;
                    break;

                default:
                    webControl.Enabled = true;
                    break;
            }
        }

        protected override void OnGotFocus( EventArgs e )
        {
            base.OnGotFocus( e );
            // Transfer focus to the control when the
            // tab acquires it.
            webControl.Focus();
        }

        protected override void OnFormClosed( FormClosedEventArgs e )
        {
            // When the hosting UI has been destroyed,
            // close the WebControl to properly destroy its
            // underlying web-view and dispose resources.
            webControl.Dispose();
            base.OnFormClosed( e );
        }

        private void UpdateFavicon()
        {
            // Execute some simple javascript that will search for a favicon.
            string val = webControl.ExecuteJavascriptWithResult( JS_FAVICON );

            // Check for any errors.
            if ( webControl.GetLastError() != Error.None )
                return;

            // Check if we got a valid response.
            if ( String.IsNullOrEmpty( val ) || !Uri.IsWellFormedUriString( val, UriKind.Absolute ) )
                return;

            // We do not need to perform the download of the favicon synchronously.
            // May be a full icon set (thus big).
            Task.Factory.StartNew<Icon>( GetFavicon, val ).ContinueWith( t =>
            {
                // If the download completed successfully, set the new favicon.
                // This post-completion procedure is executed synchronously.

                if ( t.Exception != null )
                    return;

                if ( t.Result != null )
                    this.Icon = t.Result;

                if ( this.DockPanel != null )
                    this.DockPanel.Refresh();
            },
            TaskScheduler.FromCurrentSynchronizationContext() );
        }

        private static Icon GetFavicon( Object href )
        {
            using ( WebClient client = new WebClient() )
            {
                Byte[] data = client.DownloadData( href.ToString() );

                if ( ( data == null ) || ( data.Length <= 0 ) )
                    return null;

                using ( MemoryStream ms = new MemoryStream( data ) )
                {
                    try
                    {
                        return new Icon( ms, 16, 16 );
                    }
                    catch ( ArgumentException )
                    {
                        // May not be an icon file.
                        using ( Bitmap b = new Bitmap( ms ) )
                            return Icon.FromHandle( b.GetHicon() );
                    }
                }
            }
        }
        #endregion

        #region Properties
        public MainForm MainForm
        {
            get
            {
                if ( mainForm == null )
                    mainForm = this.DockPanel != null ?
                        this.DockPanel.FindForm() as MainForm :
                        Application.OpenForms.OfType<MainForm>().FirstOrDefault();

                return mainForm;
            }
        }

        public bool IsRendering
        {
            get { return webControl.Enabled; }
            set { webControl.Enabled = value; }
        }
        #endregion

        #region Event Handlers

        #region WebControl
        private void webControl_DomReady( object sender, DocumentReadyEventArgs e )
        {
            if ( e.ReadyState == DocumentReadyState.Ready )
                return;

            // DOM is ready. We can start looking for a favicon.
            UpdateFavicon();
        }

        private void webControl_TargetUrlChanged( object sender, UrlEventArgs e )
        {
            MainForm mainForm = this.MainForm;

            if ( mainForm != null )
                mainForm.Status = e.Url.IsBlank() ? String.Empty : e.Url.AbsoluteUri;
        }

        private void webControl_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            MainForm mainForm = this.MainForm;

            switch ( e.PropertyName )
            {
                case "Title":
                    if ( fixedUrl )
                        break;

                    // The WebCocument's title is updated with a data binding.

                    if ( mainForm != null )
                        mainForm.Title = webControl.Title;

                    break;
                case "IsLoading":
                    if ( mainForm != null )
                        mainForm.ShowProgress = webControl.IsLoading;

                    break;

                case "HasSelection":
                    if ( !webControl.HasSelection )
                    {
                        cutToolStripMenuItem.Enabled = false;
                        copyToolStripMenuItem.Enabled = false;
                        copyHTMLToolStripMenuItem.Enabled = false;
                    }
                    else
                    {
                        cutToolStripMenuItem.Enabled = webControl.FocusedElementType == FocusedElementType.EditableContent;
                        copyToolStripMenuItem.Enabled = true;
                        copyHTMLToolStripMenuItem.Enabled = true;
                    }
                    break;

                case "FocusedElementType":
                    pasteToolStripMenuItem.Enabled = ( webControl.FocusedElementType == FocusedElementType.EditableContent ) && Clipboard.ContainsText();
                    break;

                case "IsCrashed":
                    bool isLive = !webControl.IsCrashed;

                    copyToolStripMenuItem.Enabled =
                        cutToolStripMenuItem.Enabled =
                        copyHTMLToolStripMenuItem.Enabled =
                        pasteToolStripMenuItem.Enabled =
                        refreshToolStripMenuItem.Enabled =
                        printToolStripMenuItem.Enabled =
                        selectAllToolStripMenuItem.Enabled = isLive;

                    break;

                default:
                    break;
            }
        }

        private void webControl_BeginLoading( object sender, LoadingFrameEventArgs e )
        {
            if ( !e.IsMainFrame )
                return;

            // Clear the old favicon.
            if ( this.Icon != null )
                this.Icon.Dispose();

            // Restore the default.
            ComponentResourceManager resources = new ComponentResourceManager( typeof( WebDocument ) );
            this.Icon = ( (Icon)( resources.GetObject( "$this.Icon" ) ) );

            if ( this.DockPanel != null )
                this.DockPanel.Refresh();
        }

        private void webControl_SelectLocalFiles( object sender, FileDialogEventArgs e )
        {
            using ( OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = e.Title,
                InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ),
                CheckFileExists = true,
                Multiselect = e.Mode == WebFileChooserMode.OpenMultiple
            } )
            {
                if ( ( dialog.ShowDialog( this.MainForm ) == DialogResult.OK ) && ( dialog.FileNames.Length > 0 ) )
                    e.SelectedFiles = dialog.FileNames;
                else
                    e.Cancel = true;
            }
        }

        private void webControl_WindowClose( object sender, WindowCloseEventArgs e )
        {
            // Respect window.close if we are a popup.
            // Otherwise, ask the user.
            if ( this.DockPanel == null )
                this.Close();
            else if ( MessageBox.Show( this, 
                "The page is asking to close this tab. Do you confirm?", 
                webControl.Source.AbsoluteUri, 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button2 ) == DialogResult.Yes )
                this.Close();
        }
        #endregion

        #region Toolstrip / Menu
        private void closeTabToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.Close();
        }

        private void closeOtherTabsToolStripMenuItem_Click( object sender, EventArgs e )
        {
            var contents = this.DockPanel.Contents.OfType<WebDocument>().Where( wd => wd != this ).ToArray();
            Array.ForEach( contents, c => c.Close() );
        }

        private void toolStripButton1_MouseEnter( object sender, EventArgs e )
        {
            ( (ToolStripButton)sender ).ImageIndex += 1;
        }

        private void toolStripButton1_MouseLeave( object sender, EventArgs e )
        {
            ( (ToolStripButton)sender ).ImageIndex -= 1;
        }

        private void toolStripButton1_Click( object sender, EventArgs e )
        {
            webControl.GoBack();
        }

        private void toolStripButton2_Click( object sender, EventArgs e )
        {
            webControl.GoForward();
        }

        private void toolStripButton3_Click( object sender, EventArgs e )
        {
            webControl.Reload( false );
        }

        private void toolStripButton4_Click( object sender, EventArgs e )
        {
            webControl.GoToHome();
        }

        private void toolStripButton5_Click( object sender, EventArgs e )
        {
            MainForm mainForm = this.MainForm;

            if ( mainForm == null )
                return;

            mainForm.DownloadsWindow.Show( this.DockPanel );
        }

        private void newTabToolStripMenuItem_Click( object sender, EventArgs e )
        {
            if ( this.DockPanel != null )
            {
                WebDocument doc = new WebDocument();
                doc.Show( this.DockPanel );
            }
        }

        private void refreshToolStripMenuItem_Click( object sender, EventArgs e )
        {
            webControl.Reload( false );
        }

        private void printToolStripMenuItem_Click( object sender, EventArgs e )
        {
            if ( !webControl.IsLive || !webControl.IsDocumentReady )
                return;

            // We can display our own dialog to pick a target folder,
            // and then call PrintToFile. But the WebControl already
            // does this in response to JavaScript window.print.
            webControl.ExecuteJavascript( "window.print();" );
        }

        private void cutToolStripMenuItem_Click( object sender, EventArgs e )
        {
            if ( addressBox.Focused )
                addressBox.Cut();
            else if ( webControl.IsLive )
                webControl.Cut();
        }

        private void copyToolStripMenuItem_Click( object sender, EventArgs e )
        {
            if ( addressBox.Focused )
                addressBox.Copy();
            else if ( webControl.IsLive )
                webControl.Copy();
        }

        private void copyHTMLToolStripMenuItem_Click( object sender, EventArgs e )
        {
            if ( !webControl.IsLive )
                return;

            if ( webControl.HasSelection )
                webControl.CopyHTML();
        }

        private void pasteToolStripMenuItem_Click( object sender, EventArgs e )
        {
            if ( addressBox.Focused )
                addressBox.Paste();
            else if ( webControl.IsLive )
                webControl.Paste();
        }

        private void selectAllToolStripMenuItem_Click( object sender, EventArgs e )
        {
            if ( addressBox.Focused )
                addressBox.SelectAll();
            else if ( webControl.IsLive )
                webControl.SelectAll();
        }

        private void customizeToolStripMenuItem_Click( object sender, EventArgs e )
        {
            MessageBox.Show( this, "Not implemented in this sample." );
        }

        private void saveToolStripMenuItem_Click( object sender, EventArgs e )
        {
            MessageBox.Show( this, "Not implemented in this sample." );
        }
        #endregion

        #endregion
    }
}
