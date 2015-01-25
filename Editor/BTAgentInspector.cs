using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Hivemind {
	
	[CustomEditor(typeof(BehaviorTreeAgent))]
	public class BTAgentInspector : Editor {

		private BTEditorManager _manager;

		public void OnEnable() {
			BehaviorTreeAgent btAgent = target as BehaviorTreeAgent;

			BehaviorTree bt = null;

			if (btAgent.behaviorTree == null) {
				btAgent.Awake();
			}
			bt = btAgent.behaviorTree;

			if (bt != null) {
				_manager = BTEditorManager.Manager;
				if (!_manager) {
					_manager = BTEditorManager.CreateInstance(bt, btAgent.btAsset);
				} else {
					_manager.behaviorTree = bt;
				}
				_manager.btInspector = this;
				_manager.inspectedAgent = btAgent;
			}

		}
		
		public void OnDisable() {
			if (!EditorApplication.isPlaying && _manager != null) {
				DestroyImmediate (_manager);
			} else if (_manager != null) {
				_manager.behaviorTree = null;
			}
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			if (_manager.selectedNode != null) {
				EditorGUILayout.LabelField("Last Status: " + _manager.selectedNode.lastStatus.ToString());
			}

			BehaviorTreeAgent btAgent = target as BehaviorTreeAgent;
			if (btAgent.context != null) {
				EditorGUILayout.LabelField("Current Context");
				foreach (KeyValuePair<string, object> item in btAgent.context.All) {
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(item.Key);
					EditorGUILayout.LabelField(item.Value.ToString ());
					EditorGUILayout.EndHorizontal();
				}
			}

			if (GUILayout.Button ("Show Behavior Tree editor")) {
				BTEditorWindow.ShowWindow ();
			}

		}
		
	}

}
