using UnityEngine;
using System.Collections;

public class SetAtomColor : MonoBehaviour {

	public int AtomicNumber;

	void Start() {
		Reset ();
	}

	void OnValidate() {
		var c = ColouredBall.GetColor ((uint)AtomicNumber);
		if (renderer != null && renderer.sharedMaterial != null) {
		//	renderer.material = Instantiate(renderer.sharedMaterial) as Material;
			renderer.sharedMaterial.color = c;
		}
	}

	public void Reset() {
		var c = ColouredBall.GetColor ((uint)AtomicNumber);
		if (renderer != null) {
			renderer.material.color = c;
		}
	}

	public void Highlight() {
		if (renderer != null) {
			renderer.material.color = Settings.HighlightColor;
		}
	}

	public GameObject InstantiateDraggableAtom(string inputName) {
		GameObject result = null;
		System.Action finished = () => {
			if (result != null) Destroy (result);
		};
		var selected = SelectedItem.Setup (AtomicNumber, transform.parent.gameObject, finished, inputName);
		result = selected.gameObject;
		return result;
	}
}
