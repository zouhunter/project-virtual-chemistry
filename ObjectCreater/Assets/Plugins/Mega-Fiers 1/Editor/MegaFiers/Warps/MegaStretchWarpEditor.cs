
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaStretchWarp))]
public class MegaStretchWarpEditor : MegaWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/Stretch")]
	static void CreateStarShape() { CreateWarp("Stretch", typeof(MegaStretchWarp)); }

	public override string GetHelpString() { return "Stretch Warp Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\stretch_help.png"); }

	public override bool Inspector()
	{
		MegaStretchWarp mod = (MegaStretchWarp)target;

		EditorGUIUtility.LookLikeControls();
		mod.amount		= EditorGUILayout.FloatField("Amount", mod.amount);
		mod.amplify		= EditorGUILayout.FloatField("Amplify", mod.amplify);
		mod.axis		= (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		mod.doRegion	= EditorGUILayout.Toggle("Do Region", mod.doRegion);
		mod.from		= EditorGUILayout.FloatField("From", mod.from);
		mod.to			= EditorGUILayout.FloatField("To", mod.to);
		return false;
	}
}
