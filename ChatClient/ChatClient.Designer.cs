namespace ChatClient
{
	partial class ChatClient
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ChatView = new System.Windows.Forms.TextBox();
			this.ChatBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// ChatView
			// 
			this.ChatView.AcceptsReturn = true;
			this.ChatView.AcceptsTab = true;
			this.ChatView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ChatView.BackColor = System.Drawing.Color.White;
			this.ChatView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ChatView.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ChatView.Location = new System.Drawing.Point(-1, -1);
			this.ChatView.Multiline = true;
			this.ChatView.Name = "ChatView";
			this.ChatView.ReadOnly = true;
			this.ChatView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ChatView.Size = new System.Drawing.Size(466, 302);
			this.ChatView.TabIndex = 0;
			this.ChatView.UseWaitCursor = true;
			// 
			// ChatBox
			// 
			this.ChatBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ChatBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ChatBox.Location = new System.Drawing.Point(-1, 302);
			this.ChatBox.Name = "ChatBox";
			this.ChatBox.Size = new System.Drawing.Size(466, 22);
			this.ChatBox.TabIndex = 1;
			this.ChatBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyPress);
			// 
			// ChatClient
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 322);
			this.Controls.Add(this.ChatBox);
			this.Controls.Add(this.ChatView);
			this.Name = "ChatClient";
			this.Text = "Chat Client";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClose);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox ChatView;
		private System.Windows.Forms.TextBox ChatBox;
	}
}

