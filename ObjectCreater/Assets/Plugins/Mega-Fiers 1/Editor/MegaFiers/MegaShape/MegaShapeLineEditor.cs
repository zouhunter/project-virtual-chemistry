
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MegaShapeLine))]
public class MegaShapeLineEditor : MegaShapeEditor
{
	public override bool Params()
	{
		MegaShapeLine shape = (MegaShapeLine)target;

		bool rebuild = false;

		float v = EditorGUILayout.FloatField("Length", shape.length);
		if ( v != shape.length )
		{
			shape.length = v;
			rebuild = true;
		}

		int p = EditorGUILayout.IntField("Points", shape.points);
		if ( p != shape.points )
		{
			shape.points = p;
			rebuild = true;
		}

		v = EditorGUILayout.FloatField("Dir", shape.dir);
		if ( v != shape.dir )
		{
			shape.dir = v;
			rebuild = true;
		}

		Transform tm = (Transform)EditorGUILayout.ObjectField("End", shape.end, typeof(Transform), true);
		if ( tm != shape.end )
		{
			shape.end = tm;
			rebuild = true;
		}

		return rebuild;
	}
}