using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using WhitStream.Data;
using WhitStream.QueryEngine.QueryOperators;
using WhitStream.Utility;

namespace WhitStream.QueryEngine.Scheduler
{
	#region Delegates
	/// <summary>
    /// Notifies listeners that something has occurred related to data (e.g. new results, EOF)
    /// </summary>
    public delegate void DataEvent(IResults r);
	#endregion

	#region IResults
	/// <summary>
    /// Interface to get results during the execution of a specific query
    /// </summary>
    public interface IResults
    {
        /// <summary> New data is available </summary>
        event DataEvent DataArrived;

        /// <summary> How much data is currently available </summary>
        int DataCount { get; }

        /// <summary> Return the current results for this query </summary>
        List<DataItem> Results { get; }

        /// <summary>Activate this query</summary>
        void Activate();

        /// <summary>Deactivate this query</summary>
        void Deactivate();

        /// <summary> Has this query run to completion </summary>
        bool EndQuery { get; }
	}
	#endregion

	#region IScheduler
	/// <summary>
    /// Interface to be supported by all schedulers
    /// </summary>
    /// <seealso cref="RoundRobinScheduler"/>
    /// <seealso cref="MonteCarloScheduler"/>
    public interface IScheduler
    {
        /// <summary> Initialize the scheduler to execute the given query </summary>
        /// <param name="qs">The query plans to execute</param>
        /// <param name="breakOutOperators">Should the query operators run in their own thread?</param>
        /// <returns>IResults objects to monitor the queries</returns>
        IResults[] Init(bool breakOutOperators, params Query[] qs);
    	
        /// <summary>Add a query to this scheduler while it's executing</summary>
        /// <param name="breakOutOperators">Should each query operator run in its own thread, or the entire query in a thread?</param>
        /// <param name="qs">The queries to add</param>
        /// <returns>IResults objects to monitor the queries</returns>
    	IResults[] AddQuery(bool breakOutOperators, params Query[] qs);

        /// <summary> Execute the query </summary>
        void Execute();

        /// <summary> Returns true if all data sources have been read to completion </summary>
        bool Complete { get; }

        /// <summary>List the operator threads that make up the queries to be executed </summary>
        List<OpThread> OpThreads { get; }
	}
	#endregion

	#region Operator Thread
	/// <summary> Operator threads for WSThreadPool</summary>
    public class OpThread
    {
        /// <summary>
        /// Constants for determining which queue to read/write
        /// </summary>
        public const int LEFT = 0, RIGHT = 1;
        private Query qop = null;
        private OpQueue[] rgqueue = null;
        private bool leaf = true;
        private OpThread otParent = null;
        private int parentQueue = OpThread.LEFT;
        private bool busy = false;
        private bool active = false;

        /// <summary>
        /// Simple constructor for creating a top-most OperatorThread object for the query plan
        /// </summary>
        public OpThread()
        {
        }

        /// <summary> Initialize this OpThread </summary>
        /// <param name="q">Query operator to execute in this thread</param>
        /// <param name="parent">parent OpThread to write results to</param>
        /// <param name="parentQ">parent queue to write results to</param>
        /// <param name="inputQueues">Should this operator read from input queues?</param>
        public virtual void Init(Query q, OpThread parent, int parentQ, bool inputQueues)
        {
            QueryOperator = q;
            otParent = parent;
            parentQueue = parentQ;
            if (inputQueues)
                SetInputQueues();
        }

        /// <summary> Is this thread done executing? </summary>
        public virtual bool Complete
        { get { return qop.EOF; } }

        /// <summary> The number of data items in a specific thread's queue waiting to be processed </summary>
        /// <param name="q">Which queue (LEFT | RIGHT) </param>
        /// <returns>The count of data items</returns>
        public virtual int QueueSize(int q)
        { return GetQueue(q).DataCount; }

        /// <summary> Does this queue have an EOF item in it? </summary>
        /// <param name="q">Which input queue (LEFT | RIGHT) </param>
        /// <returns>True if that queue contains an EOF item</returns>
        public virtual bool HasEOF(int q)
        { return GetQueue(q).HasEOF; }

        ///<summary> The query operator </summary>
        public virtual Query QueryOperator
        {
            get { return qop; }
            set { qop = value; }
        }

        /// <summary> Is this a leaf operator in the query? </summary>
        public virtual bool Leaf
        { get { return leaf; } }

