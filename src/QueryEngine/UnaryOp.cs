/*
 * Created by SharpDevelop.
 * User: Pete
 * Date: 12/29/2005
 * Time: 12:20 PM
 * 
 * Test update
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Text;
using WhitStream.Data;
using WhitStream.Expression;
using WhitStream.Utility;
using WhitStream.Database;

namespace WhitStream.QueryEngine.QueryOperators
{
	#region Generate
	/// <summary>
    /// Class to generate data
    /// </summary>
    /// <remarks>This class does not take an input query operator -- It generates data</remarks>
    /// <remarks>This class must override Iterate(), since it has no source</remarks>
    public class OpGenerate : UnaryOp, WhitStream.QueryEngine.IStreamDataSource
    {
        private int cRows = 0;
        private int iRow;
        private int iPunct = -1;
        private int cRowsOutput = 0;
        private DateTime tmStart = DateTime.Now;

        /// <summary> XML serialization constants </summary>
        private const string ROWCOUNT = "ROWS";
        private const string PUNCT = "PUNCT";

        /// <summary>
        /// Constructor to determine how much data to generate
        /// </summary>
        /// <param name="c">The number of data items to generate</param>
        public OpGenerate(int c)
            : base()
        {
            cRows = c;
            iRow = 0;
        }

        /// <summary>
        /// Constructor to determine how much data to generate
        /// </summary>
        /// <param name="c">The number of data items to generate</param>
        /// <param name="iP">The attribute to punctuate</param>
        public OpGenerate(int c, int iP) 
            : base()
        {
            cRows = c;
            iRow = 0;

            iPunct = iP;
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - Generate({1})", base.ToString(), cRows);
        }

        /// <summary>
        /// How many rows should this OpGenerate produce?
        /// </summary>
        public int RowCount
        {
            get { return cRows; }
            set { cRows = value; }
        }

        /// <summary>
        /// Constructor to determine how much data to generate
        /// </summary>
        /// <param name="id">operator ID</param>
        /// <param name="rowcount">How many rows from this operator</param>
        /// <param name="punct">What kinds of punctuations to embed</param>
        /// <remarks> To be used by deserialization only </remarks>
        public OpGenerate(string id, string rowcount, string punct) 
            : base()
        {
            opID = Int32.Parse(id.Substring(id.IndexOf('=') + 1));
            cRows = Int32.Parse(rowcount.Substring(rowcount.IndexOf('=') + 1));
            iPunct = Int32.Parse(punct.Substring(punct.IndexOf('=') + 1));

            iRow = 0;
        }

        /// <summary>
        /// Serialize the Generator operator by writing its row count and punctuation style
        /// </summary>
        /// <param name="tw"> The destination for writing </param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);

            tw.Write(string.Format("{0}=\"{1}\" ", ROWCOUNT, cRows));
            tw.Write(string.Format("{0}=\"{1}\" />\n", PUNCT, iPunct));
        }

        const int CITERATE = 150;
        const int CPUNCTLISTSIZE = CITERATE;
        /// <summary>
        /// Output new data items
        /// </summary>
        /// <returns>The DataItem objects to output</returns>
        /// <seealso cref="Data.DataItem"/>
        public override List<DataItem> Iterate(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
        {
            ldiBufferOut.Clear();

            DataItem[] rgdi = gdi(CITERATE);
            for (int i = 0; i < CITERATE && !eof; i++)
            {
                if (iRow < cRows)
                {
                    rgdi[i].AddValue((int)((int)iRow / 7));
                    rgdi[i].AddValue((int)iRow);
                }
                else
                {
                    eof = rgdi[i].EOF = true;
                    Punctuation p = new Punctuation(2);
                    p.AddValue(new Punctuation.WildcardPattern());
                    p.AddValue(new Punctuation.WildcardPattern());
                    ldiBufferOut.Add(p);
                }

                ldiBufferOut.Add(rgdi[i]);

                if (iPunct == 0 && ((int)iRow / 7) != ((int)(iRow + 1) / 7))
                {
                    //Let's add a punctuation
                    Punctuation p = new Punctuation(2);
                    p.AddValue(new Punctuation.LiteralPattern((int)(iRow / 7)));
                    p.AddValue(new Punctuation.WildcardPattern());
                    ldiBufferOut.Add(p);
                }

                if (iPunct == 1)
                {
                    if (iRow >= CPUNCTLISTSIZE && iRow % CPUNCTLISTSIZE == 0)
                    {
                        Punctuation p2 = new Punctuation(2);
                        p2.AddValue(new Punctuation.WildcardPattern());
                        object[] rglp = new object[CPUNCTLISTSIZE];
                        for (int iLit = 0; iLit < CPUNCTLISTSIZE; iLit++)
                            rglp[iLit] = ((int)iRow - CPUNCTLISTSIZE + iLit);
                        p2.AddValue(new Punctuation.ListPattern(rglp));
                        ldiBufferOut.Add(p2);
                    }
                }

                if (iPunct == 2)
                {
                    if (iRow >= CPUNCTLISTSIZE && iRow % CPUNCTLISTSIZE == 0)
                    {
                        Punctuation p2 = new Punctuation(2);
                        p2.AddValue(new Punctuation.WildcardPattern());
                        int iMin = iRow - CPUNCTLISTSIZE + 1, iMax = iRow;
                        p2.AddValue(new Punctuation.RangePattern(iMin, iMax));
                        ldiBufferOut.Add(p2);
                    }
                }
                iRow++;
            }

            cRowsOutput += ldiBufferOut.Count;
            return ldiBufferOut;
        }

        /// <summary> Return current data rate for this stream source </summary>
        public double DataCount
        {
            get { return (double)cRowsOutput; }
        }

        /// <summary> Return the data rate of this source </summary>
        public double DataRate
        {
            get { return 0; }
        }

        /// <summary> How many data items are waiting to be processed? </summary>
        public int DataBacklog
        {
            get { return 0; }
        }
	}
	#endregion

    #region OpServer
    /// <summary>
    /// Class to receieve data from a network stream
    /// </summary>
    /// <remarks>This class does not take an input query operator -- It generates data</remarks>
    /// <remarks>This class must override Iterate(), since it's source is outside WhitStream</remarks>
    public class OpServer : UnaryOp, WhitStream.QueryEngine.IStreamDataSource
    {
        private Queue<DataItem> queBuffer = new Queue<DataItem>();
        private static WhitStream.Server.TCPServer server = new WhitStream.Server.TCPServer("127.0.0.1", WhitStream.Server.TCPServer.WHITSTREAM_PORT);
        NetworkStream nsClient;
        private delegate void StreamReader();
        private const int MAXDATA = 500;

        /// <summary> Default constructor to set up this operator as a listener </summary>
        public OpServer()
        {
            server.AddListener(SetClient);
            itemsIn = itemsOut = 1; 
        }

        /// <summary>
        /// Constructor set this operator up as a listener on a specific port
        /// </summary>
        /// <param name="port">The port to listen on</param>
        public OpServer(int port)
        {
            server.AddListener(SetClient, port);
            itemsIn = itemsOut = 1; 
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - Server<{1}>", base.ToString(), queBuffer.Count);
        }

        /// <summary> Method to start listening to a network stream input </summary>
        /// <param name="ns">The network stream to listen on</param>
        public void SetClient(NetworkStream ns)
        {
            nsClient = ns;
            StreamReader sr = new StreamReader(StrReader);
            sr.BeginInvoke(null, null);
        }

        /// <summary> Read data from the stream as quickly as possible, and store it in a queue </summary>
        protected void StrReader()
        {
            DataItem di = null;
            bool eof = false;
            BinaryFormatter bf = new BinaryFormatter();

            do
            {
                try
                {
                    di = (DataItem)bf.Deserialize(nsClient);
                    eof = di.EOF;
                }
                catch (System.IO.IOException)
                {
                    //Assume that the connection was closed (though I have no way of knowing...)
                    eof = true;
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Something bad happened when we tried to read from the stream: {0} ({1})",
                        ex.Message, ((ex.InnerException != null) ? ex.InnerException.Message : ""));
                    eof = true;
                }
                lock (queBuffer)
                {
                    queBuffer.Enqueue(di);
                }
            } while (!eof);

            nsClient.Close();

            lock (queBuffer)
            {
                di = new DataItem(2, null);
                di.EOF = true;
                queBuffer.Enqueue(di);
            }
        }

        /// <summary>
        /// Finds the number of tuples ready from this operator
        /// </summary>
        /// <returns>The number of items ready to be processed</returns>
        public override int GetItemsReady()
        {
            return queBuffer.Count;
        }

        /// <summary>
        /// Output new data items
        /// </summary>
        /// <returns>The DataItem objects to output</returns>
        /// <seealso cref="Data.DataItem"/>
        public override List<DataItem> Iterate(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
        {
            ldiBufferOut.Clear();

            lock (queBuffer)
            {
                if (queBuffer.Count > 0)
                {
                    int cData = (queBuffer.Count > MAXDATA) ? MAXDATA : queBuffer.Count;
                    for (int i = 0; i < cData; i++)
                        ldiBufferOut.Add(queBuffer.Dequeue());
                }

                //if (queBuffer.Count > 0)
                //    Console.WriteLine("queue size: {0}", queBuffer.Count);
            }

            return ldiBufferOut;
        }

        /// <summary> Return current data rate for this stream source </summary>
        public double DataCount
        {
            get { return -1; }
        }

        /// <summary> Return the data rate of this source </summary>
        public double DataRate
        {
            get { return 0; }
        }

        /// <summary> How many data items are waiting to be processed? </summary>
        public int DataBacklog
        {
            get { return queBuffer.Count; }
        }
    }
    #endregion

    #region OpServerRaw
    /// <summary>
    /// Class to receieve data from a network stream in raw form
    /// </summary>
    /// <remarks>This class does not take an input query operator -- It generates data</remarks>
    /// <remarks>This class must override Iterate(), since it's source is outside WhitStream</remarks>
    public class OpServerRaw : UnaryOp, WhitStream.QueryEngine.IStreamDataSource
    {
        private Queue<DataItem> queBuffer = new Queue<DataItem>();
        private static WhitStream.Server.TCPServer server = new WhitStream.Server.TCPServer("10.200.240.1", 9448);
        NetworkStream nsClient;
        private delegate void StreamReader();
        private const int MAXDATA = 500;
        Stopwatch time = new Stopwatch();
        private int itemCount = 0;
        DataItemPool.GetDataItem osGdi = null;
        string format = ""; //the format of the header

        /// <summary> Default constructor to set up this operator as a listener </summary>
        public OpServerRaw()
        { server.AddListener(SetClient); itemsIn = itemsOut = 1; }

        /// <summary>
        /// Constructor set this operator up as a listener on a specific port
        /// </summary>
        /// <param name="port">The port to listen on</param>
        public OpServerRaw(int port)
        { server.AddListener(SetClient, port); itemsIn = itemsOut = 1; }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        { return string.Format("{0} - ServerRaw<{1}>", base.ToString(), queBuffer.Count); }

        /// <summary> Method to start listening to a network stream input </summary>
        /// <param name="ns">The network stream to listen on</param>
        public void SetClient(NetworkStream ns)
        {
            nsClient = ns;
            StreamReader sr = new StreamReader(StrReader);
            sr.BeginInvoke(null, null);
        }

        private DataItem GetDataItem()
        {
            if (osGdi != null)
                return osGdi(1)[0];
            else
                return new DataItem(format.Length, null);
        }

        /// <summary> Read data from the stream as quickly as possible, and store it in a queue </summary>
        /// <remarks> Will recieve the header file before it tries to read raw data</remarks>
        protected void StrReader()
        {
            bool eof = false;
            BinaryFormatter bf = new BinaryFormatter();
            StringBuilder sb = new StringBuilder();
            DataItem di;
            int fiter = 0; //format iterator

            try
            {
                int inByte = nsClient.ReadByte();
                while ((char)inByte != '#' && inByte != -1) //# indicates the termination of the header
                {
                    sb.Append((char)inByte);
                    inByte = nsClient.ReadByte();
                }
                format = sb.ToString();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Something bad happened when we tried to read the header: {0} ({1})",
                    ex.Message, ((ex.InnerException != null) ? ex.InnerException.Message : ""));
                eof = true;
            }

            time.Start();
            do
            {
                di = GetDataItem();
                try
                {
                    while (fiter < format.Length && !eof)
                    {
                        switch (format[fiter]) //get the raw input based on the type
                        {
                            case 'i': //the input is a 32 bit int
                                {
                                    byte[] inbytes = new byte[4];
                                    if (nsClient.Read(inbytes, 0, 4) == 0) //read in the bytes
                                        eof = true;
                                    di.AddValue(BitConverter.ToInt32(inbytes, 0));
                                    break;
                                }
                            case 'c': //the input is a char
                                {
                                    byte[] inbytes = new byte[1];
                                    if (nsClient.Read(inbytes, 0, 1) == 0) //read in the bytes
                                        eof = true;
                                    di.AddValue(BitConverter.ToChar(inbytes, 0));
                                    break;
                                }
                            case 'b': //the input is a bool
                                {
                                    byte[] inbytes = new byte[1];
                                    if (nsClient.Read(inbytes, 0, 1) == 0) //read in the bytes
                                        eof = true;
                                    di.AddValue(BitConverter.ToBoolean(inbytes, 0));
                                    break;
                                }
                            default:
                                throw new System.Exception("The specified type from the header could not be identified");
                        }
                        fiter++;
                    }
                    fiter = 0;
                }
                catch (System.IO.IOException)
                {
                    //Assume that the connection was closed (though I have no way of knowing...)
                    eof = true;
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Something bad happened when we tried to read from the stream: {0} ({1})",
                        ex.Message, ((ex.InnerException != null) ? ex.InnerException.Message : ""));
                    eof = true;
                }
                lock (queBuffer)
                {
                    if (!eof)
                    {
                        di.TimeStamp = DateTime.Now;
                        queBuffer.Enqueue(di);
                        itemCount++;
                    }
                }
            } while (!eof);
            time.Stop();
            nsClient.Close();

            lock (queBuffer)
            {
                di = new DataItem(2, null);
                di.EOF = true;
                queBuffer.Enqueue(di);
            }
        }

        /// <summary>
        /// Finds the number of tuples ready from this operator
        /// </summary>
        /// <returns>The number of items ready to be processed</returns>
        public override int GetItemsReady()
        {
            return queBuffer.Count;
        }

        /// <summary>
        /// Output new data items
        /// </summary>
        /// <returns>The DataItem objects to output</returns>
        /// <seealso cref="Data.DataItem"/>
        public override List<DataItem> Iterate(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
        {
            if (osGdi == null)
                osGdi = gdi;
            ldiBufferOut.Clear();
            lock (queBuffer)
            {
                if (queBuffer.Count > 0)
                {
                    int cData = queBuffer.Count;
                    for (int i = 0; i < cData; i++)
                        ldiBufferOut.Add(queBuffer.Dequeue());
                }
            }
            return ldiBufferOut;
        }

        /// <summary> Return the current number of data items produced by this stream source </summary>
        public double DataCount { get { return itemCount; } }
        /// <summary> Return the data rate of this source </summary>
        public double DataRate { get { return (double)itemCount / time.ElapsedMilliseconds * 1000; } }
        /// <summary> How many data items are waiting to be processed? </summary>
        public int DataBacklog{ get { return queBuffer.Count; } }
    }
    #endregion

    #region Queue
    /// <summary>
    /// Operator to model a queue
    /// Data is pulled from the queue using Iterate
    /// Data is pushed into the queue using Push (not a member of UnaryOp!)
    /// </summary>
    /// <remarks>Since the queue does not read from its children, it is
    /// not appropriate for pull-based queries. It should only be used
    /// with the multi-threaded scheduler.
    /// </remarks>
    public class OpQueue : UnaryOp
    {
        //Use a list instead of a queue to support bulk fetches
        private Queue<DataItem> data;
        private int maxData = 0;
        private int cPunct = 0;
        private bool hasEOF = false;
        private const int QUEUEINITIALSIZE = 500;

        /// <summary>
        /// Default constructor for Queue
        /// </summary>
        public OpQueue() 
            : base()
        {
            Init(10000);
        }

        /// <summary>
        /// Constructor for setting max data output size for Queue
        /// </summary>
        /// <param name="m">Maximum data items to output</param>
        public OpQueue(int m) 
            : base()
        {
            Init(m);
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - Queue<{1}>", base.ToString(), data.Count);
        }

        private void Init(int m)
        {
            data = new Queue<DataItem>(QUEUEINITIALSIZE);
            maxData = m;
            itemsIn = itemsOut = 1;
        }

        /// <summary>
        /// Serialize this Queue operator
        /// </summary>
        /// <remarks>
        /// Actually, the scheduler should handle this, so
        /// serialization of this operator is unnecessary
        /// </remarks>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);

            tw.Write(" />\n");
        }

        /// <summary>
        /// Finds the number of tuples ready from this operator
        /// </summary>
        /// <returns>The number of items ready to be processed</returns>
        public override int GetItemsReady()
        {
            return data.Count;
        }

        /// <summary>
        /// Iterate through input data items. For a queue, just 
        /// output up to maximum data items
        /// </summary>
        /// <returns></returns>
        public override List<DataItem> Iterate(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
        {
            //List<DataItem> ldiOut;
            bool fEOF = false;

            ldiBufferOut.Clear();
            lock (data)
            {
                //int c = (maxData > data.Count) ? data.Count : maxData;
                int c = data.Count;
                //ldiOut = new List<DataItem>(c);
                while (c > 0)
                {
                    DataItem di = data.Dequeue();
                    ldiBufferOut.Add(di);
                    fEOF |= di.EOF;
                    c--;
                }
                if (ldiBufferOut.Count > 0)
                    eof = fEOF;
            }
            return ldiBufferOut;
        }

        /// <summary> Add a data item to the queue </summary>
        /// <param name="d">Data to enqueue</param>
        public void Push(DataItem d)
        {
            if (d != null)
            {
                lock (data)
                {
                    data.Enqueue(d);
                    if (d is Punctuation)
                        cPunct++;
                    hasEOF |= d.EOF;
                }
            }
        }

        /// <summary> Add a list of data items to the queue </summary>
        /// <param name="ds">Data to enqueue</param>
        public void Push(List<DataItem> ds)
        {
            lock (data)
            {
                foreach (DataItem d in ds)
                {
                    Push(d);
                }
            }
        }

        /// <summary>
        /// Return whether EOF is in the queue or not
        /// </summary>
        public bool HasEOF { get { return hasEOF; } }

        /// <summary>
        /// Return how much data is in the queue
        /// </summary>
        public int DataCount
        { get { return data.Count; } }

        /// <summary>
        /// Return how many punctuations are in the queue
        /// </summary>
        public int PunctCount
        { get { return cPunct; } }
	}
	#endregion

	#region Select
	/// <summary>
    /// Operator to model the query operator Select.
    /// Filter out DataItem objects based on a given predicate.
    /// </summary>
    /// <remarks>Since Select is non-blocking, it uses the trivial Final function </remarks>
    /// <seealso cref="Query.FinalTrivial"/>
    public class OpSelect : UnaryOp
    {
        private string stExpr;

        /// <summary> Constant for serialization to XML </summary>
        private const string XMLEXPR = "EXPR";
        private Naive algoNaive;

        /// <summary>
        /// Default constructor for OpSelect
        /// </summary>
        public OpSelect() 
            : base() { }

        /// <summary>
        /// Constructor for a new Select object, given some predicate and data source
        /// </summary>
        /// <param name="pred">The predicate to filter on</param>
        /// <param name="opIn">The input query operator</param>
        /// <remarks>The predicate accepts =, !=, &lt;,&gt;, &lt;=, &gt;=, AND, OR, and NOT.</remarks>
        /// <remarks>To compare with attributes in a data item, you first give '$', followed
        /// by the DataItem number (always '1' for select), and then the attribute number (1-based)</remarks>
        /// <example>"$1.4 = 100" finds all data items with the value 100 in the fourth attribute</example>
        public OpSelect(string pred, Query opIn) 
            : base(opIn)
        {
            Init(pred);
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - Select({1})", base.ToString(), stExpr);
        }

        /// <summary>
        /// Constructor to build select operator from XML
        /// </summary>
        /// <param name="id">ID for this operator</param>
        /// <param name="pred">Predicate for this operator</param>
        /// <param name="opIn">Input for this operator</param>
        /// <remarks>To be used by deserialization only</remarks>
        public OpSelect(string id, string pred, Query opIn) 
            : base(opIn)
        {
            opID = Int32.Parse(id.Substring(id.IndexOf('=') + 1));

            pred = pred.Replace("&lt;", "<").Replace("&gt;", ">").Substring(XMLEXPR.Length + 1);
            Init(pred);
        }

        private void Init(string pred)
        {
            stExpr = pred;

            algorithm = new UnaryAlgorithm(algoNaive = new Naive(pred));
        }

        /// <summary>
        /// The selection predicate to evaluate over each data item as it arrives
        /// </summary>
        public string Predicate
        {
            get { return stExpr; }
            set { Init(value); }
        }

        /// <summary> Serialize this Select operator by writing the expression </summary>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);

            string st = stExpr.Replace("<", "&lt;").Replace(">", "&gt;");
            tw.Write(string.Format("{0}=\"{1}\" />\n", XMLEXPR, st));
        }

#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step function is called with each data item that arrives.
        /// The Final function is called when EOF is encountered
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool eofInput;

            ldiBufferOut.Clear();
            if (opIn != null)
            {
                SplitPunc(opIn.Iterate(), ref ldiBuffer, ref lpBuffer);
                algoNaive.SelStepList(ldiBuffer, ref ldiBufferOut, out eofInput);
                foreach (Punctuation p in lpBuffer)
                {
                    algoNaive.SelProp(p, ldiBufferOut);
                }
                if (eofInput)
                {
                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif
        
        /// <summary>
        /// Naive select algorithm -- simply apply predicate to each data item as they arrive
        /// </summary>
        public class Naive : UnaryAlgorithmDefinition
        {
            private IExpr expr;

            /// <summary>
            /// Constructor for a naive select implementation
            /// </summary>
            /// <param name="pred">The string expression for select</param>
            public Naive(string pred)
            {
                expr = ExprParser.Parse(pred);
            }

            /// <summary>Step functionality for Select</summary>
            public override Step FnStep { get { return SelStep; } }
            /// <summary>StepList functionality for Select</summary>
            public override StepList FnStepList { get { return SelStepList; } }
            /// <summary>Prop functionality for Select</summary>
            public override Prop FnProp { get { return SelProp; } }

            internal void SelStep(DataItem di, List<DataItem> ldi)
            {
                if (expr.Evaluate(di))
                    ldi.Add(di);
            }

            internal void SelStepList(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;

                foreach (DataItem di in rgdi)
                {
                    if (di.EOF == false && expr.Evaluate(di))
                        ldi.Add(di);
                    eofInput |= di.EOF;
                }
            }

            internal void SelProp(Punctuation p, List<DataItem> ldi)
            {
                ldi.Add(p);
            }
        }

        /// <summary>
        /// RandomDrop select algorithm -- apply predicate to each data item as they arrive,
        /// and also randomly select data items to drop for load shedding
        /// </summary>
        public class RandomDrop : UnaryAlgorithmDefinition
        {
            private IExpr expr;
            int freq;
            Random rndDrop = new Random();

            /// <summary>
            /// Constructor for a naive select implementation
            /// </summary>
            /// <param name="pred">The string expression for select</param>
            /// <param name="frequency">How often should a data item be dropped (1 data item per <code>frequency</code>)?</param>
            public RandomDrop(string pred, int frequency)
            {
                expr = ExprParser.Parse(pred);
                freq = frequency;
            }

            /// <summary>Step functionality for Select (with random drop)</summary>
            public override Step FnStep { get { return SelStep; } }
            /// <summary>StepList functionality for Select (with random drop)</summary>
            public override StepList FnStepList { get { return SelStepList; } }
            /// <summary>Prop functionality for Select (with random drop)</summary>
            public override Prop FnProp { get { return SelProp; } }

            internal void SelStep(DataItem di, List<DataItem> ldi)
            {
                if (expr.Evaluate(di) && rndDrop.Next(freq) != 0)
                    ldi.Add(di);
                else
                    //ReleaseDataItem(di);
                    di.Dispose();
            }

            internal void SelStepList(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;

                foreach (DataItem di in rgdi)
                {
                    int drop = rndDrop.Next(freq);
                    if (di.EOF == false && expr.Evaluate(di) && drop != 0)
                        ldi.Add(di);
                    eofInput |= di.EOF;
                }
            }

            internal void SelProp(Punctuation p, List<DataItem> ldi)
            {
                ldi.Add(p);
            }
        }
    }
	#endregion

	#region Project
	/// <summary>
    /// Models the query operator Project, to map DataItem objects from one format to another
    /// </summary>
    /// <remarks>Since Project is non-blocking, it uses the trivial Final function </remarks>
    /// <seealso cref="Query.FinalTrivial"/>
    public class OpProject : UnaryOp
    {
        /// <summary>
        /// The function to modify the format of each arriving DataItem
        /// </summary>
        private const string XMLATTRS = "ATTRS";
        private int[] attrsOrig = null;
        private Naive algoNaive;

        /// <summary>
        /// Default constructor for project
        /// </summary>
        public OpProject()
            : base()
        { }

        /// <summary>
        /// The constructor for Project, given a format function and the source
        /// </summary>
        /// <param name="m">The function to map the format of each DataItem</param>
        /// <param name="a">The attributes that remain after projection</param>
        /// <param name="opIn">The input query operator</param>
        public OpProject(Map m, int[] a, Query opIn)
            : base(opIn)
        {
            Init(m, a);
        }

        /// <summary>
        /// Constructor to build project operator from XML
        /// To be used by deserialization only
        /// </summary>
        /// <param name="id">ID for this operator</param>
        /// <param name="sAttr">Attributes to project</param>
        /// <param name="opIn">Input operator</param>
        public OpProject(string id, string sAttr, Query opIn)
            : base(opIn)
        {
            opID = Int32.Parse(id.Substring(id.IndexOf('=') + 1));

            string[] rgstAttrs = sAttr.Substring(sAttr.IndexOf('=') + 1).Split(',');
            int[] a = new int[rgstAttrs.Length];
            for (int i = 0; i < rgstAttrs.Length; i++)
                a[i] = Int32.Parse(rgstAttrs[i]);

            Init(null, a);
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - Project", base.ToString());
        }

        private void Init(Map m, int[] a)
        {
            attrsOrig = new int[a.Length];
            a.CopyTo(attrsOrig, 0);

            algorithm = new UnaryAlgorithm(algoNaive = new Naive(m, a));
        }

        /// <summary>
        /// What attributes are we projecting?
        /// </summary>
        public int[] Attributes
        {
            get { return attrsOrig; }
            set { Init(null, value); }
        }

        /// <summary> Serialize this Project operator </summary>
        /// <param name="tw"> Destination for XML data </param>
        /// <TODO>Need to figure out a way to serialize Map functions</TODO>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);

            tw.Write(string.Format("{0}=\"", XMLATTRS));
            for (int i = 0; i < attrsOrig.Length; i++)
            {
                if (i != 0)
                    tw.Write(", ");
                tw.Write(string.Format("{0}", attrsOrig[i]));
            }
            tw.Write("\" />\n");
        }

#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step function is called with each data item that arrives.
        /// The Final function is called when EOF is encountered
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool eofInput;

            ldiBufferOut.Clear();
            if (opIn != null)
            {
                SplitPunc(opIn.Iterate(), ref ldiBuffer, ref lpBuffer);
                algoNaive.PrjStepList(ldiBuffer, ref ldiBufferOut, out eofInput);
                foreach (Punctuation p in lpBuffer)
                {
                    algoNaive.PrjProp(p, ldiBufferOut);
                }
                if (eofInput)
                {
                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif
        
        /// <summary>
        /// Naive implementation of Project (duplicate-preserving)
        /// </summary>
        public class Naive : UnaryAlgorithmDefinition
        {
            private Map map;
            private int[] attrs = null;

            /// <summary>
            /// Constructor for naive project implementation
            /// </summary>
            /// <param name="m">Map function (if required)</param>
            /// <param name="a">Attributes to project on</param>
            public Naive(Map m, int[] a)
            {
                map = (m == null) ? mapGeneric : m;
                attrs = new int[a.Length];
                a.CopyTo(attrs, 0);
                Array.Sort(attrs);
            }

            /// <summary>Step functionality for Project</summary>
            public override Step FnStep { get { return PrjStep; } }
            /// <summary>StepList functionality for Project</summary>
            public override StepList FnStepList { get { return PrjStepList; } }
            /// <summary>Prop functionality for Project</summary>
            public override Prop FnProp { get { return PrjProp; } }

            /// <summary>
            /// Generic Map function that outputs results based on given attribute list
            /// </summary>
            /// <returns>Projected data item</returns>
            /// <seealso cref="UnaryOp.Map"/>
            private DataItem mapGeneric(DataItem di)
            {
                if (di is Punctuation)
                {
                    Punctuation pRet = new Punctuation(attrs.Length);
                    foreach (int i in attrs)
                        pRet.AddValue(di[i]);
                    return pRet;
                }
                else
                {
                    DataItem diRet = new DataItem(attrs.Length, null);
                    foreach (int i in attrs)
                        diRet.AddValue(di[i]);
                    return diRet;
                }
            }

            internal void PrjStep(DataItem di, List<DataItem> ldi)
            {
                ldi.Add(map(di));
            }

            internal void PrjStepList(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;
                foreach (DataItem di in rgdi)
                {
                    if (di.EOF == false)
                        ldi.Add(map(di));
                    eofInput |= di.EOF;
                }
            }

            internal void PrjProp(Punctuation p, List<DataItem> ldi)
            {
                //Potentially expensive to do this for every punctuation.
                // Adding a describe operator at the base of the query plan may improve
                // performance (by eliminating the check here).
                if (p.Describes(attrs))
                    ldi.Add(map(p));
            }
        }

		/// <summary>
		/// Determines if the given Punctuation scheme benefits Project
		/// </summary>
		/// <param name="ps">The punctuation scheme to check</param>
		/// <returns>If Benefits = true, else false</returns>
		public override bool Benefit(PunctuationScheme ps)
		{
			if (attrsOrig == null) return false;
			List<int> NonWildcard = new List<int>();
			//Extract all the non-wildcard patterns from the Punctuation Scheme
			for (int i = 0; i < ps.Count; i++)
			{
				if (!(ps[i] is Punctuation.WildcardPattern))
					NonWildcard.Add(i);
			}
			//Foreach non-wildcard pattern - see if it is in the kept attributes
			//Go through all the non-wildcard positions
			foreach (int position in NonWildcard)
			{
				bool contains = false;
				//Go through all the attributes being projected
				foreach (int attr in attrsOrig)
				{
					//If we found a match - we know that we can punctuate on
					//that attribute
					if (attr == position)
						contains = true;
				}
				if (!contains)
					return false;
			}
			//If we made it through the matching process without returning,
			//we know that we can pass this punctuation since we project all
			//of the non-wildcard patterns.
			return true;
		}
	}
	#endregion

	#region Duplicate Elimination
	/// <summary>
    /// Models the query operator DuplicateElimination, to remove duplicates from the input
    /// </summary>
    public class OpDupElim : UnaryOp
    {
        private HashAlgorithm algoHash;

        /// <summary>
        /// Default constructor for OpDupElim
        /// </summary>
        public OpDupElim()
            : base()
        {
            Init();
        }

        /// <summary>
        /// Constructor for DupElim, given only an input
        /// </summary>
        /// <param name="opIn">Input query operator</param>
        public OpDupElim(Query opIn)
            : base(opIn)
        {
            Init();
        }

        /// <summary>
        /// Constructor to build dupelim operator from XML
        /// </summary>
        /// <param name="id">id for this operator</param>
        /// <param name="opIn">input operator</param>
        /// <remarks>To be used by deserialization only</remarks>
        public OpDupElim(string id, Query opIn)
            : base(opIn)
        {
            opID = Int32.Parse(id.Substring(id.IndexOf('=') + 1));

            Init();
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - DupElim<{1}>", base.ToString(), algoHash.StateSize);
        }

        private void Init()
        {
            algorithm = new UnaryAlgorithm(algoHash = new HashAlgorithm());
        }

        /// <summary>
        /// Serialize this operator
        /// </summary>
        /// <param name="tw">Destination for XML data</param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);
            tw.Write(" />\n");
        }

#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step function is called with each data item that arrives.
        /// The Final function is called when EOF is encountered
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool eofInput;

            ldiBufferOut.Clear();
            if (opIn != null)
            {
                SplitPunc(opIn.Iterate(), ref ldiBuffer, ref lpBuffer);
                algoHash.DEStepList(ldiBuffer, ref ldiBufferOut, out eofInput);
                foreach (Punctuation p in lpBuffer)
                {
                    algoHash.DEKeep(p);
                    algoHash.DEProp(p, ldiBufferOut);
                }
                if (eofInput)
                {
                    algoHash.DEFinal(ldiBufferOut);

                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif

        /// <summary>
        /// Algorithm for Duplicate Elimination using a hash table
        /// </summary>
        public class HashAlgorithm : UnaryAlgorithmDefinition
        {
            // Storage for the incoming DataItem objects
            private Dictionary<int, List<DataItem>> ht = new Dictionary<int, List<DataItem>>();

            /// <summary> How many data items are curently held in state </summary>
            public override int StateSize { get { return ht.Count; } }

            /// <summary>Step functionality for Duplicate Elimination</summary>
            public override Step FnStep { get { return DEStep; } }
            /// <summary> StepList functionality for Duplicate Elimination </summary>
            public override StepList FnStepList { get { return DEStepList; } }
            /// <summary>Prop functionality for Duplicate Elimination</summary>
            public override Prop FnProp { get { return DEProp; } }
            /// <summary>Keep functionality for Duplicate Elimination</summary>
            public override Keep FnKeep { get { return DEKeep; } }
            /// <summary>Final functionality for Duplicate Elimination</summary>
            public override Final FnFinal { get { return DEFinal; } }

            internal void DEStep(DataItem di, List<DataItem> ldi)
            {
                bool fNew = false;
                int hc = di.GetHashCode();
                DataItem ndi = new DataItem(di, 0, null);

                if (ht.ContainsKey(hc) == false)
                {
                    List<DataItem> al = new List<DataItem>();
                    al.Add(ndi);
                    ht[hc] = al;
                    fNew = true;
                }
                else
                {
                    List<DataItem> al = ht[hc];
                    if (al.IndexOf(di) == -1)
                    {
                        al.Add(ndi);
                        ht[hc] = al;
                        fNew = true;
                    }
                }

                if (fNew)
                    ldi.Add(di);
            }

            internal void DEStepList(List<DataItem> rgdiIn, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;

                foreach (DataItem di in rgdiIn)
                {
                    if (di.EOF == false)
                        DEStep(di, ldi);
                    eofInput |= di.EOF;
                }
            }
            
            internal void DEFinal(List<DataItem> ldi)
            {
                ht.Clear();
            }

            internal void DEProp(Punctuation p, List<DataItem> ldi)
            {
                ldi.Add(p);
            }

            internal void DEKeep(Punctuation p)
            {
                List<int> keys = new List<int>();
                foreach (int k in ht.Keys)
                {
                    List<DataItem> al = ht[k];

                    for (int i = 0; i < al.Count; i++)
                    {
                        if (p.Match(al[i]))
                            al.RemoveAt(i--);
                    }

                    if (al.Count == 0)
                        keys.Add(k);
                }

                foreach (int k in keys)
                    ht.Remove(k);
            }
        }
	}
	#endregion

	#region Group-by
	#region Group-by (Base)
	/// <summary>
    /// Models the query operator GroupBy, to group DataItems based on some attribute(s)
    /// </summary>
    public abstract class OpGroupBy : UnaryOp
    {
        /// <summary>List of attributes for grouping</summary>
        protected int[] attrs;
        /// <summary>List of attributes that will receive punctuations</summary>
        protected int[] mainAttrs;
        /// <summary>The attribute which contains a list</summary>
        protected int listAttr;
        /// <summary> The algorithm to execute </summary>
        protected HashAlgorithm algoHash;

        /// <summary>
        /// The default constructor
        /// </summary>
        public OpGroupBy()
            : base() { }

        /// <summary>
        /// Constructor for GroupBy.
        /// </summary>
        /// <param name="a">The attributes to group on</param>
        /// <param name="opIn">The input query operator</param>
        /// <remarks>Step functionality is determined by the aggregate child class (e.g. AVG)</remarks>
        public OpGroupBy(int[] a, Query opIn)
            : base(opIn)
        {
            attrs = a;
            mainAttrs = a;
            listAttr = -1;
        }

        /// <summary>
        /// Constructor for GroupBy with main attributes
        /// </summary>
        /// <param name="a">The attributes to group on</param>
        /// <param name="m">The main attributes which will receive punctuations</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupBy(int[] a, int[] m, Query opIn)
            : base(opIn)
        {
            attrs = a;
            mainAttrs = m;
            listAttr = -1;
        }

        /// <summary>
        /// Constructor for GroupBy with a list attribute
        /// </summary>
        /// <param name="a">The attributes to group on</param>
        /// <param name="m">The main attributes which will receive punctuations</param>
        /// <param name="l">The attribute with a list</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupBy(int[] a, int[] m, int l, Query opIn)
            : base(opIn)
        {
            attrs = a;
            mainAttrs = m;
            listAttr = l;
        }

        /// <summary>
        /// Constructor to build GroupBy operators from XML
        /// </summary>
        /// <param name="id">ID for this operator</param>
        /// <param name="a">Attributes for grouping</param>
        /// <param name="opIn">Input operator</param>
        /// <remarks> To be used by deserialization only </remarks>
        public OpGroupBy(string id, string a, Query opIn)
            : base(opIn)
        {
            int[] rgAttrs = null;
            opID = Int32.Parse(id.Substring(id.IndexOf('=') + 1));

            string tmp = a.Substring(a.IndexOf('=') + 1);
            if (tmp.Length != 0)
            {
                string[] rgstAttrs = tmp.Split(',');
                rgAttrs = new int[rgstAttrs.Length];
                for (int i = 0; i < rgstAttrs.Length; i++)
                    rgAttrs[i] = Int32.Parse(rgstAttrs[i]);
            }

            attrs = rgAttrs;
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - GroupBy<{1}>", base.ToString(), algoHash.StateSize);
        }

        /// <summary>
        /// The attributes to group by
        /// </summary>
        public int[] GroupingAttributes
        {
            get { return attrs; }
            set { attrs = value; }
        }

        /// <summary> Serialize this group-by operator </summary>
        /// <param name="tw"> Destination for XML data </param> 
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);

            tw.Write("ATTRS=\"");
            if (attrs != null)
            {
                for (int i = 0; i < attrs.Length; i++)
                {
                    if (i != 0)
                        tw.Write(", ");
                    tw.Write(string.Format("{0}", attrs[i]));
                }
            }
            tw.Write("\" ");
        }

        /// <summary>
        /// Generic functionality for grouping using hash tables
        /// </summary>
        public class HashAlgorithm : UnaryAlgorithmDefinition
        {
            /// <summary>Models state for the GroupBy operator</summary>
            protected class Group
            {
                /// <summary>Which keys make up this group?</summary>
                public object[] keys;
                /// <summary>What state is required for continued execution for an aggregate operator (e.g. AVG)</summary>
                public object oState;
                /// <summary>What would the result for this group be if the result was required?</summary>
                public object oResult;
                /// <summary>What is this group's hashcode?</summary>
                public int hashcode;
                /// <summary>What is the hashcode for this group's SubDictionary?</summary>
                public int sdhashcode;
            }
            /// <summary>Dictionary class that contains context information</summary>
            protected class SubDictionary
            {
                /// <summary></summary>
                public Dictionary<int, List<Group>> subHt = new Dictionary<int, List<Group>>();
                /// <summary>What are the keys for this SubDictionary?</summary>
                public object[] keys;
                /// <summary>What is this SubDictionary's hashcode?</summary>
                public int hashcode;
            }

            // A hash table with partial keys based on the main attributes
            private Dictionary<int, List<SubDictionary>> ht = new Dictionary<int, List<SubDictionary>>();
            // The only group if attrs = null
            private Group gTotal;
            /// <summary>Which attributes are we grouping on?</summary>
            protected int[] attrs;
            /// <summary>Which attributes are we </summary>
            protected int[] mainAttrs;

            private bool fComplete = false;

            /// <summary>
            /// Create a new HashAlgorithm operator
            /// </summary>
            /// <param name="a">The attributes for grouping</param>
            public HashAlgorithm(int[] a) : this(a, a) { }

            /// <summary>
            /// Create a new HashAlgorithm operator
            /// </summary>
            /// <param name="a">The attributes for grouping</param>
            /// <param name="m">The main attributes to group on</param>
            public HashAlgorithm(int[] a, int[] m) { attrs = a; mainAttrs = m; }

            /// <summary> How many data items are curently held in state </summary>
            public override int StateSize
            {
                get
                {
                    int cState = 0;
                    //this seems really bad, but generally each list will have no more than one value
                    foreach (List<SubDictionary> lisd in ht.Values)
                    {
                        foreach (SubDictionary sd in lisd)
                        {
                            foreach (List<Group> al in sd.subHt.Values)  
                                cState += al.Count;
                        }
                    }
                    return cState;
                }
            }

            /// <summary>Generic StepList functionality for Group By</summary>
            public override StepList FnStepList { get { return GBStepList; } }            
            /// <summary>Generic Final functionality for Group By</summary>
            public override Final FnFinal { get { return GBFinal; } }
            /// <summary>Generic Pass functionality for Group By</summary>
            public override Pass FnPass { get { return GBPass; } }
            /// <summary>Generic Keep functionality for Group By</summary>
            public override Keep FnKeep { get { return GBKeep; } }
            /// <summary>Generic Prop functionality for Group By</summary>
            public override Prop FnProp { get { return GBProp; } }

            internal void GBStepList(List<DataItem> rgdiIn, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;
                Step step = FnStep;

                foreach (DataItem di in rgdiIn)
                {
                    if (di.EOF == false)
                        step(di, ldi);
                    eofInput |= di.EOF;
                }
            }

            internal void GBFinal(List<DataItem> ldi)
            {
                if (fComplete)
                    return;

                fComplete = true;

                DataItem di;
                if (attrs == null)
                {
                    di = new DataItem(1, ReleaseDataItem);
                    di.AddValue(gTotal.oResult);
                    ldi.Add(di);
                }
                else
                {
                    foreach (List<SubDictionary> lisd in ht.Values)
                    {
                        foreach (SubDictionary sd in lisd)
                        {
                            foreach (List<Group> al in sd.subHt.Values)
                            {
                                foreach (Group g in al)
                                {
                                    BuildGroupResult(g, out di);
                                    ldi.Add(di);
                                }
                            }
                            sd.subHt.Clear();
                        }
                    }
                    ht.Clear();
                }
            }

            internal void GBPass(Punctuation p, List<DataItem> ldi)
            {
                DataItem di;

                bool fEOF = true;
                for (int i = 0; fEOF && i < p.Count; i++)
                    fEOF &= (p[i] is Punctuation.WildcardPattern);

                if (fEOF)
                    GBFinal(ldi);
                else if (attrs != null)
                {
                    //Can't ever output results early unless we're grouping on specific attributes
                    List<Group> al = FindMatchingGroups(p);
                    if (al != null)
                    {
                        foreach(Group g in al)
                        {
                            BuildGroupResult(g, out di);
                            ldi.Add(di);
                        }
                    }
                }
            }

            internal void GBKeep(Punctuation p)
            {
                if (attrs != null)
                {
                    List<Group> al = FindMatchingGroups(p);
                    if (al != null && p.Describes(mainAttrs)) //we can drop all the SubDictionaries related to this punctuation
                    {
                        HashSet<int> sdlkeys = new HashSet<int>();
                        foreach (Group g in al) //find all the keys
                            sdlkeys.Add(g.sdhashcode);
                        foreach (int key in sdlkeys)
                        {
                            List<SubDictionary> lisd = ht[key];
                            if (lisd.Count <= 1) //the list will be empty, so we can remove it
                                ht.Remove(key);
                            else
                            {
                                SubDictionary sd;
                                if (FindSubDictionary(p, lisd, out sd))
                                    lisd.Remove(sd);
                            }
                        }
                    }
                    else if (al != null) //we have to do it on a group by group basis
                    {
                        SubDictionary sd;
                        List<Group> lg;
                        foreach (Group g in al)
                        {
                            FindSubDictionary(p, ht[g.sdhashcode], out sd);
                            lg = sd.subHt[g.hashcode];
                            if (lg.Count == 0)
                                sd.subHt.Remove(g.hashcode); // get rid of the entire list
                            else
                                lg.Remove(g);
                        }                        
                    }
                }
            }

            internal void GBProp(Punctuation p, List<DataItem> ldi)
            {
                bool fEOF = true;
                for (int i = 0; fEOF && i < p.Count; i++)
                {
                    fEOF &= (p[i] is Punctuation.WildcardPattern);
                }

                if (fEOF && attrs == null)
                {
                    Punctuation pOut = new Punctuation(1);
                    pOut.AddValue(new Punctuation.WildcardPattern());

                    ldi.Add(pOut);
                }
                else if (mainAttrs != null && p.Describes(mainAttrs))
                {
                    Punctuation pOut = new Punctuation(attrs.Length + 1);
                    int iMA = 0;
                    for (int i = 0; i < attrs.Length; i++)
                    {
                        if (iMA < mainAttrs.Length && attrs[i] == mainAttrs[iMA])
                            pOut.AddValue(p[mainAttrs[iMA++]]);
                        else
                            pOut.AddValue(new Punctuation.WildcardPattern());
                    }
                    pOut.AddValue(new Punctuation.WildcardPattern());

                    ldi.Add(pOut);
                }
                else if (fEOF || (attrs != null && p.Describes(attrs)))
                {
                    Punctuation pOut = new Punctuation(attrs.Length + 1);
                    foreach (int a in attrs)
                        pOut.AddValue(p[a]);
                    pOut.AddValue(new Punctuation.WildcardPattern());

                    ldi.Add(pOut);
                }
                
            }

            /// <summary>
            /// Find the appropriate group for this DataItem object
            /// </summary>
            /// <param name="di">The input DataItem</param>
            /// <param name="g">The group that the DataItem belongs to</param>
            /// <returns>True if this group already exists, false if this is a new group</returns>
            protected bool FindGroup(DataItem di, out Group g)
            {
                bool fExists = false;
                if (attrs == null)
                {
                    fExists = (gTotal != null);
                    if (!fExists)
                    {
                        gTotal = new Group();
                        gTotal.keys = null;
                    }
                    g = gTotal;
                    return fExists;
                }

                int mainhc = di.GetSpecificHashCode(mainAttrs);

                if (!ht.ContainsKey(mainhc)) //do we have the SubDictionary's list?
                    NewSubDictionary(di, null, out g);
                else
                {
                    List<SubDictionary> lisd = ht[mainhc];
                    SubDictionary sd;
                    bool foundSd = FindSubDictionary(di, lisd, out sd);

                    if (!foundSd)
                        NewSubDictionary(di, lisd, out g);
                    else //We found the SubDictionary based on the main attributes
                    {
                        int totalhc = di.GetSpecificHashCode(attrs);

                        if (!sd.subHt.ContainsKey(totalhc))
                            NewGroup(di, null, sd, out g);
                        else
                        {
                            if (!sd.subHt.ContainsKey(totalhc)) //do we have the group's list?
                                NewGroup(di, null, sd, out g);
                            else
                            {
                                List<Group> al = sd.subHt[totalhc];
                                bool foundGroup = FindGroupFromList(di, al, out g);

                                fExists = foundGroup;
                                if (!foundGroup)
                                    NewGroup(di, al, sd, out g);
                            }
                        }
                    }
                }
                return fExists;
            }

            private void NewSubDictionary(DataItem di, List<SubDictionary> lisd, out Group g)
            {
                int mainkey = di.GetSpecificHashCode(mainAttrs);
                SubDictionary sd = new SubDictionary();
                sd.hashcode = mainkey;

                int iKey = 0;
                sd.keys = new object[mainAttrs.Length];
                foreach (int a in mainAttrs)
                    sd.keys[iKey++] = di[a];

                NewGroup(di, null, sd, out g);

                if (lisd == null)
                    lisd = new List<SubDictionary>();
                lisd.Add(sd);
                ht[sd.hashcode] = lisd;
            }

            private void NewGroup(DataItem di, List<Group> al, SubDictionary sd, out Group g)
            {
                g = new Group();
                g.hashcode = di.GetSpecificHashCode(attrs);
                g.sdhashcode = sd.hashcode;

                g.keys = new object[attrs.Length];
                int iKey = 0;
                foreach (int a in attrs)
                    g.keys[iKey++] = di[a];

                if (al == null)
                    al = new List<Group>();

                al.Add(g);
                sd.subHt[g.hashcode] = al;
            }

            /// <summary>
            /// Return all groups that match the given punctuation
            /// </summary>
            /// <param name="p">The punctuation to match groups on</param>
            /// <returns>The groups that match the punctuation</returns>
            protected List<Group> FindMatchingGroups(Data.Punctuation p)
            {
                List<SubDictionary> lisd = null;
                List<Group> al = null;

                if (p.Describes(attrs)) 
                {
                    bool fComplete = true;
                    foreach (int a in mainAttrs)
                        fComplete &= (!(p[a] is Data.Punctuation.WildcardPattern));

                    //Right now, we only support those punctuations that completely describe a group
                    // and only on literal patterns
                    if (fComplete)
                    {
                        int mainhc = p.GetSpecificHashCode(mainAttrs);
                        if (ht.ContainsKey(mainhc))
                        {
                            lisd = ht[mainhc];
                            SubDictionary sd;

                            if (FindSubDictionary(p, lisd, out sd)) //we found the sub dictionary
                            {
                                if (p.Describes(mainAttrs)) //if the punctuation matches exactly the main attributes
                                {
                                    al = new List<Group>();
                                    foreach (List<Group> lg in sd.subHt.Values)
                                        al.AddRange(lg);
                                }
                                else //the punctuation describes all attributes
                                {
                                    int totalhc = p.GetSpecificHashCode(attrs);
                                    if (sd.subHt.ContainsKey(totalhc))
                                    {
                                        List<Group> lg = sd.subHt[totalhc];
                                        Group g;

                                        if (FindGroupFromList(p, lg, out g))
                                        {
                                            al = new List<Group>(1);
                                            al.Add(g);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return al;
            }

            private void BuildGroupResult(Group g, out DataItem di)
            {
                di = GetDataItem(1)[0];
                foreach (object v in g.keys)
                    di.AddValue(v);
                di.AddValue(g.oResult);
            }

            private bool FindSubDictionary(DataItem di, List<SubDictionary> lisd, out SubDictionary sd)
            {
                sd = null;
                bool foundSd = false;
                for (int i = 0; !foundSd && i < lisd.Count; i++)
                {
                    sd = lisd[i];
                    bool isEqual = true;
                    for (int j = 0; isEqual && j < sd.keys.Length; j++)
                    {
                        if (di is Punctuation)
                            isEqual &= ((Punctuation.Pattern)di[mainAttrs[j]]).Match(sd.keys[j]);
                        else
                            isEqual &= di[mainAttrs[j]].Equals(sd.keys[j]);
                    }
                    foundSd = isEqual;
                }
                return foundSd;
            }

            private bool FindGroupFromList(DataItem di, List<Group> lg, out Group g)
            {
                g = null;
                bool foundGroup = false;
                for (int i = 0; !foundGroup && i < lg.Count; i++)
                {
                    g = lg[i];
                    bool isEqual = true;
                    for (int j = 0; isEqual && j < g.keys.Length; j++)
                    {
                        if (di is Punctuation)
                            isEqual &= ((Punctuation.Pattern)di[attrs[j]]).Match(g.keys[j]);
                        else
                            isEqual &= di[attrs[j]].Equals(g.keys[j]);
                    }
                    foundGroup = isEqual;
                }
                return foundGroup;
            }
        }

		/// <summary>
		/// Determines if the given Punctuation scheme benefits Group-by 
		/// </summary>
		/// <param name="ps">The punctuation scheme to check</param>
		/// <returns>If Benefits = true, else false</returns>
        /// <remarks>Main attributes not yet supported</remarks>
		public override bool Benefit(PunctuationScheme ps)
		{
			if (attrs == null) return false;
			List<int> NonWildcard = new List<int>();
			//Extract all the non-wildcard patterns from the Punctuation Scheme
			for (int i = 0; i < ps.Count; i++)
			{
				if (!(ps[i] is Punctuation.WildcardPattern))
					NonWildcard.Add(i);
			}
			//Foreach non-wildcard pattern - see if it only affects grouped attributes
			//Go through all the non-wildcard positions
			foreach (int position in NonWildcard)
			{
				bool grouped = false;
				//Go through all the attributes being grouped
				foreach (int attr in attrs)
				{
					//If we found a match - we know that the non-wildcard pattern
					//affects a grouped attribute.
					if (attr == position)
						grouped = true;
				}
				if (!grouped)
					return false;
			}
			//If we made it through the matching process without returning,
			//we know that this punctuation scheme contains wildcard patterns
			//for all non-grouped attributes.
			return true;
		}
	}
	#endregion

	#region Group-by Count
	/// <summary>
    /// Operator to count DataItem objects based on some attributes
    /// </summary>
    public class OpGroupByCount : OpGroupBy
    {
        /// <summary>
        /// Constructor for the query operator Count
        /// </summary>
        /// <param name="attrs">Which attributes to group on</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByCount(int[] attrs, Query opIn)
            : base(attrs, opIn)
        {
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Count with main attribute support
        /// </summary>
        /// <param name="attrs">The attributes to group on</param>
        /// <param name="mainAttrs">The attributes which will recieve punctuation</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByCount(int[] attrs, int[] mainAttrs, Query opIn)
            : base(attrs, mainAttrs, opIn)
        {
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Count with a list attribute
        /// </summary>
        /// <param name="attrs">The attributes to group on</param>
        /// <param name="mainAttrs">The attributes which will recieve punctuation</param>
        /// <param name="listAttr">The attribute which contians a list</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByCount(int[] attrs, int[] mainAttrs, int listAttr, Query opIn)
            : base(attrs, mainAttrs, listAttr, opIn)
        {
            Init();
        }

        private OpGroupByCount(string id, string a, Query opIn)
            : base(id, a, opIn)
        {
            Init();
        }

        private void Init() 
        {
            algorithm = new UnaryAlgorithm(algoHash = new HashAlgorithmCount(attrs, mainAttrs, listAttr));
        }

        /// <summary>
        /// Serialize this group-by operator
        /// </summary>
        /// <param name="tw">Destination for XML data</param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);
            tw.Write(" />\n");
        }

#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step function is called with each data item that arrives.
        /// The Final function is called when EOF is encountered
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool eofInput;

            ldiBufferOut.Clear();
            if (opIn != null)
            {
                SplitPunc(opIn.Iterate(), ref ldiBuffer, ref lpBuffer);
                eofInput = false;

                foreach (DataItem di in ldiBuffer)
                {
                    if (di.EOF == false)
                        ((HashAlgorithmCount)algoHash).GBCOUNTStep(di, ldiBufferOut);
                    eofInput |= di.EOF;
                }
                foreach (Punctuation p in lpBuffer)
                {
                    algoHash.GBPass(p, ldiBufferOut);
                    algoHash.GBKeep(p);
                    algoHash.GBProp(p, ldiBufferOut);
                }
                if (eofInput)
                {
                    algoHash.GBFinal(ldiBufferOut);

                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif

        /// <summary>Specific Count functionality</summary>
        public class HashAlgorithmCount : HashAlgorithm
        {
            private int listAttr;
            /// <summary>Constructor for Group by-Count</summary>
            public HashAlgorithmCount(int[] a, int[] m, int l) : base(a, m) { listAttr = l; }
            /// <summary>Step functionality for Group By-Count</summary>
            public override Step FnStep { get { return GBCOUNTStep; } }

            internal void GBCOUNTStep(DataItem di, List<DataItem> ldi)
            {
                if (listAttr >= 0)
                {
                    List<ulong> lr = (List<ulong>)di[listAttr];
                    foreach (ulong num in lr) //flatten out the list
                    {
                        di[listAttr] = num;
                        Group g;
                        if (FindGroup(di, out g))
                            g.oResult = ((uint)g.oResult) + 1;
                        else
                            g.oResult = (uint)1;
                    }
                }
                else
                {
                    Group g;
                    if (FindGroup(di, out g))
                        g.oResult = ((uint)g.oResult) + 1;
                    else
                        g.oResult = (uint)1;
                }
            }
        }
	}
	#endregion

	#region Group-by Sum
	/// <summary>
    /// Operator to sum attributes of DataItem objects based on some attributes
    /// </summary>
    public class OpGroupBySum : OpGroupBy
    {
        private int val;
        private const string XMLVAL = "VAL";

        /// <summary>
        /// Constructor for the query operator Sum
        /// </summary>
        /// <param name="attrs">Which attributes to group on</param>
        /// <param name="v">Which attribute to sum over</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupBySum(int[] attrs, int v, Query opIn)
            : base(attrs, opIn)
        {
            val = v;
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Sum with main attribute support
        /// </summary>
        /// <param name="attrs">The attributes to group on</param>
        /// <param name="mainAttrs">The attributes which will recieve punctuation</param>
        /// <param name="v">Which attribute to sum over</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupBySum(int[] attrs, int[] mainAttrs, int v, Query opIn)
            : base(attrs, mainAttrs, opIn)
        {
            val = v;
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Sum with main attribute support
        /// </summary>
        /// <param name="attrs">The attributes to group on</param>
        /// <param name="mainAttrs">The attributes which will recieve punctuation</param>
        /// <param name="v">Which attribute to sum over</param>
        /// <param name="listAttr">The attribute which contains a list</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupBySum(int[] attrs, int[] mainAttrs, int v, int listAttr, Query opIn)
            : base(attrs, mainAttrs, listAttr, opIn)
        {
            val = v;
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Sum
        /// </summary>
        /// <param name="id">The ID for this operator</param>
        /// <param name="a">Which attributes to group on</param>
        /// <param name="v">Attribute to sum</param>
        /// <param name="opIn">The input query operator</param>
        /// <remarks> This constructor should only be called through deserialization</remarks>
        public OpGroupBySum(string id, string a, string v, Query opIn)
            : base(id, a, opIn)
        {
            val = Int32.Parse(v.Substring(v.IndexOf('=') + 1));
        }

        private void Init()
        {
            algorithm = new UnaryAlgorithm(new HashAlgorithmSum(attrs, mainAttrs, val, listAttr));
        }

        /// <summary>
        /// Serialize this Sum operator
        /// </summary>
        /// <param name="tw">Destination for XML data</param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);
            tw.Write(string.Format("{0}=\"{1}\"", XMLVAL, val));
            tw.Write(" />\n");
        }

#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step function is called with each data item that arrives.
        /// The Final function is called when EOF is encountered
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool eofInput;

            ldiBufferOut.Clear();
            if (opIn != null)
            {
                SplitPunc(opIn.Iterate(), ref ldiBuffer, ref lpBuffer);
                eofInput = false;

                foreach (DataItem di in ldiBuffer)
                {
                    if (di.EOF == false)
                        ((HashAlgorithmSum)algoHash).GBSUMStep(di, ldiBufferOut);
                    eofInput |= di.EOF;
                }
                foreach (Punctuation p in lpBuffer)
                {
                    algoHash.GBPass(p, ldiBufferOut);
                    algoHash.GBKeep(p);
                    algoHash.GBProp(p, ldiBufferOut);
                }
                if (eofInput)
                {
                    algoHash.GBFinal(ldiBufferOut);

                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif

        /// <summary>Algorithm for grouo-by/sum</summary>
        public class HashAlgorithmSum : HashAlgorithm
        {
            private int val;
            private int listAttr;
            /// <summary>
            /// Constructor for group-by/sum
            /// </summary>
            /// <param name="a">Attributes for grouping</param>
            /// <param name="m">Main attributes for grouping</param>
            /// <param name="v">Initial value for sum</param>
            /// <param name="l">Attribute that contains a list</param>
            public HashAlgorithmSum(int[] a, int[] m, int v, int l) : base(a, m) { val = v; listAttr = l;}
            /// <summary>Step functionality for group-by/sum</summary>
            public override Step FnStep { get { return GBSUMStep; } }

            internal void GBSUMStep(DataItem di, List<DataItem> ldi)
            {
                if (listAttr >= 0)
                {
                    List<ulong> lr = (List<ulong>)di[listAttr];
                    foreach (ulong num in lr) //flatten out the list
                    {
                        di[listAttr] = num;
                        Group g;
                        if (FindGroup(di, out g))
                            g.oResult = (double)g.oResult + Convert.ToDouble(di[val]);
                        else
                            g.oResult = di[val];
                    }
                }
                else
                {
                    Group g;
                    if (FindGroup(di, out g))
                        g.oResult = (double)g.oResult + Convert.ToDouble(di[val]);
                    else
                        g.oResult = di[val];
                }
            }
        }
	}
	#endregion

	#region Group-by Average
	/// <summary>
    /// Operator to average attributes of DataItem objects based on some attributes
    /// </summary>
    public class OpGroupByAvg : OpGroupBy
    {
        /// <summary>
        /// The attribute to keep average over
        /// </summary>
        private int val;
        private const string XMLVAL = "VAL";

        /// <summary>
        /// Specific state for AVG -- track the count and sume for each group
        /// </summary>
        private class State
        {
            /// <summary>
            /// Specific state variables for tracking the average
            /// </summary>
            public double sum, count;
        }

        /// <summary>
        /// Constructor for the query operator Average
        /// </summary>
        /// <param name="attrs">Which attributes to group on</param>
        /// <param name="v">Which attribute to average over</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByAvg(int[] attrs, int v, Query opIn)
            : base(attrs, opIn)
        {
            val = v;
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Average with main attribute support
        /// </summary>
        /// <param name="attrs">Which attributes to group on</param>
        /// <param name="mainAttrs">Which main attributes to group on</param>
        /// <param name="v">Which attribute to average over</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByAvg(int[] attrs, int[] mainAttrs, int v, Query opIn)
            : base(attrs, mainAttrs, opIn)
        {
            val = v;
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Average with main attribute support
        /// </summary>
        /// <param name="attrs">Which attributes to group on</param>
        /// <param name="mainAttrs">Which main attributes to group on</param>
        /// <param name="v">Which attribute to average over</param>
        /// <param name="listAttr">The attribute that contains a list</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByAvg(int[] attrs, int[] mainAttrs, int v, int listAttr, Query opIn)
            : base(attrs, mainAttrs, listAttr, opIn)
        {
            val = v;
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Average
        /// </summary>
        /// <param name="id">ID for this operator</param>
        /// <param name="a">Which attributes to group on</param>
        /// <param name="v">The attribute to average over</param>
        /// <param name="opIn">The input query operator</param>
        /// <remarks>Should only be called through deserialization</remarks> 
        public OpGroupByAvg(string id, string a, string v, Query opIn)
            : base(id, a, opIn)
        {
            val = Int32.Parse(v.Substring(v.IndexOf('=') + 1));
            Init();
        }

        private void Init()
        {
            algorithm = new UnaryAlgorithm(algoHash = new HashAlgorithmAvg(attrs, mainAttrs, val, listAttr));
        }

        /// <summary>
        /// Serialize this average operator
        /// </summary>
        /// <param name="tw">Destination for XML data </param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);
            tw.Write(string.Format("{0}=\"{1}\"", XMLVAL, val));
            tw.Write(" />\n");
        }

#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step function is called with each data item that arrives.
        /// The Final function is called when EOF is encountered
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool eofInput;

            ldiBufferOut.Clear();
            if (opIn != null)
            {
                SplitPunc(opIn.Iterate(), ref ldiBuffer, ref lpBuffer);
                eofInput = false;

                foreach (DataItem di in ldiBuffer)
                {
                    if (di.EOF == false)
                        ((HashAlgorithmAvg)algoHash).GBAVGStep(di, ldiBufferOut);
                    eofInput |= di.EOF;
                }
                foreach (Punctuation p in lpBuffer)
                {
                    algoHash.GBPass(p, ldiBufferOut);
                    algoHash.GBKeep(p);
                    algoHash.GBProp(p, ldiBufferOut);
                }
                if (eofInput)
                {
                    algoHash.GBFinal(ldiBufferOut);

                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif

        /// <summary>
        /// Algorithm for group-by/Average
        /// </summary>
        public class HashAlgorithmAvg : HashAlgorithm
        {
            private int val;
            private int listAttr;
            /// <summary>
            /// Constructor for avergae
            /// </summary>
            /// <param name="a">attributes for grouping</param>
            /// <param name="m">main attributes for grouping</param>
            /// <param name="v">initial valuefor average</param>
            /// <param name="l">attribute that contains a list</param>
            public HashAlgorithmAvg(int[] a, int[] m, int v, int l) : base(a, m) { val = v; listAttr = l; }
            /// <summary>Step functionality for group-by/average</summary>
            public override Step FnStep { get { return GBAVGStep; } }

            internal void GBAVGStep(DataItem di, List<DataItem> ldi)
            {
                if (listAttr >= 0)
                {
                    List<ulong> lr = (List<ulong>)di[listAttr];
                    foreach (ulong num in lr) //flatten out the list
                    {
                        di[listAttr] = num;
                        Group g;
                        if (FindGroup(di, out g))
                        {
                            ((State)g.oState).count = ((State)g.oState).count + 1;
                            ((State)g.oState).sum = ((State)g.oState).sum + Convert.ToDouble(di[val]);
                        }
                        else
                        {
                            g.oState = new State();
                            ((State)g.oState).count = 1;
                            ((State)g.oState).sum = Convert.ToDouble(di[val]);
                        }
                        g.oResult = ((State)g.oState).sum / ((State)g.oState).count;
                    }
                }
                else
                {
                    Group g;
                    if (FindGroup(di, out g))
                    {
                        ((State)g.oState).count = ((State)g.oState).count + 1;
                        ((State)g.oState).sum = ((State)g.oState).sum + Convert.ToDouble(di[val]);
                    }
                    else
                    {
                        g.oState = new State();
                        ((State)g.oState).count = 1;
                        ((State)g.oState).sum = Convert.ToDouble(di[val]);
                    }
                    g.oResult = ((State)g.oState).sum / ((State)g.oState).count;
                }
            }
        }
	}
	#endregion

	#region Group-by Maximum
	/// <summary>
    /// Operator to find the max of DataItem objects based on some attributes
    /// </summary>
    public class OpGroupByMax : OpGroupBy
    {
        /// <summary>
        /// Which attribute to keep max over
        /// </summary>
        private int val;
        private const string XMLVAL = "VAL";

        /// <summary>
        /// Constructor for the query operator Max
        /// </summary>
        /// <param name="attrs">Which attributes to group on</param>
        /// <param name="v">Which attribute to keep max over</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByMax(int[] attrs, int v, Query opIn)
            : base(attrs, opIn)
        {
            val = v;
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Max with main attribute support
        /// </summary>
        /// <param name="attrs">The attributes to group on</param>
        /// <param name="mainAttrs">The attributes which will recieve punctuation</param>
        /// <param name="v">The attribute to max over</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByMax(int[] attrs, int[] mainAttrs, int v, Query opIn)
            : base(attrs, mainAttrs, opIn)
        {
            val = v;
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Max with main attribute support
        /// </summary>
        /// <param name="attrs">The attributes to group on</param>
        /// <param name="mainAttrs">The attributes which will recieve punctuation</param>
        /// <param name="v">The attribute to max over</param>
        /// <param name="listAttr">The attribute that contains a list</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByMax(int[] attrs, int[] mainAttrs, int v, int listAttr, Query opIn)
            : base(attrs, mainAttrs, listAttr, opIn)
        {
            val = v;
            Init();
        }

        private void Init()
        {
            algorithm = new UnaryAlgorithm(new HashAlgorithmMax(attrs, mainAttrs, val, listAttr));
        }

        /// <summary>
        /// Constructor for the query operator Max
        /// </summary>
        /// <param name="id">ID for this operator</param>
        /// <param name="a">Which attributes to group on</param>
        /// <param name="v">Attribute to find the max of</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByMax(string id, string a, string v, Query opIn)
            : base(id, a, opIn)
        {
            val = Int32.Parse(v.Substring(v.IndexOf('=') + 1));
            Init();
        }

        /// <summary> Serialize this Max operator </summary>
        /// <param name="tw">Destination for the XML data</param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);
            tw.Write(string.Format("{0}=\"{1}\"", XMLVAL, val));
            tw.Write(" />\n");
        }

#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step function is called with each data item that arrives.
        /// The Final function is called when EOF is encountered
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool eofInput;

            ldiBufferOut.Clear();
            if (opIn != null)
            {
                SplitPunc(opIn.Iterate(), ref ldiBuffer, ref lpBuffer);
                eofInput = false;

                foreach (DataItem di in ldiBuffer)
                {
                    if (di.EOF == false)
                        ((HashAlgorithmMax)algoHash).GBMAXStep(di, ldiBufferOut);
                    eofInput |= di.EOF;
                }
                foreach (Punctuation p in lpBuffer)
                {
                    algoHash.GBPass(p, ldiBufferOut);
                    algoHash.GBKeep(p);
                    algoHash.GBProp(p, ldiBufferOut);
                }
                if (eofInput)
                {
                    algoHash.GBFinal(ldiBufferOut);

                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif

        /// <summary>
        /// Algorithm for group-by/max
        /// </summary>
        public class HashAlgorithmMax : HashAlgorithm
        {
            private int val;
            private int listAttr;
            /// <summary>
            /// Constructor for group-by/max
            /// </summary>
            /// <param name="a">attributes for grouping</param>
            /// <param name="m">main attributes for grouping</param>
            /// <param name="v">initial value</param>
            /// <param name="l">attribute that contains a list</param>
            public HashAlgorithmMax(int[] a, int[] m, int v, int l) : base(a, m) { val = v; listAttr = l; }
            /// <summary>Step functionality for group-by/max</summary>
            public override Step FnStep { get { return GBMAXStep; } }
            
            internal void GBMAXStep(DataItem di, List<DataItem> ldi)
            {
                if (listAttr >= 0)
                {
                    List<ulong> lr = (List<ulong>)di[listAttr];
                    foreach (ulong num in lr) //flatten out the list
                    {
                        di[listAttr] = num;
                        Group g;
                        if (FindGroup(di, out g))
                        {
                            //Unhandled Exception will occur if the value is not comparable
                            IComparable c = (IComparable)g.oResult;
                            if (c.CompareTo(di[val]) < 0)
                                g.oResult = di[val];
                        }
                        else
                            g.oResult = di[val];
                    }
                }
                else
                {
                    Group g;
                    if (FindGroup(di, out g))
                    {
                        //Unhandled Exception will occur if the value is not comparable
                        IComparable c = (IComparable)g.oResult;
                        if (c.CompareTo(di[val]) < 0)
                            g.oResult = di[val];
                    }
                    else
                        g.oResult = di[val];
                }
            }
        }
	}
	#endregion

	#region Group-by Minimum
	/// <summary>
    /// Operator to find the minimum of DataItem objects based on some attributes
    /// </summary>
    public class OpGroupByMin : OpGroupBy
    {
        /// <summary>
        /// Which attribute to keep minimum over
        /// </summary>
        private int val;
        private const string XMLVAL = "VAL";

        /// <summary>
        /// Constructor for the query operator Min
        /// </summary>
        /// <param name="attrs">Which attributes to group on</param>
        /// <param name="v">Which attribute to min over</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByMin(int[] attrs, int v, Query opIn)
            : base(attrs, opIn)
        {
            val = v;
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Min with main attribute support
        /// </summary>
        /// <param name="attrs">The attributes to group on</param>
        /// <param name="mainAttrs">The attributes which will recieve punctuation</param>
        /// <param name="v">The attribute to keep min over</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByMin(int[] attrs, int[] mainAttrs, int v, Query opIn)
            : base(attrs, mainAttrs, opIn)
        {
            val = v;
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Min with main attribute support
        /// </summary>
        /// <param name="attrs">The attributes to group on</param>
        /// <param name="mainAttrs">The attributes which will recieve punctuation</param>
        /// <param name="v">The attribute to keep min over</param>
        /// <param name="listAttr">The attribute which contains a list</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByMin(int[] attrs, int[] mainAttrs, int v, int listAttr, Query opIn)
            : base(attrs, mainAttrs, listAttr, opIn)
        {
            val = v;
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Min
        /// </summary>
        /// <param name="id">ID for this operator</param>
        /// <param name="a">Which attributes to group on</param>
        /// <param name="v">Attribute to find the min of</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByMin(string id, string a, string v, Query opIn)
            : base(id, a, opIn)
        {
            val = Int32.Parse(v.Substring(v.IndexOf('=') + 1));
            Init();
        }

        private void Init()
        {
            algorithm = new UnaryAlgorithm(new HashAlgorithmMin(attrs, mainAttrs, val, listAttr));
        }

        /// <summary> Serialize this operator </summary>
        /// <param name="tw">Destination for the XML data</param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);
            tw.Write(string.Format("{0}=\"{1}\"", XMLVAL, val));
            tw.Write(" />\n");
        }

#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step function is called with each data item that arrives.
        /// The Final function is called when EOF is encountered
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool eofInput;

            ldiBufferOut.Clear();
            if (opIn != null)
            {
                SplitPunc(opIn.Iterate(), ref ldiBuffer, ref lpBuffer);
                eofInput = false;

                foreach (DataItem di in ldiBuffer)
                {
                    if (di.EOF == false)
                        ((HashAlgorithmMin)algoHash).GBMINStep(di, ldiBufferOut);
                    eofInput |= di.EOF;
                }
                foreach (Punctuation p in lpBuffer)
                {
                    algoHash.GBPass(p, ldiBufferOut);
                    algoHash.GBKeep(p);
                    algoHash.GBProp(p, ldiBufferOut);
                }
                if (eofInput)
                {
                    algoHash.GBFinal(ldiBufferOut);

                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif

        /// <summary>
        /// Algorithm for group-by/min
        /// </summary>
        public class HashAlgorithmMin : HashAlgorithm
        {
            private int val;
            private int listAttr;
            /// <summary>
            /// Constructor for group-by/min
            /// </summary>
            /// <param name="a">attributes for grouping</param>
            /// <param name="m">main attribute for grouping</param>
            /// <param name="v">intial value</param>
            /// <param name="l">attribute with a list</param>
            public HashAlgorithmMin(int[] a, int[] m, int v, int l) : base(a, m) { val = v; listAttr = l; }
            /// <summary>Step functionality for group-by/min</summary>
            public override Step FnStep { get { return GBMINStep; } }

            internal void GBMINStep(DataItem di, List<DataItem> ldi)
            {
                if (listAttr >= 0)
                {
                    List<ulong> lr = (List<ulong>)di[listAttr];
                    foreach (ulong num in lr) //flatten out the list
                    {
                        di[listAttr] = num;
                        Group g;
                        if (FindGroup(di, out g))
                        {
                            //Unhandled Exception will occur if the value is not comparable
                            IComparable c = (IComparable)g.oResult;
                            if (c.CompareTo(di[val]) > 0)
                                g.oResult = di[val];
                        }
                        else
                            g.oResult = di[val];
                    }
                }
                else
                {
                    Group g;
                    if (FindGroup(di, out g))
                    {
                        //Unhandled Exception will occur if the value is not comparable
                        IComparable c = (IComparable)g.oResult;
                        if (c.CompareTo(di[val]) > 0)
                            g.oResult = di[val];
                    }
                    else
                        g.oResult = di[val];
                }
            }
        }
	}
	#endregion
	#endregion //Group-by

	#region Sort
	/// <summary>
    /// Models the query operator Sort
    /// </summary>
    public class OpSort : UnaryOp
    {
        private UnaryAlgorithmDefinition algoSort;

        /// <summary>
        /// Constructor for the query operator Sort
        /// </summary>
        /// <param name="cmp">Object to determine how to compare two DataItem objects</param>
        /// <param name="a">The sorting attributes</param>
        /// <param name="opIn">The input query operator</param>
        public OpSort(IComparer<DataItem> cmp, int[] a, Query opIn)
            : base(opIn)
        {
            //TODO: Need to fix this to ensure the punctuation describes
            // a prefix of the sorted output, and not just describes the
            // sort attributes
            Init(cmp, a);
        }

        /// <summary>String containing state for this operator</summary>
        /// <returns>String representation of this operator, including state information</returns>
        public override string ToString()
        {
            return string.Format("{0} - Sort({1})", base.ToString(), algoSort.StateSize);
        }

        /// <summary> Serialize this operator </summary>
        /// <param name="tw">Destination for the XML data</param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);

            //TODO: Can't persist the sorting function
            tw.Write("ATTRS=\"");
            tw.Write("\" />\n");
        }

        private void Init(IComparer<DataItem> c, int[] a)
        {
            algoSort = new SortedInput(c, a);
            algorithm = new UnaryAlgorithm(algoSort);
        }

        /// <summary>
        /// Algorithm for the Sort operator
        /// </summary>
        public class SortedInput : UnaryAlgorithmDefinition
        {
            private List<DataItem> al = new List<DataItem>();
            private IComparer<DataItem> cmpData;
            private int[] attrs;

            /// <summary>
            /// Constructor for sorted-input algorithm
            /// </summary>
            /// <param name="c">function to execute comparison</param>
            /// <param name="a">attributes to sort</param>
            public SortedInput(IComparer<DataItem> c, int[] a)
            {
                cmpData = c;
                attrs = a;
            }

            /// <summary> How many data items are curently held in state </summary>
            public override int StateSize { get { return al.Count; } }

            /// <summary>Step functionality for orderby</summary>
            public override Step FnStep { get { return SStep; } }
            /// <summary>Pass functionality for orderby</summary>
            public override Pass FnPass { get { return SPass; } }
            /// <summary>Keep functionality for orderby</summary>
            public override Keep FnKeep { get { return SKeep; } }
            /// <summary>Prop functionality for orderby</summary>
            public override Prop FnProp { get { return SProp; } }
            /// <summary>Final functionality for orderby</summary>
            public override Final FnFinal { get { return SFinal; } }
            
            private void SStep(DataItem di, List<DataItem> ldi)
            {
                al.Add(di);
            }

            private void SFinal(List<DataItem> ldi)
            {
                al.Sort(cmpData);
                ldi.AddRange(al);
                al.Clear();
            }

            private void SPass(Punctuation p, List<DataItem> ldi)
            {
                //TODO: Need to fix this to ensure the punctuation describes
                // a prefix of the sorted output, and not just describes the
                // sort attributes
                if (p.Describes(attrs))
                {
                    al.Sort(cmpData);
                    for (int i = 0; i < al.Count && p.Match(al[i]); i++)
                        ldi.Add(al[i]);
                }
            }

            private void SKeep(Punctuation p)
            {
                //TODO: Need to fix this to ensure the punctuation describes
                // a prefix of the sorted output, and not just describes the
                // sort attributes
                if (p.Describes(attrs))
                {
                    //Should we still have to do this, since we called SPass first?
                    al.Sort(cmpData);
                    int c;
                    for (c = 0; c < al.Count && p.Match(al[c]); c++) ;

                    al.RemoveRange(0, c);
                }
            }

            private void SProp(Punctuation p, List<DataItem> ldi)
            {
                //TODO: Need to fix this to ensure the punctuation describes
                // a prefix of the sorted output, and not just describes the
                // sort attributes
                if (p.Describes(attrs))
                    ldi.Add(p);
            }
        }
	}
	#endregion

    #region Database Access

    /// <summary>
    /// Statement a connected database and change the DataItems in some way
    /// </summary>
//    public class OpDBAccess : UnaryOp
//    {
//        MySqlDatabase db;
//        string dbquery, dbstatement, database;
//        private DBAlgorithm algoDB;

//        /// <summary>
//        /// The constructor for a DBStatement operator, given a database and a Statement to run
//        /// </summary>
//        /// <param name="database">The database name to connect to</param>
//        /// <param name="dbquery">A query to run over the database for each data item</param>
//        /// <param name="dbstatement">A command to execute for every data item</param>
//        /// <param name="opIn">The input query operator</param>
//        /// <remarks>The statements should be in the form of a SQL Statement, with any attributes
//        /// of the data item to values in the form $#$, where # is the attribute number </remarks>
//        public OpDBAccess(string database, string dbquery, string dbstatement, Query opIn)
//            : base(opIn)
//        {
//            db = new MySqlDatabase(database);
//            this.database = database;
//            this.dbquery = dbquery;
//            this.dbstatement = dbstatement;
//            algorithm = new UnaryAlgorithm(algoDB = new DBAlgorithm(dbquery, dbstatement, db));
//        }

//        /// <summary> Output stats for this operator </summary>
//        /// <returns>String with operator-specific stats</returns>
//        public override string ToString()
//        {
//            return string.Format("{0} - DatabaseAccess<{1}>", base.ToString(), database);
//        }

//        public override List<DataItem> Iterate(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
//        {
//            bool eofInput = false;
//            ldiBufferOut.Clear();
//            if (opIn != null)
//            {
//                List<DataItem> ldiIn = opIn.Iterate(gdi, rdi);
//                itemsIn += ldiIn.Count;
//                Step step = algorithm.FnStep;

//                db.StartTransaction();
//                foreach (DataItem di in ldiIn)
//                {
//                    if (!(di is Punctuation))
//                    {
//                        if (!di.EOF)
//                            step(di, ldiBufferOut);
//                        eofInput |= di.EOF;
//                    }
//                    else
//                    {
//                        Punctuation p = di as Punctuation;
//                        algorithm.FnProp(p, ldiBufferOut);
//                    }
//                }
//                db.CommitTransaction();

//                if (eofInput)
//                {
//                    ldiBufferOut.Add(diEOF);
//                    eof = true;
//                }
//            }
//            itemsOut += ldiBufferOut.Count;
//            return ldiBufferOut;
//        }

//#if NODELEGATE
//        /// <summary>
//        /// Iterate function to work through data items.
//        /// The Step function is called with each data item that arrives.
//        /// The Final function is called when EOF is encountered
//        /// </summary>
//        /// <returns>DataItem objects that can be output from this iteration</returns>
//        /// <seealso cref="QueryEngine.Step"/>
//        /// <seealso cref="QueryEngine.Final"/>
//        public override List<DataItem> Iterate()
//        {
//            bool eofInput;

//            ldiBufferOut.Clear();
//            if (opIn != null)
//            {
//                SplitPunc(opIn.Iterate(), ref ldiBuffer, ref lpBuffer);
//                algoDB.DBStepList(ldiBuffer, ref ldiBufferOut, out eofInput);
//                foreach (Punctuation p in lpBuffer)
//                {
//                    algoDB.DBProp(p, ldiBufferOut);
//                }
//                if (eofInput)
//                {
//                    ldiBufferOut.Add(diEOF);
//                    eof = true;
//                }
//            }
//            return ldiBufferOut;
//        }
//#endif

//        /// <summary>
//        /// DBAlgorithm -- Adds the result of the query the end of the data item, then runs the statement (if they aren't null) 
//        /// </summary>
//        public class DBAlgorithm : UnaryAlgorithmDefinition
//        {
//            string dbquery, dbstatement;
//            MySqlDatabase datab;

//            /// <summary>
//            /// Naive implementation of the DBStatement algorithm
//            /// </summary>
//            /// <param name="dbquery">The query to run for each data item</param>
//            /// <param name="dbstatement">The statement to run after running the query</param>
//            /// <param name="db">Database to use</param>
//            /// <remarks>If either command is not to be run, set the relevant string to null</remarks>
//            public DBAlgorithm(string dbquery, string dbstatement, MySqlDatabase db)
//            {
//                this.dbstatement = dbstatement;
//                this.dbquery = dbquery;
//                datab = db;
//            }

//            private string ParseStatement(string rawStatement, DataItem di)
//            {
//                StringBuilder finalStatement = new StringBuilder();
//                for (int i = 0; i < rawStatement.Length; i++)
//                {
//                    if (rawStatement[i] != '$')
//                        finalStatement.Append(rawStatement[i]);
//                    else
//                    {
//                        int j = ++i;
//                        while (rawStatement[j] != '$')
//                            j++;

//                        int index = Int32.Parse(rawStatement.Substring(i, (j - i))); //get the value of the index
//                        finalStatement.Append(di.GetValue(index)); //get the value at the index and append it
//                        i = j;
//                    }
//                }
//                return finalStatement.ToString();
//            }

//            /// <summary>Step functionality for DBStatement</summary>
//            public override Step FnStep { get { return DBStep; } }
//            /// <summary>StepList functionality for DBStatement</summary>
//            public override StepList FnStepList { get { return DBStepList; } }
//            /// <summary>Prop functionality for DBStatement</summary>
//            public override Prop FnProp { get { return DBProp; } }

//            internal void DBStep(DataItem di, List<DataItem> ldi)
//            {
//                if (dbquery != null)
//                    ldi.Add(datab.UpdateDI(di, ParseStatement(dbquery, di)));
//                else
//                    ldi.Add(di);

//                if (dbstatement != null)
//                    datab.RunCommand(ParseStatement(dbstatement, di));
//            }

//            internal void DBStepList(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
//            {
//                eofInput = false;
//                foreach (DataItem di in rgdi)
//                {
//                    if (di.EOF == false)
//                    {
//                        DBStep(di, ldi);
//                    }
//                    eofInput |= di.EOF;
//                }
//            }

//            internal void DBProp(Punctuation p, List<DataItem> ldi)
//            {
//                if (dbquery != null)
//                {
//                    p.AddCapacity(1);
//                    p.AddValue(new Punctuation.WildcardPattern()); //no way to punctuate on the new item
//                    ldi.Add(p);
//                }
//                else
//                    ldi.Add(p);
//            }
//        }
//    }
    #endregion

    #region Relation
    /// <summary>
    /// Operator to perform a delegate method on a data item
    /// </summary>
    public class OpRelation : UnaryOp
    {
        RelationAlgorithm relalgo;

        /// <summary>
        /// Cunstructor for a Relation operator with a relation table
        /// </summary>
        /// <param name="diRel">The relation to perform on data items</param>
        /// <param name="relTable">The table to use</param>
        /// <param name="OpIn">The input query operator</param>
        public OpRelation(Relation diRel, RelationTable relTable, Query OpIn)
            : base(OpIn)
        {
            algorithm = new UnaryAlgorithm(relalgo = new RelationAlgorithm(diRel, relTable));
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - Relation<{1}>", base.ToString(), relalgo.StateSize);
        }
        
#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step function is called with each data item that arrives.
        /// The Final function is called when EOF is encountered
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool eofInput;

            ldiBufferOut.Clear();
            if (opIn != null)
            {
                SplitPunc(opIn.Iterate(), ref ldiBuffer, ref lpBuffer);
                relalgo.RelStepList(ldiBuffer, ref ldiBufferOut, out eofInput);
                foreach (Punctuation p in lpBuffer)
                {
                    relalgo.RelProp(p, ldiBufferOut);
                }
                if (eofInput)
                {
                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif

        /// <summary>
        /// Data item maniputlation with access to a table
        /// </summary>
        public class RelationAlgorithm : UnaryAlgorithmDefinition
        {
            Relation mRel;
            RelationTable rt;

            /// <summary>
            /// Constructor for a relation algorithm
            /// </summary>
            /// <param name="rel">Relation for data items</param>
            /// <param name="relTable">The table to use</param>
            public RelationAlgorithm(Relation rel, RelationTable relTable)
            {
                mRel = rel;
                rt = relTable;
            }
            /// <summary>Step functionality for DelegateOp (relation)</summary>
            public override Step FnStep { get { return RelStep; } }
            /// <summary>StepList functionality for DelegateOp (relation)</summary>
            public override StepList FnStepList { get { return RelStepList; } }
            /// <summary>Prop functionality for DelegateOp (relation)</summary>
            public override Prop FnProp { get { return RelProp; } }

            internal void RelStep(DataItem di, List<DataItem> ldi)
            {
                ldi.Add(mRel(di, rt));
            }

            internal void RelStepList(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;

                foreach (DataItem di in rgdi)
                {
                    if (di.EOF == false)
                        ldi.Add(mRel(di, rt));
                    eofInput |= di.EOF;
                }
            }

            internal void RelProp(Punctuation p, List<DataItem> ldi)
            {
                ldi.Add(mRel(p, rt));
            }
            /// <summary>How many items are currently held in state</summary>
            public override int StateSize
            {
                get { return rt.Rows; }
            }
        }
    }
    #endregion

    #region Bucket

    /// <summary>
    /// Bucket operator for attaching window ids (WIDs) to data items
    /// </summary>
    public class OpBucket : UnaryOp
    {
        private Naive algoNaive;

        /// <summary>
        /// Base constructor for a Bucket operator
        /// </summary>
        /// <param name="range">The range of the window</param>
        /// <param name="slide">The slide of the window</param>
        /// <param name="windowAttr">The attribute to window over</param>
        /// <param name="partitionAttr">The attribute to partition over</param>
        /// <param name="opIn">The input query operator</param>
        /// <remarks>Put a -1 in for windowAttr if it is tuple based</remarks>
        /// <remarks>Partitioning is only available for tuple based windowing, use -1 to indicate no partitioning</remarks>
        public OpBucket(int range, int slide, int windowAttr, int partitionAttr, Query opIn): base(opIn)
        {
            if (windowAttr < 0 && windowAttr != -1)
                throw new System.Exception("Windowing attribute for bucket operator is illegal");
            if (partitionAttr < 0 && partitionAttr != -1)
                throw new System.Exception("Partitioning attribute for bucket operator is illegal");

            algorithm = new UnaryAlgorithm(algoNaive = new Naive(range, slide, windowAttr, partitionAttr));
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - Bucket<{1}>", base.ToString(), algoNaive.StateSize);
        }


#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step function is called with each data item that arrives.
        /// The Final function is called when EOF is encountered
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool eofInput;

            ldiBufferOut.Clear();
            if (opIn != null)
            {
                SplitPunc(opIn.Iterate(), ref ldiBuffer, ref lpBuffer);
                algoNaive.BuckStepList(ldiBuffer, ref ldiBufferOut, out eofInput);
                foreach (Punctuation p in lpBuffer)
                {
                    algoNaive.BuckProp(p, ldiBufferOut);
                }
                if (eofInput)
                {
                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif

        /// <summary>
        /// Naive - assumes tuples are in order
        /// </summary>
        public class Naive : UnaryAlgorithmDefinition
        {
            int Wrange, Wslide, Wattr, Pattr;
            ulong tuplesPast = 0, //tupleID, basically
                  lastfirstWID = 0;

            /// <summary>Maintains the state for partitioned window groups</summary>
            protected class Group
            {
                /// <summary>What is this group's hashcode?</summary>
                public int hashcode;
                /// <summary>How many data items have passed through this group?</summary>
                public int tuplesPast;
                /// <summary>What was the first WID for the last data item in this group?</summary>
                public ulong lastfirstWID;
            }
            private Dictionary<int, Group> ht = new Dictionary<int, Group>();

            /// <summary>What is the size of this operator's state?</summary>
            public override int StateSize
            {
                get { return ht.Count; }
            }

            /// <summary>
            /// Constructor for the Naive Bucket operator algorithm
            /// </summary>
            /// <param name="range">The range of the window</param>
            /// <param name="slide">The slide of the window</param>
            /// <param name="windowAttr">The attribute to window over</param>
            /// <param name="partitionAttr">The attribute to partition over</param>
            public Naive(int range, int slide, int windowAttr, int partitionAttr)
            {
                Wrange = range;
                Wslide = slide;
                Wattr = windowAttr;
                Pattr = partitionAttr;
            }

            /// <summary>Step functionality for Bucket</summary>
            public override Step FnStep { get { return BuckStep; } }
            /// <summary>StepList functionality for Bucket</summary>
            public override StepList FnStepList { get { return BuckStepList; } }
            /// <summary>Prop functionality for Bucket</summary>
            public override Prop FnProp { get { return BuckProp; } }

            internal void BuckStep(DataItem di, List<DataItem> ldi)
            {
                ulong WID = 0, firstWID = 0;
                List<ulong> liWID = new List<ulong>();
                if (Wattr != -1) //we're not doing a tuple defined window
                {
                    for (int i = 1; i * Wslide <= Wrange; i++) //for each slide in the range
                    {
                        ulong input = Convert.ToUInt64(di[Wattr]);
                        WID = input / (ulong)Wslide + (ulong)i - 1;
                        liWID.Add(WID);
                    }
                }
                else //the window slides by a number of tuples
                {
                    if (Pattr == -1) //There is no partitioning
                    {
                        for (int i = 1; i * Wslide <= Wrange; i++)
                        {
                            WID = tuplesPast / (ulong)Wslide + (ulong)i - 1;
                            if (i == 1)
                                firstWID = WID;
                            liWID.Add(WID);
                        }
                        tuplesPast++;

                        if (lastfirstWID < firstWID) //we've completed a window
                        {
                            Punctuation p = new Punctuation(di.Count + 2);
                            for (int i = 0; i < di.Count; i++)
                                p.AddValue(new Punctuation.WildcardPattern());

                            p.AddValue(new Punctuation.LiteralPattern(lastfirstWID));
                            ldi.Add(p);
                            lastfirstWID = firstWID;
                        }
                    }
                    else //we must partition the tuples according to some attribute
                    {
                        Group g;
                        if (FindGroup(di[Pattr].GetHashCode(), out g))
                        {
                            for (int i = 1; i * Wslide <= Wrange; i++)
                            {
                                WID = (ulong)(g.tuplesPast / Wslide + i - 1);
                                if (i == 1)
                                    firstWID = WID;
                                liWID.Add(WID);
                            }
                            g.tuplesPast++;

                            if (g.lastfirstWID < firstWID) //we've completed a window for this partition
                            {
                                Punctuation p = new Punctuation(di.Count + 2);
                                for (int i = 0; i < di.Count; i++)
                                {
                                    if (i != Pattr)
                                        p.AddValue(new Punctuation.WildcardPattern());
                                    else
                                        p.AddValue(new Punctuation.LiteralPattern(di[i]));
                                }
                                p.AddValue(new Punctuation.LiteralPattern(g.lastfirstWID));
                                ldi.Add(p);
                                g.lastfirstWID = firstWID;
                            }
                        }
                        else //new group
                        {
                            for (int i = 1; i * Wslide <= Wrange; i++)
                            {
                                WID = (ulong)(i - 1);
                                if (i == 1)
                                    firstWID = WID;
                                liWID.Add(WID);
                            }
                            g.tuplesPast = 1;
                            g.lastfirstWID = 0;
                        }
                    }
                }
                di.AddCapacity(1);
                di.AddValue(liWID);
                ldi.Add(di);
            }

            internal void BuckStepList(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;

                foreach (DataItem di in rgdi)
                {
                    if (di.EOF == false)
                    {
                        BuckStep(di, ldi);
                    }
                    eofInput |= di.EOF;
                }
            }

            internal void BuckProp(Punctuation p, List<DataItem> ldi)
            {
                if (Wattr != -1)
                {
                    if (p[Wattr] is Punctuation.LiteralPattern)
                    {
                        ulong input = Convert.ToUInt64(((Punctuation.LiteralPattern)p[Wattr]).Value);
                        ulong WID = input / (ulong)Wslide;
                        ulong nextWID = (input + 1) / (ulong)Wslide; //the next WID

                        if (WID < nextWID)//this window was finished
                        {
                            Punctuation outP = new Punctuation(p.Count + 2);
                            for (int i = 0; i < p.Count; i++)
                                outP.AddValue(new Punctuation.WildcardPattern());

                            outP.AddValue(new Punctuation.LiteralPattern(WID));
                            ldi.Add(outP);
                        }
                    }
                }
                else //we'll add our own punctuations, but we need to account for End of Pattr punctuations
                {
                    if (Pattr != -1 && p[Pattr] is Punctuation.LiteralPattern) //we have some value for Pattr
                    {
                        Group g;
                        int key = ((Punctuation.LiteralPattern)p[Pattr]).Value.GetHashCode();
                        if (FindGroup(key, out g))
                        {
                            for (int i = 1; i * Wslide <= Wrange; i++) //for all windows that the last tuple belonged to
                            {
                                Punctuation pOut = new Punctuation(p, 1);
                                pOut.AddValue(new Punctuation.LiteralPattern(g.lastfirstWID + (ulong)i - 1));
                                ldi.Add(pOut);
                            }
                            ht.Remove(key); //clean up
                        }
                    }
                }
            }

            private bool FindGroup(int key, out Group g)
            {
                if (!ht.ContainsKey(key))
                {
                    g = new Group();
                    g.hashcode = key;
                    ht[key] = g;
                    return false;
                }
                else
                {
                    g = ht[key];
                    return true;
                }
            }
        }
    }

    #endregion

    #region Multiplex

    /// <summary>
    /// Class to publish data items to multiple sources
    /// </summary>
    /// <remarks>Call the GetQuery function to get a Query that will return the results from the operator
    /// rather than using OpMulitplexer itself</remarks>
    public class OpMultiplexer : UnaryOp
    {
        List<DummyQuery> ldq = new List<DummyQuery>();
        private const int MAXDATA = 500;

        /// <summary>
        /// Constructor to create a multiplexing operator
        /// </summary>
        /// <param name="op">The input Query operator</param>
        public OpMultiplexer(Query op) : base(op)
        { }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - Multiplex", base.ToString());
        }

        /// <summary>
        /// Get a query which returns data items from the Multiplexer's source
        /// </summary>
        /// <returns></returns>
        public Query GetQuery()
        {
            DummyQuery dq = new DummyQuery();
            ldq.Add(dq);
            itemsIn = 1;
            itemsOut = ldq.Count;
            return dq;
        }

        /// <summary>
        /// Get more data items from the source and add it to all internal buffers
        /// </summary>
        public override List<DataItem> Iterate(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
        {
            if (opIn != null)
            {
                List<DataItem> ldiIn = opIn.Iterate(gdi, rdi);
                if (ldiIn.Count > 0)
                {
                    for (int dq = 1; dq < ldq.Count; dq++) //we need to copy ldi to each query's buffer
                    {
                        DummyQuery q = ldq[dq];
                        DataItem[] rgdi = gdi(ldiIn.Count);
                        for (int i = 0; i < ldiIn.Count; i++)
                        {
                            if (!(ldiIn[i] is Punctuation))
                            {
                                for (int j = 0; j < ldiIn[i].Count; j++)
                                    rgdi[i].AddValue(ldiIn[i][j]); //= new DataItem(di, 0, rdi);
                                rgdi[i].TimeStamp = ldiIn[i].TimeStamp;
                                rgdi[i].EOF = ldiIn[i].EOF;
                            }
                            else
                                rgdi[i] = new Punctuation(ldiIn[i] as Punctuation, 0);
                        }
                        q.AddBuffer(rgdi);
                    }
                    ldq[0].AddBuffer(ldiIn.ToArray()); //the first one can get the original
                }
            }
            return ldiBufferOut; //is always empty
        }

        private class DummyQuery : UnaryOp
        {
            public DummyQuery() { itemsIn = 1; itemsOut = 1; }

            public void AddBuffer(DataItem[] rgdi)
            {
                DataItem di;
                for (int i = 0; i < rgdi.Length; i++)
                {
                    di = rgdi[i];
                    if (di != null)
                    {
                        ldiBuffer.Add(di);
                        eof |= di.EOF;
                    }
                }
            }
            /// <summary> Output stats for this operator </summary>
            /// <returns>String with operator-specific stats</returns>
            public override string ToString()
            {
                return string.Format("{0} - MultiplexLeaf<{1}>", base.ToString(), ldiBuffer.Count);
            }
            /// <summary>
            /// Finds the number of tuples ready from this operator
            /// </summary>
            /// <returns>The number of items ready to be processed</returns>
            public override int GetItemsReady()
            {
                return ldiBuffer.Count;
            }
            /// <summary>
            /// Iterate function to work through data items.
            /// </summary>
            /// <returns>DataItem objects that can be output from this iteration</returns>
            public override List<DataItem> Iterate(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
            {
                ldiBufferOut.Clear();
                lock (ldiBuffer)
                {
                    if (ldiBuffer.Count > 0)
                    {
                        for (int i = 0; i < ldiBuffer.Count; i++)
                            ldiBufferOut.Add(ldiBuffer[i]);
                        ldiBuffer.Clear();
                    }
                }
                return ldiBufferOut;
            }
        }
    }
    #endregion
}