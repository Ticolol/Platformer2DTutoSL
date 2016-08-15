using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	public LayerMask collisionMask;

	const float skinWidth = .015f;
	public int horizontalRayCount = 4; //Number of horizontal raycasts
	public int verticalRayCount = 4; //Number of vertical raycasts

	float horizontalRaySpacing; //horizontal space between raycasts
	float verticalRaySpacing; //vertical space between raycasts

	BoxCollider2D collider;
	RaycastOrigins raycastOrigins;	
	public CollisionInfo collisions;


	void Start(){
		collider = GetComponent<BoxCollider2D>();

		CalculateRaySpacing();		
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


	public void Move(Vector3 velocity){
		UpdateRaycastOrigins();
		collisions.Reset();

		if(velocity.x != 0)
			HorizontalCollision(ref velocity);
		if(velocity.y != 0)
			VerticalCollision(ref velocity);

		transform.Translate(velocity);
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
				velocity.x = (hit.distance - skinWidth) * directionX;
				rayLength = hit.distance;

				//Set collisions left and right properly
				collisions.left = directionX == -1;
				collisions.right = directionX == 1;
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

				//Set collisions above and below properly
				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}
	}

	void UpdateRaycastOrigins(){
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);

		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
	}

	void CalculateRaySpacing(){
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);		

		horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

	public struct CollisionInfo{
		public bool above, below;
		public bool left, right;

		public void Reset(){
			above = below = false;
			left = right = false;
		}
	}

}
