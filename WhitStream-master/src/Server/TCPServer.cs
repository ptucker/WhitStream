using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace WhitStream.Server
{
    /// <summary>
    /// Class to receive and manage TCP network connections
    /// </summary>
    public class TCPServer
    {
        /// <summary> Default WhitStream port </summary>
        public const int WHITSTREAM_PORT = 9448;
        /// <summary> Method to assign an operator to a port for reading data </summary>
        /// <param name="ns">The network stream to listen to</param>
        /// <seealso cref="WhitStream.QueryEngine.QueryOperators.OpServer"/>
        public delegate void SetClient(NetworkStream ns);

        private bool listening = true;
        private string stIP;
        private Dictionary<int, PortListener> connections = new Dictionary<int, PortListener>();
        private delegate void ServingThread(int port);


        /// <summary> Default constructor -- set listening port to default port </summary>
        public TCPServer()
        {
            string address = "127.0.0.1";
            IPHostEntry ipEntry = Dns.GetHostEntry(address);
            IPAddress[] addr = ipEntry.AddressList;
            for (int i = 0; i < addr.Length; i++)
            {
                if (addr[i].AddressFamily.ToString().Equals("InterNetwork"))
                {
                    address = addr[i].ToString();
                    break;
                }
            }
            Init(address, WHITSTREAM_PORT);
        }

        /// <summary> Constructor for setting specific IP and port </summary>
        /// <param name="ip">user-defined IP address to listen on</param>
        /// <param name="port">user-defined port to listen on</param>
        public TCPServer(string ip, int port)
        {
            Init(ip, port);
        }

        private void Init(string ip, int port)
        {
            stIP = ip;
            connections.Add(port, new PortListener());
            ServingThread st = new ServingThread(StartListening);
            st.BeginInvoke(port, null, null);
        }

        /// <summary>
        /// Add a listener (usually a query operator) to the list of listeners.
        /// When a stream registers with the system, one of the listeners from the
        /// queue will be assigned to that stream
        /// </summary>
        /// <remarks>The listener will listen on the default WhitStream port</remarks>
        /// <param name="sc">The method to call when a stream is found</param>
        public void AddListener(SetClient sc)
        {
            AddListener(sc, WHITSTREAM_PORT);
        }

        /// <summary>
        /// Add a listener (usually a query operator) to the list of listeners.
        /// When a stream registers with the system, one of the listeners from the
        /// queue will be assigned to that stream
        /// </summary>
        /// <param name="sc">The method to call when a stream is found</param>
        /// <param name="port">The port to listen on</param>
        public void AddListener(SetClient sc, int port)
        {
            if (!connections.ContainsKey(port))
            {
                Init(stIP, port); //make a new connection on the port
            }
            PortListener pl;
            connections.TryGetValue(port, out pl);

            bool clientSet = false;
            lock (pl.clients)
            {
                if (pl.clients.Count > 0)
                {
                    TcpClient c = pl.clients[0];
                    pl.clients.RemoveAt(0);
                    sc(c.GetStream());
                    clientSet = true;
                }
            }
            if (!clientSet)
            {
                lock (pl.servers)
                {
                    pl.servers.Add(sc);
                }
            }
        }

        private void StartListening(int port)
        {
            PortListener pl;
            connections.TryGetValue(port, out pl);
            pl.server = new TcpListener(IPAddress.Parse(stIP), port);
            pl.server.Start();
            bool serverSet;
            while (listening)
            {
                serverSet = false;
                TcpClient client = pl.server.AcceptTcpClient();
                lock (pl.servers)
                {
                    if (pl.servers.Count > 0)
                    {
                        SetClient sc = pl.servers[0];
                        pl.servers.RemoveAt(0);
                        sc(client.GetStream());
                        serverSet = true;
                    }
                }
                if (!serverSet)
                {
                    lock (pl.clients)
                    {
                        pl.clients.Add(client);
                    }
                }
            }
        }

        /// <summary>
        /// Keeps track of a TcpListener on a specific port, as well as all the servers and clients waiting
        /// </summary>
        private class PortListener
        {
            public TcpListener server;
            public List<SetClient> servers = new List<SetClient>();
            public List<TcpClient> clients = new List<TcpClient>();

            public PortListener()
            {
            }
        }
    }
}
