using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Hivemind {

	public class GridRenderer {

		Texture2D gridTex;

		public static int width { get { return 120; } }
		public static int height { get { return 120; } }
		public static Vector2 step { get { return new Vector2(width / 10, height / 10); } }
		
		// Generates a single tile of the grid texture
		void GenerateGrid() {
			gridTex = new Texture2D(width, height);
			gridTex.hideFlags = HideFlags.DontSave;
			
			Color bg = new Color(0.64f, 0.64f, 0.64f);

			Color dark = Color.Lerp(bg, Color.black, 0.15f);
			Color darkIntersection = Color.Lerp(bg, Color.black, 0.2f);

			Color light = Color.Lerp(bg, Color.black, 0.05f);
			Color lightIntersection = Color.Lerp(bg, Color.black, 0.1f);
			
			for (int x = 0; x < width; x ++) {

				for (int y = 0; y < height; y ++) {
					
					// Left Top edge, dark intersection color
					if (x == 0 && y == 0)
						gridTex.SetPixel(x, y, darkIntersection);

					// Left and Top edges, dark color
					else if (x == 0 || y == 0)
						gridTex.SetPixel(x, y, dark);

					// Finer grid intersection color
					else if (x % step.x == 0 && y % step.y == 0)
						gridTex.SetPixel(x, y, lightIntersection);

					// Finer grid color
					else if (x % step.x == 0 || y % step.y == 0)
						gridTex.SetPixel(x, y, light);

					// Background
					else
						gridTex.SetPixel(x, y, bg);
				}

			}
			
			gridTex.Apply();
		}
		
		public void Draw(Vector2 scrollPoint, Rect canvas) {
			if (!gridTex) GenerateGrid ();
			
			float yOffset = scrollPoint.y % gridTex.height;
			float yStart = scrollPoint.y - yOffset;
			float yEnd = scrollPoint.y + canvas.height + yOffset;
			
			float xOffset = scrollPoint.x % gridTex.width;
			float xStart = scrollPoint.x - xOffset;
			float xEnd = scrollPoint.x + canvas.width + xOffset;
			
			for (float x = xStart; x < xEnd; x += gridTex.width) {
				for (float y = yStart; y < yEnd; y += gridTex.height) {
					GUI.DrawTexture(new Rect(x, y, gridTex.width, gridTex.height), gridTex);
				}
			}
		}

	}
	
}