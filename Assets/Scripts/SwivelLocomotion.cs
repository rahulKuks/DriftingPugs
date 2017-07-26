using UnityEngine;
using System.Collections;
using System.IO;
using System;

//public enum PlayerHeight
//{
//	Short,
//	Medium,
//	Tall}
//;

public class SwivelLocomotion : MonoBehaviour
{
	public enum SwivelState
	{
		inForest,
		inSea,
		inSpace
	};



	//public PlayerHeight playerHeight;

	[Header("Global Parameters")]
	[Space(5)]
	//*** public Swivel-360 External Variables
	public GameObject viveCameraEye;
	public GameObject viveRightController;
	public GameObject viveLeftController;
	public float forwardSensitivity = 5f;
	public float sidewaySensitivity = 10f;
	public float upwardSensitivity = 13f;


	[Header("Forest Parameters")]
	[Space(5)]
	[Tooltip("Maximum forwards/backwards speed. Enter 0 to disable movement in this axis or a negative number for no upper limit")]
	[SerializeField] private float maxForestForwardSpeed = 2.25f;
	[Tooltip("Maximum sideways/strafing speed. Enter 0 to disable movement in this axis or a negative number for no upper limit")]
	[SerializeField] private float maxForestSidewaySpeed = 0f;
	[Tooltip("Maximum upward/downward speed. Enter 0 to disable movement in this axis or a negative number for no upper limit")]
	[SerializeField] private float maxForestUpwardsSpeed = 0f;


	[Header("Sea Parameters")]
	[Space(5)]
	[Tooltip("Maximum forwards/backwards speed. Enter 0 to disable movement in this axis or a negative number for no upper limit")]
	[SerializeField] private float maxSeaForwardSpeed = 1.25f;
	[Tooltip("Maximum sideways/strafing speed. Enter 0 to disable movement in this axis or a negative number for no upper limit")]
	[SerializeField] private float maxSeaSidewaySpeed = 0f;
	[Tooltip("Maximum upward/downward speed. Enter 0 to disable movement in this axis or a negative number for no upper limit")]
	[SerializeField] private float maxSeaUpwardsSpeed = 0f;
	[Tooltip("The force applied in the sea to enforce constraints")]
	[SerializeField] private float seaConstraintForce = 3f;
	[Tooltip("the range of freedom if movement has been constrained")]
	[SerializeField] private float seaConstraintRange = 2f;
	[Tooltip("The range from the distance where no force is applied")]
	[SerializeField] private float seaConstraintDeadZoneThreshold = 0.2f;

	[Header("Space Parameters")]
	[Space(5)]
	[Tooltip("Maximum forwards/backwards speed. Enter 0 to disable movement in this axis or a negative number for no upper limit")]
	[SerializeField] private float maxSpaceForwardSpeed = 2.25f;
	[Tooltip("Maximum sideways/strafing speed. Enter 0 to disable movement in this axis or a negative number for no upper limit")]
	[SerializeField] private float maxSpaceSidewaySpeed = 0f;
	[Tooltip("Maximum upward/downward speed. Enter 0 to disable movement in this axis or a negative number for no upper limit")]
	[SerializeField] private float maxSpaceUpwardsSpeed = 2.25f;
	[Tooltip("The force applied in the sea to enforce constraints")]
	[SerializeField] private float spaceConstraintForce = 3f;
	[Tooltip("the range of freedom if movement has been constrained")]
	[SerializeField] private float spaceConstraintRange = 2f;
	[Tooltip("The range from the distance where no force is applied")]
	[SerializeField] private float spaceConstraintDeadZoneThreshold = 0.2f;

	[Header("Debug Parameters")]
	[Space(5)]
	[SerializeField] private float forceMagnitude;
	[SerializeField] private float vectorToOriginMagnitude;
	[SerializeField] private float constraintForceFactor;
	[SerializeField] private GameObject debugOriginSeaCube;
	[SerializeField] private GameObject debugOriginSpaceCube;

