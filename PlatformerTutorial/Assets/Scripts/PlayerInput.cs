using UnityEngine;
using System.Collections;

[RequireComponent(typeof (PlayerInput))]
public class PlayerInput : MonoBehaviour {

	Player player;

	void Start(){
		player = GetComponent<Player>();
	}

	void Update(){
		//Set directional input
		Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		player.SetDirectionalInput(directionalInput);

		//Get jump inputs and apply to velocity
		if(Input.GetKeyDown(KeyCode.Space)){
			player.OnJumpInputDown();
		}
		if(Input.GetKeyUp(KeyCode.Space)){
			player.OnJumpInputUp();
		}
	}

}
