
#if !UNITY_WP8 && !UNITY_METRO
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MegaDeepCopy : MonoBehaviour
{
#if false
	[MenuItem("GameObject/Mega Copy Mesh")]
	static void DeepCopy()
	{
		GameObject subject = Selection.activeGameObject;
		GameObject clone = (GameObject)GameObject.Instantiate(subject);

		MeshFilter[] mfs = subject.GetComponentsInChildren<MeshFilter>();
		MeshFilter[] clonemfs = clone.GetComponentsInChildren<MeshFilter>();

		MeshCollider[] mcs = clone.GetComponentsInChildren<MeshCollider>();
		MeshCollider[] clonemcs = clone.GetComponentsInChildren<MeshCollider>();

		int l = mfs.Length;

		for ( int i = 0; i < l; i++ )
		{
			MeshFilter mf = mfs[i];
			MeshFilter clonemf = clonemfs[i];
			Mesh mesh = mf.sharedMesh;
			Mesh clonemesh = new Mesh();
			clonemesh.vertices = mesh.vertices;
			clonemesh.uv1 = mesh.uv1;
			clonemesh.uv2 = mesh.uv2;
			clonemesh.uv = mesh.uv;
			clonemesh.normals = mesh.normals;
			clonemesh.tangents = mesh.tangents;
			clonemesh.colors = mesh.colors;
			
			clonemesh.subMeshCount = mesh.subMeshCount;

			for ( int s = 0; s < mesh.subMeshCount; s++ )
			{
				clonemesh.SetTriangles(mesh.GetTriangles(s), s);
			}
			
			//clonemesh.triangles = mesh.triangles;

			clonemesh.boneWeights = mesh.boneWeights;
			clonemesh.bindposes = mesh.bindposes;
			clonemesh.name = mesh.name + "_copy";
			clonemesh.RecalculateBounds();
			clonemf.sharedMesh = clonemesh;

			for ( int j = 0; j < mcs.Length; j++ )
			{
				MeshCollider mc = mcs[j];
				if ( mc.sharedMesh = mesh )
					clonemcs[j].sharedMesh = clonemesh;
			}
		}
	}
#endif

	[MenuItem("GameObject/Mega Deep Copy")]
	static void DeepCopyNew()
	{
		MegaCopyObject.DeepCopy(Selection.activeGameObject);
	}
}
#endif