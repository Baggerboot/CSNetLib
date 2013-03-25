using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using CSNetLib;
using System.Threading;

namespace ChatClient
{

	class Program
	{
		private NetClient NetClient;
		private ChatClient Window;

		private Thread TWindow;

		public Program()
		{
			TWindow = new Thread(new ThreadStart(CreateWindow));
			TWindow.Start();
			Thread.Sleep(1000);
			Window.WindowClose += OnClientWindowClose;

			NetClient = new NetClient();
			NetClient.OnLogEvent += OnClientLog;
			NetClient.OnNetworkDataAvailabe += OnServerSendLine;
			string input = Microsoft.VisualBasic.Interaction.InputBox("Please enter the IP address or URL of the server you want to connect to.", "Enter Server IP", "");
			NetClient.Connect(input, 9001);
			Window.SendLine += OnClientSendLine;

		}
		private void OnClientWindowClose(FormClosingEventArgs e)
		{
			NetClient.Disconnect();
		}

		private void OnClientLog(string line)
		{
			Window.WriteLine("[INFO] " + line);
		}

		private void OnClientSendLine(string line)
		{
			if (line.ToUpper().Equals("/DISCONNECT"))
			{
				NetClient.SendData(line);
				NetClient.Disconnect();
			}
			else
			{
				if(NetClient.Connected) NetClient.SendData(line);
			}
			
		}
		private void OnServerSendLine(string line)
		{
			if (line == null)
			{
				NetClient.Disconnect();
				Window.RequestClose();
			}
			Window.WriteLine(line);
		}

		private void CreateWindow()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(Window = new ChatClient());
		}


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			new Program();
		}
	}
}
