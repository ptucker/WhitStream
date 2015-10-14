using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;
using WhitStream.Data;
using WhitStream.Expression;
using WhitStream.Utility;

namespace WhitStream.QueryEngine.QueryOperators
{
	/// <summary>
	/// Holds connection specifications for the server to communicate
	/// with the source.
	/// </summary>
	public class Connection : UnaryOp
	{
		#region Constructors/Destroyers/IDs

		/// <summary>
		/// Name of the connection if known
		/// </summary>
		private String m_ConnectionName;
		/// <summary>
		/// Identification number for the connection (assigned its port #)
		/// </summary>
		private int m_connectionID;

		/// <summary>
		/// Constructor to create the list of connects and create
		/// a new thread to wait/accept new connections
		/// </summary>
		/// <param name="host">The host IPAddress</param>
		/// <param name="port">The port to listen on</param>
		/// <param name="name">The name of the connection</param>
		public Connection(IPAddress host, int port, String name)
		{
			m_cyclicBuffer = new CyclicQueue(1000000);
			//Create the write message queue for sending messages out
			m_writeQueue = new Queue<byte[]>();
			//Create the write/read threads for the operator
			m_writeThread = new Thread(WriteThread);
			//read_thread = new Thread(read);
			m_readThread = new Thread(Read);

			//Allow a name to more easily recognize a connection
			m_ConnectionName = name;
			m_connectionID = port;

			m_readLock = new object();

			//Register a new TcpListener
			m_listener = new TcpListener(host, port);
			m_listener.Start();

			m_localSocket = m_listener.AcceptSocket();

			//Create a networkstream to read/write to the socket
			m_netstream = new NetworkStream(m_localSocket);
			m_netstream.ReadTimeout = 10;
			m_connected = true;
			m_ConnectionStarted = false;

			Log.WriteMessage("Successfully created a connection.  Waiting for the server to start connection.", Log.eMessageType.Normal);

			//Initialize the Schema
			ParseSchema();
		}

		/// <summary>
		/// Terminates the connection
		/// </summary>
		public void Terminate()
		{
			Log.WriteMessage("Connection to the client has closed.", Log.eMessageType.Normal);
			Log.WriteMessage("Closing streams and TCP connection.", Log.eMessageType.Normal);

			//The client disconnected - close everything connected to it
			/*Don't use aborts - stops the thread in an unpredictable way - using
			the connected flag to stop the thread after it has executed the last command*/
			/*if(write_thread != null && write_thread.IsAlive)
				write_thread.Abort();
			if(read_thread != null && read_thread.IsAlive)
				read_thread.Abort();*/
			//No more input = eof

			m_connected = false;
			m_netstream.Dispose();
			m_listener.Stop();
			m_localSocket.Close();
		}

		/// <summary>
		/// Serialize the Generator operator by writing its row count and punctuation style
		/// </summary>
		/// <param name="tw"> The destination for writing </param>
		public override void SerializeOp(TextWriter tw)
		{
			base.SerializeOp(tw);
		}

		/// <summary>
		/// Overide for tostring for the connection class
		/// </summary>
		public override string ToString()
		{
			return string.Format("Name: {0} ID: {1} Port: {1}", m_ConnectionName, m_connectionID);
		}
		#endregion

		#region Get/Set
		/// <summary>
		/// Get the network stream
		/// </summary>
		/// <returns>The netstream for the connection</returns>
		/// <remarks>Only to be used by the check connection thread</remarks>
		public NetworkStream NetStream
		{
			get { return m_netstream; }
		}

		/// <summary>
		/// Get the local socket
		/// </summary>
		/// <returns>The socket for the connection</returns>
		/// <remarks>Only to be used by the check connection thread</remarks>
		public Socket LocalSocket
		{
			get { return m_localSocket; }
		}

		/// <summary>
		/// Get the name of the connection
		/// </summary>
		public String Name
		{
			get { return m_ConnectionName; }
			set { m_ConnectionName = value; }
		}

		/// <summary>
		/// Gets the Schema for the Connection
		/// </summary>
		public Schema Schema
		{
			get { return m_packetSchema; }
		}

		/// <summary>
		/// Get the ID of the connection
		/// </summary>
		public int ID
		{
			get { return m_connectionID; }
			set { m_connectionID = value; }
		}
		#endregion

		#region Starters/Stopers

		/// <summary>
		/// Whether the connection has been started by the server
		/// </summary>
		public bool m_ConnectionStarted;

