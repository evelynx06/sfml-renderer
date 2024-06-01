using SFML.Graphics;
using Engine.Objects;
using Engine.Math;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Engine
{
	/// <summary>
	/// A scene containing a camera and all objects to be rendered in the scene.
	/// </summary>
	public abstract class Scene
	{
		public string name = "DefaultScene";
		public Camera camera = Camera.Default;
		public Vector3 worldLight = new();
		internal List<EngineObject> objects = new();
		internal List<ObjectScript> scripts = new();
		private List<string> objectNames = new();
		
		public void AddObject(EngineObject engineObject)
		{
			objects.Add(engineObject);
			objectNames.Add(engineObject.name);
			Console.WriteLine(engineObject.name);
		}
		
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
		
		public virtual void Init() {}
		public virtual void Update(float dt) {}
	}
}