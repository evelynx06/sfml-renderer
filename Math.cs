using SFML.Graphics;

namespace Engine
{
	namespace Math
	{
		/// <summary>
		/// A 4x4 matrix
		/// </summary>
		public class Matrix
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
			
			public static Matrix Identity
			{
				get { return new Matrix(1, 0, 0, 0,
										0, 1, 0, 0,
										0, 0, 1, 0,
										0, 0, 0, 1); }
			}
			
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

			public override string ToString()
			{
				return $"[{m11}; {m12}; {m13}; {m14};\n {m21}; {m22}; {m23}; {m24};\n {m31}; {m32}; {m33}; {m34};\n {m41}; {m42}; {m43}; {m44}]";
			}
		}
		
		/// <summary>
		/// A 4-dimensional vector.
		/// </summary>
		public class Vector4 : Vector3
		{
			public float w;
			
			// public Vector3 Vector3 { get => new(x, y, z); }
			// public Vector2 Vector2 { get => new(x, y); }
			
			
			public Vector4()
			{ }
			
			public Vector4(Vector3 v, float w)
			{
				this.x = v.x;
				this.y = v.y;
				this.z = v.z;
				this.w = w;
			}
			
			public Vector4(Vector2 v, float z, float w)
			{
				this.x = v.x;
				this.y = v.y;
				this.z = z;
				this.w = w;
			}
			
			public Vector4(float x, float y, float z, float w)
			{
				this.x = x;
				this.y = y;
				this.z = z;
				this.w = w;
			}
			
			public new float this[int i]
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
			
			public override string ToString()
			{
				return $"[{x}; {y}; {z}; {w}]";
			}
			
			public static Vector4 operator +(Vector4 left, Vector4 right)
			{
				return new Vector4(left.x + right.x, left.y + right.y, left.z + right.z, left.w + right.w);
			}
			
			public static Vector4 operator -(Vector4 left, Vector4 right)
			{
				return new Vector4(left.x - right.x, left.y - right.y, left.z - right.z, left.w - right.w);
			}
			
			public static Vector4 operator *(Vector4 v, float factor)
			{
				return new Vector4(v.x * factor, v.y * factor, v.z * factor, v.w * factor);
			}
			
			public static Vector4 operator *(float factor, Vector4 v)
			{
				return new Vector4(v.x * factor, v.y * factor, v.z * factor, v.w * factor);
			}
			
			public static Vector4 operator *(Matrix mat, Vector4 vec)
			{
				Vector4 result = new();
				
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						result[i] += mat[i, j] * vec[j];
					}
				}
				
