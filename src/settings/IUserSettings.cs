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
		private class DefaultImpl : IUserSettings
		{
			public int BackupCount { get => 5; set => throw new InvalidOperationException(); }
			public bool RunOnStartup { get => false; set => throw new InvalidOperationException(); }
			public bool RunInBackground { get => true; set => throw new InvalidOperationException(); }
			public bool RunInBackgroundPopShown { get => false; set => throw new InvalidOperationException(); }
			public bool UseOverrideSaveLocation { get => false; set => throw new InvalidOperationException(); }
			public string OverrideSaveLocationPath { get => Environment.CurrentDirectory; set => throw new InvalidOperationException(); }

			public void ResetToDefault() {}
			public void ApplySettings(IUserSettings other) {}
		}

		public static IUserSettings Defaults  => new DefaultImpl();

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