        /// <summary> Set the parent OpThread for this operator </summary>
        public virtual OpThread OTParent
        {
            get { return otParent; }
            set { otParent = value; }
        }

        /// <summary> Set the destination queue for this operator </summary>
        public virtual int OTParentQueue
        { set { parentQueue = value; } }

        /// <summary> Is this thread already executing? </summary>
        public virtual bool Busy
        { get { return busy; } }

        /// <summary> Is this thread part of an active query? </summary>
        public bool Active
        {
            get { return active; }
            set 
            { 
                active = value;
                if (qop != null)
                {
					if (active)
					{
						qop.Activate();
					}
					else
					{
						qop.Deactivate();
					}
                }
            }
        }

        /// <summary> Set up the input queue(s) for this operator </summary>
        public virtual void SetInputQueues()
        {
            if (qop is UnaryOp)
            {
                UnaryOp u = qop as UnaryOp;
                leaf = (u.Input == null);
                rgqueue = new OpQueue[1];
                rgqueue[0] = new OpQueue();
                u.Input = rgqueue[0];
            }
            else if (qop is BinaryOp)
            {
                BinaryOp b = qop as BinaryOp;
                rgqueue = new OpQueue[2];
                rgqueue[0] = new OpQueue();
                rgqueue[1] = new OpQueue();
                b.LeftInput = rgqueue[0];
                b.RightInput = rgqueue[1];
            }
            else //NAryOp
            {
                NAryOp n = qop as NAryOp;
                rgqueue = new OpQueue[n.Inputs.Count];
                for (int i = 0; i < n.Inputs.Count; i++)
                {
                    rgqueue[i] = new OpQueue();
                    n.Inputs[i] = rgqueue[i];
                }
            }
        }

        /// <summary> How many queues are used for this operator? </summary>
        /// <returns>The total number of input queues</returns>
        public virtual int GetQueueCount()
        { return rgqueue.Length; }

        /// <summary> Get the queue operator for this operator </summary>
        /// <param name="i">Which queue (LEFT | RIGHT)</param>
        /// <returns>The queue input for this operator</returns>
        public virtual OpQueue GetQueue(int i)
        { return rgqueue[i]; }

        /// <summary>
        /// Add data to the parent queue for processing
        /// </summary>
        /// <param name="rgdiNew">The new data to be added</param>
        /// <param name="q">Which queue (LEFT, RIGHT) to write to</param>
        public virtual void AddDataToQueue(List<DataItem> rgdiNew, int q)
        { rgqueue[q].Push(rgdiNew); }

        /// <summary>
        /// Execute the query operator for this thread
        /// </summary>
        public virtual void ExecuteOnce(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
        {
            busy = true;
            lock (qop)
            {
                if (qop.EOF == false)
                    otParent.AddDataToQueue(qop.Iterate(gdi, rdi), parentQueue);

                busy = false;
            }
        }

        /// <summary> Output stats for this thread </summary>
        /// <returns>String with thread-specific stats</returns>
        public override string ToString()
        {
            string st = "";
            if (rgqueue.Length == 1)
                st = string.Format("{0}: {1} queued dataitems", qop.ToString(), rgqueue[0].DataCount);
            else if (rgqueue.Length == 2)
                st = string.Format("{0}: <{1},{2}> queued dataitems", qop.ToString(), rgqueue[0].DataCount, rgqueue[1].DataCount);
            else
            {
                int total = 0;
                foreach (OpQueue oq in rgqueue)
                    total += oq.DataCount;
                st = string.Format("{0}: {1} total queued dataitems", qop.ToString(), total);
            }

            return st;
        }
	}
	#endregion

	#region Parent Operator Thread
	/// <summary>
    /// Operator to be used at the top of query plans
    /// </summary>
    public class ParentOperatorThread : OpThread, IResults
    {
        private OpQueue queue = new OpQueue(Int32.MaxValue);
        private List<OpThread> listChildren = new List<OpThread>();

        /// <summary> Default constructor </summary>
        public ParentOperatorThread() : base() { }

        /// <summary> Initialize this OpThread </summary>
        /// <param name="q">Query to initialize with</param>
        /// <param name="parent">Parent OpThread to write results to</param>
        /// <param name="parentQueue">queue to write results to</param>
        /// <param name="inputQueues">Should this operator read from input queues?</param>
        public override void Init(Query q, OpThread parent, int parentQueue, bool inputQueues) { }

