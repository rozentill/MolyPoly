using UnityEngine;
using System.Collections;

public class HandModeChooser : MonoBehaviour {
	
	public bool UseLeap = true;
	// Use this for initialization
	public GrabHand currentGrabHand {
		get {
			if(UseLeap) {
				var mpg = FindObjectOfType<MolyPolyGrabHand>();
				return mpg as GrabHand;
			}
			else return mouseGrabHand as GrabHand;
		}
	}

	public MouseHand mouseGrabHand;
	public HandController LeapHandController;
	
	void Start () {
		if (UseLeap) {
			LeapHandController.gameObject.SetActive (true);
		} 
			mouseGrabHand.gameObject.SetActive (true);

	}
}
