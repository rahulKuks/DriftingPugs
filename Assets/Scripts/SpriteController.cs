using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
	[Tooltip("The duration that the sprite needs to wait before it invites the user.")]
	[SerializeField] float thresholdInvitationTime;

	[Tooltip("The animator controller of the child sprite object.")]
	[SerializeField] Animator childAnim;
	private Animator parentAnim;

	private int baseLayerIndex;
	private bool invitationTrigger;
	private Vector3 previousPosition;
	private float idleTime;


    private void Awake()
    {
		idleTime = 0;
		previousPosition = transform.position;
        parentAnim = GetComponent<Animator>();
		baseLayerIndex = parentAnim.GetLayerIndex ("BaseLayer");
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

	/// <summary>
	/// Trigger the next animation step to move the sprite onto the next checkpoint.
	/// </summary>
	/// <param name="index">Index.</param>
	public void MoveAnimation(int index)
	{
		if (parentAnim.enabled == true) 
		{
			parentAnim.SetInteger("Checkpoint", index);
			childAnim.SetBool ("isIdling", true);

			if (index == 9) 
			{
				childAnim.SetBool ("inForest", false);
			}
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

}
