﻿using System;
using System.IO;
using System.Text;

namespace sidesaver
{
	class FileBackupHandler : IDisposable
	{
		private const int BACKUP_COUNT = 5;

		private int _currentBackupCount;
		private readonly string _watchedFile;
		private readonly FileSystemWatcher _fileWatcher;

		public int FileHash => _watchedFile.GetHashCode();

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
			
			// TODO: Look at files in-folder to determine starting backup count
			_currentBackupCount = 0;
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

			PrepForBackup();
			_currentBackupCount++;

			string newFile = BuildBackupStringForFile(1);

			File.Copy(_watchedFile, newFile, true);
		}

		private void PrepForBackup()
		{
			// Deletes extra backups if there are any, and renames each of the existing ones by
			// shifting it forward one number.
			int maxBackups = SideSaver.instance.BackupCount;
			maxBackups = maxBackups <= 0 ? int.MaxValue : maxBackups;

			for (int i = _currentBackupCount; i > 0; --i)
			{
				var oldVal = i;
				var newVal = i + 1;

				var name = BuildBackupStringForFile(oldVal);
				if (newVal > maxBackups)
				{
					File.Delete(name);
					--_currentBackupCount;
				}
				else
				{
					FileInfo fi = new FileInfo(name);
					fi.MoveTo(BuildBackupStringForFile(newVal));
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
