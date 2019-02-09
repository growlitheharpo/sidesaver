namespace sidesaver
{
	public class SideSaver
	{
		[System.STAThread]
		public static void Main()
		{
			App app = new App();
			app.InitializeComponent();
			app.Run();
		}
	}
}
