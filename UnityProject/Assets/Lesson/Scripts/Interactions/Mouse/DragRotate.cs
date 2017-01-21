using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DragRotate : Draggable {

	private Quaternion offset;
	// Use this for initialization

	bool loggingRotation = false;
	private List<float> rotationAngles;
	void FixedUpdate() {
		if (loggingRotation) {
			var angles = transform.localRotation.eulerAngles;
			rotationAngles.Add(angles.x);
			rotationAngles.Add(angles.y);
			rotationAngles.Add(angles.z);
		}
	}

	void Start() {
		rotationAngles = new List<float> ();
	}
	void Awake () {
		Dragging.OnEnter += delegate {
			offset = transform.localRotation;
			loggingRotation = true;
			rotationAngles.Clear();
			SessionManager.session.Log("rotate_mouse_start", null);
			//LeapingChemistry.Load.sess.AddLogMessage("rotation_begin");
		};
		Dragging.Update += delegate {
			transform.localRotation = Quaternion.Euler (-distance.y, distance.x, 0) * offset;
		//	AddSample();
		};
		Dragging.OnExit += delegate {
		//	LeapingChemistry.Load.sess.AddLogMessage("rotation_end");
			loggingRotation = false;
			SessionManager.session.Log("rotate_mouse_end", null);
			var log = new Dictionary<string, object> () {
				{"array", rotationAngles.Select(f => decimal.Round((decimal)f, 2)).ToArray()},
				{"framerate", 1 / Time.fixedDeltaTime},
			};
			SessionManager.session.Log ("rotate_mouse_data", log);
			offset = transform.localRotation;
		};
	}

}
