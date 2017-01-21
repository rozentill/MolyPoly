public enum SessionLoginResult {
	Success,
	AlreadyLoggedIn,
	BadUsername,
	PleaseWait,
	DatabaseError,
};

public interface Session {

	//This will return null if we are not logged in.
	//If a username exists, we are logged in and logging is enabled.
	string username { get; }

	//An alias for username != null;
	bool isLoggedIn { get; }

	bool TryResumeLogin();

	bool ValidModule(int index);

	//This will return false if the username is invalid, or if we are already logged in.
	//Otherwise it will return true, and log us in.
	//Once we are logged in, logging is enabled.
	SessionLoginResult TryLogin(string username);

	string GetErrorMessage();

	//This will log us out and disable logging.
	//If we are already logged out, this is a no-op.
	void Logout();

	//Saves a message to the log. If logging is disabled,
	//Then this does nothing.
	void Log(string name, object data);
}

