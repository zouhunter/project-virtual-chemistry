using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaWaving))]
public class MegaWavingEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Waving Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\wave_help.png"); }

	public override bool Inspector()
	{
		MegaWaving mod = (MegaWaving)target;

		EditorGUIUtility.LookLikeControls();

		mod.amp = EditorGUILayout.FloatField("Amp", mod.amp * 100.0f) * 0.01f;
		mod.wave = EditorGUILayout.FloatField("Wave", mod.wave);
		mod.phase = EditorGUILayout.FloatField("Phase", mod.phase);
		mod.Decay = EditorGUILayout.FloatField("Decay", mod.Decay);
		mod.animate = EditorGUILayout.Toggle("Animate", mod.animate);
		mod.Speed = EditorGUILayout.FloatField("Speed", mod.Speed);
		mod.waveaxis= (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.waveaxis);
		return false;
	}
}