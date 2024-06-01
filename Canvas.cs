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
		
		// private void PutPixel(float x, float y, float z, Color color)
		// {
		// 	int screen_x = Convert.ToInt32((screenWidth/2) + x);
		// 	int screen_y = Convert.ToInt32((screenHeight/2) - y - 1);
			
		// 	if (screen_x < 0 || screen_x >= screenWidth || screen_y < 0 || screen_y >= screenHeight)
		// 	{
		// 		return;
		// 	}
		// 	else if (z > depthBuffer[screen_x, screen_y])	// the z value passed in is actually 1/z
		// 	{
		// 		depthBuffer[screen_x, screen_y] = z;
		// 		screen.SetPixel((uint)screen_x, (uint)screen_y, color);
		// 	}
		// }
		
		// private void PutPixel(float x, float y, Color color)
		// {
		// 	int screen_x = Convert.ToInt32((screenWidth/2) + x);
		// 	int screen_y = Convert.ToInt32((screenHeight/2) - y - 1);
			
		// 	if (screen_x < 0 || screen_x >= screenWidth || screen_y < 0 || screen_y >= screenHeight)
		// 	{
		// 		return;
		// 	}
			
		// 	screen.SetPixel((uint)screen_x, (uint)screen_y, color);
		// }
		
		private void PutPixel(int x, int y, float z, Color color)
		{	
			if (x < 0 || x >= screenWidth || y < 0 || y >= screenHeight)
			{
				return;
			}
			else if (1/z > depthBuffer[x, y])
			{
				depthBuffer[x, y] = 1/z;
				screen.SetPixel((uint)x, (uint)y, color);
			}
		}
		
		internal void DrawWireTriangle(Vector3 p0, Vector3 p1, Vector3 p2, Color color)
		{	
			DrawLine(p0, p1, color);
			DrawLine(p1, p2, color);
			DrawLine(p2, p0, color);
		}
		
		internal void DrawLine(Vector3 p0, Vector3 p1, Color color)
		{
			float dx = p1.x - p0.x;
			float dy = p1.y - p0.y;
			
			if (MathF.Abs(dx) > MathF.Abs(dy))	// line is horizontal-ish
			{
				if (dx < 0)	// make sure it's left to right
				{
					(p1, p0) = (p0, p1);
				}
				
				float k = dy/dx;
				
				int xStart = (int)MathF.Ceiling(p0.x - 0.5f);
				int xEnd = (int)MathF.Ceiling(p1.x - 0.5f);
				
				float zStep = (p1.z - p0.z) / dx;
				
				float z = p0.z;
				
				for (int x = xStart; x < xEnd; x++,
											   z += zStep)
				{
					PutPixel(x, (int)(k*(x+0.5f-p0.x)+p0.y), z, color);
				}
			}
			else	// line is vertical-ish
			{
				if (dy < 0)	// make sure it's bottom to top
				{
					(p1, p0) = (p0, p1);
				}
				
				
				float k = dx/dy;
				
				int yStart = (int)MathF.Ceiling(p0.y - 0.5f);
				int yEnd = (int)MathF.Ceiling(p1.y - 0.5f);
				
				float zStep = (p1.z - p0.z) / dy;
				
				float z = p0.z;
				
				for (int y = yStart; y < yEnd; y++,
											   z += zStep)
				{
					PutPixel((int)(k*(y+0.5f-p0.y)+p0.x), y, z, color);
				}
			}
		}
		
		internal void DrawTriangle(Objects.Vertex v0, Objects.Vertex v1, Objects.Vertex v2, Color color, float shade)
		{
			color.R = (byte)(color.R*shade);
			color.G = (byte)(color.G*shade);
			color.B = (byte)(color.B*shade);
			
			// sort the points so that y0 <= y1 <= y2
			if (v1.pos.y < v0.pos.y) { (v0, v1) = (v1, v0); }
			if (v2.pos.y < v0.pos.y) { (v0, v2) = (v2, v0); }
			if (v2.pos.y < v1.pos.y) { (v1, v2) = (v2, v1); }
			
			if (v0.pos.y == v1.pos.y)	// natural flat top
			{
				// sort top points so that x0 <= x1
				if (v1.pos.x < v0.pos.x) { (v0, v1) = (v1, v0); }
				DrawFlatTopTriangle(v0, v1, v2, color, shade);
			}
			else if (v1.pos.y == v2.pos.y)	// natural flat bottom
			{
				// sort bottom points so that x1 <= x2
				if (v2.pos.x < v1.pos.x) { (v1, v2) = (v2, v1); }
				DrawFlatBottomTriangle(v0, v1, v2, color, shade);
			}
			else	// general triangle
			{
				// find splitting vertex
				float alphaSplit = (v1.pos.y - v0.pos.y) / (v2.pos.y - v0.pos.y);
				Objects.Vertex vi = v0.InterpolateTo(v2, alphaSplit);
				
				if (v1.pos.x < vi.pos.x)	// major right
				{
					DrawFlatBottomTriangle(v0, v1, vi, color, shade);
					DrawFlatTopTriangle(v1, vi, v2, color, shade);
				}
				else	// major left
				{
					DrawFlatBottomTriangle(v0, vi, v1, color, shade);
					DrawFlatTopTriangle(vi, v1, v2, color, shade);
				}
			}
		}
		
		private void DrawFlatTopTriangle(Objects.Vertex v0, Objects.Vertex v1, Objects.Vertex v2, Color color, float shade)
		{
			// calculate slopes
			float m0 = (v2.pos.x - v0.pos.x) / (v2.pos.y - v0.pos.y);
			float m1 = (v2.pos.x - v1.pos.x) / (v2.pos.y - v1.pos.y);
			
			// calculate start and end scanlines
			int yStart = (int)MathF.Ceiling(v0.pos.y - 0.5f);
			int yEnd = (int)MathF.Ceiling(v2.pos.y - 0.5f);		// the scanline AFTER the last line is drawn
			
			
			// init depth edges
			float zEdgeL = v0.pos.z;
			float zEdgeR = v1.pos.z;
			float zBottom = v2.pos.z;
			
			
			// calculate depth edge unit step
			float zEdgeStepL = (zBottom - zEdgeL) / (v2.pos.y - v0.pos.y);
			float zEdgeStepR = (zBottom - zEdgeR) / (v2.pos.y - v1.pos.y);
			
			
			// do depth edge prestep
			zEdgeL += zEdgeStepL * (yStart + 0.5f - v1.pos.y);
			zEdgeR += zEdgeStepR * (yStart + 0.5f - v1.pos.y);
			
			
			for (int y = yStart; y < yEnd; y++,
										   zEdgeL += zEdgeStepL, zEdgeR += zEdgeStepR)
			{
				// calculate line start and end points (x-coordinates)
				// add 0.5 to y value because we're calculating based on pixel CENTERS
				float px0 = m0 * (y + 0.5f - v0.pos.y) + v0.pos.x;
				float px1 = m1 * (y + 0.5f - v1.pos.y) + v1.pos.x;
				
				// calculate start and end pixels
				int xStart = (int)MathF.Ceiling(px0 - 0.5f);
				int xEnd = (int)MathF.Ceiling(px1 - 0.5f);	// the pixel AFTER the last pixel drawn
				
				
				// calculate depth scanline unit step
				float zScanStep = (zEdgeR - zEdgeL) / (px1 - px0);
				
				
				// do depth scanline prestep
				float z = zEdgeL + zScanStep * (xStart + 0.5f - px0);
				
				
				for (int x = xStart; x < xEnd; x++,
											   z += zScanStep)
				{
					PutPixel(x, y, z, color);
				}
			}
		}
		
		private void DrawFlatBottomTriangle(Objects.Vertex v0, Objects.Vertex v1, Objects.Vertex v2, Color color, float shade)
		{	
			// calculate slopes
			float m0 = (v1.pos.x - v0.pos.x) / (v1.pos.y - v0.pos.y);
			float m1 = (v2.pos.x - v0.pos.x) / (v2.pos.y - v0.pos.y);
			
			// calculate start and end scanlines
			int yStart = (int)MathF.Ceiling(v0.pos.y - 0.5f);
			int yEnd = (int)MathF.Ceiling(v2.pos.y - 0.5f);		// the scanline AFTER the last line is drawn
			
			
			// init depth edges
			float zEdgeL = v1.pos.z;
			float zEdgeR = v2.pos.z;
			float zTop = v0.pos.z;
			
			
			// calculate depth edge unit step
			float zEdgeStepL = (zTop - zEdgeL) / (v0.pos.y - v1.pos.y);
			float zEdgeStepR = (zTop - zEdgeR) / (v0.pos.y - v2.pos.y);
			
			
			// do depth edge prestep
			zEdgeL += zEdgeStepL * (yStart + 0.5f - v2.pos.y);
			zEdgeR += zEdgeStepR * (yStart + 0.5f - v2.pos.y);
			
			
			for (int y = yStart; y < yEnd; y++,
										   zEdgeL += zEdgeStepL, zEdgeR += zEdgeStepR)
			{
				// calculate line start and end points (x-coordinates)
				// add 0.5 to y value because we're calculating based on pixel CENTERS
				float px0 = m0 * (y + 0.5f - v0.pos.y) + v0.pos.x;
				float px1 = m1 * (y + 0.5f - v0.pos.y) + v0.pos.x;
				
				// calculate start and end pixels
				int xStart = (int)MathF.Ceiling(px0 - 0.5f);
				int xEnd = (int)MathF.Ceiling(px1 - 0.5f);	// the pixel AFTER the last pixel drawn
				
				
				// calculate depth scanline unit step
				float zScanStep = (zEdgeR - zEdgeL) / (px1 - px0);
				
				
				// do depth scanline prestep
				float z = zEdgeL + zScanStep * (xStart + 0.5f - px0);
				
				
				for (int x = xStart; x < xEnd; x++,
											   z += zScanStep)
				{
					PutPixel(x, y, z, color);
				}
			}
		}
		
		internal void DrawTriangle(Objects.Vertex v0, Objects.Vertex v1, Objects.Vertex v2, Image texture, float shade)
		{
			// sort the points so that y0 <= y1 <= y2
			if (v1.pos.y < v0.pos.y) { (v0, v1) = (v1, v0); }
			if (v2.pos.y < v0.pos.y) { (v0, v2) = (v2, v0); }
			if (v2.pos.y < v1.pos.y) { (v1, v2) = (v2, v1); }
			
			if (v0.pos.y == v1.pos.y)	// natural flat top
			{
				// sort top points so that x0 <= x1
				if (v1.pos.x < v0.pos.x) { (v0, v1) = (v1, v0); }
				DrawFlatTopTriangle(v0, v1, v2, texture, shade);
			}
			else if (v1.pos.y == v2.pos.y)	// natural flat bottom
			{
				// sort bottom points so that x1 <= x2
				if (v2.pos.x < v1.pos.x) { (v1, v2) = (v2, v1); }
				DrawFlatBottomTriangle(v0, v1, v2, texture, shade);
			}
			else	// general triangle
			{
				// find splitting vertex
				float alphaSplit = (v1.pos.y - v0.pos.y) / (v2.pos.y - v0.pos.y);
				Objects.Vertex vi = v0.InterpolateTo(v2, alphaSplit);
				
				if (v1.pos.x < vi.pos.x)	// major right
				{
					DrawFlatBottomTriangle(v0, v1, vi, texture, shade);
					DrawFlatTopTriangle(v1, vi, v2, texture, shade);
				}
				else	// major left
				{
					DrawFlatBottomTriangle(v0, vi, v1, texture, shade);
					DrawFlatTopTriangle(vi, v1, v2, texture, shade);
				}
			}
		}
		
		private void DrawFlatTopTriangle(Objects.Vertex v0, Objects.Vertex v1, Objects.Vertex v2, Image texture, float shade)
		{
			// calculate slopes
			float m0 = (v2.pos.x - v0.pos.x) / (v2.pos.y - v0.pos.y);
			float m1 = (v2.pos.x - v1.pos.x) / (v2.pos.y - v1.pos.y);
			
			// calculate start and end scanlines
			int yStart = (int)MathF.Ceiling(v0.pos.y - 0.5f);
			int yEnd = (int)MathF.Ceiling(v2.pos.y - 0.5f);		// the scanline AFTER the last line is drawn
			
			
			// init tex coord edges
			Vector2 tcEdgeL = v0.tc!;
			Vector2 tcEdgeR = v1.tc!;
			Vector2 tcBottom = v2.tc!;
			
			// init depth edges
			float zEdgeL = v0.pos.z;
			float zEdgeR = v1.pos.z;
			float zBottom = v2.pos.z;
			
			
			// calculate tex coord edge unit step
			Vector2 tcEdgeStepL = (tcBottom - tcEdgeL) / (v2.pos.y - v0.pos.y);
			Vector2 tcEdgeStepR = (tcBottom - tcEdgeR) / (v2.pos.y - v1.pos.y);
			
			// calculate depth edge unit step
			float zEdgeStepL = (zBottom - zEdgeL) / (v2.pos.y - v0.pos.y);
			float zEdgeStepR = (zBottom - zEdgeR) / (v2.pos.y - v1.pos.y);
			
			
			// do tex coord edge prestep
			tcEdgeL += tcEdgeStepL * (yStart + 0.5f - v1.pos.y);
			tcEdgeR += tcEdgeStepR * (yStart + 0.5f - v1.pos.y);
			
			// do depth edge prestep
			zEdgeL += zEdgeStepL * (yStart + 0.5f - v1.pos.y);
			zEdgeR += zEdgeStepR * (yStart + 0.5f - v1.pos.y);
			
			
			// init tex width/height and clamp values
			float texWidth = (float)texture.Size.X;
			float texHeight = (float)texture.Size.Y;
			float texClampX = texWidth - 1.0f;
			float texClampY = texHeight - 1.0f;
			
			for (int y = yStart; y < yEnd; y++,
										   tcEdgeL += tcEdgeStepL, tcEdgeR += tcEdgeStepR,
										   zEdgeL += zEdgeStepL, zEdgeR += zEdgeStepR)
			{
				// calculate line start and end points (x-coordinates)
				// add 0.5 to y value because we're calculating based on pixel CENTERS
				float px0 = m0 * (y + 0.5f - v0.pos.y) + v0.pos.x;
				float px1 = m1 * (y + 0.5f - v1.pos.y) + v1.pos.x;
				
				// calculate start and end pixels
				int xStart = (int)MathF.Ceiling(px0 - 0.5f);
				int xEnd = (int)MathF.Ceiling(px1 - 0.5f);	// the pixel AFTER the last pixel drawn
				
				
				// calculate tex coord scanline unit step
				Vector2 tcScanStep = (tcEdgeR - tcEdgeL) / (px1 - px0);
				
				// calculate depth scanline unit step
				float zScanStep = (zEdgeR - zEdgeL) / (px1 - px0);
				
				
				// do tex coord scanline prestep
				Vector2 tc = tcEdgeL + tcScanStep * (xStart + 0.5f - px0);
				
				// do depth scanline prestep
				float z = zEdgeL + zScanStep * (xStart + 0.5f - px0);
				
				for (int x = xStart; x < xEnd; x++,
				                			   tc += tcScanStep,
											   z += zScanStep)
				{
					Color pixelColor = texture.GetPixel((uint)MathF.Min(tc.x*texWidth, texClampX), (uint)MathF.Min(tc.y*texHeight, texClampY));
					pixelColor.R = (byte)(pixelColor.R * shade);
					pixelColor.G = (byte)(pixelColor.G * shade);
					pixelColor.B = (byte)(pixelColor.B * shade);
					PutPixel(x, y, z, pixelColor);
				}
			}
		}
		
		private void DrawFlatBottomTriangle(Objects.Vertex v0, Objects.Vertex v1, Objects.Vertex v2, Image texture, float shade)
		{
			// calculate slopes
			float m0 = (v1.pos.x - v0.pos.x) / (v1.pos.y - v0.pos.y);
			float m1 = (v2.pos.x - v0.pos.x) / (v2.pos.y - v0.pos.y);
			
			// calculate start and end scanlines
			int yStart = (int)MathF.Ceiling(v0.pos.y - 0.5f);
			int yEnd = (int)MathF.Ceiling(v2.pos.y - 0.5f);		// the scanline AFTER the last line is drawn
			
			
			// init tex coord edges
			Vector2 tcEdgeL = v1.tc!;
			Vector2 tcEdgeR = v2.tc!;
			Vector2 tcTop = v0.tc!;
			
			// init depth edges
			float zEdgeL = v1.pos.z;
			float zEdgeR = v2.pos.z;
			float zTop = v0.pos.z;
			
			
			// calculate tex coord edge unit step
			Vector2 tcEdgeStepL = (tcTop - tcEdgeL) / (v0.pos.y - v1.pos.y);
			Vector2 tcEdgeStepR = (tcTop - tcEdgeR) / (v0.pos.y - v2.pos.y);
			
			// calculate depth edge unit step
			float zEdgeStepL = (zTop - zEdgeL) / (v0.pos.y - v1.pos.y);
			float zEdgeStepR = (zTop - zEdgeR) / (v0.pos.y - v2.pos.y);
			
			
			// do tex coord edge prestep
			tcEdgeL += tcEdgeStepL * (yStart + 0.5f - v2.pos.y);
			tcEdgeR += tcEdgeStepR * (yStart + 0.5f - v2.pos.y);
			
			// do depth edge prestep
			zEdgeL += zEdgeStepL * (yStart + 0.5f - v2.pos.y);
			zEdgeR += zEdgeStepR * (yStart + 0.5f - v2.pos.y);
			
			
			// init tex width/height and clamp values
			float texWidth = (float)texture.Size.X;
			float texHeight = (float)texture.Size.Y;
			float texClampX = texWidth - 1.0f;
			float texClampY = texHeight - 1.0f;
			
			for (int y = yStart; y < yEnd; y++,
										   tcEdgeL += tcEdgeStepL, tcEdgeR += tcEdgeStepR,
										   zEdgeL += zEdgeStepL, zEdgeR += zEdgeStepR)
			{
				// calculate line start and end points (x-coordinates)
				// add 0.5 to y value because we're calculating based on pixel CENTERS
				float px0 = m0 * (y + 0.5f - v0.pos.y) + v0.pos.x;
				float px1 = m1 * (y + 0.5f - v0.pos.y) + v0.pos.x;
				
				// calculate start and end pixels
				int xStart = (int)MathF.Ceiling(px0 - 0.5f);
				int xEnd = (int)MathF.Ceiling(px1 - 0.5f);	// the pixel AFTER the last pixel drawn
				
				
				// calculate tex coord scanline unit step
				Vector2 tcScanStep = (tcEdgeR - tcEdgeL) / (px1 - px0);
				
				// calculate depth scanline unit step
				float zScanStep = (zEdgeR - zEdgeL) / (px1 - px0);
				
				
				// do tex coord scanline prestep
				Vector2 tc = tcEdgeL + tcScanStep * (xStart + 0.5f - px0);
				
				// do depth scanline prestep
				float z = zEdgeL + zScanStep * (xStart + 0.5f - px0);
				
				for (int x = xStart; x < xEnd; x++,
											   tc += tcScanStep,
											   z += zScanStep)
				{
					Color pixelColor = texture.GetPixel((uint)MathF.Min(tc.x*texWidth, texClampX), (uint)MathF.Min(tc.y*texHeight, texClampY));
					pixelColor.R = (byte)(pixelColor.R * shade);
					pixelColor.G = (byte)(pixelColor.G * shade);
					pixelColor.B = (byte)(pixelColor.B * shade);
					PutPixel(x, y, z, pixelColor);
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