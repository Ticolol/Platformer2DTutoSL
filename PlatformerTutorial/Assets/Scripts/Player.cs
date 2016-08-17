using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public float timeToJumpApex = .4f;
	public float jumpHeight = 4;
	public float moveSpeed = 6;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;

	float gravity;
	float jumpVelocity;
	Vector3 velocity;	
	float velocityXSmoothing;

	Controller2D controller;

	void Start () {
		controller = GetComponent<Controller2D>();

		//jumpHeight = (gravity * timeToJumpApex^2) / 2

		//gravity = (2 * jumpHeight)/timeToJumpApex^2
		//jumpVelocity = gravity * timetoJumpApex
		gravity =  -(2 * jumpHeight)/ Mathf.Pow(timeToJumpApex,2);
		jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;

		velocity = new Vector3(0,0,0);
	}

	void Update() {
		//If not colliding above or below, reset vertical velocity to 0
		if(controller.collisions.above || controller.collisions.below)
			velocity.y = 0;

		//Get inputs for horizontal moviment
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		//Get jump inputs and apply to velocity
		if(Input.GetKeyDown(KeyCode.Space) && controller.collisions.below){
			velocity.y = jumpVelocity;
		}

		//Applying values to vertical and horizontal velocities
		float targetVelocityX = input.x * moveSpeed;
		//RESEARCH THIS COMMAND
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, 
			controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;//accel goes powwed (^2)

		//Apply velocity to move player
		controller.Move(velocity * Time.deltaTime); 
	}
	
}
