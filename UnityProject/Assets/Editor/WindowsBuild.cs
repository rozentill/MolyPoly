using UnityEditor;
using System.Diagnostics;

public class ScriptBatch 
{
	[MenuItem("MolyPolyTools/Windows Build With Postprocess")]
	public static void BuildGameWindows ()
	{
		// Get filename.
		string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "../../", "build");

		string[] levels = new string[] {"Assets/Login/LoginScene.unity"};
		
		// Build player.
		BuildPipeline.BuildPlayer(levels, path + "/MolyPoly2.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
		
		// Copy a file from the project folder to the build folder, alongside the built game.
		FileUtil.ReplaceDirectory ("Assets/Configuration", path + "/MolyPoly2_Data/Configuration");
	}

	[MenuItem("MolyPolyTools/Mac OS Build With Postprocess")]
	public static void BuildGameMac ()
	{
		// Get filename.
		string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "../../", "build");
		
		string[] levels = new string[] {"Assets/Login/LoginScene.unity"};
		
		// Build player.
		BuildPipeline.BuildPlayer(levels, path + "/MolyPoly2", BuildTarget.StandaloneOSXUniversal, BuildOptions.None);
		
		// Copy a file from the project folder to the build folder, alongside the built game.
		FileUtil.ReplaceDirectory ("Assets/Configuration", path + "/MolyPoly2.app/Contents/Configuration");
	}
}