		/// <summary>
		/// Whether we have told the sources what punctuations we need
		/// </summary>
		private bool m_sendPunctuations = false;

		/// <summary>
		/// Starts the write/read threads so that connection is sending data to
		/// the query engine
		/// </summary>
		/// <remarks>Returns a non-zero if an error occured</remarks>
		public int StartThreads()
		{
			try
			{
				m_readThread.Start();
				//Log.WriteMessage("Starting threads for client: {0}", id_num);
				m_writeThread.Start();

				return 0;
			}
			catch (Exception e)
			{
				Log.WriteMessage(e.Message, Log.eMessageType.Error);
				return -1;
			}
		}

		/// <summary>
		/// Sends a message to the client to start sending data
		/// </summary>
		/// <remarks>Returns a non-zero if an error occured</remarks>
		private void StartRead()
		{
			//Send out the start signal
			m_ConnectionStarted = true;
			byte[] signal = new byte[4];

			//Give a 0 len - start/stop signal
			m_netstream.Write(signal, 0, 4);
			//Start signal - also signals punctuation #1
			signal[0] = 1;
			m_netstream.Write(signal, 0, 1);
			//Literal pattern
			signal[0] = 2;
			m_netstream.Write(signal, 0, 1);
			//Type = int
			signal[0] = 4;
			m_netstream.Write(signal, 0, 1);
			//Factor = 60
			signal = Util.IntToByte(60);
			m_netstream.Write(signal, 0, 4);
			//Wildcard pattern
			signal[0] = 1;
			for (int i = 1; i < m_packetSchema.Count; i++)
			{
				m_netstream.Write(signal, 0, 1);
			}
			//Punctuation is done sending
			signal[0] = 0;
			m_netstream.Write(signal, 0, 1);
		}

		/// <summary>
		/// Let this operator know that it has been activated
		/// </summary>
		public override void Activate()
		{
			if (!m_ConnectionStarted)
			{
				Log.WriteMessage(string.Format("Activating client: {0}:{1}", m_ConnectionName, m_connectionID), Log.eMessageType.Normal);
				//Send out the start signal
				m_ConnectionStarted = true;
				byte[] signal = new byte[4];

				//Give a 0 len - start/stop signal
				m_netstream.Write(signal, 0, 4);
				if (!m_sendPunctuations)
				{
					//Start signal - also signals sec punctuation
					//Create punctuation schema
					Punctuation temp_punc = new Punctuation(m_packetSchema.Count);
					temp_punc.AddValue(new Punctuation.LiteralPattern((uint)60));
					//temp_punc.AddValue(new Punctuation.RangePattern((uint)0, (uint)60));
					for (int i = 0; i < m_packetSchema.Count - 1; i++)
					{
						temp_punc.AddValue(new Punctuation.WildcardPattern());
					}

					byte[] temp_b_punc = temp_punc.ToByte(m_packetSchema, 2);
					m_netstream.Write(temp_b_punc, 0, temp_b_punc.Length);
					m_sendPunctuations = true;
				}
				else
				{
					//Start signal - needs a number that doesn't have a punctuation attached to it
					signal[0] = 1;
					m_netstream.Write(signal, 0, 1);
				}
			}
		}

		/// <summary>
		/// Let this operator know that it has been deactivated
		/// </summary>
		public override void Deactivate()
		{
			base.Deactivate();
			if (m_ConnectionStarted)
			{
				Log.WriteMessage(string.Format("Deactivating client: {0}:{1}", m_ConnectionName, m_connectionID), Log.eMessageType.Debug);
				//Send out the stop signal
				m_ConnectionStarted = false;
				byte[] signal = new byte[4];

				//Give a 0 len - start/stop signal
				m_netstream.Write(signal, 0, 4);
				//Stop signal
				signal[0] = 0;
				m_netstream.Write(signal, 0, 1);
			}
		}
		#endregion

