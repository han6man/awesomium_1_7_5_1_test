namespace WinFormsSample
{
    partial class WebForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.webControlContextMenu = new Awesomium.Windows.Forms.WebControlContextMenu(this.components);
            this.openSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.webControlContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // webControlContextMenu
            // 
            this.webControlContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openSeparator,
            this.openMenuItem});
            this.webControlContextMenu.Name = "webControlContextMenu";
            this.webControlContextMenu.Size = new System.Drawing.Size(204, 154);
            this.webControlContextMenu.View = null;
            this.webControlContextMenu.Opening += new Awesomium.Windows.Forms.ContextMenuOpeningEventHandler(this.webControlContextMenu_Opening);
            this.webControlContextMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.webControlContextMenu_ItemClicked);
            // 
            // openSeparator
            // 
            this.openSeparator.Name = "openSeparator";
            this.openSeparator.Size = new System.Drawing.Size(200, 6);
            // 
            // openMenuItem
            // 
            this.openMenuItem.Name = "openMenuItem";
            this.openMenuItem.Size = new System.Drawing.Size(203, 22);
            this.openMenuItem.Tag = "open";
            this.openMenuItem.Text = "Open in new window...";
            // 
            // WebForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 729);
            this.DoubleBuffered = true;
            this.Name = "WebForm";
            this.Text = "WinFormsSample";
            this.webControlContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Awesomium.Windows.Forms.WebControlContextMenu webControlContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openMenuItem;
        private System.Windows.Forms.ToolStripSeparator openSeparator;

    }
}

