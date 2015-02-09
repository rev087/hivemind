using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace Hivemind {

	public class NodeRenderer {

		Texture2D nodeTexture;
		Texture2D nodeDebugTexture;
		Texture2D shadowTexture;
		Color edgeColor = Color.white;
		Color shadowColor = new Color(0f, 0f, 0f, 0.15f);

		// Selection
		Texture2D selectionTexture;
		Color selColor = new Color(1f, .78f, .353f);
		float selMargin = 2f;
		float selWidth = 2f;

		private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

		public static float Width { get { return GridRenderer.step.x * 6; } }
		public static float Height { get { return GridRenderer.step.y * 6; } }

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

			// Node Debug
			if (nodeDebugTexture == null) {
				
				Color colA = new Color(1.000f, 0.796f, 0.357f);
				Color colB = new Color(0.894f, 0.443f, 0.008f);
				
				nodeDebugTexture = new Texture2D(1, (int)Height);
				nodeDebugTexture.hideFlags = HideFlags.DontSave;
				for (int y = 0; y < Height; y++) {
					nodeDebugTexture.SetPixel (0, y, Color.Lerp (colA, colB, (float)y/75));
				}
				nodeDebugTexture.Apply();
			}

			if (node.behaviorTree.debugMode && node.behaviorTree.currentNode == node) {
				GUI.DrawTexture (nodeRect, nodeDebugTexture);
			} else {
				GUI.DrawTexture (nodeRect, nodeTexture);
			}

			// Icons
			DrawNodeIcon(nodeRect, node);

			// Debug status
			DrawStatusIcon(nodeRect, node);
			
			// Action title
			if (node is Action) {

				// Node title
				string title;
				if (((Action)node).methodInfo != null)
					title = ((Action)node).methodName;
				else
					title = "";
				title = title.Replace(".", ".\n");
				Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(title));
				float x = node.editorPosition.x + (Width/2) - (textSize.x/2) - 6;
				Rect titleRect = new Rect(x, node.editorPosition.y + Height, textSize.x + 10, textSize.y);
				EditorGUI.DropShadowLabel(titleRect, new GUIContent(title));
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

		private void DrawStatusIcon(Rect nodeRect, Node node) {
			EditorGUI.LabelField(new Rect(nodeRect.x, nodeRect.y + 58f, nodeRect.width, nodeRect.height), node.lastTick.ToString ());

			if (node.lastStatus != null && BTEditorManager.Manager.behaviorTree.TotalTicks - node.lastTick < 150) {

				string status = node.lastStatus.ToString();

				if (!textures.ContainsKey (status)) {
					Texture2D tex = (Texture2D) EditorGUIUtility.Load ("Hivemind/Status/"+status+".png");
					if (tex == null) {
						Debug.LogWarning (status + ".png not found");
						return;
					}
					tex.hideFlags = HideFlags.DontSave;
					textures.Add (status, tex);
				}

				Rect statusRect = new Rect(nodeRect.x, nodeRect.y, 32f, 32f);
				GUI.DrawTexture(statusRect, textures[status]);

			}
		}

		private void DrawNodeIcon(Rect nodeRect, Node node) {
			int width = NearestPowerOfTwo (nodeRect.width);
			int height = NearestPowerOfTwo (nodeRect.height);
			float xOffset = (nodeRect.width - width) / 2;
			float yOffset = (nodeRect.height - height) / 2;
			Rect iconRect = new Rect(nodeRect.x + xOffset, nodeRect.y + yOffset, width, height);

			string nodeName = node.GetType ().Name;
			if (node is Sequence && ((Sequence) node).rememberRunning) nodeName = "MemSequence";
			
			if (!textures.ContainsKey (nodeName)) {

				Texture2D tex = (Texture2D) EditorGUIUtility.Load ("Hivemind/Nodes/"+nodeName+".png");
				if (tex == null) {
					Debug.LogWarning (nodeName + ".png not found");
					return;
				}
				tex.hideFlags = HideFlags.DontSave;
				textures.Add (nodeName, tex);
			}
			GUI.DrawTexture (iconRect, textures[nodeName]);
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

