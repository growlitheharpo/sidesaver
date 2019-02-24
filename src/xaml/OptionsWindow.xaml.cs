using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace sidesaver
{
	/// <summary>
	/// Interaction logic for OptionsWindow.xaml
	/// </summary>
	public partial class OptionsWindow
	{
		private static OptionsWindow s_instance;

		public static OptionsWindow Create(NotifyChangedUserSettings settings)
		{
			return s_instance ?? new OptionsWindow(settings);
		}

		private readonly NotifyChangedUserSettings _settings;

		private OptionsWindow(NotifyChangedUserSettings settings)
		{
			if (s_instance != null)
				throw new InvalidOperationException();

			s_instance = this;

			_settings = settings;

			InitializeComponent();
			Loaded += OnLoaded;
			DataContext = _settings;

			Closed += OnClosed;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Loaded -= OnLoaded;
			OKButton.Focus();
		}

		private void OnClosed(object sender, EventArgs e)
		{
			Closed -= OnClosed;
			s_instance = null;
		}

		private void BrowseButton_OnClick(object sender, RoutedEventArgs e)
		{
			CommonOpenFileDialog dialog = new CommonOpenFileDialog
			{
				InitialDirectory = _settings.OverrideSaveLocationPath,
				IsFolderPicker = true,
				EnsurePathExists = true,
				ShowPlacesList = true,
				Multiselect = false
			};
			if (dialog.ShowDialog(this) != CommonFileDialogResult.Ok)
				return;

			_settings.OverrideSaveLocationPath = dialog.FileName;
		}

		private void OKButton_OnClick(object sender, RoutedEventArgs e)
		{
			ApplyButton_OnClick(sender, e);
			Close();
		}

		private void ApplyButton_OnClick(object sender, RoutedEventArgs e)
		{
			SideSaver.instance.CommitNewSettings(_settings);
		}

		private void CancelButton_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			FrameworkElement realSender = sender as FrameworkElement;

			if (realSender?.DataContext is string filePath)
				_settings.WatchedPrograms.Remove(filePath);
		}

		private void AddProgram_OnClick(object sender, RoutedEventArgs e)
		{
			var fileDialog = new Microsoft.Win32.OpenFileDialog
			{
				Multiselect = false,
				AddExtension = true,
				CheckFileExists = true,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
			};
			fileDialog.Multiselect = true;

			if (fileDialog.ShowDialog(Application.Current.MainWindow) != true)
				return;

			foreach (var path in fileDialog.FileNames)
			{
				if (_settings.WatchedPrograms.Contains(path))
					continue;

				_settings.WatchedPrograms.Add(path);
			}
		}
	}

	public class BackupCountConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return "ERR";
			var v = (int) value;
			return v == 0 ? "infinite" : v.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string str = value as string ?? "1";
			return str == "infinite" ? 0 : int.Parse(str);
		}
	}
}
