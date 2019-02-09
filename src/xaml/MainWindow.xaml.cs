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
	}
}
