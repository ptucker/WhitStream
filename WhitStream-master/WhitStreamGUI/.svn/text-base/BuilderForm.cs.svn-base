using System;
using System.IO;
using System.Data;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using WhitStream.QueryEngine.QueryOperators;
using WhitStream.QueryEngine;


namespace WhitStreamGUI
{
    
    public partial class BuilderForm : Form
    {
        private bool moving, connect, disconnect1, disconnect2, editPropertiesActive, add, dragging_OperatorToolBar;

        private int oldX, oldY, queuePos;

        private string propertiesType, c, groupByListBoxSelected;

        private System.Drawing.Pen blackpen, coolpen;

        private System.Windows.Forms.PictureBox origin;

        private System.Collections.Generic.List<GraphicOperator> operatorListGraphic, trashList;

        private System.Collections.Generic.List<DynamicButton> dynamicButtons;

        private System.Drawing.Size iconSize;

        private System.Drawing.Point draggedFrom_OperatorToolBar;

        public BuilderForm() { InitializeComponent(); InitializeManualComponents(); }

        private void InitializeManualComponents()
        {
            OperatorListGraphic = new List<GraphicOperator>();
            TrashList = new List<GraphicOperator>();
            DynamicButtonList = new List<DynamicButton>();
            iconSize = new Size(50, 50);
            blackpen = new Pen(Color.Black, 5);
            blackpen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            blackpen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            coolpen = new Pen(Color.Firebrick, 5);
            coolpen.StartCap = System.Drawing.Drawing2D.LineCap.DiamondAnchor;
            coolpen.EndCap = System.Drawing.Drawing2D.LineCap.Triangle;

            //Create WhitStream Queries Folder if it doesnt already exist
            try
            { 
                string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\WhitStream Queries";
                
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("Error Occured while trying to create WhitStream Queries Directory: " + e.Message
                    + "/n" + e.InnerException.ToString());
            }
        }
        public Query userQuery;
        
        public System.Collections.Generic.List<Query> OperatorListQuery;

        public int OldX { get { return oldX; } set { oldX = value; } }

        public int OldY { get { return oldY; } set { oldY = value; } }

        public int QueuePos { get { return queuePos; } set { queuePos = value; } }

        public bool Moving { get { return moving; } set { moving = value; } }

        public bool Connect { get { return connect; } set { connect = value; } }

        public bool Disconnect1 { get { return disconnect1; } set { disconnect1 = value; } }

        public bool Disconnect2 { get { return disconnect2; } set { disconnect2 = value; } }

        public bool EditPropertiesActive { get { return editPropertiesActive; } set { editPropertiesActive = value; } }

        public bool Add { get { return add; } set { add = value; } }

        public bool Dragging_OperatorToolBar { get { return dragging_OperatorToolBar; } set { dragging_OperatorToolBar = value; } }

        public string C { get { return c; } set { c = value; } }

        public string GroupByListBoxSelected { get { return groupByListBoxSelected; } set { groupByListBoxSelected = value; } }

        public string PropertiesType { get { return propertiesType; } set { propertiesType = value; } }

        public PictureBox Origin { get { return origin; } set { origin = value; } }

        public Size IconSize { get { return iconSize; } set { iconSize = value; } }

        public List<GraphicOperator> OperatorListGraphic { get { return operatorListGraphic; } set { operatorListGraphic = value; } }

        public List<GraphicOperator> TrashList { get { return trashList; } set { trashList = value; } }

        public List<DynamicButton> DynamicButtonList { get { return dynamicButtons; } set { dynamicButtons = value; } }

        public ImageList ImageList { get { return imageList1; } set { imageList1 = value; } }

        public Panel BuildArea { get { return buildArea; } set { buildArea = value; } }

        public Pen BlackPen { get { return blackpen; } set { blackpen = value; } }

        public Pen CoolPen { get { return coolpen; } set { coolpen = value; } }

        public Point DraggedFrom_OperatorToolBar { get { return draggedFrom_OperatorToolBar; } set { draggedFrom_OperatorToolBar = value; } }

