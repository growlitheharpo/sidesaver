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
		public UserSettings Settings { get; }

		private Dictionary<int, FileBackupHandler> _fileHandlers;
		private TrayIcon _icon;
		private bool _exiting;

		[STAThread]
		public static void Main()
		{
			SideSaver s = new SideSaver();
		}

		private SideSaver()
		{
			Items = new BindingList<string>();
			Settings = new UserSettings();
			instance = this;
			_exiting = false;

			Execute();
			Cleanup();
			Settings.Save();
		}

		private void Execute()
		{
			_icon = new TrayIcon(this);
			_fileHandlers = new Dictionary<int, FileBackupHandler>();

			App app = new App();
			app.InitializeComponent();
			app.Run();
		}

		public void HookMainWindow(MainWindow win)
		{
			win.Closing += OnMainWindowClose;
		}

		private void OnMainWindowClose(object sender, CancelEventArgs e)
		{
			if (!_exiting && sender is MainWindow win)
			{
				e.Cancel = true;
				win.Hide();
				_icon.PopupMessage("Sidesaver will continue running in the background...", 2);
			}
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
			_exiting = true;
			Application.Current.Shutdown(0);
		}
	}
}
