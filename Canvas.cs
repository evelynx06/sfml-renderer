using CommunityToolkit.HighPerformance;
using SFML.Graphics;
using Engine.Math;

namespace Engine
{
	/// <summary>
	/// A canvas that handles drawing lines and triangles, with a depth buffer
	/// </summary>
	internal class Canvas
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
		
		private void PutPixel(float x, float y, float z, Color color)
		{
			int screen_x = Convert.ToInt32((screenWidth/2) + x);
			int screen_y = Convert.ToInt32((screenHeight/2) - y - 1);
			
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
		
		private void PutPixel(float x, float y, Color color)
		{
			int screen_x = Convert.ToInt32((screenWidth/2) + x);
			int screen_y = Convert.ToInt32((screenHeight/2) - y - 1);
			
			if (screen_x < 0 || screen_x >= screenWidth || screen_y < 0 || screen_y >= screenHeight)
			{
				return;
			}
			
			screen.SetPixel((uint)screen_x, (uint)screen_y, color);
		}
		
		private void PutPixelInt(int x, int y, Color color)
		{
			int screen_x = (screenWidth/2) + x;
			int screen_y = (screenHeight/2) - y;
			
			if (screen_x < 0 || screen_x >= screenWidth || screen_y < 0 || screen_y >= screenHeight)
			{
				return;
			}
			// else if (z > depthBuffer[screen_x, screen_y])	// the z value passed in is actually 1/z
			// {
				// depthBuffer[screen_x, screen_y] = z;
				screen.SetPixel((uint)screen_x, (uint)screen_y, color);
			// }
		}
		
		internal void DrawWireTriangle(Point p0, Point p1, Point p2, Color color)
		{	
			DrawLine(p0, p1, color);
			DrawLine(p1, p2, color);
			DrawLine(p2, p0, color);
		}
		
		internal void DrawLine(Point p0, Point p1, Color color)
		{
			float dx = p1.x - p0.x;
			float dy = p1.y - p0.y;
			
			if (MathF.Abs(dx) > MathF.Abs(dy))	// line is horizontal-ish
			{
				if (dx < 0)	// make sure it's left to right
				{
					(p1, p0) = (p0, p1);
				}

				// float[] ys = Interpolate(p0.x, p0.y, p1.x, p1.y);
				float k = dy/dx;
				int xStart = (int)MathF.Ceiling(p0.x - 0.5f);
				int xEnd = (int)MathF.Ceiling(p1.x - 0.5f);
				for (int x = xStart; x < xEnd; x++)
				{
					// PutPixel(x, ys[x - (int)p0.x], color);
					PutPixelInt(x, (int)(k*(x+0.5f-p0.x)+p0.y), color);
				}
			}
			else	// line is vertical-ish
			{
				if (dy < 0)	// make sure it's bottom to top
				{
					(p1, p0) = (p0, p1);
				}
				
				// float[] xs = Interpolate(p0.y, p0.x, p1.y, p1.x);
				float k = dx/dy;
				int yStart = (int)MathF.Ceiling(p0.y - 0.5f);
				int yEnd = (int)MathF.Ceiling(p1.y - 0.5f);
				for (int y = yStart; y < yEnd; y++)
				{
					// PutPixel(xs[y - (int)p0.y], y, color);
					PutPixelInt((int)(k*(y+0.5f-p0.y)+p0.x), y, color);
				}
			}
		}
		
		internal void DrawFilledTriangle(Point p0, Point p1, Point p2, Color color)
		{
			// sort the points so that y0 <= y1 <= y2
			if (p1.y < p0.y) { (p0, p1) = (p1, p0); }
			if (p2.y < p0.y) { (p0, p2) = (p2, p0); }
			if (p2.y < p1.y) { (p1, p2) = (p2, p1); }
			
			if (p0.y == p1.y)	// natural flat top
			{
				// sort top points so that x0 <= x1
				if (p1.x < p0.x) { (p0, p1) = (p1, p0); }
				DrawFlatTopTriangle(p0, p1, p2, color);
			}
			else if (p1.y == p2.y)	// natural flat bottom
			{
				// sort bottom points so that x1 <= x2
				if (p2.x < p1.x) { (p1, p2) = (p2, p1); }
				DrawFlatBottomTriangle(p0, p1, p2, color);
			}
			else	// general triangle
			{
				// find splitting vertex
				float alphaSplit = (p1.y - p0.y) / (p2.y - p0.y);
				Point pSplit = p0 + (p2 - p0) * alphaSplit;
				
				if (p1.x < pSplit.x)	// major right
				{
					DrawFlatBottomTriangle(p0, p1, pSplit, color);
					DrawFlatTopTriangle(p1, pSplit, p2, color);
				}
				else	// major left
				{
					DrawFlatBottomTriangle(p0, pSplit, p1, color);
					DrawFlatTopTriangle(pSplit, p1, p2, color);
				}
			}
		}
		
		private void DrawFlatTopTriangle(Point p0, Point p1, Point p2, Color color)
		{
			// calculate slopes
			float m0 = (p2.x - p0.x) / (p2.y - p0.y);
			float m1 = (p2.x - p1.x) / (p2.y - p1.y);
			
			// calculate start and end scanlines
			int yStart = (int)MathF.Ceiling(p0.y - 0.5f);
			int yEnd = (int)MathF.Ceiling(p2.y - 0.5f);		// the scanline AFTER the last line is drawn
			
			for (int y = yStart; y < yEnd; y++)
			{
				// calculate line start and end points (x-coordinates)
				// add 0.5 to y value because we're calculating based on pixel CENTERS
				float x0 = m0 * (y + 0.5f - p0.y) + p0.x;
				float x1 = m1 * (y + 0.5f - p1.y) + p1.x;
				
				// calculate start and end pixels
				int xStart = (int)MathF.Ceiling(x0 - 0.5f);
				int xEnd = (int)MathF.Ceiling(x1 - 0.5f);	// the pixel AFTER the last pixel drawn
				
				for (int x = xStart; x < xEnd; x++)
				{
					PutPixelInt(x, y, color);
				}
			}
		}
		
		private void DrawFlatBottomTriangle(Point p0, Point p1, Point p2, Color color)
		{
			// calculate slopes
			float m0 = (p1.x - p0.x) / (p1.y - p0.y);
			float m1 = (p2.x - p0.x) / (p2.y - p0.y);
			
			// calculate start and end scanlines
			int yStart = (int)MathF.Ceiling(p0.y - 0.5f);
			int yEnd = (int)MathF.Ceiling(p2.y - 0.5f);		// the scanline AFTER the last line is drawn
			
			for (int y = yStart; y < yEnd; y++)
			{
				// calculate line start and end points (x-coordinates)
				// add 0.5 to y value because we're calculating based on pixel CENTERS
				float x0 = m0 * (y + 0.5f - p0.y) + p0.x;
				float x1 = m1 * (y + 0.5f - p0.y) + p0.x;
				
				// calculate start and end pixels
				int xStart = (int)MathF.Ceiling(x0 - 0.5f);
				int xEnd = (int)MathF.Ceiling(x1 - 0.5f);	// the pixel AFTER the last pixel drawn
				
				for (int x = xStart; x < xEnd; x++)
				{
					PutPixelInt(x, y, color);
				}
			}
		}
		
		/*
		internal void DrawFilledTriangle(Point p0, Point p1, Point p2, Color color)
		{	
			// sort the points so that y0 <= y1 <= y2
			if (p1.y < p0.y)
			{
				(p0, p1) = (p1, p0);
			}
			if (p2.y < p0.y)
			{
				(p0, p2) = (p2, p0);
			}
			if (p2.y < p1.y)
			{
				(p1, p2) = (p2, p1);
			}
			
			
			float[] x01 = Interpolate(p0.y, p0.x, p1.y, p1.x);
			float[] z01 = Interpolate(p0.y, 1/p0.z, p1.y, 1/p1.z);
			
			float[] x12 = Interpolate(p1.y, p1.x, p2.y, p2.x);
			float[] z12 = Interpolate(p1.y, 1/p1.z, p2.y, 1/p2.z);
			
			float[] x02 = Interpolate(p0.y, p0.x, p2.y, p2.x);
			float[] z02 = Interpolate(p0.y, 1/p0.z, p2.y, 1/p2.z);
			
			float[] x012 = x01.SkipLast(1).Concat(x12).ToArray();
			float[] z012 = z01.SkipLast(1).Concat(z12).ToArray();
			
			
			// determine which side is left and right
			float[] x_left;
			float[] z_left;
			float[] x_right;
			float[] z_right;
			
			int m = x02.Length/2;
			if (x02[m] < x012[m])
			{
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
			
			for (int y = (int)MathF.Ceiling(p0.y); y < (int)MathF.Ceiling(p2.y); y++)
			{
				float x_l = x_left[y-(int)p0.y];
				float x_r = x_right[y-(int)p0.y];
				float[] z_segment = Interpolate(x_l, z_left[y-(int)p0.y], x_r, z_right[y-(int)p0.y]);
				
				for (int x = (int)x_l; x < x_r; x++)
				{
					// PutPixel(x, y, z_segment[x - (int)x_l], color);
					PutPixelInt(x, y, color);
				}
			}
		}
		
		private static float[] Interpolate(float i0, float d0, float i1, float d1)
		{
			if (i0 == i1)
			{
				return new float[]{d0};
			}
			
			List<float> values = new();
			
			float a = (d1 - d0) / (i1 - i0);
			float d = d0;
			
			for (int i = (int)i0;  i <= (int)i1; i++)
			{
				values.Add(d);
				d += a;
			}
			return values.ToArray();
		}
		*/
	}
}