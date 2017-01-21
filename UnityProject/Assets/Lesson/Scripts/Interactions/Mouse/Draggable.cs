using UnityEngine;
using System.Collections;

public class Draggable : MonoBehaviour {

	[HideInInspector()]
	public Vector3 distance;

	public FSM.FSMNode Dragging;
	public FSM.FSMNode NotDragging;

	private FSM.Machine machine;

	//Assign a predicate to this variable to define when dragging is enabled.
	public FSM.PredicateAction isDragging;

	public Draggable() {

		
		NotDragging = new FSM.FSMNode ();
		Dragging = new FSM.FSMNode ();

		machine = new FSM.Machine (NotDragging);

		//A default predicate to avoid null ptr exceptions.
		isDragging = () => false;

		//Predicate is thunked so that the dragging condition may change dynamically.
		machine.transition (Dragging, NotDragging, () => !isDragging());
		machine.transition (NotDragging, Dragging, () => isDragging());
	}
	
	void Start() {
		machine.Start ();
	}

	void Update() {
		machine.Update ();
	}
}
