using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using WhitStream.QueryEngine.QueryOperators;
using WhitStream.Data;

namespace WhitStream.Utility
{
	#region Utility Functions/Types
	/// <summary>
	/// Utility functions for converting bytes, ints, strings, etc
	/// </summary>
	public class Util
	{
		/// <summary>
		/// Static types - byte, ushort, uint
		/// </summary>
		public static Type TypeByte = new Byte().GetType(), TypeUShort = new UInt16().GetType(), TypeUInt = new UInt32().GetType();

		/// <summary>
		/// Converts a byte[4] into an int
		/// </summary>
		/// <param name="b"> The byte[4] to be converted into an int </param>
		public static uint ByteToUInt(byte[] b)
		{
			uint i = ((uint)b[3] << 24) | ((uint)b[2] << 16) | ((uint)b[1] << 8) | (uint)b[0];
			return i;
		}

		/// <summary>
		/// Converts an int to a byte[4]
		/// </summary>
		/// <param name="i"> The int to be converted into a byte[4] </param>
		public static byte[] IntToByte(int i)
		{
			byte[] b = new byte[4];
			b[3] = (byte)(i >> 24);
			i -= (int)(b[3] << 24);
			b[2] = (byte)(i >> 16);
			i -= (int)(b[2] << 16);
			b[1] = (byte)(i >> 8);
			i -= (int)(b[1] << 8);
			b[0] = (byte)i;

			return b;
		}

		/// <summary>
		/// Convert a phrase stored in bytes into a string
		/// </summary>
		/// <param name="str_in_b">Byte array holding the phrase</param>
		/// <param name="len">Length of the byte array</param>
		public static string ByteToString(byte[] str_in_b, int len)
		{
			string str = "";
			for (int i = 0; i < len; i++)
			{
				str += (char)str_in_b[i];
			}
			return str;
		}

		/// <summary>
		/// Convert a phrase stored in a cyclic buffer into a string
		/// </summary>
		/// <param name="CQ">Cyclic Buffer</param>
		/// <param name="len">Length of the data in the buffer</param>
		public static string ByteToString(CyclicQueue CQ, int len)
		{
			string str = "";
			for (int i = 0; i < len; i++)
			{
				str += (char)CQ[i];
			}
			return str;
		}

		/// <summary>
		/// Convert a phrase stored in a string into bytes
		/// </summary>
		/// <param name="str">String array holding the phrase</param>
		public static byte[] StringToByte(string str)
		{
			byte[] str_in_b = new byte[str.Length];
			for (int i = 0; i < str.Length; i++)
			{
				str_in_b[i] = (byte)str[i];
			}
			return str_in_b;
		}

		/// <summary>
		/// Converts a byte[4] into an int
		/// </summary>
		/// <param name="b"> The byte[4] to be converted into an int </param>
		/// <param name="offset"> Where to start in the byte array - the LSB</param>
		/// <param name="up">If to travel up or down the array</param>
		public static uint ByteToUInt(byte[] b, int offset, bool up)
		{
			if (up)
			{
				uint i = ((uint)b[offset + 3] << 24) | ((uint)b[offset + 2] << 16) | ((uint)b[offset + 1] << 8) | (uint)b[offset];
				return i;
			}
			else
			{
				uint i = ((uint)b[offset - 3] << 24) | ((uint)b[offset - 2] << 16) | ((uint)b[offset - 1] << 8) | (uint)b[offset];
				return i;
			}

		}
		
		/// <summary>
		/// Converts a Cyclic buffer into an int
		/// </summary>
		/// <param name="CQ"> The cyclic buffer to be converted into an int </param>
		/// <param name="offset"> Where to start in the cyclic buffer</param>
		/// <param name="up">Whether to go up or down the buffer</param>
		/// <remarks>This does not convert the entire cyclic buffer.  Merely the next 4 bytes.</remarks>
		public static uint ByteToUInt(CyclicQueue CQ, int offset, bool up)
		{
			if (up)
			{
				uint i = ((uint)CQ[offset + 3] << 24) | ((uint)CQ[offset + 2] << 16) | ((uint)CQ[offset + 1] << 8) | (uint)CQ[offset];
				return i;
			}
			else
			{
				uint i = ((uint)CQ[offset - 3] << 24) | ((uint)CQ[offset - 2] << 16) | ((uint)CQ[offset - 1] << 8) | (uint)CQ[offset];
				return i;
			}
		}

