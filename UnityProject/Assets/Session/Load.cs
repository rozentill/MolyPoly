using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;

	public static class Load  {

		public static GameObject ColouredBall;
		public static GameObject LoginUI;
		public static GameObject SelectUI;
		public static GameObject SelectLessonUI;
		public static GameObject LessonUI;
		public static GameObject ColouredCylinder;
		public static GameObject DoubleCylinder;
		public static GameObject TripleCylinder;
		public static GameObject BondLineIndicator;
		
		private static Dictionary<string, string> Strings;

		public const string HDirectory = "H:/.MolyPoly/";
		public const string PDirectory = "P:/.MolyPoly/";
		public const string TempDirectory = "C:/Temp/.MolyPoly/";
	    public const string MacDirectory = "./H/MolyPoly/";
		public const string persistentDataPath = MacDirectory;

		public const string UserdataFilename = persistentDataPath + "/PersistentData.xml";

		public static string GetText(string key) {
			string output;

			if(!Strings.TryGetValue(key, out output)) {
				Debug.LogWarning("Warning: GetText key not initialised: " + key);
				return key;
			}
			else
				return output;
		}

		static Load() {

			ColouredBall 		= Resources.Load<GameObject> ("ColouredBall");
			LoginUI 			= Resources.Load<GameObject> ("LoginUI");
			SelectUI 			= Resources.Load<GameObject> ("SelectUI");
			SelectLessonUI		= Resources.Load<GameObject> ("SelectLessonUI");
			LessonUI 			= Resources.Load<GameObject> ("LessonUI");
			ColouredCylinder 	= Resources.Load<GameObject> ("ColouredCylinder");
			DoubleCylinder 		= Resources.Load<GameObject> ("DoubleCylinder");
			TripleCylinder 		= Resources.Load<GameObject> ("TripleCylinder");
			BondLineIndicator 	= Resources.Load<GameObject> ("BondLineIndicator");

			Strings = new Dictionary<string, string> () {
				{"TITLE",				"MolyPoly2"},
				{"STUDENT_ID_PROMPT",	"Please enter your Student ID:"},
				{"SEARCH_PROMPT",		"Search for a lesson:"},
				{"BAD_LOGIN_ATTEMPT",	"Sorry, only certain approved ID codes are allowed. " +
										"Please ask your teacher to be added or try again."},
				{"SEARCH_EMPTY",		"<b>No results.</b>\nPlease try another search string."},
				{"STUDENTFILE",			Application.dataPath + "/Configuration/Students.txt"},
				{"LESSONFILE",			Application.dataPath + "/Configuration/Lessons.xml"},
				{"PLEASE_WAIT", 		"Downloading student database... (There may be a problem with your internet connection.)"},
				//{"USERDATA_FILENAME",	persistentDataPath + "/UserData.xml"},
				{"LOGDIRECTORY",		persistentDataPath},
				{"LOGIN_WAIT", "Please wait while the user list is downloaded..."},
			};
		}

		/*

		public const string StringsFile = "config/strings.txt";
		private static Dictionary<string, string> LoadStrings() {
			var result = new Dictionary<string, string> ();
			var regex = new Regex (@"^(?<key>\w+)\s+(?<content>.*)$", RegexOptions.Multiline);

			DoForEachMatch(StringsFile, regex, match => {
				var key = match.Groups["key"].Value;
				var content = match.Groups["content"].Value;
				result[key] = content;
			});

			return result;
		}*/
		/*
		public static void DoForEachMatch(string input, Regex regex, Action<Match> action) {
			var matches = regex.Matches (input);
			foreach (Match match in matches) {
					action (match);
			}
		}*/

		public static GameObject InstantiateWithParent(GameObject prototype, Transform parent = null) {
			var r = (GameObject) GameObject.Instantiate (prototype);
			r.transform.SetParent(parent, false);
			return r;
		}

		public static T InstantiatePrefab<T>(string name) where T : Component {
			var obj = (GameObject) GameObject.Instantiate (Resources.Load<GameObject> (name));
			return obj.GetComponent<T> ();
		}


	}
