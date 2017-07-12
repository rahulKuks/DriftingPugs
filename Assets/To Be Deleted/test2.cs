using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test2 : MonoBehaviour {

	public Transform obj;
	public float speed = 5.0f;

	void Start () {
		transform.RotateAround(obj.position, Vector3.up, 63f);
	}
}
