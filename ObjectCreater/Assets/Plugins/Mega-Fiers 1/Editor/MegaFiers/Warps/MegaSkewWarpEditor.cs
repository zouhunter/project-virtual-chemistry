
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaSkewWarp))]
public class MegaSkewWarpEditor : MegaWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/Skew")]
	static void CreateStarShape() { CreateWarp("Skew", typeof(MegaSkewWarp)); }

	public override string GetHelpString() { return "Skew Warp Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\skew_help.png"); }

	public override bool Inspector()
	{
		MegaSkewWarp mod = (MegaSkewWarp)target;

		EditorGUIUtility.LookLikeControls();
		mod.amount = EditorGUILayout.FloatField("Amount", mod.amount);
		mod.dir = EditorGUILayout.FloatField("Dir", mod.dir);
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		mod.doRegion = EditorGUILayout.Toggle("Do Region", mod.doRegion);
		mod.from = EditorGUILayout.FloatField("From", mod.from);
		mod.to = EditorGUILayout.FloatField("To", mod.to);
		return false;
	}
}
