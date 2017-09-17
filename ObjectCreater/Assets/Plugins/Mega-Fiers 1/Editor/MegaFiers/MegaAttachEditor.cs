
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaAttach))]
public class MegaAttachEditor : Editor
{
	public override void OnInspectorGUI()
	{
		MegaAttach mod = (MegaAttach)target;

		EditorGUIUtility.LookLikeControls();
		//DrawDefaultInspector();

		mod.target = (MegaModifiers)EditorGUILayout.ObjectField("Target", mod.target, typeof(MegaModifiers), true);

		mod.attachforward = EditorGUILayout.Vector3Field("Attach Fwd", mod.attachforward);
		mod.AxisRot = EditorGUILayout.Vector3Field("Axis Rot", mod.AxisRot);
		mod.radius = EditorGUILayout.FloatField("Radius", mod.radius);
		mod.up = EditorGUILayout.Vector3Field("Up", mod.up);
		mod.worldSpace = EditorGUILayout.Toggle("World Space", mod.worldSpace);

		if ( GUI.changed )
		{
			EditorUtility.SetDirty(mod);
		}

		if ( !mod.attached )
		{
			if ( GUILayout.Button("Attach") )
			{
				mod.AttachIt();
				EditorUtility.SetDirty(mod);
			}
		}
		else
		{
			if ( GUILayout.Button("Detach") )
			{
				mod.DetachIt();
				EditorUtility.SetDirty(mod);
			}
		}
	}
}