﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using sidesaver.Properties;

namespace sidesaver
{
	internal class TrayIcon
	{
		private readonly NotifyIcon _icon;
		private readonly SideSaver _main;

		public TrayIcon(SideSaver saverInstance)
		{
			_main = saverInstance;
			_icon = new NotifyIcon();
			
			var u = new Uri("resources/main_white.ico", UriKind.Relative);
			using (var stream = System.Windows.Application.GetResourceStream(u)?.Stream)
				if (stream != null)
					_icon.Icon = new System.Drawing.Icon(stream);

			_icon.Visible = true;
			_icon.Text = Resources.TrayIcon_idle;
			_icon.DoubleClick += OnDoubleClick;

			_icon.ContextMenu = BuildContextMenu();

			_main.Items.ListChanged += OnListChanged;
		}

		public void Kill()
		{
			_icon.Visible = false;
			_icon.Dispose();
		}

		public void PopupMessage(string msg, int timeout)
		{
			_icon.ShowBalloonTip(timeout, null, msg, ToolTipIcon.Info);
		}

		private ContextMenu BuildContextMenu()
		{
			MenuItem mu1 = new MenuItem("Show Window") {Index = 0};
			mu1.Click += OnDoubleClick;

			MenuItem mu2 = new MenuItem("Exit") {Index = 1};
			mu2.Click += OnExitClick;

			ContextMenu c = new ContextMenu();
			c.MenuItems.Add(mu1);
			c.MenuItems.Add("-");
			c.MenuItems.Add(mu2);

			return c;
		}

		private void OnDoubleClick(object sender, EventArgs e)
		{
			_main.ShowWindow();
		}

		private void OnListChanged(object sender, ListChangedEventArgs e)
		{
			if (_main.Items.Count == 0)
				_icon.Text = Resources.TrayIcon_idle;
			else if (_main.Items.Count == 1)
				_icon.Text = Resources.TrayIcon_1_file;
			else
				_icon.Text = string.Format(Resources.TrayIcon_plural_files, _main.Items.Count);
		}

		private void OnExitClick(object sender, EventArgs e)
		{
			_main.ShutdownProgram();
		}
	}
}
