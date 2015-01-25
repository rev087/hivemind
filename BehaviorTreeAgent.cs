using UnityEngine;
using System.Collections;

namespace Hivemind {

	[AddComponentMenu("Miscellaneous/Behavior Tree Agent")]
	public class BehaviorTreeAgent : MonoBehaviour {
		public BTAsset btAsset;
		[HideInInspector]
		public BehaviorTree behaviorTree;
		public bool debugMode = false;
		public Context context;

		public void Awake() {
			behaviorTree = btAsset.Deserialize();
			context = new Context();
		}

		public void Start() {
			if (btAsset == null) {
				return;
			}
		}

		public void Update() {
			Tick();
		}

		public void Tick() {
			if (behaviorTree == null) {
				throw new MissingReferenceException("Behavior Tree not defined");
			}

			behaviorTree.Tick (gameObject, context);
		}
	}

}