		#region Handle Punctuations
		/// <summary>
		/// Handles incoming punctuations from a byte[]
		/// </summary>
		/// <param name="data">The data of the punctuation</param>
		/// <returns>The punctuation to add to the buffer</returns>
		private Punctuation HandlePunctuation_Byte(CyclicQueue data)
		{
			int TS_OFFSET = 1, read = 0, type_punc, num_param;

			Punctuation pi = new Punctuation(m_packetSchema.attributes.Count);

			//Add a pattern to every spot in the punctuation
			for (int i = 0; i < m_packetSchema.attributes.Count; i++)
			{
				//Get the Type of the punctuation (list, literal, etc)
				type_punc = data[TS_OFFSET + read];
				read++;
				//Wildcard pattern - only need to add it
				if (type_punc == 1)
				{
					pi.AddValue(new Punctuation.WildcardPattern());
				}
				#region Literal
				//Literal pattern - need to get the param
				else if (type_punc == 2)
				{
					//Byte
					if (m_packetSchema.attributes[i].Type == Util.TypeByte)
					{
						pi.AddValue(new Punctuation.LiteralPattern(data[TS_OFFSET + read]));
						read++;
					}
					//Short
					else if (m_packetSchema.attributes[i].Type == Util.TypeUShort)
					{
						pi.AddValue(new Punctuation.LiteralPattern(((ushort)data[TS_OFFSET + read + 1] << 8) | ((ushort)data[TS_OFFSET + read])));
						read += 2;
					}
					//Int
					else if (m_packetSchema.attributes[i].Type == Util.TypeUInt)
					{
						pi.AddValue(new Punctuation.LiteralPattern(Util.ByteToUInt(data, TS_OFFSET + read, true)));
						read += 4;
					}
					else
					{
						Log.WriteMessage("Error in evaluating punctuation.", Log.eMessageType.Error);
					}
				}
				#endregion
				#region Range
				//Range pattern - need to get both param
				else if (type_punc == 3)
				{
					if (m_packetSchema.attributes[i].Type == Util.TypeByte)
					{
						pi.AddValue(new Punctuation.RangePattern(
							//Min
							data[TS_OFFSET + read],
							//Max
							data[TS_OFFSET + read + 1]));
						read += 2;
					}
					else if (m_packetSchema.attributes[i].Type == Util.TypeUShort)
					{
						pi.AddValue(new Punctuation.RangePattern(
							//Min
							((ushort)data[TS_OFFSET + read + 1] << 8) | ((ushort)data[TS_OFFSET + read]),
							//Max
							((ushort)data[TS_OFFSET + read + 3] << 8) | ((ushort)data[TS_OFFSET + read + 2])));
						read += 4;
					}
					else if (m_packetSchema.attributes[i].Type == Util.TypeUInt)
					{
						pi.AddValue(new Punctuation.RangePattern(
							//Min
							Util.ByteToUInt(data, TS_OFFSET + read, true),
							//Max
							Util.ByteToUInt(data, TS_OFFSET + read, true)));
						read += 8;
					}
					else
					{
						Log.WriteMessage("Error in evaluating punctuation.", Log.eMessageType.Error);
					}
				}
				#endregion
				#region List
				else if (type_punc == 4)
				{
					//Get the number of params in the list
					num_param = data[TS_OFFSET + read];
					read++;
					Punctuation.LiteralPattern[] param = new Punctuation.LiteralPattern[num_param];
					if (m_packetSchema.attributes[i].Type == Util.TypeByte)
					{
						//byte[] b_param = new byte[num_param];

						for (int j = 0; j < num_param; j++)
						{
							//b_param[j] = data[TS_OFFSET + read];
							param[j] = new Punctuation.LiteralPattern(data[TS_OFFSET + read]);
							read++;
						}
						pi.AddValue(new Punctuation.ListPattern(param));
					}
					else if (m_packetSchema.attributes[i].Type == Util.TypeByte)
					{
						//ushort[] s_param = new ushort[num_param];
						for (int j = 0; j < num_param; j++)
						{
							param[j] = new Punctuation.LiteralPattern((ushort)((int)(data[TS_OFFSET + read + 1] << 8) | (data[TS_OFFSET + read])));
							read += 2;
						}
						pi.AddValue(new Punctuation.ListPattern(param));
					}
					else if (m_packetSchema.attributes[i].Type == Util.TypeUInt)
					{
						//uint[] i_param = new uint[num_param];
						for (int j = 0; j < num_param; j++)
						{
							param[j] = new Punctuation.LiteralPattern(Util.ByteToUInt(data, TS_OFFSET + read, true));
							read += 4;
						}
						pi.AddValue(new Punctuation.ListPattern(param));
					}
					else
					{
						Log.WriteMessage("Error in evaluating punctuation.", Log.eMessageType.Error);
					}
				}
				#endregion
				else
				{
					Log.WriteMessage("Error in evaluating punctuation.", Log.eMessageType.Error);
				}
			}
			Log.WriteMessage(string.Format("Found a punctuation!\n{0}\n", pi.ToString()), Log.eMessageType.Debug);
			return pi;
		}

