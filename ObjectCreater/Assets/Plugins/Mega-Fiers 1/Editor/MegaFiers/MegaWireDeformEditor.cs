
#if false
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaWireDeform))]
public class MegaWireDeformEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Wire Deformer by Eli Curtz"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	public override bool Inspector()
	{
		MegaWireDeform mod = (MegaWireDeform)target;

		EditorGUIUtility.LookLikeControls();
		mod.resolution = EditorGUILayout.IntField("Resolution", mod.resolution);
		mod.falloff = EditorGUILayout.FloatField("FallOff", mod.falloff);
		mod.source = (MegaShape)EditorGUILayout.ObjectField("Source Spline", mod.source, typeof(MegaShape), true);
		mod.target = (MegaShape)EditorGUILayout.ObjectField("Target Spline", mod.target, typeof(MegaShape), true);
		return false;
	}
}
#endif