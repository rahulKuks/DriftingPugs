using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SpriteController : MonoBehaviour
{
	private Animator parentAnim;
	[SerializeField] Animator childAnim;
	[SerializeField] float RotationDuration = 180.0f;

	public bool isRevolving = false;
	public Transform obj;
	public float speed = 5.0f;
	public bool test = true;
	public bool flag = false;

    private void Awake()
    {
        parentAnim = GetComponent<Animator>();
    }

	void Update()
	{
		if (isRevolving)
		{
			/*if (start == Vector3.zero)
				start = transform.position;
			int sign = (test) ? 1 : -1;
			transform.RotateAround(obj.position, sign * Vector3.up, speed * Time.deltaTime);*/
			Debug.DrawRay(transform.position, transform.forward, Color.green);
		}
	}

	public void MoveAnimation(int index)
    {
		if (parentAnim.enabled == true) 
		{
			parentAnim.SetInteger("Checkpoint", index);
			childAnim.SetBool ("isIdling", true);
		}
    }

	public void DisableParentAnimator()
	{
		parentAnim.enabled = false;
	}

	public void EnableParentAnimator()
	{
		parentAnim.enabled = true;
	}

	public void Revolving(Transform earth, Transform player)
	{
		// move towards desired position
		isRevolving = true;
		obj = earth;
		StartCoroutine("Rotate");
	}

	public float progress = 0f;
	public Vector3 start = Vector3.zero;
	private IEnumerator Rotate()
	{
		EditorApplication.isPaused = true;
		yield return new WaitForSeconds(2.0f);

		start = transform.position;
		transform.RotateAround(obj.position, Vector3.up, speed * Time.deltaTime);
		while(progress <= RotationDuration)
		{
			progress += Time.deltaTime;
			transform.RotateAround(obj.position, Vector3.up, speed * Time.deltaTime);
			if (Vector3.Distance(transform.position, start) < 1e-6)
			{
				Debug.Log("progress " + progress);
				Debug.Log("full rotation");
			}
			yield return new WaitForEndOfFrame();
		}
	}
}
