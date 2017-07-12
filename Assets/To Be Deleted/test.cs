using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour {

	public SpriteController obj;
	public bool flag = false;

	// Use this for initialization
	void Start () {
		//Debug.Log("test", this.transform.parent.gameObject);
		Debug.Log(this.transform.parent.parent);
	}
	
	// Update is called once per frame
	void Update () {
		if (flag)
		{
			this.transform.parent.SetParent(obj.transform, true);
		}
	}
}
