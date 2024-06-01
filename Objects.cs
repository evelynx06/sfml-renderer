using Engine.Math;
using SFML.Graphics;

namespace Engine
{
	namespace Objects
	{
		internal static class Methods
		{
			/// <param name="degrees">The rotation angle to generate a transformation matrix for.</param>
			/// <returns>Transformation matrix for a specified rotation around the X axis.</returns>
			internal static Matrix MakeOXRotationMatrix(float degrees)
			{
				float sin = MathF.Sin(degrees*MathF.PI/180);
				float cos = MathF.Cos(degrees*MathF.PI/180);
				return new Matrix(1, 0  , 0   , 0,
								  0, cos, -sin, 0,
								  0, sin, cos , 0,
								  0, 0  , 0   , 1);
			}
			
			/// <param name="degrees">The rotation angle to generate a transformation matrix for.</param>
			/// <returns>Transformation matrix for a specified rotation around the Y axis.</returns>
			internal static Matrix MakeOYRotationMatrix(float degrees)
			{
				float sin = MathF.Sin(degrees*MathF.PI/180);
				float cos = MathF.Cos(degrees*MathF.PI/180);
				return new Matrix(cos , 0, sin, 0,
								  0   , 1, 0  , 0,
								  -sin, 0, cos, 0,
								  0   , 0, 0  , 1);
			}
			
			/// <param name="degrees">The rotation angle to generate a transformation matrix for.</param>
			/// <returns>Transformation matrix for a specified rotation around the Z axis.</returns>
			internal static Matrix MakeOZRotationMatrix(float degrees)
			{
				float sin = MathF.Sin(degrees*MathF.PI/180);
				float cos = MathF.Cos(degrees*MathF.PI/180);
				return new Matrix(cos, -sin, 0, 0,
								  sin, cos , 0, 0,
								  0  , 0   , 0, 0,
								  0  , 0   , 0, 1);
			}
			
			/// <param name="translation">The translation vector to generate a transformation matrix for.</param>
			/// <returns>Transformation matrix for a specified translation.</returns>
			internal static Matrix MakeTranslationMatrix(Vector3 translation)
			{
				return new Matrix(1, 0, 0, translation[0],
								  0, 1, 0, translation[1],
								  0, 0, 1, translation[2],
								  0, 0, 0, 1             );
			}
			
			/// <param name="scale">The scale factor to generate a transformation matrix for.</param>
			/// <returns>Transformation matrix for a specified scale.</returns>
			internal static Matrix MakeScalingMatrix(float scale)
			{
				return new Matrix(scale, 0    , 0    , 0,
								  0    , scale, 0    , 0,
								  0    , 0    , scale, 0,
								  0    , 0    , 0    , 1);
			}
		}
		
		/// <summary>
		/// A specific instance of a model, with its own position, orientation and scale.
		/// </summary>
		public class EngineObject
		{
			public string name;
			public Model? model;
			public Vector3 position;
			public float scale;
			private Vector3 rotation;
			
			public Vector3 Rotation
			{
				get { return rotation; }
				set { rotation = value; orientation = Methods.MakeOXRotationMatrix(rotation.x) * Methods.MakeOYRotationMatrix(rotation.y) * Methods.MakeOZRotationMatrix(rotation.z); }
			}
			
			internal Matrix orientation;
			
			internal Matrix GetTransform
			{
				get { return Methods.MakeTranslationMatrix(position) * (orientation * Methods.MakeScalingMatrix(scale)); }
			}
			
			public EngineObject(Vector3 position, string name="DefaultObject", Model? model=null, float xRotation=0, float yRotation=0, float zRotation=0, float scale=1)
			{
				this.name = name;
				this.model = model;
				this.position = position;
				this.scale = scale;
				this.rotation = new Vector3(xRotation, yRotation, zRotation);
				
				orientation = (xRotation == 0 ? Matrix.Identity : Methods.MakeOXRotationMatrix(xRotation)) * (yRotation == 0 ? Matrix.Identity : Methods.MakeOYRotationMatrix(yRotation)) * (zRotation == 0 ? Matrix.Identity : Methods.MakeOZRotationMatrix(zRotation));
			}
		}
		
		/// <summary>
		/// A collection of vertices connected by triangles.
		/// </summary>
		public class Model
		{
			public Vertex[] vertices;
			public Triangle[] triangles;
			public Vector3 boundsCenter;
			public float boundsRadius;
			
			public Image? texture = null;
			
			public Model(Vertex[] vertices, Triangle[] triangles, Image? texture=null, float boundsRadius=0)
			{
				this.vertices = vertices;
				this.triangles = triangles;
				this.boundsCenter = FindCenter();
				this.boundsRadius = boundsRadius == 0 ? FindBoundingRadius() : boundsRadius;
				this.texture = texture;
			}
			
			public Vector4 FindCenter()
			{
				float meanX = 0;
				float meanY = 0;
				float meanZ = 0;
				
				foreach (Vertex vertex in vertices)
				{
					meanX += vertex.pos[0];
					meanY += vertex.pos[1];
					meanZ += vertex.pos[2];
				}
				int len = vertices.Length;
				return new Vector4(meanX/len, meanY/len, meanZ/len, 1);
			}
			
			public float FindBoundingRadius()
			{
				float r = 0;
				foreach (Vertex vertex in vertices)
				{
					float dist = Vector3.Distance(boundsCenter, vertex.pos);
					r = dist > r ? dist : r;
				}
				
				return r;
			}
		}
		
		/// <summary>
		/// A point of view with a position and orientation from which to view a scene.
		/// </summary>
		public class Camera
		{
			public Vector3 position;
			internal Matrix xOrientation;
			internal Matrix yOrientation;
			
			internal Vector3[] axisGizmo = new Vector3[] {
				new(0f, 0f, 0f),		// origin
				new(0.2f, 0f, 0f),		// x axis
				new(0f, 0.2f, 0f),		// y axis
				new(0f, 0f, 0.2f)			// z axis
			};
			
			internal Matrix GetOrientation
			{
				get { return yOrientation * xOrientation; }
			}
			
			public static Camera Default
			{
				get { return new Camera(new Vector3(0, 0, 0)); }
			}
			
			public Camera(Vector3 position, float xRotation=0, float yRotation=0)
			{
				this.position = position;
				this.xOrientation = Methods.MakeOXRotationMatrix(xRotation);
				this.yOrientation = Methods.MakeOYRotationMatrix(yRotation);
			}
			
			public void Translate(Vector3 vector)
			{
				position += yOrientation * new Vector4(vector, 1);
			}
			
			public void Rotate(char axis, float degrees)
			{
				switch (axis)
				{
					case 'x':
						xOrientation *= Methods.MakeOXRotationMatrix(degrees);
						break;
					case 'y':
						yOrientation *= Methods.MakeOYRotationMatrix(degrees);
						break;
					default:
						throw new ArgumentException($"Invalid/unsupported rotation axis {axis}");
				}
			}
		}
		
		public class Vertex
		{
			public Vector3 pos;
			public Vector2? tc;
			
			public Vertex(Vector3 position, Vector2? textureCoordinate=null)
			{
				pos = position;
				tc = textureCoordinate;
			}
			
			public Vertex InterpolateTo(Vertex dest, float alpha)
			{
				return new(pos.InterpolateTo(dest.pos, alpha), tc?.InterpolateTo(dest.tc ?? tc, alpha));
			}
		}
	}
}