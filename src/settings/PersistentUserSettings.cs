using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using sidesaver.serialization;

namespace sidesaver
{
	public class PersistentUserSettings : IUserSettings, INotifyPropertyChanged
	{
		// Actual fields that back our settings and get serialized:
		[SimpleSerializeField] private int _backupCount;
		[SimpleSerializeField] private bool _runOnStartup;
		[SimpleSerializeField] private bool _runInBackground;
		[SimpleSerializeField] private bool _runInBackgroundPopupRan;
		[SimpleSerializeField] private bool _saveBackupOnRename;
		[SimpleSerializeField] private bool _useOverrideSaveLocation;
		[SimpleSerializeField] private string _overrideSaveLocation;
		[SimpleSerializeField] private BindingList<string> _watchedProgramsBindable = new BindingList<string>();

		public event PropertyChangedEventHandler PropertyChanged;

		// Don't serialize the pending changes flag
		private bool _hasPendingChanges;
		public bool HasPendingChanges
		{
			get => _hasPendingChanges;
			set => OnPropertyChanged(ref _hasPendingChanges, value);
		}

		public int BackupCount
		{
			get => _backupCount;
			set => OnPropertyChanged(ref _backupCount, value);
		}

		public bool RunOnStartup
		{
			get => _runOnStartup;
			set => OnPropertyChanged(ref _runOnStartup, value);
		}

		public bool RunInBackground
		{
			get => _runInBackground;
			set => OnPropertyChanged(ref _runInBackground, value);
		}

		public bool RunInBackgroundPopShown
		{
			get => _runInBackgroundPopupRan;
			set => _runInBackgroundPopupRan = value;
		}

		public bool SaveBackupOnRename
		{
			get => _saveBackupOnRename;
			set => OnPropertyChanged(ref _saveBackupOnRename, value);
		}

		public bool UseOverrideSaveLocation
		{
			get => _useOverrideSaveLocation;
			set => OnPropertyChanged(ref _useOverrideSaveLocation, value);
		}

		public string OverrideSaveLocationPath
		{
			get => _overrideSaveLocation;
			set => OnPropertyChanged(ref _overrideSaveLocation, value);
		}

		public IList<string> WatchedPrograms
		{
			get => _watchedProgramsBindable;
			set => WatchedProgramsBindable = new BindingList<string>(value);
		}

		public BindingList<string> WatchedProgramsBindable
		{
			get => _watchedProgramsBindable;
			set => OnPropertyChanged(ref _watchedProgramsBindable, value);
		}

		public PersistentUserSettings()
		{
			ResetToDefault();
			LoadFromDisk();
			ResetPendingChanges();
		}

		public PersistentUserSettings(IUserSettings other)
		{
			SettingsUtils.CopySettings(other, this);
			ResetPendingChanges();
		}

		public void ResetToDefault()
		{
			SettingsUtils.CopySettings(SettingsUtils.Defaults, this);
		}

		public void ResetPendingChanges()
		{
			HasPendingChanges = false;
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

		protected void OnPropertyChanged<T>(ref T targetVar, T newVal, bool useRefEquals = false, [CallerMemberName] string propertyName = null)
		{
			if (useRefEquals && ReferenceEquals(targetVar, newVal))
				return;

			if (!useRefEquals && targetVar != null && targetVar.Equals(newVal))
				return;

			targetVar = newVal;

			if (propertyName != null && propertyName != "HasPendingChanges")
				HasPendingChanges = true;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
