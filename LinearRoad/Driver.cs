using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Diagnostics;
using WhitStream;
using WhitStream.Data;

namespace LinearRoad
{
    class Go
    {
        static void Main(string[] args)
        {
            /*Process p = new Process();
            p.StartInfo = new ProcessStartInfo(Environment.CurrentDirectory + "\\LinearRoadPlan.exe");
            p.Start();*/
            new LinearRoadServer();
        }
    }

    class LinearRoadServer
    {
        TcpListener server;
        NetworkStream nsClient;
        private delegate void StreamReader();
        Stopwatch st = new Stopwatch();
        List<LinearRoadClient> lrcs = new List<LinearRoadClient>();
        private const int NUMBEROFINTS = 15,
            INDEXOFTIME = 1,
            INDEXOFVID = 2,
            INDEXOFSPEED = 3,
            INDEXOFLANE = 5,
            INDEXOFPOS = 8,
            TYPE0 = 9;
        uint lastTime = 0;
        Dictionary<uint, DateTime> vids = new Dictionary<uint, DateTime>(); //cars in the exit lane

        public LinearRoadServer()
        {
            for (int i = 0; i < 3; i++)
            {
                lrcs.Add(new LinearRoadClient(9448 + i));
            }
            try
            {
                server = new TcpListener(IPAddress.Parse("10.200.240.1"), 9458);
                server.Start();
                Socket sock = server.AcceptSocket();
                if (sock.Connected)
                {
                    Console.WriteLine("Connected to the driver");
                    nsClient = new NetworkStream(sock);
                    StrReader();
                }
                sock.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("We encountered an error when we tried to read from the stream: " + ex.Message);
            }
            

        }

        protected void StrReader()
        {
            bool eof = false, tagMonitor;
            BinaryFormatter bf = new BinaryFormatter();
            DataItem di;
            DateTime sysTime = new DateTime(0); //The "System Time"
            int fiter = 0, type = 0;
            do
            {
                di = new DataItem(TYPE0, null); //TYPE0 has the most ints we'll need
                tagMonitor = false;
                try
                {
                    while (fiter < NUMBEROFINTS && !eof)
                    {
                        if (sysTime.Ticks == 0) //initialize the time
                            sysTime = DateTime.Now;
                        byte[] inbytes = new byte[4];
                        nsClient.Read(inbytes, 0, 4); //read in the bytes
                        int inint = BitConverter.ToInt32(inbytes, 0);
                        if (inint != -1)
                        {
                            if (fiter == 0)
                                type = inint;

                            if (fiter == INDEXOFTIME && inint > lastTime) //change in the time
                            {
                                InsertPuncts(sysTime.AddSeconds(lastTime));
                                lastTime = (uint)inint;
                            }

                            if (fiter == INDEXOFLANE && inint == 4) //there is a car in the exit lane
                                tagMonitor = true;

                            if (fiter == INDEXOFTIME)
                                di.AddValue((ulong)TimeSpan.FromTicks(sysTime.AddSeconds(lastTime).Ticks).TotalSeconds);
                            else
                                di.AddValue(Convert.ToUInt32(inint));
                        }
                        fiter++;
                    }
                }
                catch (System.IO.IOException ex)
                {
                    //Assume that the connection was closed (though I have no way of knowing...)
                    Console.WriteLine("Something bad happened when we tried to read from the stream: {0} ({1})",
                        ex.Message, ((ex.InnerException != null) ? ex.InnerException.Message : ""));
                    eof = true;
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Something bad happened when we tried to read from the stream: {0} ({1})",
                        ex.Message, ((ex.InnerException != null) ? ex.InnerException.Message : ""));
                    eof = true;
                }
                if (tagMonitor)
                {
                    if (type == 0)
                    {
                        uint VID = Convert.ToUInt32(di[INDEXOFVID]);
                        if (!vids.ContainsKey(VID))
                        {
                            vids.Add(VID, sysTime.AddSeconds(lastTime));
                        }
                        else
                        {
                            vids[VID] = sysTime.AddSeconds(lastTime);
                        }
                    }
                }
                switch (type) //send the data item
                {
                    case 0:
                        //st.Start();
                        lrcs[0].sendData(di);
                        //st.Stop();
                        break;
                    case 2:
                        lrcs[1].sendData(di);
                        break;
                    case 3:
                        lrcs[2].sendData(di);
                        break;
                    case 4:
                        //lrcs[3].sendData(di);
                        break;
                    default:
                        throw new System.Exception("The stream provided a type we don't know how to handle");
                }
                fiter = 0;
            } while (!eof);
            nsClient.Close();
        }

        public void InsertPuncts(DateTime sysTime)
        {
            // insert new time punctuations
            Punctuation p = new Punctuation(TYPE0);
            for (int i = 0; i < TYPE0; i++)
            {
                if (i != INDEXOFTIME)
                    p.AddValue(new Punctuation.WildcardPattern());
                else
                    p.AddValue(new Punctuation.LiteralPattern((ulong)TimeSpan.FromTicks(sysTime.Ticks).TotalSeconds));
            }
            lrcs[0].sendData(p);

            // check for expiredVehichles
            List<uint> removeVids = new List<uint>();
            foreach(KeyValuePair<uint, DateTime> kv in vids)
            {

                if (sysTime.Subtract(kv.Value).TotalSeconds > 30) //it's been longer than 30 seconds
                {
                    removeVids.Add(kv.Key);
                }
            }
            foreach (uint val in removeVids)
            {
                Punctuation p2 = new Punctuation(TYPE0);
                for (int j = 0; j < TYPE0; j++)
                {
                    if (j != INDEXOFVID)
                        p2.AddValue(new Punctuation.WildcardPattern());
                    else
                        p2.AddValue(new Punctuation.LiteralPattern((uint)val));
                }
                lrcs[0].sendData(p2);
                vids.Remove(val);
            }
            removeVids.Clear();
        }
    }

    class LinearRoadClient
    {
        private int oPort;
        string stIP;
        TcpClient tc = null;
        NetworkStream ns;
        BinaryFormatter bf = new BinaryFormatter();

        public LinearRoadClient(int port)
        {
            oPort = port;
            stIP = "127.0.0.1";
            Init();
        }

        private void Init()
        {
            bool retry;
            int retries = 0;
            do
            {
                retry = false;
                try
                {
                    Thread.Sleep(500);
                    tc = new TcpClient(stIP, oPort);
                }
                catch (SocketException s)
                {
                    if (s.ErrorCode == (int)SocketError.ConnectionRefused)
                    {
                        if (retries > 2)
                        {
                            Console.WriteLine("Server not listening. Try again (y or n)?");
                            string resp = Console.ReadLine();
                            if (resp[0] == 'y' || resp[0] == 'Y')
                                retry = true;
                        }
                        else
                            retry = true;
                        retries++;
                    }
                    else
                    {
                        Console.WriteLine("Unexpected Socket Failure: {0} ({1})", s.Message,
                            ((s.InnerException != null) ? s.InnerException.Message : ""));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected Failure: {0} ({1})", ex.Message,
                        ((ex.InnerException != null) ? ex.InnerException.Message : ""));
                }
            } while (retry == true);

            if (tc != null)
            {
                ns = tc.GetStream();
                Console.WriteLine("Sender initialized");
            }
        }

        public void sendData(DataItem di)
        {
            bf.Serialize(ns, di);
        }
    }
}
