using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaMeltWarp))]
public class MegaMeltWarpEditor : MegaWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/Melt")]
	static void CreateStarShape() { CreateWarp("Melt", typeof(MegaMeltWarp)); }

	public override string GetHelpString() { return "Melt Warp Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\melt_help.png"); }

	public override bool Inspector()
	{
		MegaMeltWarp mod = (MegaMeltWarp)target;

		EditorGUIUtility.LookLikeControls();
		mod.Amount = EditorGUILayout.FloatField("Amount", mod.Amount);
		mod.Spread = EditorGUILayout.FloatField("Spread", mod.Spread);
		mod.MaterialType = (MegaMeltMat)EditorGUILayout.EnumPopup("Material Type", mod.MaterialType);
		mod.Solidity = EditorGUILayout.FloatField("Solidity", mod.Solidity);
		//mod.zba = EditorGUILayout.FloatField("zba", mod.zba);
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		mod.FlipAxis = EditorGUILayout.Toggle("Flip Axis", mod.FlipAxis);

		mod.flatness = EditorGUILayout.Slider("Flatness", mod.flatness, 0.0f, 1.0f);

		return false;
	}
}