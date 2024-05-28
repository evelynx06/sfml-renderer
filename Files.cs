using Engine.Objects;
using Engine.Math;
using System.Text.RegularExpressions;
using SFML.Graphics;
using EarClipperLib;
using System.Globalization;

namespace Engine
{
	public static class Files
	{
		public static Model[] ReadObjFile(string path)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException();
			}
			
			List<string[]> vertices = new();
			List<string[]> vertexTextureCoordinates = new();
			List<string[]> vertexNormals = new();
			List<string[]> faces = new();
			
			Console.WriteLine("Parsing .obj file...");
			
			using (StreamReader sr = new(path))
			{
				while (sr.Peek() >= 0)	// sr.Peek() returns -1 when there are no characters left to read
				{
					string? line = sr.ReadLine();
					if (line == null) { continue; }
					
					string[] values = line.Split(' ');
					
					switch (values[0])
					{
						case "o":
							vertices.Add(new string[] {"new", values[1]});
							vertexTextureCoordinates.Add(new string[] {"new", values[1]});
							vertexNormals.Add(new string[] {"new", values[1]});
							faces.Add(new string[] {"new", values[1]});
							break;
						case "v":
							vertices.Add(values.Skip(1).ToArray());
							break;
						case "vt":
							vertexTextureCoordinates.Add(values.Skip(1).ToArray());
							break;
						case "vn":
							vertexNormals.Add(values.Skip(1).ToArray());
							break;
						case "f":
							faces.Add(values.Skip(1).ToArray());
							break;
						default:
							Console.WriteLine("Unsupported .obj syntax '{0}'", values[0]);
							break;
					}
				}
			}
				
			if (vertices[0][0] == "new")
			{
				Console.WriteLine("Found object '{0}'", vertices[0][1]);
				vertices.RemoveAt(0);
				vertexTextureCoordinates.RemoveAt(0);
				faces.RemoveAt(0);
			}
			
			Color[] colors = new Color[] {Color.Red, Color.Green, Color.Blue, Color.Magenta, Color.Yellow, Color.Cyan};
			
			List<Model> objects = new();
			
			bool objectsLeft = true;
			while (objectsLeft)
			{
				objectsLeft = false;
				List<Vector3> objectVertices = new();
				List<Vector2> objectTextureCoordinates = new();
				List<Triangle> objectTriangles = new();
				
				for (int i = 0; i < vertices.Count; i++)
				{
					if (vertices[i][0] == "new")
					{
						Console.WriteLine("Found object '{0}'", vertices[0][1]);
						vertices.RemoveRange(0, i+1);
						objectsLeft = true;
						break;
					}
					else
					{
						CultureInfo format = new("en-US");
						objectVertices.Add(new Vector3(Convert.ToSingle(vertices[i][0], format),
													  Convert.ToSingle(vertices[i][1], format),
													  Convert.ToSingle(vertices[i][2], format)));
					}
				}
				
				for (int i = 0; i < vertexTextureCoordinates.Count; i++)
				{
					if (vertexTextureCoordinates[i][0] == "new")
					{
						// Console.WriteLine("Found object '{0}'", vertices[0][1]);
						vertices.RemoveRange(0, i+1);
						// objectsLeft = true;
						break;
					}
					else
					{
						CultureInfo format = new("en-US");
						objectTextureCoordinates.Add(new Vector2(Convert.ToSingle(vertexTextureCoordinates[i][0], format),
																 Convert.ToSingle(vertexTextureCoordinates[i][1], format)));
					}
				}
				
				for (int i = 0; i < faces.Count; i++)
				{
					if (faces[i][0] == "new")
					{
						// Console.WriteLine("Found object '{0}'", vertices[0][1]);
						vertices.RemoveRange(0, i+1);
						// objectsLeft = true;
						break;
					}
					else
					{
						string vPattern = @"^[0-9]*(?=/|$)";
						if (faces[i].Length == 3)
						{
							objectTriangles.Add(new Triangle(Convert.ToInt32(Regex.Match(faces[i][0], vPattern).ToString())-1,
															 Convert.ToInt32(Regex.Match(faces[i][1], vPattern).ToString())-1,
															 Convert.ToInt32(Regex.Match(faces[i][2], vPattern).ToString())-1,
															 colors[i%6]));
						}
						else
						{
							List<Vector3m> points = new() {};
							
							foreach (string vertexIndex in faces[i])
							{
								Vector3 vertex = objectVertices[Convert.ToInt32(Regex.Match(vertexIndex, vPattern).ToString())-1];
								points.Add(new Vector3m(vertex.x, vertex.y, vertex.z));
							}
							
							EarClipping earClipping = new();
							earClipping.SetPoints(points);
							earClipping.Triangulate();
							List<Vector3m> res = earClipping.Result;
							
							for (int p = 0; p < res.Count; p += 3)
							{
								// note the change in ordering from ccw to cw
								objectTriangles.Add(new Triangle(
													objectVertices.FindIndex(a => a.x == (float)res[p].X && a.y == (float)res[p].Y && a.z == (float)res[p].Z),
													objectVertices.FindIndex(a => a.x == (float)res[p+1].X && a.y == (float)res[p+1].Y && a.z == (float)res[p+1].Z),
													objectVertices.FindIndex(a => a.x == (float)res[p+2].X && a.y == (float)res[p+2].Y && a.z == (float)res[p+2].Z),
													colors[i%6]));
							}
							
						}
					}
				}
				
				TextureVertex[] textureVertices = new TextureVertex[objectVertices.Count];
				if (objectTextureCoordinates.Count == 0)
				{
					for (int i = 0; i < textureVertices.Length; i++)
					{
						textureVertices[i] = new(objectVertices[i], objectVertices[i].Vector2);
					}
				}
				else
				{
					for (int i = 0; i < textureVertices.Length; i++)
					{
						textureVertices[i] = new(objectVertices[i], objectTextureCoordinates[i]);
					}
				}
				
				
				objects.Add(new Model(textureVertices, objectTriangles.ToArray()));
			}
			
			return objects.ToArray();
		}
	}
}