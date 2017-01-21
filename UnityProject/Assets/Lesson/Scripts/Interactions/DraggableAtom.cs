using UnityEngine;
using System.Collections.Generic;
using System.Linq;
/*Prevent Dragging until the grab hand moves a sufficient distance away from the atom*/
public class DraggingAtomPrecursor : MonoBehaviour {

	private HandModeChooser hmc;
	void Start() {
		hmc = FindObjectOfType<HandModeChooser> ();
	}
	void Update() {
		var hand = hmc.currentGrabHand;

		if (hand == null || Vector3.Distance (hand.GetHandPosition(), transform.position) > Settings.DelayGrabDistance) {
			gameObject.AddComponent<DraggableAtom>();
			Destroy(this);
		}
	}
}

public class DraggableAtom : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		var collider = gameObject.AddComponent<SphereCollider> ();
		collider.isTrigger = true;

		gameObject.layer = 8;
		collider.isTrigger = false;
	}


	public Collider OnPinch(GrabHand hand, string inputName) {


		;

		var result = Instantiate (gameObject) as GameObject;

		result.transform.SetParent (gameObject.transform.parent, false);
		Destroy (result.GetComponent<DraggableAtom>());
		var ghost = result.AddComponent<DraggingGhostAtom> ();
		ghost.source = GetComponent<EditAtom>();
		//ghost.hand = hand;
		ghost.inputName = inputName;

		gameObject.SetGhost (0.4f);
		return result.AddComponent<SphereCollider> ();
	}

	// Update is called once per frame
	void Update () {
	
	}
}

public class DraggingGhostAtom : MonoBehaviour {

	public EditAtom source;
	public EditMolecule original;
	public Lesson lesson;
	//public GrabHand hand;
	public string inputName;

	public int ID { get; private set; }
	/*
	private string inputName {
		get {
			string inputType;
			switch (hand.GetType().Name) {
				case "MolyPolyGrabHand": inputType = "leap"; break;
				case "MouseHand": inputType = "mouse"; break;
				default: inputType = "unknown"; break;
			};
			return inputType;
		}
	}*/

	bool loggingPosition = false;
	private List<float> movePositions;

	void FixedUpdate() {
		if (loggingPosition) {
			var position = transform.position;
			movePositions.Add(position.x);
			movePositions.Add(position.y);
			movePositions.Add(position.z);
		}
	}

	public EmptyMeter rebondTimer;
	void Start() {
		gameObject.SetGhost (0.4f);
		lesson = FindObjectOfType<Lesson> ();
		original = lesson.GetMolecule ();
		rebondTimer = new EmptyMeter (Settings.AttachmentTime);

		ID = IDCounter.getID ("action");

		movePositions = new List<float> ();


		var log = new System.Collections.Generic.Dictionary<string, object> () {
			{"atomID", source.ID},
			{"actionID", ID},
			{"input", inputName},
		};
		SessionManager.session.Log ("atom_grabbed", log);
		loggingPosition = true;
		movePositions.Clear ();
	}

	public void OnRelease() {

		if(!TryReAttach()) TryDetach();	

		//Reset views.
		var log = new System.Collections.Generic.Dictionary<string, object> () {
			{"atomID", source.ID},
			{"actionID", ID},
			{"input", inputName},
		};
		SessionManager.session.Log ("atom_released", log);

		loggingPosition = false;

		if (movePositions != null) {
		var log2 = new Dictionary<string, object> () {
			{"array", movePositions.Select(f => decimal.Round((decimal)f, 2)).ToArray()},
			{"framerate", 1 / Time.fixedDeltaTime},
		};
						SessionManager.session.Log ("atom_position_data", log2);
		}
		source.gameObject.SetFull ();
						
		Destroy (gameObject);
		
	}

	public void CancelInteractions(string cause) {
		Debug.Log ("Cancelling drag operation due to " + cause);
		original = null;
		OnRelease ();
	}

	public bool TryDetach() {

		if (!source.isDetachable () || !WithinDetachDistance(source.transform.position, transform.position))
			return false;

		if(original != null)
			original.DetachAtom (source);
		
		return false;
	}

	public bool TryReAttach() {

		if (!source.isRebondable ())
			return false;

		var candidates = source.FindAttachableNearby ();
		var selectedHole = candidates.ClosestWithinThreshold (hole => Vector3.Distance (hole.worldPosition, transform.position), Settings.Threshold_Close);
		//var selectedHole = candidates.ClosestHoleWithinSecondThreshold(position);
		
		foreach(var hole in candidates) {
			hole.Highlight(0);
		}
		
		if (selectedHole.any && original != null) {
			original.ConsumeHoleToIncreaseBond(selectedHole.value, source);
			return true;
		}
		
		return false;
	}



	private static bool WithinDetachDistance(Vector3 a, Vector3 b) {
		return Vector3.Distance (a, b) > Settings.Threshold_VeryFar;
	}

	private EditMolecule.Hole lastClosestHole;
	bool closestHoleHasntChanged(EditMolecule.Hole hole) {
		if (hole.Equals (lastClosestHole))
			return true;
		
		lastClosestHole = hole;
		return false;
	}

	void Update() {


		if (source.isRebondable ()) {
			var candidates = source.FindAttachableNearby ();
			var selectedHole = candidates.ClosestWithinThreshold (hole =>
			    Vector3.Distance (hole.worldPosition, transform.position), Settings.Threshold_Close);
			
			foreach (var hole in candidates) {
				hole.Highlight (1);
			}
			
			if (selectedHole.any && closestHoleHasntChanged (selectedHole.value)) {
				rebondTimer.AddTick();
				selectedHole.value.Highlight (2, rebondTimer.value * rebondTimer.value);
			} else {
				rebondTimer.RemoveTick();
			}

			if(rebondTimer.ready) {
				OnRelease();
				return;
			}
		}

		if (source.isDetachable ()) {
			if(WithinDetachDistance(source.transform.position, transform.position)
			   && Time.time.Blink(8)) {
				source.gameObject.SetGhost(0f);
			} else {
				source.gameObject.SetGhost(0.4f);
			}
		}
	}
}