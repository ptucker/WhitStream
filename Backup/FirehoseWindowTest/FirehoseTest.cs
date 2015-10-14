using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WhitStream.Data;
using WhitStream.Expression;
using WhitStream.QueryEngine;
using WhitStream.QueryEngine.Scheduler;
using WhitStream.QueryEngine.QueryOperators;
using WhitStream.Utility;
using System.Diagnostics;

namespace FirehoseWindowTest
{
    public partial class FirehoseTest : Form
    {
        private static List<int> AvaPorts = new List<int>(10);
        private static int current_port = 0;
        private static DataItemPool dipGlobal = new DataItemPool();

        public FirehoseTest()
        {
            InitializeComponent();
        }

        public void btnStartDemo_Click(object sender, System.EventArgs e)
        {
            btnStartDemo.Enabled = false;
            //MessageBox.Show("Here");
            //int exec = 4, firehoses = 2;
            //for (int i = 4001; i <= 4010; i++)
            //{
            //    AvaPorts.Add(i);
            //}
            //MessageBox.Show("Ports");

            dipGlobal.Init(10, true);
            MessageBox.Show(Environment.CurrentDirectory);
            //double sum = 0;
            //for (int x = 0; x < firehoses; x++)
            Process p = new Process();
            p.StartInfo.FileName = Environment.CurrentDirectory + "\\Firehose.exe";//, Keys.Enter.ToString());
            p.StartInfo.Arguments = "localhost 0 4";
            MessageBox.Show(p.StartInfo.FileName);
            
            //for (int i = 0; i < 4; i++)
            //{
            //    DateTime dt = DateTime.Now;
                ShowResults(TestQuery());
            //    TimeSpan tm = DateTime.Now - dt;
            //    sum += tm.TotalSeconds;
            //}
        }

        #region Comparing/Converting
        /// <summary>
        /// Function to get only the first attribute values for DataItem objects
        /// </summary>
        static DataItem Snd(DataItem di)
        {
            //Invoke the right constructor, based on the type of di (DataItem or Punctuation)
            DataItem diRet;
            if (di is Punctuation)
                diRet = new Punctuation(1);
            else
                diRet = dipGlobal.GetItem();
            diRet.AddValue(di[1]);
            return diRet;
        }

        /// <summary>
        /// Function to convert the timestamp to minutes
        /// </summary>
        static DataItem Sec_to_Min(DataItem di)
        {
            if (di is Punctuation)
            {
                ((Punctuation.LiteralPattern)di[0]).Value = (((UInt32)((Punctuation.LiteralPattern)di[0]).Value) / 60);
            }
            else
            {
                di[0] = ((uint)di[0]) / 60;
            }
            return di;
        }


        /// <summary>
        /// Comparison function for sorting two DataItem objects on attribute 2
        /// </summary>
        /// <seealso cref="WhitStream.Data.DataItem" />
        public class Comp : IComparer<DataItem>
        {
            /// <summary>
            /// Compare the values of attribute in both DataItem objects for equality
            /// </summary>
            /// <param name="o1"></param>
            /// <param name="o2"></param>
            /// <returns></returns>
            public int Compare(DataItem o1, DataItem o2)
            {
                DataItem di1 = o1 as DataItem;
                DataItem di2 = o2 as DataItem;

                object a1 = di1[0];
                object a2 = di2[0];
                return -1 * ((IComparable)a1).CompareTo(a2);
            }
        }
        #endregion

