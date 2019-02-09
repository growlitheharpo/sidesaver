using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace sidesaver
{
	class FileBackupHandler : IDisposable
	{
		private readonly Regex _fileRegex;
		private readonly string _watchedFile;
		private readonly FileSystemWatcher _fileWatcher;

		public int FileHash => _watchedFile.GetHashCode();

		class BackupData
		{
			public string filePath;
			public int fileNumber;
		}

		public void Dispose()
		{
			if (_fileWatcher != null)
			{
				_fileWatcher.Created -= OnFileCreated;
				_fileWatcher.Changed -= OnFileChanged;
				_fileWatcher.Renamed -= OnFileRenamed;
				_fileWatcher.Dispose();
			}
		}

		public FileBackupHandler(string file)
		{
			_watchedFile = file;

			_fileWatcher = new FileSystemWatcher()
			{
				Path = Path.GetDirectoryName(_watchedFile),
				Filter = Path.GetFileName(_watchedFile)
			};

			_fileWatcher.Created += OnFileCreated;
			_fileWatcher.Changed += OnFileChanged;
			_fileWatcher.Renamed += OnFileRenamed;
			_fileWatcher.EnableRaisingEvents = true;

			_fileRegex = new Regex($"(?:{Path.GetFileNameWithoutExtension(_watchedFile)}.backup)(\\d+)(?:{Path.GetExtension(_watchedFile)})");
		}

		private void OnFileCreated(object sender, FileSystemEventArgs e)
		{
			CopyAndSaveFile(e.FullPath);
		}

		private void OnFileChanged(object sender, FileSystemEventArgs e)
		{
			CopyAndSaveFile(e.FullPath);
		}

		private void OnFileRenamed(object sender, RenamedEventArgs e)
		{
			// TODO: This will throw an exception. How do we handle this gracefully
			// with the current use of hashes?
			CopyAndSaveFile(e.FullPath);
		}

		private void CopyAndSaveFile(string file)
		{
			if (file != _watchedFile)
				throw new ArgumentException($"New file \"{file}\" did not match base file \"{_watchedFile}\".");

			ReconcileExistingBackups(true);
			string newFile = BuildBackupStringForFile(1);

			File.Copy(_watchedFile, newFile, true);
		}

		private void ReconcileExistingBackups(bool addingNewItem)
		{
			string dir = Path.GetDirectoryName(_watchedFile);
			if (dir == null)
				return;

			// Build a list of all the current backups so we can reconcile them
			var allFiles = Directory.GetFiles(dir, "*" + Path.GetExtension(_watchedFile));
			var currentBackups = new List<BackupData>(allFiles.Length);

			// Filter the list of all files in this directory using a regex to
			// match only files with ".backup" in the name
			foreach (var filePath in allFiles)
			{
				string f = Path.GetFileName(filePath);
				var m = _fileRegex.Match(f);

				if (m.Length == 0)
					continue;

				currentBackups.Add(new BackupData()
				{
					filePath = filePath,
					fileNumber = int.Parse(m.Groups[1].Value)
				});
			}

			// They should already be sorted by GetFiles(), but we'll just double-check.
			currentBackups.Sort((x, y) => x.fileNumber - y.fileNumber);

			// Get our max backup count from sidesaver.instance
			// If we're adding a new item, we subtract one from that.
			int maxBackups = SideSaver.instance.BackupCount <= 0 ? int.MaxValue : SideSaver.instance.BackupCount;
			if (addingNewItem)
				maxBackups -= 1;

			// Remove any backups that push us beyond the max limit
			while (currentBackups.Count > maxBackups)
			{
				var target = currentBackups.Last();
				if (File.Exists(target.filePath))
					File.Delete(target.filePath);

				currentBackups.RemoveAt(currentBackups.Count - 1);
			}

			// If we're adding a new item, we now need to loop through and rename all of our remaining files correctly
			if (addingNewItem)
			{
				// Loop backwards through the list so we don't collide them with each other
				for (int i = currentBackups.Count - 1; i >= 0; --i)
				{
					string newName = BuildBackupStringForFile(i + 2);

					// It is possible that the user has been messing around with the files
					// and now we're in a weird state where the file we want to move to
					// already exists somehow. We catch this here.
					int badIndex = currentBackups.FindIndex(x => x.filePath == newName);
					if (badIndex >= 0)
					{
						string path = currentBackups[badIndex].filePath;
						FileInfo badFile = new FileInfo(path);
						path += ".tmp";

						badFile.MoveTo(path);
						currentBackups[badIndex].filePath = path;
					}

					FileInfo fi = new FileInfo(currentBackups[i].filePath);
					fi.MoveTo(newName);
					currentBackups[i].filePath = newName;
				}
			}
		}

		private string BuildBackupStringForFile(int number)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(Path.GetDirectoryName(_watchedFile));
			sb.Append("\\");
			sb.Append(Path.GetFileNameWithoutExtension(_watchedFile));
			sb.Append(".backup");

			sb.Append(number);

			sb.Append(Path.GetExtension(_watchedFile));
			return sb.ToString();
		}
	}
}
