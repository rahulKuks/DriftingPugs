﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
	[Tooltip("The duration that the sprite needs to wait before it invites the user.")]
	[SerializeField] float thresholdInvitationTime;
	[SerializeField] private Animator childAnim;
	[SerializeField] private float chimesForestSpaceVolume;
	[SerializeField] private float chimesLakeVolume;
    [Tooltip("The location of the sprite relative to the player in the sea.")]
    [SerializeField] private Vector3 spriteSeaLocation = new Vector3(-3.6f, 1.2f, -5.5f);
    [SerializeField] private PlayerControl playerControl;

    #region Private Variables
    //Parameters to trigger sprite invitation
    private Animator parentAnim;
    private bool invitationTrigger;
	private Vector3 previousPosition;
	private float idleTime;
    
	private AudioSource spriteAudioSource;
	private AudioSource chimesAudio;
    #endregion

    private void Awake()
    {
        parentAnim = GetComponent<Animator>();
		idleTime = 0;
		previousPosition = transform.position;

		spriteAudioSource = this.transform.Find("Sprite").GetComponent<AudioSource>();

		chimesAudio = this.GetComponent<AudioSource>();
		chimesAudio.volume = chimesForestSpaceVolume;
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
	private bool CheckIdleTime ()
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
			EnableSpriteSeaBehaviour ();
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

    public void OnPlayerStateChange()
    {
        switch (playerControl.CurrentState)
        {
            case PlayerControl.PlayerState.Grounded:
                break;
            case PlayerControl.PlayerState.InWater_Falling:
                StartCoroutine(MoveToSeaPosition());
                break;
            case PlayerControl.PlayerState.InWater_Float:
                break;
            case PlayerControl.PlayerState.Space:
                EnableSpriteSpaceBehaviour();
                break;
        }
    }

    private void EnableSpriteSeaBehaviour()
	{
		childAnim.SetBool ("inForest", false);
		childAnim.SetBool ("inLake", true);
        chimesAudio.volume = chimesLakeVolume;
	}

    private void EnableSpriteSpaceBehaviour()
    {
        childAnim.SetBool("inSpace", true);
        chimesAudio.volume = chimesForestSpaceVolume;
    }

    private IEnumerator MoveToSeaPosition()
    {
        Debug.Log("Sprite moving towards player");
        while (Vector3.Distance(this.transform.localPosition, spriteSeaLocation) > 1e-6)
        {
            this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition,
                spriteSeaLocation, 10 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }
}
