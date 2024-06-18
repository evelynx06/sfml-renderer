using SFML.Graphics;
using Engine.Objects;
using Engine.Math;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Engine
{
	/// <summary>
	/// A scene containing a camera and objects.
	/// </summary>
	public abstract class Scene
	{
		/// <summary>
		/// The name of the scene.
		/// </summary>
		public string name = "DefaultScene";
		/// <summary>
		/// The scene's camera.
		/// </summary>
		public Camera camera = Camera.Default;
		/// <summary>
		/// A directional light illuminating the scene.
		/// </summary>
		public Vector3 worldLight = Vector3.Zero;
		
		internal List<EngineObject> objects = new();
		internal List<ObjectScript> scripts = new();
		private readonly List<string> objectNames = new();
		
		/// <summary>
		/// Add an object to the scene.
		/// </summary>
		/// <param name="engineObject">The object to add.</param>
		public void AddObject(EngineObject engineObject)
		{
			objects.Add(engineObject);
			objectNames.Add(engineObject.name);
			Console.WriteLine($"Added object {engineObject.name} to scene {name}.");
		}
		
		/// <summary>
		/// Get an object in the scene.
		/// </summary>
		/// <param name="name">The name of the object to get.</param>
		/// <returns>An engine object.</returns>
		public EngineObject GetObject(string name)
		{
			int index = objectNames.IndexOf(name);
			if (index == -1)
			{
				throw new KeyNotFoundException($"Could not find any object named '{name}'");
			}
			else
			{
				return objects[index];
			}
		}
		
		/// <summary>
		/// Attach a script file to an engine object. The name of the script file must match its class name.
		/// </summary>
		/// <param name="fileName">The name of the script file to attach. The file name must match its class name.</param>
		/// <param name="engineObject">The object to attach the script to.</param>
		public void AttachScript(string fileName, EngineObject engineObject)
		{
			fileName = Regex.Match(fileName, @"(?<=/|\\|^)\w*(?=\.cs)").ToString();
			var type = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
						from t in assembly.GetTypes()
						where t.Name == fileName
						select t).FirstOrDefault();
			
			if (type == null)
				throw new InvalidOperationException($"Script '{fileName}' not found!");
			
			scripts.Add((ObjectScript)Activator.CreateInstance(type)!);
			scripts.Last().attachedObject = engineObject;
        }
		
		/// <summary>
		/// Called once every time the scene is activated.
		/// </summary>
		public virtual void Init() {}
		/// <summary>
		/// Called every frame.
		/// </summary>
		/// <param name="dt">Delta time, the time since the last frame.</param>
		public virtual void Update(float dt) {}
	}
}