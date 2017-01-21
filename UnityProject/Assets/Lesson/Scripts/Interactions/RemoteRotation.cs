using UnityEngine;
using System.Collections;

public class RemoteRotation : MonoBehaviour {

	public Transform remote;
	public Transform indicatorTransform;
	private bool increase_radius=false;
	private Lesson lesson;
	public static bool locked =false;
	public static bool stopGrab = false;
	//private bool refresh=false;
	//private Coroutine co;
	//rotation1 leaprotation = rotation1.leapRotation;

	void Start () {
		lesson = FindObjectOfType<Lesson>();

		indicatorTransform = transform.FindChild("modelScaleIndicator");
		//indicatorTransform.localScale = Vector3.one * 10;
		//timer = new EmptyMeter (3.0f);
	}

	IEnumerator waitforlocked()
	{
		yield return new WaitForSeconds(3.0f);

		locked = false;
		stopGrab = false;
		//Debug.Log("unlocked");
	}

	void Update () {
		var leaprotation = rotation1.leapRotation;

		//stopped grabbing
		if (!locked) {
			if (stopGrab){
				locked = true;
				//Debug.Log ("locked");
				StartCoroutine (waitforlocked ());
			}
		}

		//if timer is still running, keep big circle
		if (!locked) 
		{
			increase_radius = false;
		}

		if(leaprotation != null && leaprotation.rotatingThisFrame) {
			remote.rotation = transform.rotation;
			increase_radius = true;
		}
		if (stopGrab) {
			remote.rotation = transform.rotation;
			increase_radius = true;
				}

			

		if(lesson != null) {
			var molecule = lesson.GetMolecule();

			float distance = 1.0f;
			foreach(var atom in molecule.atoms) {
				var next_distance = Vector3.Distance(atom.transform.position, remote.transform.position);
				if(next_distance > distance)
					distance = next_distance;
			}




			var collider = GetComponent<SphereCollider>();

			collider.radius = Mathf.Max(distance, 7.0f) * (increase_radius ? Settings.IncreaseRotationFactor : 1.0f);
			indicatorTransform.localScale = increase_radius ? Vector3.one * 22 : Vector3.one * collider.radius*2;
			//indicatorTransform.localScale = Vector3.one * collider.radius*2;

			if (leaprotation != null && leaprotation.inside == true)
			{
				indicatorTransform.gameObject.SetActive(true);
			}
			else
			{
				indicatorTransform.gameObject.SetActive(false);
			}
			if(locked)
			{
				indicatorTransform.gameObject.SetActive(true);
			}

		}
	}
}
