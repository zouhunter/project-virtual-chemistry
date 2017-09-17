
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaCurveSculpt))]
public class MegaCurveSculptEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Mega Curve Sculpt Modifier by Chris West"; }

	public override bool Inspector()
	{
		MegaCurveSculpt mod = (MegaCurveSculpt)target;

		EditorGUIUtility.LookLikeControls();

		mod.OffsetAmount = EditorGUILayout.Vector3Field("Offset Amount", mod.OffsetAmount);
		mod.offsetX = (MegaAxis)EditorGUILayout.EnumPopup("Alter", mod.offsetX);
		//mod.symX = EditorGUILayout.Toggle("Sym", mod.symX);
		mod.defCurveX = EditorGUILayout.CurveField("Offset X", mod.defCurveX);
		mod.offsetY = (MegaAxis)EditorGUILayout.EnumPopup("Alter", mod.offsetY);
		//mod.symY = EditorGUILayout.Toggle("Sym", mod.symY);
		mod.defCurveY = EditorGUILayout.CurveField("Offset Y", mod.defCurveY);
		mod.offsetZ = (MegaAxis)EditorGUILayout.EnumPopup("Alter", mod.offsetZ);
		//mod.symZ = EditorGUILayout.Toggle("Sym", mod.symZ);
		mod.defCurveZ = EditorGUILayout.CurveField("Offset Z", mod.defCurveZ);

		mod.ScaleAmount = EditorGUILayout.Vector3Field("Scale Amount", mod.ScaleAmount);
		mod.scaleX = (MegaAxis)EditorGUILayout.EnumPopup("Alter", mod.scaleX);
		mod.defCurveSclX = EditorGUILayout.CurveField("Scale X", mod.defCurveSclX);
		mod.scaleY = (MegaAxis)EditorGUILayout.EnumPopup("Alter", mod.scaleY);
		mod.defCurveSclY = EditorGUILayout.CurveField("Scale Y", mod.defCurveSclY);
		mod.scaleZ = (MegaAxis)EditorGUILayout.EnumPopup("Alter", mod.scaleZ);
		mod.defCurveSclZ = EditorGUILayout.CurveField("Scale Z", mod.defCurveSclZ);

		return false;
	}
}
