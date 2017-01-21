using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public static class EditAtomExtensions {
	public static uint IndexOf<T>(this IEnumerable<T> seq, Func<T, bool> predicate) {
		return seq
		.Select ((b, i) => new { value = b, index = (uint)i})
		.Where (p => predicate (p.value))
		.Select(p=>p.index)
		.Single ();
	}

	public static Vector3 TransformPointInParentScale(this Transform t, Vector3 point) {

		//transform into t's parent's world space offset by t's position;
		return t.parent.TransformPoint (t.localPosition + (t.localRotation * point));
	}

	public static bool isRebondable(this EditAtom atom) {
		//does this atom have any holes?
		//and also neighbouts with at least one hole?
		return atom.holes.Any() && atom.neighbours.Any(p => p.holes.Any());
	}
	
	public static bool isDetachable(this EditAtom atom) {
		//Must have exactly one neighbour.
		return atom.neighbours.Count () == 1;
	}
	
	public static float DistanceTo(this EditAtom atom, Ray ray) {
		return atom.transform.position.DistanceToRay (ray);
	}
	
	public static float DistanceTo(this EditAtom atom, Vector3 point) {
		return Vector3.Distance (atom.transform.position, point);
	}
}

[Serializable]
public class EditAtom : MonoBehaviour {

	private static int NextID = 0;

	public int ID { get; private set; }

	public static EditAtom Create(GameObject parent, Chemistry.Element elem) {
		var o = ColouredBall.Setup ((uint)elem, parent);
		var r = o.AddComponent<EditAtom> ();
		o.AddComponent<DraggingAtomPrecursor> ();
		r.SetupSlots (elem);
		r.ID = EditAtom.NextID++;
		return r;
	}

	public void SetupSlots(Chemistry.Element elem) {
		this.elem = elem;
		geometry = Chemistry.GetBondGeometry (elem).ToArray ();
		//slots = new Bond[geometry.Length];

		slots = geometry.Select<Vector3, Bond> (SetupHole).ToArray ();

		/*
		for(uint i = 0; i < slots.Length; i++) {
			uint x = i; //we want the lambda to capture a _COPY_ of i, not i.
			slots[i].bond = EditBond.Create(
				gameObject, this.ToTarget(),
				() => transform.TransformPointWithoutScale(GetSlotVector(x)),
				0);
			slots[i].bond.SetHighlight(0);
		}*/
	}

	Bond SetupHole(Vector3 position, int i) {

		var bond = EditBond.Create(
						gameObject, this,i,
						0);
		bond.SetHighlight (0);
		return new Bond () {
			bond = bond,
		};
	}

	Bond SetupHole(int i) { return SetupHole (Vector3.zero, i); }

	public IEnumerable<EditMolecule.Hole> holes {
		get {
			for (uint i = 0; i < slots.Length; i++) {
				if(slots[i].target == null) {
					yield return new EditMolecule.Hole() {
						parent = this,
						slot = i,
					};
				}
			}
		}
	}

	public IEnumerable<EditAtom> neighbours {
		get {
			foreach(var b in slots) {
				if(b.target != null) {
					yield return b.target;
				}
			}
		}
	}

	public IEnumerable<EditMolecule.Hole> FindAttachableNearby() {
		return slots
			.Where (b => b.target != null && b.degree < 3)
			.Select (b => b.target.holes)
			.Aggregate (new List<EditMolecule.Hole>().AsEnumerable(), (a, b) => a.Concat(b));
	}

	public Vector3 GetSlotVector(uint i) {
		return geometry [i];
	}

	public void BondWith(EditAtom r, EditBond bond, uint slot = 0) {
		if (slots [slot].bond != null) {
			Destroy(slots[slot].bond.gameObject);
		}

		slots [slot] = new Bond () {
			target = r,
			bond = bond,
		};
	}

	public static EditBond IncreaseBondTo(EditAtom atomB, EditMolecule.Hole holeA) {
		//We need some hole to consume on b.
		var holeB = atomB.holes.First ();
		var atomA = holeA.parent;

		atomB.Collapse (holeB.slot);
		atomA.Collapse (holeA.slot);

		var bond = atomA.slots.Where (slot => slot.target == atomB).Single ().bond;
		bond.IncreaseDegree ();
		return bond;
	}

	public static void RebuildGeometry(EditAtom A, EditAtom B) {
		B.PropagateNewPosition (new HashSet<EditAtom> (), A, B.transform.position);
	}

	public int CountAtomsRecursively(HashSet<EditAtom> exclude) {
		if (exclude.Contains (this))
						return 0;

		int total = 1;
		exclude.Add (this);
		foreach (var a in neighbours) {
			total += a.CountAtomsRecursively(exclude);
		}
		return total;
	}

