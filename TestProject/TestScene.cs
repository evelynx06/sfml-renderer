using Engine;
using Engine.Objects;
using Engine.Math;
using SFML.Graphics;
using SFML.Window;

namespace TestProject
{
	public class TestScene : Scene
	{
		public override void Init()
		{
			// Vector[] cubeVertices = new Vector[] {new(-1, -1, -1, 1), new(1, -1, -1, 1), new(-1, 1, -1, 1), new(1, 1, -1, 1),
			// 									  new(-1, -1, 1, 1), new(1, -1, 1, 1), new(-1, 1, 1, 1), new(1, 1, 1, 1)};
			// Triangle[] cubeTriangles = new Triangle[] {new(0, 2, 1, Color.Red), new(2, 3, 1, Color.Red), new(1, 3, 5, Color.Green), new(3, 7, 5, Color.Green),
			// 										   new(2, 6, 3, Color.Blue), new(3, 6, 7, Color.Blue), new(4, 5, 7, Color.Magenta), new(4, 7, 6, Color.Magenta),
			// 										   new(0, 4, 2, Color.Yellow), new(2, 4, 6, Color.Yellow), new(0, 1, 4, Color.Cyan), new(1, 5, 4, Color.Cyan)};
			
			// Model cube = new(cubeVertices, cubeTriangles);
			
			Model cube = Files.ReadObjFile(@"TestProject\assets\overhang.obj")[0];
			Model thing = Files.ReadObjFile(@"TestProject\assets\teddy.obj")[0];
			
			// Console.WriteLine(cube)
			
			objects = new Instance[] {new(cube, new(-1.5f, 0f, 7f), 0, 0, 0, 0.75f),
									  new(thing, new(1.25f, 2.5f, 7.5f), 0, -195) };
									//   new(cube, new(0f, 0f, -10f, 1f), 0, -195)};
			camera = new(new(-3, 2, 1), 0, 30);
			worldLight = new(0, 0, -1);
		}
	
		public override void Update(float dt)
		{
			if (Keyboard.IsKeyPressed(Keyboard.Key.LSystem))
			{
				return;
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
			{
				camera.Rotate('y', -45*dt);
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
			{
				camera.Rotate('y', 45*dt);
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
			{
				camera.Rotate('x', -45*dt);
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
			{
				camera.Rotate('x', 45*dt);
			}
			
			if (Keyboard.IsKeyPressed(Keyboard.Key.W))		// positive z: forward
			{
				camera.Translate(new Vector3(0, 0, 4*dt));
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.S))		// negative z: backward
			{
				camera.Translate(new Vector3(0, 0, -4*dt));
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.D))		// positive x: right
			{
				camera.Translate(new Vector3(4*dt, 0, 0));
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.A))		// negative x: left
			{
				camera.Translate(new Vector3(-4*dt, 0, 0));
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.Space))	// positive y: up
			{
				camera.Translate(new Vector3(0, 4*dt, 0));
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.LShift))	// negative y: down
			{
				camera.Translate(new Vector3(0, -4*dt, 0));
			}
		}
	}
}