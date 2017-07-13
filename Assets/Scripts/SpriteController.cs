using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SpriteController : MonoBehaviour
{

	[Tooltip("The duration that the sprite needs to wait before it invites the user.")]
	[SerializeField] float thresholdInvitationTime;

	private Animator parentAnim;
	[SerializeField] private Animator childAnim;
	[SerializeField] private float RotationDuration = 180.0f;
	[Tooltip("The speed at which the sprite will move towards the rotation point during earth gaze.")]
	[SerializeField] private float moveSpeed = 5.0f;

	private Transform earth;
	private Transform rotationPoint;

	//Parameters to trigger sprite invitation
	private bool invitationTrigger;
	private Vector3 previousPosition;
	private float idleTime;


	// magic ratio...
	private static readonly float SPEED_DURATION_RATIO = 20/11f;

    private void Awake()
    {
        parentAnim = GetComponent<Animator>();
		idleTime = 0;
		previousPosition = transform.position;
    }

	void Update()
	{
		// If the idle animation is active, then check the time passed and trigger the invitation animation if needed.
		if(childAnim.GetBool("isIdling"))
		{
			invitationTrigger = CheckIdleTime ();

			if (invitationTrigger) 
			{
				childAnim.SetTrigger ("isInviting");
				invitationTrigger = false;
				idleTime = 0;
			}
		}

	}

	/// <summary>
	/// Keeps track of the time spent idling and not moving. 
	/// </summary>
	/// <returns><c>true</c>, if time spent idling was greater than invitation threshold time, <c>false</c> otherwise.</returns>
	bool CheckIdleTime ()
	{
		bool triggerAnimation = false;

		if (transform.position == previousPosition) 
		{
			idleTime += Time.deltaTime;
			if (idleTime >= thresholdInvitationTime) 
			{
				Debug.Log ("Triggering invitation");
				idleTime = 0;
				triggerAnimation = true;
			}
		}
		else 
		{
			idleTime = 0;
		}

		previousPosition = transform.position;
		return triggerAnimation;
	}

	public void MoveAnimation(int index)
    {
		if (parentAnim.enabled == true) 
		{
			parentAnim.SetInteger("Checkpoint", index);
			childAnim.SetBool ("isIdling", true);
		}

		if (index == 9) 
		{
			childAnim.SetBool ("inForest", false);
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