	//[Tooltip("Forest floor bottom (should be just above the terrain)")]
	//[SerializeField] private float forestFloorBottom;
	// *** SteamVR controller devices and tracked objects ***
	private SteamVR_TrackedObject leftTrackedObj;
	private SteamVR_Controller.Device leftControllerDevice;


	//Constrain flags and variables;
	SwivelState currentState = SwivelState.inForest;
	private bool locomotionDisabled;
	private Vector3 constraintOrigin;
	Rigidbody rb;



	// *** Swivel-360 internal SerializePrivateVariables *** (Just copy them in your project with no change, because Swivel-360 methods communicating each other through these variables)
	StreamReader sr;
	bool instantHandBrake = true;
	float handBrakeAcceleration = 1f;
	bool handBrake = false, touchPadPressingStatus = false;
	float handBrakeForwardSpeed = 1f, handBrakeSidewaySpeed = 1f, handBrakeUpwardSpeed = 1f;
	float ChairLocomotionDirection = 0f;
	double ViveControllerX = 0, ViveControllerY = 0, ViveControllerZ = 0, ViveControllerYaw = 0, ViveControllerPitch = 0, ViveControllerRoll = 0;
	bool ViveControllerIsAvailable = false, InterfaceIsReady = false;
	double ViveControllerPitchZero = 0, ViveControllerPitchForward = 0, ViveControllerYawOffset = 0;
	float SidewayLeaning = 0f, HeadWidth = .09f, headHeight = 0.07f;

	//Maximum SidewayLeaningDistance = 10 cm (only used for Full Swivel Chair)
	bool headJoystick = true;
	float headP1 = 0, headP2 = 0, headX1 = 0, headX2 = 0, headDeltaP = 0, headChairRadious = 0, headBeta = 0, headXo = 0, headYo = 0, headControllerYaw = 0, headControllerZ1 = 0, headControllerZ2 = 0, headControllerX1 = 0, headControllerX2 = 0, headControllerY1 = 0, headControllerY2 = 0;
	float headXf = 0, headXz = 0, forwardZero = 0, forwardMax = 0, angularDifference = 0, forwardDistanceMax = 0, forwardDistanceZero = 0;
	double ChairPx1 = 0, ChairPx2 = 0, ChairPy1 = 0, ChairPy2 = 0, ChairPz1 = 0, ChairPz2 = 0, ChairRx1 = 0, ChairRx2 = 0, ChairRotationRadious = 0;
	float ChairDeltaX = 0.0f, ChairDeltaY = 0.0f, ChairDeltaZ = 0.0f, LastChairDeltaX = 0.0f, LastChairDeltaY = 0.0f, LastChairDeltaZ = 0.0f, ChairMaxDeltaXZ = 0.0f;
	float exponentialTransferFuntionPower = 1.53f;
	int swivel360InitializeStep = 0;
	//0 = before printing PressSpace message, 1 = after PressSpace message waiting for space, 2 = after space press waiting for Right Alt Press
	float viveCameraZeroY = 0;



	// Use this for initialization
	void Start ()
	{
		locomotionDisabled = false;

		try
		{
			rb = GetComponent<Rigidbody>();
		}
		catch (Exception e)
		{
			Debug.LogError("Player Rigidbody not found!");
		}

		loadChairProfile (); //Load Chair Profile saved into a file by the chair calibration program
	}



