using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

/*
 * Best practices for serialization:
 * - Don't use the `new` constructor
 * - Instead use ScriptableObject.CreateInstance()
 * - For initialization, use OnEnable() instead of the constructor
 * 
 * Unity calls the constructor, deserializes the data (populating the object) and THEN calls OnEnable(),
 * so the data is guaranteed to be there in this method.
 * 
 */

namespace Hivemind {

	public enum Status
	{
		Success,
		Failure,
		Running,
		Error
	}

	public class Context {
		private Dictionary<string, object> context = new Dictionary<string, object>();

		public Dictionary<string, object> All {
			get { return context; }
		}

		public bool ContainsKey(string key) {
			return context.ContainsKey(key);
		}
		
		public T Get<T>(string key) {
			if (!context.ContainsKey(key)) {
				throw new System.MissingMemberException(string.Format ("Key {0} not found in the current context", key));
			}
			T value = (T) context[key];
			return value;
		}

		public T Get<T>(string key, T defaultValue) {
			if (!context.ContainsKey(key)) {
				Set<T>(key, defaultValue);
				return defaultValue;
			}
			T value = (T) context[key];
			return value;
		}
		
		public void Set<T>(string key, T value) {
			context[key] = value;
		}

		public void Unset(string key) {
			context.Remove (key);
		}
	}

	[System.Serializable]
	public class BehaviorTree : ScriptableObject {

		public static string[] NodeTypes = {
			"Action", "Inverter", "Parallel",
			"RandomSelector", "Repeater", "Selector",
			"Sequence", "Succeeder", "UntilSucceed"
		};

		public Root rootNode;
		public List<Node> nodes = new List<Node>();

		public bool debugMode = false;
		public Node currentNode = null;
		public int TotalTicks { get; private set; }

		public delegate void NodeWillTick(Node node);
		public delegate void NodeDidTick(Node node, Status result);

		public NodeWillTick nodeWillTick;
		public NodeDidTick nodeDidTick;

		public void SetRoot(Root root) {
			rootNode = root;
			nodes.Add(root);
			root.behaviorTree = this;
		}

		public T CreateNode<T>() where T : Node {
			T node = (T) ScriptableObject.CreateInstance<T>();
			node.behaviorTree = this;
			return node;
		}
		
		// Lifecycle
		public void OnEnable() {
			hideFlags = HideFlags.HideAndDontSave;
		}

		public void OnDestroy() {
			foreach (Node node in nodes) {
				DestroyImmediate (node);
			}
		}

		public Status Tick(GameObject agent, Context context) {
			BehaviorTreeAgent btAgent = agent.GetComponent<BehaviorTreeAgent>();
			if (btAgent) {
				debugMode = btAgent.debugMode;
			}

			TotalTicks++;

			Status result = rootNode.Tick(agent, context);
			rootNode.lastStatus = result;
			rootNode.lastTick = TotalTicks;
			return result;
		}

		public Status Tick(Node node, GameObject agent, Context context) {

			if (nodeWillTick != null && node != currentNode)
				nodeWillTick(node);

			Status result = node.Tick(agent, context);

			if (nodeDidTick != null)
				nodeDidTick(node, result);

			currentNode = node;
			node.lastStatus = result;
			node.lastTick = TotalTicks;
			return result;
		}
	}


	[System.Serializable]
	public class Node : ScriptableObject, System.IComparable {

		// Editor settings
		[SerializeField]
		public Vector2 editorPosition;

		// The Behavior Tree this node belongs too
		public BehaviorTree behaviorTree;

		// Used by the debugger to visually display the last status returned
		public Status? lastStatus;
		public int lastTick = 0;
		
		// Child connections
		public virtual void ConnectChild(Node child) {}
		public virtual void DisconnectChild(Node child) {}
		public virtual List<Node> Children { get { return null;} }
		public virtual int ChildCount { get { return 0; } }
		public virtual bool CanConnectChild { get { return false; } }
		public virtual bool ContainsChild(Node child) { return false; }

		// IComparable for sorting left-to-right in the visual editor
		public int CompareTo(object other) {
			Node otherNode = other as Node;
			return editorPosition.x < otherNode.editorPosition.x ? -1 : 1;
		}
		
		// Parent connections
		[SerializeField]
		Node _parent;
		public virtual Node parent {
			get { return _parent; }
			set {
				
				if (value == null && _parent.ContainsChild(this)) {
					throw new System.InvalidOperationException(string.Format ("Cannot set parent of {0} to null because {1} still contains it in its children", this, value));
				} else if (value == null || (value != null && value.ContainsChild(this))) {
					_parent = value;
				} else {
					throw new System.InvalidOperationException(string.Format ("{0} must contain {1} as a child before setting the child parent property", value, this));
				}
				
			}
		}
		public virtual void Unparent() {
			if (_parent != null) {
				_parent.DisconnectChild(this);
			} else {
				Debug.LogWarning(string.Format ("Attempted unparenting {0} while it has no parent"));
			}
		}

