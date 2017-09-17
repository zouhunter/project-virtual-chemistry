using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEditor;

public class MB2_MeshBakerEditorFunctions {
	public static void _bakeIntoCombined(MB2_MeshBakerCommon mom, MB_OutputOptions prefabOrSceneObject){
		if (MB2_MeshCombiner.EVAL_VERSION && prefabOrSceneObject == MB_OutputOptions.bakeIntoPrefab){
			Debug.LogError("Cannot BakeIntoPrefab with evaluation version.");
			return;
		}
		if (prefabOrSceneObject != MB_OutputOptions.bakeIntoPrefab && prefabOrSceneObject != MB_OutputOptions.bakeIntoSceneObject){
			Debug.LogError("Paramater prefabOrSceneObject must be bakeIntoPrefab or bakeIntoSceneObject");
			return;
		}
		//MB2_MeshBakerCommon mom = (MB2_MeshBakerCommon) target;
		MB2_TextureBaker tb = mom.GetComponent<MB2_TextureBaker>();
		if (mom.textureBakeResults == null && tb != null){
			Debug.Log("setting results");
			mom.textureBakeResults = tb.textureBakeResults;	
		}

		if (mom.useObjsToMeshFromTexBaker){
			if (tb != null){
				mom.GetObjectsToCombine().Clear();
				mom.GetObjectsToCombine().AddRange(tb.GetObjectsToCombine());
			}	
		}
		
		Mesh mesh;
		if (!MB2_MeshBakerRoot.doCombinedValidate(mom, MB_ObjsToCombineTypes.sceneObjOnly, new MB2_EditorMethods())) return;	
		if (prefabOrSceneObject == MB_OutputOptions.bakeIntoPrefab && 
			mom.resultPrefab == null){
			Debug.LogError("Need to set the Combined Mesh Prefab field. Create a prefab asset, drag an empty game object into it, and drag it to the 'Combined Mesh Prefab' field.");
			return;
		}
		mom.ClearMesh();
		mesh = mom.AddDeleteGameObjects(mom.GetObjectsToCombine().ToArray(),null,false, true);
		if (mesh != null){
			mom.Apply( Unwrapping.GenerateSecondaryUVSet );
			Debug.Log(String.Format("Successfully baked {0} meshes into a combined mesh",mom.GetObjectsToCombine().Count));
			if (prefabOrSceneObject == MB_OutputOptions.bakeIntoSceneObject){
				PrefabType pt = PrefabUtility.GetPrefabType(mom.resultSceneObject);
				if (pt == PrefabType.Prefab || pt == PrefabType.ModelPrefab){
					Debug.LogError("Combined Mesh Object is a prefab asset. If output option bakeIntoSceneObject then this must be an instance in the scene.");
					return;
				}
			} else if (prefabOrSceneObject == MB_OutputOptions.bakeIntoPrefab){
				string prefabPth = AssetDatabase.GetAssetPath(mom.resultPrefab);
				if (prefabPth == null || prefabPth.Length == 0){
					Debug.LogError("Could not save result to prefab. Result Prefab value is not an Asset.");
					return;
				}
				string baseName = Path.GetFileNameWithoutExtension(prefabPth);
				string folderPath = prefabPth.Substring(0,prefabPth.Length - baseName.Length - 7);		
				string newFilename = folderPath + baseName + "-mesh";
				SaveMeshsToAssetDatabase(mom, folderPath,newFilename);
				
				if (mom.renderType == MB_RenderType.skinnedMeshRenderer){
					Debug.LogWarning("Render type is skinned mesh renderer. " +
							"Can't create prefab until all bones have been added to the combined mesh object " + mom.resultPrefab + 
							" Add the bones then drag the combined mesh object to the prefab.");	
					
				} else {
					RebuildPrefab(mom);
				}
			} else {
				Debug.LogError("Unknown parameter");
			}
		}
		
	}

	public static void SaveMeshsToAssetDatabase(MB2_MeshBakerCommon mom, string folderPath, string newFileNameBase){
		if (MB2_MeshCombiner.EVAL_VERSION) return;
		if (mom is MB2_MeshBaker){
			MB2_MeshBaker mb = (MB2_MeshBaker) mom;
			string newFilename = newFileNameBase + ".asset";
			string ap = AssetDatabase.GetAssetPath(mb.meshCombiner.GetMesh());
			if (ap == null || ap.Equals("")){
				Debug.Log("Saving mesh asset to " + newFilename);
				AssetDatabase.CreateAsset(mb.meshCombiner.GetMesh(), newFilename);
			} else {
				Debug.Log("Mesh is an asset at " + ap);	
			}
		} else if (mom is MB2_MultiMeshBaker){
			MB2_MultiMeshBaker mmb = (MB2_MultiMeshBaker) mom;
			for (int i = 0; i < mmb.meshCombiner.meshCombiners.Count; i++){
				string newFilename = newFileNameBase + i + ".asset";
				Mesh mesh = mmb.meshCombiner.meshCombiners[i].combinedMesh.GetMesh();
				string ap = AssetDatabase.GetAssetPath(mesh);
				if (ap == null || ap.Equals("")){
					Debug.Log("Saving mesh asset to " + newFilename);
					AssetDatabase.CreateAsset(mesh, newFilename);
				} else {
					Debug.Log("Mesh is an asset at " + ap);	
				}			
			}				
		} else {
			Debug.LogError("Argument was not a MB2_MeshBaker or an MB2_MultiMeshBaker.");	
		}
	}
	
	public static void RebuildPrefab(MB2_MeshBakerCommon mom){
		if (MB2_MeshCombiner.EVAL_VERSION) return;
		if (mom is MB2_MeshBaker){
			MB2_MeshBaker mb = (MB2_MeshBaker) mom;
			GameObject prefabRoot = mom.resultPrefab;
			GameObject rootGO = (GameObject) PrefabUtility.InstantiatePrefab(prefabRoot);
			mb.meshCombiner.buildSceneMeshObject(rootGO,mb.meshCombiner.GetMesh(),true);		
			string prefabPth = AssetDatabase.GetAssetPath(prefabRoot);
			PrefabUtility.ReplacePrefab(rootGO,AssetDatabase.LoadAssetAtPath(prefabPth,typeof(GameObject)),ReplacePrefabOptions.ConnectToPrefab);
			Editor.DestroyImmediate(rootGO);
		} else if (mom is MB2_MultiMeshBaker){
			MB2_MultiMeshBaker mmb = (MB2_MultiMeshBaker) mom;
			GameObject prefabRoot = mom.resultPrefab;
			GameObject rootGO = (GameObject) PrefabUtility.InstantiatePrefab(prefabRoot);
			for (int i = 0; i < mmb.meshCombiner.meshCombiners.Count; i++){
				mmb.meshCombiner.meshCombiners[i].combinedMesh.buildSceneMeshObject(rootGO,mmb.meshCombiner.meshCombiners[i].combinedMesh.GetMesh(),true);
			}		
			string prefabPth = AssetDatabase.GetAssetPath(prefabRoot);
			PrefabUtility.ReplacePrefab(rootGO,AssetDatabase.LoadAssetAtPath(prefabPth,typeof(GameObject)),ReplacePrefabOptions.ConnectToPrefab);
			Editor.DestroyImmediate(rootGO);				
		} else {
			Debug.LogError("Argument was not a MB2_MeshBaker or an MB2_MultiMeshBaker.");	
		}				
	}
}
