using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace Engine
{
	/// <summary>
	/// A window that can display scenes.
	/// </summary>
	public class Viewer
	{
		private readonly Renderer renderer;
		private readonly Dictionary<string, Scene> scenes;
		private float deltaTime;
		
		/// <summary>
		/// The scene currently displayed in the viewer.
		/// </summary>
		public Scene activeScene;
		/// <summary>
		/// The width of the viewer window.
		/// </summary>
		public int WindowWidth { get => renderer.CanvasWidth; }
		/// <summary>
		/// The height of the viewer window.
		/// </summary>
		public int WindowHeight { get => renderer.CanvasHeight; }
		
		/// <summary>
		/// Initializes a new instance of the Viewer class with a specified window size and a main scene.
		/// </summary>
		/// <param name="windowWidth">The width of the viewer window.</param>
		/// <param name="windowHeight">The height of the viewer window.</param>
		/// <param name="mainScene">The first scene, which is displayed by default.</param>
		/// <param name="fov">The field of view.</param>
		public Viewer(int windowWidth, int windowHeight, Scene mainScene, float fov=90.0f)
		{	
			renderer = new(windowWidth, windowHeight, fov);
			
			scenes = new();
			AddScene(mainScene);
			activeScene = mainScene;
		}
		
		/// <summary>
		/// Add a scene to the viewer's list of scenes.
		/// </summary>
		/// <param name="scene">The scene to add.</param>
		public void AddScene(Scene scene)
		{
			scenes.Add(scene.name, scene);
			Console.WriteLine($"Added scene {scene.name}.");
		}
		
		/// <summary>
		/// Change which scene is currently displayed in the viewer.
		/// </summary>
		/// <param name="sceneName">The name of the scene to change to.</param>
		public void ChangeToScene(string sceneName)
		{
			if (scenes.ContainsKey(sceneName))
			{
				activeScene = scenes[sceneName];
			}
			else
			{
				throw new ArgumentOutOfRangeException($"Could not find scene '{sceneName}'!");
			}
		}
		
		/// <summary>
		/// Change which scene is currently displayed in the viewer.
		/// </summary>
		/// <param name="scene">The scene to change to.</param>
		public void ChangeToScene(Scene scene)
		{
			if (scenes.ContainsKey(scene.name))
			{
				activeScene = scenes[scene.name];
				scene.Init();
			}
			else
			{
				throw new ArgumentException($"Could not find scene '{scene.name}'!");
			}
		}
		
		/// <summary>
		/// Start the viewer window.
		/// </summary>
		/// <param name="title">The title of the window.</param>
		public void Run(string title="Window")
		{
			uint width = (uint)WindowWidth;
			uint height = (uint)WindowHeight;
			RenderWindow window = new(new VideoMode(width, height), title, Styles.Close);
			
			window.Closed += new EventHandler(OnClose);
			window.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPressed);
			window.Resized += new EventHandler<SizeEventArgs>(OnResize);
			
			Texture canvasTexture = new(width, height);
			Sprite canvasSprite = new(canvasTexture);
			
			Clock clock = new();
			int frames = 0;
			float dtTotal = 0;
			
			// activate user written functions
			activeScene.Init();
			foreach (ObjectScript script in activeScene.scripts)
			{
				script.Init();
			}
			
			
			while (window.IsOpen)
			{
				// calculate deltaTime
				deltaTime = clock.ElapsedTime.AsSeconds();
				clock.Restart();
				dtTotal += deltaTime;
				frames++;
				if (dtTotal > 1)	// once every second, print the average fps
				{
					Console.Write("\rFPS: " + (frames/dtTotal).ToString("0.0") + "   ");
					dtTotal = 0;
					frames = 0;
				}
				
				
				// event handling
				window.DispatchEvents();
				
				// activate user written functions
				activeScene.Update(deltaTime);
				foreach (ObjectScript script in activeScene.scripts)
				{
					script.Update(deltaTime);
				}
				
				
				// render scene
				Canvas canvas = new(width, height);
				renderer.RenderScene(activeScene, ref canvas);
				
				// update display
				canvasTexture.Update(canvas.screen);
				window.Draw(canvasSprite);
				window.Display();
			}
		}
		
		private void OnClose(object? sender, EventArgs e)
		{
			if (sender != null)
			{
				RenderWindow window = (RenderWindow)sender;
				window.Close();
			}
		}
		
		private void OnKeyPressed(object? sender, KeyEventArgs e)
		{
			switch (e.Code)
			{
				case Keyboard.Key.Enter:
					renderer.fillTriangles = !renderer.fillTriangles;
					Console.WriteLine($"\nfillTriangles: {renderer.fillTriangles}");
					break;
				case Keyboard.Key.RShift:
					renderer.useShading = !renderer.useShading;
					Console.WriteLine($"\nuseShading: {renderer.useShading}");
					break;
				case Keyboard.Key.End:
					renderer.showGizmo = !renderer.showGizmo;
					Console.WriteLine($"\nshowGizmo: {renderer.showGizmo}");
					break;
			}
		}
		
		private void OnResize(object? sender, SizeEventArgs e)
		{
			renderer.CanvasHeight = (int)e.Height;
			renderer.CanvasWidth = (int)e.Width;
		}
	}
}
