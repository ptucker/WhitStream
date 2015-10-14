using System;
using System.Collections.Generic;
using System.Threading;
using WhitStream.Data;
using WhitStream.QueryEngine.QueryOperators;

namespace WhitStream.QueryEngine.Scheduler
{
    /// <summary>
    /// Represents the thread context for each operator
    /// </summary>
    public class OperatorThread : OpThread
    {
        private Query qop = null;
        private OpQueue[] rgqueue = new OpQueue[2];
        private Thread thread = null;
        private bool leaf = true;
        private OpThread otParent = null;
        private int parentQueue = OpThread.LEFT;

        /// <summary>
        /// Simple constructor for creating a top-most OperatorThread object for the query plan
        /// </summary>
        public OperatorThread()
        {
            rgqueue[0] = new OpQueue();
            rgqueue[1] = new OpQueue();
        }

        /// <summary> Initialize this OpThread </summary>
        /// <param name="q">Query operator to execute in this thread</param>
        /// <param name="parent">parent OpThread to write results to</param>
        /// <param name="parentQ">parent queue to write results to</param>
        /// <param name="inputQueues">Should this operator read from input queues?</param>
        public override void Init(Query q, OpThread parent, int parentQ, bool inputQueues)
        {
            QueryOperator = q;
            otParent = parent;
            parentQueue = parentQ;
            if (inputQueues)
                SetInputQueues();
            thread = new Thread(Execute);
        }

        /// <summary>The thread executing this operator</summary>
        public override Thread OperatingThread
        { get { return thread; } }

        /// <summary> Is this thread asleep? </summary>
        public override bool Sleeping
        { get { return (thread.ThreadState & ThreadState.WaitSleepJoin) != 0; } }

        /// <summary> Is this thread done executing? </summary>
        public override bool Complete
        { get { return qop.EOF || (thread.ThreadState & ThreadState.Stopped) != 0; } }

        /// <summary> Wake this thread up </summary>
        public override void FireThread()
        { thread.Interrupt(); }

        /// <summary> The number of data items in a specific thread's queue waiting to be processed </summary>
        /// <param name="q">Which queue (LEFT | RIGHT) </param>
        /// <returns>The count of data items</returns>
        public override int QueueSize(int q)
        { return GetQueue(q).DataCount; }

        /// <summary> Does this queue have an EOF item in it? </summary>
        /// <param name="q">Which input queue (LEFT | RIGHT) </param>
        /// <returns>True if that queue contains an EOF item</returns>
        public override bool HasEOF(int q)
        { return GetQueue(q).HasEOF; }
        
        ///<summary> The query operator </summary>
        public override Query QueryOperator
        { 
            get { return qop; }
            set { qop = value; }
        }

        /// <summary> Is this a leaf operator in the query? </summary>
        public override bool Leaf
        { get { return leaf; } }

        /// <summary> Set the parent OpThread for this operator </summary>
        public override OpThread OTParent
        {
            get { return otParent; }
            set { otParent = value; } 
        }

        /// <summary> Set the destination queue for this operator </summary>
        public override int OTParentQueue
        { set { parentQueue = value; } }

        /// <summary> Set up the input queue(s) for this operator </summary>
        public override void SetInputQueues()
        {
            if (qop is UnaryOp)
            {
                UnaryOp u = qop as UnaryOp;
                leaf = (u.Input == null);
                u.Input = rgqueue[0];
            }
            else if (qop is BinaryOp)
            {
                BinaryOp b = qop as BinaryOp;
                b.LeftInput = rgqueue[0];
                b.RightInput = rgqueue[1];
            }
        }

        /// <summary> Get the queue operator for this operator </summary>
        /// <param name="i">Which queue (LEFT | RIGHT)</param>
        /// <returns>The queue input for this operator</returns>
        public override OpQueue GetQueue(int i)
        { return rgqueue[i]; }

        /// <summary>
        /// Add data to the parent queue for processing
        /// </summary>
        /// <param name="rgdiNew">The new data to be added</param>
        /// <param name="q">Which queue (LEFT, RIGHT) to write to</param>
        public override void AddDataToQueue(List<DataItem> rgdiNew, int q)
        { rgqueue[q].Push(rgdiNew); }

        /// <summary> Fire off this thread </summary>
        public override void Start()
        {
            thread = new Thread(new ThreadStart(Execute));
            thread.Name = string.Format("Thread: {0}", qop.ToString());
            thread.Priority = ThreadPriority.AboveNormal;
            thread.Start();
        }

