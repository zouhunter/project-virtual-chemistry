
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MegaWrapRef : MonoBehaviour
{
	public float			gap				= 0.0f;
	public float			shrink			= 1.0f;
	//public List<int>		neededVerts		= new List<int>();
	public Vector3[]		skinnedVerts;
	public Mesh				mesh			= null;
	public Vector3			offset			= Vector3.zero;
	public bool				targetIsSkin	= false;
	public bool				sourceIsSkin	= false;
	public int				nomapcount		= 0;
	public Matrix4x4[]		bindposes;
	//public BoneWeight[]		boneweights;
	public Transform[]		bones;
	public float			size			= 0.01f;
	public int				vertindex		= 0;
	//public Vector3[]		freeverts;	// position for any vert with no attachments
	//public Vector3[]		startverts;
	public Vector3[]		verts;
	//public MegaBindVert[]	bindverts;
	public MegaModifyObject	target;
	//bool					skinned			= false;
	public float			maxdist			= 0.25f;
	public int				maxpoints		= 4;
	public bool				WrapEnabled		= true;

	public MegaWrap			source;

	struct MegaCloseFace
	{
		public int		face;
		public float	dist;
	}

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=3709");
	}

	Vector4 Plane(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		Vector3 normal = Vector4.zero;
		normal.x = (v2.y - v1.y) * (v3.z - v1.z) - (v2.z - v1.z) * (v3.y - v1.y);
		normal.y = (v2.z - v1.z) * (v3.x - v1.x) - (v2.x - v1.x) * (v3.z - v1.z);
		normal.z = (v2.x - v1.x) * (v3.y - v1.y) - (v2.y - v1.y) * (v3.x - v1.x);

		normal = normal.normalized;
		return new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(v2, normal));
	}

	float PlaneDist(Vector3 p, Vector4 plane)
	{
		Vector3 n = plane;
		return Vector3.Dot(n, p) + plane.w;
	}

	float GetDistance(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		return MegaNearestPointTest.DistPoint3Triangle3Dbl(p, p0, p1, p2);
	}

	float GetPlaneDistance(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector4 pl = Plane(p0, p1, p2);
		return PlaneDist(p, pl);
		//return MegaNearestPointTest.DistPoint3Triangle3Dbl(p, p0, p1, p2);
	}

	public Vector3 MyBary(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector3 bary = Vector3.zero;
		Vector3 normal = FaceNormal(p0, p1, p2);

		float areaABC = Vector3.Dot(normal, Vector3.Cross((p1 - p0), (p2 - p0)));
		float areaPBC = Vector3.Dot(normal, Vector3.Cross((p1 - p), (p2 - p)));
		float areaPCA = Vector3.Dot(normal, Vector3.Cross((p2 - p), (p0 - p)));

		bary.x = areaPBC / areaABC; // alpha
		bary.y = areaPCA / areaABC; // beta
		bary.z = 1.0f - bary.x - bary.y; // gamma
		return bary;
	}

	public Vector3 MyBary1(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
		float d00 = Vector3.Dot(v0, v0);
		float d01 = Vector3.Dot(v0, v1);
		float d11 = Vector3.Dot(v1, v1);
		float d20 = Vector3.Dot(v2, v0);
		float d21 = Vector3.Dot(v2, v1);
		float denom = d00 * d11 - d01 * d01;

		float w = (d11 * d20 - d01 * d21) / denom;
		float v = (d00 * d21 - d01 * d20) / denom;
		float u = 1.0f - v - w;
		return new Vector3(u, v, w);
	}

	public Vector3 CalcBary(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		//return MegaNearestPointTest.oBary;	//mTriangleBary;
		return MyBary(p, p0, p1, p2);
	}

	public float CalcArea(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector3 e1 = p1 - p0;
		Vector3 e2 = p2 - p0;
		Vector3 e3 = Vector3.Cross(e1, e2);
		return 0.5f * e3.magnitude;
	}

	public Vector3 FaceNormal(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector3 e1 = p1 - p0;
		Vector3 e2 = p2 - p0;
		return Vector3.Cross(e1, e2);
	}

	Mesh CloneMesh(Mesh m)
	{
		Mesh clonemesh = new Mesh();
		clonemesh.vertices = m.vertices;
		clonemesh.uv2 = m.uv2;
		clonemesh.uv2 = m.uv2;
		clonemesh.uv = m.uv;
		clonemesh.normals = m.normals;
		clonemesh.tangents = m.tangents;
		clonemesh.colors = m.colors;

		clonemesh.subMeshCount = m.subMeshCount;

		for ( int s = 0; s < m.subMeshCount; s++ )
			clonemesh.SetTriangles(m.GetTriangles(s), s);

		clonemesh.boneWeights = m.boneWeights;
		clonemesh.bindposes = m.bindposes;
		clonemesh.name = m.name + "_copy";
		clonemesh.RecalculateBounds();
		return clonemesh;
	}

	[ContextMenu("Reset Mesh")]
	public void ResetMesh()
	{
		if ( mesh && source )
		{
			mesh.vertices = source.startverts;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
		}

		target = null;
		//bindverts = null;
	}

	public void Attach(MegaModifyObject modobj)
	{
		targetIsSkin = false;
		sourceIsSkin = false;

		//if ( mesh && startverts != null )
			//mesh.vertices = startverts;

		//if ( modobj == null )
		//{
		//	bindverts = null;
		//	return;
		//}

		nomapcount = 0;

		//if ( mesh )
		//	mesh.vertices = startverts;

		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh srcmesh = null;

		if ( mf != null )
		{
			//skinned = false;
			srcmesh = mf.mesh;
		}
		else
		{
			SkinnedMeshRenderer smesh = (SkinnedMeshRenderer)GetComponent(typeof(SkinnedMeshRenderer));

			if ( smesh != null )
			{
				//skinned = true;
				srcmesh = smesh.sharedMesh;
				sourceIsSkin = true;
			}
		}

		if ( mesh == null )
			mesh = CloneMesh(srcmesh);	//mf.mesh);

		if ( mf )
			mf.mesh = mesh;
		else
		{
			SkinnedMeshRenderer smesh = (SkinnedMeshRenderer)GetComponent(typeof(SkinnedMeshRenderer));
			smesh.sharedMesh = mesh;
		}

		if ( sourceIsSkin == false )
		{
			SkinnedMeshRenderer tmesh = (SkinnedMeshRenderer)modobj.GetComponent(typeof(SkinnedMeshRenderer));
			if ( tmesh != null )
			{
				targetIsSkin = true;

				if ( !sourceIsSkin )
				{
					Mesh sm = tmesh.sharedMesh;
					bindposes = sm.bindposes;
					//boneweights = sm.boneWeights;
					bones = tmesh.bones;
					skinnedVerts = sm.vertices;	//new Vector3[sm.vertexCount];
				}
			}
		}

		//neededVerts.Clear();

		verts = mesh.vertices;

		//startverts = mesh.vertices;
		//freeverts = new Vector3[startverts.Length];
		//Vector3[] baseverts = modobj.verts;	//basemesh.vertices;
		//int[] basefaces = modobj.tris;	//basemesh.triangles;

		//bindverts = new MegaBindVert[verts.Length];

#if false
		// matrix to get vertex into local space of target
		Matrix4x4 tm = transform.localToWorldMatrix * modobj.transform.worldToLocalMatrix;

		List<MegaCloseFace> closefaces = new List<MegaCloseFace>();

		Vector3 p0 = Vector3.zero;
		Vector3 p1 = Vector3.zero;
		Vector3 p2 = Vector3.zero;

		for ( int i = 0; i < verts.Length; i++ )
		{
			MegaBindVert bv = new MegaBindVert();
			bindverts[i] = bv;

			Vector3 p = tm.MultiplyPoint(verts[i]);

			p = transform.TransformPoint(verts[i]);
			p = modobj.transform.InverseTransformPoint(p);
			freeverts[i] = p;

			closefaces.Clear();

			for ( int t = 0; t < basefaces.Length; t += 3 )
			{
				if ( targetIsSkin && !sourceIsSkin )
				{
					p0 = modobj.transform.InverseTransformPoint(GetSkinPos(basefaces[t]));
					p1 = modobj.transform.InverseTransformPoint(GetSkinPos(basefaces[t + 1]));
					p2 = modobj.transform.InverseTransformPoint(GetSkinPos(basefaces[t + 2]));
				}
				else
				{
					p0 = baseverts[basefaces[t]];
					p1 = baseverts[basefaces[t + 1]];
					p2 = baseverts[basefaces[t + 2]];
				}

				float dist = GetDistance(p, p0, p1, p2);

				if ( Mathf.Abs(dist) < maxdist )
				{
					MegaCloseFace cf = new MegaCloseFace();
					cf.dist = Mathf.Abs(dist);
					cf.face = t;

					bool inserted = false;
					for ( int k = 0; k < closefaces.Count; k++ )
					{
						if ( cf.dist < closefaces[k].dist )
						{
							closefaces.Insert(k, cf);
							inserted = true;
							break;
						}
					}

					if ( !inserted )
						closefaces.Add(cf);
				}
			}

			float tweight = 0.0f;
			int maxp = maxpoints;
			if ( maxp == 0 )
				maxp = closefaces.Count;

			for ( int j = 0; j < maxp; j++ )
			{
				if ( j < closefaces.Count )
				{
					int t = closefaces[j].face;

					if ( targetIsSkin && !sourceIsSkin )
					{
						p0 = modobj.transform.InverseTransformPoint(GetSkinPos(basefaces[t]));
						p1 = modobj.transform.InverseTransformPoint(GetSkinPos(basefaces[t + 1]));
						p2 = modobj.transform.InverseTransformPoint(GetSkinPos(basefaces[t + 2]));
					}
					else
					{
						p0 = baseverts[basefaces[t]];
						p1 = baseverts[basefaces[t + 1]];
						p2 = baseverts[basefaces[t + 2]];
					}

					Vector3 normal = FaceNormal(p0, p1, p2);

					float dist = closefaces[j].dist;	//GetDistance(p, p0, p1, p2);

					MegaBindInf bi = new MegaBindInf();
					bi.dist = GetPlaneDistance(p, p0, p1, p2);	//dist;
					bi.face = t;
					bi.i0 = basefaces[t];
					bi.i1 = basefaces[t + 1];
					bi.i2 = basefaces[t + 2];
					bi.bary = CalcBary(p, p0, p1, p2);
					bi.weight = 1.0f / (1.0f + dist);
					bi.area = normal.magnitude * 0.5f;	//CalcArea(baseverts[basefaces[t]], baseverts[basefaces[t + 1]], baseverts[basefaces[t + 2]]);	// Could calc once at start
					tweight += bi.weight;
					bv.verts.Add(bi);
				}
			}

			if ( maxpoints > 0 && maxpoints < bv.verts.Count )
				bv.verts.RemoveRange(maxpoints, bv.verts.Count - maxpoints);

			// Only want to calculate skin vertices we use
			if ( !sourceIsSkin && targetIsSkin )
			{
				for ( int fi = 0; fi < bv.verts.Count; fi++ )
				{
					if ( !neededVerts.Contains(bv.verts[fi].i0) )
						neededVerts.Add(bv.verts[fi].i0);

					if ( !neededVerts.Contains(bv.verts[fi].i1) )
						neededVerts.Add(bv.verts[fi].i1);

					if ( !neededVerts.Contains(bv.verts[fi].i2) )
						neededVerts.Add(bv.verts[fi].i2);
				}
			}

			if ( tweight == 0.0f )
				nomapcount++;

			bv.weight = tweight;
		}
#endif
	}

	void LateUpdate()
	{
		DoUpdate();
	}

	Vector3 GetSkinPos(MegaWrap src, int i)
	{
		Vector3 pos = target.sverts[i];
		Vector3 bpos = bindposes[src.boneweights[i].boneIndex0].MultiplyPoint(pos);
		Vector3 p = bones[src.boneweights[i].boneIndex0].TransformPoint(bpos) * src.boneweights[i].weight0;

		bpos = bindposes[src.boneweights[i].boneIndex1].MultiplyPoint(pos);
		p += bones[src.boneweights[i].boneIndex1].TransformPoint(bpos) * src.boneweights[i].weight1;

		bpos = bindposes[src.boneweights[i].boneIndex2].MultiplyPoint(pos);
		p += bones[src.boneweights[i].boneIndex2].TransformPoint(bpos) * src.boneweights[i].weight2;

		bpos = bindposes[src.boneweights[i].boneIndex3].MultiplyPoint(pos);
		p += bones[src.boneweights[i].boneIndex3].TransformPoint(bpos) * src.boneweights[i].weight3;

		return p;
	}

	public Vector3 GetCoordMine(Vector3 A, Vector3 B, Vector3 C, Vector3 bary)
	{
		Vector3 p = Vector3.zero;
		p.x = (bary.x * A.x) + (bary.y * B.x) + (bary.z * C.x);
		p.y = (bary.x * A.y) + (bary.y * B.y) + (bary.z * C.y);
		p.z = (bary.x * A.z) + (bary.y * B.z) + (bary.z * C.z);

		return p;
	}

	// Weight is 1 / (1 + dist)
	void DoUpdate()
	{
		if ( source == null || WrapEnabled == false || target == null || source.bindverts == null )	//|| bindposes == null )
			return;

		//if ( neededVerts != null && neededVerts.Count > 0 )
		if ( targetIsSkin && source.neededVerts != null && source.neededVerts.Count > 0 )
		{
			if ( source.boneweights == null )
			{
				SkinnedMeshRenderer tmesh = (SkinnedMeshRenderer)target.GetComponent(typeof(SkinnedMeshRenderer));
				if ( tmesh != null )
				{
					if ( !sourceIsSkin )
					{
						Mesh sm = tmesh.sharedMesh;
						bindposes = sm.bindposes;
						source.boneweights = sm.boneWeights;
					}
				}
			}

			for ( int i = 0; i < source.neededVerts.Count; i++ )
				skinnedVerts[source.neededVerts[i]] = GetSkinPos(source, source.neededVerts[i]);
		}

		//Debug.Log("1");
		Vector3 p = Vector3.zero;
		if ( targetIsSkin && !sourceIsSkin )
		{
			for ( int i = 0; i < source.bindverts.Length; i++ )
			{
				if ( source.bindverts[i].verts.Count > 0 )
				{
					p = Vector3.zero;

					for ( int j = 0; j < source.bindverts[i].verts.Count; j++ )
					{
						MegaBindInf bi = source.bindverts[i].verts[j];

						Vector3 p0 = skinnedVerts[bi.i0];
						Vector3 p1 = skinnedVerts[bi.i1];
						Vector3 p2 = skinnedVerts[bi.i2];

						Vector3 cp = GetCoordMine(p0, p1, p2, bi.bary);
						Vector3 norm = FaceNormal(p0, p1, p2);
						cp += ((bi.dist * shrink) + gap) * norm.normalized;
						p += cp * (bi.weight / source.bindverts[i].weight);
					}

					verts[i] = transform.InverseTransformPoint(p) + offset;
				}
			}
		}
		else
		{
			for ( int i = 0; i < source.bindverts.Length; i++ )
			{
				if ( source.bindverts[i].verts.Count > 0 )
				{
					p = Vector3.zero;

					for ( int j = 0; j < source.bindverts[i].verts.Count; j++ )
					{
						MegaBindInf bi = source.bindverts[i].verts[j];

						Vector3 p0 = target.sverts[bi.i0];
						Vector3 p1 = target.sverts[bi.i1];
						Vector3 p2 = target.sverts[bi.i2];

						Vector3 cp = GetCoordMine(p0, p1, p2, bi.bary);
						Vector3 norm = FaceNormal(p0, p1, p2);
						cp += ((bi.dist * shrink) + gap) * norm.normalized;
						p += cp * (bi.weight / source.bindverts[i].weight);
					}
				}
				else
					p = source.freeverts[i];	//startverts[i];

				p = target.transform.TransformPoint(p);
				verts[i] = transform.InverseTransformPoint(p) + offset;
			}
		}

		mesh.vertices = verts;
		mesh.RecalculateNormals();	// Need Mega method here
		mesh.RecalculateBounds();
	}
}
// 994
// 510