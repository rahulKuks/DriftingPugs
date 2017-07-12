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
	public Transform earth;
	public float speed = 5.0f;
	public bool test = true;
	public bool flag = false;

    private void Awake()
    {
        parentAnim = GetComponent<Animator>();
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

    private  Vector3 playerDelta = Vector3.zero;
    private float playerY = 0f;
    private Transform point;

    public void Revolving(Transform earth, Transform player, Transform point)
	{
		// move towards desired position
		isRevolving = true;
		this.earth = earth;
        playerDelta = player.position - transform.position;
        this.point = point;
        StartCoroutine("Rotate");
	}

	public float progress = 0f;
	public Vector3 start = Vector3.zero;
    private Vector3 dst = new Vector3(73.0085f, 0f, 43.78458f);
	private IEnumerator Rotate()
	{
        Debug.Log("Move towards: " + dst);
        EditorApplication.isPaused = true;
        // Move towards to position where the rotation will begin
        while (Vector3.Distance(transform.position, point.position) > 1e-6)
        {
            transform.position = Vector3.MoveTowards(transform.position, point.position, speed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        Debug.Log(transform.position - earth.position);
        EditorApplication.isPaused = true;

        Debug.Log("Do rotation");
        // Calculate speed to rotate around to match RotationDuration
        float rotationSpeed = Vector3.Distance(transform.position, earth.position) / RotationDuration;

		start = transform.position;
		while(progress <= RotationDuration)
		{
			progress += Time.deltaTime;
			transform.RotateAround(earth.position, Vector3.up, rotationSpeed * Time.fixedDeltaTime);
			yield return new WaitForFixedUpdate();
		}
        Debug.Log("Start position: " + start);
        Debug.Log("End position: " + transform.position);
	}
}
