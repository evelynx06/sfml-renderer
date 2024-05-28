using Engine;

namespace TestProject
{
	class Program
	{
		public static void Main()
		{
			Scene testScene = new TestScene { name = "TestScene" };
			
			Viewer viewer = new(1280, 720, 90, testScene);
			
			viewer.Run("Test Window");
		}
	}
}