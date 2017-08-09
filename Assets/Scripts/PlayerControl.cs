using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

public class PlayerControl : MonoBehaviour
{
    #region GameObject Variables
    // For transitions
    [SerializeField] private GameObject fadeTrigger;
    [Tooltip("The list of GameObjects to collide with to transition into the next state." +
        " Order of the GameObjects are important.")]
    [SerializeField] private List<GameObject> transitionColliders;
    #endregion

    #region In Lake Parameters
    [Header("Falling in Water Parameters")]
    [Space(5)]
    [Tooltip("The factor to calculate the drag force when in the water. The bigger this percentage," +
        " the more the drag force will be according to the current speed, and the" +
        " faster the player is going to reach a stable speed.")]
    [Range(0, 1)]
    [SerializeField] private float dragPercentageWater = 0.2f;
    [SerializeField] private GameObject bubbles;
    [Tooltip("How far down (y-axis) the bubbles will be offset when entering the lake.")]
    [SerializeField] private float bubblesOffset = 5.0f;
    #endregion

	#region Debug Parameters
	[Header("Debug Parameters")]
	[Space(5)]
	[SerializeField]
	private float debug_SinkSpeed = 1;
	#endregion

    #region Public Variables
    public enum PlayerState { Grounded, InWater_Falling, InWater_Float, Space };
    public PlayerState CurrentState { get { return currentState; } }
    public UnityEvent PlayerUpdateStateEvent;
    #endregion

    #region Private Variables
    private Vector3 startPos = Vector3.zero;    // The position to return to after space
    private Rigidbody rb;
    private PlayerState currentState = PlayerState.Grounded;
    private SwivelLocomotion swivel;
    #endregion

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Unable to get rigidbody.", this.gameObject);
        }

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

        startPos = this.transform.position;
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case (PlayerState.Grounded):
                break;
            case (PlayerState.InWater_Falling):
                break;
            case (PlayerState.InWater_Float):
                rb.useGravity = false;
				#if UNITY_EDITOR
					rb.AddForce(debug_SinkSpeed * Physics.gravity);
				#else
                	rb.AddForce(Physics.gravity);
				#endif
                rb.drag = -dragPercentageWater * rb.velocity.y;
                break;
		    case (PlayerState.Space):
				rb.velocity = Vector3.zero;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Find the collider that determines to transition to the next state
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
            GameManager.Instance.LakeSpaceFadeTransition();
        }
    }

    /// <summary>
    /// Resets the players position and state to where it was at the beginning.
    /// </summary>
    public void ResetPlayer()
    {
        currentState = PlayerState.Grounded;
        UpdateState();
        this.transform.position = startPos;
        this.transform.SetParent(null);
    }

    /// <summary>
    /// Update player based on the state it is in.
    /// Also invokes the PlayerUpdateState event for all observers.
    /// </summary>
    private void UpdateState()
    {
		Debug.Log("Change state to: " + currentState);
        switch (currentState)
        {
			case (PlayerState.Grounded):
				rb.useGravity = true;
                break;
			case (PlayerState.InWater_Falling):
				SoundController.Instance.EnterLake();
				
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
                break;
			case (PlayerState.Space):
				rb.useGravity = false;
				rb.drag = 0;
                rb.velocity = Vector3.zero;
                break;
        }

        PlayerUpdateStateEvent.Invoke();
    }
}
