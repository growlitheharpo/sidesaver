using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Linq;
using System.Threading;
using System.Runtime.Versioning;

namespace sidesaver
{
	[SupportedOSPlatform("windows")]
	internal class ProgramWatcher
	{
		private readonly TrayIcon _iconRef;

		private readonly object _processStateLock = new object();
		private readonly Dictionary<string, bool> _targetProcessStates;
		private readonly Thread _watcherThread;

		private bool _shouldShutdown;

		private class ProcessInfo
		{
			public Process? process;
			public string path = "";
		}

		public ProgramWatcher(TrayIcon icon)
		{
			_targetProcessStates = new Dictionary<string, bool>();
			_watcherThread = new Thread(ProgramWatcherThread);
			_iconRef = icon;

			_shouldShutdown = false;
			_watcherThread.Start();
		}

		public void UpdateWatchedPrograms(IList<string> programs)
		{
			string[] keys;
			lock (_processStateLock)
				keys = _targetProcessStates.Keys.ToArray();

			var toAdd = new List<string>(programs.Where(p => !keys.Contains(p)));
			var toRemove = new List<string>(keys.Where(k => !programs.Contains(k)));

			lock (_processStateLock)
			{
				foreach (var r in toRemove)
					_targetProcessStates.Remove(r);

				foreach (var a in toAdd)
					_targetProcessStates.Add(a, false);
			}
		}

		public void Kill()
		{
			_shouldShutdown = true;
			_watcherThread.Join();
		}

		private void ProgramWatcherThread()
		{
			Thread.CurrentThread.IsBackground = true;

			while (!_shouldShutdown)
			{
				List<string> allPaths = new List<string>();

				// This method for getting all processes comes from https://stackoverflow.com/a/5497319
				var wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
				using (var searcher = new ManagementObjectSearcher(wmiQueryString))
				using (var results = searcher.Get())
				{
					var query = Process.GetProcesses()
						.Join(results.Cast<ManagementObject>(), p => p.Id, mo => (int) (uint) mo["ProcessId"],
							(p, mo) => new ProcessInfo {process = p, path = (string) mo["ExecutablePath"],});
					foreach (var item in query)
					{
						if (item.path != null)
						{
							allPaths.Add(item.path);
						}
					}
				}

				allPaths.Sort();
				bool anyNewInstances = false;

				lock (_processStateLock)
				{
					var targetCopy = new Dictionary<string, bool>(_targetProcessStates);
					foreach (var watching in targetCopy)
					{
						int index = allPaths.BinarySearch(watching.Key);
						if (index >= 0) // Process is running
						{
							if (watching.Value == false) // it wasn't running last time
							{
								_targetProcessStates[watching.Key] = true;
								anyNewInstances = true;
							}
						}
						else // Process is not running
						{
							_targetProcessStates[watching.Key] = false;
						}
					}
				}

				if (anyNewInstances)
				{
					_iconRef.PopupMessage("Don't forget to watch your important files...", 4);
				}

				Thread.Sleep(2000);
			}
		}
	}
}
