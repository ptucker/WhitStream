/*
 * Created by SharpDevelop.
 * User: Pete
 * Date: 12/29/2005
 * Time: 12:21 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.IO;
using WhitStream.Data;
using WhitStream.Expression;

namespace WhitStream.QueryEngine.QueryOperators
{
	#region Union
	/// <summary>
    /// Models the query operator Union
    /// </summary>
    /// <remarks>Since this operator is non-blocking, it uses the trivial final function</remarks>
    /// <seealso cref="QueryEngine.Query.FinalTrivial"/>
    public class OpUnion : BinaryOp
    {
        private Naive algoNaive;

        /// <summary>
        /// Default constructor for Union
        /// </summary>
        public OpUnion()
            : base()
        {
            Init();
        }

        /// <summary> Constructor for Union </summary>
        /// <param name="opInL">The left input query operator</param>
        /// <param name="opInR">The right input query operator</param>
        public OpUnion(Query opInL, Query opInR)
            : base(opInL, opInR)
        {
            Init();
        }

        /// <summary> Constructor for Union </summary>
        /// <param name="id">ID for this operator</param>
        /// <param name="opInL">The left input query operator</param>
        /// <param name="opInR">The right input query operator</param>
        /// <remarks>Should only be used with deserialization</remarks>
        public OpUnion(string id, Query opInL, Query opInR)
            : base(opInL, opInR)
        {
            opID = Int32.Parse(id.Substring(id.IndexOf('=') + 1));

            Init();
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - BinaryUnion", base.ToString());
        }

        private void Init()
        {
            algorithm = new BinaryAlgorithm(algoNaive = new Naive());
        }

        /// <summary> Serialize this Union operator </summary>
        /// <param name="tw">Destination for XML data</param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);
            tw.Write(" />\n");
        }

#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step functions are called with each data item that arrives from each input.
        /// The Final functions aer called when EOF is encountered on each input
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool fLEOF = false, fREOF = false;

            ldiBufferOut.Clear();
            if (opInLeft != null && opInRight != null)
            {
                SplitPunc(opInLeft.Iterate(), ref ldiBufferLeft, ref lpBufferLeft);
                SplitPunc(opInRight.Iterate(), ref ldiBufferRight, ref lpBufferRight);

                algoNaive.UStepList(ldiBufferLeft, ref ldiBufferOut, out fLEOF);
                algoNaive.UStepList(ldiBufferRight, ref ldiBufferOut, out fREOF);

                foreach (Punctuation p in lpBufferLeft)
                {
                    algoNaive.UPropLeft(p, ldiBufferOut);
                }
                foreach (Punctuation p in lpBufferRight)
                {
                    algoNaive.UPropRight(p, ldiBufferOut);
                }

                if (fLEOF)
                {
                    fLeftEOF = true; //Don't set main EOF flag until after calling final
                }
                if (fREOF)
                {
                    fRightEOF = true; //Don't set main EOF flag until after calling final
                }

                if (fLeftEOF && fRightEOF)
                {
                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif

        /// <summary>
        /// Naive implementation for union
        /// </summary>
        public class Naive : BinaryAlgorithmDefinition
        {
            private List<Punctuation> alLeft = new List<Punctuation>(), alRight = new List<Punctuation>();

            /// <summary>Step functionality for union</summary>
            public override Step FnStepLeft { get { return UStep; } }
            /// <summary>Step functionality for union</summary>
            public override Step FnStepRight { get { return UStep; } }
            /// <summary>StepList functionality for union</summary>
            public override StepList FnStepListLeft { get { return UStepList; } }
            /// <summary>StepList functionality for union</summary>
            public override StepList FnStepListRight { get { return UStepList; } }
            /// <summary>Prop functionality for union</summary>
            public override Prop FnPropLeft { get { return UPropLeft; } }
            /// <summary>Prop functionality for union</summary>
            public override Prop FnPropRight { get { return UPropRight; } }

            internal void UStep(DataItem di, List<DataItem> ldi)
            { ldi.Add(di); }

            internal void UStepList(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;
                foreach (DataItem di in rgdi)
                {
                    if (di.EOF == false)
                        ldi.Add(di);
                    eofInput |= di.EOF;
                }
            }

            internal void UPropLeft(Punctuation p, List<DataItem> ldi)
            {
                //Only output punctuations that have matches from the Right
                if (alRight.Exists(p.Equals))
                {
                    ldi.Add(p);
                    alRight.RemoveAll(p.Equals);
                }
                else
                    alLeft.Add(p);
            }

            internal void UPropRight(Punctuation p, List<DataItem> ldi)
            {
                //Only output punctuations that have matches from the Left
                if (alLeft.Exists(p.Equals))
                {
                    ldi.Add(p);
                    alLeft.RemoveAll(p.Equals);
                }
                else
                    alRight.Add(p);
            }
        }
    }
	#endregion

	#region Intersect
	/// <summary>
    /// Models the query operator Intersect
    /// </summary>
    public class OpIntersect : BinaryOp
    {
        private Naive algoNaive;

        /// <summary>
        /// Default constructor for Intersect
        /// </summary>
        public OpIntersect()
            : base()
        {
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Intersect
        /// </summary>
        /// <param name="opInL">The left input query operator</param>
        /// <param name="opInR">The right input query operator</param>
        /// <returns></returns>
        public OpIntersect(Query opInL, Query opInR)
            : base(opInL, opInR)
        {
            Init();
        }

        /// <summary> Constructor for Intersect </summary>
        /// <param name="id">ID for this operator</param>
        /// <param name="opInL">The left input query operator</param>
        /// <param name="opInR">The right input query operator</param>
        /// <remarks>Should only be used with deserialization</remarks>
        public OpIntersect(string id, Query opInL, Query opInR)
            : base(opInL, opInR)
        {
            opID = Int32.Parse(id.Substring(id.IndexOf('=') + 1));
            Init();
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - Intersect<{1}>", base.ToString(), algoNaive.StateSize);
        }

        private void Init()
        {
            algorithm = new BinaryAlgorithm(algoNaive = new Naive());
        }

        /// <summary> Serialize this operator </summary>
        /// <param name="tw">Destination for XML data</param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);
            tw.Write(" />\n");
        }

#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step functions are called with each data item that arrives from each input.
        /// The Final functions aer called when EOF is encountered on each input
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool fLEOF = false, fREOF = false;

            ldiBufferOut.Clear();
            if (opInLeft != null && opInRight != null)
            {
                SplitPunc(opInLeft.Iterate(), ref ldiBufferLeft, ref lpBufferLeft);
                SplitPunc(opInRight.Iterate(), ref ldiBufferRight, ref lpBufferRight);

                algoNaive.IStepListLeft(ldiBufferLeft, ref ldiBufferOut, out fLEOF);
                algoNaive.IStepListRight(ldiBufferRight, ref ldiBufferOut, out fREOF);

                foreach (Punctuation p in lpBufferLeft)
                {
                    algoNaive.IKeepLeft(p);
                    algoNaive.IPropLeft(p, ldiBufferOut);
                }
                foreach (Punctuation p in lpBufferRight)
                {
                    algoNaive.IKeepRight(p);
                    algoNaive.IPropRight(p, ldiBufferOut);
                }

                if (fLEOF)
                {
                    algoNaive.IFinalLeft(ldiBufferOut);
                    fLeftEOF = true; //Don't set main EOF flag until after calling final
                }
                if (fREOF)
                {
                    algoNaive.IFinalRight(ldiBufferOut);
                    fRightEOF = true; //Don't set main EOF flag until after calling final
                }

                if (fLeftEOF && fRightEOF)
                {
                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif

        /// <summary>
        /// Naive implementation for Intersect
        /// </summary>
        public class Naive : BinaryAlgorithmDefinition
        {
            private Dictionary<int, List<DataItem>> htLeft = new Dictionary<int, List<DataItem>>();
            private Dictionary<int, List<DataItem>> htRight = new Dictionary<int, List<DataItem>>();
            private List<Punctuation> alLeft = new List<Punctuation>(), alRight = new List<Punctuation>();
            private bool fEOFLeft = false, fEOFRight = false;

            /// <summary> How many data items are curently held in state </summary>
            public override int StateSize { get { return htLeft.Count + htRight.Count; } }

            /// <summary>Step functionality for intersect</summary>
            public override Step FnStepLeft { get { return IStepLeft; } }
            /// <summary>Step functionality for intersect</summary>
            public override Step FnStepRight { get { return IStepRight; } }
            /// <summary>Prop functionality for intersect</summary>
            public override Prop FnPropLeft { get { return IPropLeft; } }
            /// <summary>Prop functionality for intersect</summary>
            public override Prop FnPropRight { get { return IPropRight; } }
            /// <summary>Keep functionality for intersect</summary>
            public override Keep FnKeepLeft { get { return IKeepLeft; } }
            /// <summary>Keep functionality for intersect</summary>
            public override Keep FnKeepRight { get { return IKeepRight; } }
            /// <summary>Final functionality for intersect</summary>
            public override Final FnFinalLeft { get { return IFinalLeft; } }
            /// <summary>Final functionality for intersect</summary>
            public override Final FnFinalRight { get { return IFinalRight; } }

            internal void IStepListLeft(List<DataItem> rgdiIn, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;

                foreach (DataItem di in rgdiIn)
                {
                    if (di.EOF == false)
                        IStepLeft(di, ldi);
                    eofInput |= di.EOF;
                }
            }

            internal void IStepListRight(List<DataItem> rgdiIn, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;

                foreach (DataItem di in rgdiIn)
                {
                    if (di.EOF == false)
                        IStepRight(di, ldi);
                    eofInput |= di.EOF;
                }
            }

            internal void IStepLeft(DataItem di, List<DataItem> ldi)
            {
                List<DataItem> al = null;
                int hc = di.GetHashCode();

                if (fEOFRight == false)
                {
                    if (htLeft.ContainsKey(hc))
                        al = htLeft[hc];
                    if (al == null)
                    {
                        al = new List<DataItem>();
                        htLeft.Add(hc, al);
                    }
                    al.Add(di);
                }
                al = null;
                if (htRight.ContainsKey(hc))
                    al = htRight[hc];
                if (al != null && al.IndexOf(di) != -1)
                    ldi.Add(di);
            }

            internal void IStepRight(DataItem di, List<DataItem> ldi)
            {
                List<DataItem> al = null;
                int hc = di.GetHashCode();

                if (fEOFLeft == false)
                {
                    if (htRight.ContainsKey(hc))
                        al = htRight[hc];
                    if (al == null)
                    {
                        al = new List<DataItem>();
                        htRight.Add(hc, al);
                    }
                    al.Add(di);
                }
                al = null;
                if (htLeft.ContainsKey(hc))
                    al = htLeft[hc];
                if (al != null && al.IndexOf(di) != -1)
                    ldi.Add(di);
            }

            internal void IKeepLeft(Punctuation p)
            {
                List<int> keys = new List<int>();
                foreach (int k in htRight.Keys)
                {
                    List<DataItem> al = htRight[k];

                    for (int i = 0; i < al.Count; i++)
                    {
                        if (p.Match(al[i]))
                            al.RemoveAt(i--);
                    }

                    if (al.Count == 0)
                        keys.Add(k);
                }

                foreach (int k in keys)
                    htRight.Remove(k);
            }

            internal void IKeepRight(Punctuation p)
            {
                List<int> keys = new List<int>();
                foreach (int k in htLeft.Keys)
                {
                    List<DataItem> al = htLeft[k];

                    for (int i = 0; i < al.Count; i++)
                    {
                        if (p.Match(al[i]))
                            al.RemoveAt(i--);
                    }

                    if (al.Count == 0)
                        keys.Add(k);
                }

                foreach (int k in keys)
                    htLeft.Remove(k);
            }

            internal void IPropLeft(Punctuation p, List<DataItem> ldi)
            {
                //Only output punctuations that have matches from the Right
                if (alRight.Exists(p.Equals))
                    ldi.Add(p);

                alLeft.Add(p);
            }

            internal void IPropRight(Punctuation p, List<DataItem> ldi)
            {
                //Only output punctuations that have matches from the Left
                if (alLeft.Exists(p.Equals))
                    ldi.Add(p);

                alRight.Add(p);
            }

            internal void IFinalLeft(List<DataItem> ldi)
            {
                htRight.Clear();
                fEOFLeft = true;
            }

            internal void IFinalRight(List<DataItem> ldi)
            {
                htLeft.Clear();
                fEOFRight = true;
            }
        }
	}
	#endregion

	#region Difference
	/// <summary>
    /// Models the query operator Set Difference
    /// </summary>
    public class OpDifference : BinaryOp
    {
        private Naive algoNaive;

        /// <summary>
        /// Default constructor for Difference
        /// </summary>
        public OpDifference()
            : base()
        {
            Init();
        }

        /// <summary>
        /// Constructor for the query operator Difference
        /// </summary>
        /// <param name="opInL">The left (positive) input query operator</param>
        /// <param name="opInR">The right (negative) input query operator</param>
        /// <returns></returns>
        public OpDifference(Query opInL, Query opInR)
            : base(opInL, opInR)
        {
            Init();
        }

        /// <summary> Constructor for Union </summary>
        /// <param name="id">ID for this operator</param>
        /// <param name="opInL">The left input query operator</param>
        /// <param name="opInR">The right input query operator</param>
        /// <remarks>Should only be used with deserialization</remarks>
        public OpDifference(string id, Query opInL, Query opInR)
            : base(opInL, opInR)
        {
            opID = Int32.Parse(id.Substring(id.IndexOf('=') + 1));

            Init();
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - Difference<{1}>", base.ToString(), algoNaive.StateSize);
        }

        private void Init()
        {
            algorithm = new BinaryAlgorithm(algoNaive = new Naive());
        }

        /// <summary> Serialize this operator </summary>
        /// <param name="tw">Destination for the XML data </param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);
            tw.Write(" />\n");
        }

#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step functions are called with each data item that arrives from each input.
        /// The Final functions aer called when EOF is encountered on each input
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool fLEOF = false, fREOF = false;

            ldiBufferOut.Clear();
            if (opInLeft != null && opInRight != null)
            {
                SplitPunc(opInLeft.Iterate(), ref ldiBufferLeft, ref lpBufferLeft);
                SplitPunc(opInRight.Iterate(), ref ldiBufferRight, ref lpBufferRight);

                algoNaive.DStepListLeft(ldiBufferLeft, ref ldiBufferOut, out fLEOF);
                algoNaive.DStepListRight(ldiBufferRight, ref ldiBufferOut, out fREOF);

                foreach (Punctuation p in lpBufferRight)
                    algoNaive.DPassRight(p, ldiBufferOut);

                foreach (Punctuation p in lpBufferLeft)
                {
                    algoNaive.DKeepLeft(p);
                }
                foreach (Punctuation p in lpBufferRight)
                {
                    algoNaive.DKeepRight(p);
                    algoNaive.DPropRight(p, ldiBufferOut);
                }

                if (fLEOF)
                {
                    algoNaive.DFinalLeft(ldiBufferOut);
                    fLeftEOF = true; //Don't set main EOF flag until after calling final
                }
                if (fREOF)
                {
                    algoNaive.DFinalRight(ldiBufferOut);
                    fRightEOF = true; //Don't set main EOF flag until after calling final
                }

                if (fLeftEOF && fRightEOF)
                {
                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif

        /// <summary>
        /// Naive implementation for Difference
        /// </summary>
        public class Naive : BinaryAlgorithmDefinition
        {
            private List<DataItem> alLeft = new List<DataItem>();
            private Dictionary<int, List<DataItem>> htRight = new Dictionary<int, List<DataItem>>();
            List<Punctuation> alLeftP = new List<Punctuation>(), alRightP = new List<Punctuation>(),
                alLeftPIter = new List<Punctuation>();
            bool fEOFLeft = false, fEOFRight = false;

            /// <summary>Step functionality for difference</summary>
            public override Step FnStepLeft { get { return DStepLeft; } }
            /// <summary>Step functionality for difference</summary>
            public override Step FnStepRight { get { return DStepRight; } }
            /// <summary>Keep functionality for difference</summary>
            public override Keep FnKeepLeft { get { return DKeepLeft; } }
            /// <summary>Keep functionality for difference</summary>
            public override Keep FnKeepRight { get { return DKeepRight; } }
            /// <summary>Pass functionality for difference</summary>
            public override Pass FnPassRight { get { return DPassRight; } }
            /// <summary>Prop functionality for difference</summary>
            public override Prop FnPropRight { get { return DPropRight; } }
            /// <summary>Final functionality for difference</summary>
            public override Final FnFinalLeft { get { return DFinalLeft; } }
            /// <summary>Final functionality for difference</summary>
            public override Final FnFinalRight { get { return DFinalRight; } }

            /// <summary> How many data items are curently held in state </summary>
            public override int StateSize { get { return alLeft.Count + htRight.Count; } }

            internal void DStepListLeft(List<DataItem> rgdiIn, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;

                foreach (DataItem di in rgdiIn)
                {
                    if (di.EOF == false)
                        DStepLeft(di, ldi);
                    eofInput |= di.EOF;
                }
            }

            internal void DStepListRight(List<DataItem> rgdiIn, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;

                foreach (DataItem di in rgdiIn)
                {
                    if (di.EOF == false)
                        DStepRight(di, ldi);
                    eofInput |= di.EOF;
                }
            }
            internal void DStepLeft(DataItem di, List<DataItem> ldi)
            {
                List<DataItem> al = null;
                int hc = di.GetHashCode();
                if (htRight.ContainsKey(hc))
                    al = htRight[hc];

                if (fEOFRight && (al == null || al.IndexOf(di) == -1))
                    ldi.Add(di);
                else
                    alLeft.Add(di);
            }

            internal void DStepRight(DataItem di, List<DataItem> ldi)
            {
                if (fEOFLeft == false)
                {
                    int hc = di.GetHashCode();
                    List<DataItem> al = null;
                    if (htRight.ContainsKey(hc))
                        al = htRight[hc];
                    if (al == null)
                    {
                        al = new List<DataItem>();
                        htRight.Add(hc, al);
                    }
                    al.Add(di);
                }
            }

            internal void DFinalLeft(List<DataItem> ldi)
            {
                if (fEOFRight == false)
                    alLeft.Clear();

                fEOFLeft = true;
            }

            internal void DFinalRight(List<DataItem> ldi)
            {
                ldi.AddRange(OutputDiff(null));
                alLeft.Clear();
                fEOFRight = true;
            }

            internal void DPassRight(Punctuation p, List<DataItem> ldi)
            {
                ldi.AddRange(OutputDiff(p));
            }

            internal void DPropRight(Punctuation p, List<DataItem> ldi)
            {
                //Only output punctuations that have matches from the Right
                if (alLeftP.Exists(p.Equals))
                    ldi.Add(p);
            }

            internal void DKeepLeft(Punctuation p)
            {
                alLeftP.Add(p);
                alLeftPIter.Add(p);
            }

            internal void DKeepRight(Punctuation p)
            {
                List<int> keys = new List<int>();

                alRightP.Add(p);

                foreach (Punctuation pl in alLeftPIter)
                {
                    foreach (int k in htRight.Keys)
                    {
                        List<DataItem> al = htRight[k];

                        for (int i = 0; i < al.Count; i++)
                        {
                            if (pl.Match(al[i]))
                                al.RemoveAt(i--);
                        }

                        if (al.Count == 0)
                            keys.Add(k);
                    }

                    foreach (int k in keys)
                        htRight.Remove(k);
                }
                alLeftPIter.Clear();

                for (int i = 0; i < alLeft.Count; i++)
                {
                    if (p.Match(alLeft[i]))
                        alLeft.RemoveAt(i--);
                }
            }

            private List<DataItem> OutputDiff(Punctuation p)
            {
                List<DataItem> alOut = new List<DataItem>();
                List<DataItem> al = null;

                foreach (DataItem di in alLeft)
                {
                    al = null;
                    int hc = di.GetHashCode();
                    if (htRight.ContainsKey(hc))
                        al = htRight[hc];
                    if (al == null || al.IndexOf(di) == -1)
                    {
                        if (p == null || p.Match(di))
                            alOut.Add(di);
                    }
                }

                return (alOut.Count == 0) ? null : alOut;
            }
        }
	}
	#endregion

	#region Join

	/// <summary>
	/// Base Join class.  Has no functionality on its own.
	/// </summary>
    public class OpJoin : BinaryOp
    {
        private SymmetricHash algoSHJ;
        //private SymmRangePunct algoSRP;
        private string stExpr;

        /// <summary> XML Attribute name for join expression </summary>
        protected const string XMLEXPR = "EXPR";

        /// <summary>
        /// Default Constructor for OpJoin
        /// </summary>
        public OpJoin() : base() { }

        /// <summary>
        /// Constructor for a Join operator
        /// </summary>
        /// <param name="pred">The predicate defining the join</param>
        /// <param name="opL">The left input query operator</param>
        /// <param name="opR">The right input query operator</param>
        public OpJoin(string pred, Query opL, Query opR)
            : base(opL, opR)
        {
            Init(pred);
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - Join<{1}>", base.ToString(), algoSHJ.StateSize);
        }

        /// <summary>
        /// Property to get and set the join predicate
        /// </summary>
        public String Predicate
        {
            get { return stExpr; }
            set { Init(value); }
        }

        /// <summary> Serialize this operator </summary>
        /// <param name="tw">Destination for this XML data</param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);
            string st = stExpr.Replace("<", "&lt;").Replace(">", "&gt;");
            tw.Write(string.Format(" {0}=\"{1}\" />\n", XMLEXPR, st));
        }

        private void Init(string pred)
        {
            stExpr = pred;
            algorithm = new BinaryAlgorithm(algoSHJ = new SymmetricHash(pred));
            //algorithm = new BinaryAlgorithm(algoSRP = new SymmRangePunct(pred, 0, 1));
        }

        /// <summary>
        /// Determines if the given Punctuation sheme benefits Join
        /// </summary>
        /// <param name="ps">The punctuation scheme to check</param>
        /// <returns>If Benefits = true, else false</returns>
        public override bool Benefit(PunctuationScheme ps)
        {
            return false;
        }

        /// <summary>
        /// Determines if the given Punctuation sheme benefits Group-by
        /// </summary>
        /// <param name="ps">The punctuation scheme to check</param>
        /// <param name="attrs">The int[] to check</param>
        /// <returns>If Benefits = true, else false</returns>
        private bool Benefit(PunctuationScheme ps, int[] attrs)
        {
            if (attrs == null) return false;
            List<int> NonWildcard = new List<int>();
            //Extract all the non-wildcard patterns from the Punctuation Scheme
            for (int i = 0; i < ps.Count; i++)
            {
                if (!(ps[i] is Punctuation.WildcardPattern))
                    NonWildcard.Add(i);
            }
            //Foreach non-wildcard pattern - see if it only affects joined attributes
            //Go through all the non-wildcard positions
            foreach (int position in NonWildcard)
            {
                bool grouped = false;
                //Go through all the attributes being grouped
                foreach (int attr in attrs)
                {
                    //If we found a match - we know that the non-wildcard pattern
                    //affects a joined attribute.
                    if (attr == position)
                        grouped = true;
                }
                if (!grouped)
                    return false;
            }
            //If we made it through the matching process without returning,
            //we know that this punctuation scheme contains wildcard patterns
            //for all non-joined attributes.
            return true;
        }

#if NODELEGATE
        /// <summary>
        /// Iterate function to work through data items.
        /// The Step functions are called with each data item that arrives from each input.
        /// The Final functions aer called when EOF is encountered on each input
        /// </summary>
        /// <returns>DataItem objects that can be output from this iteration</returns>
        /// <seealso cref="QueryEngine.Step"/>
        /// <seealso cref="QueryEngine.Final"/>
        public override List<DataItem> Iterate()
        {
            bool fLEOF = false, fREOF = false;

            ldiBufferOut.Clear();
            if (opInLeft != null && opInRight != null)
            {
                SplitPunc(opInLeft.Iterate(), ref ldiBufferLeft, ref lpBufferLeft);
                SplitPunc(opInRight.Iterate(), ref ldiBufferRight, ref lpBufferRight);

                algoSHJ.JStepListLeft(ldiBufferLeft, ref ldiBufferOut, out fLEOF);
                algoSHJ.JStepListRight(ldiBufferRight, ref ldiBufferOut, out fREOF);

                foreach (Punctuation p in lpBufferLeft)
                {
                    algoSHJ.JKeepLeft(p);
                    algoSHJ.JPropLeft(p, ldiBufferOut);
                }
                foreach (Punctuation p in lpBufferRight)
                {
                    algoSHJ.JKeepRight(p);
                    algoSHJ.JPropRight(p, ldiBufferOut);
                }

                if (fLEOF)
                {
                    algoSHJ.JFinalLeft(ldiBufferOut);
                    fLeftEOF = true; //Don't set main EOF flag until after calling final
                }
                if (fREOF)
                {
                    algoSHJ.JFinalRight(ldiBufferOut);
                    fRightEOF = true; //Don't set main EOF flag until after calling final
                }

                if (fLeftEOF && fRightEOF)
                {
                    ldiBufferOut.Add(diEOF);
                    eof = true;
                }
            }
            return ldiBufferOut;
        }
#endif

        #region Symmetric Join
        /// <summary>
        /// Models the query operator Join using the pipelined double-hash join algorithm
        /// </summary>
        public class SymmetricHash : BinaryAlgorithmDefinition
        {
            private IExpr expr;
            // Join attributes for each input
            private int[] attrsLeft, attrsRight;

            // DataItem objects from the left input
            private Dictionary<int, List<DataItem>> htLeft = new Dictionary<int, List<DataItem>>();
            private Dictionary<int, List<Punctuation>> htLeftP = new Dictionary<int, List<Punctuation>>();
            // DataItem objects from the right input
            private Dictionary<int, List<DataItem>> htRight = new Dictionary<int, List<DataItem>>();
            private Dictionary<int, List<Punctuation>> htRightP = new Dictionary<int, List<Punctuation>>();
            //Converted punctuations to compare to opposite input
            private MetaPunctuation mpLeft = null, mpRight = null;
            private bool fEOFLeft = false, fEOFRight = false;
            DataItem diLeftCache = null, diRightCache = null;

            /// <summary>
            /// Constructor for a new Join object, given some predicate and data sources
            /// </summary>
            /// <param name="pred">The predicate to filter on</param>
            /// <remarks>The predicate accepts =, !=, &lt;,&gt;, &lt;=, &gt;=, AND, OR, and NOT.</remarks>
            /// <remarks>To compare with attributes in a data item, you first give '$', followed
            /// by the DataItem number (always '1' or '2' for join), and then the attribute number (1-based)</remarks>
            /// <example>"$1.4 = $2.1" finds all data items that match on the fourth attribute equals the first attribute</example>
            public SymmetricHash(string pred)
            {
                expr = ExprParser.Parse(pred);
                FindJoinAttributes(pred);
            }

            /// <summary> How many data items are curently held in state </summary>
            public override int StateSize { get { return htLeft.Count + htRight.Count; } }

            /// <summary>StepList functionality for symmetric hash join</summary>
            public override StepList FnStepListLeft { get { return JStepListLeft; } }
            /// <summary>StepList functionality for symmetric hash join</summary>
            public override StepList FnStepListRight { get { return JStepListRight; } }
            /// <summary>Keep functionality for symmetric hash join</summary>
            public override Keep FnKeepLeft { get { return JKeepLeft; } }
            /// <summary>Keep functionality for symmetric hash join</summary>
            public override Keep FnKeepRight { get { return JKeepRight; } }
            /// <summary>Prop functionality for symmetric hash join</summary>
            public override Prop FnPropLeft { get { return JPropLeft; } }
            /// <summary>Prop functionality for symmetric hash join</summary>
            public override Prop FnPropRight { get { return JPropRight; } }
            /// <summary>Final functionality for symmetric hash join</summary>
            public override Final FnFinalLeft { get { return JFinalLeft; } }
            /// <summary>Final functionality for symmetric hash join</summary>
            public override Final FnFinalRight { get { return JFinalRight; } }

            private void FindJoinAttributes(string pred)
            {
                int lTemp = -1, rTemp = -1;
                bool fEq = false;
                List<int> alLeft = new List<int>(), alRight = new List<int>();
                for (int i = 0; i < pred.Length; i++)
                {
                    while (i < pred.Length && pred[i] != '$')
                        i++;
                    if (i < pred.Length)
                    {
                        //We've found an attribute to work with
                        i = GetAttribute(pred, i, ref lTemp, ref rTemp);

                        while (pred[i] == ' ')
                            i++;

                        if (pred[i] == '=')
                            fEq = true;

                        while (pred[i] != ' ')
                            i++;
                        while (pred[i] == ' ')
                            i++;

                        if (fEq && pred[i] == '$')
                        {
                            i = GetAttribute(pred, i, ref lTemp, ref rTemp);
                            alLeft.Add(lTemp);
                            alRight.Add(rTemp);
                        }

                        fEq = false;
                    }
                }

                attrsLeft = new int[alLeft.Count];
                attrsRight = new int[alRight.Count];
                alLeft.CopyTo(attrsLeft);
                alRight.CopyTo(attrsRight);
            }

            /// Find the join attributes for the appropriate input
            private int GetAttribute(string pred, int i, ref int l, ref int r)
            {
                while (i < pred.Length && !Char.IsDigit(pred[i]))
                    i++;
                int iAhead = i;
                while (iAhead < pred.Length && Char.IsDigit(pred[iAhead]))
                    iAhead++;
                int iInput = Int32.Parse(pred.Substring(i, iAhead - i));

                i = ++iAhead;
                while (iAhead < pred.Length && Char.IsDigit(pred[iAhead]))
                    iAhead++;

                if (iInput == 1)
                    l = Int32.Parse(pred.Substring(i, iAhead - i)) - 1;
                else
                    r = Int32.Parse(pred.Substring(i, iAhead - i)) - 1;

                return iAhead;
            }

            internal void JStepListLeft(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
            {
                List<DataItem> al = null;
                eofInput = false;

                foreach (DataItem di in rgdi)
                {
                    al = null;
                    if (di.EOF) eofInput = true;
                    else
                    {
                        if (diLeftCache == null)
                            diLeftCache = di;

                        int hc = di.GetSpecificHashCode(attrsLeft);
                        if (fEOFRight == false)
                        {
                            //Check if this data item matches any punctuations that have arrived from the right
                            bool fPunct = (mpLeft != null) ? mpLeft.Match(di) : false;

                            //Don't hold the data in state if it is already matching a punctuation from the right
                            if (fPunct == false)
                            {
                                if (htLeft.ContainsKey(hc))
                                    al = htLeft[hc];
                                if (al == null)
                                {
                                    al = new List<DataItem>();
                                    htLeft.Add(hc, al);
                                }
                                al.Add(di);
                            }
                        }

                        al = null;
                        if (htRight.ContainsKey(hc))
                            al = htRight[hc];
                        for (int i = 0; al != null && i < al.Count; i++)
                        {
                            DataItem diR = (DataItem)al[i];
                            if (expr.Evaluate(di, diR))
                                ldi.Add(Pair(di, diR, GetDataItem));
                        }
                    }
                }
            }

            internal void JStepListRight(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
            {
                List<DataItem> al = null;
                eofInput = false;

                foreach (DataItem di in rgdi)
                {
                    al = null;
                    if (di.EOF) eofInput = true;
                    else
                    {
                        if (diRightCache == null)
                            diRightCache = di;

                        int hc = di.GetSpecificHashCode(attrsRight);
                        if (fEOFLeft == false)
                        {
                            //Check if this data item matches any punctuations from the left
                            bool fPunct = (mpRight != null) ? mpRight.Match(di) : false;

                            //Don't hold the data in state if it is already matching a punctuation from the left
                            if (fPunct == false)
                            {
                                if (htRight.ContainsKey(hc))
                                    al = htRight[hc];
                                if (al == null)
                                {
                                    al = new List<DataItem>();
                                    htRight.Add(hc, al);
                                }
                                al.Add(di);
                            }
                        }

                        al = (htLeft.ContainsKey(hc)) ? al = htLeft[hc] : null;
                        if (al != null)
                        {
                            for (int i = 0; i < al.Count; i++)
                            {
                                DataItem diL = (DataItem)al[i];
                                if (expr.Evaluate(diL, di))
                                    ldi.Add(Pair(diL, di, GetDataItem));
                            }
                        }
                    }
                }
            }

            internal void JPropLeft(Punctuation p, List<DataItem> ldi)
            {
                List<Punctuation> al = null;

                int hc = p.GetSpecificHashCode(attrsLeft);
                if (fEOFRight == false)
                {
                    if (htLeftP.ContainsKey(hc))
                        al = htLeftP[hc];
                    if (al == null)
                    {
                        al = new List<Punctuation>();
                        htLeftP.Add(hc, al);
                    }
                    al.Add(p);
                }

                al = null;
                if (htRightP.ContainsKey(hc))
                    al = htRightP[hc];
                for (int i = 0; al != null && i < al.Count; i++)
                {
                    Punctuation pR = (Punctuation)al[i];
                    if (expr.Evaluate(p, pR))
                    {
                        ldi.Add(Pair(p, pR, GetDataItem));
                        if (mpLeft != null)
                            mpLeft.Remove(Convert(pR, diLeftCache, attrsRight, attrsLeft));
                        if (mpRight != null)
                            mpRight.Remove(Convert(p, diRightCache, attrsLeft, attrsRight));
                    }
                }
            }

            internal void JPropRight(Punctuation p, List<DataItem> ldi)
            {
                List<Punctuation> al = null;

                int hc = p.GetSpecificHashCode(attrsRight);
                if (fEOFLeft == false)
                {
                    if (htRightP.ContainsKey(hc))
                        al = htRightP[hc];
                    if (al == null)
                    {
                        al = new List<Punctuation>();
                        htRightP.Add(hc, al);
                    }
                    al.Add(p);
                }

                al = null;
                if (htLeftP.ContainsKey(hc))
                    al = htLeftP[hc];
                for (int i = 0; al != null && i < al.Count; i++)
                {
                    Punctuation pL = (Punctuation)al[i];
                    if (expr.Evaluate(pL, p))
                    {
                        ldi.Add(Pair(pL, p, GetDataItem));
                        if (mpLeft != null)
                            mpLeft.Remove(Convert(p, diLeftCache, attrsRight, attrsLeft));
                        if (mpRight != null)
                            mpRight.Remove(Convert(pL, diRightCache, attrsLeft, attrsRight));
                    }
                }
            }

            internal void JKeepLeft(Punctuation p)
            {
                JKeep(p, attrsLeft, attrsRight, diRightCache, htRight);

                if (diRightCache != null)
                {
                    if (mpRight == null)
                        mpRight = new MetaPunctuation(diRightCache.Count);

                    mpRight.Include(Convert(p, diRightCache, attrsLeft, attrsRight));
                }
            }

            internal void JKeepRight(Punctuation p)
            {
                JKeep(p, attrsRight, attrsLeft, diLeftCache, htLeft);

                if (diLeftCache != null)
                {
                    if (mpLeft == null)
                        mpLeft = new MetaPunctuation(diLeftCache.Count);

                    mpLeft.Include(Convert(p, diLeftCache, attrsRight, attrsLeft));
                }
            }

            private Punctuation Convert(Punctuation pThis, DataItem diCache,
                int[] attrsThis, int[] attrsOther)
            {
                Punctuation pRet = new Punctuation(diCache.Count);

                for (int i = 0; i < diCache.Count; i++)
                    pRet.AddValue(new Punctuation.WildcardPattern());

                for (int i = 0; i < attrsOther.Length; i++)
                    pRet[attrsOther[i]] = pThis[attrsThis[i]];

                return pRet;
            }

            private void JKeep(Punctuation p, int[] attrsThisSide, int[] attrsOtherSide,
                DataItem diCache, Dictionary<int, List<DataItem>> ht)
            {
                if (p.Describes(attrsThisSide) && diCache != null)
                {
                    int i;

                    Punctuation pOther = Convert(p, diCache, attrsThisSide, attrsOtherSide);

                    //Now, go through the data items on the left side and remove matches
                    List<int> alKeys = new List<int>();
                    //Check to see if the join attributes are literals or wildcards
                    bool joinKey = true;
                    for (i = 0; i < attrsOtherSide.Length && joinKey; i++)
                        joinKey = (pOther[attrsOtherSide[i]] is Punctuation.ListPattern) ||
                            (pOther[attrsOtherSide[i]] is Punctuation.LiteralPattern);
                    //If the join attribute patterns are all literals or lists, then we can compute the hash code 
                    // on the punctuation to probe the hash table. Otherwise, we have to go through key by key
                    if (joinKey)
                    {
                        List<Punctuation> lps = pOther.Flatten();
                        foreach (Punctuation pCheck in lps)
                        {
                            int k = pCheck.GetSpecificHashCode(attrsOtherSide);
                            if (ht.ContainsKey(k))
                            {
                                List<DataItem> al = ht[k];
                                for (i = 0; i < al.Count; i++)
                                {
                                    if (pCheck.Match(al[i]))
                                        al.RemoveAt(i--);
                                }
                                if (al.Count == 0)
                                    alKeys.Add(k);
                            }
                        }
                    }
                    else
                    {
                        if (pOther.IsAllWildcard)
                            ht.Clear();
                        else
                        {
                            foreach (int k in ht.Keys)
                            {
                                List<DataItem> al = ht[k];
                                for (i = 0; i < al.Count; i++)
                                {
                                    if (pOther.Match(al[i]))
                                        al.RemoveAt(i--);
                                }
                                if (al.Count == 0)
                                    alKeys.Add(k);
                            }
                        }
                    }

                    if (alKeys.Count != 0)
                    {
                        foreach (int k in alKeys)
                            ht.Remove(k);
                    }
                }
            }

            /// <summary>
            /// Final functionality for Join -- clear out the right input
            /// </summary>
            /// <param name="ldi">Always unchanged</param>
            /// <seealso cref="QueryEngine.Final"/>
            public void JFinalLeft(List<DataItem> ldi)
            {
                htRight.Clear();
                fEOFLeft = true;
            }

            /// <summary>
            /// Final functionality for Join -- clear out the left input
            /// </summary>
            /// <param name="ldi">Always unchanged</param>
            /// <seealso cref="QueryEngine.Final"/>
            public void JFinalRight(List<DataItem> ldi)
            {
                htLeft.Clear();
                fEOFRight = true;
            }
        }
        #endregion

        #region Merge Join
        /// <summary>
        /// Models the query operator Join using the sort-merge join algorithm
        /// </summary>
        public class MergeJoin : BinaryAlgorithmDefinition
        {
            private IExpr expr;
            // Join attributes for each input
            private int[] attrsLeft, attrsRight;
            /// DataItem objects from the left input
            private List<DataItem> lLeft = new List<DataItem>();
            private List<Punctuation> lLeftP = new List<Punctuation>();
            /// DataItem objects from the right input
            private List<DataItem> lRight = new List<DataItem>();
            private List<Punctuation> lRightP = new List<Punctuation>();

            /// Since merge needs order - store which value of the dataitem is sorted
            private int orderLeft = 0, orderRight = 0;
            //Sometimes ordered data is almost ordered, but not exactly.  Slack allows for
            //error in the ordered data.  For example, in network packets - packets arive in order
            //usually within +/- 2 sec
            private int slack = 0;
            private bool fEOFLeft = false, fEOFRight = false;
            DataItem diLeftCache = null, diRightCache = null;

            /// <summary>
            /// Default constructor for OpJoinMerge
            /// </summary>
            public MergeJoin() : base() { }

            /// <summary>
            /// Constructor for a new Join object, given some predicate and data sources
            /// </summary>
            /// <param name="pred">The predicate to filter on</param>
            /// <remarks>The predicate accepts =, !=, &lt;,&gt;, &lt;=, &gt;=, AND, OR, and NOT.</remarks>
            /// <remarks>To compare with attributes in a data item, you first give '$', followed
            /// by the DataItem number (always '1' or '2' for join), and then the attribute number (1-based)</remarks>
            /// <example>"$1.4 = $2.1" finds all data items that match on the fourth attribute equals the first attribute</example>
            public MergeJoin(string pred)
            {
                Init(pred);
            }

            /// <summary> How many data items are curently held in state </summary>
            public override int StateSize { get { return lLeft.Count + lRight.Count; } }

            /// <summary>Step functionality for merge join</summary>
            public override Step FnStepLeft { get { return JStepLeft; } }
            /// <summary>Step functionality for merge join</summary>
            public override Step FnStepRight { get { return JStepRight; } }
            /// <summary>StepList functionality for merge join</summary>
            public override StepList FnStepListLeft { get { return JStepListLeft; } }
            /// <summary>StepList functionality for merge join</summary>
            public override StepList FnStepListRight { get { return JStepListRight; } }
            /// <summary>Keep functionality for merge join</summary>
            public override Keep FnKeepLeft { get { return JKeepLeft; } }
            /// <summary>Keep functionality for merge join</summary>
            public override Keep FnKeepRight { get { return JKeepRight; } }
            /// <summary>Prop functionality for merge join</summary>
            public override Prop FnPropLeft { get { return JPropLeft; } }
            /// <summary>Prop functionality for merge join</summary>
            public override Prop FnPropRight { get { return JPropRight; } }
            /// <summary>Final functionality for merge join</summary>
            public override Final FnFinalLeft { get { return JFinalLeft; } }
            /// <summary>Final functionality for merge join</summary>
            public override Final FnFinalRight { get { return JFinalRight; } }

            private void Init(string pred)
            {
                expr = ExprParser.Parse(pred);
                FindJoinAttributes(pred);
            }

            /// <summary>
            /// Find the attributes that are participating in this join
            /// </summary>
            /// <param name="pred">The predicate for this join</param>
            private void FindJoinAttributes(string pred)
            {
                int lTemp = -1, rTemp = -1;
                bool fEq = false;
                List<int> alLeft = new List<int>(), alRight = new List<int>();
                for (int i = 0; i < pred.Length; i++)
                {
                    while (i < pred.Length && pred[i] != '$')
                        i++;
                    if (i < pred.Length)
                    {
                        //We've found an attribute to work with
                        i = GetAttribute(pred, i, ref lTemp, ref rTemp);

                        while (pred[i] == ' ')
                            i++;

                        if (pred[i] == '=')
                            fEq = true;

                        while (pred[i] != ' ')
                            i++;
                        while (pred[i] == ' ')
                            i++;

                        if (fEq && pred[i] == '$')
                        {
                            i = GetAttribute(pred, i, ref lTemp, ref rTemp);
                            alLeft.Add(lTemp);
                            alRight.Add(rTemp);
                        }

                        fEq = false;
                    }
                }

                attrsLeft = new int[alLeft.Count];
                attrsRight = new int[alRight.Count];
                alLeft.CopyTo(attrsLeft);
                alRight.CopyTo(attrsRight);
            }

            private int GetAttribute(string pred, int i, ref int l, ref int r)
            {
                while (i < pred.Length && !Char.IsDigit(pred[i]))
                    i++;
                int iAhead = i;
                while (iAhead < pred.Length && Char.IsDigit(pred[iAhead]))
                    iAhead++;
                int iInput = Int32.Parse(pred.Substring(i, iAhead - i));

                i = ++iAhead;
                while (iAhead < pred.Length && Char.IsDigit(pred[iAhead]))
                    iAhead++;

                if (iInput == 1)
                    l = Int32.Parse(pred.Substring(i, iAhead - i)) - 1;
                else
                    r = Int32.Parse(pred.Substring(i, iAhead - i)) - 1;

                return iAhead;
            }

            private void JStepLeft(DataItem di, List<DataItem> ldi)
            {
                if (diLeftCache == null)
                    diLeftCache = di;

                foreach (DataItem diR in lRight)
                {
                    if (expr.Evaluate(di, diR))
                        ldi.Add(Pair(di, diR, GetDataItem));
                }

                if (fEOFRight == false)
                {
                    lLeft.Add(di);
                }
            }

            private void JStepRight(DataItem di, List<DataItem> ldi)
            {
                if (diRightCache == null)
                    diRightCache = di;

                foreach (DataItem diL in lLeft)
                {
                    if (expr.Evaluate(diL, di))
                        ldi.Add(Pair(diL, di, GetDataItem));
                }

                if (fEOFLeft == false)
                {
                    lRight.Add(di);
                }
            }

            private void JStepListLeft(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;

                foreach (DataItem di in rgdi)
                {
                    if (di.EOF) eofInput = true;
                    else
                    {
                        if (diLeftCache == null)
                            diLeftCache = di;

                        foreach (DataItem diR in lRight)
                        {
                            if (expr.Evaluate(di, diR))
                                ldi.Add(Pair(di, diR, GetDataItem));
                        }

                        if (fEOFRight == false)
                        {
                            lLeft.Add(di);
                        }
                    }
                }
            }

            private void JStepListRight(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
            {
                eofInput = false;

                foreach (DataItem di in rgdi)
                {
                    if (di.EOF) eofInput = true;
                    else
                    {
                        if (diRightCache == null)
                            diRightCache = di;

                        foreach (DataItem diL in lLeft)
                        {
                            if (expr.Evaluate(diL, di))
                                ldi.Add(Pair(diL, di, GetDataItem));
                        }

                        if (fEOFLeft == false)
                        {
                            lRight.Add(di);
                        }
                    }
                }
            }

            private void JPropLeft(Punctuation p, List<DataItem> ldi)
            {
                foreach (Punctuation pR in lRightP)
                {
                    if (expr.Evaluate(p, pR))
                        ldi.Add(Pair(p, pR, GetDataItem));
                }

                if (fEOFRight == false)
                {
                    lLeftP.Add(p);
                }
            }

            private void JPropRight(Punctuation p, List<DataItem> ldi)
            {
                foreach (Punctuation pL in lLeftP)
                {
                    if (expr.Evaluate(pL, p))
                        ldi.Add(Pair(pL, p, GetDataItem));
                }

                if (fEOFLeft == false)
                {
                    lRightP.Add(p);
                }
            }

            private void JKeepLeft(Punctuation p)
            {
                if (p.Describes(attrsLeft) && diRightCache != null)
                {
                    if (p.IsAllWildcard)
                    {
                        lRight.Clear();
                    }
                    else
                    {
                        //Set the key that we should match on
                        int key = (int)(((Punctuation.LiteralPattern)p[orderLeft]).Value);
                        for (int i = 0; lRight.Count != 0 && i < lRight.Count && (uint)(lRight[i].GetValue(orderRight)) <= (key + slack); i++)
                        {
                            if ((uint)(lRight[i].GetValue(orderRight)) == key)
                                lRight.RemoveAt(i--);
                        }
                    }
                }
            }

            private void JKeepRight(Punctuation p)
            {
                if (p.Describes(attrsRight) && diLeftCache != null)
                {
                    int count = lLeft.Count;
                    if (p.IsAllWildcard)
                    {
                        lLeft.Clear();
                    }
                    else
                    {
                        //Set the key that we should match on
                        int key = (int)(((Punctuation.LiteralPattern)p[orderRight]).Value);
                        for (int i = 0; lLeft.Count != 0 && i < lLeft.Count && (uint)(lLeft[i].GetValue(orderLeft)) <= (key + slack); i++)
                        {
                            if ((uint)(lLeft[i].GetValue(orderLeft)) == key)
                                lLeft.RemoveAt(i--);
                        }
                    }
                    //if (count != 0 && lLeft.Count != count)
                    //Console.Write("{0} : {1}\n", count, lLeft.Count);
                }
            }

            private void JFinalLeft(List<DataItem> ldi)
            {
                lRight.Clear();
                lRightP.Clear();
                fEOFLeft = true;
            }

            private void JFinalRight(List<DataItem> ldi)
            {
                lLeft.Clear();
                lLeftP.Clear();
                fEOFRight = true;
            }
        }
        #endregion

        #region Symmetric/Range Punctuation Join
        ///<summary>
        /// Models the pipelined double-hash join algorithm, with an enhancement for punctuations. The
        ///  standard symmetric hash join algorithm works well for equality searches, and therefore
        ///  also for equality punctuation matches, so punctuations with literal and list patterns
        ///  on the join attributes work well. However, those kinds of punctuations take up much more
        ///  bandwidth and require many more match calls than a range punctuation will. This algorithm
        ///  must know a priori the incoming punctuation scheme. If the punctuations are range punctuations,
        ///  then a hash table of hash tables is created for each input. Each nested hash table is created
        ///  for data items that fall into one specific punctuation range. When the range punctuation
        ///  arrives, the entire hash table can be removed, rather than individual data items. The hash key
        ///  for the outer hash table for data items is computed using a punctuation with the range for that
        ///  data item.
        ///</summary>
        public class SymmRangePunct : BinaryAlgorithmDefinition
        {
            private IExpr expr;
            // Join attributes for each input
            private int[] attrsLeft, attrsRight;
            // DataItem objects from the left input
            private Dictionary<int, Dictionary<int, List<DataItem>>> htLeft =
                new Dictionary<int, Dictionary<int, List<DataItem>>>();
            private Dictionary<int, List<Punctuation>> htLeftP = new Dictionary<int, List<Punctuation>>();
            // DataItem objects from the right input
            private Dictionary<int, Dictionary<int, List<DataItem>>> htRight =
                new Dictionary<int, Dictionary<int, List<DataItem>>>();
            private Dictionary<int, List<Punctuation>> htRightP = new Dictionary<int, List<Punctuation>>();
            //Converted punctuations to compare to opposite input
            private MetaPunctuation mpLeft = null, mpRight = null;
            private int attrLeftRangePattern, attrRightRangePattern;
            private int rangepatternMin, rangepatternSize;
            private List<DataItem> ldiLeftTemp = new List<DataItem>(), ldiRightTemp = new List<DataItem>();
            private bool fEOFLeft = false, fEOFRight = false;
            DataItem diLeftCache = null, diRightCache = null;
            Punctuation pCache;

            /// <summary>
            /// Default constructor for OpJoinSymmRangePattern
            /// </summary>
            public SymmRangePunct() : base() { }

            /// <summary>
            /// Constructor for a new Join object, given some predicate and data sources
            /// </summary>
            /// <param name="pred">The predicate to filter on</param>
            /// <param name="iLeftRangePattern">Which attribute on the left input will be the range pattern</param> 
            /// <param name="iRightRangePattern">Which attribute on the right input will be the range pattern</param>
            /// <remarks>The predicate accepts =, !=, &lt;,&gt;, &lt;=, &gt;=, AND, OR, and NOT.</remarks>
            /// <remarks>To compare with attributes in a data item, you first give '$', followed
            /// by the DataItem number (always '1' or '2' for join), and then the attribute number (1-based)</remarks>
            /// <example>"$1.4 = $2.1" finds all data items that match on the fourth attribute equals the first attribute</example>
            public SymmRangePunct(string pred, int iLeftRangePattern, int iRightRangePattern)
            {
                Init(pred, iLeftRangePattern, iRightRangePattern);
            }

            private void Init(string pred, int iLeftRangePattern, int iRightRangePattern)
            {
                attrLeftRangePattern = iLeftRangePattern;
                attrRightRangePattern = iRightRangePattern;

                expr = ExprParser.Parse(pred);
                FindJoinAttributes(pred);
            }

            /// <summary>
            /// Property to get and set the attribute for the range pattern for the left input
            /// </summary>
            public int LeftRangeAttribute
            {
                get { return attrLeftRangePattern; }
                set { attrLeftRangePattern = value; }
            }

            /// <summary>
            /// Property to get and set the attribute for the range pattern for the right input
            /// </summary>
            public int RightRangeAttribute
            {
                get { return attrRightRangePattern; }
                set { attrRightRangePattern = value; }
            }

            /// <summary>StepList functionality for symmetric hash/punct join</summary>
            public override StepList FnStepListLeft { get { return JStepListLeft; } }
            /// <summary>StepList functionality for symmetric hash/punct join</summary>
            public override StepList FnStepListRight { get { return JStepListRight; } }
            /// <summary>Pass functionality for symmetric hash/punct join</summary>
            public override Pass FnPassLeft { get { return JPassLeft; } }
            /// <summary>Pass functionality for symmetric hash/punct join</summary>
            public override Pass FnPassRight { get { return JPassRight; } }
            /// <summary>Keep functionality for symmetric hash/punct join</summary>
            public override Keep FnKeepLeft { get { return JKeepLeft; } }
            /// <summary>Keep functionality for symmetric hash/punct join</summary>
            public override Keep FnKeepRight { get { return JKeepRight; } }
            /// <summary>Prop functionality for symmetric hash/punct join</summary>
            public override Prop FnPropLeft { get { return JPropLeft; } }
            /// <summary>Prop functionality for symmetric hash/punct join</summary>
            public override Prop FnPropRight { get { return JPropRight; } }
            /// <summary>Final functionality for symmetric hash/punct join</summary>
            public override Final FnFinalLeft { get { return JFinalLeft; } }
            /// <summary>Final functionality for symmetric hash/punct join</summary>
            public override Final FnFinalRight { get { return JFinalRight; } }

            /// <summary>
            /// Find the attributes that are participating in this join
            /// </summary>
            /// <param name="pred">The predicate for this join</param>
            private void FindJoinAttributes(string pred)
            {
                int lTemp = -1, rTemp = -1;
                bool fEq = false;
                List<int> alLeft = new List<int>(), alRight = new List<int>();
                for (int i = 0; i < pred.Length; i++)
                {
                    while (i < pred.Length && pred[i] != '$')
                        i++;
                    if (i < pred.Length)
                    {
                        //We've found an attribute to work with
                        i = GetAttribute(pred, i, ref lTemp, ref rTemp);

                        while (pred[i] == ' ')
                            i++;

                        if (pred[i] == '=')
                            fEq = true;

                        while (pred[i] != ' ')
                            i++;
                        while (pred[i] == ' ')
                            i++;

                        if (fEq && pred[i] == '$')
                        {
                            i = GetAttribute(pred, i, ref lTemp, ref rTemp);
                            alLeft.Add(lTemp);
                            alRight.Add(rTemp);
                        }

                        fEq = false;
                    }
                }

                attrsLeft = new int[alLeft.Count];
                attrsRight = new int[alRight.Count];
                alLeft.CopyTo(attrsLeft);
                alRight.CopyTo(attrsRight);
            }

            /// <summary>
            /// Find the join attributes for the appropriate input
            /// </summary>
            /// <param name="pred">The predicate for this join</param>
            /// <param name="i">The current location to look at in the predicate</param>
            /// <param name="l">The setting if this attribute is for the left input</param>
            /// <param name="r">The setting if this attribute is for the right input</param>
            private int GetAttribute(string pred, int i, ref int l, ref int r)
            {
                while (i < pred.Length && !Char.IsDigit(pred[i]))
                    i++;
                int iAhead = i;
                while (iAhead < pred.Length && Char.IsDigit(pred[iAhead]))
                    iAhead++;
                int iInput = Int32.Parse(pred.Substring(i, iAhead - i));

                i = ++iAhead;
                while (iAhead < pred.Length && Char.IsDigit(pred[iAhead]))
                    iAhead++;

                if (iInput == 1)
                    l = Int32.Parse(pred.Substring(i, iAhead - i)) - 1;
                else
                    r = Int32.Parse(pred.Substring(i, iAhead - i)) - 1;

                return iAhead;
            }

            private void GetInnerHashTables(out Dictionary<int, List<DataItem>> htLeftInner, out Dictionary<int, List<DataItem>> htRightInner,
                                               DataItem di, int attr)
            {
                int valueDI = Utility.Util.ReturnInt(di[attr]);
                int patternMin = (((valueDI - rangepatternMin) / rangepatternSize) * rangepatternSize) + 1;
                int patternMax = patternMin + (rangepatternSize - 1);
                if (pCache == null)
                {
                    pCache = new Punctuation(1);
                    pCache.AddValue(new Punctuation.RangePattern(patternMin, patternMax));
                }
                else
                {
                    Punctuation.RangePattern rp = (Punctuation.RangePattern)pCache[0];
                    if ((int)rp.MinValue != patternMin || (int)rp.MaxValue != patternMax)
                        pCache[0] = new Punctuation.RangePattern(patternMin, patternMax);
                }

                int hcOuter = pCache.GetSpecificHashCode(0);
                if (htLeft.ContainsKey(hcOuter))
                    htLeftInner = htLeft[hcOuter];
                else
                {
                    htLeftInner = new Dictionary<int, List<DataItem>>();
                    htLeft.Add(hcOuter, htLeftInner);
                }
                if (htRight.ContainsKey(hcOuter))
                    htRightInner = htRight[hcOuter];
                else
                {
                    htRightInner = new Dictionary<int, List<DataItem>>();
                    htRight.Add(hcOuter, htRightInner);
                }
            }

            private Punctuation Convert(Punctuation pThis, DataItem diCache,
                int[] attrsThis, int[] attrsOther)
            {
                Punctuation pRet = new Punctuation(diCache.Count);

                for (int i = 0; i < diCache.Count; i++)
                    pRet.AddValue(new Punctuation.WildcardPattern());

                for (int i = 0; i < attrsOther.Length; i++)
                    pRet[attrsOther[i]] = pThis[attrsThis[i]];

                return pRet;
            }

            /// <summary>
            /// Step functionality for Join -- output all matches from the right input
            /// </summary>
            /// <param name="rgdi">The input DataItems</param>
            /// <param name="ldi">The result data items to be output</param>
            /// <param name="eofInput">Whether we've hit EOF</param>
            /// <seealso cref="QueryEngine.Step"/>
            public void JStepListLeft(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
            {
                List<DataItem> al = null;
                eofInput = false;

                if (ldiLeftTemp != null)
                {
                    //Temporarily hold data items until the first punctuation arrives
                    ldiLeftTemp.AddRange(rgdi);
                    return;
                }

                foreach (DataItem di in rgdi)
                {
                    al = null;
                    if (di.EOF) eofInput = true;
                    else
                    {
                        if (diLeftCache == null)
                            diLeftCache = di;

                        //First, determine which HashTable to put the data item into by calculating
                        // the range punctuation that would match it.
                        Dictionary<int, List<DataItem>> htLeftInner, htRightInner;
                        GetInnerHashTables(out htLeftInner, out htRightInner, di, attrLeftRangePattern);

                        //Now, store the data item in the appropriate hash table
                        int hc = di.GetSpecificHashCode(attrsLeft);
                        if (fEOFRight == false)
                        {
                            //Check if this data item matches any punctuations that have arrived from the right
                            bool fPunct = (mpLeft != null) ? mpLeft.Match(di) : false;

                            //Don't hold the data in state if it is already matching a punctuation from the right
                            if (fPunct == false)
                            {
                                if (htLeftInner.ContainsKey(hc))
                                    al = htLeftInner[hc];
                                if (al == null)
                                {
                                    al = new List<DataItem>();
                                    htLeftInner.Add(hc, al);
                                }
                                al.Add(di);
                            }
                        }

                        //Finally, probe the other hash table and check for joins
                        al = null;
                        if (htRightInner.ContainsKey(hc))
                            al = htRightInner[hc];
                        for (int i = 0; al != null && i < al.Count; i++)
                        {
                            DataItem diR = (DataItem)al[i];
                            if (expr.Evaluate(di, diR))
                                ldi.Add(Pair(di, diR, GetDataItem));
                        }
                    }
                }
            }

            /// <summary>
            /// Step functionality for Join -- output all matches from the left input
            /// </summary>
            /// <param name="rgdi">The input DataItems</param>
            /// <param name="ldi">The resulting data items to be output</param>
            /// <param name="eofInput">Whether we've hit EOF</param>
            /// <seealso cref="QueryEngine.Step"/>
            public void JStepListRight(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
            {
                List<DataItem> al = null;
                eofInput = false;

                if (ldiRightTemp != null)
                {
                    //Temporarily hold data items until the first punctuation arrives
                    ldiRightTemp.AddRange(rgdi);
                    return;
                }

                foreach (DataItem di in rgdi)
                {
                    al = null;
                    if (di.EOF) eofInput = true;
                    else
                    {
                        if (diRightCache == null)
                            diRightCache = di;

                        //First, determine which HashTable to put the data item into by calculating
                        // the range punctuation that would match it.
                        Dictionary<int, List<DataItem>> htLeftInner, htRightInner;
                        GetInnerHashTables(out htLeftInner, out htRightInner, di, attrRightRangePattern);

                        //Now, store the data item in the appropriate hash table
                        int hc = di.GetSpecificHashCode(attrsRight);
                        if (fEOFRight == false)
                        {
                            //Check if this data item matches any punctuations that have arrived from the left
                            bool fPunct = (mpRight != null) ? mpRight.Match(di) : false;

                            //Don't hold the data in state if it is already matching a punctuation from the right
                            if (fPunct == false)
                            {
                                if (htRightInner.ContainsKey(hc))
                                    al = htRightInner[hc];
                                if (al == null)
                                {
                                    al = new List<DataItem>();
                                    htRightInner.Add(hc, al);
                                }
                                al.Add(di);
                            }
                        }

                        //Finally, probe the other hash table and check for joins
                        al = null;
                        if (htLeftInner.ContainsKey(hc))
                            al = htLeftInner[hc];
                        for (int i = 0; al != null && i < al.Count; i++)
                        {
                            DataItem diL = (DataItem)al[i];
                            if (expr.Evaluate(diL, di))
                                ldi.Add(Pair(diL, di, GetDataItem));
                        }
                    }
                }
            }

            /// <summary>
            /// Final functionality for Join -- clear out all hash tables on the right side
            /// </summary>
            /// <param name="ldi">Any final output data (always empty)</param>
            /// <seealso cref="QueryEngine.Final"/>
            public void JFinalLeft(List<DataItem> ldi)
            {
                htRight.Clear();
                htRightP.Clear();
                fEOFLeft = true;
            }

            /// <summary>
            /// Final functionality for Join -- clear out all hash tables on the left side
            /// </summary>
            /// <param name="ldi">Any final output data (always empty)</param>
            /// <seealso cref="QueryEngine.Final"/>
            public void JFinalRight(List<DataItem> ldi)
            {
                htLeft.Clear();
                htLeftP.Clear();
                fEOFRight = true;
            }

            /// <summary>
            /// Pass functionality for join : output results when first punctuation arrives (as per step)
            /// </summary>
            /// <param name="p">The input punctuation</param>
            /// <param name="ldi">Output data items</param>
            public void JPassLeft(Punctuation p, List<DataItem> ldi)
            {
                bool eof;
                if (ldiLeftTemp != null)
                {
                    Punctuation.RangePattern rp = (Punctuation.RangePattern)p[attrLeftRangePattern];
                    rangepatternMin = Utility.Util.ReturnInt(rp.MinValue);
                    rangepatternSize = Utility.Util.ReturnInt(rp.MaxValue) - Utility.Util.ReturnInt(rp.MinValue) + 1;

                    List<DataItem> ldiCache = new List<DataItem>(ldiLeftTemp);
                    ldiLeftTemp = null;
                    JStepListLeft(ldiCache, ref ldi, out eof);
                }
            }

            /// <summary>
            /// Pass functionality for join : output results when first punctuation arrives (as per step)
            /// </summary>
            /// <param name="p">The input punctuation</param>
            /// <param name="ldi">Output data items</param>
            public void JPassRight(Punctuation p, List<DataItem> ldi)
            {
                bool eof;
                if (ldiRightTemp != null)
                {
                    Punctuation.RangePattern rp = (Punctuation.RangePattern)p[attrRightRangePattern];
                    rangepatternMin = Utility.Util.ReturnInt(rp.MinValue);
                    rangepatternSize = Utility.Util.ReturnInt(rp.MaxValue) - Utility.Util.ReturnInt(rp.MinValue) + 1;

                    List<DataItem> ldiCache = new List<DataItem>(ldiRightTemp);
                    ldiRightTemp = null;
                    JStepListRight(ldiCache, ref ldi, out eof);
                }
            }

            /// <summary>
            /// Keep functionality for Join -- clear out all stored data from the right that matches this punctuation 
            /// </summary>
            /// <param name="p">The punctuation to match</param>
            /// <seealso cref="QueryEngine.Keep"/>
            public void JKeepLeft(Punctuation p)
            {
                if (p.Describes(attrsLeft))
                {
                    //Only support range pattern on the join attribute, since this is a punctuation-specific
                    // implementation of join.
                    if (p[attrLeftRangePattern] is Punctuation.RangePattern)
                    {
                        int hc = p.GetSpecificHashCode(attrLeftRangePattern);
                        if (htRight.ContainsKey(hc))
                            htRight.Remove(hc);
                        else
                            Console.WriteLine("Left: No matching hash table for {0}", p);
                    }
                }
                else
                {
                    //Only support the all wildcard punctuation, since this is a punctuation-specific implementation
                    // of join
                    if (p.IsAllWildcard)
                        htRight.Clear();
                }

                if (diRightCache != null)
                {
                    if (mpRight == null)
                        mpRight = new MetaPunctuation(diRightCache.Count);

                    mpRight.Include(Convert(p, diRightCache, attrsLeft, attrsRight));
                }
            }

            /// <summary>
            /// Keep functionality for Join -- clear out all stored data from the left that matches this punctuation 
            /// </summary>
            /// <param name="p">The punctuation to match</param>
            /// <seealso cref="QueryEngine.Keep"/>
            public void JKeepRight(Punctuation p)
            {
                if (p.Describes(attrsRight))
                {
                    //Only support range pattern on the join attribute, since this is a punctuation-specific
                    // implementation of join.
                    if (p[attrRightRangePattern] is Punctuation.RangePattern)
                    {
                        int hc = p.GetSpecificHashCode(attrRightRangePattern);
                        if (htLeft.ContainsKey(hc))
                            htLeft.Remove(hc);
                        else
                            Console.WriteLine("Right: No matching hash table for {0}", p);
                    }
                }
                else
                {
                    //Only support the all wildcard punctuation, since this is a punctuation-specific implementation
                    // of join
                    if (p.IsAllWildcard)
                        htLeft.Clear();
                }

                if (diLeftCache != null)
                {
                    if (mpLeft == null)
                        mpLeft = new MetaPunctuation(diLeftCache.Count);

                    mpLeft.Include(Convert(p, diLeftCache, attrsRight, attrsLeft));
                }
            }

            /// <summary>
            /// Propagate functionality for Join -- output any joined punctuations 
            /// </summary>
            /// <param name="p">The punctuation to match from the left</param>
            /// <param name="ldi">Any output punctuations</param>
            /// <seealso cref="QueryEngine.Prop"/>
            public void JPropLeft(Punctuation p, List<DataItem> ldi)
            {
                List<Punctuation> al = null;

                int hc = p.GetSpecificHashCode(attrsLeft);
                if (fEOFRight == false)
                {
                    if (htLeftP.ContainsKey(hc))
                        al = htLeftP[hc];
                    if (al == null)
                    {
                        al = new List<Punctuation>();
                        htLeftP.Add(hc, al);
                    }
                    al.Add(p);
                }

                al = null;
                if (htRightP.ContainsKey(hc))
                    al = htRightP[hc];
                for (int i = 0; al != null && i < al.Count; i++)
                {
                    Punctuation pR = (Punctuation)al[i];
                    if (expr.Evaluate(p, pR))
                    {
                        ldi.Add(Pair(p, pR, GetDataItem));
                        if (mpLeft != null)
                            mpLeft.Remove(Convert(pR, diLeftCache, attrsRight, attrsLeft));
                        if (mpRight != null)
                            mpRight.Remove(Convert(p, diRightCache, attrsLeft, attrsRight));
                    }
                }
            }

            /// <summary>
            /// Propagate functionality for Join -- output any joined punctuations 
            /// </summary>
            /// <param name="p">The punctuation to match from the right</param>
            /// <param name="ldi">Any output punctuations</param>
            /// <seealso cref="QueryEngine.Prop"/>
            public void JPropRight(Punctuation p, List<DataItem> ldi)
            {
                List<Punctuation> al = null;

                int hc = p.GetSpecificHashCode(attrsRight);
                if (fEOFLeft == false)
                {
                    if (htRightP.ContainsKey(hc))
                        al = htRightP[hc];
                    if (al == null)
                    {
                        al = new List<Punctuation>();
                        htRightP.Add(hc, al);
                    }
                    al.Add(p);
                }

                al = null;
                if (htLeftP.ContainsKey(hc))
                    al = htLeftP[hc];
                for (int i = 0; al != null && i < al.Count; i++)
                {
                    Punctuation pL = (Punctuation)al[i];
                    if (expr.Evaluate(pL, p))
                    {
                        ldi.Add(Pair(pL, p, GetDataItem));
                        if (mpLeft != null)
                            mpLeft.Remove(Convert(p, diLeftCache, attrsRight, attrsLeft));
                        if (mpRight != null)
                            mpRight.Remove(Convert(pL, diRightCache, attrsLeft, attrsRight));
                    }
                }
            }
        }
        #endregion
    }
	#endregion
}