        private void ShowNewForm(object sender, EventArgs e)
        {
            /* Interesting code, but not relevant to GUI
            // Create a new instance of the child form.
            Form childForm = new Form();
            // Make it a child of this MDI form before showing it.
            childForm.MdiParent = this;
            childForm.Text = "Window " + childFormNumber++;
            childForm.Show();
             * */
            //
            // TODO: This code is interesting, but useless to the project overall
            //
        }

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\WhitStream Queries";
            openFileDialog.InitialDirectory = initialPath;
            openFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = openFileDialog.FileName;
                // TODO: Add code here to open the file.
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            string initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\WhitStream Queries";
            saveFileDialog.InitialDirectory = initialPath;
            saveFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";

            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                SaveQueryXML(initialPath, fileName);
                // TODO: Add code here to save the current contents of the form to a file.
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (xmlFile.fileName == "Untitled")
            {
                SaveFileDialog saveAsFileDialog = new SaveFileDialog();
                string initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\WhitStream Queries";
                saveAsFileDialog.InitialDirectory = initialPath;
                saveAsFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
                if (saveAsFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    string fileName = saveAsFileDialog.FileName;
                    SaveQueryXML(initialPath, fileName);
                    // TODO: Add code here to save the current contents of the form to a file.
                }
            }
            else
            {
                saveToolStripButton_Click(sender, e);     
            }
        }
        
        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            string initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\WhitStream Queries";
            saveFileDialog.InitialDirectory = initialPath;
            saveFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            saveFileDialog.FileName = xmlFile.fileName;

            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                SaveQueryXML(initialPath, fileName);
                // TODO: Add code here to save the current contents of the form to a file.
            }
        }

        private void OpenQueryXML(string directory)
        {
            
        }

        private void SaveQueryXML(string directory, string fileName)
        {
            XmlDocument xd = new XmlDocument();
            TextWriter tw = new StringWriter();
            

            //userQuery.Serialize();


            if (operatorListGraphic.Count != 0)
            {
                //MessageBox.Show(operatorListGraphic.Count.ToString());
                for (int i = 0; i < operatorListGraphic.Count; i++)
                {
                    switch (operatorListGraphic[i].Type.ToString())
                    {
                        case "Select":
                            MessageBox.Show("Select");
                            Query s = new OpGenerate(100);
                            Query q = new OpSelect("$1.1 > 4", s);

                            q.SerializeOp(tw);
                            XmlTextWriter xw = new XmlTextWriter(tw);
                            MessageBox.Show(tw.ToString());
                            MessageBox.Show(xw.ToString());
                            break;
                        case "Project":
                            MessageBox.Show("Project");
                            break;
                        case "Union":
                            MessageBox.Show("Union");
                            break;
                        case "Intersection":
                            MessageBox.Show("Intersection");
                            break;
                        case "Join":
                            MessageBox.Show("Join");
                            break;
                        case "Difference":
                            MessageBox.Show("Difference");
                            break;
                        case "Generate":
                            MessageBox.Show("Generate");
                            break;
                        case "Sort":
                            MessageBox.Show("Sort");
                            break;
                        case "GroupBy":
                            MessageBox.Show("Group-by");
                            break;
                        case "DupElim":
                            MessageBox.Show("Duplicate Elimination");
                            break;
                        case "InputStream":
                            MessageBox.Show("Input Stream");
                            break;
                        case "OutputStream":
                            MessageBox.Show("Output Stream");
                            break;
                        default:
                            break;
                    };
                }
            }
            else
            {
                //Write empty XML file.
                MessageBox.Show("No Operators in Build Area!");
            }
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Use System.Windows.Forms.Clipboard to insert the selected text or images into the clipboard
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Use System.Windows.Forms.Clipboard to insert the selected text or images into the clipboard
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Use System.Windows.Forms.Clipboard.GetText() or System.Windows.Forms.GetData to retrieve information from the clipboard.
        }

        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
            //
            // TODO: This code is interesting, but useless to the project overall
            //
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
            //
            // TODO: This code is interesting, but useless to the project overall
            //
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
            //
            // TODO: This code is interesting, but useless to the project overall
            //
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
            //
            // TODO: This code is interesting, but useless to the project overall
            //
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
            //
            // TODO: This code is interesting, but useless to the project overall
            //
        }

        private void operatorsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            OperatorToolStrip.Visible = OperatorsToolStripMenuItem.Checked;
            OperatorToolStrip.BringToFront();
            if (OperatorsToolStripMenuItem.Checked)
                OperatorToolStrip.Dock = DockStyle.Top;
        }

        private void organizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Organize Operators on build area
            //
            // TODO: Should do the same as the organize button on the toolbar
            //
        }

        private void sampleQueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Places a sample query on the build area atop what is already displayed
            Set_Sample_Query1();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Opens the About Whitstream Child Window
            // Open Help Box Child Window, displays helpful ideas
            bool found = false;
            foreach (Form h in this.MdiChildren)
            {
                if (h is AboutForm)
                    found = true;
            }
            if (!found)
            {
                AboutBox = new AboutForm();
                AboutBox.MdiParent = this;
                AboutBox.Show();
            }
        }

        private void queryViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Opens Query Viewer Screen and hides Builder Screen
            //
            // TODO: Implement the switch to viewer screen 
            //
        }

        private void helpBoxToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Open Help Box Child Window, displays helpful ideas
            bool found = false;
            foreach (Form h in this.MdiChildren)
            {
                if (h is HelpBoxForm)
                    found = true;
            }
            if (!found)
            {
                HelpBox = new HelpBoxForm();
                HelpBox.MdiParent = this;
                HelpBox.Show();
            }
        }

        public void Search_For_Open_Area_BuildArea(PictureBox p)
        {
            if (OperatorListGraphic != null)
            {
                bool again = false;
                foreach (GraphicOperator go in this.OperatorListGraphic)
                {
                    if (p.Bounds.IntersectsWith(go.PicBox.Bounds) && !again && p.Name != go.PicBoxName)
                    {
                        if ((p.Location.X < this.BuildArea.Width - 60) && (this.BuildArea.Width > 100))
                            p.Location = new Point(p.Location.X + 55, p.Location.Y);
                        else
                            p.Location = new Point(25, p.Location.Y + 55);
                        again = true;
                        break;
                    }
                }
                if (!again)
                {
                    foreach (Form f in this.MdiChildren)
                    {
                        if (p.Bounds.IntersectsWith(f.Bounds) && !again)
                        {
                            if ((p.Location.X < this.BuildArea.Width - 60) && (this.BuildArea.Width > 100))
                                p.Location = new Point(p.Location.X + 55, p.Location.Y);
                            else
                                p.Location = new Point(25, p.Location.Y + 55);
                            again = true;
                            break;
                        }
                    }
                }
                if (!again)
                {
                    foreach (Object o in this.Controls)
                    {
                        if (o is ToolStrip)
                        {
                            ToolStrip t = o as ToolStrip;
                            if (p.Bounds.IntersectsWith(t.Bounds) && !again)
                            {
                                if ((p.Location.X < this.BuildArea.Width - 60) && (this.BuildArea.Width > 100))
                                    p.Location = new Point(p.Location.X + 55, p.Location.Y);
                                else
                                    p.Location = new Point(25, p.Location.Y + 55);
                                again = true;
                                break;
                            }
                        }
                    }
                }
                if (again)
                    Search_For_Open_Area_BuildArea(p);
            }
        }

        public void Set_Sample_Query1()
        {
            RemoveFromList("Sample Input Stream", OperatorListGraphic);
            RemoveFromList("Sample Select Operator", OperatorListGraphic);
            RemoveFromList("Sample Output Stream", OperatorListGraphic);
            PictureBox tempInput1 = PicBoxMaker(new Point(100, 250), new Size(50, 50), imageList1.Images[8], "Sample Input Stream");
            PictureBox tempSelect1 = PicBoxMaker(new Point(300, 250), new Size(50, 50), imageList1.Images[13], "Sample Select Operator");
            PictureBox tempOutput = PicBoxMaker(new Point(500, 250), new Size(50, 50), imageList1.Images[11], "Sample Output Stream");
            GraphicOperator InStream1 = new GraphicOperator("InputStream", tempInput1, "Sample Input Stream");
            GraphicOperator Select1 = new GraphicOperator("Select", tempSelect1, "Sample Select");
            GraphicOperator OutStream = new GraphicOperator("OutputStream", tempOutput, "Sample Output Stream");
            tempInput1.Name = InStream1.Name;
            tempSelect1.Name = Select1.Name;
            tempOutput.Name = OutStream.Name;
            Select1.Predicate = "$1.2 > i50";
            Select1.Input_1 = InStream1;
            OutStream.Input_1 = Select1;
            this.BuildArea.Controls.Add(tempInput1);
            this.BuildArea.Controls.Add(tempSelect1);
            this.BuildArea.Controls.Add(tempOutput);
            this.OperatorListGraphic.Add(InStream1);
            this.OperatorListGraphic.Add(Select1);
            this.OperatorListGraphic.Add(OutStream);
            BuildArea.Invalidate();
        }

        public void Set_Sample_Query2()
        {
            RemoveFromList("Sample Input Stream 2", OperatorListGraphic);
            RemoveFromList("Sample Select Operator 2", OperatorListGraphic);
            RemoveFromList("Sample Output Stream 2", OperatorListGraphic);
            PictureBox tempInput1 = PicBoxMaker(new Point(100, 350), new Size(50, 50), imageList1.Images[8], "Sample Input Stream 2");
            PictureBox tempSelect1 = PicBoxMaker(new Point(300, 350), new Size(50, 50), imageList1.Images[13], "Sample Select Operator 2");
            PictureBox tempOutput = PicBoxMaker(new Point(500, 350), new Size(50, 50), imageList1.Images[11], "Sample Output Stream 2");
            GraphicOperator InStream1 = new GraphicOperator("InputStream", tempInput1, "Sample Input Stream 2");
            GraphicOperator Select1 = new GraphicOperator("Select", tempSelect1, "Sample Select 2");
            GraphicOperator OutStream = new GraphicOperator("OutputStream", tempOutput, "Sample Output Stream 2");
            tempInput1.Name = InStream1.Name;
            tempSelect1.Name = Select1.Name;
            tempOutput.Name = OutStream.Name;
            Select1.Predicate = "$1.2 > i50";
            Select1.Input_1 = InStream1;
            OutStream.Input_1 = Select1;
            this.BuildArea.Controls.Add(tempInput1);
            this.BuildArea.Controls.Add(tempSelect1);
            this.BuildArea.Controls.Add(tempOutput);
            this.OperatorListGraphic.Add(InStream1);
            this.OperatorListGraphic.Add(Select1);
            this.OperatorListGraphic.Add(OutStream);
            BuildArea.Invalidate();
        }

        public void Change_PicBox_Name(string oldname, string newname)
        {
            foreach (Control c in this.Controls)
            {
                if (c is PictureBox)
                {
                    PictureBox pb = c as PictureBox;
                    if (pb.Name == oldname)
                    {
                        pb.Name = newname;
                        break;
                    }
                }
            }
        }

        public PictureBox PicBoxMaker(Point point, Size size, Image bg_image, string tag)
        {
            PictureBox pb = new PictureBox();   // Declares new control
            pb.Location = point;    // Sets the location of control in the window
            pb.Size = size; // Sets the height and width of control
            pb.Image = bg_image;  // Sets location of image
            pb.Enabled = true;  // Sets whether the user can interact with control
            pb.SizeMode = PictureBoxSizeMode.StretchImage;
            pb.BorderStyle = BorderStyle.Fixed3D;
            pb.Parent = this;
            pb.MouseDown += PictureBox_MouseDown;
            pb.MouseMove += PictureBox_MouseMove;
            pb.MouseUp += PictureBox_MouseUp;
            pb.MouseDoubleClick += PictureBox_MouseDoubleClick;
            pb.MouseClick += PictureBox_MouseClick;
            pb.Click += PictureBox_Click;
            pb.MouseHover += PictureBox_MouseHover;
            pb.Tag = tag;
            //pb.ContextMenuStrip = RightClick_Menu;
            return pb;
        }

        public bool Check_For_Unique_Name(string name)
        {
            if (OperatorListGraphic != null)
                foreach (GraphicOperator go in OperatorListGraphic)
                {
                    if (go.Name == name)
                        return false;
                }

            return true;
        }

        public string Search_For_Unique_Name(string type, int num)
        {
            string name = String.Format("{0} {1}", type, num);
            bool again = false;
            if (OperatorListGraphic != null)
            {
                foreach (GraphicOperator go in this.OperatorListGraphic)
                {
                    if (go.Name == name)
                    {
                        name = String.Format("{0} {1}", type, num);
                        num++;
                        again = true;
                        break;
                    }
                }
            }
            if (again)
                name = Search_For_Unique_Name(type, num);
            return name;
        }

        public int Find_OpGraphicList_Index(PictureBox pb)
        {
            QueuePos = -1;
            if (OperatorListGraphic != null && pb != null)
            {
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                {
                    if (pb.Name == OperatorListGraphic[i].Name)
                        return i;
                }
            }
            return -1;
        }

        private void BuilderForm_Shown(object sender, EventArgs e)
        {
            foreach (Form f in this.MdiChildren)
            {
                f.Show();
            }
            //
            // TODO: May not be neccessary in program 
            //
        }

        private void BuilderForm_Paint(object sender, PaintEventArgs p)
        {
            //
            // TODO: This may or may not ever get called due to confusion with buildArea_Paint 
            //
            // Occurs when BuilderForm is repainted
            Point OffsetOrigin = new Point(), OffsetConnectedTo = new Point();
            Graphics g = p.Graphics;
            #region Moving
            if (Moving)
            {
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                {
                    // Operator has both inputs
                    if (OperatorListGraphic[i].has_Input1() && OperatorListGraphic[i].has_Input2())
                    {
                        OffsetOrigin = OperatorListGraphic[i].Input_1.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 15);
                        g.DrawLine(BlackPen, OffsetOrigin, OffsetConnectedTo);

                        OffsetOrigin = OperatorListGraphic[i].Input_2.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 35);
                        g.DrawLine(BlackPen, OffsetOrigin, OffsetConnectedTo);
                    }
                    // Operator has only input one
                    else if (OperatorListGraphic[i].has_Input1() && !OperatorListGraphic[i].has_Input2())
                    {
                        OffsetOrigin = OperatorListGraphic[i].Input_1.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 25);
                        g.DrawLine(BlackPen, OffsetOrigin, OffsetConnectedTo);
                    }
                    // Operator has only input two
                    else if (OperatorListGraphic[i].has_Input2() && !OperatorListGraphic[i].has_Input1())
                    {
                        OffsetOrigin = OperatorListGraphic[i].Input_2.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 35);
                        g.DrawLine(BlackPen, OffsetOrigin, OffsetConnectedTo);
                    }
                }
                BuildArea.Invalidate();
            }
            #endregion
            #region Not Moving
            else
            {
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                {
                    // Operator has both inputs
                    if (OperatorListGraphic[i].has_Input1() && OperatorListGraphic[i].has_Input2())
                    {
                        OffsetOrigin = OperatorListGraphic[i].Input_1.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 15);
                        g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);

                        OffsetOrigin = OperatorListGraphic[i].Input_2.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 35);
                        g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);
                    }
                    // Operator has only input one
                    else if (OperatorListGraphic[i].has_Input1() && !OperatorListGraphic[i].has_Input2())
                    {
                        OffsetOrigin = OperatorListGraphic[i].Input_1.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 25);
                        g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);
                    }
                    // Operator has only input two
                    else if (OperatorListGraphic[i].has_Input2() && !OperatorListGraphic[i].has_Input1())
                    {
                        OffsetOrigin = OperatorListGraphic[i].Input_2.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 35);
                        g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);
                    }
                }
            }
            #endregion
        }

        private void HideToolStripButton_Click(object sender, EventArgs e)
        {
            if (OperatorToolStrip.Visible == false)
                OperatorToolStrip.Visible = true;
            else
                OperatorToolStrip.Visible = false;
        }

        private void HideToolStripButton1_Click(object sender, EventArgs e)
        {
            if (OperatorToolStrip.Visible == false)
                OperatorToolStrip.Visible = true;
            else
                OperatorToolStrip.Visible = false;
                OperatorToolStrip.Visible = false;
        }

        private void SelectToolStripButton_Click(object sender, EventArgs e)
        {
            // Select Button Clicked
            if (OperatorsBox != null) { OperatorsBox.Close(); }
            BuildArea.Controls.Add(OperatorsBox = new OperatorsForm("Select", this));
            OperatorsBox.BringToFront();
        }

        private void ProjectToolStripButton_Click(object sender, EventArgs e)
        {
            // Project Button Clicked
            if (OperatorsBox != null) { OperatorsBox.Close(); }
            BuildArea.Controls.Add(OperatorsBox = new OperatorsForm("Project", this));
            OperatorsBox.BringToFront();
        }

        private void DupElimToolStripButton_Click(object sender, EventArgs e)
        {
            // DupElim Button Clicked
            if (OperatorsBox != null) { OperatorsBox.Close(); }
            BuildArea.Controls.Add(OperatorsBox = new OperatorsForm("DupElim", this));
            OperatorsBox.BringToFront();
        }

        private void PictureBox_Click(object sender, EventArgs e)
        {
            this.toolStripStatusLabel.Text = "PictureBox_Click";
            //
            // TODO: This code may never get used/reached 
            //
        }

        private void PictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < OperatorListGraphic.Count; i++)
                OperatorListGraphic[i].PicBox.BorderStyle = BorderStyle.FixedSingle;
            if (sender is PictureBox)
            {
                PictureBox pb = sender as PictureBox;
                QueuePos = Find_OpGraphicList_Index(pb);
                pb.BorderStyle = BorderStyle.Fixed3D;
                if (QueuePos != -1)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        #region Find and Display Available Connections
                        DynamicButtonList.Clear();
                        // This for loop may be redundant because of the one in the PictureBox_MouseDown method ////////
                        for (int i = 0; i < BuildArea.Controls.Count; i++)
                            if (BuildArea.Controls[i] is Button)
                                BuildArea.Controls.RemoveAt(i);
                        ////////////////////////////////////////////////////////////////////////////////////////////////
                        if (OperatorListGraphic[QueuePos].Type == "OutputStream")
                        {
                            if (OperatorListGraphic[QueuePos].Input_1 != null)
                                DynamicButtonList.Add(new DynamicButton(pb, "Disconnect Input 1 Unary", this, OperatorListGraphic[QueuePos], null));
                            DynamicButtonList.Add(new DynamicButton(pb, "Add Query", this, OperatorListGraphic[QueuePos], null));
                            DynamicButtonList.Add(new DynamicButton(pb, "Test Query", this, OperatorListGraphic[QueuePos], null));
                        }
                        else if (OperatorListGraphic[QueuePos].Type == "InputStream")
                        {
                            for (int i = 0; i < OperatorListGraphic.Count; i++)
                            {
                                if (OperatorListGraphic[i].Type != "OutputStream" && OperatorListGraphic[i].Type != "InputStream" && OperatorListGraphic[i] != null)
                                {
                                    if (OperatorListGraphic[i].isUnaryOp())
                                    {
                                        DynamicButtonList.Add(new DynamicButton(OperatorListGraphic[i].PicBox, "Connect Input 1 Unary", this, OperatorListGraphic[i], OperatorListGraphic[QueuePos]));
                                    }
                                    else
                                    {
                                        DynamicButtonList.Add(new DynamicButton(OperatorListGraphic[i].PicBox, "Connect Input 1 Binary", this, OperatorListGraphic[i], OperatorListGraphic[QueuePos]));
                                        DynamicButtonList.Add(new DynamicButton(OperatorListGraphic[i].PicBox, "Connect Input 2 Binary", this, OperatorListGraphic[i], OperatorListGraphic[QueuePos]));
                                    }
                                }
                            }
                        }
                        else // All other operators
                        {
                            for (int i = 0; i < OperatorListGraphic.Count; i++)
                            {
                                if (i != QueuePos && OperatorListGraphic[i] != null && OperatorListGraphic[i].Type != "InputStream")
                                {
                                    if (OperatorListGraphic[i].isUnaryOp())
                                    {
                                        DynamicButtonList.Add(new DynamicButton(OperatorListGraphic[i].PicBox, "Connect Input 1 Unary", this, OperatorListGraphic[i], OperatorListGraphic[QueuePos]));
                                    }
                                    else
                                    {
                                        DynamicButtonList.Add(new DynamicButton(OperatorListGraphic[i].PicBox, "Connect Input 1 Binary", this, OperatorListGraphic[i], OperatorListGraphic[QueuePos]));
                                        DynamicButtonList.Add(new DynamicButton(OperatorListGraphic[i].PicBox, "Connect Input 2 Binary", this, OperatorListGraphic[i], OperatorListGraphic[QueuePos]));
                                    }
                                }
                            }
                            if (OperatorListGraphic[QueuePos].isUnaryOp() && OperatorListGraphic[QueuePos].has_Input1())
                                DynamicButtonList.Add(new DynamicButton(pb, "Disconnect Input 1 Unary", this, OperatorListGraphic[QueuePos], null));
                            else
                            {
                                if (OperatorListGraphic[QueuePos].has_Input1())
                                    DynamicButtonList.Add(new DynamicButton(pb, "Disconnect Input 1 Binary", this, OperatorListGraphic[QueuePos], null));
                                if (OperatorListGraphic[QueuePos].has_Input2())
                                    DynamicButtonList.Add(new DynamicButton(pb, "Disconnect Input 2 Binary", this, OperatorListGraphic[QueuePos], null));
                            }
                        }

                        for (int i = 0; i < DynamicButtonList.Count; i++)
                        {
                            BuildArea.Controls.Add(DynamicButtonList[i].Button);
                            DynamicButtonList[i].Button.BringToFront();
                        }
                        #endregion
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        if (EditOperatorsBox != null) { EditOperatorsBox.Close(); }
                        BuildArea.Controls.Add(EditOperatorsBox = new EditOperatorForm(QueuePos, this));
                        EditOperatorsBox.BringToFront();
                    }
                    toolStripButtonDeleteOperator.Visible = true;
                    toolStripButtonDeleteOperator.Enabled = true;
                    toolStripSeparatorDeleteOperator.Visible = true;
                }
                BuildArea.Invalidate();
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is PictureBox)
            {
                if (e.Button == MouseButtons.Left)
                {
                    PictureBox pb = sender as PictureBox;
                    OldX = e.X;
                    OldY = e.Y;
                    Moving = true;
                    for (int i = 0; i < BuildArea.Controls.Count; i++)
                        if (BuildArea.Controls[i] is Button)
                        {
                            BuildArea.Controls.RemoveAt(i);
                            i = -1;
                        }
                }
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (Moving)
                if (sender is PictureBox)
                    if (e.Button == MouseButtons.Left)
                    {
                        PictureBox pb = sender as PictureBox;
                        pb.Top = pb.Top + (e.Y - OldY);
                        pb.Left = pb.Left + (e.X - OldX);
                        if (pb.Top > pb.Parent.Bottom - pb.Height - 10)
                            pb.Top = pb.Parent.Bottom - pb.Height - 10;
                        else if (pb.Top < 10)
                            pb.Top = 10;
                        if (pb.Left > pb.Parent.Width - pb.Width - 10)
                            pb.Left = pb.Parent.Width - pb.Width - 10;
                        else if (pb.Left < 10)
                            pb.Left = 10;
                    }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is PictureBox)
            {
                PictureBox pb = sender as PictureBox;
                Moving = false;
                //
                // TODO: Delete trashed icons here
                //
                if (autoAlignToolStripMenuItem.Checked)
                {
                    while (pb.Location.X % 60 != 0) { pb.Location = new Point(pb.Location.X - 1, pb.Location.Y); }
                    while (pb.Location.Y % 60 != 0) { pb.Location = new Point(pb.Location.X, pb.Location.Y - 1); }
                }
            }
        }

        private void PictureBox_MouseHover(object sender, EventArgs e)
        {
            if (sender is PictureBox)
            {
                PictureBox pb = sender as PictureBox;
                toolStripStatusLabel.Text = String.Format("Tag: {0}", pb.Tag);
                //
                // TODO: This can be better taken advantage of somehow 
                //
            }
        }

        private void PictureBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //Open EditOperatorBox here?
            //
            // TODO: What to do on doubleclick? 
            //
        }

        private void BuildArea_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < BuildArea.Controls.Count; i++)
                if (BuildArea.Controls[i] is Button)
                {
                    BuildArea.Controls.RemoveAt(i);
                    i = -1;
                }
            for (int i = 0; i < OperatorListGraphic.Count; i++)
                OperatorListGraphic[i].PicBox.BorderStyle = BorderStyle.FixedSingle;
            toolStripButtonDeleteOperator.Visible = false;
            toolStripButtonDeleteOperator.Enabled = false;
            toolStripSeparatorDeleteOperator.Visible = false;
        }

        private void buildArea_ControlAdded(object sender, ControlEventArgs e)
        {
            //MessageBox.Show("buildArea_ControlAdded");
            //
            // TODO: This code may never be used 
            //
        }

        private void GroupByToolStripButton_Click(object sender, EventArgs e)
        {
            // GroupBy Button Clicked
            if (OperatorsBox != null) { OperatorsBox.Close(); }
            BuildArea.Controls.Add(OperatorsBox = new OperatorsForm("GroupBy", this));
            OperatorsBox.BringToFront();
        }

        private void buildArea_Paint(object sender, PaintEventArgs p)
        {
            //
            // TODO: This may or may not ever get called due to confusion with BuilderForm_Paint 
            //
            Point OffsetOrigin = new Point(), OffsetConnectedTo = new Point();
            Graphics g = p.Graphics;
            #region Moving
            if (Moving)
            {
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                {
                    // Operator has both inputs
                    if (OperatorListGraphic[i].has_Input1() && OperatorListGraphic[i].has_Input2())
                    {
                        OffsetOrigin = OperatorListGraphic[i].Input_1.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 15);
                        g.DrawLine(BlackPen, OffsetOrigin, OffsetConnectedTo);

                        OffsetOrigin = OperatorListGraphic[i].Input_2.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 35);
                        g.DrawLine(BlackPen, OffsetOrigin, OffsetConnectedTo);
                    }
                    // Operator has only input one
                    else if (OperatorListGraphic[i].has_Input1() && !OperatorListGraphic[i].has_Input2())
                    {
                        OffsetOrigin = OperatorListGraphic[i].Input_1.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 25);
                        g.DrawLine(BlackPen, OffsetOrigin, OffsetConnectedTo);
                    }
                    // Operator has only input two
                    else if (OperatorListGraphic[i].has_Input2() && !OperatorListGraphic[i].has_Input1())
                    {
                        OffsetOrigin = OperatorListGraphic[i].Input_2.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 35);
                        g.DrawLine(BlackPen, OffsetOrigin, OffsetConnectedTo);
                    }
                }
                BuildArea.Invalidate();
            }
            #endregion
            #region Not Moving
            else
            {
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                {
                    // Operator has both inputs
                    if (OperatorListGraphic[i].has_Input1() && OperatorListGraphic[i].has_Input2())
                    {
                        OffsetOrigin = OperatorListGraphic[i].Input_1.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 15);
                        g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);

                        OffsetOrigin = OperatorListGraphic[i].Input_2.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 35);
                        g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);
                    }
                    // Operator has only input one
                    else if (OperatorListGraphic[i].has_Input1() && !OperatorListGraphic[i].has_Input2())
                    {
                        OffsetOrigin = OperatorListGraphic[i].Input_1.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 25);
                        g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);
                    }
                    // Operator has only input two
                    else if (OperatorListGraphic[i].has_Input2() && !OperatorListGraphic[i].has_Input1())
                    {
                        OffsetOrigin = OperatorListGraphic[i].Input_2.PicBox.Location;
                        OffsetOrigin.Offset(50, 25);
                        OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                        OffsetConnectedTo.Offset(0, 35);
                        g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);
                    }
                }
            }
            #endregion
        }

        private void SortToolStripButton_Click(object sender, EventArgs e)
        {
            // Sort Button Clicked
            if (OperatorsBox != null) { OperatorsBox.Close(); }
            BuildArea.Controls.Add(OperatorsBox = new OperatorsForm("Sort", this));
            OperatorsBox.BringToFront();
        }

        private void toolStripButtonOrganize_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel.Text = "Organized Build Area";
            int oy = 0, ox = 0, pblx = 10, pbly = 10;
            List<GraphicOperator> OutputOps = new List<GraphicOperator>();
            List<GraphicOperator> UnassignedOps = new List<GraphicOperator>();
            #region Organize Algorithm            
            BuildArea.VerticalScroll.Value = 0;
            BuildArea.HorizontalScroll.Value = 0;
            // Organize Outputs along right side first, spaced for multiple inputs to branch out
            // Organize all others along the top or left, just somewhere out of the way
            foreach (GraphicOperator go in OperatorListGraphic)
            {
                if (go.Type == "OutputStream")
                {
                    OutputOps.Add(go);
                    go.PicBox.Location = new Point(BuildArea.Width - go.PicBox.Width - 10 - ox, 50 + oy);
                    if (oy < BuildArea.Height - 49) { oy += 100; }
                    else { oy = 50; ox += 60; }
                }
                else { UnassignedOps.Add(go); }
            }
            if (OutputOps.Count == 1) { OutputOps[0].PicBox.Location = new Point((9 * BuildArea.Width / 10), BuildArea.Height / 2); }
            if (OutputOps.Count > 0)
            {
                for (int i = 0; i < OutputOps.Count; i++)
                {
                    RemoveFromList(OutputOps[i].Input_1, UnassignedOps);
                    RepositionInputs(OutputOps[i], UnassignedOps, 0, true, 25);
                }
            }
            if (UnassignedOps.Count > 0)
            {
                for (int i = 0; i < UnassignedOps.Count; i++)
                {
                    UnassignedOps[i].PicBox.Location = new Point(pblx, pbly);
                    if (pblx < BuildArea.Width - 110) { pblx += 60; }
                    else
                    {
                        pblx = 10;
                        if (pbly < BuildArea.Height - 110) { pbly += 60; }
                        else { pbly = 10; }
                    }
                }
            }            
            #endregion
            int leftmost = 0;
            int topmost = 0;
            for (int i = 0; i < OperatorListGraphic.Count; i++)
            {
                int j = i;
                if (OperatorListGraphic[j].PicBox.Location.Y < topmost)
                {
                    topmost = OperatorListGraphic[i].PicBox.Location.Y;
                    i = -1;
                }
                if (OperatorListGraphic[j].PicBox.Location.X < leftmost)
                {
                    leftmost = OperatorListGraphic[j].PicBox.Location.X;
                    i = -1;
                }
            }
            ShiftOperators(leftmost, topmost);
            DynamicButtonList.Clear();
            for (int i = 0; i < BuildArea.Controls.Count; i++)
                if (BuildArea.Controls[i] is Button)
                {
                    BuildArea.Controls.RemoveAt(i);
                    i = -1;
                }
            BuildArea.Invalidate();
        }

        private void RepositionInputs(GraphicOperator go, List<GraphicOperator> list, int upward, bool input1, int Xseparator)//input1 is whether go is coming in as an input1 or input2
        {
            if (go != null && list != null && (go.Input_1 != null || go.Input_2 != null))
            {
                #region level direction
                if (upward == 0)
                {
                    if (go.isUnaryOp() && go.has_Input1())
                    {
                        go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y);
                        RemoveFromList(go.Input_1, list);
                        RepositionInputs(go.Input_1, list, 0, true, Xseparator);
                    }
                    else
                    {
                        if (go.has_Input1() && go.has_Input2())
                        {
                            go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y - 60);
                            go.Input_2.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y + 60);
                            RemoveFromList(go.Input_1, list);
                            RemoveFromList(go.Input_2, list);
                            RepositionInputs(go.Input_1, list, 1, true, Xseparator);
                            RepositionInputs(go.Input_2, list, 2, false, Xseparator);
                        }
                        else if (go.has_Input1())
                        {
                            RemoveFromList(go.Input_1, list);
                            go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y);
                            RepositionInputs(go.Input_1, list, 0, true, Xseparator);
                        }
                        else if (go.has_Input2())
                        {
                            RemoveFromList(go.Input_2, list);
                            go.Input_2.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y);
                            RepositionInputs(go.Input_2, list, 0, false, Xseparator);
                        }
                    }
                }
                #endregion
                #region upward
                else if (upward == 1)
                {
                    if (input1)
                    {
                        if (go.isUnaryOp() && go.has_Input1())
                        {
                            go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y - 60);
                            RemoveFromList(go.Input_1, list);
                            RepositionInputs(go.Input_1, list, 1, true, Xseparator);
                        }
                        else
                        {
                            if (go.has_Input1() && go.has_Input2())
                            {
                                go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y - 120);
                                go.Input_2.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y - 60);
                                RemoveFromList(go.Input_1, list);
                                RemoveFromList(go.Input_2, list);
                                RepositionInputs(go.Input_1, list, 1, true, Xseparator);
                                RepositionInputs(go.Input_2, list, 1, false, Xseparator);
                            }
                            else if (go.has_Input1())
                            {
                                go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y - 60);
                                RemoveFromList(go.Input_1, list);
                                RepositionInputs(go.Input_1, list, 1, true, Xseparator);
                            }
                            else if (go.has_Input2())
                            {
                                go.Input_2.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y - 60);
                                RemoveFromList(go.Input_2, list);
                                RepositionInputs(go.Input_2, list, 1, false, Xseparator);
                            }
                        }
                    }
                    else //input2
                    {
                        if (go.isUnaryOp() && go.has_Input1())
                        {
                            go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y);
                            RemoveFromList(go.Input_1, list);
                            RepositionInputs(go.Input_1, list, 1, true, Xseparator);
                        }
                        else
                        {
                            if (go.has_Input1() && go.has_Input2())
                            {
                                go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y - 60);
                                go.Input_2.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y);
                                RemoveFromList(go.Input_1, list);
                                RemoveFromList(go.Input_2, list);
                                RepositionInputs(go.Input_1, list, 1, true, Xseparator);
                                RepositionInputs(go.Input_2, list, 1, false, Xseparator);
                            }
                            else if (go.has_Input1())
                            {
                                go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y);
                                RemoveFromList(go.Input_1, list);
                                RepositionInputs(go.Input_1, list, 1, true, Xseparator);
                            }
                            else if (go.has_Input2())
                            {
                                go.Input_2.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y);
                                RemoveFromList(go.Input_2, list);
                                RepositionInputs(go.Input_2, list, 1, false, Xseparator);
                            }
                        }
                    }
                }
                #endregion
                #region downward
                else //downward
                {
                    if (input1)
                    {
                        if (go.isUnaryOp() && go.has_Input1())
                        {
                            go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y);
                            RemoveFromList(go.Input_1, list);
                            RepositionInputs(go.Input_1, list, 2, true, Xseparator);
                        }
                        else
                        {
                            if (go.has_Input1() && go.has_Input2())
                            {
                                go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y);
                                go.Input_2.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y + 60);
                                RemoveFromList(go.Input_1, list);
                                RemoveFromList(go.Input_2, list);
                                RepositionInputs(go.Input_1, list, 2, true, Xseparator);
                                RepositionInputs(go.Input_2, list, 2, false, Xseparator);
                            }
                            else if (go.has_Input1())
                            {
                                go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y);
                                RemoveFromList(go.Input_1, list);
                                RepositionInputs(go.Input_1, list, 2, true, Xseparator);
                            }
                            else if (go.has_Input2())
                            {
                                go.Input_2.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y);
                                RemoveFromList(go.Input_2, list);
                                RepositionInputs(go.Input_2, list, 2, false, Xseparator);
                            }
                        }
                    }
                    else //input2
                    {
                        if (go.isUnaryOp() && go.has_Input1())
                        {
                            go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y + 60);
                            RemoveFromList(go.Input_1, list);
                            RepositionInputs(go.Input_1, list, 2, true, Xseparator);
                        }
                        else
                        {
                            if (go.has_Input1() && go.has_Input2())
                            {
                                go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y + 60);
                                go.Input_2.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y + 120);
                                RemoveFromList(go.Input_1, list);
                                RemoveFromList(go.Input_2, list);
                                RepositionInputs(go.Input_1, list, 2, true, Xseparator);
                                RepositionInputs(go.Input_2, list, 2, false, Xseparator);
                            }
                            else if (go.has_Input1())
                            {
                                go.Input_1.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y + 60);
                                RemoveFromList(go.Input_1, list);
                                RepositionInputs(go.Input_1, list, 2, true, Xseparator);
                            }
                            else if (go.has_Input2())
                            {
                                go.Input_2.PicBox.Location = new Point(go.PicBox.Location.X - 50 - Xseparator, go.PicBox.Location.Y + 60);
                                RemoveFromList(go.Input_2, list);
                                RepositionInputs(go.Input_2, list, 2, false, Xseparator);
                            }
                        }
                    }
                }
                #endregion
            }
        }

        private void UnaryReposition(GraphicOperator left, GraphicOperator right, bool upwards, bool passedBinary)
        {
            //
            // TODO: double check that this code actually gets used, may be old code 
            //
            if (left != null && right != null)
            {
                QueuePos = Find_OpGraphicList_Index(left.PicBox);
                if (QueuePos != -1)
                {
                    if (passedBinary)
                    {
                        if (upwards)
                            OperatorListGraphic[QueuePos].PicBox.Location = new Point(right.PicBox.Location.X - 60, right.PicBox.Location.Y - 60);
                        else
                            OperatorListGraphic[QueuePos].PicBox.Location = new Point(right.PicBox.Location.X - 60, right.PicBox.Location.Y + 60);
                    }
                    else
                        OperatorListGraphic[QueuePos].PicBox.Location = new Point(right.PicBox.Location.X - 60, right.PicBox.Location.Y);
                }
                BuildArea.Invalidate();
            }
        }

        private void BinaryReposition(GraphicOperator topleft, GraphicOperator bottomleft, GraphicOperator right, bool upwards, bool passedBinary)
        {
            //
            // TODO: double check that this method actually gets used, may be old code 
            //
            if (topleft != null && bottomleft != null && right != null)
            {
                int Q1 = -1, Q2 = -1;
                if (passedBinary)
                {
                    if (upwards)
                    {
                        Q1 = Find_OpGraphicList_Index(topleft.PicBox);
                        if (Q1 != -1)
                        {
                            OperatorListGraphic[Q1].PicBox.Location = new Point(right.PicBox.Location.X - 60, right.PicBox.Location.Y - 60);
                            Q2 = Find_OpGraphicList_Index(bottomleft.PicBox);
                            if (Q2 != -1)
                                OperatorListGraphic[Q2].PicBox.Location = new Point(OperatorListGraphic[Q1].PicBox.Location.X, OperatorListGraphic[Q1].PicBox.Location.Y + 60);
                        }
                    }
                    else
                    {
                        Q1 = Find_OpGraphicList_Index(topleft.PicBox);
                        if (Q1 != -1)
                        {
                            OperatorListGraphic[Q1].PicBox.Location = new Point(right.PicBox.Location.X - 60, right.PicBox.Location.Y);
                            Q2 = Find_OpGraphicList_Index(bottomleft.PicBox);
                            if (Q2 != -1)
                                OperatorListGraphic[Q2].PicBox.Location = new Point(OperatorListGraphic[Q1].PicBox.Location.X, right.PicBox.Location.Y + 60);
                        }
                    }
                }
                else
                {
                    Q1 = Find_OpGraphicList_Index(topleft.PicBox);
                    if (Q1 != -1)
                    {
                        OperatorListGraphic[Q1].PicBox.Location = new Point(right.PicBox.Location.X - 60, right.PicBox.Location.Y - 60);
                        Q2 = Find_OpGraphicList_Index(bottomleft.PicBox);
                        if (Q2 != -1)
                            OperatorListGraphic[Q2].PicBox.Location = new Point(OperatorListGraphic[Q1].PicBox.Location.X, right.PicBox.Location.Y + 60);
                    }
                }
                BuildArea.Invalidate();
            }
        }

        private void OrganizeAlgorithm(GraphicOperator go, List<GraphicOperator> list, bool upwards, bool passedBinary)
        {
            //
            // TODO: double check that this code actually gets used, may be old code 
            //
            GraphicOperator temp1 = new GraphicOperator();
            GraphicOperator temp2 = new GraphicOperator();
            temp1 = go.Input_1;
            temp2 = go.Input_2;
            if (temp1 != null)
            {
                if (temp1.isUnaryOp() && temp1.has_Input1())
                {
                    UnaryReposition(temp1.Input_1, temp1, upwards, passedBinary);
                    OrganizeAlgorithm(temp1, list, upwards, passedBinary); //also try !passedBinary
                }
                else if (temp1.isBinaryOp() && (temp1.has_Input1() || temp1.has_Input2()))
                {
                    if (temp1.has_Input1() && temp1.has_Input2())
                    {
                        BinaryReposition(temp1.Input_1, temp1.Input_2, temp1, upwards, passedBinary);
                        OrganizeAlgorithm(temp1, list, true, true);
                    }
                    else if (temp1.has_Input1() && !temp1.has_Input2())
                    {
                        UnaryReposition(temp1, temp1, upwards, passedBinary);
                        OrganizeAlgorithm(temp1, list, upwards, passedBinary);
                    }
                    else if (!temp1.has_Input1() && temp1.has_Input2())
                    {
                        UnaryReposition(temp1.Input_2, temp1, upwards, passedBinary);
                        OrganizeAlgorithm(temp1, list, upwards, passedBinary);
                    }
                }
                RemoveFromList(temp1, list);
            }
            if (temp2 != null)
            {
                if (temp2.isUnaryOp() && temp2.has_Input1())
                {
                    UnaryReposition(temp2.Input_1, temp2, upwards, passedBinary);
                    OrganizeAlgorithm(temp2, list, upwards, passedBinary);
                }
                else if (temp2.isBinaryOp() && (temp2.has_Input1() || temp2.has_Input2()))
                {
                    if (temp2.has_Input1() && temp2.has_Input2())
                    {
                        BinaryReposition(temp2.Input_1, temp2.Input_2, temp2, upwards, passedBinary);
                        OrganizeAlgorithm(temp2, list, false, true);
                    }
                    else if (temp2.has_Input1() && !temp2.has_Input2())
                    {
                        UnaryReposition(temp2.Input_1, temp2, upwards, passedBinary);
                        OrganizeAlgorithm(temp2, list, upwards, passedBinary);
                    }
                    else if (!temp2.has_Input1() && temp2.has_Input2())
                    {
                        UnaryReposition(temp2.Input_2, temp2, upwards, passedBinary);
                        OrganizeAlgorithm(temp2, list, upwards, passedBinary);
                    }
                }
                RemoveFromList(temp2, list);
            }
        }

        private void RemoveFromList(GraphicOperator remove, List<GraphicOperator> list)
        {
            if (remove != null && list != null)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i].PicBoxName == remove.PicBoxName) { list.RemoveAt(i); break; }
            }
        }

        private void RemoveFromList(string remove, List<GraphicOperator> list)
        {
            if (remove != null && list != null)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i].PicBoxName == remove) { list.RemoveAt(i); break; }
            }
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            // Clears BuildArea
            OperatorListGraphic.Clear();
            BuildArea.Controls.Clear();
            BuildArea.Invalidate();
        }

        private void JoinToolStripButton_Click(object sender, EventArgs e)
        {
            // Join Button Clicked
            if (OperatorsBox != null) { OperatorsBox.Close(); }
            BuildArea.Controls.Add(OperatorsBox = new OperatorsForm("Join", this));
            OperatorsBox.BringToFront();
        }

        private void InstreamToolStripButton_Click(object sender, EventArgs e)
        {
            // InputStream Button Clicked
            if (OperatorsBox != null) { OperatorsBox.Close(); }
            BuildArea.Controls.Add(OperatorsBox = new OperatorsForm("InputStream", this));
            OperatorsBox.BringToFront();
        }

        private void OutstreamToolStripButton_Click(object sender, EventArgs e)
        {
            // OutputStream Button Clicked
            if (OperatorsBox != null) { OperatorsBox.Close(); }
            BuildArea.Controls.Add(OperatorsBox = new OperatorsForm("OutputStream", this));
            OperatorsBox.BringToFront();
        }

        private void OrientationChangerToolStripButton_Click(object sender, EventArgs e)
        {
            if (OperatorToolStrip.LayoutStyle == ToolStripLayoutStyle.HorizontalStackWithOverflow)
            {
                switch (OperatorToolStrip.Dock)
                {
                    case DockStyle.Top:
                        {
                            OperatorToolStrip.Dock = DockStyle.Left;
                            break;
                        }
                    case DockStyle.Bottom:
                        {
                            OperatorToolStrip.Dock = DockStyle.Right;
                            break;
                        }
                    case DockStyle.None:
                        { break; }
                }
                OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            }
            else
            {
                switch (OperatorToolStrip.Dock)
                {
                    case DockStyle.Left:
                        {
                            OperatorToolStrip.Dock = DockStyle.Top;
                            break;
                        }
                    case DockStyle.Right:
                        {
                            OperatorToolStrip.Dock = DockStyle.Bottom;
                            break;
                        }
                    case DockStyle.None:
                        { break; }
                }
                OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            }
            OperatorToolStrip.BringToFront();
        }

        private void DifferenceToolStripButton_Click(object sender, EventArgs e)
        {
            // Difference Button Clicked
            if (OperatorsBox != null) { OperatorsBox.Close(); }
            BuildArea.Controls.Add(OperatorsBox = new OperatorsForm("Difference", this));
            OperatorsBox.BringToFront();
        }

        private void UnionToolStripButton_Click(object sender, EventArgs e)
        {
            // Union Button Clicked
            if (OperatorsBox != null) { OperatorsBox.Close(); }
            BuildArea.Controls.Add(OperatorsBox = new OperatorsForm("Union", this));
            OperatorsBox.BringToFront();
        }

        private void IntersectToolStripButton_Click(object sender, EventArgs e)
        {
            // Intersect Button Clicked
            if (OperatorsBox != null) { OperatorsBox.Close(); }
            BuildArea.Controls.Add(OperatorsBox = new OperatorsForm("Intersect", this));
            OperatorsBox.BringToFront();
        }

        private void IncreaseSizeToolStripButton_Click(object sender, EventArgs e)
        {
            if (OperatorToolStrip.ImageScalingSize.Height < 50 && OperatorToolStrip.ImageScalingSize.Width < 50)
                OperatorToolStrip.ImageScalingSize = new Size(OperatorToolStrip.ImageScalingSize.Width + 5, OperatorToolStrip.ImageScalingSize.Height + 5);
            DockStyle tempDock = OperatorToolStrip.Dock;
            OperatorToolStrip.Dock = DockStyle.None;
            OperatorToolStrip.Dock = tempDock;
        }

        private void DecreaseSizeToolStripButton_Click(object sender, EventArgs e)
        {
            if (OperatorToolStrip.ImageScalingSize.Height > 10 && OperatorToolStrip.ImageScalingSize.Width > 10)
                OperatorToolStrip.ImageScalingSize = new Size(OperatorToolStrip.ImageScalingSize.Width - 5, OperatorToolStrip.ImageScalingSize.Height - 5);
            DockStyle tempDock = OperatorToolStrip.Dock;
            OperatorToolStrip.Dock = DockStyle.None;
            OperatorToolStrip.Dock = tempDock;
        }

        private void toolStripButtonAlignGrid_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < OperatorListGraphic.Count; i++)
            {
                if (!scrollableBuildAreaToolStripMenuItem.Checked && (OperatorListGraphic[i].PicBox.Location.X < 0 || OperatorListGraphic[i].PicBox.Location.X > (OperatorListGraphic[i].PicBox.Parent.Size.Width - 60) || OperatorListGraphic[i].PicBox.Location.Y < 0 || OperatorListGraphic[i].PicBox.Location.Y > (OperatorListGraphic[i].PicBox.Parent.Size.Width - 60)))
                {
                    Search_For_Open_Area_BuildArea(OperatorListGraphic[i].PicBox);
                }
                if (OperatorListGraphic[i].PicBox.Location.X % 60 != 0)
                {
                    OperatorListGraphic[i].PicBox.Location = new Point(OperatorListGraphic[i].PicBox.Location.X - 1, OperatorListGraphic[i].PicBox.Location.Y);
                    i = -1;
                }
                else if (OperatorListGraphic[i].PicBox.Location.Y % 60 != 0)
                {
                    OperatorListGraphic[i].PicBox.Location = new Point(OperatorListGraphic[i].PicBox.Location.X, OperatorListGraphic[i].PicBox.Location.Y - 1);
                    i = -1;
                }
            }
            int leftmost = 0;
            int topmost = 0;
            for (int i = 0; i < OperatorListGraphic.Count; i++)
            {
                if (OperatorListGraphic[i].PicBox.Location.Y < topmost)
                {
                    topmost = OperatorListGraphic[i].PicBox.Location.Y;
                    i = -1;
                }
                if (OperatorListGraphic[i].PicBox.Location.X < leftmost)
                {
                    leftmost = OperatorListGraphic[i].PicBox.Location.X;
                    i = -1;
                }
            }
            ShiftOperators(leftmost, topmost);
            DynamicButtonList.Clear();
            for (int i = 0; i < BuildArea.Controls.Count; i++)
                if (BuildArea.Controls[i] is Button)
                {
                    BuildArea.Controls.RemoveAt(i);
                    i = -1;
                }
            BuildArea.Invalidate();
        }

        

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            //
            // TODO: put help code here
            //
        }

        public void AddQuery()
        {
            if (TestQuery())
                AddQueryToViewerList();
        }

        private void AddQueryToViewerList()
        {
            //
            // TODO: Add the query to the viewer list if it passes TestQuery()
            //
        }

        public bool TestQuery()
        {
            return false;
            //
            // TODO: write code to test the query here 
            //
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool temp = TestQuery();
            //
            // TODO: write code to test the query here or simply call TestQuery()
            //
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TestQuery())
                AddQueryToViewerList();
            //
            // TODO: write code to test and/or add the query to viewer list 
            //
        }

        private void scrollableBuildAreaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BuildArea.AutoScroll == true)
            {
                scrollableBuildAreaToolStripMenuItem.Checked = false;
                BuildArea.AutoScroll = false;
            }
            else
            {
                scrollableBuildAreaToolStripMenuItem.Checked = true;
                BuildArea.AutoScroll = true;
            }
        }

        private void toolStripButtonDeleteOperator_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < OperatorListGraphic.Count; i++)
            {
                if (OperatorListGraphic[i].PicBox.BorderStyle == BorderStyle.Fixed3D)
                {
                    for (int j = 0; j < OperatorListGraphic.Count; j++)
                    {
                        if (OperatorListGraphic[j].Input_1 == OperatorListGraphic[i] && j != i)
                            OperatorListGraphic[j].Input_1 = null;
                        if (OperatorListGraphic[j].Input_2 == OperatorListGraphic[i] && j != i)
                            OperatorListGraphic[j].Input_2 = null;
                    }
                    BuildArea.Controls.Remove(OperatorListGraphic[i].PicBox);
                    TrashList.Add(OperatorListGraphic[i]);
                    OperatorListGraphic.RemoveAt(i);
                    break;
                }
            }
            DynamicButtonList.Clear();
            for (int i = 0; i < BuildArea.Controls.Count; i++)
                if (BuildArea.Controls[i] is Button)
                {
                    BuildArea.Controls.RemoveAt(i);
                    i = -1;
                }
            toolStripButtonDeleteOperator.Visible = false;
            toolStripButtonDeleteOperator.Enabled = false;
            toolStripSeparatorDeleteOperator.Visible = false;
            BuildArea.Invalidate();
        }

        public void ShiftOperators(int leftmost, int topmost)
        {
            if (leftmost < 0)
            {
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                    OperatorListGraphic[i].PicBox.Location = new Point(OperatorListGraphic[i].PicBox.Location.X - leftmost + 10, OperatorListGraphic[i].PicBox.Location.Y);
            }
            else
            {
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                    OperatorListGraphic[i].PicBox.Location = new Point(OperatorListGraphic[i].PicBox.Location.X + leftmost + 10, OperatorListGraphic[i].PicBox.Location.Y);
            }
            if (topmost < 0)
            {
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                    OperatorListGraphic[i].PicBox.Location = new Point(OperatorListGraphic[i].PicBox.Location.X, OperatorListGraphic[i].PicBox.Location.Y - topmost + 10);
            }
            else
            {
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                    OperatorListGraphic[i].PicBox.Location = new Point(OperatorListGraphic[i].PicBox.Location.X, OperatorListGraphic[i].PicBox.Location.Y + topmost + 10);
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // At some point I want an OptionsForm to pop up with various changes that can be made to the Builder
            MessageBox.Show("OptionsForm will go here");
            //
            // TODO: create options form 
            //
        }

        private void OperatorToolStrip_MouseDown(object sender, MouseEventArgs e)
        {
            toolStripStatusLabel.Text = "Toolbar Mouse Down";
            Dragging_OperatorToolBar = true;
            DraggedFrom_OperatorToolBar = new Point(e.X, e.Y);
            OperatorToolStrip.Capture = true;
        }

        private void OperatorToolStrip_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripStatusLabel.Text = "Toolbar Moving";
            if (Dragging_OperatorToolBar)
            {
                switch (OperatorToolStrip.Dock)
                {
                    case DockStyle.Top:
                        {
                            // Check if the dragging has reached the threshhold.
                            if (DraggedFrom_OperatorToolBar.X < (e.X - 20) || DraggedFrom_OperatorToolBar.Y < (e.Y - 20))
                            {
                                Dragging_OperatorToolBar = false;
                                // Disconnect the toolbar
                                OperatorToolStrip.Dock = DockStyle.None;
                                OperatorToolStrip.Location = new Point(100, BuildArea.Top + 10);
                                OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
                            }
                            break;
                        }
                    case DockStyle.Left:
                        {
                            // Check if the dragging has reached the threshhold.
                            if (DraggedFrom_OperatorToolBar.X < (e.X - 20) || DraggedFrom_OperatorToolBar.Y < (e.Y - 20))
                            {
                                Dragging_OperatorToolBar = false;
                                // Disconnect the toolbar
                                OperatorToolStrip.Dock = DockStyle.None;
                                OperatorToolStrip.Location = new Point(100, BuildArea.Top);
                                OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
                            }
                            break;
                        }
                    case DockStyle.Right:
                        {
                            // Check if the dragging has reached the threshhold.
                            if (DraggedFrom_OperatorToolBar.X < (e.X + 20) || DraggedFrom_OperatorToolBar.Y < (e.Y - 20))
                            {
                                Dragging_OperatorToolBar = false;
                                // Disconnect the toolbar
                                OperatorToolStrip.Dock = DockStyle.None;
                                OperatorToolStrip.Location = new Point(BuildArea.Right - OperatorToolStrip.Width - 10, BuildArea.Top);
                                OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
                            }
                            break;
                        }
                    case DockStyle.Bottom:
                        {
                            // Check if the dragging has reached the threshhold.
                            if (DraggedFrom_OperatorToolBar.X < (e.X - 20) || DraggedFrom_OperatorToolBar.Y < (e.Y + 20))
                            {
                                Dragging_OperatorToolBar = false;
                                // Disconnect the toolbar
                                OperatorToolStrip.Dock = DockStyle.None;
                                OperatorToolStrip.Location = new Point(100, 100);
                                OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
                            }
                            break;
                        }
                    case DockStyle.None:
                        {
                            OperatorToolStrip.Left = e.X + OperatorToolStrip.Left - DraggedFrom_OperatorToolBar.X;
                            OperatorToolStrip.Top = e.Y + OperatorToolStrip.Top - DraggedFrom_OperatorToolBar.Y;
                            if (OperatorToolStrip.LayoutStyle == ToolStripLayoutStyle.HorizontalStackWithOverflow)
                            {
                                if (OperatorToolStrip.Top < 5)
                                {
                                    Dragging_OperatorToolBar = false;
                                    // Re-dock the toolbar
                                    OperatorToolStrip.Dock = DockStyle.Top;
                                    OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
                                }
                                else if (OperatorToolStrip.Bottom > BuildArea.Bottom - 5)
                                {
                                    Dragging_OperatorToolBar = false;
                                    // Re-dock the toolbar
                                    OperatorToolStrip.Dock = DockStyle.Bottom;
                                    OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
                                }
                                else if (OperatorToolStrip.Right > BuildArea.Right - 50)
                                {
                                    Dragging_OperatorToolBar = false;
                                    //Re-dock the toolbar
                                    OperatorToolStrip.Dock = DockStyle.Right;
                                    OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
                                }
                                else if (OperatorToolStrip.Left < 5)
                                {
                                    Dragging_OperatorToolBar = false;
                                    //Re-dock the toolbar
                                    OperatorToolStrip.Dock = DockStyle.Left;
                                    OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
                                }
                            }
                            else
                            {
                                if (OperatorToolStrip.Top < 5)
                                {
                                    Dragging_OperatorToolBar = false;
                                    // Re-dock the toolbar
                                    OperatorToolStrip.Dock = DockStyle.Top;
                                    OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
                                }
                                else if (OperatorToolStrip.Bottom > BuildArea.Bottom + (OperatorToolStrip.Size.Height / 2))
                                {
                                    Dragging_OperatorToolBar = false;
                                    // Re-dock the toolbar
                                    OperatorToolStrip.Dock = DockStyle.Bottom;
                                    OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
                                }
                                else if (OperatorToolStrip.Right > BuildArea.Right - 5)
                                {
                                    Dragging_OperatorToolBar = false;
                                    //Re-dock the toolbar
                                    OperatorToolStrip.Dock = DockStyle.Right;
                                    OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
                                }
                                else if (OperatorToolStrip.Left < 5)
                                {
                                    Dragging_OperatorToolBar = false;
                                    //Re-dock the toolbar
                                    OperatorToolStrip.Dock = DockStyle.Left;
                                    OperatorToolStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
                                }
                            }
                            break;
                        }
                }
            }
        }

        private void OperatorToolStrip_MouseUp(object sender, MouseEventArgs e)
        {
            Dragging_OperatorToolBar = false;
            OperatorToolStrip.Capture = false;
            OperatorToolStrip.BringToFront();
            toolStripStatusLabel.Text = "Toolbar Released";
        }
    }
    
    public class XmlFile
    {
        public string fileName = "";
        public string queryName = "";
        public bool isDirty = false;
        
        public XmlFile()
        {
            fileName = "Untitled";
            isDirty = true;
        }
   
    }
}
