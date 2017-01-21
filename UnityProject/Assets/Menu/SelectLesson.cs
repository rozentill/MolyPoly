using UnityEngine;
using System.Collections;
using System.Linq;
public class SelectLesson : MonoBehaviour {
	
	public UnityEngine.UI.Text[] buttonTexts;

	public delegate void LessonChosenListener(int lessonID);

	public event LessonChosenListener LessonChosen;

	public static ModuleData.Module module;

	public void ChooseLesson(int lessonID) {
		LessonChosen (lessonID);
	}

	public void SetModule(string moduleID) {
		if (ModuleData.modules.TryGetValue (moduleID, out module)) {
			Debug.Log ("Loaded module " + module.name);
			var names = module.lessons.Select (lesson => lesson.name).ToArray ();

			for (int i = 0; i < buttonTexts.Length; i++) {
					buttonTexts [i].text = names [i];
			}
		}
	}

	public UnityEngine.UI.Button back;
	public UnityEngine.UI.Button exit;
}
