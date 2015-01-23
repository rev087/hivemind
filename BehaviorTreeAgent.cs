using UnityEngine;
using System.Collections;

namespace Hivemind {

	public class BehaviorTreeAgent : MonoBehaviour {
		public BTAsset behaviorTree;

		public void Start() {
			Debug.Log ("Starting");
		}

		public void RunBehaviorTree() {
			if (behaviorTree == null) {
				throw new MissingReferenceException("Behavior Tree not defined");
			}

			behaviorTree.behaviorTree.Run (this);
		}
	}

}