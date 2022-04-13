using System;

namespace sidesaver
{
	public class BackupFileRenamedEventArgs : EventArgs
	{
		public string OriginalName { get; set; } = string.Empty;
		public string NewName { get; set; } = string.Empty;

		public int OriginalHash => OriginalName.GetHashCode();
		public int NewHash => NewName.GetHashCode();
	}
}
