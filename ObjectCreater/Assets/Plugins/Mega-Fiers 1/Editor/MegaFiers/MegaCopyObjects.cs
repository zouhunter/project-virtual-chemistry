
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;

#if !UNITY_FLASH && !UNITY_METRO && !UNITY_WP8
public class MegaCopyObjects : MonoBehaviour
{
	[MenuItem("GameObject/Mega Instance Object")]
	static void InstanceModifiedMesh()
	{
		GameObject from = Selection.activeGameObject;
		MegaCopyObject.InstanceObject(from);
	}

	[MenuItem("GameObject/Mega Copy Object")]
	static void DoCopyObjects()
	{
		GameObject from = Selection.activeGameObject;
		MegaCopyObject.DoCopyObjects(from);
	}

	[MenuItem("GameObject/Mega Copy Hier")]
	static void DoCopyObjectsHier()
	{
		GameObject from = Selection.activeGameObject;
		MegaCopyObject.DoCopyObjectsChildren(from);
	}

	static Mesh CloneMesh(Mesh mesh)
	{
		Mesh clonemesh = new Mesh();
		clonemesh.vertices = mesh.vertices;
		clonemesh.uv2 = mesh.uv2;
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

		return clonemesh;
	}

	[MenuItem("GameObject/Create Mega Prefab")]
	static void AllNewCreateSimplePrefab()
	{
		if ( Selection.activeGameObject != null )
		{
			if ( !Directory.Exists("Assets/MegaPrefabs") )
				AssetDatabase.CreateFolder("Assets", "MegaPrefabs");

			GameObject obj = Selection.activeGameObject;

			// Make a copy?
			GameObject newobj = MegaCopyObject.DoCopyObjects(obj);
			newobj.name = obj.name;

			// Get all modifyObjects in children
			MegaModifyObject[] mods = newobj.GetComponentsInChildren<MegaModifyObject>();

			for ( int i = 0; i < mods.Length; i++ )
			{
				// Need method to get the base mesh
				GameObject pobj = mods[i].gameObject;

				// Get the mesh and make an asset for it
				Mesh mesh = MegaUtils.GetSharedMesh(pobj);

				if ( mesh )
				{
					string mname = mesh.name;
					int ix = mname.IndexOf("Instance");
					if ( ix != -1 )
						mname = mname.Remove(ix);

					string meshpath = "Assets/MegaPrefabs/" + mname + ".prefab"; 
					AssetDatabase.CreateAsset(mesh, meshpath);
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			}

			MegaWrap[] wraps = newobj.GetComponentsInChildren<MegaWrap>();

			for ( int i = 0; i < wraps.Length; i++ )
			{
				// Need method to get the base mesh
				GameObject pobj = wraps[i].gameObject;

				// Get the mesh and make an asset for it
				Mesh mesh = MegaUtils.GetSharedMesh(pobj);

				if ( mesh )
				{
					string mname = mesh.name;

					int ix = mname.IndexOf("Instance");
					if ( ix != -1 )
						mname = mname.Remove(ix);

					string meshpath = "Assets/MegaPrefabs/" + mname + ".prefab";
					AssetDatabase.CreateAsset(mesh, meshpath);
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			}

			Object prefab = PrefabUtility.CreateEmptyPrefab("Assets/MegaPrefabs/" + newobj.name + "_Prefab.prefab");
			EditorUtility.ReplacePrefab(newobj, prefab, ReplacePrefabOptions.ConnectToPrefab);
			DestroyImmediate(newobj);
		}
	}
}
#endif