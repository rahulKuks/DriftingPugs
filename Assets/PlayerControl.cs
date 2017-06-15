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
    const int INWATER = 1;
    const int INCLOUD = 2;
    const int INSPACE = 3;

    void Start () {
		state = GROUNDED;
        rb = GetComponent<Rigidbody>();
    }

    void Update () {
        PrintState();

        if (state == GROUNDED)
        {
            if (transform.position.y < -fallingDistance)
            {
                Debug.Log("Change state: to in water");
                state = INWATER;
				rb = GetComponent<Rigidbody>(); // in case the rigidbody hasn't been created by the VRTK when Start()
				touchpadControl.enabled = false;
            }
        }
        else if (state == INWATER)
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
				touchpadControl.enabled = true;
				state = INSPACE;
            }
        }
    }

	void FixedUpdate()
    {
        if (state == INWATER)
        {
            Vector3 vel = rb.velocity;
            rb.useGravity = false;
            rb.AddForce(1f * Physics.gravity);
            rb.drag = -dragPercentageWater * vel.y;
            Debug.Log("velocity: " + vel.y);
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
            case INWATER:
                stateStr = "in water";
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
