using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WhitStream.Admin
{
    /// <summary>
    /// Form for monitoring and administering to the WhitStream Server
    /// </summary>
    public partial class AdminForm : Form
    {
        private WhitStream.QueryEngine.Scheduler.IScheduler sch = null;

        /// <summary> Simple constructor </summary>
        public AdminForm()
        {
            InitializeComponent();
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            if (sch != null)
            {
                tbScheduler.Text = sch.ToString();
            }
        }

        /// <summary>Get and set the scheduler currently executing</summary>
        public WhitStream.QueryEngine.Scheduler.IScheduler Scheduler
        {
            get { return sch; }
            set { sch = value; }
        }

        /// <summary> Update the interface </summary>
        public void UpdateStats()
        {
            lbOperators.DataSource = null;
            lbOperators.DataSource = sch.OpThreads;
            
            //Allow the Admin form to update itself
            System.Windows.Forms.Application.DoEvents();
        }
    }
}