        /// <summary> Is this query done? </summary>
        public override bool Complete { get { return queue.EOF && queue.DataCount == 0; } }
        /// <summary> The number of data items in a specific thread's queue waiting to be processed </summary>
        /// <param name="q">Which queue (LEFT | RIGHT) </param>
        /// <returns>The count of data items</returns>
        public override int QueueSize(int q) { return queue.DataCount; }
        /// <summary> Does this queue have an EOF item in it? </summary>
        /// <param name="q">Which input queue (LEFT | RIGHT) </param>
        /// <returns>True if that queue contains an EOF item</returns>
        public override bool HasEOF(int q) { return queue.HasEOF; }
        /// <summary> Return this query operator </summary>
        public override Query QueryOperator 
        { 
            get { return null; }
            set { }
        }
        /// <summary> Is this query operator a leaf in the query plan? </summary>
        public override bool Leaf { get { return false; } }
        /// <summary> Set up the parent OpThread for this thread. </summary>
        public override OpThread OTParent 
        {
            get { return null; }
            set { }
        }
        /// <summary> Set up the destination queue for this operator </summary>
        public override int OTParentQueue { set { } }
        /// <summary> Set up the input queue(s) for this operator </summary>
        public override void SetInputQueues() { }
        /// <summary> Get the input queue for this operator (LEFT | RIGHT) </summary>
        /// <param name="i"> Which queue (LEFT | RIGHT)? </param>
        /// <returns>The desired OpQueue object</returns>
        public override OpQueue GetQueue(int i) { return queue; }
        /// <summary> Add new data items into this input queue </summary>
        /// <param name="rgdiNew">The data items to add</param>
        /// <param name="q">Which queue to write to (LEFT | RIGHT) </param>
        public override void AddDataToQueue(List<DataItem> rgdiNew, int q)
        {
            queue.Push(rgdiNew);
            if (DataArrived != null) DataArrived(this);
        }
        /// <summary> The thread function to run one time </summary>
        public override void ExecuteOnce(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi) { }

        /// <summary>Add a child to this query</summary>
        /// <param name="ot">The child to add</param>
        public void AddChild(OpThread ot)
        { listChildren.Add(ot); }

        #region IResults interface
        /// <summary> New data is available to be read </summary>
        public event DataEvent DataArrived;

        /// <summary>How much data is available</summary>
        public int DataCount { get { return queue.DataCount; } }

        /// <summary> Return the results for this operator </summary>
        public List<DataItem> Results
        { get { return queue.Iterate(null, null); } }

        /// <summary> Is this query done? </summary>
        public bool EndQuery { get { return Complete; } }

        /// <summary> Activate this query for this operator </summary>
        public void Activate()
        {
            if (Active == false)
            {
                Active = true;
                foreach (OpThread ot in listChildren)
                    ot.Active = true;
            }
        }

        /// <summary> Deactivate this query </summary>
        public void Deactivate()
        {
            if (Active)
            {
                Active = false;
                foreach (OpThread ot in listChildren)
                    ot.Active = false;
            }
        }
        #endregion
	}
	#endregion

	#region WhitStream Threadpool
	/// <summary>
    /// Manage a pool of threads for the scheduler
    /// </summary>
    public class WSThreadPool : IEnumerable<OpThread>
    {
        /// <summary> Function to determine which operator to execute next </summary>
        /// <returns>The operator to execute</returns>
        public delegate int NextOperator();

        /// <summary> Nothing more to do </summary>
        public static int COMPLETE = -1;
        /// <summary> How much history should we keep? </summary>
        public static int THREADHISTORY = 8;
        private List<OpThread> rgOpThread = null;
#if DEBUG
        private int cThreads = 3;
#else
        private int cThreads = 5;
#endif
        private Thread[] rgThread;
        private NextOperator NOp;
        private int[] rgLastExecuted = new int[THREADHISTORY];

        /// <summary>Default constructor for WSThreadPool</summary>
        public WSThreadPool() { }
        /// <summary>Specify how many threads for thread pool</summary>
        /// <param name="threads">how many threads to execute</param>
        public WSThreadPool(int threads) { cThreads = threads; }

        /// <summary> Initialize the pool </summary>
        /// <param name="ots">The OperatorThread objects that are participating</param>
        /// <param name="n">The function to call to get the next operator to execute</param>
        public void Init(List<OpThread> ots, NextOperator n)
        {
            NOp = n;

            rgOpThread = new List<OpThread>(ots);

            rgThread = new Thread[cThreads];

            for (int i = 0; i < THREADHISTORY; i++)
                rgLastExecuted[i] = -1;

            for (int i = 0; i < cThreads; i++)
            {
                ThreadStart tstart = new ThreadStart(ThreadFunction);
                rgThread[i] = new Thread(tstart);
                rgThread[i].Name = string.Format("WhitStreamThread {0}", i);
            }
        }

