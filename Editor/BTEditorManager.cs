using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace Hivemind {

	/* 
	 * This manager is created by the BTInspector, the custom inspector for BehaviorTree assets.
	 * Every time the inspector receives the OnEnable message, a fresh manager is created, and
	 * destroyed when the inspector receives the OnDisable message.
	 * 
	 * Its main responsibility is to serve as the sole route for BehaviorTree manipulation.
	 * 
	 * It achieves that by exposing a selected BehaviorTree (determined by the current BehaviorTree
	 * being inspected in BTInspector) to the BTEditorWindow
	 * 
	 * BTEditorWindow manages its own sub systems to provide editing functionality, and forwards
	 * all actual manipulations to this manager.
	 * 
	 * Creation of new BehaviorTrees are also handled here.
	 */

	public class BTEditorManager : ScriptableObject {

		public Editor btInspector;
		public Editor nodeInspector;
		public BTEditorWindow editorWindow;

		public Node selectedNode;

		public BehaviorTree behaviorTree;
		public BTAsset btAsset;

		public static BTEditorManager Manager { get; private set; }

		public static BTEditorManager CreateInstance(BehaviorTree bt, BTAsset asset) {
			if (Manager == null) {
				Manager = (BTEditorManager) ScriptableObject.CreateInstance (typeof(BTEditorManager));
				Manager.behaviorTree = bt;
				Manager.btAsset = asset;
			}
			return Manager;
		}

		// Lifecycle

		public void OnEnable() {
			hideFlags = HideFlags.HideAndDontSave;
		}

		public void OnDestroy() {
			Manager = null;
			DestroyImmediate (behaviorTree);
		}

		// Asset management ------------------------------------------------------------------------------------------------------------------------------------

		[MenuItem("Assets/Create/Behavior Tree", false, 1)]
		static void CreateNewBehaviorTree(MenuCommand menuCommand) {
			string path = AssetDatabase.GetAssetPath (Selection.activeObject);
			if (path == "")
				path = "Assets";
			else if (Path.GetExtension (path) != "")
				path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
			
			string fullPath = AssetDatabase.GenerateUniqueAssetPath (path + "/New Behavior Tree.asset");
			
			BehaviorTree bt = ScriptableObject.CreateInstance<BehaviorTree>();
			Root root = ScriptableObject.CreateInstance<Root>();
			root.editorPosition = new Vector2(0, 0);
			bt.SetRoot(root);
			BTAsset btAsset = ScriptableObject.CreateInstance<BTAsset>();
			btAsset.Serialize(bt);

			AssetDatabase.CreateAsset (btAsset, fullPath);
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = btAsset;
		}

		public void Dirty() {
			if (editorWindow != null) editorWindow.Repaint();
			btAsset.Serialize(behaviorTree);
			EditorUtility.SetDirty (btAsset);
		}

		// Behavior Tree manipulation --------------------------------------------------------------------------------------------------------------------------

		public void Add(Node parent, Vector2 position, string nodeType) {
			
			switch (nodeType) {
			case "Action":
				Action action = (Action) ScriptableObject.CreateInstance<Action>();
				behaviorTree.nodes.Add (action);
				PositionNewNode (action, parent, position);
				break;
			case "Inverter":
				Inverter inverter = (Inverter) ScriptableObject.CreateInstance<Inverter>();
				behaviorTree.nodes.Add (inverter);
				PositionNewNode (inverter, parent, position);
				break;
			case "Parallel":
				Parallel parallel = (Parallel) ScriptableObject.CreateInstance<Parallel>();
				behaviorTree.nodes.Add (parallel);
				PositionNewNode (parallel, parent, position);
				break;
			case "RandomSelector":
				RandomSelector randomSelector = (RandomSelector) ScriptableObject.CreateInstance<RandomSelector>();
				behaviorTree.nodes.Add (randomSelector);
				PositionNewNode (randomSelector, parent, position);
				break;
			case "Repeater":
				Repeater repeater = (Repeater) ScriptableObject.CreateInstance<Repeater>();
				behaviorTree.nodes.Add (repeater);
				PositionNewNode (repeater, parent, position);
				break;
			case "Selector":
				Selector selector = (Selector) ScriptableObject.CreateInstance<Selector>();
				behaviorTree.nodes.Add (selector);
				PositionNewNode (selector, parent, position);
				break;
			case "Sequence":
				Sequence sequence = (Sequence) ScriptableObject.CreateInstance<Sequence>();
				behaviorTree.nodes.Add (sequence);
				PositionNewNode (sequence, parent, position);
				break;
			case "Succeeder":
				Succeeder succeeder = (Succeeder) ScriptableObject.CreateInstance<Succeeder>();
				behaviorTree.nodes.Add (succeeder);
				PositionNewNode (succeeder, parent, position);
				break;
			case "UntilSucceed":
				UntilSucceed untilsucceed = (UntilSucceed) ScriptableObject.CreateInstance<UntilSucceed>();
				behaviorTree.nodes.Add (untilsucceed);
				PositionNewNode (untilsucceed, parent, position);;
				break;
			}
			
		}

		private void PositionNewNode(Node node, Node parent, Vector2 position) {

			if (parent != null && parent.CanConnectChild) {
				if (parent.ChildCount > 0) {
					Node lastSibling = parent.Children[parent.ChildCount - 1];
					node.editorPosition = lastSibling.editorPosition + new Vector2(GridRenderer.step.x * 10, 0);
				} else {
					node.editorPosition = new Vector2(parent.editorPosition.x, parent.editorPosition.y + GridRenderer.step.y * 10);
				}
				parent.ConnectChild(node);
				SortChildren(parent);
			} else {
				float xOffset = position.x % GridRenderer.step.x;
				float yOffset = position.y % GridRenderer.step.y;
				node.editorPosition = new Vector2(position.x - xOffset, position.y - yOffset);
			}
			Dirty ();

			// Select the newly added node
			if (editorWindow != null)
				editorWindow.view.SelectNode(node);
		}

		public void Connect(Node parent, Node child) {
			if (parent.CanConnectChild) {
				parent.ConnectChild(child);
				SortChildren(parent);
				Dirty ();
			} else {
				Debug.LogWarning (string.Format ("{0} can't accept child {1}", parent, child));
			}
		}
		
		public void Unparent(Node node) {
			node.Unparent();
			Dirty ();
		}
		
		public void Delete(Node node) {
			node.Disconnect();
			behaviorTree.nodes.Remove (node);
			DestroyImmediate(node, true);
			Dirty ();
		}

		public void SetEditorPosition(Node node, Vector2 position) {
			node.editorPosition = position;
			SortChildren(node.parent);
			Dirty ();
		}

		private void SortChildren(Node parent) {
			Composite parentComposite = parent as Composite;
			if (parentComposite != null)
				parentComposite.SortChildren();
		}
	}

}