	// Update is called once per frame
	void Update ()
	{
		// If controller hasn't been initialised yet, try to initialise it
		if (leftControllerDevice == null) 
		{
			try
			{
				leftTrackedObj = viveLeftController.GetComponent<SteamVR_TrackedObject>();
				leftControllerDevice = SteamVR_Controller.Input((int) leftTrackedObj.index);

			}
			catch (Exception e) 
			{
				Debug.LogWarning ("Exception - Controllers not yet initialised for swivel locomotion");
			}
		}

		//This method applies the Vive Controller data to the user position in Virtual Environment
		//Each argument define one velocity limit either forward, sideway, or upward velocity limit
		//For each argument:
		//			- positive number limits the speed. For example if you put the first argument equals to 1, maximum forward speed will be 1 meter/second
		//			- negative value disables the speed limit. For example -1 means no speed limit
		//			- 0 disable that movement. For example, if the third argument will be 0, flying will be disabled, and user can't go up/down
		if(!locomotionDisabled)
		{
			switch (currentState) 
			{
				case SwivelState.inForest:
					swivelChairLocomotion (maxForestForwardSpeed, maxForestSidewaySpeed, maxForestUpwardsSpeed);
					break;

				case SwivelState.inSea:
					swivelChairLocomotion (maxSeaForwardSpeed, maxSeaSidewaySpeed, maxSeaUpwardsSpeed);
					break;

				case SwivelState.inSpace:
					swivelChairLocomotion (maxSpaceForwardSpeed, maxSpaceSidewaySpeed, maxSpaceUpwardsSpeed);
					break;
					
			}
			
		}
		 
	}

	// *** Call this method in start() *** (loads the chair profile stored in a file by chair calibration project)
	void loadChairProfile ()
	{
		sr = new StreamReader ("ChairCalibrationData.txt");
		while (!sr.EndOfStream) {
			string FileDescription = sr.ReadLine ();			
			string ChairRadiousDescription = sr.ReadLine ();
			string ChairRadious = sr.ReadLine ();
			headChairRadious = float.Parse (ChairRadious);
			string AngularDifferenceDescription = sr.ReadLine ();
			string AngularDifference = sr.ReadLine ();
			angularDifference = float.Parse (AngularDifference);
		}
		Debug.Log ("Chair profile loaded: Rotation Radious = " + headChairRadious + " - Angular Difference = " + angularDifference);
	}

