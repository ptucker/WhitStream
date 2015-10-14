namespace WhitStreamGUI
{
    partial class OpeningScreen
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpeningScreen));
            this.OpenViewer = new System.Windows.Forms.Button();
            this.OpenBuilder = new System.Windows.Forms.Button();
            this.BuilderScreen = new WhitStreamGUI.BuilderForm();
            this.ViewerScreen = new WhitStreamGUI.ViewerForm();
            this.SuspendLayout();
            // 
            // OpenViewer
            // 
            this.OpenViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenViewer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OpenViewer.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.OpenViewer.Location = new System.Drawing.Point(354, 235);
            this.OpenViewer.Name = "OpenViewer";
            this.OpenViewer.Size = new System.Drawing.Size(145, 44);
            this.OpenViewer.TabIndex = 3;
            this.OpenViewer.Text = "Open Viewer";
            this.OpenViewer.UseVisualStyleBackColor = true;
            this.OpenViewer.Click += new System.EventHandler(this.OpenViewer_Click);
            // 
            // OpenBuilder
            // 
            this.OpenBuilder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OpenBuilder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OpenBuilder.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.OpenBuilder.Location = new System.Drawing.Point(12, 235);
            this.OpenBuilder.Name = "OpenBuilder";
            this.OpenBuilder.Size = new System.Drawing.Size(145, 44);
            this.OpenBuilder.TabIndex = 2;
            this.OpenBuilder.Text = "Open New Builder";
            this.OpenBuilder.UseVisualStyleBackColor = true;
            this.OpenBuilder.Click += new System.EventHandler(this.OpenBuilder_Click);
            // 
            // BuilderScreen
            // 
            this.BuilderScreen.Add = false;
            this.BuilderScreen.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.BuilderScreen.C = null;
            this.BuilderScreen.ClientSize = new System.Drawing.Size(1280, 974);
            this.BuilderScreen.Connect = false;
            this.BuilderScreen.Disconnect1 = false;
            this.BuilderScreen.Disconnect2 = false;
            this.BuilderScreen.EditPropertiesActive = false;
            this.BuilderScreen.GroupByListBoxSelected = null;
            this.BuilderScreen.Icon = ((System.Drawing.Icon)(resources.GetObject("BuilderScreen.Icon")));
            this.BuilderScreen.IconSize = new System.Drawing.Size(50, 50);
            this.BuilderScreen.ImeMode = System.Windows.Forms.ImeMode.On;
            this.BuilderScreen.IsMdiContainer = true;
            this.BuilderScreen.Location = new System.Drawing.Point(-8, -8);
            this.BuilderScreen.MinimumSize = new System.Drawing.Size(380, 206);
            this.BuilderScreen.Moving = false;
            this.BuilderScreen.Name = "BuilderScreen";
            this.BuilderScreen.OldX = 0;
            this.BuilderScreen.OldY = 0;
            this.BuilderScreen.PropertiesType = null;
            this.BuilderScreen.QueuePos = 0;
            this.BuilderScreen.ShowInTaskbar = false;
            this.BuilderScreen.Text = "Query Builder";
            this.BuilderScreen.TrashList = null;
            this.BuilderScreen.Visible = false;
            this.BuilderScreen.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            // 
            // ViewerScreen
            // 
            this.ViewerScreen.ClientSize = new System.Drawing.Size(632, 453);
            this.ViewerScreen.Icon = ((System.Drawing.Icon)(resources.GetObject("ViewerScreen.Icon")));
            this.ViewerScreen.IsMdiContainer = true;
            this.ViewerScreen.Location = new System.Drawing.Point(230, 230);
            this.ViewerScreen.Name = "ViewerScreen";
            this.ViewerScreen.ShowInTaskbar = false;
            this.ViewerScreen.Text = "Query Viewer";
            this.ViewerScreen.Visible = false;
            // 
            // OpeningScreen
            // 
            this.AcceptButton = this.OpenBuilder;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(511, 291);
            this.Controls.Add(this.OpenViewer);
            this.Controls.Add(this.OpenBuilder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MinimumSize = new System.Drawing.Size(521, 321);
            this.Name = "OpeningScreen";
            this.Text = "WhitStream";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button OpenBuilder;
        private System.Windows.Forms.Button OpenViewer;
        private BuilderForm BuilderScreen;
        private ViewerForm ViewerScreen;
    }
}

