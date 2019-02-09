using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace sidesaver
{
	public class SideSaver
	{
		public static SideSaver instance { get; private set; }

		public BindingList<string> Items { get; }
		public int BackupCount { get; set; }

		private Dictionary<int, FileBackupHandler> _fileHandlers;
		private TrayIcon _icon;

		[System.STAThread]
		public static void Main()
		{
			SideSaver s = new SideSaver();
		}

		private SideSaver()
		{
			BackupCount = 5; // default is 5
			Items = new BindingList<string>();
			instance = this;
			Execute();
			Cleanup();
		}

		private void Execute()
		{
			_icon = new TrayIcon(this);
			_fileHandlers = new Dictionary<int, FileBackupHandler>();

			App app = new App();
			app.InitializeComponent();
			app.Run();
		}

		private void Cleanup()
		{
			foreach (var f in _fileHandlers)
				f.Value.Dispose();
		}

		public void ShowWindow()
		{
			var w = Application.Current.MainWindow ?? new MainWindow();

			w.Show();
			w.WindowState = WindowState.Normal;
			w.Focus();

			Application.Current.MainWindow = w;
		}

		public void AddNewFile()
		{
			var fileDialog = new Microsoft.Win32.OpenFileDialog
			{
				Multiselect = false,
				AddExtension = true,
				CheckFileExists = true,
			};
			fileDialog.Multiselect = true;

			if (fileDialog.ShowDialog(Application.Current.MainWindow) != true)
				return;

			foreach (var path in fileDialog.FileNames)
			{
				if (Items.Contains(path))
					continue;

				if (_fileHandlers.ContainsKey(path.GetHashCode()))
					continue;

				var handler = new FileBackupHandler(path);
				_fileHandlers.Add(handler.FileHash, handler);
				Items.Add(path);
			}
		}

		public void StopWatching(string filePath)
		{
			int hash = filePath.GetHashCode();

			if (!_fileHandlers.ContainsKey(hash))
				return;

			_fileHandlers[hash].Dispose();
			_fileHandlers.Remove(hash);

			Items.Remove(filePath);
		}

		public void ShutdownProgram()
		{
			Application.Current.Shutdown(0);
		}
	}
}
