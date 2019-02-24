using System;
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
			set
			{
				if (_hasPendingChanges != value)
				{
					_hasPendingChanges = value;
					OnPropertyChanged();
				}
			}
		}

		private int _backupCount;
		public int BackupCount
		{
			get => _backupCount;
			set
			{
				if (_backupCount != value)
				{
					_backupCount = value;
					OnPropertyChanged();
				}
			}
		}

		private bool _runInBackground;
		public bool RunInBackground
		{
			get => _runInBackground;
			set
			{
				if (_runInBackground != value)
				{
					_runInBackground = value;
					OnPropertyChanged();
				}
			} 
		}

		public bool RunInBackgroundPopShown { get; set; }

		private string _overrideSaveLocationPath;
		public string OverrideSaveLocationPath
		{
			get => _overrideSaveLocationPath;
			set
			{
				if (_overrideSaveLocationPath != value)
				{
					_overrideSaveLocationPath = value;
					OnPropertyChanged();
				}
			}
		}

		private bool _useOverrideSaveLocation;
		public bool UseOverrideSaveLocation
		{
			get => _useOverrideSaveLocation;
			set 
			{
				if (_useOverrideSaveLocation != value)
				{
					_useOverrideSaveLocation = value;
					OnPropertyChanged();
				}
			}
		}

		public NotifyChangedUserSettings(IUserSettings other = null)
		{
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
			ApplySettings(SettingsUtils.GetDefaultSettings());
		}

		public void ResetPendingChanges()
		{
			HasPendingChanges = false;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (propertyName != null && propertyName != "HasPendingChanges")
				HasPendingChanges = true;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
