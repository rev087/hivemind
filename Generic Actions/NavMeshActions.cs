using UnityEngine;
using System.Collections;

namespace Hivemind {

	public class NavMeshActions : ActionLibrary {
		
		[Hivemind.Action]
		[Hivemind.Expects("gameObject", typeof(GameObject))]
		public Hivemind.Status MoveToGameObject() {
			GameObject go = context.Get<GameObject> ("gameObject");
			NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
			Animator anim = agent.GetComponent<Animator>();
			NavMeshHit sampledDestination;
			NavMesh.SamplePosition(go.transform.position, out sampledDestination, 10f, 1);
			float distance = Vector3.Distance (agent.transform.position, sampledDestination.position);

			if (!context.Get<bool>("didSetDestination", false)) {
				
				// Set destination
				navMeshAgent.SetDestination(sampledDestination.position);
				anim.SetBool ("IsWalking", true);
				context.Set<bool>("didSetDestination", true);
				return Status.Running;

			}
			else {
				
				// Planning path or moving towards destination
				if (navMeshAgent.pathPending || distance > navMeshAgent.stoppingDistance) {
					return Status.Running;
				}
				
				// Reached destination
				else if (distance <= navMeshAgent.stoppingDistance) {
					anim.SetBool ("IsWalking", false);
					context.Unset("didSetDestination");
					return Status.Success;
				}
				
				// Can't reach destination
				anim.SetBool ("IsWalking", false);
				context.Unset("didSetDestination");
				return Status.Failure;

			}
		}

		[Hivemind.Action]
		[Hivemind.Expects("position", typeof(Vector3))]
		public Hivemind.Status MoveToPosition() {
			
			NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
			Animator anim = agent.GetComponent<Animator>();
			NavMeshHit sampledDestination;
			Vector3 position = context.Get<Vector3>("position");
			NavMesh.SamplePosition(position, out sampledDestination, 10f, 1);
			float distance = Vector3.Distance (agent.transform.position, sampledDestination.position);
			
			// Planning path or moving towards destination
			if (navMeshAgent.pathPending || navMeshAgent.hasPath && distance > navMeshAgent.stoppingDistance) {
				return Status.Running;
			}
			
			// Reached destination
			else if (distance <= navMeshAgent.stoppingDistance) {
				anim.SetBool ("IsWalking", false);
				return Status.Success;
			}
			
			// Can't reach destination
			else if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid) {
				return Status.Failure;
			}
			
			// Set destination
			else {
				navMeshAgent.SetDestination(sampledDestination.position);
				anim.SetBool ("IsWalking", true);
				return Status.Running;
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