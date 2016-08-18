using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController {
	
	public LayerMask passengerMask;

	public Vector3 move;


	public override void Start(){	
		base.Start();
	}

	void Update(){
		UpdateRaycastOrigins();

		Vector3 velocity = move * Time.deltaTime;

		MovePassengers(velocity);
		transform.Translate(velocity);
	}

	//Move passengers
	void MovePassengers(Vector3 velocity){
		HashSet<Transform> movedPassengers = new HashSet<Transform>();

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

						hit.transform.Translate(new Vector3(pushX, pushY));
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
						float pushY = 0;
						float pushX = velocity.x - (hit.distance - skinWidth) * directionY;

						hit.transform.Translate(new Vector3(pushX, pushY));
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

}