		/// <summary>
		/// Handles incoming punctuations from a literal string
		/// </summary>
		/// <param name="data">The data of the punctuation</param>
		/// <returns>The punctuation to add to the buffer</returns>
		private Punctuation HandlePunctuation_String(CyclicQueue data)
		{
			//Index for where we are in the byte[] - starts at 1, 0->flag
			int index = 1;
			Punctuation pi = new Punctuation(m_packetSchema.attributes.Count);

			//If the beginning character isn't right
			if (data[1] != '<')
			{
				Log.WriteMessage("Error in parsing Punctuation: No '<' at start.", Log.eMessageType.Error);
				return null;
			}
			index++;

			for (int i = 0; i < m_packetSchema.attributes.Count; i++)
			{
				//Wildcard
				if (data[index] == '*')
				{
					pi.AddValue(new Punctuation.WildcardPattern());
					index++;
				}
				#region Literal
				else if (data[index] == 'c')
				{
					index++;
					//Byte
					if (m_packetSchema.attributes[i].Type == Util.TypeByte)
					{
						pi.AddValue(new Punctuation.LiteralPattern(data[index]));
						index++;
					}
					//Short
					else if (m_packetSchema.attributes[i].Type == Util.TypeUShort)
					{
						pi.AddValue(new Punctuation.LiteralPattern(((ushort)data[index] << 8) | ((ushort)data[index])));
						index += 2;
					}
					//Int
					else if (m_packetSchema.attributes[i].Type == Util.TypeUInt)
					{
						pi.AddValue(new Punctuation.LiteralPattern(Util.ByteToUInt(m_cyclicBuffer, index, true)));
						index += 4;
					}
					else
					{
						Log.WriteMessage("Error in evaluating punctuation: Schema is incorrect.", Log.eMessageType.Error);
						break;
					}
				}
				#endregion
				#region Range
				//Range
				else if (data[index] == '[')
				{
					index++;
					if (m_packetSchema.attributes[i].Type == Util.TypeByte)
					{
						pi.AddValue(new Punctuation.RangePattern(
							//Min
							data[index],
							//Max
							data[index]));
						index += 2;
					}
					else if (m_packetSchema.attributes[i].Type == Util.TypeUShort)
					{
						pi.AddValue(new Punctuation.RangePattern(
							//Min
							((ushort)data[index + 1] << 8) | ((ushort)data[index]),
							//Max
							((ushort)data[index + 3] << 8) | ((ushort)data[index + 2])));
						index += 4;
					}
					else if (m_packetSchema.attributes[i].Type == Util.TypeUInt)
					{
						pi.AddValue(new Punctuation.RangePattern(
							//Min
							Util.ByteToUInt(data, index, true),
							//Max
							Util.ByteToUInt(data, index + 4, true)));
						index += 8;
					}
					else
					{
						Log.WriteMessage("Error in evaluating punctuation: Schema is incorrect.", Log.eMessageType.Error);
						break;
					}

					if (data[index] != ']')
					{
						Log.WriteMessage("Error in evaluating punctuation: missing ']' at end of range.", Log.eMessageType.Error);
						break;
					}
					else
					{
						index++;
					}
				}
				#endregion
				#region List
				else if (data[index] == '{')
				{
					index++;

					//Get the number of params in the list
					int num_param = data[index++];
					Punctuation.LiteralPattern[] param = new Punctuation.LiteralPattern[num_param];

					if (m_packetSchema.attributes[i].Type == Util.TypeByte)
					{
						//byte[] b_param = new byte[num_param];
						for (int j = 0; j < num_param; j++)
						{
							//b_param[j] = data[index];
							param[j] = new Punctuation.LiteralPattern(data[index]);
							index++;
						}
						pi.AddValue(new Punctuation.ListPattern(param));
					}
					else if (m_packetSchema.attributes[i].Type == Util.TypeByte)
					{
						//ushort[] s_param = new ushort[num_param];
						for (int j = 0; j < num_param; j++)
						{
							//s_param[j] = (ushort)((int)(data[index + 1] << 8) | (data[index]));
							param[j] = new Punctuation.LiteralPattern((ushort)((int)(data[index + 1] << 8) | (data[index])));
							index += 2;
						}
						pi.AddValue(new Punctuation.ListPattern(param));
					}
					else if (m_packetSchema.attributes[i].Type == Util.TypeUInt)
					{
						//uint[] i_param = new uint[num_param];
						for (int j = 0; j < num_param; j++)
						{
							//i_param[j] = (uint)Util.convertb_to_i(data, index, true);
							param[j] = new Punctuation.LiteralPattern((uint)Util.ByteToUInt(data, index, true));
							index += 4;
						}
						pi.AddValue(new Punctuation.ListPattern(param));
					}
					else
					{
						Log.WriteMessage("Error in evaluating punctuation: Schema is incorrect.", Log.eMessageType.Error);
						break;
					}

					if (data[index] != '}')
					{
						Log.WriteMessage("Error in evaluating punctuation: missing '}' at end of list.", Log.eMessageType.Error);
						break;
					}
					else
					{
						index++;
					}
				}
				#endregion
				else
				{
					Log.WriteMessage("Error in evaluating punctuation: No matched starts.", Log.eMessageType.Error);
					break;
				}

				if (data[index] != ',')
				{
					if (data[index] != '>')
					{
						Log.WriteMessage("Error in evaluating punctuation: missing ','.", Log.eMessageType.Error);
					}
					break;
				}
				else
				{
					index++;
				}
			}

			if (data[index] != '>')
			{
				Log.WriteMessage("Error in parsing Punctuation: No '>' at end.", Log.eMessageType.Error);
				return null;
			}
			else
			{
				Log.WriteMessage(string.Format("Found Punctuation!:\n{0}\n", pi.ToString()), Log.eMessageType.Debug);
				return pi;
			}
		}

