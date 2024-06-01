using Engine.Math;
using Engine.Objects;
// using SFML.Graphics;
// using SFML.Graphics;
// using SFML.Graphics;

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
		internal float zNear;
		internal float zFar;
		internal Plane[] clippingPlanes;
		
		private Matrix projectionMatrix;
		private float aspectRatio;
		private float fovFactor;
		
		
		internal bool fillTriangles = false;
		internal bool useShading = true;
		
		
		private void SetCanvasSize(int? width=null, int? height=null)
		{
			canvasWidth = width ?? canvasWidth;
			canvasHeight = height ?? canvasHeight;
			aspectRatio = canvasWidth/canvasHeight;
			projectionMatrix[0, 0] = aspectRatio*fovFactor;
		}
		
		internal Renderer(int canvasWidth, int canvasHeight, float fov = 90.0f, float zNear=0.1f, float zFar = 1000.0f)
		{
			this.canvasWidth = canvasWidth;
			this.canvasHeight = canvasHeight;
			this.fov = fov*(MathF.PI/180);	// convert to radians
			viewportWidth = 2;
			viewportHeight = 2;
			this.zNear = zNear;
			this.zFar = zFar;
			this.clippingPlanes = new Plane[]{new Plane(new Vector3( 0, 0, 1), -zNear),	// near
											  new Plane(new Vector3( 1, 0, 1), 0),		// left
											  new Plane(new Vector3(-1, 0, 1), 0),		// right
											  new Plane(new Vector3( 0, 1, 1), 0),		// bottom
											  new Plane(new Vector3( 0,-1, 1), 0)};		// top
											  
			aspectRatio = canvasWidth > canvasHeight ? canvasWidth/canvasHeight : canvasHeight/canvasWidth;
			fovFactor = 1/MathF.Tan(this.fov/2);
			
			projectionMatrix = new(aspectRatio*fovFactor, 0         , 0          , 0                      ,
								   0                    , -fovFactor, 0         , 0                      ,
								   0                    , 0         , zFar/(zFar-zNear), -zNear*zFar/(zFar-zNear),
								   0                    , 0         , 1          , 0                      );
		}
		
		internal void RenderScene(Scene scene, ref Canvas canvas)
		{
			Matrix cameraMatrix = scene.camera.Orientation.Transposed() * Methods.MakeTranslationMatrix(-1 * scene.camera.position);
			Vector3 worldLight = scene.worldLight.Normalized(); // (cameraMatrix * new Vector4(scene.worldLight, 1)).Normalized();
			
			foreach (Instance o in scene.objects)
			{
				Matrix transform = cameraMatrix * o.Transform;
				
				
				Model? clipped = TransformAndClip(o.model, o.scale, transform);
				if (clipped != null)
				{
					RenderModel(clipped, worldLight, canvas);
				}
			}
		}
		
		internal void RenderModel(Model model, Vector3 worldLight, Canvas canvas)
		{
			Vertex[] verticesCopy = new Vertex[model.vertices.Length];
			bool[] isProjected = new bool[model.vertices.Length];
			
			if (useShading)
			{
				foreach (Triangle t in model.triangles)
				{
					if (!BackfaceCullTriangle(t, model.vertices))
					{
						Vector3 triangleNormal = Vector3.Cross(model.vertices[t.v1].pos - model.vertices[t.v0].pos, model.vertices[t.v2].pos - model.vertices[t.v0].pos).Normalized();
						
						float shade = (Vector3.Dot(worldLight, triangleNormal) + 1) / 2;	// converted to value between 0 and 1
						
						if (!isProjected[t.v0]) { verticesCopy[t.v0] = ProjectVertex(model.vertices[t.v0]); isProjected[t.v0] = true; }
						if (!isProjected[t.v1]) { verticesCopy[t.v1] = ProjectVertex(model.vertices[t.v1]); isProjected[t.v1] = true; }
						if (!isProjected[t.v2]) { verticesCopy[t.v2] = ProjectVertex(model.vertices[t.v2]); isProjected[t.v2] = true; }
						RenderTriangle(verticesCopy[t.v0], verticesCopy[t.v1], verticesCopy[t.v2], canvas, t.color, shade, model.texture);
					}
				}
			}
			else
			{
				foreach (Triangle t in model.triangles)
				{
					if (!BackfaceCullTriangle(t, model.vertices))
					{	
						if (!isProjected[t.v0]) { verticesCopy[t.v0] = ProjectVertex(model.vertices[t.v0]); isProjected[t.v0] = true; }
						if (!isProjected[t.v1]) { verticesCopy[t.v1] = ProjectVertex(model.vertices[t.v1]); isProjected[t.v1] = true; }
						if (!isProjected[t.v2]) { verticesCopy[t.v2] = ProjectVertex(model.vertices[t.v2]); isProjected[t.v2] = true; }
						RenderTriangle(verticesCopy[t.v0], verticesCopy[t.v1], verticesCopy[t.v2], canvas, t.color, 1, model.texture);
					}
				}
			}
			
		}
		
		private static bool BackfaceCullTriangle(Triangle t, Vertex[] vertices)
		{
			// N = (B - A) x (C - A)
			// doesn't really need to be normalised, even though it's a normal
			Vector3 triangleNormal = Vector3.Cross(vertices[t.v1].pos - vertices[t.v0].pos, vertices[t.v2].pos - vertices[t.v0].pos);
			
			// V = camPos - A	(camPos is [0, 0, 0])
			Vector3 triangleToCamera = -1 * vertices[t.v2].pos;
			
			return Vector3.Dot(triangleToCamera, triangleNormal) <= 0;
		}
		
		private void RenderTriangle(Vertex v0, Vertex v1, Vertex v2, Canvas canvas, SFML.Graphics.Color color, float shade, SFML.Graphics.Image? texture)
		{
			if (fillTriangles)
			{
				if (v0.tc != null && v1.tc != null && v2.tc != null && texture != null)
				{
					canvas.DrawTriangleTex(v0, v1, v2, texture, shade);
					// canvas.DrawWireTriangle(projected[t.v0].pos, projected[t.v1].pos, projected[t.v2].pos, t.color);
				}
				else
				{
					canvas.DrawTriangle(v0.pos, v1.pos, v2.pos, color, shade);
					// canvas.DrawWireTriangle(projected[t.v0].pos, projected[t.v1].pos, projected[t.v2].pos, t.color);
				}
			}
			else
			{
				canvas.DrawWireTriangle(v0.pos, v1.pos, v2.pos, color);
			}
		}
		
		private Vertex ProjectVertex(Vertex v)
		{
			Vector4 projected;
			// perspective projection
			if (v.pos.z == 0)
			{
				projected = new(0, 0, 0, 1);
			}
			else
			{
				// x = v.pos.x * projectionPlaneZ/v.pos.z;
				// y = v.pos.y * projectionPlaneZ/v.pos.z;
				projected = projectionMatrix * new Vector4(v.pos, 1);
				projected /= projected.w;
			}
			
			projected.x += 1.0f;
			projected.y += 1.0f;
			// viewport to canvas
			projected.x *= canvasWidth/2;
			projected.y *= canvasHeight/2;
			
			return new Vertex(projected, v.tc);
		}
		
		private Model? TransformAndClip(Model model, float scale, Matrix transform)
		{
			Vector4 center = transform * model.FindCenter();
			float radius = scale * model.boundsRadius;
			
			int tmp = 0;
			
			foreach (Plane p in clippingPlanes)
			{
				float d = p.SignedDistance(center);
				if (d < -radius)
				{
					return null;
				}
				else if (d > radius)
				{
					tmp++;
				}
			}
			
			List<Vertex> vertices = new();
			foreach (Vertex v in model.vertices)
			{
				vertices.Add(new Vertex(transform * new Vector4(v.pos, 1), v.tc));
			}
			
			// return new Model(vertices.ToArray(), model.triangles, model.texture, model.boundsRadius);
			
			if (tmp == clippingPlanes.Length)
			{
				return new Model(vertices.ToArray(), model.triangles, model.texture, model.boundsRadius);
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
			
			return new Model(vertices.ToArray(), triangles, model.texture, model.boundsRadius);
		}
		
		private static void ClipTriangle(Triangle triangle, Plane plane, ref List<Triangle> clippedTriangles, ref List<Vertex> vertices)
		{
			float d0 = plane.SignedDistance(vertices[triangle.v0].pos);
			float d1 = plane.SignedDistance(vertices[triangle.v1].pos);
			float d2 = plane.SignedDistance(vertices[triangle.v2].pos);
			
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
				
				Vector3 bPrime = plane.LineIntersect(vertices[a].pos, vertices[b].pos);
				Vector3 cPrime = plane.LineIntersect(vertices[a].pos, vertices[c].pos);


				vertices.Add(new Vertex(bPrime, vertices[b].tc));	// THIS CAUSES THE TEXTURE COORDINATES TO MOVE WHEN CLIPPED
				vertices.Add(new Vertex(cPrime, vertices[c].tc));		// but the clipping doesn't really work anyway...
				
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
				
				Vector3 aPrime = plane.LineIntersect(vertices[a].pos, vertices[c].pos);
				Vector3 bPrime = plane.LineIntersect(vertices[b].pos, vertices[c].pos);
				
				vertices.Add(new Vertex(aPrime, vertices[c].tc));	// THIS CAUSES THE TEXTURE COORDINATES TO MOVE WHEN CLIPPED
				vertices.Add(new Vertex(bPrime, vertices[c].tc));	// but the clipping doesn't really work anyway...
				
				clippedTriangles.Add(new Triangle(triangle.v0, triangle.v1, vertices.Count-2, triangle.color));
				clippedTriangles.Add(new Triangle(vertices.Count-2, triangle.v1, vertices.Count-1, triangle.color));
				return;		// the returned triangles has the vertices [A, B, A'] and [A', B, B'] respectively
			}
		}
	}
}