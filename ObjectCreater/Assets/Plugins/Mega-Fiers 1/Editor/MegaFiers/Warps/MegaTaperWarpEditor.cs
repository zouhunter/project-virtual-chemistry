
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaTaperWarp))]
public class MegaTaperWarpEditor : MegaWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/Taper")]
	static void CreateStarShape() { CreateWarp("Taper", typeof(MegaTaperWarp)); }

	public override string GetHelpString() { return "Taper Warp Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\taper_help.png"); }

	public override bool Inspector()
	{
		MegaTaperWarp mod = (MegaTaperWarp)target;

		EditorGUIUtility.LookLikeControls();

		mod.amount = EditorGUILayout.FloatField("Amount", mod.amount);
		mod.crv = EditorGUILayout.FloatField("Crv", mod.crv);
		mod.dir = EditorGUILayout.FloatField("Dir", mod.dir);
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		mod.EAxis = (MegaEffectAxis)EditorGUILayout.EnumPopup("EAxis", mod.EAxis);
		mod.sym = EditorGUILayout.Toggle("Sym", mod.sym);
		mod.doRegion = EditorGUILayout.Toggle("Do Region", mod.doRegion);
		mod.from = EditorGUILayout.FloatField("From", mod.from);
		mod.to = EditorGUILayout.FloatField("To", mod.to);
		return false;
	}
}
