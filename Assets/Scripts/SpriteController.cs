﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
	[Tooltip("The duration that the sprite needs to wait before it invites the user by using the invitation animation clip.")]
	[SerializeField] float thresholdInvitationTime;
	[Tooltip("The Animator  of the inner sprite. This is responsible for playing all sprite animations in local coordinates.")]
	[SerializeField] private Animator childAnim;
    [Tooltip("The location of the sprite relative to the player in the lake.")]
	[SerializeField] private Vector3 spriteLakeLocation = new Vector3(-3.6f, 1.2f, -5.5f);
    [SerializeField] private PlayerControl playerControl;
	[SerializeField] private CheckpointController checkpointController;
	[Tooltip("The checkpoint the user hits before entering the lake. This is used to trigger the sprite's lake behaviour.")]
	[SerializeField] private Checkpoint beforeLakeCheckpoint;
	[Tooltip("The sprite chime volume in Forest and Space.")]
	[SerializeField] private float chimesForestSpaceVolume;
	[Tooltip("The sprite chime volume in the Lake.")]
	[SerializeField] private float chimesLakeVolume;

    #region Private Variables
    //Parameters to trigger sprite invitation
    private Animator parentAnim;
    private bool invitationTrigger;
	private Vector3 previousPosition;
	private float idleTime;
	private int beforeLakeCheckpointIndex;

	private AudioSource spriteAudioSource;
	private AudioSource chimesAudio;
    #endregion

    private void Awake()
    {
        parentAnim = GetComponent<Animator>();
		idleTime = 0;
		previousPosition = transform.position;

		spriteAudioSource = this.transform.Find("Sprite").GetComponent<AudioSource>();

		beforeLakeCheckpointIndex = checkpointController.IndexOfCheckpoint(beforeLakeCheckpoint);
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

		if (index == beforeLakeCheckpointIndex) 
		{
			EnableSpriteLakeBehaviour ();
		}
    }

	/// <summary>
	/// Disables the parent animator.
	/// </summary>
	public void DisableParentAnimator()
	{
		parentAnim.enabled = false;
	}

	/// <summary>
	/// Enables the parent animator.
	/// </summary>
	public void EnableParentAnimator()
	{
		parentAnim.enabled = true;
	}

    /// <summary>
    /// Updates the sprites behaviour when the player's state updates.
    /// </summary>
    public void OnPlayerStateChange()
    {
        switch (playerControl.CurrentState)
        {
            case PlayerControl.PlayerState.Grounded:
                break;
            case PlayerControl.PlayerState.InWater_Falling:
                StartCoroutine(MoveToLakePosition());
                break;
            case PlayerControl.PlayerState.InWater_Float:
                break;
            case PlayerControl.PlayerState.Space:
                EnableSpriteSpaceBehaviour();
                break;
        }
    }

    private void EnableSpriteLakeBehaviour()
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

    /// <summary>
    /// Move the sprite to where it should be relative to the player while in the lake.
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveToLakePosition()
    {
        Debug.Log("Sprite moving towards player");
        while (Vector3.Distance(this.transform.localPosition, spriteLakeLocation) > 1e-6)
        {
            this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition,
                spriteLakeLocation, 10 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }
}