		/// <summary>
		/// Gets the type and returns it as an int
		/// </summary>
		/// <param name="di"></param>
		public static int ReturnInt(object di)
		{
			if (di is byte)
				return (int)((byte)di);
			else if (di is ushort)
				return (int)((ushort)di);
			else if (di is uint)
				return (int)((uint)di);
			else
				return (int)di;
		}
	}
	#endregion

	#region Cyclic Queue/Buffer
	/// <summary>
	/// A simple cyclic queue.  Instead of being an array of objects, this uses an
	/// array of bytes which make up the objects.  Before the start of each object is
	/// 4 bytes that contain the length of that object
	/// </summary>
	public class CyclicQueue
	{
		/// <summary>
		/// The cyclic array
		/// </summary>
		/// <remarks>Do not use other than within a stream.read() call.
		/// Use custom indexing to get the byte values.  Needs to be public
		/// for the read() function call.</remarks>
		byte[] m_cyclicBuffer;
		//Size of the array (cyclic queue)
		int m_arraySize;
		//Index of the next object
		int m_nextIndex;
		//Index of the byte after the last byte of the last object
		int m_availableIndex;
		//Set when av_byte looped back to zero
		bool m_looped;
		//Holds the count of current and next loops counts
		int m_currentLoopCount, m_nextLoopCount;

		/// <summary>
		/// Constructs a cyclic queue
		/// </summary>
		/// <param name="q_size">Fixed size of the queue.  Will never change.</param>
		public CyclicQueue(int q_size)
		{
			m_arraySize = q_size;
			m_cyclicBuffer = new byte[m_arraySize];
			m_looped = false;
		}

		/// <summary>
		/// Gets the index of the where the incoming data item should be put depending on that
		/// data item's length.  The main function of CyclicQueue.
		/// </summary>
		/// <remarks>This also places the length of the incoming data item in the first four bytes.</remarks>
		/// <param name="len">Length of the data item (in bytes).</param>
		/// <returns>The next available index to play the data item.</returns>
		public int New_Index(int len)
		{
			lock (this)
			{
				//Is there enough space to put the data item at the end of the buffer
				if (m_availableIndex + len + 4 <= m_arraySize)
				{
					if (m_availableIndex <= m_nextIndex && m_looped)
					{
						while (m_availableIndex + len + 4 > m_nextIndex && m_looped)
						{
							Monitor.Wait(this);
						}
					}
					InsertLen(len);
					int ret = m_availableIndex + 4;
					m_availableIndex += len + 4;
					return ret;
				}
				//No room - loop to the front
				else
				{
					if (m_availableIndex == m_nextIndex)
					{
						if (m_looped)
						{
							while (m_looped)
							{
								Monitor.Wait(this);
							}
						}
						else
						{
							m_availableIndex = 0;
							m_nextIndex = 0;
							InsertLen(len);
							m_availableIndex = len + 4;
							return 4;
						}
					}
					else if (m_availableIndex < m_nextIndex && m_looped)
					{
						while (m_looped)
						{
							Monitor.Wait(this);
						}
					}
					m_availableIndex = 0;
					//Make sure there is room
					while (len + 4 > m_nextIndex && m_currentLoopCount > 0)
					{
						Monitor.Wait(this);
					}
					m_looped = true;
					InsertLen(len);
					//Reset next_object to our position since we had to loop
					if (m_currentLoopCount == 0)
					{
						m_nextIndex = 0;
						m_looped = false;
					}
					m_availableIndex = len + 4;
					return 4;
				}
			}
		}

