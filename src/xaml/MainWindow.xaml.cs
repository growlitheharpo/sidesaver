using System;
using System.Windows;

namespace sidesaver
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = SideSaver.instance;
		}

		protected override void OnStateChanged(EventArgs e)
		{
			if (WindowState == WindowState.Minimized)
				Hide();

			base.OnStateChanged(e);
		}

		private void AddFile_OnClick(object sender, RoutedEventArgs e)
		{
			SideSaver.instance.AddNewFile();
		}

		private void MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			FrameworkElement realSender = sender as FrameworkElement;

			if (realSender?.DataContext is string filePath)
				SideSaver.instance.StopWatching(filePath);
		}

		private void Settings_OnClick(object sender, RoutedEventArgs e)
		{
			OptionsWindow opt = new OptionsWindow {DataContext = DataContext};
			opt.Show();
		}
	}
}
