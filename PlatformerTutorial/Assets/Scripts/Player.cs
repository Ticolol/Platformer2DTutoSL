using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	float gravity = -20;

	public float moveSpeed = 6;

	Vector3 velocity;

	Controller2D controller;

	void Start () {
		controller = GetComponent<Controller2D>();
	}

	void Update() {
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		velocity.x = input.x * moveSpeed;
		velocity.y += gravity * Time.deltaTime;

		//vvv: Not sure if another multiplication with Time.deltaTime is needed
		controller.Move(velocity * Time.deltaTime); 
	}
	
}
