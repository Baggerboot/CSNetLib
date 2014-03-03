using System;
using System.Collections.Generic;
using System.Text;

using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace CSNetLib
{
	public delegate void NetworkDataAvailableEvent(string data);
	public delegate void LogEvent(string line);
	public delegate void ConnectionLostEvent();

	public class NetClient
	{
		public event NetworkDataAvailableEvent OnNetworkDataAvailabe;
		public event LogEvent OnLogEvent;
		public event ConnectionLostEvent OnDisconnect;
		public event OnLocalPortKnownEvent OnLocalPortKnown
		{
			add
			{
				Listener.OnLocalPortKnown += value;
			}
			remove
			{
				Listener.OnLocalPortKnown -= value;
			}
		}

		public bool Connected { get { return Listener.Connected; } }
		private ClientListener Listener;

		public SocketInformation DuplicateAndClose(int targetProcessId)
		{
			return Listener.DuplicateAndClose(targetProcessId);
		}

		public Socket GetSocket()
		{
			return Listener.GetSocket();
		}

		public NetClient()
		{
			Listener = new ClientListener(this);
		}

		public void Connect(string host, int port)
		{
			try {
				Listener.Connect(host, port);
			} catch (SocketException e) {
				throw e;
			}
		}

		public void ConnectFromSocket(Socket s)
		{
			Listener.ConnectFromSocket(s);
		}

		public void ConnectFromSocket(SocketInformation si)
		{
			Listener.ConnectFromSocket(si);
		}

		public void Disconnect()
		{
			Listener.Disconnect();
			if (OnDisconnect != null) OnDisconnect();
		}
		public void DisconnectWithoutEvent()
		{
			Listener.Disconnect();
		}

		internal void ProcessData(string data)
		{
			if (OnNetworkDataAvailabe != null) OnNetworkDataAvailabe(data);
		}

		internal void Log(string line)
		{
			if (OnLogEvent != null) OnLogEvent(line);
		}

		public bool SendData(string data)
		{
			return Listener.SendData(data);
		}

		
	}
}
