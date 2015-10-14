/*
 * Created by SharpDevelop.
 * User: Pete
 * Date: 12/28/2005
 * Time: 4:43 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using WhitStream.Data;
using WhitStream.Utility;

namespace WhitStream.QueryEngine
{
	#region Delegates
	/// <summary>
    /// Delegate function for query operator to iterate one step due to new, incoming data
    /// </summary>
    /// <param name="di">The incoming DataItem object</param>
    /// <param name="ldi">Any new DataItem objects that can be output due to the incoming DataItem</param>
    /// <seealso cref="Data.DataItem" />
    /// <seealso cref="QueryEngine.Query.StepTrivial"/>
    public delegate void Step(DataItem di, List<DataItem> ldi);

    /// <summary>
    /// Delegate function for query operator to iterate one step due to new, incoming list of data
    /// </summary>
    /// <param name="rgdi">The incoming DataItem object</param>
    /// <param name="eofInput">Whether EOF was found in teh input</param>
    /// <param name="ldi">Any new DataItem objects that can be output due to the incoming DataItem</param>
    /// <seealso cref="Data.DataItem" />
    /// <seealso cref="QueryEngine.Query.StepTrivial"/>
    public delegate void StepList(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput);

    /// <summary>
    /// Delegate function for query operator to finish processing, as the source is complete
    /// </summary>
    /// <param name="ldi">Any new DataItem objects that can be output at the end of the input</param>
    /// <seealso cref="Data.DataItem" />
    /// <seealso cref="QueryEngine.Query.FinalTrivial"/>
    public delegate void Final(List<DataItem> ldi);

    /// <summary>
    /// Delegate function for query operator to output data items early due to punctuations
    /// </summary>
    /// <param name="p">The incoming punctuation</param>
    /// <param name="ldi">Any new DataItem objects that can be output due to input punctuation</param>
    /// <seealso cref="Data.DataItem" />
    /// <seealso cref="Data.Punctuation" />
    /// <seealso cref="QueryEngine.Query.PassTrivial"/>
    public delegate void Pass(Punctuation p, List<DataItem> ldi);

    /// <summary>
    /// Delegate function for query operator to decrease required state, if possible, due to punctuation
    /// </summary>
    /// <seealso cref="Data.DataItem" />
    /// <seealso cref="Data.Punctuation" />
    /// <seealso cref="QueryEngine.Query.KeepTrivial"/>
    public delegate void Keep(Punctuation p);

    /// <summary>
    /// Delegate function for query operator to output punctuations due to incoming punctuation
    /// </summary>
    /// <param name="p">The incoming punctuation</param>
    /// <param name="ldi">Any new Punctuation objects that can be output</param>
    /// <seealso cref="Data.DataItem" />
    /// <seealso cref="Data.Punctuation" />
    /// <seealso cref="QueryEngine.Query.PropTrivial"/>
    public delegate void Prop(Punctuation p, List<DataItem> ldi);
	#endregion

    #region Interfaces
    interface IStreamDataSource
    {
        double DataCount { get; }
        double DataRate { get; }
        int DataBacklog { get; }
    }
    #endregion

    #region Query
    /// <summary>
    /// Interface to model execution of a query operator
    /// </summary>
    public abstract class Query
    {
        /// <summary> Initial buffer size for data for this operator </summary>
        protected const int DATABUFFERSIZE = 10000;
        /// <summary>Initial buffer size for punctuations for this operator </summary>
        protected const int PUNCTBUFFERSIZE = 2500;
        private static int SeedOpID = 1;

        /// <summary> ID for this operator in the query </summary>
        protected int opID;

        /// <summary> Whether this operator has completed execution </summary>
        protected bool eof = false;

        /// <summary> Data item representing EOF </summary>
        protected DataItem diEOF = new DataItem(0, null);

        /// <summary>
        /// Constructor sets operator ID for this operator and increments the OpID seed
        /// </summary>
        public Query()
        { 
            opID = SeedOpID++;
            diEOF.EOF = true;
        }

        /// <summary>
        /// Returns the opID for this operator
        /// </summary>
        public int OpID
        { get { return opID; } }

        /// <summary>
        /// Returns whether this operator has completed execution
        /// </summary>
        public bool EOF
        { get { return eof; } }

        /// <summary> String for serialization to XML </summary>
        public const string XMLQUERYTOPLEVEL = "WhitStreamQueryPlan";
        /// <summary> String for serialization to XML </summary>
        public const string XMLTOPID = "TOP";
        /// <summary> String for serialization to XML </summary>
        public const string XMLOPID = "ID";
        /// <summary> String for serialization to XML </summary>
        public const string XMLINPUTID = "INPUT";
        /// <summary> String for serializaation the number of inputs to XML (NAry operators only) </summary>
        public const string XMLINPUTCOUNT = "INPUTCOUNT";
        /// <summary> String for serialization to XML </summary>
        public const string XMLLEFTINPUTID = "LEFTINPUT";
        /// <summary> String for serialization to XML </summary>
        public const string XMLRIGHTINPUTID = "RIGHTINPUT";

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("#{0}", OpID);
        }

        /// <summary>
        /// Serialize this query
        /// </summary>
        public void Serialize(string outfile)
        {
            TextWriter tw = File.CreateText(outfile);
            tw.Write(string.Format("<{1} {2}=\"{0}\">\n", OpID, XMLQUERYTOPLEVEL, XMLTOPID));
            SerializeOp(tw);
            tw.Write("</{0}>\n", XMLQUERYTOPLEVEL);
            tw.Flush();
            tw.Close();
        }

        /// <summary>
        /// Serialize this query to the console
        /// </summary>
        public void Serialize()
        {
            TextWriter tw = Console.Out;
            tw.Write(string.Format("<{1} {2}=\"{0}\">\n", OpID, XMLQUERYTOPLEVEL, XMLTOPID));
            SerializeOp(tw);
            tw.Write("</{0}>\n", XMLQUERYTOPLEVEL);
            tw.Flush();
            tw.Close();
        }

        /// <summary>
        /// Serialize this specific operator
        /// </summary>
        /// <remarks> We should never get here, but its in for completeness </remarks>
        public virtual void SerializeOp(TextWriter tw)
        {
            tw.Write("<oops>Shouldn't be here!</oops>\n");
        }

        /// <summary>
        /// Parameters for query operators, so that we can
        /// construct operators for them
        /// </summary>
        protected class OperatorParams
        {
            /// <summary> Type name for this operator </summary>
            public string opName;
            /// <summary> Input operator ids for this operator </summary>
            public int[] rginputs = null;
            /// <summary> Any initialization parameters for this operator </summary>
            public string[] opParams;
        }

        /// <summary>
        /// Load a query from a file
        /// </summary>
        public static Query Deserialize(string outfile)
        {
            List<OperatorParams> ops = new List<OperatorParams>();
            int idTopOperator = -1;
            using (XmlTextReader reader = new XmlTextReader(outfile))
            {
                reader.Read();
                idTopOperator = Int32.Parse(reader.GetAttribute(XMLTOPID));
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        ops.Add(new OperatorParams());
                        ops[ops.Count - 1].opName = reader.Name;
                        ops[ops.Count - 1].opParams = new string[reader.AttributeCount];
                        reader.MoveToFirstAttribute();
                        int i = 0;
                        do
                        {
                            if (reader.Name == XMLINPUTID)
                            {
                                ops[ops.Count - 1].rginputs = new int[1];
                                ops[ops.Count - 1].rginputs[0] = Int32.Parse(reader.Value);
                            }
                            else if (reader.Name == XMLLEFTINPUTID)
                            {
                                if (ops[ops.Count - 1].rginputs == null)
                                    ops[ops.Count - 1].rginputs = new int[2];
                                ops[ops.Count - 1].rginputs[0] = Int32.Parse(reader.Value);
                            }
                            else if (reader.Name == XMLRIGHTINPUTID)
                            {
                                if (ops[ops.Count - 1].rginputs == null)
                                    ops[ops.Count - 1].rginputs = new int[2];
                                ops[ops.Count - 1].rginputs[1] = Int32.Parse(reader.Value);
                            }
                            else if (reader.Name == XMLINPUTCOUNT)
                                ops[ops.Count - 1].rginputs = new int[Int32.Parse(reader.Value)];
                            else if (ops[ops.Count-1].rginputs != null)
                            {
                                //Check to see if this is an expected input number
                                for (int j = 0; j < ops[ops.Count - 1].rginputs.Length; j++)
                                {
                                    if (reader.Name == string.Format("{0}{1}", XMLINPUTID, j))
                                        ops[ops.Count - 1].rginputs[j] = Int32.Parse(reader.Value);
                                }
                            }

                            ops[ops.Count - 1].opParams[i] =
                                string.Format("{0}={1}", reader.Name, reader.Value);
                            i++;
                        } while (reader.MoveToNextAttribute());
                    }
                }

                reader.Close();
            }

            //Now, build the query plan
            return BuildQueryPlan(ops, idTopOperator);
        }

		/// <summary>
		/// Determines if the operator benefits from the punctuation scheme
		/// </summary>
		/// <param name="ps">The punctuation scheme to be tested</param>
		public virtual bool Benefit(PunctuationScheme ps) { return true; }

        private static Query BuildQueryPlan(List<OperatorParams> ops, int idTop)
        {
            Queue<Query> opnodes = new Queue<Query>();
            Assembly asm = Assembly.GetExecutingAssembly();

            //First, find all leaf nodes. Build them, and add them to the queue
            EnqueueLeafNodes(ops, asm, opnodes);
            //Now, use those leaf nodes to build the rest of the plan
            return BuildPlan(ops, asm, opnodes, idTop);
        }

        private static void EnqueueLeafNodes(List<OperatorParams> ops, Assembly asm, Queue<Query> opnodes)
        {
            Query qop;

            foreach (OperatorParams op in ops)
            {
                if (op.rginputs == null)
                {
                    //Leaf node. Instantiate it and put it in the queue
                    Type type = asm.GetType(op.opName);
                    //Load this query operator on the fly
                    qop = (Query)Activator.CreateInstance(type, op.opParams);
                    opnodes.Enqueue(qop);
                }
            }
        }

        private static Query BuildPlan(List<OperatorParams> ops, Assembly asm, Queue<Query> opnodes, int idTop)
        {
            List<Query> inputs = new List<Query>();
            Query qop = null;
            object[] opParams;

            if (ops.Count == 1)
                //Just a Generate operator. Return it.
                return opnodes.Dequeue();

            //Now, while there is an operator in the queue, find it's parent and instantiate it, then add it to the queue
            while (opnodes.Count > 0)
            {
                inputs.Add((Query)opnodes.Dequeue());

                int i;
                for (i = 0; i < ops.Count && (ops[i].rginputs == null || ops[i].rginputs[0] != inputs[0].OpID); i++)
                    //Do nothing
                    ;

                if (i == ops.Count)
                {
                    //This operator is not the left-most input to any operator. Put it back in the queue until we find it a parent
                    opnodes.Enqueue(inputs[0]);
                    inputs.Clear();
                }
                else
                {
                    for (int o = 1; o < ops[i].rginputs.Length; o++ )
                    {
                        //Need to find the right input in the queue and remove it
                        inputs.Add(GetNextInputOp(opnodes, ops[i].rginputs[o]));
                    }

                    //Assume operator was found
                    Type type = asm.GetType(ops[i].opName);
                    //Copy over each parameter, except the INPUTID parameter
                    opParams = new object[ops[i].opParams.Length];
                    int src, dst;
                    for (src = dst = 0; src < ops[i].opParams.Length; src++)
                    {
                        if (ops[i].opParams[src].Contains(XMLINPUTID) == false)
                            opParams[dst++] = ops[i].opParams[src];
                    }

                    for (int o=0; o<ops[i].rginputs.Length; o++)
                        opParams[opParams.Length - ops[i].rginputs.Length + o] = inputs[o];
                    qop = (Query)Activator.CreateInstance(type, opParams);
                    if (qop.OpID != idTop)
                        opnodes.Enqueue(qop);

                    inputs.Clear();
                }
            }

            return qop;
        }

        private static Query GetNextInputOp(Queue<Query> opnodes, int idNext)
        {
            Query opNext = null;
            Queue<Query> q = new Queue<Query>();

            while (opnodes.Count > 0)
            {
                Query tmp = opnodes.Dequeue();
                if (tmp.OpID == idNext)
                    opNext = tmp;
                else
                    q.Enqueue(tmp);
            }

            opnodes = q;

            return opNext;
        }

        /// <summary>
        /// Load this specific operator
        /// </summary>
        /// <remarks> We should never get here </remarks> 
        public virtual void DeserializeOp(TextWriter tw)
        {
            tw.Write("<oops>Shouldn't be here!</oops>\n");
        }

        /// <summary>
        /// Read DataItems iteratively from the source query operator(s)
        /// </summary>
        /// <returns>Any DataItem objects to be output</returns>
        public virtual List<DataItem> Iterate(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
        {
            return null;
        }

        /// <summary>
        /// Let this operator know that it has been activated
        /// </summary>
        public virtual void Activate() { }

        /// <summary>
        /// Let this operator know that it has been deactivated
        /// </summary>
        public virtual void Deactivate() { }

        /// <summary>
        /// Split out the punctuations from the data items
        /// </summary>
        /// <param name="ldiIn">The input data items (and possibly punctuations)</param>
        /// <param name="ldiOut">The data items in the input</param>
        /// <param name="lpOut">The punctuations in the input</param>
        public void SplitPunc(List<DataItem> ldiIn, ref List<DataItem> ldiOut, ref List<Punctuation> lpOut)
        {
            ldiOut.Clear();
            lpOut.Clear();

            if (ldiIn != null)
            {
                foreach (DataItem di in ldiIn)
                {
                    if (di is Punctuation)
                        lpOut.Add(di as Punctuation);
                    else
                        ldiOut.Add(di);
                }
            }
        }

        /// <summary>
        /// Trivial step functionality : ignore the input continue
        /// </summary>
        /// <param name="di">The input data item</param>
        /// <param name="ldi">The output data items</param>
        public static void StepTrivial(DataItem di, List<DataItem>ldi) { }

        /// <summary>
        /// Trivial step functionality : ignore the input continue
        /// </summary>
        /// <param name="rgdi">The input data item</param>
        /// <param name="eofInput">Whether or not the input contained eof</param>
        /// <param name="ldi">The output data items</param>
        public static void StepListTrivial(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
        { eofInput = false; }

        /// <summary>
        /// Trivial final functionality : return nothing
        /// </summary>
        /// <param name="ldi">Output data items</param>
        public static void FinalTrivial(List<DataItem> ldi) { }

        /// <summary>
        /// Trivial pass functionality : ignore the input continue
        /// </summary>
        /// <param name="p">The input punctuation</param>
        /// <param name="ldi">Output data items</param>
        public static void PassTrivial(Punctuation p, List<DataItem> ldi) { }

        /// <summary>
        /// Trivial keep functionality : ignore the input continue
        /// </summary>
        /// <param name="p">The input punctuation</param>
        public static void KeepTrivial(Punctuation p) { }

        /// <summary>
        /// Trivial prop functionality : ignore the input continue
        /// </summary>
        /// <param name="p">The input punctuation</param>
        /// <param name="ldi">The output punctuations due to this punctuation</param>
        public static void PropTrivial(Punctuation p, List<DataItem> ldi) { }
	}
	#endregion

    #region Unary Algorithm
    /// <summary>Interface for all algorithm definitions</summary>
    public abstract class UnaryAlgorithmDefinition
    {
        private Utility.DataItemPool.GetDataItem getdataitem;
        private Utility.DataItemPool.ReleaseDataItem releasedataitem;

        /// <summary> How many data items are curently held in state </summary>
        public virtual int StateSize { get { return 0; } }

        /// <summary>Property for the GetDataItem DataPool method</summary>
        public Utility.DataItemPool.GetDataItem GetDataItem
        {
            get { return getdataitem; }
            set { getdataitem = value; }
        }

        /// <summary>Property for the ReleaseDataItem DataPool method</summary>
        public Utility.DataItemPool.ReleaseDataItem ReleaseDataItem
        {
            get { return releasedataitem; }
            set { releasedataitem = value; }
        }

        /// <summary>Default Step functionality</summary>
        public virtual Step FnStep { get { return Query.StepTrivial; } }
        /// <summary>Default StepList functionality</summary>
        public virtual StepList FnStepList { get { return Query.StepListTrivial; } }
        /// <summary>Default Pass functionality</summary>
        public virtual Pass FnPass { get { return Query.PassTrivial; } }
        /// <summary>Default Prop functionality</summary>
        public virtual Prop FnProp { get { return Query.PropTrivial; } }
        /// <summary>Default Keep functionality</summary>
        public virtual Keep FnKeep { get { return Query.KeepTrivial; } }
        /// <summary>Default Final functionality</summary>
        public virtual Final FnFinal { get { return Query.FinalTrivial; } }
    }

    /// <summary>
    /// Class for specifying algorithms for a particular operator
    /// </summary>
    public class UnaryAlgorithm
    {
        UnaryAlgorithmDefinition definition = null;

        private Step step;
        private StepList steplist;
        private Final final;
        private Pass pass;
        private Keep keep;
        private Prop prop;

        /// <summary>
        /// Constructor with specific algorithm definition
        /// </summary>
        /// <param name="def">The object defining the algorith to use</param>
        public UnaryAlgorithm(UnaryAlgorithmDefinition def)
        {
            definition = def;

            FnStep = def.FnStep;
            FnStepList = def.FnStepList;
            FnPass = def.FnPass;
            FnProp = def.FnProp;
            FnKeep = def.FnKeep;
            FnFinal = def.FnFinal;
        }

        /// <summary>Property for the GetDataItem DataPool method</summary>
        public Utility.DataItemPool.GetDataItem GetDataItem
        {
            get { return definition.GetDataItem; }
            set { definition.GetDataItem = value; }
        }

        /// <summary>Property for the ReleaseDataItem DataPool method</summary>
        public Utility.DataItemPool.ReleaseDataItem ReleaseDataItem
        {
            get { return definition.ReleaseDataItem; }
            set { definition.ReleaseDataItem = value; }
        }

        /// <summary>
        /// Property to set the Step function for this operator
        /// </summary>
        /// <seealso cref="QueryEngine.Step"/>
        public Step FnStep
        {
            get { return step; }
            set { step = value; }
        }

        /// <summary>
        /// Property to set the StepList function for this operator
        /// </summary>
        /// <seealso cref="QueryEngine.StepList"/>
        public StepList FnStepList
        {
            get { return steplist; }
            set { steplist = value; }
        }

        /// <summary>
        /// Property to set the Final function for this operator
        /// </summary>
        /// <seealso cref="QueryEngine.Final"/>
        public Final FnFinal
        {
            get { return final; }
            set { final = value; }
        }
        /// <summary>
        /// Property to set the Pass function for this operator
        /// </summary>
        /// <seealso cref="QueryEngine.Pass"/>
        public Pass FnPass
        {
            get { return pass; }
            set { pass = value; }
        }
        /// <summary>
        /// Property to set the Keep function for this operator
        /// </summary>
        /// <seealso cref="QueryEngine.Keep"/>
        public Keep FnKeep
        {
            get { return keep; }
            set { keep = value; }
        }
        /// <summary>
        /// Property to set the Propagation function for this operator
        /// </summary>
        /// <seealso cref="QueryEngine.Prop"/>
        public Prop FnProp
        {
            get { return prop; }
            set { prop = value; }
        }

        private void StepListTrivial(List<DataItem> rgdiIn, ref List<DataItem> ldi, out bool eofInput)
        {
            eofInput = false;

            foreach (DataItem di in rgdiIn)
            {
                if (di.EOF == false)
                    step(di, ldi);
                eofInput |= di.EOF;
            }
        }

    }
    #endregion

    #region Unary Operator
    /// <summary>
    /// Models general behavior of a unary query operator (e.g. Select, Project)
    /// </summary>
    public abstract class UnaryOp : Query
    {
        /// <summary> Buffer for containing input data for each iteration </summary>
        protected List<DataItem> ldiBuffer = new List<DataItem>(DATABUFFERSIZE);
        /// <summary> Buffer for containing input punctuations for each iteration </summary>
        protected List<Punctuation> lpBuffer = new List<Punctuation>(PUNCTBUFFERSIZE);
        /// <summary> Output data item buffer </summary>
        protected List<DataItem> ldiBufferOut = new List<DataItem>(DATABUFFERSIZE);

        /// <summary>
        /// The algorithm to use for this operator
        /// </summary>
        /// <seealso cref="UnaryAlgorithm"/>
        protected UnaryAlgorithm algorithm;

        /// <summary>
        /// The input to this unary query operator
        /// </summary>
        protected Query opIn;

        /// <summary>
        /// Delegate for functions that map DataItem objects into DataItem objects
        /// of a different format
        /// </summary>
        /// <param name="di" >The DataItem object to process</param>
        /// <returns>A possibly modified DataItem</returns>
        public delegate DataItem Map(DataItem di);

        /// <summary>
        /// dummy constructor
        /// </summary>
        public UnaryOp() : base() 
        {
            opIn = null;
        }

        /// <summary>
        /// Constructor to set up a unary operator with its input
        /// </summary>
        /// <param name="op">The input to this operator</param>
        public UnaryOp(Query op) : base()
        {
            opIn = op;
        }

		/// <summary>
		/// Let this operator know that it has been activated
		/// </summary>
		public override void Activate()
		{
			if (opIn != null)
			{
				opIn.Activate();
			}
		}

		/// <summary>
		/// Let this operator know that it has been deactivated
		/// </summary>
		public override void Deactivate()
		{
			if (opIn != null)
			{
				opIn.Deactivate();
			}
		}

        /// <summary>
        /// Serialize this specific operator
        /// </summary>
        public override void SerializeOp(TextWriter tw)
        {
            if (opIn != null)
            {
                opIn.SerializeOp(tw);
                tw.Write(string.Format("\t<{0} {1}=\"{2}\" {3}=\"{4}\" ",
                    this.GetType().ToString(), XMLOPID, OpID, XMLINPUTID, opIn.OpID));
            }
            else
                tw.Write(string.Format("\t<{0} {1}=\"{2}\" ", this.GetType().ToString(), XMLOPID, OpID));
        }

        /// <summary>
        /// The input operator to this operator
        /// </summary>
        public Query Input
        {
            get { return opIn; }
            set { opIn = value; }
        }


        /// <summary>
        /// Iterate function to work through data items.
        /// The Step function is called with each data item that arrives.
        /// The Final function is called when EOF is encountered
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
        {
            bool eofInput;

            ldiBufferOut.Clear();
            if (opIn != null)
            {
                algorithm.GetDataItem = gdi;
                algorithm.ReleaseDataItem = rdi;

                SplitPunc(opIn.Iterate(gdi, rdi), ref ldiBuffer, ref lpBuffer);
                algorithm.FnStepList(ldiBuffer, ref ldiBufferOut, out eofInput);
                foreach (Punctuation p in lpBuffer)
                {
                    algorithm.FnPass(p, ldiBufferOut);
                    algorithm.FnKeep(p);
                    algorithm.FnProp(p, ldiBufferOut);
                }
                if (eofInput)
                {
                    algorithm.FnFinal(ldiBufferOut);

                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }

                algorithm.GetDataItem = null;
                algorithm.ReleaseDataItem = null;
            }
            return ldiBufferOut;
        }
	}
	#endregion

    #region Binary Algorithm
    /// <summary>Interface for all algorithm definitions</summary>
    public abstract class BinaryAlgorithmDefinition
    {
        private Utility.DataItemPool.GetDataItem getdataitem;
        private Utility.DataItemPool.ReleaseDataItem releasedataitem;

        /// <summary> How many data items are curently held in state </summary>
        public virtual int StateSize { get { return 0; } }

        /// <summary>Property for the GetDataItem DataPool method</summary>
        public Utility.DataItemPool.GetDataItem GetDataItem
        {
            get { return getdataitem; }
            set { getdataitem = value; }
        }

        /// <summary>Property for the ReleaseDataItem DataPool method</summary>
        public Utility.DataItemPool.ReleaseDataItem ReleaseDataItem
        {
            get { return releasedataitem; }
            set { releasedataitem = value; }
        }

        /// <summary>Default Step functionality</summary>
        public virtual Step FnStepLeft { get { return Query.StepTrivial; } }
        /// <summary>Default Step functionality</summary>
        public virtual Step FnStepRight { get { return Query.StepTrivial; } }
        /// <summary>Default StepList functionality</summary>
        public virtual StepList FnStepListLeft { get { return Query.StepListTrivial; } }
        /// <summary>Default StepList functionality</summary>
        public virtual StepList FnStepListRight { get { return Query.StepListTrivial; } }
        /// <summary>Default Pass functionality</summary>
        public virtual Pass FnPassLeft { get { return Query.PassTrivial; } }
        /// <summary>Default Pass functionality</summary>
        public virtual Pass FnPassRight { get { return Query.PassTrivial; } }
        /// <summary>Default Prop functionality</summary>
        public virtual Prop FnPropLeft { get { return Query.PropTrivial; } }
        /// <summary>Default Prop functionality</summary>
        public virtual Prop FnPropRight { get { return Query.PropTrivial; } }
        /// <summary>Default Keep functionality</summary>
        public virtual Keep FnKeepLeft { get { return Query.KeepTrivial; } }
        /// <summary>Default Keep functionality</summary>
        public virtual Keep FnKeepRight { get { return Query.KeepTrivial; } }
        /// <summary>Default Final functionality</summary>
        public virtual Final FnFinalLeft { get { return Query.FinalTrivial; } }
        /// <summary>Default Final functionality</summary>
        public virtual Final FnFinalRight { get { return Query.FinalTrivial; } }
    }

    /// <summary>
    /// Class for specifying algorithms for a particular operator
    /// </summary>
    public class BinaryAlgorithm
    {
        BinaryAlgorithmDefinition definition = null;

        private Step stepLeft, stepRight;
        private StepList steplistLeft, steplistRight;
        private Final finalLeft, finalRight;
        private Pass passLeft, passRight;
        private Keep keepLeft, keepRight;
        private Prop propLeft, propRight;

        /// <summary>
        /// Default constructor -- algorithm is the trivial (identity) algorithm
        /// </summary>
        public BinaryAlgorithm()
        {
            FnStepRight = Query.StepTrivial;
			FnStepLeft = Query.StepTrivial;
            FnStepListLeft = StepListLeftTrivial;
            FnStepListRight = StepListRightTrivial;
            FnPassRight = Query.PassTrivial;
			FnPassLeft = Query.PassTrivial;
            FnPropRight = Query.PropTrivial;
			FnPropLeft = Query.PropTrivial;
            FnKeepRight = Query.KeepTrivial;
			FnKeepLeft = Query.KeepTrivial;
            FnFinalRight = Query.FinalTrivial;
			FnFinalLeft = Query.FinalTrivial;
        }

        /// <summary>
        /// Constructor with specific algorithm definition
        /// </summary>
        /// <param name="def">The object defining the algorith to use</param>
        public BinaryAlgorithm(BinaryAlgorithmDefinition def)
        {
            definition = def;

            FnStepLeft = def.FnStepLeft;
            FnStepRight = def.FnStepRight;
            FnStepListLeft = def.FnStepListLeft;
            FnStepListRight = def.FnStepListRight;
            FnPassLeft = def.FnPassLeft;
            FnPassRight = def.FnPassRight;
            FnPropLeft = def.FnPropLeft;
            FnPropRight = def.FnPropRight;
            FnKeepLeft = def.FnKeepLeft;
            FnKeepRight = def.FnKeepRight;
            FnFinalLeft = def.FnFinalLeft;
            FnFinalRight = def.FnFinalRight;
        }

        /// <summary>Property for the GetDataItem DataPool method</summary>
        public Utility.DataItemPool.GetDataItem GetDataItem
        {
            get { return definition.GetDataItem; }
            set { definition.GetDataItem = value; }
        }

        /// <summary>Property for the ReleaseDataItem DataPool method</summary>
        public Utility.DataItemPool.ReleaseDataItem ReleaseDataItem
        {
            get { return definition.ReleaseDataItem; }
            set { definition.ReleaseDataItem = value; }
        }

        /// <summary>
        /// Property to set the Step function for this operator's left input
        /// </summary>
        /// <seealso cref="QueryEngine.Step"/>
        public Step FnStepLeft
        {
            get { return stepLeft; }
            set { stepLeft = value; }
        }
        /// <summary>
        /// Property to set the Step function for this operator's right input
        /// </summary>
        /// <seealso cref="QueryEngine.Step"/>
        public Step FnStepRight
        {
            get { return stepRight; }
            set { stepRight = value; }
        }

        /// <summary>
        /// Property to set the StepList function for this operator's left input
        /// </summary>
        /// <seealso cref="QueryEngine.StepList"/>
        public StepList FnStepListLeft
        {
            get { return steplistLeft; }
            set { steplistLeft = value; }
        }
        /// <summary>
        /// Property to set the StepList function for this operator's right input
        /// </summary>
        /// <seealso cref="QueryEngine.StepList"/>
        public StepList FnStepListRight
        {
            get { return steplistRight; }
            set { steplistRight = value; }
        }

        /// <summary>
        /// Property to set the Final function for this operator's left input
        /// </summary>
        /// <seealso cref="QueryEngine.Final"/>
        public Final FnFinalLeft
        {
            get { return finalLeft; }
            set { finalLeft = value; }
        }
        /// <summary>
        /// Property to set the Final function for this operator's right input
        /// </summary>
        /// <seealso cref="QueryEngine.Final"/>
        public Final FnFinalRight
        {
            get { return finalRight; }
            set { finalRight = value; }
        }

        /// <summary>
        /// Property to set the Pass function for this operator's left input
        /// </summary>
        /// <seealso cref="QueryEngine.Pass"/>
        public Pass FnPassLeft
        {
            get { return passLeft; }
            set { passLeft = value; }
        }
        /// <summary>
        /// Property to set the Pass function for this operator's right input
        /// </summary>
        /// <seealso cref="QueryEngine.Pass"/>
        public Pass FnPassRight
        {
            get { return passRight; }
            set { passRight = value; }
        }

        /// <summary>
        /// Property to set the Keep function for this operator's left input
        /// </summary>
        /// <seealso cref="QueryEngine.Keep"/>
        public Keep FnKeepLeft
        {
            get { return keepLeft; }
            set { keepLeft = value; }
        }
        /// <summary>
        /// Property to set the Keep function for this operator's right input
        /// </summary>
        /// <seealso cref="QueryEngine.Keep"/>
        public Keep FnKeepRight
        {
            get { return keepRight; }
            set { keepRight = value; }
        }

        /// <summary>
        /// Property to set the Propagation function for this operator's left input
        /// </summary>
        /// <seealso cref="QueryEngine.Prop"/>
        public Prop FnPropLeft
        {
            get { return propLeft; }
            set { propLeft = value; }
        }
        /// <summary>
        /// Property to set the Propagation function for this operator's right input
        /// </summary>
        /// <seealso cref="QueryEngine.Prop"/>
        public Prop FnPropRight
        {
            get { return propRight; }
            set { propRight = value; }
        }

        private void StepListLeftTrivial(List<DataItem> rgdiIn, ref List<DataItem> ldi, out bool eofInput)
        {
            eofInput = false;

            foreach (DataItem di in rgdiIn)
            {
                if (di.EOF == false)
                    stepLeft(di, ldi);
                eofInput |= di.EOF;
            }
        }
        private void StepListRightTrivial(List<DataItem> rgdiIn, ref List<DataItem> ldi, out bool eofInput)
        {
            eofInput = false;

            foreach (DataItem di in rgdiIn)
            {
                if (di.EOF == false)
                    stepRight(di, ldi);
                eofInput |= di.EOF;
            }
        }

    }
    #endregion

	#region Binary Operator
	/// <summary>
    /// Models general behavior of binary query operators
    /// </summary>
    public abstract class BinaryOp : Query
    {
        /// <summary> Buffer for input data from the left input for each iteration</summary>
        protected List<DataItem> ldiBufferLeft = new List<DataItem>(DATABUFFERSIZE);
        /// <summary> Buffer for input punctuation from the left input for each iteration</summary>
        protected List<Punctuation> lpBufferLeft = new List<Punctuation>(PUNCTBUFFERSIZE);
        /// <summary> Buffer for input data from the rightinput for each iteration</summary>
        protected List<DataItem> ldiBufferRight = new List<DataItem>(DATABUFFERSIZE);
        /// <summary> Buffer for input punctuation from the right input for each iteration</summary>
        protected List<Punctuation> lpBufferRight = new List<Punctuation>(PUNCTBUFFERSIZE);
        /// <summary> Buffer for output data items </summary>
        protected List<DataItem> ldiBufferOut = new List<DataItem>(DATABUFFERSIZE);
        /// <summary>
        /// The algorithm(s) to execute
        /// </summary>
        protected BinaryAlgorithm algorithm;

        /// <summary>
        /// The left and right input operators for this binary operator
        /// </summary>
        protected Query opInLeft, opInRight;
        /// <summary>
        /// Whether EOF has been reached on the left and right inputs, respectively.
        /// </summary>
        protected bool fLeftEOF = false, fRightEOF = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BinaryOp()
        {
            opInLeft = null;
            opInRight = null;
        }

        /// <summary>
        /// Constructor for a binary operator
        /// </summary>
        /// <param name="opL">The left input query operator</param>
        /// <param name="opR">The right input query operator</param>
        public BinaryOp(Query opL, Query opR)
            : base()
        {
            opInLeft = opL;
            opInRight = opR;
        }

		/// <summary>
		/// Let this operator know that it has been activated
		/// </summary>
		public override void Activate()
		{
			if (opInRight != null && opInLeft != null)
			{
				opInRight.Activate();
				opInLeft.Activate();
			}
		}

		/// <summary>
		/// Let this operator know that it has been deactivated
		/// </summary>
		public override void Deactivate()
		{
			if (opInRight != null && opInLeft != null)
			{
				opInRight.Deactivate();
				opInLeft.Deactivate();
			}
		}

        /// <summary>
        /// Serialize this specific operator
        /// </summary>
        public override void SerializeOp(TextWriter tw)
        {
            if (opInLeft != null)
                opInLeft.SerializeOp(tw);

            if (opInRight != null)
                opInRight.SerializeOp(tw);

            tw.Write(string.Format("\t<{0} {1}=\"{2}\" ", this.GetType().ToString(), XMLOPID, OpID));
            if (opInLeft != null)
                tw.Write(string.Format("{0}=\"{1}\" ", XMLLEFTINPUTID, opInLeft.OpID));
            if (opInRight != null)
                tw.Write(string.Format("{0}=\"{1}\" ", XMLRIGHTINPUTID, opInRight.OpID));
        }

        /// <summary>
        /// The left-side input operator to this operator
        /// </summary>
        public Query LeftInput
        {
            get { return opInLeft; }
            set { opInLeft = value; }
        }

        /// <summary>
        /// The right-side input operator to this operator
        /// </summary>
        public Query RightInput
        {
            get { return opInRight; }
            set { opInRight = value; }
        }

        /// <summary>
        /// Combine two DataItem objects (in cross-product fashion)
        /// </summary>
        /// <param name="diL">Left DataItem object</param>
        /// <param name="diR">Right DataItem object</param>
        /// <param name="gdi">GetDataItem function</param>
        /// <returns>a data item which is a pair of the given data items</returns>
        public static DataItem Pair(DataItem diL, DataItem diR, Utility.DataItemPool.GetDataItem gdi)
        {
            //Call the correct constructor to create a DataItem or Punctuation
            DataItem di;
            if (diL is Punctuation && diR is Punctuation)
                di = new Punctuation(diL.Count + diR.Count);
            else
                di = gdi(1)[0];
            for (int i = 0; i < diL.Count; i++)
                di.AddValue(diL[i]);
            for (int i = 0; i < diR.Count; i++)
                di.AddValue(diR[i]);

            return di;
        }

        /// <summary>
        /// Iterate function to work through data items.
        /// The Step functions are called with each data item that arrives from each input.
        /// The Final functions aer called when EOF is encountered on each input
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
        {
            bool fLEOF = false, fREOF = false;

            ldiBufferOut.Clear();
            if (opInLeft != null && opInRight != null)
            {
                algorithm.GetDataItem = gdi;
                algorithm.ReleaseDataItem = rdi;

                SplitPunc(opInLeft.Iterate(gdi, rdi), ref ldiBufferLeft, ref lpBufferLeft);
                SplitPunc(opInRight.Iterate(gdi, rdi), ref ldiBufferRight, ref lpBufferRight);

                algorithm.FnStepListLeft(ldiBufferLeft, ref ldiBufferOut, out fLEOF);
                algorithm.FnStepListRight(ldiBufferRight, ref ldiBufferOut, out fREOF);

                foreach (Punctuation p in lpBufferLeft)
                    algorithm.FnPassLeft(p, ldiBufferOut);
                foreach (Punctuation p in lpBufferRight)
                    algorithm.FnPassRight(p, ldiBufferOut);

                foreach (Punctuation p in lpBufferLeft)
                {
                    algorithm.FnKeepLeft(p);
                    algorithm.FnPropLeft(p, ldiBufferOut);
                }
                foreach (Punctuation p in lpBufferRight)
                {
                    algorithm.FnKeepRight(p);
                    algorithm.FnPropRight(p, ldiBufferOut);
                }

                if (fLEOF)
                {
                    algorithm.FnFinalLeft(ldiBufferOut);
                    fLeftEOF = true; //Don't set main EOF flag until after calling final
                }
                if (fREOF)
                {
                    algorithm.FnFinalRight(ldiBufferOut);
                    fRightEOF = true; //Don't set main EOF flag until after calling final
                }

                if (fLeftEOF && fRightEOF)
                {
                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
                algorithm.GetDataItem = null;
                algorithm.ReleaseDataItem = null;
            }
            return ldiBufferOut;
        }
	}
	#endregion

    #region NAry Operator
    /// <summary>
    /// Class to model operators with n inputs
    /// </summary>
    public class NAryOp : Query
    {
        private List<DataItem> ldiBuffer = new List<DataItem>(DATABUFFERSIZE);
        private List<Punctuation> lpBuffer = new List<Punctuation>(DATABUFFERSIZE);
        private List<DataItem> ldiBufferOut = new List<DataItem>(DATABUFFERSIZE);

        //The input operators for this operator
        private List<Query> qopInputs = new List<Query>();

        //The behavior functions for this operator
        private Step[] rgStep;
        private StepList[] rgSteplist;
        private Final[] rgFinal;
        private Pass[] rgPass;
        private Prop[] rgProp;
        private Keep[] rgKeep;

        //Track EOF for each input
        private bool[] rgeof;

        //Track the previously read input
        private int iLastInput = -1;

        /// <summary>
        /// Function to determine which input to read from next
        /// </summary>
        /// <returns>the input index to read from</returns>
        public delegate int NextInput();

        private NextInput nextInput;

        /// <summary>
        /// Function to determine which input to read from next
        /// </summary>
        public NextInput Next
        {
            get { return nextInput; }
            set { nextInput = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public NAryOp()
        {
            Init(null);
        }

        /// <summary>
        /// Constructor with all inputs for this operator
        /// </summary>
        /// <param name="qsInput">The input query operators</param>
        public NAryOp(params Query[] qsInput)
        {
            Init(qsInput);

            nextInput = NextInputRoundRobin;

            rgStep = new Step[qsInput.Length];
            rgSteplist = new StepList[qsInput.Length];
            rgFinal = new Final[qsInput.Length];
            rgPass = new Pass[qsInput.Length];
            rgProp = new Prop[qsInput.Length];
            rgKeep = new Keep[qsInput.Length];
            rgeof = new bool[qsInput.Length];

            for (int i = 0; i < qsInput.Length; i++)
            {
                rgStep[i] = StepTrivial;
                rgSteplist[i] = NAryStepListTrivial;
                rgFinal[i] = FinalTrivial;
                rgPass[i] = PassTrivial;
                rgProp[i] = PropTrivial;
                rgKeep[i] = KeepTrivial;
                rgeof[i] = false;
            }
        }

        private void Init(Query[] qs)
        {
            qopInputs.Clear();
            qopInputs.AddRange(qs);
        }

		/// <summary>
		/// Let this operator know that it has been activated
		/// </summary>
		public override void Activate()
		{
			foreach (Query input in Inputs)
			{
				if (input != null)
					input.Activate();
			}
		}

		/// <summary>
		/// Let this operator know that it has been deactivated
		/// </summary>
		public override void Deactivate()
		{
			foreach (Query input in Inputs)
			{
				if (input != null)
					input.Deactivate();
			}
		}

        /// <summary>
        /// Serialize this specific operator
        /// </summary>
        public override void SerializeOp(TextWriter tw)
        {
            foreach (Query q in Inputs)
                q.SerializeOp(tw);

            tw.Write(string.Format("\t<{0} {1}=\"{2}\" {3}=\"{4}\" ", this.GetType().ToString(), XMLOPID, OpID, XMLINPUTCOUNT, Inputs.Count));
            for (int i=0; i<Inputs.Count; i++)
            {
                if (Inputs[i] != null)
                    tw.Write(string.Format("{0}{1}=\"{2}\" ", XMLINPUTID, i, Inputs[i].OpID));
            }
        }

        /// <summary> Accessor to the input operators </summary>
        public List<Query> Inputs
        { get { return qopInputs; } }

        /// <summary> Which inputs are we currently reading from? </summary>
        public int CurrentInput
        { get { return iLastInput; } }

        /// <summary> Accessor to the Step functions for this operator </summary>
        public Step[] StepFunctions
        { get { return rgStep; } }

        /// <summary> Accessor to the StepList functions for this operator </summary>
        public StepList[] StepListFunctions
        { get { return rgSteplist; } }

        /// <summary> Accessor to the Final functions for this operator </summary>
        public Final[] FinalFunctions
        { get { return rgFinal; } }

        /// <summary> Accessor to the Pass functions for this operator </summary>
        public Pass[] PassFunctions
        { get { return rgPass; } }

        /// <summary> Accessor to the Prop functions for this operator </summary>
        public Prop[] PropFunctions
        { get { return rgProp; } }

        /// <summary> Accessor to the Keep functions for this operator </summary>
        public Keep[] KeepFunctions
        { get { return rgKeep; } }

        private void NAryStepListTrivial(List<DataItem> rgdiIn, ref List<DataItem> ldi, out bool eofInput)
        {
            eofInput = false;

            foreach (DataItem di in rgdiIn)
            {
                if (di.EOF == false)
                    rgStep[iLastInput](di, ldi);
                eofInput |= di.EOF;
            }
        }

        /// <summary>
        /// Read each input in turn
        /// </summary>
        /// <returns>the next input to read</returns>
        public int NextInputRoundRobin()
        {
            iLastInput = (iLastInput + 1) % qopInputs.Count;
            return iLastInput;
        }

        /// <summary>
        /// Iterate function to work through data items.
        /// The Step functions are called with each data item that arrives from one chosen input
        /// The Final functions are called when EOF is encountered on each input
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
        {
            ldiBufferOut.Clear();

            //Which input should we read from next?
            int iNext = Next();

            if (qopInputs[iNext] != null)
            {
                SplitPunc(qopInputs[iNext].Iterate(gdi, rdi), ref ldiBuffer, ref lpBuffer);

                if (rgeof[iNext] == false)
                    rgSteplist[iNext](ldiBuffer, ref ldiBufferOut, out rgeof[iNext]);

                foreach (Punctuation p in lpBuffer)
                    rgPass[iNext](p, ldiBufferOut);

                foreach (Punctuation p in lpBuffer)
                {
                    rgKeep[iNext](p);
                    rgProp[iNext](p, ldiBufferOut);
                }

                if (rgeof[iNext])
                    rgFinal[iNext](ldiBufferOut);

                bool fEOF = true;
                for (int i = 0; i < qopInputs.Count && fEOF == true; i++)
                    fEOF &= rgeof[i];

                if (fEOF)
                {
                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
    }
    #endregion
}
