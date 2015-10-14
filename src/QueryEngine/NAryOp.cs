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
    public class OpNUnion : NAryOp
    {
        private List<Punctuation>[] liPuncts;

        /// <summary>
        /// Default constructor for Union
        /// </summary>
        public OpNUnion()
            : base()
        {
            Init();
        }

        /// <summary> Constructor for Union </summary>
        /// <param name="ops">The input query operators</param>
        public OpNUnion(params Query[] ops)
            : base(ops)
        {
            Init();
        }

        /// <summary> Constructor for Union </summary>
        /// <param name="id">ID for this operator</param>
        /// <param name="ops">The input query operators</param>
        /// <remarks>Should only be used with deserialization</remarks>
        public OpNUnion(string id, params Query[] ops)
            : base(ops)
        {
            opID = Int32.Parse(id.Substring(id.IndexOf('=') + 1));

            Init();
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - NUnion({1})", base.ToString(), Inputs.Count);
        }

        private void Init()
        {
            liPuncts = new List<Punctuation>[Inputs.Count];
            for (int i = 0; i < Inputs.Count; i++)
            {
                liPuncts[i] = new List<Punctuation>();
                StepListFunctions[i] = UStepList;
                PropFunctions[i] = UProp;
            }
        }

        /// <summary> Serialize this Union operator </summary>
        /// <param name="tw">Destination for XML data</param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);
            tw.Write(" />\n");
        }

        /// <summary>
        /// Step functionality for Union -- when a DataItem arrives, output it
        /// </summary>
        /// <param name="di">The input DataItem</param>
        /// <param name="ldi">The DataItem that was input</param>
        /// <seealso cref="QueryEngine.Step"/>
        public void UStep(DataItem di, List<DataItem> ldi)
        {
            ldi.Add(di);
        }

        /// <summary>
        /// StepList functionality for Union -- when a DataItem arrives, output it
        /// </summary>
        /// <param name="rgdi">The input DataItem</param>
        /// <param name="ldi">The DataItem that was input</param>
        /// <param name="eofInput">Whether we've hit eof</param>
        /// <seealso cref="QueryEngine.Step"/>
        public void UStepList(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
        {
            eofInput = false;
            foreach (DataItem di in rgdi)
            {
                if (di.EOF == false)
                    ldi.Add(di);
                eofInput |= di.EOF;
            }
        }

        /// <summary>
        /// Propagation functionality for inputs of union
        /// </summary>
        /// <param name="p">Punctuation from the input</param>
        /// <param name="ldi">Any punctuations that can be output</param>
        /// <seealso cref="QueryEngine.Prop"/>
        public void UProp(Punctuation p, List<DataItem> ldi)
        {
            bool fCoerced = false;
            Punctuation pNew = null, pProp;

            //First, coerce this punctuation with those seen on this input before
            for (int i = 0; i < liPuncts[CurrentInput].Count && !fCoerced; i++)
            {
                Punctuation pTmp = Punctuation.Coerce(p, liPuncts[CurrentInput][i]);
                if (pTmp != null)
                {
                    liPuncts[CurrentInput][i] = pTmp;
                    fCoerced = true;
                    pNew = pTmp;
                }
            }
            if (!fCoerced)
            {
                liPuncts[CurrentInput].Add(p);
                pNew = p;
            }
            
            //Now, see if we can combine a punctuation from punctuations from all inputs
            pProp = CombineFromLists(liPuncts, 0, pNew);

            if (pProp != null)
            {
                //Before we output, we need to remove the combined punctuation from the existing
                // punctuations, so that they don't grow huge
                for (int i = 0; i < liPuncts.Length; i++)
                {
                    for (int j = 0; j < liPuncts[i].Count; j++)
                    {
                        Punctuation pUpdate = Punctuation.Uncoerce(liPuncts[i][j], pProp);
                        if (pUpdate == null)
                            liPuncts[i].RemoveAt(j--);
                        else
                            liPuncts[i][j] = pUpdate;
                    }
                }
                
                ldi.Add(pProp);
            }
        }

        private Punctuation CombineFromLists(List<Punctuation>[] rgliPunct, int iCurrent, Punctuation pCombine)
        {
            if (iCurrent == rgliPunct.Length)
                return pCombine;
            for (int i = 0; i < rgliPunct[iCurrent].Count && i != CurrentInput; i++)
            {
                Punctuation pTmp = Punctuation.Combine(pCombine, rgliPunct[iCurrent][i]);
                if (pTmp != null)
                {
                    pTmp = CombineFromLists(rgliPunct, iCurrent + 1, pTmp);
                    if (pTmp != null)
                        return pTmp;
                }
            }
            
            //If we get out of the loop, then we couldn't combine punctutations to a valid punctuation.
            return null;
        }
    }
    #endregion

    #region Eddy
    /// <summary>
    /// Models the query operator Eddy
    /// </summary>
    /// <remarks>Since this operator is non-blocking, it uses the trivial final function</remarks>
    /// <seealso cref="QueryEngine.Query.FinalTrivial"/>
    public class OpEddy : NAryOp
    {
        private class EddyDataItems
        {
            private List<DataItem> ldi;
            private List<bool> ready;
            private List<bool> done;

            public EddyDataItems(DataItem diIn, int cEddyParticipants)
            {
                ldi = new List<DataItem>();
                ldi.Add(diIn);
                ready = new List<bool>(cEddyParticipants);
                done = new List<bool>(cEddyParticipants);

                for (int i = 0; i < cEddyParticipants; i++)
                {
                    ready.Add(true);    //This data item is ready for whatever operator wants to execute it
                    done.Add(false);    //This data item is not done with any operator
                }
            }

            public List<bool> Ready
            { get { return ready; } }

            public List<bool> Done
            { get { return done; } }

            public List<DataItem> Data
            { 
                get { return ldi; }
                set { ldi = value; }
            }
        }

        private static string XMLPARTICIPANT = "PARTICIPANT";
        private const int CITERATE = 100;   //How many data items should be processed in StepList?

        private Queue<EddyDataItems> queueDataPool;  //The data pool for the eddy
        private List<Query> listOperators;      //The operators that are paricipating in the eddy
        private List<OpQueue> listQueues;       //The inputs to the participant operators
        private int cIterate;

        /// <summary>
        /// Default constructor for Eddy
        /// </summary>
        public OpEddy()
            : base()
        {
            Init(null, CITERATE);
        }

        /// <summary> Constructor for Eddy </summary>
        /// <param name="opParticipants">The operators participating in the eddy</param>
        /// <param name="ops">The input query operators</param>
        public OpEddy(List<Query> opParticipants, params Query[] ops)
            : base(ops)
        {
            Init(opParticipants, CITERATE);
        }

        /// <summary> Constructor for Eddy </summary>
        /// <param name="id">ID for this operator</param>
        /// <param name="opParticipants">The operators participating in the eddy</param>
        /// <param name="ops">The input query operators</param>
        /// <remarks>Should only be used with deserialization</remarks>
        public OpEddy(string id, List<Query> opParticipants, params Query[] ops)
            : base(ops)
        {
            opID = Int32.Parse(id.Substring(id.IndexOf('=') + 1));

            Init(opParticipants, CITERATE);
        }

        /// <summary> Output stats for this operator </summary>
        /// <returns>String with operator-specific stats</returns>
        public override string ToString()
        {
            return string.Format("{0} - Eddy<{1}>", base.ToString(), queueDataPool.Count);
        }

        private void Init(List<Query> opParticipants, int cIter)
        {
            listOperators = new List<Query>(opParticipants);
            listQueues = new List<OpQueue>(opParticipants.Count);
            queueDataPool = new Queue<EddyDataItems>();
            cIterate = cIter;

            for (int i = 0; i < opParticipants.Count; i++)
            {
                listQueues.Add(new OpQueue(cIterate));
                //For now, assume unary operators only
                ((UnaryOp)listOperators[i]).Input = listQueues[i];
            }

            for (int i = 0; i < Inputs.Count; i++)
            {
                StepListFunctions[i] = EStepList;
                KeepFunctions[i] = EKeep;
            }
        }

        /// <summary> Serialize this Eddy operator </summary>
        /// <param name="tw">Destination for XML data</param>
        public override void SerializeOp(TextWriter tw)
        {
            base.SerializeOp(tw);
            for (int i=0; i<listOperators.Count; i++)
            {
                tw.Write("{0}{1}={2} ", XMLPARTICIPANT, i, listOperators[i].OpID);
            }
            tw.Write(" />\n");

            foreach (Query q in listOperators)
                q.SerializeOp(tw);
        }

        /// <summary>
        /// StepList functionality for Eddy
        /// </summary>
        /// <param name="rgdi">The input DataItem</param>
        /// <param name="ldi">The DataItem that was input</param>
        /// <param name="eofInput">Whether we've hit eof</param>
        /// <seealso cref="QueryEngine.Step"/>
        public void EStepList(List<DataItem> rgdi, ref List<DataItem> ldi, out bool eofInput)
        {
            List<DataItem> ldiIterate = new List<DataItem>();
            eofInput = false;

            //First, add incoming data items to the data pool
            foreach (DataItem di in rgdi)
                queueDataPool.Enqueue(new EddyDataItems(di, listOperators.Count));

            //Now process data items in the queue for output
            for (int i = 0; i < (cIterate * listOperators.Count) && queueDataPool.Count != 0; i++)
            {
                EddyDataItems edi = queueDataPool.Dequeue();
                List<DataItem> ldiEddy = null;
                int op;

                for (op = 0; op < edi.Ready.Count && ldiEddy == null; op++)
                {
                    if (edi.Ready[op])
                        ldiEddy = edi.Data;
                }

                if (ldiEddy != null)
                {
                    op--;
                    listQueues[op].Push(ldiEddy);
                    ldiIterate = listOperators[op].Iterate(null, null);
                    if (ldiIterate != null && ldiIterate.Count != 0)
                    {
                        edi.Ready[op] = false;
                        edi.Done[op] = true;
                    }

                    bool output = true;
                    for (int j = 0; j < edi.Done.Count && output; j++)
                        output &= edi.Done[j];

                    if (output)
                        ldi.AddRange(ldiIterate);
                    else if (ldiIterate.Count != 0)
                    {
                        edi.Data = new List<DataItem>(ldiIterate);
                        queueDataPool.Enqueue(edi);
                    }
                }
            }

            foreach (DataItem di in ldi)
                eofInput |= di.EOF;
        }

        /// <summary>
        /// Function to handle punctuations as they arrive in the eddy.
        /// In this case, we simply enqueue the punctuation as if it were
        /// a data item, and the Iterate function for each operator will
        /// process it appropriately
        /// </summary>
        /// <param name="p">The incoming punctuation</param>
        public void EKeep(Punctuation p)
        {
            //All we need to do is treat the punctuation as if it were a data item.
            // Just enqueue it, and let the Iterate function for each participating
            // operator process it as usual.
            queueDataPool.Enqueue(new EddyDataItems(p, listOperators.Count));
        }
    }
    #endregion

    #region Box
    /// <summary>
    /// Box up a group of operators into a single operator, using a single queue
    /// </summary>
    // TODO: Need to implement this operator
    // It needs to be able to dynamically add/remove operators
    public class Box : NAryOp
    {
        /// <summary>
        /// default Box operator constructor
        /// </summary>
        public Box() : base() {}

        /// <summary>
        /// Box operator constructor
        /// </summary>
        /// <param name="qInputs">The input operators</param>
        public Box(params Query[] qInputs) : base(qInputs)
        {
        }
    }
    #endregion
}