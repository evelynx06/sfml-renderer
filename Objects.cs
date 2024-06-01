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
			public static Matrix MakeOXRotationMatrix(float degrees)
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
			public static Matrix MakeOYRotationMatrix(float degrees)
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
			public static Matrix MakeOZRotationMatrix(float degrees)
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
			public static Matrix MakeTranslationMatrix(Vector3 translation)
			{
				return new Matrix(1, 0, 0, translation[0],
								  0, 1, 0, translation[1],
								  0, 0, 1, translation[2],
								  0, 0, 0, 1             );
			}
			
			/// <param name="scale">The scale factor to generate a transformation matrix for.</param>
			/// <returns>Transformation matrix for a specified scale.</returns>
			public static Matrix MakeScalingMatrix(float scale)
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
		public class Instance
		{
			public Model model;
			public Vector3 position;
			public Matrix orientation;
			public float scale;
			
			public Matrix Transform
			{
				get { return Methods.MakeTranslationMatrix(position) * (orientation * Methods.MakeScalingMatrix(scale)); }
			}
			
			public Instance(Model model, Vector3 position, float xRotation=0, float yRotation=0, float zRotation=0, float scale=1)
			{
				this.model = model;
				this.position = position;
				this.orientation = (xRotation == 0 ? Matrix.Identity() : Methods.MakeOXRotationMatrix(xRotation)) * (yRotation == 0 ? Matrix.Identity() : Methods.MakeOYRotationMatrix(yRotation)) * (zRotation == 0 ? Matrix.Identity() : Methods.MakeOZRotationMatrix(zRotation));
				this.scale = scale;
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
			public Matrix xOrientation;
			public Matrix yOrientation;
			
			public Matrix Orientation
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