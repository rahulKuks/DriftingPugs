using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class GameManager : MonoBehaviour {

    #region GameObject variables
    [SerializeField]
    private Transform player;
    [SerializeField]
    private GameObject sprite;
    [SerializeField]
    private AC.TimeOfDaySystemFree.TimeOfDayManager timeManager;

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
    [Tooltip("Objects in space that needs to placed relative to where the player will be in space.")]
    [SerializeField]
    private Transform spacePivot;
    [SerializeField]
    private Transform landingPoint;
    [SerializeField]
    private Transform speedUpCheckpoint;

    // For transitions
    [SerializeField]
    private GameObject fadeTrigger;
    [Tooltip("The list of GameObjects to collide with to transition into the next state." +
        " Order of the GameObjects are important.")]
    [SerializeField]
    private List<GameObject> transitionColliders;

    [SerializeField]
    private Material seaFadeMaterial;
    #endregion

    [SerializeField]
    private float initialMoveSpeed = 2.5f;
    [Tooltip("The speed at which the sprite will move towards from the speed up checkpoint to the rotation point during space.")]
    [SerializeField]
    private float moveSpeed = 5.0f;

    [Tooltip("The speed of the sprite moving towards the sprite exploration point.")]
    [SerializeField]
    private float spriteSetupSpeed = 10.0f;
    [SerializeField]
    private float rotationDuration = 180.0f;

    #region Private Variables
	private static readonly float SPEED_DURATION_RATIO = 20/11f;

    private PlayerControl playerControl;
    private SwivelLocomotion swivel;
    private SpriteController spriteController;

    private Renderer seaRenderer;       // Used to get the material to change the opacity
    private Color seaColor;             // Used to preserve the color while changing the alpha level

    private GameObject dummyParent;
    private float speed;
    private bool isSpeedUp = false;

    private float sunEarthDeltaY;
    private Transform rotationPoint;
    
    #endregion

    void Awake()
    {
        // Compute here else the delta is different cuz the objects are moving
        sunEarthDeltaY = sun.transform.position.y - earth.transform.position.y;
    }

    void Start()
    {
        playerControl = player.gameObject.GetComponent<PlayerControl>();
        swivel = player.gameObject.GetComponent<SwivelLocomotion>();

        spriteController = sprite.GetComponent<SpriteController>();
    }

    public void OnPlayerStateChange()
    {
        switch(playerControl.CurrentState)
        {
            case PlayerControl.PlayerState.Grounded:
                break;
            case PlayerControl.PlayerState.InWater_Falling:
                break;
            case PlayerControl.PlayerState.InWater_Float:
                break;
            case PlayerControl.PlayerState.Space:
                break;
        }
    }

    #region Coroutines
    // fade out lake & fade in star clusters
    private IEnumerator FadeTransition()
    {
        // TODO: fade these out properly
        jellyfishes.SetActive(false);
        // TODO: probably should gc with object pool
        fishes.SetActive(false);
        seaRenderer.material = seaFadeMaterial;

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
    }

    private IEnumerator Space()
    {
        Debug.Log("Starting space exploration.");
        // Disable the sea world
        forestWorld.SetActive(false);
        sea.transform.parent.gameObject.SetActive(false); sprite.transform.position = this.transform.position;

        spriteController.DisableParentAnimator();

        // Move the sprite to where it should be position during exploration
        swivel.enabled = false;
        //rb.velocity = Vector3.zero;

        // TODO move to sprite
        /*while (Vector3.Distance(sprite.transform.position, spriteExplorationPoint.position) > 1)
        {
            sprite.transform.position = Vector3.MoveTowards(sprite.transform.position,
                spriteExplorationPoint.position, spriteSetupSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }*/
        swivel.enabled = true;

        // TODO move to sprite
        /*childAnim.SetBool("inSpace", true);
        chimesAudio.volume = chimesForestSpaceVolume;*/
        // Make objects in space pivot relative to this and set them up

        spacePivot.position = player.position;
        GameObject go;
        for (int i = spacePivot.childCount - 1; i >= 0; i--)
        {
            go = spacePivot.GetChild(i).gameObject;
            go.SetActive(true);
            go.transform.SetParent(space.transform, true);
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
        for (int i = 0; i < landingPoint.childCount; i++)
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

        Debug.Log("Starting earth gaze");
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
		 *  and reset sun's rotation, and parent to space world*/
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
        // Disable leaning locomotion
        if (swivel != null)
        {
            swivel.enabled = false;
            // TODO: add in swivel
            //rb.velocity = Vector3.zero;
        }

        // Play earth gaze sound
        SoundController.Instance.PlayEarthGaze();

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
        float radius = Vector3.Distance(dummyParent.transform.position, earth.transform.position);
        float rotationSpeed = 2 * Mathf.PI * radius / (rotationDuration * SPEED_DURATION_RATIO);

        // Rotate around earth for the rotation duration
        /*float progress = 0f;
        bool fadeTriggered = false;
        while (progress <= (rotationDuration + playerControl.FadeDuration))
        {
            progress += Time.fixedDeltaTime;
            dummyParent.transform.RotateAround(earth.transform.position, Vector3.up, rotationSpeed * Time.fixedDeltaTime);
            if (progress >= rotationDuration && !fadeTriggered)
            {
                Debug.Log("Starting resolution");
                fadeTriggered = true;

                sun.SetActive(false);

                SteamVR_Fade.Start(Color.clear, 0f);    // Set start color
                SteamVR_Fade.Start(Color.black, fadeDuration);  // Set and start fade to
                SoundController.Instance.PlayResolution(fadeDuration);
            }
            yield return new WaitForFixedUpdate();
        }

        forestWorld.SetActive(true);
        timeManager.timeline = morningTime;
        //yield return new WaitForSeconds(fadePauseDuration);
        // Return to tent
        player.transform.SetParent(null);
        player.transform.position = startPos;
        currentState = PlayerState.Grounded;
        UpdateState();
        //rb.velocity = Vector3.zero; disable swivel
        swivel.enabled = false;
        // Change skybox
        RenderSettings.skybox = morningSkybox;

        // Fade in
        SteamVR_Fade.Start(Color.black, 0f);
        SteamVR_Fade.Start(Color.clear, fadeDuration);

        // Final fade out to end experience
        yield return new WaitForSeconds(finalFadeDuration);
        SteamVR_Fade.Start(Color.clear, 0f);
        SteamVR_Fade.Start(Color.black, fadeDuration);*/
    }
    #endregion
}
