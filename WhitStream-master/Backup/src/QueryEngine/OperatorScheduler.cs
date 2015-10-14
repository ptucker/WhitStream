using System;
using System.Collections.Generic;
using System.Threading;
using WhitStream.Data;
using WhitStream.QueryEngine;
using WhitStream.QueryEngine.QueryOperators;

namespace WhitStream.QueryEngine.Scheduler
{
	#region RoundRobin Scheduler
	/// <summary>
    /// A multi-threaded scheduler for executing a query using a round-robin algorithm
    /// </summary>
    public class RoundRobinScheduler : Scheduler
    {
        WSThreadPool pool = new WSThreadPool();
        bool complete = false;
        int iNextOp = 0;

        /// <summary> Initialize the scheduler to execute the given query </summary>
        /// <param name="breakOut">Should the query operators run in their own thread?</param>
        /// <param name="qs">The query plans to execute</param>
        public override IResults[] Init(bool breakOut, params Query[] qs)
        {
            IResults[] res = base.Init(breakOut, qs);
            pool.Init(ots, NextOperatorRoundRobin);

            return res;
        }
        
        /// <summary> Add new queries to the scheduler during execution </summary>
        /// <param name="breakOutOperators">Should the query operators run in their own thread?</param>
        /// <param name="qs">The query plans to execute</param>
        public override IResults[] AddQuery(bool breakOutOperators, params Query[] qs)
        {
        	IResults[] res = base.AddQuery(breakOutOperators, qs);
        	pool.AddQuery(ots);
        	
        	return res;
        }

        /// <summary> Execute the query </summary>
        public override void Execute()
        {
            pool.Start();
        }

        private int NextOperatorRoundRobin()
        {
            int iRet = WSThreadPool.COMPLETE;

            for (int i = 0; iRet == WSThreadPool.COMPLETE && i < pool.Length; i++)
            {
                iNextOp = (iNextOp + 1) % pool.Length;
                if (pool[iNextOp].Complete == false)
                    iRet = iNextOp;
            }

            complete = (iRet == WSThreadPool.COMPLETE);
            return iRet;
        }

        /// <summary> Returns true if all data sources have been read to completion </summary>
        public override bool Complete
        { get { return complete; } }

        /// <summary> String to represent this scheduler </summary>
        /// <returns>String to represent this scheduler</returns>
        public override string ToString()
        {
            return "Round Robin Scheduler";
        }
	}
	#endregion

    #region Genetic Algorithm Scheduler
    /// <summary>
    /// Scheduler algorithm using genetic programming techniques
    /// </summary>
    public class GAScheduler : Scheduler
    {
        private class ExecutionOrder
        {
            private List<int> order = new List<int>();
            private int next = 0;
            private int fitness = -1;

            public void Add(int i) { order.Add(i); }
            public int Count { get { return order.Count; } }
            public int Next 
            { 
                get 
                { 
                    int i = order[next];
                    next = (next + 1) % order.Count;
                    return i;
                } 
            }
            public int this[int i]
            {
                get { return order[i]; }
                set { order[i] = value; }
            }
            public int Fitness
            {
                get { return fitness; }
                set { fitness = value; }
            }
        }

        WSThreadPool pool = new WSThreadPool();
        bool complete = false;
        int iNextOp = 0;
        Random rnd;
        /// <summary>How many schedules to maintain at once</summary>
        public const int POPULATION = 10;
        int iOrder;
        ExecutionOrder[] orders = new ExecutionOrder[POPULATION];
        List<int> executionOrder = new List<int>();
        List<IStreamDataSource> leafNodes = new List<IStreamDataSource>();
        List<double> dataCounts = new List<double>();
        DateTime tmShuffle;
        TimeSpan tsShuffle = new TimeSpan(40000000);
        const int CMUTATIONS = 5;

        /// <summary>Constructor for Genetic Algorithm Scheduler</summary>
        /// <param name="i">how many items in the order</param>
        public GAScheduler(int i) { iOrder = i; }

