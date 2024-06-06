using Engine.Math;
using Engine.Objects;
using SFML.Graphics;

namespace Engine
{
	class Renderer
	{
		public int canvasWidth;
		public int canvasHeight;
		public int viewportWidth;
		public int viewportHeight;
		public float projectionPlaneZ;
		public Plane[] clippingPlanes;
		
		public bool fillTriangles = false;
		public bool useBackfaceCulling = true;
		
		public Renderer(int canvasWidth, int canvasHeight, int viewportWidth=1, int viewportHeight=1, float projectionPlaneZ=1)
		{
			this.canvasWidth = canvasWidth;
			this.canvasHeight = canvasHeight;
			this.viewportWidth = viewportWidth;
			this.viewportHeight = viewportHeight;
			this.projectionPlaneZ = projectionPlaneZ;
			this.clippingPlanes = new Plane[]{new Plane(new Vector( 0, 0, 1, 0), -projectionPlaneZ),	// near
											  new Plane(new Vector( 1, 0, 1, 0), 0),		// left
											  new Plane(new Vector(-1, 0, 1, 0), 0),		// right
											  new Plane(new Vector( 0, 1, 1, 0), 0),		// bottom
											  new Plane(new Vector( 0,-1, 1, 0), 0)};		// top
		}
		
		public void RenderScene(Scene scene, ref Canvas canvas)
		{
			// everything is centered around (0, 0), so it has to be transformed to camera space
			// this is done by transforming it with the inverse of the camera's position and rotation
			// (the inverse of a rotation matrix is obtained by just transposing the original matrix)
			Matrix cameraMatrix = scene.camera.Orientation.Transposed() * Methods.MakeTranslationMatrix(-1 * scene.camera.position);
			
			foreach (Instance o in scene.objects)
			{
				Matrix transform = cameraMatrix * o.Transform;	// make camera and instance transforms into one matrix
				
				Model? clipped = TransformAndClip(clippingPlanes, o.model, o.scale, transform);
				if (clipped != null)
				{
					RenderModel(clipped, canvas);
				}
			}
		}
		
