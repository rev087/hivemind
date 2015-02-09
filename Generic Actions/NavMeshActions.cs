using UnityEngine;
using System.Collections;

namespace Hivemind {

	public class NavMeshActions : ActionLibrary {
		
		[Hivemind.Action]
		[Hivemind.Expects("gameObject", typeof(GameObject))]
		public Hivemind.Status MoveToGameObject(string animationFloat, float animationFactor) {
			GameObject go = context.Get<GameObject> ("gameObject");
			NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
			Animator anim = agent.GetComponent<Animator>();
			NavMeshHit sampledDestination;
			NavMesh.SamplePosition(go.transform.position, out sampledDestination, 3f, 1);
			float distance = Vector3.Distance (agent.transform.position, sampledDestination.position);
			Debug.DrawRay (sampledDestination.position, Vector3.up, Color.green);

			// Planning path
			if (navMeshAgent.pathPending) {
				return Status.Running;
			}

			// Moving
			else if (distance > navMeshAgent.stoppingDistance) {
				navMeshAgent.SetDestination(sampledDestination.position);
				if (animationFloat != null) anim.SetFloat (animationFloat, animationFactor);
				return Status.Running;
			}
			
			// Reached destination
			else if (distance <= navMeshAgent.stoppingDistance) {
				if (animationFloat != null) anim.SetFloat (animationFloat, 0f);
				return Status.Success;
			}

			else {
				return Status.Error;
			}
		}

		[Hivemind.Action]
		[Hivemind.Expects("position", typeof(Vector3))]
		public Hivemind.Status MoveToPosition(string animationFloat, float animationFactor) {
			
			NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
			Animator anim = agent.GetComponent<Animator>();
			NavMeshHit sampledDestination;
			Vector3 position = context.Get<Vector3>("position");
			NavMesh.SamplePosition(position, out sampledDestination, 10f, 1);
			float distance = Vector3.Distance (agent.transform.position, sampledDestination.position);
			
			// Planning path
			if (navMeshAgent.pathPending) {
				return Status.Running;
			}

			// Moving towards destination
			else if (distance > navMeshAgent.stoppingDistance) {
				navMeshAgent.SetDestination(sampledDestination.position);
				if (animationFloat != null) anim.SetFloat (animationFloat, animationFactor);
				return Status.Running;
			}
			
			// Reached destination
			else if (distance <= navMeshAgent.stoppingDistance) {
				if (animationFloat != null) anim.SetFloat (animationFloat, 0f);
				return Status.Success;
			}
			
			// Can't reach destination
			else if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid) {
				return Status.Failure;
			}
			
			// Set destination
			else {
				return Status.Error;
			}
		}

		[Hivemind.Action]
		[Hivemind.Outputs("position", typeof(Vector3))]
		public Hivemind.Status GetRandomPosition(float radius) {
			Vector2 position = Random.insideUnitCircle * radius;
			NavMeshHit navMeshHit;
			bool sampleSuccessful = NavMesh.SamplePosition(position, out navMeshHit, 5f, 1);
			if (sampleSuccessful) {
				context.Set<Vector3>("position", navMeshHit.position);
				return Status.Success;
			} else {
				return Status.Failure;
			}

		}

	}
	

}