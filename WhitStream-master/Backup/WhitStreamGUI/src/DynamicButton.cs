using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;

namespace WhitStreamGUI
{
    public class DynamicButton
    {
        //
        // Variables
        //
        Button button;
        PictureBox picBox;
        string type;
        BuilderForm builderParent;
        GraphicOperator AssignedTo, ComingFrom;
        //
        // Constructors
        //
        public DynamicButton()
        {
            button = new Button();
            picBox = new PictureBox();
        }

        public DynamicButton(PictureBox pb, string t, BuilderForm parent, GraphicOperator assignedTo, GraphicOperator comingFrom)
        {
            button = ButtonMaker(pb, t);
            picBox = pb;
            type = t;
            builderParent = parent;
            button.Parent = picBox;
            AssignedTo = assignedTo;
            ComingFrom = comingFrom;
        }
        //
        // Methods
        //
        public Button Button { get { return button; } set { button = value; } }

        public PictureBox PicBox { get { return picBox; } set { picBox = value; } }

        public BuilderForm BuilderParent { get { return builderParent; } set { builderParent = value; } }

        public string Type { get { return type; } set { type = value; } }

        private Button ButtonMaker(PictureBox pb, string type)
        {
            Button b = new Button();
            b.Name = Search_For_Unique_Name("DynamicButton", 1);
            switch (type)
            {
                case "Connect Input 1 Unary":
                    {
                        b.Size = new Size(15, 15);
                        b.Location = new Point(pb.Left - (b.Size.Width / 2), ((pb.Top + pb.Bottom) / 2) - (b.Size.Height / 2));
                        b.Text = "C1";
                        b.Parent = pb;
                        b.Anchor = AnchorStyles.Left;
                        b.MouseClick += this.DynamicButton_MouseClick_ConnectUnaryInput1;
                        b.BackColor = Color.Green;
                        b.FlatStyle = FlatStyle.Flat;
                        break;
                    }
                case "Connect Input 1 Binary":
                    {
                        b.Size = new Size(15, 15);
                        b.Location = new Point(pb.Left - (b.Size.Width / 2), pb.Top);
                        b.Text = "C1";
                        b.Parent = pb;
                        b.Anchor = AnchorStyles.Left;
                        b.MouseClick += this.DynamicButton_MouseClick_ConnectBinaryInput1;
                        b.BackColor = Color.Green;
                        b.FlatStyle = FlatStyle.Flat;
                        break;
                    }
                case "Connect Input 2 Binary":
                    {
                        b.Size = new Size(15, 15);
                        b.Location = new Point(pb.Left - (b.Size.Width / 2), pb.Bottom - b.Size.Height);
                        b.Text = "C2";
                        b.Parent = pb;
                        b.Anchor = AnchorStyles.Left;
                        b.MouseClick += this.DynamicButton_MouseClick_ConnectBinaryInput2;
                        b.BackColor = Color.Green;
                        b.FlatStyle = FlatStyle.Flat;
                        break;
                    }
                case "Disconnect Input 1 Unary":
                    {
                        b.Size = new Size(15, 15);
                        b.Location = new Point(pb.Left - (b.Size.Width / 2), ((pb.Top + pb.Bottom) / 2)-(b.Size.Height/2));
                        b.Text = "D1";
                        b.Parent = pb;
                        b.Anchor = AnchorStyles.Left;
                        b.MouseClick += this.DynamicButton_MouseClick_DisconnectUnaryInput1;
                        b.BackColor = Color.Red;
                        b.FlatStyle = FlatStyle.Flat;
                        break;
                    }
                case "Disconnect Input 1 Binary":
                    {
                        b.Size = new Size(15, 15);
                        b.Location = new Point(pb.Left - (b.Size.Width / 2), pb.Top);
                        b.Text = "D1";
                        b.Parent = pb;
                        b.Anchor = AnchorStyles.Left;
                        b.MouseClick += this.DynamicButton_MouseClick_DisconnectBinaryInput1;
                        b.BackColor = Color.Red;
                        b.FlatStyle = FlatStyle.Flat;
                        break;
                    }
                case "Disconnect Input 2 Binary":
                    {
                        b.Size = new Size(15, 15);
                        b.Location = new Point(pb.Left - (b.Size.Width / 2), pb.Bottom - (b.Size.Height));
                        b.Text = "D2";
                        b.Parent = pb;
                        b.Anchor = AnchorStyles.Left;
                        b.MouseClick += this.DynamicButton_MouseClick_DisconnectBinaryInput2;
                        b.BackColor = Color.Red;
                        b.FlatStyle = FlatStyle.Flat;
                        break;
                    }
                case "Add Query":
                    {
                        b.Size = new Size(15, 15);
                        b.Location = new Point(pb.Right - (b.Size.Width / 2), pb.Top);
                        b.Text = "A";
                        b.Parent = pb;
                        b.Anchor = AnchorStyles.Right;
                        b.MouseClick += this.DynamicButton_MouseClick_Add;
                        b.BackColor = Color.Yellow;
                        b.FlatStyle = FlatStyle.Flat;
                        break;
                    }
                case "Test Query":
                    {
                        b.Size = new Size(15, 15);
                        b.Location = new Point(pb.Right - (b.Size.Width / 2), pb.Bottom - (b.Size.Height));
                        b.Text = "T";
                        b.Parent = pb;
                        b.Anchor = AnchorStyles.Right;
                        b.MouseClick += this.DynamicButton_MouseClick_Test;
                        b.BackColor = Color.Blue;
                        b.FlatStyle = FlatStyle.Flat;
                        break;
                    }
                case default(string):
                    {
                        b.Size = new Size(20, 20);
                        b.Location = new Point((pb.Left + pb.Right) / 2, ((pb.Top + pb.Bottom) / 2));
                        b.Text = "Error";
                        b.Parent = pb;
                        b.Anchor = AnchorStyles.Left;
                        b.BackColor = Color.Orange;
                        b.FlatStyle = FlatStyle.Flat;
                        break;
                    }
            }
            return b;
        }

