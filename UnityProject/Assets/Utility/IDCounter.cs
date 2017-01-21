using UnityEngine;
using System.Collections.Generic;

public class IDCounter {
	private static Dictionary<string, int> nextID;

	static IDCounter() {
		nextID = new Dictionary<string, int> ();
	}
	public static int getID(string space) {
		if (!nextID.ContainsKey (space)) nextID.Add (space, 0);
		return nextID [space]++;
	}
}
