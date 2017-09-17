
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

// Support anim uvs here as well
[CustomEditor(typeof(MegaPointCacheRef))]
public class MegaPointCacheRefEditor : MegaModifierEditor
{
	public override void OnInspectorGUI()
	{
		MegaPointCacheRef am = (MegaPointCacheRef)target;

		// Basic mod stuff
		showmodparams = EditorGUILayout.Foldout(showmodparams, "Modifier Common Params");

		if ( showmodparams )
			CommonModParamsBasic(am);

		am.source = (MegaPointCache)EditorGUILayout.ObjectField("Source", am.source, typeof(MegaPointCache), true);
		am.time = EditorGUILayout.FloatField("Time", am.time);
		am.maxtime = EditorGUILayout.FloatField("Loop Time", am.maxtime);
		am.animated = EditorGUILayout.Toggle("Animated", am.animated);
		am.speed = EditorGUILayout.FloatField("Speed", am.speed);
		am.LoopMode = (MegaRepeatMode)EditorGUILayout.EnumPopup("Loop Mode", am.LoopMode);
		am.interpMethod = (MegaInterpMethod)EditorGUILayout.EnumPopup("Interp Method", am.interpMethod);

		am.blendMode = (MegaBlendAnimMode)EditorGUILayout.EnumPopup("Blend Mode", am.blendMode);
		if ( am.blendMode == MegaBlendAnimMode.Additive )
			am.weight = EditorGUILayout.FloatField("Weight", am.weight);

		if ( GUI.changed )
			EditorUtility.SetDirty(target);
	}
}