        /// <summary>Add a query to this scheduler while it's executing</summary>
        /// <param name="ots">The oeprator threads to add`</param>
        public void AddQuery(List<OpThread> ots)
        {
        	lock (this)
        	{
        		rgOpThread.AddRange(ots);
        	}
        }

        /// <summary> Begin query execution </summary>
        public void Start()
        {
            for (int i = 0; i < cThreads; i++)
                rgThread[i].Start();
        }

        private void ThreadFunction()
        {
            int iEvent;
            int iIter = 0;
            bool complete = false;
            DataItemPool dip = new DataItemPool();
            const int DATAPOOLSIZE = 50;
            dip.Init(DATAPOOLSIZE);

            do
            {
                lock (this)
                {
                    iEvent = NOp();
                    UpdateHistory(iEvent);
                }
                if (iEvent != COMPLETE)
                    this[iEvent].ExecuteOnce(dip.GetItem, dip.ReleaseItem);
                iIter = ((iIter + 1) & 0xFF);   //Sleep every 256 iterations
                if (iIter == 0)
                    Thread.Sleep(1);

                //There may not have been a query thread to execute, but it might be that some threads are
                // deactivated. We need to make sure that all threads are complete (and not just deactivated)
                if (iEvent == COMPLETE)
                {
                    complete = true;
                    for (int i = 0; complete && i < rgOpThread.Count; i++)
                        complete &= rgOpThread[i].Complete;
                }
            }
            while (!complete);
        }

        private void UpdateHistory(int iEvent)
        {
            for (int i = THREADHISTORY - 2; i > 0; i--)
                rgLastExecuted[i] = rgLastExecuted[i - 1];

            rgLastExecuted[0] = iEvent;
        }

        /// <summary> What were the last operators executed? </summary>
        /// <param name="i">Index into history (0=Most recent, THREADHISTORY-1=Least Recent</param>
        /// <returns>Which operator was most recently run</returns>
        /// <seealso cref="WSThreadPool.THREADHISTORY"/>
        public int GetLastExecuted(int i)
        {
            if (i < THREADHISTORY) return rgLastExecuted[i];
            else return -1;
        }

        /// <summary> Remove a completed thread from the pool </summary>
        /// <param name="i">The thread to remove</param>
        public void RemoveAt(int i)
        {
            rgOpThread.RemoveAt(i);
        }

        #region Custom Indexing
        /// <summary> Allow for index access to the active threads in the pool </summary>
        /// <param name="iActive">Which thread to access</param>
        /// <returns>That thread</returns>
        public OpThread this[int iActive]
        {
            get
            {
                int i;
                for (i = 0; i < rgOpThread.Count && iActive >= 0; i++)
                    if (rgOpThread[i].Active) iActive--;

                if (iActive < 0) return rgOpThread[i-1];
                else return null;
            }
        }

        /// <summary> How many threads are in the pool? </summary>
        public int Length
        { 
            get 
            {
                int c=0;
                for (int i = 0; i < rgOpThread.Count; i++)
                    if (rgOpThread[i].Active) 
                        c++;

                return c;
            } 
        }
        #endregion

        #region IEnumerable
        /// <summary> Return the enumerator for this class, so we can do foreach </summary>
        /// <returns> this object, which supports IEnumerator</returns>
        public IEnumerator<OpThread> GetEnumerator()
        {
            for (int i = 0; i < rgOpThread.Count; i++)
                if (rgOpThread[i].Active) yield return rgOpThread[i];
        }
        //Lame C# must have this hidden override to support non-generic IEnumerable
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
        #endregion

        #region IDisposable
        /// <summary> Clean up after ourselves </summary>
        public void Dispose() 
        {
            //Wait for the threads to go away
            foreach (Thread t in rgThread)
            {
                while ((t.ThreadState & ThreadState.Stopped) == 0)
                    Thread.Sleep(100);
            }
        }
        #endregion
	}
	#endregion

	#region Scheduler
	/// <summary>
    /// Scheduler abstract class
    /// </summary>
    public abstract class Scheduler : IScheduler
    {
        /// <summary> The OperatorThreads that make up the queries to be executed </summary>
        protected List<OpThread> ots = new List<OpThread>();

