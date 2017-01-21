using UnityEngine;
using System.Collections;

public class DragInputController : MonoBehaviour {

	private Draggable draggable;
	public string AxisName = Settings.MouseRotationButton;

	// Use this for initialization
	void Start() {
		draggable = GetComponent<Draggable> ();
		Setup (draggable, AxisName);
	}

	public static void Setup(Draggable draggable, string AxisName) {

		Vector3 origin = Vector3.zero;

		draggable.Dragging.OnEnter += delegate {
			origin = Input.mousePosition;
		};
		draggable.Dragging.Update += delegate {
			draggable.distance = origin - Input.mousePosition;
		};
		draggable.Dragging.OnExit += delegate {
						draggable.distance = Vector3.zero; 
				};
		
		draggable.isDragging = () => Input.GetAxis(AxisName) > 0;
	}

	
}
