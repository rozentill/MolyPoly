using UnityEngine;
using System.Collections.Generic;
using Leap;
using System.Linq;

//handle rotation with this frame and previous frame



public class rotation1 : MonoBehaviour {
	
	public static rotation1 leapRotation;
	
	public float modelScale2 =0.1f;
	
	Quaternion quaStartModel;
	Vector3 lastHand;
	
	public bool rotatingPreviousFrame = false;
	public bool rotatingThisFrame = false;
	public bool inside = false;

	
	private Vector3 pinch_position {
		get {
			HandModel hand_model = GetComponent<HandModel> ();
			var p = hand_model.GetPalmPosition () + MolyPolyGrabHand.f1;
			///Debug.Log(p);
			return p;
		}
	}

	bool loggingRotation = true;
	private List<float> rotationAngles;
	void FixedUpdate() {
		if (loggingRotation) {
			var midModel = GameObject.FindGameObjectWithTag ("midModel");
			var angles = midModel.GetComponent<RemoteRotation>().remote.transform.localRotation.eulerAngles;
			rotationAngles.Add(angles.x);
			rotationAngles.Add(angles.y);
			rotationAngles.Add(angles.z);
		}
	}

	void Start () {
		rotationAngles = new List<float> ();
		//HandModel hand_model = GetComponent<HandModel> ();
		//Hand leap_hand = hand_model.GetLeapHand ();
		//if(leap_hand.IsLeft)
		leapRotation = this;
		RemoteRotation.stopGrab = false;
		//rotatingPreviousFrame = false;
		//Debug.Log("asdasdadasdas");
	}
	
	void LogRotationStopped() {
		Debug.Log ("rotation_stopped");
		loggingRotation = false;
		SessionManager.session.Log("rotate_leap_stop", null);
		var log = new Dictionary<string, object> () {
			{"array", rotationAngles.Select(f => decimal.Round((decimal)f, 2)).ToArray()},
			{"framerate", 1 / Time.fixedDeltaTime},
		};
		SessionManager.session.Log ("rotate_leap_data", log);
	}
	void Update () {
		//Debug.Log("22222222222222");
		HandModel hand_model = GetComponent<HandModel> ();
		Hand leap_hand = hand_model.GetLeapHand ();
		var close_things = Physics.OverlapSphere(pinch_position, modelScale2);//center radius
		var midModel = close_things.Where(collider => collider.tag == "midModel").FirstOrDefault();
		


		if ((leap_hand == null) || (leap_hand.IsRight) || (midModel == null)) {
			if(rotatingPreviousFrame)
			{
				RemoteRotation.stopGrab = true;
				LogRotationStopped();
				rotatingPreviousFrame = false;
			}

				inside = false;
				return;
	
		}
		else 
		{
			inside = true;
			if (leap_hand.GrabStrength < Settings.Threshold_RotationGrab) {
				rotatingThisFrame = false;
				//return;
			}
			else {rotatingThisFrame = true;}
		}
		
		if (rotatingPreviousFrame) {
			if (rotatingThisFrame) {
				//T T rotation and record current postion
				midModel.gameObject.transform.rotation = Quaternion.FromToRotation(lastHand,pinch_position)*quaStartModel;
				quaStartModel = midModel.transform.rotation;
				lastHand = pinch_position;
			}
			
			else {
				// T F 
				RemoteRotation.stopGrab = true;
				LogRotationStopped();
				//rotation stopped 
			}
		} else {
			if (rotatingThisFrame) {
				// F T
				// first frame where the rotation started
				// record current hand position / rotation?
				quaStartModel = midModel.transform.rotation;
				lastHand = pinch_position;
				RemoteRotation.locked=false;
				Debug.Log ("rotation_started");
				loggingRotation = true;
				rotationAngles.Clear();
				SessionManager.session.Log("rotate_leap_start", null);
			}
			else {
				//F F 
				//stopGrab = false;
				//nothing rotation has ended
			}
		}
		rotatingPreviousFrame = rotatingThisFrame;
	}
	
}