        /// <summary>List the operator threads that make up the queries to be executed </summary>
        public List<OpThread> OpThreads { get { return ots; } }

        /// <summary> Initialize the scheduler to execute the given query </summary>
        /// <param name="breakOutOperators">Should the query be broken up so that each operator runs in its own thread?</param>
        /// <param name="qs">The query plan to execute</param>
        public virtual IResults[] Init(bool breakOutOperators, params Query[] qs)
        {
            IResults[] res = BuildPlan(qs, breakOutOperators);
            foreach (IResults ir in res)
                ir.Activate();

            return res;
        }
        
        /// <summary> Add a new query to the existing pool of queries </summary>
        /// <param name="breakOutOperators">Should the query be broken up so that each operator runs in its own thread?</param>
        /// <param name="qs">The query plans to add</param>
        public virtual IResults[] AddQuery(bool breakOutOperators, params Query[] qs)
        {
            IResults[] res = BuildPlan(qs, breakOutOperators);
            foreach (IResults ir in res)
                ir.Activate();

            return res;
        }

        /// <summary> Execute the query </summary>
        public virtual void Execute() { }

        /// <summary> Returns true if all data sources have been read to completion </summary>
        public virtual bool Complete { get { return false; } }

        /// <summary>
        /// Initialize the scheduler with a query. The operators given will define
        /// the query to be executed. A thread pool is created with each operator
        /// and its input queue running in its own thread.
        /// </summary>
        /// <param name="qs">The simple (pull-based) query plans</param>
        /// <param name="fBreakOut">Should we break this query into separate threads?</param>
        public IResults[] BuildPlan(Query[] qs, bool fBreakOut)
        {
            ParentOperatorThread[] otTops = new ParentOperatorThread[qs.Length];
            ots.Clear();

            if (fBreakOut)
            {
                //Each query operator should be executed in its own thread
                for (int i = 0; i < qs.Length; i++)
                {
                    //This top-most queue has no query operator. It will run in the same
                    // thread as the scheduler, and can be read from as results arrive
                    otTops[i] = new ParentOperatorThread();
                    BuildPlan(qs[i], otTops[i], OpThread.LEFT, otTops[i]);
                }
            }
            else
            {
                //Each query is executed in its own thread, but all operators on that query
                // execute in that same thread (pull-based fashion)
                for (int i = 0; i < qs.Length; i++)
                {
                    otTops[i] = new ParentOperatorThread();
                    OpThread ot = new OpThread();
                    ot.Init(qs[i], otTops[i], OpThread.LEFT, false);
                    otTops[i].AddChild(ot);
                    ots.Add(ot);
                }
            }
            return otTops;
        }

        private void BuildPlan(Query op, OpThread opParent, int parentQueue, ParentOperatorThread potTop)
        {
            OpThread ot = null;

            if (op is UnaryOp)
            {
                UnaryOp uop = (UnaryOp)op;
                Query input = uop.Input;
                ot = new OpThread();

                ot.Init(uop, opParent, parentQueue, true);
                potTop.AddChild(ot);
                if (input != null)
                    BuildPlan(input, ot, OpThread.LEFT, potTop);
            }
            else if (op is BinaryOp)
            {
                BinaryOp bop = (BinaryOp)op;
                Query linput = bop.LeftInput, rinput = bop.RightInput;
                ot = new OpThread();
                ot.Init(bop, opParent, parentQueue, true);
                potTop.AddChild(ot);

                if (linput != null)
                    BuildPlan(linput, ot, OpThread.LEFT, potTop);
                if (rinput != null)
                    BuildPlan(rinput, ot, OpThread.RIGHT, potTop);
            }
            else //NAryOp
            {
                NAryOp nop = (NAryOp)op;
                ot = new OpThread();
                Query[] inputs = new Query[nop.Inputs.Count];
                for (int i = 0; i < nop.Inputs.Count; i++)
                    inputs[i] = nop.Inputs[i];
                ot.Init(nop, opParent, parentQueue, true);
                potTop.AddChild(ot);

                for (int i = 0; i < inputs.Length; i++)
                {
                    if (inputs[i] != null)
                        BuildPlan(inputs[i], ot, i, potTop);
                }
            }

            ots.Add(ot);
        }

        /// <summary>String representation of this scheduler</summary>
        /// <returns>String representation of this scheduler</returns>
        public override string ToString()
        {
            return "Generic Scheduler";
        }
	}
	#endregion
}
