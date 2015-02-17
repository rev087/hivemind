using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Hivemind {

	public class HivemindInspector : Editor {

		private static GUIStyle _titleStyle;
		private static GUIStyle _subtitleStyle;

		public static GUIStyle TitleStyle {
			get {
				if (_titleStyle == null) {
					_titleStyle = new GUIStyle();
					_titleStyle.fontSize = 18;
				}
				return _titleStyle;
			}
		}

		public static GUIStyle SubtitleStyle {
			get {
				if (_subtitleStyle == null) {
					_subtitleStyle = new GUIStyle();
					_subtitleStyle.fontSize = 15;
				}
				return _subtitleStyle;
			}
		}
	}

	[CustomEditor(typeof(BTAsset))]
	public class BTInspector : HivemindInspector {
		private BTEditorManager manager;

		public void OnEnable() {
			BTAsset btAsset = (BTAsset) serializedObject.targetObject;
			BehaviorTree bt = btAsset.Deserialize();
			manager = BTEditorManager.CreateInstance(bt, btAsset);
			manager.btInspector = this;
		}

		public void OnDisable() {
			DestroyImmediate (manager);
		}

		public override void OnInspectorGUI() {

			if (BTEditorManager.Manager.nodeInspector == null) {
				
				EditorGUILayout.LabelField("Behavior Tree", TitleStyle);

				if (manager.behaviorTree.nodes.Count > 2)
					EditorGUILayout.LabelField(string.Format ("{0} nodes", manager.behaviorTree.nodes.Count - 1));
				else if (manager.behaviorTree.nodes.Count == 2)
					EditorGUILayout.LabelField("Empty");
				else
					EditorGUILayout.LabelField("1 node");

				EditorGUILayout.Space ();

				if (GUILayout.Button ("Show Behavior Tree editor")) {
					BTEditorWindow.ShowWindow ();
				}
				
			}
			else {
				manager.nodeInspector.OnInspectorGUI();
			}
			
			if (GUI.changed) {
				manager.Dirty();
			}
		}
		
	}

	[CustomEditor(typeof(Node), true)]
	public class BTNodeInspector : HivemindInspector {

		public override void OnInspectorGUI() {

			Node node = (Node) serializedObject.targetObject;

			if (node is Root) { DrawInspector ((Root) node); }
			else if (node is Action) { DrawInspector ((Action) node); }
			else if (node is Selector) { DrawInspector ((Selector) node); }
			else if (node is Sequence) { DrawInspector ((Sequence) node); }
			else if (node is Parallel) { DrawInspector ((Parallel) node); }
			else if (node is Repeater) { DrawInspector ((Repeater) node); }
			else if (node is RandomSelector) { DrawInspector ((RandomSelector) node); }
			else if (node is UntilSucceed) { DrawInspector ((UntilSucceed) node); }
			else if (node is Inverter) { DrawInspector ((Inverter) node); }
			else if (node is Succeeder) { DrawInspector ((Succeeder) node); }

			if (GUI.changed) {
				 BTEditorManager.Manager.Dirty();
			}
		}

		public void DrawInspector(Root node) {
			EditorGUILayout.LabelField(new GUIContent("Root"), TitleStyle);
		}

		private int IndexOf(string[] array, string target) {
			int length = array.Length;
			for (var i = 0; i < length; i++) {
				if (array[i] == target) return i;
			}
			return 0;
		}

		public void DrawInspector(Action action) {
			EditorGUILayout.LabelField(new GUIContent("Action"), TitleStyle);

			EditorGUILayout.Space ();


			// MonoScript selection field
			if ( action.HasScript && action.scriptInstance == null) {
				action.scriptInstance = (Object) AssetDatabase.LoadAssetAtPath (action.ScriptPath, typeof(MonoScript));
			}
			MonoScript monoScript = (MonoScript) EditorGUILayout.ObjectField("Action Library", (MonoScript) action.scriptInstance, typeof(MonoScript), false);
			if (monoScript != null) {
				string scriptClass = monoScript.GetClass ().ToString();
				string scriptPath = AssetDatabase.GetAssetPath(monoScript);
				action.SetScript(scriptClass, scriptPath, (Object) monoScript);
			} else {
				action.SetScript(null, null, null);
			}

			EditorGUILayout.Space ();

			 // Method selection field
			if (monoScript != null) {
				if (!monoScript.GetClass ().IsSubclassOf(typeof(ActionLibrary))) {
					EditorGUILayout.HelpBox (string.Format ("{0} is not a subclass of Hivemind.ActionLibrary", monoScript.GetClass().ToString()), MessageType.Warning);
				} else {
					EditorGUILayout.LabelField(monoScript.GetClass ().ToString (), SubtitleStyle);
					EditorGUILayout.Space ();
					System.Type type = monoScript.GetClass();
					MethodInfo[] methods = type.GetMethods();
					List<string> options = new List<string>();
					options.Add (" ");
					foreach (MethodInfo method in methods) {
						object[] attrs = method.GetCustomAttributes (typeof(ActionAttribute), false);
						if (attrs.Length > 0) {
							options.Add (method.Name);
						}
					}
					string[] opts = options.ToArray();
					
					action.methodName = opts[EditorGUILayout.Popup("Method", IndexOf (opts, action.methodName), opts)];
				}
			}

			EditorGUILayout.Space ();

			// Method parameters
			foreach (KeyValuePair<string, Action.ActionParameter> parameter in action.Parameters) {
				parameter.Value.Value = DrawParamControl(parameter.Value.Type, parameter.Key, parameter.Value.Value);
			}

			EditorGUILayout.Space ();

			// Contextual Inputs
			if (action.Inputs.Count > 0) {
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField(new GUIContent("Inputs"), SubtitleStyle);
				foreach (KeyValuePair<string, System.Type> input in action.Inputs) {
					EditorGUILayout.Space ();
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(input.Value.ToString ());
					EditorGUILayout.SelectableLabel(input.Key);
					EditorGUILayout.EndHorizontal();
				}
			}

			// Contextual Outputs
			if (action.Outputs.Count > 0) {
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField(new GUIContent("Outputs"), SubtitleStyle);
				foreach (KeyValuePair<string, System.Type> output in action.Outputs) {
					EditorGUILayout.Space ();
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(output.Value.ToString ());
					EditorGUILayout.SelectableLabel(output.Key);
					EditorGUILayout.EndHorizontal();
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		private object DrawParamControl(System.Type type, string label, object value) {
			if (type == typeof(string)) {
				return EditorGUILayout.TextField(label, (string) value);
			}
			else if (type == typeof(float)) {
				return EditorGUILayout.FloatField(label, (float) value);
			}
			else if (type == typeof(bool)) {
				return EditorGUILayout.Toggle(label, (bool) value);
			}
			else {
				string msg = string.Format("{0}: parameters of type \"{1}\" are not supported", label, type.ToString());
				EditorGUILayout.HelpBox(msg, MessageType.Warning);
				return null;
			}
		}

		public void DrawInspector(Selector node) {
			EditorGUILayout.LabelField(new GUIContent("Selector"), TitleStyle);
			EditorGUILayout.Space ();
			node.rememberRunning = EditorGUILayout.Toggle("Remember running child", node.rememberRunning);
			string message = "The Selector node ticks its children sequentially from left to right, until one of them returns SUCCESS, RUNNING or ERROR, at which point it returns that state. If all children return the failure state, the priority also returns FAILURE.";
			EditorGUILayout.HelpBox(message, MessageType.Info);
			message = "If \"Remember running chikd\" is on, when a child returns RUNNING the Selector will remember that child, and in future ticks it will skip directly to that child until it returns something other than RUNNING.";
			EditorGUILayout.HelpBox(message, MessageType.Info);
		}

		public void DrawInspector(Sequence node) {
			EditorGUILayout.LabelField(new GUIContent("Sequence"), TitleStyle);
			EditorGUILayout.Space ();
			node.rememberRunning = EditorGUILayout.Toggle("Remember running child", node.rememberRunning);
			string message = "The Sequence node ticks its children sequentially from left to right, until one of them returns FAILURE, RUNNING or ERROR, at which point the Sequence returns that state. If all children return the success state, the sequence also returns SUCCESS.";
			EditorGUILayout.HelpBox(message, MessageType.Info);
			message = "If \"Remember running child\" is on, when a child returns RUNNING the Sequence will remember that child, and in future ticks it will skip directly to that child until it returns something other than RUNNING.";
			EditorGUILayout.HelpBox(message, MessageType.Info);
		}

		public void DrawInspector(Parallel node) {
			EditorGUILayout.LabelField(new GUIContent("Parallel"), TitleStyle);
			EditorGUILayout.Space ();


			node.strategy = (Parallel.ResolutionStrategy) EditorGUILayout.EnumPopup("Return Strategy", node.strategy);
			string message = "The parallel node ticks all children sequentially from left to right, regardless of their return states. It returns SUCCESS if the number of succeeding children is larger than a local constant S, FAILURE if the number of failing children is larger than a local constant F or RUNNING otherwise.";
			EditorGUILayout.HelpBox(message, MessageType.Info);
			EditorGUILayout.HelpBox("Not yet implemented!", MessageType.Error);
		}

		public void DrawInspector(Repeater node) {
			EditorGUILayout.LabelField(new GUIContent("Repeater"), TitleStyle);
			EditorGUILayout.Space ();
			string message = "Repeater decorator sends the tick signal to its child every time that its child returns a SUCCESS or FAILURE.";
			EditorGUILayout.HelpBox(message, MessageType.Info);
			EditorGUILayout.HelpBox("Not yet implemented!", MessageType.Error);
		}

		public void DrawInspector(Inverter node) {
			EditorGUILayout.LabelField(new GUIContent("Inveter"), TitleStyle);
			EditorGUILayout.Space ();
			string message = "Like the NOT operator, the inverter decorator negates the result of its child node, i.e., SUCCESS state becomes FAILURE, and FAILURE becomes SUCCESS. RUNNING or ERROR states are returned as is.";
			EditorGUILayout.HelpBox(message, MessageType.Info);
			EditorGUILayout.HelpBox("Not yet implemented!", MessageType.Error);
		}

		public void DrawInspector(Succeeder node) {
			EditorGUILayout.LabelField(new GUIContent("Succeeder"), TitleStyle);
			EditorGUILayout.Space ();
			string message = "Succeeder always returns a SUCCESS, no matter what its child returns.";
			EditorGUILayout.HelpBox(message, MessageType.Info);
			EditorGUILayout.HelpBox("Not yet implemented!", MessageType.Error);
		}

		public void DrawInspector(UntilSucceed node) {
			EditorGUILayout.LabelField(new GUIContent("Repeat Until Succeed"), TitleStyle);
			EditorGUILayout.Space ();
			string message = "This decorator keeps calling its child until the child returns a SUCCESS value. When this happen, the decorator return a SUCCESS state.";
			EditorGUILayout.HelpBox(message, MessageType.Info);
			EditorGUILayout.HelpBox("Not yet implemented!", MessageType.Error);
		}

		public void DrawInspector(RandomSelector node) {
			EditorGUILayout.LabelField(new GUIContent("Random Selector"), TitleStyle);
			EditorGUILayout.Space ();
			if (node.ChildCount > 0) {
				float chance = 100f / node.ChildCount;
				EditorGUILayout.LabelField(new GUIContent("Each child has a " + chance.ToString ("F1") + "% chance of being selected."), SubtitleStyle);
			}
			string message = "The Random Selector sends the tick signal to one of its children, selected at random, and returns the state returned by that child.";
			EditorGUILayout.HelpBox(message, MessageType.Info);
			EditorGUILayout.HelpBox("Not yet implemented!", MessageType.Error);
		}

		public void DrawError() {
			EditorGUILayout.LabelField ("Selected node is invalid");
		}

	}

}