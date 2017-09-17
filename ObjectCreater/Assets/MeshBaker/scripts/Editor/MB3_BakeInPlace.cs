using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using DigitalOpus.MB.Core;

namespace DigitalOpus.MB.Core{
	
	public class MB3_BakeInPlace {
		
		public static Mesh BakeMeshesInPlace(MB3_MeshCombinerSingle mom, List<GameObject> objsToMesh, string saveFolder, ProgressUpdateDelegate updateProgressBar){
			if (MB3_MeshCombiner.EVAL_VERSION) return null; 
	
			if (!Directory.Exists(Application.dataPath + saveFolder.Substring(6))){
				Debug.Log((Application.dataPath + saveFolder.Substring(6)));
				Debug.Log(Path.GetFullPath(Application.dataPath + saveFolder.Substring(6)));
				Debug.LogError("The selected Folder For Meshes does not exist or is not inside the projects Assets folder. Please 'Choose Folder For Bake In Place Meshes' that is inside the project's assets folder.");	
				return null;
			}

			MB3_EditorMethods editorMethods = new MB3_EditorMethods();
			mom.DestroyMeshEditor(editorMethods);
			
			MB_RenderType originalRenderType = mom.renderType;
			Mesh outMesh = null;
			for (int i = 0; i < objsToMesh.Count; i++){
				if (objsToMesh[i] == null){
					Debug.LogError("The " + i + "th object on the list of objects to combine is 'None'. Use Command-Delete on Mac OS X; Delete or Shift-Delete on Windows to remove this one element.");
					return null;					
				}
				string[] objNames = GenerateNames(objsToMesh);
				outMesh = BakeOneMesh(mom, saveFolder + "/" + objNames[i], objsToMesh[i]);
				if (updateProgressBar != null) updateProgressBar("Created mesh saving mesh on " + objsToMesh[i].name + " to asset " + objNames[i],.6f);				
			}
			mom.renderType = originalRenderType;
			return outMesh;
		}
		
		static public Mesh BakeOneMesh(MB3_MeshCombinerSingle mom, string newMeshFilePath, GameObject objToBake){
			Mesh outMesh = null;
			if (objToBake == null){
				Debug.LogError("An object on the list of objects to combine is 'None'. Use Command-Delete on Mac OS X; Delete or Shift-Delete on Windows to remove this one element.");
				return null;					
			}
			
			MB3_EditorMethods editorMethods = new MB3_EditorMethods();
			GameObject[] objs = new GameObject[] {objToBake};
			Renderer r = MB_Utility.GetRenderer(objToBake);
			if (r is SkinnedMeshRenderer){
				mom.renderType = MB_RenderType.skinnedMeshRenderer;	
			} else if (r is MeshRenderer) {
				mom.renderType = MB_RenderType.meshRenderer;
			} else {
				Debug.LogError("Unsupported Renderer type on object. Must be SkinnedMesh or MeshFilter.");
				return null;	
			}
			if (newMeshFilePath == null && newMeshFilePath.Length != 0){ //todo check directory exists
				Debug.LogError("File path was not in assets folder.");
				return null;				
			}
			if (mom.AddDeleteGameObjects(objs,null,false)){
				mom.Apply();
				Mesh mf = MB_Utility.GetMesh(objToBake);
				if (mf != null){
					Debug.Log("Creating mesh for " + objToBake.name + " with adjusted UVs at: " + newMeshFilePath);
					AssetDatabase.CreateAsset(mom.GetMesh(),  newMeshFilePath);
					outMesh = (Mesh) AssetDatabase.LoadAssetAtPath(newMeshFilePath, typeof(Mesh));
				}
			}
			mom.DestroyMeshEditor(editorMethods);
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