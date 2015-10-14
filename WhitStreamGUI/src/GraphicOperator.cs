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
    public class GraphicOperator
    {
        //
        // Variables
        //
        PictureBox OperatorPicBox;
        string OpType, Op2ndType;
        string OpName, OpPredicate;
        int oldX, oldY, Qposition;
        GraphicOperator Input1, Input2;
        int[] attrs = new int[1];
        int value;
        //
        // Constructors
        //
        public GraphicOperator()
        {
            Input1 = null;
            Input2 = null;
            OperatorPicBox = null;
            oldX = 0;
            oldY = 0;
            OpType = "None";
            Op2ndType = "None";
            OpName = "No Name";
            OpPredicate = "No Predicate";
        }

        public GraphicOperator(GraphicOperator go)
        {
            OpType = go.OpType;
            OperatorPicBox = go.OperatorPicBox;
            OpName = go.OpName;
            Input1 = null;
            Input2 = null;
            OperatorPicBox.BorderStyle = BorderStyle.FixedSingle;
            OperatorPicBox.SizeMode = PictureBoxSizeMode.StretchImage;
        }
        
        public GraphicOperator(string optype, PictureBox pb, string name, int PosInQ)
        {
            OpType = optype;
            OperatorPicBox = pb;
            OpName = name;
            Input1 = null;
            Input2 = null;
            Qposition = PosInQ;
            Op2ndType = null;
            OperatorPicBox.BorderStyle = BorderStyle.FixedSingle;
            OperatorPicBox.SizeMode = PictureBoxSizeMode.StretchImage;
        }
        
        public GraphicOperator(string optype, PictureBox pb, string name)
        {
            OpType = optype;
            OperatorPicBox = pb;
            OpName = name;
            Input1 = null;
            Input2 = null;
            Op2ndType = null;
            OperatorPicBox.BorderStyle = BorderStyle.FixedSingle;
            OperatorPicBox.SizeMode = PictureBoxSizeMode.StretchImage;
        }
        //
        // Methods
        //
        public int OldX { get { return oldX; } set { oldX = value; } }
        
        public int OldY { get { return oldY; } set { oldY = value; } }
        
        public PictureBox PicBox { get { return OperatorPicBox; } set { OperatorPicBox = value; } }
        
        public GraphicOperator Input_1
        {
            get { return Input1; }
            set
            {
                if (OpType == "InputStream")
                    Input1 = null;
                else
                    Input1 = value;
            }
        }
        
        public GraphicOperator Input_2
        {
            get { return Input2; }
            set
            {
                if (OpType == "InputStream" || OpType == "OutputStream" || OpType == "Select" || OpType == "Project" || OpType == "DupElim" || OpType == "GroupBy" || OpType == "Sort")
                    Input2 = null;
                else
                    Input2 = value;
            }
        }
        
        public string Name { get { return OpName; } set { OpName = value; } }
        
        public string Type { get { return OpType; } set { OpType = value; } }
        
        public string SecondaryType { get { return Op2ndType; } set { Op2ndType = value; } }
        
        public string Predicate { get { return OpPredicate; } set { OpPredicate = value; } }
        
        public string PicBoxName { get { return OperatorPicBox.Name; } set { OperatorPicBox.Name = value; } }
        
        public int PositionInQueue { get { return Qposition; } set { Qposition = value; } }
        
        public bool has_Input1()
        {
            if (Input1 != null) { return true; }
            else { return false; }
        }
        
        public bool has_Input2()
        {
            if (Input2 != null) { return true; }
            else { return false; }
        }
        
        public void set_XY(int X, int Y)
        {
            if (OperatorPicBox != null)
            {
                OperatorPicBox.Top = OperatorPicBox.Top + (Y - oldY);
                OperatorPicBox.Left = OperatorPicBox.Left + (X - oldX);
                if (OperatorPicBox.Top > 650)
                    OperatorPicBox.Top = 650;
                else if (OperatorPicBox.Top < 10)
                    OperatorPicBox.Top = 10;
                if (OperatorPicBox.Left > 800)
                    OperatorPicBox.Left = 800;
                else if (OperatorPicBox.Left < 10)
                    OperatorPicBox.Left = 10;
            }
        }
        
        public bool has_Predicate()
        {
            if (Type == "Select" || Type == "Join") { return true; }
            else { return false; }
        }
        
        public int[] Attrs { get { return attrs; } set { attrs = value; } }
        
        public int Value { get { return this.value; } set { this.value = value; } }
        
        public bool isUnaryOp()
        {
            if (Type == "Join" || Type == "Intersect" || Type == "Union" || Type == "Difference")
                return false;
            else
                return true;
        }
        
        public bool isBinaryOp()
        {
            if (Type == "Join" || Type == "Intersect" || Type == "Union" || Type == "Difference")
                return true;
            else
                return false;
        }
    }
}
