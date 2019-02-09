using System.ComponentModel;
using System.Windows;

namespace sidesaver
{
	public class SideSaver
	{
		public static SideSaver instance { get; private set; }

		private TrayIcon _icon;
		
		[System.STAThread]
		public static void Main()
		{
			SideSaver s = new SideSaver();
		}

		private SideSaver()
		{
			instance = this;
			Initialize();
		}

		private void Initialize()
		{
			_icon = new TrayIcon(this);

			App app = new App();
			app.InitializeComponent();
			app.Run();
		}

		public void ShowWindow()
		{
			if (Application.Current.MainWindow == null)
				Application.Current.MainWindow = new MainWindow();

			Application.Current.MainWindow.Show();
			Application.Current.MainWindow.WindowState = WindowState.Normal;
			Application.Current.MainWindow.Focus();
		}
	}
}
