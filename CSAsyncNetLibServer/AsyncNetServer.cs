using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace CSAsyncNetLibServer
{
	public class AsyncNetServer
	{
		private TcpListener Listener;
		private Thread ConnectionListenerThread;

		public delegate void ServerLogEvent(string line);
		public event ServerLogEvent OnServerLogEvent;

		public delegate void ClientInputEvent(AsyncClientConnection client, string line);
		public event ClientInputEvent OnClientInputEvent;

		public delegate void ClientConnectEvent(AsyncClientConnection client);
		public event ClientConnectEvent OnClientConnection;

		public delegate void ClientDisconnectEvent(AsyncClientConnection client, string reason);
		public event ClientDisconnectEvent OnClientDisconnect;

		private List<AsyncClientConnection> ClientConnections = new List<AsyncClientConnection>();

		public void Log(string line)
		{
			if (OnServerLogEvent != null)
			{
				OnServerLogEvent(line);
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
				AsyncClientConnection connection = new AsyncClientConnection(this);
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
			foreach (AsyncClientConnection connection in ClientConnections)
			{
				connection.SendData(data);
			}
		}
		public void BroadcastExcludeId(int id, string data)
		{
			foreach (AsyncClientConnection connection in ClientConnections)
			{
				if (connection.ClientID == id) continue;
				connection.SendData(data);
			}
		}
		public void SendTo(int id, string data)
		{
			foreach (AsyncClientConnection connection in ClientConnections)
			{
				if (connection.ClientID == id) connection.SendData(data);
				return;
			}
		}
		internal void ProcessIncominData(string data, AsyncClientConnection connection)
		{
			Log(data);
			if (data == null)
			{
				connection.Disconnect();
			}
			else if (data.Equals("PING"))
			{
				connection.SendData("PONG");
				Log("Replying with PONG");
			}
			else
			{
				if (OnClientInputEvent != null)
				{
					OnClientInputEvent(connection, data);
				}
			}
		}
		internal void RemoveClientConnection(AsyncClientConnection connection, string reason)
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
		public Socket workSocket = null;
		// Size of receive buffer.
		public const int BufferSize = 256;
		// Receive buffer.
		public byte[] buffer = new byte[BufferSize];
		// Received data string.
		public StringBuilder sb = new StringBuilder();
	}

	public class AsyncClientConnection
	{
		private static int ClientIDCounter = 0;
		public readonly int ClientID;

		public TcpClient Client
		{
			get;
			private set;
		}
		public string Host;
		private Thread ConnectionThread;
		private NetworkStream stream;
		private AsyncNetServer Server;

		public AsyncClientConnection(AsyncNetServer server)
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
		public void SendData(string data)
		{
			byte[] command = System.Text.Encoding.ASCII.GetBytes(data + "\n");

			stream.Write(command, 0, command.Length);
		}
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
				int i = stream.ReadByte();
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
				Thread.Sleep(50);
			}
		}
	}
}
