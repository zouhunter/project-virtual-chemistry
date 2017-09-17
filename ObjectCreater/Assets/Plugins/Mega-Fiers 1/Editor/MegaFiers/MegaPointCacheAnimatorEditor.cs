
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaPointCacheAnimator))]
public class MegaPointCacheAnimatorEditor : Editor
{
	// TODO: Need none in the popup to clear a channel
	public override void OnInspectorGUI()
	{
		MegaPointCacheAnimator anim = (MegaPointCacheAnimator)target;

		string[] clips = anim.GetClipNames();
		anim.useFrames = EditorGUILayout.BeginToggleGroup("Use Frames", anim.useFrames);
		anim.sourceFPS = EditorGUILayout.IntField("Source FPS", anim.sourceFPS);
		EditorGUILayout.EndToggleGroup();

		anim.LinkedUpdate = EditorGUILayout.Toggle("Linked Update", anim.LinkedUpdate);
		anim.PlayOnStart = EditorGUILayout.Toggle("Play On Start", anim.PlayOnStart);

		//anim.current = EditorGUILayout.Popup("Playing Clip", anim.current, clips);
		int current = EditorGUILayout.Popup("Playing Clip", anim.current, clips);
		if ( current != anim.current )
		{
			anim.PlayClip(current);
		}

		//anim.t = EditorGUILayout.FloatField("t", anim.t);
		//anim.at = EditorGUILayout.FloatField("at", anim.at);

		if ( GUILayout.Button("Add Clip") )
			anim.AddClip("Clip " + anim.clips.Count, 0.0f, 1.0f, MegaRepeatMode.Loop);

		EditorGUILayout.BeginVertical();
		for ( int i = 0; i < anim.clips.Count; i++ )
		{
			EditorGUILayout.BeginHorizontal();

			//EditorGUILayout.TextArea("" + i + " - ");
			anim.clips[i].name = EditorGUILayout.TextField(anim.clips[i].name);

			if ( anim.useFrames )
			{
				anim.clips[i].start = (float)EditorGUILayout.FloatField((float)(anim.clips[i].start * anim.sourceFPS), GUILayout.Width(40)) / (float)anim.sourceFPS;
				anim.clips[i].end = (float)EditorGUILayout.FloatField((float)(anim.clips[i].end * anim.sourceFPS), GUILayout.Width(40)) / (float)anim.sourceFPS;
				anim.clips[i].speed = EditorGUILayout.FloatField(anim.clips[i].speed, GUILayout.Width(40));
				anim.clips[i].loop = (MegaRepeatMode)EditorGUILayout.EnumPopup(anim.clips[i].loop);
			}
			else
			{
				anim.clips[i].start = EditorGUILayout.FloatField(anim.clips[i].start, GUILayout.Width(40));
				anim.clips[i].end = EditorGUILayout.FloatField(anim.clips[i].end, GUILayout.Width(40));
				anim.clips[i].speed = EditorGUILayout.FloatField(anim.clips[i].speed, GUILayout.Width(40));
				anim.clips[i].loop = (MegaRepeatMode)EditorGUILayout.EnumPopup(anim.clips[i].loop);
			}

			if ( GUILayout.Button("-") )
			{
				anim.clips.Remove(anim.clips[i]);
			}

			EditorGUILayout.EndHorizontal();
		}
	}
}