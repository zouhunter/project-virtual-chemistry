
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaBubbleWarp))]
public class MegaBubbleWarpEditor : MegaWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/Bubble")]
	static void CreateStarShape() { CreateWarp("Bubble", typeof(MegaBubbleWarp)); }

	public override string GetHelpString() { return "Bubble Warp Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bubble_help.png"); }

	public override bool Inspector()
	{
		MegaBubbleWarp mod = (MegaBubbleWarp)target;

		EditorGUIUtility.LookLikeControls();
		mod.radius = EditorGUILayout.FloatField("Radius", mod.radius);
		mod.falloff = EditorGUILayout.FloatField("Falloff", mod.falloff);
		return false;
	}
}