        /// <summary>
        /// Execute the query operator for this thread
        /// </summary>
        public override void Execute()
        {
            while (qop.EOF == false)
            {
                otParent.AddDataToQueue(qop.Iterate(), parentQueue);

                if (qop.EOF == false)
                {
                    try { Thread.Sleep(System.Threading.Timeout.Infinite); }
                    catch (ThreadInterruptedException) { ; }
                }
            }
        }

        /// <summary>
        /// Execute the query operator for this thread
        /// </summary>
        public override void ExecuteOnce()
        {
            lock (this)
            {
                if (qop.EOF == false)
                    otParent.AddDataToQueue(qop.Iterate(), parentQueue);
            }
        }
    }

    /// <summary>
    /// A multi-threaded scheduler for executing a query using a round-robin algorithm
    /// </summary>
    public class RoundRobinScheduler : Scheduler<OperatorThread>
    {
        WSThreadPoolNew<OperatorThread> pool = new WSThreadPoolNew<OperatorThread>();
        bool complete = false;
        int iNextOp = 0;

        /// <summary> Initialize the scheduler to execute the given query </summary>
        /// <param name="breakOut">Should the query operators run in their own thread?</param>
        /// <param name="qs">The query plans to execute</param>
        public override IResults[] Init(bool breakOut, params Query[] qs)
        {
            IResults[] res = base.Init(breakOut, qs);
            pool.Init(ots, Thread.CurrentThread, NextOperatorRoundRobin);

            return res;
        }

        /// <summary> Execute the query </summary>
        public override void Execute()
        {
            pool.Start();
        }

        private int NextOperatorRoundRobin()
        {
            int iRet = WSThreadPoolNew<OperatorThread>.COMPLETE;

            lock (this)
            {
                for (int i = 0; iRet == WSThreadPoolNew<OperatorThread>.COMPLETE && i < pool.Length; i++)
                {
                    iNextOp = (iNextOp + 1) % pool.Length;
                    if (pool[iNextOp].Complete == false)
                        iRet = iNextOp;
                }
            }

            return iRet;
        }

        /// <summary> Returns true if all data sources have been read to completion </summary>
        public override bool Complete
        { get { return complete; } }
    }

    /// <summary>
    /// A multi-threaded scheduler for executing a query using a round-robin algorithm
    /// </summary>
    public class MonteCarloScheduler : Scheduler<OperatorThread>
    {
        /// <summary> Points calculator for specific operator </summary>
        /// <param name="iOp">The operator to calculate</param>
        /// <returns>That operator's points</returns>
        public delegate int MCPoints(int iOp);

        WSThreadPoolNew<OperatorThread> pool = new WSThreadPoolNew<OperatorThread>();
        bool complete = false;
        int[] points = null;
        Random rnd = new Random(DateTime.Now.Millisecond);
        MCPoints Mcp;

        /// <summary> Point calculator based on queue size </summary>
        /// <param name="iOp">The operator to calculate</param>
        /// <returns>The points for that operator</returns>
        public int MCPQueueSize(int iOp)
        {
            return (pool[iOp].Leaf ? 50000 :
                        pool[iOp].QueueSize(OpThread.LEFT) + pool[iOp].QueueSize(OpThread.RIGHT) +
                        ((pool[iOp].HasEOF(OpThread.LEFT) || pool[iOp].HasEOF(OpThread.RIGHT)) ? 10000 : 0));
        }

        /// <summary> Initialize the scheduler to execute the given query </summary>
        /// <param name="breakOut">Should the query operators run in their own thread?</param>
        /// <param name="qs">The query plans to execute</param>
        public override IResults[] Init(bool breakOut, params Query[] qs)
        {
            Mcp = MCPQueueSize;
            IResults[] res = base.Init(breakOut, qs);
            pool.Init(ots, Thread.CurrentThread, NextOperatorMonteCarlo);
            points = new int[pool.Length];
            for (int i = 0; i < points.Length; i++)
                points[i] = 0;

            return res;
        }

        /// <summary> Execute the query </summary>
        public override void Execute()
        {
            pool.Start();
        }

        private int NextOperatorMonteCarlo()
        {
            int iRet = WSThreadPoolNew<OperatorThread>.COMPLETE;
            int cSum = 0;

            lock (this)
            {
                for (int i = 0; i < pool.Length; i++)
                {
                    points[i] = 
                    cSum += points[i];
                }

                if (cSum != 0)
                {
                    int opPoints = rnd.Next(cSum);
                    for (iRet = 0; opPoints > points[iRet]; iRet++) ;
                }
            }

            return iRet;
        }

        /// <summary> Returns true if all data sources have been read to completion </summary>
        public override bool Complete
        { get { return complete; } }
    }
}
