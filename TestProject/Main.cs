using Engine;

namespace TestProject
{
	class Program
	{
		public static void Main()
		{
			Scene testScene = new TestScene { name = "TestScene" };
			
			Viewer viewer = new(900, 900, 80, testScene);
			
			viewer.Run("Test Window");
		}
	}
}