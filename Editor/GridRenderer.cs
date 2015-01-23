using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Hivemind {

	public class GridRenderer {

		Texture2D gridTex;

		public static int width { get { return 120; } }
		public static int height { get { return 120; } }
		public static Vector2 step { get { return new Vector2(width / 10, height / 10); } }
		
		void GenerateGrid() {
			gridTex = new Texture2D(width, height);
			gridTex.hideFlags = HideFlags.DontSave;
			
			Color bg = new Color(0.365f, 0.365f, 0.365f);
			Color dark = new Color(0.278f, 0.278f, 0.278f);
			Color light = new Color(0.329f, 0.329f, 0.329f);
			Color darkX = new Color(0.216f, 0.216f, 0.216f);
			Color lightX = new Color(0.298f, 0.298f, 0.298f);
			
			for (int x = 0; x < width; x ++) {
				for (int y = 0; y < height; y ++) {
					
					if (x == 0 && y == 0)
						gridTex.SetPixel(x, y, darkX);
					else if (x == 0 || y == 0)
						gridTex.SetPixel(x, y, dark);
					else if (x % step.x == 0 && y % step.y == 0)
						gridTex.SetPixel(x, y, lightX);
					else if (x % step.x == 0 || y % step.y == 0)
						gridTex.SetPixel(x, y, light);
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