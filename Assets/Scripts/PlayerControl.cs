using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using AC.TimeOfDaySystemFree;

public class PlayerControl : MonoBehaviour
{
    #region GameObject Variables
    [Header("GameObject Variables")]
    [Space(5)]

    [SerializeField]
    private GameObject sprite;
    [SerializeField]
    private TimeOfDayManager timeManager;

    // For forest world
    [SerializeField]
    private GameObject forestWorld;

    // For sea world
    [SerializeField]
    private GameObject sea;
    [SerializeField]
    private GameObject jellyfishes;
    [SerializeField]
    private GameObject fishes;

    // For space world
    [SerializeField]
    private GameObject starCluster;
    [SerializeField]
    private GameObject earth;
    [SerializeField]
    private GameObject sun;
    [SerializeField]
    private GameObject space;
    [Tooltip("The position of the sprite relative to the user during the space exploration phase.")]
    [SerializeField]
    private Transform spriteExplorationPoint;
    [Tooltip("The point where the rotation around earth will begin.")]
    [SerializeField]
    private Transform spriteRotationPoint;

    // For transitions
    [SerializeField]
    private GameObject fadeTrigger;
    [Tooltip("The list of GameObjects to collide with to transition into the next state." +
        " Order of the GameObjects are important.")]
    [SerializeField]
    private List<GameObject> transitionColliders;
    #endregion

    #region Falling in Water Parameters
    [Header("Falling in Water Parameters")]
    [Space(5)]
    [Tooltip("The factor to calculate the drag force when in the water. The bigger this percentage," +
        " the more the drag force will be according to the current speed, and the" +
        " faster the player is going to reach a stable speed.")]
    [Range(0, 1)]
    [SerializeField]
    private float dragPercentageWater = 0.2f;
    [SerializeField]
    private GameObject bubbles;
    [Tooltip("How far down (y-axis) the bubbles will be offset when entering the lake.")]
    [SerializeField]
    private float bubblesOffset = 5.0f;
    [SerializeField]
    private Material seaFadeMaterial;
    #endregion

    #region In Sea Parameters
	[SerializeField] private GameObject upperLakesurface;

    // Parameters to rotate the player while they are floating down in the sea
    [Tooltip("A flag to determine if the player is rotation while floating in the sea.")]
    [SerializeField]
    private bool doTwist = false;
    [Tooltip("The amount the player will rotaion while floating in the sea.")]
    [Range(0, 360)]
    [SerializeField]
    private int twistAngle = 90;
    [Tooltip("The time it takes for the player to rotate the Twist Angle while floating in the sea.")]
    [SerializeField]
    private float twistDuration = 20.0f;

    // TODO: probably move this somewhere else
    // Fade out parameters
    [Tooltip("A flag to determine if the sea will fade out while the player is floating.")]
    [SerializeField]
    private bool doFade = false;
    [Tooltip("The time it takes for the sea to fade completely away.")]
    [SerializeField]
    private float seaFadeDuration = 20.0f;
    [Tooltip("The time it takes for the sea creatures to fade completely away.")]
    [SerializeField]
    private float seaCreaturesFadeDuration = 5.0f;

    [Tooltip("The location of the sprite relative to the player in the sea.")]
    [SerializeField]
    private Vector3 spriteSeaLocation = new Vector3(-3.6f, 1.2f, -5.5f);
    #endregion

    #region Space Parameters
    [Header("Space Parameters")]
    [Space(5)]

    [SerializeField]
    private Material spaceSkybox;
    [Tooltip("The speed of the sprite moving towards the sprite exploration point.")]
    [SerializeField]
    private float spriteSetupSpeed = 10.0f;
    #endregion

    #region Resolution Parameters
    [Header("Resolution Parameters")]
    [Space(5)]

    [Tooltip("The duration it takes for the camera to fade out to black or in from black.")]
    [SerializeField]
    private float fadeDuration = 1.0f;
    [Tooltip("The duration in between the fade out and fade in.")]
    [SerializeField]
    private float fadePauseDuration = 1.0f;
    [SerializeField]
    private Material morningSkybox;
    [Tooltip("The time before completely fading out and ending the experience.")]
    [SerializeField]
    private float finalFadeDuration = 5.0f;
    [SerializeField]
    private float morningTime;
    #endregion

	#region Debug Parameters
	[Header("Debug Parameters")]
	[Space(5)]
	[SerializeField]
	private float debug_SinkSpeed = 10;
	#endregion

    #region Public Variables
    public enum PlayerState { Grounded, InWater_Falling, InWater_Float, Space };
    public PlayerState CurrentState { get { return currentState; } }
    public UnityEvent PlayerUpdateStateEvent;
    public float FadeDuration { get { return fadeDuration; } }
    #endregion

    #region Private Variables
    private float sunEarthDeltaY;       // Keep difference for when they are re-positioned
    private Vector3 startPos = Vector3.zero;    // The position to return to after space
    private Rigidbody rb;
    private PlayerState currentState = PlayerState.Grounded;
    private Transform spriteParent;     // Original parent of sprite
    private Renderer seaRenderer;       // Used to get the material to change the opacity
    private Color seaColor;             // Used to preserve the color while changing the alpha level
    private SwivelLocomotion swivel;
    private SpriteController spriteController;
    #endregion

