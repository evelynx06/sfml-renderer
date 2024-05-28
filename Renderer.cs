using Engine.Math;
using Engine.Objects;
using SFML.Graphics;

namespace Engine
{
	internal class Renderer
	{
		private int canvasWidth;
		internal int CanvasWidth
		{
			get => canvasWidth;
			set => SetCanvasSize(width: value);
		}
		private int canvasHeight;
		internal int CanvasHeight
		{
			get => canvasHeight;
			set => SetCanvasSize(height: value);
		}
		internal float fov;
		internal float viewportWidth;
		internal float viewportHeight;
		internal float projectionPlaneZ;
		internal Plane[] clippingPlanes;
		
		internal bool fillTriangles = false;
		internal bool useBackfaceCulling = true;
		
		
		private void SetCanvasSize(int? width=null, int? height=null)
		{
			canvasWidth = width ?? canvasWidth;
			canvasHeight = height ?? canvasHeight;
			
			float aspectRatio = (float)canvasHeight/(float)canvasWidth;
			viewportWidth = MathF.Tan((fov/2)*(MathF.PI/180))*projectionPlaneZ;
			viewportHeight = aspectRatio;
		}
		
		internal Renderer(int canvasWidth, int canvasHeight, float fov, float projectionPlaneZ=1)
		{
			this.canvasWidth = canvasWidth;
			this.canvasHeight = canvasHeight;
			this.fov = fov;
			float aspectRatio = (float)canvasHeight/(float)canvasWidth;
			viewportWidth = MathF.Tan((fov/2)*(MathF.PI/180))*projectionPlaneZ;
			viewportHeight = aspectRatio;
			this.projectionPlaneZ = projectionPlaneZ;
			this.clippingPlanes = new Plane[]{new Plane(new Vector3( 0, 0, 1), -projectionPlaneZ),	// near
											  new Plane(new Vector3( 1, 0, 1), 0),		// left
											  new Plane(new Vector3(-1, 0, 1), 0),		// right
											  new Plane(new Vector3( 0, 1, 1), 0),		// bottom
											  new Plane(new Vector3( 0,-1, 1), 0)};		// top
		}
		
		internal void RenderScene(Scene scene, ref Canvas canvas)
		{
			Matrix cameraMatrix = scene.camera.Orientation.Transposed() * Methods.MakeTranslationMatrix(-1 * scene.camera.position);
			
			foreach (Instance o in scene.objects)
			{
				Matrix transform = cameraMatrix * o.Transform;
				
				Model? clipped = TransformAndClip(o.model, o.scale, transform);
				if (clipped != null)
				{
					RenderModel(clipped, canvas);
				}
			}
		}
		
		internal void RenderModel(Model model, Canvas canvas)
		{
			Point[] projected = new Point[model.vertices.Length];
			for (int i = 0; i < projected.Length; i++)
			{
				projected[i] = ProjectVertex(model.vertices[i].position);
			}
			if (useBackfaceCulling)
			{
				foreach (Triangle t in model.triangles)
				{
					if (!BackfaceCullTriangle(t, model.vertices))
					{
						RenderTriangle(t, projected, canvas);
					}
					// else{
					// 	Vector3 triangleNormal = Vector3.Cross(model.vertices[t.v1].position - model.vertices[t.v0].position,
					// 										   model.vertices[t.v2].position - model.vertices[t.v0].position);
					// 	canvas.DrawWireTriangle(ProjectVertex(triangleNormal + model.vertices[t.v2].position), projected[t.v2], projected[t.v1], t.color);
					//  	canvas.DrawWireTriangle(projected[t.v0], projected[t.v1], projected[t.v2], Color.White);
					// }
				}
			}
			else
			{
				foreach (Triangle t in model.triangles)
				{
					RenderTriangle(t, projected, canvas);
				}
			}
			
		}
		
		private static bool BackfaceCullTriangle(Triangle t, TextureVertex[] vertices)
		{
			// N = (B - A) x (C - A)
			Vector3 triangleNormal = Vector3.Cross(vertices[t.v1].position - vertices[t.v0].position, vertices[t.v2].position - vertices[t.v0].position);
			
			// V = camPos - A	(camPos is [0, 0, 0])
			Vector3 triangleToCamera = -1 * vertices[t.v2].position;
			
			return Vector3.Dot(triangleToCamera, triangleNormal) <= 0;
		}
		
		private void RenderTriangle(Triangle t, Point[] projected, Canvas canvas)
		{
			if (fillTriangles)
			{
				canvas.DrawFilledTriangle(projected[t.v0], projected[t.v1], projected[t.v2], t.color);
			}
			else
			{
				canvas.DrawWireTriangle(projected[t.v0], projected[t.v1], projected[t.v2], t.color);
			}
		}
		
