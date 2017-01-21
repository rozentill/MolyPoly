using UnityEngine;
using System.Collections;
using System;

public class MenuStateMachine : MonoBehaviour {


	private FSM.Machine fsm;

	private GameObject ActiveRoot;

	public bool SkipLogin = false;

	void Start () {

		/* The top level application is an FSM with three states:
		 * 1: The login screen
		 * 2: The lesson select screen
		 * 3: The lesson screen.
		 * 4: Application closing.
		 */

		var LoginScreen = new FSM.FSMNode ();
		var ModuleSelectScreen = new FSM.FSMNode ();
		var LessonSelectScreen = new FSM.FSMNode ();

		var LessonInProgress = new FSM.FSMNode ();
		var CloseApplication = new FSM.FSMNode();

		/**********************************************************
		 * Specify the initial FSM State;
		 **********************************************************/

		SkipLogin = SessionManager.session.TryResumeLogin();

		if(SkipLogin) fsm = new FSM.Machine(ModuleSelectScreen);
		else fsm = new FSM.Machine(LoginScreen);


		/**********************************************************
		 * Conditions to transfer betweeen states*
		 **********************************************************/
	
		FSM.PredicateAction LoggedIn = delegate {
			return SessionManager.session.isLoggedIn;
		};

		var CloseSwitch = new FSM.PredicateSwitch ();
		var BackSwitch = new FSM.PredicateSwitch (() => Input.GetKeyDown(KeyCode.Escape));
		var ModuleChosenSwitch = new FSM.PredicateSwitch();
		var LessonChosenSwitch = new FSM.PredicateSwitch ();
		var FreeStyleBackSwitch = new FSM.PredicateSwitch (() => Input.GetKeyDown(KeyCode.Escape));

		//				From		To					Condition
		fsm.transition(LoginScreen, ModuleSelectScreen, LoggedIn);

		fsm.transition(ModuleSelectScreen, LessonSelectScreen, ModuleChosenSwitch.enabled);
		fsm.transition (LessonSelectScreen, LessonInProgress, LessonChosenSwitch.enabled);


		fsm.transition(LoginScreen, CloseApplication, BackSwitch.enabled);
		fsm.transition(ModuleSelectScreen, CloseApplication, CloseSwitch.enabled);
		fsm.transition(LessonSelectScreen, CloseApplication, CloseSwitch.enabled);
		//fsm.transition(LessonSelectScreen, CloseApplication, ExitButton);

		fsm.transition (LessonSelectScreen, ModuleSelectScreen, BackSwitch.enabled);
		fsm.transition (LessonInProgress, LessonSelectScreen, BackSwitch.enabled);
		fsm.transition (LessonInProgress, CloseApplication, CloseSwitch.enabled);
		fsm.transition (LessonInProgress, ModuleSelectScreen, FreeStyleBackSwitch.enabled);

		/**********************************************************
		 * The actions to perform when we transfer between states.*
		 **********************************************************/

		Action ClearScreen = delegate { Destroy (ActiveRoot); };

		LoginScreen.OnEnter += delegate {
			//Make sure we're logged out each time we enter the login screen.
			if(LoggedIn()) {
			//	SessionManager.session.Log("log_out", null);
			//	SessionManager.session.Logout();
			}

			ActiveRoot = Load.InstantiateWithParent(Load.LoginUI);
		};
		LoginScreen.OnExit += delegate {
			if(LoggedIn()) {
				SessionManager.session.Log("log_in", null);
			}
		};

		string moduleID = "";
		int lessonID = 0;

		ModuleSelectScreen.OnEnter += delegate {
			ActiveRoot = Load.InstantiateWithParent (Load.SelectUI);
			var c = ActiveRoot.GetComponent<SelectModule>();
			c.ModuleChosen += delegate(string module) {
				ModuleChosenSwitch.SetSwitch();
				moduleID = module;
			};
			c.exit.onClick.AddListener(CloseSwitch.SetSwitch);

		};

		LessonSelectScreen.OnEnter += delegate {
			ActiveRoot = Load.InstantiateWithParent(Load.SelectLessonUI);
			var c = ActiveRoot.GetComponent<SelectLesson>();
			c.SetModule(moduleID);
			c.back.onClick.AddListener(BackSwitch.SetSwitch);
			c.exit.onClick.AddListener(CloseSwitch.SetSwitch);

			if (moduleID =="FreeStyle"){				
					SessionManager.session.Log("menu_select_module", moduleID);
					LessonChosenSwitch.SetSwitch();
					lessonID = 0;
				}else{
				c.LessonChosen += delegate(int lesson) {
				LessonChosenSwitch.SetSwitch();
				lessonID = lesson;
					};}
		};

		LessonInProgress.OnEnter += delegate {
			ActiveRoot = Load.InstantiateWithParent(Load.LessonUI);
			var c = ActiveRoot.GetComponent<Lesson>();
			ModuleData.Module module;
			if(ModuleData.modules.TryGetValue(moduleID, out module)) {
				c.SetAvailableAtoms(module.atoms);
				if(lessonID < module.lessons.Length) c.SetTargetMolecule(module.lessons[lessonID]);
				SessionManager.session.Log("menu_select_lesson", module.lessons[lessonID].name);
			}
			if (moduleID =="FreeStyle"){
				c.FreeStyleBackButton.onClick.AddListener(FreeStyleBackSwitch.SetSwitch);
				GameObject.Find("FreeStyle_BackButton2").SetActive(true);
				GameObject.Find("BackButton2").SetActive(false);
				GameObject.Find("ShowFeedbackButton").SetActive(false);
			}else{
				c.BackButton.onClick.AddListener(BackSwitch.SetSwitch);
				GameObject.Find("FreeStyle_BackButton2").SetActive(false);
				GameObject.Find("BackButton2").SetActive(true);
				GameObject.Find("ShowFeedbackButton").SetActive(true);
			}
			c.ExitButton.onClick.AddListener(CloseSwitch.SetSwitch);
			
		};



		LoginScreen.OnExit += ClearScreen;
		ModuleSelectScreen.OnExit += ClearScreen;
		LessonInProgress.OnExit += ClearScreen;
		LessonSelectScreen.OnExit += ClearScreen;

		CloseApplication.OnEnter += delegate {
			Application.Quit();
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#endif
		};

		/**********************************************************
		 * Initialise the fsm; calls the first OnEnter.
		 **********************************************************/

		fsm.Start();
	}

	void Update () {
		if(fsm != null) fsm.Update ();
	}

	void OnApplicationQuit() {
		SessionManager.session.Logout();
	}

}
