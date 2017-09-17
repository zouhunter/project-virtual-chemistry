
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaMelt))]
public class MegaMeltEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Melt Modifier by Chris West"; }
	//public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	public override bool Inspector()
	{
		MegaMelt mod = (MegaMelt)target;

		EditorGUIUtility.LookLikeControls();
		mod.Amount = EditorGUILayout.FloatField("Amount", mod.Amount);
		mod.Spread = EditorGUILayout.FloatField("Spread", mod.Spread);
		mod.MaterialType = (MegaMeltMat)EditorGUILayout.EnumPopup("Material Type", mod.MaterialType);
		mod.Solidity = EditorGUILayout.FloatField("Solidity", mod.Solidity);
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		mod.FlipAxis = EditorGUILayout.Toggle("Flip Axis", mod.FlipAxis);
		mod.flatness = EditorGUILayout.Slider("Flatness", mod.flatness, 0.0f, 1.0f);
		return false;
	}
}