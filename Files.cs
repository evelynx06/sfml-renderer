using Engine.Objects;
using Engine.Math;
using System.Text.RegularExpressions;
using EarClipperLib;
using System.Globalization;

namespace Engine
{
	public static class Files
	{
		private static bool IsFullPath(string path)
		{
			// rejects path if it's invalid or isn't rooted
			if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(Path.GetInvalidPathChars()) != -1 || !Path.IsPathRooted(path))
			{
				return false;
			}
			
			// since IsPathRooted(path) must have returned true to get here, GetPathRoot(path) will not be null
			string pathRoot = Path.GetPathRoot(path)!;
			
			// accepts any root longer than 2 (e.g "X:\", "\\UNC\PATH"), rejects anything shorter (e.g "", "\", "X:"),
			// BUT "/" is accepted, to support unix file paths
			if (pathRoot.Length <= 2 && pathRoot != "/")
			{
				return false;
			}
			
			if (pathRoot[0] != '\\' || pathRoot[1] != '\\')
			{
				return true;	// rooted and not a UNC path
			}
			
			// A UNC server name without a share name (e.g "\\NAME" or "\\NAME\") is invalid
			return pathRoot.Trim('\\').IndexOf('\\') != -1;
		}
		
		private static void ReadMtlFile(string path, ref List<Dictionary<string, string[]>> materials)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException();
			}
			Console.WriteLine($"Reading file '{path}'");
			
			string pathToDir = Regex.Match(path, @".*[\\/]").ToString();
			using StreamReader sr = new(path);
			Dictionary<string, string[]> materialValues = new();
			while (sr.Peek() > 0)
			{
				string? line = sr.ReadLine();
				if (line == null) { continue; }

				string[] values = line.Split(' ');

				switch (values[0])
				{
					case "#":
						break;
					case "":
						break;
					case "newmtl":
						if (materialValues.Count != 0)
						{
							materials.Add(materialValues);
							materialValues = new();
						}
						materialValues.Add("name", new[] { values[1] });
						Console.WriteLine($"Found material '{values[1]}'");
						break;
					case "Ka":
						materialValues.Add("Ka", new[] { values[1], values[2], values[3] });
						break;
					case "d":
						materialValues.Add("d", new[] { values[1] });
						break;
					case "map_Ka":
						string map_KaPath = string.Join(' ', values.Skip(1));
						materialValues.Add("map_Kd", new[] { IsFullPath(map_KaPath) ? map_KaPath : Path.Combine(pathToDir, map_KaPath) });
						break;
					case "map_Kd":
						string map_KdPath = string.Join(' ', values.Skip(1));
						materialValues.Add("map_Kd", new[] { IsFullPath(map_KdPath) ? map_KdPath : Path.Combine(pathToDir, map_KdPath) });
						break;
					default:
						// Console.WriteLine("Unsupported .mtl syntax '{0}'", values[0]);
						break;
				}
			}
			
			materials.Add(materialValues);
		}
		
		public static Model[] ReadObjFile(string path)
		{
			string fullPath = Path.GetFullPath(path);
			if (!File.Exists(fullPath))
			{
				throw new FileNotFoundException();
			}
			Console.WriteLine($"Reading file '{fullPath}'");
			
			List<Dictionary<string, string[]>> materials = new();
			List<string[]> vertices = new();
			List<string[]> vertexTextureCoordinates = new();
			List<string[]> vertexNormals = new();
			List<string[]> faces = new();
			
			using (StreamReader sr = new(fullPath))
			{
				while (sr.Peek() >= 0)	// sr.Peek() returns -1 when there are no characters left to read
				{
					string? line = sr.ReadLine();
					if (line == null) { continue; }
					
					string[] values = line.Split(' ');
					
					switch (values[0])
					{
						case "#":
							break;
						case "":
							break;
						case "mtllib":
							string mtlPath = string.Join(' ', values.Skip(1));
							string pathToDir = Regex.Match(fullPath, @".*[\\/]").ToString();
							ReadMtlFile(IsFullPath(mtlPath) ? mtlPath : Path.Combine(pathToDir, mtlPath), ref materials);
							break;
						case "usemtl":
							faces.Add(new string[] {"color", values[1]});
							break;
						case "o":
							vertices.Add(new string[] {"new", values[1]});
							vertexTextureCoordinates.Add(new string[] {"new", values[1]});
							vertexNormals.Add(new string[] {"new", values[1]});
							faces.Add(new string[] {"new", values[1]});
							Console.WriteLine($"Found object '{values[1]}'");
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
							// Console.WriteLine($"Unsupported .obj syntax '{values[0]}'");
							break;
					}
				}
			}
			
			Dictionary<string, SFML.Graphics.Color> colors = new();
			Dictionary<string, SFML.Graphics.Image> textures = new();
			
			CultureInfo format = new("en-US");
			foreach (Dictionary<string, string[]> material in materials)
			{
				byte r = 0xff;
				byte g = 0xff;
				byte b = 0xff;
				byte a = 0xff;
				
				if (material.ContainsKey("Ka"))
				{
					r = (byte)(Convert.ToSingle(material["Ka"][0], format)*0xff);
					g = (byte)(Convert.ToSingle(material["Ka"][0], format)*0xff);
					b = (byte)(Convert.ToSingle(material["Ka"][0], format)*0xff);
				}
				else if (material.ContainsKey("Kd"))
				{
					r = (byte)(Convert.ToSingle(material["Ka"][0], format)*0xff);
					g = (byte)(Convert.ToSingle(material["Ka"][0], format)*0xff);
					b = (byte)(Convert.ToSingle(material["Ka"][0], format)*0xff);
				}
				
				if (material.ContainsKey("d"))
				{
					a = (byte)(Convert.ToSingle(material["d"][0], format)*0xff);
				}
				
				if (material.ContainsKey("map_Ka"))
				{
					textures.Add(material["name"][0], new SFML.Graphics.Image(material["map_Ka"][0]));
				}
				else if (material.ContainsKey("map_Kd"))
				{
					textures.Add(material["name"][0], new SFML.Graphics.Image(material["map_Kd"][0]));
				}
				colors.Add(material["name"][0], new(r, g, b, a));
			}
				
			if (vertices[0][0] == "new")
			{
				vertices.RemoveAt(0);
				vertexTextureCoordinates.RemoveAt(0);
				faces.RemoveAt(0);
			}
			
			SFML.Graphics.Color[] basicColors = new SFML.Graphics.Color[] {new(0xff, 0x00, 0x00), new(0x00, 0xff, 0x00), new(0x00, 0x00, 0xff),
																		   new(0xff, 0x00, 0xff), new(0xff, 0xff, 0x00), new(0x00, 0xff, 0xff)};
			
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
						vertices.RemoveRange(0, i+1);
						objectsLeft = true;
						break;
					}
					else
					{
						objectVertices.Add(new Vector3(Convert.ToSingle(vertices[i][0], format),
													  Convert.ToSingle(vertices[i][1], format),
													  Convert.ToSingle(vertices[i][2], format)));
					}
				}
				
				for (int i = 0; i < vertexTextureCoordinates.Count; i++)
				{
					if (vertexTextureCoordinates[i][0] == "new")
					{
						vertices.RemoveRange(0, i+1);
						// objectsLeft = true;
						break;
					}
					else
					{
						objectTextureCoordinates.Add(new Vector2(Convert.ToSingle(vertexTextureCoordinates[i][0], format),
																 Convert.ToSingle(vertexTextureCoordinates[i][1], format)));
					}
				}
				
				string materialOverride = "";
				
				for (int i = 0; i < faces.Count; i++)
				{
					if (faces[i][0] == "new")
					{
						vertices.RemoveRange(0, i+1);
						// objectsLeft = true;
						break;
					}
					else if (faces[i][0] == "color")
					{
						materialOverride = faces[i][1];
					}
					else
					{
						string vPattern = @"^[0-9]*(?=/|$)";
						if (faces[i].Length == 3)
						{
							objectTriangles.Add(new Triangle(Convert.ToInt32(Regex.Match(faces[i][0], vPattern).ToString())-1,
															 Convert.ToInt32(Regex.Match(faces[i][1], vPattern).ToString())-1,
															 Convert.ToInt32(Regex.Match(faces[i][2], vPattern).ToString())-1,
															 materialOverride == "" ? basicColors[i%6] : colors[materialOverride]));
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
													materialOverride == "" ? basicColors[i%6] : colors[materialOverride]));
							}
							
						}
					}
				}
				
				Vertex[] textureVertices = new Vertex[objectVertices.Count];
				if (objectTextureCoordinates.Count == 0)
				{
					for (int i = 0; i < textureVertices.Length; i++)
					{
						textureVertices[i] = new(objectVertices[i], objectVertices[i]);
					}
				}
				else
				{
					for (int i = 0; i < textureVertices.Length; i++)
					{
						textureVertices[i] = new(objectVertices[i], objectTextureCoordinates[i]);
					}
				}
				
				Model model = new(textureVertices, objectTriangles.ToArray());
				
				if (materialOverride != "" && textures.ContainsKey(materialOverride))
				{
					model.texture = textures[materialOverride];
				}
				
				objects.Add(model);
			}
			
			Console.WriteLine($"Finished reading file '{Path.GetFileName(path)}'\n");
			return objects.ToArray();
		}
	}
}