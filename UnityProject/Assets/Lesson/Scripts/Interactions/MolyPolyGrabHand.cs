using UnityEngine;
using System.Collections.Generic;
using Leap;
using System.Linq;


public static class MagneticPinchExtensions {
	public static Vector3 ClosestPoint(this Bounds b, Vector3 p) {
		return new Vector3 (
			Mathf.Clamp (p.x, b.min.x, b.max.x),
			Mathf.Clamp (p.y, b.min.y, b.max.y),
			Mathf.Clamp (p.z, b.min.z, b.max.z));
	}
}

public interface GrabHand {
	void ReleaseHand();
	Vector3 GetHandPosition();
}

public class MolyPolyGrabHand : MonoBehaviour, GrabHand {
	
	private const float TRIGGER_DISTANCE_RATIO = 0.8f;

	public static Vector3 position;

	//invariants
	//BOUNDARY_LEFT < x < BOUNDARY_RIGHT;
	//BOUNDARY_BOTTOM < y < BOUNDARY_TOP;
	//BOUNDARY_FRONT < z < BOUNDARY_BACK;
	
	private WallBounds boundary;

	public static readonly Vector3 f1 = new Vector3 (-0.2f, 0, 1.2f); //position adjust of ball hold
	public static readonly float magnetDistance = Settings.HandMagnetDistance;


	private Collider grabbed_;
	
	void Start() {
		boundary = GameObject.FindObjectOfType<WallBounds> ();
		grabbed_ = null;
	}
	void OnDestroy() {
		OnRelease ();
	}

	public Collider GetPinchableCollider(Vector3 pinch_position) {
		var close_things = Physics.OverlapSphere(pinch_position, magnetDistance);
		
		return close_things.FirstOrDefault (thingJ =>
		                                      Vector3.Distance (pinch_position, thingJ.transform.position) < Settings.HandMagnetDistance
		                                            && !thingJ.transform.IsChildOf (transform)
		                                            && thingJ.gameObject.tag != "terrain"
													&& thingJ.tag != "midModel");
	}
	
	Collider OnPinch(Vector3 pinch_position) {

		var collider = GetPinchableCollider (pinch_position);

		if (collider != null) {
			var atom = collider.gameObject.GetComponent<DraggableAtom>();
			if(atom != null) {
				collider = atom.OnPinch(this, "leap");
			}
		}

		return collider;
	}

	bool tag_is_element (Collider c)
	{
		string[] tags = {"element1", "element2", "element3", "element4"};
		return tags.Contains (c.gameObject.tag);
	}

	void destroy_balls()
	{
		GameObject[] balls;
		balls = GameObject.FindGameObjectsWithTag("ball");
		foreach (GameObject b in balls) {
			Destroy(b.gameObject);
		}
	}
	void resetBallsColour()
	{
		GameObject[] respawns;
		respawns = GameObject.FindGameObjectsWithTag("ball");
		foreach (GameObject b in respawns) {
			var sec = b.GetComponent<ColouredBall>();
			if(sec != null)
				sec.ResetColor();
			else
				b.gameObject.renderer.material.color = Color.white;
			
		}

		string[] tags = {"element1", "element2", "element3", "element4"};

		foreach (var tag in tags) {
			respawns = GameObject.FindGameObjectsWithTag (tag);
			foreach (GameObject b in respawns) {
				b.gameObject.renderer.material.color = ColouredBall.GetColor ((uint)Chemistry.WhichElement (tag));
			}
		}
	}

	void OnRelease() {
		if(grabbed_ != null){
			//Release
			var ghost = grabbed_.gameObject.GetComponent<DraggingGhostAtom>();
			if(ghost != null) {
				ghost.OnRelease();
			} else {
				Destroy(grabbed_.gameObject);
			}


			grabbed_ = null;
		}
	}

	public Vector3 GetHandPosition() {
		HandModel hand_model = GetComponent<HandModel> ();
		var p = hand_model.GetPalmPosition ()+f1;
		///Debug.Log(p);
		return p;
	}

	public void ReleaseHand() {
		OnRelease ();
	}

	void Update() {

		var hand_model = GetComponent<HandModel> ();
		var leap_hand = hand_model.GetLeapHand ();
		var pinch_position = GetHandPosition ();
		position = pinch_position;

		if (leap_hand == null || leap_hand.IsLeft) {

			return;
		}
		resetBallsColour();


		var colours = from collider in Physics.OverlapSphere (pinch_position, Settings.HandHighLightDistance * 2)
			where Vector3.Distance (pinch_position, collider.transform.position) < Settings.HandHighLightDistance
						//&& !collider.transform.IsChildOf (transform)
						//&& collider.gameObject.tag != "terrain"
						&& collider.gameObject.GetComponent<SetAtomColor>() != null
						&& collider != grabbed_ //added to prevent the carried ball from being highlighted.
				select collider.gameObject.GetComponent<SetAtomColor>();

		foreach (var c in colours) {
			c.Highlight();
		}


		if (leap_hand.GrabStrength > Settings.Threshold_Grab && grabbed_ == null)
			grabbed_ = OnPinch (pinch_position);
		else if(leap_hand.GrabStrength < Settings.Threshold_Release)
			OnRelease ();

		if (grabbed_ != null) {
		

			if (tag_is_element(grabbed_) == true)
			{
				//Debug.Log ("grab_element");
				var menuItem = grabbed_.gameObject.GetComponent<SetAtomColor>();
				if(menuItem != null) {
					var new_one = menuItem.InstantiateDraggableAtom("leap");
					destroy_balls();
					//which_element(grabbed_);
					new_one.tag = "ball";
					grabbed_ = new_one.collider;
					grabbed_.gameObject.SetActive(true);
				}
			}

			if(boundary != null) {
				var bounds = boundary.GetBoundary();
				grabbed_.transform.position = bounds.ClosestPoint(pinch_position);
			}
		}
	}
}
