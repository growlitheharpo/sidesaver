using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace sidesaver
{
	public class UserSettings
	{
		private SettingsBacking _currentSettings;

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
			//using (var s = File.OpenWrite(filePath))
			using (var s = new FileStream(filePath, FileMode.Create))
			using (var sw = new StreamWriter(s))
				_currentSettings.WriteToFile(sw);
		}
		
		private class SettingsBacking
		{
			public int _backupCount;
			public bool _runInBackground;
			public bool _runInBackgroundPopupRan;

			public void WriteToFile(StreamWriter w)
			{
				Type t = this.GetType();
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