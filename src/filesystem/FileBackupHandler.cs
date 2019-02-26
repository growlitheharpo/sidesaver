using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace sidesaver
{
	class FileBackupHandler : IDisposable
	{
		public delegate void BackupFileRenamedHandler(object sender, BackupFileRenamedEventArgs e);
		public int FileHash => _watchedFile.GetHashCode();
		public string FilePath => _watchedFile;

		private Regex _fileRegex;
		private string _watchedFile;
		private FileSystemWatcher _fileWatcher;

		private class BackupData
		{
			public string filePath;
			public int fileNumber;
		}

		public event BackupFileRenamedHandler FileRenamed;

		public FileBackupHandler(string file)
		{
			InitializeForFile(file);
			OnFileCreated(this, new FileSystemEventArgs(WatcherChangeTypes.Created, _fileWatcher.Path, _fileWatcher.Filter));
		}

		private void TeardownFileWatcher()
		{
			if (_fileWatcher != null)
			{
				_fileWatcher.Created -= OnFileCreated;
				_fileWatcher.Changed -= OnFileChanged;
				_fileWatcher.Renamed -= OnFileRenamed;
				_fileWatcher.Dispose();
			}
		}

		private void InitializeForFile(string file)
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

			Console.WriteLine(@"Now watching {0}", file);

			_fileRegex = new Regex($"(?:{Path.GetFileNameWithoutExtension(_watchedFile)}.backup)(\\d+)(?:{Path.GetExtension(_watchedFile)})");
		}

		public void Dispose()
		{
			TeardownFileWatcher();
		}

		private void OnFileCreated(object sender, FileSystemEventArgs e)
		{
			Console.WriteLine(@"CREATED EVENT: {0}", FilePath);
			CopyAndSaveFile(e.FullPath);
		}

		private void OnFileChanged(object sender, FileSystemEventArgs e)
		{
			Console.WriteLine(@"CHANGED EVENT: {0}", FilePath);
			CopyAndSaveFile(e.FullPath);
		}

		private void OnFileRenamed(object sender, RenamedEventArgs e)
		{
			Console.WriteLine(@"RENAMED EVENT: {0}", FilePath);
			var eventArgs = new BackupFileRenamedEventArgs() {NewName = e.FullPath, OriginalName = e.OldFullPath};
			FileRenamed?.Invoke(this, eventArgs);

			Regex oldFileRegex = _fileRegex;

			TeardownFileWatcher();
			InitializeForFile(e.FullPath);

			var oldBackups = GenerateListOfBackups(oldFileRegex);
			ReconcileExistingBackups(oldBackups, false);

			// Photoshop does some weird stuff...
			// When you change a file (possibly only large files?), we actually
			// get a renamed event. So we're going to give the user the option
			// to make a backup on rename too
			if (SideSaver.instance.Settings.SaveBackupOnRename)
				CopyAndSaveFile(e.FullPath);
		}

		private void CopyAndSaveFile(string file)
		{
			if (file != _watchedFile)
				throw new ArgumentException($"New file \"{file}\" did not match base file \"{_watchedFile}\".");

			ReconcileExistingBackups(null, true);
			string newFile = BuildBackupStringForFile(1);

			File.Copy(_watchedFile, newFile, true);
		}

		private string GetBackupDirectory()
		{
			if (SideSaver.instance.Settings.UseOverrideSaveLocation)
				return SideSaver.instance.Settings.OverrideSaveLocationPath;
			return Path.GetDirectoryName(_watchedFile);
		}

		private List<BackupData> GenerateListOfBackups(Regex pattern)
		{
			string dir = GetBackupDirectory();
			if (dir == null)
				return null;

			// Build a list of all the current backups so we can reconcile them
			var allFiles = Directory.GetFiles(dir, "*" + Path.GetExtension(_watchedFile));
			var currentBackups = new List<BackupData>(allFiles.Length);

			// Filter the list of all files in this directory using a regex to
			// match only files with ".backup" in the name
			foreach (var filePath in allFiles)
			{
				string f = Path.GetFileName(filePath);
				var m = pattern.Match(f);

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
			return currentBackups;
		}

		private void ReconcileExistingBackups(List<BackupData> currentBackups, bool addingNewItem)
		{
			if (currentBackups == null)
				currentBackups = GenerateListOfBackups(_fileRegex);
			if (currentBackups == null)
				return;

			// Get our max backup count from sidesaver.instance
			// If we're adding a new item, we subtract one from that.
			int backupSetting = SideSaver.instance.Settings.BackupCount;
			int maxBackups = backupSetting <= 0 ? int.MaxValue : backupSetting;
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

			// Rename all of our files to reconcile them with the current name of the file
			// and our current max count.
			// We loop backwards through the list so we don't collide them with each other
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

		private string BuildBackupStringForFile(int number)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(GetBackupDirectory());
			sb.Append("\\");
			sb.Append(Path.GetFileNameWithoutExtension(_watchedFile));
			sb.Append(".backup");

			sb.Append(number);

			sb.Append(Path.GetExtension(_watchedFile));
			return sb.ToString();
		}
	}
}
