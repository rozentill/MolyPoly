MolyPoly Unity Project Guide.

File structure:

tools
	Programs for building application runtime data structures. (Uses OpenBabel to generate loadable XML files that describe molecules.)

doc
	Information about program architecture.
	Note: currently out of date.
	
UnityProject
	Directory containing the Unity project files.
	
UnityProject/Assets/start.unity
	The login scene for the project; a good place to start.

UnityProject/Assets/LeapMotion
	The LeapMotion SDK - mostly (except for Tommy's changes) not original code.

UnityProject/Assets/Plugins
	The LeapMotion binaries.
	
***IMPORTANT***
UnityProject/Assets/Lesson
	This folder contains entirely original assets. This is the main folder for the actual display and manipulation of the 3d-scene.
	
UnityProject/Assets/Menu
	This folder contains assets used to display menus for the purpose of login and lesson selection.

UnityProject/Assets/Utility
	This folder contains small convenience code used across the project.
	
UnityProject/Assets/Session
	This folder contains code used to manage the user session, including logging and username checking.
	
---------------------------------------------------------
***Description of start.unity scene***
This scene should only contains two active objects; the EventSystem object and the ApplicationManager object.
The ApplicationManager is responsible for loading the login menu and transitioning between interfaces.
This scene may contain an instance of the LessonUI prefab - this is NOT the instance that will be used in the final build; the actual LessonUI should be created by the ApplicationManager. Before doing a build, disable the LessonUI object and enable the Application manager.

***How interface transitions work***
Interface transitions are done by loading prefabs, rather than separate scenes.
Therefore, if a change is made to the project heirachy, the appropriate prefab needs to be updated.
To see a list of top-level prefabs that need to be updated, look in the file: UnityProject/Assets/Session/Load.cs
Other prefabs that are relevant to LeapMotion HandController are found in the UnityProject/Assets/Lesson folder.

***Mouse vs Leap***
Use the checkbox on the HandModeChooser object to select whether to enable Leap or Mouse controls.

***More about the "Lesson/Scripts" folder***
Interactions:
    SelectedItem, DraggableAtom:
    Components for controlling the attach interaction & the rebonding, detach interactions (respectively)

    Wallbounds, HandModeChooser MolyPolyGrabHand & Mouse scripts
    Mouse & Leap Motion control code. 

Model:
    EditAtom, EditBond, EditMolecule:
    Data structures (& some display code) for keeping track of the molecule state.
    

***Usernames & Mysql***

***Logging***


