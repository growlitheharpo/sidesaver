using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace sidesaver
{
	public class NotifyChangedUserSettings : INotifyPropertyChanged, IUserSettings
	{
		private bool _hasPendingChanges;
		public bool HasPendingChanges
		{
			get => _hasPendingChanges;
			set => OnPropertyChanged(ref _hasPendingChanges, value);
		}

		private int _backupCount;
		public int BackupCount
		{
			get => _backupCount;
			set => OnPropertyChanged(ref _backupCount, value);
		}

		private bool _runOnStartup;
		public bool RunOnStartup
		{
			get => _runOnStartup;
			set => OnPropertyChanged(ref _runOnStartup, value);
		}

		private bool _runInBackground;
		public bool RunInBackground
		{
			get => _runInBackground;
			set => OnPropertyChanged(ref _runInBackground, value);
		}

		public bool RunInBackgroundPopShown { get; set; }

		private string _overrideSaveLocationPath;
		public string OverrideSaveLocationPath
		{
			get => _overrideSaveLocationPath;
			set => OnPropertyChanged(ref _overrideSaveLocationPath, value);
		}

		private bool _saveBackupOnRename;
		public bool SaveBackupOnRename
		{
			get => _saveBackupOnRename;
			set => OnPropertyChanged(ref _saveBackupOnRename, value);
		}

		private bool _useOverrideSaveLocation;
		public bool UseOverrideSaveLocation
		{
			get => _useOverrideSaveLocation;
			set => OnPropertyChanged(ref _useOverrideSaveLocation, value);
		}

		private BindingList<string> _watchedProgramsBindable;
		public BindingList<string> WatchedProgramsBindable
		{
			get => _watchedProgramsBindable;
			set => OnPropertyChanged(ref _watchedProgramsBindable, value);
		}
		public IList<string> WatchedPrograms
		{
			get => _watchedProgramsBindable;
			set => OnPropertyChanged(ref _watchedProgramsBindable, new BindingList<string>(value));
		}

		public NotifyChangedUserSettings(IUserSettings other = null)
		{
			_watchedProgramsBindable = new BindingList<string>();
			_watchedProgramsBindable.ListChanged += (sender, args) => HasPendingChanges = true;

			if (other != null)
				ApplySettings(other);
			else
				ResetToDefault();

			HasPendingChanges = false;
		}

		public void ApplySettings(IUserSettings other)
		{
			SettingsUtils.CopySettings(other, this);
		}

		public void ResetToDefault()
		{
			ApplySettings(SettingsUtils.Defaults);
		}

		public void ResetPendingChanges()
		{
			HasPendingChanges = false;
		}

		public event PropertyChangedEventHandler PropertyChanged;

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
