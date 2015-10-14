namespace WhitStreamUI
{
    partial class OperatorsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OperatorsForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.buttonDifference = new System.Windows.Forms.Button();
            this.buttonUnion = new System.Windows.Forms.Button();
            this.buttonIntersect = new System.Windows.Forms.Button();
            this.buttonJoin = new System.Windows.Forms.Button();
            this.buttonSort = new System.Windows.Forms.Button();
            this.buttonGroupBy = new System.Windows.Forms.Button();
            this.buttonDupElim = new System.Windows.Forms.Button();
            this.buttonProject = new System.Windows.Forms.Button();
            this.buttonSelect = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.buttonOutputStream = new System.Windows.Forms.Button();
            this.buttonInputStream = new System.Windows.Forms.Button();
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.domainUpDown1 = new System.Windows.Forms.DomainUpDown();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.statusStrip2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(242, 406);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.buttonDifference);
            this.tabPage1.Controls.Add(this.buttonUnion);
            this.tabPage1.Controls.Add(this.buttonIntersect);
            this.tabPage1.Controls.Add(this.buttonJoin);
            this.tabPage1.Controls.Add(this.buttonSort);
            this.tabPage1.Controls.Add(this.buttonGroupBy);
            this.tabPage1.Controls.Add(this.buttonDupElim);
            this.tabPage1.Controls.Add(this.buttonProject);
            this.tabPage1.Controls.Add(this.buttonSelect);
            this.tabPage1.Controls.Add(this.statusStrip1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(234, 380);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Operators";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // buttonDifference
            // 
            this.buttonDifference.BackColor = System.Drawing.Color.White;
            this.buttonDifference.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonDifference.Image = ((System.Drawing.Image)(resources.GetObject("buttonDifference.Image")));
            this.buttonDifference.Location = new System.Drawing.Point(85, 219);
            this.buttonDifference.Name = "buttonDifference";
            this.buttonDifference.Size = new System.Drawing.Size(68, 64);
            this.buttonDifference.TabIndex = 9;
            this.buttonDifference.Text = "Difference";
            this.buttonDifference.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonDifference.UseVisualStyleBackColor = false;
            this.buttonDifference.Click += new System.EventHandler(this.buttonDifference_Click);
            this.buttonDifference.MouseEnter += new System.EventHandler(this.buttonDifference_MouseEnter);
            // 
            // buttonUnion
            // 
            this.buttonUnion.BackColor = System.Drawing.Color.White;
            this.buttonUnion.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonUnion.Image = ((System.Drawing.Image)(resources.GetObject("buttonUnion.Image")));
            this.buttonUnion.Location = new System.Drawing.Point(85, 149);
            this.buttonUnion.Name = "buttonUnion";
            this.buttonUnion.Size = new System.Drawing.Size(68, 64);
            this.buttonUnion.TabIndex = 8;
            this.buttonUnion.Text = "Union";
            this.buttonUnion.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonUnion.UseVisualStyleBackColor = false;
            this.buttonUnion.Click += new System.EventHandler(this.buttonUnion_Click);
            this.buttonUnion.MouseEnter += new System.EventHandler(this.buttonUnion_MouseEnter);
            // 
            // buttonIntersect
            // 
            this.buttonIntersect.BackColor = System.Drawing.Color.White;
            this.buttonIntersect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonIntersect.Image = ((System.Drawing.Image)(resources.GetObject("buttonIntersect.Image")));
            this.buttonIntersect.Location = new System.Drawing.Point(85, 79);
            this.buttonIntersect.Name = "buttonIntersect";
            this.buttonIntersect.Size = new System.Drawing.Size(68, 64);
            this.buttonIntersect.TabIndex = 7;
            this.buttonIntersect.Text = "Intersect";
            this.buttonIntersect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonIntersect.UseVisualStyleBackColor = false;
            this.buttonIntersect.Click += new System.EventHandler(this.buttonIntersect_Click);
            this.buttonIntersect.MouseEnter += new System.EventHandler(this.buttonIntersect_MouseEnter);
            // 
            // buttonJoin
            // 
            this.buttonJoin.BackColor = System.Drawing.Color.White;
            this.buttonJoin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonJoin.Image = ((System.Drawing.Image)(resources.GetObject("buttonJoin.Image")));
            this.buttonJoin.Location = new System.Drawing.Point(85, 9);
            this.buttonJoin.Name = "buttonJoin";
            this.buttonJoin.Size = new System.Drawing.Size(68, 64);
            this.buttonJoin.TabIndex = 6;
            this.buttonJoin.Text = "Join";
            this.buttonJoin.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonJoin.UseVisualStyleBackColor = false;
            this.buttonJoin.Click += new System.EventHandler(this.buttonJoin_Click);
            this.buttonJoin.MouseEnter += new System.EventHandler(this.buttonJoin_MouseEnter);
            // 
            // buttonSort
            // 
            this.buttonSort.BackColor = System.Drawing.Color.White;
            this.buttonSort.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonSort.Image = ((System.Drawing.Image)(resources.GetObject("buttonSort.Image")));
            this.buttonSort.Location = new System.Drawing.Point(10, 289);
            this.buttonSort.Name = "buttonSort";
            this.buttonSort.Size = new System.Drawing.Size(68, 64);
            this.buttonSort.TabIndex = 5;
            this.buttonSort.Text = "Sort";
            this.buttonSort.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonSort.UseVisualStyleBackColor = false;
            this.buttonSort.Click += new System.EventHandler(this.buttonSort_Click);
            this.buttonSort.MouseEnter += new System.EventHandler(this.buttonSort_MouseEnter);
            // 
            // buttonGroupBy
            // 
            this.buttonGroupBy.BackColor = System.Drawing.Color.White;
            this.buttonGroupBy.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonGroupBy.Image = ((System.Drawing.Image)(resources.GetObject("buttonGroupBy.Image")));
            this.buttonGroupBy.Location = new System.Drawing.Point(10, 219);
            this.buttonGroupBy.Name = "buttonGroupBy";
            this.buttonGroupBy.Size = new System.Drawing.Size(68, 64);
            this.buttonGroupBy.TabIndex = 4;
            this.buttonGroupBy.Text = "Group By";
            this.buttonGroupBy.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonGroupBy.UseVisualStyleBackColor = false;
            this.buttonGroupBy.Click += new System.EventHandler(this.buttonGroupBy_Click);
            this.buttonGroupBy.MouseEnter += new System.EventHandler(this.buttonGroupBy_MouseEnter);
            // 
            // buttonDupElim
            // 
            this.buttonDupElim.BackColor = System.Drawing.Color.White;
            this.buttonDupElim.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonDupElim.Image = ((System.Drawing.Image)(resources.GetObject("buttonDupElim.Image")));
            this.buttonDupElim.Location = new System.Drawing.Point(10, 149);
            this.buttonDupElim.Name = "buttonDupElim";
            this.buttonDupElim.Size = new System.Drawing.Size(68, 64);
            this.buttonDupElim.TabIndex = 3;
            this.buttonDupElim.Text = "Dup Elim";
            this.buttonDupElim.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonDupElim.UseVisualStyleBackColor = false;
            this.buttonDupElim.Click += new System.EventHandler(this.buttonDupElim_Click);
            this.buttonDupElim.MouseEnter += new System.EventHandler(this.buttonDupElim_MouseEnter);
            // 
            // buttonProject
            // 
            this.buttonProject.BackColor = System.Drawing.Color.White;
            this.buttonProject.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonProject.Image = ((System.Drawing.Image)(resources.GetObject("buttonProject.Image")));
            this.buttonProject.Location = new System.Drawing.Point(10, 79);
            this.buttonProject.Name = "buttonProject";
            this.buttonProject.Size = new System.Drawing.Size(68, 64);
            this.buttonProject.TabIndex = 2;
            this.buttonProject.Text = "Project";
            this.buttonProject.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonProject.UseVisualStyleBackColor = false;
            this.buttonProject.Click += new System.EventHandler(this.buttonProject_Click);
            this.buttonProject.MouseEnter += new System.EventHandler(this.buttonProject_MouseEnter);
            // 
            // buttonSelect
            // 
            this.buttonSelect.BackColor = System.Drawing.Color.White;
            this.buttonSelect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonSelect.Image = ((System.Drawing.Image)(resources.GetObject("buttonSelect.Image")));
            this.buttonSelect.Location = new System.Drawing.Point(10, 9);
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.Size = new System.Drawing.Size(69, 64);
            this.buttonSelect.TabIndex = 1;
            this.buttonSelect.Text = "Select";
            this.buttonSelect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonSelect.UseVisualStyleBackColor = false;
            this.buttonSelect.Click += new System.EventHandler(this.buttonSelect_Click);
            this.buttonSelect.MouseEnter += new System.EventHandler(this.buttonSelect_MouseEnter);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(3, 355);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(228, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "Operator Window";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(78, 17);
            this.toolStripStatusLabel1.Text = "Operator Info";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.buttonOutputStream);
            this.tabPage2.Controls.Add(this.buttonInputStream);
            this.tabPage2.Controls.Add(this.statusStrip2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(234, 380);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Streams";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // buttonOutputStream
            // 
            this.buttonOutputStream.BackColor = System.Drawing.Color.White;
            this.buttonOutputStream.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonOutputStream.Image = ((System.Drawing.Image)(resources.GetObject("buttonOutputStream.Image")));
            this.buttonOutputStream.Location = new System.Drawing.Point(83, 6);
            this.buttonOutputStream.Name = "buttonOutputStream";
            this.buttonOutputStream.Size = new System.Drawing.Size(68, 64);
            this.buttonOutputStream.TabIndex = 8;
            this.buttonOutputStream.Text = "Output";
            this.buttonOutputStream.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonOutputStream.UseVisualStyleBackColor = false;
            this.buttonOutputStream.Click += new System.EventHandler(this.buttonOutputStream_Click);
            this.buttonOutputStream.MouseEnter += new System.EventHandler(this.buttonOutputStream_MouseEnter);
            // 
            // buttonInputStream
            // 
            this.buttonInputStream.BackColor = System.Drawing.Color.White;
            this.buttonInputStream.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonInputStream.Image = ((System.Drawing.Image)(resources.GetObject("buttonInputStream.Image")));
            this.buttonInputStream.Location = new System.Drawing.Point(8, 6);
            this.buttonInputStream.Name = "buttonInputStream";
            this.buttonInputStream.Size = new System.Drawing.Size(69, 64);
            this.buttonInputStream.TabIndex = 7;
            this.buttonInputStream.Text = "Input";
            this.buttonInputStream.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonInputStream.UseVisualStyleBackColor = false;
            this.buttonInputStream.Click += new System.EventHandler(this.buttonInputStream_Click);
            this.buttonInputStream.MouseEnter += new System.EventHandler(this.buttonInputStream_MouseEnter);
            // 
            // statusStrip2
            // 
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel2});
            this.statusStrip2.Location = new System.Drawing.Point(3, 355);
            this.statusStrip2.Name = "statusStrip2";
            this.statusStrip2.Size = new System.Drawing.Size(228, 22);
            this.statusStrip2.TabIndex = 0;
            this.statusStrip2.Text = "Stream Status";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(68, 17);
            this.toolStripStatusLabel2.Text = "Stream Info";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.buttonCancel);
            this.tabPage3.Controls.Add(this.buttonAccept);
            this.tabPage3.Controls.Add(this.groupBox5);
            this.tabPage3.Controls.Add(this.groupBox4);
            this.tabPage3.Controls.Add(this.groupBox3);
            this.tabPage3.Controls.Add(this.groupBox2);
            this.tabPage3.Controls.Add(this.groupBox1);
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.pictureBox1);
            this.tabPage3.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(234, 380);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Op Properties";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.domainUpDown1);
            this.groupBox5.Location = new System.Drawing.Point(74, 6);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(157, 50);
            this.groupBox5.TabIndex = 6;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "GroupBy Types:";
            // 
            // domainUpDown1
            // 
            this.domainUpDown1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.domainUpDown1.Items.Add("GroupByAvg");
            this.domainUpDown1.Items.Add("GroupByCount");
            this.domainUpDown1.Items.Add("GroupByMax");
            this.domainUpDown1.Items.Add("GroupByMin");
            this.domainUpDown1.Items.Add("GroupByThis");
            this.domainUpDown1.Location = new System.Drawing.Point(3, 16);
            this.domainUpDown1.Name = "domainUpDown1";
            this.domainUpDown1.Size = new System.Drawing.Size(151, 20);
            this.domainUpDown1.TabIndex = 0;
            this.domainUpDown1.Text = "Select a Type to Group On";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.listBox1);
            this.groupBox4.Location = new System.Drawing.Point(8, 259);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(223, 75);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Attributes List 2:";
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Items.AddRange(new object[] {
            "Item1",
            "Item2",
            "Item3",
            "Item4",
            "Item5",
            "Item6",
            "Item7",
            "Item8"});
            this.listBox1.Location = new System.Drawing.Point(3, 16);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(217, 56);
            this.listBox1.Sorted = true;
            this.listBox1.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.checkedListBox1);
            this.groupBox3.Location = new System.Drawing.Point(8, 173);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(223, 83);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Attributes List 1:";
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.CheckOnClick = true;
            this.checkedListBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Items.AddRange(new object[] {
            "CheckedItem1",
            "CheckedItem2",
            "CheckedItem3",
            "CheckedItem4",
            "CheckedItem5"});
            this.checkedListBox1.Location = new System.Drawing.Point(3, 16);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(217, 64);
            this.checkedListBox1.Sorted = true;
            this.checkedListBox1.TabIndex = 0;
            this.checkedListBox1.ThreeDCheckBoxes = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.textBox2);
            this.groupBox2.Location = new System.Drawing.Point(8, 125);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(223, 42);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Predicate";
            // 
            // textBox2
            // 
            this.textBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox2.Location = new System.Drawing.Point(3, 16);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(217, 20);
            this.textBox2.TabIndex = 0;
            this.textBox2.Text = "Enter Predicate Here";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Location = new System.Drawing.Point(8, 77);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(223, 42);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Name:";
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox1.Location = new System.Drawing.Point(3, 16);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(217, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "Enter Name Here";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Modern No. 20", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Output Stream Operator";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(50, 50);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(150, 338);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(78, 34);
            this.buttonAccept.TabIndex = 7;
            this.buttonAccept.Text = "Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(88, 338);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(56, 34);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // OperatorsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(242, 406);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(182, 442);
            this.Name = "OperatorsForm";
            this.ShowIcon = false;
            this.Text = "Operators";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.StatusStrip statusStrip2;
        private System.Windows.Forms.Button buttonSelect;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.Button buttonProject;
        private System.Windows.Forms.Button buttonDifference;
        private System.Windows.Forms.Button buttonUnion;
        private System.Windows.Forms.Button buttonIntersect;
        private System.Windows.Forms.Button buttonJoin;
        private System.Windows.Forms.Button buttonSort;
        private System.Windows.Forms.Button buttonGroupBy;
        private System.Windows.Forms.Button buttonDupElim;
        private System.Windows.Forms.Button buttonOutputStream;
        private System.Windows.Forms.Button buttonInputStream;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.DomainUpDown domainUpDown1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;

    }
}