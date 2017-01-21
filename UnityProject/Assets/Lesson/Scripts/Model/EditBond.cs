using UnityEngine;
using System.Collections;
using System;


[Serializable]
public struct Target {

	public enum Method {
		Transform,
		AtomHole
	}

	public Transform first;
	public Transform second;

	public EditAtom source;
	public int slot;

	public Method method;

	public Vector3 GetSlotPosition() {
		return source.GetSlotVector ((uint)slot);
	}

	public void GetVectors(out Vector3 firstv, out Vector3 secondv) {
		if (method == Method.AtomHole) {
			firstv = source.transform.position;
			//transform the slot vector from local space to world space,
			//scaled by the parent's scaling.
			var vector = source.GetSlotVector ((uint)slot);
			//var offset = source.transform.parent.TransformPointInParentScale(vector);
			secondv = source.transform.TransformPoint(vector);
		} else {
			firstv = first.transform.position;
			secondv = second.transform.position;
		}
	}


}

[Serializable]
public class EditBond : MonoBehaviour {

	public delegate Vector3 BondAttachTarget();


	public static EditBond Create(GameObject parent, Target target, int degree) {
		var o = new GameObject ("EditBond");
		o.transform.SetParent (parent.transform, false);
		var r = o.AddComponent<EditBond> ();
		//r.first = first;
		//r.second = second;
		r.targetInfo = target;
		r.degree = degree;
		r.UpdateVisuals ();
		r.UpdateOrientation ();
		return r;
	}

	public static EditBond Create(GameObject parent, EditAtom source, int slot, int degree = 0) {
		return Create (parent, new Target () {
			method = Target.Method.AtomHole,
			source = source,
			slot = slot,
		}, degree);
	}

	public static EditBond Create(GameObject parent, Transform first, Transform second, int degree = 0) {
		return Create (parent, new Target () {
			method = Target.Method.Transform,
			first = first,
			second = second,
		}, degree);
	}

	public void IncreaseDegree() {
		degree++;
		UpdateVisuals ();
	}

	private void UpdateVisuals() {
		if (visuals != null)
						Destroy (visuals);

		GameObject prototype = null;
		switch (degree) {
		case 2:
			prototype = Load.DoubleCylinder;
			break;
		case 3:
			prototype = Load.TripleCylinder;
			break;
		default:
		case 1:
			prototype = Load.ColouredCylinder;
			break;
		}
		
		visuals = Load.InstantiateWithParent (prototype, transform);
		visuals.transform.SetParent (transform, false);
		SetupColours ();

		UpdateOrientation ();
	}

	public void UpdateOrientation() {
		Vector3 first, second;
		targetInfo.GetVectors (out first, out second);
		ApplyTransform (visuals.transform, first, second);
	}

	public static void ApplyTransform(Transform t, Vector3 first, Vector3 second) {

		float length = Vector3.Distance (t.InverseTransformPoint(first), t.InverseTransformPoint(second));
		t.localScale = new Vector3 (1, 1, length);

	
		t.position = first;
		t.LookAt (second);
	}

	[SerializeField]
	private Material m;
	[SerializeField]
	private Color normal;
	[SerializeField]
	private Color highlighted;

	void SetupColours() {
		m = visuals.GetComponentInChildren<Renderer> ().material;
		normal = m.color;
		highlighted = Color.magenta;	
	}

	public const float flashes = 6.0f * 2.0f * Mathf.PI;

	public void SetHighlight(uint level, float elapsedtime = 0) {
		if (level == 0)
			visuals.SetActive(false);
		if (level >= 1) {
			visuals.SetActive (true);
			m.color = normal;
		}
		if (level == 2) {
			if (Mathf.Sin (elapsedtime * flashes) > 0)
					m.color = highlighted;
		}
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public int degree {
		get {
			return _degree;
		}
		set { 
			Debug.Log (string.Format("Set Degree from {0} to {1}", _degree, value));
			_degree = value;
		}
	}

	[SerializeField]
	public int _degree;

	//public BondAttachTarget first;
	//public BondAttachTarget second;

	[SerializeField]
	private Target targetInfo;

	[SerializeField]
	GameObject visuals;
}
