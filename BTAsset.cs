using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Hivemind {

	[System.Serializable]
	public class BTAsset : ScriptableObject {

		public string serializedBehaviorTree;
		public BehaviorTree behaviorTree;

		public BehaviorTree Deserialize() {
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(serializedBehaviorTree);

			// Behavior Tree
			XmlElement btEl = (XmlElement) doc.GetElementsByTagName("behaviortree").Item(0);
			BehaviorTree bt = ScriptableObject.CreateInstance<BehaviorTree>();
			bt.title = btEl.GetAttribute("title");

			// Root
			XmlElement rootEl = (XmlElement) doc.GetElementsByTagName("root").Item(0);
			Root root = (Root) DeserializeSubTree(rootEl, bt);
			bt.SetRoot(root);

			// Unparented nodes
			XmlElement unparentedRoot = (XmlElement) doc.GetElementsByTagName("unparented").Item (0);
			foreach (XmlNode xmlNode in unparentedRoot.ChildNodes) {
				XmlElement el = xmlNode as XmlElement;
				if (el != null)
					DeserializeSubTree(el, bt);
			}

			behaviorTree = bt;
			return bt;
		}

		private Node DeserializeSubTree(XmlElement el, BehaviorTree bt) {
			Node node = null;

			if (el.Name == "root") node = ScriptableObject.CreateInstance<Root>();
			else if (el.Name == "action") node = ScriptableObject.CreateInstance<Action>();
			
			else if(el.Name == "sequence") node = ScriptableObject.CreateInstance<Sequence>();
			else if(el.Name == "selector") node = ScriptableObject.CreateInstance<Selector>();
			else if(el.Name == "randomselector") node = ScriptableObject.CreateInstance<RandomSelector>();
			else if(el.Name == "parallel") node = ScriptableObject.CreateInstance<Parallel>();
			
			else if(el.Name == "repeater") node = ScriptableObject.CreateInstance<Repeater>();
			else if(el.Name == "untilsucceed") node = ScriptableObject.CreateInstance<UntilSucceed>();
			else if(el.Name == "inverter") node = ScriptableObject.CreateInstance<Inverter>();
			else if(el.Name == "succeeder") node = ScriptableObject.CreateInstance<Succeeder>();
			
			else
				throw new System.NotImplementedException(string.Format ("{0} deserialization not implemented", el.Name));

			float x = float.Parse (el.GetAttribute("editorx"));
			float y = float.Parse (el.GetAttribute("editory"));
			node.editorPosition = new Vector2(x, y);
			
			if (node is Action) {
				((Action) node).Deserialize(el);
			}
			
			bt.nodes.Add(node);

			foreach (XmlNode xmlNode in el.ChildNodes) {
				XmlElement childEl = xmlNode as XmlElement;
				if (childEl != null && childEl.Name != "param") {
					Node child = DeserializeSubTree(childEl, bt);
					node.ConnectChild(child);
				}
			}

			return node;
		}
		
		public void Serialize(BehaviorTree behaviorTree) {

			// XML Document
			XmlDocument doc = new XmlDocument();

			// Behavior Tree
			XmlElement btEl = doc.CreateElement("behaviortree");
			btEl.SetAttribute("title", behaviorTree.title);
			doc.AppendChild(btEl);

			// Root SubTree
			SerializeSubTree (behaviorTree.rootNode, btEl);

			// Unparented nodes root
			XmlElement unparentedEl = doc.CreateElement("unparented");
			btEl.AppendChild(unparentedEl);

			// Unparented nodes
			for (int i = 0; i < behaviorTree.nodes.Count; i++) {
				if (behaviorTree.nodes[i].parent == null && !(behaviorTree.nodes[i] is Root)) {
					SerializeSubTree(behaviorTree.nodes[i], unparentedEl);
				}
			}

			serializedBehaviorTree = doc.InnerXml;
		}

		private void SerializeSubTree(Node node, XmlElement parentEl) {
		
			XmlDocument doc = parentEl.OwnerDocument;

			string tagName = TagForNodeType(node.GetType());
			XmlElement el = doc.CreateElement(tagName);
			el.SetAttribute("editorx", node.editorPosition.x.ToString());
			el.SetAttribute("editory", node.editorPosition.y.ToString());

			if (node is Action) {
				el = ((Action)node).Serialize(doc);
			}

			parentEl.AppendChild(el);

			int count = node.ChildCount;
			for (int i = 0; i < count; i ++) {
				SerializeSubTree(node.Children[i], el);
			}
		}

		private string TagForNodeType(System.Type nodeType) {
			if (nodeType == typeof(Root)) return "root";
			else if (nodeType == typeof(Action)) return "action";

			else if (nodeType == typeof(Sequence)) return "sequence";
			else if (nodeType == typeof(Selector)) return "sequence";
			else if (nodeType == typeof(RandomSelector)) return "sequence";
			else if (nodeType == typeof(Parallel)) return "sequence";

			else if (nodeType == typeof(Repeater)) return "sequence";
			else if (nodeType == typeof(UntilSucceed)) return "sequence";
			else if (nodeType == typeof(Succeeder)) return "sequence";
			else if (nodeType == typeof(Inverter)) return "sequence";

			else return "node";

		}
	}
	
}