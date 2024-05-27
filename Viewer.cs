using SFML.Window;
using SFML.Graphics;
using Engine.Objects;
using Engine.Math;
using SFML.System;

namespace Engine
{
	class Viewer
	{
		static Renderer renderer = default!;
		static Scene scene = default!;
		static float deltaTime;
		
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
			
			Clock clock = new();
			
			int frames = 0;
			float dtTotal = 0;
			
			while (window.IsOpen)
			{
				deltaTime = clock.ElapsedTime.AsSeconds();	// calculate deltaTime
				clock.Restart();
				
				dtTotal += deltaTime;
				frames++;
				
				if (dtTotal > 1)
				{
					Console.Write("\rFPS: " + (frames/dtTotal).ToString("0.0") + "   ");
					dtTotal = 0;
					frames = 0;
				}
				
				
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
				scene.RotateCam('y', -45*deltaTime);
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
			{
				scene.RotateCam('y', 45*deltaTime);
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
			{
				scene.RotateCam('x', -45*deltaTime);
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
			{
				scene.RotateCam('x', 45*deltaTime);
			}
			
			if (Keyboard.IsKeyPressed(Keyboard.Key.W))		// positive z: forward
			{
				scene.TranslateCam(new Vector(0, 0, 4*deltaTime, 0));
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.S))		// negative z: backward
			{
				scene.TranslateCam(new Vector(0, 0, -4*deltaTime, 0));
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.D))		// positive x: right
			{
				scene.TranslateCam(new Vector(4*deltaTime, 0, 0, 0));
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.A))		// negative x: left
			{
				scene.TranslateCam(new Vector(-4*deltaTime, 0, 0, 0));
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.Space))	// positive y: up
			{
				scene.TranslateCam(new Vector(0, 4*deltaTime, 0, 0));
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.LShift))	// negative y: down
			{
				scene.TranslateCam(new Vector(0, -4*deltaTime, 0, 0));
			}
		}
		
		static Scene InitializeScene()
		{
			Vector[] cubeVertices = new Vector[] {new(-1, -1, -1, 1), new(1, -1, -1, 1), new(-1, 1, -1, 1), new(1, 1, -1, 1),
												  new(-1, -1, 1, 1), new(1, -1, 1, 1), new(-1, 1, 1, 1), new(1, 1, 1, 1)};
			Triangle[] cubeTriangles = new Triangle[] {new(0, 2, 1, Color.Red), new(2, 3, 1, Color.Red), new(1, 3, 5, Color.Green), new(3, 7, 5, Color.Green),
													   new(2, 6, 3, Color.Blue), new(3, 6, 7, Color.Blue), new(4, 5, 7, Color.Magenta), new(4, 7, 6, Color.Magenta),
													   new(0, 4, 2, Color.Yellow), new(2, 4, 6, Color.Yellow), new(0, 1, 4, Color.Cyan), new(1, 5, 4, Color.Cyan)};
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
					Console.WriteLine($"\nfillTriangles: {renderer.fillTriangles}");
					break;
				case Keyboard.Key.RShift:
					renderer.useBackfaceCulling = !renderer.useBackfaceCulling;
					Console.WriteLine($"\nuseBackfaceCulling: {renderer.useBackfaceCulling}");
					break;
			}
		}
	}
}
