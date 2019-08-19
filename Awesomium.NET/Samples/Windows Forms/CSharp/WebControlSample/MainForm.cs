/********************************************************************************
 *    Project   : Awesomium.NET (TabbedFormsSample)
 *    File      : MainForm.cs
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
 *    Application window. This does not act as a main-parent window. 
 *    It's reusable. The application will exit when all windows are closed.
 *    
 *    
 ********************************************************************************/

#region Using
using System;
using System.Linq;
using Awesomium.Core;
using System.Threading;
using System.Windows.Forms;
using System.Globalization;
using Awesomium.Windows.Forms;
using System.Collections.Generic;
using TabbedFormsSample.Properties;
using WeifenLuo.WinFormsUI.Docking;
#endregion

namespace TabbedFormsSample
{
    partial class MainForm : Form
    {
        #region Fields
        private DownloadsForm downloadsWindow;
        #endregion


        #region Ctors
        public MainForm()
        {
            if ( !WebCore.IsInitialized )
                Thread.CurrentThread.SetCulture( CultureInfo.GetCultureInfo( "de-DE" ) );

            // See Program.cs for WebCore initialization.

            InitializeComponent();

            WebCore.DownloadBegin += OnDownloadBegin;
        }
        #endregion


        #region Methods

        #region Overrides
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Settings.Default.Downloads )
                DownloadsWindow.Show( dockPanel );

            // Create the initial tab.
            this.OpenTab();
        }

        protected override void OnFormClosing( FormClosingEventArgs e )
        {
            // Hide during cleanup.
            this.Hide();

            // Close the views and perform cleanup for every tab.
            List<Form> contents = new List<Form>( dockPanel.Contents.OfType<Form>() );

            foreach ( Form content in contents )
                content.Close();

            // Save application settings.
            Settings.Default.Save();

            // For WebCore.Shutdown, see OnApplicationExit in Program.cs.

            base.OnFormClosing( e );
        }
        #endregion


        #region OpenTab
        internal WebDocument OpenTab( Uri url = null, string title = null )
        {
            // - A tab with no url specified, will open WebCore.HomeURL.
            // - A tab with a predefined title, will not display a toolbar
            //   and address-box. This is used to display fixed web content
            //   such as the Help Contents.
            WebDocument doc = url == null ? new WebDocument() :
                String.IsNullOrEmpty( title ) ? new WebDocument( url ) : new WebDocument( url, title );
            doc.Show( dockPanel );

            return doc;
        }

        internal void OpenTab( WebDocument doc )
        {
            doc.Show( dockPanel );
        }
        #endregion

        #endregion

        #region Properties
        public DownloadsForm DownloadsWindow
        {
            get
            {
                if ( downloadsWindow == null )
                    downloadsWindow = new DownloadsForm( this );

                return downloadsWindow;
            }
        }

        public string Status
        {
            get
            {
                return statusLabel.Text;
            }
            set
            {
                if ( statusLabel.Text == value )
                    return;

                statusLabel.Text = value;
            }
        }

        public string Title
        {
            get
            {
                return this.Text;
            }
            set
            {
                this.Text = String.Format( "{0} - {1}", Application.ProductName, value );
            }
        }

        public bool ShowProgress
        {
            get { return progressBar.Visible; }
            set { progressBar.Visible = value; }
        }

        public DownloadCollection Downloads
        {
            get
            {
                return WebCore.Downloads;
            }
        }
        #endregion

        #region Event Handlers
        private void OnDownloadBegin( object sender, DownloadBeginEventArgs e )
        {
            DownloadsWindow.Show( dockPanel, DockState.DockBottom );
        }

        private void dockPanel_ActiveDocumentChanged( object sender, EventArgs e )
        {
            foreach ( WebDocument doc in dockPanel.Documents )
                // We pause WebControl rendering for documents
                // that are currently not visible.
                doc.IsRendering = ( doc == dockPanel.ActiveDocument );

            if ( dockPanel.ActiveDocument != null )
            {
                WebDocument doc = (WebDocument)dockPanel.ActiveDocument;
                this.Text = String.Format( "{0} - {1}", Application.ProductName, doc.Text );
                doc.Focus();
            }
            else
                this.Text = Application.ProductName;
        }

        private void newToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.OpenTab();
        }

        private void openToolStripMenuItem_Click( object sender, EventArgs e )
        {
            using ( OpenFileDialog dialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ),
                CheckFileExists = true,
                Multiselect = false,
                Filter = "HTML files (*.htm;*.html)|*.htm;*.html|Text files (*.txt)|*.txt|All files (*.*)|*.*"
            } )
            {
                if ( ( dialog.ShowDialog( this ) == DialogResult.OK ) && !String.IsNullOrEmpty( dialog.FileName ) )
                    this.OpenTab( dialog.FileName.ToUri() );
            }
        }

        private void downloadsToolStripMenuItem_CheckedChanged( object sender, EventArgs e )
        {
            if ( downloadsToolStripMenuItem.Checked )
                DownloadsWindow.Show( dockPanel );
            else
                DownloadsWindow.Hide();
        }

        private void viewToolStripMenuItem_DropDownOpening( object sender, EventArgs e )
        {
            // MenuItems (Component), have no DataBindings.
            // Update the UI manually.
            downloadsToolStripMenuItem.Checked = Settings.Default.Downloads;
        }

        private void contentsToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.OpenTab( "http://docs.awesomium.net".ToUri(), "Help" );
        }

        private void supportToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.OpenTab( "http://answers.awesomium.com/spaces/11/".ToUri() );
        }

        private void wikiToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.OpenTab( "http://wiki.awesomium.net".ToUri() );
        }

        private void webSiteToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.OpenTab( "http://www.awesomium.net".ToUri() );
        }

        private void codeplexToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.OpenTab( "http://awesomium.codeplex.com".ToUri() );
        }

        private void exitToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.Close();
        }

        private void aboutToolStripMenuItem_Click( object sender, EventArgs e )
        {
            using ( AboutBox about = new AboutBox() )
                about.ShowDialog( this );
        }

        private void optionsToolStripMenuItem_Click( object sender, EventArgs e )
        {
            MessageBox.Show( this, "Not implemented yet." );
        }
        #endregion
    }
}