        #region Queries
        private static Query TestQuery()
        {
            /*
            Query q0 = new OpGenerate(15);
            Query q1 = new OpDupElim(new OpProject(new UnaryOp.Map(Snd), new int[] {1}, 
                                            new OpSelect("$1.2 < 50.0",
                                                      new OpSelect("$1.1 > 4",
                                                                new OpGenerate(100)))));
            Query q2 = new OpUnion(new OpGenerate(100, 0), new OpGenerate(40, 0));
            Query q3 = new OpIntersect(new OpGenerate(15), new OpGenerate(12));
            Query q4 = new OpDifference(new OpGenerate(15, 0), new OpGenerate(12, 0));
            Query q5 = new OpGroupByCount(null, new OpGenerate(50));
            Query q6 = new OpJoinSymm("$1.1 = $2.2", new OpGenerate(40, 0), new OpGenerate(35, 1));
            Query q7 = new OpSort(new Comp(), new int[] {0}, q0);
            Query q8 = new OpGroupByCount(new int[] {0}, new OpGenerate(100));
            Query q9 = new OpSelect("$1.1 > 10 OR $1.1 < 5", new OpGenerate(50));
            Query q10 = new OpJoinSymm("$1.1 = $2.2", new OpProject(new UnaryOp.Map(Snd), new int[] { 1 },
                         new OpUnion(new OpSelect("$1.2 < 50.0",
                                      new OpSelect("$1.1 > 4", new OpGenerate(100000))),
                                   new OpGenerate(150000))),
                         new OpGenerate(150000));
            Query q11 = new OpGenerateListener("10.28.29.2", "4000"));
            Query qTest = new OpJoinSymm("$1.1 = $2.2", new OpProject(new UnaryOp.Map(Snd), new int[] { 1 },
                         new OpUnion(new OpSelect("$1.2 < 50.0",
                                      new OpSelect("$1.1 > 4", new OpGenerate(1000000))),
                                   new OpGenerate(1500000))),
                         new OpGenerate(1500000)); 
             */
            //return new OpJoinMerge("$1.1 = $2.2", new OpGenerate(100000, 0), new OpGenerate(100000, 1), 0, 1);
            //return new OpJoinSymm("$1.1 = $2.2", new OpGenerate(100000, 0), new OpGenerate(100000, 1));
            //return new OpGenerate(1, 0);

            //return new OpJoinSymm("$1.1 = $2.2", new OpProject(new UnaryOp.Map(Snd), new int[] { 1 },
            //                new OpUnion(new OpSelect("$1.2 < 50",
            //                                new OpSelect("$1.1 > 4", new OpGenerate(15000000, -1))),
            //                            new OpGenerate(20000000, -1))),
            //                new OpGenerate(20000000, -1));

            //int iPunct = -1;
            //int iRow1 = 53000;
            //int iRow2 = 80000;
            //return new OpGenerate(100);
            //return new OpServer();
            //return new OpGenerate(20000000, 1);
            //return new OpUnion(new OpGenerate(100), new OpGenerate(100));
            //return new OpUnion( new OpServer(), new OpServer());
            return new OpIntersect(new OpGenerate(100), new OpGenerate(200));
            //return new OpJoin("$1.1 = $2.2", new OpProject(new UnaryOp.Map(Snd), new int[] { 1 },
            //                new OpNUnion(new OpSelect("$1.2 < 50",
            //                                new OpSelect("$1.1 > 4", new OpGenerate(iRow2, iPunct))),
            //                            new OpGenerate(iRow1, iPunct))),
            //                new OpNUnion(new OpGenerate(iRow1, iPunct), new OpGenerate(iRow2, iPunct), new OpGenerate(iRow1, iPunct), new OpGenerate(iRow2, iPunct)));

            //return new OpSelect("$1.1 = 0", new OpGenerate(20, 2));
            //return new OpGenerate(50);

            //return new OpJoinSymmRangePunct("$1.1 = $2.2", 0, 1, new OpProject(new UnaryOp.Map(Snd), new int[] { 1 },
            //                new OpNUnion(new OpSelect("$1.2 < 50",
            //                                new OpSelect("$1.1 > 4", new OpGenerate(150000, 2))),
            //                            new OpGenerate(200000, 2))),
            //                new OpUnion(new OpUnion(new OpGenerate(200000, 2), new OpGenerate(150000, 2)), new OpUnion(new OpGenerate(200000, 2), new OpGenerate(150000, 2))));
        }

        //private static Query TestQueryCons()
        //{
        //    //private static ConnectionManager cm = new ConnectionManager( "10.28.29.2", "4000" );
        //    ConnectionManager cm = new ConnectionManager("4000");
        //    Connection c = null;
        //    while (c == null)
        //    {
        //        c = cm.LocateCon(GetPort(), "WhitDevice");
        //        Thread.Sleep(1);
        //    }
        //    c.StartThreads();
        //    current_port++;

        //    //return c;

        //    //return new OpSelect("$1.16 = s139", c);

        //    return new OpGroupByCount(new int[] { 0 }, new OpProject(new UnaryOp.Map(Sec_to_Min), new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, c));
        //}
        #endregion

        #region Show Results
        private void ShowResults(Query q)
        {
            List<DataItem> rgdi;
            bool eof = false;
            int cRow = 0;

            while (eof == false)
            {
                
                int i = 0;
                rgdi = q.Iterate(dipGlobal.GetItem, dipGlobal.ReleaseItem);
                foreach (DataItem di in rgdi)
                {
                    if (di.EOF == false)
                    {
                        //txtResults.Text += (di.ToString() + "\n");
                        //DGResults.Rows.Add(cRow.ToString());
                        //txtResults.Text = txtResults + (cRow.ToString() + "\n");
                        //Log.WriteMessage(di.ToString(), Log.eMessageType.Debug);
                        cRow++;
                    }
                    eof |= di.EOF;
                    DGResults.Rows.Add();
                    DGResults.Rows[i].Cells["data"].Value = di.GetValue(0);
                    DGResults.Rows[i].Cells["data2"].Value = di.GetValue(1);
                    //txtResults.AppendText(di.GetValue(0) + "\t" + di.GetValue(1) + "\n");
                    di.Dispose();
                    i++;
                }
            }
            try
            {
                //DGResults.Rows.Add(cRow.ToString());
                txtResults.AppendText(cRow.ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString() + "\n" + e.InnerException.ToString());
            }
            MessageBox.Show("Total Rows = " + cRow);
            //Log.WriteMessage(string.Format("Total Rows: {0}", cRow), Log.eMessageType.Debug);
        }
        #endregion

        #region Ports
        private static int GetPort()
        {
            if (current_port == 10)
                current_port = 0;
            int port = AvaPorts[current_port];
            return port;
        }
        #endregion

        private void lbResultPanel_Click(object sender, EventArgs e)
        {

        }

    }
}
