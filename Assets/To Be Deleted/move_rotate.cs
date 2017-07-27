using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move_rotate : MonoBehaviour {

	public bool trigger = false;
	public Transform rotationPoint;
	public float moveSpeed = 5.0f;
	public Transform earth;
	[SerializeField] private float RotationDuration = 180.0f;
	private static readonly float SPEED_DURATION_RATIO = 20/11f;

	private Transform child;

	void Start() {
		child = transform.GetChild(0);
	}

	void Update () {
		if (trigger)
		{
			trigger = !trigger;
			StartCoroutine(MoveAndRotate());
			child.SetParent(null);
		}
	}

	public IEnumerator MoveAndRotate()
	{
		while(Vector3.Distance(transform.position,rotationPoint.position) > 1e-6)
		{
			transform.position = Vector3.MoveTowards(transform.position, rotationPoint.position, moveSpeed * Time.fixedDeltaTime);
			yield return new WaitForEndOfFrame();
		}

		float radius = Vector3.Distance(transform.position, earth.position);
		float rotationSpeed = 2 * Mathf.PI * radius / (RotationDuration * SPEED_DURATION_RATIO);

		// Rotate around earth for the rotation duration
		float progress = 0f;
		while(progress <= RotationDuration)
		{
			progress += Time.fixedDeltaTime;
			transform.RotateAround(earth.position, Vector3.up, rotationSpeed * Time.fixedDeltaTime);
			yield return new WaitForFixedUpdate();
		}
	}
}
