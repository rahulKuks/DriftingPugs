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
    [Tooltip("The speed the sprite will begin moving at in space until the speed up checkpoint.")]
    [SerializeField] private float initialMoveSpeed = 2.5f;
    [Tooltip("The speed at which the sprite will move towards from the speed up checkpoint to the rotation point during space.")]
	[SerializeField] private float moveSpeed = 5.0f;
	[SerializeField] AudioClip spriteSeaSound;
    [Tooltip("Objects in space that neds to placed relative to where the player will be in space.")]
    [SerializeField] private Transform spacePivot;
    [SerializeField] private Transform landingPoint;
    [SerializeField] private Transform speedUpCheckpoint;
    [SerializeField] private Transform space;
    [Tooltip("Where the sprite will be when rotating around earth.")]
    [SerializeField]
    private Transform spriteRotationPoint;
    [Tooltip("The speed of the sprite when it move to the sprite rotation point at the beginning of earth gaze.")]
    [SerializeField] private float spriteRotationSetupSpeed = 5.0f;
    [SerializeField] private Transform player;

    private Transform earth;
	private Transform rotationPoint;
    private float speed;
    private bool isSpeedUp = false;

    //Parameters to trigger sprite invitation
    private bool invitationTrigger;
	private Vector3 previousPosition;
	private float idleTime;
	private AudioSource spriteAudioSource;

	// magic ratio...
	private static readonly float SPEED_DURATION_RATIO = 20/11f;

    private void Awake()
    {
        parentAnim = GetComponent<Animator>();
		idleTime = 0;
		previousPosition = transform.position;

		spriteAudioSource = this.transform.Find ("Sprite").GetComponent<AudioSource> ();
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

	void EnableSpriteSeaBehaviour ()
	{
		childAnim.SetBool ("inForest", false);
		//spriteAudioSource.clip = spriteSeaSound;
		//spriteAudioSource.loop = true;
		//spriteAudioSource.Play ();
	}

    public IEnumerator Explore()
    {
        // Make objects in space pivot relative to this and set them up
        spacePivot.position = this.transform.position;
        GameObject go;
        for (int i = spacePivot.childCount-1; i >= 0; i--)
        {
            go = spacePivot.GetChild(i).gameObject;
            go.SetActive(true);
            go.transform.SetParent(space, true);
        }
        spacePivot.gameObject.SetActive(false);

        speed = initialMoveSpeed;
        Vector3 dest = Vector3.zero;

        // Move through space checkpoints
        for (int i=0; i<landingPoint.childCount; i++)
        {
            dest = landingPoint.GetChild(i).transform.position;
            while (Vector3.Distance(transform.position, dest) > 1e-6)
            {
                // Gradually speed up at speed up checkpoint
                if (isSpeedUp)
                {
                    speed = Mathf.Lerp(speed, moveSpeed, Time.fixedDeltaTime);
                }

                transform.position = Vector3.MoveTowards(transform.position, dest, speed * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            if (!isSpeedUp)
                isSpeedUp = landingPoint.GetChild(i).GetInstanceID() == speedUpCheckpoint.GetInstanceID();
        }
    }

    public void TriggerEarthGaze(Transform earth, Transform rotationPoint)
	{
        // Set variables
		this.earth = earth;
		this.rotationPoint = rotationPoint;
		spriteAudioSource.volume = Mathf.Lerp (spriteAudioSource.volume, 0, 1.5f);
        StartCoroutine("Rotate");
	}

	private IEnumerator Rotate()
	{
		Debug.Log("Moving towards rotation point.");
        // Move towards to position where the rotation will begin
		while (Vector3.Distance(transform.position, rotationPoint.position) > 1e-6)
        {
			transform.position = Vector3.MoveTowards(transform.position, rotationPoint.position, speed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        /*  Move the sprite to where it should be position during rotation  */
        // Un-parent from sprite for now
        spriteRotationPoint.SetParent(null, true);
        player.parent.SetParent(null, true);
        // Move sprite
        while (Vector3.Distance(transform.position, spriteRotationPoint.position) > 1e-6)
        {
            transform.position = Vector3.MoveTowards(transform.position, spriteRotationPoint.position, spriteRotationSetupSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        // Re-add player as a child
        player.parent.SetParent(this.transform, true);

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
