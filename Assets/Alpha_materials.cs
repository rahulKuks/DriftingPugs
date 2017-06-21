using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alpha_materials : MonoBehaviour {

	Color colorStart;
	Color colorEnd;
	float duration = 1.0;

	void Start () {
		colorStart = renderer.material.color;
		colorEnd = Color(colorStart.r, colorStart.g, colorStart.b, 0.0);
	}

	void OnMouseDown () {
		FadeOut();
	}

	void FadeOut ()
	{
		for (t = 0.0; t < duration; t += Time.deltaTime) {
			renderer.material.color = Color.Lerp (colorStart, colorEnd, t/duration);
			yield;
		}
	}
}
