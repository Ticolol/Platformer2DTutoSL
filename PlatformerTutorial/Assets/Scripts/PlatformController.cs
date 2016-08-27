using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController {
	
	public LayerMask passengerMask;

	//public Vector3 move;
	public Vector3[] localWaypoints;
	Vector3[] globalWaypoints;
	
	int fromWaypointIndex;
	float percentBetweenWaypoints;
	float nextMoveTime;

	public float speed;
	public bool cyclic = true;
	public float waitTime = 0;
	[Range(0,2)]
	public float easeAmount = 0;

	List<PassengerMovement> passengerMovement;
	Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

	public override void Start(){	
		base.Start();
		percentBetweenWaypoints = 0;
		nextMoveTime = 0;

		//Sets globalWaypoints to lock waypoints in position when platform is moving
		globalWaypoints = new Vector3[localWaypoints.Length];
		for(int i=0; i<localWaypoints.Length; i++){
			globalWaypoints[i] = localWaypoints[i] + transform.position;
		}

	}

	void Update(){
		UpdateRaycastOrigins();

		Vector3 velocity = CalculatePlatformMovement();

		CalculatePassengersMovement(velocity);

		MovePassengers(true);
		transform.Translate(velocity);
		MovePassengers(false);
	}

	/*Implements easing based on the following equation:
		f(x) =  x^a / (x^a + (1-x)^a)		
	  Best results are found when a belongs to the interval [1,3]*/
	float Ease(float x){
		float a = easeAmount+1;
		return Mathf.Pow(x,a) / (Mathf.Pow(x,a) + Mathf.Pow(1-x,a));
	}

	//Calculate next waypoint to move. Uses well known interpolation via Lerp
	Vector3 CalculatePlatformMovement(){
		//Assure wait time in waypoint works properly
		if(Time.time < nextMoveTime)
			return Vector3.zero;
		//Assure cyclicality of waypoints by getting the division's remainder
		fromWaypointIndex %= globalWaypoints.Length;
		int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
		float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
		percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
		//Ease function gets crazy when x exceeds 1
		percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
		//Apply easing to percentage of Lerp
		float easedPercent = Ease(percentBetweenWaypoints);

		//LERP
		Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercent);

		if(percentBetweenWaypoints >= 1){
			percentBetweenWaypoints = 0;
			fromWaypointIndex++;
			if(!cyclic && fromWaypointIndex >= localWaypoints.Length-1){
				fromWaypointIndex = 0;
				//After passing through al waypoints, start reverse path
				System.Array.Reverse(globalWaypoints);
			}
			nextMoveTime = Time.time + waitTime;
		}

		return newPos - transform.position;	
	}

	void MovePassengers(bool beforeMovePlatform){
		//Move each passenger using passengerMovement. 
		//Order os movement occurs based on the movement direction
		//The main rule is: the agent on the front moves first
		foreach(PassengerMovement passenger in passengerMovement){
			//Let's store references to the Controller2D using a dictionary to minimize the use of GetComponents every frame
			if(!passengerDictionary.ContainsKey(passenger.transform)){
				passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
			}
			if(passenger.moveBeforePlatform == beforeMovePlatform){
				//Move passenger
				passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
			}
		}
	}

	//Move passengers
	void CalculatePassengersMovement(Vector3 velocity){
		HashSet<Transform> movedPassengers = new HashSet<Transform>();
		passengerMovement = new List<PassengerMovement>();

		float directionY = Mathf.Sign(velocity.y);
		float directionX = Mathf.Sign(velocity.x);

		//Vertically moving platform
		if(velocity.y != 0){
			float rayLength = Mathf.Abs(velocity.y) + skinWidth;
			//Raycast upward searching for passengers
			for(int i=0; i < verticalRayCount; i++){
				Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
				rayOrigin += Vector2.right * verticalRaySpacing * i;
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

				if(hit && hit.distance != 0){
					if(!movedPassengers.Contains(hit.transform)){
						movedPassengers.Add(hit.transform);
						float pushX = (directionY == 1) ? velocity.x : 0;
						float pushY = velocity.y - (hit.distance - skinWidth) * directionY;
						
						//Save push informations to the List of PassengersMovement
						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
						//hit.transform.Translate(new Vector3(pushX, pushY));
					}
				}
			}
		}

		//Horizontally moving platform
		if(velocity.x != 0){
			float rayLength = Mathf.Abs(velocity.x) + skinWidth;
			//Raycast upward searching for passengers
			for(int i=0; i < horizontalRayCount; i++){
				Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
				rayOrigin += Vector2.up * horizontalRaySpacing * i;
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

				if(hit && hit.distance != 0){
					if(!movedPassengers.Contains(hit.transform)){
						movedPassengers.Add(hit.transform);
						float pushY = -skinWidth;
						float pushX = velocity.x - (hit.distance - skinWidth) * directionY;
						//Save push informations to the List of PassengersMovement
						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
						//hit.transform.Translate(new Vector3(pushX, pushY));
					}
				}
			}
		}

		//Move passengers horizontally or downwards
		if(directionY == -1 || velocity.y == 0 && velocity.x != 0){
			float rayLength = 2 * skinWidth;
			//Raycast upward searching for passengers
			for(int i=0; i < verticalRayCount; i++){
				Vector2 rayOrigin = raycastOrigins.topLeft;
				rayOrigin += Vector2.right * verticalRaySpacing * i;
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

				if(hit && hit.distance != 0){
					if(!movedPassengers.Contains(hit.transform)){
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x;
						float pushY = velocity.y;

						hit.transform.Translate(new Vector3(pushX, pushY));
					}
				}
			}
		}


	}

	struct PassengerMovement {
		public Transform transform;
		public Vector3 velocity;
		public bool standingOnPlatform;
		public bool moveBeforePlatform;

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform){
			transform = _transform;
			velocity = _velocity;
			standingOnPlatform = _standingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
		}
	}

	//Draws gizmos for points the platform has to follow
	void OnDrawGizmos(){
		if(localWaypoints != null){
			Gizmos.color = Color.red;
			float size = .3f;

			for(int i=0; i < localWaypoints.Length; i++){
				Vector3 globalWaypointPos = ((Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position);
				Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
				Gizmos.DrawLine(globalWaypointPos - Vector3.right * size, globalWaypointPos + Vector3.right * size);

			}
		}
	}

}

