using UnityEngine;
using System.Collections;

public class Login : MonoBehaviour {

	public GameObject BadLoginPanel;

	private string waitingUsername;

	public void Submit(string username) {

		var status = SessionManager.session.TryLogin (username);
		if (status != SessionLoginResult.Success) {
			BadLoginPanel.SetActive(true);
			var text = BadLoginPanel.transform.FindChild("ResultText").GetComponent<UnityEngine.UI.Text>();

			switch(status) {
			case SessionLoginResult.PleaseWait:
				text.text = Load.GetText("LOGIN_WAIT");
				waitingUsername = username;
				return;
			case SessionLoginResult.BadUsername:
				text.text = Load.GetText("BAD_LOGIN_ATTEMPT");
				break;
			case SessionLoginResult.DatabaseError:
				text.text = SessionManager.session.GetErrorMessage();
				break;
			}
		}
		waitingUsername = null;
	}

	public void Edit(string s) {
		BadLoginPanel.SetActive(false);
		waitingUsername = null;
	}

	public void Update() {
		if (waitingUsername != null) {
			Submit(waitingUsername);
		}
	}

}
