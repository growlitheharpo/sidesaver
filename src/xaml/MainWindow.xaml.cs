using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace sidesaver
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ImageSource _imageSource;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = SideSaver.instance;
			SideSaver.instance.HookMainWindow(this);

			Loaded += (s, e) => GenerateIconImage();
			GenerateIconImage();
		}

		private void GenerateIconImage()
		{
			var u = new Uri("resources/main_black.ico", UriKind.Relative);
			using (var resourceStream = Application.GetResourceStream(u)?.Stream)
			{
				if (resourceStream == null)
					return;

				var i = new Icon(resourceStream, 256, 256);

				Int32Rect iRect = new Int32Rect(0, 0, i.Width, i.Height);
				IconImage.Source = Imaging.CreateBitmapSourceFromHIcon(i.Handle, iRect, BitmapSizeOptions.FromEmptyOptions());
			}
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
			SideSaver.instance.CreateSettingsMenu();
		}

		private void HideWindow_OnClick(object sender, RoutedEventArgs e)
		{
			Hide();
		}

		private void Thumb_OnDragDelta(object sender, DragDeltaEventArgs e)
		{
			Left += e.HorizontalChange;
			Top += e.VerticalChange;
		}
	}
}
