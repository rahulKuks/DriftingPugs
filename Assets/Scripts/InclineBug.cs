using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InclineBug : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter(Collision coll)
	{
		Debug.Log("Collision with: " + coll.gameObject.name);
	}

	void OnTriggerStay(Collider coll)
	{
		Debug.Log("Trigger stay: " + coll.gameObject.name);
	}
}
