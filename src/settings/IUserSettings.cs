using System;

namespace sidesaver
{
	public interface IUserSettings
	{
		int BackupCount { get; set; }
		
		bool RunOnStartup { get; set; }
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
			public bool RunOnStartup { get; set; }
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
				RunOnStartup = false,
				RunInBackground = true,
				RunInBackgroundPopShown = false,
				UseOverrideSaveLocation = false,
				OverrideSaveLocationPath = Environment.CurrentDirectory,
			};
		}

		public static void CopySettings(IUserSettings src, IUserSettings dst)
		{
			dst.BackupCount = src.BackupCount;
			dst.RunOnStartup = src.RunOnStartup;
			dst.RunInBackground = src.RunInBackground;
			dst.RunInBackgroundPopShown = src.RunInBackgroundPopShown;
			dst.UseOverrideSaveLocation = src.UseOverrideSaveLocation;
			dst.OverrideSaveLocationPath = src.OverrideSaveLocationPath;
		}
	}
}
