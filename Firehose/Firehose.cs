using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Firehose
{
    delegate bool FirehoseStart();

    class Program
    {
        static void Main(string[] args)
        {
            string stIP, stPort;
            int cThread = 1;
            int cMinutes = -1;
            bool fPunct = false;

            if (args.Length == 0)
            {
                Console.WriteLine("Which IP address for the WhitStream server?");
                Console.Write("\tPress enter for 127.0.0.1 (localhost): ");
                stIP = Console.ReadLine();
                if (stIP.Length == 0) stIP = "127.0.0.1";
            }
            else
                stIP = args[0];

            if (args.Length <= 1)
            {
                Console.WriteLine("Which port for the WhitStream server:");
                Console.Write("\tPress enter for port {0}: ", WhitStream.Server.TCPServer.WHITSTREAM_PORT);
                stPort = Console.ReadLine();
                if (stPort.Length == 0) stPort = WhitStream.Server.TCPServer.WHITSTREAM_PORT.ToString();
            }
            else
                stPort = args[1];

            if (args.Length <= 2)
            {
                string stThread;
                Console.WriteLine("How many Firehose threads:");
                Console.Write("\tPress enter for only 1 thread: ");
                stThread = Console.ReadLine();
                if (stThread.Length == 0) stThread = "1";
                cThread = Int32.Parse(stThread);
            }
            else
                cThread = Int32.Parse(args[2]);

            if (args.Length <= 3)
            {
                string stMinutes;
                Console.WriteLine("How many minutes:");
                Console.Write("\tPress enter to run indefinitely: ");
                stMinutes = Console.ReadLine();
                if (stMinutes.Length == 0) stMinutes = "-1";
                cMinutes = Int32.Parse(stMinutes);
            }
            else
                cMinutes = Int32.Parse(args[3]);

            if (args.Length <= 4)
            {
                string stPunct;
                Console.WriteLine("Punctuations [y or n]:");
                Console.Write("\tPress enter for no punctuations: ");
                stPunct = Console.ReadLine();
                if (stPunct.Length == 0) stPunct = "n";
                fPunct = (stPunct[0] == 'y');
            }
            else
                fPunct = (args[4][0] == 'p');

            Firehose[] rgf = new Firehose[cThread];
            FirehoseStart[] rgfs = new FirehoseStart[cThread];

            for (int iThread = 0; iThread < cThread; iThread++)
            {
                rgf[iThread] = new Firehose(stIP, Int32.Parse(stPort), fPunct, iThread);
                rgfs[iThread] = new FirehoseStart(rgf[iThread].Start);
            }
            for (int iThread = 0; iThread < cThread; iThread++)
                rgfs[iThread].BeginInvoke(null, null);

            if (cMinutes <= 0)
            {
                Console.WriteLine("Generating data. Press any key to quit.");
                Console.ReadKey(true);
            }
            else
            {
                Console.WriteLine("Generating data for {0} minute{1}.", cMinutes, (cMinutes == 1) ? "" : "s");
                Thread.Sleep(new TimeSpan(0, cMinutes, 0));
            }

            for (int iThread = 0; iThread < cThread; iThread++)
                rgf[iThread].Stop();

        }
    }

    class Firehose 
    {
        private string stIP;
        private int nPort;
        private bool fPunct;
        private int id;
        private bool produceData = true;
        delegate void Generator(TcpClient tc);
        IAsyncResult ar;

            public int PowerHose() //returns random power levels from 9230kw per hour/60/60 to 15061kw per hour/60/60
        {
            Random r = new Random();
            int rInt = r.Next(((9230/60)/60),((15061/60)/60));
            return rInt; //for ints
        }

        public Firehose(string IP, int port, bool punct, int i)
        {
            stIP = IP;
            nPort = port;
            fPunct = punct;
            id = i;
        }

        public bool Start()
        {
            bool retry;
            TcpClient tc = null;
            do
            {
                retry = false;
                try
                {
                    tc = new TcpClient(stIP, nPort);
                }
                catch (SocketException s)
                {
                    if (s.ErrorCode == (int)SocketError.ConnectionRefused)
                    {
                        Console.WriteLine("Server not listening. Try again (y or n)?");
                        string resp = Console.ReadLine();
                        if (resp[0] == 'y' || resp[0] == 'Y')
                            retry = true;
                    }
                    else
                    {
                        Console.WriteLine("Unexpected Failure: {0} ({1})", s.Message,
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
                Generator g = new Generator(GenerateData);

                ar = g.BeginInvoke(tc, null, null);
                return true;
            }
            else
                return false;
        }

        public void Stop() {
            produceData = false;
            if (ar != null && ar.AsyncWaitHandle != null)
                ar.AsyncWaitHandle.WaitOne();
        }

        private const int MAXDATA = 100;
        private void GenerateData(TcpClient tc)
        {
            UInt64 iRow = 0;
            WhitStream.Data.DataItem[] rgdi = new WhitStream.Data.DataItem[MAXDATA];
            WhitStream.Data.Punctuation p = new WhitStream.Data.Punctuation(2);
            BinaryFormatter bf = new BinaryFormatter();
            NetworkStream ns = tc.GetStream();

            for (int i = 0; i < MAXDATA; i++)
                rgdi[i] = new WhitStream.Data.DataItem(2, null);

            int cData;
            Random rnd = new Random();
            while (produceData)
            {
                cData = rnd.Next(MAXDATA);
                UInt64 iRowStart = iRow;
                for (int i = 0; i < cData; i++)
                {
                    rgdi[i].Clear();
                    //-------------------------------------------------------------------------->added power hose to give a reasonable guess to amount off power used max 15061kw to 9230kw perhour per person
                    int num = PowerHose();
                    rgdi[i].AddValue(num);
                    //rgdi[i].AddValue((UInt64)((UInt64)iRow / 7));
                    //rgdi[i].AddValue((UInt64)iRow);
                    rgdi[i].EOF = false;
                    iRow++;
                }

                for (int i = 0; i < cData; i++)
                    bf.Serialize(ns, rgdi[i]);

                if (fPunct && iRow - iRowStart > 0)
                {
                    p.Clear();
                    p.AddValue(new WhitStream.Data.Punctuation.WildcardPattern());
                    object[] rglp = new object[iRow - iRowStart];
                    for (UInt64 iLit = 0; iLit < iRow - iRowStart; iLit++)
                        rglp[iLit] = ((UInt64) (iLit + iRowStart));
                    p.AddValue(new WhitStream.Data.Punctuation.ListPattern(rglp));
                    bf.Serialize(ns, p);
                }

                //if (iPunct == 2)
                //{
                //    if (iRow >= CPUNCTLISTSIZE && iRow % CPUNCTLISTSIZE == 0)
                //    {
                //        WhitStream.Data.Punctuation p2 = new Punctuation(2);
                //        p2.AddValue(new WhitStream.Data.Punctuation.WildcardPattern());
                //        int iMin = iRow - CPUNCTLISTSIZE + 1, iMax = iRow;
                //        p2.AddValue(new WhitStream.Data.Punctuation.RangePattern(iMin, iMax));
                //        ldiBufferOut.Add(p2);
                //    }
                //}
            }

            if (fPunct)
            {
                p.Clear();
                p.AddValue(new WhitStream.Data.Punctuation.WildcardPattern());
                p.AddValue(new WhitStream.Data.Punctuation.WildcardPattern());
                bf.Serialize(ns, p);
            }
            WhitStream.Data.DataItem di = new WhitStream.Data.DataItem(2, null);
            di.AddValue((UInt64)((UInt64)iRow / 7));
            di.AddValue((UInt64)iRow);
            di.EOF = true;

            bf.Serialize(ns, di);

            ns.Flush();
            System.Threading.Thread.Sleep(1000);
            ns.Close();

            Console.WriteLine("Generated {0} data items.", iRow);
        }
    }
}
