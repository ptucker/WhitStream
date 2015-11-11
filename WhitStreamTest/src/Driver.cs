/*
 * Created by SharpDevelop.
 * User: Pete
 * Date: 12/29/2005
 * Time: 12:17 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WhitStream.Data;
using WhitStream.Expression;
using WhitStream.QueryEngine;
using WhitStream.QueryEngine.Scheduler;
using WhitStream.QueryEngine.QueryOperators;
using WhitStream.Utility;
using WhitStream.Admin;
using WhitStreamTest;

namespace WhitStreamTest
{
	/// <summary>
	/// Driver class for WhitStream
	/// </summary>
	public class Driver
	{
		private static List<int> AvaPorts = new List<int>(10);
		private static int current_port = 0;
        private static DataItemPool dipGlobal = new DataItemPool();

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

		#region Main
		/// <summary>
		/// Simple testing harness driver
		/// </summary>
		public static void Main()
		{
            

			Log.Init("output", true);
			int exec = 1;
			for (int i = 4001; i <= 4010; i++)
			{
				AvaPorts.Add(i);
			}

            dipGlobal.Init(10, true);

            //double sum = 0;
            //for (int i = 0; i < exec; i++)
            //{
            //    DateTime dt = DateTime.Now;
            //    ShowResults(TestQuery());
            //    TimeSpan tm = DateTime.Now - dt;
            //    sum += tm.TotalSeconds;
            //}
            //Log.WriteMessage(string.Format("Pull-based time: {0}", sum), Log.eMessageType.Debug);
            //Log.WriteMessage(string.Format("Data Item Count: {0}", dipGlobal.DICount), Log.eMessageType.Debug);

            IScheduler sch;
            sch = new RoundRobinScheduler();
            //Log.WriteMessage(string.Format("Global Data Item Count: {0}", dipGlobal.DICount), Log.eMessageType.Debug);
            ExecuteScheduler("Round Robin (op/thread)", sch, true, exec);
            //Log.WriteMessage(string.Format("Global Data Item Count: {0}", dipGlobal.DICount), Log.eMessageType.Debug);
            
            //for (int i = 0; i < 10; i++)
            //{
            //    sch = new GAScheduler(i);
            //    ExecuteScheduler("Genetic Algorithm (op/thread)", sch, true, exec);
            //}

            //sch = new MonteCarloScheduler();
            //((MonteCarloScheduler)sch).MCP = ((MonteCarloScheduler)sch).MCPQueueSize;
            //ExecuteScheduler("Monte Carlo (QS) (op/thread)", sch, true, exec);

            //sch = new MonteCarloScheduler();
            //((MonteCarloScheduler)sch).MCP = ((MonteCarloScheduler)sch).MCPRecent;
            //ExecuteScheduler("Monte Carlo (MR) (op/thread)", sch, true, exec);

            //sch = new GAScheduler(); 
            //ExecuteScheduler("Genetic Algorithm (query/thread)", sch, false, exec);

            //sch = new MonteCarloScheduler();
            //ExecuteScheduler("Monte Carlo (QS) (query/thread)", sch, false, exec);

            //sch = new MonteCarloScheduler();
            //((MonteCarloScheduler)sch).MCP = ((MonteCarloScheduler)sch).MCPRecent;
            //ExecuteScheduler("Monte Carlo (MR) (query/thread)", sch, false, exec);

			Log.Close();
		}
		#endregion

		#region Result Handler
		private class ResultsHandler
		{
			IResults res;
			int c;
			DateTime dtStart = DateTime.Now;
			TimeSpan tsComplete = TimeSpan.Zero;
			string name;
			List<DataItem> dataCache = new List<DataItem>();

			public ResultsHandler(string n, IResults r)
			{
				res = r;
				c = 0;
				res.DataArrived += new DataEvent(ProcessResults);
				name = n;
			}

			public bool EOF { get { return res.EndQuery; } }

			public int DataCount { get { return c; } }

			public TimeSpan CompletionTime { get { return tsComplete; } }

			public string Name { get { return name; } }

			public List<DataItem> ResultData { get { return dataCache; } }

			public void ProcessResults(IResults results)
			{
				List<DataItem> ldi = results.Results;
                foreach (DataItem di in ldi)
                {
                    di.Dispose();
                    //Log.WriteMessage(string.Format("\t{0}\n", di.ToString()), Log.eMessageType.Debug);
                }
				c += ldi.Count;
				if (res.EndQuery)
					tsComplete = (DateTime.Now - dtStart);
			}
		}
		#endregion

		#region Execute Scheduler
		private static void ExecuteScheduler(string title, IScheduler sch, bool breakOut, int exec)
		{
			int q;
			bool complete = false;
            AdminForm frmAdmin = new AdminForm();

			Query[] qs = new Query[exec];
			for (q = 0; q < exec; q++)
			{
				qs[q] = TestQuery();
			}
            
			IResults[] res = sch.Init(breakOut, qs);
			ResultsHandler[] rh = new ResultsHandler[res.Length];
			for (q = 0; q < res.Length; q++)
				rh[q] = new ResultsHandler(string.Format("{0}{1}", title, q), res[q]);
			sch.Execute();
            frmAdmin.Scheduler = sch;
            frmAdmin.Show();

            while (!complete && !frmAdmin.ShutdownServer)
            {
                frmAdmin.UpdateStats();
                //Thread.Sleep(5000);
				complete = true;
				for (int i = 0; i < rh.Length && complete; i++)
					complete &= rh[i].EOF;
				//Log.WriteMessage(DataItemPool.ToString(), Log.eMessageType.Debug);
			}

			double totalSec = 0;
			for (int i = 0; i < rh.Length; i++)
			{
				Log.WriteMessage(string.Format("{0} Total Rows: {1}, Time: {2}", rh[i].Name, rh[i].DataCount, rh[i].CompletionTime), Log.eMessageType.Debug);
				totalSec += (((double)rh[i].CompletionTime.Minutes) * 60) +
				    ((double)rh[i].CompletionTime.Seconds) +
				    (((double)rh[i].CompletionTime.Milliseconds) / 1000.0);
			}

			Log.WriteMessage(string.Format("Average Time: {0}", totalSec / rh.Length), Log.eMessageType.Debug);
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
            //return new OpGenerate(iRow2, iPunct);

            //return new OpServer(); 

            //return new OpGroupByCount(new int[] {0}, new OpServer());

            //return new OpDifference(new OpServer(), new OpServer());

            return new OpJoin("$1.1 = $2.2", new OpProject(new UnaryOp.Map(Snd), new int[] { 1 },
                            new OpNUnion(new OpSelect("$1.2 < 50",
                                            new OpSelect("$1.1 > 4", new OpServer())),
                                        new OpServer())),
                            new OpNUnion(new OpServer(), new OpServer(), new OpServer(), new OpServer()));
            //return new OpSelect("$1.1 = 0", new OpServer());
            //return new OpNUnion(new OpServer(), new OpServer(), new OpServer(), new OpServer());

            //string que = "SELECT name FROM users WHERE age = '$0$' OR age = '$1$';";
            //return new OpDBStatement("test", que, new OpServer());

			//return new OpSelect("$1.1 = 0", new OpServer());

            //return new OpJoinSymmRangePunct("$1.1 = $2.2", 0, 1, new OpProject(new UnaryOp.Map(Snd), new int[] { 1 },
            //                new OpNUnion(new OpSelect("$1.2 < 50",
            //                                new OpSelect("$1.1 > 4", new OpGenerate(150000, 2))),
            //                            new OpServer())),
            //                new OpUnion(new OpUnion(new OpServer(), new OpServer()), new OpUnion(new OpServer(), new OpServer())));
            
            
            ///////////////////Linear Road\\\\\\\\\\\\\\\\\\\\\\\

            /*int[] attrs = {7, 9};
            int[] mattrs = {9};
            return new OpNUnion(new OpGroupByAvg(attrs, mattrs, 3, new OpBucket(60, 30, 1, -1, new OpServer(9448))), new OpServer(9449), new OpServer(9450), new OpServer(9451));*/

            /*int[] attrs = { 1, 2, 3 }, //Dir, Seg, WID
                mattrs = { 3 }, //WID
                forattrs = { 2, 6, 7, 9 }, //VID, Dir, Seg, WID
                pos = { 0, 1, 2, 3 };
            return new OpNUnion(new OpGroupByCount(attrs, mattrs, new OpDupElim(new OpFormat(forattrs, pos, new OpBucket(60, 60, 1, -1, new OpServer(9448))))), new OpServer(9449), new OpServer(9450), new OpServer(9451));*/

            /*int[] attrs = { 2, 8, 9 }; //VID, Pos, WID
            int[] mattrs = { 2, 9 }; //VID, WID
            //return new OpNUnion(new OpSelect("$1.3 >= 3", new OpGroupByCount(attrs, mattrs, new OpBucket(4, 1, -1, 2, new OpServer(9448)))), new OpServer(9449), new OpServer(9450), new OpServer(9451));
            return new OpNUnion(new OpGroupByCount(attrs, mattrs, new OpBucket(4, 1, -1, 2, new OpServer(9448))), new OpServer(9449), new OpServer(9450), new OpServer(9451));*/
        }

		private static Query TestQueryCons()
		{
			//private static ConnectionManager cm = new ConnectionManager( "10.28.29.2", "4000" );
			ConnectionManager cm = new ConnectionManager("4000");
			Connection c = null;
			while (c == null)
			{
				c = cm.LocateCon(GetPort(), "WhitDevice");
				Thread.Sleep(1);
			}
			c.StartThreads();
			current_port++;

			//return c;

			//return new OpSelect("$1.16 = s139", c);

			return new OpGroupByCount(new int[] { 0 }, new OpProject(new UnaryOp.Map(Sec_to_Min), new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, c));
		}
		#endregion

		#region Show Results
		private static void ShowResults(Query q)
		{
			List<DataItem> rgdi;
			bool eof = false;
			int cRow = 0;

			while (eof == false)
			{
                rgdi = q.Iterate(dipGlobal.GetItem, dipGlobal.ReleaseItem);
				foreach (DataItem di in rgdi)
				{
					if (di.EOF == false)
					{
						//Log.WriteMessage(di.ToString(), Log.eMessageType.Debug);
                        //Console.WriteLine(di.ToString()); 
						cRow++;
					}
					eof |= di.EOF;
                    di.Dispose();
				}
			}
			Log.WriteMessage(string.Format("Total Rows: {0}", cRow), Log.eMessageType.Debug);
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
	}


}
