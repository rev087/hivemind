using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Hivemind {

	public class NodeRenderer {

		Texture2D nodeTexture;
		Texture2D shadowTexture;
		Color edgeColor = Color.white;
		Color shadowColor = new Color(0f, 0f, 0f, 0.15f);

		// Selection
		Texture2D selectionTexture;
		Color selColor = new Color(1f, .78f, .353f);
		float selMargin = 2f;
		float selWidth = 2f;

		// Node icons
		Texture2D rootTex;
		Texture2D selectorTex;
		Texture2D succeederTex;
		Texture2D inverterTex;
		Texture2D untilSucceedTex;
		Texture2D actionTex;
		Texture2D repeaterTex;
		Texture2D repeaterXTex;
		Texture2D sequenceTex;
		Texture2D parallelTex;
		Texture2D randomSelectorTex;

		public float Width { get { return GridRenderer.step.x * 6; } }
		public float Height { get { return GridRenderer.step.y * 6; } }

		public NodeRenderer() {

		}

		public void Draw(Node node, bool selected) {
			float shadowOffset = 3;

			// Edge
			if (node.parent != null) {

				// Shadow
				Vector2 offset = new Vector2(shadowOffset, shadowOffset);
				DrawEdge(node.parent.editorPosition + offset, node.editorPosition + offset, Width, Height, shadowColor);

				// Line
				DrawEdge(node.parent.editorPosition, node.editorPosition, Width, Height, edgeColor);
			}

			// Node Shadow

			Rect nodeRect = new Rect(node.editorPosition.x, node.editorPosition.y, Width, Height);
			Rect shadowRect = new Rect(nodeRect.x + shadowOffset, nodeRect.y + shadowOffset, nodeRect.width, nodeRect.height);

			if (shadowTexture == null) {
				shadowTexture = new Texture2D(1, 1);
				shadowTexture.hideFlags = HideFlags.DontSave;
				shadowTexture.SetPixel(0, 0, shadowColor);
				shadowTexture.Apply();
			}

			GUI.DrawTexture (shadowRect, shadowTexture);
			
			// Node

			if (nodeTexture == null) {
				Color colA = new Color(0.765f, 0.765f, 0.765f);
				Color colB = new Color(0.886f, 0.886f, 0.886f);

				nodeTexture = new Texture2D(1, (int)Height);
				nodeTexture.hideFlags = HideFlags.DontSave;
				for (int y = 0; y < Height; y++) {
					nodeTexture.SetPixel (0, y, Color.Lerp (colA, colB, (float)y/75));
				}
				nodeTexture.Apply();
			}

			GUI.DrawTexture (nodeRect, nodeTexture);

			// Icons

			// Root
			if (node is Root) {
				if (rootTex == null) {
					rootTex = (Texture2D) EditorGUIUtility.Load ("Hivemind/Root.png");
					rootTex.hideFlags = HideFlags.DontSave;
				}
				GUI.DrawTexture (IconRect (nodeRect), rootTex);
			}

			// Sequence
			else if (node is Sequence) {
				if (sequenceTex == null) {
					sequenceTex = (Texture2D) EditorGUIUtility.Load ("Hivemind/Sequence.png");
					sequenceTex.hideFlags = HideFlags.DontSave;
				}
				GUI.DrawTexture (IconRect (nodeRect), sequenceTex);
			}

			// Succeeder
			else if (node is Succeeder) {
				if (succeederTex == null) {
					succeederTex = (Texture2D) EditorGUIUtility.Load ("Hivemind/Succeeder.png");
					succeederTex.hideFlags = HideFlags.DontSave;
				}
				GUI.DrawTexture (IconRect (nodeRect), succeederTex);
			}

			// Selector
			else if (node is Selector) {
				if (selectorTex == null) {
					selectorTex = (Texture2D) EditorGUIUtility.Load ("Hivemind/Selector.png");
					selectorTex.hideFlags = HideFlags.DontSave;
				}
				GUI.DrawTexture (IconRect (nodeRect), selectorTex);
			}

			// Repeater
			else if (node is Repeater) {

				// Infinite
				if (((Repeater)node).repetitions == 0) {
					if (repeaterTex == null) {
						repeaterTex = (Texture2D) EditorGUIUtility.Load ("Hivemind/Repeater.png");
						repeaterTex.hideFlags = HideFlags.DontSave;
					}
					GUI.DrawTexture (IconRect (nodeRect), repeaterTex);
				
				// Finite
				} else {
					if (repeaterXTex == null) {
						repeaterXTex = (Texture2D) EditorGUIUtility.Load ("Hivemind/Repeater X.png");
						repeaterXTex.hideFlags = HideFlags.DontSave;
					}
					GUI.DrawTexture (IconRect (nodeRect), repeaterXTex);
				}
			}

			// Until Succeed Repeater
			else if (node is UntilSucceed) {
				if (untilSucceedTex == null) {
					untilSucceedTex = (Texture2D) EditorGUIUtility.Load ("Hivemind/Until Succeed.png");
					untilSucceedTex.hideFlags = HideFlags.DontSave;
				}
				GUI.DrawTexture (IconRect (nodeRect), untilSucceedTex);
			}

			// Inverter
			else if (node is Inverter) {
				if (inverterTex == null) {
					inverterTex = (Texture2D) EditorGUIUtility.Load ("Hivemind/Inverter.png");
					inverterTex.hideFlags = HideFlags.DontSave;
				}
				GUI.DrawTexture (IconRect (nodeRect), inverterTex);
			}


			// Action
			else if (node is Action) {
				if (actionTex == null) {
					actionTex = (Texture2D) EditorGUIUtility.Load ("Hivemind/Action.png");
					actionTex.hideFlags = HideFlags.DontSave;
				}
				GUI.DrawTexture (IconRect (nodeRect), actionTex);
			}

			// Parallel
			else if (node is Parallel) {
				if (parallelTex == null) {
					parallelTex = (Texture2D) EditorGUIUtility.Load ("Hivemind/Parallel.png");
					parallelTex.hideFlags = HideFlags.DontSave;
				}
				GUI.DrawTexture (IconRect (nodeRect), parallelTex);
			}
			
			// Random
			else if (node is RandomSelector) {
				if (randomSelectorTex == null) {
					randomSelectorTex = (Texture2D) EditorGUIUtility.Load ("Hivemind/Random Selector.png");
					randomSelectorTex.hideFlags = HideFlags.DontSave;
				}
				GUI.DrawTexture (IconRect (nodeRect), randomSelectorTex);
			}

			// Selection highlight
			if (selected) {

				if (selectionTexture == null) {
					selectionTexture = new Texture2D(1, 1);
					selectionTexture.hideFlags = HideFlags.DontSave;
					selectionTexture.SetPixel (0, 0, selColor);
					selectionTexture.Apply();
				}

				float mbOffset = selMargin + selWidth; // Margin + Border offset
				GUI.DrawTexture (new Rect(nodeRect.x - mbOffset, nodeRect.y - mbOffset, nodeRect.width + mbOffset * 2, selWidth), selectionTexture); // Top
				GUI.DrawTexture (new Rect(nodeRect.x - mbOffset, nodeRect.y - selMargin, selWidth, nodeRect.height + selMargin * 2), selectionTexture); // Left
				GUI.DrawTexture (new Rect(nodeRect.x + nodeRect.width + selMargin, nodeRect.y - selMargin, selWidth, nodeRect.height + selMargin * 2), selectionTexture); // Right
				GUI.DrawTexture (new Rect(nodeRect.x - mbOffset, nodeRect.y + nodeRect.height + selMargin, nodeRect.width + mbOffset * 2, selWidth), selectionTexture); // Top
			}

		}

		Rect IconRect(Rect nodeRect) {
			int width = NearestPowerOfTwo (nodeRect.width);
			int height = NearestPowerOfTwo (nodeRect.height);
			float xOffset = (nodeRect.width - width) / 2;
			float yOffset = (nodeRect.height - height) / 2;
			Rect iconRect = new Rect(nodeRect.x + xOffset, nodeRect.y + yOffset, width, height);
			return iconRect;
		}

		int NearestPowerOfTwo(float value) {
			int result = 1;
			do {
				result = result << 1;
			} while (result << 1 < value);
			return result;
		}

		public static void DrawEdge(Vector2 start, Vector2 end, float width, float height, Color color) {
			float offset = width / 2;
			Vector3 startPos = new Vector3(start.x + offset, start.y + height, 0);
			Vector3 endPos = new Vector3(end.x + offset, end.y, 0);
			Vector3 startTan = startPos + Vector3.up * GridRenderer.step.x * 2;
			Vector3 endTan = endPos + Vector3.down * GridRenderer.step.x * 2;
			Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 4);
		}

		public Rect rectForNode(Node node, Vector2 offset) {
			return new Rect(node.editorPosition.x - offset.x, node.editorPosition.y - offset.y, Width, Height);
		}
	}

}

