using UnityEngine;
using System.Collections;

public class WallBounds : MonoBehaviour {
	
	public Bounds GetBoundary() {
		return collider.bounds;
	}
}
