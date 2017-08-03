using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class GameManager : MonoBehaviour {

    #region GameObject variables
    [SerializeField] private Transform player;
    [SerializeField] private Transform sprite;
    [SerializeField] private AC.TimeOfDaySystemFree.TimeOfDayManager timeManager;

    // For forest world
    [SerializeField] private GameObject forestWorld;

    // For sea world
    [SerializeField] private GameObject sea;
    [SerializeField] private GameObject jellyfishes;
    [SerializeField] private GameObject fishes;
    [SerializeField] private Material seaFadeMaterial;
    [SerializeField] private GameObject upperLakesurface;

    // For space world
    [SerializeField] private GameObject starCluster;
    [SerializeField] private GameObject earth;
    [SerializeField] private GameObject sun;
    [SerializeField] private GameObject space;
    [Tooltip("The position of the sprite relative to the user during the space exploration phase.")]
    [SerializeField] private Transform spriteExplorationPoint;
    [Tooltip("The point where the rotation around earth will begin.")]
    [SerializeField] private Transform spriteRotationPoint;
	[SerializeField] private Transform playerRotationPoint;
    [Tooltip("Objects in space that needs to placed relative to where the player will be in space.")]
    [SerializeField] private Transform spacePivot;
    [SerializeField] private Transform landingPoint;
    [SerializeField] private Transform speedUpCheckpoint;

    // For transitions
	[SerializeField] private Transform fadeTrigger;
	[SerializeField] private Transform spaceTrigger;
    #endregion

    #region Space Parameters
    [SerializeField] private Material spaceSkybox;
    [SerializeField] private float initialSpaceSpeed = 2.5f;
    [Tooltip("The speed at which the sprite will move towards from the speed up checkpoint to the rotation" +
        "point during space.")]
    [SerializeField] private float spaceSpedUpSpeed = 5.0f;

    [Tooltip("The speed of the sprite moving towards the sprite exploration point.")]
    [SerializeField] private float spriteSetupSpeed = 10.0f;
    [SerializeField] private float rotationDuration = 180.0f;
    #endregion

    #region Resolution Parameters
    [Header("Resolution Parameters")]
    [Space(5)]

    [Tooltip("The duration it takes for the camera to fade out to black or in from black.")]
    [SerializeField] private float fadeDuration = 1.0f;
    [Tooltip("The duration in between the fade out and fade in.")]
    [SerializeField] private float fadePauseDuration = 1.0f;
    [SerializeField] private Material morningSkybox;
    [Tooltip("The time before completely fading out and ending the experience.")]
    [SerializeField] private float finalFadeDuration = 5.0f;
    [SerializeField] private float morningTime;
    #endregion

    #region Private Variables
    private static readonly float SPEED_DURATION_RATIO = 20/11f;

    private PlayerControl playerControl;
    private SwivelLocomotion playerSwivel;
    private SpriteController spriteController;

    private Renderer seaRenderer;       // Used to get the material to change the opacity
    private Color seaColor;             // Used to preserve the color while changing the alpha level

    private GameObject spaceParent;
    private float spaceSpeed;
    private bool isSpeedUp = false;

    private float sunEarthDeltaY;
    #endregion

    // Singleton pattern
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        // Compute here else the delta is different cuz the objects are moving
        sunEarthDeltaY = sun.transform.position.y - earth.transform.position.y;
    }

    void Start()
    {
        playerControl = player.gameObject.GetComponent<PlayerControl>();
        playerSwivel = player.gameObject.GetComponent<SwivelLocomotion>();
        spriteController = sprite.GetComponent<SpriteController>();

		seaRenderer = sea.GetComponent<Renderer>();
		seaColor = seaRenderer.material.color;

        // Setup dummy parent object
        spaceParent = new GameObject()
        {
            name = "Space Parent"
        };
        spaceParent.SetActive(false);
        spaceParent.transform.SetParent(this.transform);
    }

    public void SeaSpaceFadeTransition()
    {
        StartCoroutine(FadeTransition());
    }

    public void OnPlayerStateChange()
    {
        switch(playerControl.CurrentState)
        {
            case PlayerControl.PlayerState.Grounded:
                break;
            case PlayerControl.PlayerState.InWater_Falling:
                StartCoroutine(LakeFall());
                break;
            case PlayerControl.PlayerState.InWater_Float:
                upperLakesurface.SetActive(false);
                break;
            case PlayerControl.PlayerState.Space:
                StartCoroutine(Space());
                break;
        }
    }

    #region Coroutines
    // Fade out lake & fade in star clusters
    private IEnumerator FadeTransition()
    {
        // TODO: fade these out properly
        jellyfishes.SetActive(false);
        // TODO: probably should gc with object pool
        fishes.SetActive(false);
        forestWorld.SetActive(false);
        seaRenderer.material = seaFadeMaterial;

        // Set all star cluster renderers to not render anything to fade in
        Renderer[] renderers = starCluster.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material.SetColor("_Color_Tint", new Color(0, 0, 0, 0));
        }

        starCluster.SetActive(true);
		//float distance = player.position.y - fadeTrigger.position.y;
		float distance = Mathf.Abs(fadeTrigger.position.y - spaceTrigger.position.y);
        Vector3 prevPosition = player.position;
        yield return new WaitForFixedUpdate();

		Debug.Log(distance);

        // Do fade
        float progress = 0f;
		float value;
        Color color;
		//while (progress <= distance)
		while ((distance - progress) > 2.0f)
        {
			//progress += Vector3.Distance(prevPosition, player.position) / distance;
			progress += Mathf.Abs(prevPosition.y - player.position.y);
			Debug.Log(progress);
			value = progress / distance;
			//Debug.Log(value);
            foreach (Renderer r in renderers)
            {
                // fade in
				//r.material.SetColor("_Color_Tint", new Color(progress, progress, progress, progress));
				r.material.SetColor("_Color_Tint", new Color(value, value, value, value));
            }
			prevPosition = player.position;

            // fade out
            color = new Color(seaColor.r, seaColor.g, seaColor.b, 1 - value);
            seaRenderer.material.color = color;
            yield return new WaitForFixedUpdate();
        }
		Debug.Log("here");
    }

    private IEnumerator LakeFall()
    {
        spriteController.DisableParentAnimator();
		sprite.transform.SetParent(player, true);
        SoundController.Instance.EnterLake();
        RenderSettings.skybox = spaceSkybox;
        yield return null;
    }

    private IEnumerator Space()
    {
        Debug.Log("Entered space.");
        SoundController.Instance.EnterSpace();
        // Disable the other worlds
        forestWorld.SetActive(false);
        sea.transform.parent.gameObject.SetActive(false);

        spriteController.DisableParentAnimator();

        // Move the sprite to where it should be position during exploration
        playerSwivel.enabled = false;

        while (Vector3.Distance(sprite.position, spriteExplorationPoint.position) > 1)
        {
            sprite.position = Vector3.MoveTowards(sprite.position,
                spriteExplorationPoint.position, spriteSetupSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        playerSwivel.enabled = true;

        // Make objects in space pivot relative to player and set them up
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
        spaceParent.transform.position = player.position;
        spaceParent.transform.SetParent(null);
        spaceParent.SetActive(true);
        sprite.SetParent(spaceParent.transform);
        player.SetParent(spaceParent.transform);

        if (playerSwivel != null)
        {
            //Enable space constraints and parameters
            playerSwivel.SetSwivelState(SwivelLocomotion.SwivelState.inSpace);
        }

        spaceSpeed = initialSpaceSpeed;
        Vector3 dest = Vector3.zero;

        Debug.Log("Starting space exploration");
        // Move through space checkpoints
        for (int i = 0; i < landingPoint.childCount; i++)
        {
            dest = landingPoint.GetChild(i).transform.position;
            while (Vector3.Distance(spaceParent.transform.position, dest) > 1e-6)
            {
                // Gradually speed up at speed up checkpoint
                if (isSpeedUp)
                {
                    spaceSpeed = Mathf.Lerp(spaceSpeed, spaceSpedUpSpeed, Time.fixedDeltaTime);
                }

                spaceParent.transform.position = Vector3.MoveTowards(spaceParent.transform.position,
                    dest, spaceSpeed * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            if (!isSpeedUp)
                isSpeedUp = landingPoint.GetChild(i).GetInstanceID() == speedUpCheckpoint.GetInstanceID();
        }

        Debug.Log("Starting earth gaze");
        // Level objects so its not reliant on the angle the player is looking
		earth.transform.position = new Vector3(earth.transform.position.x, player.transform.position.y,
            earth.transform.position.z);
		sun.transform.position = new Vector3(sun.transform.position.x, player.transform.position.y +
            sunEarthDeltaY, sun.transform.position.z);      // offset sun slightly in y-axis
        spriteRotationPoint.position = new Vector3(spriteRotationPoint.position.x,
			player.transform.position.y, spriteRotationPoint.position.z);
		playerRotationPoint.transform.position = new Vector3(playerRotationPoint.transform.position.x,
			player.transform.position.y, playerRotationPoint.transform.position.z);

        /* Enable the earth & sun, set earth at 23.5 tilt
		 *  and reset sun's rotation, and parent to space world*/
        earth.SetActive(true);
        sun.SetActive(true);
        earth.transform.eulerAngles = new Vector3(0, 0, 23.5f);
        sun.transform.rotation = Quaternion.identity;

        earth.transform.SetParent(space.transform);
        sun.transform.SetParent(space.transform);
        spriteRotationPoint.SetParent(space.transform);
		playerRotationPoint.SetParent(space.transform);

        // Move the sprite to where it should be position during rotation
        while (Vector3.Distance(sprite.position, spriteRotationPoint.position) > 1e-6)
        {
            sprite.position = Vector3.MoveTowards(sprite.position, spriteRotationPoint.position,
                spriteSetupSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        // Disable leaning locomotion
        if (playerSwivel != null)
        {
            playerSwivel.enabled = false;
        }

        // Play earth gaze sound
        SoundController.Instance.PlayEarthGaze();

        Debug.Log("Moving towards rotation point.");
        // Move towards to position where the rotation will begin
		while (Vector3.Distance(spaceParent.transform.position, playerRotationPoint.position) > 1e-6)
        {
            spaceParent.transform.position = Vector3.MoveTowards(spaceParent.transform.position,
				playerRotationPoint.position, spaceSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("Do rotation");
        // Calculate speed using rotation duration
        // Distant travelled is the circumference thus 2*pi*r
        float radius = Vector3.Distance(spaceParent.transform.position, earth.transform.position);
        float rotationSpeed = 2 * Mathf.PI * radius / (rotationDuration * SPEED_DURATION_RATIO);

        // Rotate around earth for the rotation duration
        float progress = 0f;
        bool fadeTriggered = false;
        while (progress <= (rotationDuration + playerControl.FadeDuration))
        {
            progress += Time.fixedDeltaTime;
            spaceParent.transform.RotateAround(earth.transform.position, Vector3.up,
                rotationSpeed * Time.fixedDeltaTime);
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
        playerControl.ResetPlayer();
        playerSwivel.enabled = false;
        // Change skybox
        RenderSettings.skybox = morningSkybox;

        // Fade in
        SteamVR_Fade.Start(Color.black, 0f);
        SteamVR_Fade.Start(Color.clear, fadeDuration);

        // Final fade out to end experience
        yield return new WaitForSeconds(finalFadeDuration);
        SteamVR_Fade.Start(Color.clear, 0f);
        SteamVR_Fade.Start(Color.black, fadeDuration);
    }
    #endregion
}
