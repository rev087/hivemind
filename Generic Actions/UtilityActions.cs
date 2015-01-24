using UnityEngine;
using System.Collections;

namespace Hivemind {
	
	public class UtilityActions : ActionLibrary {

		[Hivemind.Action]
		[Hivemind.Outputs("gameObject", typeof(GameObject))]
		public Hivemind.Result FindNearestObjectWithTag(string tag, float maxDistance) {
			
			GameObject[] gameObjects = GameObject.FindGameObjectsWithTag (tag);
			
			float nearestDistance = float.PositiveInfinity;
			GameObject nearestGameObject = null;

			foreach (GameObject gameObject in gameObjects) {
				float distance = Vector3.Distance(gameObject.transform.position, agent.transform.position);
				if (distance < nearestDistance) {
					nearestDistance = distance;
					nearestGameObject = gameObject;
				}
			}
			
			if (nearestGameObject != null) {
				context.Set<GameObject> ("gameObject", nearestGameObject);
				return new Hivemind.Result { status=Hivemind.Status.Success };
			} else {
				return new Hivemind.Result { status=Hivemind.Status.Failure };
			}
		}
		
		[Hivemind.Action]
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