        /// <summary> Initialize the scheduler to execute the given query </summary>
        /// <param name="breakOut">Should the query operators run in their own thread?</param>
        /// <param name="qs">The query plans to execute</param>
        public override IResults[] Init(bool breakOut, params Query[] qs)
        {
            rnd = new Random(9);
            IResults[] res = base.Init(breakOut, qs);
            pool.Init(ots, NextOperatorGenetic);

            //First, find the leaf nodes in the query
            for (int i = 0; i < pool.Length; i++)
            {
                if (pool[i].QueryOperator is IStreamDataSource)
                {
                    leafNodes.Add((IStreamDataSource)pool[i].QueryOperator);
                    dataCounts.Add(0);
                }
            }

            //Now, make up a population with the existing operators
            for (int p = 0; p < POPULATION; p++)
            {
                orders[p] = new ExecutionOrder();
                for (int i = 0; i < pool.Length; i++)
                    orders[p].Add(i);

                //Add p duplicates to the population
                for (int iDup = 0; iDup < p; iDup++)
                    orders[p].Add(rnd.Next(pool.Length-1));

                Mutate(ref orders[p]);
            }

            Console.Write("Order: ");
            for (int i = 0; i < orders[iOrder].Count; i++)
                Console.Write("{0}, ", orders[iOrder][i]);
            Console.WriteLine();

            return res;
        }

        private void Mutate(ref ExecutionOrder eo)
        {
            int tmp, r1, r2;
            for (int i = 0; i < CMUTATIONS; i++)
            {
                r1 = rnd.Next(eo.Count);
                r2 = rnd.Next(eo.Count);
                tmp = eo[r1];
                eo[r1] = eo[r2];
                eo[r2] = tmp;
            }
        }

        /// <summary> Add new queries to the scheduler during execution </summary>
        /// <param name="breakOutOperators">Should the query operators run in their own thread?</param>
        /// <param name="qs">The query plans to execute</param>
        public override IResults[] AddQuery(bool breakOutOperators, params Query[] qs)
        {
            IResults[] res = base.AddQuery(breakOutOperators, qs);
            pool.AddQuery(ots);

            return res;
        }

        /// <summary> Execute the query </summary>
        public override void Execute()
        {
            tmShuffle = DateTime.Now;
            pool.Start();
        }

        private int NextOperatorGenetic()
        {
            int iRet = WSThreadPool.COMPLETE;

            //if (DateTime.Now - tmShuffle > tsShuffle)
            //{
            //    double cnt = 0;
            //    for (int i=0; i<leafNodes.Count; i++)
            //    {
            //        cnt += (leafNodes[i].DataCount - dataCounts[i]);
            //        dataCounts[i] = leafNodes[i].DataCount;
            //    }
            //    Console.WriteLine("Schedule DataRate: {0}\n", (cnt / (DateTime.Now.Ticks - tmShuffle.Ticks)) * 10000000);
            //    Mutation();
            //    tmShuffle = DateTime.Now;
            //}

            for (int i = 0; iRet == WSThreadPool.COMPLETE && i < orders[iOrder].Count; i++)
            {
                iNextOp = orders[iOrder].Next;
                if (pool[iNextOp].Complete == false)
                    iRet = iNextOp;
            }

            complete = (iRet == WSThreadPool.COMPLETE);
            return iRet;
        }

        /// <summary> Returns true if all data sources have been read to completion </summary>
        public override bool Complete
        { get { return complete; } }

        /// <summary> String to represent this scheduler </summary>
        /// <returns>String to represent this scheduler</returns>
        public override string ToString()
        {
            return "Generic Algorithm Scheduler";
        }
    }
    #endregion

    #region MonteCarlo Scheduler
    /// <summary>
    /// A multi-threaded scheduler for executing a query using a round-robin algorithm
    /// </summary>
    public class MonteCarloScheduler : Scheduler
    {
        /// <summary> Points calculator for specific operator </summary>
        /// <param name="iOp">The operator to calculate</param>
        /// <returns>That operator's points</returns>
        public delegate int MCPoints(int iOp);

