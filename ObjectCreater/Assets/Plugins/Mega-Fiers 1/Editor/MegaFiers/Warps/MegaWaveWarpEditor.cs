
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaWaveWarp))]
public class MegaWaveWarpEditor : MegaWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/Wave")]
	static void CreateStarShape() { CreateWarp("Wave", typeof(MegaWaveWarp)); }

	public override string GetHelpString() { return "Wave Warp Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("wave_help.png"); }

	public override bool Inspector()
	{
		MegaWaveWarp mod = (MegaWaveWarp)target;

		EditorGUIUtility.LookLikeControls();
		mod.amp = EditorGUILayout.FloatField("Amp", mod.amp);
		mod.amp2 = EditorGUILayout.FloatField("Amp 2", mod.amp2);
		mod.wave = EditorGUILayout.FloatField("Wave", mod.wave);
		mod.phase = EditorGUILayout.FloatField("Phase", mod.phase);
		mod.Decay = EditorGUILayout.FloatField("Decay", mod.Decay);
		mod.animate = EditorGUILayout.Toggle("Animate", mod.animate);
		mod.Speed = EditorGUILayout.FloatField("Speed", mod.Speed);
		mod.divs = EditorGUILayout.IntField("Divs", mod.divs);
		mod.numSegs = EditorGUILayout.IntField("Num Segs", mod.numSegs);
		mod.numSides = EditorGUILayout.IntField("Num Sides", mod.numSides);
		return false;
	}
}