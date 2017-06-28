using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using VRTK;

public class PlayerControl : MonoBehaviour
{
    // public variables
    [Header("GameObject Variables")]
    [Space(5)]
    public GameObject spriteWrap;
    public GameObject spriteLake;
    public float speed = 50f;

    [Header("Control Variables")]
    [Space(5)]
    public VRTK_TouchpadControl touchpadControl;

    [Header("In Water Variables")]
    [Space(5)]

    [Tooltip("Falling without drag within this period of distance")]
    public float fallingDistance = 10.0f;

    [Tooltip("The factor to calculate the drag force when in the water. The bigger this percentage, the more the drag force will be according to the current speed, and the faster the player is going to reach a stable speed.")]
    [Range(0, 1)]
    public float dragPercentageWater = 0.2f;

    [Header("In Cloud Variables")]
    [Space(5)]

    [Tooltip("This is to decide whether the player enters the space from the water. It should be the bottom position of the cloud.")]
    public float enterCloudElevaton = -200.0f;

    [Tooltip("This is to decide whether the player enters the space from the water. It should be the bottom position of the cloud.")]
    public float enterSpaceElevaton = -260.0f;

    [Tooltip("The factor to calculate the drag force when in the water. The bigger this percentage, the more the drag force will be according to the current speed, and the faster the player is going to reach a stable speed.")]
    [Range(0, 1)]
    public float dragPercentageCloud = 0.5f;

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

    [Tooltip("")]
    [SerializeField] private List<GameObject> transitionColliders;

    public enum PlayerState { Grounded, InWater_Falling, InWater_Float, Space };

    // private variables
    private Rigidbody rb;
    private PlayerState currentState = PlayerState.Grounded;
    private Transform spriteParent;     // original parent of sprite
    private float rotateProgress = 0f;
    private float fadeProgress = 0f;
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
        spriteParent = spriteWrap.transform.parent;
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
                spriteWrap.transform.position = Vector3.MoveTowards(spriteWrap.transform.position, new Vector3(-7.1f, -283, 162), speed * Time.deltaTime);

                vel = rb.velocity;
                rb.useGravity = false;
                rb.drag = 0;
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
                break;
            }
        }

        UpdateState();
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case (PlayerState.Grounded):
                break;
            case (PlayerState.InWater_Falling):
                //spriteLake.SetActive(true);
                spriteWrap.transform.SetParent(this.transform, true);
                StartCoroutine("MoveSprite");
                SoundController.Instance.EnterLake();
                break;
            case (PlayerState.InWater_Float):
                if (doTwist)
                    StartCoroutine("Rotate");
                break;
            case (PlayerState.Space):
                /*spriteWrap.transform.position = spriteLake.transform.position;
                spriteWrap.SetActive(true);
                spriteLake.SetActive(false);*/
                spriteWrap.transform.SetParent(spriteParent, true);
                break;
        }
    }

    private IEnumerator MoveSprite()
    {
        // TODO: don't hardcode
        Vector3 dst = new Vector3(-3.6f, 1.2f, -5.5f);
        while (Vector3.Distance(spriteWrap.transform.position, dst) < 1e-6)
            {
            Vector3.MoveTowards(spriteWrap.transform.position, dst, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator Rotate()
    {
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
        while (fadeProgress < 1f)
        {
            fadeProgress += Time.fixedDeltaTime / fadeDuration;
            float alpha = 1 - fadeProgress;
            Color color = new Color(seaColor.r, seaColor.g, seaColor.b, alpha);
            seaRenderer.material.color = color;
            yield return new WaitForFixedUpdate();
        }
    }
}