				return result;
			}
			
			public static Vector4 operator /(Vector4 v, float scalar)
			{
				return new Vector4(v.x/scalar, v.y/scalar, v.z/scalar, v.w/scalar);
			}
			
			public static float Dot(Vector4 left, Vector4 right)
			{
				return left.x*right.x + left.y*right.y + left.z*right.z + left.w*right.w;
			}
			
			public float Dot(Vector4 other)
			{
				return this.x*other.x + this.y*other.y + this.z*other.z + this.w*other.w;
			}
			
			// public static Vector4 Cross(Vector4 v1, Vector4 v2)
			// {
			// 	return new Vector4(v1.y*v2.z - v1.z*v2.y,
			// 					  v1.z*v2.x - v1.x*v2.z,
			// 					  v1.x*v2.y - v1.y*v2.x,
			// 					  v1.w);
			// }
			
			// public static float Distance(Vector4 v1, Vector4 v2)
			// {
			// 	Vector4 a = v1/v1.w;
			// 	Vector4 b = v2/v2.w;
			// 	return MathF.Abs(MathF.Sqrt((b.x-a.x)*(b.x-a.x) + (b.y-a.y)*(b.y-a.y) + (b.z-a.z)*(b.z-a.z)));
			// }
			
			// public Vector4 Normalized()
			// {
			// 	float magnitude = MathF.Sqrt(x*x + y*y + z*z);
			// 	return new Vector4(x/magnitude, y/magnitude, z/magnitude, w);
			// }
		}
		
		/// <summary>
		/// A 3-dimensional vector.
		/// </summary>
		public class Vector3 : Vector2
		{
			public float z;
			
			public Vector3()
			{ }
			
			// public Vector3(Vector4 v)
			// {
			// 	this.x = v.x;
			// 	this.y = v.y;
			// 	this.z = v.z;
			// }
			
			public Vector3(Vector2 v, float z)
			{
				this.x = v.x;
				this.y = v.y;
				this.z = z;
			}
			
			public Vector3(float x, float y, float z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}
			
			public new float this[int i]
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
					default:
						throw new IndexOutOfRangeException();
				}
			}
			
			public override string ToString()
			{
				return $"[{x}; {y}; {z}]";
			}
			
			public static Vector3 operator +(Vector3 left, Vector3 right)
			{
				return new Vector3(left.x + right.x, left.y + right.y, left.z + right.z);
			}
			
			public static Vector3 operator +(Vector3 left, Vector4 right)
			{
				return new Vector3(left.x + right.x, left.y + right.y, left.z + right.z);
			}
			
			public static Vector3 operator -(Vector3 left, Vector3 right)
			{
				return new Vector3(left.x - right.x, left.y - right.y, left.z - right.z);
			}
			
			public static Vector3 operator -(Vector3 left, Vector4 right)
			{
				return new Vector3(left.x - right.x, left.y - right.y, left.z - right.z);
			}
			
			public static Vector3 operator *(Vector3 v, float factor)
			{
				return new Vector3(v.x * factor, v.y * factor, v.z * factor);
			}
			
			public static Vector3 operator *(float factor, Vector3 v)
			{
				return new Vector3(v.x * factor, v.y * factor, v.z * factor);
			}
			
			public static Vector3 operator /(Vector3 v, float factor)
			{
				return new Vector3(v.x/factor, v.y/factor, v.z/factor);
			}
			
			public static float Dot(Vector3 left, Vector3 right)
			{
				return left.x*right.x + left.y*right.y + left.z*right.z;
			}
			
			public float Dot(Vector3 other)
			{
				return this.x*other.x + this.y*other.y + this.z*other.z;
			}
			
			public static Vector3 Cross(Vector3 left, Vector3 right)
			{
				return new Vector3(left.y*right.z - left.z*right.y,
								  left.z*right.x - left.x*right.z,
								  left.x*right.y - left.y*right.x);
			}
			
			public static float Distance(Vector3 left, Vector3 right)
			{
				return MathF.Abs(MathF.Sqrt((right.x-left.x)*(right.x-left.x) + (right.y-left.y)*(right.y-left.y) + (right.z-left.z)*(right.z-left.z)));
			}
			
			public Vector3 Normalized()
			{
				float magnitude = MathF.Sqrt(x*x + y*y + z*z);
				return new Vector3(x/magnitude, y/magnitude, z/magnitude);
			}
			
			public Vector3 InterpolateTo(Vector3 dest, float alpha)
			{
				return this + (dest - this) * alpha;
			}
		}
		
		/// <summary>
		/// A 2-dimensional vector.
		/// </summary>
		public class Vector2
		{
			public float x;
			public float y;
			
			// public Vector2(Vector4 v)
			// {
			// 	this.x = v.x;
			// 	this.y = v.y;
			// }
			
			// public Vector2(Vector3 v)
			// {
			// 	this.x = v.x;
			// 	this.y = v.y;
			// }
			
			public Vector2()
			{ }
			
			public Vector2(float x, float y)
			{
				this.x = x;
				this.y = y;
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
					default:
						throw new IndexOutOfRangeException();
				}
			}
			
			public override string ToString()
			{
				return $"[{x}; {y}]";
			}
			
			public static Vector2 operator +(Vector2 left, Vector2 right)
			{
				return new Vector2(left.x + right.x, left.y + right.y);
			}
			
			public static Vector2 operator -(Vector2 left, Vector2 right)
			{
				return new Vector2(left.x - right.x, left.y - right.y);
			}
			
			public static Vector2 operator *(float factor, Vector2 v)
			{
				return new Vector2(v.x * factor, v.y * factor);
			}
			
			public static Vector2 operator *(Vector2 v, float factor)
			{
				return new Vector2(v.x * factor, v.y * factor);
			}
			
			public static Vector2 operator /(Vector2 v, float factor)
			{
				return new Vector2(v.x/factor, v.y/factor);
			}
			
			public Vector2 InterpolateTo(Vector2 dest, float alpha)
			{
				return this + (dest - this) * alpha;
			}
		}
		
		/// <summary>
		/// A plane in 3D space, defined by a normal vector (automatically normalised) and a distance from the origin.
		/// </summary>
		internal class Plane
		{
			internal Vector3 normal;
			internal float distance;
			
			internal Plane(Vector3 normal, float distance)
			{
				this.normal = normal.Normalized();
				this.distance = distance;
			}
			
			/// <summary>
			/// Calculate the signed distance to a point from the plane.
			/// </summary>
			/// <param name="vertex">The point to calculate the distance to.</param>
			internal float SignedDistance(Vector3 vertex)
			{
				return Vector3.Dot(normal, vertex.Normalized()) - distance;
			}
			
			/// <summary>
			/// Calculate the point at which a specified line segment intersects the plane.
			/// </summary>
			/// <param name="pointA">First end of the line segment.</param>
			/// <param name="pointB">Second end of the line segment.</param>
			internal Vector3 LineIntersect(Vector3 pointA, Vector3 pointB)
			{	
				float t = (-distance - Vector3.Dot(normal, pointA)) / Vector3.Dot(normal, pointB-pointA);
				return pointA + t*(pointB-pointA);
			}
		}
		
		/// <summary>
		/// A triangle containing the indexes of its vertices and a color value.
		/// </summary>
		public class Triangle
		{
			public int v0;
			public int v1;
			public int v2;
			public Color color;
			public Vector3 normal;
			
			
			public Triangle(int v0, int v1, int v2, Color color, Vector3 normal)
			{
				this.v0 = v0;
				this.v1 = v1;
				this.v2 = v2;
				this.color = color;
				this.normal = normal;
			}

			public override string ToString()
			{
				return $"[{v0}, {v1}, {v2}, {color}]";
			}
		}
	}
}