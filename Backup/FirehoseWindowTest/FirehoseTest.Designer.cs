namespace FirehoseWindowTest
{
    partial class FirehoseTest
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
            this.txtResults = new System.Windows.Forms.TextBox();
            this.lbResultTxtBox = new System.Windows.Forms.Label();
            this.lbResultPanel = new System.Windows.Forms.Label();
            this.btnStartDemo = new System.Windows.Forms.Button();
            this.DGResults = new System.Windows.Forms.DataGridView();
            this.data = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.data2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.DGResults)).BeginInit();
            this.SuspendLayout();
            // 
            // txtResults
            // 
            this.txtResults.Location = new System.Drawing.Point(12, 25);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.Size = new System.Drawing.Size(285, 449);
            this.txtResults.TabIndex = 0;
            // 
            // lbResultTxtBox
            // 
            this.lbResultTxtBox.AutoSize = true;
            this.lbResultTxtBox.Location = new System.Drawing.Point(12, 9);
            this.lbResultTxtBox.Name = "lbResultTxtBox";
            this.lbResultTxtBox.Size = new System.Drawing.Size(42, 13);
            this.lbResultTxtBox.TabIndex = 1;
            this.lbResultTxtBox.Text = "Results";
            // 
            // lbResultPanel
            // 
            this.lbResultPanel.AutoSize = true;
            this.lbResultPanel.Location = new System.Drawing.Point(303, 8);
            this.lbResultPanel.Name = "lbResultPanel";
            this.lbResultPanel.Size = new System.Drawing.Size(42, 13);
            this.lbResultPanel.TabIndex = 3;
            this.lbResultPanel.Text = "Results";
            this.lbResultPanel.Click += new System.EventHandler(this.lbResultPanel_Click);
            // 
            // btnStartDemo
            // 
            this.btnStartDemo.Location = new System.Drawing.Point(12, 485);
            this.btnStartDemo.Name = "btnStartDemo";
            this.btnStartDemo.Size = new System.Drawing.Size(75, 23);
            this.btnStartDemo.TabIndex = 4;
            this.btnStartDemo.Text = "Start Demo";
            this.btnStartDemo.UseVisualStyleBackColor = true;
            this.btnStartDemo.Click += new System.EventHandler(this.btnStartDemo_Click);
            // 
            // DGResults
            // 
            this.DGResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGResults.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.data,
            this.data2});
            this.DGResults.Location = new System.Drawing.Point(306, 25);
            this.DGResults.Name = "DGResults";
            this.DGResults.Size = new System.Drawing.Size(472, 449);
            this.DGResults.TabIndex = 5;
            // 
            // data
            // 
            this.data.HeaderText = "DATA";
            this.data.Name = "data";
            // 
            // data2
            // 
            this.data2.HeaderText = "DATA 2";
            this.data2.Name = "data2";
            // 
            // FirehoseTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(790, 520);
            this.Controls.Add(this.DGResults);
            this.Controls.Add(this.btnStartDemo);
            this.Controls.Add(this.lbResultPanel);
            this.Controls.Add(this.lbResultTxtBox);
            this.Controls.Add(this.txtResults);
            this.Name = "FirehoseTest";
            this.Text = "Firehose Window Test";
            ((System.ComponentModel.ISupportInitialize)(this.DGResults)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox txtResults;
        private System.Windows.Forms.Label lbResultTxtBox;
        private System.Windows.Forms.Label lbResultPanel;
        private System.Windows.Forms.Button btnStartDemo;
        private System.Windows.Forms.DataGridView DGResults;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn data;
        private System.Windows.Forms.DataGridViewTextBoxColumn data2;
    }
}

