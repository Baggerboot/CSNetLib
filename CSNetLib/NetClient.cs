using System;
using System.Collections.Generic;
using System.Text;

using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace CSNetLib
{
	public class NetClient
	{
		public delegate void NetworkDataAvailableEvent(string data);
		public event NetworkDataAvailableEvent OnNetworkDataAvailabe;

		public delegate void LogEvent(string line);
		public event LogEvent OnLogEvent;

		public delegate void ConnectionLostEvent();
		public event ConnectionLostEvent OnDisconnect;

		public bool Connected
		{
			get
			{
				return Listener.Connected;
			}
		}

		private ClientListener Listener;
		public NetClient()
		{
			Listener = new ClientListener(this);
		}
		public void Connect(string host, int port)
		{
			Listener.Connect(host, port);
		}
		public void Disconnect()
		{
			Listener.Disconnect();
			if (OnDisconnect != null) {
				OnDisconnect();
			}
		}

		internal void ProcessData(string data)
		{
			if (OnNetworkDataAvailabe != null) {
				OnNetworkDataAvailabe(data);
			}
		}

		internal void Log(string line)
		{
			if (OnLogEvent != null) {
				OnLogEvent(line);
			}
		}

		public bool SendData(string data)
		{
			return Listener.SendData(data);
		}
	}

	class ClientListener
	{
		internal bool Connected
		{
			get
			{
				return Client.Connected;
			}
		}

		private TcpClient Client;
		private NetworkStream stream;

		private Thread ListenThread;

		private NetClient NetClient;

		public ClientListener(NetClient client)
		{
			NetClient = client;
		}

		internal bool SendData(string data)
		{
			byte[] command = System.Text.Encoding.ASCII.GetBytes(data + "\n");
			try {
				stream.Write(command, 0, command.Length);
			}
			catch (Exception) {
				return false;
			}
			return true;
		}

		internal void Disconnect()
		{
			stream.Close();
			Client.Close();
		}

		internal string ReadLine()
		{
			List<byte> buffer = new List<byte>();
			bool endReached = false;
			while (!endReached) {
				int i = stream.ReadByte();
				if (i != -1 && i != 10) {
					byte b = (byte)i;
					buffer.Add(b);
				} else if (i == 10) {
					endReached = true;
				} else {
					endReached = true;
				}
			}
			char[] chars = System.Text.Encoding.ASCII.GetChars(buffer.ToArray());
			return new string(chars);
		}

		internal bool Connect(string host, int port)
		{
			Client = new TcpClient();
			Client.Connect(host, port);
			stream = Client.GetStream();

			NetClient.Log("Connection established.");
			ListenThread = new Thread(new ThreadStart(Listen));
			ListenThread.Start();
			return true;
		}
		internal void Listen()
		{
			while (Client.Connected) {
				try {
					string line = ReadLine();
					NetClient.ProcessData(line);
				}
				catch (IOException) {
					NetClient.Log("Connection closed.");
					NetClient.Disconnect();
				}
			}
			NetClient.Log("Listening thread shut down.");
		}
	}
}
