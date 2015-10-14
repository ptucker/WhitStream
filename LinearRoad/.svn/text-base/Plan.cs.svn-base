using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Data;
using WhitStream.Data;
using WhitStream.Database;
using WhitStream.Expression;
using WhitStream.QueryEngine;
using WhitStream.QueryEngine.Scheduler;
using WhitStream.QueryEngine.QueryOperators;
using WhitStream.Utility;
using WhitStream.Admin;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace LinearRoad
{
    class Plan
    {
        const string LINEARDATABASE = "linearroad";
        const uint EXPRESSWAYS = 1, //round up if partial
            SEGMENTS = 100,
            DIRECTIONS = 2,
            MAXVID = 137337;
        private static DataItemPool dipGlobal = new DataItemPool();

        public static void ExecuteThread(Query q, DataItemPool dip)
        {
            while (true)
            {
                q.Iterate(dip.GetItem, dip.ReleaseItem);
                Thread.Sleep(10);
            }
        }
        delegate void DoQuery(Query q, DataItemPool dip);

        static void Main(string[] args)
        {
            IScheduler sch;
            sch = new PriorityScheduler(); //new RoundRobinScheduler();
            QueryManager qm = new QueryManager();
            clearTables();
            ExecuteScheduler("Priority", sch, true, qm.GetQueries());
            /*Query[] queries = qm.GetQueries();
            dipGlobal.Init(10000);
            DoQuery[] dqs = new DoQuery[queries.Length];
            for (int i = 0; i < dqs.Length; i++)
            {
                dqs[i] = ExecuteThread;
                dqs[i].BeginInvoke(queries[i], dipGlobal, null, null);
            }
            Thread.Sleep(Timeout.Infinite);*/
            /*while (true)
            {
                foreach (Query q in queries)
                {
                    q.Iterate(dipGlobal.GetItem, dipGlobal.ReleaseItem);
                }
            }*/
        }
        /// <summary>
        /// Truncate all the tables in the MySql database
        /// </summary>
        public static void clearTables()
        {
            MySqlConnection mycon = new MySqlConnection("datasource=localhost;username=whitstream;password=whitstream;database=" + LINEARDATABASE);
            mycon.Open();

            MySqlCommand clear0 = new MySqlCommand("TRUNCATE TABLE output0", mycon);
            clear0.Prepare();
            MySqlCommand clear1 = new MySqlCommand("TRUNCATE TABLE output1", mycon);
            clear1.Prepare();
            MySqlCommand clear2 = new MySqlCommand("TRUNCATE TABLE output2", mycon);
            clear2.Prepare();
            MySqlCommand clear3 = new MySqlCommand("TRUNCATE TABLE output3", mycon);
            clear3.Prepare();

            clear0.ExecuteNonQuery();
            clear1.ExecuteNonQuery();
            clear2.ExecuteNonQuery();
            clear3.ExecuteNonQuery();
        }

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
        private static void ExecuteScheduler(string title, IScheduler sch, bool breakOut, Query[] qs)
        {
            int q;
            bool complete = false;

            AdminForm frmAdmin = new AdminForm();

            IResults[] res = sch.Init(breakOut, qs);
            ResultsHandler[] rh = new ResultsHandler[res.Length];
            for (q = 0; q < res.Length; q++)
                rh[q] = new ResultsHandler(string.Format("{0}{1}", title, q), res[q]);
            sch.Execute();
            //frmAdmin.Scheduler = sch;
            //frmAdmin.Show();

            while (!complete && !frmAdmin.ShutdownServer)
            {
                //frmAdmin.UpdateStats();
                Thread.Sleep(10000);
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
        class QueryManager
        {
            RelationTable accountBalances;
            RelationTable accidents;
            RelationTable tollQuotes;
            RelationTable segStats;
            List<Query> lrQueries;
            Dictionary<uint, uint> NSSTable;
            List<int> IsDupTable;

            public QueryManager()
            {
                initTables();
                lrQueries = new List<Query>();

                lrQueries.AddRange(GetType0());
                lrQueries.Add(GetType2());
                lrQueries.AddRange(GetType3());
                //lrQueries.Add(new OpServer(9451)); //we don't need to respond to type 4 queries
            }

            #region Type 0

            public DataItem PreProcess(DataItem di)
            {
                //<Time, VID, Spd, Xway, Lane, Dir, Seg, Pos>
                if ((int)di[1] == -1) //we need to turn it into a proper punctuation
                {
                    const int WildCards = 7;
                    Punctuation p = new Punctuation(WildCards + 1);
                    p.AddValue(new Punctuation.LiteralPattern(di[0]));
                    for (int i = 0; i < WildCards; i++)
                        p.AddValue(new Punctuation.WildcardPattern());
                    return p;
                }
                else
                    return di;
                //<Time, VID, Spd, Xway, Lane, Dir, Seg, Pos>
            }

            public DataItem NSSCheckSeg(DataItem di)
            {
                //<Time, VID, Spd, Xway, Lane, Dir, Seg, Pos>
                int[] attrs = { 0, 1, 3, 4, 5, 6 };
                if (di is Punctuation)
                {
                    Punctuation pRet = new Punctuation(attrs.Length + 1);
                    foreach (int i in attrs)
                        pRet.AddValue(di[i]);

                    if (pRet[1] is Punctuation.LiteralPattern) //an end of vehicle punctuation
                    {
                        NSSTable.Remove(Convert.ToUInt32(((Punctuation.LiteralPattern)pRet[1]).Value));
                    }
                    pRet.AddValue(new Punctuation.WildcardPattern());
                    return pRet;
                }
                else
                {
                    DataItem diRet = new DataItem(attrs.Length + 1, null);
                    foreach (int i in attrs)
                        diRet.AddValue(di[i]);
                    diRet.EOF = di.EOF;
                    diRet.TimeStamp = di.TimeStamp;

                    uint VID = Convert.ToUInt32(diRet[1]);
                    uint Seg = Convert.ToUInt32(diRet[5]);
                    if (NSSTable.ContainsKey(VID))
                    {
                        uint OldSeg = NSSTable[VID];
                        if (OldSeg == Seg)
                            diRet.AddValue((int)0);
                        else
                        {
                            diRet.AddValue((int)1);
                            NSSTable[VID] = Seg;
                        }
                    }
                    else
                    {
                        NSSTable.Add(VID, Seg);
                        diRet.AddValue((uint)1);
                    }
                    return diRet;
                }
                //<Time, VID, Xway, Lane, Dir, Seg, ?isNSS>
            }

            public DataItem IsDupCheckVID(DataItem di)
            {
                //<Time, VID, Spd, Xway, Lane, Dir, Seg, Pos, WID(List)>
                di.AddCapacity(1);
                if (di is Punctuation)
                {
                    Punctuation p = di as Punctuation;
                    p.AddValue(new Punctuation.WildcardPattern());
                    IsDupTable.Clear();
                    return p;
                }
                else
                {
                    int VID = (int)di[1];
                    if (IsDupTable.Contains(VID))
                    {
                        di.AddValue((int)1);
                    }
                    else
                    {
                        di.AddValue((int)0);
                        IsDupTable.Add(VID);
                    }
                    return di;
                }
                //<Time, VID, Spd, Xway, Lane, Dir, Seg, Pos, WID(List), ?isDup>
            }

            public DataItem SStatsLAVInsertion(DataItem di, RelationTable rt)
            {
                if (!(di is Punctuation))
                {
                    object[] keys = { di[2], di[0], di[1] }; //Seg, Xway, Dir
                    lock (rt)
                    {
                        DataRow drow = rt.Find(keys);
                        if (drow != null)
                        {
                            drow["LAV"] = di[4];
                            drow["LAVWID"] = di[3];
                        }
                        else
                        {
                            drow = rt.GetRow();
                            drow["Seg"] = di[2];
                            drow["Xway"] = di[0];
                            drow["Dir"] = di[1];
                            drow["LAV"] = di[4];
                            drow["LAVWID"] = di[3];
                            drow["Count"] = -1; //we have no idea what it might be
                            drow["CountWID"] = di[3];
                            rt.Add(drow);
                        }
                    }
                }
                else //we need to check every segment
                {
                    Punctuation p = di as Punctuation;
                    ulong WID = (ulong)((Punctuation.LiteralPattern)p[3]).Value;
                    for (int s = 0; s < SEGMENTS; s++)
                    {
                        for (int x = 0; x < EXPRESSWAYS; x++)
                        {
                            for (int d = 0; d < DIRECTIONS; d++)
                            {
                                object[] keys = { s, x, d };
                                lock (rt)
                                {
                                    DataRow drow = rt.Find(keys);
                                    if (drow != null)
                                    {
                                        if ((ulong)drow["LAVWID"] < WID)
                                        {
                                            drow["LAV"] = -1;
                                            drow["LAVWID"] = WID;
                                        }
                                    }
                                    else
                                    {
                                        drow = rt.GetRow();
                                        drow["Seg"] = s;
                                        drow["Xway"] = x;
                                        drow["Dir"] = d;
                                        drow["LAV"] = -1;
                                        drow["LAVWID"] = 0;
                                        drow["Count"] = -1;
                                        drow["CountWID"] = 0;
                                        rt.Add(drow);
                                    }
                                }
                            }
                        }
                    }
                }
                return di;
            }

            public DataItem SStatsCountInsertion(DataItem di, RelationTable rt)
            {
                if (!(di is Punctuation))
                {
                    object[] keys = { di[2], di[0], di[1] }; //Seg, Xway, Dir
                    lock (rt)
                    {
                        DataRow drow = rt.Find(keys);
                        if (drow != null)
                        {
                            drow["Count"] = di[4];
                            drow["CountWID"] = di[3];
                        }
                        else
                        {
                            drow = rt.GetRow();
                            drow["Seg"] = di[2];
                            drow["Xway"] = di[0];
                            drow["Dir"] = di[1];
                            drow["LAV"] = -1; //we have no idea what it might be
                            drow["LAVWID"] = di[3];
                            drow["Count"] = di[4];
                            drow["CountWID"] = di[3];
                            rt.Add(drow);
                        }
                    }
                }
                else //we need to check every segment
                {
                    Punctuation p = di as Punctuation;
                    ulong WID = (ulong)((Punctuation.LiteralPattern)p[3]).Value;
                    for (int s = 0; s < SEGMENTS; s++)
                    {
                        for (int x = 0; x < EXPRESSWAYS; x++)
                        {
                            for (int d = 0; d < DIRECTIONS; d++) //only 200 iterations per Xway, fairly inexpensive
                            {
                                object[] keys = { s, x, d };
                                lock (rt)
                                {
                                    DataRow drow = rt.Find(keys);
                                    if (drow != null)
                                    {
                                        if ((ulong)drow["CountWID"] < WID)
                                        {
                                            drow["Count"] = -1;
                                            drow["CountWID"] = WID;
                                        }
                                    }
                                    else
                                    {
                                        drow = rt.GetRow();
                                        drow["Seg"] = s;
                                        drow["Xway"] = x;
                                        drow["Dir"] = d;
                                        drow["LAV"] = -1;
                                        drow["LAVWID"] = 0;
                                        drow["Count"] = -1;
                                        drow["CountWID"] = 0;
                                        rt.Add(drow);
                                    }
                                }
                            }
                        }
                    }
                }
                return di;
            }

            public DataItem AccidentDetection(DataItem di, RelationTable rt)
            {
                if (!(di is Punctuation))
                {
                    uint count = Convert.ToUInt32(di[6]);

                    if (count == 4)
                    {
                        object[] keys = { di[1], di[2], di[3] }; //Xway, Dir, Seg
                        lock (rt)
                        {
                            DataRow drow = rt.Find(keys);
                            if (drow == null)
                            {
                                drow = rt.GetRow();
                                drow["Xway"] = di[1];
                                drow["Dir"] = di[2];
                                drow["Seg"] = di[3];
                                rt.Add(drow);
                            }
                            //Console.WriteLine("Accidents: {0} - From Insertion", rt.Rows);
                        }
                    }
                    else //4 > count > 1
                    {
                        object[] keys = { di[1], di[2], di[3] }; //Xway, Dir, Seg
                        lock (rt)
                        {
                            DataRow drow = rt.Find(keys); //assumes that there will only be one accident per Xway/dir
                            if (drow != null)
                                rt.Drop(drow);
                            //Console.WriteLine("Accidents: {0} - From Deletion", rt.Rows);
                        }
                    }
                }
                return di;
            }

            public DataItem AccidentInRange(DataItem di, RelationTable rt)
            {
                //<Time, VID, Xway, Lane, Dir, Seg, ?isNSS>
                di.AddCapacity(1);
                if (di is Punctuation)
                {
                    Punctuation p = di as Punctuation;
                    p.AddValue(new Punctuation.WildcardPattern());
                    return p;
                }
                else
                {
                    int Xway = (int)di[2],
                        dir = (int)di[4],
                        seg = (int)di[5];
                    lock (rt)
                    {
                        DataRow[] drows = rt.Find("[Xway] = " + Xway + " AND [Dir] = " + dir);
                        if (drows.Length > 0)
                        {
                            int accseg = (int)drows[0]["Seg"],
                                lbound, hbound;
                            if (dir == 0)//segs are increasing
                            {
                                lbound = accseg - 4;
                                hbound = accseg;
                            }
                            else
                            {
                                lbound = accseg;
                                hbound = accseg + 4;
                            }

                            if (seg <= hbound && seg >= lbound) //between the bounds
                                di.AddValue(accseg);
                            else
                                di.AddValue((int)-1);
                        }
                        else
                            di.AddValue((int)-1);
                    }
                    return di;
                }
                //<Time, VID, Xway, Lane, Dir, Seg, ?isNSS, isAIR>
            }

            public DataItem FindToll(DataItem di, RelationTable rt)
            {
                //<Time, VID, Xway, Lane, Dir, Seg, ?isNSS>
                di.AddCapacity(2);
                if (di is Punctuation)
                {
                    Punctuation p = di as Punctuation;
                    p.AddValue(new Punctuation.WildcardPattern());
                    p.AddValue(new Punctuation.WildcardPattern());
                    return p;
                }
                else
                {
                    uint seg = Convert.ToUInt32(di[5]);
                    if (Convert.ToUInt32(di[4]) == 0) //find the seg before this one
                        seg--;
                    else
                        seg++;
                    object[] keys = { di[1], seg }; //VID
                    lock (rt)
                    {
                        DataRow drow = rt.Find(keys);
                        if (drow == null) //there's no toll, we can drop it
                        {
                            di.AddValue((int)0);
                            di.AddValue((int)0);
                        }
                        else
                        {
                            di.AddValue(drow["Toll"]);
                            di.AddValue(drow["LAV"]);
                            rt.Drop(drow);
                            //Console.WriteLine("We found a toll");
                        }
                    }
                    return di;
                }
                //<Time, VID, Xway, Lane, Dir, Seg, ?isNSS, Toll, LAV>
            }

            public DataItem ChargeToll(DataItem di, RelationTable rt)
            {
                //<Time, VID, Xway, Lane, Dir, Seg, ?isNSS, Toll, LAV>
                if (!(di is Punctuation))
                {
                    object[] keys = { di[1] }; //VID
                    lock (rt)
                    {
                        DataRow drow = rt.Find(keys);
                        if (drow == null)
                        {
                            drow = rt.GetRow();
                            drow["Vid"] = di[1];
                            drow["Bal"] = di[7];
                            drow["Time"] = di[0];
                            rt.Add(drow);
                        }
                        else 
                        {
                            drow["Bal"] = Convert.ToUInt32(di[7]) + (uint)drow["Bal"];
                            drow["Time"] = di[0];
                        }
                    }
                }
                return di;
                //<Time, VID, Xway, Lane, Dir, Seg, ?isNSS, Toll, LAV>
            }

            public DataItem ConvertTimeStamp(DataItem di)
            {
                //<Time, VID, Xway, Lane, Dir, Seg, ?isNSS, ...>
                if (!(di is Punctuation))
                {
                    di.AddCapacity(1);
                    di.AddValue(Convert.ToUInt32(di[0]) + ((DateTime.Now.Subtract(di.TimeStamp)).TotalSeconds));
                    return di;
                }
                return di;
                //<Time, VID, Xway, Lane, Dir, Seg, ?isNSS, ...>
            }

            public DataItem AttachStats(DataItem di, RelationTable rt)
            {
                //<Time, VID, Xway, Lane, Dir, Seg, InTime(DateTime), ?isNSS, isAIR>
                di.AddCapacity(2);
                if (!(di is Punctuation))
                {
                    try
                    {
                        object[] keys = { di[5], di[2], di[4] }; //Seg, Xway, Dir
                        lock (rt)
                        {
                            DataRow drow = rt.Find(keys);
                            if (drow == null)
                            {
                                di.AddValue((uint)0);
                                di.AddValue((uint)0);
                            }
                            else
                            {
                                di.AddValue(drow["LAV"]);
                                di.AddValue(drow["Count"]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                return di;
                //<Time, VID, Xway, Lane, Dir, Seg, InTime(DateTime), ?isNSS, ?isAIR, LAV, Count>
            }

            public DataItem QuoteTolls(DataItem di, RelationTable rt)
            {
                //<Time, VID, Xway, Lane, Dir, Seg, ?isNSS, ?isAIR, LAV, Count>
                if (!(di is Punctuation))
                {
                    uint toll,
                        LAV = Convert.ToUInt32(di[8]),
                        Count = Convert.ToUInt32(di[9]);
                    int isAccident = Convert.ToInt32(di[7]);

                    if (isAccident != -1 || Count <= 50 || LAV >= 40)
                        toll = 0;
                    else
                    {
                        toll = 2 * (uint)Math.Pow((Count - 50), 2.0);
                        //Console.WriteLine("Toll Quote: {0}, Count: {1}, LAV: {2}", toll, Count, LAV);
                        lock (rt)
                        {
                            DataRow drow = rt.GetRow();
                            drow["VID"] = di[1];
                            drow["Seg"] = di[5];
                            drow["LAV"] = LAV;
                            drow["Toll"] = toll;
                            rt.Add(drow);
                        }
                    }
                }
                //<Time, VID, Xway, Lane, Dir, Seg, ?isNSS, ?isAIR, LAV, Count>
                return di;                
            }

            private List<Query> GetType0()
            {
                List<Query> qOut = new List<Query>();
                int[] npAttrs = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }; //null project attributes

                UnaryOp.Map FeedPreProcess = new UnaryOp.Map(PreProcess);
                OpMultiplexer Feeder = new OpMultiplexer(
                    new OpProject(FeedPreProcess, npAttrs,
                        new OpServerRaw(9448)));
                qOut.Add(Feeder);

                //////regions are of the form: name (# of Queries returned) - Source [-> new Source]\\\\\\

                #region SegStats (2) - Feeder
                UnaryOp.Relation ssLAVRelation = new UnaryOp.Relation(SStatsLAVInsertion);
                int[] ssAvg = { 3, 5, 6, 8 };
                int[] ssAvgMain = { 8 };
                OpRelation sStatsLAV = new OpRelation(ssLAVRelation, segStats,
                    new OpGroupByAvg(ssAvg, ssAvgMain, 2, 8,
                        new OpBucket(5 * 60, 60, 0, -1,
                            Feeder.GetQuery())));//<Time, VID, Spd, Xway, Lane, Dir, Seg, Pos>
                qOut.Add(sStatsLAV);

                UnaryOp.Map isDuplicate = new UnaryOp.Map(IsDupCheckVID);
                UnaryOp.Relation ssCountRelation = new UnaryOp.Relation(SStatsCountInsertion);
                int[] ssCount = { 3, 5, 6, 8 };
                int[] ssCountMain = { 8 };
                //int[] ssCountProject = { 1, 3, 5, 6, 8 }; //<VID, Xway, Dir, Seg, WID(List)>
                OpRelation sStatsCount = new OpRelation(ssCountRelation, segStats,
                    new OpGroupByCount(ssCount, ssCountMain, 8,
                        new OpSelect("$1.10 = I0", //<Time, VID, Spd, Xway, Lane, Dir, Seg, Pos, WID(List), ?isDup>
                            new OpProject(isDuplicate, npAttrs,
                                new OpBucket(60, 60, 0, -1,
                                    Feeder.GetQuery())))));//<Time, VID, Spd, Xway, Lane, Dir, Seg, Pos>
                qOut.Add(sStatsCount);
                #endregion

                #region Accident Detection (1) - Feeder
                int[] adGroupBy = { 1, 3, 5, 6, 7, 8}; //VID, Xway, Dir, Seg, Pos, WID
                int[] adGroupByMain = { 8 };
                
                UnaryOp.Relation adRelation = new UnaryOp.Relation(AccidentDetection);
                OpRelation aDetection = new OpRelation(adRelation, accidents,
                    new OpSelect("$1.7 > U1",
                        new OpGroupByCount(adGroupBy, adGroupByMain, 8,
                            new OpBucket(4 * 30, 30, 0, -1,
                                Feeder.GetQuery())))); //<Time, VID, Spd, Xway, Lane, Dir, Seg, Pos>
                qOut.Add(aDetection);
                #endregion

                #region New Segment Stream (1) - Feeder -> NSS
                UnaryOp.Map nssMap = new UnaryOp.Map(NSSCheckSeg);
                OpMultiplexer NSSFeeder = new OpMultiplexer(
                    new OpSelect("$1.7 = I1",
                            new OpProject(nssMap, npAttrs,
                                Feeder.GetQuery())));
                qOut.Add(NSSFeeder);
                #endregion

                #region Toll Charging (1) - NSS
                string Tollstatement = "INSERT INTO output0 (Type, VID, Time, Emit, Speed, Toll) VALUES('0', $1$, $0$, $9$, $8$, $7$)";
                UnaryOp.Relation chargeTollRelation = new UnaryOp.Relation(ChargeToll);
                UnaryOp.Relation findTollRelation = new UnaryOp.Relation(FindToll);
                UnaryOp.Map convertTollTimeStamp = new UnaryOp.Map(ConvertTimeStamp);
                OpDBAccess chargeTolls = new OpDBAccess(LINEARDATABASE, null, Tollstatement, //<Time, VID, Xway, Lane, Dir, Seg, Emit, ?isNSS, Toll, LAV>
                    new OpProject(convertTollTimeStamp, npAttrs,
                        new OpRelation(chargeTollRelation, accountBalances,
                            new OpSelect("$1.8 != I0", //it has a toll
                                new OpRelation(findTollRelation, tollQuotes,
                                    new OpSelect("$1.4 > I0", //not entering the expressway
                                        NSSFeeder.GetQuery()))))));
                qOut.Add(chargeTolls);
                #endregion

                #region Accident in Range Section (1) - NSS
                UnaryOp.Relation arsRelation = new UnaryOp.Relation(AccidentInRange);

                UnaryOp.Relation tqAttachStatsRelation = new UnaryOp.Relation(AttachStats);
                UnaryOp.Relation tqQuoteTollsRelation = new UnaryOp.Relation(QuoteTolls);
                OpRelation quoteTolls = new OpRelation(tqQuoteTollsRelation, tollQuotes,
                    new OpRelation(tqAttachStatsRelation, segStats,
                        new OpRelation(arsRelation, accidents, //<Time, VID, Xway, Lane, Dir, Seg, ?isNSS, ?isAIR>
                            NSSFeeder.GetQuery())));

                UnaryOp.Map accConvertTimeStamp = new UnaryOp.Map(ConvertTimeStamp);
                //<Time, VID, Xway, Lane, Dir, Seg, ?isNSS, ?isAIR, LAV, Count, Emit>
                string Accidentstatement = "INSERT INTO output1 (Type, Time, Emit, Seg) VALUES('1', $0$, $10$, $7$)";
                OpDBAccess accidentNotification = new OpDBAccess(LINEARDATABASE, null, Accidentstatement,
                    new OpProject(accConvertTimeStamp, npAttrs,
                        new OpSelect("$1.8 >= I0", //there is an accident in range
                            quoteTolls)));
                qOut.Add(accidentNotification);
                #endregion

                return qOut;
            }

            #endregion

            #region Type 2
            public DataItem Type2Relation(DataItem di, RelationTable rt)
            {
                //<Time, VID, QID>
                if (!(di is Punctuation))
                {
                    object[] keys = { di[1] }; //VID
                    int bal, time;
                    di.AddCapacity(2);
                    lock (rt)
                    {
                        DataRow drow = rt.Find(keys);
                        if (drow == null)
                        {
                            drow = rt.GetRow();
                            drow["VID"] = Convert.ToInt32(di[1]);
                            drow["Bal"] = 0;
                            drow["Time"] = DateTime.Now.Subtract(di.TimeStamp).TotalSeconds;
                            rt.Add(drow);
                            //Console.WriteLine("We had to make a new balance for a type 2 query");
                        }
                        bal = drow["Bal"];
                        time = drow["Time"];
                    }
                    di.AddValue(bal);
                    di.AddValue(time);
                    return di;
                }
                else
                    return di;
                //<Time, VID, QID, Bal, UTime>
            }

            public DataItem Type2MapConvert(DataItem di)
            {
                //<Time, VID, QID, Bal, UTime>
                di.AddCapacity(1);
                di.AddValue(Convert.ToInt32(di[0]) + (int)(DateTime.Now.Subtract(di.TimeStamp).TotalSeconds));
                return di;
                //<Time, VID, QID, Bal, UTime, Emit>
            }


            private Query GetType2()
            {
                string statementType2 = "INSERT INTO output2 (Type, Time, Emit, QID, Bal, RTime) VALUES('2', $0$, $5$, $2$, $3$, $4$)";
                UnaryOp.Relation relType2 = new UnaryOp.Relation(Type2Relation);
                UnaryOp.Map convType2 = new UnaryOp.Map(Type2MapConvert);
                int[] keepType2 = { 0, 1, 2, 3, 4, 5 };

                return new OpDBAccess(LINEARDATABASE, null, statementType2,
                    new OpProject(convType2, keepType2,
                        new OpRelation(relType2, accountBalances,
                            new OpServerRaw(9449))));
            }
            #endregion

            #region Type 3

            public DataItem Type3Relation(DataItem di, RelationTable rt)
            {
                //<Time, VID, Xway, QID, Day, InTime(DateTime)>
                di.AddCapacity(1);
                object[] keys = { di[1] };
                lock (rt)
                {
                    DataRow drow = rt.Find(keys);
                    if (drow == null)
                    {
                        drow = rt.GetRow();
                        drow["VID"] = Convert.ToInt32(di[1]);
                        drow["Bal"] = 0;
                        drow["Time"] = DateTime.Now.Subtract(di.TimeStamp).TotalSeconds;
                        rt.Add(drow);
                        //Console.WriteLine("We had to make a new balance for a type 3 query"); 
                    }
                    di.AddValue(drow["Bal"]);
                }
                return di;
                //<Time, VID, Xway, QID, Day, Bal>
            }

            public DataItem Type3MapConvert(DataItem di)
            {
                //<Time, VID, Xway, QID, Day, Bal>
                di.AddCapacity(1);
                di.AddValue(Convert.ToInt32(di[0]) + (int)(DateTime.Now.Subtract(di.TimeStamp).TotalSeconds));
                return di;
                //<Time, VID, Xway, QID, Day, Bal, Emit>
            }

            private List<Query> GetType3()
            {
                List<Query> type3q = new List<Query>();
                string statementType3 = "INSERT INTO output3 (Type, Time, Emit, QID, Bal) VALUES('3', $0$, $6$, $3$, $5$)";
                string queryType3 = "SELECT Toll FROM tollhistory WHERE VID = $1$ AND Xway = $2$ AND Day = $4$";
                UnaryOp.Map mapConv = new UnaryOp.Map(Type3MapConvert);
                int[] keepType3 = { 0, 1, 2, 3, 4, 5, 6 };
                UnaryOp.Relation relType3 = new UnaryOp.Relation(Type3Relation);
                OpMultiplexer omxType3 = new OpMultiplexer(new OpServerRaw(9450));
                type3q.Add(omxType3);

                OpDBAccess dbtype3 = new OpDBAccess(LINEARDATABASE, null, statementType3,
                    new OpProject(mapConv, keepType3,
                        new OpUnion(
                            new OpDBAccess(LINEARDATABASE, queryType3, null,
                                new OpSelect("$1.5 > I0",
                                    omxType3.GetQuery())),
                            new OpRelation(relType3, accountBalances,
                                new OpSelect("$1.5 = I0",
                                    omxType3.GetQuery())))));
                type3q.Add(dbtype3);
                return type3q;
            }

            #endregion

            private void initTables()
            {
                NSSTable = new Dictionary<uint, uint>();
                IsDupTable = new List<int>((int)MAXVID);

                string[] accNames = {"VID", "Bal", "Time"};
                Type[] accTypes = { typeof(Int32), typeof(Int32), typeof(Int32) };
                int[] accIndexes = { 0 };
                accountBalances = new RelationTable(accNames, accTypes, accIndexes);

                DataRow drow;
                for (uint i = 0; i < MAXVID; i++)
                {
                    drow = accountBalances.GetRow();
                    drow["VID"] = i;
                    drow["Bal"] = 0;
                    drow["Time"] = 0;
                    accountBalances.Add(drow);
                }

                string[] acdntNames = { "Xway", "Dir", "Seg" };
                Type[] acdntTypes = { typeof(Int32), typeof(Int32), typeof(Int32) };
                int[] acdntIndexes = { 0, 1, 2};
                accidents = new RelationTable(acdntNames, acdntTypes, acdntIndexes);

                string[] tqNames = { "VID", "Seg", "LAV", "Toll" };
                Type[] tqTypes = { typeof(Int32), typeof(Int32), typeof(Int32), typeof(Int32) };
                int[] tqIndexes = { 0, 1 };
                tollQuotes = new RelationTable(tqNames, tqTypes, tqIndexes);

                string[] ssNames = { "Seg", "Xway", "Dir", "LAV", "Count", "LAVWID", "CountWID" };
                Type[] ssTypes = { typeof(Int32), typeof(Int32), typeof(Int32), typeof(Int32), typeof(Int32), typeof(UInt64), typeof(UInt64) };
                int[] ssIndexes = { 0, 1, 2 };
                segStats = new RelationTable(ssNames, ssTypes, ssIndexes);
            }

            public Query[] GetQueries()
            {
                return lrQueries.ToArray();
            }
        }
        #endregion
    }
}
