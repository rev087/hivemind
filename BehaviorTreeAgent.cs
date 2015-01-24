using UnityEngine;
using System.Collections;

namespace Hivemind {

	[AddComponentMenu("Miscellaneous/Behavior Tree Agent")]
	public class BehaviorTreeAgent : MonoBehaviour {
		public BTAsset btAsset;
		private BehaviorTree behaviorTree;
		private Context context;

		public void Start() {
			if (btAsset == null) {
				return;
			}
			context = new Context();
			behaviorTree = btAsset.Deserialize();

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