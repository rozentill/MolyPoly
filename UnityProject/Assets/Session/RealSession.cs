using UnityEngine;

/* A real session is a session that implements all the required features for this application;
 * 1) Download a list of acceptable usernames and use this data to check which users are valid.
 * 2) Once logged in, stream log events to disk.
 * 3) On logout, upload saved file to a remote location;
 * 4) Upload any unsaved log files to the remote location.
 * 
 * This class is basically a proxy interface to MySQLManager; it implements the semantics of a session,
 * but uses methods in MySQLManager to do so.
 */
public class RealSession : Session {
	#region Session implementation

	public bool ValidModule (int index)
	{
		return userdata.modules != null && userdata.modules.Contains (index);
	}

	public bool TryResumeLogin ()
	{
		if (PersistentDataFile.TryRead (out userdata)) {
			if(db.ResumeLogin(ref userdata)) PersistentDataFile.write(userdata);
			//If the data file was successfully found, we still need to update the module list.
			db.OpenLogFile();
			return true;
		}
		return false;
	}
	#endregion

	private MySQLManager db;

	public string GetErrorMessage() {
		return db.databaseError;
	}

	public RealSession() {
	
		//Set up the database;
		db = new MySQLManager ();

	}
	
	private PersistentDataFile.UserData userdata;
	
	//This will return null if we are not logged in.
	//If a username exists, we are logged in and logging is enabled.
	public string username { get { return userdata.Username; } }
	public bool isLoggedIn { get { return username != null; } }

	//This will return false if the username is invalid, or if we are already logged in.
	//Otherwise it will return true, and log us in.
	//Once we are logged in, logging is enabled.
	public SessionLoginResult TryLogin(string username) {
		if (isLoggedIn)
			return SessionLoginResult.AlreadyLoggedIn;
		else {
			var status = db.TryLogin (username, ref userdata);
			if(status == SessionLoginResult.Success) {
				db.OpenLogFile();
				PersistentDataFile.write(userdata);
			}
			return status;
		}
	}
	
	//This will log us out and disable logging.
	//If we are already logged out, this is a no-op.
	public void Logout() {
		if (isLoggedIn) {
			userdata.Username = null;
			db.FinishLog();
		}
	}
	
	//Saves a message to the log. If logging is disabled,
	//Then this does nothing.
	public void Log(string name, object data) {
		if (isLoggedIn)
						db.AddLogMessage (name, data);
				else
						Debug.LogWarning ("Log message without username: " + name + " " + data);
	}
	
}