using Engine.Math;
using SFML.Graphics;

namespace Engine
{
	namespace Objects
	{
		static class Methods
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
			public static Matrix MakeTranslationMatrix(Vector translation)
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
		/// A scene containing a camera and all objects to be rendered in the scene.
		/// </summary>
		class Scene
		{
			public Camera camera;
			public Instance[] objects;
			
			public Scene(Camera camera, Instance[] objects)
			{
				this.camera = camera;
				this.objects = objects;
			}

			public void TranslateCam(Vector vector)
			{
				camera.position += camera.yOrientation * vector;
			}
			
			public void RotateCam(char axis, float degrees)
			{
				switch (axis)
				{
					case 'x':
						camera.xOrientation *= Methods.MakeOXRotationMatrix(degrees);
						break;
					case 'y':
						camera.yOrientation *= Methods.MakeOYRotationMatrix(degrees);
						break;
					default:
						throw new ArgumentException($"Invalid/unsupported rotation axis {axis}");
				}
			}
		}
		
		/// <summary>
		/// A specific instance of a model, with its own position, orientation and scale.
		/// </summary>
		class Instance
		{
			public Model model;
			public Vector position;
			public Matrix orientation;
			public float scale;
			
			public Matrix Transform
			{
				get { return Methods.MakeTranslationMatrix(position) * (orientation * Methods.MakeScalingMatrix(scale)); }
			}
			
			public Instance(Model model, Vector position, float xRotation=0, float yRotation=0, float zRotation=0, float scale=1)
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
		class Model
		{
			public Vector[] vertices;
			public Triangle[] triangles;
			public Vector boundsCenter;
			public float boundsRadius;
			
			public Model(Vector[] vertices, Triangle[] triangles, float boundsRadius=0)
			{
				this.vertices = vertices;
				this.triangles = triangles;
				this.boundsCenter = FindCenter();
				this.boundsRadius = boundsRadius == 0 ? FindBoundingRadius() : boundsRadius;
			}
			
			public Vector FindCenter()
			{
				float meanX = 0;
				float meanY = 0;
				float meanZ = 0;
				
				foreach (Vector vertex in vertices)
				{
					meanX += vertex[0];
					meanY += vertex[1];
					meanZ += vertex[2];
				}
				int len = vertices.Length;
				return new Vector(meanX/len, meanY/len, meanZ/len, 1);
			}
			
			public float FindBoundingRadius()
			{
				float r = 0;
				foreach (Vector vertex in vertices)
				{
					float dist = Vector.Distance(boundsCenter, vertex);
					r = dist > r ? dist : r;
				}
				
				return r;
			}
		}
		
		/// <summary>
		/// A point of view with a position and orientation from which to view a scene.
		/// </summary>
		class Camera
		{
			public Vector position;
			public Matrix xOrientation;
			public Matrix yOrientation;
			
			public Matrix Orientation
			{
				get { return yOrientation * xOrientation; }
			}
			
			public Camera(Vector position, float xRotation=0, float yRotation=0)
			{
				this.position = position;
				this.xOrientation = Methods.MakeOXRotationMatrix(xRotation);
				this.yOrientation = Methods.MakeOYRotationMatrix(yRotation);
			}
		}
		
		/// <summary>
		/// A triangle containing the indexes of its vertices and a color value.
		/// </summary>
		class Triangle
		{
			public int v0;
			public int v1;
			public int v2;
			public Color color;
			
			public Triangle(int v0, int v1, int v2, Color color)
			{
				this.v0 = v0;
				this.v1 = v1;
				this.v2 = v2;
				this.color = color;
			}
		}
	}
}