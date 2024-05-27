using SFML.Window;
using SFML.Graphics;
using Engine.Objects;
using Engine.Math;

namespace Engine
{
	class Viewer
	{
		static Renderer renderer;
		static Scene scene;
		
		static void Main()
		{
			uint width = 1200;
			uint height = 600;
			RenderWindow window = new(new VideoMode(width, height), "Test Window", Styles.Close);
			window.Closed += new EventHandler(OnClose);
			window.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPressed);
			
			Texture canvasTexture = new(width, height);
			Sprite canvasSprite = new(canvasTexture);
			renderer = new((int)width, (int)height, 2, 1, 1);
			
			scene = InitializeScene();
			
			while (window.IsOpen)
			{
				window.DispatchEvents();	// event handling
				Movements(scene);
				Canvas canvas = new(width, height);
				renderer.RenderScene(scene, ref canvas);
				
				canvasTexture.Update(canvas.screen);
				window.Draw(canvasSprite);
				window.Display();	// update display
			}
		}
		
		static void Movements(Scene scene)
		{
			if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
			{
				scene.RotateCam('y', -0.5f);
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
			{
				scene.RotateCam('y', 0.5f);
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
			{
				scene.RotateCam('x', -0.5f);
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
			{
				scene.RotateCam('x', 0.5f);
			}
			
			if (Keyboard.IsKeyPressed(Keyboard.Key.W))
			{
				scene.TranslateCam(new Vector(0, 0, 0.05f, 0));
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.S))
			{
				scene.TranslateCam(new Vector(0, 0, -0.05f, 0));
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.A))
			{
				scene.TranslateCam(new Vector(-0.05f, 0, 0, 0));
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.D))
			{
				scene.TranslateCam(new Vector(0.05f, 0, 0, 0));
			}
		}
		
		static Scene InitializeScene()
		{
			Vector[] cubeVertices = new Vector[] {new(1, 1, 1, 1), new(-1, 1, 1, 1), new(-1, -1, 1, 1), new(1, -1, 1, 1),
												  new(1, 1, -1, 1), new(-1, 1, -1, 1), new(-1, -1, -1, 1), new(1, -1, -1, 1)};
			Triangle[] cubeTriangles = new Triangle[] {new(0, 1, 2, Color.Red), new(0, 2, 3, Color.Red), new(4, 0, 3, Color.Green), new(4, 3, 7, Color.Green),
													   new(5, 4, 7, Color.Blue), new(5, 7, 6, Color.Blue), new(1, 5, 6, Color.Yellow), new(1, 6, 2, Color.Yellow),
													   new(4, 5, 1, Color.Magenta), new(4, 1, 0, Color.Magenta), new(2, 6, 7, Color.Cyan), new(2, 7, 3, Color.Cyan)};
			Model cube = new(cubeVertices, cubeTriangles);
			
			
			Instance[] objects = new Instance[] {new(cube, new(-1.5f, 0f, 7f, 1f), 0, 0, 0, 0.75f),
												 new(cube, new(1.25f, 2.5f, 7.5f, 1f), 0, -195) };
												//  new(cube, new(0f, 0f, -10f, 1f), 0, -195)};
			Camera camera = new(new Vector(-3f, 1f, 2f, 1f), 0, 30);
			return new Scene(camera, objects);
		}
		
		static void OnClose(object? sender, EventArgs e)
		{
			if (sender != null)
			{
				RenderWindow window = (RenderWindow)sender;
				window.Close();
			}
		}
		
		static void OnKeyPressed(object? sender, KeyEventArgs e)
		{
			switch (e.Code)
			{
				case Keyboard.Key.Enter:
					renderer.fillTriangles = !renderer.fillTriangles;
					Console.WriteLine(renderer.fillTriangles);
					break;
				case Keyboard.Key.R:
					Console.Write("Rotation angle: ");
					float input = Convert.ToSingle(Console.ReadLine());
					scene.RotateCam('y', input);
					break;
			}
		}
	}
}
