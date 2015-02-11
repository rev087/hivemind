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
			BehaviorTree bt = ScriptableObject.CreateInstance<BehaviorTree>();

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

			if (el.Name == "root") node = bt.CreateNode<Root>();
			else if (el.Name == "action") node = bt.CreateNode<Action>();
			
			else if(el.Name == "sequence") node = bt.CreateNode<Sequence>();
			else if(el.Name == "selector") node = bt.CreateNode<Selector>();
			else if(el.Name == "randomselector") node = bt.CreateNode<RandomSelector>();
			else if(el.Name == "parallel") node = bt.CreateNode<Parallel>();
			
			else if(el.Name == "repeater") node = bt.CreateNode<Repeater>();
			else if(el.Name == "untilsucceed") node = bt.CreateNode<UntilSucceed>();
			else if(el.Name == "inverter") node = bt.CreateNode<Inverter>();
			else if(el.Name == "succeeder") node = bt.CreateNode<Succeeder>();
			
			else
				throw new System.NotImplementedException(string.Format ("{0} deserialization not implemented", el.Name));

			float x = float.Parse (el.GetAttribute("editorx"));
			float y = float.Parse (el.GetAttribute("editory"));
			node.editorPosition = new Vector2(x, y);
			node.GUID = el.GetAttribute ("guid");
			
			if (node is Action) ((Action) node).Deserialize(el);
			else if (node is Sequence) ((Sequence) node).Deserialize(el);
			else if (node is Selector) ((Selector) node).Deserialize(el);
			
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
			el.SetAttribute("guid", node.GUID);

			if (node is Action) ((Action) node).Serialize(ref el);
			else if (node is Sequence) ((Sequence) node).Serialize(ref el);
			else if (node is Selector) ((Selector) node).Serialize(ref el);

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
			else if (nodeType == typeof(Selector)) return "selector";
			else if (nodeType == typeof(RandomSelector)) return "randomselector";
			else if (nodeType == typeof(Parallel)) return "parallel";

			else if (nodeType == typeof(Repeater)) return "repeater";
			else if (nodeType == typeof(UntilSucceed)) return "untilsucceed";
			else if (nodeType == typeof(Succeeder)) return "succeeder";
			else if (nodeType == typeof(Inverter)) return "inverter";

			else return "node";

		}
	}
	
}