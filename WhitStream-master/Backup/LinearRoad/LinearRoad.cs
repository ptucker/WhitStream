using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using WhitStream;
using WhitStream.Server;
using WhitStream.Data;

namespace LinearRoad
{
    class LinearRoadFirehose
    {
        private const int NUMBEROFTYPES = 4;
        static void Main(string[] args)
        {
            LinearRoadServer lrs = new LinearRoadServer();
            List<List<DataItem>> ldis = new List<List<DataItem>>();
            List<LinearRoadClient> lrcs = new List<LinearRoadClient>();
            bool eof = false;

            for (int i = 0; i < NUMBEROFTYPES; i++)
            {
                ldis.Add(new List<DataItem>());
                lrcs.Add(new LinearRoadClient(9448 + i));
                lrcs[i].Init();
            }
            while (!eof)
            {
                while (lrs.queBuffer.Count > 0)
                {
                    DataItem di = lrs.queBuffer.Dequeue();
                    if (di != null)
                    {
                        int streamTo = (int)di[0];
                        switch (streamTo)//add to the type to the relevant list
                        {
                            case 0:
                                ldis[0].Add(di);
                                break;
                            case 2:
                                ldis[1].Add(di);
                                break;
                            case 3:
                                ldis[2].Add(di);
                                break;
                            case 4:
                                ldis[3].Add(di);
                                break;
                            default:
                                throw new System.Exception("The stream provided a type we don't know how to handle");
                        }
                        //Console.WriteLine(di.ToString());
                    }
                        
                }
                for (int i = 0; i < NUMBEROFTYPES; i++)
                {
                    if (ldis[i].Count > 0)
                    {
                        lrcs[i].addData(ldis[i]); //send the respective list
                        ldis[i].Clear();
                    }
                }
            }

        }
    }

    class LinearRoadServer
    {
        private static WhitStream.Server.TCPServer server = new WhitStream.Server.TCPServer("10.200.240.3", 9458);
        NetworkStream nsClient;
        private delegate void StreamReader();
        public Queue<DataItem> queBuffer = new Queue<DataItem>();
        private const int MAXDATA = 500, NUMBEROFINTS = 15;

        //Set up the server to listen for the LR feeder on port 9458
        public LinearRoadServer()
        {
            server.AddListener(SetClient, 9458);
        }

        public void SetClient(NetworkStream ns)
        {
            nsClient = ns;
            StreamReader sr = new StreamReader(StrReader);
            sr.BeginInvoke(null, null);
        }

        protected void StrReader()
        {
            const int STREAMDATABUFFER = 100;
            bool eof = false;
            BinaryFormatter bf = new BinaryFormatter();
            DataItem[] rgdi = new DataItem[STREAMDATABUFFER];
            int iData = 0, fiter = 0;
            do
            {
                while (iData < STREAMDATABUFFER && nsClient.DataAvailable && !eof)
                {
                    try
                    {
                        rgdi[iData] = new DataItem(NUMBEROFINTS, null);
                        while (fiter < NUMBEROFINTS && !eof) //TODO: this will lock out if an item is not completed, we may still want to push previously completed items
                        {
                            if (nsClient.DataAvailable)
                            {
                                byte[] inbytes = new byte[4];
                                nsClient.Read(inbytes, 0, 4); //read in the bytes
                                int inint = BitConverter.ToInt32(inbytes, 0);
                                if (inint != -1)
                                    rgdi[iData].AddValue(inint);
                                fiter++;
                            }
                        }
                    }
                    catch (System.IO.IOException)
                    {
                        //Assume that the connection was closed (though I have no way of knowing...)
                        eof = true;
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("Something bad happened when we tried to read from the stream: {0} ({1})",
                            ex.Message, ((ex.InnerException != null) ? ex.InnerException.Message : ""));
                        eof = true;
                    }
                    iData++;
                    fiter = 0;
                }
                lock (queBuffer)
                {
                    for (int i = 0; i < iData; i++)
                    {
                        queBuffer.Enqueue(rgdi[i]);
                        rgdi[i] = null;
                    }
                    iData = 0;
                }
            } while (!eof);
            nsClient.Close();

            if (rgdi.Length == 0 || (rgdi[rgdi.Length - 1] != null && !rgdi[rgdi.Length - 1].EOF))
            {
                lock (queBuffer)
                {
                    DataItem di = new DataItem(2, null);
                    di.EOF = true;
                    queBuffer.Enqueue(di);
                }
            }
        }

    }

    class LinearRoadClient
    {
        private int oPort;
        string stIP;
        TcpClient tc = null;
        List<DataItem> sendlist = new List<DataItem>();
        private delegate void Sender();

        public LinearRoadClient(int port)
        {
            oPort = port;
            stIP = "10.200.240.3";
        }

        public void Init()
        {
            bool retry;
            do
            {
                retry = false;
                try
                {
                    tc = new TcpClient(stIP, oPort);
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
                new Thread (sendData).Start();
        }

        public void addData(List<DataItem> ldi)
        {
            lock (sendlist)
            {
                sendlist.AddRange(ldi);
            }
        }

        private void sendData()
        {
            BinaryFormatter bf = new BinaryFormatter();
            NetworkStream ns = tc.GetStream();
            bool eof = false;
            Console.WriteLine("Sender initialized");
            Thread.Sleep(100);
            while (!eof)
            {
                if (sendlist.Count > 0)
                {
                    lock (sendlist)
                    {
                        while (sendlist.Count > 0)
                        {
                            Console.WriteLine(sendlist[0]);
                            bf.Serialize(ns, sendlist[0]);
                            sendlist.RemoveAt(0);
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}
