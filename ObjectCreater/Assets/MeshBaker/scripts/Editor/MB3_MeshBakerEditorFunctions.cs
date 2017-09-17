using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEditor;

public class MB3_MeshBakerEditorFunctions {
	public static void BakeIntoCombined(MB3_MeshBakerCommon mom){
		MB2_OutputOptions prefabOrSceneObject = mom.meshCombiner.outputOption;
		if (MB3_MeshCombiner.EVAL_VERSION && prefabOrSceneObject == MB2_OutputOptions.bakeIntoPrefab){
			Debug.LogError("Cannot BakeIntoPrefab with evaluation version.");
			return;
		}
		if (prefabOrSceneObject != MB2_OutputOptions.bakeIntoPrefab && prefabOrSceneObject != MB2_OutputOptions.bakeIntoSceneObject){
			Debug.LogError("Paramater prefabOrSceneObject must be bakeIntoPrefab or bakeIntoSceneObject");
			return;
		}

		MB3_TextureBaker tb = mom.GetComponent<MB3_TextureBaker>();
		if (mom.textureBakeResults == null && tb != null){
			mom.textureBakeResults = tb.textureBakeResults;	
		}

		if (!MB3_MeshBakerRoot.DoCombinedValidate(mom, MB_ObjsToCombineTypes.sceneObjOnly, new MB3_EditorMethods())) return;	
		if (prefabOrSceneObject == MB2_OutputOptions.bakeIntoPrefab && 
			mom.resultPrefab == null){
			Debug.LogError("Need to set the Combined Mesh Prefab field. Create a prefab asset, drag an empty game object into it, and drag it to the 'Combined Mesh Prefab' field.");
			return;
		}
		mom.ClearMesh();
		if (mom.AddDeleteGameObjects(mom.GetObjectsToCombine().ToArray(),null,false)){
			mom.Apply( Unwrapping.GenerateSecondaryUVSet );
			Debug.Log(String.Format("Successfully baked {0} meshes",mom.GetObjectsToCombine().Count));
			if (prefabOrSceneObject == MB2_OutputOptions.bakeIntoSceneObject){
				PrefabType pt = PrefabUtility.GetPrefabType(mom.meshCombiner.resultSceneObject);
				if (pt == PrefabType.Prefab || pt == PrefabType.ModelPrefab){
					Debug.LogError("Combined Mesh Object is a prefab asset. If output option bakeIntoSceneObject then this must be an instance in the scene.");
					return;
				}
			} else if (prefabOrSceneObject == MB2_OutputOptions.bakeIntoPrefab){
				string prefabPth = AssetDatabase.GetAssetPath(mom.resultPrefab);
				if (prefabPth == null || prefabPth.Length == 0){
					Debug.LogError("Could not save result to prefab. Result Prefab value is not an Asset.");
					return;
				}
				string baseName = Path.GetFileNameWithoutExtension(prefabPth);
				string folderPath = prefabPth.Substring(0,prefabPth.Length - baseName.Length - 7);		
				string newFilename = folderPath + baseName + "-mesh";
				SaveMeshsToAssetDatabase(mom, folderPath,newFilename);
				
				if (mom.meshCombiner.renderType == MB_RenderType.skinnedMeshRenderer){
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

	public static void SaveMeshsToAssetDatabase(MB3_MeshBakerCommon mom, string folderPath, string newFileNameBase){
		if (MB3_MeshCombiner.EVAL_VERSION) return;
		if (mom is MB3_MeshBaker){
			MB3_MeshBaker mb = (MB3_MeshBaker) mom;
			string newFilename = newFileNameBase + ".asset";
			string ap = AssetDatabase.GetAssetPath(((MB3_MeshCombinerSingle) mb.meshCombiner).GetMesh());
			if (ap == null || ap.Equals("")){
				Debug.Log("Saving mesh asset to " + newFilename);
				AssetDatabase.CreateAsset(((MB3_MeshCombinerSingle) mb.meshCombiner).GetMesh(), newFilename);
			} else {
				Debug.Log("Mesh is an asset at " + ap);	
			}
		} else if (mom is MB3_MultiMeshBaker){
			MB3_MultiMeshBaker mmb = (MB3_MultiMeshBaker) mom;
			List<MB3_MultiMeshCombiner.CombinedMesh> combiners = ((MB3_MultiMeshCombiner) mmb.meshCombiner).meshCombiners;
			for (int i = 0; i < combiners.Count; i++){
				string newFilename = newFileNameBase + i + ".asset";
				Mesh mesh = combiners[i].combinedMesh.GetMesh();
				string ap = AssetDatabase.GetAssetPath(mesh);
				if (ap == null || ap.Equals("")){
					Debug.Log("Saving mesh asset to " + newFilename);
					AssetDatabase.CreateAsset(mesh, newFilename);
				} else {
					Debug.Log("Mesh is an asset at " + ap);	
				}			
			}				
		} else {
			Debug.LogError("Argument was not a MB3_MeshBaker or an MB3_MultiMeshBaker.");	
		}
	}
	
	public static void RebuildPrefab(MB3_MeshBakerCommon mom){
		if (MB3_MeshCombiner.EVAL_VERSION) return;
		if (mom is MB3_MeshBaker){
			MB3_MeshBaker mb = (MB3_MeshBaker) mom;
			MB3_MeshCombinerSingle mbs = (MB3_MeshCombinerSingle) mb.meshCombiner;
			GameObject prefabRoot = mom.resultPrefab;
			GameObject rootGO = (GameObject) PrefabUtility.InstantiatePrefab(prefabRoot);
			MB3_MeshCombinerSingle.BuildSceneHierarch(mbs, rootGO, mbs.GetMesh());		
			string prefabPth = AssetDatabase.GetAssetPath(prefabRoot);
			PrefabUtility.ReplacePrefab(rootGO,AssetDatabase.LoadAssetAtPath(prefabPth,typeof(GameObject)),ReplacePrefabOptions.ConnectToPrefab);
			Editor.DestroyImmediate(rootGO);
		} else if (mom is MB3_MultiMeshBaker){
			MB3_MultiMeshBaker mmb = (MB3_MultiMeshBaker) mom;
			MB3_MultiMeshCombiner mbs = (MB3_MultiMeshCombiner) mmb.meshCombiner;
			GameObject prefabRoot = mom.resultPrefab;
			GameObject rootGO = (GameObject) PrefabUtility.InstantiatePrefab(prefabRoot);
			for (int i = 0; i < mbs.meshCombiners.Count; i++){
				MB3_MeshCombinerSingle.BuildSceneHierarch(mbs.meshCombiners[i].combinedMesh, rootGO, mbs.meshCombiners[i].combinedMesh.GetMesh(),true);
			}
			string prefabPth = AssetDatabase.GetAssetPath(prefabRoot);
			PrefabUtility.ReplacePrefab(rootGO,AssetDatabase.LoadAssetAtPath(prefabPth,typeof(GameObject)),ReplacePrefabOptions.ConnectToPrefab);
			Editor.DestroyImmediate(rootGO);				
		} else {
			Debug.LogError("Argument was not a MB3_MeshBaker or an MB3_MultiMeshBaker.");	
		}				
	}
}
