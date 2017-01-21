using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* ColouredBall represents a sphere which determines its own size and colour at runtime.
 * 
 */
static class ColouredBallExternal {
		
	public static void SetGhost(this GameObject obj, float a) {
		var mat = obj.GetComponentInChildren<Renderer> ().material;
		if (mat != null) {
			mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, a);
		}
	}

	public static void SetFull(this GameObject obj) {
		var mat = obj.GetComponentInChildren<Renderer> ().material;
		if (mat != null) {
			mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1.0f);
		}
	}

	public static bool Blink(this float t, float frequency) {
		return Mathf.FloorToInt (2 * t * frequency) % 2 == 0;
	}
}

[System.Serializable]
public class ColouredBall : MonoBehaviour {

	public static readonly Dictionary<int, ElementDisplayProperty> displayConfiguration;
	
	[SerializeField]
	private ElementDisplayProperty display;
	
	[System.Serializable]
	public struct ElementDisplayProperty
	{
		public Color c;
		public float size;
		public string name;
		
		public ElementDisplayProperty(Color c, float s, string n) {
			this.c = c;
			this.size = s;
			this.name = n;
		}
	}



	static ColouredBall() {
		var orange = new Color (0.976f, 0.451f, 0.240f);
		var silver = new Color (0.773f, 0.788f, 0.708f);
		var purple = new Color (0.494f, 0.118f, 0.612f);
		var brown =  new Color (0.396f, 0.216f, 0.000f);
		//var black = new Color (0.320f, 0.320f, 0.320f);
		
		var c = new Dictionary<int, ElementDisplayProperty> (12);
		
		c.Add(-1,	new ElementDisplayProperty(silver,			1.0f,	"Metal"));
		c.Add(0,	new ElementDisplayProperty(Color.magenta,	1.0f,	"Default"));
		c.Add(1,	new ElementDisplayProperty(Color.white,		0.6f,	"Hydrogen"));
		c.Add(5,	new ElementDisplayProperty(brown,			1.0f,	"Boron"));
		c.Add(6,	new ElementDisplayProperty(Color.black,		1.0f,	"Carbon"));
		c.Add(7,	new ElementDisplayProperty(Color.blue,		1.0f,	"Nitrogen"));
		c.Add(8,	new ElementDisplayProperty(Color.red,		1.0f,	"Oxygen"));
		c.Add(9,	new ElementDisplayProperty(Color.green,		1.0f,	"Fluorine"));
		c.Add(15,	new ElementDisplayProperty(purple,			1.0f,	"Phosphorus"));
		c.Add(16,	new ElementDisplayProperty(Color.yellow,	1.0f,	"Sulfur"));
		c.Add(17,	new ElementDisplayProperty(Color.green,		1.0f,	"Chlorine"));
		c.Add(35,	new ElementDisplayProperty(orange,			1.0f,	"Bromine"));
		
		displayConfiguration = c;

	}


	public static ElementDisplayProperty GetDisplay(int atomicnumber) {
			ElementDisplayProperty displayProperty;
			
			if (!displayConfiguration.TryGetValue(atomicnumber, out displayProperty)) {
				if( Chemistry.isMetal((uint)atomicnumber) ) {
					displayProperty = displayConfiguration[-1];
				} else {
					displayProperty = displayConfiguration[0];
				}
			}

			return displayProperty;
	}
	public static Color GetColor(uint atomicnumber) {
			return GetDisplay ((int)atomicnumber).c;
	}

	public void ResetColor() {
		renderer.material.color = display.c;
	}
			
	public void SetColor(Color c) {
		GetComponent<Renderer> ().material.color = c;
	}
	
	public void SetDefaultColor() {
		SetColor (display.c);
	}
	
	public void Refresh() {
		GetComponent<Renderer>().material.color = display.c;
		transform.localScale = Vector3.one * display.size;
		gameObject.name = display.name;
	}
	

	public struct BallSetupData
	{
		public uint AtomicNumber;
		public Vector3 position;
	}
	public static GameObject Setup(uint atom, GameObject parent) {
		return Setup (new BallSetupData () {
				AtomicNumber = atom,
				position = Vector3.zero,
			}, parent);
	}
	public static GameObject Setup(BallSetupData data, GameObject parent) {
			
			ElementDisplayProperty displayProperty = GetDisplay ((int)data.AtomicNumber);

			var obj = Load.InstantiateWithParent (Load.ColouredBall, parent.transform);
			obj.transform.position = data.position;
			obj.GetComponent<ColouredBall> ().display = displayProperty;
			obj.GetComponent<ColouredBall> ().Refresh ();
			//let spec reference the gameobject so the bond can dynamically point to it.
			//spec.attach (obj);

			return obj;
	}
	

}