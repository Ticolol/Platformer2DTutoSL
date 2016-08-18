using UnityEngine;
using System.Collections;

public class Controller2D : RaycastController {


	public CollisionInfo collisions;

	float maxClimbAngle = 70;
	float maxDescendAngle = 70;


	public override void Start(){	
		base.Start();
	}
	
	/*void Update(){
		UpdateRaycastOrigins();
		CalculateRaySpacing();

		//draw vertical rays
		for(int i=0; i < verticalRayCount; i++){
			Debug.DrawRay(raycastOrigins.bottomLeft + Vector2.right * verticalRaySpacing * i,
						 Vector2.up * -2, Color.red);
		}
	}*/


	public void Move(Vector3 velocity, bool standingOnPlatform = false){
		UpdateRaycastOrigins();
		collisions.Reset();
		collisions.velocityOld = velocity;

		if(velocity.y < 0)
			DescendSlope(ref velocity);

		if(velocity.x != 0)
			HorizontalCollision(ref velocity);

		if(velocity.y != 0)
			VerticalCollision(ref velocity);

		transform.Translate(velocity);

		if(standingOnPlatform){
			collisions.below = true;
		}
	}

	void HorizontalCollision(ref Vector3 velocity){
		float directionX = Mathf.Sign(velocity.x);
		float rayLength = Mathf.Abs(velocity.x) + skinWidth;
		//draw vertical rays
		for(int i=0; i < horizontalRayCount; i++){
			//Bizarre yet quick n useful language
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			//another raycast style
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);			

			Debug.DrawRay(rayOrigin, Vector2.right * rayLength * directionX, Color.red);
			
			if(hit){
				//Treats the case of a platform, when passing through the player, messes with his movement
				if(hit.distance == 0)
					continue;

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if(i==0 && slopeAngle <= maxClimbAngle){
					//Treats the case of a descending slope meeting an ascending slope
					if(collisions.descendingSlope){
						collisions.descendingSlope = false;
						velocity = collisions.velocityOld;
					}
					//Corrections with slope distance
					float distanceToSlopeStart = 0;
					if(slopeAngle != collisions.slopeAngleOld){
						distanceToSlopeStart = hit.distance - skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					//ClimbSlope
					ClimbSlope(ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}else{
					//Correct velocity and length of ray
					velocity.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;

					//Correction of slope and wall from side combination
					if(collisions.climbingSlope){
						velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}

					//Set collisions left and right properly
					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}
		}
	}

	void VerticalCollision(ref Vector3 velocity){
		float directionY = Mathf.Sign(velocity.y);
		float rayLength = Mathf.Abs(velocity.y) + skinWidth;
		//draw vertical rays
		for(int i=0; i < verticalRayCount; i++){
			//Bizarre yet quick n useful language
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			//another raycast style
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);			
			//Debug.DrawRay
			Debug.DrawRay(rayOrigin, Vector2.up * rayLength * directionY, Color.red);

			if(hit){
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				//Correction of slope and wall from above combination
				if(collisions.climbingSlope){
					velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
				}

				//Set collisions above and below properly
				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}

		//Remove 1 frame trap when changing slopes
		if(collisions.climbingSlope){
			float directionX = Mathf.Sign(velocity.x);
			rayLength = Mathf.Abs(velocity.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			if(hit){
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if(slopeAngle != collisions.slopeAngle){
					velocity.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}

		}
	}

	//Recalculate velocity vector to properly climb the slope
	void ClimbSlope(ref Vector3 velocity, float slopeAngle){
		float moveDistance = Mathf.Abs(velocity.x);
		float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

		//Allow jumping on slope
		if(velocity.y > climbVelocityY){
			print("Jumping on slope");
			//ISSUE: Can't jump descending the slope <====
		}else{
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
			collisions.below = true;	
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}

	}

	//Calculate descending slope
	void DescendSlope(ref Vector3 velocity){
		float directionX = Mathf.Sign(velocity.x);
		//Set origin as the bottom corner opposite to the moviment
		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		//Raycasts down from origin to hit descending slope
		RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);	
		if(hit){
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			//If angle it's not zero and is lesser than maxDescendAngle
			if(slopeAngle != 0 && slopeAngle <= maxDescendAngle){
				//Check if we are really descending the slope (and not just falling)
				if(Mathf.Sign(hit.normal.x) == directionX){
					// Check if player is close enough to the slope for the descending effect to activate
					if(hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x)){
						float moveDistance = Mathf.Abs(velocity.x);
						float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);	
						velocity.y -= descendVelocityY;//<===
						//Update collisions
						collisions.below = true;
						collisions.descendingSlope = true;
						collisions.slopeAngle = slopeAngle;

					}
				}
			}
		}

	}






	public struct CollisionInfo{
		public bool above, below;
		public bool left, right;

		public bool climbingSlope, descendingSlope;
		public float slopeAngle, slopeAngleOld;

		public Vector3 velocityOld;

		public void Reset(){
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;
			slopeAngle = slopeAngleOld = 0;
		}
	}

}
