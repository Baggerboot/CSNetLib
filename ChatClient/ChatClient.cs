using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChatClient
{
	public delegate void SendLineEventHandler(string line);
	public delegate void WindowCloseEventHandler(FormClosingEventArgs e);

	public partial class ChatClient : Form
	{
		public event SendLineEventHandler SendLine;
		public event WindowCloseEventHandler WindowClose;

		delegate void SetTextCallback(string text);
		delegate void CloseRequestCallback();

		public ChatClient()
		{
			InitializeComponent();
		}

		private void OnKeyPress(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				if (SendLine != null)
				{
					SendLine(ChatBox.Text);
					ChatBox.Text = "";
				}
			}
		}
		public void WriteLine(string line)
		{
			if (!line.EndsWith("\r\n"))
			{
				line += "\r\n";
			}
			AddTextToChatView(line);
		}
		public void RequestClose()
		{
			if (this.InvokeRequired)
			{
				CloseRequestCallback d = new CloseRequestCallback(RequestClose);
				try
				{
					this.Invoke(d);
				}
				catch (InvalidOperationException)
				{

				}
				
			}
			else
			{
				this.Close();
			}
		}
		private void AddTextToChatView(string text)
		{
			if (this.ChatView.InvokeRequired)
			{
				SetTextCallback d = new SetTextCallback(AddTextToChatView);
				try
				{
					this.Invoke(d, new object[] { text });
				}
				catch (InvalidOperationException)
				{

				}
			}
			else
			{
				this.ChatView.Text += text;
				this.ChatView.ScrollToCaret();
			}
		}

		private void OnClose(object sender, FormClosingEventArgs e)
		{
			if (WindowClose != null)
			{
				WindowClose(e);
			}
		}
	}
}