	// *** Call this method in FixedUpdate() *** (updates the position of Vivev HMD and Controller at each frame inside internal variables)
	void readControllerData ()
	{

		//Read all the Vive Controller data in each frame: GameObject.Find ("Controller (right)")
		ViveControllerX = viveRightController.transform.position.x;
		ViveControllerY = viveRightController.transform.position.y;
		ViveControllerZ = viveRightController.transform.position.z;
		ViveControllerPitch = viveRightController.transform.rotation.eulerAngles.x;
		ViveControllerYaw = viveRightController.transform.rotation.eulerAngles.y;
		ViveControllerRoll = viveRightController.transform.rotation.eulerAngles.z;
		//Check If the user pressed touchpad, change handBrake status
		if (viveLeftController.GetComponent<SteamVR_TrackedController> ().padPressed && !touchPadPressingStatus) {
			Debug.Log ("Left Controller pad is pressed!");
			touchPadPressingStatus = true;
			if (!handBrake) {
				if (!instantHandBrake || (handBrakeForwardSpeed == 1 && handBrakeSidewaySpeed == 1)) {
					handBrake = true;
				}
			} else if (!instantHandBrake || (handBrakeForwardSpeed == 0 && handBrakeSidewaySpeed == 0)) {
				handBrake = false;
			}
		}
		if (touchPadPressingStatus && !viveLeftController.GetComponent<SteamVR_TrackedController> ().padPressed) {
			touchPadPressingStatus = false;
		}
		//Calculate the chair displacement (ChairDeltaX,ChairDeltaY,ChairDeltaZ) to not change the user's position in VR if the stationary chair is active
		//If stationary chair is active, (ChairDeltaX,ChairDeltaY,ChairDeltaZ) will be applied to the chair position in the SwivelChairLocomotion()
		if (InterfaceIsReady) { // This part calculates the horizontal/verttical displacement of the chair back in case we want to ignore them in updating position 
			double dRx = Mathf.Abs ((float)(ViveControllerPitch - ChairRx2));
			float Distance = (float)(2 * ChairRotationRadious * Mathf.Sin ((float)(dRx / 2 * Mathf.PI / 180)));
			ChairDeltaY = (float)(ChairPy2 - ViveControllerY);
			double dXZ = Mathf.Sqrt (Mathf.Abs ((float)(Mathf.Pow (Distance, 2) - Mathf.Pow ((float)(ChairDeltaY), 2))));
			double ChairRy = ViveControllerYaw + ViveControllerYawOffset;
			ChairDeltaZ = -(float)(dXZ * Mathf.Cos ((float)(ChairRy * Mathf.PI / 180)));
			ChairDeltaX = -(float)(dXZ * Mathf.Sin ((float)(ChairRy * Mathf.PI / 180)));
		}

		if (swivel360InitializeStep == 0) {
			Debug.Log ("Ask the user to sit comfortable and look forward and then press Right ALT (prefably twice)");
			swivel360InitializeStep = 2;
		}

		//if (Input.GetKeyDown ("space")) {
		if(leftControllerDevice.GetPress(SteamVR_Controller.ButtonMask.Trigger))
		{
			ViveControllerPitchZero = ViveControllerPitch;
			InterfaceIsReady = true;
			//Read the Vive Controller data
			ChairPx2 = viveRightController.transform.position.x;
			ChairPy2 = viveRightController.transform.position.y;
			ChairPz2 = viveRightController.transform.position.z;
			ChairRx2 = viveRightController.transform.rotation.eulerAngles.x;
			viveCameraZeroY = viveCameraEye.transform.localPosition.y;
			viveCameraZeroY += headHeight * Mathf.Sin (viveCameraEye.transform.rotation.eulerAngles.x * Mathf.PI / 180); //Calculate the Neck y Position
			// Calculate the ChairRotationRadious to measure the chair displacement
			//double Distance = Mathf.Sqrt (Mathf.Pow ((float)(ChairPx1 - ChairPx2), 2) + Mathf.Pow ((float)(ChairPy1 - ChairPy2), 2) + Mathf.Pow ((float)(ChairPz1 - ChairPz2), 2));
			//double dRx = Mathf.Abs ((float)(ChairRx1 - ChairRx2));
			//ChairMaxDeltaXZ = Mathf.Sqrt (Mathf.Pow ((float)(ChairPx1 - ChairPx2), 2) + Mathf.Pow ((float)(ChairPz1 - ChairPz2), 2));
			//ChairRotationRadious = Distance / (2 * Mathf.Sin ((float)(dRx / 2 * Mathf.PI / 180)));
			//print ("Vive Controller Zero Pitch = " + ViveControllerPitchZero + " - Chair Rotation Radious = " + ChairRotationRadious);
			if (headJoystick) {
				headP2 = -viveRightController.transform.localRotation.eulerAngles.x;
				headControllerX2 = viveRightController.transform.localPosition.x;
				headControllerY2 = viveRightController.transform.localPosition.y;
				headControllerZ2 = viveRightController.transform.localPosition.z;
				headControllerYaw = 180 + viveRightController.transform.localRotation.eulerAngles.y;
				if (headControllerYaw > 360)
					headControllerYaw -= 360;

				headX2 = headControllerZ2 * Mathf.Cos (headControllerYaw * Mathf.PI / 180) + headControllerX2 * Mathf.Sin (headControllerYaw * Mathf.PI / 180);

				float headAlpha = (headP2 - angularDifference) * Mathf.PI / 180;// = Mathf.PI / 2.0f - (headDeltaP / 2.0f) - headBeta; // (radians)
				headXo = headX2 + headChairRadious * Mathf.Cos (headAlpha);
				headYo = headControllerY2 - headChairRadious * Mathf.Sin (headAlpha);
				headXz = viveCameraEye.transform.localPosition.z * Mathf.Cos (headControllerYaw * Mathf.PI / 180) + viveCameraEye.transform.localPosition.x * Mathf.Sin (headControllerYaw * Mathf.PI / 180);
				headXz += headHeight * (1 - Mathf.Abs (Mathf.Cos (viveCameraEye.transform.rotation.eulerAngles.x * Mathf.PI / 180))); //Calculate the Neck y Position
				forwardDistanceMax = headXo - headXf;
				forwardDistanceZero = headXo - headXz;
			}

			Debug.Log ("Great! Now Swivel-360 is working and the user can sit straight to move");
			swivel360InitializeStep = 3;
		}
	}


