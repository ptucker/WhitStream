using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WhitStreamUI
{
    public partial class OperatorsForm : Form
    {
        public OperatorsForm()
        {
            InitializeComponent();
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            // Select Button Clicked
        }

        private void buttonSelect_MouseEnter(object sender, EventArgs e)
        {
            // Mouse is over Select Button
            toolStripStatusLabel1.Text = "Select Operator";
        }

        private void buttonJoin_Click(object sender, EventArgs e)
        {
            // Join Button Clicked
        }

        private void buttonJoin_MouseEnter(object sender, EventArgs e)
        {
            // Mouse is over Join Button
            toolStripStatusLabel1.Text = "Join Operator";
        }

        private void buttonProject_Click(object sender, EventArgs e)
        {
            // Project Button Clicked
        }

        private void buttonProject_MouseEnter(object sender, EventArgs e)
        {
            // Mouse is over Project Button
            toolStripStatusLabel1.Text = "Project Operator";
        }

        private void buttonIntersect_Click(object sender, EventArgs e)
        {
            // Intersect Button Clicked
        }

        private void buttonIntersect_MouseEnter(object sender, EventArgs e)
        {
            // Mouse is over Intersect Button
            toolStripStatusLabel1.Text = "Intersect Operator";
        }

        private void buttonDupElim_Click(object sender, EventArgs e)
        {
            // Dup Elim Button Clicked
        }

        private void buttonDupElim_MouseEnter(object sender, EventArgs e)
        {
            // Mouse is over Dup Elim Button
            toolStripStatusLabel1.Text = "Duplicate Elimination Operator";
        }

        private void buttonUnion_Click(object sender, EventArgs e)
        {
            // Union Button Clicked
        }

        private void buttonUnion_MouseEnter(object sender, EventArgs e)
        {
            // Mouse is over Union Button
            toolStripStatusLabel1.Text = "Union Operator";
        }

        private void buttonGroupBy_Click(object sender, EventArgs e)
        {
            // Group By Button Clicked
        }

        private void buttonGroupBy_MouseEnter(object sender, EventArgs e)
        {
            // Mouse is over GroupBy Button
            toolStripStatusLabel1.Text = "GroupBy Operator";
        }

        private void buttonDifference_Click(object sender, EventArgs e)
        {
            // Difference Button Clicked
        }

        private void buttonDifference_MouseEnter(object sender, EventArgs e)
        {
            // Mouse is over Difference Button
            toolStripStatusLabel1.Text = "Difference Operator";
        }

        private void buttonSort_Click(object sender, EventArgs e)
        {
            // Sort Button Clicked
        }

        private void buttonSort_MouseEnter(object sender, EventArgs e)
        {
            // Mouse is over Sort Button
            toolStripStatusLabel1.Text = "Sort Operator";
        }

        private void buttonInputStream_Click(object sender, EventArgs e)
        {
            // Input Stream Button Clicked
        }

        private void buttonInputStream_MouseEnter(object sender, EventArgs e)
        {
            // Mouse is over Input Stream Button
            toolStripStatusLabel2.Text = "Input Stream Operator";
        }

        private void buttonOutputStream_Click(object sender, EventArgs e)
        {
            // Output Stream Button Clicked
        }

        private void buttonOutputStream_MouseEnter(object sender, EventArgs e)
        {
            // Mouse is over Output Stream Button
            toolStripStatusLabel2.Text = "Output Stream Operator";
        }
    }
}