        private WSThreadPool pool = new WSThreadPool();
        private bool complete = false;
        private int[] points = null;
        private Random rnd = new Random(DateTime.Now.Millisecond);
        private MCPoints Mcp = null;
        const int mcpRecentPoints = 4;
        int mcpRecentMult = mcpRecentPoints * (WSThreadPool.THREADHISTORY + 2);

        /// <summary> Set the Monte Carlo Points function </summary>
        public MCPoints MCP { set { Mcp = value; } }

        /// <summary> Point calculator based on queue size </summary>
        /// <param name="iOp">The operator to calculate</param>
        /// <returns>The points for that operator</returns>
        public int MCPQueueSize(int iOp)
        {
            return ((pool[iOp].Complete || pool[iOp].Busy) ? 0 :
                (pool[iOp].Leaf ? 200 : GetTotalQueueSize(iOp) + (EOFExists(iOp) ? 500 : 0)));
        }

        private int GetTotalQueueSize(int iOp) 
        {
            int c = 0;
            for (int i = 0; i < pool[iOp].GetQueueCount(); i++)
                c += pool[iOp].QueueSize(i);

            return c;
        }

        private bool EOFExists(int iOp)
        {
            bool eof = false;

            for (int i = 0; i < pool[iOp].GetQueueCount() && eof == false; i++)
                eof |= pool[iOp].HasEOF(i);

            return eof;
        }

        /// <summary> Point calculator based on most recently run operator</summary>
        /// <param name="iOp">The operator to calculate</param>
        /// <returns>The points for that operator</returns>
        public int MCPRecent(int iOp)
        {
            int mult = mcpRecentMult;
            for (int i = 0; i < WSThreadPool.THREADHISTORY; i++)
            {
                if (iOp == pool.GetLastExecuted(i))
                    return MCPQueueSize(iOp) * mult;
                else
                    mult -= mcpRecentPoints;
            }

            return MCPQueueSize(iOp);
        }

        /// <summary> Initialize the scheduler to execute the given query </summary>
        /// <param name="breakOut">Should the query operators run in their own thread?</param>
        /// <param name="qs">The query plans to execute</param>
        public override IResults[] Init(bool breakOut, params Query[] qs)
        {
            if (Mcp == null)
                Mcp = MCPQueueSize;
            IResults[] res = base.Init(breakOut, qs);
            pool.Init(ots, NextOperatorMonteCarlo);
            points = new int[pool.Length];
            for (int i = 0; i < points.Length; i++)
                points[i] = 0;

            return res;
        }

        /// <summary> Add new queries to the scheduler during execution </summary>
        /// <param name="breakOutOperators">Should the query operators run in their own thread?</param>
        /// <param name="qs">The query plans to execute</param>
        public override IResults[] AddQuery(bool breakOutOperators, params Query[] qs)
        {
        	IResults[] res = base.AddQuery(breakOutOperators, qs);
        	pool.AddQuery(ots);
        	lock (this)
        	{
	            points = new int[pool.Length];
	            for (int i = 0; i < points.Length; i++)
	                points[i] = 0;
        	}
        	
        	return res;
        }
        
        /// <summary> Execute the query </summary>
        public override void Execute()
        {
            pool.Start();
        }

        private int NextOperatorMonteCarlo()
        {
            int iRet = WSThreadPool.COMPLETE;
            int cSum = 0;

            for (int i = 0; i < pool.Length; i++)
            {
                points[i] = Mcp(i);
                cSum += points[i];
            }

            if (cSum != 0)
            {
                int opPoints = rnd.Next(cSum);
                for (iRet = 0; opPoints > points[iRet]; iRet++)
                    opPoints -= points[iRet];
            }

            complete = (iRet == WSThreadPool.COMPLETE);
            return iRet;
        }

        /// <summary> Returns true if all data sources have been read to completion </summary>
        public override bool Complete
        { get { return complete; } }

        /// <summary> String to represent this scheduler </summary>
        /// <returns>String to represent this scheduler</returns>
        public override string ToString()
        {
            return "Monte Carlo Scheduler";
        }
	}
	#endregion
}
