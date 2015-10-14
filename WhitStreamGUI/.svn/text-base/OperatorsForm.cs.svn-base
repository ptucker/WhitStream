using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WhitStreamGUI
{
    public partial class OperatorsForm : Form
    {
        public OperatorsForm()
        {
            InitializeComponent();
        }
        
        public OperatorsForm(string OpName, BuilderForm bf)
        {
            this.BuilderParent = bf;
            this.MdiParent = bf;            
            InitializeComponent();
            Fill_OperatorBox_Properties(OpName);
            this.Show();
            this.BringToFront();
        }
        
        private void buttonAccept_Click(object sender, EventArgs e)
        {
            // Accept Properties
            if(BuilderParent.PropertiesType == "GroupBy" && (string)domainUpDownGroupBy.SelectedItem == null)
            {
                textBoxName.Text = "GROUPBY_TYPE_REQUIRED!!";
                //GroupByTypeNeeded.Visible = true;
            }
            else if (BuilderParent.Check_For_Unique_Name(textBoxName.Text))
            {
                GraphicOperator GO = new GraphicOperator(BuilderParent.PropertiesType, BuilderParent.PicBoxMaker(new Point(50,50),new Size(50,50),null,"Unassigned Tag"), textBoxName.Text);
                GO.PicBox.Parent = BuilderParent.BuildArea;
                GO.PicBoxName = textBoxName.Text;
                GO.PicBox.Size = BuilderParent.IconSize;
                GO.PicBox.Image = pictureBoxOpProperties.Image;
                // Predicate changes
                if (GO.has_Predicate())
                {
                    GO.Predicate = textBoxPredicate.Text;
                }
                switch (GO.Type)
                {
                    case "InputStream":
                        {
                            //foreach (Connection c in cm.Connections)
                            //{
                            //    if (c.ToString() == C)
                            //        OperatorListGraphic[QueuePos].Value = c.ID;
                            //}
                            //
                            // TODO: uncomment and get to work with input stream
                            //
                            break;
                        }
                    case "GroupBy":
                        {
                            GO.SecondaryType = (string)domainUpDownGroupBy.SelectedItem;
                            switch (GO.SecondaryType)
                            {
                                case "GroupByCount":
                                    {
                                        GO.PicBox.Image = BuilderParent.ImageList.Images[4];
                                        break;
                                    }
                                case "GroupByMax":
                                    {
                                        GO.PicBox.Image = BuilderParent.ImageList.Images[5];
                                        break;
                                    }
                                case "GroupByMin":
                                    {
                                        GO.PicBox.Image = BuilderParent.ImageList.Images[6];
                                        break;
                                    }
                                case "GroupBySum":
                                    {
                                        GO.PicBox.Image = BuilderParent.ImageList.Images[7];
                                        break;
                                    }
                                case "GroupByAvg":
                                    {
                                        GO.PicBox.Image = BuilderParent.ImageList.Images[3];
                                        break;
                                    }
                                default:
                                    {
                                        GO.PicBox.Image = BuilderParent.ImageList.Images[2];
                                        break;
                                    }
                            }
                            break;
                        }
                }
                if (textBoxTag.Text != "Enter Tag Here")
                    GO.PicBox.Tag = textBoxTag.Text;
                else
                    GO.PicBox.Tag = String.Format("{0}", GO.PicBoxName);
                GO.PicBox.Show();
                BuilderParent.Search_For_Open_Area_BuildArea(GO.PicBox);
                BuilderParent.OperatorListGraphic.Add(GO);
                BuilderParent.BuildArea.Controls.Add(GO.PicBox);
                addOpQuery();
                this.Close();
            }
            else
            {
                UniqueNameRequiredLabel.Visible = true;
                textBoxName.Text = "UNIQUE_NAME_REQUIRED!!";
                //
                // TODO: this is an annoying way to let the user know that a unique name is required
                // find a better way to do it
                //
            }
        }

        private void addOpQuery()
        {
            
            switch (BuilderParent.PropertiesType)
            { 
                case "Select":
                    MessageBox.Show("Adding Select Query");
                    WhitStream.QueryEngine.Query opIn = new WhitStream.QueryEngine.QueryOperators.OpGenerate(100);
                    BuilderParent.userQuery = new WhitStream.QueryEngine.QueryOperators.OpSelect(textBoxPredicate.ToString(),opIn);
                    MessageBox.Show(BuilderParent.userQuery.ToString());
                    break;
                case "Project":
                    MessageBox.Show("Adding Project Query");
                    break;
                case "DupElim":
                    MessageBox.Show("Adding DuplicateElimination Query");
                    break;
                case "GroupBy":
                    MessageBox.Show("Adding GroupBy Query");
                    break;
                case "Sort":
                    MessageBox.Show("Adding Sort Query");
                    break;
                case "Join":
                    MessageBox.Show("Adding Join Query");
                    break;
                case "Intersect":
                    MessageBox.Show("Adding Intersect Query");
                    break;
                case "Union":
                    MessageBox.Show("Adding Union Query");
                    break;
                case "Difference":
                    MessageBox.Show("Adding Difference Query");
                    break;
                case "InputStream":
                    MessageBox.Show("Adding Instream Query");
                    break;
                case "OutputStream":
                    MessageBox.Show("Adding Outstream Query");
                    break;
            }

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            // Cancel Properties
            this.Close();
        }

        public void Fill_OperatorBox_Properties(string type)
        {
            BuilderParent.PropertiesType = type;
            textBoxName.Text = BuilderParent.Search_For_Unique_Name(type, 1);
            textBoxPredicate.Text = "Enter Predicate Here";
            groupBoxName.Text = "Name:";
            groupBoxPredicate.Text = "Predicate:";
            groupBoxAttributesList1.Text = "Attribute CheckList:";
            groupBoxAttributesList2.Text = "Attribute List:";
            groupBoxGroupByTypes.Text = "GroupBy Types:";
            groupBoxPredicate.Hide();
            groupBoxAttributesList1.Hide();
            groupBoxAttributesList2.Hide();
            groupBoxGroupByTypes.Hide();
            switch(type)
            {
                case "Select": //Name, Predicate
                    {
                        pictureBoxOpProperties.Image = this.BuilderParent.ImageList.Images[13];
                        label1.Text = "Select Operator";
                        groupBoxPredicate.Show();
                        break;
                    }
                case "Project": //Name, Checklist
                    {
                        groupBoxAttributesList1.Show();
                        groupBoxAttributesList1.Text = String.Format("{0} Attributes", type);
                        pictureBoxOpProperties.Image = BuilderParent.ImageList.Images[12];
                        //
                        // TODO: actually display meaningful attributes 
                        //
                        for (int i = 0; i < 5; i++)
                            checkedListBox1.Items.Add(String.Format("Attribute {0}", i));
                        break;
                    }
                case "DupElim": //Name, Checklist
                    {
                        groupBoxAttributesList1.Show();
                        groupBoxAttributesList1.Text = String.Format("{0} Attributes", type);
                        pictureBoxOpProperties.Image = this.BuilderParent.ImageList.Images[1];
                        //
                        // TODO: display meaningful attributes here 
                        //
                        for (int i = 0; i < 5; i++)
                            checkedListBox1.Items.Add(String.Format("Attribute {0}", i));
                        break;
                    }
                case "Sort": // Name, List
                    {
                        groupBoxAttributesList2.Show();
                        label1.Text = "Sort Operator";
                        pictureBoxOpProperties.Image = this.BuilderParent.ImageList.Images[14];
                        groupBoxAttributesList2.Text = String.Format("Sort Attributes");
                        listBox1.BeginUpdate();
                        //
                        // TODO: display meaningful attributes here 
                        //
                        for (int i = 0; i < 5; i++)
                        {
                            listBox1.Items.Add(String.Format("Attribute {0}", i));
                        }
                        listBox1.EndUpdate();
                        break;
                    }
                case "InputStream": // Name, List
                    {
                        groupBoxAttributesList2.Show();
                        label1.Text = "Input Stream Operator";
                        pictureBoxOpProperties.Image = this.BuilderParent.ImageList.Images[8];
                        groupBoxAttributesList2.Text = String.Format("Ports");
                        listBox1.BeginUpdate();
                        //for (int i = 0; i < cm.Connections.Count; i++)
                        //{
                        //    SetPropertiesListBox.Items.Add(cm.Connections[i]);
                        //}
                        //
                        // TODO: uncomment and confirm that it works 
                        //
                        listBox1.EndUpdate();
                        break;
                    }
                case "Join": // Name, Predicate, JoinList
                    {
                        groupBoxPredicate.Show();
                        groupBoxAttributesList1.Show();
                        label1.Text = "Join Operator";
                        pictureBoxOpProperties.Image = this.BuilderParent.ImageList.Images[10];
                        groupBoxAttributesList1.Text = "Join Attributes:";
                        listBox1.BeginUpdate();
                        //
                        // TODO: display meaningful attributes 
                        //
                        for (int i = 0; i < 5; i++)
                        {
                            listBox1.Items.Add(String.Format("Join Attribute {0}", i));
                        }
                        listBox1.EndUpdate();
                        break;
                    }
                case "Intersect": // Name, Note
                    {
                        label1.Text = String.Format("{0} Operator", type);
                        pictureBoxOpProperties.Image = this.BuilderParent.ImageList.Images[9];
                        break;
                    }
                case "Difference": // Name, Note
                    {
                        label1.Text = String.Format("{0} Operator", type);
                        pictureBoxOpProperties.Image = this.BuilderParent.ImageList.Images[0];
                        break;
                    }
                case "Union": // Name, Note
                    {
                        label1.Text = String.Format("{0} Operator", type);
                        pictureBoxOpProperties.Image = this.BuilderParent.ImageList.Images[16];
                        break;
                    }
                case "GroupBy":
                    {
                        groupBoxAttributesList1.Show();
                        groupBoxAttributesList2.Show();
                        groupBoxGroupByTypes.Show();
                        pictureBoxOpProperties.Image = this.BuilderParent.ImageList.Images[2];
                        System.Collections.Generic.List<string> GroupByListBoxItems = new List<string>();
                        GroupByListBoxItems.Add("GroupByCount");
                        GroupByListBoxItems.Add("GroupByAvg");
                        GroupByListBoxItems.Add("GroupBySum");
                        GroupByListBoxItems.Add("GroupByMax");
                        GroupByListBoxItems.Add("GroupByMin");
                        //
                        // TODO: display meaningful results below 
                        //
                        for (int i = 0; i < GroupByListBoxItems.Count; i++)
                        {
                            domainUpDownGroupBy.Items.Add(GroupByListBoxItems[i]);
                        }
                        for (int i = 0; i < 5; i++)
                        {
                            checkedListBox1.Items.Add(String.Format("Attribute {0}", i));
                        }
                        for (int i = 0; i < 5; i++)
                        {
                            listBox1.Items.Add(String.Format("Attribute {0}", i));
                        }
                        break;
                    }
                case "OutputStream":
                    {
                        label1.Text = String.Format("Output Stream Operator");
                        pictureBoxOpProperties.Image = this.BuilderParent.ImageList.Images[11];
                        break;
                    }
                case default(string):
                    {
                        MessageBox.Show("The operator name does not match any of the options");
                        break;
                    }
            }            
        }

        private void OperatorsForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            //
            // TODO: make the hotkey presses do something helpful
            //
            textBoxTag.Text = (String.Format("Key pressed: {0}", e.KeyChar));            
            if (e.KeyChar == 'f')
            {
                
            }
        }
    }
}