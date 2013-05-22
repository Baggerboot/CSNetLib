using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CSNetLib;
using System.Threading;

namespace ConnectionTest1
{
	class Program
	{
		private Thread InputListener;

		private NetClient Client;

		public Program()
		{
			Client = new NetClient();
			Client.OnNetworkDataAvailabe += ProcessNetworkEvent;
			Client.OnLogEvent += ProcessLogEvent;

			InputListener = new Thread(new ThreadStart(HandleInput));
			Console.WriteLine("Please enter the IP address or URL of the server you want to connect to.");
			string host = Console.ReadLine();
			Console.WriteLine("Please enter the port number of the server you want to connect to.");
			int port = int.Parse(Console.ReadLine());
			Client.Connect(host, port);
			InputListener.Start();
		}

		static void Main(string[] args)
		{
			new Program();
		}
		void ProcessNetworkEvent(string data)
		{
			Console.WriteLine(data);
		}
		void ProcessLogEvent(string data)
		{
			Console.WriteLine("[INFO] " + data);
		}
		void HandleInput()
		{
			while (true)
			{
				Client.SendData(Console.ReadLine());
			}
		}
	}
}