		public List<Node> Ancestors() {
			List<Node> ancestorNodes = new List<Node>();
			if (parent != null) parent.Ancestors (ref ancestorNodes);
			return ancestorNodes;
		}
		private void Ancestors(ref List<Node> ancestorNodes) {
			ancestorNodes.Add (this);
			if (parent != null) parent.Ancestors (ref ancestorNodes);
		}
		
		// All connections
		public virtual void Disconnect() {
			
			// Disconnect parent
			if (parent != null) {
				Unparent();
			}
			
			// Disconnect children
			if (ChildCount > 0) {
				for (int i = ChildCount - 1; i >= 0; i--) {
					DisconnectChild(Children[i]);
				}
			}
		}

		// Lifecycle
		public void OnEnable() {
			hideFlags = HideFlags.HideAndDontSave;
		}
		
		// Runtime
		public virtual Status Tick(GameObject agent, Context context) { return Status.Error; }
	}

	// Root ------------------------------------------------------------------------------------------------------------------------------------------------------

	[System.Serializable]
	public class Root : Node {

		// Child connections
		[SerializeField]
		Node _child;
		
		public override void ConnectChild(Node child) {
			if (_child == null) {
				_child = child;
				child.parent = this;
			} else {
				throw new System.InvalidOperationException(string.Format ("{0} already has a connected child, cannot connect {1}", this, child));
			}
		}
		
		public override void DisconnectChild(Node child) {
			if (_child == child) {
				_child = null;
				child.parent = null;
			} else {
				throw new System.InvalidOperationException(string.Format ("{0} is not a child of {1}", child, this));
			}
		}
		
		public override List<Node> Children {
			get {
				List<Node> nodeList = new List<Node>();
				nodeList.Add(_child);
				return nodeList;
			}
		}

		public override int ChildCount {
			get { return _child != null ? 1 : 0; }
		}

		public override bool CanConnectChild {
			get { return _child == null; }
		}

		public override bool ContainsChild (Node child)
		{
			return _child == child;
		}

		// Parent Connections

		public override Node parent {
			get { return null; }
			set { throw new System.InvalidOperationException("The Root node cannot have a parent connection"); }
		}
		public override void Unparent() {
			throw new System.InvalidOperationException("The Root node cannot have a parent connection");
		}

		// Runtime

		public override Status Tick(GameObject agent, Context context)
		{
			Status result = Status.Error;
			if (_child != null) {
				result = behaviorTree.Tick (_child, agent, context);
			}
			return result;
		}
	}
	
	// Composite nodes ------------------------------------------------------------------------------------------------------------------------------------------------------

	[System.Serializable]
	public abstract class Composite : Node
	{

		// Child connections
		[SerializeField]
		List<Node> _children = new List<Node>();
		
		public override void ConnectChild(Node child) {
			_children.Add (child);
			child.parent = this;
		}
		
		public override void DisconnectChild(Node child) {
			if (_children.Contains(child)) {
				_children.Remove (child);
				child.parent = null;
			} else {
				throw new System.InvalidOperationException(string.Format ("{0} is not a child of {1}", child, this));
			}
		}

		public void SortChildren() {
			_children.Sort ();
		}
		
		public override List<Node> Children { get { return _children; } }

		public override int ChildCount { get { return _children.Count; } }

		public override bool CanConnectChild { get { return true; } }

		public override bool ContainsChild(Node child) { return _children.Contains (child); }

		// Runtime

		public override abstract Status Tick(GameObject agent, Context context);
	}

	[System.Serializable]
	public class Selector : Composite
	{
		[SerializeField]
		int lastRunning = 0;

		public bool rememberRunning = false;
		
		public override Status Tick(GameObject agent, Context context)
		{
			int start = rememberRunning ? lastRunning : 0;
			for (int i = start; i < ChildCount; i++)
			{
				Node node = Children[i];
				Status status = behaviorTree.Tick(node, agent, context);
				if (status != Status.Failure)
				{
					lastRunning = status == Status.Running ? i : 0;
					return status;
				}
			}
			return Status.Failure;
		}

		// Serialization
		public void Serialize(ref XmlElement el) {
			el.SetAttribute("remember", rememberRunning ? bool.TrueString : bool.FalseString);
		}
		
		// Deserialization
		public void Deserialize(XmlElement el) {
			rememberRunning = bool.Parse(el.GetAttribute("remember"));
		}
	}