        private void DynamicButton_MouseClick_ConnectUnaryInput1(object sender, MouseEventArgs e)
        {
            AssignedTo.Input_1 = ComingFrom;
            BuilderParent.BuildArea.Invalidate();
        }

        private void DynamicButton_MouseClick_ConnectBinaryInput1(object sender, MouseEventArgs e)
        {
            AssignedTo.Input_1 = ComingFrom;
            BuilderParent.BuildArea.Invalidate();
        }

        private void DynamicButton_MouseClick_ConnectBinaryInput2(object sender, MouseEventArgs e)
        {
            AssignedTo.Input_2 = ComingFrom;
            BuilderParent.BuildArea.Invalidate();
        }

        private void DynamicButton_MouseClick_DisconnectUnaryInput1(object sender, MouseEventArgs e)
        {
            AssignedTo.Input_1 = null;
            BuilderParent.BuildArea.Invalidate();
        }

        private void DynamicButton_MouseClick_DisconnectBinaryInput1(object sender, MouseEventArgs e)
        {
            AssignedTo.Input_1 = null;
            BuilderParent.BuildArea.Invalidate();
        }

        private void DynamicButton_MouseClick_DisconnectBinaryInput2(object sender, MouseEventArgs e)
        {
            AssignedTo.Input_2 = null;
            BuilderParent.BuildArea.Invalidate();
        }

        private void DynamicButton_MouseClick_Add(object sender, MouseEventArgs e)
        {
            BuilderParent.AddQuery();
        }

        private void DynamicButton_MouseClick_Test(object sender, MouseEventArgs e)
        {
            BuilderParent.TestQuery();
        }
        
        private string Search_For_Unique_Name(string type, int num)
        {
            string name = String.Format("{0} {1}", type, num);
            bool again = false;
            if (BuilderParent != null)
                if (BuilderParent.DynamicButtonList != null)
                    foreach (DynamicButton db in BuilderParent.DynamicButtonList)
                        if (db.Button.Name == name)
                        {
                            name = String.Format("{0} {1}", type, num++);
                            again = true;
                            break;
                        }
            if (again) { name = Search_For_Unique_Name(type, num); }
            return name;
        }
    }
}
