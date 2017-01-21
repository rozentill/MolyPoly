using UnityEngine;
using System.Collections;


//The constants that define various thresholds or timings.
public static class Settings  {

	public static readonly Color HighlightColor = Color.magenta;
	public const float HandMagnetDistance = 2.0f;
	public const float HandHighLightDistance = Settings.HandMagnetDistance * 1.0f;

	public const float Threshold_VeryFar   = 7.0f;  //Threshold 0: to detach atoms.
	public const float Threshold_Medium    = 5.0f;	//Threshold 1: to show target holes for attachment
	public const float Threshold_Close     = 1.5f;	//Threshold 2: to start attachment countdown to target hole
	public const float Threshold_VeryClose = 0.5f;  //Threshold 3: to start bond attachment countdown to target hold

	//Grab/Release threshold : 1-0, where 1 == fist, 0 == open palm.
	//note that threshold_grab >= threshold_release.
	public const float Threshold_Grab = 0.1f;
	public const float Threshold_Release = 0.1f;

	public const float Threshold_RotationGrab = 0.2f;
	public const float IncreaseRotationFactor = 2.0f;

	public const float DelayGrabDistance = Threshold_Medium;
	public const float AttachmentTime = 0.3f; //Seconds to attach the atom when hovering over attachment point.

	public const string MouseRotationButton = "Fire2";
	public const string MouseDragButton = "Fire1";

	//How long to display the error panel when the user clicks the "Check Molecule" button.
	public const float ErrorPanelTimerCountdown = 5.0f;
}
