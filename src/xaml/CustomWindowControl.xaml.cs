using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace sidesaver
{
	/// <summary>
	/// Interaction logic for CustomWindowControl.xaml
	/// </summary>
	public partial class CustomWindowControl : UserControl, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private Window ParentWindow => Window.GetWindow(this);

		public Visibility MaximizeButtonVisibility
		{
			get
			{
				if (ParentWindow != null && ParentWindow.WindowState == WindowState.Maximized)
					return Visibility.Collapsed;
				return Visibility.Visible;
			}
			set
			{
				OnPropertyChanged();
				RestoreButtonVisibility = value;
			}
		}

		public Visibility RestoreButtonVisibility
		{
			get => MaximizeButtonVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
			set => OnPropertyChanged();
		}

		public CustomWindowControl()
		{
			DataContext = this;
			InitializeComponent();

			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (ParentWindow != null)
			{
				ParentWindow.StateChanged += OnParentWindowStateChanged;
				OnParentWindowStateChanged(null, null);
			}

			Loaded -= OnLoaded;
		}

		private void OnParentWindowStateChanged(object? sender, EventArgs? e)
		{
			if (ParentWindow == null)
				return;

			MaximizeButtonVisibility =
				ParentWindow.WindowState == WindowState.Maximized ? Visibility.Visible : Visibility.Collapsed;
		}

		private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
		{
			SystemCommands.MinimizeWindow(ParentWindow);
		}

		private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
		{
			SystemCommands.MaximizeWindow(ParentWindow);
		}

		private void RestoreButton_OnClick(object sender, RoutedEventArgs e)
		{
			SystemCommands.RestoreWindow(ParentWindow);
		}

		private void CloseButton_OnClick(object sender, RoutedEventArgs e)
		{
			SystemCommands.CloseWindow(ParentWindow);
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
