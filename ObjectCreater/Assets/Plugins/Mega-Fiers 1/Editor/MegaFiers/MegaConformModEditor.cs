
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaConformMod))]
public class MegaConformModEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Conform Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	public override bool DisplayCommon()
	{
		return false;
	}


	public override bool Inspector()
	{
		MegaConformMod mod = (MegaConformMod)target;

		EditorGUIUtility.LookLikeControls();
		CommonModParamsBasic(mod);

		mod.target = (GameObject)EditorGUILayout.ObjectField("Target", mod.target, typeof(GameObject), true);
		mod.conformAmount = EditorGUILayout.Slider("Conform Amount", mod.conformAmount, 0.0f, 1.0f);
		mod.raystartoff = EditorGUILayout.FloatField("Ray Start Off", mod.raystartoff);
		mod.raydist = EditorGUILayout.FloatField("Ray Dist", mod.raydist);
		mod.offset = EditorGUILayout.FloatField("Offset", mod.offset);
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		return false;
	}
}