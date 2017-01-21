using UnityEngine;

/* A dummy session is a session that follows the basic sematics but doesn't do any logging.
 * it is useful for the purpose of testing code when we don't care about sessions.
 * #define UseDummySession in SessionManager to enable this.
 */
public class DummySession : Session {
	#region Session implementation

	public bool ValidModule (int index)
	{
		return true;
	}

	#endregion

	#region Session implementation
	public bool TryResumeLogin ()
	{
		return false;
	}
	#endregion

	private string p_username;
	
	public string GetErrorMessage() { return ""; }
	
	//This will return null if we are not logged in.
	//If a username exists, we are logged in and logging is enabled.
	public string username { get { return p_username; } }

	public bool isLoggedIn { get { return p_username != null; } }
	
	//This will return false if the username is invalid, or if we are already logged in.
	//Otherwise it will return true, and log us in.
	//Once we are logged in, logging is enabled.
	public SessionLoginResult TryLogin(string username) { 
		p_username = username;
		return SessionLoginResult.Success;
	}
	
	//This will log us out and disable logging.
	//If we are already logged out, this is a no-op.
	public void Logout() {
		p_username = null;
	}
	
	//Saves a message to the log. If logging is disabled,
	//Then this does nothing.
	public void Log(string type, object data) {
		if (Debug.isDebugBuild) {
			Debug.Log(string.Format("{0} {1} {2}", p_username, type, data));
		}
	}
}
