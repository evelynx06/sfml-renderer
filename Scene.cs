using SFML.Graphics;
using Engine.Objects;
using Engine.Math;

namespace Engine
{
	/// <summary>
	/// A scene containing a camera and all objects to be rendered in the scene.
	/// </summary>
	public abstract class Scene
	{
		public string name = "DefaultScene";
		public Camera camera = default!;
		public Instance[] objects = Array.Empty<Instance>();
		
		public virtual void Init() {}
		public virtual void Update(float dt) {}
	}
}