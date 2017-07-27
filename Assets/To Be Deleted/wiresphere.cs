using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wiresphere : MonoBehaviour {

	private float explosionRadius = 4.0f;
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, explosionRadius);
	}
}
