using UnityEngine;
using System.Collections;

public class LeanFlying : MonoBehaviour
{
	/******************************************************************************************************************************/
	/**********             Speed Parameters: You can change them to move faster/slower toward each direction           ***********/
	/******************************************************************************************************************************/
	public float translationalSpeedLimit = -1;
	public float rotationalSpeedLimit = -1;
	public float forwardSpeed = 1;
	public float rotationSpeed = 1;
	public float upwardSpeed = 1;

	/******************************************************************************************************************************/
	/******   Vive Objects: Whenever you add this script to any project, drag vive controller objects into these variables    *****/
	/******************************************************************************************************************************/
	public GameObject viveCameraEye;
	public GameObject viveLeftController;
	public GameObject viveRightController;

	/******************************************************************************************************************************/
	/******                                                Other parameters                                                   *****/
	/******************************************************************************************************************************/

	public float groundLevel;

	/******************************************************************************************************************************/
	/**** 								Internal Variables: Don't change these variables										***/
	/******************************************************************************************************************************/
	float locomotionDirection = 0;
	Vector3 euler;
	Quaternion quat;
	bool interfaceIsReady = false, viveControllerTriggerStatus = false;
	float headXo = 0, headYo = 0, headZo = 0, headWidth = .09f;
	float exponentialTransferFuntionPower = 1.53f;
	int initializeStep = 0;

	//0 = before printing PressSpace message, 1 = after PressSpace message waiting for space, 2 = after space press and when the user can fly

	// Use this for initialization
	void Start ()
	{
		
	}

	void FixedUpdate ()
	{
		ReadControllerData (); //Read Vive Controller data and store them inside internal variables
	}

	// Update is called once per frame
	void Update ()
	{
		FlyingLocomotion (); //Apply the Vive Controller data to the user position in Virtual Environment
	}

	// *** Call this method in FixedUpdate() *** (updates the position of Vivev HMD and Controller at each frame inside internal variables)
	void ReadControllerData ()
	{
		//Check If the user released trigger
		if (viveControllerTriggerStatus && !viveRightController.GetComponent<SteamVR_TrackedController> ().triggerPressed) {
			//Debug.Log ("Left Controller pad is released!");
			viveControllerTriggerStatus = false;
		}

		//Print the initial message on screen
		if (initializeStep == 0) {
			Debug.Log ("Ask the user to sit straight and look forward and then press the controller trigger");
			initializeStep = 1;
		}

		//Check If the user pressed trigger
		if (!viveControllerTriggerStatus && viveRightController.GetComponent<SteamVR_TrackedController> ().triggerPressed) {
			//Debug.Log ("Left Controller trigger is pressed!");
			viveControllerTriggerStatus = true;

			//Read the Vive Controller data to calculate the ChairRotationRadious to measure the chair displacement
			float headYaw = viveCameraEye.transform.localRotation.eulerAngles.y;	
			headXo = viveCameraEye.transform.localPosition.x - headWidth * Mathf.Sin (headYaw * Mathf.PI / 180); //Calculate the Neck x Position
			headYo = viveCameraEye.transform.localPosition.y;
			headZo = viveCameraEye.transform.localPosition.z - headWidth * Mathf.Cos (headYaw * Mathf.PI / 180); //Calculate the Neck y Position

			Debug.Log ("Great! Now the user can fly");
			initializeStep = 2;

		}

	}


