using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;


public static class LogMenuGrabbedEvent {

}

public class SelectedItem : MonoBehaviour {

	public int actionID { get; private set; }
	public string inputName { private get; set; }

	private Lesson lesson;
	
	public static SelectedItem Setup(int element, GameObject parent, Action finishedCallback, string inputName) {
				var obj = ColouredBall.Setup ((uint)element, parent);
				var selected = obj.AddComponent<SelectedItem> ();
				//	var rigidbody = obj.AddComponent<Rigidbody> ();
				//	rigidbody.useGravity = false;
				//		rigidbody.drag = Mathf.Infinity;
				//rigidbody.angularDrag = Mathf.Infinity		;
		obj.AddComponent<SphereCollider> ();

		selected.meter = new EmptyMeter(Settings.AttachmentTime);
		selected.element = element;
		selected.finishedCallback = finishedCallback;
		selected.actionID = IDCounter.getID ("action");
		selected.inputName = inputName;
		return selected;
	}

	//private EditMolecule.Molecule molecule;
	private MeterTimer meter;
	private int element;
	private Action finishedCallback;

	//public Plane fallbackPlane;

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

	public void LogGrabbed() {
		var log = new System.Collections.Generic.Dictionary<string, object> () {
			{"input", inputName},
			{"actionID", actionID},
		};
		SessionManager.session.Log ("menu_grabbed", log);
		loggingPosition = true;
		movePositions.Clear ();
	}
	
	public void LogReleased() {
		var log = new System.Collections.Generic.Dictionary<string, object> () {
			{"input", inputName},
			{"actionID", actionID},
			};
			SessionManager.session.Log ("menu_released", log);
		loggingPosition = false;
		var log2 = new Dictionary<string, object> () {
			{"array", movePositions.Select(f => decimal.Round((decimal)f, 2)).ToArray()},
			{"framerate", 1 / Time.fixedDeltaTime},
		};
		SessionManager.session.Log ("menu_position_data", log2);
		}

	void Start() {
		lesson = GameObject.FindObjectOfType<Lesson>();
		movePositions = new List<float> ();
		LogGrabbed ();
	}

	void Update() {
		var molecule = lesson.GetMolecule ();
		if (molecule != null) {
			UpdateSelection(transform.position, molecule);
		}
	}

	private EditMolecule.Hole closestHoleLastFrame;
	bool closestHoleHasntChanged(EditMolecule.Hole hole) {
		if (hole.Equals (closestHoleLastFrame)) 
						return true;

		closestHoleLastFrame = hole;
		return false;
	}

	/*public void UpdatePositionOrFallback(Maybe<EditMolecule.Hole> target, Ray ray) {
		transform.localPosition = target.any ? target.value.worldPosition : EditMoleculeExternalSearching.fallbackPlane.CastPoint(ray).otherwise(Vector3.zero);
	}*/
	
	public void UpdateSelection(Vector3 position, EditMolecule molecule) {



		//var closestAtom = molecule.atoms.ClosestWithinThreshold(atom => atom.DistanceTo(position), Settings.Threshold_Medium);
		var closestAtom = molecule.ClosestBondableAtom(atom => atom.DistanceTo(position));

		//Candidates are the holes attached to that atom.
		var candidates = closestAtom.AttachmentCandidates (position);



		if (closestAtom.any) {
			closestAtom.value.EnableFlashAtom ();
			foreach (var atom in molecule.atoms.Except(new EditAtom[] {closestAtom.value})) {
				atom.DisableFlashAtom ();
			}
		} else {
			foreach (var atom in molecule.atoms) {
				atom.DisableFlashAtom ();
			}
		}

		foreach (var c in candidates) {
			c.Highlight (1);
		}

		foreach (var c in System.Linq.Enumerable.Except(molecule.holes, candidates)) {
			c.Highlight (0);
		}


		//var closest = candidates.ClosestHoleWithinSecondThreshold (position);
		var closest = candidates.ClosestWithinThreshold(hole => Vector3.Distance(hole.worldPosition, position), Settings.Threshold_Close);

		//UpdatePositionOrFallback (closest, position);			                        

		if (closest.any && closestHoleHasntChanged (closest.value)) {
				meter.AddTick ();
				closest.value.Highlight(2, meter.value * meter.value);
	
				if (meter.ready) {
						molecule.UpgradeHole (closest.value, (uint)element);
						finishedCallback ();
					//	molecule.gameObject.BroadcastMessage ("DisableFlashAtom", SendMessageOptions.DontRequireReceiver);
				}
		} else {
			//
			meter.value = 0f;
		}

	}

	void OnDestroy() {
		var molecule = lesson.GetMolecule ();
		if (molecule != null) {
			molecule.gameObject.BroadcastMessage ("DisableFlashAtom", null, SendMessageOptions.DontRequireReceiver);

			foreach (var c in molecule.holes) {
				c.Highlight (0);
			}
		}
		LogReleased ();
	}

}
