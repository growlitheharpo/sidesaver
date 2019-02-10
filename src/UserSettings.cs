using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;

namespace sidesaver
{
	public class UserSettings : INotifyPropertyChanged
	{
		private readonly SettingsBacking _currentSettings;

		public int BackupCount
		{
			get => _currentSettings._backupCount;
			set
			{
				_currentSettings._backupCount = value;
				OnPropertyChanged();
			}
		}

		public bool RunInBackground
		{
			get => _currentSettings._runInBackground;
			set
			{
				_currentSettings._runInBackground = value;
				OnPropertyChanged();
			} 
		}

		public bool RunInBackgroundPopShown
		{
			get => _currentSettings._runInBackgroundPopupRan;
			set => _currentSettings._runInBackgroundPopupRan = value;
		}

		public string OverrideSaveLocation
		{
			get => _currentSettings._overrideSaveLocation;
			set
			{
				_currentSettings._overrideSaveLocation = value;
				OnPropertyChanged();
			}
		}

		public bool UseOverrideSaveLocation
		{
			get => _currentSettings._useOverrideSaveLocation;
			set 
			{
				_currentSettings._useOverrideSaveLocation = value;
				OnPropertyChanged();
			}
		}

		public UserSettings()
		{
			_currentSettings = new SettingsBacking();

			if (!Load())
				MakeDefault();
		}

		private void MakeDefault()
		{
			BackupCount = 5;
			RunInBackground = true;
			RunInBackgroundPopShown = false;
			UseOverrideSaveLocation = false;
			OverrideSaveLocation = Environment.CurrentDirectory;
		}

		private bool Load()
		{
			var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sidesaver\\");
			if (!Directory.Exists(dataPath))
				return false;

			var filePath = Path.Combine(dataPath, "settings.ini");
			if (!File.Exists(filePath))
				return false;

			using (var s = File.OpenRead(filePath))
			using (var sr = new StreamReader(s))
				_currentSettings.ReadFromFile(sr);

			return true;
		}

		public void Save()
		{
			var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sidesaver\\");
			if (!Directory.Exists(dataPath))
				Directory.CreateDirectory(dataPath);

			var filePath = Path.Combine(dataPath, "settings.ini");
			using (var s = new FileStream(filePath, FileMode.Create))
			using (var sw = new StreamWriter(s))
				_currentSettings.WriteToFile(sw);
		}
		
		private class SettingsBacking
		{
			public int _backupCount;
			public bool _runInBackground;
			public bool _runInBackgroundPopupRan;
			public bool _useOverrideSaveLocation;
			public string _overrideSaveLocation;

			public void WriteToFile(StreamWriter w)
			{
				Type t = GetType();
				var mems = t.GetFields(BindingFlags.Public | BindingFlags.Instance);

				foreach (var m in mems)
				{
					string s = $"[{m.Name}] {m.GetValue(this)}";
					w.WriteLine(s);
				}
			}

			public void ReadFromFile(StreamReader r)
			{
				Type t = GetType();
				var mems = t.GetFields(BindingFlags.Public | BindingFlags.Instance);

				Regex reg = new Regex("(\\[)(\\w+)(\\])( )(.+)");

				while (!r.EndOfStream)
				{
					string line = r.ReadLine();
					if (line == null)
						continue;

					Match m = reg.Match(line);
					if (m.Length == 0)
						continue;

					var memName = m.Groups[2].Value;
					var memVal = m.Groups[5].Value;

					var member = mems.FirstOrDefault(x => x.Name == memName);
					if (member == null)
						continue;

					if (member.FieldType == typeof(int))
						member.SetValue(this, int.Parse(memVal));
					else if (member.FieldType == typeof(float))
						member.SetValue(this, float.Parse(memVal));
					else if (member.FieldType == typeof(bool))
						member.SetValue(this, bool.Parse(memVal));
					else if (member.FieldType == typeof(string))
						member.SetValue(this, memVal);
					else
						throw new TypeLoadException($"Type ({member.FieldType.Name} is not serializable by default.");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
