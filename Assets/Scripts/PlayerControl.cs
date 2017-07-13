using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using VRTK;
using System;

public class PlayerControl : MonoBehaviour
{
    [Header("GameObject Variables")]
    [Space(5)]
	[SerializeField] private GameObject sprite;
	[SerializeField] private float speed = 50f;
	[SerializeField] private GameObject fadeTrigger;

    [Header("Control Variables")]
    [Space(5)]
	[SerializeField] private VRTK_TouchpadControl touchpadControl;

    [Header("In Water Variables")]
    [Space(5)]

    [Tooltip("The factor to calculate the drag force when in the water. The bigger this percentage, the more the drag force will be according to the current speed, and the faster the player is going to reach a stable speed.")]
    [Range(0, 1)]
	[SerializeField] private float dragPercentageWater = 0.2f;

	[Header("Fade Out Parameters")]
	[Space(5)]
    [Tooltip("A flag to determine if the player is rotation while floating in the sea.")]
    [SerializeField] private bool doTwist = false;
    [Tooltip("The amount the player will rotaion while floating in the sea.")]
    [Range(0,360)]
    [SerializeField] private int twistAngle = 90;
    [Tooltip("The time it takes for the player to rotate the Twist Angle while floating in the sea.")]
    [SerializeField] private float twistDuration = 20.0f;

    // TODO: probably move this somewhere else
    [Tooltip("A flag to determine if the sea will fade out while the player is floating.")]
    [SerializeField] private bool doFade = false;
    [Tooltip("The time it takes for the sea to fade completely away.")]
    [SerializeField] private float fadeDuration = 20.0f;
	[Tooltip("The location of the sprite relative to the player in the sea.")]
	[SerializeField] private Vector3 spriteSeaLocation = new Vector3(-3.6f, 1.2f, -5.5f); 
	[SerializeField] private WaterFog waterFog;
	[SerializeField] private GameObject sea;
	[SerializeField] private GameObject jellyfishes;
	[SerializeField] private GameObject fishes;

    [Tooltip("The list of GameObjects to collide with to transition into the next state.")]
    [SerializeField] private List<GameObject> transitionColliders;

    [SerializeField] private GameObject earth;
    [SerializeField] private GameObject sun;
    [SerializeField] private GameObject space;
	[Tooltip("The point where the rotation around earth will begin.")]
	[SerializeField] private Transform rotationPoint;

    public enum PlayerState { Grounded, InWater_Falling, InWater_Float, Space, Earth_Gaze };

    // private variables
    private Rigidbody rb;
	private PlayerState currentState = PlayerState.Grounded;
    private Transform spriteParent;     // original parent of sprite
    // used to get the material to change the opacity
    private Renderer seaRenderer;
    private Color seaColor;
	private SwivelLocomotion swivel;
	private SpriteController spriteController;

    void Start()
    {
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
			swivel = this.transform.parent.gameObject.GetComponent<SwivelLocomotion> ();
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
                rb.AddForce(1f * Physics.gravity);
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
                        break;
                }
				UpdateState();
                break;
            }
        }

		// if it is the fadeTrigger
		if (other.gameObject == fadeTrigger)
			StartCoroutine("FadeOut");
    }

    private void UpdateState()
    {
		Debug.Log(currentState);
        switch (currentState)
        {
            case (PlayerState.Grounded):
                break;
			case (PlayerState.InWater_Falling):
				spriteController.DisableParentAnimator ();
				sprite.transform.SetParent (this.transform, true);
				StartCoroutine ("MoveSpriteLake");
				SoundController.Instance.EnterLake ();
				//disable movement
				if (swivel != null) 
				{
					swivel.enabled = false;
				}
				break;
				
            case (PlayerState.InWater_Float):
                if (doTwist)
                    StartCoroutine("Rotate");
                break;
			case (PlayerState.Space):
				SoundController.Instance.EnterSpace ();
				StartCoroutine ("EarthGaze");
				sprite.transform.SetParent (spriteParent, true);

				rb.useGravity = false;
				rb.drag = 0;
                rb.velocity = Vector3.zero;

                //enable locomotion
                /*if (swivel != null) 
				{
					swivel.enabled = true;
				}*/
                break;
        }
    }

    private IEnumerator MoveSpriteLake()
    {
		while (Vector3.Distance(sprite.transform.localPosition, spriteSeaLocation) > 1e-6)
        {
			Debug.Log ("Sprite moving towards player");
			sprite.transform.localPosition = Vector3.MoveTowards(sprite.transform.localPosition, spriteSeaLocation, 10 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    // TODO: probably move this somewhere else
    private IEnumerator FadeOut()
    {
		jellyfishes.SetActive(false);
		fishes.SetActive(false);

		float fadeProgress = 0f;
        while (fadeProgress < 1f)
        {
            fadeProgress += Time.fixedDeltaTime / fadeDuration;
            float alpha = 1 - fadeProgress;
            Color color = new Color(seaColor.r, seaColor.g, seaColor.b, alpha);
            seaRenderer.material.color = color;
            yield return new WaitForFixedUpdate();
        }
    }
		
    private IEnumerator EarthGaze()
    {
		// Update state so it can't trigger again
        currentState = PlayerState.Earth_Gaze;
        // Disable the sea world
		sea.transform.parent.gameObject.SetActive(false);

		/* Enable the earth & sun,
		 * set earth at 23.5 tilt and reset sun's rotation,
		 * and parent to space world*/
        earth.SetActive(true);
        sun.SetActive(true);
		earth.transform.eulerAngles = new Vector3(0, 0, 23.5f);
		sun.transform.rotation = Quaternion.identity;

        earth.transform.SetParent(space.transform, true);
        sun.transform.SetParent(space.transform, true);
		rotationPoint.transform.SetParent(space.transform, true);

        // Trigger earth gaze sound
        SoundController.Instance.PlayEarthGaze();

        // Parent to sprite to follow it
		// Do dumb loop since it doesn't set the first time
		while (this.transform.parent.parent == null) {
			this.transform.parent.SetParent(sprite.transform, true);
			yield return new WaitForSeconds(1.0f);
		}

        // Trigger the next part
		spriteController.TriggerEarthGaze(earth.transform, rotationPoint);
    }
}
