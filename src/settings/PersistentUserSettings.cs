using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using sidesaver.serialization;

namespace sidesaver
{
	class PersistentUserSettings : IUserSettings
	{
		// Actual fields that back our settings and get serialized:
		private int _backupCount;
		private bool _runOnStartup;
		private bool _runInBackground;
		private bool _runInBackgroundPopupRan;
		private bool _saveBackupOnRename;
		private bool _useOverrideSaveLocation;
		private string _overrideSaveLocation;
		private List<string> _watchedProgramList = new List<string>();

		public int BackupCount
		{
			get => _backupCount;
			set => _backupCount = value;
		}

		public IList<string> WatchedPrograms
		{
			get => _watchedProgramList;
			set => _watchedProgramList = value.ToList();
		}

		public bool RunOnStartup
		{
			get => _runOnStartup;
			set => _runOnStartup = value;
		}

		public bool RunInBackground
		{
			get => _runInBackground;
			set => _runInBackground = value;
		}

		public bool RunInBackgroundPopShown
		{
			get => _runInBackgroundPopupRan;
			set => _runInBackgroundPopupRan = value;
		}

		public bool SaveBackupOnRename
		{
			get => _saveBackupOnRename;
			set => _saveBackupOnRename = value;
		}

		public bool UseOverrideSaveLocation
		{
			get => _useOverrideSaveLocation;
			set => _useOverrideSaveLocation = value;
		}

		public string OverrideSaveLocationPath
		{
			get => _overrideSaveLocation;
			set => _overrideSaveLocation = value;
		}

		public void ResetToDefault()
		{
			ApplySettings(SettingsUtils.Defaults);
		}

		public void ApplySettings(IUserSettings other)
		{
			SettingsUtils.CopySettings(other, this);
		}

		public PersistentUserSettings()
		{
			ResetToDefault();
			LoadFromDisk();
		}

		public bool LoadFromDisk()
		{
			var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sidesaver\\");
			if (!Directory.Exists(dataPath))
				return false;

			var filePath = Path.Combine(dataPath, "settings.ini");
			if (!File.Exists(filePath))
				return false;

			using (var s = File.OpenRead(filePath))
			using (var sr = new StreamReader(s))
				SimpleSerializer.ReadFromFile(sr, this, BindingFlags.Instance | BindingFlags.NonPublic);

			return true;
		}

		public void SaveToDisk()
		{
			var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sidesaver\\");
			if (!Directory.Exists(dataPath))
				Directory.CreateDirectory(dataPath);

			var filePath = Path.Combine(dataPath, "settings.ini");
			using (var s = new FileStream(filePath, FileMode.Create))
			using (var sw = new StreamWriter(s))
				SimpleSerializer.WriteToFile(sw, this, BindingFlags.Instance | BindingFlags.NonPublic);
		}
	}
}
