using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;

public static class PersistentDataFile {

	const string UserdataFilename = Load.UserdataFilename;

	[Serializable()]
	public struct UserData {
		public int UserID;
		public string Username;
		public List<int> modules;
		public int GroupNumber;
	}

	/* Overwrites any existing data */
	public static void write(UserData data) {
		Debug.Log("Writing userdata at: " + UserdataFilename);
		var writer = new XmlSerializer(typeof(UserData));
		using (var file = new StreamWriter(UserdataFilename, false))
		{
			writer.Serialize(file, data);
		}
	}

	/* Only read from the file if it exist.
		 * It is assumed that if it exists, it will be in the corrent format. (No tampering)
		 */
	public static bool TryRead(out UserData data) {
		if (System.IO.File.Exists (UserdataFilename)) {
			Debug.Log("Reading userdata at: " + UserdataFilename);
			var reader = new XmlSerializer(typeof(UserData));
			using(var file = new StreamReader(UserdataFilename)) {
				data = (UserData) reader.Deserialize(file);
			}			
			return true;
		}
		data = new UserData () {
			Username = null,
		};
		return false;
	}


}
