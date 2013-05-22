using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CSNetLibServer;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace ServerTest1
{
	class Program
	{
		private NetServer Server;

		private Dictionary<int, string> Nicks = new Dictionary<int, string>();

		public Program(string host, int port)
		{
			Server = new NetServer();
			Server.OnServerLogEvent += OnLogEvent;
			Server.OnClientInputEvent += OnClientInput;
			Server.OnClientConnection += OnClientConnection;
			Server.OnClientDisconnect += OnClientDisconnect;
			Server.StartListen(host, port);
			Console.ReadKey();
		}
		static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				new Program(args[0], int.Parse(args[1]));
			}
			else
			{
				Console.WriteLine("Please specify the IP address at which you want the server to listen, \"auto\" to automatically detect an IP address, or leave this field blank to use the default IP address (192.168.178.26)");
				string input = Console.ReadLine();
				if (input.Equals(""))
				{
					Console.WriteLine("Using default IP address.");
					new Program("192.168.178.26", 9001);
				}
				else if (input.Equals("auto"))
				{
					string ip = LocalIPAddress();
					Console.WriteLine("Auto-detected IP address: " + ip);
					new Program(ip, 9001);
				}
				else
				{
					Console.WriteLine("Using manually configured IP address.");
					Console.WriteLine("Please enter the port you want to listen on.");
					new Program(input, int.Parse(Console.ReadLine()));
				}
			}
		}
		public static string LocalIPAddress()
		{
			IPHostEntry host;
			string localIP = "";
			host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					localIP = ip.ToString();
				}
			}
			return localIP;
		}

		private void OnLogEvent(string line)
		{
			Console.WriteLine(line);
		}
		private void OnClientInput(ClientConnection client, string line)
		{
			if (line.StartsWith("/"))
			{
				ProcessCommand(client, line.Substring(1).Split(new char[] {' '}));
			}
			else
			{
				ProcessMessage(client, line);
			}
			
		}
		private void ProcessCommand(ClientConnection client, string[] args)
		{
			if (args.Length < 1) return;
			if (args.Length >= 2 && args[0].ToUpper().Equals("NICK"))
			{
				if(args[1].Equals("") || args[1].Equals(" ") || args[1].Equals(null))
				{
					client.SendData("<Server> Invalid characters (Spaces?) in username. Allowed characters are a-z, A-Z, 0-9, _, and -");
					client.SendData("The nick you tried to set is: <" + args[1] + ">");
				}
				if (args[1].ToLower().Equals("server") || args[1].ToLower().Contains("admin"))
				{
					client.SendData("<Server> That nick is reserved.");
				}
				else if (Nicks.ContainsValue(args[1]))
				{
					client.SendData("<Server> That nick is already taken.");
				}
				else if (Regex.IsMatch(args[1], @"^[a-zA-Z0-9_\-]"))
				{
					Server.Broadcast("<Server> " + Nicks[client.ClientID] + " changed nick to " + args[1] + ".");
					Nicks[client.ClientID] = args[1];
				}
				else
				{
					client.SendData("<Server> Invalid characters in username. Allowed characters are a-z, A-Z, 0-9, _, and -");
				}
			}
			else if (args[0].ToUpper().Equals("DISCONNECT"))
			{
				if (args.Length > 1)
				{
					string quitReason = "";
					for (int i = 1; i < args.Length; i++)
					{
						quitReason += args[i] + " ";
					}
					client.Disconnect(quitReason);
				}
				else
				{
					client.Disconnect(null);
				}
			}
		}
		private void ProcessMessage(ClientConnection client, string line)
		{
			Server.Broadcast("<" + Nicks[client.ClientID] + "> " + line);
		}
		private void OnClientConnection(ClientConnection client)
		{
			Nicks.Add(client.ClientID, client.Host);
			//client.SendData("<Server> Welcome! Please choose a nickname using the /nick <nickname> command.");
		}
		private void OnClientDisconnect(ClientConnection client, string reason)
		{
			if (reason == null)
			{
				Server.Broadcast("<Server> " + Nicks[client.ClientID] + " has quit.");
			}
			else
			{
				string addition = reason.EndsWith(".") ? "" : ".";
				Server.Broadcast("<Server> " + Nicks[client.ClientID] + " has quit. Reason: " + reason + addition);
			}
			Nicks.Remove(client.ClientID);
			
		}
	}
}
