
using UnityEngine;
using UnityEditor;
using System.IO;

#if false
// May want a window for this for options
public class MegaPrefabMaker : MonoBehaviour
{
	[MenuItem("GameObject/Prefab Maker")]
	static void PrefabMaker()
	{
		GameObject from = Selection.activeGameObject;

		if ( from != null )
		{
			if ( !Directory.Exists("Assets/MegaPrefabs") )	// Have a path option
			{
				AssetDatabase.CreateFolder("Assets", "MegaPrefabs");
			}


			GameObject prefab = PrefabUtility.CreatePrefab("Assets/MegaPrefabs/" + Selection.activeGameObject.name + ".prefab", from);

			// So we need to replace all the meshes, and then have modifier methods for duplicating data

			// Check prefab folder exists, if not make it
			// Create the prefab object from the from object

			// clone any components on the object

			// run through every child and clone what we find
		}

	}

	void AddChildren(GameObject child, GameObject parent)
	{
		if ( child )
		{
			// Clone the object to a prefab, parent is the prefab parent
			GameObject prefab = null;

			for ( int i = 0; i < child.transform.childCount; i++ )
			{
				// Clone the object to a prefab
				AddChildren(child.transform.GetChild(i).gameObject, prefab);
			}
		}
	}


	static public GameObject CloneMeshes(GameObject from, GameObject to)
	{
		//GameObject clone = null;
		if ( from && to )
		{
			//clone = (GameObject)GameObject.Instantiate(subject);

			SkinnedMeshRenderer[] fromskinmeshes = from.GetComponentsInChildren<SkinnedMeshRenderer>();
			SkinnedMeshRenderer[] toskinmeshes = to.GetComponentsInChildren<SkinnedMeshRenderer>();

			int l = fromskinmeshes.Length;

			for ( int i = 0; i < l; i++ )
			{
				Mesh mesh = fromskinmeshes[i].sharedMesh;
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
					clonemesh.SetTriangles(mesh.GetTriangles(s), s);

				clonemesh.boneWeights = mesh.boneWeights;
				clonemesh.bindposes = mesh.bindposes;
				clonemesh.name = mesh.name + "_copy";
				clonemesh.RecalculateBounds();
				toskinmeshes[i].sharedMesh = clonemesh;

				AssetDatabase.AddObjectToAsset(clonemesh, to);
			}

			MeshFilter[] mfs = from.GetComponentsInChildren<MeshFilter>();
			MeshFilter[] clonemfs = to.GetComponentsInChildren<MeshFilter>();

			MeshCollider[] mcs = from.GetComponentsInChildren<MeshCollider>();
			MeshCollider[] clonemcs = to.GetComponentsInChildren<MeshCollider>();

			for ( int i = 0; i < mfs.Length; i++ )
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
					clonemesh.SetTriangles(mesh.GetTriangles(s), s);

				clonemesh.boneWeights = mesh.boneWeights;
				clonemesh.bindposes = mesh.bindposes;
				clonemesh.name = mesh.name + "_copy";
				clonemesh.RecalculateBounds();
				clonemf.sharedMesh = clonemesh;

				AssetDatabase.AddObjectToAsset(clonemesh, to);

				for ( int j = 0; j < mcs.Length; j++ )
				{
					MeshCollider mc = mcs[j];
					if ( mc.sharedMesh = mesh )
						clonemcs[j].sharedMesh = clonemesh;
				}
			}

			MegaModifyObject[] modobjs = to.GetComponentsInChildren<MegaModifyObject>();

			for ( int i = 0; i < modobjs.Length; i++ )
			{
				modobjs[i].MeshUpdated();
			}
		}
	}
}
#endif