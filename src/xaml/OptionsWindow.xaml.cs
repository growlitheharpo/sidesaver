using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace sidesaver
{
	/// <summary>
	/// Interaction logic for OptionsWindow.xaml
	/// </summary>
	public partial class OptionsWindow : Window
	{
		public OptionsWindow()
		{
			InitializeComponent();
			Loaded += OnLoaded;
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