	[System.Serializable]
	public class RandomSelector : Composite
	{	
		public override Status Tick(GameObject agent, Context context)
		{
			throw new System.NotImplementedException ();
		}
	}

	[System.Serializable]
	public class Sequence : Composite
	{
		[SerializeField]
		int lastRunning = 0;

		public bool rememberRunning = false;
		
		public override Status Tick(GameObject agent, Context context)
		{
			int start = rememberRunning ? lastRunning : 0;
			for ( int i = start; i < ChildCount; i++)
			{
				Node node = Children[i];
				Status status = behaviorTree.Tick(node, agent, context);
				if (status != Status.Success)
				{
					lastRunning =  status == Status.Running ? i : 0;
					return status;
				} else {
					lastRunning = 0;
				}
				
			}
			return Status.Success;
		}

		// Serialization
		public void Serialize(ref XmlElement el) {
			el.SetAttribute("remember", rememberRunning ? bool.TrueString : bool.FalseString);
		}

		// Deserialization
		public void Deserialize(XmlElement el) {
			rememberRunning = bool.Parse(el.GetAttribute("remember"));
		}
	}

	[System.Serializable]
	public class Parallel : Composite
	{
		public enum ResolutionStrategy {
			FirstSuccess, FirstFailure, FirstResolved, AllResolved
		}
		public ResolutionStrategy strategy = ResolutionStrategy.FirstSuccess;

		[SerializeField]
		List<Node> stillRunning;
		List<Status> results;
		
		public override Status Tick(GameObject agent, Context context)
		{
			// Initialize stillRunning with all child nodes
//			if (stillRunning == null) {
//				results = new List<Status>();
//				stillRunning = new List<Node>();
//				for ( int i = 0; i < ChildCount; i++) {
//					stillRunning.Add (Children[i]);
//				}
//			}
//
//			foreach (Node node in stillRunning) {
//				Status status = behaviorTree.Tick (node, agent, context);
//				if (status != Status.Running) {
//					stillRunning.Remove (node);
//				}
//				results.Add (status);
//
//				if (strategy == ResolutionStrategy.FirstSuccess && status == Status.Success)
//					return Status.Success;
//				else if (strategy == ResolutionStrategy.FirstFailure && status == Status.Failure)
//					return Status.Failure;
//				else if (status == Status.Error)
//					return Status.Error;
//			}
//
//			switch(strategy) {
//			case ResolutionStrategy.FirstSuccess:
//				break;
//			}

			return Status.Error;
		}
	}
	
	// Decorator nodes ------------------------------------------------------------------------------------------------------------------------------------------------------

	[System.Serializable]
	public abstract class Decorator : Node
	{
		// Child connections
		[SerializeField]
		Node _child;
		
		public override void ConnectChild(Node child) {
			if (_child == null) {
				_child = child;
				child.parent = this;
			} else {
				throw new System.InvalidOperationException(string.Format ("{0} already has a connected child, cannot connect {1}", this, child));
			}
		}
		
		public override void DisconnectChild(Node child) {
			if (_child == child) {
				_child = null;
				child.parent = null;
			} else {
				throw new System.InvalidOperationException(string.Format ("{0} is not a child of {1}", child, this));
			}
		}
		
		public override List<Node> Children {
			get {
				List<Node> nodeList = new List<Node>();
				nodeList.Add(_child);
				return nodeList;
			}
		}

		public override int ChildCount {
			get { return _child != null ? 1 : 0; }
		}

		public override bool CanConnectChild {
			get { return _child == null; }
		}

		public override bool ContainsChild(Node child) {
			return _child == child;
		}

		// Runtime

		public override Status Tick(GameObject agent, Context context)
		{
			throw new System.NotImplementedException ();
		}
	}

	[System.Serializable]
	public class Repeater : Decorator
	{
		[SerializeField]
		int _repetitions;

		// Setting repetitions to zero will repeat forever
		public void SetRepetitions(int repetitions) {
			_repetitions = repetitions;
		}

		public int repetitions { get { return _repetitions; } }

		public override Status Tick(GameObject agent, Context context)
		{
			throw new System.NotImplementedException ();
		}
	}

	[System.Serializable]
	public class UntilSucceed : Decorator
	{
		public override Status Tick(GameObject agent, Context context)
		{
			throw new System.NotImplementedException ();
		}
	}

	[System.Serializable]
	public class Inverter : Decorator
	{
		public override Status Tick(GameObject agent, Context context)
		{
			throw new System.NotImplementedException ();
		}
	}

	[System.Serializable]
	public class Succeeder : Decorator
	{
		public override Status Tick(GameObject agent, Context context)
		{
			throw new System.NotImplementedException ();
		}
	}
}