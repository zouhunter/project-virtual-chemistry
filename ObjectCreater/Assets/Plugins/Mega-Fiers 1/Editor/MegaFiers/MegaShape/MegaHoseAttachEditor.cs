
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaHoseAttach))]
public class MegaHoseAttachEditor : Editor
{
	public override void OnInspectorGUI()
	{
		MegaHoseAttach mod = (MegaHoseAttach)target;

		EditorGUIUtility.LookLikeControls();

		mod.hose = (MegaHose)EditorGUILayout.ObjectField("Hose", mod.hose, typeof(MegaHose), true);
		mod.alpha = EditorGUILayout.FloatField("Alpha", mod.alpha);
		mod.offset = EditorGUILayout.Vector3Field("Offset", mod.offset);

		mod.rot = EditorGUILayout.BeginToggleGroup("Rot On", mod.rot);
		mod.rotate = EditorGUILayout.Vector3Field("Rotate", mod.rotate);
		EditorGUILayout.EndToggleGroup();
		mod.doLateUpdate = EditorGUILayout.Toggle("Late Update", mod.doLateUpdate);

		if ( GUI.changed )	//rebuild )
		{
			EditorUtility.SetDirty(mod);
		}
	}
}