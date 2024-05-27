namespace Engine
{
	namespace Math
	{
		/// <summary>
		/// A 4x4 matrix
		/// </summary>
		class Matrix
		{
			public float m11;
			public float m12;
			public float m13;
			public float m14;
			public float m21;
			public float m22;
			public float m23;
			public float m24;
			public float m31;
			public float m32;
			public float m33;
			public float m34;
			public float m41;
			public float m42;
			public float m43;
			public float m44;
			
			
			public Matrix()
			{ }
			
			public Matrix(float m11, float m12, float m13, float m14,
						  float m21, float m22, float m23, float m24,
						  float m31, float m32, float m33, float m34,
						  float m41, float m42, float m43, float m44)
			{
				this.m11 = m11;
				this.m12 = m12;
				this.m13 = m13;
				this.m14 = m14;
				this.m21 = m21;
				this.m22 = m22;
				this.m23 = m23;
				this.m24 = m24;
				this.m31 = m31;
				this.m32 = m32;
				this.m33 = m33;
				this.m34 = m34;
				this.m41 = m41;
				this.m42 = m42;
				this.m43 = m43;
				this.m44 = m44;
			}
			
			public float this[int x, int y]
			{
				get => GetValue(x, y);
				set => SetValue(x, y, value);
			}
			
			private float GetValue(int x, int y)
			{
				return (x, y) switch
				{
					(0, 0) => m11,
					(0, 1) => m12,
					(0, 2) => m13,
					(0, 3) => m14,
					(1, 0) => m21,
					(1, 1) => m22,
					(1, 2) => m23,
					(1, 3) => m24,
					(2, 0) => m31,
					(2, 1) => m32,
					(2, 2) => m33,
					(2, 3) => m34,
					(3, 0) => m41,
					(3, 1) => m42,
					(3, 2) => m43,
					(3, 3) => m44,
					_ => throw new IndexOutOfRangeException(),
				};
			}
			
			private void SetValue(int x, int y, float value)
			{
				switch (x, y)
				{
					case (0, 0):
						m11 = value;
						break;
					case (0, 1):
						m12 = value;
						break;
					case (0, 2):
						m13 = value;
						break;
					case (0, 3):
						m14 = value;
						break;
					case (1, 0):
						m21 = value;
						break;
					case (1, 1):
						m22 = value;
						break;
					case (1, 2):
						m23 = value;
						break;
					case (1, 3):
						m24 = value;
						break;
					case (2, 0):
						m31 = value;
						break;
					case (2, 1):
						m32 = value;
						break;
					case (2, 2):
						m33 = value;
						break;
					case (2, 3):
						m34 = value;
						break;
					case (3, 0):
						m41 = value;
						break;
					case (3, 1):
						m42 = value;
						break;
					case (3, 2):
						m43 = value;
						break;
					case (3, 3):
						m44 = value;
						break;
					default:
						throw new IndexOutOfRangeException();
				}
			}
			
			public static Matrix operator *(Matrix m1, Matrix m2)
			{
				Matrix result = new();
				
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						for (int k = 0; k < 4; k++)
						{
							result[i, j] += m1[i, k] * m2[k, j];
						}
					}
				}
				
				return result;
			}
			
			public Matrix Transposed()
			{
				Matrix result = new();
				
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						result[i, j] = this[j, i];
					}
				}
				
				return result;
			}
			
			public static Matrix Identity()
			{
				return new Matrix(1, 0, 0, 0,
								  0, 1, 0, 0,
								  0, 0, 1, 0,
								  0, 0, 0, 1);
			}

			public override string ToString()
			{
				return $"[{m11}; {m12}; {m13}; {m14};\n {m21}; {m22}; {m23}; {m24};\n {m31}; {m32}; {m33}; {m34};\n {m41}; {m42}; {m43}; {m44}]";
			}
		}
		
		/// <summary>
		/// A point (or a vector) in 3D space, expressed in homogenous coordinates (w=1 for point, w=0 for vector)
		/// </summary>
		class Vector
		{
			public float x;
			public float y;
			public float z;
			public float w;
			
			
			public Vector()
			{ }
			
			public Vector(float x, float y, float z, float w)
			{
				this.x = x;
				this.y = y;
				this.z = z;
				this.w = w;
			}
			
			public float this[int i]
			{
				get => GetValue(i);
				set => SetValue(i, value);
			}
			
			private float GetValue(int index)
			{
				return index switch
				{
					0 => x,
					1 => y,
					2 => z,
					3 => w,
					_ => throw new IndexOutOfRangeException(),
				};
			}
			
			private void SetValue(int index, float value)
			{
				switch (index)
				{
					case 0:
						x = value;
						break;
					case 1:
						y = value;
						break;
					case 2:
						z = value;
						break;
					case 3:
						w = value;
						break;
					default:
						throw new IndexOutOfRangeException();
				}
			}
			
			public static Vector operator +(Vector v1, Vector v2)
			{
				return new Vector(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
			}
			
			public static Vector operator -(Vector v1, Vector v2)
			{
				return new Vector(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
			}
			
			public static Vector operator *(Vector v, float scalar)
			{
				return new Vector(v.x * scalar, v.y * scalar, v.z * scalar, v.w * scalar);
			}
			
			public static Vector operator *(float scalar, Vector v)
			{
				return new Vector(v.x * scalar, v.y * scalar, v.z * scalar, v.w * scalar);
			}
			
			public static Vector operator *(Matrix mat, Vector vec)
			{
				Vector result = new();
				
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						result[i] += mat[i, j] * vec[j];
					}
				}
				
				return result;
			}
			
			public static Vector operator /(Vector v, float scalar)
			{
				return new Vector(v.x/scalar, v.y/scalar, v.z/scalar, v.w/scalar);
			}
			
			public static float Dot(Vector v1, Vector v2)
			{
				return v1.x*v2.x + v1.y*v2.y + v1.z*v2.z + v1.w*v2.w;
			}
			
			public float Dot(Vector other)
			{
				return this.x*other.x + this.y*other.y + this.z*other.z + this.w*other.w;
			}
			
			public static float Distance(Vector v1, Vector v2)
			{
				Vector a = v1/v1.w;
				Vector b = v2/v2.w;
				return MathF.Abs(MathF.Sqrt((b.x-a.x)*(b.x-a.x) + (b.y-a.y)*(b.y-a.y) + (b.z-a.z)*(b.z-a.z)));
			}
			
			public Vector Normalized()
			{
				float magnitude = MathF.Sqrt(x*x + y*y + z*z);
				return new Vector(x/magnitude, y/magnitude, z/magnitude, w);
			}

			public override string ToString()
			{
				return $"[{x}; {y}; {z}; {w}]";
			}
		}
		
		/// <summary>
		/// A plane in 3D space, defined by a normal vector (automatically normalised) and a distance from the origin.
		/// </summary>
		class Plane
		{
			public Vector normal;
			public float distance;
			
			public Plane(Vector normal, float distance)
			{
				this.normal = normal.Normalized();
				this.distance = distance;
			}
			
			/// <summary>
			/// Calculate the signed distance to a point from the plane.
			/// </summary>
			/// <param name="vertex">The point to calculate the distance to.</param>
			public float SignedDistance(Vector vertex)
			{
				return Vector.Dot(normal, vertex) + distance;
			}
			
			/// <summary>
			/// Calculate the point at which a specified line segment intersects the plane.
			/// </summary>
			/// <param name="pointA">First end of the line segment.</param>
			/// <param name="pointB">Second end of the line segment.</param>
			public Vector LineIntersect(Vector pointA, Vector pointB)
			{	
				float t = (-distance - Vector.Dot(normal, pointA)) / Vector.Dot(normal, pointB-pointA);
				return pointA + t*(pointB-pointA);
			}
		}
		
		/// <summary>
		/// A point to be drawn on the screen.
		/// </summary>
		class Point
		{
			public float x;
			public float y;
			public float z;
			
			public Point(float x, float y, float depth)
			{
				this.x = x;
				this.y = y;
				this.z = depth;
			}
			
			public override string ToString()
			{
				return $"[{x}; {y}; {z}]";
			}
		}
	}
}