	// *** Call this method in update() *** (Uses the Vive HMD & Controller data stored in internal variables to move the player in Virtual Environment)
	void swivelChairLocomotion (float forwardVelocityLimit, float sidewayVelocityLimit, float upwardVelocityLimit, bool activateExponentialTransferFunction = true)
	{

		//*************************************************************************************************************
		//*************************************************************************************************************
		//***																										***
		//***						Calculating distance of the head from Zero Point								***
		//***																										***
		//*************************************************************************************************************
		//*************************************************************************************************************

		float forwardInputRate, SidewayInputRate = 0f, upwardInputRate = 0f;


		float ChairDirectionYaw = 180 + viveRightController.transform.localRotation.eulerAngles.y;
		if (ChairDirectionYaw > 360)
			ChairDirectionYaw -= 360;
		float headXnow = viveCameraEye.transform.localPosition.z * Mathf.Cos (ChairDirectionYaw * Mathf.PI / 180) + viveCameraEye.transform.localPosition.x * Mathf.Sin (ChairDirectionYaw * Mathf.PI / 180);
		headXnow += headHeight * (1 - Mathf.Abs (Mathf.Cos (viveCameraEye.transform.rotation.eulerAngles.x * Mathf.PI / 180))); //Calculate the Neck y Position
		//print ("Head Position X = " + headXnow + "- Yaw = " + ChairDirectionYaw + " - Local Z = " + viveCameraEye.GetComponent<Transform> ().localPosition.z + " - Local X = " + viveCameraEye.GetComponent<Transform> ().localPosition.x);


		float headP3 = -viveRightController.transform.localRotation.eulerAngles.x;
		float headControllerX3 = viveRightController.transform.localPosition.x;
		float headControllerY3 = viveRightController.transform.localPosition.y;
		float headControllerZ3 = viveRightController.transform.localPosition.z;


		float headX3 = headControllerZ3 * Mathf.Cos (ChairDirectionYaw * Mathf.PI / 180) + headControllerX3 * Mathf.Sin (ChairDirectionYaw * Mathf.PI / 180);
		float chairAlpha = (headP3 - angularDifference) * Mathf.PI / 180;
		headXo = headX3 + headChairRadious * Mathf.Cos (chairAlpha);
		headYo = headControllerY3 - headChairRadious * Mathf.Sin (chairAlpha);

		float forwardDistance = headXo - headXnow;
		forwardInputRate = (forwardDistance - forwardDistanceZero) * forwardSensitivity;
		//Debug.Log ("Forward Distance= " + forwardDistance + " - ZeroDistance = " + forwardDistance + " - InputRate = " + forwardInputRate);


		//forwardInputRate = Mathf.Max (0f, forwardInputRate); //No backward Locomotion

		//Calculating sideway leaning for Full Swivel Chair
		float ChairYaw = 360 - viveRightController.transform.rotation.eulerAngles.y;
		float HeadYaw = 360 - viveCameraEye.transform.rotation.eulerAngles.y;
		float HeadX = -viveCameraEye.transform.position.z + HeadWidth * Mathf.Cos (HeadYaw * Mathf.PI / 180); //Calculate the Neck x Position
		float HeadY = viveCameraEye.transform.position.x + HeadWidth * Mathf.Sin (HeadYaw * Mathf.PI / 180); //Calculate the Neck y Position
		float ChairX = -viveRightController.transform.position.z;
		float ChairY = viveRightController.transform.position.x;
		float LeaningDistanceSideway = Mathf.Sin (ChairYaw * Mathf.PI / 180) * (HeadX - ChairX) - Mathf.Cos (ChairYaw * Mathf.PI / 180) * (HeadY - ChairY) - .005f;

		SidewayInputRate = (float)(-LeaningDistanceSideway * sidewaySensitivity);

		//Debug.Log ("Sideway Leaning Rate = " + SidewayInputRate);



		float currentY = viveCameraEye.transform.localPosition.y;
		currentY += headHeight * Mathf.Sin (viveCameraEye.transform.rotation.eulerAngles.x * Mathf.PI / 180); //Calculate the Neck y Position
		float leaningUpwardDistance = currentY - viveCameraZeroY;
		upwardInputRate = (float)(leaningUpwardDistance * upwardSensitivity);

		float forwardLocomotionSpeed = forwardSensitivity * forwardInputRate;
		float sidewayLocomotionSpeed = sidewaySensitivity * SidewayInputRate;
		float upwardLocomotionSpeed = upwardSensitivity * upwardInputRate;

		//*************************************************************************************************************
		//*************************************************************************************************************
		//***																										***
		//***									Applying Transfer Function											***
		//***																										***
		//*************************************************************************************************************
		//*************************************************************************************************************

		if (activateExponentialTransferFunction) {	//Apply Exponential transfer function
			//Apply Exponential transfer function on forward/backward locomotion
			if (forwardInputRate > 0)
				forwardLocomotionSpeed = forwardSensitivity * Mathf.Pow ((float)(forwardInputRate), exponentialTransferFuntionPower);
			else
				forwardLocomotionSpeed = forwardSensitivity * -Mathf.Pow ((float)(-forwardInputRate), exponentialTransferFuntionPower);
			//Apply Exponential transfer function on sideways locomotion
			if (SidewayInputRate > 0)
				sidewayLocomotionSpeed = sidewaySensitivity * Mathf.Pow ((float)(SidewayInputRate), exponentialTransferFuntionPower);
			else
				sidewayLocomotionSpeed = sidewaySensitivity * -Mathf.Pow ((float)(-SidewayInputRate), exponentialTransferFuntionPower);
			//Apply Exponential transfer function on up/down locomotion
			if (upwardInputRate > 0)
				upwardLocomotionSpeed = upwardSensitivity * Mathf.Pow ((float)(upwardInputRate), exponentialTransferFuntionPower);
			else
				upwardLocomotionSpeed = upwardSensitivity * -Mathf.Pow ((float)(-upwardInputRate), exponentialTransferFuntionPower);
		} else {	//Linear Transfer Function with 10% dead-zone
			//Apply linear transfer function on forward/backward locomotion
			if (Mathf.Abs (forwardInputRate) < 0.1f)
				forwardLocomotionSpeed = 0;
			else if (forwardInputRate > 0)
				forwardLocomotionSpeed = forwardSensitivity * (forwardInputRate - .1f);
			else
				forwardLocomotionSpeed = -forwardSensitivity * ((-forwardInputRate) - .1f);
			//Apply linear transfer function on sideways locomotion
			if (Mathf.Abs (SidewayInputRate) < 0.1f)
				sidewayLocomotionSpeed = 0;
			else if (SidewayInputRate > 0)
				sidewayLocomotionSpeed = sidewaySensitivity * (SidewayInputRate - .1f);
			else
				sidewayLocomotionSpeed = -sidewaySensitivity * ((-SidewayInputRate) - .1f);
			//Apply linear transfer function on up/down locomotion
			if (Mathf.Abs (upwardInputRate) < 0.1f)
				upwardLocomotionSpeed = 0;
			else if (upwardInputRate > 0)
				upwardLocomotionSpeed = upwardSensitivity * (upwardInputRate - .1f);
			else
				upwardLocomotionSpeed = -upwardSensitivity * ((-upwardInputRate) - .1f);
		}


		//*************************************************************************************************************
		//*************************************************************************************************************
		//***																										***
		//***								    	Applying Speed Limit											***
		//***																										***
		//*************************************************************************************************************
		//*************************************************************************************************************

		if (forwardVelocityLimit >= 0) {
			if (forwardLocomotionSpeed > forwardVelocityLimit)
				forwardLocomotionSpeed = forwardVelocityLimit;
			else if (forwardLocomotionSpeed < -forwardVelocityLimit)
				forwardLocomotionSpeed = -forwardVelocityLimit;
		}

		if (sidewayVelocityLimit >= 0) {
			if (sidewayLocomotionSpeed > sidewayVelocityLimit)
				sidewayLocomotionSpeed = sidewayVelocityLimit;
			else if (sidewayLocomotionSpeed < -sidewayVelocityLimit)
				sidewayLocomotionSpeed = -sidewayVelocityLimit;
		}

		if (upwardVelocityLimit >= 0) {
			if (upwardLocomotionSpeed > upwardVelocityLimit)
				upwardLocomotionSpeed = upwardVelocityLimit;
			else if (upwardLocomotionSpeed < -upwardVelocityLimit)
				upwardLocomotionSpeed = -upwardVelocityLimit;
		}

		//*************************************************************************************************************
		//*************************************************************************************************************
		//***																										***
		//***											Applying Brake												***
		//***																										***
		//*************************************************************************************************************
		//*************************************************************************************************************

		if (handBrake) {
			if (handBrakeForwardSpeed > 0) {
				handBrakeForwardSpeed -= handBrakeAcceleration * Time.deltaTime;
				if (handBrakeForwardSpeed < 0)
					handBrakeForwardSpeed = 0;					
				handBrakeSidewaySpeed -= handBrakeAcceleration * Time.deltaTime;
				if (handBrakeSidewaySpeed < 0)
					handBrakeSidewaySpeed = 0;
			}
		} else {
			if (handBrakeForwardSpeed < 1) {
				handBrakeForwardSpeed += handBrakeAcceleration * Time.deltaTime;
				if (handBrakeForwardSpeed > 1)
					handBrakeForwardSpeed = 1;
				handBrakeSidewaySpeed += handBrakeAcceleration * Time.deltaTime;
				if (handBrakeSidewaySpeed > 1)
					handBrakeSidewaySpeed = 1;
			}
		}

		forwardLocomotionSpeed *= handBrakeForwardSpeed;
		sidewayLocomotionSpeed *= handBrakeSidewaySpeed;
		upwardLocomotionSpeed *= handBrakeUpwardSpeed;

		//  ****************************** Calculate the Rotatoion locomotion *****************************************
		ChairLocomotionDirection = (float)(ViveControllerYaw + ViveControllerYawOffset);

		float TranslateX = (float)(forwardLocomotionSpeed * Mathf.Sin ((float)(ChairLocomotionDirection * Mathf.PI / 180)) * Time.deltaTime);
		float TranslateY = (float)(upwardLocomotionSpeed * Time.deltaTime);
		if (transform.position.y + TranslateY < 0) {
			TranslateY = 0;
		}
		float TranslateZ = (float)(forwardLocomotionSpeed * Mathf.Cos ((float)(ChairLocomotionDirection * Mathf.PI / 180)) * Time.deltaTime);
		if (sidewayVelocityLimit >= 0) {
			TranslateX += (float)(sidewayLocomotionSpeed * Mathf.Cos ((float)(ChairLocomotionDirection * Mathf.PI / 180)) * Time.deltaTime);
			TranslateZ -= (float)(sidewayLocomotionSpeed * Mathf.Sin ((float)(ChairLocomotionDirection * Mathf.PI / 180)) * Time.deltaTime);
		}

		if (forwardVelocityLimit == 0 && sidewayVelocityLimit == 0) {
			TranslateX = 0;
			TranslateZ = 0;
		}

		Vector3 pos = new Vector3 (TranslateX, TranslateY, TranslateZ);

		if (InterfaceIsReady) 
		{
			transform.Translate (pos); 

			/* if in forest, then clamp the Y position to always be above forestFloorBottom
			if (transform.position.y < forestFloorBottom && currentState == SwivelState.inForest) 
			{
				transform.position = new Vector3 (transform.position.x, forestFloorBottom, transform.position.z);
			}*/	
		}

	}

