
using UnityEngine;

[ExecuteInEditMode]
public class MegaAttach : MonoBehaviour
{
	public MegaModifiers	target;
	[HideInInspector]
	public Vector3			BaryCoord = Vector3.zero;
	[HideInInspector]
	public int[]			BaryVerts = new int[3];
	[HideInInspector]
	public bool				attached = false;
	[HideInInspector]
	public Vector3			BaryCoord1 = Vector3.zero;
	[HideInInspector]
	public int[]			BaryVerts1 = new int[3];
	public Vector3			attachforward = Vector3.forward;
	public Vector3			AxisRot = Vector3.zero;
	public float			radius = 0.1f;
	public Vector3			up = Vector3.up;
	public bool				worldSpace = false;
	Vector3					pt = Vector3.zero;
	Vector3					norm = Vector3.zero;
	public bool				skinned;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2645");
	}
	
	public void DetachIt()
	{
		attached = false;
	}

	public void AttachIt()
	{
		if ( target )
		{
			attached = true;

			if ( !InitSkin() )
			{
				Mesh mesh = target.mesh;
				Vector3 objSpacePt = target.transform.InverseTransformPoint(pt);
				Vector3[] verts = target.sverts;
				int[] tris = mesh.triangles;
				int index = -1;
				MegaNearestPointTest.NearestPointOnMesh1(objSpacePt, verts, tris, ref index, ref BaryCoord);

				if ( index >= 0 )
				{
					BaryVerts[0] = tris[index];
					BaryVerts[1] = tris[index + 1];
					BaryVerts[2] = tris[index + 2];
				}

				MegaNearestPointTest.NearestPointOnMesh1(objSpacePt + attachforward, verts, tris, ref index, ref BaryCoord1);

				if ( index >= 0 )
				{
					BaryVerts1[0] = tris[index];
					BaryVerts1[1] = tris[index + 1];
					BaryVerts1[2] = tris[index + 2];
				}
			}
		}
	}

	void OnDrawGizmosSelected()
	{
		pt = transform.position;

		Gizmos.color = Color.white;
		Gizmos.DrawSphere(pt, radius);

		if ( target )
		{
			if ( attached )
			{
				SkinnedMeshRenderer skin = target.GetComponent<SkinnedMeshRenderer>();

				if ( skin )
				{
					Vector3 pos = transform.position;

					Vector3 worldPt = pos;
					Gizmos.color = Color.green;
					Gizmos.DrawSphere(worldPt, radius);
				}
				else
				{
					Vector3 pos = GetCoordMine(target.sverts[BaryVerts[0]], target.sverts[BaryVerts[1]], target.sverts[BaryVerts[2]], BaryCoord);

					Vector3 worldPt = target.transform.TransformPoint(pos);
					Gizmos.color = Color.green;
					Gizmos.DrawSphere(worldPt, radius);
					Vector3 nw = target.transform.TransformDirection(norm * 40.0f);
					Gizmos.DrawLine(worldPt, worldPt + nw);
				}
			}
			else
			{
				SkinnedMeshRenderer skin = target.GetComponent<SkinnedMeshRenderer>();

				if ( skin )
				{
					CalcSkinVerts();
					Mesh mesh = target.mesh;
					Vector3 objSpacePt = pt;
					Vector3[] verts = calcskinverts;
					int[] tris = mesh.triangles;
					int index = -1;
					Vector3 tribary = Vector3.zero;
					Vector3 meshPt = MegaNearestPointTest.NearestPointOnMesh1(objSpacePt, verts, tris, ref index, ref tribary);
					Vector3 worldPt = target.transform.TransformPoint(meshPt);

					if ( index >= 0 )
					{
						Vector3 cp2 = GetCoordMine(verts[tris[index]], verts[tris[index + 1]], verts[tris[index + 2]], tribary);
						worldPt = cp2;
					}

					Gizmos.color = Color.red;
					Gizmos.DrawSphere(worldPt, radius);

					Gizmos.color = Color.blue;
					meshPt = MegaNearestPointTest.NearestPointOnMesh1(objSpacePt + attachforward, verts, tris, ref index, ref tribary);
					Vector3 worldPt1 = meshPt;

					Gizmos.DrawSphere(worldPt1, radius);

					Gizmos.color = Color.yellow;
					Gizmos.DrawLine(worldPt, worldPt1);
				}
				else
				{
					Mesh mesh = target.mesh;
					Vector3 objSpacePt = target.transform.InverseTransformPoint(pt);
					Vector3[] verts = target.sverts;
					int[] tris = mesh.triangles;
					int index = -1;
					Vector3 tribary = Vector3.zero;
					Vector3 meshPt = MegaNearestPointTest.NearestPointOnMesh1(objSpacePt, verts, tris, ref index, ref tribary);
					Vector3 worldPt = target.transform.TransformPoint(meshPt);

					if ( index >= 0 )
					{
						Vector3 cp2 = GetCoordMine(verts[tris[index]], verts[tris[index + 1]], verts[tris[index + 2]], tribary);
						worldPt = target.transform.TransformPoint(cp2);
					}

					Gizmos.color = Color.red;
					Gizmos.DrawSphere(worldPt, radius);

					Gizmos.color = Color.blue;
					meshPt = MegaNearestPointTest.NearestPointOnMesh1(objSpacePt + attachforward, verts, tris, ref index, ref tribary);
					Vector3 worldPt1 = target.transform.TransformPoint(meshPt);

					Gizmos.DrawSphere(worldPt1, radius);

					Gizmos.color = Color.yellow;
					Gizmos.DrawLine(worldPt, worldPt1);
				}
			}
		}
	}

	void LateUpdate()
	{
		if ( attached )
		{
			if ( skinned )
			{
				GetSkinPos1();
				return;
			}

			if ( worldSpace )
			{
				Vector3 v0 = target.sverts[BaryVerts[0]];
				Vector3 v1 = target.sverts[BaryVerts[1]];
				Vector3 v2 = target.sverts[BaryVerts[2]];

				Vector3 pos = target.transform.localToWorldMatrix.MultiplyPoint(GetCoordMine(v0, v1, v2, BaryCoord));

				transform.position = pos;

				// Rotation
				Vector3 va = v1 - v0;
				Vector3 vb = v2 - v1;

				norm = Vector3.Cross(va, vb);

				v0 = target.sverts[BaryVerts1[0]];
				v1 = target.sverts[BaryVerts1[1]];
				v2 = target.sverts[BaryVerts1[2]];

				Vector3 fwd = target.transform.localToWorldMatrix.MultiplyPoint(GetCoordMine(v0, v1, v2, BaryCoord1)) - pos;

				Quaternion erot = Quaternion.Euler(AxisRot);
				Quaternion rot = Quaternion.LookRotation(fwd, norm) * erot;
				transform.rotation = rot;
			}
			else
			{
				Vector3 v0 = target.sverts[BaryVerts[0]];
				Vector3 v1 = target.sverts[BaryVerts[1]];
				Vector3 v2 = target.sverts[BaryVerts[2]];

				Vector3 pos = GetCoordMine(v0, v1, v2, BaryCoord);

				transform.localPosition = pos;

				// Rotation
				Vector3 va = v1 - v0;
				Vector3 vb = v2 - v1;

				norm = Vector3.Cross(va, vb);

				v0 = target.sverts[BaryVerts1[0]];
				v1 = target.sverts[BaryVerts1[1]];
				v2 = target.sverts[BaryVerts1[2]];

				Vector3 fwd = GetCoordMine(v0, v1, v2, BaryCoord1) - pos;

				Quaternion erot = Quaternion.Euler(AxisRot);
				Quaternion rot = Quaternion.LookRotation(fwd, norm) * erot;
				transform.localRotation = rot;
			}
		}
	}

	Vector3 GetCoordMine(Vector3 A, Vector3 B, Vector3 C, Vector3 bary)
	{
		Vector3 p = Vector3.zero;
		p.x = (bary.x * A.x) + (bary.y * B.x) + (bary.z * C.x);
		p.y = (bary.x * A.y) + (bary.y * B.y) + (bary.z * C.y);
		p.z = (bary.x * A.z) + (bary.y * B.z) + (bary.z * C.z);

		return p;
	}

	[System.Serializable]
	public class MegaSkinVert
	{
		public MegaSkinVert()
		{
			weights = new float[4];
			bones = new Transform[4];
			bindposes = new Matrix4x4[4];
		}

		public float[]		weights;
		public Transform[]	bones;
		public Matrix4x4[]	bindposes;
		public int			vert;
	}

	public MegaSkinVert[]	skinverts;

	bool InitSkin()
	{
		if ( target )
		{
			SkinnedMeshRenderer skin = target.GetComponent<SkinnedMeshRenderer>();

			if ( skin )
			{
				Quaternion rot = transform.rotation;
				attachrot = Quaternion.identity;

				skinned = true;

				Mesh ms = skin.sharedMesh;

				Vector3 pt = transform.position;

				CalcSkinVerts();
				Vector3 objSpacePt = pt;
				Vector3[] verts = calcskinverts;
				int[] tris = ms.triangles;
				int index = -1;
				//Vector3 tribary = Vector3.zero;
				MegaNearestPointTest.NearestPointOnMesh1(objSpacePt, verts, tris, ref index, ref BaryCoord);	//ref tribary);

				if ( index >= 0 )
				{
					BaryVerts[0] = tris[index];
					BaryVerts[1] = tris[index + 1];
					BaryVerts[2] = tris[index + 2];
				}

				//Vector3 worldPt = target.transform.TransformPoint(meshPt);

				//if ( index >= 0 )
				//{
					//Vector3 cp2 = GetCoordMine(verts[tris[index]], verts[tris[index + 1]], verts[tris[index + 2]], tribary);
					//worldPt = cp2;
				//}

				MegaNearestPointTest.NearestPointOnMesh1(objSpacePt + attachforward, verts, tris, ref index, ref BaryCoord1);

				if ( index >= 0 )
				{
					BaryVerts1[0] = tris[index];
					BaryVerts1[1] = tris[index + 1];
					BaryVerts1[2] = tris[index + 2];
				}

				skinverts = new MegaSkinVert[6];

				for ( int i = 0; i < 3; i++ )
				{
					int vert = BaryVerts[i];
					BoneWeight bw = ms.boneWeights[vert];
					skinverts[i] = new MegaSkinVert();

					skinverts[i].vert = vert;
					skinverts[i].weights[0] = bw.weight0;
					skinverts[i].weights[1] = bw.weight1;
					skinverts[i].weights[2] = bw.weight2;
					skinverts[i].weights[3] = bw.weight3;

					skinverts[i].bones[0] = skin.bones[bw.boneIndex0];
					skinverts[i].bones[1] = skin.bones[bw.boneIndex1];
					skinverts[i].bones[2] = skin.bones[bw.boneIndex2];
					skinverts[i].bones[3] = skin.bones[bw.boneIndex3];

					skinverts[i].bindposes[0] = ms.bindposes[bw.boneIndex0];
					skinverts[i].bindposes[1] = ms.bindposes[bw.boneIndex1];
					skinverts[i].bindposes[2] = ms.bindposes[bw.boneIndex2];
					skinverts[i].bindposes[3] = ms.bindposes[bw.boneIndex3];
				}

				for ( int i = 3; i < 6; i++ )
				{
					int vert = BaryVerts1[i - 3];
					BoneWeight bw = ms.boneWeights[vert];
					skinverts[i] = new MegaSkinVert();

					skinverts[i].vert = vert;

					skinverts[i].weights[0] = bw.weight0;
					skinverts[i].weights[1] = bw.weight1;
					skinverts[i].weights[2] = bw.weight2;
					skinverts[i].weights[3] = bw.weight3;

					skinverts[i].bones[0] = skin.bones[bw.boneIndex0];
					skinverts[i].bones[1] = skin.bones[bw.boneIndex1];
					skinverts[i].bones[2] = skin.bones[bw.boneIndex2];
					skinverts[i].bones[3] = skin.bones[bw.boneIndex3];

					skinverts[i].bindposes[0] = ms.bindposes[bw.boneIndex0];
					skinverts[i].bindposes[1] = ms.bindposes[bw.boneIndex1];
					skinverts[i].bindposes[2] = ms.bindposes[bw.boneIndex2];
					skinverts[i].bindposes[3] = ms.bindposes[bw.boneIndex3];
				}

				//Vector3 up = transform.up;
				//Vector3 fwd = transform.forward;
				GetSkinPos1();
				attachrot = Quaternion.Inverse(transform.rotation) * rot;

				//Debug.Log("arot " + attachrot.eulerAngles);
				return true;
			}
			else
				skinned = false;
		}

		return false;
	}

	public Quaternion	attachrot = Quaternion.identity;

	Vector3 GetSkinPos(int i)
	{
		Vector3 pos = target.sverts[skinverts[i].vert];
		Vector3 bpos = skinverts[i].bindposes[0].MultiplyPoint(pos);
		Vector3 p = skinverts[i].bones[0].TransformPoint(bpos) * skinverts[i].weights[0];

		bpos = skinverts[i].bindposes[1].MultiplyPoint(pos);
		p += skinverts[i].bones[1].TransformPoint(bpos) * skinverts[i].weights[1];
		bpos = skinverts[i].bindposes[2].MultiplyPoint(pos);
		p += skinverts[i].bones[2].TransformPoint(bpos) * skinverts[i].weights[2];
		bpos = skinverts[i].bindposes[3].MultiplyPoint(pos);
		p += skinverts[i].bones[3].TransformPoint(bpos) * skinverts[i].weights[3];
		return p;
	}

	Vector3[]	calcskinverts;

	void CalcSkinVerts()
	{
		if ( calcskinverts == null || calcskinverts.Length != target.sverts.Length )
			calcskinverts = new Vector3[target.sverts.Length];

		SkinnedMeshRenderer skin = target.GetComponent<SkinnedMeshRenderer>();
		Mesh mesh = target.mesh;
		Matrix4x4[] bindposes = mesh.bindposes;
		BoneWeight[] boneweights = mesh.boneWeights;

		for ( int i = 0; i < target.sverts.Length; i++ )
		{
			Vector3 p = Vector3.zero;

			Vector3 pos = target.sverts[i];
			Vector3 bpos = bindposes[boneweights[i].boneIndex0].MultiplyPoint(pos);
			p += skin.bones[boneweights[i].boneIndex0].TransformPoint(bpos) * boneweights[i].weight0;

			bpos = bindposes[boneweights[i].boneIndex1].MultiplyPoint(pos);
			p += skin.bones[boneweights[i].boneIndex1].TransformPoint(bpos) * boneweights[i].weight1;

			bpos = bindposes[boneweights[i].boneIndex2].MultiplyPoint(pos);
			p += skin.bones[boneweights[i].boneIndex2].TransformPoint(bpos) * boneweights[i].weight2;

			bpos = bindposes[boneweights[i].boneIndex3].MultiplyPoint(pos);
			p += skin.bones[boneweights[i].boneIndex3].TransformPoint(bpos) * boneweights[i].weight3;

			calcskinverts[i] = p;
		}
	}

	void GetSkinPos1()
	{
		Vector3 v0 = GetSkinPos(0);
		Vector3 v1 = GetSkinPos(1);
		Vector3 v2 = GetSkinPos(2);

		Vector3 pos = GetCoordMine(v0, v1, v2, BaryCoord);

		transform.position = pos;

		Vector3 va = v1 - v0;
		Vector3 vb = v2 - v1;

		norm = Vector3.Cross(va, vb);

		//Debug.Log("norm " + norm.ToString("0.000"));
		v0 = GetSkinPos(3);
		v1 = GetSkinPos(4);
		v2 = GetSkinPos(5);

		Vector3 fwd = GetCoordMine(v0, v1, v2, BaryCoord1) - pos;

		//Debug.Log("fwd " + fwd.ToString("0.000"));

		Quaternion erot = Quaternion.Euler(AxisRot);
		Quaternion rot = Quaternion.LookRotation(fwd, norm) * erot * attachrot;
		transform.rotation = rot;
	}
}