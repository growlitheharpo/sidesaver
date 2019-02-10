using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace sidesaver
{
	/// <summary>
	/// Interaction logic for OptionsWindow.xaml
	/// </summary>
	public partial class OptionsWindow : Window
	{
		private UserSettings _settings;

		public OptionsWindow(UserSettings settings)
		{
			_settings = settings;

			InitializeComponent();
			Loaded += OnLoaded;
			DataContext = _settings;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Loaded -= OnLoaded;
			OKButton.Focus();
		}

		private void OKButton_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void BrowseButton_OnClick(object sender, RoutedEventArgs e)
		{
			CommonOpenFileDialog dialog = new CommonOpenFileDialog
			{
				InitialDirectory = _settings.OverrideSaveLocation,
				IsFolderPicker = true,
				EnsurePathExists = true,
				ShowPlacesList = true,
				Multiselect = false
			};
			if (dialog.ShowDialog(this) != CommonFileDialogResult.Ok)
				return;

			_settings.OverrideSaveLocation = dialog.FileName;
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
