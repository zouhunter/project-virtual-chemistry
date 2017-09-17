
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaGlobeWarp))]
public class MegaGlobeWarpEditor : MegaWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/Globe")]
	static void CreateStarShape() { CreateWarp("Globe", typeof(MegaGlobeWarp)); }

	public override string GetHelpString() { return "Globe Warp Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	public override bool Inspector()
	{
		MegaGlobeWarp mod = (MegaGlobeWarp)target;

		EditorGUIUtility.LookLikeControls();
		mod.radius = EditorGUILayout.FloatField("Radius", mod.radius);

		mod.linkRadii = EditorGUILayout.Toggle("Link Radii", mod.linkRadii);
		if ( !mod.linkRadii )
			mod.radius1 = EditorGUILayout.FloatField("Radius1", mod.radius1);

		mod.dir = EditorGUILayout.FloatField("Dir", mod.dir);
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);

		mod.twoaxis = EditorGUILayout.BeginToggleGroup("Two Axis", mod.twoaxis);
		mod.dir1 = EditorGUILayout.FloatField("Dir1", mod.dir1);
		mod.axis1 = (MegaAxis)EditorGUILayout.EnumPopup("Axis1", mod.axis1);
		EditorGUILayout.EndToggleGroup();

		return false;
	}
}