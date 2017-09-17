
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaWrap))]
public class MegaWrapEditor : Editor
{
	public override void OnInspectorGUI()
	{
		MegaWrap mod = (MegaWrap)target;

		EditorGUIUtility.LookLikeControls();
		mod.WrapEnabled = EditorGUILayout.Toggle("Enabled", mod.WrapEnabled);
		mod.target = (MegaModifyObject)EditorGUILayout.ObjectField("Target", mod.target, typeof(MegaModifyObject), true);

		float max = 1.0f;
		if ( mod.target )
			max = mod.target.bbox.size.magnitude;

		mod.maxdist = EditorGUILayout.Slider("Max Dist", mod.maxdist, 0.0f, max);	//2.0f);	//mod.maxdist);
		if ( mod.maxdist < 0.0f )
			mod.maxdist = 0.0f;

		mod.maxpoints = EditorGUILayout.IntField("Max Points", mod.maxpoints);	//mod.maxdist);
		if ( mod.maxpoints < 1 )
			mod.maxpoints = 1;

		Color col = GUI.backgroundColor;
		EditorGUILayout.BeginHorizontal();
		if ( mod.bindverts == null )
		{
			GUI.backgroundColor = Color.red;
			if ( GUILayout.Button("Map") )
				mod.Attach(mod.target);
		}
		else
		{
			GUI.backgroundColor = Color.green;
			if ( GUILayout.Button("ReMap") )
				mod.Attach(mod.target);
		}

		GUI.backgroundColor = col;
		if ( GUILayout.Button("Reset") )
			mod.ResetMesh();

		EditorGUILayout.EndHorizontal();

		if ( GUI.changed )
			EditorUtility.SetDirty(mod);

		mod.gap = EditorGUILayout.FloatField("Gap", mod.gap);
		mod.shrink = EditorGUILayout.Slider("Shrink", mod.shrink, 0.0f, 1.0f);
		mod.size = EditorGUILayout.Slider("Size", mod.size, 0.001f, 0.04f);
		if ( mod.bindverts != null )
			mod.vertindex = EditorGUILayout.IntSlider("Vert Index", mod.vertindex, 0, mod.bindverts.Length - 1);
		mod.offset = EditorGUILayout.Vector3Field("Offset", mod.offset);

		if ( mod.bindverts == null || mod.target == null )
			EditorGUILayout.LabelField("Object not wrapped");
		else
			EditorGUILayout.LabelField("UnMapped", mod.nomapcount.ToString());

		if ( GUI.changed )
			EditorUtility.SetDirty(mod);
	}

	public void OnSceneGUI()
	{
		DisplayDebug();
	}

	void DisplayDebug()
	{
		MegaWrap mod = (MegaWrap)target;
		if ( mod.target )
		{
			if ( mod.bindverts != null && mod.bindverts.Length > 0 )
			{
				if ( mod.targetIsSkin && !mod.sourceIsSkin )
				{
					Color col = Color.black;
					Handles.matrix = Matrix4x4.identity;

					MegaBindVert bv = mod.bindverts[mod.vertindex];

					for ( int i = 0; i < bv.verts.Count; i++ )
					{
						MegaBindInf bi = bv.verts[i];
						float w = bv.verts[i].weight / bv.weight;

						if ( w > 0.5f )
							col = Color.Lerp(Color.green, Color.red, (w - 0.5f) * 2.0f);
						else
							col = Color.Lerp(Color.blue, Color.green, w * 2.0f);
						Handles.color = col;

						Vector3 p = (mod.skinnedVerts[bv.verts[i].i0] + mod.skinnedVerts[bv.verts[i].i1] + mod.skinnedVerts[bv.verts[i].i2]) / 3.0f;	//tm.MultiplyPoint(mod.vr[i].cpos);
						Handles.DotCap(i, p, Quaternion.identity, mod.size);	//0.01f);

						Vector3 p0 = mod.skinnedVerts[bi.i0];
						Vector3 p1 = mod.skinnedVerts[bi.i1];
						Vector3 p2 = mod.skinnedVerts[bi.i2];

						Vector3 cp = mod.GetCoordMine(p0, p1, p2, bi.bary);
						Handles.color = Color.gray;
						Handles.DrawLine(p, cp);

						Vector3 norm = mod.FaceNormal(p0, p1, p2);
						Vector3 cp1 = cp + (((bi.dist * mod.shrink) + mod.gap) * norm.normalized);
						Handles.color = Color.green;
						Handles.DrawLine(cp, cp1);
					}
				}
				else
				{
					Color col = Color.black;
					Matrix4x4 tm = mod.target.transform.localToWorldMatrix;
					Handles.matrix = tm;	//Matrix4x4.identity;

					MegaBindVert bv = mod.bindverts[mod.vertindex];

					for ( int i = 0; i < bv.verts.Count; i++ )
					{
						MegaBindInf bi = bv.verts[i];
						float w = bv.verts[i].weight / bv.weight;

						if ( w > 0.5f )
							col = Color.Lerp(Color.green, Color.red, (w - 0.5f) * 2.0f);
						else
							col = Color.Lerp(Color.blue, Color.green, w * 2.0f);
						Handles.color = col;

						Vector3 p = (mod.target.sverts[bv.verts[i].i0] + mod.target.sverts[bv.verts[i].i1] + mod.target.sverts[bv.verts[i].i2]) / 3.0f;	//tm.MultiplyPoint(mod.vr[i].cpos);
						Handles.DotCap(i, p, Quaternion.identity, mod.size);	//0.01f);

						Vector3 p0 = mod.target.sverts[bi.i0];
						Vector3 p1 = mod.target.sverts[bi.i1];
						Vector3 p2 = mod.target.sverts[bi.i2];

						Vector3 cp = mod.GetCoordMine(p0, p1, p2, bi.bary);
						Handles.color = Color.gray;
						Handles.DrawLine(p, cp);

						Vector3 norm = mod.FaceNormal(p0, p1, p2);
						Vector3 cp1 = cp + (((bi.dist * mod.shrink) + mod.gap) * norm.normalized);
						Handles.color = Color.green;
						Handles.DrawLine(cp, cp1);
					}
				}

				// Show unmapped verts
				Handles.color = Color.yellow;
				for ( int i = 0; i < mod.bindverts.Length; i++ )
				{
					if ( mod.bindverts[i].weight == 0.0f )
					{
						Vector3 pv1 = mod.freeverts[i];
						Handles.DotCap(0, pv1, Quaternion.identity, mod.size);	//0.01f);
					}
				}
			}

			if ( mod.verts != null && mod.verts.Length > mod.vertindex )
			{
				Handles.color = Color.red;
				Handles.matrix = mod.transform.localToWorldMatrix;
				Vector3 pv = mod.verts[mod.vertindex];
				Handles.DotCap(0, pv, Quaternion.identity, mod.size);	//0.01f);
			}
		}
	}
}