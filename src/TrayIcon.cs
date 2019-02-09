using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
		}

		private void OnDoubleClick(object sender, EventArgs e)
		{
			_main.ShowWindow();
		}
	}
}
