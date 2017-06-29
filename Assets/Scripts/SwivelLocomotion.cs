using UnityEngine;
using System.Collections;

public class SwivelLocomotion : MonoBehaviour
{


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
		//SwivelChairLocomotion (3, 3, true); //Apply the Vive Controller data to the user position in Virtual Environment
		SwivelChairLocomotion(3,0,false);
	}

	//*** public Swivel-360 External Variables
	public GameObject viveCameraEye;
	public GameObject viveRightController;
	public GameObject viveLeftController;
	public float handBrakeAcceleration = 1f;
	public bool instantHandBrake = true;

	// *** Swivel-360 internal SerializePrivateVariables *** (Just copy them in your project with no change, because Swivel-360 methods communicating each other through these variables)
	bool handBrake = false, touchPadPressingStatus = false;
	float handBrakeForwardSpeed = 1f, handBrakeSidewaySpeed = 1f;
	float ChairLocomotionDirection = 0f;
	double ViveControllerX = 0, ViveControllerY = 0, ViveControllerZ = 0, ViveControllerYaw = 0, ViveControllerPitch = 0, ViveControllerRoll = 0;
	bool ViveControllerIsAvailable = false, InterfaceIsReady = false;
	double ViveControllerPitchZero = 0, ViveControllerPitchForward = 0, ViveControllerYawOffset = 0;
	float SidewayLeaning = 0f, HeadWidth = .09f;
	//Needed for calculation of Sideway leaning for Full Swivel Chair
	float MaximumSidewayLeaningDistance = .12f, MaximumForwardLeaningDistance = .30f;
	//Maximum SidewayLeaningDistance = 10 cm (only used for Full Swivel Chair)
	public bool headJoystick = true;
	float headP1 = 0, headP2 = 0, headX1 = 0, headX2 = 0, headDeltaP = 0, headChairRadious = 0, headBeta = 0, headXo = 0, headYo = 0, headControllerYaw = 0, headControllerZ1 = 0, headControllerZ2 = 0, headControllerX1 = 0, headControllerX2 = 0, headControllerY1 = 0, headControllerY2 = 0;
	float headXf = 0, headXz = 0, forwardZero = 0, forwardMax = 0, angularDifference = 0, forwardDistanceMax = 0, forwardDistanceZero = 0;
	double ChairPx1 = 0, ChairPx2 = 0, ChairPy1 = 0, ChairPy2 = 0, ChairPz1 = 0, ChairPz2 = 0, ChairRx1 = 0, ChairRx2 = 0, ChairRotationRadious = 0;
	float ChairDeltaX = 0.0f, ChairDeltaY = 0.0f, ChairDeltaZ = 0.0f, LastChairDeltaX = 0.0f, LastChairDeltaY = 0.0f, LastChairDeltaZ = 0.0f, ChairMaxDeltaXZ = 0.0f;
	float exponentialTransferFuntionPower = 1.53f;
	int swivel360InitializeStep = 0; //0 = before printing PressSpace message, 1 = after PressSpace message waiting for space, 2 = after space press waiting for Right Alt Press



	// *** Call this method in FixedUpdate() *** (updates the position of Vivev HMD and Controller at each frame inside internal variables)
	void ReadControllerData ()
	{

		//Read all the Vive Controller data in each frame: GameObject.Find ("Controller (right)")
		ViveControllerX = viveRightController.GetComponent<Transform> ().position.x;
		ViveControllerY = viveRightController.GetComponent<Transform> ().position.y;
		ViveControllerZ = viveRightController.GetComponent<Transform> ().position.z;
		ViveControllerPitch = viveRightController.GetComponent<Transform> ().rotation.eulerAngles.x;
		ViveControllerYaw = viveRightController.GetComponent<Transform> ().rotation.eulerAngles.y;
		ViveControllerRoll = viveRightController.GetComponent<Transform> ().rotation.eulerAngles.z;
		//Check If the user pressed touchpad, change handBrake status
		if (viveLeftController.GetComponent<SteamVR_TrackedController> ().padPressed && !touchPadPressingStatus) {
			Debug.Log ("Left Controller pad is pressed!");
			touchPadPressingStatus = true;
			if (!handBrake) {
				if (!instantHandBrake || (handBrakeForwardSpeed == 1 && handBrakeSidewaySpeed == 1)) {
					handBrake = true;
					//handBrakeForwardSpeed = 1f;
					//handBrakeSidewaySpeed = 1f;
				}
			} else if(!instantHandBrake || (handBrakeForwardSpeed == 0 && handBrakeSidewaySpeed == 0)) {
				handBrake = false;
				//handBrakeForwardSpeed = 0f;
				//handBrakeSidewaySpeed = 0f;
			}
		}
		if (touchPadPressingStatus && !viveLeftController.GetComponent<SteamVR_TrackedController> ().padPressed) {
			//Debug.Log ("Left Controller pad is released!");
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
			Debug.Log ("Ask the user to sit straight and look forward and then press SPACEBAR (prefably twice)");
			swivel360InitializeStep = 1;
		}

		if (Input.GetKeyDown ("space")) {

			float yawZero = viveCameraEye.transform.rotation.eulerAngles.y;

			ViveControllerPitchForward = ViveControllerPitch;
			ViveControllerYawOffset = yawZero - ViveControllerYaw;
			//print ("Vive Controller Pitch = " + ViveControllerPitchForward + " - Vive Controller Yaw Offset = " + ViveControllerYawOffset);
			while (ViveControllerYawOffset > 180)
				ViveControllerYawOffset -= 360;
			while (ViveControllerYawOffset < -180)
				ViveControllerYawOffset += 360;
			//Read the Vive Controller data to calculate the ChairRotationRadious to measure the chair displacement
			ChairPx1 = viveRightController.GetComponent<Transform> ().position.x;
			ChairPy1 = viveRightController.GetComponent<Transform> ().position.y;
			ChairPz1 = viveRightController.GetComponent<Transform> ().position.z;
			ChairRx1 = viveRightController.GetComponent<Transform> ().rotation.eulerAngles.x;

			if (headJoystick) {
				headP1 = -viveRightController.GetComponent<Transform> ().localRotation.eulerAngles.x;
				headControllerX1 = viveRightController.GetComponent<Transform> ().localPosition.x;
				headControllerY1 = viveRightController.GetComponent<Transform> ().localPosition.y;
				headControllerZ1 = viveRightController.GetComponent<Transform> ().localPosition.z;
				headControllerYaw = 180 + viveRightController.GetComponent<Transform> ().localRotation.eulerAngles.y;
				if (headControllerYaw > 360)
					headControllerYaw -= 360;
				float currentChairYaw = headControllerYaw * Mathf.PI / 180;
				float currentHeadZ = viveCameraEye.GetComponent<Transform> ().localPosition.z;
				float currentHeadX = viveCameraEye.GetComponent<Transform> ().localPosition.x;
				headXf = currentHeadZ * Mathf.Cos (currentChairYaw) + currentHeadX * Mathf.Sin (currentChairYaw);
				//print ("Chair Yaw = " + currentChairYaw * 180 / Mathf.PI + " - Head Z = " + currentHeadZ + " - Head X = " + currentHeadX + " - Head Pitch = " + headP1 + " - Head Forward Position = " + headXf);
			}

			Debug.Log ("Now ask the user to lean back and then press Right-ALT on keyboard (prefably twice)");
			swivel360InitializeStep = 2;

		}


		if (Input.GetKeyDown (KeyCode.RightAlt)) {

			ViveControllerPitchZero = ViveControllerPitch;
			InterfaceIsReady = true;
			//Read the Vive Controller data
			ChairPx2 = viveRightController.GetComponent<Transform> ().position.x;
			ChairPy2 = viveRightController.GetComponent<Transform> ().position.y;
			ChairPz2 = viveRightController.GetComponent<Transform> ().position.z;
			ChairRx2 = viveRightController.GetComponent<Transform> ().rotation.eulerAngles.x;
			// Calculate the ChairRotationRadious to measure the chair displacement
			double Distance = Mathf.Sqrt (Mathf.Pow ((float)(ChairPx1 - ChairPx2), 2) + Mathf.Pow ((float)(ChairPy1 - ChairPy2), 2) + Mathf.Pow ((float)(ChairPz1 - ChairPz2), 2));
			double dRx = Mathf.Abs ((float)(ChairRx1 - ChairRx2));
			ChairMaxDeltaXZ = Mathf.Sqrt (Mathf.Pow ((float)(ChairPx1 - ChairPx2), 2) + Mathf.Pow ((float)(ChairPz1 - ChairPz2), 2));
			ChairRotationRadious = Distance / (2 * Mathf.Sin ((float)(dRx / 2 * Mathf.PI / 180)));
			//print ("Vive Controller Zero Pitch = " + ViveControllerPitchZero + " - Chair Rotation Radious = " + ChairRotationRadious);
			if (headJoystick) {
				headP2 = -viveRightController.GetComponent<Transform> ().localRotation.eulerAngles.x;
				headControllerX2 = viveRightController.GetComponent<Transform> ().localPosition.x;
				headControllerY2 = viveRightController.GetComponent<Transform> ().localPosition.y;
				headControllerZ2 = viveRightController.GetComponent<Transform> ().localPosition.z;
				headControllerYaw = 180 + viveRightController.GetComponent<Transform> ().localRotation.eulerAngles.y;
				if (headControllerYaw > 360)
					headControllerYaw -= 360;
				headX1 = headControllerZ1 * Mathf.Cos (headControllerYaw * Mathf.PI / 180) + headControllerX1 * Mathf.Sin (headControllerYaw * Mathf.PI / 180);
				headX2 = headControllerZ2 * Mathf.Cos (headControllerYaw * Mathf.PI / 180) + headControllerX2 * Mathf.Sin (headControllerYaw * Mathf.PI / 180);
				headDeltaP = (headP1 - headP2) * Mathf.PI / 180; // (radians)
				float headD = Mathf.Sqrt (Mathf.Pow (headControllerY2 - headControllerY1, 2) + Mathf.Pow (headX2 - headX1, 2));
				headChairRadious = headD / (2.0f * Mathf.Sin (headDeltaP / 2.0f));
				headBeta = Mathf.Atan2 (headControllerY1 - headControllerY2, headX1 - headX2); // (radians)
				float headAlpha = Mathf.PI / 2.0f - (headDeltaP / 2.0f) - headBeta; // (radians)
				headXo = headX2 + headChairRadious * Mathf.Cos (headAlpha);
				headYo = headControllerY2 - headChairRadious * Mathf.Sin (headAlpha);
				headXz = viveCameraEye.GetComponent<Transform> ().localPosition.z * Mathf.Cos (headControllerYaw * Mathf.PI / 180) + viveCameraEye.GetComponent<Transform> ().localPosition.x * Mathf.Sin (headControllerYaw * Mathf.PI / 180);
				angularDifference = headP2 - (headAlpha * 180 / Mathf.PI);
				forwardDistanceMax = headXo - headXf;
				forwardDistanceZero = headXo - headXz;
				//print("Head Pitch = " + headP2 + " - alpha = " + headAlpha1 * 180 / Mathf.PI + " - Angular Difference = " + angularDifference);
				//print ("Center of the chair: X = " + headXo + " - Y = " + headYo + " - Head Distance = " + headD + " - Chair Rotation Radious = " + headChairRadious);
			}

			Debug.Log ("Great! Now Swivel-360 is working and the user can sit straight to move");
			swivel360InitializeStep = 3;
		}
	}


	// *** Call this method in update() *** (Uses the Vive HMD & Controller data stored in internal variables to move the player in Virtual Environment)
	void SwivelChairLocomotion (float maximumForwardVelocity, float maximumSidewayVelocity, bool allowHigherThanMaximumSpeed, bool activateExponentialTransferFunction = true)
	{

		// ***************************  Caqlculate the forward locomotion  *******************************************
		float InputRate, SidewayInputRate = 0f;

		if (headJoystick) {
			float ChairDirectionYaw = 180 + viveRightController.GetComponent<Transform> ().localRotation.eulerAngles.y;
			if (ChairDirectionYaw > 360)
				ChairDirectionYaw -= 360;
			float headXnow = viveCameraEye.GetComponent<Transform> ().localPosition.z * Mathf.Cos (ChairDirectionYaw * Mathf.PI / 180) + viveCameraEye.GetComponent<Transform> ().localPosition.x * Mathf.Sin (ChairDirectionYaw * Mathf.PI / 180);
			//print ("Head Position X = " + headXnow + "- Yaw = " + ChairDirectionYaw + " - Local Z = " + viveCameraEye.GetComponent<Transform> ().localPosition.z + " - Local X = " + viveCameraEye.GetComponent<Transform> ().localPosition.x);


			float headP3 = -viveRightController.GetComponent<Transform> ().localRotation.eulerAngles.x;
			float headControllerX3 = viveRightController.GetComponent<Transform> ().localPosition.x;
			float headControllerY3 = viveRightController.GetComponent<Transform> ().localPosition.y;
			float headControllerZ3 = viveRightController.GetComponent<Transform> ().localPosition.z;


			float headX3 = headControllerZ3 * Mathf.Cos (ChairDirectionYaw * Mathf.PI / 180) + headControllerX3 * Mathf.Sin (ChairDirectionYaw * Mathf.PI / 180);
			float chairAlpha = (headP3 - angularDifference) * Mathf.PI / 180;
			headXo = headX3 + headChairRadious * Mathf.Cos (chairAlpha);
			headYo = headControllerY3 - headChairRadious * Mathf.Sin (chairAlpha);

			float forwardDistance = headXo - headXnow;
			InputRate = (forwardDistance - forwardDistanceZero) / (forwardDistanceMax - forwardDistanceZero);

		} else
			InputRate = (float)((ViveControllerPitch - ViveControllerPitchZero) / (ViveControllerPitchForward - ViveControllerPitchZero));

		if (!allowHigherThanMaximumSpeed)
		if (InputRate > 1)
			InputRate = 1;
		else if (InputRate < -1)
			InputRate = -1;
		//InputRate = Mathf.Max (0f, InputRate); //No backward Locomotion

		if (maximumSidewayVelocity != 0) { //Calculating sideway leaning for Full Swivel Chair
			float ChairYaw = 360 - viveRightController.transform.rotation.eulerAngles.y;
			float HeadYaw = 360 - viveCameraEye.transform.rotation.eulerAngles.y;
			float HeadX = -viveCameraEye.transform.position.z + HeadWidth * Mathf.Cos (HeadYaw * Mathf.PI / 180); //Calculate the Neck x Position
			float HeadY = viveCameraEye.transform.position.x + HeadWidth * Mathf.Sin (HeadYaw * Mathf.PI / 180); //Calculate the Neck y Position
			float ChairX = -viveRightController.transform.position.z;
			float ChairY = viveRightController.transform.position.x;
			float LeaningDistanceSideway = Mathf.Sin (ChairYaw * Mathf.PI / 180) * (HeadX - ChairX) - Mathf.Cos (ChairYaw * Mathf.PI / 180) * (HeadY - ChairY) - .005f;

			SidewayInputRate = (float)(LeaningDistanceSideway / MaximumSidewayLeaningDistance);

			if (!allowHigherThanMaximumSpeed)
			if (SidewayInputRate > 1)
				SidewayInputRate = 1;
			else if (SidewayInputRate < -1)
				SidewayInputRate = -1;
			//Debug.Log ("Sideway Leaning Rate = " + SidewayInputRate);

		}


		float LocomotionSpeed = maximumForwardVelocity * InputRate;
		float SidewayLocomotionSpeed = maximumSidewayVelocity * SidewayInputRate;

		//*************************************************************************************************************
		//*************************************************************************************************************
		//***																										***
		//***									Applying Transfer Function											***
		//***																										***
		//*************************************************************************************************************
		//*************************************************************************************************************

		if (activateExponentialTransferFunction) {	//Apply Exponential transfer function
			//Apply Exponential transfer function on forward/backward locomotion
			if (InputRate > 0)
				LocomotionSpeed = maximumForwardVelocity * Mathf.Pow ((float)(InputRate), exponentialTransferFuntionPower);
			else
				LocomotionSpeed = maximumForwardVelocity * -Mathf.Pow ((float)(-InputRate), exponentialTransferFuntionPower);
			//Apply Exponential transfer function on sideways locomotion
			if (SidewayInputRate > 0)
				SidewayLocomotionSpeed = maximumSidewayVelocity * Mathf.Pow ((float)(SidewayInputRate), exponentialTransferFuntionPower);
			else
				SidewayLocomotionSpeed = maximumSidewayVelocity * -Mathf.Pow ((float)(-SidewayInputRate), exponentialTransferFuntionPower);
		} else {	//Linear Transfer Function with 10% dead-zone
			//Apply linear transfer function on forward/backward locomotion
			if (Mathf.Abs (InputRate) < 0.1f)
				LocomotionSpeed = 0;
			else if (InputRate > 0)
				LocomotionSpeed = maximumForwardVelocity * (InputRate - .1f);
			else
				LocomotionSpeed = -maximumForwardVelocity * ((-InputRate) - .1f);
			//Apply linear transfer function on sideways locomotion
			if (Mathf.Abs (SidewayInputRate) < 0.1f)
				SidewayLocomotionSpeed = 0;
			else if (SidewayInputRate > 0)
				SidewayLocomotionSpeed = maximumSidewayVelocity * (SidewayInputRate - .1f);
			else
				SidewayLocomotionSpeed = -maximumSidewayVelocity * ((-SidewayInputRate) - .1f);
		}


		//*************************************************************************************************************
		//*************************************************************************************************************
		//***																										***
		//***											Applying Brake												***
		//***																										***
		//*************************************************************************************************************
		//*************************************************************************************************************

		if (handBrake) {
			if (instantHandBrake) {
				if (handBrakeForwardSpeed > 0) {
					handBrakeForwardSpeed -= handBrakeAcceleration * Time.deltaTime;
					if (handBrakeForwardSpeed < 0)
						handBrakeForwardSpeed = 0;					
					handBrakeSidewaySpeed -= handBrakeAcceleration * Time.deltaTime;
					if (handBrakeSidewaySpeed < 0)
						handBrakeSidewaySpeed = 0;
				}
			} else {
				if (Mathf.Abs (LocomotionSpeed / maximumForwardVelocity) < 0.1f)
					handBrakeForwardSpeed = 0;
				if (Mathf.Abs (SidewayLocomotionSpeed / maximumSidewayVelocity) < 0.1f)
					handBrakeSidewaySpeed = 0;
			}
		} else {
			if (instantHandBrake) {
				if (handBrakeForwardSpeed < 1) {
					handBrakeForwardSpeed += handBrakeAcceleration * Time.deltaTime;
					if (handBrakeForwardSpeed > 1)
						handBrakeForwardSpeed = 1;
					handBrakeSidewaySpeed += handBrakeAcceleration * Time.deltaTime;
					if (handBrakeSidewaySpeed > 1)
						handBrakeSidewaySpeed = 1;
				}
			} else {
				if (Mathf.Abs (LocomotionSpeed / maximumForwardVelocity) < 0.1f)
					handBrakeForwardSpeed = 1;
				if (Mathf.Abs (SidewayLocomotionSpeed / maximumSidewayVelocity) < 0.1f)
					handBrakeSidewaySpeed = 1;
			}
		}

		LocomotionSpeed *= handBrakeForwardSpeed;
		SidewayLocomotionSpeed *= handBrakeSidewaySpeed;

		//  ****************************** Calculate the Rotatoion locomotion *****************************************
		ChairLocomotionDirection = (float)(ViveControllerYaw + ViveControllerYawOffset);

		float TranslateX = (float)(LocomotionSpeed * Mathf.Sin ((float)(ChairLocomotionDirection * Mathf.PI / 180)) * Time.deltaTime);
		float TranslateZ = (float)(LocomotionSpeed * Mathf.Cos ((float)(ChairLocomotionDirection * Mathf.PI / 180)) * Time.deltaTime);
		if (maximumSidewayVelocity != 0) {
			TranslateX += (float)(SidewayLocomotionSpeed * Mathf.Cos ((float)(ChairLocomotionDirection * Mathf.PI / 180)) * Time.deltaTime);
			TranslateZ -= (float)(SidewayLocomotionSpeed * Mathf.Sin ((float)(ChairLocomotionDirection * Mathf.PI / 180)) * Time.deltaTime);
		}

		if (maximumForwardVelocity == 0 && maximumSidewayVelocity == 0) {
			TranslateX = 0;
			TranslateZ = 0;
		}

		Vector3 pos = new Vector3 (TranslateX, 0.0f, TranslateZ);
		Debug.Log("Pos: " + pos.ToString());
		if (InterfaceIsReady)
			transform.Translate (pos); 

	}

}