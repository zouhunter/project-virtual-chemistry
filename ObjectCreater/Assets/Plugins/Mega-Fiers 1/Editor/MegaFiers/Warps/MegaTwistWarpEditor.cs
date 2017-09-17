
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaTwistWarp))]
public class MegaTwistWarpEditor : MegaWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/Twist")]
	static void CreateStarShape() { CreateWarp("Twist", typeof(MegaTwistWarp)); }

	public override string GetHelpString() { return "Twist Warp Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\twist_help.png"); }

	public override bool Inspector()
	{
		MegaTwistWarp mod = (MegaTwistWarp)target;

		EditorGUIUtility.LookLikeControls();
		mod.angle		= EditorGUILayout.FloatField("Angle", mod.angle);
		mod.Bias		= EditorGUILayout.FloatField("Bias", mod.Bias);
		mod.axis		= (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		mod.doRegion	= EditorGUILayout.Toggle("Do Region", mod.doRegion);
		mod.from		= EditorGUILayout.FloatField("From", mod.from);
		mod.to			= EditorGUILayout.FloatField("To", mod.to);
		return false;
	}
}
