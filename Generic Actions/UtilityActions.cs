using UnityEngine;
using System.Collections;

namespace Hivemind {
	
	public class UtilityActions : ActionLibrary {

		[Hivemind.Action("tag", "maxDistance")]
		[Hivemind.Outputs("gameObject", typeof(GameObject))]
		public Hivemind.Result FindNearestObjectWithTag(string tag, float maxDistance) {
			
			GameObject[] nodes = GameObject.FindGameObjectsWithTag (tag);
			
			float nearestDistance = float.PositiveInfinity;
			GameObject nearestNode = null;
			foreach (GameObject node in nodes) {
				float distance = Vector3.Distance(node.transform.position, agent.transform.position);
				if (distance < nearestDistance) {
					nearestDistance = distance;
					nearestNode = node;
				}
			}
			
			if (nearestNode != null) {
				context.Set<GameObject> ("gameObject", nearestNode);
				return new Hivemind.Result { status=Hivemind.Status.Success };
			} else {
				return new Hivemind.Result { status=Hivemind.Status.Failure };
			}
		}
		
		[Hivemind.Action("seconds")]
		public Hivemind.Result Wait(float seconds) {
			float time = context.Get<float>("timeWaited", 0f);
			if (time < seconds) {
				time += Time.deltaTime;
				context.Set<float>("timeWaited", time);
				return new Hivemind.Result { status = Hivemind.Status.Running };
			} else {
				context.Unset("timeWaited");
				return new Hivemind.Result { status = Hivemind.Status.Success };
			}
		}
		
	}
	
	
}