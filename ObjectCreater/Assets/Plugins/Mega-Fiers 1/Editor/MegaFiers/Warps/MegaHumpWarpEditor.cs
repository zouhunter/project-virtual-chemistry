
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaHumpWarp))]
public class MegaHumpWarpEditor : MegaWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/Hump")]
	static void CreateStarShape() { CreateWarp("Hump", typeof(MegaHumpWarp)); }

	public override string GetHelpString() { return "Hump Warp Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\hump_help.png"); }

	public override bool Inspector()
	{
		MegaHumpWarp mod = (MegaHumpWarp)target;

		EditorGUIUtility.LookLikeControls();
		mod.amount = EditorGUILayout.FloatField("Amount", mod.amount);
		mod.cycles = EditorGUILayout.FloatField("Cycles", mod.cycles);
		mod.phase = EditorGUILayout.FloatField("Phase", mod.phase);
		mod.animate = EditorGUILayout.Toggle("Animate", mod.animate);
		mod.speed = EditorGUILayout.FloatField("Speed", mod.speed);
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		return false;
	}
}