		/// <summary>
		/// Overloaded ++ Operator
		/// </summary>
		public static CyclicQueue operator ++(CyclicQueue CQ)
		{
			lock (CQ)
			{
				//CQ.sb.Append(string.Format("\nDecrementing cur count: {0} -> {1}", CQ.cur_loop_count, CQ.cur_loop_count - 1));
				//++ is called after we parse a data item - decrement counter
				CQ.m_currentLoopCount--;

				if (CQ.m_looped)
				{
					//Haven't reached the end of the current loop
					if (CQ.m_currentLoopCount > 0)
					{
						CQ.m_nextIndex += CQ.Next_Object_Len + 4;
					}
					//Reached the end - restart at the beginning
					else
					{
						CQ.m_currentLoopCount = CQ.m_nextLoopCount;
						CQ.m_nextLoopCount = 0;
						CQ.m_nextIndex = 0;
						CQ.m_looped = false;
					}
				}
				else
				{
					CQ.m_nextIndex += CQ.Next_Object_Len + 4;
				}

				Monitor.PulseAll(CQ);
				return CQ;
			}
		}

		/// <summary> Support indexing on the cyclic queue </summary>
		/// <param name="i">The attribute to read</param>
		/// <returns>The value for that byte</returns>
		/// <remarks>This returns the value of the byte 4 passed the index
		/// of the next object.  In other words - 0 will return the first byte
		/// of the object.  Read-only</remarks>
		public byte this[int i]
		{
			get { return m_cyclicBuffer[m_nextIndex + 4 + i]; }
		}

		/// <summary>
		/// Gets the size of the array
		/// </summary>
		/// <remarks>Read-only</remarks>
		public int Size
		{ get { return m_arraySize; } }

		/// <summary>
		/// Gets the index of the next object
		/// </summary>
		/// <remarks>Read-only</remarks>
		public int Next_Object
		{ get { return m_nextIndex; } }

		/// <summary>
		/// Gets the length of the next object
		/// </summary>
		/// <remarks>Read-only</remarks>
		public int Next_Object_Len
		{
			get { return (int)Util.ByteToUInt(m_cyclicBuffer, m_nextIndex, true); }
		}

		/// <summary>
		/// Gets the total count of data items in the queue
		/// </summary>
		/// <remarks>Read-only</remarks>
		public int Total_Count
		{
			get { return m_currentLoopCount + m_nextLoopCount; }
		}

		/// <summary>
		/// Get the byte[] buffer for the cyclic queue
		/// </summary>
		public byte[] Buffer
		{
			get { return m_cyclicBuffer; }
		}

		/// <summary>
		/// Increments the right count depending on if the buffer has looped
		/// </summary>
		/// <remarks>Write-only.  This doesn't return anything useful, just 0.</remarks>
		public void IncCount()
		{
			lock (this)
			{
				if (m_looped)
				{
					//sb.Append(string.Format("\nIncrementing next count: {0} -> {1}", next_loop_count, next_loop_count + 1));
					m_nextLoopCount++;
				}
				else
				{
					//sb.Append(string.Format("\nIncrementing cur count: {0} -> {1}", cur_loop_count, cur_loop_count + 1));
					m_currentLoopCount++;
				}
			}
		}

		//Used by New_Index only - relies on av_byte being set correctly
		private void InsertLen(int len)
		{
			//Place the length of the data item infront
			m_cyclicBuffer[m_availableIndex + 3] = (byte)(len >> 24);
			len -= (int)(m_cyclicBuffer[m_availableIndex + 3] << 24);
			m_cyclicBuffer[m_availableIndex + 2] = (byte)(len >> 16);
			len -= (int)(m_cyclicBuffer[m_availableIndex + 2] << 16);
			m_cyclicBuffer[m_availableIndex + 1] = (byte)(len >> 8);
			len -= (int)(m_cyclicBuffer[m_availableIndex + 1] << 8);
			m_cyclicBuffer[m_availableIndex + 0] = (byte)len;
		}
	}
	#endregion

