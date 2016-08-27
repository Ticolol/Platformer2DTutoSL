using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public float timeToJumpApex = .4f;
	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float moveSpeed = 6;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;

	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallJumpLeap;
	public float wallSlideMaxSpeed = 3f;
	public float wallStickTime = .25f;


	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	Vector3 velocity;	
	float velocityXSmoothing;
	float timeToWallUnstick;

	Controller2D controller;

	void Start () {
		controller = GetComponent<Controller2D>();

		//maxJumpHeight = (gravity * timeToJumpApex^2) / 2
		//
		//gravity = (2 * maxJumpHeight)/timeToJumpApex^2
		//maxJumpVelocity = gravity * timetoJumpApex
		gravity =  -(2 * maxJumpHeight)/ Mathf.Pow(timeToJumpApex,2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		//vf^2 = vo^2 + 2*a*dist
		//minJumpForce = sqrt(2 * gravity * minJumpHeight)
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
		velocity = new Vector3(0,0,0);
	}

	void Update() {
		//Get inputs for horizontal moviment
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		int wallDirX = (controller.collisions.left) ? -1 : 1;

		//Applying values to vertical and horizontal velocities
		float targetVelocityX = input.x * moveSpeed;
		//RESEARCH THIS COMMAND
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, 
			controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne);
		

		//Check for wallSlide and wallJump possibility
		bool wallSliding = false;
		if((controller.collisions.left || controller.collisions.right) && !controller.collisions.below){
			wallSliding = true;
			//max descent speed must be wallSlideMaxSpeed
			if(velocity.y < -wallSlideMaxSpeed){
				velocity.y = -wallSlideMaxSpeed;
			}
			//Stick to wall before jump
			if(timeToWallUnstick > 0){
				velocity.x = 0;
				velocityXSmoothing = 0;
				//Start to count time when player goes into wall's opposite direction
				if(input.x != wallDirX && input.x != 0){
					timeToWallUnstick -= Time.deltaTime;
				}else{
					timeToWallUnstick = wallStickTime;
				}
			//reset and unstick
			}else{
				timeToWallUnstick = wallStickTime;	
			}
		}

		//If not colliding above or below, reset vertical velocity to 0
		if(controller.collisions.above || controller.collisions.below)
			velocity.y = 0;


		//Get jump inputs and apply to velocity
		if(Input.GetKeyDown(KeyCode.Space)){
			//Treats normal jumps
			if(controller.collisions.below){
				velocity.y = maxJumpVelocity;
			}
			//Treats wall jumps
			if(wallSliding){
				//WallJumpClimb
				if(wallDirX == input.x){
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				//WallJumpOff
				}else if(wallDirX == 0){
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				//WallJumpLeap
				}else{
					velocity.x = -wallDirX * wallJumpLeap.x;
					velocity.y = wallJumpLeap.y;
				}
			}
		}
		if(Input.GetKeyUp(KeyCode.Space)){
			//Alterable Jump Height
			if(velocity.y > minJumpVelocity){
				velocity.y = minJumpVelocity;
			}
		}

		velocity.y += gravity * Time.deltaTime;//accel goes powwed (^2)

		//Apply velocity to move player
		controller.Move(velocity * Time.deltaTime, input); 
	}
	
}
