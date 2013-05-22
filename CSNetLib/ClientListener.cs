using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Net.Sockets;

namespace CSNetLib
{
	internal class ClientListener
	{
		internal bool Connected { get { return Client.Connected; } }

		private TcpClient Client;
		private NetworkStream stream;
		private Thread ListenThread;
		private NetClient NetClient;
		private List<byte> buffer = new List<byte>();

		internal ClientListener(NetClient client)
		{
			NetClient = client;
		}

		internal SocketInformation DuplicateAndClose(int targetProcessId)
		{
			ListenThread.Abort();
			return Client.Client.DuplicateAndClose(targetProcessId);
		}

		internal Socket GetHandle()
		{
			return Client.Client;
		}

		internal bool SendData(string data)
		{
			byte[] command = System.Text.Encoding.ASCII.GetBytes(data + "\n");
			try {
				stream.Write(command, 0, command.Length);
			} catch (Exception) {
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
			try {
				buffer.Clear();
				int i;
				while ((i = stream.ReadByte()) != 10 && i != -1)
					buffer.Add((byte)i);

				if (buffer.Count == 0) {
					return null;
				}

				return new string(System.Text.Encoding.ASCII.GetChars(buffer.ToArray()));
			} catch (ThreadAbortException) {
				Console.WriteLine("Shutting down");
				return null;
			}
		}

		internal void Connect(string host, int port)
		{
			Client = new TcpClient();
			try {
				Client.Connect(host, port);
			} catch (SocketException e) {
				throw e;
			}
			stream = Client.GetStream();
			NetClient.Log("Connection established.");
			ListenThread = new Thread(new ThreadStart(Listen));
			ListenThread.Name = "CSNetLibClient Network Listener";
			ListenThread.Start();
		}

		internal void ConnectFromSocket(SocketInformation si)
		{
			Socket s = new Socket(si);
			ConnectFromSocket(s);
		}

		internal void Listen()
		{
			while (Client.Connected) {
				try {
					string line = ReadLine();
					if (line == null) {
						NetClient.Disconnect();
						return;
					}
					NetClient.ProcessData(line);
				} catch (System.IO.IOException) {
					NetClient.Log("Connection closed.");
					NetClient.Disconnect();
				}
			}
			NetClient.Log("Listening thread shut down.");
		}

		internal void ConnectFromSocket(Socket s)
		{
			Client = new TcpClient();
			Client.Client = s;
			stream = Client.GetStream();
			NetClient.Log("Reconnected to existing socket.");
			ListenThread = new Thread(new ThreadStart(Listen));
			ListenThread.Name = "CSNetLibClient Network Listener";
			ListenThread.Start();
		}
	}
}