	#region Schema
	/// <summary>
	/// Holds the packet specification in a list of byte[] so that
	/// a packet can be parsed correctly
	/// </summary>
	/// <remarks>
	/// The position is in the form start,finish.  For example, an integer will
	/// be 13,10 such that byte[13] is shifted 24, byte[12] is shifted 16, etc.
	/// </remarks>
	public class Schema
	{
		/// <summary>
		/// Holds the list of attributes that make up the Schema
		/// </summary>
		public List<Attribute> attributes;

		/// <summary>
		/// Constructor to initialize the class
		/// </summary>
		public Schema()
		{
			attributes = new List<Attribute>();
		}

		/// <summary>
		/// Adds an attribute to the schema
		/// </summary>
		/// <param name="MSB"> The most significant byte </param>
		/// <param name="LSB"> The least significant byte</param>
		/// <param name="name"> The name of the attribute being store </param>
		public void AddAttr(int MSB, int LSB, String name)
		{
			//Add the type as a byte
			if (Math.Abs(MSB - LSB) == 0)
				attributes.Add(new Attribute(MSB, LSB, name, Util.TypeByte));
			//Add the type as a short
			else if (Math.Abs(MSB - LSB) == 1)
				attributes.Add(new Attribute(MSB, LSB, name, Util.TypeUShort));
			//Add the type as an int
			else if (Math.Abs(MSB - LSB) == 3)
				attributes.Add(new Attribute(MSB, LSB, name, Util.TypeUInt));
			else
				attributes.Add(new Attribute(MSB, LSB, name, null));
		}

		/// <summary>
		/// Support indexing on a particular attribute
		/// </summary>
		/// <param name="i">The attribute to read/write</param>
		/// <returns>The value for that attribute</returns>
		/// <remarks>Read only.</remarks>
		public Attribute this[int i]
		{
			get { return attributes[i]; }
		}

		/// <summary>
		/// Read only - gets the count of attributes
		/// </summary>
		public int Count
		{
			get { return attributes.Count; }
		}

		/// <summary>
		/// Read only - returns a list of all the names in the Schema
		/// </summary>
		public List<String> Names
		{
			get
			{
				List<String> ret = new List<string>(Count);
				foreach (Attribute attr in attributes)
				{
					ret.Add(attr.Name);
				}
				return ret;
			}
		}

		#region Attribute
		/// <summary>
		/// Holds an attribute for a dataitem
		/// </summary>
		public class Attribute
		{
			/// <summary>
			/// Most and least significant bytes.  The MSB is shifted over
			/// 24 bits and the LSB is the last byte of an int.
			/// </summary>
			private int msB, lsB;
			/// <summary>
			/// Name of the attribute
			/// </summary>
			private String name;
			/// <summary>
			/// Type of the attribute - byte, ushort, uint
			/// </summary>
			private Type t;

			/// <summary>
			/// Constructor for an Attribute
			/// </summary>
			public Attribute(int msb, int lsb, String attr_name, Type attr_type)
			{
				msB = msb;
				lsB = lsb;
				name = attr_name;
				t = attr_type;
			}

			/// <summary>
			/// Read only - get the msB
			/// </summary>
			public int MSB
			{
				get { return msB; }
			}

			/// <summary>
			/// Read only - get the lsB
			/// </summary>
			public int LSB
			{
				get { return lsB; }
			}

			/// <summary>
			/// Read only - get the name
			/// </summary>
			public String Name
			{
				get { return name; }
			}

			/// <summary>
			/// Read only - get the type
			/// </summary>
			public Type Type
			{
				get { return t; }
			}

		}
		#endregion
	}
	#endregion

