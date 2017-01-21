using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public static class EditMoleculeExternalSearching {
	
	public static Plane fallbackPlane;

	static EditMoleculeExternalSearching() {
		fallbackPlane = new Plane (Vector3.back, Vector3.zero);
	}

	public static float DistanceToRay(this Vector3 point, Ray ray) {
		return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
	}
		
	public static Maybe<Vector3> CastPoint(this Plane plane, Ray ray) {
		float distance;
		if (plane.Raycast (ray, out distance)) {
			return new Just<Vector3> (ray.GetPoint (distance));
		} else
			return new None<Vector3>();
	}
	
	public static Maybe<T> MaybeFirst<T>(this IEnumerable<T> seq) {
		if (seq.Any ())
			return new Just<T> (seq.First ());
		else
			return new None<T> ();
	}
	
	public static Maybe<EditAtom> ClosestBondableAtom(this EditMolecule molecule, Func<EditAtom, float> distance) {
		return molecule.atoms
			.Select (a => new {atom = a, distance = distance(a)})
				.Where (p => 
				        p.distance < Settings.Threshold_Medium && //atom within threshold
				        p.atom.holes.Any ()) //atom has some holes
				        //p.atom.neighbours.Any (a => a.holes.Any ()) //atom has a neighbour with some holes. 
				.OrderBy (p => p.distance)
				.Select (p => p.atom)
				.MaybeFirst ();
	}

	public static Maybe<T> ClosestWithinThreshold<T>(this IEnumerable<T> Group, Func<T, float> distance, float threshold) {
		return (from a in Group
		let d = distance (a)
		where d < threshold
		orderby d
		select a).MaybeFirst ();
	}
	

	public static IOrderedEnumerable<EditMolecule.Hole> AttachmentCandidates(this Maybe<EditAtom> atom, Vector3 position) {
		return atom
			.Select (a => a.holes.OrderBy (x => Vector3.Distance(position, x.worldPosition)))
				.otherwise(Enumerable.Empty<EditMolecule.Hole>().OrderBy(x => 1));
	}


}
public class Lesson : MonoBehaviour {

	public GameObject root;

	private EditMolecule molecule;
	private Camera cam {  get { return Camera.main; } }
	
	public UnityEngine.UI.Button UndoButton;
	public UnityEngine.UI.Button RedoButton;
	public UnityEngine.UI.Button BackButton;
	public UnityEngine.UI.Text Title;
	public UnityEngine.UI.Text Error;
	public UnityEngine.UI.Button CheckMoleculeButton;
	public UnityEngine.UI.Button ResetButton;
	public UnityEngine.UI.Button ExitButton;
	public UnityEngine.UI.Button FreeStyleBackButton;

	private UndoRedo undoman;

	public void Undo() {
		if (undoman.canUndo) {

			SessionManager.session.Log ("model_edit_undo", null);

			molecule.gameObject.BroadcastMessage("CancelInteractions", "Undo", SendMessageOptions.DontRequireReceiver);
			var parent = root.transform.parent;
			root = undoman.Undo(molecule.gameObject);
			root.transform.SetParent(parent, false);
			molecule = root.GetComponent<EditMolecule>();
			molecule.gameObject.BroadcastMessage("CancelInteractions", "Undo", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void Redo() {
		if (undoman.canRedo) {

			SessionManager.session.Log ("model_edit_redo", null);

			molecule.gameObject.BroadcastMessage("CancelInteractions", "Redo", SendMessageOptions.DontRequireReceiver);
			var parent = root.transform.parent;
			root = undoman.Redo(molecule.gameObject);
			root.transform.SetParent(parent, false);
			molecule = root.GetComponent<EditMolecule>();
			molecule.gameObject.BroadcastMessage("CancelInteractions", "Redo", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void Reset(){
		if (undoman.canUndo){
			
			SessionManager.session.Log ("model_reset", null);
			
			do {
				Undo();
			} while (undoman.canUndo);
		}
	}


	void Start () {

		//The initial editable molecule needs to be constructed;
		//The process simply mimics the user interaction;

		molecule = EditMolecule.Create (root);
		undoman = gameObject.AddComponent<UndoRedo> ();

		UndoButton.onClick.AddListener (Undo);
		RedoButton.onClick.AddListener (Redo);
		ResetButton.onClick.AddListener(Reset);

		var origin = molecule.holes.Single ();
		molecule.UpgradeHole(origin, (uint)Chemistry.Element.Carbon);

		undoman.ClearUndoHistory ();

		errorPanelTimer = new EmptyMeter (Settings.ErrorPanelTimerCountdown);
		errorPanelTimer.value = 1;
	}

	public EditMolecule GetMolecule() {
		return molecule;
	}
	
	void Update() {

		UndoButton.interactable = undoman.canUndo;
		RedoButton.interactable = undoman.canRedo;
		ResetButton.interactable = undoman.canUndo;

		errorPanelTimer.AddTick ();
		Error.transform.parent.gameObject.SetActive (!errorPanelTimer.ready);
	}

	void UpdateErrorPanel (Maybe<string> message) {
		Error.text = message.otherwise ("The molecule is correct!");
		SessionManager.session.Log(Error.text, null);
	}

	private EmptyMeter errorPanelTimer;
	void ResetErrorPanelTimer() {
		errorPanelTimer.value = 0;
	}
	
	/*Set the target molecule of the lesson */
	public void SetTargetMolecule(ModuleData.Lesson lesson) {
		Title.text = lesson.name;
		Error.transform.parent.gameObject.SetActive (false);
		CheckMoleculeButton.onClick.AddListener (() => {
			Error.transform.parent.gameObject.SetActive (true);
			UpdateErrorPanel(lesson.run(molecule.atoms));
			ResetErrorPanelTimer();
		});
	}

	public void SetAvailableAtoms(Chemistry.Element[] atoms) {

	}
}
