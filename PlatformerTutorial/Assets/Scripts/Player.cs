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

	Vector2 directionalInput;
	bool wallSliding;
	int wallDirX;

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
		wallDirX = 0;		
	}

	void Update() {				
		print("Start Update =======<><><><><>");
		CalculateVelocity();
		HandleWallSliding();

		//Apply velocity to move player
		print(velocity);
		controller.Move(velocity * Time.deltaTime, directionalInput); 

		//If not colliding above or below, reset vertical velocity to 0
		if(controller.collisions.above || controller.collisions.below)
			velocity.y = 0;			
	}

	//Separate the input in a new class
	public void SetDirectionalInput(Vector2 input){
		directionalInput = input;
	}

	//Implements jump down
	public void OnJumpInputDown(){
		//Treats normal jumps
		if(controller.collisions.below){
			velocity.y = maxJumpVelocity;
		}
		//Treats wall jumps
		if(wallSliding){
			//WallJumpClimb
			if(wallDirX == directionalInput.x){
				print("WallJumpClimb");
				velocity.x = -wallDirX * wallJumpClimb.x;
				velocity.y = wallJumpClimb.y;
			//WallJumpOff
			}else if(directionalInput.x == 0){
				print("WallJumpOff");
				velocity.x = -wallDirX * wallJumpOff.x;
				velocity.y = wallJumpOff.y;
			//WallJumpLeap
			}else{
				print("WallJumpLeap");
				velocity.x = -wallDirX * wallJumpLeap.x;
				velocity.y = wallJumpLeap.y;
			}
		}
	}

	//Implements jump up
	public void OnJumpInputUp(){
		//Alterable Jump Height
		if(velocity.y > minJumpVelocity){
			velocity.y = minJumpVelocity;
		}
	}

	void CalculateVelocity(){		
		//Applying values to vertical and horizontal velocities
		float targetVelocityX = directionalInput.x * moveSpeed;
		//RESEARCH THIS COMMAND
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, 
			controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;//accel goes powwed (^2)
	}

	void HandleWallSliding(){		
		wallDirX = (controller.collisions.left) ? -1 : 1;
		//Check for wallSlide and wallJump possibility
		wallSliding = false;
		if((controller.collisions.left || controller.collisions.right) && !controller.collisions.below){
			wallSliding = true;
			//max descent speed must be wallSlideMaxSpeed
			if(velocity.y < -wallSlideMaxSpeed){
				velocity.y = -wallSlideMaxSpeed;
				print("wallDown");
			}
			//Stick to wall before jump
			if(timeToWallUnstick > 0){
				velocity.x = 0;
				velocityXSmoothing = 0;
				//Start to count time when player goes into wall's opposite direction
				if(directionalInput.x != wallDirX && directionalInput.x != 0){
					timeToWallUnstick -= Time.deltaTime;
				}else{
					timeToWallUnstick = wallStickTime;
				}
			//reset and unstick
			}else{
				timeToWallUnstick = wallStickTime;	
			}
		}
	}
	
}
