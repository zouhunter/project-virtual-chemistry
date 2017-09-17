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
/// Maps a list of source materials to a combined material. Included in MB2_TextureBakeResults
/// </summary>


/// <summary>
/// Abstract root of the mesh combining classes
/// </summary>
public abstract class MB3_MeshBakerCommon : MB3_MeshBakerRoot {	
	
	//todo should be list of <Renderer>
	public List<GameObject> objsToMesh;
	
	public abstract MB3_MeshCombiner meshCombiner{
		get;
	}
	
	public bool useObjsToMeshFromTexBaker = true;
	
	//todo put this in the batch baker
	public string bakeAssetsInPlaceFolderPath;	
	
	[HideInInspector] public GameObject resultPrefab;

	public override MB2_TextureBakeResults textureBakeResults{
		get {return meshCombiner.textureBakeResults; }
		set {meshCombiner.textureBakeResults = value; }
	}
	
	public override List<GameObject> GetObjectsToCombine(){
		if (useObjsToMeshFromTexBaker){
			MB3_TextureBaker tb = gameObject.GetComponent<MB3_TextureBaker>();
			if (tb != null) {
				return tb.GetObjectsToCombine();	
			} else {
				Debug.LogWarning("Use Objects To Mesh From Texture Baker was checked but no texture baker");
				return new List<GameObject>();
			}
		} else {
			if (objsToMesh == null) objsToMesh = new List<GameObject>();
			return objsToMesh;
		}
	}

	public void EnableDisableSourceObjectRenderers(bool show){
		for (int i = 0; i < GetObjectsToCombine().Count; i++){
			GameObject go = GetObjectsToCombine()[i];
			if (go != null){
				Renderer mr = MB_Utility.GetRenderer(go);
				if (mr != null){
					mr.enabled = show;
				}
			}
		}
	}

/// <summary>
///  Clears the meshs and mesh related data but does not destroy it.
/// </summary>
	public virtual void ClearMesh(){
		meshCombiner.ClearMesh();
	}

/// <summary>
///  Clears and desroys the mesh. Clears mesh related data.
/// </summary>		
	public virtual void DestroyMesh(){
		meshCombiner.DestroyMesh();
	}

	public virtual void DestroyMeshEditor(MB2_EditorMethodsInterface editorMethods){
		meshCombiner.DestroyMeshEditor(editorMethods);
	}	

	public virtual int GetNumObjectsInCombined(){
		return meshCombiner.GetNumObjectsInCombined();	
	}
	
	public virtual int GetNumVerticesFor(GameObject go){
		return meshCombiner.GetNumVerticesFor(go);
	}

/// <summary>
/// Adds and deletes objects from the combined mesh. gos and deleteGOs can be null. 
/// You need to call Apply or ApplyAll to see the changes. 
/// objects in gos must not include objects already in the combined mesh.
/// objects in gos and deleteGOs must be the game objects with a Renderer component
/// This method is slow, so should be called as infrequently as possible.
/// </summary>
/// <returns>
/// The first generated combined mesh
/// </returns>
/// <param name='gos'>
/// gos. Array of objects to add to the combined mesh. Array can be null. Must not include objects
/// already in the combined mesh. Array must contain game objects with a render component.
/// </param>
/// <param name='deleteGOs'>
/// deleteGOs. Array of objects to delete from the combined mesh. Array can be null.
/// </param>
/// <param name='disableRendererInSource'>
/// Disable renderer component on objects in gos after they have been added to the combined mesh.
/// </param>
/// <param name='fixOutOfBoundUVs'>
/// Whether to fix out of bounds UVs in meshes as they are being added. This paramater should be set to the same as the combined material.
/// </param>
/// </summary>
	public abstract bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource=true);
	
	/// <summary>
	/// This is the best version to use for deleting game objects since the source GameObjects may have been destroyed
	/// Internaly Mesh Baker only stores the instanceID for Game Objects, so objects can be removed after they have been destroyed
	/// </summary>
	public abstract bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource=true);	
	
/// <summary>
/// Apply changes to the mesh. All channels set in this instance will be set in the combined mesh.
/// </summary>	
	public virtual void Apply(MB3_MeshCombiner.GenerateUV2Delegate uv2GenerationMethod=null){
		meshCombiner.name = name + "-mesh";
		meshCombiner.Apply(uv2GenerationMethod);
	}

/// <summary>	
/// Applys the changes to flagged properties of the mesh. This method is slow, and should only be called once per frame. The speed is directly proportional to the number of flags that are true. Only apply necessary properties.	
/// </summary>	
	public virtual void Apply(bool triangles,
					  bool vertices,
					  bool normals,
					  bool tangents,
					  bool uvs,
					  bool colors,
					  bool uv1,
					  bool uv2,
					  bool bones=false,
					  MB3_MeshCombiner.GenerateUV2Delegate uv2GenerationMethod=null){
		meshCombiner.name = name + "-mesh";
		meshCombiner.Apply(triangles,vertices,normals,tangents,uvs,colors,uv1,uv2,bones,uv2GenerationMethod);
	}	
	
	public virtual bool CombinedMeshContains(GameObject go){
		return meshCombiner.CombinedMeshContains(go);
	}
	
	public virtual void UpdateGameObjects(GameObject[] gos, bool recalcBounds = true, bool updateVertices = true, bool updateNormals = true, bool updateTangents = true,
									    bool updateUV = false, bool updateUV1 = false, bool updateUV2 = false,
										bool updateColors = false, bool updateSkinningInfo = false){
		meshCombiner.name = name + "-mesh";
		meshCombiner.UpdateGameObjects(gos,recalcBounds, updateVertices, updateNormals, updateTangents, updateUV, updateUV1, updateUV2, updateColors, updateSkinningInfo);
	}
	
	public virtual void UpdateSkinnedMeshApproximateBounds(){
		if (_ValidateForUpdateSkinnedMeshBounds()){
			meshCombiner.UpdateSkinnedMeshApproximateBounds();
		}
	}

	public virtual void UpdateSkinnedMeshApproximateBoundsFromBones(){
		if (_ValidateForUpdateSkinnedMeshBounds()){
			meshCombiner.UpdateSkinnedMeshApproximateBoundsFromBones();
		}
	}

	public virtual void UpdateSkinnedMeshApproximateBoundsFromBounds(){
		if (_ValidateForUpdateSkinnedMeshBounds()){
			meshCombiner.UpdateSkinnedMeshApproximateBoundsFromBounds();
		}
	}

	protected virtual bool _ValidateForUpdateSkinnedMeshBounds(){
		if (meshCombiner.outputOption == MB2_OutputOptions.bakeMeshAssetsInPlace){
			Debug.LogWarning("Can't UpdateSkinnedMeshApproximateBounds when output type is bakeMeshAssetsInPlace");
			return false;
		}
		if (meshCombiner.resultSceneObject == null){
			Debug.LogWarning("Result Scene Object does not exist. No point in calling UpdateSkinnedMeshApproximateBounds.");
			return false;			
		}
		SkinnedMeshRenderer smr = meshCombiner.resultSceneObject.GetComponentInChildren<SkinnedMeshRenderer>();	
		if (smr == null){
			Debug.LogWarning("No SkinnedMeshRenderer on result scene object.");
			return false;			
		}
		return true;
	}	
}
