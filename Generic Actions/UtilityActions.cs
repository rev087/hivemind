using UnityEngine;
using System.Collections;

namespace Hivemind {
	
	public class UtilityActions : ActionLibrary {

		[Hivemind.Action]
		[Hivemind.Outputs("gameObject", typeof(GameObject))]
		public Hivemind.Status FindNearestObjectWithTag(string tag, float maxDistance) {
			
			GameObject[] gameObjects = GameObject.FindGameObjectsWithTag (tag);
			
			float nearestDistance = float.PositiveInfinity;
			GameObject nearestGameObject = null;

			foreach (GameObject gameObject in gameObjects) {
				float distance = Vector3.Distance(gameObject.transform.position, agent.transform.position);
				if (distance < maxDistance && distance < nearestDistance) {
					nearestDistance = distance;
					nearestGameObject = gameObject;
				}
			}
			
			if (nearestGameObject != null) {
				context.Set<GameObject> ("gameObject", nearestGameObject);
				return Status.Success;
			} else {
				context.Unset("gameObject");
				return Status.Failure;
			}
		}
		
		[Hivemind.Action]
		public Hivemind.Status Wait(float seconds) {
			float time = context.Get<float>("timeWaited", 0f);
			if (time < seconds) {
				time += Time.deltaTime;
				context.Set<float>("timeWaited", time);
				return Status.Running;
			} else {
				context.Unset("timeWaited");
				return Status.Success;
			}
		}

		[Hivemind.Action]
		[Hivemind.Expects("gameObject", typeof(GameObject))]
		public Hivemind.Status LookAtGameObject() {
			GameObject gameObject = context.Get<GameObject>("gameObject");
			agent.transform.LookAt (gameObject.transform.position);
			return Status.Success;
		}

		[Hivemind.Action]
		[Hivemind.Expects("gameObject", typeof(GameObject))]
		public Hivemind.Status ObjectWithinDistance(float radius) {
			GameObject gameObject = context.Get<GameObject>("gameObject");
			if (Vector3.Distance(gameObject.transform.position, agent.transform.position) < radius) {
				return Status.Success;
			} else {
				return Status.Failure;
			}
		}

		[Hivemind.Action]
		[Hivemind.Expects("gameObject", typeof(GameObject))]
		public Hivemind.Status DestroyGameObject() {
			GameObject gameObject = context.Get<GameObject>("gameObject");
			GameObject.DestroyObject(gameObject);
			context.Unset ("gameObject");
			return Status.Success;
		}
		
	}
	
	
}