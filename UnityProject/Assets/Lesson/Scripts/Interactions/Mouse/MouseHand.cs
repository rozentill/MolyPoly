using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MouseHand : MonoBehaviour, GrabHand {

	public static readonly Plane fallbackPlane;

	static MouseHand() {
		fallbackPlane = new Plane (Vector3.back, new Vector3(0f, 0f, -4.08f));
	}

	public Vector3 position {
		get {
			return _position;
		}
	}

	private Vector3 _position;

	public GameObject MoleculeRotationPivot;

	private GameObject _grabbed;

	// Use this for initialization
	void Start () {
		MoleculeRotationPivot.AddComponent<DragRotate> ();
		MoleculeRotationPivot.AddComponent<DragInputController> ();
	}

	private GameObject GetPinchable(Ray ray) {
		//var candidates = Physics.OverlapSphere (point, Settings.HandMagnetDistance);
		var candidates = Physics.SphereCastAll (ray, Settings.HandMagnetDistance, 30.0f, 1 << 8)
			.OrderBy(raycasthit => raycasthit.distance)
			.Select(raycasthit => raycasthit.collider);

		var menuItem = (from collider in candidates
				where collider.GetComponent<SetAtomColor> () != null
		                select collider.GetComponent<SetAtomColor> ()).FirstOrDefault ();
		if (menuItem != null) {
			//var result = Instantiate(menuItem.gameObject, position, Quaternion.identity) as GameObject;
			var result = menuItem.InstantiateDraggableAtom("mouse");
			//result.transform.SetParent(menuItem.transform.parent, false);
			return result;
		}

		var draggableAtom = (from collider in candidates
		                where collider.GetComponent<DraggableAtom> () != null
		                select collider.GetComponent<DraggableAtom> ()).FirstOrDefault ();

		if (draggableAtom != null) {
			var result = draggableAtom.OnPinch(this, "mouse").gameObject;
			return result;
		}

		return null;
	}
	
	// Update is called once per frame
	void Update () {
		if (Camera.main == null)
						return;

		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Debug.DrawRay (ray.origin, ray.direction * 30);

		_position = fallbackPlane.CastPoint(ray).otherwise(Vector3.zero);

		if (_grabbed == null && Input.GetAxis(Settings.MouseDragButton) > 0) {
			_grabbed = GetPinchable(ray);
		}

		if (_grabbed != null) {
			_grabbed.transform.position = position;
		}
		
		if (_grabbed != null && Input.GetAxis (Settings.MouseDragButton) == 0) {
			ReleaseHand();
		}
	}

	#region GrabHand implementation

	public void ReleaseHand ()
	{
		if (_grabbed != null) {
			var draggable = _grabbed.GetComponent<DraggingGhostAtom>();
			if(draggable != null) draggable.OnRelease();

			Destroy(_grabbed);
			_grabbed = null;
		}
	}

	public Vector3 GetHandPosition ()
	{
		return position;
	}

	#endregion
}
