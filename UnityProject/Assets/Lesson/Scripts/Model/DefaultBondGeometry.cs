using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public static class DefaultBondGeometry {

	private const float scale = 1.2f;

	/* The basic "ideal" shapes. */								//BD	NBD		ED = BD + NBD
	public static readonly Vector3[] Single;					//1		0			
	public static readonly Vector3[] Linear;					//2		0
	public static readonly Vector3[] TrigonalPlanar;			//3		0
	public static readonly Vector3[] Tetrahedral;				//4		0
	public static readonly Vector3[] TrigonalBypyramidal;		//5		0
	public static readonly Vector3[] Octahedral;				//6		0

	/* Next, the shapes which are a subset of vertices given above. */
	public static readonly Vector3[] Bent3;						//2		1		3
	public static readonly Vector3[] TrigonalPyramidal;			//3		1		4
	public static readonly Vector3[] Bent4;						//2		2		4
	public static readonly Vector3[] Seesaw;					//4		1		5
	public static readonly Vector3[] Tshaped;					//3		2		5
	public static readonly Vector3[] Linear5;					//2		3		5
	public static readonly Vector3[] SquarePyramidal;			//5		1		6
	public static readonly Vector3[] SquarePlanar;				//4		2		6

	//public static Dictionary<Chemistry.Element, uint> ElectronDomains;

	static Vector3[] Scale(this Vector3[] input, float extra = 1f) {
		return input.Select (v => extra * scale * v).ToArray();
	}

	static DefaultBondGeometry() {

		var rot120 = Quaternion.AngleAxis (120, Vector2.up);
		var rot90 = Quaternion.AngleAxis (90, Vector3.up);
		var rot180 = rot90 * rot90;
		var rot270 = rot90 * rot180;
		
		/* From wikipedia:
	 * The following Cartesian coordinates define the four vertices of a tetrahedron with edge length 2, centered at the origin:
	 * (±1, 0, −1/√2)
	 * (0, ±1, 1/√2)
	 */
		var invsqrt2 = 1 / Mathf.Sqrt (2);

		Tetrahedral = new Vector3[] {
			new Vector3 (1.0f, 0.0f, -invsqrt2),
			new Vector3 (-1.0f, 0.0f, -invsqrt2),
			new Vector3 (0, 1.0f, invsqrt2),
			new Vector3 (0, -1.0f, invsqrt2),
		}.Scale ();

		Linear = new Vector3[] { Vector3.up, Vector3.down, }.Scale ();
		TrigonalPlanar = new Vector3[] { Vector3.left, rot120 * Vector3.left, rot120 * rot120 * Vector3.left}.Scale();
		TrigonalBypyramidal = Linear.Concat (TrigonalPlanar).ToArray ();
		Octahedral = Linear.Concat (new Vector3[] {
						Vector3.left,
						rot90 * Vector3.left,
						rot180 * Vector3.left,
						rot270 * Vector3.left,
				}.Scale ()).ToArray ();

		Bent3 = TrigonalPlanar.Take (2).ToArray ();
		TrigonalPyramidal = Tetrahedral.Take (3).ToArray ();
		Bent4 = Tetrahedral.Take (2).ToArray ();
		Seesaw = TrigonalBypyramidal.Except(TrigonalPlanar.Take(1)).ToArray();
		Tshaped = TrigonalBypyramidal.Except(TrigonalPlanar.Take(2)).ToArray();
		Linear5 = Linear;
		SquarePyramidal = Octahedral.Except (Linear.Take (1)).ToArray ();
		SquarePlanar = Octahedral.Except (Linear).ToArray ();

		Single = new Vector3[] {
			Vector3.up,
		}.Scale ();
		/*
		ElectronDomains = new Dictionary<Chemistry.Element, uint> () {
			{Chemistry.Element.Carbon, 4},
			{Chemistry.Element.Nitrogen, 4}
		};*/
	}

}