	void FixedUpdate ()
	{
		if(leftControllerDevice != null) 	// ensure device initialisation before reading data
		{
			readControllerData (); //Read Vive Controller data and store them inside internal variables

			if (InterfaceIsReady) 
			{
				switch (currentState) 
				{
					case SwivelState.inSea:
						Debug.Log("Constraining in sea"); 
						ConstrainXZ ();
						break;

					case SwivelState.inSpace:
						Debug.Log("Constraining in space"); 
						ConstrainAll();
						break;
				}
			}


		}
	}

	/// <summary>
	/// Adds a force onto the user that pushes it back to its origin point. The force is smaller than the forward speed until the user hits max speed, when the force is greater. This is done on all three axes.
	/// </summary>
	private void ConstrainAll()
	{
		
		Vector3 vectorToOrigin = constraintOrigin - transform.localPosition;
		Vector3 forceDirection = vectorToOrigin.normalized;

		//update debug serialized parameters
		vectorToOriginMagnitude = vectorToOrigin.magnitude;
		constraintForceFactor = vectorToOrigin.magnitude / spaceConstraintRange;

		//calculate force and apply
		if(constraintForceFactor >= spaceConstraintDeadZoneThreshold)
		{
			// if gone too far, disable locomotion
			if(constraintForceFactor >= 1)
			{
				locomotionDisabled = true;
			}

			forceMagnitude = spaceConstraintForce * (vectorToOrigin.magnitude / spaceConstraintRange);
			rb.AddForce (forceDirection * forceMagnitude);
		}
		else if(locomotionDisabled)	//enable locomotion if player has been reeled in enough
		{
			locomotionDisabled = false;
		}



	}

