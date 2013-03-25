using System;
using System.Collections.Generic;
using System.Text;

using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace CSNetLibServer
{
	public class NetServer
	{
		private TcpListener Listener;
		private Thread ConnectionListenerThread;
	
		public delegate void ServerLogEvent(string line);
		public event ServerLogEvent OnServerLogEvent;

		public delegate void ServerDebugLogEvent(string line);
		public event ServerDebugLogEvent OnServerDebugLogEvent;

		public delegate void ClientInputEvent(ClientConnection client, string line);
		public event ClientInputEvent OnClientInputEvent;

		public delegate void ClientConnectEvent(ClientConnection client);
		public event ClientConnectEvent OnClientConnection;

		public delegate void ClientDisconnectEvent(ClientConnection client, string reason);
		public event ClientDisconnectEvent OnClientDisconnect;

		private List<ClientConnection> ClientConnections = new List<ClientConnection>();

		public void Log(string line)
		{
			if (OnServerLogEvent != null)
			{
				OnServerLogEvent(line);
			}
		}
		public void LogDebug(string line)
		{
			if (OnServerDebugLogEvent != null) {
				OnServerDebugLogEvent(line);
			}
		}
		public void StartListen(string host, int port)
		{
			IPAddress address = IPAddress.Parse(host);
			Log("Starting listener at " + address.ToString() + ":" + port); 

			Listener = new TcpListener(address, port);
			Listener.Start();
			Log("Listener started."); 
			ConnectionListenerThread = new Thread(new ThreadStart(WaitForIncomingConnections));
			ConnectionListenerThread.Start();
			Log("Thread started.");
		}
		private void WaitForIncomingConnections()
		{
			Log("Listening for new connections.");
			while (true)
			{
				ClientConnection connection = new ClientConnection(this);
				connection.Connect(Listener.AcceptTcpClient());
				Log("Client connected at " + connection.Host);
				ClientConnections.Add(connection);
				if (OnClientConnection != null)
				{
					OnClientConnection(connection);
				}
			}
		}
		public void Broadcast(string data)
		{
			LogDebug("ALL -> " + data);
			foreach(ClientConnection connection in ClientConnections)
			{
				connection.SendData(data);
			}
		}
		public void BroadcastExcludeId(int id, string data)
		{
			LogDebug("!" + id + " -> " + data);
			foreach (ClientConnection connection in ClientConnections)
			{
				if (connection.ClientID == id)
				{
					continue;
				}
				else
				{
					connection.SendData(data);
				}
			}
		}
		public void SendTo(int id, string data)
		{
			LogDebug(id + " -> " + data);
			foreach (ClientConnection connection in ClientConnections)
			{
				if (connection.ClientID == id)
				{
					connection.SendData(data);
					return;
				}
			} 
		}
		internal void ProcessIncominData(string data, ClientConnection connection)
		{
			if (data == null)
			{
				connection.Disconnect();
			}
			else
			{
				if (OnClientInputEvent != null)
				{
					OnClientInputEvent(connection, data);
				}
			}
		}
		internal void RemoveClientConnection(ClientConnection connection, string reason)
		{
			if (!connection.Client.Connected)
			{
				ClientConnections.Remove(connection);
				if (OnClientDisconnect != null)
				{
					OnClientDisconnect(connection, reason);
				}
				
				Log("Client disconnected");
			}
		}
	}

	public class StateObject
	{
		// Client socket.
		public NetworkStream workStream = null;
		// Size of receive buffer.
		public const int BufferSize = 256;
		// Receive buffer.
		public byte[] buffer = new byte[BufferSize];
		// Received data string.
		public StringBuilder sb = new StringBuilder();
	}

	public class ClientConnection
	{
		private static int ClientIDCounter = 0;
		public readonly int ClientID;

		/*private string response
		{
			get
			{
				return response;
			}
			set
			{
				if (value.Contains('\n'))
				{
					string[] commands = value.Split(new char[] { '\n' });
					availableCommands.AddRange(commands.OfType<string>().ToList());
					if (!value.EndsWith("\n"))
					{
						response = value;
						availableCommands.RemoveAt(availableCommands.Count - 1);
					}
				}
			}
		}*/
		private List<string> availableCommands = new List<string>();
		//private ManualResetEvent receiveDone;

		public TcpClient Client
		{
			get;
			private set;
		}
		public string Host;
		private Thread ConnectionThread;
		private NetworkStream stream;
		private NetServer Server;

		public ClientConnection(NetServer server)
		{
			Server = server;
			ClientID = ClientIDCounter;
			ClientIDCounter++;
		}
		public void Connect(TcpClient client)
		{
			Client = client;
			Host = client.Client.RemoteEndPoint.ToString();
			ConnectionThread = new Thread(new ThreadStart(Listen));
			ConnectionThread.Start();
			stream = client.GetStream();
		}
		public bool SendData(string data)
		{
			{
				byte[] command = System.Text.Encoding.ASCII.GetBytes(data + "\n");
				try {
					stream.Write(command, 0, command.Length);
					return true;
				} catch (IOException) {
					return false;
				}
			}
		}

		/*public void Receive()
		{
			try
			{
				// Create the state object.
				StateObject state = new StateObject();
				state.workStream = stream;

				// Begin receiving the data from the remote device.
				stream.BeginRead(state.buffer, 0, StateObject.BufferSize, new AsyncCallback(ReceiveCallback), state);
			}
			catch (Exception e)
			{
				Server.Log(e.ToString());
			}
		}*/

		/*private void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				// Retrieve the state object and the client socket 
				// from the asynchronous state object.
				StateObject state = (StateObject)ar.AsyncState;
				NetworkStream client = state.workStream;
				// Read data from the remote device.
				int bytesRead = client.EndRead(ar);
				if (bytesRead > 0)
				{
					// There might be more data, so store the data received so far.
					state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
					//  Get the rest of the data.
					client.BeginRead(state.buffer, 0, StateObject.BufferSize, new AsyncCallback(ReceiveCallback), state);
				}
				else
				{
					// All the data has arrived; put it in response.
					if (state.sb.Length > 1)
					{
						response += state.sb.ToString();
					}
					// Signal that all bytes have been received.
					//receiveDone.Set();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}*/

		public void Disconnect()
		{
			Client.Close();
			Server.RemoveClientConnection(this, null);
		}
		public void Disconnect(string reason)
		{
			Client.Close();
			Server.RemoveClientConnection(this, reason);
		}
		internal string ReadLine()
		{
			List<byte> buffer = new List<byte>();
			bool endReached = false;
			while (!endReached)
			{
				int i;
				try
				{
					i = stream.ReadByte();
				}
				catch (IOException)
				{
					return "";
				}
				if (i != -1 && i != 10)
				{
					byte b = (byte)i;
					buffer.Add(b);
				}
				else if (i == 10)
				{
					endReached = true;
				}
				else
				{
					endReached = true;
				}
			}
			char[] chars = System.Text.Encoding.ASCII.GetChars(buffer.ToArray());
			return new string(chars);
		}
		private void Listen()
		{
			while (Client.Connected)
			{
				Server.ProcessIncominData(ReadLine(), this);
			}
		}
	}
}