		public void RenderModel(Model model, Canvas canvas)
		{
			Point[] projected = new Point[model.vertices.Length];
			// projects all vertices
			for (int i = 0; i < projected.Length; i++)
			{
				projected[i] = ProjectVertex(model.vertices[i]);
			}
			
			
			if (useBackfaceCulling)	// backface culling is toggled with the right shift key
			{
				foreach (Triangle t in model.triangles)
				{
					if (!BackfaceCullTriangle(t, model.vertices))	// if the triangle is not backface culled, render it
					{
						RenderTriangle(t, projected, canvas);
					}
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
		
		public bool BackfaceCullTriangle(Triangle t, Vector[] vertices)
		{
			// calculate the triangle normal direction
			// this is dependent on the order the triangle vertices are defined in
			// here, if the the vertices are going counter-clockwise, the front is facing you
			// N = (B - A) x (C - A)
			Vector triangleNormal = Vector.Cross(vertices[t.v1] - vertices[t.v0], vertices[t.v2] - vertices[t.v0]);
			
			// calculate vector from triangle to camera
			// V = camPos - A	(camPos is [0, 0, 0])
			Vector triangleToCamera = -1 * vertices[t.v2];
			
			// compare the directions of the two vectors
			// if the angle between two vectors is more than 90 degrees, the dot product of those vectors is negative and vice versa
			// returns true if the dot pruduct is less than 0, and therefore the triangle should be culled (not rendered)
			return Vector.Dot(triangleToCamera, triangleNormal) <= 0;
		}
		
		public void RenderTriangle(Triangle t, Point[] projected, Canvas canvas)
		{
			if (fillTriangles)	// fill triangles is toggled with the enter key
			{
				canvas.DrawFilledTriangle(projected[t.v0], projected[t.v1], projected[t.v2], t.color);
			}
			else
			{
				canvas.DrawWireTriangle(projected[t.v0], projected[t.v1], projected[t.v2], t.color);
			}
		}
		
		public Point ProjectVertex(Vector v)
		{
			float x;
			float y;
			
			// perspective projection
			if (v.z == 0)	// avoid divide by zero
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
		
		public static Model? TransformAndClip(Plane[] planes, Model model, float scale, Matrix transform)
		{
			Vector center = transform * model.boundsCenter;	// apply instance and camera transform to center point
			float radius = scale * model.boundsRadius;	// scale model radius according to instance scale
			
			int tmp = 0;	// count to see if object is in front of all clipping planes
			
			foreach (Plane p in planes)
			{
				float d = p.SignedDistance(center);	// distance (signed) from point center to closest point on plane p
				if (d < -radius)	// object is completely behind clipping plane
				{
					// return nothing
					return null;
				}
				else if (d > radius)	// object is completely in front of clipping plane
				{
					tmp++;
				}
			}
			
			
			List<Vector> vertices = new();
			foreach (Vector v in model.vertices)	// apply instance and camera transform to all vertices
			{
				vertices.Add(transform * v);
			}
			
			
			if (tmp == planes.Length)	// object is completely in front of all clipping planes
			{
				// return transformed model
				return new Model(vertices.ToArray(), model.triangles, radius);
			}
			
			
			Triangle[] triangles = model.triangles;
			foreach (Plane p in planes)
			{
				// clip each triangle against the plane and then update the triangle and vertices lists
				List<Triangle> clippedTriangles = new();
				foreach (Triangle t in triangles)
				{
					ClipTriangle(t, p, ref clippedTriangles, ref vertices);
				}
				triangles = clippedTriangles.ToArray();
			}
			
			// return transformed and clipped model
			return new Model(vertices.ToArray(), triangles, radius);
		}
		
		public static void ClipTriangle(Triangle triangle, Plane plane, ref List<Triangle> clippedTriangles, ref List<Vector> vertices)
		{
			float d0 = plane.SignedDistance(vertices[triangle.v0]);
			float d1 = plane.SignedDistance(vertices[triangle.v1]);
			float d2 = plane.SignedDistance(vertices[triangle.v2]);
			
			if (d0 >= 0 && d1 >= 0 && d2 >= 0)	// all vertices are in front of plane
			{
				// return triangle as-is
				clippedTriangles.Add(triangle);
				return;
			}
			else if (d0 < 0 && d1 < 0 && d2 < 0)	// all vertices are behind plane
			{
				// return nothing
				return;
			}
			else if (d0 >= 0 ^ d1 >= 0 ^ d2 >= 0)	// one vertex is in front of plane, two behind
			{
				// THIS DOES NOT WORK PROPERLY
				
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
				
				Vector bPrime = plane.LineIntersect(vertices[a], vertices[b]);
				Vector cPrime = plane.LineIntersect(vertices[a], vertices[c]);
				
				vertices.Add(bPrime);
				vertices.Add(cPrime);
				
				clippedTriangles.Add(new Triangle(triangle.v0, vertices.Count-2, vertices.Count-1, triangle.color));
				return;		// the returned triangle has the vertices [A, B', C']
			}
			else if (d0 < 0 ^ d1 < 0 ^ d2 < 0)	// two vertices are in front of plane, one behind
			{
				// THIS DOES NOT WORK PROPERLY
				
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
				
				Vector aPrime = plane.LineIntersect(vertices[a], vertices[c]);
				Vector bPrime = plane.LineIntersect(vertices[b], vertices[c]);
				
				vertices.Add(aPrime);
				vertices.Add(bPrime);
				
				clippedTriangles.Add(new Triangle(triangle.v0, triangle.v1, vertices.Count-2, triangle.color));
				clippedTriangles.Add(new Triangle(vertices.Count-2, triangle.v1, vertices.Count-1, triangle.color));
				return;		// the returned triangles has the vertices [A, B, A'] and [A', B, B'] respectively
			}
		}
	}
}