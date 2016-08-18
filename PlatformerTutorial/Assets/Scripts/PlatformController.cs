using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController {
	
	public LayerMask passengerMask;

	public Vector3 move;

	List<PassengerMovement> passengerMovement;
	Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

	public override void Start(){	
		base.Start();
	}

	void Update(){
		UpdateRaycastOrigins();

		Vector3 velocity = move * Time.deltaTime;

		CalculatePassengersMovement(velocity);

		MovePassengers(true);
		transform.Translate(velocity);
		MovePassengers(false);
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

				if(hit){
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

				if(hit){
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

				if(hit){
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

}