		/// <summary>
		/// Handles incoming punctuations from a predfined punctuation
		/// </summary>
		/// <param name="data">The data of the punctuation</param>
		/// <returns>The punctuation to add to the buffer</returns>
		private Punctuation HandlePunctuation_Predefined(byte[] data)
		{
			return null;
		}
		#endregion

		#region Network

		// Socket connecting the server to a particular source
		Socket m_localSocket;
		//Stream to read/write to the socket
		NetworkStream m_netstream;
		//Listener to connect with the source
		TcpListener m_listener;
		// Whether the connection is active
		bool m_connected;
		//Holds information waiting to be sent to the clients
		Queue<byte[]> m_writeQueue;
		//Read/Write threads
		Thread m_writeThread, m_readThread;
		//Cyclic queue/buffer to eliminate new calls
		CyclicQueue m_cyclicBuffer;

		/// <summary>
		/// Determines if it is safe to read
		/// </summary>
		public object m_readLock;

		/// <summary>
		/// Function for the read thread
		/// </summary>
		/// <remarks>Uses a cyclic buffer to store data.</remarks>
		private void Read()
		{
			//Buffers to hold the length of the packet and the packet
			byte[] b_len = new byte[4];

			int len, read_len, next_index;

			while (m_connected)
			{
				try
				{
					if (m_ConnectionStarted && m_netstream.DataAvailable)
					{
						lock (m_readLock)
						{
							//Make sure the socket blocks so that it receives the entire packet
							m_localSocket.Blocking = true;
							//Try to read the length of the incoming packet
							if ((read_len = m_netstream.Read(b_len, 0, 4)) != 0)
							{
								while (read_len != 4)
								{
									read_len += m_netstream.Read(b_len, read_len, 4 - read_len);
								}
								//Convert length to an int
								len = (int)Util.ByteToUInt(b_len);
								//Log.WriteMessage("Packet Len: {0}", len);

								//Find where the packet will go in the CQ
								next_index = m_cyclicBuffer.New_Index(len);
								//Log.WriteMessage("NI: {0}", next_index);

								//Read the rest of the packet
								read_len = 0;
								while (read_len != len)
								{
									read_len += m_netstream.Read(m_cyclicBuffer.Buffer, next_index + read_len, len - read_len);
								}
								m_cyclicBuffer.IncCount();
							}
							else
							{
								m_connected = false;
								break;
							}
						}
					}
					else
					{
						Thread.Sleep(1);
					}
				}
				catch (Exception e)
				{
					Log.WriteMessage(e.Message, Log.eMessageType.Error);
					if (e.InnerException is SocketException)
					{
						SocketException se = (SocketException)e.InnerException;
						if (se.ErrorCode == 10054)
							m_connected = false;
					}
				}
			}
		}

