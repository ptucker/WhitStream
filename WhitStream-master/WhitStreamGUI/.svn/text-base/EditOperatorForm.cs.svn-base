using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WhitStreamGUI
{
    public partial class EditOperatorForm : Form
    {
        private string OpName;
        int Qpos;

        public EditOperatorForm()
        {
            InitializeComponent();
        }
        public EditOperatorForm(GraphicOperator go)
        {
            InitializeComponent();
        }
        public EditOperatorForm(int qp, BuilderForm bf)
        {
            this.BuilderParent = bf;
            this.MdiParent = bf; 
            InitializeComponent();
            this.Qpos = qp;
            this.OpName = BuilderParent.OperatorListGraphic[Qpos].Name;           
            Fill_OperatorBox_Properties(BuilderParent.OperatorListGraphic[Qpos]);
            this.Show();
        }
        private void buttonAccept_Click(object sender, EventArgs e)
        {
            GraphicOperator GO = new GraphicOperator(BuilderParent.OperatorListGraphic[Qpos]);
            // Accept Properties
            GO.Predicate = textBoxPredicate.Text;
            switch (GO.Type)
            {
                case "InputStream":
                    {
                        //foreach (Connection c in cm.Connections)
                        //{
                        //    if (c.ToString() == C)
                        //        GO.Value = c.ID;
                        //}
                        //
                        // TODO: uncomment and get the input stream working 
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
            GO.PicBox.Tag = textBoxTag.Text;
            GO.PicBox.Show();
            if (OpName != textBoxName.Text) 
            {
                //
                // TODO: This way of letting the user know that a unique name is required is annoying and a
                // better way can be found to do it.
                //
                if (!BuilderParent.Check_For_Unique_Name(textBoxName.Text)) { textBoxName.Text = "UNIQUE_NAME_REQUIRED!!"; }
                else 
                { 
                    GO.Name = textBoxName.Text;
                    MessageBox.Show(GO.Name);
                    this.Close(); 
                }
            }
            else { this.Close(); }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            // Cancel Properties
            this.Close();
        }

        public void Fill_OperatorBox_Properties(GraphicOperator go)
        {
            textBoxTag.Text = go.PicBox.Tag.ToString();
            pictureBoxOpProperties.Image = go.PicBox.Image;
            BuilderParent.PropertiesType = go.Type;
            textBoxName.Text = go.PicBoxName;
            textBoxPredicate.Text = go.Predicate;
            groupBox1.Text = "Name:";
            groupBox2.Text = "Predicate:";
            groupBox3.Text = "Attribute CheckList:";
            groupBox4.Text = "Attribute List:";
            groupBox5.Text = "GroupBy Types:";
            groupBox2.Hide();
            groupBox3.Hide();
            groupBox4.Hide();
            groupBox5.Hide();
            switch (go.Type)
            {
                case "Select": //Name, Predicate
                    {
                        label1.Text = "Select Operator";
                        groupBox2.Show();
                        break;
                    }
                case "Project": //Name, Checklist
                    {
                        groupBox3.Show();
                        groupBox3.Text = "Project Attributes";
                        //
                        // TODO: display meaningful attributes here 
                        //
                        for (int i = 0; i < 5; i++)
                            checkedListBox1.Items.Add(String.Format("Attribute {0}", i));
                        break;
                    }
                case "DupElim": //Name, Checklist
                    {
                        groupBox3.Show();
                        groupBox3.Text = "DupElim Attributes";
                        //
                        // TODO: display meaningful attributes here 
                        //
                        for (int i = 0; i < 5; i++)
                            checkedListBox1.Items.Add(String.Format("Attribute {0}", i));
                        break;
                    }
                case "Sort": // Name, List
                    {
                        groupBox4.Show();
                        label1.Text = "Sort Operator";
                        groupBox4.Text = "Sort Attributes";
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
                        groupBox4.Show();
                        label1.Text = "Input Stream Operator";
                        groupBox4.Text = "Ports";
                        listBox1.BeginUpdate();
                        //for (int i = 0; i < cm.Connections.Count; i++)
                        //{
                        //    SetPropertiesListBox.Items.Add(cm.Connections[i]);
                        //}
                        //
                        // TODO: display meaningful attributes here 
                        //
                        listBox1.EndUpdate();
                        break;
                    }
                case "Join": // Name, Predicate, JoinList
                    {
                        groupBox2.Show();
                        groupBox3.Show();
                        label1.Text = "Join Operator";
                        groupBox3.Text = "Join Attributes:";
                        listBox1.BeginUpdate();
                        //
                        // TODO: display meaningful attributes here 
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
                        label1.Text = "Intersect Operator";
                        break;
                    }
                case "Difference": // Name, Note
                    {
                        label1.Text = "Difference Operator";
                        break;
                    }
                case "Union": // Name, Note
                    {
                        label1.Text = "Union Operator";
                        break;
                    }
                case "GroupBy":
                    {
                        groupBox3.Show();
                        groupBox4.Show();
                        groupBox5.Show();
                        System.Collections.Generic.List<string> GroupByListBoxItems = new List<string>();
                        GroupByListBoxItems.Add("GroupByCount");
                        GroupByListBoxItems.Add("GroupByAvg");
                        GroupByListBoxItems.Add("GroupBySum");
                        GroupByListBoxItems.Add("GroupByMax");
                        GroupByListBoxItems.Add("GroupByMin");
                        //
                        // TODO: display meaningful attributes here 
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
                        label1.Text = "Output Stream Operator";
                        break;
                    }
                case default(string):
                    {
                        MessageBox.Show("The operator name does not match any of the options");
                        break;
                    }
            }
        }
    }
}