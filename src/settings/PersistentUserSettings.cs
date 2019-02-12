using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace sidesaver
{
	class PersistentUserSettings : IUserSettings
	{
		private readonly SerializedBacking _currentSettings;

		public int BackupCount
		{
			get => _currentSettings._backupCount;
			set => _currentSettings._backupCount = value;
		}

		public bool RunInBackground
		{
			get => _currentSettings._runInBackground;
			set => _currentSettings._runInBackground = value;
		}

		public bool RunInBackgroundPopShown
		{
			get => _currentSettings._runInBackgroundPopupRan;
			set => _currentSettings._runInBackgroundPopupRan = value;
		}

		public bool UseOverrideSaveLocation
		{
			get => _currentSettings._useOverrideSaveLocation;
			set => _currentSettings._useOverrideSaveLocation = value;
		}

		public string OverrideSaveLocationPath
		{
			get => _currentSettings._overrideSaveLocation;
			set => _currentSettings._overrideSaveLocation = value;
		}

		public void ResetToDefault()
		{
			ApplySettings(SettingsUtils.GetDefaultSettings());
		}

		public void ApplySettings(IUserSettings other)
		{
			BackupCount = other.BackupCount;
			RunInBackground = other.RunInBackground;
			RunInBackgroundPopShown = other.RunInBackgroundPopShown;
			UseOverrideSaveLocation = other.UseOverrideSaveLocation;
			OverrideSaveLocationPath = other.OverrideSaveLocationPath;
		}

		public PersistentUserSettings()
		{
			_currentSettings = new SerializedBacking();

			if (!LoadFromDisk())
				ResetToDefault();
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
				_currentSettings.ReadFromFile(sr);

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
				_currentSettings.WriteToFile(sw);
		}

		private class SerializedBacking
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
	}
}