	#region Connection Manager
	/// <summary>
	/// Holds a list of connections so that when a connection is needed
	/// for a query, it is easily found in the list
	/// </summary>
	public class ConnectionManager
	{
		//Holds all the connections made to the server
		List<Connection> m_connectionList;
		//Holds all avaliable port numbers starting at 4001 to 4010
		//meaning the server can holds 10 connections
		Queue<int> m_availablePorts;
		//Socket connecting the server to a particular source
		Socket m_localListenSocket;
		//Stream to read/write to the socket
		NetworkStream m_netstream;
		//Listener to connect with the source
		TcpListener m_incomingListener;
		//Checks non-started connections to see if there are still there
		Thread m_checkThread;
		//Host ip address to create the connections with
		IPAddress m_ipAddress;
		//Host listen port
		int m_listenPort;
		//Whether the ConnectionManger is alive
		bool m_alive = true;

		/// <summary>
		/// Constructor to create the list of connects and create
		/// a new thread to wait/accept new connections
		/// </summary>
		public ConnectionManager(String host, String p)
		{
			Log.WriteMessage("WhitStream Server Started.", Log.eMessageType.Normal);
			m_ipAddress = IPAddress.Parse(host);
			m_listenPort = int.Parse(p);

			//Initialize the list to hold no connections and default capacity
			m_connectionList = new List<Connection>();

			//Initialize the list to hold all ports from 4001 to 4010
			m_availablePorts = new Queue<int>(10);
			for (int i = 4001; i <= 4010; i++)
			{
				m_availablePorts.Enqueue(i);
			}

			Log.WriteMessage("Waiting for connections....", Log.eMessageType.Debug);

			//Register a new TcpListener
			m_incomingListener = new TcpListener(m_ipAddress, m_listenPort);
			m_incomingListener.Start();
			m_incomingListener.BeginAcceptSocket(new AsyncCallback(Accept_Socket), null);

			m_checkThread = new Thread(CheckConnections);
			m_checkThread.Start();
		}
		/// <summary>
		/// Constructor to create the list of connects and create
		/// a new thread to wait/accept new connections
		/// </summary>
		public ConnectionManager(String p)
		{
			Log.WriteMessage("WhitStream Server Started.", Log.eMessageType.Normal);
			m_listenPort = int.Parse(p);

			//Initialize the list to hold no connections and default capacity
			m_connectionList = new List<Connection>();

			//Initialize the list to hold all ports from 4001 to 4010
			m_availablePorts = new Queue<int>(10);
			for (int i = 4001; i <= 4010; i++)
			{
				m_availablePorts.Enqueue(i);
			}

			Log.WriteMessage("Waiting for connections....", Log.eMessageType.Debug);

			//Register a new TcpListener
			m_incomingListener = new TcpListener(IPAddress.Any, m_listenPort);
			m_incomingListener.Start();
			m_incomingListener.BeginAcceptSocket(new AsyncCallback(Accept_Socket), null);

			m_checkThread = new Thread(CheckConnections);
			m_checkThread.Start();
		}

