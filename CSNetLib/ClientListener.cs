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

		internal ClientListener(NetClient client)
		{
			NetClient = client;
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
			List<byte> buffer = new List<byte>();
			
			int i;
			while ((i = stream.ReadByte()) != 10 && i != -1)
				buffer.Add((byte)i);

			return new string(System.Text.Encoding.ASCII.GetChars(buffer.ToArray()));
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
			ListenThread.Start();
		}
		internal void Listen()
		{
			while (Client.Connected) {
				try {
					string line = ReadLine();
					NetClient.ProcessData(line);
				} catch (System.IO.IOException) {
					NetClient.Log("Connection closed.");
					NetClient.Disconnect();
				}
			}
			NetClient.Log("Listening thread shut down.");
		}
	}
}
