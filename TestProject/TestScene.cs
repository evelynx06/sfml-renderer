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
			// Read assets
			// Model cubeModel = Files.ReadObjFile(@"TestProject\assets\blenderCube.obj")[0];
			// Model teapotModel = Files.ReadObjFile(@"TestProject\assets\teapot.obj")[0];
			// Model teddyModel = Files.ReadObjFile(@"TestProject\assets\teddy.obj")[0];
			// Model capsuleModel = Files.ReadObjFile(@"TestProject\assets\capsule\capsule.obj")[0];
			Model elizaModel = Files.ReadObjFile(@"TestProject\assets\Eliza.obj")[0];
			
			// Define objects
			// AddObject(new EngineObject(new Vector3(0f, 0f, 7f), "cube", cubeModel, 0, 0, 0, 0.75f));
			// AddObject(new EngineObject(new Vector3(1.25f, 2.5f, 7.5f), "teapot", teapotModel, 0, -195));
			// AddObject(new EngineObject(new Vector3(0f, 0f, 10f), "teddy", teddyModel));
			// AddObject(new EngineObject(new Vector3(25f, 0f, 10f), "capsule", capsuleModel));
			AddObject(new EngineObject(new Vector3(0f, 0f, 10f), "eliza", elizaModel));
			
			// Attach scripts
			// AttachScript("CubeScript.cs", GetObject("cube"));
			
			// Define camera and lighting
			camera = new(new(-3, 2, 1), 0, 30);
			worldLight = new(0, 0, -1);
		}
	
		public override void Update(float dt)
		{
			if (Keyboard.IsKeyPressed(Keyboard.Key.LSystem))	// don't process my fucking input when i'm trying to take a screenshot
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