		/// <summary>
		/// Function for the write thread
		/// </summary>
		private void WriteThread()
		{
			try
			{
				while (m_connected)
				{
					if (m_writeQueue.Count > 0)
					{
						byte[] b_str;

						lock (m_writeQueue)
						{
							b_str = m_writeQueue.Dequeue();
						}

						//First: send the length of the outgoing message
						byte[] b_len = Util.IntToByte(b_str.Length);
						m_netstream.Write(b_len, 0, 4);
						//Second: send the entire message
						m_netstream.Write(b_str, 0, b_str.Length);
					}
					else
					{
						Thread.Sleep(5000);
					}
				}
			}
			catch
			{
				//Catches the thread was being aborted exception
				//Log.WriteMessage(e.Message);
			}
		}
		#endregion

		#region Schema

		/// <summary>
		/// How the "packet" looks when received
		/// </summary>
		private Schema m_packetSchema;

		/// <summary>
		/// Parses a the schema for the connection sent by the source
		/// </summary>
		private void ParseSchema()
		{
			m_packetSchema = new Schema();

			//holds the length of the incoming attribute

			byte[] b_len = new byte[4];
			int len;

			while (true)
			{
				byte[] b_position = new byte[2];

				if (m_netstream.Read(b_position, 0, 2) != 0)
				{
					//Length of name
					m_netstream.Read(b_len, 0, 4);
					len = (int)Util.ByteToUInt(b_len);
					byte[] b_name = new byte[len];

					//Read name
					m_netstream.Read(b_name, 0, len);

					if (b_position[0] == 0 && b_position[1] == 0)
					{
						break;
					}

					//Add the attribute to the schema list
					m_packetSchema.AddAttr((int)b_position[0], (int)b_position[1], Util.ByteToString(b_name, b_name.Length));
				}
			}
		}	
		#endregion

		#region Handle Data
		/// <summary>
		/// Output new data items
		/// </summary>
		/// <remarks>Uses the cyclical buffer.</remarks>
		/// <returns>The DataItem objects to output</returns>
		/// <seealso cref="Data.DataItem"/>
        public override List<DataItem> Iterate(DataItemPool.GetDataItem gdi, DataItemPool.ReleaseDataItem rdi)
		{
			ldiBufferOut.Clear();
			try
			{
				int i;
				for (i = 0; i < 50 && m_cyclicBuffer.Total_Count > 0; i++, m_cyclicBuffer++)
				{
					//A punctuation's flag is 1
					if (m_cyclicBuffer[0] == 1)
					{
						//if (data.Length < 100)
						//Punctuation pi = new Punctuation(packet_schema.attr_pos.Count);
						//Handle punctuation - try 3 ways
						//1 - predefined
						//ldiBufferOut.Add(predef_handle_punc(data));
						//2 - byte array
						//ldiBufferOut.Add(byte_handle_punc(data));
						//3 - literal string
						ldiBufferOut.Add(HandlePunctuation_String(m_cyclicBuffer));
					}
					//A packet's flag is 0
					else if (m_cyclicBuffer[0] == 0)
					{
                        DataItem di = algorithm.GetDataItem(1)[0];

						foreach (Schema.Attribute attr in m_packetSchema.attributes)
						{
							//Leave single bytes as bytes
							if (attr.Type == Util.TypeByte)
							{
								di.AddValue(m_cyclicBuffer[attr.MSB]);
							}
							//Convert the byte[2] in an short
							else if (attr.Type == Util.TypeUShort)
							{
								di.AddValue((ushort)((int)m_cyclicBuffer[attr.MSB] << 8 | m_cyclicBuffer[attr.LSB]));
							}
							//Convert the byte[4] into an int -> b[1] = b[0] - 3
							else if (attr.Type == Util.TypeUInt)
							{
								//LSB is down the array
								if (attr.LSB > attr.MSB)
								{
									di.AddValue(Util.ByteToUInt(m_cyclicBuffer, attr.LSB, false));
								}
								//LSB is up the array
								else
								{
									di.AddValue(Util.ByteToUInt(m_cyclicBuffer, attr.LSB, true));
								}
							}
							//Don't handle anything larger than an int
							else
								di.AddValue(0);
						}
						ldiBufferOut.Add(di);
					}
				}
			}
			catch(Exception e)
			{
				Log.WriteMessage(e.Message, Log.eMessageType.Error);
			}

			try
			{
				if (!m_connected)
				{
					this.eof = true;
                    DataItem end = algorithm.GetDataItem(1)[0];
					end.EOF = true;
					ldiBufferOut.Add(end);
				}
			}
			catch
			{
				Log.WriteMessage("Inside Iterate().", Log.eMessageType.Error);
			}
			return ldiBufferOut;
		}
		#endregion
	}
}
