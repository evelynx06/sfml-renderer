using CommunityToolkit.HighPerformance;
using SFML.Graphics;
using Engine.Math;

namespace Engine
{
	/// <summary>
	/// A canvas that handles drawing lines and triangles, with a depth buffer
	/// </summary>
	class Canvas
	{
		public Image screen;
		float[,] depthBuffer;
		int screenWidth;
		int screenHeight;
		
		public Canvas(uint width, uint height)
		{
			screen = new(width, height);
			depthBuffer = new float[width, height];
			depthBuffer.AsSpan2D().Fill(0);
			screenWidth = (int)width;
			screenHeight = (int)height;
		}
		
		private void PutPixel(float x, float y, float z, Color color)	// put a pixel on the screen, using depth buffer
		{
			// convert from coords with (0, 0) in the middle to screen coords with (0, 0) in the top left and with inverted y-axis
			int screen_x = Convert.ToInt32((screenWidth/2) + x);
			int screen_y = Convert.ToInt32((screenHeight/2) - y - 1);
			
			// ignore pixels outside the screen
			if (screen_x < 0 || screen_x >= screenWidth || screen_y < 0 || screen_y >= screenHeight)
			{
				return;
			}
			else if (z > depthBuffer[screen_x, screen_y])	// the z value passed in is actually 1/z
			{
				depthBuffer[screen_x, screen_y] = z;
				screen.SetPixel((uint)screen_x, (uint)screen_y, color);
			}
		}
		
		private void PutPixel(float x, float y, Color color)	// put a pixel on the screen, without using depth buffer
		{
			// convert from coords with (0, 0) in the middle to screen coords with (0, 0) in the top left and with inverted y-axis
			int screen_x = Convert.ToInt32((screenWidth/2) + x);
			int screen_y = Convert.ToInt32((screenHeight/2) - y - 1);
			
			// ignore pixels outside the screen
			if (screen_x < 0 || screen_x >= screenWidth || screen_y < 0 || screen_y >= screenHeight)
			{
				return;
			}
			
			screen.SetPixel((uint)screen_x, (uint)screen_y, color);
		}
		
		public void DrawWireTriangle(Point p0, Point p1, Point p2, Color color)
		{	
			DrawLine(p0, p1, color);
			DrawLine(p1, p2, color);
			DrawLine(p2, p0, color);
		}
		
		public void DrawFilledTriangle(Point p0, Point p1, Point p2, Color color)
		{	
			// sort the points so that y0 <= y1 <= y2
			// i.e. p2 will be the topmost point and p0 will be the bottommost
			if (p1.y < p0.y)
			{
				(p0, p1) = (p1, p0);	// swap
			}
			if (p2.y < p0.y)
			{
				(p0, p2) = (p2, p0);	// swap
			}
			if (p2.y < p1.y)
			{
				(p1, p2) = (p2, p1);	// swap
			}
			
			
			// interpolate all x values between p0 and p1
			float[] x01 = Interpolate(p0.y, p0.x, p1.y, p1.x);
			// interpolate all z values between p0 and p1 (actually 1/z)
			float[] z01 = Interpolate(p0.y, 1/p0.z, p1.y, 1/p1.z);
			
			// interpolate all x values between p1 and p2
			float[] x12 = Interpolate(p1.y, p1.x, p2.y, p2.x);
			// interpolate all z values between p1 and p2 (actually 1/z)
			float[] z12 = Interpolate(p1.y, 1/p1.z, p2.y, 1/p2.z);
			
			// interpolate all x values between p0 and p2
			float[] x02 = Interpolate(p0.y, p0.x, p2.y, p2.x);
			// interpolate all z values between p0 and p2 (actually 1/z)
			float[] z02 = Interpolate(p0.y, 1/p0.z, p2.y, 1/p2.z);
			
			// combine x01 and x12 into one list, removing the duplicate value in the middle
			float[] x012 = x01.SkipLast(1).Concat(x12).ToArray();
			// combine z01 and z12 into one list, removing the duplicate value in the middle
			float[] z012 = z01.SkipLast(1).Concat(z12).ToArray();
			
			
			// determine which side is left and right
			float[] x_left;
			float[] z_left;
			float[] x_right;
			float[] z_right;
			
			int m = x02.Length/2;
			if (x02[m] < x012[m])
			{	// if the x value at the midpoint of x02 is less than that of x012, x02 must be the left side
				x_left = x02;
				z_left = z02;
				
				x_right = x012;
				z_right = z012;
			}
			else
			{
				x_left = x012;
				z_left = z012;
				
				x_right = x02;
				z_right = z02;
			}
			
			
			for (int y = (int)p0.y; y <= (int)p2.y; y++)	// loop through all y values between p0 and p2
			{
				float x_l = x_left[y-(int)p0.y];	// get the left side x value at current y
				float x_r = x_right[y-(int)p0.y];	// get the right side x value at current y
				
				// interpolate the z values between x_l and x_r	(actually 1/z)
				// this is where 1/z matters, detailed info here: (https://www.gabrielgambetta.com/computer-graphics-from-scratch/12-hidden-surface-removal.html#why-1z-instead-of-z)
				float[] z_segment = Interpolate(x_l, z_left[y-(int)p0.y], x_r, z_right[y-(int)p0.y]);
				
				
				for (int x = (int)x_l; x < x_r; x++)	// loop through all x values between x_l and x_r
				{
					PutPixel(x, y, z_segment[x - (int)x_l], color);
				}
			}
		}
		
		public void DrawLine(Point p0, Point p1, Color color)
		{
			float dx = p1.x - p0.x;
			float dy = p1.y - p0.y;
			
			if (MathF.Abs(dx) > MathF.Abs(dy))	// line is horizontal-ish
			{
				if (dx < 0)	// make sure it's left to right
				{
					(p1, p0) = (p0, p1);	// swap
				}
				
				// interpolate all y values between p0 and p1
				float[] ys = Interpolate(p0.x, p0.y, p1.x, p1.y);
				
				for (int x = (int)p0.x; x <= (int)p1.x ; x++)	// loop through all x values between p0 and p1
				{
					// ys[x - (int)p0.x] gets the y value at the current x
					PutPixel(x, ys[x - (int)p0.x], color);
				}
			}
			else	// line is vertical-ish
			{
				if (dy < 0)	// make sure it's bottom to top
				{
					(p1, p0) = (p0, p1);	// swap
				}
				
				// interpolate all x values between p0 and p1
				float[] xs = Interpolate(p0.y, p0.x, p1.y, p1.x);
				
				for (int y = (int)p0.y; y <= (int)p1.y ; y++)	// loop through all y values between p0 and p1
				{
					// xs[y - (int)p0.y] gets the x value at the current y
					PutPixel(xs[y - (int)p0.y], y, color);
				}
			}
		}
		
		static float[] Interpolate(float i0, float d0, float i1, float d1)
		{
			// if both values of the independent variable are the same, there is just one value for the dependent variable
			if (i0 == i1)
			{
				return new float[]{d0};
			}
			
			
			List<float> values = new();
			
			float a = (d1 - d0) / (i1 - i0);	// calculating the slope (dd/di)
			
			float d = d0;
			
			for (int i = (int)i0;  i <= (int)i1; i++)	// loop through all values for the independent variable
			{
				// append d to the list of output values, then increase d by the slope
				values.Add(d);
				d += a;
			}
			return values.ToArray();
		}
	}
}