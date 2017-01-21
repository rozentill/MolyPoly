using UnityEngine;
using System.Collections;

public class SelectModule : MonoBehaviour {

	public delegate void ModuleChosenListener(string moduleID);
	public Transform ButtonContainer;
	
	public event ModuleChosenListener ModuleChosen;

	public void ChooseModule(string moduleID) {
		ModuleChosen (moduleID);
	}

	void Start() {
		int i = 1;
		foreach (Transform child in ButtonContainer) {
			var button = child.GetComponent<UnityEngine.UI.Button>();
			if(button != null) {
				button.interactable = SessionManager.session.ValidModule(i);
			}
			i++;
		}
	}

	public UnityEngine.UI.Button back;
	public UnityEngine.UI.Button exit;
}
