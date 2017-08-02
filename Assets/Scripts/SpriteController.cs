using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{

	[Tooltip("The duration that the sprite needs to wait before it invites the user.")]
	[SerializeField] float thresholdInvitationTime;

	private Animator parentAnim;
	[SerializeField] private Animator childAnim;
	[SerializeField] private float chimesForestSpaceVolume;
	[SerializeField] private float chimesLakeVolume;
	[SerializeField] private float rotationDuration = 180.0f;
    [Tooltip("The speed the sprite will begin moving at in space until the speed up checkpoint.")]
    [SerializeField] private float initialMoveSpeed = 2.5f;
    [Tooltip("The speed at which the sprite will move towards from the speed up checkpoint to the rotation point during space.")]
	[SerializeField] private float moveSpeed = 5.0f;
	[SerializeField] AudioClip spriteSeaSound;
    [Tooltip("Objects in space that needs to placed relative to where the player will be in space.")]
    [SerializeField] private Transform spacePivot;
    [SerializeField] private Transform landingPoint;
    [SerializeField] private Transform speedUpCheckpoint;
    [SerializeField] private Transform space;
    [Tooltip("Where the sprite will be when rotating around earth.")]
    [SerializeField]
    private Transform spriteRotationPoint;
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
	private GameObject dummyParent;
	private PlayerControl playerControl;
	private AudioSource chimesAudio;

	// magic ratio...
	private static readonly float SPEED_DURATION_RATIO = 20/11f;

    private void Awake()
    {
        parentAnim = GetComponent<Animator>();
		idleTime = 0;
		previousPosition = transform.position;

		spriteAudioSource = this.transform.Find("Sprite").GetComponent<AudioSource>();

		dummyParent = new GameObject();
		dummyParent.name = "dummyParent";
		dummyParent.SetActive(false);
		dummyParent.transform.SetParent(this.transform);

		playerControl = player.gameObject.GetComponent<PlayerControl>();
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

	private void EnableSpriteSeaBehaviour()
	{
		childAnim.SetBool ("inForest", false);
		childAnim.SetBool ("inLake", true);
		chimesAudio.volume = chimesLakeVolume;
		//spriteAudioSource.clip = spriteSeaSound;
		//spriteAudioSource.loop = true;
		//spriteAudioSource.Play ();
	}

	public IEnumerator Explore(SwivelLocomotion swivel)
    {
		Debug.Log("Starting coroutine Explore");
		childAnim.SetBool("inSpace", true);
		chimesAudio.volume = chimesForestSpaceVolume;
        // Make objects in space pivot relative to this and set them up
		spacePivot.position = player.position;
        GameObject go;
        for (int i = spacePivot.childCount-1; i >= 0; i--)
        {
            go = spacePivot.GetChild(i).gameObject;
            go.SetActive(true);
            go.transform.SetParent(space, true);
        }
        spacePivot.gameObject.SetActive(false);

		// Setup dummy parent that will move the sprite and player
		dummyParent.transform.position = player.position;
		dummyParent.transform.SetParent(null);
		dummyParent.SetActive(true);
		this.transform.SetParent(dummyParent.transform);
		player.SetParent(dummyParent.transform);

		if (swivel != null) 
		{
            //Enable space constraints and parameters
            swivel.SetSwivelState(SwivelLocomotion.SwivelState.inSpace);
		}

        speed = initialMoveSpeed;
        Vector3 dest = Vector3.zero;

        // Move through space checkpoints
        for (int i=0; i<landingPoint.childCount; i++)
        {
            dest = landingPoint.GetChild(i).transform.position;
            while (Vector3.Distance(dummyParent.transform.position, dest) > 1e-6)
            {
                // Gradually speed up at speed up checkpoint
                if (isSpeedUp)
                {
                    speed = Mathf.Lerp(speed, moveSpeed, Time.fixedDeltaTime);
                }

				dummyParent.transform.position = Vector3.MoveTowards(dummyParent.transform.position,
					dest, speed * Time.fixedDeltaTime);
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
		while (Vector3.Distance(dummyParent.transform.position, rotationPoint.position) > 1e-6)
        {
			dummyParent.transform.position = Vector3.MoveTowards(dummyParent.transform.position, rotationPoint.position, speed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("Do rotation");
        /* Calculate speed using rotation duration
         * Distant travelled is the circumference thus 2*pi*r */
		float radius = Vector3.Distance(dummyParent.transform.position, earth.position);
		float rotationSpeed = 2 * Mathf.PI * radius / (rotationDuration * SPEED_DURATION_RATIO);

		// Rotate around earth for the rotation duration
		float progress = 0f;
		bool fadeTriggered = false;
		while(progress <= (rotationDuration + playerControl.FadeDuration))
		{
			progress += Time.fixedDeltaTime;
			dummyParent.transform.RotateAround(earth.position, Vector3.up, rotationSpeed * Time.fixedDeltaTime);
			if (progress >= rotationDuration && !fadeTriggered)
			{
				fadeTriggered = true;
				StartCoroutine(playerControl.Resolution());
			}
			yield return new WaitForFixedUpdate();
		}
	}
}
