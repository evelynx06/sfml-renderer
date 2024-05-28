using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace Engine
{
	public class Viewer
	{
		private Renderer renderer = default!;
		private Dictionary<string, Scene> scenes = default!;
		private float deltaTime;
		
		public Scene activeScene;
		public int WindowWidth { get => renderer.CanvasWidth; }
		public int WindowHeight { get => renderer.CanvasHeight; }
		
		public Viewer(int windowWidth, int windowHeight, float fov, Scene mainScene)
		{	
			renderer = new(windowWidth, windowHeight, fov, 1);
			// AddScene(mainScene);
			activeScene = mainScene;
		}
		
		public void AddScene(Scene scene)
		{
			scenes.Add(scene.name, scene);
		}
		
		public void ChangeToScene(string sceneName)
		{
			if (scenes.ContainsKey(sceneName))
			{
				activeScene = scenes[sceneName];
			}
			else
			{
				throw new ArgumentException($"Could not find scene '{sceneName}'!");
			}
		}
		
		public void ChangeToScene(Scene scene)
		{
			if (scenes.ContainsKey(scene.name))
			{
				activeScene = scenes[scene.name];
			}
			else
			{
				throw new ArgumentException($"Could not find scene '{scene.name}'!");
			}
		}
		
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
			
			activeScene.Init();	// user defined
			
			while (window.IsOpen)
			{
				deltaTime = clock.ElapsedTime.AsSeconds();	// calculate deltaTime
				clock.Restart();
				
				dtTotal += deltaTime;
				frames++;
					
				if (dtTotal > 1)	// once every second print the average fps
				{
					Console.Write("\rFPS: " + (frames/dtTotal).ToString("0.0") + "   ");
					dtTotal = 0;
					frames = 0;
				}
				
				window.DispatchEvents();	// event handling
				
				activeScene.Update(deltaTime);	// user defined
				
				Canvas canvas = new(width, height);
				renderer.RenderScene(activeScene, ref canvas);	// render scene
				
				canvasTexture.Update(canvas.screen);	// update display
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
					renderer.useBackfaceCulling = !renderer.useBackfaceCulling;
					Console.WriteLine($"\nuseBackfaceCulling: {renderer.useBackfaceCulling}");
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
