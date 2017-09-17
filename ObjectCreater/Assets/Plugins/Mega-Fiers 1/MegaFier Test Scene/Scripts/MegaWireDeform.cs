
#if false
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public struct MegaWireVert
{
	public int      vert;
	public int      u;
	public float    w;
}

[AddComponentMenu("Modifiers/Wire Deform")]
public class MegaWireDeform : MegaModifier
{
	//public MegaSpline     source      = null;
	//public MegaSpline     target      = null;
	public MegaShape        source      = null;
	public MegaShape        target      = null;
	public int              resolution  = 50;
	public float            falloff     = 1f;

	public override string ModName() { return "WireDeform"; }

	Vector3[]       sourcePositions;
	Vector3[]       targetOffsets;

	MegaWireVert[]  wireVerts;

	public override void ModStart(MegaModifiers mc)
	{
		Debug.Log("ModStart");
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		Debug.Log("ModLateUpdate");

		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		Debug.Log("Prepare");

		if ( source == null )
			return false;
		if ( target == null )
			return false;

		sourcePositions = new Vector3[resolution];
		targetOffsets = new Vector3[resolution];

		// Find [resolution] steps along the source shape
		bool type = false;
		int knot = 0;
		float alpha = 0f;
		float step = 1f / (resolution - 1);
		Matrix4x4 trans = transform.worldToLocalMatrix * source.transform.localToWorldMatrix;
		for ( int i = 0; i < resolution; i++ )
		{
			//sourcePositions[i] = trans.MultiplyPoint3x4(source.Interpolate(alpha, type, ref knot));
			sourcePositions[i] = trans.MultiplyPoint3x4(source.InterpCurve3D(0, alpha, type));
			alpha += step;
		}

		// Where does this go?
		List<MegaWireVert> vertList = new List<MegaWireVert>();
		int vertCount = verts.Length;
		for ( int i = 0; i < vertCount; i++ )
		{
			float dist = Mathf.Infinity;
			int closest = 0;
			for ( int j = 0; j < resolution; j++ )
			{
				float sourceDist = Vector3.Distance(verts[i], sourcePositions[j]);
				if ( sourceDist < dist )
				{
					dist = sourceDist;
					closest = j;
				}
			}

			if ( dist < falloff )
			{
				MegaWireVert wireVert;
				wireVert.vert = i;
				wireVert.u = closest;
				float portion = dist / falloff;
				wireVert.w = (1f - portion * portion) * (1f - portion * portion);
				vertList.Add(wireVert);
			}
		}

		wireVerts = vertList.ToArray();
		Debug.Log("found " + wireVerts.Length + " verts within falloff.");

		return true;
	}

	public override void Modify(MegaModifiers mc)
	{
		// Copy the verts to start
		verts.CopyTo(sverts, 0);
		/*
		int vertCount = verts.Length;
		for (int i = 0; i < vertCount; i++) {
			sverts[i] = verts[i];
		}
		*/

		// Find [resolution] offsets from source to target shape
		bool type = false;
		int knot = 0;
		float alpha = 0f;
		float step = 1f / (resolution - 1);
		Matrix4x4 trans = transform.worldToLocalMatrix * target.transform.localToWorldMatrix;
		for ( int i = 0; i < resolution; i++ )
		{
			//targetOffsets[i] = trans.MultiplyPoint3x4(target.Interpolate(alpha, type, ref knot)) - sourcePositions[i];
			targetOffsets[i] = trans.MultiplyPoint3x4(target.InterpCurve3D(0, alpha, type)) - sourcePositions[i];
			alpha += step;
		}

		int wireVertCount = wireVerts.Length;
		Debug.Log("Modify() moving " + wireVertCount + " verts.");
		for ( int i = 0; i < wireVertCount; i++ )
		{
			sverts[wireVerts[i].vert] += targetOffsets[wireVerts[i].u] * wireVerts[i].w;
		}
	}
}
#endif