using SFML.Window;
using Engine;
using Engine.Math;
using Engine.Objects;

namespace TestProject
{
	public class CubeScript : ObjectScript
	{
		public override void Init()
		{
			base.Init();
		}

		public override void Update(float dt)
		{
			if (Keyboard.IsKeyPressed(Keyboard.Key.I))		// positive z: forward
			{
				Position.z += 4*dt;
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.K))		// negative z: backward
			{
				Position.z -= 4*dt;
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.L))		// positive x: right
			{
				Position.x += 4*dt;
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.J))		// negative x: left
			{
				Position.x -= 4*dt;
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.H))	// positive y: up
			{
				Position.y += 4*dt;
			}
			if (Keyboard.IsKeyPressed(Keyboard.Key.N))	// negative y: down
			{
				Position.y -= 4*dt;
			}
		}
	}
}