	void Awake()
	{
		// Compute here else the delta is different cuz the objects are moving
		//sunEarthDeltaY = sun.transform.position.y - earth.transform.position.y;
	}

    void Start()
    {
		//startPos = transform.position;
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Unable to get rigidbody.", this.gameObject);
        }
        spriteParent = sprite.transform.parent;
		spriteController = sprite.GetComponent<SpriteController> ();
        seaRenderer = sea.GetComponent<Renderer>();
        seaColor = seaRenderer.material.color;

		/* Acquire the swivel locomotion component from the parent player gameObject, 	*
		 * catch exception if running the FPS controller, or if swivel is not available.*/
		try
		{
			swivel = this.transform.GetComponent<SwivelLocomotion> ();
		}
		catch (Exception e) 
		{
			Debug.LogWarning ("Swivel not enabled or found");
		}
    }

    void FixedUpdate()
    {
        Vector3 vel;
        switch (currentState)
        {
            case (PlayerState.Grounded):
                break;
            case (PlayerState.InWater_Falling):
                break;
            case (PlayerState.InWater_Float):
                vel = rb.velocity;
                rb.useGravity = false;
				#if UNITY_EDITOR
					rb.AddForce(debug_SinkSpeed * Physics.gravity);
				#else
                	rb.AddForce(Physics.gravity);
				#endif
                rb.drag = -dragPercentageWater * vel.y;
                break;
		    case (PlayerState.Space):
				rb.velocity = Vector3.zero;
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // find the collider that determines to transition to the next state
        foreach (GameObject go in transitionColliders)
        {
            if (other.gameObject == go)
            {
                int index = transitionColliders.IndexOf(go);
                // TODO: generalize/don't hardcode
                switch (index)
                {
                    case 0:
                        currentState = PlayerState.InWater_Falling;
                        break;
                    case 1:
                        currentState = PlayerState.InWater_Float;
                        break;
					case 2:
						currentState = PlayerState.Space;
						other.gameObject.SetActive(false); //disable so it can't trigger again
                        break;
                }
				UpdateState();
                break;
            }
        }

        // if it is the fadeTrigger
        if (other.gameObject == fadeTrigger)
        {
            GameManager.Instance.SeaSpaceFadeTransition();
			//StartCoroutine(FadeTransition());
        }
    }

    public void ResetPlayer()
    {
        currentState = PlayerState.Grounded;
        UpdateState();
        this.transform.position = startPos;
        this.transform.SetParent(null);
    }

    private void UpdateState()
    {
		Debug.Log("Change state to: " + currentState);
        switch (currentState)
        {
			case (PlayerState.Grounded):
				rb.useGravity = true;
                break;
			case (PlayerState.InWater_Falling):
				/*spriteController.DisableParentAnimator();
				sprite.transform.SetParent(this.transform, true);
				StartCoroutine("MoveSpriteLake");
				SoundController.Instance.EnterLake();
				RenderSettings.skybox = spaceSkybox;*/
				spriteController.DisableParentAnimator();
				sprite.transform.SetParent(this.transform, true);
				StartCoroutine("MoveSpriteLake");
				SoundController.Instance.EnterLake();
				RenderSettings.skybox = spaceSkybox;
				
                // Re-position bubbles to where player is and offset in y-axis
                bubbles.SetActive(true);
                bubbles.transform.position = this.transform.position - new Vector3(0, bubblesOffset, 0);

				//disable movement
				if (swivel != null) 
				{
					swivel.SetSwivelState(SwivelLocomotion.SwivelState.inSea);
				}
				break;
				
            case (PlayerState.InWater_Float):
				upperLakesurface.SetActive(false);
                if (doTwist)
                    StartCoroutine("Rotate");
                break;

			case (PlayerState.Space):
                //StartCoroutine(SpaceEploration());

				rb.useGravity = false;
				rb.drag = 0;
                rb.velocity = Vector3.zero;
                break;
        }

        PlayerUpdateStateEvent.Invoke();
    }

    /*private IEnumerator MoveSpriteLake()
    {
		while (Vector3.Distance(sprite.transform.localPosition, spriteSeaLocation) > 1e-6)
        {
			Debug.Log ("Sprite moving towards player");
			sprite.transform.localPosition = Vector3.MoveTowards(sprite.transform.localPosition,
                spriteSeaLocation, 10 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }*/

    // fade out lake & fade in star clusters
    /*private IEnumerator FadeTransition()
    {
		// TODO: fade these out properly
		jellyfishes.SetActive(false);
        // TODO: probably should gc with object pool
		fishes.SetActive(false);
		seaRenderer.material = seaFadeMaterial;
		forestWorld.SetActive(false);

        // Set all star cluster renderers to not render anything to fade in
        Renderer[] renderers = starCluster.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material.SetColor("_Color_Tint", new Color(0, 0, 0, 0));
        }

        starCluster.SetActive(true);
        float distance = transform.position.y -
            transitionColliders[transitionColliders.Count - 1].transform.position.y;
        Vector3 prevPosition = transform.position;
        yield return new WaitForFixedUpdate();

        // Do fade
        float progress = 0f;
		Color color;
        while (progress <= distance)
        {
            progress += Vector3.Distance(prevPosition, transform.position) / distance;
            foreach (Renderer r in renderers)
            {
				// fade in
                r.material.SetColor("_Color_Tint", new Color(progress, progress, progress, progress));
            }
            prevPosition = transform.position;

			// fade out
			color = new Color(seaColor.r, seaColor.g, seaColor.b, 1 - progress);
			seaRenderer.material.color = color;
            yield return new WaitForFixedUpdate();
        }
    }*/

    /*public IEnumerator SpaceEploration()
    {
		Debug.Log("Starting coroutine SpaceExploration.");
        // Disable the sea world

        sea.transform.parent.gameObject.SetActive(false); sprite.transform.position = this.transform.position;

		spriteController.DisableParentAnimator();

		// Move the sprite to where it should be position during exploration
		swivel.enabled = false;
		rb.velocity = Vector3.zero;
		yield return new WaitForFixedUpdate();

		while (Vector3.Distance(sprite.transform.position, spriteExplorationPoint.position) > 1)
		{
			sprite.transform.position = Vector3.MoveTowards(sprite.transform.position,
                spriteExplorationPoint.position, spriteSetupSpeed * Time.fixedDeltaTime);
			yield return new WaitForFixedUpdate();
		}
		swivel.enabled = true;

		yield return StartCoroutine(spriteController.Explore(swivel));

        StartCoroutine(EarthGaze());
    }*/

    /*private IEnumerator EarthGaze()
    {
		// Level objects so its not reliant on the angle the player is looking
		earth.transform.position = new Vector3(earth.transform.position.x, this.transform.position.y,
			earth.transform.position.z);
		sun.transform.position = new Vector3(sun.transform.position.x, this.transform.position.y + sunEarthDeltaY,
			sun.transform.position.z);      // offset sun slightly in y-axis
        spriteRotationPoint.position = new Vector3(spriteRotationPoint.position.x,
			this.transform.position.y, spriteRotationPoint.position.z);
		spriteRotationPoint.transform.position = new Vector3(spriteRotationPoint.transform.position.x,
			this.transform.position.y, spriteRotationPoint.transform.position.z);

        /* Enable the earth & sun, set earth at 23.5 tilt
		 *  and reset sun's rotation, and parent to space world
		
        // Trigger earth gaze sound
		SoundController.Instance.PlayEarthGaze();

		/* Enable the earth & sun,
		 * set earth at 23.5 tilt and reset sun's rotation,
		 * and parent to space world

        earth.SetActive(true);
        sun.SetActive(true);
		earth.transform.eulerAngles = new Vector3(0, 0, 23.5f);
		sun.transform.rotation = Quaternion.identity;

        earth.transform.SetParent(space.transform);
        sun.transform.SetParent(space.transform);
        spriteRotationPoint.SetParent(space.transform);
		spriteRotationPoint.SetParent(space.transform);

		// Move the sprite to where it should be position during rotation
		while (Vector3.Distance(sprite.transform.position, spriteRotationPoint.position) > 1e-6)
		{
			sprite.transform.position = Vector3.MoveTowards(sprite.transform.position, spriteRotationPoint.position,
				spriteSetupSpeed * Time.fixedDeltaTime);
			yield return new WaitForFixedUpdate();
		}
		//Disable leaning locomotion
		if (swivel != null) 
		{
			swivel.enabled = false;
			rb.velocity = Vector3.zero;
		}

       

        // Trigger the next part
		spriteController.TriggerEarthGaze(earth.transform, spriteRotationPoint);
    }*/

	/*public IEnumerator Resolution()
	{
		Debug.Log("Starting coroutine Resolution.");
		sun.SetActive(false);
		// Fade out
		SteamVR_Fade.Start(Color.clear, 0f);    // Set start color
		SteamVR_Fade.Start(Color.black, fadeDuration);  // Set and start fade to
        SoundController.Instance.PlayResolution(fadeDuration);

		forestWorld.SetActive(true);
		timeManager.timeline = morningTime;
		//yield return new WaitForSeconds(fadePauseDuration);
		// Return to tent
		transform.SetParent(null);
		transform.position = startPos;
		currentState = PlayerState.Grounded;
		UpdateState();
		rb.velocity = Vector3.zero;
		swivel.enabled = false;
		// Change skybox
		RenderSettings.skybox = morningSkybox;

		// Fade in
		SteamVR_Fade.Start(Color.black, 0f);
		SteamVR_Fade.Start(Color.clear, fadeDuration);

        // Final fade out to end experience
		yield return new WaitForSeconds(finalFadeDuration);
		SteamVR_Fade.Start(Color.clear, 0f);
		SteamVR_Fade.Start(Color.black, fadeDuration);
	}*/
}
