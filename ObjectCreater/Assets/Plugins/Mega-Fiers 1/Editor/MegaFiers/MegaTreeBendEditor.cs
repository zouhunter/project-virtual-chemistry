using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaTreeBend))]
public class MegaTreeBendEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Tree Bend Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	public override bool Inspector()
	{
		MegaTreeBend mod = (MegaTreeBend)target;

		EditorGUIUtility.LookLikeControls();
		mod.fBendScale = EditorGUILayout.FloatField("Bend Scale", mod.fBendScale);
		//mod.vWind = EditorGUILayout.Vector3Field("Wind", mod.vWind);
		mod.WindDir = EditorGUILayout.FloatField("Wind Dir", mod.WindDir);
		mod.WindSpeed = EditorGUILayout.FloatField("Wind Speed", mod.WindSpeed);
		return false;
	}
}
