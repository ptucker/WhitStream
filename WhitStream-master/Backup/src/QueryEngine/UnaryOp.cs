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
using WhitStream.Data;
using WhitStream.Expression;
using WhitStream.Utility;

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
                        Punctuation.LiteralPattern[] rglp = new Punctuation.LiteralPattern[CPUNCTLISTSIZE];
                        for (int iLit = 0; iLit < CPUNCTLISTSIZE; iLit++)
                            rglp[iLit] = new Punctuation.LiteralPattern((int)iRow - CPUNCTLISTSIZE + iLit);
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
        private static WhitStream.Server.TCPServer server = new WhitStream.Server.TCPServer();
        NetworkStream nsClient;
        private delegate void StreamReader();
        private const int MAXDATA = 500;

        /// <summary> Default constructor to set up this operator as a listener </summary>
        public OpServer()
        {
            server.AddListener(SetClient);
        }

        /// <summary>
        /// Constructor set this operator up as a listener on a specific port
        /// </summary>
        /// <param name="port">The port to listen on</param>
        public OpServer(int port)
        {
            server.AddListener(SetClient, port);
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - Server", base.ToString());
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
            const int STREAMDATABUFFER = 100;
            //DataItem di = null;
            bool eof = false;
            BinaryFormatter bf = new BinaryFormatter();
            DataItem[] rgdi = new DataItem[STREAMDATABUFFER];
            int iData = 0;

            do
            {
                while (iData < STREAMDATABUFFER && nsClient.DataAvailable && !eof)
                {
                    try
                    {
                        rgdi[iData] = (DataItem)bf.Deserialize(nsClient);
                        eof = rgdi[iData].EOF;
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
                    iData++;
                }
                lock (queBuffer)
                {
                    for (int i = 0; i < iData; i++)
                    {
                        queBuffer.Enqueue(rgdi[i]);
                        rgdi[i] = null;
                    }
                    iData = 0;
                }
            } while (!eof);

            nsClient.Close();

            if (rgdi.Length == 0 || (rgdi[rgdi.Length-1] != null && !rgdi[rgdi.Length-1].EOF))
            {
                lock (queBuffer)
                {
                    DataItem di = new DataItem(2, null);
                    di.EOF = true;
                    queBuffer.Enqueue(di);
                }
            }
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
                    DataItem[] rgdi = gdi(cData);
                    for (int i = 0; i < cData; i++)
                        rgdi[i] = queBuffer.Dequeue();
                    ldiBufferOut.AddRange(rgdi);
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
        private static WhitStream.Server.TCPServer server = new WhitStream.Server.TCPServer();
        NetworkStream nsClient;
        private delegate void StreamReader();
        private const int MAXDATA = 500;
        Stopwatch time = new Stopwatch();
        private int itemCount = 0;
        string format = ""; //the format of the header

        /// <summary> Default constructor to set up this operator as a listener </summary>
        public OpServerRaw()
        {
            server.AddListener(SetClient);
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - ServerRaw", base.ToString());
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
        /// <remarks> Will recieve the header file before it tries to read raw data</remarks>
        /// <remarks> Todo: Support for eof/stream termination, Non-blocking read of data items</remarks>
        protected void StrReader()
        {
            const int STREAMDATABUFFER = 100;
            bool eof = false,
                 receivedHeader = false;
            BinaryFormatter bf = new BinaryFormatter();
            DataItem[] rgdi = new DataItem[STREAMDATABUFFER];
            int iData = 0,
                fiter = 0; //format iterator

            while (!receivedHeader) //try to get the initial setup for data input
            {
                if (nsClient.DataAvailable)
                {
                    try
                    {
                        int inByte = nsClient.ReadByte();
                        while ((char)inByte != '#' && inByte != -1) //# indicates the termination of the header
                        {
                            format += (char)inByte;
                            inByte = nsClient.ReadByte();
                        }
                        if ((char)inByte == '#')
                            receivedHeader = true;
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("Something bad happened when we tried to read the header: {0} ({1})",
                            ex.Message, ((ex.InnerException != null) ? ex.InnerException.Message : ""));
                        eof = true;
                    }
                }
            }

            time.Start();
            do
            {
                while (iData < STREAMDATABUFFER && nsClient.DataAvailable && !eof)
                {
                    try
                    {
                        rgdi[iData] = new DataItem(format.Length, null);
                        while (fiter < format.Length && !eof) //TODO: this will lock out if an item is not completed, we may still want to push previously completed items
                        {
                            if (nsClient.DataAvailable)
                            {
                                switch (format[fiter]) //get the raw input based on the type
                                {
                                    case 'i': //the input is a 32 bit int
                                        {
                                            byte[] inbytes = new byte[4];
                                            nsClient.Read(inbytes, 0, 4); //read in the bytes
                                            rgdi[iData].AddValue(BitConverter.ToInt32(inbytes, 0));
                                            break;
                                        }
                                    case 'l': //the input is a 64 bit long (int)
                                        {
                                            byte[] inbytes = new byte[8];
                                            nsClient.Read(inbytes, 0, 8); //read in the bytes
                                            rgdi[iData].AddValue(BitConverter.ToInt64(inbytes, 0));
                                            break;
                                        }
                                    case 'c': //the input is a char
                                        {
                                            rgdi[iData].AddValue((char)nsClient.ReadByte());
                                            break;
                                        }
                                    case 'b': //the input is a bool
                                        {
                                            byte[] inbytes = new byte[1];
                                            nsClient.Read(inbytes, 0, 1);
                                            rgdi[iData].AddValue(BitConverter.ToBoolean(inbytes, 0));
                                            break;
                                        }
                                    default:
                                        {
                                            throw new System.Exception("The specified type from the header could not be identified");
                                        }
                                }
                                eof = rgdi[iData].EOF; //this won't work
                                fiter++;
                            }
                        }
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

                    iData++;
                    fiter = 0;
                }
                lock (queBuffer)
                {
                    for (int i = 0; i < iData; i++)
                    {
                        queBuffer.Enqueue(rgdi[i]);
                        rgdi[i] = null;
                        itemCount++;
                    }
                    /*if (fiter != 0) //there is a partially completed data item
                    {
                        rgdi[0] = rgdi[iData];
                        rgdi[iData] = null;
                        Console.WriteLine("I am here, fiter: {0} nsClient.DataAvailable: {1} iData: {2}", fiter, nsClient.DataAvailable, iData);
                    }*/
                    iData = 0;
                }
            } while (!eof);
            time.Stop();
            nsClient.Close();

            if (rgdi.Length == 0 || (rgdi[rgdi.Length - 1] != null && !rgdi[rgdi.Length - 1].EOF))
            {
                lock (queBuffer)
                {
                    DataItem di = new DataItem(2, null);
                    di.EOF = true;
                    queBuffer.Enqueue(di);
                }
            }
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
                    DataItem[] rgdi = gdi(cData);
                    for (int i = 0; i < cData; i++)
                        rgdi[i] = queBuffer.Dequeue();
                    ldiBufferOut.AddRange(rgdi);
                }

                //if (queBuffer.Count > 0)
                //    Console.WriteLine("queue size: {0}", queBuffer.Count);
            }

            return ldiBufferOut;
        }

        /// <summary> Return the current number of data items produced by this stream source </summary>
        public double DataCount
        {
            get { return itemCount; }
        }

        /// <summary> Return the data rate of this source </summary>
        public double DataRate
        {
            get { return (double)itemCount/time.ElapsedMilliseconds * 1000; }
        }

        /// <summary> How many data items are waiting to be processed? </summary>
        public int DataBacklog
        {
            get { return queBuffer.Count; }
        }
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
        /// Iterate through input data items. For a queue, just 
        /// output up to maximum data items
        /// </summary>
        /// <returns></returns>
        public override List<DataItem> Iterate(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
        {
            List<DataItem> ldiOut;
            bool fEOF = false;

            lock (data)
            {
                int c = (maxData > data.Count) ? data.Count : maxData;
                ldiOut = new List<DataItem>(c);
                while (c > 0)
                {
                    DataItem di = data.Dequeue();
                    ldiOut.Add(di);
                    fEOF |= di.EOF;
                    c--;
                }
                if (ldiOut.Count > 0)
                    eof = fEOF;
            }
            return ldiOut;
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
                DataItem diRet = new DataItem(attrs.Length, null);
                foreach (int i in attrs)
                    diRet.AddValue(di[i]);
                return diRet;
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

                if (ht.ContainsKey(hc) == false)
                {
                    List<DataItem> al = new List<DataItem>();
                    al.Add(di);
                    ht[hc] = al;
                    fNew = true;
                }
                else
                {
                    List<DataItem> al = ht[hc];
                    if (al.IndexOf(di) == -1)
                    {
                        al.Add(di);
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
            }

            // All groups during execution
            private Dictionary<int, List<Group>> ht = new Dictionary<int, List<Group>>();
            // The only group if attrs = null
            private Group gTotal;
            /// <summary>Which attributes are we grouping on?</summary>
            protected int[] attrs;

            private bool fComplete = false;

            /// <summary>
            /// Create a new HashAlgorithm operator
            /// </summary>
            /// <param name="a">The attributes for grouping</param>
            public HashAlgorithm(int[] a) { attrs = a; }

            /// <summary> How many data items are curently held in state </summary>
            public override int StateSize
            {
                get
                {
                    int cState = 0;
                    foreach (int o in ht.Keys)
                    {
                        cState += ht[o].Count;
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
                    foreach (int o in ht.Keys)
                    {
                        List<Group> al = (List<Group>)ht[o];
                        for (int i = 0; i < al.Count; i++)
                        {
                            Group g = al[i];
                            BuildGroupResult(g, out di);
                            ldi.Add(di);
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
                        for (int i = 0; i < al.Count; i++)
                        {
                            Group g = al[i];
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
                    if (al != null)
                    {
                        for (int i = 0; i < al.Count; i++)
                        {
                            int hc = al[i].hashcode;
                            List<Group> alCached = ht[hc];
                            alCached.Remove(al[i]);
							foreach (object obj in al[i].keys)
							    (obj as DataItem).Dispose();
                            if (alCached.Count == 0)
                                ht.Remove(hc);
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

                int hc = di.GetSpecificHashCode(attrs);
                if (!ht.ContainsKey(hc))
                    NewGroup(di, null, out g);
                else
                {
                    bool fMatch = false;
                    bool fEq;
                    List<Group> al = ht[hc];
                    g = null;
                    for (int i = 0; fMatch == false && i < al.Count; i++)
                    {
                        g = al[i];
                        fEq = true;
                        for (int j = 0; fEq && j < g.keys.Length; j++)
                            fEq &= di[attrs[j]].Equals(g.keys[j]);
                        fMatch = fEq;
                    }

                    fExists = fMatch;
                    if (fMatch == false)
                        NewGroup(di, al, out g);
                }

                return fExists;
            }

            private void NewGroup(DataItem di, List<Group> al, out Group g)
            {
                g = new Group();
                g.keys = new object[attrs.Length];
                int iKey = 0;
                foreach (int a in attrs)
                    g.keys[iKey++] = di[a];

                if (al == null)
                    al = new List<Group>();

                g.hashcode = di.GetSpecificHashCode(attrs);
                al.Add(g);
                ht[g.hashcode] = al;
            }

            /// <summary>
            /// Return all groups that match the given punctuation
            /// </summary>
            /// <param name="p">The punctuation to match groups on</param>
            /// <returns>The groups that match the punctuation</returns>
            protected List<Group> FindMatchingGroups(Data.Punctuation p)
            {
                List<Group> al = null;

                if (p.Describes(attrs))
                {
                    bool fComplete = true;
                    foreach (int a in attrs)
                        fComplete &= (!(p[a] is Data.Punctuation.WildcardPattern));

                    //Right now, we only support those punctuations that completely describe a group
                    // and only on literal patterns
                    if (fComplete)
                    {
                        int hc = p.GetSpecificHashCode(attrs);
                        if (ht.ContainsKey(hc))
                        {
                            bool fMatch = false;

                            al = ht[hc];
                            Group g = null;
                            for (int i = 0; fMatch == false && i < al.Count; i++)
                            {
                                g = al[i];
                                bool fEq = true;
                                for (int j = 0; fEq && j < g.keys.Length; j++)
                                {
                                    fEq &= ((Punctuation.Pattern)p[attrs[j]]).Match(g.keys[j]);
                                }
                                fMatch = fEq;
                            }

                            if (fMatch)
                            {
                                al = new List<Group>(1);
                                al.Add(g);
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
        }

		/// <summary>
		/// Determines if the given Punctuation sheme benefits Group-by
		/// </summary>
		/// <param name="ps">The punctuation scheme to check</param>
		/// <returns>If Benefits = true, else false</returns>
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

        private OpGroupByCount(string id, string a, Query opIn)
            : base(id, a, opIn)
        {
            Init();
        }

        private void Init() 
        {
            algorithm = new UnaryAlgorithm(algoHash = new HashAlgorithmCount(attrs));
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
            /// <summary>Constructor for Group by-Count</summary>
            public HashAlgorithmCount(int[] a) : base(a) { }
            /// <summary>Step functionality for Group By-Count</summary>
            public override Step FnStep { get { return GBCOUNTStep; } }

            internal void GBCOUNTStep(DataItem di, List<DataItem> ldi)
            {
                Group g;
                if (FindGroup(di, out g))
                    g.oResult = ((int)g.oResult) + 1;
                else
                    g.oResult = 1;
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
            algorithm = new UnaryAlgorithm(new HashAlgorithmSum(attrs, val));
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
            /// <summary>
            /// Constructor for group-by/sum
            /// </summary>
            /// <param name="a">Attributes for grouping</param>
            /// <param name="v">Initial value for sum</param>
            public HashAlgorithmSum(int[] a, int v) : base(a) { val = v; }
            /// <summary>Step functionality for group-by/sum</summary>
            public override Step FnStep { get { return GBSUMStep; } }

            internal void GBSUMStep(DataItem di, List<DataItem> ldi)
            {
                Group g;
                if (FindGroup(di, out g))
                    g.oResult = (double)g.oResult + (double)di[val];
                else
                    g.oResult = di[val];
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
            algorithm = new UnaryAlgorithm(new HashAlgorithmAvg(attrs, val));
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

            /// <summary>
            /// Constructor for avergae
            /// </summary>
            /// <param name="a">attributes for grouping</param>
            /// <param name="v">initial valuefor average</param>
            public HashAlgorithmAvg(int[] a, int v) : base(a) { val = v; }
            /// <summary>Step functionality for group-by/average</summary>
            public override Step FnStep { get { return GBAVGStep; } }

            internal void GBAVGStep(DataItem di, List<DataItem> ldi)
            {
                Group g;
                if (FindGroup(di, out g))
                {
                    ((State)g.oState).count = ((State)g.oState).count + 1;
                    ((State)g.oState).sum = ((State)g.oState).sum + (double)di[val];
                }
                else
                {
                    g.oState = new State();
                    ((State)g.oState).count = 1;
                    ((State)g.oState).sum = (double)di[val];
                }
                g.oResult = ((State)g.oState).sum / ((State)g.oState).count;
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

        private void Init()
        {
            algorithm = new UnaryAlgorithm(new HashAlgorithmMax(attrs, val));
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
            /// <summary>
            /// Constructor for group-by/max
            /// </summary>
            /// <param name="a">attributes for grouping</param>
            /// <param name="v">initial value</param>
            public HashAlgorithmMax(int[] a, int v) : base(a) { val = v; }
            /// <summary>Step functionality for group-by/max</summary>
            public override Step FnStep { get { return GBMAXStep; } }
            
            internal void GBMAXStep(DataItem di, List<DataItem> ldi)
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
        /// <param name="v">Which attribute to max over</param>
        /// <param name="opIn">The input query operator</param>
        public OpGroupByMin(int[] attrs, int v, Query opIn)
            : base(attrs, opIn)
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
            algorithm = new UnaryAlgorithm(new HashAlgorithmMin(attrs, val));
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
            /// <summary>
            /// Constructor for group-by/min
            /// </summary>
            /// <param name="a">attributes for grouping</param>
            /// <param name="v">intiali value</param>
            public HashAlgorithmMin(int[] a, int v) : base(a) { val = v; }
            /// <summary>Step functionality for group-by/min</summary>
            public override Step FnStep { get { return GBMINStep; } }

            internal void GBMINStep(DataItem di, List<DataItem> ldi)
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
}
