
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaDynamicRipple))]
public class MegaDynamicRippleEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Dynamic Ripple Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	public override bool Inspector()
	{
		MegaDynamicRipple mod = (MegaDynamicRipple)target;

		EditorGUIUtility.LookLikeControls();

		bool dirty = false;
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);

		mod.cols = EditorGUILayout.IntField("Columns", mod.cols);
		if ( mod.cols < 1 )
			mod.cols = 1;

		mod.rows = EditorGUILayout.IntField("Rows", mod.rows);
		if ( mod.rows < 1 )
			mod.rows = 1;

		if ( GUI.changed )
		{
			dirty = true;
		}

		mod.damping = EditorGUILayout.Slider("Damping", mod.damping, 0.0f, 1.0f);
		mod.inputdamp = EditorGUILayout.Slider("Input Damping", mod.inputdamp, 0.0f, 1.0f);
		mod.scale = EditorGUILayout.Slider("Scale", mod.scale, 0.0f, 4.0f);

		mod.speed = EditorGUILayout.Slider("Speed", mod.speed, 0.0f, 0.5f);

		mod.Force = EditorGUILayout.FloatField("Force", mod.Force);

		mod.InputForce = EditorGUILayout.FloatField("InputForce", mod.InputForce);

		mod.Obstructions = EditorGUILayout.Toggle("Obstructions", mod.Obstructions);

		mod.DropsPerSec = EditorGUILayout.FloatField("Drops Per Sec", mod.DropsPerSec);
		if ( mod.DropsPerSec < 0.0f )
			mod.DropsPerSec = 0.0f;

		if ( dirty )
		{
			mod.ResetGrid();
		}

		if ( GUILayout.Button("Reset Physics") )
		{
			mod.ResetGrid();
		}

		return false;
	}
}
