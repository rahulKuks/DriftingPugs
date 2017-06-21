using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class PlayerControl : MonoBehaviour {
    // public variables
    [Header("GameObject Variables")]
    [Space(5)]
    public GameObject sea;
	public GameObject cloud;
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

    // private variables
    private Rigidbody rb;
	private int state;
	const int GROUNDED = 0;
    const int INWATER_FALL = 1;
    const int INWATER_FLOAT = 2;
    const int INCLOUD = 3;
    const int INSPACE = 4;

    void Start () {
		state = GROUNDED;
        rb = GetComponent<Rigidbody>();
    }

    void Update () {
        PrintState();
        //Debug.Log(spriteLake.transform.position - sprite.transform.position);
        if (state == GROUNDED)
        {
            if (transform.position.y < -1)
            {
                Debug.Log("Change state: to in water falling");
                spriteLake.SetActive(true);
                //spriteWrap.SetActive(false);
                state = INWATER_FALL;
            }
        }
        else if (state == INWATER_FALL)
        {
            if (transform.position.y < -fallingDistance)
            {
                Debug.Log("Change state: to in water floating");
                rb = GetComponent<Rigidbody>(); // in case the rigidbody hasn't been created by the VRTK when Start()
                touchpadControl.enabled = false;
                state = INWATER_FLOAT;
            }
        }
        else if (state == INWATER_FLOAT)
        {
            if (transform.position.y < enterCloudElevaton)
            {
                Debug.Log("Change state: to in cloud");
                state = INCLOUD;
            }
        }
        else if (state == INCLOUD)
        {
            if (transform.position.y < enterSpaceElevaton)
            {
                Debug.Log("Change state: to in space");
                cloud.SetActive(false);
				sea.SetActive(false);

                spriteWrap.transform.position = spriteLake.transform.position;
                //spriteWrap.transform.rotation = spriteLake.transform.rotation;
                spriteWrap.SetActive(true);
                spriteLake.SetActive(false);

                touchpadControl.enabled = true;
				state = INSPACE;
            }
        }
    }

	void FixedUpdate()
    {
        if (state == INWATER_FLOAT)
        {
            Vector3 vel = rb.velocity;
            rb.useGravity = false;
            rb.AddForce(1f * Physics.gravity);
            rb.drag = -dragPercentageWater * vel.y;
            //Debug.Log("velocity: " + vel.y);
        }
        else if (state == INCLOUD)
        {
            Vector3 vel = rb.velocity;
            rb.useGravity = false;
            rb.drag = -dragPercentageCloud * vel.y;
            //Debug.Log("velocity: " + vel.y);
        }
        else if (state == INSPACE)
        {
            Debug.Log(spriteLake.transform.position - spriteWrap.transform.position);
            spriteWrap.transform.position = Vector3.MoveTowards(spriteWrap.transform.position, new Vector3(-7.1f, -283, 162), speed * Time.deltaTime);

            Vector3 vel = rb.velocity;
            rb.useGravity = false;
            rb.drag = 0;
            //Debug.Log("velocity: " + vel.y);
        }
    }

    private void PrintState() {
        string stateStr = "";
        switch (state) {
            case GROUNDED:
                stateStr = "on the ground";
                break;
            case INWATER_FALL:
                stateStr = "in water falling";
                break;
            case INWATER_FLOAT:
                stateStr = "in water floating";
                break;
            case INCLOUD:
                stateStr = "in cloud";
                break;
            case INSPACE:
                stateStr = "in space";
                break;
        }
        Debug.Log("Current state: " + stateStr);
    }
}
