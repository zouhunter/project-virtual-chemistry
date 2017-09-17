
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaSqueezeWarp))]
public class MegaSqueezeWarpEditor : MegaWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/Squeeze")]
	static void CreateSqueezeWarp() { CreateWarp("Squeeze", typeof(MegaSqueezeWarp)); }

	public override string GetHelpString() { return "Squeeze Warp Modifier by Chris West"; }
	//public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	public override bool Inspector()
	{
		MegaSqueezeWarp mod = (MegaSqueezeWarp)target;

		EditorGUIUtility.LookLikeControls();
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		mod.amount = EditorGUILayout.FloatField("Amount", mod.amount);
		mod.crv = EditorGUILayout.FloatField("Crv", mod.crv);
		mod.radialamount = EditorGUILayout.FloatField("Radial Amount", mod.radialamount);
		mod.radialcrv = EditorGUILayout.FloatField("Radial Crv", mod.radialcrv);
		mod.doRegion = EditorGUILayout.Toggle("Do Region", mod.doRegion);
		mod.from = EditorGUILayout.FloatField("From", mod.from);
		mod.to = EditorGUILayout.FloatField("To", mod.to);

		return false;
	}
}