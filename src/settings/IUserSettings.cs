namespace sidesaver
{
	public interface IUserSettings
	{
		int BackupCount { get; set; }
		
		bool RunInBackground { get; set; }
		bool RunInBackgroundPopShown { get; set; }

		bool UseOverrideSaveLocation { get; set; }
		string OverrideSaveLocationPath { get; set; }
	}
}