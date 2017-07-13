using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SpriteController : MonoBehaviour
{
	private Animator parentAnim;
	[SerializeField] private Animator childAnim;
	[SerializeField] private float RotationDuration = 180.0f;
	[Tooltip("The speed at which the sprite will move towards the rotation point during earth gaze.")]
	[SerializeField] private float moveSpeed = 5.0f;

	private Transform earth;
	private Transform rotationPoint;

	// magic ratio...
	private static readonly float SPEED_DURATION_RATIO = 20/11f;

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

    public void TriggerEarthGaze(Transform earth, Transform rotationPoint)
	{
		// Set variables
		this.earth = earth;
		this.rotationPoint = rotationPoint;
        StartCoroutine("Rotate");
	}

	private IEnumerator Rotate()
	{
		Debug.Log("Moving towards rotation point.");
        // Move towards to position where the rotation will begin
		while (Vector3.Distance(transform.position, rotationPoint.position) > 1e-6)
        {
			transform.position = Vector3.MoveTowards(transform.position, rotationPoint.position, moveSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("Do rotation");
        /* Calculate speed using rotation duration
         * Distant travelled is the circumference thus 2*pi*r
         * This doesn't calculate exactly so doing workaround */
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