		/// <summary>
		/// Accepts incoming connections
		/// </summary>
		private void Accept_Socket(IAsyncResult ar)
		{
			try
			{
				//TcpListener l = (TcpListener)ar.AsyncState;
				m_localListenSocket = m_incomingListener.EndAcceptSocket(ar);

				//Create a networkstream to read/write to the socket
				m_netstream = new NetworkStream(m_localListenSocket);
				m_netstream.ReadTimeout = 10000;

				//Wait for the socket to send data
				while (!m_netstream.DataAvailable)
					Thread.Sleep(1000);

				if (m_availablePorts.Count > 0)
				{
					//Buffer to hold the length of an incoming packet
					byte[] n_len = new byte[4];
					int len;

					//Read the length of the incoming packet
					m_netstream.Read(n_len, 0, 4);

					//Convert length to an int
					len = (int)Util.ByteToUInt(n_len);

					//Buffer to hold the packet
					byte[] con_name = new byte[len];

					//Read the rest of the packet
					m_netstream.Read(con_name, 0, len);

					string name = Util.ByteToString(con_name, con_name.Length);
					Log.WriteMessage(string.Format("Connection found with name {0}.", name), Log.eMessageType.Debug);

					//Get a new port for the connection and send the port to the source
					int new_port = m_availablePorts.Dequeue();
					m_netstream.Write(Util.IntToByte(new_port), 0, 4);

					lock (m_connectionList)
					{
						//Add the connection to the list so it can be found
						if (m_ipAddress != null)
						{
							m_connectionList.Add(new Connection(m_ipAddress, new_port, name));
						}
						else
						{
							m_connectionList.Add(new Connection(IPAddress.Any, new_port, name));
						}

					}
				}
				else
				{
					Log.WriteMessage("Sorry, no more ports are avaliable to connect to.  Connection not accepted.", Log.eMessageType.Debug);
				}
			}
			catch (Exception)
			{
				Log.WriteMessage("Inside Accept_Socket().", Log.eMessageType.Error);
			}

			//Start Accepting more Connections
			m_incomingListener.BeginAcceptSocket(new AsyncCallback(Accept_Socket), null);
		}

		/// <summary>
		/// Check the connections
		/// </summary>
		void CheckConnections()
		{
			byte[] test_byte = new byte[0];
			List<Connection> removeList = new List<Connection>(1);

			while (m_alive)
			{
				if(m_connectionList.Count > 0)
				{
					//Go through each connection to check to see if it is still alive
					//Log.WriteMessage("Checking connections");
					foreach (Connection con in m_connectionList)
					{
						NetworkStream netstream = con.NetStream;

						lock (con.m_readLock)
						{
							try
							{
								//Either returns after reading nothing or exits
								//immediately if there is not connection to read from

								//If there's data - it will think that it read something
								if (netstream.DataAvailable)
								{
									netstream.Read(test_byte, 0, 0);
									//Log.WriteMessage("Check con read {0} bytes.", len);
								}
								else
								{
									//If no data - we need to set the socket to non-blocking
									con.LocalSocket.Blocking = false;
									netstream.Read(test_byte, 0, 0);
									//Log.WriteMessage("Check con read {0} bytes.", len);
								}

							}
							catch (IOException e)
							{
								if (e.InnerException is SocketException)
								{
									SocketException se = (SocketException)e.InnerException;
									if (se.ErrorCode == 10054)
									{
										//Let the connection clean up what it needs to
										con.Terminate();
										//Add its port back to the available ports
										m_availablePorts.Enqueue(con.ID);
										//Remove the connection from the queue
										//Log.WriteMessage("Removing: {0}:{1}", con.Name, con.ID);
										//Lets add it to a temp list to remove later
										removeList.Add(con);
									}
								}
							}
						}
					}

					if (removeList.Count > 0)
					{
						foreach (Connection con in removeList)
							m_connectionList.Remove(con);
						removeList.Clear();
					}
				}
				Thread.Sleep(5000);
			}
		}

		/// <summary>
		/// Return the connection wanted by the query
		/// </summary>
		/// <param name="name"> Name of the connection wanted </param>
		public Connection LocateCon(String name)
		{
			if (m_connectionList.Count == 0)
				return null;
			foreach (Connection con in m_connectionList)
			{
				if (con.Name == name)
					return con;
			}
			return null;
		}

		/// <summary>
		/// Return the connection wanted by the query
		/// </summary>
		/// <param name="id"> ID of the connection wanted </param>
		public Connection LocateCon(int id)
		{
			if (m_connectionList.Count == 0)
				return null;
			foreach (Connection con in m_connectionList)
			{
				if (con.ID == id)
					return con;
			}
			return null;
		}

		/// <summary>
		/// Return the connection wanted by the query
		/// </summary>
		/// <param name="id"> ID of the connection wanted </param>
		/// <param name="name"> Name of the connection wanted </param>
		public Connection LocateCon(int id, String name)
		{
			if (m_connectionList.Count == 0)
				return null;
			foreach (Connection con in m_connectionList)
			{
				if (con.ID == id && con.Name == name)
					return con;
			}

			return null;
		}

