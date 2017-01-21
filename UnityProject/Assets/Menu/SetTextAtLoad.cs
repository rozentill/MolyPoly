using UnityEngine;
using System.Collections;

public class SetTextAtLoad : MonoBehaviour {

	private UnityEngine.UI.Text field;

	void Start () {
		field = GetComponent<UnityEngine.UI.Text> ();
		var key = field.text;
		field.text = Load.GetText (key);

		//We only want this to happen once.
		Destroy (this);
	}

}
