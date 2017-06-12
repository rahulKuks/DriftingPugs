using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour {

	// GO speed
	[SerializeField] private float speed = 5.0f;

	// mouse speed
	[SerializeField] private float speedH = 2.0f;
	[SerializeField] private float speedV = 2.0f;

	private float yaw = 0.0f;
	private float pitch = 0.0f;

	void Update()
	{
		if (Input.GetKey(KeyCode.W))
		{
			transform.position += transform.forward * Time.deltaTime * speed;
		}

		if (Input.GetKey(KeyCode.S))
		{
			transform.position -= transform.forward * Time.deltaTime * speed;
		}

		if (Input.GetKey(KeyCode.A))
		{
			transform.position -= transform.right * Time.deltaTime * speed;
		}

		if (Input.GetKey(KeyCode.D))
		{
			transform.position += transform.right * Time.deltaTime * speed;
		}

		yaw += speedH * Input.GetAxis("Mouse X");
		pitch -= speedV * Input.GetAxis("Mouse Y");

		transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
	}
}
