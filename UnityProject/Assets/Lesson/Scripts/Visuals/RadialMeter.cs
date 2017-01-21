using UnityEngine;
using System.Collections;

public interface MeterTimer {
	void DoTick(bool increasing);
	void AddTick();
	void RemoveTick();
	bool ready { get; }
	float value { get; set; }
}

public class EmptyMeter : MeterTimer {

	public const float DefaultTimeToSelect = 1.0f;
	public EmptyMeter(float TimeToSelect = DefaultTimeToSelect) {
		this.TimeToSelect = TimeToSelect;
	}
	private readonly float TimeToSelect;
	private float hiddenValue = 0f;
	
	public void DoTick(bool increasing) {
		if (increasing)
			AddTick ();
		else
			RemoveTick ();
	}
	
	public float value {
		set { hiddenValue = Mathf.Clamp01 (value); }
		get { return hiddenValue; }
	}
	
	public void AddTick() {
		value += Time.deltaTime / TimeToSelect;
	}
	
	public void RemoveTick() {
		value -= Time.deltaTime / TimeToSelect;
	}
	
	public bool ready {
		get {
			return hiddenValue >= 1.0f;
		}
	}
}

public class RadialMeter : MonoBehaviour, MeterTimer {

	//public delegate float RecieveValue();
	//public RecieveValue valueDelegate;

	public const float TimeToSelect = EmptyMeter.DefaultTimeToSelect;
	private float hiddenValue = 0f;
	private UnityEngine.UI.Image image;

	void Start() {
		image = GetComponent<UnityEngine.UI.Image> ();	
	}

	void Update() {
		image.fillAmount = value;
	}

	public void DoTick(bool increasing) {
		if (increasing)
						AddTick ();
				else
						RemoveTick ();
	}

	public float value {
		set { hiddenValue = Mathf.Clamp01 (value); }
		get { return hiddenValue; }
	}
	
	public void AddTick() {
		value += Time.deltaTime / TimeToSelect;
	}
	
	public void RemoveTick() {
		value -= Time.deltaTime / TimeToSelect;
	}
	
	public bool ready {
		get {
			return hiddenValue >= 1.0f;
		}
	}
}