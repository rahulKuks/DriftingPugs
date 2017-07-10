using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using VRTK;

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
	[SerializeField] private GameObject sea;
	[SerializeField] private GameObject jellyfishes;
	[SerializeField] private GameObject fishes;

    [Tooltip("The list of GameObjects to collide with to transition into the next state.")]
    [SerializeField] private List<GameObject> transitionColliders;

    [SerializeField] private GameObject earth;
    [SerializeField] private GameObject sun;
    [SerializeField] private GameObject space;

    [Tooltip("The duration the user wanders around before the light appears.")]
    [SerializeField]
    private float wanderingDuration = 40f;
    [Tooltip("The duration before the light starts orbiting.")]
    [SerializeField]
    private float pauseDuration = 10f;

    public enum PlayerState { Grounded, InWater_Falling, InWater_Float, Space };

    // private variables
    private Rigidbody rb;
	[SerializeField]private PlayerState currentState = PlayerState.Grounded;
    private Transform spriteParent;     // original parent of sprite
    // used to get the material to change the opacity
    private Renderer seaRenderer;
    private Color seaColor;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Unable to get rigidbody.", this.gameObject);
        }
        spriteParent = sprite.transform.parent;
        seaRenderer = sea.GetComponent<Renderer>();
        seaColor = seaRenderer.material.color;
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
                sprite.transform.position = Vector3.MoveTowards(sprite.transform.position, new Vector3(-7.1f, -283, 162), speed * Time.deltaTime);
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
                sprite.transform.SetParent(this.transform, true);
				StartCoroutine("MoveSpriteLake");
                SoundController.Instance.EnterLake();
                break;
            case (PlayerState.InWater_Float):
                if (doTwist)
                    StartCoroutine("Rotate");
                break;
			case (PlayerState.Space):
				SoundController.Instance.EnterSpace();
                StartCoroutine("EarthGaze");
                sprite.transform.SetParent(spriteParent, true);

				rb.useGravity = false;
				rb.drag = 0;
                break;
        }
    }

    private IEnumerator MoveSpriteLake()
    {
        // TODO: don't hardcode
        Vector3 dst = new Vector3(-3.6f, 1.2f, -5.5f);
		while (Vector3.Distance(sprite.transform.localPosition, dst) > 1e-6)
            {
			sprite.transform.localPosition = Vector3.MoveTowards(sprite.transform.localPosition, dst, 10 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator Rotate()
    {
		float rotateProgress = 0f;
        while (rotateProgress < 1f)
        {
            rotateProgress += Time.fixedDeltaTime / twistDuration;
            transform.root.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.AngleAxis(twistAngle, Vector3.right), rotateProgress);
            yield return new WaitForFixedUpdate();
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
		sea.transform.parent.gameObject.SetActive(false);
        float progress = 0f;
        while (progress < wanderingDuration)
        {
            progress += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        earth.SetActive(true);
        sun.SetActive(true);


        earth.transform.SetParent(space.transform, true);
        sun.transform.SetParent(space.transform, true);


    }
}
