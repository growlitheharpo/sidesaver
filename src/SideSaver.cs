using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using Application = System.Windows.Application;

namespace sidesaver
{
	public class SideSaver
	{
		public static SideSaver instance { get; private set; }

		public BindingList<string> Items { get; }
		public IUserSettings Settings => _settings;

		private readonly PersistentUserSettings _settings;
		private readonly Dictionary<int, FileBackupHandler> _fileHandlers;
		private readonly TrayIcon _icon;
		private bool _exiting;

		[STAThread]
		public static void Main()
		{
			AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver.OnAssemblyResolve;
			SideSaver s = new SideSaver();
		}

		private SideSaver()
		{
			instance = this;

			Items = new BindingList<string>();

			_fileHandlers = new Dictionary<int, FileBackupHandler>();
			_settings = new PersistentUserSettings();
			_icon = new TrayIcon(this);

			// Once we've loaded our settings, update our startup setting
			ApplyStartupSetting(_settings.RunOnStartup);

			Execute();

			Cleanup();
			_settings.SaveToDisk();
		}

		private void Execute()
		{
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
			if (sender is MainWindow win && !_exiting)
			{
				if (Settings.RunInBackground)
				{
					e.Cancel = true;
					win.Hide();

					if (!Settings.RunInBackgroundPopShown)
					{
						_icon.PopupMessage("Sidesaver will continue running in the background...", 2);
						Settings.RunInBackgroundPopShown = true;
					}
				}
			}
		}

		private void Cleanup()
		{
			foreach (var f in _fileHandlers)
			{
				f.Value.FileRenamed -= OnFileRenamed;
				f.Value.Dispose();
			}
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
				handler.FileRenamed += OnFileRenamed;
				_fileHandlers.Add(handler.FileHash, handler);
				Items.Add(path);
			}
		}

		private void OnFileRenamed(object sender, BackupFileRenamedEventArgs e)
		{
			if (_fileHandlers.ContainsKey(e.OriginalHash))
			{
				if (_fileHandlers.ContainsKey(e.NewHash))
				{
					throw new InvalidOperationException(
						$"New file {e.NewName} has the same hash as an existing file: {_fileHandlers[e.NewHash].FilePath }");
				}

				_fileHandlers[e.NewHash] = _fileHandlers[e.OriginalHash];
				_fileHandlers.Remove(e.OriginalHash);
			}

			Application.Current.Dispatcher.Invoke(() =>
			{
				int index = Items.IndexOf(e.OriginalName);
				if (index >= 0)
					Items[index] = e.NewName;
			});
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

		public void CreateSettingsMenu()
		{
			NotifyChangedUserSettings tmpSettings = new NotifyChangedUserSettings(_settings);
			OptionsWindow newWin = OptionsWindow.Create(tmpSettings);
			newWin.Owner = Application.Current.MainWindow;
			newWin.Show();
			newWin.Focus();
		}

		public void CommitNewSettings(NotifyChangedUserSettings newSettings)
		{
			_settings.ApplySettings(newSettings);
			newSettings.ResetPendingChanges();

			ApplyStartupSetting(_settings.RunOnStartup);
		}

		private void ApplyStartupSetting(bool shouldRunOnStartup)
		{
			const string startupKeyName = @"Software\\Microsoft\\Windows\\CurrentVersion\\Run";
			const string startupValueName = "Sidesaver";

			var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(startupKeyName, true);
			if (key == null)
			{
				Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\\" + startupKeyName, startupValueName, "");
				key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(startupKeyName, true);

				if (key == null)
					return;
			}

			using (key)
			{
				if (shouldRunOnStartup)
				{
					string path = $"\"{Assembly.GetEntryAssembly().Location}\"";
					key.SetValue(startupValueName, path);
				}
				else
				{
					if (key.GetValue(startupValueName) != null)
						key.DeleteValue(startupValueName);
				}
			}
		}
	}
}
