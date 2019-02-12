using System;

namespace sidesaver
{
	public interface IUserSettings
	{
		int BackupCount { get; set; }
		
		bool RunInBackground { get; set; }
		bool RunInBackgroundPopShown { get; set; }

		bool UseOverrideSaveLocation { get; set; }
		string OverrideSaveLocationPath { get; set; }

		void ResetToDefault();
		void ApplySettings(IUserSettings other);
	}

	public static class SettingsUtils
	{
		private class UtilImpl : IUserSettings
		{
			public int BackupCount { get; set; }
			public bool RunInBackground { get; set; }
			public bool RunInBackgroundPopShown { get; set; }
			public bool UseOverrideSaveLocation { get; set; }
			public string OverrideSaveLocationPath { get; set; }
			public void ResetToDefault() {}
			public void ApplySettings(IUserSettings other) {}
		}

		public static IUserSettings GetDefaultSettings()
		{
			return new UtilImpl
			{
				BackupCount = 5,
				RunInBackground = true,
				RunInBackgroundPopShown = false,
				UseOverrideSaveLocation = false,
				OverrideSaveLocationPath = Environment.CurrentDirectory,
			};
		}
	}
}
