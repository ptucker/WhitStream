using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WhitStreamGUI
{
    public partial class OpeningScreen : Form
    {
        public OpeningScreen()
        {
            InitializeComponent();
        }

        private void OpenBuilder_Click(object sender, EventArgs e)
        {
            // Opens/Unhides the Builder Screen
            if (!this.BuilderScreen.Focused)
                this.BuilderScreen = new BuilderForm();
            this.BuilderScreen.Show();
        }

        private void OpenViewer_Click(object sender, EventArgs e)
        {
            // Opens/Unhides the Viewer Screen
            if (!this.ViewerScreen.Focused)
                this.ViewerScreen = new ViewerForm();
            this.ViewerScreen.Show();
        }
    }
}