//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;
using System.Text;
using DigitalOpus.MB.Core;

/// <summary>
/// Root class of all the baking Components
/// </summary>
public abstract class MB3_MeshBakerRoot : MonoBehaviour {
	[HideInInspector] public abstract MB2_TextureBakeResults textureBakeResults{
		get;
		set;
	}
	
	//todo switch this to List<Renderer>
	public virtual List<GameObject> GetObjectsToCombine(){
		return null;	
	}

	
	public static bool DoCombinedValidate(MB3_MeshBakerRoot mom, MB_ObjsToCombineTypes objToCombineType, MB2_EditorMethodsInterface editorMethods){
		if (mom.textureBakeResults == null){
			Debug.LogError("Need to set Material Bake Result on " + mom);
			return false;
		}
		if (!(mom is MB3_TextureBaker)){
			MB3_TextureBaker tb = mom.GetComponent<MB3_TextureBaker>();
			if (tb != null && tb.textureBakeResults != mom.textureBakeResults){
				Debug.LogWarning("Material Bake Result on this component is not the same as the Material Bake Result on the MB3_TextureBaker.");
			}
		}
		
		List<GameObject> objsToMesh = mom.GetObjectsToCombine();
		for (int i = 0; i < objsToMesh.Count; i++){
			GameObject go = objsToMesh[i];
			if (go == null){
				Debug.LogError("The list of objects to combine contains a null at position." + i + " Select and use [shift] delete to remove");
				return false;					
			}
			for (int j = i + 1; j < objsToMesh.Count; j++){
				if (objsToMesh[i] == objsToMesh[j]){
					Debug.LogError("The list of objects to combine contains duplicates.");
					return false;	
				}
			}
			if (MB_Utility.GetGOMaterials(go) == null){
				Debug.LogError("Object " + go + " in the list of objects to be combined does not have a material");
				return false;
			}
			if (MB_Utility.GetMesh(go) == null){
				Debug.LogError("Object " + go + " in the list of objects to be combined does not have a mesh");
				return false;
			}
		}
		
		if (mom.textureBakeResults.doMultiMaterial){
			if (!validateSubmeshOverlap(mom)){//only warns currently
				return false;
			}
		}
		List<GameObject> objs = objsToMesh;
		if (mom is MB3_MeshBaker){
			MB3_TextureBaker tb = mom.GetComponent<MB3_TextureBaker>();
			if (((MB3_MeshBaker)mom).useObjsToMeshFromTexBaker && tb != null) objs = tb.objsToMesh; 
			if (objs == null || objs.Count == 0){
				Debug.LogError("No meshes to combine. Please assign some meshes to combine.");
				return false;
			}
			if (mom is MB3_MeshBaker && ((MB3_MeshBaker)mom).meshCombiner.renderType == MB_RenderType.skinnedMeshRenderer){
				if (!editorMethods.ValidateSkinnedMeshes(objs)){
					return false;
				}
			}
		}
		
		if (editorMethods != null){
			editorMethods.CheckPrefabTypes(objToCombineType, objsToMesh);
		}
		return true;
	}

	static bool validateSubmeshOverlap(MB3_MeshBakerRoot mom){
		List<GameObject> objsToMesh = mom.GetObjectsToCombine();
		for (int i = 0; i < objsToMesh.Count; i++){
			Mesh m = MB_Utility.GetMesh(objsToMesh[i]);
			if (MB_Utility.doSubmeshesShareVertsOrTris(m) != 0){
				Debug.LogWarning("Object " + objsToMesh[i] + " in the list of objects to combine has overlapping submeshes (submeshes share vertices). If the UVs associated with the shared vertices are important then this bake may not work. If you are using multiple materials then this object can only be combined with objects that use the exact same set of textures (each atlas contains one texture). There may be other undesirable side affects as well. Mesh Master, available in the asset store can fix overlapping submeshes.");	
				return true;
			}
		}
		return true;
	}	
}

