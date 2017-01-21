using System.Collections.Generic;
using UnityEngine;
//using System.Linq;
using System;

public static class Externals {

	public static EditBond.BondAttachTarget ToTarget(this Transform t) {
		return () => t.position;
	}
	
	public static EditBond.BondAttachTarget ToTarget(this EditAtom a) {
		return a.transform.ToTarget ();
	}
	
	
}
[Serializable]
public class EditMolecule : MonoBehaviour {

	/* Public Types & Variables
	 * ----------------------------------------------------------*/
	public struct Hole {
		public EditAtom parent;
		public uint slot;
		public Vector3 position {
			get {
				if(parent == null) return Vector3.zero;
				else return parent.GetSlotVector(slot);
			}
		}
		public Vector3 worldPosition {
			get {
				if(parent == null) return Vector3.zero;
				return parent.transform.TransformPoint(position);
			}
		}

		public void ReplaceWith(EditAtom r, EditBond b) {
			if (parent == null)
								return;

			parent.BondWith (r, b, slot);
			r.BondWith (parent, b);

			Hole replaced = new Hole() {
				parent = r,
				slot = 0
			};

			var rotation = Quaternion.FromToRotation (replaced.position, -this.position);
			r.transform.localRotation = parent.transform.localRotation * rotation;
		}

		public void Highlight(uint level, float value = 0f) {
			if (parent != null)
				parent.HighlightHole (this, level, value);
		}

	}

	/* Static Methods, Constructors
	 * ----------------------------------------------------------*/
	public static EditMolecule Create(GameObject root) {
		var r = root.AddComponent<EditMolecule> ();
		r._atoms = new List<EditAtom> ();
		return r;
	}

	/* Modification Methods
	 * ----------------------------------------------------------*/

	public EditAtom UpgradeHole(Hole h, uint element) {
		SendMessageUpwards ("Record", gameObject, SendMessageOptions.RequireReceiver);
		var r = EditAtom.Create (gameObject, (Chemistry.Element)element);


		var log = new Dictionary<string, object> () {
			{"atomicNumber", element},
			{"elementName", ColouredBall.GetDisplay ((int)element).name},
			{"atomID", r.ID},
		};
		if (h.parent != null) log.Add ("parentID", h.parent.ID);
		SessionManager.session.Log ("model_add_atom", log);


		r.transform.position = h.worldPosition;

		_atoms.Add (r);

		if (h.parent != null) {
			var b = EditBond.Create(gameObject, r.transform, h.parent.transform, 1);
			h.ReplaceWith (r, b);
		}

		UpdateOffsetPosition ();
		return r;
	}

	private static Vector3 CloserToOrigin(EditAtom A, EditAtom B) {
		var a = A.transform.position;
		var b = B.transform.position;
		
		return a.sqrMagnitude < b.sqrMagnitude ? a : b;
	}

	public void ConsumeHoleToIncreaseBond(Hole h, EditAtom a) {
		SendMessageUpwards ("Record", gameObject, SendMessageOptions.RequireReceiver);
		//Bond is between two atoms:
		//Atom 1: a
		//Atom 2: h.parent
		var b = h.parent;




		var bond = EditAtom.IncreaseBondTo (a, h);

		var log = new Dictionary<string, object> () {
			{"atoms", new int[] {a.ID, b.ID}},
			{"degree", bond.degree},
		};
		SessionManager.session.Log ("model_increase_bond", log);

		Maybe<EditAtom> biggestA;
		Maybe<EditAtom> biggestB;

		int sizeA = a.LargestSubchain (out biggestA, b);
		int sizeB = b.LargestSubchain (out biggestB, a);
		if(sizeA > sizeB && biggestA.any) {
			EditAtom.RebuildGeometry(a, biggestA.value);
			Debug.Log("Largest subchain is on source: " + string.Format ("{0} {1}", sizeA, sizeB));
		} else if(biggestB.any) {
			EditAtom.RebuildGeometry(b, biggestB.value);
			Debug.Log("Largest subchain is on target: " + string.Format ("{0} {1}", sizeA, sizeB));
		} else {
			EditAtom.RebuildGeometry(a, b);
		}

		UpdateOffsetPosition ();




	}

	public void DetachAtom(EditAtom a) {
		SendMessageUpwards ("Record", gameObject, SendMessageOptions.RequireReceiver);
		//assert a.isDetachable()

		var log = new Dictionary<string, object> () {
			{"atomID", a.ID}
		};
		SessionManager.session.Log ("model_remove_atom", log);

		a.Detach ();
		_atoms.Remove (a);
		UpdateOffsetPosition ();
	}

	/* Query Methods
	 * ----------------------------------------------------------*/
	public IEnumerable<Hole> holes {
		get {
			if(_atoms.Count == 0) yield return new Hole();
			else {
				foreach(var a in _atoms) {
					foreach(var h in a.holes) {
						yield return h;
					}
				}
			}
		}
	}

	public IEnumerable<EditAtom> atoms {
		get { if(_atoms == null) return System.Linq.Enumerable.Empty<EditAtom>();
			return _atoms; }
	}

	/* Cosmetic modifiers
	 * ----------------------------------------------------------*/

	Bounds bounds;
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		//var b = new Bounds (Vector3.zero, Vector3.one * 5);
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}

	void UpdateOffsetPosition() {
		bounds = new Bounds (atoms.MaybeFirst ().Select(a => a.transform.position).otherwise (Vector3.zero), Vector3.zero);
		//bounds = new Bounds ();

		foreach(var a in atoms) {
				bounds.Encapsulate(a.transform.position);
		}

		transform.position += transform.parent.position-bounds.center;
	}

	void Start() {
		foreach (var c in holes) {
			c.Highlight (0);
		}
	}

	void OnEnable() {
	//	UpdateOffsetPosition ();

	}

	void Update() {
		//UpdateOffsetPosition ();

	}

//	Vector3 movingVelocity = Vector3.zero;
//	Vector3 offsetPosition = Vector3.zero;
	[SerializeField]
	List<EditAtom> _atoms;
}