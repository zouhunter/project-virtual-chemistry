
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaFFDAnimate))]
public class MegaFFDAnimateEditor : Editor
{
	bool showpoints = false;

	public override void OnInspectorGUI()
	{
		MegaFFDAnimate ffd = (MegaFFDAnimate)target;

		EditorGUIUtility.LookLikeControls();

		ffd.Enabled = EditorGUILayout.Toggle("Enabled", ffd.Enabled);
		ffd.SetRecord(EditorGUILayout.Toggle("Record", ffd.GetRecord()));

		MegaFFD mod = ffd.GetFFD();

		showpoints = EditorGUILayout.Foldout(showpoints, "Points");

		if ( mod && showpoints )
		{
			int size = mod.GridSize();
			size = size * size * size;
			for ( int i = 0; i < size; i++ )
			{
				ffd.SetPoint(i, EditorGUILayout.Vector3Field("p" + i, ffd.GetPoint(i)));
			}
		}

		//if ( AnimationUtility.InAnimationMode() )
		//	ffd.SetRecord(true);
		//else
		//	ffd.SetRecord(false);

		if ( GUI.changed )
		{
			EditorUtility.SetDirty(target);
		}
	}
}