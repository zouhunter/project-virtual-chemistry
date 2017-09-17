
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaSpherifyWarp))]
public class MegaSpherifyWarpEditor : MegaWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/Spherify")]
	static void CreateStarShape() { CreateWarp("Spherify", typeof(MegaSpherifyWarp)); }

	public override string GetHelpString() { return "Spherify Warp Modifier by Chris West"; }
	//public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("bend_help.png"); }

	public override bool Inspector()
	{
		MegaSpherifyWarp mod = (MegaSpherifyWarp)target;

		EditorGUIUtility.LookLikeControls();
		mod.percent = EditorGUILayout.FloatField("Percent", mod.percent);
		mod.FallOff = EditorGUILayout.FloatField("FallOff", mod.FallOff);
		return false;
	}
}