using UnityEngine;
using System.Collections.Generic;

public class UndoRedo : MonoBehaviour {

	private GameObject dumpster;

	void Awake () {
		past = new Stack<GameObject> ();
		future = new Stack<GameObject> ();
		dumpster = new GameObject ("UndoHistory");
		dumpster.transform.SetParent (transform);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool canUndo {
		get { return past.Count > 0; }
	}

	public bool canRedo {
		get { return future.Count > 0; }
	}

	public void Record(GameObject state) {
		var save = (GameObject) GameObject.Instantiate (state);
		save.SetActive (false);
		save.transform.SetParent (dumpster.transform, false);
		past.Push (save);

		ClearFutureHistory ();
	}

	public void ClearFutureHistory() {
		foreach (var o in future) {
			Destroy(o);
		}
		future.Clear ();
	}

	public void ClearUndoHistory() {
		foreach (var o in past) {
			Destroy(o);
		}
		past.Clear ();
	}


	public GameObject Undo(GameObject current) {
		current.SetActive (false);
		current.transform.SetParent (dumpster.transform, false);
		future.Push (current);
		var result = past.Pop ();
		result.SetActive (true);
		return result;
	}

	public GameObject Redo(GameObject current) {
		current.SetActive (false);
		current.transform.SetParent (dumpster.transform, false);
		past.Push (current);
		var result = future.Pop ();
		result.SetActive (true);
		return result;
	}
	
	Stack<GameObject> past;
	Stack<GameObject> future;
}
