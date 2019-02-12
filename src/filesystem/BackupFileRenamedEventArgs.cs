using System;

namespace sidesaver
{
	public class BackupFileRenamedEventArgs : EventArgs
	{
		public string OriginalName { get; set; }
		public string NewName { get; set; }

		public int OriginalHash => OriginalName.GetHashCode();
		public int NewHash => NewName.GetHashCode();
	}
}
