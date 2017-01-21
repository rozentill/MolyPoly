
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Chemistry {

	public enum Element
	{
		Hydrogen 	= 1,
		Carbon 		= 6,
		Nitrogen 	= 7,
		Oxygen 		= 8,
	}
	
	private static readonly uint[] metals;

	static Chemistry() {
		metals = new uint[]{
			3,4,11,12,13,19,20,21,22,23,24,25,26,27,28,29,
			30,31,37,38,39,40,41,42,43,44,45,46,47,48,49,50,55,56,57,58,59,60,61,62,63,
			64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,87,88,89,90,91,
			92,93,94,95,96,97,98,99,100,101,102,103
		};
	}

	public static int WhichElement(string tag) {
		switch (tag) {
			case "element2": return 6; //Carbon
			case "element3": return 7; //Nitrogen
			case "element4": return 8; //Oxygen
			default:
			case "element1": return 1; //Hydrogen
		}
	}
	
	public static bool isMetal(uint AtomicNum) {
		//Check whether an atom is metal. Adapted from the latest OpenBabel source 16/02/2015
		return metals.Contains (AtomicNum);
	}

	public static IEnumerable<UnityEngine.Vector3> GetBondGeometry(Element elem) {
		return NieveBondGeometry (elem).AsEnumerable ();
	}

	/* This method doesn't maintain the ordering of vectors to provide the correct mapping after updating the bonds.
	 * This will give strange results when bond geometries are not symmetrical on some axes*/
	public static Vector3[] NieveBondGeometry(Chemistry.Element elem, int size = 0) {

		switch (elem) {
		case Chemistry.Element.Carbon:
			goto Carbon;
		case Chemistry.Element.Nitrogen:
			goto Nitrogen;
		case Chemistry.Element.Oxygen:
			goto Oxygen;
		default:
		case Chemistry.Element.Hydrogen:
			return DefaultBondGeometry.Single;
		}

		Carbon:
		switch (size) {
		default:
			case 4: return DefaultBondGeometry.Tetrahedral;
			case 3: return DefaultBondGeometry.TrigonalPlanar;
			case 2: return DefaultBondGeometry.Linear;
		}
	
		Nitrogen:
		switch (size) {
			default:
			case 3: return DefaultBondGeometry.TrigonalPyramidal;
			case 2: return DefaultBondGeometry.Bent3;
			case 1: return DefaultBondGeometry.Single;
		}

		Oxygen:
		switch (size) {
			default:
			case 2: return DefaultBondGeometry.Bent3;
			case 1: return DefaultBondGeometry.Single;
		}
		
		
	}

}