		private Point ProjectVertex(Vector3 v)
		{
			float x;
			float y;
			// perspective projection
			if (v.z == 0)
			{
				x = 0;
				y = 0;
			}
			else
			{
				x = v.x * projectionPlaneZ/v.z;
				y = v.y * projectionPlaneZ/v.z;
			}
			
			// viewport to canvas
			x = x * canvasWidth/viewportWidth;
			y = y * canvasHeight/viewportHeight;
			
			return new Point(x, y, v.z);
		}
		
		private Model? TransformAndClip(Model model, float scale, Matrix transform)
		{
			Vector4 center = transform * model.FindCenter();
			float radius = scale * model.boundsRadius;
			
			int tmp = 0;
			
			foreach (Plane p in clippingPlanes)
			{
				float d = p.SignedDistance(center.Vector3);
				if (d < -radius)
				{
					return null;
				}
				else if (d > radius)
				{
					tmp++;
				}
			}
			
			List<TextureVertex> vertices = new();
			foreach (TextureVertex v in model.vertices)
			{
				vertices.Add(new TextureVertex((transform * new Vector4(v.position, 1)).Vector3, v.textureCoordinate));
			}
			
			if (tmp == clippingPlanes.Length)
			{
				return new Model(vertices.ToArray(), model.triangles, model.boundsRadius);
			}
			
			
			Triangle[] triangles = model.triangles;
			foreach (Plane p in clippingPlanes)
			{
				List<Triangle> clippedTriangels = new();
				foreach (Triangle t in triangles)
				{
					ClipTriangle(t, p, ref clippedTriangels, ref vertices);
				}
				triangles = clippedTriangels.ToArray();
			}
			
			return new Model(vertices.ToArray(), triangles, model.boundsRadius);
		}
		
		private static void ClipTriangle(Triangle triangle, Plane plane, ref List<Triangle> clippedTriangles, ref List<TextureVertex> vertices)
		{
			float d0 = plane.SignedDistance(vertices[triangle.v0].position);
			float d1 = plane.SignedDistance(vertices[triangle.v1].position);
			float d2 = plane.SignedDistance(vertices[triangle.v2].position);
			
			if (d0 >= 0 && d1 >= 0 && d2 >= 0)
			{
				clippedTriangles.Add(triangle);
				return;
			}
			else if (d0 < 0 && d1 < 0 && d2 < 0)
			{
				return;
			}
			else if (d0 >= 0 ^ d1 >= 0 ^ d2 >= 0)
			{
				// only one vertex is in
				// A is the vertex that's in
				// B and C are the other two
				int a, b, c;
				if (d0 >= 0)
				{
					a = triangle.v0;
					b = triangle.v1;
					c = triangle.v2;
				}
				else if (d1 >= 0)
				{
					a = triangle.v1;
					b = triangle.v2;
					c = triangle.v0;
				}
				else
				{
					a = triangle.v2;
					b = triangle.v0;
					c = triangle.v1;
				}
				
				Vector3 bPrime = plane.LineIntersect(vertices[a].position, vertices[b].position);
				Vector3 cPrime = plane.LineIntersect(vertices[a].position, vertices[c].position);
				
				vertices.Add(new TextureVertex(bPrime, vertices[b].textureCoordinate));		// THIS CAUSES THE TEXTURE COORDINATES TO MOVE WHEN CLIPPED
				vertices.Add(new TextureVertex(cPrime, vertices[c].textureCoordinate));		// but the clipping doesn't really work anyway...
				
				clippedTriangles.Add(new Triangle(triangle.v0, vertices.Count-2, vertices.Count-1, triangle.color));
				return;		// the returned triangle has the vertices [A, B', C']
			}
			else if (d0 < 0 ^ d1 < 0 ^ d2 < 0)
			{
				// only one vertex is out
				// C is the vertex that's out
				// A and B are the other two
				int a, b, c;
				if (d0 < 0)
				{
					a = triangle.v1;
					b = triangle.v2;
					c = triangle.v0;
				}
				else if (d1 < 0)
				{
					a = triangle.v2;
					b = triangle.v0;
					c = triangle.v1;
				}
				else
				{
					a = triangle.v0;
					b = triangle.v1;
					c = triangle.v2;
				}
				
				Vector3 aPrime = plane.LineIntersect(vertices[a].position, vertices[c].position);
				Vector3 bPrime = plane.LineIntersect(vertices[b].position, vertices[c].position);
				
				vertices.Add(new TextureVertex(aPrime, vertices[c].textureCoordinate));	// THIS CAUSES THE TEXTURE COORDINATES TO MOVE WHEN CLIPPED
				vertices.Add(new TextureVertex(bPrime, vertices[c].textureCoordinate));	// but the clipping doesn't really work anyway...
				
				clippedTriangles.Add(new Triangle(triangle.v0, triangle.v1, vertices.Count-2, triangle.color));
				clippedTriangles.Add(new Triangle(vertices.Count-2, triangle.v1, vertices.Count-1, triangle.color));
				return;		// the returned triangles has the vertices [A, B, A'] and [A', B, B'] respectively
			}
		}
	}
}