	public int LargestSubchain(out Maybe<EditAtom> result, EditAtom excluded) {
		if (!neighbours.Any ()) {
			result = new None<EditAtom> ();
			return 0;
		}

		var biggest = neighbours
			.Where(atom => atom != excluded)
			.Select (atom => new {size = atom.CountAtomsRecursively (new HashSet<EditAtom> () {this}), atom = atom})
			.OrderBy (p => p.size)
			.MaybeFirst ();

		result = biggest.Select(a => a.atom);
		return biggest.Select (a => a.size).otherwise (0);
	}

	void ContinuePropagate(HashSet<EditAtom> exclude) {
		exclude.Add (this);
		for(uint i = 0; i < slots.Length; i++) {
			var b = slots[i];
			if(b.target == null) {
				b.bond.UpdateOrientation();
			}
			else {
				b.target.PropagateNewPosition(exclude, this, transform.TransformPointInParentScale(GetSlotVector(i)));
				b.bond.UpdateOrientation();
			}
		}
	}

	void PropagateNewPosition(HashSet<EditAtom> exclude, EditAtom source, Vector3 position) {
		if (exclude.Contains (this))
						return;

		transform.position = position;
		Vector3 slotPosition = GetSlotVector (slots.IndexOf (b => b.target == source));
		var rotation = Quaternion.FromToRotation (slotPosition, Vector3.forward);

		transform.LookAt (source.transform.position, source.transform.rotation * Vector3.up);
		transform.localRotation = transform.localRotation * rotation;

		ContinuePropagate (exclude);

	}


	void Collapse(uint hole) {

		System.Diagnostics.Debug.Assert (slots [hole].target == null);

		int newSlotCount = slots.Length - 1;

		geometry = Chemistry.NieveBondGeometry (elem, newSlotCount);

		Destroy (slots [hole].bond.gameObject);
		slots = slots.Where ((b, i) => i != hole).ToArray ();

			
		for(int i = 0; i < slots.Count (); i++) {


			if(slots[i].target == null) {
				Destroy(slots[i].bond.gameObject);
				slots[i] = SetupHole(i);
			}
		}
	}
	
	public void Detach() {
		neighbours.Single().RemoveAsNeighbour (this);
		Destroy (gameObject);
	}

	void RemoveAsNeighbour(EditAtom a) {
		var slot = slots.Where (b => b.target == a).Single ();
		Destroy (slot.bond.gameObject);

		if (slot.degree > 1) {
			int newSlotCount = slots.Length + (int)slot.degree - 1;
			geometry = Chemistry.NieveBondGeometry(elem, newSlotCount);

			var oldSlots = slots;
			slots = new Bond[geometry.Count()];
			
			for (int i = 0; i < geometry.Count(); i++) {
				if(i < oldSlots.Count() && oldSlots[i].target != a) {
					slots[i] = oldSlots[i];
					slots[i].bond.UpdateOrientation();
				} else {
					slots[i] = SetupHole(i);
				}
			}

			if(neighbours.Any())
				RebuildGeometry(this, neighbours.First());
		} else {
			var i = slots.IndexOf(b => b.target == a);
			slots[i] = SetupHole((int)i);
		}
	}

	public void HighlightHole(EditMolecule.Hole h, uint level, float elapsedTime) {
		var b = slots [h.slot];
		b.bond.SetHighlight (level, elapsedTime);
	}

	void Update() {
		if (EnableFlash)
		if (Time.time.Blink (4)) {
			gameObject.GetComponent<ColouredBall>().SetColor (Color.magenta);
		} else {
			gameObject.GetComponent<ColouredBall>().SetDefaultColor ();
		}
	}
	private bool EnableFlash = false;
	
	public void EnableFlashAtom() {
		EnableFlash = true;
	}
	
	public void DisableFlashAtom() {
		EnableFlash = false;
		gameObject.GetComponent<ColouredBall> ().SetDefaultColor ();
	}

	[Serializable]
	struct Bond {
		public int degree {
			get {
				if(bond == null) {
					Debug.Log("Empty Bond");
					return 0;
				}
				else return bond.degree;
			}
		}
		public EditAtom target;
		public EditBond bond;
	}

	public struct TargetWithDegree
	{
		public EditAtom target;
		public int degree;
	}

	public IEnumerable<TargetWithDegree> neighbourWithDegree {
		get {
			return slots.Where (b => b.target != null).Select(b => new TargetWithDegree() {
				target = b.target,
				degree = b.bond.degree,
			});
		}
	}

	
	public Chemistry.Element elem;

	[SerializeField]
	Bond[] slots;

	[SerializeField]
	Vector3[] geometry;
	//public GameObject visuals;
}