		/// <summary>
		/// Get a list of all the connections represented as strings
		/// </summary>
		public List<Connection> Connections
		{
			get { return m_connectionList; }
		}

		/// <summary>
		/// Kills the connection manager
		/// </summary>
		public void Kill()
		{
			try
			{
				foreach (Connection con in m_connectionList)
				{
					con.Terminate();
				}
				//TODO: take this out eventually
				m_checkThread.Abort();
				m_alive = false;
			}
			catch {}
		}
	}
	#endregion

	#region DataItem Pool
	/// <summary>
	/// Class to allocate a pool of data items which get used and then replaced
	/// </summary>
	public class DataItemPool
	{
        /// <summary>Get a data item from the data pool</summary>
        /// <param name="cdi">The number of data items to get</param>
        /// <returns>A array of data items to manipulate</returns>
        public delegate DataItem[] GetDataItem(int cdi);
        /// <summary>Return a data item to the pool</summary>
        /// <param name="di">the data item to return</param>
        public delegate void ReleaseDataItem(DataItem di);

		/// <summary>
		/// Queue to hold a pool of data items
		/// </summary>
		Queue<DataItem> m_dataQueue;

        int m_cntDI = 0;
        /// <summary> How many data items are currently in the queue </summary>
        public int DICount { get { return m_cntDI; } }

		/// <summary>
		/// The initial count created
		/// </summary>
		/// <remarks>Useful for determining how many more data items to create</remarks>
		int m_initCount;

		/// <summary>
		/// Number of attributes in the data items
		/// </summary>
		int m_attributeCount;

        bool m_fGlobal;

		/// <summary>
		/// Initialize this class to allocate a number of dataitems for a given pool
		/// </summary>
		/// <param name="count">Number of initial dataitems to queue up</param>
        /// <param name="fGlob">Is this pool global or local to this thread</param>
		public void Init(int count, bool fGlob)
		{
            m_fGlobal = fGlob;
            m_attributeCount = sizeof(UInt64) * 8;
#if DATAITEMPOOL
			m_initCount = count;
			m_dataQueue = new Queue<DataItem>(count);
			CreateMoreDataItems(count);
#endif
		}

        /// <summary> Initialize a local data item pool </summary>
        /// <param name="count">Number of data items to queue</param>
        public void Init(int count)
        {
            Init(count, false);
        }

		/// <summary>
		/// Create more data items for the system to use
		/// </summary>
		/// <param name="count">The count to create</param>
		/// <returns>False if we ran out of memory, true otherwise</returns>
		private bool CreateMoreDataItems(int count)
		{
#if DEBUG
            if (m_fGlobal)
                System.Diagnostics.Trace.WriteLine("Still using a global thread pool rather than a local pool");
#endif
#if DATAITEMPOOL
			lock (m_dataQueue)
			{
                m_cntDI += count;
				for (int i = 0; i < count; i++)
					m_dataQueue.Enqueue(new DataItem(m_attributeCount, this.ReleaseItem));
			}
#endif
			return true;
		}

		/// <summary>
		/// Delete some data items from the queue (to clean up memory)
		/// </summary>
		/// <param name="count">The count to delete</param>
		/// <returns>False if didn't delete count, else true</returns>
		/// <remarks>Not sure why a false return would be necessary.</remarks>
		private bool DeleteSomeDataItems(int count)
		{
#if DATAITEMPOOL
			lock (m_dataQueue)
			{
				int generation = GC.GetGeneration(m_dataQueue.Peek());
				for (int i = 0; i < count && i < m_dataQueue.Count; i++)
				{
					m_dataQueue.Dequeue();
				}
#if MONO
				//Mono doesn't support GCCollectionMode
				GC.Collect(generation);
#else
				GC.Collect(generation, GCCollectionMode.Forced);
#endif
			}
#endif
			return true;
		}

