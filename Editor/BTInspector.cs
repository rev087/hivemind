using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Hivemind {

	public class HivemindInspector : Editor {

		private static GUIStyle _titleStyle;

		public static GUIStyle TitleStyle {
			get {
				if (_titleStyle == null) {
					_titleStyle = new GUIStyle();
					_titleStyle.fontSize = 14;
				}
				return _titleStyle;
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
				
				manager.behaviorTree.title = EditorGUILayout.TextField("Title", manager.behaviorTree.title);
				
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
			else if (node is Sequence) { DrawInspector ((Sequence) node); }
			else if (node is Repeater) { DrawInspector ((Repeater) node); }

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

		public void DrawInspector(Action node) {
			EditorGUILayout.LabelField(new GUIContent("Action"), TitleStyle);

			// MonoScript selection field
			Action action = (Action) serializedObject.targetObject;
			action.monoScript = (MonoScript) EditorGUILayout.ObjectField(action.monoScript, typeof(MonoScript), false);

			 // Method selection field
			if (action.monoScript != null) {
				System.Type type = action.monoScript.GetClass();
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

			// Method parameters
			foreach (KeyValuePair<string, Action.ActionParameter> parameter in action.EditorParameters) {
				parameter.Value.Value = DrawParamControl(parameter.Value.Type, parameter.Key, parameter.Value.Value);
			}

			serializedObject.ApplyModifiedProperties();
		}

		private object DrawParamControl(System.Type type, string label, object value) {
			if (type == typeof(string)) {
				if (value == null) value = "";
				return EditorGUILayout.TextField(label, (string) value);
			}
			else if (type == typeof(float)) {
				if (value == null) value = 0f;
				return EditorGUILayout.FloatField(label, (float) value);
			}
			return null;
		}

		public void DrawInspector(Sequence node) {
			EditorGUILayout.LabelField(new GUIContent("Sequence"), TitleStyle);
		}

		public void DrawInspector(Repeater node) {
			EditorGUILayout.LabelField(new GUIContent("Repeater"), TitleStyle);
		}
		
		public void DrawError() {
			EditorGUILayout.LabelField ("Selected node is invalid");
		}

	}

}