	/// <summary>
	/// Adds a force onto the user that pushes it back to its origin point on the XZ plane. The force is smaller than the forward speed until the user hits max speed, when the force is greater.
	/// </summary>
	private void ConstrainXZ()
	{
		Vector3 originXZ = new Vector3 (constraintOrigin.x, this.transform.localPosition.y, constraintOrigin.z);
		//debugOriginSeaCube.transform.position = originXZ;
		Vector3 vectorToOrigin = originXZ - this.transform.localPosition;
		Vector3 forceDirection = vectorToOrigin.normalized;

		//update debug serialiszed parameters
		vectorToOriginMagnitude = vectorToOrigin.magnitude;
		constraintForceFactor = vectorToOrigin.magnitude / seaConstraintRange;

		//calculate force and apply
		if(constraintForceFactor >= seaConstraintDeadZoneThreshold)
		{
			// if gone too far, disable locomotion
			if(constraintForceFactor >= 1)
			{
				locomotionDisabled = true;
			}

			forceMagnitude = seaConstraintForce * (vectorToOrigin.magnitude / seaConstraintRange);
			rb.AddForce (forceDirection * forceMagnitude);
		}
		else if(locomotionDisabled)	//enable locomotion if player has been reeled in enough
		{
			locomotionDisabled = false;
		}


	}

	/// <summary>
	/// Sets the swivel state and accordingly flag constraints
	/// </summary>
	/// <param name="state">Swivel State.</param>
	public void SetSwivelState(SwivelState state)
	{
		this.currentState = state;;

		switch (currentState) 
		{
			case SwivelState.inForest:
				break;

			case SwivelState.inSea:
				//debugOriginSeaCube.SetActive(true);
				constraintOrigin = transform.localPosition;
				Debug.Log("Swivel Sea state, Origin: " + constraintOrigin);
				break;

			case SwivelState.inSpace:
				constraintOrigin = transform.localPosition;
				//debugOriginSeaCube.transform.localPosition = constraintOrigin;
				Debug.Log("Swivel space state, Origin: " + constraintOrigin);
				break;
		}
	}

	public SwivelState GetSwivelState()
	{
		return this.currentState;
	}

}