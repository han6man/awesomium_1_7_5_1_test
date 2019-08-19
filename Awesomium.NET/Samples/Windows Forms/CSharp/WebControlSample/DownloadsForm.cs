/********************************************************************************
 *    Project   : Awesomium.NET (TabbedFormsSample)
 *    File      : DownloadsForm.cs
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
 *    A Downloads content window displaying a list of active downloads.
 *    
 *    
 ********************************************************************************/

#region Using
using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using TabbedFormsSample.Properties;
using WeifenLuo.WinFormsUI.Docking;
using System.Collections.Specialized;
using Awesomium.Core;
#endregion

namespace TabbedFormsSample
{
    partial class DownloadsForm : DockContent
    {
        #region Fields
        MainForm mainForm;
        #endregion

        #region Ctors
        public DownloadsForm( MainForm parentForm )
        {
            mainForm = parentForm;

            InitializeComponent();

            downloadCollectionBindingSource.DataSource = parentForm.Downloads;
            ( (INotifyCollectionChanged)parentForm.Downloads ).CollectionChanged += OnSourceCollectionChanged;
        }
        #endregion


        #region Methods
        protected override void OnDockStateChanged( EventArgs e )
        {
            base.OnDockStateChanged( e );

            if ( this.IsHidden )
                Settings.Default.Downloads = false;
            else if ( this.VisibleState != DockState.Unknown )
                Settings.Default.Downloads = true;
        }

        private DownloadItem GetSelectedDownload()
        {
            if ( downloadCollectionDataGridView.SelectedRows.Count == 0 )
                return null;

            return downloadCollectionDataGridView.SelectedRows[ 0 ].DataBoundItem as DownloadItem;
        }

        private void CancelSelectedDownload()
        {
            DownloadItem download = GetSelectedDownload();

            if ( ( download != null ) && download.IsActive )
            {
                if ( MessageBox.Show( this,
                        String.Format( "Are you sure you want to cancel downloading {0}?", download.FileName ),
                        "Awesomium.NET",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question ) == DialogResult.Yes )
                {
                    // Race condition.
                    if ( download.IsActive )
                        download.Cancel();
                }
            }
        }
        #endregion

        #region Properties
        public MainForm MainForm
        {
            get
            {
                return mainForm;
            }
        }
        #endregion

        #region Event Handlers

        private void clearToolStripButton_MouseEnter( object sender, EventArgs e )
        {
            ( (ToolStripButton)sender ).ImageIndex += 1;
        }

        private void clearToolStripButton_MouseLeave( object sender, EventArgs e )
        {
            ( (ToolStripButton)sender ).ImageIndex -= 1;
        }

        private void OnSourceCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            downloadCollectionBindingSource.ResetBindings( true );
        }

        private void downloadCollectionDataGridView_CellContentDoubleClick( object sender, DataGridViewCellEventArgs e )
        {
            if ( e.ColumnIndex != 1 )
                return;

            DownloadItem download = GetSelectedDownload();

            if ( ( download != null ) && download.CanOpenDownloadedFile() )
                download.OpenDownloadedFile();
        }

        private void downloadCollectionDataGridView_CellContentClick( object sender, DataGridViewCellEventArgs e )
        {
            if ( e.ColumnIndex != 5 )
                return;

            this.CancelSelectedDownload();
        }

        private void downloadsContextMenuStrip_Opening( object sender, CancelEventArgs e )
        {
            DownloadItem download = GetSelectedDownload();

            if ( download != null )
            {
                openContainingFolderToolStripMenuItem.Enabled = download.CanOpenDownloadedFile();
                cancelDownloadToolStripMenuItem.Enabled = download.IsActive;
                removeEntryToolStripMenuItem.Enabled = !download.IsActive;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void openContainingFolderToolStripMenuItem_Click( object sender, EventArgs e )
        {
            DownloadItem download = GetSelectedDownload();

            if ( ( download != null ) && download.CanOpenDownloadedFile() )
                download.OpenDownloadedFileFolder();
        }

        private void cancelDownloadToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.CancelSelectedDownload();
        }

        private void removeEntryToolStripMenuItem_Click( object sender, EventArgs e )
        {
            DownloadItem download = GetSelectedDownload();

            if ( ( download != null ) && !download.IsActive )
                this.MainForm.Downloads.Remove( download );
        }

        private void clearToolStripButton_Click( object sender, EventArgs e )
        {
            if ( this.MainForm.Downloads.Count == 0 )
                return;

            this.MainForm.Downloads.ClearNonActive();
        }
        #endregion
    }
}
