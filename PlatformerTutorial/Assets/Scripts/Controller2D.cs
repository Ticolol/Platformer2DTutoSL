using UnityEngine;
using System.Collections;

public class Controller2D : RaycastController {


	public CollisionInfo collisions;

	float maxClimbAngle = 70;
	float maxDescendAngle = 70;

	[HideInInspector]
	public Vector2 playerInput;//gets the player input

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

	public void Move(Vector2 moveAmount, bool standingOnPlatform = false){
		Move(moveAmount, Vector2.zero, standingOnPlatform);
	}
	public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false){
		UpdateRaycastOrigins();
		collisions.Reset();
		collisions.moveAmountOld = moveAmount;
		playerInput = input;

		//Apply face direction correctly
		if(moveAmount.x != 0){
			collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
		}

		if(moveAmount.y < 0)
			DescendSlope(ref moveAmount);

		HorizontalCollision(ref moveAmount);

		if(moveAmount.y != 0)
			VerticalCollision(ref moveAmount);

		transform.Translate(moveAmount);

		if(standingOnPlatform){
			collisions.below = true;
		}
	}

	void HorizontalCollision(ref Vector2 moveAmount){
		float directionX = collisions.faceDir;
		float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
		//For wallSlide check, set a minimum amount of rayLength to the face direction
		if(Mathf.Abs(moveAmount.x) < skinWidth){
			rayLength = 2*skinWidth;//one to exit the collider and another to distantiate from wall
		}
		//draw vertical rays
		for(int i=0; i < horizontalRayCount; i++){
			//Bizarre yet quick n useful language
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			//another raycast style
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);			

			Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);
			
			if(hit){
				//Treats the case of a platform, when passing through the player, messes with his movement
				if(hit.distance == 0)
					continue;

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if(i==0 && slopeAngle <= maxClimbAngle){
					//Treats the case of a descending slope meeting an ascending slope
					if(collisions.descendingSlope){
						collisions.descendingSlope = false;
						moveAmount = collisions.moveAmountOld;
					}
					//Corrections with slope distance
					float distanceToSlopeStart = 0;
					if(slopeAngle != collisions.slopeAngleOld){
						distanceToSlopeStart = hit.distance - skinWidth;
						moveAmount.x -= distanceToSlopeStart * directionX;
					}
					//ClimbSlope
					ClimbSlope(ref moveAmount, slopeAngle);
					moveAmount.x += distanceToSlopeStart * directionX;
				}else{
					//Correct moveAmount and length of ray
					moveAmount.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;

					//Correction of slope and wall from side combination
					if(collisions.climbingSlope){
						moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
					}

					//Set collisions left and right properly
					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}
		}
	}

	void VerticalCollision(ref Vector2 moveAmount){
		float directionY = Mathf.Sign(moveAmount.y);
		float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;
		//draw vertical rays
		for(int i=0; i < verticalRayCount; i++){
			//Bizarre yet quick n useful language
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
			//another raycast style
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);			
			//Debug.DrawRay
			Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

			if(hit){
				//Through platforms
				if(hit.collider.tag == "Through"){
					if(directionY == 1 || hit.distance == 0)
						continue;
					if(collisions.fallingThroughPlatform){
						continue;
					}
					if(playerInput.y == -1){						
						//Set as falling through plaform
						collisions.fallingThroughPlatform = true; 
						//Prevent from being locked by moving platform
						Invoke("ResetFallingThroughPlatform", .5f);
						continue;
					}
				}

				moveAmount.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				//Correction of slope and wall from above combination
				if(collisions.climbingSlope){
					moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
				}

				//Set collisions above and below properly
				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}

		//Remove 1 frame trap when changing slopes
		if(collisions.climbingSlope){
			float directionX = Mathf.Sign(moveAmount.x);
			rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			if(hit){
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if(slopeAngle != collisions.slopeAngle){
					moveAmount.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}

		}
	}

	//Recalculate moveAmount vector to properly climb the slope
	void ClimbSlope(ref Vector2 moveAmount, float slopeAngle){		
		float moveDistance = Mathf.Abs(moveAmount.x);
		float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

		//Allow jumping on slope
		if(moveAmount.y > climbVelocityY){
			//print("Jumping on slope");
		}else{
			moveAmount.y = climbVelocityY;
			moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
			collisions.below = true;	
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}

	}

	//Calculate descending slope
	void DescendSlope(ref Vector2 moveAmount){
		float directionX = Mathf.Sign(moveAmount.x);
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
					if(hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x)){
						float moveDistance = Mathf.Abs(moveAmount.x);
						float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
						moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);	
						moveAmount.y -= descendVelocityY;//<===
						//Update collisions
						collisions.below = true;
						collisions.descendingSlope = true;
						collisions.slopeAngle = slopeAngle;

					}
				}
			}
		}

	}

	//Resets value for falling through platform
	void ResetFallingThroughPlatform(){
		collisions.fallingThroughPlatform = false;
	}



	public struct CollisionInfo{
		public bool above, below;
		public bool left, right;

		public bool climbingSlope, descendingSlope;
		public float slopeAngle, slopeAngleOld;

		public Vector2 moveAmountOld;
		public int faceDir;//1 - right; -1 - left
		public bool fallingThroughPlatform;

		public void Reset(){
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;
			slopeAngle = slopeAngleOld = 0;
		}
	}

}