		/// <summary>
		/// Return a data item from the pool
		/// </summary>
		/// <returns>A fresh dataitem</returns>
		/// <remarks>If there are no more data items, it creates some more</remarks>
		public DataItem[] GetItem(int cdi)
		{
#if DATAITEMPOOL
			DataItem[] items = new DataItem[cdi];

			lock (m_dataQueue)
			{
				if (m_dataQueue.Count < cdi)
					CreateMoreDataItems(Math.Max(m_initCount, cdi));
                for (int i = 0; i < cdi; i++)
                {
                    items[i] = m_dataQueue.Dequeue();
                    items[i].Disposed = false;
                }
			}
			return items;
#else
            return new DataItem(m_attributeCount);
#endif
		}

        /// <summary>
        /// Return a data item from the pool
        /// </summary>
        /// <returns>A fresh dataitem</returns>
        /// <remarks>If there are no more data items, it creates some more</remarks>
        public DataItem GetItem()
        {
            return GetItem(1)[0];
        }

		/// <summary>
		/// Give an item back to the pool
		/// </summary>
		/// <param name="item">The item to give back</param>
		public void ReleaseItem(DataItem item)
		{
#if DATAITEMPOOL
			//Put the item back in the queue
			lock (m_dataQueue)
			{
				m_dataQueue.Enqueue(item);
			}
			//Log.WriteMessage(string.Format("Releasing data item. item: {0}", item.ToString()), Log.eMessageType.Debug);
#endif
		}

		/// <summary>
		/// Get the current state of this data pool
		/// </summary>
		/// <returns>String containing the current state</returns>
		public new string ToString()
		{
			return string.Format("DataItem count: {0}", m_dataQueue.Count);
		}
	}
	#endregion

	#region Log

	/// <summary>
	/// Log class to output text to the console/file
	/// </summary>
	public static class Log
	{
#if DEBUG
		private static StreamWriter filewriter = null;
#endif

		/// <summary>
		/// Type of messages to be written to a log file
		/// </summary>
		public enum eMessageType
		{
			/// <summary>
			/// Red text
			/// </summary>
			Error,
			/// <summary>
			/// Grey text
			/// </summary>
			Debug,
			/// <summary>
			/// White text
			/// </summary>
			Normal
		}

		/// <summary>
		/// Shall we output to the console
		/// </summary>
		private static bool m_consoleEnabled = true;

		/// <summary>
		/// Outputs an error message in red
		/// </summary>
		/// <param name="message">message to write</param>
		/// <param name="type">Which type of message shall we send</param>
		public static void WriteMessage(string message, eMessageType type)
		{
#if DEBUG
			switch (type)
			{
				case eMessageType.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				case eMessageType.Debug:
					Console.ForegroundColor = ConsoleColor.Gray;
					break;
				case eMessageType.Normal:
					Console.ForegroundColor = ConsoleColor.White;
					break;
			}

			if (filewriter != null)
				filewriter.WriteLine(message);
#endif
			if (m_consoleEnabled)
				Console.WriteLine(message);
		}

		/// <summary>
		/// Initialize the log files
		/// </summary>
		/// <param name="filename">The filename we should create - appends the date to it</param>
		/// <param name="consoleEnabled">Whether we should output to the console</param>
		public static void Init(string filename, bool consoleEnabled)
		{
#if DEBUG
			if (filename != null && filename != "")
			{
				DateTime dt = DateTime.Now;
				string newFileName = string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}.txt", filename, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);

				if (!File.Exists(newFileName))
				{
					filewriter = new StreamWriter(newFileName);
				}
			}

			m_consoleEnabled = consoleEnabled;
#endif
		}

		/// <summary>
		/// Close the log file
		/// </summary>
		public static void Close()
		{
#if DEBUG
			if (filewriter != null)
			{
				filewriter.Flush();
				filewriter.Close();
				filewriter = null;
			}
#endif
		}
	}

	#endregion
}
