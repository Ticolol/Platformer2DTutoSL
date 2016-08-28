using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

	public Controller2D target;
	public Vector2 focusAreaSize;

	public float verticalOffset;

	//For the look ahead movement
	public float lookAheadDistX;
	public float lookSmoothTimeX;
	public float verticalSmoothTime;

	FocusArea focusArea;

	//For the look ahead movement
	float currentLookAheadX;
	float targetLookAheadX;
	float lookAheadDirX;
	float smoothLookVelocityX;
	float smoothVelocityY;

	bool lookAheadStopped;

	void Start(){
		focusArea = new FocusArea(target.collider.bounds, focusAreaSize);
	}

	//Runs after all Update functions. Cameras must move after everyone else (or, specifically, their targets)
	void LateUpdate(){
		focusArea.Update(target.collider.bounds);

		//Make camera move and follow player
		Vector3 focusPosition = focusArea.centre + Vector2.up * verticalOffset;

		//Calculate look ahead position
		if(focusArea.velocity.x != 0){
			lookAheadDirX = Mathf.Sign(focusArea.velocity.x);
			if(target.playerInput.x != 0 && Mathf.Sign(target.playerInput.x) == Mathf.Sign(focusArea.velocity.x)){
				lookAheadStopped = false;
				targetLookAheadX = lookAheadDirX * lookAheadDistX;
			}else{
				if(!lookAheadStopped){
					lookAheadStopped = true;
					targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDistX - currentLookAheadX)/4f;
				}
			}
		}
		currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);

		//Move camera
		focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, verticalSmoothTime);
		focusPosition += Vector3.right * currentLookAheadX;
		transform.position = focusPosition + Vector3.forward * -10;
	}

	void OnDrawGizmos(){
		Gizmos.color = new Color(1,0,0,0.5f);
		Gizmos.DrawCube(focusArea.centre, focusAreaSize);
	}

	struct FocusArea{
		public Vector2 centre;
		float left, right;
		float top, bottom;
		public Vector2 velocity;

		public FocusArea(Bounds targetBounds, Vector2 size){
			//Calculate bounds of focus area as horizontally on the middle and vertically on the bottom of focus area
			left = targetBounds.center.x - size.x/2;
			right = targetBounds.center.x + size.x/2;
			bottom = targetBounds.min.y;
			top = targetBounds.min.y + size.y;
			centre = new Vector2((left+right)/2, (bottom + top)/2);
			velocity = Vector2.zero;
		}

		public void Update(Bounds targetBounds){
			//Apply movement on X axis
			float shiftX = 0;
			if(targetBounds.min.x < left){
				//Move focus area to left
				shiftX = targetBounds.min.x - left;
			}else if(targetBounds.max.x > right){
				//Move focus area to right
				shiftX = targetBounds.max.x - right;
			}
			left+= shiftX;
			right+= shiftX;

			//Apply movement on Y axis
			float shiftY = 0;
			if(targetBounds.min.y < bottom){
				//Move focus area to bottom
				shiftY = targetBounds.min.y - bottom;
			}else if(targetBounds.max.y > top){
				//Move focus area to top
				shiftY = targetBounds.max.y - top;
			}
			bottom+= shiftY;
			top+= shiftY;

			//Recalculate centre
			centre = new Vector2((left+right)/2, (bottom + top)/2);
			//Set velocity
			velocity = new Vector2(shiftX, shiftY);

		}
	}


}

