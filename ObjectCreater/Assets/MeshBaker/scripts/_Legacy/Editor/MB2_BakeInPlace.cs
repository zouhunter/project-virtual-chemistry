using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using DigitalOpus.MB.Core;

namespace DigitalOpus.MB.Core{
	
	public class MB2_BakeInPlace {
		public static Mesh BakeMeshesInPlace(MB2_MeshCombiner mom, List<GameObject> objsToMesh, string saveFolder, ProgressUpdateDelegate updateProgressBar){
			if (MB2_MeshCombiner.EVAL_VERSION) return null;
				
			Mesh mesh;
	
			if (!Directory.Exists(Application.dataPath + saveFolder.Substring(6,saveFolder.Length-6))){
				Debug.Log((Application.dataPath + saveFolder));
				Debug.Log(Path.GetFullPath(Application.dataPath + saveFolder));
				Debug.LogError("The selected Folder For Meshes does not exist or is not inside the projects Assets folder. Please 'Choose Folder For Bake In Place Meshes' that is inside the project's assets folder.");	
				return null;
			}

			MB2_EditorMethods editorMethods = new MB2_EditorMethods();
			mom.DestroyMeshEditor(editorMethods);
			
			GameObject[] objs = new GameObject[1];
			MB_RenderType originalRenderType = mom.renderType;
			Mesh outMesh = null;
			for (int i = 0; i < objsToMesh.Count; i++){
				if (objsToMesh[i] == null){
					Debug.LogError("The " + i + "th object on the list of objects to combine is 'None'. Use Command-Delete on Mac OS X; Delete or Shift-Delete on Windows to remove this one element.");
					return null;					
				}
				string[] objNames = GenerateNames(objsToMesh);
				objs[0] = objsToMesh[i];
				Renderer r = MB_Utility.GetRenderer(objsToMesh[i]);
				if (r is SkinnedMeshRenderer){
					mom.renderType = MB_RenderType.skinnedMeshRenderer;	
				} else {
					mom.renderType = MB_RenderType.meshRenderer;
				}
				mesh = mom.AddDeleteGameObjects(objs,null,false);
				if (mesh != null){
					mom.Apply();
					Mesh mf = MB_Utility.GetMesh(objs[0]);
					if (mf != null){
						string newFilename = saveFolder + "/" + objNames[i];
						if (updateProgressBar != null) updateProgressBar("Created mesh saving mesh on " + objs[0].name + " to asset " + newFilename,.6f);
						if (newFilename != null && newFilename.Length != 0){
							Debug.Log("Creating mesh for " + objs[0].name + " with adjusted UVs at: " + newFilename);
							AssetDatabase.CreateAsset(mesh,  newFilename);
							outMesh = (Mesh) AssetDatabase.LoadAssetAtPath(newFilename, typeof(Mesh));
						} else {
							Debug.LogWarning("Could not save mesh for " + objs[0].name);	
						}
					}
				}
				mom.DestroyMeshEditor(editorMethods);
			}
			mom.renderType = originalRenderType;
			return outMesh;
		}
		
		static string[] GenerateNames(List<GameObject> objsToMesh){
			string[] ns = new string[objsToMesh.Count];
			for (int i = 0; i < objsToMesh.Count; i++){
				string newNameBase = objsToMesh[i].name;
				string newName = newNameBase + ".asset";
				int j = 1;
				while (ArrayUtility.Contains<string>(ns,objsToMesh[i].name)){
					newName = newNameBase + "-" + j + ".asset";
					j++;
				}
				ns[i] = newName;
			}
			return ns;
		}
	}
}