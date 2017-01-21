using System;
using System.Collections.Generic;
/* A Finite State Machine Framework.
 * Author: Phillip Warren 24/03/2015
 */

namespace FSM {


	public delegate bool PredicateAction();
	
	public class PredicateSwitch {
		public PredicateSwitch(PredicateAction chainWith) {
			enabled = () => ConsumeSwitch () || chainWith ();
		}
		public PredicateSwitch() {
			enabled = ConsumeSwitch;
		}

		public readonly PredicateAction enabled;

		private bool on = false;
		private bool ConsumeSwitch() {
			var result = on;
			on = false;
			return result;
		}
		public void SetSwitch() {
			on = true;
		}
	}

	
	/* FSMNode is designed to be extended by using closures and function composition:
	* create a new FSMNode, optionally provide actions.
	* (users of FSMNodes may assume some action is present, so NoOps are provided by default.)
	* to compose FSMNode actions, use += operator. (Like node.OnEnter += delegate { <some code> };)
	*/
	public sealed class FSMNode {

		public event Action OnEnter;
		public event Action OnExit;
		public event Action Update;

		public void AddTransition(FSMNode target, PredicateAction condition) {
			if (transitions == null)
								transitions = new List<FSMTransition> ();

			transitions.Add (new FSMTransition () {
				condition = condition,
				target = target,
			});
		}

		public void Enter() {
			if(OnEnter != null) OnEnter ();
		}

		public void Exit() {
			if(OnExit != null) OnExit ();
		}

		public FSMNode Continue() {
			if(Update != null) Update ();

			foreach (var transition in transitions) {
				if(transition.condition()) {
					Exit ();
					var result = transition.target;
					result.Enter();
					return result;
				}
			}

			return this;
		}

		private List<FSMTransition> transitions;
	}
	
	struct FSMTransition
	{
		public PredicateAction condition;
		public FSMNode target;
	}

	public class Machine
	{
		private FSMNode _current;

		public FSMNode current {
			get { return _current; }
		}

		public void Start() {
			_current.Enter ();
		}

		public void Stop() {
			if (_current != null) {
				_current.Exit ();
				_current = null;
			}
		}

		public void transition(FSMNode from, FSMNode to, PredicateAction condition) {
			from.AddTransition (to, condition);
		}
				
		public void Update() {
			if (_current == null)
								return;

			_current = _current.Continue ();
		}

		public Machine (FSMNode start)
		{
			_current = start;
		}
	}

}

