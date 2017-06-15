using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {
	// public variables
	[Header("In Water Variables")]
	[Space(10)]

	[Tooltip("Falling without drag within this period of distance")]
	public float fallingDistance = 10.0f;

	[Tooltip("The factor to calculate the drag force when in the water. The bigger this percentage, the more the drag force will be according to the current speed, and the faster the player is going to reach a stable speed.")]
	[Range(0, 1)]
	public float dragPercentage = 0.2f;

	// private variables
	private Rigidbody rb;
	private int state;
	const int GROUNDED = 0;
	const int INWATER = 1;

	void Start () {
		state = GROUNDED;
	}
	
	void Update () {

		if (state == GROUNDED)
		{
			Debug.Log("on the ground");
			if (transform.position.y < -fallingDistance)
			{
				state = INWATER;
				rb = GetComponent<Rigidbody>();
			}
		}
	}

	void FixedUpdate() {
		if (state == INWATER)
		{
			Debug.Log("in water");
			Vector3 vel = rb.velocity;
			rb.useGravity = false;
			rb.AddForce(1f * Physics.gravity);
			rb.drag = -dragPercentage * vel.y;
			Debug.Log("velocity: " + vel.y);
		}
	}
}
