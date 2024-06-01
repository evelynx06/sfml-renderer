using Engine.Math;
using Engine.Objects;

namespace Engine
{
	public abstract class ObjectScript
	{
		internal EngineObject attachedObject = null!;
		
		/// <summary>
		/// The object's position in 3D space.
		/// </summary>
		protected Vector3 Position
		{
			get { return attachedObject.position; }
			set { attachedObject.position = value; }
		}
		
		/// <summary>
		/// The object's rotation, represented as a Vector3 of the x, y and z rotation in degrees.
		/// </summary>
		protected Vector3 Rotation
		{
			get { return attachedObject.Rotation; }
			set { attachedObject.Rotation = value; }
		}
		
		/// <summary>
		/// The object's scale.
		/// </summary>
		protected float Scale
		{
			get { return attachedObject.scale; }
			set { attachedObject.scale = value; }
		}
		
		
		public virtual void Init() {}
		public virtual void Update(float dt) {}
	}
}