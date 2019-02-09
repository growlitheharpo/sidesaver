using System;
using System.ComponentModel;
using System.Windows.Forms;
using sidesaver.Properties;

namespace sidesaver
{
	class TrayIcon
	{
		private NotifyIcon _icon;
		private SideSaver _main;

		public TrayIcon(SideSaver saverInstance)
		{
			_main = saverInstance;
			_icon = new NotifyIcon();
			
			var u = new Uri("resources/main.ico", UriKind.Relative);
			using (var stream = System.Windows.Application.GetResourceStream(u)?.Stream)
				if (stream != null)
					_icon.Icon = new System.Drawing.Icon(stream);

			_icon.Visible = true;
			_icon.DoubleClick += OnDoubleClick;
			_icon.Text = Resources.TrayIcon_idle;

			_main.Items.ListChanged += OnListChanged;
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
	}
}
