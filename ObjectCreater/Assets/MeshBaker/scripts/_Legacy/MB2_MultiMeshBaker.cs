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
/// Component that is an endless mesh. You don't need to worry about the 65k limit when adding meshes. It is like a List of combined meshes. Internally it manages
/// a collection of CombinedMeshes that are added and deleted as necessary. 
/// 
/// Note that this implementation does
/// not attempt to split meshes. Each mesh is added to one of the internal meshes as an atomic unit.
/// 
/// This class is a Component. It must be added to a GameObject to use it. It is a wrapper for MB2_MultiMeshCombiner which contains the same functionality but is not a component
/// so it can be instantiated like a normal class.
/// </summary>
public class MB2_MultiMeshBaker : MB2_MeshBakerCommon {
		
	[HideInInspector] public MB2_MultiMeshCombiner meshCombiner = new MB2_MultiMeshCombiner();
	
	public override void ClearMesh(){
		_update_MB2_MeshCombiner();
		meshCombiner.ClearMesh();
	}
	public override void DestroyMesh(){
		_update_MB2_MeshCombiner();
		meshCombiner.DestroyMesh();
	}

	public override void DestroyMeshEditor(MB2_EditorMethodsInterface editorMethods){
		_update_MB2_MeshCombiner();
		meshCombiner.DestroyMeshEditor(editorMethods);
	}	
	
	//todo could use this
//	public void BuildSceneMeshObject(){
//		if (resultSceneObject == null){
//			resultSceneObject = new GameObject("CombinedMesh-" + name);
//		}
//		_update_MB2_MeshCombiner();
////		meshCombiner.BuildSceneMeshObject();
//	}

	public override int GetNumObjectsInCombined(){
		return meshCombiner.GetNumObjectsInCombined();	
	}
	
	public override int GetNumVerticesFor(GameObject go){
		return meshCombiner.GetNumVerticesFor(go);
	}
	
	public override Mesh AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource, bool fixOutOfBoundUVs){
		if (resultSceneObject == null){
			resultSceneObject = new GameObject("CombinedMesh-" + name);	
		}
		_update_MB2_MeshCombiner();
		Mesh mesh = meshCombiner.AddDeleteGameObjects(gos,deleteGOs,disableRendererInSource,fixOutOfBoundUVs);		
		return mesh;
	}
	
	public override Mesh AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOs, bool disableRendererInSource, bool fixOutOfBoundUVs){
		if (resultSceneObject == null){
			resultSceneObject = new GameObject("CombinedMesh-" + name);	
		}
		_update_MB2_MeshCombiner();
		Mesh mesh = meshCombiner.AddDeleteGameObjectsByID(gos,deleteGOs,disableRendererInSource,fixOutOfBoundUVs);		
		return mesh;
	}	
	
	public override bool CombinedMeshContains(GameObject go){return meshCombiner.CombinedMeshContains(go);}
	public override void UpdateGameObjects(GameObject[] gos, bool recalcBounds = true, bool updateVertices = true, bool updateNormals = true, bool updateTangents = true,
									    bool updateUV = false, bool updateUV1 = false, bool updateUV2 = false,
										bool updateColors = false, bool updateSkinningInfo = false){
		_update_MB2_MeshCombiner();
		meshCombiner.UpdateGameObjects(gos,recalcBounds, updateVertices, updateNormals, updateTangents, updateUV, updateUV1, updateUV2, updateColors, updateSkinningInfo);
	}
	public override void Apply(MB2_MeshCombiner.GenerateUV2Delegate uv2GenerationMethod=null){
		_update_MB2_MeshCombiner();
		meshCombiner.Apply(uv2GenerationMethod);
	}
	
	public override void Apply(bool triangles,
					  bool vertices,
					  bool normals,
					  bool tangents,
					  bool uvs,
					  bool colors,
					  bool uv1,
					  bool uv2,
					  bool bones=false,
					  MB2_MeshCombiner.GenerateUV2Delegate uv2GenerationMethod=null){
		_update_MB2_MeshCombiner();
		meshCombiner.Apply(triangles,vertices,normals,tangents,uvs,colors,uv1,uv2,bones);
	}

	public void UpdateSkinnedMeshApproximateBounds(){
		meshCombiner.UpdateSkinnedMeshApproximateBounds();
	}

	public void UpdateSkinnedMeshApproximateBoundsFromBones(){
		meshCombiner.UpdateSkinnedMeshApproximateBoundsFromBones();
	}

	public void UpdateSkinnedMeshApproximateBoundsFromBounds(){
		meshCombiner.UpdateSkinnedMeshApproximateBoundsFromBounds();
	}
	
	void _update_MB2_MeshCombiner(){
		meshCombiner.name = name;
		meshCombiner.textureBakeResults = textureBakeResults;
		meshCombiner.resultSceneObject = resultSceneObject;
		meshCombiner.renderType = renderType;
		meshCombiner.outputOption = outputOption;
		meshCombiner.lightmapOption = lightmapOption;
		meshCombiner.doNorm = doNorm;
		meshCombiner.doTan = doTan;
		meshCombiner.doCol = doCol;	
		meshCombiner.doUV = doUV;
		meshCombiner.doUV1 = doUV1;		
	}
}
