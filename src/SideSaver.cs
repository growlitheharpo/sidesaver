using System.Windows;

namespace sidesaver
{
	public class SideSaver
	{
		public static SideSaver instance { get; private set; }

		public string MainScreenText
		{
			get { return "This is some stuff!"; }
		}

		[System.STAThread]
		public static void Main()
		{
			SideSaver s = new SideSaver();
		}

		private SideSaver()
		{
			instance = this;
			Initialize();
		}

		private void Initialize()
		{
			App app = new App();
			app.InitializeComponent();
			app.Run();
		}
	}
}