	// *** Call this method in update() *** (Uses the Vive HMD & Controller data stored in internal variables to move the player in Virtual Environment)
	void FlyingLocomotion ()
	{

		// ***************************  Caqlculate the forward locomotion  *******************************************
		float headYaw = viveCameraEye.transform.localRotation.eulerAngles.y;	
		float headX = viveCameraEye.transform.localPosition.x - headWidth * Mathf.Sin (headYaw * Mathf.PI / 180); //Calculate the Neck x Position
		float headZ = viveCameraEye.transform.localPosition.z - headWidth * Mathf.Cos (headYaw * Mathf.PI / 180); //Calculate the Neck y Position

		float forwardRatio = (headZ - headZo) * forwardSpeed;
		float rotationRatio = (headX - headXo) * rotationSpeed;
		float upwardRatio = (viveCameraEye.transform.localPosition.y - headYo) * upwardSpeed;

		Debug.Log("forwardRatio: " + forwardRatio + " rotationRatio: " + rotationRatio + " upward Ratio: " + upwardRatio);

		// **************************** Apply exponential transfer function ***********************************************
		float forwardVelocity, rotationVelocity, upwardVelocity;
		//Apply Exponential transfer function on forward/backward locomotion
		if (forwardRatio > 0)
			forwardVelocity = Mathf.Pow ((float)(forwardRatio), exponentialTransferFuntionPower);
		else
			forwardVelocity = -Mathf.Pow ((float)(-forwardRatio), exponentialTransferFuntionPower);
		//Apply Exponential transfer function on rotational locomotion
		if (rotationRatio > 0)
			rotationVelocity = Mathf.Pow ((float)(rotationRatio), exponentialTransferFuntionPower);
		else
			rotationVelocity = -Mathf.Pow ((float)(-rotationRatio), exponentialTransferFuntionPower);
		//Apply Exponential transfer function on up/down locomotion
		if (upwardRatio > 0)
			upwardVelocity = Mathf.Pow ((float)(upwardRatio), exponentialTransferFuntionPower);
		else
			upwardVelocity = -Mathf.Pow ((float)(-upwardRatio), exponentialTransferFuntionPower);
		//Debug.Log ("Velocity: Forward = " + forwardVelocity + " - Rotation = " + rotationVelocity + " Upward = " + upwardVelocity + " - Yaw = " + headYaw);

		// **************************** Limiting the speed if needed ***********************************************
		if (translationalSpeedLimit >= 0) {
			if (forwardVelocity > translationalSpeedLimit)
				forwardVelocity = translationalSpeedLimit;
			else if (forwardVelocity < -translationalSpeedLimit)
				forwardVelocity = -translationalSpeedLimit;			
			if (rotationVelocity > rotationalSpeedLimit)
				rotationVelocity = rotationalSpeedLimit;
			else if (rotationVelocity < -rotationalSpeedLimit)
				rotationVelocity = -rotationalSpeedLimit;
			if (upwardVelocity > translationalSpeedLimit)
				upwardVelocity = translationalSpeedLimit;
			else if (upwardVelocity < -translationalSpeedLimit)
				upwardVelocity = -translationalSpeedLimit;
		}
		//Debug.Log ("Limited Velocity: Forward = " + forwardVelocity + " - Rotation = " + rotationVelocity + " Upward = " + upwardVelocity + " - Yaw = " + headYaw);


		//  ****************************** Calculate the Rotatoion locomotion *****************************************
		if (initializeStep == 2) {

			locomotionDirection += rotationVelocity * Time.deltaTime;
			euler.y = locomotionDirection;
			quat.eulerAngles = euler;
			transform.rotation = quat;
			//Debug.Log ("Speed: Forward = " + forwardVelocity + " - Rotation = " + rotationVelocity + " Upward = " + upwardVelocity + " - Yaw = " + headYaw + " - Direction = " + (int)(locomotionDirection));


			float translateX = (float)(forwardVelocity * Time.deltaTime * Mathf.Sin ((float)(locomotionDirection * Mathf.PI / 180)));
			float translateY = (float)(upwardVelocity * Time.deltaTime);
			float translateZ = (float)(forwardVelocity * Time.deltaTime * Mathf.Cos ((float)(locomotionDirection * Mathf.PI / 180)));		
			Vector3 pos;
			if(transform.position.y + translateY >= groundLevel)
				pos = new Vector3 (transform.position.x + translateX, transform.position.y + translateY, transform.position.z + translateZ);
			else
				pos =  new Vector3 (transform.position.x + translateX, transform.position.y , transform.position.z + translateZ);
			transform.position = pos;

		}

	}
}
