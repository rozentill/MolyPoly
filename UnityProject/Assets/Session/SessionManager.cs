//Define this to disable sessions, (for testing purposes.)
//#define UseDummySession

using UnityEngine;
using System.Collections;

/* A simple singleton for finding the session object. */
public static class SessionManager {

	private static Session p_session;


	private const bool UseDummySession = false;

	public static Session session {
		get {
			if(p_session == null) {

#if UseDummySession
				p_session = new DummySession();
#else
				p_session = new RealSession();
#endif

			}
			
			return p_session;
		}
	}
}
